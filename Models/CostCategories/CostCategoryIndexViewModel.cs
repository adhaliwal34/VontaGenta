#nullable disable

namespace VontaGenta.Models
{
    public class CostCategoryIndexViewModel
    {
        // For the field labels
        public CostCategoryDetailsViewModel Category { get; set; }
        public List<CostCategoryDetailsViewModel> List { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEditOrDelete { get; set; }

        public CostCategoryIndexViewModel()
        {
            List = new List<CostCategoryDetailsViewModel>();
        }
    }
}
