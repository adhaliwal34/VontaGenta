using System.Data;
using AutoMapper;
using VontaGenta.Data;
using VontaGenta.Entities;


namespace VontaGenta.Services
{
    public class OrgsService : IOrgsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrgsService(IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<GetOrgsDTO> GetOrgs(string userId, CancellationToken ct)
        {
            var dto = new GetOrgsDTO();

            var orgs = await _unitOfWork.Orgs.GetOrgs(userId, ct);

            if (orgs == null)
            {
                dto.Error = "Failed to get orgs";
                return dto;
            }

            _mapper.Map(orgs, dto.List);

            var activeOrg = await _unitOfWork.UserOrgs.GetActiveOrg(userId, ct);

            if (activeOrg != null)
            {
                for (int i = 0; i < dto.List.Count; i++)
                {
                    if (dto.List[i].ID == activeOrg.ID)
                    {
                        dto.List[i].IsActive = true;
                        dto.ListHasActiveOrg = true;
                    }
                }
            }

            return dto;
        }

        public async Task<GetOrgDTO> GetOrg(int id, string userId, CancellationToken ct)
        {
            var dto = new GetOrgDTO
            {
                Org = await _unitOfWork.Orgs.GetOrg(id, userId, ct)
            };

            if (dto.Org == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            return dto;
        }

        public async Task<GetOrgDTO> GetOrgAndRelatedFields(int id, string userId, CancellationToken ct)
        {
            var dto = new GetOrgDTO();

            dto.Org = await _unitOfWork.Orgs.GetOrgAndRelatedFields(id, userId, ct);

            if (dto.Org == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            return dto;
        }

        public async Task<SaveOrgDTO> SaveOrg(Org org, string userId, CancellationToken ct)
        {
            var dto = new SaveOrgDTO();

            if (org.AppUserID != userId)
            {
                dto.Error = "Error: Cannot create orgs for other users";
                return dto;
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                int affectedRows = await _unitOfWork.Orgs.SaveOrg(org, ct);

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

        public async Task<UpdateOrgDTO> UpdateOrg(int id, Org org, string userId, CancellationToken ct)
        {
            var dto = new UpdateOrgDTO();

            var storedOrg = await _unitOfWork.Orgs.GetOrg(id, userId, ct);

            if (storedOrg == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            // Mapping should never overwrite user
            _mapper.Map(org, storedOrg);

            try
            {
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                int affectedRows = await _unitOfWork.Orgs.UpdateOrg(storedOrg, userId, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Item updated";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Error: Error on update";
                    return dto;
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                //TODO log error
                dto.Error = "Error: Error on update";
                return dto;
            }

            return dto;
        }

        public async Task<DeleteOrgDTO> DeleteOrg(int id, string userId, CancellationToken ct)
        {
            var dto = new DeleteOrgDTO();

            var org = await _unitOfWork.Orgs.GetOrg(id, userId, ct);

            if (org == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            if (org.AppUserID != userId)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                int affectedRows = await _unitOfWork.Orgs.DeleteOrg(org, userId, ct);

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
                //TODO log error
                dto.Error = "Error on delete";
                return dto;
            }

            return dto;
        }

    }
}
