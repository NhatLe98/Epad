using EPAD_Common.Types;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace EPAD_Common.Clients
{
    public class RelayControllerClient
    {
        public HttpClient Client { get; }

        private readonly IConfiguration Configuration;
        public RelayControllerClient(HttpClient client, IConfiguration configuration)
        {
            this.Configuration = configuration;
            var appConfig = Configuration.GetSection("RelayController").Get<AppConfiguration>();
            client.BaseAddress = new Uri(appConfig.Api);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            Client = client;
        }
    }
}
