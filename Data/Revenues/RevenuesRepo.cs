using Dapper;
using Npgsql;
using VontaGenta.Entities;

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
                        revenues.*
                        ,aspnetusers.*
                        ,currencies.*
                        ,projects.*
                        ,clients.*
                        ,orgs.*
                        FROM revenues
                        INNER JOIN aspnetusers ON aspnetusers.id = revenues.appuserid
                        INNER JOIN currencies ON currencies.id = revenues.currencyid
                        INNER JOIN projects ON projects.id = revenues.projectid
                        INNER JOIN clients ON clients.id = projects.clientid
                        INNER JOIN orgs ON orgs.id = clients.orgid
                        INNER JOIN userorgs ON userorgs.orgid = clients.orgid
                        WHERE
                            revenues.appuserid = @AppUserId
                            AND revenues.entrydate = @EntryDate
                            AND userorgs.orgid = @OrgId
                            AND userorgs.appuserid = @AppUserId;";

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
                        revenues.*
                        FROM revenues
                        WHERE
                            revenues.id = @Id;";

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
                        revenues.*
                        ,projects.*
                        ,aspnetusers.*
                        ,currencies.*
                        FROM revenues
                        INNER JOIN projects ON projects.id = revenues.projectid
                        INNER JOIN aspnetusers ON aspnetusers.id = revenues.appuserid
                        INNER JOIN currencies ON currencies.id = revenues.currencyid
                        WHERE
                            revenues.id = @Id;";

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

        public async Task<int> SaveRevenue(Revenue revenue, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (revenue == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { revenue.Description, revenue.Amount, revenue.CurrencyID, revenue.EntryDate, revenue.AppUserID, revenue.ProjectID };
                var sql = @"INSERT INTO revenues (description, amount, currencyid, entrydate, appuserid, projectid)
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

        public async Task<int> DeleteRevenue(int id, CancellationToken ct, NpgsqlTransaction? tx = null)
        {

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { Id = id };
                var sql = @"DELETE FROM revenues WHERE revenues.id = @Id;";

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

        public async Task<int> UpdateRevenue(Revenue revenue, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (revenue == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { revenue.ID, revenue.Description, revenue.Amount, revenue.CurrencyID, revenue.EntryDate, revenue.ProjectID };
                var sql = @"UPDATE revenues
                            SET description = @Description, amount = @Amount, currencyid = @CurrencyID, entrydate = @EntryDate, projectid = @ProjectID
                            WHERE revenues.id = @Id;";

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
                        true
                        FROM revenues
                        WHERE
                            revenues.appuserid = @AppUserId
                            AND revenues.id = @Id;";

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
