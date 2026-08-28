#nullable disable
using System.ComponentModel.DataAnnotations;
namespace VontaGenta.Models
{
    public class CostCategoryCreateViewModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
