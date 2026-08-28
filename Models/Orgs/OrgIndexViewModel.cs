#nullable disable
using VontaGenta.Entities;

namespace VontaGenta.Models
{
    public class OrgIndexViewModel
    {
        // For the field labels
        public OrgDetailsViewModel Org { get; set; }
        public List<OrgDetailsViewModel> OrgsOwned { get; set; }
        public List<OrgDetailsViewModel> OrgsJoined { get; set; }
        public List<OrgInvite> OrgInvites { get; set; }

        public OrgIndexViewModel()
        {
            OrgsOwned = new List<OrgDetailsViewModel>();
            OrgsJoined = new List<OrgDetailsViewModel>();
            OrgInvites = new List<OrgInvite>();
        }
    }
}
