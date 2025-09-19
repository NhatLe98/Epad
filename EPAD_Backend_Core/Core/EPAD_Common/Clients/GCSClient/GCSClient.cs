using EPAD_Common.Types;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EPAD_Common.Clients
{
    public class GCSClient
    {
        public HttpClient Client { get; }

        private readonly IConfiguration Configuration;
        public GCSClient(HttpClient client, IConfiguration configuration)
        {
            this.Configuration = configuration;
            var appConfig = Configuration.GetSection("GCS").Get<AppConfiguration>();
            client.BaseAddress = new Uri(appConfig.Api);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            Client = client;
        }
    }
}
