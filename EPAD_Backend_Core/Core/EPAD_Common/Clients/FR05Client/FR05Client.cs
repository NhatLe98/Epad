using EPAD_Common.Types;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Common.Clients.FR05Client
{
    public class FR05Client
    {
        public HttpClient Client { get; set; }
        private readonly IConfiguration Configuration;
        private AppConfiguration _AppConfig;

        protected readonly IMemoryCache _Cache;

        public FR05Client(HttpClient client, IConfiguration configuration, IMemoryCache pCache)
        {
            this.Configuration = configuration;
            _Cache = pCache;
        }
        
      

    }
}
