using VontaGenta.Data;

namespace VontaGenta.DependencyInjection
{
    public static class UnitOfWorkServicesExtensions
    {
        public static IServiceCollection AddUnitOfWorkServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
