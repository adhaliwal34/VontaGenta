using VontaGenta.Data;
using VontaGenta.Services;

namespace VontaGenta.DependencyInjection
{
    public static class CostCategoriesServicesExtensions
    {
        public static IServiceCollection AddCostCategoriesServices(this IServiceCollection services)
        {
            services.AddTransient<ICostCategoriesRepo, CostCategoriesRepo>();
            services.AddTransient<ICostCategoriesService, CostCategoriesService>();
            return services;
        }
    }
}
