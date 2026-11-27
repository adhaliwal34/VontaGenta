using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public interface IClientsService
    {
        Task<GetClientsDTO> GetClientsForOrg(int activeOrgId, string userId, CancellationToken ct);
        Task<GetClientDTO> GetClientAndRelatedFields(int activeOrgId, int id, CancellationToken ct);
        Task<SaveClientDTO> SaveClient(int activeOrgId, Client client, string userId, CancellationToken ct);
        Task<UpdateClientDTO> UpdateClient(int activeOrgId, Client client, string userId, CancellationToken ct);
        Task<DeleteClientDTO> DeleteClient(int activeOrgId, int id, string userId, CancellationToken ct);
    }
}
