using VontaGenta.Data;
using VontaGenta.Services;

namespace VontaGenta.DependencyInjection
{
    public static class ProjectsServicesExtensions
    {
        public static IServiceCollection AddProjectsServices(this IServiceCollection services)
        {
            services.AddTransient<IProjectsRepo, ProjectsRepo>();
            services.AddTransient<IProjectsService, ProjectsService>();
            return services;
        }
    }
}
