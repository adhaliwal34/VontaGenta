using AutoMapper;
using VontaGenta.Entities;
using VontaGenta.Models;
using VontaGenta.Services;

namespace VontaGenta.Mapping
{
    public class OrgsMapper : Profile
    {
        public OrgsMapper()
        {
            CreateMap<OrgDetailsViewModel, Org>();

            CreateMap<Org, OrgDetailsViewModel>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.Name));

            CreateMap<OrgEditViewModel, Org>();

            CreateMap<Org, OrgEditViewModel>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.Name));

            CreateMap<Org, OrgDTO>();

            CreateMap<OrgDTO, OrgDetailsViewModel>();

            CreateMap<Org, Org>();

            CreateMap<Org, OrgAddInviteViewModel>();

            CreateMap<OrgInvite, OrgInviteDeleteConfirmationViewModel>()
                .ForMember(d => d.FromUserName, opt => opt.MapFrom(s => s.Org.User.Name))
                .ForMember(d => d.OrgName, opt => opt.MapFrom(s => s.Org.Name))
                .ForMember(d => d.OrgDescription, opt => opt.MapFrom(s => s.Org.Description));
        }
    }
}
