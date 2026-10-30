#nullable disable
using System.ComponentModel.DataAnnotations;
namespace VontaGenta.Models
{
    public class CostCategoryFormViewModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
