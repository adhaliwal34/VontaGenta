using Dapper;
using Newtonsoft.Json;
using Npgsql;
using VontaGenta.AppEnums;
using VontaGenta.Entities;

namespace VontaGenta.Data
{
    public class ClientsRepo : RepoBase, IClientsRepo
    {
        private const string EntityName = "Client";

        public ClientsRepo(IUnitOfWork uow, ILogger<ClientsRepo> logger)
        : base(uow, logger)
        {
            Logger.LogInformation("ClientsRepo instantiated");
        }

        public async Task<List<Client>> GetClientsForOrg(int orgId, string userId, CancellationToken ct, NpgsqlTransaction? tx = null)
        {

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { OrgId = orgId };
                var sql = @"SELECT
                        clients.*
                        ,aspnetusers.*
                        ,orgs.*
                        FROM clients
                        INNER JOIN aspnetusers ON aspnetusers.id = clients.appuserid
                        INNER JOIN orgs ON orgs.id = clients.orgid
                        WHERE clients.orgid = @OrgID
                        ORDER BY orgs.name, clients.name;";

                var command = new CommandDefinition(
                                    sql,
                                    parameters,
                                    cancellationToken: ct,
                                    transaction: tran
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
                Logger.LogError("Error: {Exception}", ex);
                throw;
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
                Logger.LogError("Error: {Exception}", ex);
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
                Logger.LogError("Error: {Exception}", ex);
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
                Logger.LogError("Error: {Exception}", ex);
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
                Logger.LogError("Error: {Exception}", ex);
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
                Logger.LogError("Error: {Exception}", ex);
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
                Logger.LogError("Error: {Exception}", ex);
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
                Logger.LogError("Error: {Exception}", ex);
                return false;
            }
        }

        #endregion
    }
}
// cSpell:ignore tran
