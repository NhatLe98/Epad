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
    public class TA_ShiftController : ApiControllerBase
    {
        ITA_ShiftService _TA_ShiftService;
        public TA_ShiftController(IConfiguration configuration, IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            _TA_ShiftService = TryResolve<ITA_ShiftService>();
        }

        [Authorize]
        [ActionName("GetShiftByCompanyIndex")]
        [HttpGet]
        public async Task<IActionResult> GetShiftByCompanyIndex()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = await _TA_ShiftService.GetAllShift(user.CompanyIndex);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetShiftByIndex")]
        [HttpGet]
        public async Task<IActionResult> GetShiftByIndex(int ShiftIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = await _TA_ShiftService.GetShiftByIndex(ShiftIndex);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("AddShift")]
        [HttpPost]
        public async Task<IActionResult> AddShift([FromBody] TA_ShiftDTO data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var format = "yyyy-MM-dd HH:mm:ss";
            if (!string.IsNullOrWhiteSpace(data.PaidHolidayStartTimeString))
            {
                if (!DateTime.TryParseExact(data.PaidHolidayStartTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.PaidHolidayStartTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.PaidHolidayEndTimeString))
            {
                if (!DateTime.TryParseExact(data.PaidHolidayEndTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.PaidHolidayEndTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.BreakStartTimeString))
            {
                if (!DateTime.TryParseExact(data.BreakStartTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.BreakStartTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.BreakEndTimeString))
            {
                if (!DateTime.TryParseExact(data.BreakEndTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.BreakEndTime = time;
            }
            //Overtime first
            if (!string.IsNullOrWhiteSpace(data.OTStartTimeFirstString))
            {
                if (!DateTime.TryParseExact(data.OTStartTimeFirstString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.OTStartTimeFirst = time;
            }

            if (!string.IsNullOrWhiteSpace(data.OTEndTimeFirstString))
            {
                if (!DateTime.TryParseExact(data.OTEndTimeFirstString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.OTEndTimeFirst = time;
            }

            if (!string.IsNullOrWhiteSpace(data.OTStartTimeString))
            {
                if (!DateTime.TryParseExact(data.OTStartTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.OTStartTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.OTEndTimeString))
            {
                if (!DateTime.TryParseExact(data.OTEndTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.OTEndTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.CheckInTimeString))
            {
                if (!DateTime.TryParseExact(data.CheckInTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.CheckInTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.CheckOutTimeString))
            {
                if (!DateTime.TryParseExact(data.CheckOutTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.CheckOutTime = time;
            }

            data.UpdatedUser = user.FullName;
            data.CreatedDate = DateTime.Now;
            data.UpdatedDate = DateTime.Now;

            var nameExist = await _TA_ShiftService.GetShiftByName(data.Name, user.CompanyIndex);
            if (nameExist != null && nameExist.Count > 0)
            {
                return ApiError("NameExisted");
            }

            var codeExist = await _TA_ShiftService.GetShiftByCode(data.Code, user.CompanyIndex);
            if (codeExist != null && codeExist.Count > 0)
            {
                return ApiError("CodeExisted");
            }

            var isSuccess = await _TA_ShiftService.AddShift(data);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateShift")]
        [HttpPut]
        public async Task<IActionResult> UpdateShift([FromBody] TA_ShiftDTO data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var format = "yyyy-MM-dd HH:mm:ss";
            if (!string.IsNullOrWhiteSpace(data.PaidHolidayStartTimeString))
            {
                if (!DateTime.TryParseExact(data.PaidHolidayStartTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.PaidHolidayStartTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.PaidHolidayEndTimeString))
            {
                if (!DateTime.TryParseExact(data.PaidHolidayEndTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.PaidHolidayEndTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.BreakStartTimeString))
            {
                if (!DateTime.TryParseExact(data.BreakStartTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.BreakStartTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.BreakEndTimeString))
            {
                if (!DateTime.TryParseExact(data.BreakEndTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.BreakEndTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.OTStartTimeFirstString))
            {
                if (!DateTime.TryParseExact(data.OTStartTimeFirstString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.OTStartTimeFirst = time;
            }

            if (!string.IsNullOrWhiteSpace(data.OTEndTimeFirstString))
            {
                if (!DateTime.TryParseExact(data.OTEndTimeFirstString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.OTEndTimeFirst = time;
            }
            if (!string.IsNullOrWhiteSpace(data.OTStartTimeString))
            {
                if (!DateTime.TryParseExact(data.OTStartTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.OTStartTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.OTEndTimeString))
            {
                if (!DateTime.TryParseExact(data.OTEndTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.OTEndTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.CheckInTimeString))
            {
                if (!DateTime.TryParseExact(data.CheckInTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.CheckInTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.CheckOutTimeString))
            {
                if (!DateTime.TryParseExact(data.CheckOutTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.CheckOutTime = time;
            }

            data.UpdatedUser = user.FullName;
            data.UpdatedDate = DateTime.Now;

            var nameExist = await _TA_ShiftService.GetShiftByName(data.Name, user.CompanyIndex);
            if (nameExist != null && nameExist.Count > 0 && nameExist.Any(x => x.Index != data.Index))
            {
                return ApiError("NameExisted");
            }

            var codeExist = await _TA_ShiftService.GetShiftByCode(data.Code, user.CompanyIndex);
            if (codeExist != null && codeExist.Count > 0 && codeExist.Any(x => x.Index != data.Index))
            {
                return ApiError("CodeExisted");
            }

            var dataExist = await _TA_ShiftService.GetShiftByIndex(data.Index);
            if (dataExist == null)
            {
                return ApiError("RuleNotExist");
            }

            var isSuccess = await _TA_ShiftService.UpdateShift(data);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("DeleteShift")]
        [HttpDelete]
        public async Task<IActionResult> DeleteShift(int index)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var dataExist = await _TA_ShiftService.GetShiftByIndex(index);
            if (dataExist == null)
            {
                return ApiError("ShiftNotExist");
            }

            var isShiftUsing = await _TA_ShiftService.IsShiftUsing(index);
            if (isShiftUsing)
            {
                return ApiError("ShiftIsUsing");
            }

            var isSuccess = await _TA_ShiftService.DeleteShift(index);

            return ApiOk(isSuccess);
        }
    }
}
