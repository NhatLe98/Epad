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
    public class GC_Rules_GeneralAccessController : ApiControllerBase
    {
        IGC_Rules_GeneralAccessService _GC_Rules_GeneralAccessService;
        IGC_Rules_General_AreaGroupService _GC_Rules_General_AreaGroupService;
        IGC_Rules_GeneralAccess_GatesService _GC_Rules_GeneralAccess_GatesService;
        public GC_Rules_GeneralAccessController(IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            _GC_Rules_GeneralAccessService = TryResolve<IGC_Rules_GeneralAccessService>();
            _GC_Rules_General_AreaGroupService = TryResolve<IGC_Rules_General_AreaGroupService>();
            _GC_Rules_GeneralAccess_GatesService = TryResolve<IGC_Rules_GeneralAccess_GatesService>();
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
            var rulesAreaGroup = _GC_Rules_General_AreaGroupService.Where(e => e.CompanyIndex == user.CompanyIndex).ToList(); 
            var rulesGates = _GC_Rules_GeneralAccess_GatesService.Where(e => e.CompanyIndex == user.CompanyIndex).ToList();
            List<Rules_GeneralAccessParam> listRules = _GC_Rules_GeneralAccessService.Where(x=>x.CompanyIndex == user.CompanyIndex)
                .Select(x=> new Rules_GeneralAccessParam()
                {
                    Index=x.Index,
                    Name = x.Name,
                    NameInEng = x.NameInEng,
                    CheckInByShift = x.CheckInByShift,
                    CheckInTime = x.CheckInTime,
                    MaxEarlyCheckInMinute = x.MaxEarlyCheckInMinute,
                    MaxLateCheckInMinute = x.MaxLateCheckInMinute,
                    CheckOutByShift = x.CheckOutByShift,
                    CheckOutTime = x.CheckOutTime,
                    BeginLastHaftTime = x.BeginLastHaftTime,
                    EndFirstHaftTime = x.EndFirstHaftTime,
                    MaxEarlyCheckOutMinute = x.MaxEarlyCheckOutMinute,
                    MaxLateCheckOutMinute = x.MaxLateCheckOutMinute,
                    AllowFreeInAndOutInTimeRange = x.AllowFreeInAndOutInTimeRange,
                    AllowEarlyOutLateInMission = x.AllowEarlyOutLateInMission,
                    MissionMaxEarlyCheckOutMinute = x.MissionMaxEarlyCheckOutMinute,
                    MissionMaxLateCheckInMinute = x.MissionMaxLateCheckInMinute,
                    AdjustByLateInEarlyOut = x.AdjustByLateInEarlyOut,
                    AllowInLeaveDay = x.AllowInLeaveDay,
                    AllowInMission = x.AllowInMission,
                    AllowInBreakTime = x.AllowInBreakTime,
                    AllowCheckOutInWorkingTime = x.AllowCheckOutInWorkingTime,
                    AllowCheckOutInWorkingTimeRange = x.AllowCheckOutInWorkingTimeRange,
                    MaxMinuteAllowOutsideInWorkingTime = x.MaxMinuteAllowOutsideInWorkingTime,
                    DenyInLeaveWholeDay = x.DenyInLeaveWholeDay,
                    DenyInMissionWholeDay = x.DenyInMissionWholeDay,
                    DenyInStoppedWorkingInfo = x.DenyInStoppedWorkingInfo,
                    CheckLogByAreaGroup = x.CheckLogByAreaGroup,
                    CheckLogByShift = x.CheckLogByShift,
                    AreaGroups = _GC_Rules_General_AreaGroupService.GetAreaGroupByRulesIndex(rulesAreaGroup, x.Index, x.CompanyIndex),
                    ListGatesInfo = _GC_Rules_GeneralAccess_GatesService.GetListGateByRulesIndex(rulesGates, x.Index, x.CompanyIndex),
                }).ToList();

            return ApiOk(listRules);
        }

        [Authorize]
        [ActionName("AddRuleGeneral")]
        [HttpPost]
        public IActionResult AddRuleGeneral([FromBody] Rules_GeneralAccessParam param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            if (param.Name == "")
            {
                return ApiError("PleaseFillAllRequiredFields");
            }

            var ruleData = new GC_Rules_GeneralAccess();

            var duplicates = _GC_Rules_GeneralAccessService.Where(t => t.CompanyIndex == user.CompanyIndex && t.Name.Contains("Tên quy định"));
            var countDuplicate = duplicates.Count();

            ruleData.Name = countDuplicate > 0 ? string.Format("{0} {1}", param.Name, countDuplicate) : param.Name;
            ruleData.NameInEng = param.NameInEng;
            ruleData.CheckInByShift = param.CheckInByShift;
            ruleData.CheckInTime = param.CheckInTime;

            ruleData.MaxEarlyCheckInMinute = param.MaxEarlyCheckInMinute;
            ruleData.MaxLateCheckInMinute = param.MaxLateCheckInMinute;
            ruleData.CheckOutByShift = param.CheckOutByShift;
            ruleData.CheckOutTime = param.CheckOutTime;
            ruleData.EndFirstHaftTime = param.EndFirstHaftTime;
            ruleData.BeginLastHaftTime = param.BeginLastHaftTime;
            ruleData.MaxEarlyCheckOutMinute = param.MaxEarlyCheckOutMinute;
            ruleData.MaxLateCheckOutMinute = param.MaxLateCheckOutMinute;
            ruleData.AllowFreeInAndOutInTimeRange = param.AllowFreeInAndOutInTimeRange;
            ruleData.AllowEarlyOutLateInMission = param.AllowEarlyOutLateInMission;
            ruleData.MissionMaxEarlyCheckOutMinute = param.MissionMaxEarlyCheckOutMinute;
            ruleData.MissionMaxLateCheckInMinute = param.MissionMaxLateCheckInMinute;
            ruleData.AdjustByLateInEarlyOut = param.AdjustByLateInEarlyOut;

            ruleData.AllowInLeaveDay = param.AllowInLeaveDay;
            ruleData.AllowInMission = param.AllowInMission;
            ruleData.AllowInBreakTime = param.AllowInBreakTime;

            ruleData.AllowCheckOutInWorkingTime = param.AllowCheckOutInWorkingTime;
            ruleData.AllowCheckOutInWorkingTimeRange = param.AllowCheckOutInWorkingTimeRange;
            ruleData.MaxMinuteAllowOutsideInWorkingTime = param.MaxMinuteAllowOutsideInWorkingTime;

            ruleData.DenyInLeaveWholeDay = param.DenyInLeaveWholeDay;
            ruleData.DenyInMissionWholeDay = param.DenyInMissionWholeDay;
            ruleData.DenyInStoppedWorkingInfo = param.DenyInStoppedWorkingInfo;

            ruleData.CheckLogByAreaGroup = param.CheckLogByAreaGroup;
            ruleData.CheckLogByShift = param.CheckLogByShift;

            ruleData.CompanyIndex = user.CompanyIndex;
            ruleData.CreatedDate = DateTime.Now;

            ruleData.UpdatedDate = DateTime.Now;
            ruleData.UpdatedUser = user.UserName;


            _GC_Rules_GeneralAccessService.Insert(ruleData);

            /*SaveRuleAreaGroup*/
            DeleteAndInsertRulesGeneralAreaGroup(param.AreaGroups, ruleData.Index);
            SaveChange();

            return ApiOk(ruleData.Index);
        }

        [Authorize]
        [ActionName("UpdateRuleGeneral")]
        [HttpPut]
        public IActionResult UpdateRuleGeneral([FromBody] Rules_GeneralAccessParam param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            if (param.Name == "")
            {
                return ApiError("PleaseFillAllRequiredFields");
            }
            var duplicates = _GC_Rules_GeneralAccessService.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index != param.Index && t.Name == param.Name);
            if (duplicates.Any())
            {
                return ApiError("RuleNameHasExists");
            }

            var updateData = _GC_Rules_GeneralAccessService.FirstOrDefault(x=>x.Index == param.Index);
            if (updateData == null)
            {
                return ApiError("RuleNotExists");
            }

            updateData.Name = param.Name;
            updateData.NameInEng = param.NameInEng;
            updateData.CheckInByShift = param.CheckInByShift;
            updateData.CheckInTime = param.CheckInTime!=null ? (new DateTime(2000,01,01, param.CheckInTime.Value.Hour, param.CheckInTime.Value.Minute,0)): param.CheckInTime;

            updateData.MaxEarlyCheckInMinute = param.MaxEarlyCheckInMinute;
            updateData.MaxLateCheckInMinute = param.MaxLateCheckInMinute;
            updateData.CheckOutByShift = param.CheckOutByShift;
            updateData.CheckOutTime = param.CheckOutTime != null ? (new DateTime(2000, 01, 01, param.CheckOutTime.Value.Hour, param.CheckOutTime.Value.Minute, 0)) : param.CheckOutTime;
            updateData.EndFirstHaftTime = param.EndFirstHaftTime != null ? (new DateTime(2000, 01, 01, param.EndFirstHaftTime.Value.Hour, param.EndFirstHaftTime.Value.Minute, 0)) : param.EndFirstHaftTime;
            updateData.BeginLastHaftTime = param.BeginLastHaftTime != null ? (new DateTime(2000, 01, 01, param.BeginLastHaftTime.Value.Hour, param.BeginLastHaftTime.Value.Minute, 0)) : param.BeginLastHaftTime;

            updateData.MaxEarlyCheckOutMinute = param.MaxEarlyCheckOutMinute;
            updateData.MaxLateCheckOutMinute = param.MaxLateCheckOutMinute;
            updateData.AllowFreeInAndOutInTimeRange = param.AllowFreeInAndOutInTimeRange;
            updateData.AllowEarlyOutLateInMission = param.AllowEarlyOutLateInMission;
            updateData.MissionMaxLateCheckInMinute = param.MissionMaxLateCheckInMinute;
            updateData.MissionMaxEarlyCheckOutMinute = param.MissionMaxEarlyCheckOutMinute;
            updateData.AdjustByLateInEarlyOut = param.AdjustByLateInEarlyOut;

            updateData.AllowInLeaveDay = param.AllowInLeaveDay;
            updateData.AllowInMission = param.AllowInMission;
            updateData.AllowInBreakTime = param.AllowInBreakTime;

            updateData.AllowCheckOutInWorkingTime = param.AllowCheckOutInWorkingTime;
            updateData.AllowCheckOutInWorkingTimeRange = param.AllowCheckOutInWorkingTimeRange;
            updateData.MaxMinuteAllowOutsideInWorkingTime = param.MaxMinuteAllowOutsideInWorkingTime;

            updateData.DenyInLeaveWholeDay = param.DenyInLeaveWholeDay;
            updateData.DenyInMissionWholeDay = param.DenyInMissionWholeDay;
            updateData.DenyInStoppedWorkingInfo = param.DenyInStoppedWorkingInfo;

            updateData.CheckLogByAreaGroup = param.CheckLogByAreaGroup;
            updateData.CheckLogByShift = param.CheckLogByShift;

            updateData.UpdatedDate = DateTime.Now;
            updateData.UpdatedUser = user.UserName;

            /*SaveRuleAreaGroup*/
            DeleteAndInsertRulesGeneralAreaGroup(param.AreaGroups, param.Index);
            /*SaveRuleGate*/
            DeleteAndInsertRulesGeneralGates(param.ListGatesInfo, param.Index);

            SaveChange();

            return ApiOk();
        }

        [Authorize]
        [ActionName("DeleteRuleGeneral")]
        [HttpDelete("{ruleIndex}")]
        public IActionResult DeleteRuleGeneral(int ruleIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }


            var data = _GC_Rules_GeneralAccessService.FirstOrDefault(x => x.Index == ruleIndex);
            if (data == null)
            {
                return ApiError("RuleNotExists");
            }
            // kiem tra su dung o chuc nang khac
            bool isUse = false;
            if (isUse == true)
            {
                return ApiError("RuleIsUseForOtherFunction");
            }

            _GC_Rules_GeneralAccessService.Delete(data);
            SaveChange();

            return ApiOk();
        }

        private void DeleteAndInsertRulesGeneralAreaGroup(List<GC_Rules_General_AreaGroup> areaGroups, int ruleIndex)
        {
            _GC_Rules_General_AreaGroupService.Delete(e => e.Rules_GeneralIndex == ruleIndex);
            foreach (var item in areaGroups)
            {
                item.UpdatedDate = DateTime.Now;
                _GC_Rules_General_AreaGroupService.Insert(item);
            }
        }
        private void DeleteAndInsertRulesGeneralGates(List<GC_Rules_GeneralAccess_Gates> ruleGates, int ruleIndex)
        {
            _GC_Rules_GeneralAccess_GatesService.Delete(e => e.RulesGeneralIndex == ruleIndex);
            //save gates list
            foreach (var item in ruleGates)
            {
                item.UpdatedDate = DateTime.Now;
                _GC_Rules_GeneralAccess_GatesService.Insert(item);
            }
        }
      
        public class Rules_GeneralAccessParam
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public string NameInEng { get; set; }
            /*  các qui định về thời gian vào ra cho phép  */
            public bool CheckInByShift { get; set; }
            public DateTime? CheckInTime { get; set; }
            public int MaxEarlyCheckInMinute { get; set; }
            public int MaxLateCheckInMinute { get; set; }
            public bool CheckOutByShift { get; set; }
            public DateTime? CheckOutTime { get; set; }
            public int MaxEarlyCheckOutMinute { get; set; }
            public int MaxLateCheckOutMinute { get; set; }
            public bool AllowFreeInAndOutInTimeRange { get; set; }
            public bool AllowEarlyOutLateInMission { get; set; }
            public int MissionMaxEarlyCheckOutMinute { get; set; }
            public int MissionMaxLateCheckInMinute { get; set; }
            public bool AdjustByLateInEarlyOut { get; set; }
            public DateTime? BeginLastHaftTime { get; set; }
            public DateTime? EndFirstHaftTime { get; set; }
            /*  các qui định liên quan đến ra giữa giờ có đăng ký  */
            public bool AllowInLeaveDay { get; set; }
            public bool AllowInMission { get; set; }
            public bool AllowInBreakTime { get; set; }
            /*  các qui định liên quan đến ra giữa giờ không đăng ký */
            public bool AllowCheckOutInWorkingTime { get; set; }
            public string AllowCheckOutInWorkingTimeRange { get; set; }
            public int MaxMinuteAllowOutsideInWorkingTime { get; set; }
            /*  các qui định cấm vào ra  */
            public bool DenyInLeaveWholeDay { get; set; }
            public bool DenyInMissionWholeDay { get; set; }
            public bool DenyInStoppedWorkingInfo { get; set; }

            public bool CheckLogByAreaGroup { get; set; }
            public bool CheckLogByShift { get; set; }
            public List<GC_Rules_General_AreaGroup> AreaGroups { get; set; }
            public List<GC_Rules_GeneralAccess_Gates> ListGatesInfo { get; set; }
        }
    }
}
