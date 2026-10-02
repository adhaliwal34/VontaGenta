using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public interface ICostCategoriesService
    {
        Task<DeleteCategoryDTO> DeleteCategory(int id, string userId, CancellationToken ct);
        Task<GetCategoriesDTO> GetCategoriesForMemberOrg(string userId, int orgId, CancellationToken ct);
        Task<GetCategoryDTO> GetCategory(int id, string userId, CancellationToken ct);
        Task<SaveCategoryDTO> SaveCategory(Category category, string userId, CancellationToken ct);
        Task<UpdateCategoryDTO> UpdateCategory(int id, Category category, string userId, CancellationToken ct);
    }
}
