
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public interface IRevenuesService
    {
        Task<GetRevenuesDTO> GetRevenues(DateTime? date, string timeZoneId, string userId, int orgId, CancellationToken ct);
        GetRevenueDTO GetNewRevenue(string userId, string timeZoneId, DateTime? date);
        Task<SaveRevenueDTO> SaveRevenue(Revenue revenue, string userId, CancellationToken ct);
        Task<UpdateRevenueDTO> UpdateRevenue(Revenue updatedRevenue, string userId, CancellationToken ct);
        Task<GetRevenueDTO> GetRevenueById(int? id, string userId, CancellationToken ct);
        Task<GetRevenueDTO> GetRevenueAndRelatedFieldsById(int? id, string userId, string timeZoneId, CancellationToken ct);
        Task<DeleteRevenueDTO> DeleteRevenue(int? id, string userId, CancellationToken ct);
    }
}
