using VontaGenta.Entities;
using MySqlConnector;

namespace VontaGenta.Data
{
    public interface ICostCategoriesRepo
    {
        Task<int> DeleteCategory(Category category, CancellationToken ct, MySqlTransaction? tx = null);
        Task<List<Category>> GetCategoriesForMemberOrg(string userId, int orgId, CancellationToken ct);
        Task<Category?> GetCategory(int orgId, CancellationToken ct);
        Task<int> SaveCategory(Category category, CancellationToken ct, MySqlTransaction? tx = null);
        Task<int> UpdateCategory(Category category, CancellationToken ct, MySqlTransaction? tx = null);
        Task<bool> AllowGetCostCategory(int categoryId, string userId, CancellationToken ct);
        Task<bool> AllowCreateCostCategory(Category category, string userId, CancellationToken ct);
    }
}