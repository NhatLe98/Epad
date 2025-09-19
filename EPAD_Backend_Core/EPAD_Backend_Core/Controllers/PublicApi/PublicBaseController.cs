using EPAD_Data;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EPAD_Backend_Core.Controllers.PublicApi
{
    public class PublicBaseController : ControllerBase
    {
        protected readonly EPAD_Context _epadContext;
        protected readonly ezHR_Context _otherContext;
        protected readonly IMemoryCache _cache;
        protected readonly IConfiguration _config;
        protected readonly IServiceProvider _serviceProvider;
        public PublicBaseController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _epadContext = TryResolve<EPAD_Context>();
            _otherContext = TryResolve<ezHR_Context>();
            _cache = TryResolve<IMemoryCache>();
        }

        protected T TryResolve<T>()
        {
            return _serviceProvider.GetService<T>();
        }

        protected UserInfo GetCurrentUser()
        {
            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            return user;
        }

        protected UnauthorizedObjectResult ApiUnauthorize()
        {
            return Unauthorized(new { Status = "false", MessageCode = "Unauthorize", MessageDetail = "Unauthorize", Data = new { } });
        }



    }
}
