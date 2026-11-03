using VontaGenta.Data;
using VontaGenta.Services;

namespace VontaGenta.DependencyInjection
{
    public static class CurrenciesServicesExtensions
    {
        public static IServiceCollection AddCurrenciesServices(this IServiceCollection services)
        {
            services.AddTransient<ICurrenciesRepo, CurrenciesRepo>();
            services.AddTransient<ICurrenciesService, CurrenciesService>();
            return services;
        }
    }
}
