using VontaGenta.Entities;
using Dapper;
using Npgsql;

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
                        orgs.*
                        ,aspnetusers.*
                        FROM userorgs
                        INNER JOIN orgs ON orgs.id = userorgs.orgid
                        INNER JOIN aspnetusers ON aspnetusers.id = orgs.appuserid
                        WHERE userorgs.appuserid = @AppUserId;";

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
                        orgs.*
                        FROM userorgs
                        INNER JOIN orgs ON orgs.id = userorgs.orgid
                        WHERE
                        orgs.id = @Id
                        AND userorgs.appuserid = @AppUserId;";

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
                        orgs.*
                        ,aspnetusers.*
                        FROM userorgs
                        INNER JOIN orgs ON orgs.id = userorgs.orgid
                        INNER JOIN aspnetusers ON aspnetusers.id = orgs.appuserid
                        WHERE
                        orgs.id = @Id
                        AND userorgs.appuserid = @AppUserId;";

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

        public async Task<int> SaveOrg(Org org, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (org == null)
            {
                return 0;
            }


            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { org.Name, org.Description, AppUserId = org.AppUserID };
                var sql = @"INSERT INTO orgs (name, description, appuserid)
                            VALUES (@Name, @Description, @AppUserID) RETURNING id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct,
                    transaction: tran
                );

                var orgID = await Connection.QuerySingleAsync<int>(command);

                var parameters2 = new { OrgID = orgID, org.AppUserID };
                var sql2 = @"INSERT INTO userorgs (appuserid, orgid)
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

        public async Task<int> UpdateOrg(Org org, string userId, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (org == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { Id = org.ID, org.Name, org.Description, AppUserId = userId };
                var sql = "UPDATE orgs SET name = @Name, description = @Description WHERE id = @Id AND appuserid = @AppUserId;";

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

        public async Task<int> DeleteOrg(Org org, string userId, CancellationToken ct, NpgsqlTransaction? tx = null)
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
                var sql1 = "DELETE FROM userorgs WHERE orgid = @Id;";

                var command = new CommandDefinition(
                    sql1,
                    parameters1,
                    cancellationToken: ct,
                    transaction: tran
                );

                await Connection.ExecuteAsync(command);

                var parameters2 = new { Id = org.ID };
                var sql2 = "DELETE FROM orgs WHERE id = @Id;";

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