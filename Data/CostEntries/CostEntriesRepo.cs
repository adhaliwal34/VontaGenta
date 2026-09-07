using VontaGenta.Entities;
using MySqlConnector;
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
                        FROM CostEntries
                        INNER JOIN Projects ON Projects.ID = CostEntries.ProjectID
                        INNER JOIN Clients ON Clients.ID = Projects.ClientID
                        INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                        WHERE UserOrgs.AppUserID = @AppUserId
                            AND UserOrgs.OrgID = @OrgId
                            AND YEAR(CostEntries.EntryDate) = @Year
                            AND MONTH(CostEntries.EntryDate) = @Month
                        ORDER BY CostEntries.EntryDate;";

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
                        CostEntries.*
                        ,AspNetUsers.*
                        ,CostCategories.*
                        ,Currencies.*
                        ,Projects.*
                        FROM CostEntries
                        INNER JOIN AspNetUsers ON AspNetUsers.ID = CostEntries.AppUserID
                        INNER JOIN CostCategories ON CostCategories.ID = CostEntries.CategoryID
                        INNER JOIN Currencies ON Currencies.ID = CostEntries.CurrencyID
                        INNER JOIN Projects ON Projects.ID = CostEntries.ProjectID
                        INNER JOIN Clients ON Clients.ID = Projects.ClientID
                        INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                        WHERE UserOrgs.AppUserID = @AppUserId
                            AND UserOrgs.OrgID = @OrgId
                            AND YEAR(CostEntries.EntryDate) = @Year
                            AND MONTH(CostEntries.EntryDate) = @Month
                        ORDER BY CostEntries.EntryDate
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
                        CostEntries.*
                        FROM CostEntries
                        WHERE CostEntries.ID = @Id;";

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
                        CostEntries.*
                        ,AspNetUsers.*
                        ,Projects.*
                        ,Clients.*
                        ,Orgs.*
                        ,CostCategories.*
                        ,Currencies.*
                        FROM CostEntries
                        INNER JOIN AspNetUsers ON AspNetUsers.ID = CostEntries.AppUserID
                        INNER JOIN Projects ON Projects.ID = CostEntries.ProjectID
                        INNER JOIN Clients ON Clients.ID = Projects.ClientID
                        INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                        INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                        INNER JOIN CostCategories ON CostCategories.ID = CostEntries.CategoryID
                        INNER JOIN Currencies ON Currencies.ID = CostEntries.CurrencyID
                        WHERE CostEntries.ID = @Id
                            AND UserOrgs.AppUserID = @AppUserId;";

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

        public async Task<int> SaveCostEntry(CostEntry costEntry, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (costEntry == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { costEntry.EntryDate, costEntry.Name, costEntry.Description, costEntry.CategoryID, costEntry.Amount, costEntry.CurrencyID, costEntry.AppUserID, costEntry.ProjectID };
                var sql = @"INSERT CostEntries (EntryDate, Name, Description, CategoryID, Amount, CurrencyID, AppUserID, ProjectID)
                    VALUES (@EntryDate, @Name, @Description, @CategoryID, @Amount, @CurrencyID, @AppUserID, @ProjectID); SELECT LAST_INSERT_ID();";

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

        public async Task<int> UpdateCostEntry(CostEntry costEntry, CancellationToken ct, MySqlTransaction? tx = null)
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
                var sql = @"UPDATE CostEntries
                            SET EntryDate = @EntryDate
                            ,Name = @Name
                            ,Description = @Description
                            ,CategoryID = @CategoryID
                            ,ProjectID = @ProjectID
                            ,Amount = @Amount
                            ,CurrencyID = @CurrencyID
                            WHERE CostEntries.ID = @ID;";

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

        public async Task<int> DeleteCostEntry(CostEntry costEntry, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (costEntry == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { Id = costEntry.ID };
                var sql = @"DELETE FROM CostEntries WHERE CostEntries.ID = @Id;";

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
                            1
                            FROM CostEntries
                            INNER JOIN Projects ON Projects.ID = CostEntries.ProjectID
                            INNER JOIN Clients ON Clients.ID = Projects.ClientID
                            INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                            WHERE CostEntries.ID = @Id
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

        public async Task<bool> AllowCreateCostEntry(CostEntry costEntry, string userId, int orgId, CancellationToken ct)
        {
            try
            {
                var parameters = new { AppUserId = userId, costEntry.CategoryID, costEntry.ProjectID, OrgId = orgId };
                var sql = @"SELECT
                            1
                            FROM UserOrgs
                            INNER JOIN Projects ON Projects.ID = @ProjectID
                            INNER JOIN Clients ON Clients.ID = Projects.ClientID
                            INNER JOIN CostCategories ON CostCategories.ID = @CategoryID
                            WHERE UserOrgs.AppUserID = @AppUserId
                                AND UserOrgs.OrgID = @OrgId
                                AND Clients.OrgID = @OrgId
                                AND CostCategories.OrgID = @OrgId;";

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
                            1
                            FROM CostEntries
                            INNER JOIN Projects ON Projects.ID = CostEntries.ProjectID
                            INNER JOIN Clients ON Clients.ID = Projects.ClientID
                            INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                            INNER JOIN CostCategories ON CostCategories.ID = CostEntries.CategoryID
                            WHERE CostEntries.ID = @Id
                                AND CostEntries.AppUserID = @AppUserId
                                AND UserOrgs.AppUserID = @AppUserId
                                AND UserOrgs.OrgID = @OrgId
                                AND CostCategories.OrgID = @OrgId;";

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
