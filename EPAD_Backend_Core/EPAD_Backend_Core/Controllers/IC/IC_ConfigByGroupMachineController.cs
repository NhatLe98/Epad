using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Data.Entities;
using EPAD_Common.Utility;
using EPAD_Backend_Core.Base;
using Newtonsoft.Json;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/ConfigByGroupMachine/[action]")]
    [ApiController]
    public class IC_ConfigByGroupMachineController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private static IServiceScopeFactory _scopeFactory;
        private static IServiceScope scope;
        public IC_ConfigByGroupMachineController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _scopeFactory = TryResolve<IServiceScopeFactory>();
        }

        [ActionName("GetAllConfigByGroupMachine")]
        [HttpGet]
        public IActionResult GetAllConfigByGroupMachine(int groupDeviceIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            AllConfig responseData = new AllConfig();

            List<IC_ConfigByGroupMachine> listConfig = context.IC_ConfigByGroupMachine.Where(t => t.CompanyIndex == user.CompanyIndex && t.GroupDeviceIndex.Equals(groupDeviceIndex)).ToList();
            if (listConfig != null)
            {
                foreach (IC_ConfigByGroupMachine icConfig in listConfig)
                {
                    if (icConfig.EventType == ConfigAuto.INTEGRATE_LOG_REALTIME.ToString()) continue;
                    Config cfg = new Config()
                    {
                        EventType = icConfig.EventType,
                        SendMailWhenError = icConfig.SendMailWhenError,
                        AlwaysSend = icConfig.AlwaysSend,
                        TitleEmailSuccess = icConfig.TitleEmailSuccess ?? "",
                        BodyEmailSuccess = icConfig.BodyEmailSuccess ?? "",
                        TitleEmailError = icConfig.TitleEmailError ?? "",
                        BodyEmailError = icConfig.BodyEmailError ?? ""
                    };

                    if (icConfig.TimePos.Trim() != "")
                    {
                        cfg.TimePos = icConfig.TimePos.Split(';').ToList();
                    }
                    else
                    {
                        cfg.TimePos = new List<string>();
                    }

                    if (icConfig.Email != null && icConfig.Email.Trim() != "")
                    {
                        cfg.Email = icConfig.Email.Split(';').ToList();
                    }
                    else
                    {
                        cfg.Email = new List<string>();
                    }
                    if (icConfig.PreviousDays != null)
                    {
                        cfg.PreviousDays = icConfig.PreviousDays;
                    }
                    if (icConfig.EventType == ConfigAuto.DOWNLOAD_LOG.ToString())
                    {
                        cfg.DeleteLogAfterSuccess = bool.Parse(string.IsNullOrEmpty(icConfig.ProceedAfterEvent) ? "false" : icConfig.ProceedAfterEvent);
                    }

                    if (icConfig.EventType == ConfigAuto.DOWNLOAD_USER.ToString())
                    {
                        if (icConfig.CustomField != null && icConfig.CustomField != "")
                        {
                            var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                            cfg.IsOverwriteData = param.IsOverwriteData;

                            if (!string.IsNullOrWhiteSpace(param.ListSerialNumber) && param.ListSerialNumber.Trim() != "")
                            {
                                cfg.ListSerialNumber = param.ListSerialNumber.Split(';').ToList();
                            }
                            else
                            {
                                cfg.ListSerialNumber = new List<string>();
                            }
                        }
                    }

                    responseData.Data.Add(cfg.EventType, cfg);
                }
            }

            foreach (string eventType in Enum.GetNames(typeof(ConfigAutoGroupDevice)))
            {
                if (!responseData.Data.ContainsKey(eventType))
                {
                    responseData.Data.Add(eventType, new Config(eventType));
                }
            }

            result = Ok(responseData);
            return result;
        }

        [Authorize]
        [ActionName("UpdateGroupDeviceConfig")]
        [HttpPost]
        public IActionResult UpdateGroupDeviceConfig([FromBody]AllGroupDeviceConfig allConfig)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            string[] enumCheck = Enum.GetNames(typeof(ConfigAuto));

            IQueryable<IC_ConfigByGroupMachine> lstConfig = context.IC_ConfigByGroupMachine.Where(t => t.CompanyIndex == user.CompanyIndex && t.GroupDeviceIndex.Equals(allConfig.GroupIndex));
            foreach (KeyValuePair<string, Config> item in allConfig.Data)
            {
                if (!enumCheck.Contains(item.Key)) continue;
                IC_ConfigByGroupMachine icCfg = lstConfig.FirstOrDefault(x => x.EventType == item.Key);
                bool isEdit = true;
                if (icCfg == null)
                {
                    icCfg = new IC_ConfigByGroupMachine();
                    icCfg.CompanyIndex = user.CompanyIndex;
                    isEdit = false;
                }
                icCfg.CompanyIndex = user.CompanyIndex;
                icCfg.GroupDeviceIndex = allConfig.GroupIndex;
                icCfg.TimePos = string.Join(";", item.Value.TimePos);
                icCfg.EventType = item.Key;
                if (item.Value.Email != null)
                    icCfg.Email = string.Join(";", item.Value.Email);
                icCfg.SendMailWhenError = item.Value.SendMailWhenError;
                icCfg.AlwaysSend = item.Value.AlwaysSend;
                icCfg.UpdatedDate = DateTime.Now;
                icCfg.UpdatedUser = user.UserName;
                icCfg.TitleEmailSuccess = item.Value.TitleEmailSuccess;
                icCfg.BodyEmailSuccess = item.Value.BodyEmailSuccess;
                icCfg.TitleEmailError = item.Value.TitleEmailError;
                icCfg.BodyEmailError = item.Value.BodyEmailError;

                if (item.Value.PreviousDays != null)
                {
                    icCfg.PreviousDays = item.Value.PreviousDays;
                }

                if (item.Key == ConfigAuto.DOWNLOAD_LOG.ToString())
                {
                    icCfg.ProceedAfterEvent = item.Value.DeleteLogAfterSuccess.ToString();
                }

                if (item.Key == ConfigAuto.DOWNLOAD_USER.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    param.IsOverwriteData = item.Value.IsOverwriteData.HasValue ? item.Value.IsOverwriteData.HasValue : false;
                    if (item.Value.ListSerialNumber != null)
                    {
                        param.ListSerialNumber = string.Join(";", item.Value.ListSerialNumber);
                    }
                    else
                    {
                        param.ListSerialNumber = "";
                    }
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;
                }

                if (!isEdit)
                {
                    context.IC_ConfigByGroupMachine.Add(icCfg);
                }
            }
            context.SaveChanges();
            CompanyInfo companyInfo = CompanyInfo.GetFromCache(cache, user.CompanyIndex.ToString());
            if (companyInfo != null)
            {
                companyInfo.SetConfigGroupDevice(lstConfig.ToList());
            }
            // Hoang to check
            // ScheduleAutoConfig.db = new EPAD_Context();
            //scope = _scopeFactory.CreateScope();
            //ScheduleAutoHostedService.db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            result = Ok();
            return result;
        }

        public class AllGroupDeviceConfig
        {
            public Dictionary<string, Config> Data;
            public int GroupIndex;
            public AllGroupDeviceConfig()
            {
                Data = new Dictionary<string, Config>();
            }
        }
    }
}