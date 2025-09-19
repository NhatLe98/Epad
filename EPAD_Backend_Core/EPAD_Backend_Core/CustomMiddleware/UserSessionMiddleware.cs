using EPAD_Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.CustomMiddleware
{
    public class UserSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        public UserSessionMiddleware(RequestDelegate next, IMemoryCache pCache)
        {
            _next = next;
            _cache = pCache;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.User.Identity.IsAuthenticated == true)
            {
                UserInfo user = UserInfo.GetFromCache(_cache, httpContext.User.Identity.Name);
                if (user != null)
                {
                    httpContext.Session.SetInt32("PrivilegeIndex", user.PrivilegeIndex);
                    httpContext.Session.SetInt32("CompanyIndex", user.CompanyIndex);
                    httpContext.Session.SetString("UserName", user.UserName);
                }
                await _next(httpContext);
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
