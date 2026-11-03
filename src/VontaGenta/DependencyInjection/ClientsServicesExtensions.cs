using VontaGenta.Data;
using VontaGenta.Services;

namespace VontaGenta.DependencyInjection
{
    public static class ClientsServicesExtensions
    {
        public static IServiceCollection AddClientsServices(this IServiceCollection services)
        {
            services.AddTransient<IClientsRepo, ClientsRepo>();
            services.AddTransient<IClientsService, ClientsService>();
            return services;
        }
    }
}
