using AutoMapper;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Extensions;
using EPAD_Common.Repository;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Data.Models.Other;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Base
{
    public class ApiControllerBase : ControllerBase
    {
        protected EPAD_Context _DbContext;
        protected IMemoryCache _Cache;
        protected readonly IHttpClientFactory _ClientFactory;
        protected readonly ILoggerFactory _LoggerFactory;
        protected readonly IServiceProvider _ServiceProvider;
        protected readonly IConfiguration _Configuration;
        protected readonly IntegrateConfiguration _IntegrateConfiguration;
        protected ConfigObject _Config;
        protected ILogger _Logger;
        protected IMapper _Mapper;
        protected IBaseRepository<EPAD_Context> _RepoBase;
        protected IHttpContextAccessor _httpContextAccessor;


        public ApiControllerBase(IServiceProvider pProvider)
        {
            _ServiceProvider = pProvider;
            _Configuration = TryResolve<IConfiguration>();
            _Cache = TryResolve<IMemoryCache>();
            _LoggerFactory = TryResolve<ILoggerFactory>();
            _DbContext = TryResolve<EPAD_Context>();
            _RepoBase = TryResolve<IBaseRepository<EPAD_Context>>();
            _Mapper = TryResolve<IMapper>();
            _ClientFactory = TryResolve<IHttpClientFactory>();
            _Config = ConfigObject.GetConfig(_Cache);
            _IntegrateConfiguration = CreateIntegrateConfiguration();
            _httpContextAccessor = TryResolve<IHttpContextAccessor>();
        }

        private IntegrateConfiguration CreateIntegrateConfiguration()
        {
            var rs = new IntegrateConfiguration();
            rs.GCS = _Configuration.GetSection("GCS").Get<AppConfiguration>();
            rs.EZHR = _Configuration.GetSection("EZHR").Get<AppConfiguration>();
            rs.ECMS = _Configuration.GetSection("ECMS").Get<AppConfiguration>();
            rs.ICMS = _Configuration.GetSection("ICMS").Get<AppConfiguration>();
            rs.RelayController = _Configuration.GetSection("RelayController").Get<AppConfiguration>();
            return rs;
        }

        protected T TryResolve<T>()
        {
            return _ServiceProvider.GetService<T>();
        }

        protected void AutoResolve(IServiceProvider provider)
        {
            var allFieldInfo = this.GetType().GetAllFields().Where(x => x.FieldType.FullName.StartsWith("EPAD_"));
            foreach (var field in allFieldInfo)
            {
                var obj = provider.GetService(field.FieldType);
                field.SetValue(this, obj);
            }
        }

        protected string GetAccessorApi()
        {
            var ApiLink = _httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host.Value;
            return ApiLink;
        }

        protected UserInfo GetUserInfo()
        {
            var user = UserInfo.GetFromCache(_Cache, User.Identity.Name);
            if (user != null) return user;
            string mCommunicateToken = _Configuration.GetValue<string>("CommunicateToken");
            string token = "";
            int companyIndex = 2;
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if(HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "CompanyIndex")
                {
                    int.TryParse(HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString(), out companyIndex);
                }

                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return null;
            }

            return new UserInfo("IntegrateSys") { CompanyIndex = companyIndex };
        }

        protected UnauthorizedObjectResult ApiUnauthorized(string pMessageCode = "TokenExpired")
        {
            return Unauthorized(pMessageCode);
        }

        protected OkResult ApiOk()
        {
            return Ok();
        }

        protected OkObjectResult ApiOk<T>(T data)
        {
            return Ok(data);
        }

        protected OkObjectResult ApiOkPublic<T>(T data)
        {
            return Ok(new { Status = "success", MessageCode = "ok", MessageDetail = "", Data = data });
        }

        protected BadRequestObjectResult ApiErrorPublic<T>(string pMessageCode, string pMessageDetail, T data)
        {
            return BadRequest(new { Status = "fail", MessageCode = pMessageCode, MessageDetail = pMessageDetail, Data = data });
        }

        protected BadRequestObjectResult ApiError(string pMessageCode)
        {
            return BadRequest(pMessageCode);
        }

        protected ConflictObjectResult ApiConflict(string pMessageCode)
        {
            return Conflict(pMessageCode);
        }

        protected int SaveChange(bool _ = false)
        {
            var iRe = _RepoBase.SaveChange();
            return iRe;
        }

        protected async Task<int> SaveChangeAsync(bool ensureAutoHistory = false)
        {
            var iRe = await _RepoBase.SaveChangeAsync(ensureAutoHistory);
            return iRe;
        }

        protected void BeginTransaction(System.Data.IsolationLevel? isolationLevel = null)
        {
            _RepoBase.AppDbContext.Database.BeginTransaction();
        }
        protected async Task BeginTransactionAsync(System.Data.IsolationLevel? isolationLevel = null)
        {
            await _RepoBase.AppDbContext.Database.BeginTransactionAsync();
        }

        protected void CommitTransaction()
        {
            _RepoBase.AppDbContext.Database.CommitTransaction();
        }

        protected void RollbackTransaction()
        {
            _RepoBase.AppDbContext.Database.RollbackTransaction();
        }

        protected UserInfo GetCurrentUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userNameClaim = User.Claims.FirstOrDefault(x => x.Type == StringHelper.UserNameClaimType);
                var systemTypeClaim = User.Claims.FirstOrDefault(x => x.Type == StringHelper.SystemTypeClaimType);

                UserInfo userInfo = null;

                // systemClaimType == null => login from SSO
                // else login from web (for service)
                if (systemTypeClaim != null)
                {
                    userInfo = UserInfo.GetFromCache(_Cache, User.Identity.Name);
                }
                else
                {
                    string urnIdentity = $"urn:identity:{User.Identity.Name}";
                    userInfo = UserInfo.GetFromCache(_Cache, urnIdentity);
                    if (userInfo != null)
                    {
                        SetUserSession(userInfo);
                        return userInfo;
                    }

                    var user = _RepoBase.AppDbContext.IC_UserAccount.FirstOrDefault(x => x.UserName.Equals(userNameClaim.Value, StringComparison.OrdinalIgnoreCase));
                    if (user == null) return null;

                    EPAD_Services.Interface.IHR_UserService hrUserService = TryResolve<EPAD_Services.Interface.IHR_UserService>();

                    var employee = hrUserService.FirstOrDefault(x => x.EmployeeATID == user.EmployeeATID && x.CompanyIndex == user.CompanyIndex);

                    if (employee == null) return null;

                    userInfo = new UserInfo(user.EmployeeATID)
                    {
                        FullName = employee.FullName,
                        EmployeeATID = employee.EmployeeATID,
                        UserName = userNameClaim.Value,
                        CompanyIndex = user.CompanyIndex,
                        Language = "vi",
                        PrivilegeIndex = user.AccountPrivilege
                    };
                    userInfo.AddToCache(_Cache, urnIdentity, TimeSpan.FromHours(1));
                }

                SetUserSession(userInfo);
                return userInfo;
            }
            return null;
        }

        private void SetUserSession(UserInfo user)
        {
            if (user == null) return;
            HttpContext.Session.SetInt32("PrivilegeIndex", user.PrivilegeIndex);
            HttpContext.Session.SetInt32("CompanyIndex", user.CompanyIndex);
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("Language", user.Language);
        }
    }
}
