using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public interface ICostEntriesService
    {
        Task<DeleteCostEntryDTO> DeleteCostEntry(int id, string userId, CancellationToken ct);
        Task<GetTotalCountCostEntryDTO> GetTotalCountCostEntriesForMemberOrg(string userId, int orgId, int year, int month, CancellationToken ct);
        Task<GetCostEntriesDTO> GetCostEntriesForMemberOrg(string userId, int orgId, int year, int month, int page, int itemsPerPage, CancellationToken ct);
        Task<GetCostEntryDTO> GetCostEntryAndRelatedFields(int id, string userId, CancellationToken ct);
        Task<SaveCostEntryDTO> SaveCostEntry(CostEntry costEntry, string userId, CancellationToken ct);
        Task<UpdateCostEntryDTO> UpdateCostEntry(CostEntry costEntry, string userId, CancellationToken ct);
    }
}
