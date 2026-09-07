using VontaGenta.Entities;
using MySqlConnector;
using Dapper;

namespace VontaGenta.Data
{
    public class RevenuesRepo : RepoBase, IRevenuesRepo
    {

        public RevenuesRepo(IUnitOfWork uow, ILogger<RevenuesRepo> logger)
        : base(uow, logger)
        {
            _logger.LogInformation("RevenuesRepo instantiated");
        }

        // TODO currently only creator of items can get the items.
        public async Task<IEnumerable<Revenue>> GetRevenuesAndRelatedFields(DateTime date, string userId, int orgId, CancellationToken ct)
        {
            try
            {
                var parameters = new { AppUserId = userId, EntryDate = date, OrgId = orgId };
                var sql = @"SELECT
                        Revenues.*
                        ,AspNetUsers.*
                        ,Currencies.*
                        ,Projects.*
                        ,Clients.*
                        ,Orgs.*
                        FROM Revenues
                        INNER JOIN AspNetUsers ON AspNetUsers.ID = Revenues.AppUserID
                        INNER JOIN Currencies ON Currencies.ID = Revenues.CurrencyID
                        INNER JOIN Projects ON Projects.ID = Revenues.ProjectID
                        INNER JOIN Clients ON Clients.ID = Projects.ClientID
                        INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                        INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                        WHERE
                            Revenues.AppUserID = @AppUserId
                            AND Revenues.EntryDate = @EntryDate
                            AND UserOrgs.OrgID = @OrgId
                            AND UserOrgs.AppUserID = @AppUserId;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Revenue, AppUser, Currency, Project, Client, Org, Revenue>(command, (revenue, appUser, currency, project, client, org) =>
                {
                    client.Org = org;
                    project.Client = client;
                    revenue.Project = project;
                    revenue.User = appUser;
                    revenue.Currency = currency;
                    return revenue;
                })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return [];
            }
        }

        public async Task<Revenue?> GetRevenueById(int id, CancellationToken ct)
        {
            try
            {

                var parameters = new { Id = id };
                var sql = @"SELECT
                        Revenues.*
                        FROM Revenues
                        WHERE
                            Revenues.ID = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Revenue>(command)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        public async Task<Revenue?> GetRevenueAndRelatedFields(int id, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = id };
                var sql = @"SELECT
                        Revenues.*
                        ,Projects.*
                        ,AspNetUsers.*
                        ,Currencies.*
                        FROM Revenues
                        INNER JOIN Projects ON Projects.ID = Revenues.ProjectID
                        INNER JOIN AspNetUsers ON AspNetUsers.ID = Revenues.AppUserID
                        INNER JOIN Currencies ON Currencies.ID = Revenues.CurrencyID
                        WHERE
                            Revenues.ID = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Revenue, Project, AppUser, Currency, Revenue>(command, (revenue, project, appUser, currency) =>
                {
                    revenue.User = appUser;
                    revenue.Project = project;
                    revenue.Currency = currency;
                    return revenue;
                })).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        public async Task<int> SaveRevenue(Revenue revenue, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (revenue == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { revenue.Description, revenue.Amount, revenue.CurrencyID, revenue.EntryDate, revenue.AppUserID, revenue.ProjectID };
                var sql = @"INSERT Revenues (Description, Amount, CurrencyID, EntryDate, AppUserID, ProjectID)
                            SELECT @Description, @Amount, @CurrencyID, @EntryDate, @AppUserID, @ProjectID;";

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

        public async Task<int> DeleteRevenue(int id, CancellationToken ct, MySqlTransaction? tx = null)
        {

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { Id = id };
                var sql = @"DELETE FROM Revenues WHERE Revenues.ID = @Id;";

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

        public async Task<int> UpdateRevenue(Revenue revenue, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (revenue == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { revenue.ID, revenue.Description, revenue.Amount, revenue.CurrencyID, revenue.EntryDate, revenue.ProjectID };
                var sql = @"UPDATE Revenues
                            SET Description = @Description, Amount = @Amount, CurrencyID = @CurrencyID, EntryDate = @EntryDate, ProjectID = @ProjectID
                            WHERE Revenues.ID = @Id;";

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

        // Test that user is the creator.
        public async Task<bool> CanGetRevenue(int id, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { AppUserId = userId, Id = id };
                var sql = @"SELECT
                        1
                        FROM Revenues
                        WHERE
                            Revenues.AppUserID = @AppUserId
                            AND Revenues.ID = @Id;";

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
// cSpell:ignore tran
