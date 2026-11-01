using VontaGenta.Data;
using VontaGenta.Services;

namespace VontaGenta.DependencyInjection
{
    public static class AppTimeZonesServicesExtensions
    {
        public static IServiceCollection AddAppTimeZonesServices(this IServiceCollection services)
        {
            services.AddTransient<IAppTimeZonesRepo, AppTimeZonesRepo>();
            services.AddSingleton<IAppTimeZonesHelper, AppTimeZonesHelper>();
            services.AddSingleton<IAppTimeZonesService, AppTimeZonesService>();
            return services;
        }
    }
}
