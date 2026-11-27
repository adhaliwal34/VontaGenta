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

        public async Task<List<Client>> GetClientsForOrg(int activeOrgId, string userId, CancellationToken ct, NpgsqlTransaction? tx = null)
        {

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { OrgId = activeOrgId };
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

        public async Task<Client?> GetClientAndRelatedFields(int clientId, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { Id = clientId };
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
                    cancellationToken: ct,
                    transaction: tran
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
                throw;
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
                    Action = AppActions.Edit,
                    EntryTimestamp = DateTime.UtcNow,
                    AppUserID = userId
                };

                var parameters = new { client.ID, client.Name, client.Description, dataLog.EntityName, dataLog.Action, dataLog.EntryTimestamp, dataLog.AppUserID };
                var sql = @"WITH u AS (
                                UPDATE clients
                                SET name = @Name, description = @Description
                                WHERE id = @ID
                                RETURNING id, name, description, orgid, appuserid
                            )
                            INSERT INTO datalogs (entityname, entityid, action, entrytimestamp, appuserid, json)
                            SELECT @EntityName, u.id, @Action, @EntryTimestamp, @AppUserID,
                                   json_build_object('ID', u.id, 'Name', u.name, 'Description', u.description,
                                                     'OrgID', u.orgid, 'AppUserID', u.appuserid)::text
                            FROM u;";

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
                Logger.LogError("Error: {Exception}", ex);
                return -2;
            }
        }

        public async Task<int> DeleteClient(int clientId, string userId, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            try
            {
                var tran = tx ?? Transaction;

                var dataLog = new DataLog
                {
                    EntityName = EntityName,
                    Action = AppActions.Delete,
                    EntryTimestamp = DateTime.UtcNow,
                    AppUserID = userId
                };

                // Using Postgres's json_build_object() as we do not have a client object to serialise.
                var parameters = new { ID = clientId, dataLog.EntityName, dataLog.Action, dataLog.EntryTimestamp, dataLog.AppUserID };
                var sql = @"WITH d AS (
                                DELETE FROM clients WHERE id = @ID
                                RETURNING id, name, description, orgid, appuserid
                            )
                            INSERT INTO datalogs (entityname, entityid, action, entrytimestamp, appuserid, json)
                            SELECT @EntityName, d.id, @Action, @EntryTimestamp, @AppUserID,
                                   json_build_object('ID', d.id, 'Name', d.name, 'Description', d.description,
                                                     'OrgID', d.orgid, 'AppUserID', d.appuserid)::text
                            FROM d;";

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
                Logger.LogError("Error: {Exception}", ex);
                return 0;
            }
        }
    }
}
