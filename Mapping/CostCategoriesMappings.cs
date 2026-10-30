using VontaGenta.Entities;
using VontaGenta.Models;

namespace VontaGenta.Mapping
{
    public static class CostCategoriesMappings
    {
        public static CostCategoryDetailsViewModel ToDetailsViewModel(this Category source)
        {
            var dest = new CostCategoryDetailsViewModel();
            source.MapTo(dest);
            return dest;
        }

        public static void MapTo(this Category source, CostCategoryDetailsViewModel dest)
        {
            dest.ID = source.ID;
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.UserName = source.User?.Name;
        }

        public static void MapTo(this Category source, CostCategoryFormViewModel dest)
        {
            dest.Name = source.Name;
            dest.Description = source.Description;
        }

        public static void MapTo(this CostCategoryFormViewModel source, Category dest)
        {
            dest.Name = source.Name;
            dest.Description = source.Description;
        }

        // When editing a category we want to keep the existing user and org. These should not be overwritten.
        public static void MapTo(this Category source, Category dest)
        {
            dest.ID = source.ID;
            dest.Name = source.Name;
            dest.Description = source.Description;
        }
    }
}
