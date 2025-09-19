using System;
using EPAD_Backend_Core.Provider;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using EPAD_Common.Utility;
using EPAD_Services.Interface;
using EPAD_Backend_Core.Base;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class IC_LoginController : ApiControllerBase
    {
        private readonly IConfiguration _config;
        private readonly EPAD_Context context;
        private readonly ezHR_Context otherContext;
        private readonly IMemoryCache cache;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        private readonly IIC_ConfigService _ser;
        private readonly IIC_CachingLogic _iC_CachingLogic;
        public IC_LoginController(IServiceProvider provider) : base(provider)
        {
            _config = TryResolve<IConfiguration>();
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            otherContext = TryResolve<ezHR_Context>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _ser = TryResolve<IIC_ConfigService>();
            _iC_CachingLogic = TryResolve<IIC_CachingLogic>();
            _Logger = _LoggerFactory.CreateLogger<IC_LoginController>();
        }

        [Route("api/Login")]
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] LoginInfo login)
        {
            IActionResult response = Unauthorized();

            var t = _ser.FirstOrDefault();

            UserInfo user = null;
            CompanyInfo company = null;
            bool isUser = StringHelper.IsValidEmail(login.UserName);
            bool result = false;
            string error = "";
            try
            {
                // user login
                if (isUser)
                {
                    result = LoginProcess.Login(login.UserName, login.Password, ref error, ref user, ref company, context, otherContext, cache);
                    if (result == true)
                    {
                        IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                        audit.UserName = user.UserName;
                        audit.CompanyIndex = company.CompanyIndex;
                        audit.State = AuditType.Login;
                        audit.DateTime = DateTime.Now;
                        _iIC_AuditLogic.Create(audit);
                    }
                }
                else // service login
                {

                    result = LoginProcess.ServiceLogin(login.UserName, login.Password, ref error, ref user, ref company, login.ServiceId, context, cache);
                    if (result)
                    {
                        _iC_CachingLogic.SyncSystemCommandCacheAndDatabase();
                        _iC_CachingLogic.ResetCommandCacheForService();
                    }
                }

                if (result == true)
                {
                    string guid = "";
                    var tokenString = Provider.TokenProvider.CreateJsonWebToken(cache, user, ref guid);

                    if (company.ListCacheUserInfo != null)
                    {
                        // remove old userinfo
                        var userKey = company.ListCacheUserInfo.Find(e => e.UserName == user.UserName);
                        if (userKey != null)
                        {
                            UserInfo.RemoveFromCache(cache, userKey.GuidID);
                            company.ListCacheUserInfo.Remove(userKey);
                            company.ListCacheUserInfo.Add(new UserKey { UserName = user.UserName, GuidID = guid });
                        }
                        else
                        {
                            company.ListCacheUserInfo.Add(new UserKey { UserName = user.UserName, GuidID = guid });
                        }
                    }

                    response = Ok(new { access_token = tokenString, token_type = "bearer", expires_in = TimeSpan.FromHours(24).TotalSeconds, message = error });
                }
                else
                {
                    response = Unauthorized(new { status = "false", message = error });
                }
                cache.GetAllHWLicense();
            }
            catch (Exception ex)
            {
                if (!isUser)
                {
                    _iC_CachingLogic.SyncSystemCommandCacheAndDatabase();
                    _iC_CachingLogic.ResetCommandCacheForService();

                }
                return BadRequest(ex);
            }
            return response;
        }

        [Route("api/Login/CheckLogin")]
        [AllowAnonymous]
        [HttpGet]
        public IActionResult CheckLogin(string userName)
        {
            IActionResult response = Unauthorized();
            UserInfo user = null;
            CompanyInfo company = null;
            bool isUser = StringHelper.IsValidEmail(userName);
            bool result = false;
            string error = "";

            var api = GetAccessorApi();
            // user login
            if (isUser)
            {
                result = LoginProcess.LoginSSO(userName, ref error, ref user, ref company, context, otherContext, cache);
                if (result == true)
                {
                    IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                    audit.UserName = user.UserName;
                    audit.CompanyIndex = company.CompanyIndex;
                    audit.State = AuditType.Login;
                    audit.DateTime = DateTime.Now;
                    _iIC_AuditLogic.Create(audit);
                }
            }

            if (result == true)
            {
                string guid = "";
                var tokenString = Provider.TokenProvider.CreateJsonWebToken(cache, user, ref guid);
                cache.Set("token", tokenString);
                cache.Set("accessorApi", api);

                if (company.ListCacheUserInfo != null)
                {
                    // remove old userinfo
                    var userKey = company.ListCacheUserInfo.Find(e => e.UserName == user.UserName);
                    if (userKey != null)
                    {
                        UserInfo.RemoveFromCache(cache, userKey.GuidID);
                        company.ListCacheUserInfo.Remove(userKey);
                        company.ListCacheUserInfo.Add(new UserKey { UserName = user.UserName, GuidID = guid });
                    }
                    else
                    {
                        company.ListCacheUserInfo.Add(new UserKey { UserName = user.UserName, GuidID = guid });
                    }
                }

                response = Ok(new { access_token = tokenString, token_type = "bearer", expires_in = TimeSpan.FromHours(24).TotalSeconds, message = error });
            }
            else
            {
                response = Unauthorized(new { status = "false", message = error });
            }
            cache.GetAllHWLicense();

            return response;
            //var user = GetCurrentUser();
            // return Ok(response);
        }

        public class LoginInfo
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public string ServiceId { get; set; }

            // public string userName { get; set; }
        }
        public class LoginResult
        {
            public string token { get; set; }
        }
    }
}