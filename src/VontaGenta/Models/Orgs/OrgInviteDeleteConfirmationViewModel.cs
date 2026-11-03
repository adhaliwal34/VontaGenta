#nullable disable
using System.ComponentModel.DataAnnotations;
namespace VontaGenta.Models
{
    public class OrgInviteDeleteConfirmationViewModel
    {
        public int ID { get; set; }
        [Display(Name = "Org name")]
        public string OrgName { get; set; }
        [Display(Name = "Org description")]
        public string OrgDescription { get; set; }
        [Display(Name = "From")]
        public string FromUserName { get; set; }
    }
}
