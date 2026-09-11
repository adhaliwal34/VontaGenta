using VontaGenta.Entities;
using Npgsql;

namespace VontaGenta.Data
{
    public interface IOrgInvitesRepo
    {
        Task<List<OrgInvite>?> GetOrgInvites(int id, string userId, CancellationToken ct);
        Task<int> SaveOrgInvites(List<OrgInvite> orgInvites, CancellationToken ct, NpgsqlTransaction? tx = null);
        Task<List<OrgInvite>?> GetAllInvitesToUser(string userId, CancellationToken ct);
        Task<int> AcceptOrgInvite(OrgInvite orgInvite, string userId, CancellationToken ct, NpgsqlTransaction? tx = null);
        Task<int> DeleteOrgInvite(OrgInvite orgInvite, string userId, CancellationToken ct, NpgsqlTransaction? tx = null);
        Task<OrgInvite?> GetOrgInvite(int orgInviteId, string userId, CancellationToken ct);
    }
}