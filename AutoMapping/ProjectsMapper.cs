using AutoMapper;
using VontaGenta.Entities;
using VontaGenta.Models;

namespace VontaGenta.Mapping
{
    public class ProjectsMapper : Profile
    {
        public ProjectsMapper()
        {
            // When editing a project keep the existing user. This should not be overwritten.
            CreateMap<Project, Project>()
                .ForMember(p => p.AppUserID, opt => opt.Ignore())
                .ForMember(p => p.User, opt => opt.Ignore());

            CreateMap<ProjectFormViewModel, Project>();

            CreateMap<Project, ProjectFormViewModel>();

            CreateMap<Project, ProjectDetailsViewModel>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.Name))
                .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client.Name));
        }
    }
}
