using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Spire.Xls.Core.Spreadsheet.AutoFilter;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TA_TimeAttendanceLogController : ApiControllerBase
    {
        ITA_TimeAttendanceLogService _TA_TimeAttendanceLogService;
        private string mLinkCoreETA_Api;
        private readonly IHR_UserService _HR_UserService;
        private IMemoryCache cache;
        ITA_AjustTimeAttendanceLogService _TA_AjustTimeAttendanceLogService;
        public TA_TimeAttendanceLogController(IConfiguration configuration, IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _TA_TimeAttendanceLogService = TryResolve<ITA_TimeAttendanceLogService>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _TA_AjustTimeAttendanceLogService = TryResolve<ITA_AjustTimeAttendanceLogService>();
            mLinkCoreETA_Api = _Configuration.GetValue<string>("CoreETA_Api");
        }

        [Authorize]
        [ActionName("CaculateAttendance")]
        [HttpPost]
        public async Task<IActionResult> CaculateAttendance(TA_TimeAttendanceProccess request)
        {
            var user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            if (!string.IsNullOrEmpty(mLinkCoreETA_Api))
            {
                await _TA_TimeAttendanceLogService.SendTimeLogToECMSAPIAsync(request);
            }

            return ApiOk();
        }

        [Authorize]
        [ActionName("GetCaculateAttendanceData")]
        [HttpPost]
        public async Task<IActionResult> GetCaculateAttendanceData(SyntheticAttendanceRequest request)
        {
            var user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var syntheticAttendanceLog = await _TA_TimeAttendanceLogService.GetCaculateAttendanceData(request, user.CompanyIndex);
            return ApiOk(syntheticAttendanceLog);
        }

        [Authorize]
        [ActionName("GetSyntheticAttendanceData")]
        [HttpPost]
        public IActionResult GetSyntheticAttendanceData([FromBody] TA_AjustAttendanceLogParam param)
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
            table.Rows = _TA_AjustTimeAttendanceLogService.GetSyntheticDataGrid(user.CompanyIndex, param.Page, param.Limit, param.Filter, param.Departments, param.EmployeeATIDs, fromDate, toDate, param.FilterByType);
            return ApiOk(table);
        }


        [ActionName("CaculateAttendanceByDay")]
        [HttpGet]
        public async Task<IActionResult> CaculateAttendanceByDay()
        {
            if (!string.IsNullOrEmpty(mLinkCoreETA_Api))
            {
                var listEmployee = await _HR_UserService.GetAllEmployeeCompactInfo(2);
                var request = new TA_TimeAttendanceProccess();

                request.EmployeeATIDs = listEmployee.Select(x => x.EmployeeATID).ToList();
                request.FromDate = DateTime.Now.AddDays(-1);
                request.ToDate = DateTime.Now;
                await _TA_TimeAttendanceLogService.SendTimeLogToECMSAPIAsync(request);
            }

            return ApiOk();
        }

    }



}
