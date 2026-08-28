namespace VontaGenta.Services
{
    public interface IOrgInvitesService
    {
        Task<AddOrgInvitesDTO> AddOrgInvites(int id, string userId, string emailsToInvite, CancellationToken ct);
        Task<GetOrgInvitesDTO> GetAllInvitesToUser(string userId, CancellationToken ct);
        Task<GetOrgInviteDTO> GetOrgInviteToUser(int id, string userId, CancellationToken ct);
        Task<AcceptOrgInviteDTO> AcceptOrgInvite(int orgInviteId, string userId, CancellationToken ct);
        Task<DeleteOrgInviteDTO> DeleteOrgInvite(int orgInviteId, string userId, CancellationToken ct);
    }
}