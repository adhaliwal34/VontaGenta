using VontaGenta.Entities;
using Npgsql;
using Dapper;

namespace VontaGenta.Data
{
    public class CostEntriesRepo : RepoBase, ICostEntriesRepo
    {

        public CostEntriesRepo(IUnitOfWork uow, ILogger<CostEntriesRepo> logger)
        : base(uow, logger)
        {
            _logger.LogInformation("CostEntriesRepo instantiated");
        }

        // Can view cost entries made by other users in the same org.
        public async Task<int> GetTotalCountCostEntriesForMemberOrg(string userId, int orgId, int year, int month, CancellationToken ct)
        {
            try
            {
                var parameters = new { AppUserId = userId, OrgId = orgId, Year = year, Month = month };
                var sql = @"SELECT
                        Count(*)
                        FROM costentries
                        INNER JOIN projects ON projects.id = costentries.projectid
                        INNER JOIN clients ON clients.id = projects.clientid
                        INNER JOIN userorgs ON userorgs.orgid = clients.orgid
                        WHERE userorgs.appuserid = @AppUserId
                            AND userorgs.orgid = @OrgId
                            AND EXTRACT(YEAR FROM costentries.entrydate) = @Year
                            AND EXTRACT(MONTH FROM costentries.entrydate) = @Month;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<int>(command)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return -1;
            }
        }

        // Can view cost entries made by other users in the same org.
        // Gets related fields too
        public async Task<List<CostEntry>> GetCostEntriesForMemberOrg(string userId, int orgId, int year, int month, int page, int itemsPerPage, CancellationToken ct)
        {
            try
            {

                var parameters = new { AppUserId = userId, OrgId = orgId, Year = year, Month = month, Limit = itemsPerPage, Offset = (page-1) * itemsPerPage };
                var sql = @"SELECT
                        costentries.*
                        ,aspnetusers.*
                        ,costcategories.*
                        ,currencies.*
                        ,projects.*
                        FROM costentries
                        INNER JOIN aspnetusers ON aspnetusers.id = costentries.appuserid
                        INNER JOIN costcategories ON costcategories.id = costentries.categoryid
                        INNER JOIN currencies ON currencies.id = costentries.currencyid
                        INNER JOIN projects ON projects.id = costentries.projectid
                        INNER JOIN clients ON clients.id = projects.clientid
                        INNER JOIN userorgs ON userorgs.orgid = clients.orgid
                        WHERE userorgs.appuserid = @AppUserId
                            AND userorgs.orgid = @OrgId
                            AND EXTRACT(YEAR FROM costentries.entrydate) = @Year
                            AND EXTRACT(MONTH FROM costentries.entrydate) = @Month
                        ORDER BY costentries.entrydate
                        LIMIT @Limit OFFSET @offset;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<CostEntry, AppUser, Category, Currency, Project, CostEntry>(command, (costEntry, appUser, category, currency, project) =>
                {
                    costEntry.Category = category;
                    costEntry.Project = project;
                    costEntry.User = appUser;
                    costEntry.Currency = currency;
                    return costEntry;
                })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return new List<CostEntry>();
            }
        }

