using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using EPAD_Data.Models.IC;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/HR_Rules_InOutTime/[action]")]
    [ApiController]
    public class HR_Rules_InOutTimeController : ApiControllerBase
    {
        private EPAD_Context context;
        private ezHR_Context otherContext;
        private IMemoryCache cache;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHR_Rules_InOutTimeService _iHR_Rules_InOutTimeService;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        private List<long> Ids { get; set; }

        public HR_Rules_InOutTimeController(IServiceProvider provider, IHostingEnvironment hostingEnvironment) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            otherContext = TryResolve<ezHR_Context>();
            _iHR_Rules_InOutTimeService = TryResolve<IHR_Rules_InOutTimeService>();
            _hostingEnvironment = hostingEnvironment;
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
        }

        [Authorize]
        [ActionName("GetAllRules")]
        [HttpGet]
        public IActionResult GetAllRules()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var listRules = _iHR_Rules_InOutTimeService.GetAllRules(user);

            return ApiOk(listRules);
        }

        [Authorize]
        [ActionName("AddRuleInOutTime")]
        [HttpPost]
        public async Task<IActionResult> AddRuleInOutTimeAsync([FromBody] Rules_InOutTimeParam param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            if (string.IsNullOrWhiteSpace(param.FromDate))
            {
                return ApiError("PleaseFillAllRequiredFields");
            }

            var format = "dd-MM-yyyy";
            if (DateTime.TryParseExact(param.FromDate, format, null, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                var existedRule = await _iHR_Rules_InOutTimeService.GetByDate(date, user);
                if (existedRule != null)
                {
                    return ApiError("RuleExisted");
                }
                else
                {
                    var isSuccess = await _iHR_Rules_InOutTimeService.AddRuleInOutTime(param, user);
                    return ApiOk(isSuccess);
                }
            }
            else
            {
                return ApiError("DateTimeWrongFormat");
            }
        }

        [Authorize]
        [ActionName("UpdateRuleInOutTime")]
        [HttpPost]
        public async Task<IActionResult> UpdateRuleInOutTime([FromBody] Rules_InOutTimeParam param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            if (string.IsNullOrWhiteSpace(param.FromDate))
            {
                return ApiError("PleaseFillAllRequiredFields");
            }
            var format = "dd-MM-yyyy";
            if (DateTime.TryParseExact(param.FromDate, format, null, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                var existedRule = await _iHR_Rules_InOutTimeService.GetByDate(date, user);
                if (existedRule != null && existedRule.Index != param.Index)
                {
                    return ApiError("RuleExisted");
                }
                else
                {
                    var checkTimeFormat = "dd-MM-yyyy HH:mm:ss";
                    if (DateTime.TryParseExact(param.CheckInTimeString, checkTimeFormat, null, System.Globalization.DateTimeStyles.None,
                        out DateTime checkInTime))
                    {
                        param.CheckInTime = checkInTime;
                    }
                    if (DateTime.TryParseExact(param.CheckOutTimeString, checkTimeFormat, null, System.Globalization.DateTimeStyles.None,
                        out DateTime checkOutTime))
                    {
                        param.CheckOutTime = checkOutTime;
                    }

                    if (param.Index != 0)
                    {
                        var rule = await _iHR_Rules_InOutTimeService.GetByIndex(param.Index);
                        if (rule != null)
                        {
                            var isSuccess = await _iHR_Rules_InOutTimeService.UpdateRuleInOutTime(param, user);
                            return ApiOk(isSuccess);
                        }
                        else
                        {
                            return ApiError("RuleNotExist");
                        }
                    }
                    else
                    {
                        var isSuccess = await _iHR_Rules_InOutTimeService.AddRuleInOutTime(param, user);
                        return ApiOk(isSuccess);
                    }
                }
            }
            else
            {
                return ApiError("DateTimeWrongFormat");
            }
        }

        [Authorize]
        [ActionName("DeleteRuleInOutTime")]
        [HttpDelete("{ruleIndex}")]
        public async Task<IActionResult> DeleteRuleInOutTimeAsync(int ruleIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var isSuccess = await _iHR_Rules_InOutTimeService.DeleteRuleInOutTime(ruleIndex);
            return ApiOk(isSuccess);
        }
    }
}
