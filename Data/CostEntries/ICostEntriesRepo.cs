using VontaGenta.Entities;
using MySqlConnector;

namespace VontaGenta.Data
{
    public interface ICostEntriesRepo
    {
        Task<int> GetTotalCountCostEntriesForMemberOrg(string userId, int orgId, int year, int month, CancellationToken ct);
        Task<List<CostEntry>> GetCostEntriesForMemberOrg(string userId, int orgId, int year, int month, int page, int itemsPerPage, CancellationToken ct);
        Task<CostEntry?> GetCostEntry(int id, CancellationToken ct);
        Task<CostEntry?> GetCostEntryAndRelatedFields(int id, string userId, CancellationToken ct);
        Task<int> SaveCostEntry(CostEntry costEntry, CancellationToken ct, MySqlTransaction? tx = null);
        Task<int> UpdateCostEntry(CostEntry costEntry, CancellationToken ct, MySqlTransaction? tx = null);
        Task<int> DeleteCostEntry(CostEntry costEntry, CancellationToken ct, MySqlTransaction? tx = null);
        Task<bool> AllowGetCostEntry(int id, string userId, CancellationToken ct);
        Task<bool> AllowCreateCostEntry(CostEntry costEntry, string userId, int orgId, CancellationToken ct);
        Task<bool> AllowUpdateCostEntry(int id, string userId, int orgId, CancellationToken ct);
    }
}