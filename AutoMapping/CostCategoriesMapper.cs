using AutoMapper;
using VontaGenta.Entities;
using VontaGenta.Models;

namespace VontaGenta.Mapping
{
    public class CostCategoriesMapper : Profile
    {
        public CostCategoriesMapper()
        {
            // When editing a category we want to keep the existing user and org. These should not be overwritten.
            CreateMap<Category, Category>()
                .ForMember(d => d.AppUserID, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore())
                .ForMember(d => d.OrgID, opt => opt.Ignore())
                .ForMember(d => d.Org, opt => opt.Ignore());

            CreateMap<CostCategoryDetailsViewModel, Category>();

            CreateMap<Category, CostCategoryDetailsViewModel>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.Name));

            CreateMap<Category, CostCategoryEditViewModel>();
            CreateMap<CostCategoryEditViewModel, Category>();

            CreateMap<CostCategoryCreateViewModel, Category>();
        }
    }
}
