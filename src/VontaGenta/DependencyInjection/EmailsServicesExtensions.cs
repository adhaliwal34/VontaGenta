using VontaGenta.Services;

namespace VontaGenta.DependencyInjection
{
    public static class EmailsServicesExtensions
    {
        public static IServiceCollection AddEmailsServices(this IServiceCollection services)
        {
            services.AddTransient<IEmailService, EmailService>();
            return services;
        }
    }
}
