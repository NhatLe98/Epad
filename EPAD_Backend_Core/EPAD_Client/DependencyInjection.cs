using System;
using EPAD_Client.EZHR;
using EPAD_Data.Models.EZHR;
using EPAD_Services.Interface.EZHR;
using Microsoft.Extensions.DependencyInjection;

namespace EPAD_Client
{
    public static class DependencyInjection
    {
        public static void RegisterClientServices(this IServiceCollection services, EzHRConfiguration ezHRConfig)
        {
            services.AddTransient<IEzhrConfigurationService, EzhrConfigurationService>(init => new EzhrConfigurationService(ezHRConfig));
            services.AddHttpClient<IEzhrApiClient, EzhrApiClient>(config =>
            {
                config.BaseAddress = new Uri(ezHRConfig.Host);
                config.DefaultRequestHeaders.Add("Accept", "application/json");
            });
        }
    }
}
