using AutoMapper;
using VontaGenta.Entities;
using VontaGenta.Models;

namespace VontaGenta.Mapping
{
    public class RevenuesMapper : Profile
    {
        public RevenuesMapper()
        {
            CreateMap<Revenue, RevenueDetailsViewModel>()
                .ForMember(d => d.ProjectName, opt => opt.MapFrom(s => s.Project.Name))
                .ForMember(d => d.CurrencySymbol, opt => opt.MapFrom(s => s.Currency.Symbol));

            CreateMap<Revenue, RevenueCreateViewModel>();
            CreateMap<RevenueCreateViewModel, Revenue>();
        }
    }
}
