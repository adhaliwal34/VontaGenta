using VontaGenta.Data;
using VontaGenta.Services;

namespace VontaGenta.DependencyInjection
{
    public static class TimesheetsServicesExtensions
    {
        public static IServiceCollection AddTimesheetsServices(this IServiceCollection services)
        {
            services.AddTransient<ITimesheetsRepo, TimesheetsRepo>();
            services.AddTransient<ITimesheetsService, TimesheetsService>();
            return services;
        }
    }
}
