using VontaGenta.Data;
using VontaGenta.Services;

namespace VontaGenta.DependencyInjection
{
    public static class CostEntriesServicesExtensions
    {
        public static IServiceCollection AddCostEntriesServices(this IServiceCollection services)
        {
            services.AddTransient<ICostEntriesRepo, CostEntriesRepo>();
            services.AddTransient<ICostEntriesService, CostEntriesService>();
            return services;
        }
    }
}
