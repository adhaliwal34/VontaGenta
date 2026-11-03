using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public interface IClientsService
    {
        Task<DeleteClientDTO> DeleteClient(int id, string userId, CancellationToken ct);
        Task<GetClientsDTO> GetClientsForMemberOrg(int orgId, string userId, CancellationToken ct);
        Task<GetClientDTO> GetClient(int id, string userId, CancellationToken ct);
        Task<GetClientDTO> GetClientAndRelatedFields(int id, string userId, CancellationToken ct);
        Task<SaveClientDTO> SaveClient(Client client, string userId, CancellationToken ct);
        Task<UpdateClientDTO> UpdateClient(Client client, string userId, CancellationToken ct);
    }
}
