using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common;
using EPAD_Common.Extensions;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.IC;
using EPAD_Data.Models.TimeLog;
using EPAD_Logic;
using EPAD_Logic.SendMail;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/Dashboard/[action]")]
    [ApiController]
    public class IC_DashboardController : ApiControllerBase
    {
        private readonly string _configClientName;
        private readonly IIC_DashboardService _IC_DashboardService;
        public IC_DashboardController(IServiceProvider provider) : base(provider)
        {
            _Logger = _LoggerFactory.CreateLogger<IC_AttendanceLogController>();
            _configClientName = _Configuration.GetValue<string>("ClientName").ToUpper();
            _IC_DashboardService = TryResolve<IIC_DashboardService>();
        }

        [Authorize]
        [ActionName("SaveDashboard")]
        [HttpPost]
        public async Task<IActionResult> SaveDashboard(IC_DashboardDTO dashboard)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_DashboardService.SaveDashboard(user.UserName, dashboard.DashboardConfig);

            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetDashboard")]
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_DashboardService.GetDashboard(user.UserName);

            return ApiOk(result);
        }
    }
}
