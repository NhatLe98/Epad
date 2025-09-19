using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TA_AjustTimeAttendanceLogController : ApiControllerBase
    {
        private IMemoryCache cache;
        ITA_AjustTimeAttendanceLogService _TA_AjustTimeAttendanceLogService;
        public TA_AjustTimeAttendanceLogController(IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _TA_AjustTimeAttendanceLogService = TryResolve<ITA_AjustTimeAttendanceLogService>();
        }

        [Authorize]
        [ActionName("GetAjustTimeAttendanceLogAtPage")]
        [HttpPost]
        public IActionResult GetAjustTimeAttendanceLogAtPage([FromBody] TA_AjustAttendanceLogParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var fromDate = Convert.ToDateTime(param.FromDate);
            var toDate = Convert.ToDateTime(param.ToDate);

            var listDate = Enumerable.Range(0, 1 + toDate.Date.Subtract(fromDate.Date).Days).Select(offset => fromDate.Date.AddDays(offset)).ToArray();

            #region SetColumn
            var table = new HeaderTableAjust();
         
            table.Columns.Add(new ColumnData
            {
                Index = 0,
                Name = "MCC",
                Code = "EmployeeATID"
            });
            table.Columns.Add(new ColumnData
            {
                Index = 1,
                Name = "EmployeeCode",
                Code = "EmployeeCode"
            });
            table.Columns.Add(new ColumnData
            {
                Index = 2,
                Name = "FullName",
                Code = "FullName"
            });
            table.Columns.Add(new ColumnData
            {
                Index = 3,
                Name = "Department",
                Code = "DepartmentName"
            });
            int index = 5;
            foreach (var date in listDate)
            {
                table.Columns.Add(new ColumnData
                {
                    Index = index,
                    Name = date.Day.ToString(),
                    Code = date.ToString("yyyy/MM/dd")
                });
                index++;
            }
            table.Columns.Add(new ColumnData
            {
                Index = 50,
                Name = "TotalWork",
                Code = "TotalWorkingDay"
            });
            table.Columns.Add(new ColumnData
            {
                Index = 51,
                Name = "WorkingDay(X)",
                Code = "WorkingDay"
            });
            table.Columns.Add(new ColumnData
            {
                Index = 52,
                Name = "AnnualLeave(P)",
                Code = "AnnualLeave"
            });
            table.Columns.Add(new ColumnData
            {
                Index = 53,
                Name = "Leave(V)",
                Code = "Leave"
            });
            table.Columns.Add(new ColumnData
            {
                Index = 54,
                Name = "NoSalaryLeave(KL)",
                Code = "NoSalaryLeave"
            });
            table.Columns.Add(new ColumnData
            {
                Index = 55,
                Name = "BusinessTrip(CT)",
                Code = "BusinessTrip"
            });
            #endregion

            table.Rows = _TA_AjustTimeAttendanceLogService.GetDataGrid(user.CompanyIndex, param.Page, param.Limit, param.Filter, param.Departments, param.EmployeeATIDs, fromDate, toDate);
            return ApiOk(table);
        }

        [Authorize]
        [ActionName("UpdateAjustTimeAttendanceLog")]
        [HttpPost]
        public IActionResult UpdateAjustTimeAttendanceLog(AjustTimeAttendanceLogInsertParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            _TA_AjustTimeAttendanceLogService.UpdateAjustTimeAttendanceLog(param, user);
            return Ok();
        }

        [Authorize]
        [ActionName("GetAllRegistrationType")]
        [HttpGet]
        public IActionResult GetAllRegistrationType()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var result = _TA_AjustTimeAttendanceLogService.GetAllRegistrationType();
            return Ok(result);


        }
    }
}
