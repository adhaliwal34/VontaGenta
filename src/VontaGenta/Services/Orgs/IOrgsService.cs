using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public interface IOrgsService
    {
        Task<DeleteOrgDTO> DeleteOrg(int id, string userId, CancellationToken ct);
        Task<GetOrgDTO> GetOrg(int id, string userId, CancellationToken ct);
        Task<GetOrgDTO> GetOrgAndRelatedFields(int id, string userId, CancellationToken ct);
        Task<GetOrgsDTO> GetOrgs(string userId, CancellationToken ct);
        Task<SaveOrgDTO> SaveOrg(Org org, string userId, CancellationToken ct);
        Task<UpdateOrgDTO> UpdateOrg(Org org, string userId, CancellationToken ct);
    }
}
