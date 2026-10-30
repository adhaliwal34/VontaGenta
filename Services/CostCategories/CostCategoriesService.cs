using System.Data;
using VontaGenta.Data;
using VontaGenta.Entities;
using VontaGenta.Mapping;

namespace VontaGenta.Services
{
    public class CostCategoriesService : ICostCategoriesService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CostCategoriesService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetCategoriesDTO> GetCategoriesForMemberOrg(string userId, int orgId, CancellationToken ct)
        {
            var dto = new GetCategoriesDTO();

            dto.List = await _unitOfWork.CostCategories.GetCategoriesForMemberOrg(userId, orgId, ct);

            return dto;
        }

        public async Task<GetCategoryDTO> GetCategory(int id, string userId, CancellationToken ct)
        {
            var dto = new GetCategoryDTO();

            if (!await _unitOfWork.CostCategories.AllowGetCostCategory(id, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            dto.Category = await _unitOfWork.CostCategories.GetCategory(id, ct);

            if (dto.Category == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            return dto;
        }

        public async Task<SaveCategoryDTO> SaveCategory(Category category, string userId, CancellationToken ct)
        {
            var dto = new SaveCategoryDTO();

            if (category.OrgID == 0)
            {
                dto.Error = "Error: Org must be selected.";
                return dto;
            }

            if (!await _unitOfWork.CostCategories.AllowCreateCostCategory(category, userId, ct))
            {
                dto.Error = "Error: Action not allowed.";
                return dto;
            }

            if (category.AppUserID != userId)
            {
                dto.Error = "Error: Cannot create categories for other users";
                return dto;
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                var affectedRows = await _unitOfWork.CostCategories.SaveCategory(category, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Item saved";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Save failed";
                    return dto;
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                //TODO log error
                dto.Error = "Error on save";
                return dto;
            }

            return dto;
        }

        public async Task<UpdateCategoryDTO> UpdateCategory(Category category, string userId, CancellationToken ct)
        {
            var dto = new UpdateCategoryDTO();

            var storedCategory = await _unitOfWork.CostCategories.GetCategory(category.ID, ct);

            if (storedCategory == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            category.OrgID = storedCategory.OrgID;

            // Any org member can edit a cost category in their org.
            if (!await _unitOfWork.CostCategories.AllowCreateCostCategory(category, userId, ct))
            {
                dto.Error = "Error: Action not allowed.";
                return dto;
            }

            category.MapTo(storedCategory);

            try
            {
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                var affectedRows = await _unitOfWork.CostCategories.UpdateCategory(storedCategory, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Item updated";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Update failed";
                    return dto;
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                // TODO log error
                dto.Error = "Error: Error on update";
                return dto;
            }

            return dto;
        }

        public async Task<DeleteCategoryDTO> DeleteCategory(int id, string userId, CancellationToken ct)
        {
            var dto = new DeleteCategoryDTO();

            var category = await _unitOfWork.CostCategories.GetCategory(id, ct);

            if (category == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            // Any org member can delete a cost category in their org.
            if (!await _unitOfWork.CostCategories.AllowCreateCostCategory(category, userId, ct))
            {
                dto.Error = "Error: Action not allowed.";
                return dto;
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
                var affectedRows = await _unitOfWork.CostCategories.DeleteCategory(category, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Item deleted";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Failed to delete item. There might be existing data associated to this item which is blocking the deletion.";
                    return dto;
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                // TODO log error
                dto.Error = "Error: Error on delete";
                return dto;
            }

            return dto;
        }
    }
}
