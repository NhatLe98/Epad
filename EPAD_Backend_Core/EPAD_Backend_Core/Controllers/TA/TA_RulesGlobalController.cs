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
    public class TA_RulesGlobalController : ApiControllerBase
    {
        ITA_RulesGlobalService _TA_RulesGlobalService;
        public TA_RulesGlobalController(IConfiguration configuration, IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            _TA_RulesGlobalService = TryResolve<ITA_RulesGlobalService>();
        }

        [Authorize]
        [ActionName("GetRulesGlobal")]
        [HttpGet]
        public async Task<IActionResult> GetRulesGlobal()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = await _TA_RulesGlobalService.GetRulesGlobal(user);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("UpdateRulesGlobal")]
        [HttpPut]
        public async Task<IActionResult> UpdateRulesGlobal([FromBody] TA_Rules_GlobalDTO data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var format = "yyyy-MM-dd HH:mm:ss";
            if (!string.IsNullOrWhiteSpace(data.NightShiftStartTimeString))
            {
                if (!DateTime.TryParseExact(data.NightShiftStartTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.NightShiftStartTime = time;
            }
            if (!string.IsNullOrWhiteSpace(data.NightShiftEndTimeString))
            {
                if (!DateTime.TryParseExact(data.NightShiftEndTimeString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.NightShiftEndTime = time;
            }
            var isSuccess = await _TA_RulesGlobalService.UpdateRulesGlobal(data, user);

            return ApiOk(isSuccess);
        }
    }
}
