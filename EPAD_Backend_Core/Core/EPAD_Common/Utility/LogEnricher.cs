using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using System;

namespace EPAD_Common.Utility
{
    public class LogEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;

        public LogEnricher(IServiceProvider provider)
        {
            _httpContextAccessor = provider.GetService<IHttpContextAccessor>();
            _cache = provider.GetService<IMemoryCache>();
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var req = _httpContextAccessor.HttpContext?.Request;
            if (req != null)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Request", req.Path));

                var userIdentity = _httpContextAccessor.HttpContext.User.Identity.Name;
                //var user = UserInfo.GetFromCache(_cache, userIdentity);
                //if(user != null)
                //{
                //    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserName", user.UserName));
                //    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CompanyIndex", user.CompanyIndex));
                //}
            }
        }
    }
}
