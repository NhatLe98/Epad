using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EPAD_Common;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace EPAD_Backend_Core.CustomMiddleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class PrivilegeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private Dictionary<string, string> _dicFormName;
        HashSet<string> _ignoreRoute;
        public PrivilegeMiddleware(RequestDelegate next, IMemoryCache pCache)
        {
            _next = next;
            _cache = pCache;
            InitFormName();
        }

        private void InitFormName()
        {
            _dicFormName = new Dictionary<string, string>();
            _ignoreRoute = new HashSet<string>() {  "ActivateLicense", "HR_StudentInfo/Get_HR_Student_ClassInfo", "v1​/oauth2", "UserNotification/PostMany", "HR_User/GetEmployeeLookup", "Department/GetAll", "AttendanceLog/GetAtPageAttendanceLog", "GetDevicesByServiceOffline" };
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                if (httpContext.User.Identity.IsAuthenticated == true && _ignoreRoute.Count(x => httpContext.Request.Path.Value.EndsWith(x)) == 0)
                {
                    UserInfo user = UserInfo.GetFromCache(_cache, httpContext.User.Identity.Name);
                    if (user == null)
                    {
                        httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        httpContext.Response.ContentType = "application/json";
                        await httpContext.Response.WriteAsync("Error");
                    }
                    else if (MainProcess.PrivilegeFunction.CheckPrivilegeForAction(user, httpContext) == false)
                    {
                        httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        httpContext.Response.ContentType = "application/json";
                        await httpContext.Response.WriteAsync("NotPrivilege");
                    }
                    else
                    {
                        //only save userlog for user action
                        if (user.ServiceName == null || user.ServiceName == "")
                        {
                            var listUserAccountLog = new List<UserAccountLog>();
                            string action = "";
                            string path = httpContext.Request.Path.Value.ToLower();
                            if (path.Contains("insert") || path.Contains("add")) action = "Insert";
                            else if (path.Contains("update")) action = "Update";
                            else if (path.Contains("delete")) action = "Delete";
                            else action = "";
                            string controllerName = httpContext.Request.Path.Value.Split('/')[2].ToString();
                            string formName = controllerName;

                            if (action != "")
                            {
                                var userAccountLog = new UserAccountLog()
                                {
                                    Time = DateTime.Now,
                                    UserName = user.UserName,
                                    Action = action,
                                    FormName = formName,
                                    Detail = ""
                                };
                                listUserAccountLog.Add(userAccountLog);
                                var mongoObject = new MongoDBHelper<UserAccountLog>("user_account_log", _cache);
                                mongoObject.AddListDataToCollection(listUserAccountLog, true);
                            }
                        }
                        await _next(httpContext);
                    }
                }
                else
                {
                    await _next(httpContext);
                }
            }
            catch { }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class PrivilegeMiddlewareExtensions
    {
        public static IApplicationBuilder UsePrivilegeMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PrivilegeMiddleware>();
        }
    }
}
