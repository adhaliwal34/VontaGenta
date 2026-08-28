using VontaGenta.Entities;
using Dapper;
using MySqlConnector;
using VontaGenta.Helpers;

namespace VontaGenta.Data
{
    public class OrgInvitesRepo : RepoBase, IOrgInvitesRepo
    {

        public OrgInvitesRepo(IUnitOfWork uow, ILogger<OrgInvitesRepo> logger)
        : base(uow, logger)
        {
           _logger.LogInformation("OrgInvitesRepo instantiated");
        }

        // Get all invites to a org user is creator of.
        // id is an Org id.
        public async Task<List<OrgInvite>?> GetOrgInvites(int id, string userId, CancellationToken ct)
        {
            try
            {
                // TODO move authorisation checks to service
                // Checking User is a member of the org.
                var parameters1 = new { Id = id, AppUserID = userId };
                var sql1 = @"SELECT
                             1
                             FROM UserOrgs
                             WHERE OrgID = @Id AND AppUserID = @AppUserID;";

                var command = new CommandDefinition(
                    sql1,
                    parameters1,
                    cancellationToken: ct
                );

                var result = Connection.ExecuteScalar<int>(command);

                if (result != 1)
                {
                    return null;
                }

                var parameters2 = new { Id = id };
                var sql2 = @"SELECT
                            *
                            FROM OrgInvites
                            WHERE OrgID = @Id;";

                command = new CommandDefinition(
                    sql2,
                    parameters2,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<OrgInvite>(command)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        public async Task<OrgInvite?> GetOrgInvite(int orgInviteId, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { OrgInviteId = orgInviteId, AppUserId = userId };
                var sql = @"SELECT
                            *
                            FROM OrgInvites
                            INNER JOIN Orgs ON Orgs.ID = OrgInvites.OrgID
                            INNER JOIN AspNetUsers ON AspNetUsers.ID = Orgs.AppUserID
                            WHERE
                            OrgInvites.ToUserID = @AppUserId
                            AND OrgInvites.ID = @OrgInviteId;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<OrgInvite, Org, AppUser, OrgInvite>(command, (orgInvite, org, appUser) =>
                {
                    org.User = appUser;
                    orgInvite.Org = org;
                    return orgInvite;
                })).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        // Get all invites sent to a user.
        public async Task<List<OrgInvite>?> GetAllInvitesToUser(string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { AppUserId = userId };
                var sql = @"SELECT
                            *
                            FROM OrgInvites
                            INNER JOIN Orgs ON Orgs.ID = OrgInvites.OrgID
                            INNER JOIN AspNetUsers ON AspNetUsers.ID = Orgs.AppUserID
                            WHERE
                            OrgInvites.ToUserID = @AppUserId;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<OrgInvite, Org, AppUser, OrgInvite>(command, (orgInvite, org, appUser) =>
                {
                    org.User = appUser;
                    orgInvite.Org = org;
                    return orgInvite;
                })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        public async Task<int> SaveOrgInvites(List<OrgInvite> orgInvites, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (orgInvites.Count == 0)
            {
                // Not an error.
                return 0;
            }

            try
            {

                var tran = tx ?? Transaction;

                var dapperHelper = new DapperHelper();
                var colNames = new List<string> { "ToUserID", "OrgID" };
                var sql = dapperHelper.PrepareBulkInsertStatement("OrgInvites", colNames, orgInvites.Count);

                // Prepare arguments
                var queryParams = new DynamicParameters();
                for (int i = 0; i < orgInvites.Count; i++)
                {
                    queryParams.Add($"ToUserID{i}", orgInvites[i].ToUserID, System.Data.DbType.Guid);
                    queryParams.Add($"OrgID{i}", orgInvites[i].OrgID, System.Data.DbType.Int32);
                }

                var command = new CommandDefinition(
                    sql,
                    queryParams,
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

        public async Task<int> AcceptOrgInvite(OrgInvite orgInvite, string userId, CancellationToken ct, MySqlTransaction? tx = null)
        {
            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { AppUserID = userId, orgInvite.OrgID, orgInvite.ID };
                var sql = @"INSERT UserOrgs (AppUserID, OrgID)
                            VALUES (@AppUserID, @OrgID);

                            DELETE FROM OrgInvites
                            WHERE ID=@ID;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct,
                    transaction: tran
                );

                await Connection.ExecuteAsync(command);

                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return -2;
            }
        }

        public async Task<int> DeleteOrgInvite(OrgInvite orgInvite, string userId, CancellationToken ct, MySqlTransaction? tx = null)
        {
            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { ID = orgInvite.ID };
                var sql = @"DELETE FROM OrgInvites WHERE ID=@ID;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct,
                    transaction: tran
                );

                await Connection.ExecuteAsync(command);

                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return -2;
            }
        }
    }
}
// cSpell:ignore tran