        // Does not filter by org or creator
        public async Task<CostEntry?> GetCostEntry(int id, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = id };
                var sql = @"SELECT
                        costentries.*
                        FROM costentries
                        WHERE costentries.id = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<CostEntry>(command)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        // Filters by org membership, not creator
        public async Task<CostEntry?> GetCostEntryAndRelatedFields(int id, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = id, AppUserId = userId };
                var sql = @"SELECT
                        costentries.*
                        ,aspnetusers.*
                        ,projects.*
                        ,clients.*
                        ,orgs.*
                        ,costcategories.*
                        ,currencies.*
                        FROM costentries
                        INNER JOIN aspnetusers ON aspnetusers.id = costentries.appuserid
                        INNER JOIN projects ON projects.id = costentries.projectid
                        INNER JOIN clients ON clients.id = projects.clientid
                        INNER JOIN orgs ON orgs.id = clients.orgid
                        INNER JOIN userorgs ON userorgs.orgid = clients.orgid
                        INNER JOIN costcategories ON costcategories.id = costentries.categoryid
                        INNER JOIN currencies ON currencies.id = costentries.currencyid
                        WHERE costentries.id = @Id
                            AND userorgs.appuserid = @AppUserId;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<CostEntry, AppUser, Project, Client, Org, Category, Currency, CostEntry>(command, (costEntry, appUser, project, client, org, category, currency) =>
                {
                    costEntry.Category = category;
                    client.Org = org;
                    project.Client = client;
                    costEntry.Project = project;
                    costEntry.User = appUser;
                    costEntry.Currency = currency;
                    return costEntry;
                })).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        public async Task<int> SaveCostEntry(CostEntry costEntry, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (costEntry == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { costEntry.EntryDate, costEntry.Name, costEntry.Description, costEntry.CategoryID, costEntry.Amount, costEntry.CurrencyID, costEntry.AppUserID, costEntry.ProjectID };
                var sql = @"INSERT INTO costentries (entrydate, name, description, categoryid, amount, currencyid, appuserid, projectid)
                    VALUES (@EntryDate, @Name, @Description, @CategoryID, @Amount, @CurrencyID, @AppUserID, @ProjectID) RETURNING id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct,
                    transaction: tran
                );

                return await Connection.QuerySingleAsync<int>(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return -1;
            }
        }

        public async Task<int> UpdateCostEntry(CostEntry costEntry, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (costEntry == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                // AppUser is not modifiable.
                var parameters = new { costEntry.ID, costEntry.EntryDate, costEntry.Name, costEntry.Description, costEntry.CategoryID, costEntry.ProjectID, costEntry.Amount, costEntry.CurrencyID };
                var sql = @"UPDATE costentries
                            SET entrydate = @EntryDate
                            ,name = @Name
                            ,description = @Description
                            ,categoryid = @CategoryID
                            ,projectid = @ProjectID
                            ,amount = @Amount
                            ,currencyid = @CurrencyID
                            WHERE costentries.id = @ID;";

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

        public async Task<int> DeleteCostEntry(CostEntry costEntry, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (costEntry == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { Id = costEntry.ID };
                var sql = @"DELETE FROM costentries WHERE costentries.id = @Id;";

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

        public async Task<bool> AllowGetCostEntry(int id, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = id, AppUserId = userId };
                var sql = @"SELECT
                            true
                            FROM costentries
                            INNER JOIN projects ON projects.id = costentries.projectid
                            INNER JOIN clients ON clients.id = projects.clientid
                            INNER JOIN userorgs ON userorgs.orgid = clients.orgid
                            WHERE costentries.id = @Id
                                AND userorgs.appuserid = @AppUserId;";

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

        public async Task<bool> AllowCreateCostEntry(CostEntry costEntry, string userId, int orgId, CancellationToken ct)
        {
            try
            {
                var parameters = new { AppUserId = userId, costEntry.CategoryID, costEntry.ProjectID, OrgId = orgId };
                var sql = @"SELECT
                            true
                            FROM userorgs
                            INNER JOIN projects ON projects.id = @ProjectID
                            INNER JOIN clients ON clients.id = projects.clientid
                            INNER JOIN costcategories ON costcategories.id = @CategoryID
                            WHERE userorgs.appuserid = @AppUserId
                                AND userorgs.orgid = @OrgId
                                AND clients.orgid = @OrgId
                                AND costcategories.orgid = @OrgId;";

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

        public async Task<bool> AllowUpdateCostEntry(int id, string userId, int orgId, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = id, AppUserId = userId, OrgId = orgId };
                var sql = @"SELECT
                            true
                            FROM costentries
                            INNER JOIN projects ON projects.id = costentries.projectid
                            INNER JOIN clients ON clients.id = projects.clientid
                            INNER JOIN userorgs ON userorgs.orgid = clients.orgid
                            INNER JOIN costcategories ON costcategories.id = costentries.categoryid
                            WHERE costentries.id = @Id
                                AND costentries.appuserid = @AppUserId
                                AND userorgs.appuserid = @AppUserId
                                AND userorgs.orgid = @OrgId
                                AND costcategories.orgid = @OrgId;";

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
