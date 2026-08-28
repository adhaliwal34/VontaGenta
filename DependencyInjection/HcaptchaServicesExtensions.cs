using VontaGenta.Services;

namespace VontaGenta.DependencyInjection
{
    public static class HcaptchaServicesExtensions
    {
        public static IServiceCollection AddHcaptchaServices(this IServiceCollection services)
        {
            services.AddHttpClient<IHcaptchaService, HcaptchaService>();
            return services;
        }
    }
}
