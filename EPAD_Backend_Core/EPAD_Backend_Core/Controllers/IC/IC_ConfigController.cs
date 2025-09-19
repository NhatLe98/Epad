
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.MainProcess;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using EPAD_Common.Extensions;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Threading.Tasks;
using ServiceStack.Web;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Config/[action]")]
    [ApiController]
    public class IC_ConfigController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private static IServiceScopeFactory _scopeFactory;
        private static IServiceScope scope;
        private IIC_AuditLogic _iIC_AuditLogic;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;
        public IC_ConfigController(IServiceProvider provider, IConfiguration configuration) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _scopeFactory = TryResolve<IServiceScopeFactory>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _configuration = configuration;
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
        }

        [Authorize]
        [ActionName("IsUsingECMS")]
        [HttpGet]
        public IActionResult IsUsingECMS()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            bool isUsingECMS = Convert.ToBoolean(_configuration.GetValue<string>("IsUsingECMS"));
            return ApiOk(isUsingECMS);
        }

        [ActionName("GetAllConfig")]
        [HttpGet]
        public IActionResult GetAllConfig()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            var responseData = new AllConfig();
            var listConfig = context.IC_Config.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            foreach (var icConfig in listConfig)
            {
                if (icConfig.EventType == ConfigAuto.INTEGRATE_LOG_REALTIME.ToString()) continue;
                var cfg = new Config()
                {
                    EventType = icConfig.EventType,
                    SendMailWhenError = icConfig.SendMailWhenError,
                    AlwaysSend = icConfig.AlwaysSend,
                    TitleEmailSuccess = icConfig.TitleEmailSuccess ?? "",
                    BodyEmailSuccess = icConfig.BodyEmailSuccess ?? "",
                    TitleEmailError = icConfig.TitleEmailError ?? "",
                    BodyEmailError = icConfig.BodyEmailError ?? "",
                    BodyTemperature = icConfig.BodyTemperature ?? null
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
                if (icConfig.EventType == ConfigAuto.INTEGRATE_LOG.ToString())
                {
                    cfg.WriteToDatabase = false;
                    cfg.WriteToFile = false;
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        cfg.WriteToDatabase = param.WriteToDatabase;
                        cfg.WriteToFile = param.WriteToFile;
                        cfg.LinkAPI = param.LinkAPI;
                        cfg.WriteToFilePath = param.WriteToFilePath;
                        cfg.SoftwareType = param.SoftwareType;
                        cfg.UserName = param.UserName;
                        cfg.Password = param.Password;
                        cfg.FileType = param.FileType;
                    }
                }
                if (icConfig.EventType == ConfigAuto.EMPLOYEE_INTEGRATE.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        cfg.LinkAPI = param.LinkAPI == null ? "" : param.LinkAPI;
                        cfg.UsingDatabase = param.UsingDatabase;
                    }
                }
                if (icConfig.EventType == ConfigAuto.EMPLOYEE_SHIFT_INTEGRATE.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegratedShiftConfiguration>(icConfig.CustomField);
                        cfg.LinkAPI = param.LinkAPI == null ? "" : param.LinkAPI;
                        cfg.UsingDatabase = param.UsingDatabase;
                        cfg.FromDate = param.FromDate;
                        cfg.ToDate = param.ToDate;
                    }
                }
                if (icConfig.EventType == ConfigAuto.ADD_OR_DELETE_USER.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        cfg.AutoIntegrate = param.AutoIntegrate;
                        cfg.IntegrateWhenNotInclareDepartment = param.IntegrateWhenNotInclareDepartment;
                    }
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

                if (icConfig.EventType == ConfigAuto.AUTO_DELETE_BLACKLIST.ToString())
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


                if (icConfig.EventType == ConfigAuto.ADD_OR_DELETE_USER.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        cfg.AutoIntegrate = param.AutoIntegrate;

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
                if (icConfig.EventType == ConfigAuto.GENERAL_SYSTEM_CONFIG.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                    }
                }
                if (icConfig.EventType == ConfigAuto.DELETE_SYSTEM_COMMAND.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        cfg.SendEmailWithFile = param.SendEmailWithFile;
                        cfg.AfterHours = param.AfterHours;
                    }
                }
                if (icConfig.EventType == ConfigAuto.DOWNLOAD_LOG.ToString())
                {
                    cfg.DeleteLogAfterSuccess = bool.Parse(string.IsNullOrEmpty(icConfig.ProceedAfterEvent) ? "false" : icConfig.ProceedAfterEvent);
                }
                if (icConfig.EventType == ConfigAuto.ECMS_DEFAULT_MEAL_CARD_DEPARTMENT.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        cfg.DepartmentIndex = param.DepartmentIndex;
                    }
                }
                if (icConfig.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        cfg.RemoveStoppedWorkingEmployeesType = param.RemoveStoppedWorkingEmployeesType;
                        cfg.RemoveStoppedWorkingEmployeesDay = param.RemoveStoppedWorkingEmployeesDay;
                        cfg.RemoveStoppedWorkingEmployeesWeek = param.RemoveStoppedWorkingEmployeesWeek;
                        cfg.RemoveStoppedWorkingEmployeesMonth = param.RemoveStoppedWorkingEmployeesMonth;
                        cfg.RemoveStoppedWorkingEmployeesTime = param.RemoveStoppedWorkingEmployeesTime;
                        cfg.ShowStoppedWorkingEmployeesType = param.ShowStoppedWorkingEmployeesType;
                        cfg.ShowStoppedWorkingEmployeesDay = param.ShowStoppedWorkingEmployeesDay;
                        cfg.ShowStoppedWorkingEmployeesWeek = param.ShowStoppedWorkingEmployeesWeek;
                        cfg.ShowStoppedWorkingEmployeesMonth = param.ShowStoppedWorkingEmployeesMonth;
                        cfg.ShowStoppedWorkingEmployeesTime = param.ShowStoppedWorkingEmployeesTime;
                    }
                }
                if (icConfig.EventType == ConfigAuto.EMPLOYEE_INTEGRATE_TO_DATABASE.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        cfg.SoftwareType = param.SoftwareType;
                        cfg.LinkAPIIntegrate = param.LinkAPIIntegrate;
                        cfg.Token = param.Token;
                        cfg.UserName = param.UserName;
                        cfg.Password = param.Password;
                    }
                }
                if (icConfig.EventType == ConfigAuto.LOG_INTEGRATE_TO_DATABASE.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        cfg.SoftwareType = param.SoftwareType;
                        cfg.LinkAPIIntegrate = param.LinkAPIIntegrate;
                        cfg.Token = param.Token;
                        cfg.PreviousDays = icConfig.PreviousDays;
                    }
                }
                if (icConfig.EventType == ConfigAuto.RE_PROCESSING_REGISTERCARD.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        cfg.LinkAPIIntegrate = param.LinkAPIIntegrate;
                        cfg.PreviousDays = icConfig.PreviousDays;
                    }
                }

                if (icConfig.EventType == ConfigAuto.DOWNLOAD_PARKING_LOG.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        cfg.LinkAPIIntegrate = param.LinkAPIIntegrate;
                        cfg.PreviousDays = icConfig.PreviousDays;
                    }
                }

                if (icConfig.EventType == ConfigAuto.CREATE_DEPARTMENT_IMPORT_EMPLOYEE.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        cfg.AutoCreateDepartmentImportEmployee = param.AutoCreateDepartmentImportEmployee;
                    }
                }

                if (icConfig.EventType == ConfigAuto.CONFIG_EMAIL_ALLOW_IMPORT_GGSHEET.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        if (!string.IsNullOrWhiteSpace(param.EmailAllowImportGoogleSheet) && param.EmailAllowImportGoogleSheet.Trim() != "")
                        {
                            cfg.EmailAllowImportGoogleSheet = param.EmailAllowImportGoogleSheet.Split(';').ToList();
                        }
                        else
                        {
                            cfg.EmailAllowImportGoogleSheet = new List<string>();
                        }
                    }
                }

                if (icConfig.EventType == ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString())
                {
                    if (icConfig.CustomField != null && icConfig.CustomField != "")
                    {
                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                        cfg.LinkAPIIntegrate = param.LinkAPIIntegrate;
                        cfg.PreviousDays = icConfig.PreviousDays;
                    }
                }
                if (responseData.Data.ContainsKey(cfg.EventType))
                {
                    continue;
                }
                responseData.Data.Add(cfg.EventType, cfg);
            }

            foreach (string eventType in Enum.GetNames(typeof(ConfigAuto)))
            {
                if (!responseData.Data.ContainsKey(eventType) && eventType != ConfigAuto.INTEGRATE_LOG_REALTIME.ToString()
                    && eventType != ConfigAuto.INTEGRATE_LOG.ToString())
                {
                    var config = new Config(eventType);
                    if (config.FromDate == DateTime.MinValue)
                    {
                        config.FromDate = DateTime.Now;
                    }
                    if (config.ToDate == DateTime.MinValue)
                    {
                        config.ToDate = DateTime.Now;
                    }
                    responseData.Data.Add(eventType, config);
                }
            }

            result = Ok(responseData);
            return result;
        }

        [ActionName("GetIntegrateLogRealTimeConfig")]
        [HttpGet]
        public IActionResult GetIntegrateLogRealTimeConfig()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            IntegrateLogRealtimeConfig cfgLogRealtime = null;

            IC_Config cfg = context.IC_Config.FirstOrDefault(x => x.EventType == ConfigAuto.INTEGRATE_LOG_REALTIME.ToString());
            if (cfg == null)
            {
                cfgLogRealtime = new IntegrateLogRealtimeConfig()
                {
                    EventType = ConfigAuto.INTEGRATE_LOG_REALTIME.ToString(),
                    IntegrateLogRealtime = false,
                    LinkAPI = ""
                };
            }
            else
            {
                string[] cfgSplit = cfg.ProceedAfterEvent.Split('|');
                cfgLogRealtime = new IntegrateLogRealtimeConfig()
                {
                    EventType = ConfigAuto.INTEGRATE_LOG_REALTIME.ToString(),
                    IntegrateLogRealtime = cfgSplit.Length > 0 ? bool.Parse(cfgSplit[0]) : false,
                    LinkAPI = cfgSplit.Length > 1 ? cfgSplit[1] : ""
                };
            }
            result = Ok(cfgLogRealtime);
            return result;
        }

        [Authorize]
        [ActionName("UpdateConfig")]
        [HttpPost]
        public IActionResult UpdateConfig([FromBody] AllConfig allConfig)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            string[] enumCheck = Enum.GetNames(typeof(ConfigAuto));

            IQueryable<IC_Config> lstConfig = context.IC_Config.Where(t => t.CompanyIndex == user.CompanyIndex);
            foreach (KeyValuePair<string, Config> item in allConfig.Data)
            {
                if (!enumCheck.Contains(item.Key)) continue;
                IC_Config icCfg = lstConfig.FirstOrDefault(x => x.EventType == item.Key);
                bool isEdit = true;
                if (icCfg == null)
                {
                    icCfg = new IC_Config();
                    icCfg.CompanyIndex = user.CompanyIndex;
                    isEdit = false;
                }

                icCfg.TimePos = string.Join(";", item.Value.TimePos);
                icCfg.EventType = item.Key;
                if (item.Value.Email != null)
                {
                    icCfg.Email = string.Join(";", item.Value.Email);
                }

                icCfg.SendMailWhenError = item.Value.SendMailWhenError;
                icCfg.AlwaysSend = item.Value.AlwaysSend;
                icCfg.UpdatedDate = DateTime.Now;
                icCfg.UpdatedUser = user.UserName;
                icCfg.TitleEmailSuccess = item.Value.TitleEmailSuccess;
                icCfg.BodyEmailSuccess = item.Value.BodyEmailSuccess;
                icCfg.TitleEmailError = item.Value.TitleEmailError;
                icCfg.BodyEmailError = item.Value.BodyEmailError;
                icCfg.BodyTemperature = item.Value.BodyTemperature;

                if (item.Value.PreviousDays != null)
                {
                    icCfg.PreviousDays = item.Value.PreviousDays;
                }
                if (item.Key == ConfigAuto.INTEGRATE_LOG.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    param.WriteToDatabase = item.Value.WriteToDatabase.HasValue ? item.Value.WriteToDatabase.Value : false;
                    param.LinkAPI = item.Value.LinkAPI;
                    param.WriteToFilePath = item.Value.WriteToFilePath;
                    param.WriteToFile = item.Value.WriteToFile.HasValue ? item.Value.WriteToFile.Value : false;
                    param.UsingDatabase = item.Value.UsingDatabase.HasValue ? item.Value.UsingDatabase.Value : false;
                    param.SendEmailWithFile = item.Value.SendEmailWithFile.HasValue ? item.Value.SendEmailWithFile.Value : false;
                    param.SoftwareType = item.Value.SoftwareType;
                    param.UserName = item.Value.UserName;
                    param.Password = item.Value.Password;
                    param.FileType = item.Value.FileType;
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;

                }
                if (item.Key == ConfigAuto.DOWNLOAD_LOG.ToString())
                {
                    icCfg.ProceedAfterEvent = item.Value.DeleteLogAfterSuccess.ToString();
                }
                if (item.Key == ConfigAuto.EMPLOYEE_INTEGRATE.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    param.WriteToDatabase = item.Value.WriteToDatabase.HasValue ? item.Value.WriteToDatabase.Value : false;
                    param.WriteToFilePath = item.Value.WriteToFilePath;
                    param.AutoIntegrate = item.Value.AutoIntegrate.HasValue ? item.Value.AutoIntegrate.Value : false;
                    param.UsingDatabase = item.Value.UsingDatabase.HasValue ? item.Value.UsingDatabase.Value : false;
                    param.LinkAPI = item.Value.UsingDatabase == false ? (item.Value.LinkAPI != null && item.Value.LinkAPI.Length > 0 ? item.Value.LinkAPI : null) : null;
                    param.SendEmailWithFile = item.Value.SendEmailWithFile.HasValue ? item.Value.SendEmailWithFile.Value : false;
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;
                }
                if (item.Key == ConfigAuto.EMPLOYEE_SHIFT_INTEGRATE.ToString())
                {
                    var param = new IntegratedShiftConfiguration();
                    param.WriteToDatabase = item.Value.WriteToDatabase.HasValue ? item.Value.WriteToDatabase.Value : false;
                    param.LinkAPI = item.Value.LinkAPI;
                    param.WriteToFilePath = item.Value.WriteToFilePath;
                    param.AutoIntegrate = item.Value.AutoIntegrate.HasValue ? item.Value.AutoIntegrate.Value : false;
                    param.UsingDatabase = item.Value.UsingDatabase.HasValue ? item.Value.UsingDatabase.Value : false;
                    param.SendEmailWithFile = item.Value.SendEmailWithFile.HasValue ? item.Value.SendEmailWithFile.Value : false;
                    param.FromDate = item.Value.FromDate;
                    param.ToDate = item.Value.ToDate;
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;
                }

                if (item.Key == ConfigAuto.ADD_OR_DELETE_USER.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    param.AutoIntegrate = item.Value.AutoIntegrate.HasValue ? item.Value.AutoIntegrate.Value : false;
                    param.SendEmailWithFile = item.Value.SendEmailWithFile.HasValue ? item.Value.SendEmailWithFile.Value : false;
                    param.IntegrateWhenNotInclareDepartment = item.Value.IntegrateWhenNotInclareDepartment.HasValue ? item.Value.IntegrateWhenNotInclareDepartment.Value : false;
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
                if (item.Key == ConfigAuto.AUTO_DELETE_BLACKLIST.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();

                    param.SendEmailWithFile = item.Value.SendEmailWithFile.HasValue ? item.Value.SendEmailWithFile.Value : false;
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
                if (item.Key == ConfigAuto.GENERAL_SYSTEM_CONFIG.ToString())
                {

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
                if (item.Key == ConfigAuto.DELETE_SYSTEM_COMMAND.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    param.LinkAPI = item.Value.LinkAPI;
                    param.SendEmailWithFile = item.Value.SendEmailWithFile.HasValue ? item.Value.SendEmailWithFile.Value : false;
                    param.AfterHours = item.Value.AfterHours.HasValue ? item.Value.AfterHours.Value : 0;
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;
                }
                if (item.Key == ConfigAuto.ECMS_DEFAULT_MEAL_CARD_DEPARTMENT.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    param.DepartmentIndex = item.Value.DepartmentIndex;
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;
                }
                if (item.Key == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    param.RemoveStoppedWorkingEmployeesType = item.Value.RemoveStoppedWorkingEmployeesType;
                    param.RemoveStoppedWorkingEmployeesDay = item.Value.RemoveStoppedWorkingEmployeesDay;
                    param.RemoveStoppedWorkingEmployeesWeek = item.Value.RemoveStoppedWorkingEmployeesWeek;
                    param.RemoveStoppedWorkingEmployeesMonth = item.Value.RemoveStoppedWorkingEmployeesMonth;
                    param.RemoveStoppedWorkingEmployeesTime = item.Value.RemoveStoppedWorkingEmployeesTime;
                    param.ShowStoppedWorkingEmployeesType = item.Value.ShowStoppedWorkingEmployeesType;
                    param.ShowStoppedWorkingEmployeesDay = item.Value.ShowStoppedWorkingEmployeesDay;
                    param.ShowStoppedWorkingEmployeesWeek = item.Value.ShowStoppedWorkingEmployeesWeek;
                    param.ShowStoppedWorkingEmployeesMonth = item.Value.ShowStoppedWorkingEmployeesMonth;
                    param.ShowStoppedWorkingEmployeesTime = item.Value.ShowStoppedWorkingEmployeesTime;
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;
                }
                if (item.Key == ConfigAuto.EMPLOYEE_INTEGRATE_TO_DATABASE.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    param.SoftwareType = item.Value.SoftwareType;
                    param.SendEmailWithFile = item.Value.SendEmailWithFile.HasValue ? item.Value.SendEmailWithFile.Value : false;
                    param.LinkAPIIntegrate = item.Value.LinkAPIIntegrate;
                    param.UserName = item.Value.UserName;
                    param.Password = item.Value.Password;
                    param.Token = item.Value.Token;
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;
                }
                if (item.Key == ConfigAuto.LOG_INTEGRATE_TO_DATABASE.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    param.SoftwareType = item.Value.SoftwareType;
                    param.SendEmailWithFile = item.Value.SendEmailWithFile.HasValue ? item.Value.SendEmailWithFile.Value : false;
                    param.LinkAPIIntegrate = item.Value.LinkAPIIntegrate;
                    param.Token = item.Value.Token;
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;
                    if (item.Value.PreviousDays != null)
                    {
                        icCfg.PreviousDays = item.Value.PreviousDays;
                    }
                }

                if (item.Key == ConfigAuto.RE_PROCESSING_REGISTERCARD.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    param.LinkAPIIntegrate = item.Value.LinkAPIIntegrate;
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;
                    if (item.Value.PreviousDays != null)
                    {
                        icCfg.PreviousDays = item.Value.PreviousDays;
                    }
                }

                if (item.Key == ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    param.LinkAPIIntegrate = item.Value.LinkAPIIntegrate;
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;
                    if (item.Value.PreviousDays != null)
                    {
                        icCfg.PreviousDays = item.Value.PreviousDays;
                    }
                }

                if (item.Key == ConfigAuto.DOWNLOAD_PARKING_LOG.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    param.LinkAPIIntegrate = item.Value.LinkAPIIntegrate;
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;
                    if (item.Value.PreviousDays != null)
                    {
                        icCfg.PreviousDays = item.Value.PreviousDays;
                    }
                }

                //AutoCreateDepartmentImportEmployee
                if (item.Key == ConfigAuto.CREATE_DEPARTMENT_IMPORT_EMPLOYEE.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    param.AutoCreateDepartmentImportEmployee = item.Value.AutoCreateDepartmentImportEmployee;
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;
                }

                if (item.Key == ConfigAuto.CONFIG_EMAIL_ALLOW_IMPORT_GGSHEET.ToString())
                {
                    IntegrateLogParam param = new IntegrateLogParam();
                    if (item.Value.EmailAllowImportGoogleSheet != null)
                    {
                        param.EmailAllowImportGoogleSheet = string.Join(";", item.Value.EmailAllowImportGoogleSheet);
                    }
                    else
                    {
                        param.EmailAllowImportGoogleSheet = "";
                    }
                    string strParam = JsonConvert.SerializeObject(param);
                    icCfg.CustomField = strParam;
                }

                if (!isEdit)
                {
                    context.IC_Config.Add(icCfg);
                }
            }
            context.SaveChanges();
            // Add audit log
            IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
            audit.TableName = "IC_Config";
            audit.UserName = user.UserName;
            audit.CompanyIndex = user.CompanyIndex;
            audit.State = AuditType.Modified;
            audit.Description = AuditType.Modified.ToString() + "Config";
            audit.DateTime = DateTime.Now;
            _iIC_AuditLogic.Create(audit);
            CompanyInfo companyInfo = CompanyInfo.GetFromCache(cache, user.CompanyIndex.ToString());
            if (companyInfo != null)
            {
                companyInfo.SetConfig(lstConfig.ToList());
            }

            // ScheduleAutoConfig.db = new EPAD_Context();
            //scope = _scopeFactory.CreateScope();
            //ScheduleAutoHostedService.db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            result = Ok();
            return result;
        }

        [ActionName("UpdateIntegrateLogRealTimeConfig")]
        [HttpPost]
        public IActionResult UpdateIntegrateLogRealTimeConfig([FromBody] IntegrateLogRealtimeConfig cfgLogRealTime)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            IC_Config icCfg = context.IC_Config.FirstOrDefault(x => x.EventType == ConfigAuto.INTEGRATE_LOG_REALTIME.ToString());
            bool isEdit = true;
            if (icCfg == null)
            {
                icCfg = new IC_Config();
                icCfg.CompanyIndex = user.CompanyIndex;
                isEdit = false;
            }

            icCfg.TimePos = "";
            icCfg.EventType = ConfigAuto.INTEGRATE_LOG_REALTIME.ToString();
            icCfg.SendMailWhenError = false;
            icCfg.AlwaysSend = false;
            icCfg.UpdatedDate = DateTime.Now;
            icCfg.UpdatedUser = user.UserName;
            icCfg.TitleEmailSuccess = "";
            icCfg.BodyEmailSuccess = "";
            icCfg.TitleEmailError = "";
            icCfg.BodyEmailError = "";
            icCfg.BodyTemperature = null;

            icCfg.ProceedAfterEvent = cfgLogRealTime.IntegrateLogRealtime.ToString() + "|" + cfgLogRealTime.LinkAPI;
            if (!isEdit)
            {
                context.IC_Config.Add(icCfg);
            }
            context.SaveChanges();
            // Add audit log
            IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
            audit.TableName = "IC_Config";
            audit.UserName = user.UserName;
            audit.CompanyIndex = user.CompanyIndex;
            audit.State = AuditType.Modified;
            audit.Description = AuditType.Modified.ToString() + "Config";
            audit.DateTime = DateTime.Now;
            _iIC_AuditLogic.Create(audit);
            result = Ok();
            return result;
        }

        [ActionName("ChangeUpdateUI")]
        [HttpGet]
        public async Task<IActionResult> ChangeUpdateUI(string uiName)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            string sWebRootFolder = _hostingEnvironment.ContentRootPath;
            var folderDetails = Path.Combine(sWebRootFolder, @"epad/dist/static/variables/config.js");
            //Use local 
#if DEBUG
            folderDetails = Path.Combine(sWebRootFolder, @"epad/src/static/variables/config.js");
#endif
            try
            {
                var fileContent = await System.IO.File.ReadAllLinesAsync(folderDetails);

                // Identify and modify the line containing window.__env.updateUI
                for (int i = 0; i < fileContent.Length; i++)
                {
                    if (fileContent[i].Contains("window.__env.uiName"))
                    {
                        fileContent[i] = $"    window.__env.uiName = '" + uiName + "';";
                    }
                    if (fileContent[i].Contains("window.__env.updateUI"))
                    {
                        if (uiName == "Default")
                        {
                            fileContent[i] = $"    window.__env.updateUI = false;";
                        }
                        else
                        {
                            fileContent[i] = $"    window.__env.updateUI = true;";
                        }
                    }
                }

                // Write the updated content back to the file
                await System.IO.File.WriteAllLinesAsync(folderDetails, fileContent);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return ApiError(ex.ToString());
            }

            return ApiOk();
        }

        [ActionName("GetEmployeeIntegrateConfig")]
        [HttpGet]
        public IActionResult GetEmployeeIntegrateConfig()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            result = NotFound();
            if (user.Index > 0)
            {

                CompanyInfo companyInfo = CompanyInfo.GetFromCache(cache, user.CompanyIndex.ToString());
                if (companyInfo != null)
                {
                    List<IC_Config> listConfig = companyInfo.GetListConfig();
                    bool come = false;
                    IC_Config integrateConfig = listConfig.Where(t => t.EventType == ConfigAuto.EMPLOYEE_INTEGRATE.ToString()
                        && t.TimePos != "").FirstOrDefault();
                    if (integrateConfig != null)
                    {
                        string timeCompare = DateTime.Now.ToHHmm();
                        string[] arrTimePos = integrateConfig.TimePos.Split(';');
                        for (int i = 0; i < arrTimePos.Length; i++)
                        {
                            if (timeCompare == arrTimePos[i])
                            {
                                come = true;
                                break;
                            }
                        }
                    }
                    if (come == true)
                    {
                        result = Ok();
                    }
                }
                else
                {
                    result = NotFound("NoConfig");
                }
            }

            return result;
        }

        [ActionName("GetRealTimeServerLink")]
        [HttpGet]
        public IActionResult GetRealTimeServerLink()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            ConfigObject config = ConfigObject.GetConfig(cache);
            string url = config.RealTimeServerLink;
            if (config.RealTimeServerLink.EndsWith("/"))
            {
                url = config.RealTimeServerLink.Substring(0, config.RealTimeServerLink.Length - 1);
            }

            result = Ok(url);
            return result;
        }

        [ActionName("GetServerLink")]
        [HttpGet]
        public IActionResult GetServerLink()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            ConfigObject config = ConfigObject.GetConfig(cache);
            string url = config.ServerLink;
            if (config.ServerLink.EndsWith("/"))
            {
                url = config.ServerLink.Substring(0, config.ServerLink.Length - 1);
            }

            result = Ok(url);
            return result;
        }

        [ActionName("GetWisenetWaveServerConfig")]
        [HttpGet]
        public IActionResult GetWisenetWaveServerConfig()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            ConfigObject config = ConfigObject.GetConfig(cache);
            string localAddress = config.WisenetWaveServerLocalAddress;
            string cloudAddress = config.WisenetWaveServerCloudAddress;
            string username = config.WisenetWaveServerUsername;
            string password = config.WisenetWaveServerPassword;
            if (!string.IsNullOrWhiteSpace(localAddress) && localAddress.EndsWith("/"))
            {
                localAddress = localAddress.Substring(0, config.RealTimeServerLink.Length - 1);
            }
            if (!string.IsNullOrWhiteSpace(cloudAddress) && cloudAddress.EndsWith("/"))
            {
                cloudAddress = cloudAddress.Substring(0, config.RealTimeServerLink.Length - 1);
            }

            result = Ok(new { localAddress, cloudAddress, username, password });
            return result;
        }

        [ActionName("GetPushNotificatioinLink")]
        [HttpGet]
        public IActionResult GetPushNotificatioinLink()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            ConfigObject config = ConfigObject.GetConfig(cache);
            string url = config.PushNotificatioinLink;
            if (config.PushNotificatioinLink.EndsWith("/"))
            {
                url = config.PushNotificatioinLink.Substring(0, config.PushNotificatioinLink.Length - 1);
            }

            result = Ok(url);
            return result;
        }
    }
}
