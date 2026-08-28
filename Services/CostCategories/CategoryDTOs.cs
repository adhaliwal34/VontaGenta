#nullable disable
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public class GetCategoriesDTO : RequestStatus
    {
        public List<Category> List { get; set; }

        public GetCategoriesDTO()
        {
            List = new List<Category>();
        }
    }

    public class GetCategoryDTO : RequestStatus
    {
        public Category Category { get; set; }
    }

    public class SaveCategoryDTO : RequestStatus { }

    public class UpdateCategoryDTO : RequestStatus { }

    public class DeleteCategoryDTO : RequestStatus { }

}
