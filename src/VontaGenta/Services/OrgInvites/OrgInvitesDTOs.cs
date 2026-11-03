#nullable disable
using VontaGenta.Entities;

namespace VontaGenta.Services
{

    public class AddOrgInvitesDTO : RequestStatus { }

    public class AcceptOrgInviteDTO : RequestStatus { }

    public class DeleteOrgInviteDTO : RequestStatus { }

    public class GetOrgInvitesDTO : RequestStatus
    {
        public List<OrgInvite> OrgInvites { get; set; }
    }

    public class GetOrgInviteDTO : RequestStatus
    {
        public OrgInvite OrgInvite { get; set; }
    }

}
