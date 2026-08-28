#nullable disable
using System.ComponentModel.DataAnnotations;

namespace VontaGenta.Models
{
    public class CostCategoryEditViewModel
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
