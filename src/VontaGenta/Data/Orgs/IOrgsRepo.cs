using Npgsql;
using VontaGenta.Entities;

namespace VontaGenta.Data
{
    public interface IOrgsRepo
    {
        Task<List<Org>?> GetOrgs(string userId, CancellationToken ct);
        Task<Org?> GetOrg(int orgId, string userId, CancellationToken ct);
        Task<Org?> GetOrgAndRelatedFields(int orgId, string userId, CancellationToken ct);
        Task<int> SaveOrg(Org org, CancellationToken ct, NpgsqlTransaction? tx = null);
        Task<int> UpdateOrg(Org org, string userId, CancellationToken ct, NpgsqlTransaction? tx = null);
        Task<int> DeleteOrg(Org org, string userId, CancellationToken ct, NpgsqlTransaction? tx = null);
    }
}
