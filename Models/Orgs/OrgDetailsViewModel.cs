#nullable disable
using System.ComponentModel.DataAnnotations;
namespace VontaGenta.Models
{
    public class OrgDetailsViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Display(Name = "Creator")]
        public string UserName { get; set; }
        public bool IsActive { get; set; }
        public List<string> OrgMembers { get; set; }
        public bool ShowEditLink { get; set; }

        public OrgDetailsViewModel()
        { 
            OrgMembers = [];
        }
    }
}
