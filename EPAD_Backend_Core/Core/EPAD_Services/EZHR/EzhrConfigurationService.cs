using EPAD_Data.Models.EZHR;
using EPAD_Services.Interface.EZHR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.EZHR
{
    public class EzhrConfigurationService : IEzhrConfigurationService
    {
        private readonly EzHRConfiguration _ezHRConfiguration;
        public EzhrConfigurationService(EzHRConfiguration ezHRConfiguration)
        {
            _ezHRConfiguration = ezHRConfiguration ?? throw new ArgumentNullException(nameof(ezHRConfiguration));
        }

        public string Host => _ezHRConfiguration.Host;

        public string ClientID => _ezHRConfiguration.ClientID;

        public string ClientSecret => _ezHRConfiguration.ClientSecret;
    }
}
