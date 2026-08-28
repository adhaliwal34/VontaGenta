using VontaGenta.Entities;
using MySqlConnector;

namespace VontaGenta.Data
{
    public interface IOrgsRepo
    {
        Task<List<Org>?> GetOrgs(string userId, CancellationToken ct);
        Task<Org?> GetOrg(int orgId, string userId, CancellationToken ct);
        Task<Org?> GetOrgAndRelatedFields(int orgId, string userId, CancellationToken ct);
        Task<int> SaveOrg(Org org, CancellationToken ct, MySqlTransaction? tx = null);
        Task<int> UpdateOrg(Org org, string userId, CancellationToken ct, MySqlTransaction? tx = null);
        Task<int> DeleteOrg(Org org, string userId, CancellationToken ct, MySqlTransaction? tx = null);
    }
}