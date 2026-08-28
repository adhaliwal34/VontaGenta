#nullable disable
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public class OrgDTO : Org
    {
        public bool IsActive { get; set; }
    }

    public class GetOrgsDTO : RequestStatus
    {
        public List<OrgDTO> List { get; set; }
        public bool ListHasActiveOrg { get; set; }

        public GetOrgsDTO()
        {
            List = new List<OrgDTO>();
        }
    }

    public class GetOrgDTO : RequestStatus
    {
        public Org Org { get; set; }
    }

    public class SaveOrgDTO : RequestStatus { }

    public class UpdateOrgDTO : RequestStatus { }

    public class DeleteOrgDTO : RequestStatus { }

}
