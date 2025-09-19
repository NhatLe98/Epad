using Chilkat;
using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Enums;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TA_AjustAttendanceLogHistoryController : ApiControllerBase
    {
        private IMemoryCache cache;
        ITA_AjustAttendanceLogHistoryService _TA_AjustAttendanceLogHistoryService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        public TA_AjustAttendanceLogHistoryController(IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _TA_AjustAttendanceLogHistoryService = TryResolve<ITA_AjustAttendanceLogHistoryService>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
        }

        [Authorize]
        [ActionName("GetAjustAttendanceLogHistoryAtPage")]
        [HttpPost]
        public IActionResult GetAjustAttendanceLogHistoryAtPage([FromBody] TA_AjustAttendanceLogHistoryParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var fromDate = Convert.ToDateTime(param.FromDate);
            var toDate = Convert.ToDateTime(param.ToDate);
            var result = _TA_AjustAttendanceLogHistoryService.GetDataGrid(user.CompanyIndex, param.Page, param.Limit, param.Filter, param.Departments, param.EmployeeATIDs, fromDate, toDate, param.Operators);
            return ApiOk(result);
        }
    }
}
