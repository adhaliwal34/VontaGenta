using Dapper;
using VontaGenta.AppEnums;
using VontaGenta.Entities;
using MySqlConnector;
using Newtonsoft.Json;

namespace VontaGenta.Data
{
    public class ClientsRepo : RepoBase, IClientsRepo
    {
        private const string EntityName = "Client";

        public ClientsRepo(IUnitOfWork uow, ILogger<ClientsRepo> logger)
        : base(uow, logger)
        {
            _logger.LogInformation("ClientsRepo instantiated");
        }

        /*
         * Get the clients created for an org the user is a member of.
         * If they created a client for an org but now are no longer
         * a member of the org, do not show that client.
         *
         * Note, the permissions for which clients a user can see are
         * hard-coded into the SQL.
         *
         * Are the permissions actually hard-coded? The method is just doing
         * its job of getting clients for an org the user is a member of.
         * It's just another method for lower layers to call. It doesn't specify
         * that this is the only permission that must be checked. More can be
         * written.
         *
        */
        public async Task<List<Client>> GetClientsForMemberOrg(int orgId, string userId, CancellationToken ct)
        {

            try
            {
                var parameters = new { AppUserId = userId, OrgId = orgId };
                var sql = @"SELECT
                        Clients.*
                        ,AspNetUsers.*
                        ,Orgs.*
                        FROM Clients
                        INNER JOIN AspNetUsers ON AspNetUsers.ID = Clients.AppUserID
                        INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                        INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                        WHERE UserOrgs.AppUserID = @AppUserId
                            AND Orgs.ID = @OrgID
                        ORDER BY Orgs.Name, Clients.Name;";

                var command = new CommandDefinition(
                                    sql,
                                    parameters,
                                    cancellationToken: ct
                                );

                return (await Connection.QueryAsync<Client, AppUser, Org, Client>(command, (client, appUser, org) =>
                {
                    client.User = appUser;
                    client.Org = org;
                    return client;
                })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return new List<Client>();
            }
        }

        // Does not check for membership of org.
        public async Task<List<Client>> GetClientsForOrg(int orgId, CancellationToken ct)
        {
            try
            {
                var parameters = new { OrgID = orgId };
                var sql = @"SELECT
                        Clients.*
                        ,AspNetUsers.*
                        ,Orgs.*
                        FROM Clients
                        INNER JOIN AspNetUsers ON AspNetUsers.ID = Clients.AppUserID
                        INNER JOIN Orgs ON Clients.OrgID = Orgs.ID
                        WHERE Clients.OrgID = @OrgID
                        ORDER BY Orgs.Name, Clients.Name;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Client, AppUser, Org, Client>(command, (client, appUser, org) =>
                {
                    client.User = appUser;
                    client.Org = org;
                    return client;
                })).ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return new List<Client>();
            }
        }

        public async Task<Client?> GetClient(int id, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = id };
                var sql = @"SELECT
                            Clients.*
                            FROM Clients
                            WHERE Clients.ID = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Client>(command)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        // TODO currently only client creator can get the client.
        public async Task<Client?> GetClientAndRelatedFields(int id, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = id };
                var sql = @"SELECT
                            Clients.*
                            ,Orgs.*
                            ,AspNetUsers.*
                            FROM Clients
                            INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                            INNER JOIN AspNetUsers ON AspNetUsers.ID = Clients.AppUserID
                            WHERE Clients.ID = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Client, Org, AppUser, Client>(command, (client, org, appUser) =>
                {
                    client.User = appUser;
                    client.Org = org;
                    return client;
                })).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        public async Task<int> SaveClient(Client client, string userId, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (client == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { client.Name, client.Description, client.AppUserID, client.OrgID };
                var sql = @"INSERT Clients (Name, Description, AppUserID, OrgID)
                                VALUES (@Name, @Description, @AppUserId, @OrgId); SELECT LAST_INSERT_ID();";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct,
                    transaction: tran
                );

                var insertedId = await Connection.QuerySingleAsync<int>(command);

                // Set ID before serialisation
                client.ID = insertedId;

                var dataLog = new DataLog
                {
                    EntityName = EntityName,
                    EntityID = insertedId,
                    Action = AppActions.Create,
                    Timestamp = DateTime.UtcNow,
                    AppUserID = userId,
                    Json = JsonConvert.SerializeObject(client)
                };

                var parameters2 = new { dataLog.EntityName, dataLog.EntityID, dataLog.Action, dataLog.Timestamp, dataLog.AppUserID, dataLog.Json };
                var sql2 = @"INSERT DataLogs (EntityName, EntityID, Action, Timestamp, AppUserID, Json)
                                VALUES (@EntityName, @EntityID, @Action, @Timestamp, @AppUserID, @Json);";

                var command2 = new CommandDefinition(
                    sql2,
                    parameters2,
                    cancellationToken: ct,
                    transaction: tran
                );

                await Connection.ExecuteAsync(command2);

                return insertedId;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return -2;
            }
        }

        public async Task<int> UpdateClient(Client client, string userId, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (client == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var dataLog = new DataLog
                {
                    EntityName = EntityName,
                    EntityID = client.ID,
                    Action = AppActions.Edit,
                    Timestamp = DateTime.UtcNow,
                    AppUserID = userId,
                    Json = JsonConvert.SerializeObject(client)
                };

                var parameters = new { client.ID, client.Name, client.Description, client.OrgID, dataLog.EntityName, dataLog.EntityID, dataLog.Action, dataLog.Timestamp, dataLog.AppUserID, dataLog.Json };
                var sql = @"UPDATE Clients
                            SET Name = @Name, Description = @Description, OrgID = @OrgID
                            WHERE Clients.ID = @ID;

                            INSERT DataLogs (EntityName, EntityID, Action, Timestamp, AppUserID, Json)
                            VALUES (@EntityName, @EntityID, @Action, @Timestamp, @AppUserID, @Json);";

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

        public async Task<int> DeleteClient(Client client, string userId, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (client == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var dataLog = new DataLog
                {
                    EntityName = EntityName,
                    EntityID = client.ID,
                    Action = AppActions.Delete,
                    Timestamp = DateTime.UtcNow,
                    AppUserID = userId,
                    Json = JsonConvert.SerializeObject(client)
                };

                var parameters = new { client.ID, dataLog.EntityName, dataLog.EntityID, dataLog.Action, dataLog.Timestamp, dataLog.AppUserID, dataLog.Json };
                var sql = @"DELETE FROM Clients WHERE Clients.ID = @ID;

                            INSERT DataLogs (EntityName, EntityID, Action, Timestamp, AppUserID, Json)
                            VALUES (@EntityName, @EntityID, @Action, @Timestamp, @AppUserID, @Json);";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct,
                    transaction: tran
                );

                var affectedRows = await Connection.ExecuteAsync(command);

                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return 0;
            }
        }

        #region Permissions

        public async Task<bool> AllowGetClient(int clientId, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { ClientId = clientId, AppUserId = userId };
                var sql = @"SELECT
                                1
                                FROM Clients
                                INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                                WHERE UserOrgs.AppUserID = @AppUserId
								    AND Clients.ID = @ClientId;";

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

        public async Task<bool> AllowUpdateOrCreateClient(Client client, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { AppUserId = userId, OrgId = client.OrgID };
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

        #endregion
    }
}
// cSpell:ignore tran