using Npgsql;
using VontaGenta.Entities;

namespace VontaGenta.Data
{
    public interface IClientsRepo
    {
        Task<List<Client>> GetClientsForOrg(int activeOrgId, string userId, CancellationToken ct, NpgsqlTransaction? tx = null);
        Task<Client?> GetClient(int id, CancellationToken ct, NpgsqlTransaction? tx = null);
        Task<Client?> GetClientAndRelatedFields(int id, CancellationToken ct, NpgsqlTransaction? tx = null);
        Task<int> SaveClient(Client client, string userId, CancellationToken ct, NpgsqlTransaction? tx = null);
        Task<int> UpdateClient(Client client, string userId, CancellationToken ct, NpgsqlTransaction? tx = null);

        /// <summary>
        /// Delete a client asynchronously.
        /// </summary>
        /// <param name="clientId">The client ID to delete.</param>
        /// <param name="userId">The ID of the user executing the command.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <param name="tx">The db transaction. Should be created with UnitOfWork.BeginTransactionAsync().</param>
        /// <returns>1 for success. 0 or lower for errors.</returns>
        Task<int> DeleteClient(int clientId, string userId, CancellationToken ct, NpgsqlTransaction? tx = null);
    }
}
