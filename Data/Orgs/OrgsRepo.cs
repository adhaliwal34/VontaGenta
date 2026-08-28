using VontaGenta.Entities;
using Dapper;
using MySqlConnector;

namespace VontaGenta.Data
{
    public class OrgsRepo : RepoBase, IOrgsRepo
    {

        public OrgsRepo(IUnitOfWork uow, ILogger<OrgsRepo> logger)
        : base(uow, logger)
        {
           _logger.LogInformation("OrgsRepo instantiated");
        }

        // Get all orgs user is a member of.
        public async Task<List<Org>?> GetOrgs(string userId, CancellationToken ct)
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
                        WHERE UserOrgs.AppUserID = @AppUserId;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Org, AppUser, Org>(command, (org, appUser) =>
                {
                    org.User = appUser;
                    return org;
                })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        // Get org if user is a member.
        public async Task<Org?> GetOrg(int orgId, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = orgId, AppUserId = userId };
                var sql = @"SELECT
                        Orgs.*
                        FROM UserOrgs
                        INNER JOIN Orgs ON Orgs.ID = UserOrgs.OrgID
                        WHERE
                        Orgs.ID = @Id
                        AND UserOrgs.AppUserID = @AppUserId;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return await Connection.QueryFirstOrDefaultAsync<Org>(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        // Get org if user is a member.
        public async Task<Org?> GetOrgAndRelatedFields(int orgId, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = orgId, AppUserId = userId };
                var sql = @"SELECT
                        Orgs.*
                        ,AspNetUsers.*
                        FROM UserOrgs
                        INNER JOIN Orgs ON Orgs.ID = UserOrgs.OrgID
                        INNER JOIN AspNetUsers ON AspNetUsers.ID = Orgs.AppUserID
                        WHERE
                        Orgs.ID = @Id
                        AND UserOrgs.AppUserID = @AppUserId;";

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

        public async Task<int> SaveOrg(Org org, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (org == null)
            {
                return 0;
            }


            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { org.Name, org.Description, AppUserId = org.AppUserID };
                var sql = @"INSERT Orgs (Name, Description, AppUserID)
                            VALUES (@Name, @Description, @AppUserID); SELECT LAST_INSERT_ID();";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct,
                    transaction: tran
                );

                var orgID = await Connection.QuerySingleAsync<int>(command);

                var parameters2 = new { OrgID = orgID, org.AppUserID };
                var sql2 = @"INSERT UserOrgs (AppUserID, OrgID)
                            VALUES (@AppUserID, @OrgID);";

                command = new CommandDefinition(
                    sql2,
                    parameters2,
                    cancellationToken: ct,
                    transaction: tran
                );

                await Connection.ExecuteAsync(command);

                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return 0;
            }
        }

        public async Task<int> UpdateOrg(Org org, string userId, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (org == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { Id = org.ID, org.Name, org.Description, AppUserId = userId };
                var sql = "UPDATE Orgs SET Name = @Name, Description = @Description WHERE ID = @Id AND AppUserID = @AppUserId;";

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

        public async Task<int> DeleteOrg(Org org, string userId, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (org == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                // Delete all members of the Org.
                var parameters1 = new { Id = org.ID };
                var sql1 = "DELETE FROM UserOrgs WHERE OrgID = @Id;";

                var command = new CommandDefinition(
                    sql1,
                    parameters1,
                    cancellationToken: ct,
                    transaction: tran
                );

                await Connection.ExecuteAsync(command);

                var parameters2 = new { Id = org.ID };
                var sql2 = "DELETE FROM Orgs WHERE ID = @Id;";

                command = new CommandDefinition(
                    sql2,
                    parameters2,
                    cancellationToken: ct,
                    transaction: tran
                );

                await Connection.ExecuteAsync(command);

                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return 0;
            }
        }

    }
}
// cSpell:ignore tran