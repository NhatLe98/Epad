using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPAD_Common.Extensions;
using EPAD_Data.Models;
using EPAD_Data.Models.TimeLog;
using Microsoft.Extensions.Logging;

namespace EPAD_Backend_Core.CustomMiddleware
{
    public class CaptureRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private IMemoryCache _cache;
        private Dictionary<string, string> _dicFormName;
        HashSet<string> _ignoreRoute;
        private readonly ILogger _logger;

        public CaptureRequestMiddleware(RequestDelegate next, IMemoryCache pCache, ILoggerFactory _loggerFactory)
        {
            _next = next;
            _cache = pCache;
            _logger = _loggerFactory.CreateLogger<CaptureRequestMiddleware>();
            InitFormName();
        }

        private void InitFormName()
        {
            _dicFormName = new Dictionary<string, string>();
            _ignoreRoute = new HashSet<string>() {  "ActivateLicense", "UserNotification/PostMany", "v1​/oauth2" };
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                httpContext.Request.EnableBuffering(bufferThreshold: 5242880); // limit 5MB

                // Leave the body open so the next middleware can read it.
                using (var reader = new StreamReader(
                httpContext.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 5242880,
                leaveOpen: true))
                {
                    var body = await reader.ReadToEndAsync();
                    httpContext.Request.Body.Position = 0;
                    if (httpContext.Request.Path.ToString().Contains("AddAttendanceLogRealTime"))
                    {
                        var data = JsonConvert.DeserializeObject<AttendanceLogPram>(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        bool checkls = _cache.HaveHWLicense(data.SerialNumber);
                        if (!checkls)
                        {
                            return;
                        }
                    }
                    /*
                    if (httpContext.Request.Path.ToString().Contains("UpdateLastConnectionBySDK"))
                    {
                        var data = JsonConvert.DeserializeObject<SerialNumberInfos>(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (data.ListSerialNumber != null && data.ListSerialNumber.Count > 0)
                        {
                            bool checkls = _cache.HaveHWLicense(data.ListSerialNumber.FirstOrDefault()); ;
                            if (!checkls)
                            {
                                return;
                            }
                        }
                    }*/
                    if (httpContext.Request.Path.ToString().Contains("AddOrUpdateUserInfo"))
                    {
                        var data = JsonConvert.DeserializeObject<UserInfoPram>(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        bool checkls = _cache.HaveHWLicense(data.SerialNumber); ;
                        if (!checkls)
                        {
                            return; 
                        }
                    }
                    if (httpContext.Request.Path.ToString().Contains("GetSystemCommandNeedExecute"))
                    {
                        var data = JsonConvert.DeserializeObject<SerialNumberInfos>(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (data.ListSerialNumber != null && data.ListSerialNumber.Count > 0)
                        {
                            bool checkls = _cache.HaveHWLicense(data.ListSerialNumber.FirstOrDefault()); ;
                            if (!checkls)
                            {
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            await _next(httpContext);
        }

        private string GetMenuFromNameControllerName(string pControllerName)
        {
            string menuFromName = "";
            return menuFromName;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CaptureRequestMiddlewareExtensions
    {
        public static IApplicationBuilder UseCaptureRequestMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CaptureRequestMiddleware>();
        }
    }
}
