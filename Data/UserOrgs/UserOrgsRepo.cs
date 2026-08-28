using VontaGenta.Entities;
using Dapper;
using MySqlConnector;

namespace VontaGenta.Data
{
    public class UserOrgsRepo : RepoBase, IUserOrgsRepo
    {

        public UserOrgsRepo(IUnitOfWork uow, ILogger<UserOrgsRepo> logger)
        : base(uow, logger)
        {
            _logger.LogInformation("UserOrgsRepo instantiated");
        }

        public async Task<List<UserOrg>?> GetUsersInOrg(int orgId, CancellationToken ct)
        {
            try
            {
                var parameters = new { OrgID = orgId };
                var sql = @"SELECT
                        UserOrgs.*, AspNetUsers.*
                        FROM UserOrgs
                        INNER JOIN AspNetUsers ON AspNetUsers.ID = UserOrgs.AppUserID
                        WHERE UserOrgs.OrgID = @OrgID;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<UserOrg, AppUser, UserOrg>(command, (userOrgs, appUser) =>
                {
                    userOrgs.User = appUser;
                    return userOrgs;
                })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        public async Task<bool> IsOrgMember(string userId, int orgId, CancellationToken ct)
        {
            try
            {
                var parameters = new { AppUserId = userId, OrgId = orgId };
                var sql = @"SELECT
                            1
                            FROM UserOrgs
                            WHERE UserOrgs.AppUserID = @AppUserId
                            AND UserOrgs.OrgID = @OrgId;";

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

        public async Task<int> LeaveOrg(string userId, int orgId, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (orgId == 0)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { Id = orgId, AppUserId = userId };
                var sql = @"DELETE FROM UserOrgs
                                WHERE
                                OrgID = @Id
                                AND AppUserID = @AppUserId;";

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

        // TODO: good candidate for caching in Redis. This method is called a lot.
        public async Task<Org?> GetActiveOrg(string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { AppUserId = userId };
                var sql = @"SELECT
                            Orgs.*
                            ,AspNetUsers.*
                            FROM UserOrgs
                            INNER JOIN Orgs ON Orgs.ID = UserOrgs.OrgID
                            INNER JOIN AspNetUsers ON AspNetUsers.ID = Orgs.AppUserID
                            WHERE UserOrgs.AppUserID = @AppUserID
                            AND UserOrgs.IsActive = 1;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Org, AppUser, Org>(command, (org, appUser) =>
                {
                    org.User = appUser;
                    return org;
                })).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        public async Task<int> SetActiveOrg(string userId, int orgId, CancellationToken ct, MySqlTransaction? tx = null)
        {
            try
            {
                var tran = tx ?? Transaction;

                // Set current active user org to not active.
                var parameters1 = new { AppUserID = userId };
                var sql1 = @"UPDATE UserOrgs SET IsActive = 0
                            WHERE AppUserID = @AppUserID
                            AND IsActive = 1;";

                var command = new CommandDefinition(
                    sql1,
                    parameters1,
                    cancellationToken: ct,
                    transaction: tran
                );

                await Connection.ExecuteAsync(command);

                // Set new user org to active
                var parameters2 = new { AppUserID = userId, OrgID = orgId };
                var sql2 = @"UPDATE UserOrgs SET IsActive = 1
                            WHERE AppUserID = @AppUserID
                            AND OrgID = @OrgID;";

                command = new CommandDefinition(
                    sql2,
                    parameters2,
                    cancellationToken: ct,
                    transaction: tran
                );

                return await Connection.ExecuteAsync(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return -1;
            }
        }
    }
}
// cSpell:ignore tran