using EPAD_Common.Types;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace EPAD_Common.Clients
{
    public class ECMSClient
    {
        public HttpClient Client { get; }

        private readonly IConfiguration Configuration;
        public ECMSClient(HttpClient client, IConfiguration configuration)
        {
            this.Configuration = configuration;
            var appConfig = Configuration.GetSection("ECMS").Get<AppConfiguration>();
            client.BaseAddress = new Uri(appConfig.Api);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            Client = client;
        }
    }
}
