using VontaGenta.Entities;
using MySqlConnector;
using Dapper;

namespace VontaGenta.Data
{
    public class TimesheetsRepo : RepoBase, ITimesheetsRepo
    {

        public TimesheetsRepo(IUnitOfWork uow, ILogger<TimesheetsRepo> logger)
        : base(uow, logger)
        {
            _logger.LogInformation("TimesheetsRepo instantiated");
        }

        // Get timesheets in member org where user is the creator.
        public async Task<IEnumerable<Timesheet>> GetTimesheetsAndRelatedFieldsForMemberOrg(DateTime fromDate, DateTime toDate, string userId, int orgId, CancellationToken ct)
        {
            try
            {
                var parameters = new { AppUserId = userId, OrgId = orgId, FromDate = fromDate, ToDate = toDate };
                var sql = @"SELECT
                        Timesheets.*
                        ,AspNetUsers.*
                        ,Projects.*
                        ,Clients.*
                        ,Orgs.*
                        FROM Timesheets
                        INNER JOIN AspNetUsers ON AspNetUsers.ID = Timesheets.AppUserID
                        INNER JOIN Projects ON Projects.ID = Timesheets.ProjectID
                        INNER JOIN Clients ON Clients.ID = Projects.ClientID
                        INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                        INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                        WHERE
                            Timesheets.AppUserID = @AppUserId
                            AND Orgs.ID = @OrgId
                            AND UserOrgs.AppUserID = @AppUserId
                            AND (DateTimeStart <= @ToDate AND IFNULL(DateTimeEnd,'2050-12-31') >= @FromDate);";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Timesheet, AppUser, Project, Client, Org, Timesheet>(command, (timesheet, appUser, project, client, org) =>
                {
                    client.Org = org;
                    project.Client = client;
                    timesheet.Project = project;
                    timesheet.User = appUser;
                    return timesheet;
                })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return [];
            }
        }

