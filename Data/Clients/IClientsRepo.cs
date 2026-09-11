using Npgsql;
using VontaGenta.Entities;

namespace VontaGenta.Data
{
    public interface IClientsRepo
    {
        Task<List<Client>> GetClientsForMemberOrg(int orgId, string userId, CancellationToken ct);
        Task<List<Client>> GetClientsForOrg(int orgId, CancellationToken ct);
        Task<Client?> GetClient(int id, CancellationToken ct);
        Task<Client?> GetClientAndRelatedFields(int id, CancellationToken ct);
        Task<int> SaveClient(Client client, string userId, CancellationToken ct, NpgsqlTransaction? tx = null);
        Task<int> UpdateClient(Client client, string userId, CancellationToken ct, NpgsqlTransaction? tx = null);

        /// <summary>
        /// Delete a client asynchronously.
        /// </summary>
        /// <param name="client">The client instance to delete. Only ID needs to be populated.</param>
        /// <param name="userId">The ID of the user executing the command.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <param name="tx">The db transaction. Should be created with UnitOfWork.BeginTransactionAsync().</param>
        /// <returns>1 for success. 0 or lower for errors.</returns>
        Task<int> DeleteClient(Client client, string userId, CancellationToken ct, NpgsqlTransaction? tx = null);
        Task<bool> AllowGetClient(int clientId, string userId, CancellationToken ct);
        Task<bool> AllowUpdateOrCreateClient(Client client, string userId, CancellationToken ct);
    }
}