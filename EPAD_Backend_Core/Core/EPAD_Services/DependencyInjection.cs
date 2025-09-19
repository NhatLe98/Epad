using EPAD_Common.Types;
using EPAD_Data.Models.EZHR;
using EPAD_Logic.Configuration;
using EPAD_Services.EZHR;
using EPAD_Services.Interface.EZHR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EPAD_Services
{
    public static class DependencyInjection
    {
        public static void RegisterServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var thirdPartyIntegrationConfig = configuration.GetSection(ThirdPartyIntegrationConfiguration.ThirdPartyIntegration).Get<ThirdPartyIntegrationConfiguration>();

            services.AddTransient<IThirdPartyIntegrationConfigurationService, ThirdPartyIntegrationConfigurationService>
                (_ => new ThirdPartyIntegrationConfigurationService(thirdPartyIntegrationConfig));
        }

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
