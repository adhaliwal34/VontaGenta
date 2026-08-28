using VontaGenta.Entities;
using MySqlConnector;

namespace VontaGenta.Data
{
    public interface IRevenuesRepo
    {
        Task<Revenue?> GetRevenueAndRelatedFields(int id, CancellationToken ct);
        Task<Revenue?> GetRevenueById(int id, CancellationToken ct);
        Task<IEnumerable<Revenue>> GetRevenuesAndRelatedFields(DateTime date, string userId, int orgId, CancellationToken ct);
        Task<int> SaveRevenue(Revenue revenue, CancellationToken ct, MySqlTransaction? tx = null);
        Task<int> UpdateRevenue(Revenue revenue, CancellationToken ct, MySqlTransaction? tx = null);
        Task<int> DeleteRevenue(int id, CancellationToken ct, MySqlTransaction? tx = null);
        Task<bool> CanGetRevenue(int id, string userId, CancellationToken ct);
    }
}