using VontaGenta.Entities;
using Dapper;
using Npgsql;

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
                        userorgs.*, aspnetusers.*
                        FROM userorgs
                        INNER JOIN aspnetusers ON aspnetusers.id = userorgs.appuserid
                        WHERE userorgs.orgid = @OrgID;";

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
                            true
                            FROM userorgs
                            WHERE userorgs.appuserid = @AppUserId
                            AND userorgs.orgid = @OrgId;";

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

        public async Task<int> LeaveOrg(string userId, int orgId, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (orgId == 0)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { Id = orgId, AppUserId = userId };
                var sql = @"DELETE FROM userorgs
                                WHERE
                                orgid = @Id
                                AND appuserid = @AppUserId;";

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
                            orgs.*
                            ,aspnetusers.*
                            FROM userorgs
                            INNER JOIN orgs ON orgs.id = userorgs.orgid
                            INNER JOIN aspnetusers ON aspnetusers.id = orgs.appuserid
                            WHERE userorgs.appuserid = @AppUserID
                            AND userorgs.isactive = true;";

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

        public async Task<int> SetActiveOrg(string userId, int orgId, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            try
            {
                var tran = tx ?? Transaction;

                // Set current active user org to not active.
                var parameters1 = new { AppUserID = userId };
                var sql1 = @"UPDATE userorgs SET isactive = false
                            WHERE appuserid = @AppUserID
                            AND isactive = true;";

                var command = new CommandDefinition(
                    sql1,
                    parameters1,
                    cancellationToken: ct,
                    transaction: tran
                );

                await Connection.ExecuteAsync(command);

                // Set new user org to active
                var parameters2 = new { AppUserID = userId, OrgID = orgId };
                var sql2 = @"UPDATE userorgs SET isactive = true
                            WHERE appuserid = @AppUserID
                            AND orgid = @OrgID;";

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