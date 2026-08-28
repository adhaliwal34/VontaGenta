using VontaGenta.Data;
using VontaGenta.Entities;
using AutoMapper;
using System.Data;

namespace VontaGenta.Services
{
    public class CostEntriesService : ICostEntriesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CostEntriesService(IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<GetTotalCountCostEntryDTO> GetTotalCountCostEntriesForMemberOrg(string userId, int orgId, int year, int month, CancellationToken ct)
        {
            var dto = new GetTotalCountCostEntryDTO();

            dto.Total = await _unitOfWork.CostEntries.GetTotalCountCostEntriesForMemberOrg(userId, orgId, year, month, ct);

            if (dto.Total == -1)
            {
                dto.Error = "Error: Getting total count.";
            }

            return dto;
        }

        public async Task<GetCostEntriesDTO> GetCostEntriesForMemberOrg(string userId, int orgId, int year, int month, int page, int itemsPerPage, CancellationToken ct)
        {
            var dto = new GetCostEntriesDTO();

            dto.List = await _unitOfWork.CostEntries.GetCostEntriesForMemberOrg(userId, orgId, year, month, page, itemsPerPage, ct);

            return dto;
        }

        public async Task<GetCostEntryDTO> GetCostEntryAndRelatedFields(int id, string userId, CancellationToken ct)
        {
            var dto = new GetCostEntryDTO();

            if (!await _unitOfWork.CostEntries.AllowGetCostEntry(id, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            dto.CostEntry = await _unitOfWork.CostEntries.GetCostEntryAndRelatedFields(id, userId, ct);

            if (dto.CostEntry == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            return dto;
        }

        public async Task<SaveCostEntryDTO> SaveCostEntry(CostEntry costEntry, string userId, CancellationToken ct)
        {
            var dto = new SaveCostEntryDTO();

            if (costEntry.AppUserID != userId)
            {
                dto.Error = "Error: Cannot create cost entries for other users";
                return dto;
            }

            if (!await _unitOfWork.Projects.AllowGetProject(costEntry.ProjectID, userId, ct))
            {
                dto.Error = "Error: Project not found.";
                return dto;
            }

            var project = await _unitOfWork.Projects.GetProjectAndRelatedFields(costEntry.ProjectID, ct);

            if (!await _unitOfWork.CostCategories.AllowGetCostCategory(costEntry.CategoryID, userId, ct))
            {
                dto.Error = "Error: Category not found.";
                return dto;
            }

            var category = await _unitOfWork.CostCategories.GetCategory(costEntry.CategoryID, ct);

            if (category == null || project == null)
            {
                dto.Error = "Error: Project or category not found.";
                return dto;
            }

            if (category.OrgID != project.Client.OrgID || !await _unitOfWork.UserOrgs.IsOrgMember(userId, project.Client.OrgID, ct))
            {
                dto.Error = "Error: Org error.";
                return dto;
            }

            costEntry.Project = project;
            costEntry.Category = category;

            if (!await _unitOfWork.CostEntries.AllowCreateCostEntry(costEntry, userId, project.Client.OrgID, ct))
            {
                dto.Error = "Error: Action not allowed";
                return dto;
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                int result = await _unitOfWork.CostEntries.SaveCostEntry(costEntry, ct);

                if (result > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.SavedID = result;
                    dto.Status = "Item saved";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Save failed";
                    return dto;
                }
            }
            catch(Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                //TODO log error
                dto.Error = "Error on save";
                return dto;
            }

            return dto;
        }

        public async Task<UpdateCostEntryDTO> UpdateCostEntry(CostEntry costEntry, string userId, CancellationToken ct)
        {
            var dto = new UpdateCostEntryDTO();

            if (!await _unitOfWork.CostEntries.AllowGetCostEntry(costEntry.ID, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            // Getting project details to check org membership.
            var storedCostEntry = await _unitOfWork.CostEntries.GetCostEntryAndRelatedFields(costEntry.ID, userId, ct);

            if (storedCostEntry == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            // Get the org Id before the project is possibly changed on the object during mapping.
            var originalOrgID = storedCostEntry.Project.Client.OrgID;

            // Check that the new project is in the same org as old project.
            var project = await _unitOfWork.Projects.GetProjectAndRelatedFields(costEntry.ProjectID, ct);

            if (project == null)
            {
                dto.Error = "Error: Project setting error";
                return dto;
            }

            if (project.Client.OrgID != originalOrgID)
            {
                dto.Error = "Error: Project setting error";
                return dto;
            }

            // Mapping should never overwrite user
            _mapper.Map(costEntry, storedCostEntry);

            if (!await _unitOfWork.CostEntries.AllowUpdateCostEntry(storedCostEntry.ID, userId, project.Client.OrgID, ct))
            {
                dto.Error = "Error: Action not allowed";
                return dto;
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                var affectedRows = await _unitOfWork.CostEntries.UpdateCostEntry(storedCostEntry, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Item updated";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Update failed";
                    // TODO log error
                    return dto;
                }
            }
            catch(Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                //TODO log error
                dto.Error = "Error: Error on update";
                return dto;
            }

            return dto;
        }

        public async Task<DeleteCostEntryDTO> DeleteCostEntry(int id, string userId, CancellationToken ct)
        {
            var dto = new DeleteCostEntryDTO();

            if (!await _unitOfWork.CostEntries.AllowGetCostEntry(id, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            var costEntry = await _unitOfWork.CostEntries.GetCostEntryAndRelatedFields(id, userId, ct);

            if (costEntry == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            if (!await _unitOfWork.CostEntries.AllowUpdateCostEntry(costEntry.ID, userId, costEntry.Project.Client.OrgID, ct))
            {
                dto.Error = "Error: Action not allowed";
                return dto;
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                var affectedRows = await _unitOfWork.CostEntries.DeleteCostEntry(costEntry, ct);

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
