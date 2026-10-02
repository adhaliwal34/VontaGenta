#nullable disable
using VontaGenta.Entities;

namespace VontaGenta.Services
{

    public class GetUserOrgsDTO : RequestStatus
    {
        public List<UserOrg> List { get; set; }

        public GetUserOrgsDTO()
        {
            List = new List<UserOrg>();
        }
    }

    public class GetUserOrgDTO : RequestStatus
    {
        public Org Org { get; set; }
    }

    public class SetActiveOrgDTO : RequestStatus { }

    public class GetActiveOrgDTO : RequestStatus
    {
        public Org Org { get; set; }
    }

    public class SaveUserOrgsDTO : RequestStatus { }

    public class UpdateUserOrgsDTO : RequestStatus { }

    public class DeleteUserOrgsDTO : RequestStatus { }

    public class LeaveOrgDTO : RequestStatus { }

    public class IsOrgMemberDTO : RequestStatus
    {
        public bool IsOrgMember { get; set; }
    }

}
