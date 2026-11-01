using VontaGenta.Data;
using VontaGenta.Services;

namespace VontaGenta.DependencyInjection
{
    public static class RevenuesServicesExtensions
    {
        public static IServiceCollection AddRevenuesServices(this IServiceCollection services)
        {
            services.AddTransient<IRevenuesRepo, RevenuesRepo>();
            services.AddTransient<IRevenuesService, RevenuesService>();
            return services;
        }
    }
}
