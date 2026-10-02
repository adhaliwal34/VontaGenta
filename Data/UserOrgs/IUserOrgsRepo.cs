using Npgsql;
using VontaGenta.Entities;

namespace VontaGenta.Data
{
    public interface IUserOrgsRepo
    {
        Task<List<UserOrg>?> GetUsersInOrg(int orgId, CancellationToken ct);
        Task<bool> IsOrgMember(string userId, int orgId, CancellationToken ct);
        Task<int> LeaveOrg(string userId, int orgId, CancellationToken ct, NpgsqlTransaction? tx = null);
        Task<Org?> GetActiveOrg(string userId, CancellationToken ct);
        Task<int> SetActiveOrg(string userId, int orgId, CancellationToken ct, NpgsqlTransaction? tx = null);
    }
}
