using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EPAD_Common.Utility.AppUtils;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TA_Rules_ShiftController : ApiControllerBase
    {
        ITA_RulesShiftService _TA_RulesShiftService;
        public TA_Rules_ShiftController(IConfiguration configuration, IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            _TA_RulesShiftService = TryResolve<ITA_RulesShiftService>();
        }

        [Authorize]
        [ActionName("GetRulesShiftByCompanyIndex")]
        [HttpGet]
        public async Task<IActionResult> GetRulesShiftByCompanyIndex()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = await _TA_RulesShiftService.GetAllRulesShift(user.CompanyIndex);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetRulesShiftByIndex")]
        [HttpGet]
        public async Task<IActionResult> GetRulesShiftByIndex(int RulesShiftIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = await _TA_RulesShiftService.GetRulesShiftByIndex(RulesShiftIndex);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("AddRulesShift")]
        [HttpPost]
        public async Task<IActionResult> AddRulesShift([FromBody] TA_Rules_ShiftDTO data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var format = "yyyy-MM-dd HH:mm:ss";
            if (!DateTime.TryParseExact(data.EarliestAttendanceRangeTimeString, format, null, System.Globalization.DateTimeStyles.None, 
                out var earliest))
            {
                return ApiError("DateTimeWrongFormat");
            }
            if (!DateTime.TryParseExact(data.LatestAttendanceRangeTimeString, format, null, System.Globalization.DateTimeStyles.None,
                out var latest))
            {
                return ApiError("DateTimeWrongFormat");
            }
            if (data.RuleInOutTime != null && data.RuleInOutTime.Count > 0) 
            {
                foreach (var item in data.RuleInOutTime)
                {
                    if (!DateTime.TryParseExact(item.FromTimeString, format, null, System.Globalization.DateTimeStyles.None,
                        out var fromTime))
                    {
                        return ApiError("DateTimeWrongFormat");
                    }
                    if (!DateTime.TryParseExact(item.ToTimeString, format, null, System.Globalization.DateTimeStyles.None,
                        out var toTime))
                    {
                        return ApiError("DateTimeWrongFormat");
                    }
                    item.FromTime = fromTime;
                    item.ToTime = toTime;
                }
            }
            data.EarliestAttendanceRangeTime = earliest;
            data.LatestAttendanceRangeTime = latest;

            data.UpdatedUser = user.FullName;
            data.CreatedDate = DateTime.Now;
            data.UpdatedDate = DateTime.Now;

            var nameExist = await _TA_RulesShiftService.GetRulesShiftByName(data.Name, user.CompanyIndex);
            if (nameExist != null && nameExist.Count > 0)
            {
                return ApiError("NameExisted");
            }

            var isSuccess = await _TA_RulesShiftService.AddRulesShift(data);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateRulesShift")]
        [HttpPut]
        public async Task<IActionResult> UpdateRulesShift([FromBody] TA_Rules_ShiftDTO data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var format = "yyyy-MM-dd HH:mm:ss";
            if (!DateTime.TryParseExact(data.EarliestAttendanceRangeTimeString, format, null, System.Globalization.DateTimeStyles.None,
                out var earliest))
            {
                return ApiError("DateTimeWrongFormat");
            }
            if (!DateTime.TryParseExact(data.LatestAttendanceRangeTimeString, format, null, System.Globalization.DateTimeStyles.None,
                out var latest))
            {
                return ApiError("DateTimeWrongFormat");
            }
            if (data.RuleInOutTime != null && data.RuleInOutTime.Count > 0)
            {
                foreach (var item in data.RuleInOutTime)
                {
                    if (!DateTime.TryParseExact(item.FromTimeString, format, null, System.Globalization.DateTimeStyles.None,
                        out var fromTime))
                    {
                        return ApiError("DateTimeWrongFormat");
                    }
                    if (!DateTime.TryParseExact(item.ToTimeString, format, null, System.Globalization.DateTimeStyles.None,
                        out var toTime))
                    {
                        return ApiError("DateTimeWrongFormat");
                    }
                    item.FromTime = fromTime;
                    item.ToTime = toTime;
                }
            }
            data.EarliestAttendanceRangeTime = earliest;
            data.LatestAttendanceRangeTime = latest;

            data.UpdatedUser = user.FullName;
            data.UpdatedDate = DateTime.Now;

            var nameExist = await _TA_RulesShiftService.GetRulesShiftByName(data.Name, user.CompanyIndex);
            if (nameExist != null && nameExist.Count > 0 && nameExist.Any(x => x.Index != data.Index))
            {
                return ApiError("NameExisted");
            }

            var dataExist = await _TA_RulesShiftService.GetRulesShiftByIndex(data.Index);
            if (dataExist == null)
            {
                return ApiError("RuleNotExist");
            }

            var isSuccess = await _TA_RulesShiftService.UpdateRulesShift(data);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("IsRuleUsing")]
        [HttpGet]
        public async Task<IActionResult> IsRuleUsing(int index)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var dataExist = await _TA_RulesShiftService.GetRulesShiftByIndex(index);
            if (dataExist == null)
            {
                return ApiError("RuleNotExist");
            }

            var isRuleUsing = await _TA_RulesShiftService.IsRuleUsing(index);

            return ApiOk(isRuleUsing);
        }

        [Authorize]
        [ActionName("DeleteRulesShift")]
        [HttpDelete]
        public async Task<IActionResult> DeleteRulesShift(int index)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var dataExist = await _TA_RulesShiftService.GetRulesShiftByIndex(index);
            if (dataExist == null)
            {
                return ApiError("RuleNotExist");
            }

            var isRuleUsing = await _TA_RulesShiftService.IsRuleUsing(index);
            if (isRuleUsing)
            {
                return ApiError("RuleIsUsing");
            }

            var isSuccess = await _TA_RulesShiftService.DeleteRulesShift(index);

            return ApiOk(isSuccess);
        }
    }
}
