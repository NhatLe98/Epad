using EPAD_Common.HTTPClient;
using EPAD_Data.HTTPClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface.EZHR
{
    public interface IEzhrConfigurationService
    {
        string Host { get; }
        string ClientID { get; }
        string ClientSecret { get; }
    }
}
