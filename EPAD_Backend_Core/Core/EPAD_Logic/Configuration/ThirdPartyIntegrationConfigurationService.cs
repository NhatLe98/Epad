using EPAD_Common.Types;
using System;

namespace EPAD_Logic.Configuration
{
    public class ThirdPartyIntegrationConfigurationService : IThirdPartyIntegrationConfigurationService
    {
        private readonly ThirdPartyIntegrationConfiguration _thirdPartyIntegrationConfiguration;

        public ThirdPartyIntegrationConfigurationService(ThirdPartyIntegrationConfiguration thirdPartyIntegrationConfiguration)
        {
            _thirdPartyIntegrationConfiguration = thirdPartyIntegrationConfiguration ?? throw new ArgumentNullException(nameof(thirdPartyIntegrationConfiguration));
        }

        public string GetConnectionString => _thirdPartyIntegrationConfiguration.ConnectionString;
        public string GetConnectionStringHRPRO7 => _thirdPartyIntegrationConfiguration.ConnectionStringHRPRO7;
        public string GetConnectionStringHIK => _thirdPartyIntegrationConfiguration.ConnectionStringHIK;
    }
}
