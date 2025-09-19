using EPAD_Common.Types;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace EPAD_Common.Clients
{
    public class ICMSClient
    {
        public HttpClient Client { get; }

        private readonly IConfiguration Configuration;
        public ICMSClient(HttpClient client, IConfiguration configuration)
        {
            this.Configuration = configuration;
            var appConfig = Configuration.GetSection("ICMS").Get<AppConfiguration>();
            client.BaseAddress = new Uri(appConfig.Api);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            Client = client;
        }
    }
}
