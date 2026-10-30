using AutoMapper;
using VontaGenta.Entities;
using VontaGenta.Models;

namespace VontaGenta.Mapping
{
    public class CostEntriesMapper : Profile
    {
        public CostEntriesMapper()
        {
            // When editing a cost entry we want to keep the existing user. It should not be overwritten.
            // Since ProjectID might change don't copy project reference.
            CreateMap<CostEntry, CostEntry>()
                .ForMember(c => c.AppUserID, opt => opt.Ignore())
                .ForMember(c => c.User, opt => opt.Ignore())
                .ForMember(c => c.Project, opt => opt.Ignore());

            CreateMap<CostEntryFormViewModel, CostEntry>();
            CreateMap<CostEntry, CostEntryFormViewModel>();

            CreateMap<CostEntry, CostEntryDetailsViewModel>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.Name))
                .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
                .ForMember(d => d.ProjectName, opt => opt.MapFrom(s => s.Project.Name))
                .ForMember(d => d.CurrencySymbol, opt => opt.MapFrom(s => s.Currency.Symbol));
        }
    }
}
