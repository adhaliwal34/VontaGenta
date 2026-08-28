namespace VontaGenta.Services
{
    public interface IUserOrgsService
    {
        Task<GetUserOrgsDTO> GetUsersInOrg(int orgId, CancellationToken ct);
        Task<IsOrgMemberDTO> IsOrgMember(string userId, int orgId, CancellationToken ct);
        Task<LeaveOrgDTO> LeaveOrg(string userId, int orgId, CancellationToken ct);
        Task<SetActiveOrgDTO> SetActiveOrg(string userId, int orgId, CancellationToken ct);
        Task<GetActiveOrgDTO> GetActiveOrg(string userId, CancellationToken ct);
    }
}