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

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GC_Rules_GeneralController : ApiControllerBase // Quy định chung
    {
        IGC_Rules_GeneralService _GC_Rules_GeneralService;
        IGC_Rules_General_LogService _GC_Rules_General_LogService;
        public GC_Rules_GeneralController(IConfiguration configuration, IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            _GC_Rules_GeneralService = TryResolve<IGC_Rules_GeneralService>();
            _GC_Rules_General_LogService = TryResolve<IGC_Rules_General_LogService>();
        }

        [Authorize]
        [ActionName("GetRulesGeneralByCompanyIndex")]
        [HttpGet]
        public async Task<IActionResult> GetRulesGeneralByCompanyIndex()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var dummy = await _GC_Rules_GeneralService.GetRulesGeneralByCompanyIndex(user.CompanyIndex);
            return ApiOk(dummy);
        }

        [Authorize]
        [ActionName("GetRulesGeneralRunWithoutScreen")]
        [HttpGet]
        public IActionResult GetRulesGeneralRunWithoutScreen()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var dummy = _GC_Rules_GeneralService.GetRulesGeneralUsing(user.CompanyIndex);
            if (dummy != null)
            {
                return ApiOk(dummy.RunWithoutScreen);
            }
            return ApiOk(false);
        }

        [Authorize]
        [ActionName("GetRuleGeneralLogByRuleGeneralIndex")]
        [HttpGet]
        public async Task<IActionResult> GetRuleGeneralLogByRuleGeneralIndex(int rulesGeneralIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var dummy = await _GC_Rules_GeneralService.GetRulesGeneralLog(rulesGeneralIndex, user.CompanyIndex);
            return ApiOk(dummy);
        }

        [Authorize]
        [ActionName("AddRulesGeneral")]
        [HttpPost]
        public IActionResult AddRulesGeneral([FromBody] GC_Rules_General data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var oldData = _GC_Rules_GeneralService.FirstOrDefault(e => e.Name == data.Name);
            if (oldData != null)
            {
                return ApiConflict("RulesGeneralNameIsExist");
            }
            var dummy = _GC_Rules_GeneralService.GetRulesGeneralByCompanyIndex(user.CompanyIndex).Result;
            var ruleIsUsing = dummy.FirstOrDefault(x => x.IsUsing == true);
            if (ruleIsUsing != null && data.IsUsing == true)
            {
                return ApiConflict("RulesAlreadyInUsePleaseChooseAnotherRule");
            }
            var newData = new GC_Rules_General()
            {
                CompanyIndex = user.CompanyIndex,
                UpdatedDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                UpdatedUser = user.UserName,
                Name = data.Name,
                NameInEng = data.NameInEng,
                FromDate = data.FromDate,
                ToDate = data.ToDate,
                StartTimeDay = data.StartTimeDay,
                MaxAttendanceTime = data.MaxAttendanceTime,
                IsUsing = data.IsUsing,
                PresenceTrackingTime = data.PresenceTrackingTime,
                RunWithoutScreen = data.RunWithoutScreen,
                IgnoreInLog = data.IgnoreInLog
            };
            _GC_Rules_GeneralService.Insert(newData);
            SaveChange();

            return ApiOk(newData.Index);
        }

        [Authorize]
        [ActionName("UpdateRulesGeneral")]
        [HttpPut]
        public IActionResult UpdateRulesGeneral([FromBody] GC_Rules_General data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var oldData = _GC_Rules_GeneralService.GetRulesGeneralByIndexAndCompanyIndex(data.Index, user.CompanyIndex);
            if (oldData == null)
            {
                return ApiConflict("RulesGeneralNotExist");
            }

            var checkExist = _GC_Rules_GeneralService.FirstOrDefault(e => e.Name == data.Name && e.Index != data.Index);
            if (checkExist != null)
            {
                return ApiConflict("RulesGeneralNameIsExist");
            }
            //Check rule is using
            var dummy = _GC_Rules_GeneralService.GetRulesGeneralByCompanyIndex(user.CompanyIndex).Result;
            var ruleIsUsing = dummy.FirstOrDefault(x => x.Index != data.Index && x.IsUsing == true && data.IsUsing == true);
            if (ruleIsUsing != null)
            {
                return ApiConflict("RulesAlreadyInUsePleaseChooseAnotherRule");
            }

            oldData.UpdatedDate = DateTime.Now;
            oldData.UpdatedUser = user.UserName;
            oldData.Name = data.Name;
            oldData.NameInEng = data.NameInEng;
            oldData.FromDate = data.FromDate;
            oldData.ToDate = data.ToDate;
            oldData.StartTimeDay = data.StartTimeDay;
            oldData.MaxAttendanceTime = data.MaxAttendanceTime;
            oldData.IsUsing = data.IsUsing;
            oldData.IsBypassRule = data.IsBypassRule;
            oldData.PresenceTrackingTime = data.PresenceTrackingTime;
            oldData.RunWithoutScreen = data.RunWithoutScreen;
            oldData.IgnoreInLog = data.IgnoreInLog;
            _GC_Rules_GeneralService.Update(oldData);
            SaveChange();
            return ApiOk(oldData.Index);
        }
        [Authorize]
        [ActionName("DeleteRulesGeneral")]
        [HttpDelete]
        public IActionResult DeleteRulesGeneral(int index)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var oldData = _GC_Rules_GeneralService.GetRulesGeneralByIndexAndCompanyIndex(index, user.CompanyIndex);
            if (oldData == null)
            {
                return ApiConflict("RulesGeneralNotExist");
            }
            var listSchedule = _GC_Rules_GeneralService.GetRulesGeneralLog(oldData.Index).Result;
            if (listSchedule != null && listSchedule.Any())
            {
                foreach (var item in listSchedule)
                {
                    _GC_Rules_General_LogService.Delete(item);
                }
            }
            _GC_Rules_GeneralService.Delete(oldData);

            SaveChange();
            return ApiOk();
        }

        [Authorize]
        [ActionName("AddRulesGeneralLog")]
        [HttpPost]
        public IActionResult AddRulesGeneralLog(List<GC_Rules_General_Log> data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var first = data.FirstOrDefault();
            if (first == null)
            {
                return ApiOk();
            }

            var listOldDate = _GC_Rules_GeneralService.GetRulesGeneralLog(first.RuleGeneralIndex, user.CompanyIndex).Result;
            if (listOldDate != null && listOldDate.Any())
            {
                foreach (var item in listOldDate)
                {
                    _GC_Rules_General_LogService.Delete(item);
                }
            }
            SaveChange();
            var baseDate = new DateTime(2020, 1, 1);
            foreach (var item in data)
            {
                var newData = new GC_Rules_General_Log()
                {
                    CompanyIndex = user.CompanyIndex,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    UpdatedUser = user.UserName,

                    UseDeviceMode = item.UseDeviceMode,
                    UseMinimumLog = item.UseMinimumLog,
                    UseSequenceLog = item.UseSequenceLog,
                    UseTimeLog = item.UseTimeLog,
                    UseMode = item.UseMode,

                    AreaGroupIndex = item.AreaGroupIndex,
                    RuleGeneralIndex = item.RuleGeneralIndex,
                    MinimumLog = item.UseMode == 2 ? item.MinimumLog : 1,
                };

                if (item.UseMode == 3)
                {
                    newData.FromIsNextDay = item.FromIsNextDay;
                    newData.ToIsNextDay = item.ToIsNextDay;
                    newData.ToLateIsNextDay = item.ToLateIsNextDay;

                    newData.FromEarlyDate = NewDateFromTwoDateTime(baseDate, item.FromEarlyDate.Value);
                    newData.FromDate = NewDateFromTwoDateTime(baseDate, item.FromDate.Value, item.FromIsNextDay);
                    newData.ToDate = NewDateFromTwoDateTime(baseDate, item.ToDate.Value, item.ToIsNextDay);
                    newData.ToLateDate = NewDateFromTwoDateTime(baseDate, item.ToLateDate.Value, item.ToLateIsNextDay);
                }

                _GC_Rules_General_LogService.Insert(newData);
            }
            SaveChange();

            return ApiOk();
        }
        private DateTime NewDateFromTwoDateTime(DateTime baseDate, DateTime dateTime, bool isNexDay = false)
        {
            var day = isNexDay ? baseDate.Day + 1 : baseDate.Day;
            return new DateTime(baseDate.Year, baseDate.Month, day, dateTime.Hour, dateTime.Minute, 0);
        }
    }
}
