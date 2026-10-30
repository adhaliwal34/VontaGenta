using AutoMapper;
using VontaGenta.Entities;
using VontaGenta.Models;

namespace VontaGenta.Mapping
{
    public class ClientsMapper : Profile
    {
        public ClientsMapper()
        {
            // When editing a client keep the existing user and org. These should not be overwritten.
            CreateMap<Client, Client>()
                .ForMember(p => p.AppUserID, opt => opt.Ignore())
                .ForMember(p => p.User, opt => opt.Ignore())
                .ForMember(d => d.OrgID, opt => opt.Ignore())
                .ForMember(d => d.Org, opt => opt.Ignore());

            CreateMap<ClientFormViewModel, Client>();

            CreateMap<Client, ClientFormViewModel>();

            CreateMap<Client, ClientDetailsViewModel>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.Name));
        }
    }
}
