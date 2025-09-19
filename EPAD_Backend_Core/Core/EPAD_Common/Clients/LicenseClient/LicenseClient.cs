using EPAD_Common.Extensions;
using EPAD_Common.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Common.Clients
{
    public class LicenseClient
    {
        public HttpClient Client { get; }
        private readonly IConfiguration Configuration;
        public LicenseClient(HttpClient client, IConfiguration configuration)
        {
            this.Configuration = configuration;
            string baseAddress = Configuration.GetValue<string>("LicenseServer");
            client.BaseAddress = new Uri(baseAddress);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-License");
            Client = client;
        }

        public async Task<AppLicenseResponse> GetAppLicenseInfoAsync(string pLicenseKey, string pComputerIdentity)
        {
            try
            {
                
                var response = await Client.GetAsync($"/api/LC_AppLicense/GetAppLicenseInfo?LicenseKey={pLicenseKey}&ComputerIdentity={pComputerIdentity}");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsAsync<AppLicenseInfo>();
                return new AppLicenseResponse()
                {
                    LicenseInfo = result,
                    Error = ""
                };
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().StartsWith("no connection") || ex.Message.Contains("A socket operation") || ex.Message.StartsWith("A connection attempt failed") || ex.Message.StartsWith("No such host is known"))
                {
                    return new AppLicenseResponse()
                    {
                        LicenseInfo = null,
                        Error = "NoConnection"
                    };
                }
                else if (ex.Message.Contains("404"))
                {
                    return new AppLicenseResponse()
                    {
                        LicenseInfo = null,
                        Error = "AppLicenseNotExsit"
                    };
                }
                else if (ex.Message.Contains("400"))
                {
                    return new AppLicenseResponse()
                    {
                        LicenseInfo = null,
                        Error = "ServerComputerNotMatch"
                    };
                }
                else
                {
                    return new AppLicenseResponse()
                    {
                        LicenseInfo = null,
                        Error = ex.Message
                    };
                }
            }
        }
    }

    public class AppLicenseResponse
    {
        public AppLicenseInfo LicenseInfo { get; set; }
        public string Error { get; set; }
    }
}
