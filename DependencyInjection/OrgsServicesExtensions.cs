using VontaGenta.Data;
using VontaGenta.Services;

namespace VontaGenta.DependencyInjection
{
    public static class OrgsServicesExtensions
    {
        public static IServiceCollection AddOrgsServices(this IServiceCollection services)
        {
            services.AddTransient<IOrgsRepo, OrgsRepo>();
            services.AddTransient<IOrgsService, OrgsService>();
            services.AddTransient<IUserOrgsRepo, UserOrgsRepo>();
            services.AddTransient<IUserOrgsService, UserOrgsService>();
            services.AddTransient<IOrgInvitesRepo, OrgInvitesRepo>();
            services.AddTransient<IOrgInvitesService, OrgInvitesService>();
            return services;
        }
    }
}
