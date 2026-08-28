#nullable disable
using System.ComponentModel.DataAnnotations;
namespace VontaGenta.Models
{
    public class OrgAddInviteViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        [Display(Name = "Emails to invite")]
        public string EmailsToInvite { get; set; }
    }
}
