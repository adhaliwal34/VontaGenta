using Dapper;
using VontaGenta.AppEnums;
using VontaGenta.Entities;
using Npgsql;
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
                        clients.*
                        ,aspnetusers.*
                        ,orgs.*
                        FROM clients
                        INNER JOIN aspnetusers ON aspnetusers.id = clients.appuserid
                        INNER JOIN orgs ON orgs.id = clients.orgid
                        INNER JOIN userorgs ON userorgs.orgid = clients.orgid
                        WHERE userorgs.appuserid = @AppUserId
                            AND orgs.id = @OrgID
                        ORDER BY orgs.name, clients.name;";

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
                        clients.*
                        ,aspnetusers.*
                        ,orgs.*
                        FROM clients
                        INNER JOIN aspnetusers ON aspnetusers.id = clients.appuserid
                        INNER JOIN orgs ON clients.orgid = orgs.id
                        WHERE clients.orgid = @OrgID
                        ORDER BY orgs.name, clients.name;";

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
                            clients.*
                            FROM clients
                            WHERE clients.id = @Id;";

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
                            clients.*
                            ,orgs.*
                            ,aspnetusers.*
                            FROM clients
                            INNER JOIN orgs ON orgs.id = clients.orgid
                            INNER JOIN aspnetusers ON aspnetusers.id = clients.appuserid
                            WHERE clients.id = @Id;";

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

        public async Task<int> SaveClient(Client client, string userId, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (client == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { client.Name, client.Description, client.AppUserID, client.OrgID };
                var sql = @"INSERT INTO clients (name, description, appuserid, orgid)
                                VALUES (@Name, @Description, @AppUserId, @OrgId) RETURNING id;";

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
                    EntryTimestamp = DateTime.UtcNow,
                    AppUserID = userId,
                    Json = JsonConvert.SerializeObject(client)
                };

                var parameters2 = new { dataLog.EntityName, dataLog.EntityID, dataLog.Action, dataLog.EntryTimestamp, dataLog.AppUserID, dataLog.Json };
                var sql2 = @"INSERT INTO datalogs (entityname, entityid, action, entrytimestamp, appuserid, json)
                                VALUES (@EntityName, @EntityID, @Action, @EntryTimestamp, @AppUserID, @Json);";

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

        public async Task<int> UpdateClient(Client client, string userId, CancellationToken ct, NpgsqlTransaction? tx = null)
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
                    EntryTimestamp = DateTime.UtcNow,
                    AppUserID = userId,
                    Json = JsonConvert.SerializeObject(client)
                };

                var parameters = new { client.ID, client.Name, client.Description, client.OrgID, dataLog.EntityName, dataLog.EntityID, dataLog.Action, dataLog.EntryTimestamp, dataLog.AppUserID, dataLog.Json };
                var sql = @"UPDATE clients
                            SET name = @Name, description = @Description, orgid = @OrgID
                            WHERE clients.id = @ID;

                            INSERT INTO datalogs (entityname, entityid, action, entrytimestamp, appuserid, json)
                            VALUES (@EntityName, @EntityID, @Action, @EntryTimestamp, @AppUserID, @Json);";

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

        public async Task<int> DeleteClient(Client client, string userId, CancellationToken ct, NpgsqlTransaction? tx = null)
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
                    EntryTimestamp = DateTime.UtcNow,
                    AppUserID = userId,
                    Json = JsonConvert.SerializeObject(client)
                };

                var parameters = new { client.ID, dataLog.EntityName, dataLog.EntityID, dataLog.Action, dataLog.EntryTimestamp, dataLog.AppUserID, dataLog.Json };
                var sql = @"DELETE FROM clients WHERE clients.id = @ID;

                            INSERT INTO datalogs (entityname, entityid, action, entrytimestamp, appuserid, json)
                            VALUES (@EntityName, @EntityID, @Action, @EntryTimestamp, @AppUserID, @Json);";

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
                                true
                                FROM clients
                                INNER JOIN userorgs ON userorgs.orgid = clients.orgid
                                WHERE userorgs.appuserid = @AppUserId
								    AND clients.id = @ClientId;";

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

        #endregion
    }
}
// cSpell:ignore tran