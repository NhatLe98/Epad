using EPAD_Backend_Core.Base;
using EPAD_Background.Schedule.Job;
using EPAD_Common.FileProvider;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.Other;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GC_Rules_WarningController : ApiControllerBase
    {
        private IMemoryCache cache;
        private readonly IGC_Rules_WarningService _GC_Rules_WarningService;
        
        private readonly CheckWarningViolation _CheckWarningViolation;
        public GC_Rules_WarningController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _GC_Rules_WarningService = TryResolve<IGC_Rules_WarningService>();
            _CheckWarningViolation = TryResolve<CheckWarningViolation>();
        }

        [Authorize]
        [ActionName("GetRulesWarningGroup")]
        [HttpGet]
        public async Task<IActionResult> GetRulesWarningGroup()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var dummy = await _GC_Rules_WarningService.GetRulesWarningGroupsByCompanyIndex(user.CompanyIndex);
            return ApiOk(dummy);
        }

        [Authorize]
        [ActionName("GetRulesWarningByCompanyIndex")]
        [HttpGet]
        public async Task<IActionResult> GetRulesWarningByCompanyIndex()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var dummy = await _GC_Rules_WarningService.GetRulesWarningByCompanyIndex(user.CompanyIndex);
            return ApiOk(dummy);
        }

        [Authorize]
        [ActionName("GetEmailScheduleByRuleWarningIndex")]
        [HttpGet]
        public async Task<IActionResult> GetEmailScheduleByRuleWarningIndex(int rulesWarningIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var dummy = await _GC_Rules_WarningService.GetRulesWarningEmailSchedule(rulesWarningIndex, user.CompanyIndex);
            return ApiOk(dummy);
        }

        [Authorize]
        [ActionName("GetControllerChannelByRuleWarningIndex")]
        [HttpGet]
        public async Task<IActionResult> GetControllerChannelByRuleWarningIndex(int rulesWarningIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var list = await _GC_Rules_WarningService.GetRulesWarningControllerChannel(rulesWarningIndex, user.CompanyIndex);

            //var group = list.GroupBy(e => new { e.Type, e.ControllerIndex, UseLine = e.LineIndex != null});

            //var dummy = new List<CotrollerWarningRequestModel>();
            //foreach (var item in group)
            //{
            //    if (item.Key.UseLine)
            //    {
            //        var channelIndexs = item.Select(e => e.ChannelIndex);
            //        channelIndexs = channelIndexs.Distinct();
            //        if (channelIndexs != null && channelIndexs.Any())
            //        {
            //            foreach (var channel in channelIndexs)
            //            {
            //                dummy.Add(new CotrollerWarningRequestModel()
            //                {
            //                    ControllerIndex = item.Key.ControllerIndex,
            //                    Index = 0,
            //                    RulesWarningIndex = rulesWarningIndex,
            //                    Type = item.Key.Type,
            //                    LineIndexs = item.Where(e => e.ChannelIndex == channel).Select(e => e.LineIndex).ToList(),
            //                    ChannelIndexs = new List<int>() { channel }
            //                });
            //            }

            //        }
            //    }
            //    else
            //    {
            //        dummy.Add(new CotrollerWarningRequestModel()
            //        {
            //            ControllerIndex = item.Key.ControllerIndex,
            //            Index = 0,
            //            RulesWarningIndex = rulesWarningIndex,
            //            Type = item.Key.Type,
            //            LineIndexs = new List<int?>(),
            //            ChannelIndexs = item.Select(e => e.ChannelIndex).ToList()
            //        });
            //    }


            //}
            return ApiOk(list);
        }

        [Authorize]
        [ActionName("AddRulesWarning")]
        [HttpPost]
        public async Task<IActionResult> AddRulesWarning([FromBody] GC_Rules_Warning data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var oldData = _GC_Rules_WarningService.GetRulesWarningByGroupIndex(data.RulesWarningGroupIndex, user.CompanyIndex);
            if (oldData != null)
            {
                return ApiConflict("RulesWarningExist");
            }

            var isSuccess = await _GC_Rules_WarningService.AddRulesWarning(data, user);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateRulesWarning")]
        [HttpPut]
        public async Task<IActionResult> UpdateRulesWarning([FromBody] GC_Rules_Warning data)
        {
            UserInfo user = GetUserInfo();
            if (data.UseLed == false)
            {
                data.UseLedFocus = null;
                data.UseLedInPlace = null;
            }
            if (data.UseSpeaker == false)
            {
                data.UseSpeakerFocus = null;
                data.UseSpeakerInPlace = null;
            }
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var oldData = await _GC_Rules_WarningService.GetDataByIndex(data.Index);
            if (oldData == null)
            {
                return ApiConflict("RulesWarningNotExist");
            }

            var isSuccess = await _GC_Rules_WarningService.UpdateRulesWarning(data, user);

            return ApiOk(isSuccess);
        }
        [Authorize]
        [ActionName("DeleteRulesWarning")]
        [HttpDelete]
        public async Task<IActionResult> DeleteRulesWarning(int index)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var oldData = await _GC_Rules_WarningService.GetDataByIndex(index);
            if (oldData == null)
            {
                return ApiConflict("RulesWarningNotExist");
            }

            var isSuccess = await _GC_Rules_WarningService.DeleteRulesWarning(index, user);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("AddRulesWarningEmailSchedule")]
        [HttpPost]
        public async Task<IActionResult> AddRulesWarningEmailSchedule(List<EmailScheduleRequestModel> data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var isSuccess = await _GC_Rules_WarningService.AddRulesWarningEmailSchedule(data, user);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("AddRulesWarningControllerChannels")]
        [HttpPost]
        public async Task<IActionResult> AddRulesWarningControllerChannels(List<GC_Rules_Warning_ControllerChannel> data)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var isSuccess = await _GC_Rules_WarningService.AddRulesWarningControllerChannels(data, user);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("AddEzFileRulesWarning")]
        [HttpPost]
        public async Task<IActionResult> AddEzFileRulesWarning(EzFileRequestSimple data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var oldData = await _GC_Rules_WarningService.GetDataByIndex(data.Index);
            if (oldData == null)
            {
                return ApiConflict("RulesWarningNotExist");
            }

            if (data.Attachments != null)
            {
                var isSuccess = await _GC_Rules_WarningService.AddEzFileRulesWarning(data, user);
                return ApiOk(isSuccess);
            }
            return ApiConflict("CannotSaveFile");
        }

        [ActionName("CheckAndSendMail")]
        [HttpPost]
        public async Task<IActionResult> CheckAndSendMail(SendEmailByTimeLog data)
        {
            ////Check tiếp nếu là Monitoring_API gọi
            //string token = GetApiTokenFromHeader();

            //if (token != _EPADConfig.ApiKey)
            //{
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            //}

            await _CheckWarningViolation.CreateAndSendMail(data.Email, data.TimeLog);
            return ApiOk();
        }

        [ActionName("SendReloadWarningRulesSignal")]
        [HttpGet]
        public async Task<IActionResult> SendReloadWarningRulesSignal()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            await _GC_Rules_WarningService.SendReloadWarningRuleToClientAsync(user.CompanyIndex, "reload");
            return ApiOk();
        }
    }
}
