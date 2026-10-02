using System.Data;
using VontaGenta.Data;

namespace VontaGenta.Services
{
    public class UserOrgsService : IUserOrgsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserOrgsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetUserOrgsDTO> GetUsersInOrg(int orgId, CancellationToken ct)
        {
            var dto = new GetUserOrgsDTO
            {
                List = await _unitOfWork.UserOrgs.GetUsersInOrg(orgId, ct)
            };

            return dto;
        }

        public async Task<IsOrgMemberDTO> IsOrgMember(string userId, int orgId, CancellationToken ct)
        {
            var dto = new IsOrgMemberDTO
            {
                IsOrgMember = await _unitOfWork.UserOrgs.IsOrgMember(userId, orgId, ct)
            };

            return dto;
        }

        public async Task<LeaveOrgDTO> LeaveOrg(string userId, int orgId, CancellationToken ct)
        {
            var dto = new LeaveOrgDTO();

            var isOrgMemberDto = new IsOrgMemberDTO
            {
                IsOrgMember = await _unitOfWork.UserOrgs.IsOrgMember(userId, orgId, ct)
            };

            if (!isOrgMemberDto.IsOrgMember)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                int affectedRows = await _unitOfWork.UserOrgs.LeaveOrg(userId, orgId, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Left organisation";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Failed to leave. There might be existing data associated to this org which is blocking leaving.";
                    return dto;
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                //TODO log error
                dto.Error = "Error on leaving org";
                return dto;
            }

            return dto;
        }

        public async Task<SetActiveOrgDTO> SetActiveOrg(string userId, int orgId, CancellationToken ct)
        {
            var dto = new SetActiveOrgDTO();

            try
            {
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                int affectedRows = await _unitOfWork.UserOrgs.SetActiveOrg(userId, orgId, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Active org set";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Failed to set active org.";
                    return dto;
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                //TODO log error
                dto.Error = "Error on setting active org";
                return dto;
            }

            return dto;
        }

        public async Task<GetActiveOrgDTO> GetActiveOrg(string userId, CancellationToken ct)
        {
            var dto = new GetActiveOrgDTO();

            try
            {
                dto.Org = await _unitOfWork.UserOrgs.GetActiveOrg(userId, ct);
                if (dto.Org == null)
                {
                    dto.Error = "Active org not found";
                }
            }
            catch
            {
                // TODO log error
                dto.Error = "Error getting active org";
            }

            return dto;
        }
    }
}
