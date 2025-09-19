using Microsoft.Extensions.DependencyInjection;

namespace EPAD_Common.Clients
{
    public static class ClientsDI
    {
        public static IServiceCollection AddClients(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddHttpClient<LicenseClient>();
            services.AddHttpClient<GCSClient>();
            services.AddHttpClient<EZHRClient>();
            services.AddHttpClient<ECMSClient>();
            services.AddHttpClient<ICMSClient>();
            services.AddHttpClient<RelayControllerClient>();

            services.AddSingleton<RabbitMQHelper>();
            return services;
        }
    }
}