        // Only returns an un-ended timesheet on the date specified if any timesheet can be found. Does not look back further than the specified date.
        public async Task<Timesheet?> GetClosestOpenPreviousTimesheet(DateTime utcDateTime, string userId, CancellationToken ct)
        {
            //return _context.Timesheets.Where(t => t.OwnerId.Equals(userId) &&
            // t.DateTimeStart >= utcDateTime.Date && t.DateTimeStart <= utcDateTime &&
            //  !t.DateTimeEnd.HasValue).OrderByDescending(t => t.DateTimeStart).FirstOrDefault();

            // start date time >= date with 0:0:0 time and less than equal to the given date time.
            var utcDateMidnight = new DateTime(utcDateTime.Year, utcDateTime.Month, utcDateTime.Day);
            try
            {
                var parameters = new { AppUserId = userId, UtcDateTime = utcDateTime, UtcDateMidnight = utcDateMidnight };
                var sql = @"SELECT
                            Timesheets.*
                            FROM Timesheets
                            WHERE
                                Timesheets.AppUserId = @AppUserId
                                AND DateTimeStart >= @UtcDateMidnight
                                AND DateTimeStart <= @UtcDateTime
                                AND DateTimeEnd is null
                                ORDER BY DESC DateTimeStart;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Timesheet>(command)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        // TODO check creator and org membership
        public async Task<Timesheet?> GetTimesheetById(int id, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = id };
                var sql = @"SELECT
                        Timesheets.*
                        FROM Timesheets
                        WHERE
                            Timesheets.ID = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Timesheet>(command)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        public async Task<Timesheet?> GetTimesheetAndRelatedFields(int id, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = id };
                var sql = @"SELECT
                        Timesheets.*
                        ,AspNetUsers.*
                        ,Projects.*
                        ,Clients.*
                        ,Orgs.*
                        FROM Timesheets
                        INNER JOIN AspNetUsers ON AspNetUsers.ID = Timesheets.AppUserID
                        INNER JOIN Projects ON Projects.ID = Timesheets.ProjectID
                        INNER JOIN Clients ON Clients.ID = Projects.ClientID
                        INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                        WHERE
                            Timesheets.ID = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Timesheet, AppUser, Project, Client, Org, Timesheet>(command, (timesheet, appUser, project, client, org) =>
                {
                    timesheet.User = appUser;
                    client.Org = org;
                    project.Client = client;
                    timesheet.Project = project;
                    return timesheet;
                })).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        public async Task<int> SaveTimesheet(Timesheet timesheet, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (timesheet == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { timesheet.Name, timesheet.Description, timesheet.DateTimeStart, timesheet.DateTimeEnd, timesheet.AppUserID, timesheet.ProjectID };
                var sql = @"INSERT Timesheets (Name, Description, DateTimeStart, DateTimeEnd, AppUserID, ProjectID)
                            SELECT @Name, @Description, @DateTimeStart, @DateTimeEnd, @AppUserID, @ProjectID;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct,
                    transaction: tran
                );

                return await Connection.ExecuteAsync(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return 0;
            }
        }

        public async Task<int> DeleteTimesheet(Timesheet timesheet, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (timesheet == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { Id = timesheet.ID };
                var sql = @"DELETE FROM Timesheets WHERE Timesheets.ID = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct,
                    transaction: tran
                );

                return await Connection.ExecuteAsync(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return 0;
            }
        }

        public async Task<int> UpdateTimesheet(Timesheet timesheet, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (timesheet == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { timesheet.ID, timesheet.Name, timesheet.Description, timesheet.DateTimeStart, timesheet.DateTimeEnd, timesheet.ProjectID };
                var sql = @"UPDATE Timesheets
                            SET Name = @Name, Description = @Description, DateTimeStart = @DateTimeStart, DateTimeEnd = @DateTimeEnd, ProjectID = @ProjectID
                            WHERE Timesheets.ID = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct,
                    transaction: tran
                );

                return await Connection.ExecuteAsync(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return 0;
            }
        }

        #region Permissions

        // Allow getting timesheet if user is an org member
        public async Task<bool> AllowGetTimesheetIfOrgMember(int timesheetId, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { TimesheetId = timesheetId, AppUserId = userId };
                var sql = @"SELECT
                            1
                            FROM Timesheets
                            INNER JOIN Projects ON Projects.ID = Timesheets.ProjectID
                            INNER JOIN Clients ON Clients.ID = Projects.ClientID
                            INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                            INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                            WHERE Timesheets.ID = @TimesheetId
                                AND UserOrgs.AppUserID = @AppUserId;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<bool>(command)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return false;
            }
        }

        // Allow create timesheet if user is an org member.
        public async Task<bool> AllowCreateTimesheet(int projectId, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { ProjectId = projectId, AppUserId = userId };
                var sql = @"SELECT
                            1
                            FROM Projects
                            INNER JOIN Clients ON Clients.ID = Projects.ClientID
                            INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                            INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                            WHERE Projects.ID = @ProjectId
                                AND UserOrgs.AppUserID = @AppUserId;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<bool>(command)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return false;
            }
        }

        // Allow update timesheet if user is an org member and is timesheet creator.
        public async Task<bool> AllowUpdateTimesheet(int timesheetId, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { TimesheetId = timesheetId, AppUserId = userId };
                var sql = @"SELECT
                            1
                            FROM Timesheets
                            INNER JOIN Projects ON Projects.ID = Timesheets.ProjectID
                            INNER JOIN Clients ON Clients.ID = Projects.ClientID
                            INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                            INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                            WHERE Timesheets.ID = @TimesheetId
                                AND Timesheets.AppUserID = @AppUserId
                                AND UserOrgs.AppUserID = @AppUserId;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<bool>(command)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return false;
            }
        }

        #endregion
    }
}
// cSpell:ignore tran IFNULL