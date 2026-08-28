using System.Data;
using VontaGenta.Data;
using Microsoft.AspNetCore.Identity;
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public class OrgInvitesService : IOrgInvitesService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public OrgInvitesService(UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<AddOrgInvitesDTO> AddOrgInvites(int id, string userId, string emailsToInvite, CancellationToken ct)
        {
            var dto = new AddOrgInvitesDTO();

            var org = await _unitOfWork.Orgs.GetOrg(id, userId, ct);

            if (org == null)
            {
                dto.Error = "Cannot find org";
                return dto;
            }

            if (string.IsNullOrEmpty(emailsToInvite))
            {
                dto.Error = "Must enter an email to invite or a comma separated list of emails to invite.";
                return dto;
            }

            if (emailsToInvite.Length > 2500)
            {
                dto.Error = "The maximum length of the email list is 2500 characters.";
                return dto;
            }

            var emailList = emailsToInvite.Split(',').Select(i => _userManager.NormalizeEmail(i.Trim())).Distinct();

            var orgInvites = new List<OrgInvite>();

            // Get org invites.
            var existingInvites = await _unitOfWork.OrgInvites.GetOrgInvites(id, userId, ct);

            if (existingInvites == null)
            {
                dto.Error = "Error loading existing invites. Aborted.";
                return dto;
            }

            // Get org members.
            var orgMemberIds = (await _unitOfWork.UserOrgs.GetUsersInOrg(id, ct))?.Select(u => u.AppUserID).ToList();

            foreach (var email in emailList)
            {
                var user = await _userManager.FindByEmailAsync(email);

                // Only create an invite if the person is not already a member and an invite doesn't already exist.
                if (user != null && orgMemberIds != null && !orgMemberIds.Contains(user.Id) && !existingInvites.Any(i => i.ToUserID == user.Id))
                {
                    orgInvites.Add(new OrgInvite { OrgID = id, ToUserID = user.Id });
                }
            }

            if (orgInvites.Any())
            {
                try
                {
                    await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                    var affectedRows = await _unitOfWork.OrgInvites.SaveOrgInvites(orgInvites, ct);

                    if (affectedRows > 0)
                    {
                        await _unitOfWork.CommitAsync(ct);
                        dto.Status = "Invites sent if the email(s) found on the site.";
                    }
                    else
                    {
                        await _unitOfWork.RollbackAsync(ct);
                        dto.Error = "Sending invites failed";
                        return dto;
                    }
                }
                catch(Exception)
                {
                    await _unitOfWork.RollbackAsync(ct);
                    //TODO log error
                    dto.Error = "Error sending invites.";
                    return dto;
                }
            }

            return dto;
        }

        public async Task<GetOrgInvitesDTO> GetAllInvitesToUser(string userId, CancellationToken ct)
        {
            var dto = new GetOrgInvitesDTO();

            try
            {
                dto.OrgInvites = await _unitOfWork.OrgInvites.GetAllInvitesToUser(userId, ct);
            }
            catch
            {
                // TODO Log exception
                dto.Error = "Error getting invites to orgs.";
            }

            return dto;
        }

        public async Task<GetOrgInviteDTO> GetOrgInviteToUser(int id, string userId, CancellationToken ct)
        {
            var dto = new GetOrgInviteDTO();

            try
            {
                dto.OrgInvite = await _unitOfWork.OrgInvites.GetOrgInvite(id, userId, ct);

                if (dto.OrgInvite == null)
                {
                    dto.Error = "Invite not found";
                }
            }
            catch
            {
                // TODO Log exception
                dto.Error = "Error getting invites to orgs.";
            }

            return dto;
        }

        public async Task<AcceptOrgInviteDTO> AcceptOrgInvite(int orgInviteId, string userId, CancellationToken ct)
        {
            var dto = new AcceptOrgInviteDTO();

            try
            {
                var orgInvite = await _unitOfWork.OrgInvites.GetOrgInvite(orgInviteId, userId, ct);

                if (orgInvite == null)
                {
                    dto.Error = "Error accepting invite to org.";
                    return dto;
                }

                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                var affectedRows = await _unitOfWork.OrgInvites.AcceptOrgInvite(orgInvite, userId, ct);
                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Accepted invite to org.";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Error accepting invite to org.";
                    return dto;
                }
            }
            catch(Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                //TODO log error
                dto.Error = "Error accepting invite to org.";
                return dto;
            }

            return dto;
        }

        public async Task<DeleteOrgInviteDTO> DeleteOrgInvite(int orgInviteId, string userId, CancellationToken ct)
        {
            var dto = new DeleteOrgInviteDTO();

            try
            {
                var orgInvite = await _unitOfWork.OrgInvites.GetOrgInvite(orgInviteId, userId, ct);

                if (orgInvite == null)
                {
                    dto.Error = "Error ignoring invite to org.";
                    return dto;
                }

                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                var affectedRows = await _unitOfWork.OrgInvites.DeleteOrgInvite(orgInvite, userId, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Invite ignored. The inviter was not informed.";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Error ignoring invite to org.";
                    return dto;
                }
            }
            catch(Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                //TODO log error
                dto.Error = "Error ignoring invite to org.";
                return dto;
            }

            return dto;
        }
    }
}
