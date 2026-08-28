using VontaGenta.Data;

namespace VontaGenta.DependencyInjection
{
    public static class AppUsersServicesExtensions
    {
        public static IServiceCollection AddAppUsersServices(this IServiceCollection services)
        {
            services.AddTransient<IAppUserRepo, AppUserRepo>();
            return services;
        }
    }
}
