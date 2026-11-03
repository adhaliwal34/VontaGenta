#nullable disable
using System.ComponentModel.DataAnnotations;
namespace VontaGenta.Models
{
    public class OrgEditViewModel : OrgFormViewModel
    {
        // Display only, shown in the edit page heading
        [Display(Name = "Creator")]
        public string UserName { get; set; }
    }
}
