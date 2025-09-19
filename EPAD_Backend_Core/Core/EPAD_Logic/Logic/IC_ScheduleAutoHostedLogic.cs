using EPAD_Common;
using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.IC;
using EPAD_Data.Models.Lovad;
using EPAD_Data.Models.Other;
using EPAD_Data.Models.TimeLog;
using EPAD_Data.Models.WebAPIHeader;
using EPAD_Data.Sync_Entities;
using EPAD_Logic.Configuration;
using EPAD_Logic.MainProcess;
using EPAD_Logic.SendMail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace EPAD_Logic
{
    public class IC_ScheduleAutoHostedLogic : IIC_ScheduleAutoHostedLogic
    {
        private IServiceScopeFactory _scopeFactory;
        private readonly EPAD_Context _dbContext;
        public Sync_Context _dbSyncContext;
        public ezHR_Context _dbEzHRContext;
        private readonly ILogger _logger;
        private static IMemoryCache _cache;
        private ConfigObject _config;
        private IEmailProvider _emailProvider;
        private IIC_ConfigLogic _iC_ConfigLogic;
        private IIC_DeviceLogic _iC_DeviceLogic;
        private IIC_ServiceLogic _iC_ServiceLogic;
        private IIC_ServiceAndDeviceLogic _iC_ServiceAndDeviceLogic;
        private IIC_CommandLogic _iC_CommandLogic;
        private IIC_SystemCommandLogic _iC_SystemCommandLogic;
        private IIC_CommandSystemGroupLogic _iC_CommandSystemGroupLogic;
        private IIC_EmployeeLogic _iC_EmployeeLogic;
        private IIC_WorkingInfoLogic _iC_WorkingInfoLogic;
        private IIC_UserMasterLogic _iC_UserMasterLogic;
        private IIC_DepartmentLogic _iC_DepartmentLogic;
        private IIC_Employee_IntegrateLogic _iC_Employee_IntegrateLogic;
        private IEmployee_Shift_IntegrateLogic _employee_Shift_IntegrateLogic;
        private IHR_EmployeeLogic _hR_EmployeeLogic;
        private IIC_UserNotificationLogic _iC_UserNotificationLogic;
        private IConfiguration _Configuration;
        private IHttpClientFactory _ClientFactory;
        private AutoMapper.IMapper _Mapper;
        private string mLinkGCSMonitoringApi;
        private double _configPreviousHoursIntergrateLog;
        private string _configNameSerialNumber_IN;
        private string _configNameSerialNumber_OUT;
        private string _configNameSerialNumberHRPRO7_IN;
        private string _configNameSerialNumberHRPRO7_OUT;
        private string _configClientName;
        private AppConfiguration _AppConfigAEONEmployee;
        private AppConfiguration _AppConfigAEONDepartment;
        private AppConfiguration _AppConfigAVN;
        private readonly IThirdPartyIntegrationConfigurationService _thirdPartyIntegrationConfigurationService;
        private IHR_PositionInfoLogic _hR_PositionInfoLogic;
        private string mLinkECMSApi;
        private string mCommunicateToken;
        private int mCompanyIndex = 2;
        private int mTimesResendMail = 1;
        private IIC_IntegrateLogic _ic_IntegrateLogic;
        private readonly IHostingEnvironment _hostingEnvironment;

        public IC_ScheduleAutoHostedLogic(IServiceScopeFactory scopeFactory, EPAD_Context dbContext, Sync_Context sync_Context, ezHR_Context dbEzHRContext,
            IMemoryCache cache, IIC_ConfigLogic iC_ConfigLogic,
            IIC_DeviceLogic iC_DeviceLogic, IIC_ServiceLogic iC_ServiceLogic, IIC_ServiceAndDeviceLogic iC_ServiceAndDeviceLogic,
            IIC_CommandLogic iC_CommandLogic, IIC_CommandSystemGroupLogic iC_CommandSystemGroupLogic, IIC_SystemCommandLogic iC_SystemCommandLogic,
            IEmailProvider emailProvider, IIC_EmployeeLogic iC_EmployeeLogic, IIC_WorkingInfoLogic iC_WorkingInfoLogic, IIC_UserMasterLogic iC_UserMasterLogic,
            IIC_DepartmentLogic iC_DepartmentLogic, IIC_Employee_IntegrateLogic iC_Employee_IntegrateLogic,
            IEmployee_Shift_IntegrateLogic employee_Shift_IntegrateLogic, IHR_EmployeeLogic hR_EmployeeLogic, IHR_EmployeeInfoLogic hR_EmployeeInfoLogic,
            IIC_UserNotificationLogic iC_UserNotificationLogic,
            IThirdPartyIntegrationConfigurationService thirdPartyIntegrationConfigurationService,
            IHttpClientFactory clientFactory, IIC_IntegrateLogic ic_IntegrateLogic,
            IConfiguration configuration, AutoMapper.IMapper Mapper, ILogger<IC_ScheduleAutoHostedLogic> logger, IHR_PositionInfoLogic hR_PositionInfoLogic, IHostingEnvironment hostingEnvironment)
        {
            _scopeFactory = scopeFactory;
            _dbContext = dbContext;
            _dbSyncContext = sync_Context;
            _cache = cache;
            _config = ConfigObject.GetConfig(_cache);
            _iC_ConfigLogic = iC_ConfigLogic;
            _iC_DeviceLogic = iC_DeviceLogic;
            _iC_ServiceLogic = iC_ServiceLogic;
            _iC_ServiceAndDeviceLogic = iC_ServiceAndDeviceLogic;
            _iC_CommandLogic = iC_CommandLogic;
            _iC_CommandSystemGroupLogic = iC_CommandSystemGroupLogic;
            _iC_SystemCommandLogic = iC_SystemCommandLogic;
            _emailProvider = emailProvider;
            _iC_EmployeeLogic = iC_EmployeeLogic;
            _iC_WorkingInfoLogic = iC_WorkingInfoLogic;
            _iC_UserMasterLogic = iC_UserMasterLogic;
            _iC_DepartmentLogic = iC_DepartmentLogic;
            _iC_Employee_IntegrateLogic = iC_Employee_IntegrateLogic;
            _employee_Shift_IntegrateLogic = employee_Shift_IntegrateLogic;
            _hR_EmployeeLogic = hR_EmployeeLogic;
            _dbEzHRContext = dbEzHRContext;
            _iC_UserNotificationLogic = iC_UserNotificationLogic;
            _thirdPartyIntegrationConfigurationService = thirdPartyIntegrationConfigurationService;
            _logger = logger;
            _ClientFactory = clientFactory;
            _Configuration = configuration;
            mLinkGCSMonitoringApi = _Configuration.GetValue<string>("GCSApi");
            _Configuration.GetValue<double>("IntervalSecond");
            _configPreviousHoursIntergrateLog = _Configuration.GetValue<double>("PreviousHours");
            _AppConfigAEONEmployee = _Configuration.GetSection("AEON_Employee").Get<AppConfiguration>();
            _AppConfigAEONDepartment = _Configuration.GetSection("AEON_Department").Get<AppConfiguration>();
            _configNameSerialNumber_IN = _Configuration.GetValue<string>("SerialNumber_IN");
            _configNameSerialNumber_OUT = _Configuration.GetValue<string>("SerialNumber_OUT");
            _configNameSerialNumberHRPRO7_IN = _Configuration.GetValue<string>("SerialNumberHRPRO7_IN");
            _configNameSerialNumberHRPRO7_OUT = _Configuration.GetValue<string>("SerialNumberHRPRO7_OUT");
            _configClientName = _Configuration.GetValue<string>("ClientName").ToUpper();
            _Configuration.GetValue<string>("DEPARTMENT_NAME");
            _Mapper = Mapper;
            _AppConfigAVN = _Configuration.GetSection("AVN").Get<AppConfiguration>();
            _hR_PositionInfoLogic = hR_PositionInfoLogic;
            mLinkECMSApi = _Configuration.GetValue<string>("ECMSApi");
            mCommunicateToken = _Configuration.GetValue<string>("CommunicateToken");
            mCompanyIndex = _config.CompanyIndex;
            mTimesResendMail = _config.TimesResendMail;
            _ic_IntegrateLogic = ic_IntegrateLogic;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task DownloadLogFromToTime()
        {
            string timePostCheck = DateTime.Now.ToHHmm();
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.DOWNLOAD_LOG.ToString() });
            List<IC_ConfigDTO> downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);

            if (downloadConfig != null)
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {
                    List<string> timePos = config.TimePos.Split(new char[] { ';', '|', ',' }).ToList();
                    if (!timePos.Contains(timePostCheck))
                    {
                        //return;
                    }
                    else
                    {
                        System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/log.txt", "DOWNLOAD_LOG");
                        DateTime toTime = DateTime.Now;
                        DateTime fromTime = toTime.AddDays((config.PreviousDays ?? 0) * -1);
                        // tải log từ 0h sáng ngày hiện tại
                        fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);

                        addedParams = new List<AddedParam>();
                        addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
                        var listService = _iC_ServiceLogic.GetMany(addedParams);
                        if (listService != null)
                        {
                            foreach (var service in listService)
                            {
                                List<string> lsSerialHw = new List<string>();

                                addedParams = new List<AddedParam>();
                                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
                                addedParams.Add(new AddedParam { Key = "ServiceIndex", Value = service.Index });
                                var listDeviceOfService = _iC_ServiceAndDeviceLogic.GetMany(addedParams);
                                if (listDeviceOfService != null)
                                {
                                    ListSerialCheckHardWareLicense(listDeviceOfService.Select(e => e.SerialNumber).ToList(), ref lsSerialHw);

                                    IC_CommandParamDTO comParam = new IC_CommandParamDTO();
                                    comParam.ListSerialNumber = lsSerialHw;
                                    comParam.Action = CommandAction.DownloadLogFromToTime;
                                    comParam.FromTime = fromTime;
                                    comParam.ToTime = toTime;
                                    comParam.ExternalData = "";
                                    comParam.IsOverwriteData = false;
                                    List<CommandResult> lstCmdDownload = _iC_CommandLogic.CreateListCommands(comParam);
                                    if (lstCmdDownload != null)
                                    {
                                        IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                                        grouComParam.CompanyIndex = config.CompanyIndex;
                                        grouComParam.ListCommand = lstCmdDownload;
                                        grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                                        grouComParam.GroupName = GroupName.DownloadLogFromToTime.ToString();
                                        grouComParam.EventType = config.EventType;
                                        _iC_CommandLogic.CreateGroupCommands(grouComParam);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var downloadConfigByGroupDevice = await _dbContext.IC_ConfigByGroupMachine.AsNoTracking().Where(x
                => x.CompanyIndex == 2 && x.EventType == ConfigAuto.DOWNLOAD_LOG.ToString()).ToListAsync();
            if (downloadConfigByGroupDevice.Count > 0)
            {
                var groupDeviceIndexes = downloadConfigByGroupDevice.Select(x => x.GroupDeviceIndex).ToList();
                var timePostList = downloadConfigByGroupDevice.SelectMany(x => x.TimePos.Split(new char[] { ';', '|', ',' }).ToList()).ToList();
                if (!timePostList.Contains(timePostCheck))
                {
                    //return;
                }
                else
                {
                    var listDeviceGroupIndex = downloadConfigByGroupDevice.Select(x => x.GroupDeviceIndex).ToList();
                    var listDeviceOfGroup = await _dbContext.IC_GroupDeviceDetails.AsNoTracking().Where(x
                            => listDeviceGroupIndex.Contains(x.GroupDeviceIndex)).ToListAsync();
                    foreach (var config in downloadConfigByGroupDevice)
                    {
                        System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/log.txt", "DOWNLOAD_LOG");
                        DateTime toTime = DateTime.Now;
                        DateTime fromTime = toTime.AddDays((config.PreviousDays ?? 0) * -1);
                        // tải log từ 0h sáng ngày hiện tại
                        fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);

                        var deviceOfGroup = listDeviceOfGroup.Where(x
                            => x.GroupDeviceIndex == config.GroupDeviceIndex).ToList();
                        if (deviceOfGroup.Count > 0)
                        {
                            List<string> lsSerialHw = new List<string>();
                            var listSerial = deviceOfGroup.Select(x => x.SerialNumber).ToList();
                            ListSerialCheckHardWareLicense(listSerial, ref lsSerialHw);

                            IC_CommandParamDTO comParam = new IC_CommandParamDTO();
                            comParam.ListSerialNumber = lsSerialHw;
                            comParam.Action = CommandAction.DownloadLogFromToTime;
                            comParam.FromTime = fromTime;
                            comParam.ToTime = toTime;
                            comParam.ExternalData = "";
                            comParam.IsOverwriteData = false;
                            List<CommandResult> lstCmdDownload = _iC_CommandLogic.CreateListCommands(comParam);
                            if (lstCmdDownload != null)
                            {
                                IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                                grouComParam.CompanyIndex = config.CompanyIndex;
                                grouComParam.ListCommand = lstCmdDownload;
                                grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                                grouComParam.GroupName = GroupName.DownloadLogFromToTime.ToString();
                                grouComParam.EventType = config.EventType;
                                _iC_CommandLogic.CreateGroupCommands(grouComParam);
                            }
                        }
                    }
                }
            }
        }
        public void DeleteHolidayByDay()
        {
            string timePostCheck = DateTime.Now.ToHHmm();
            var addedParams = new List<AddedParam>();
            IC_CommandRequestDTO commandRequest = new IC_CommandRequestDTO()
            {
                AutoOffSecond = 0
            };

            if (timePostCheck != "17:15")
            {
                return;
            }
            else
            {
                List<string> lsSerialHw = new List<string>();
                var lstHolidays = new List<AC_AccHoliday>();
                var timezones = new List<AC_TimeZone>();

                lstHolidays = _dbContext.AC_AccHoliday.AsNoTracking().Where(x => x.CompanyIndex == _config.CompanyIndex && x.DoorIndex != 0).ToList();


                lstHolidays = lstHolidays.Where(x => x.Loop || x.StartDate.Date >= DateTime.Now.Date).ToList();
                timezones = _dbContext.AC_TimeZone.Where(x => x.CompanyIndex == _config.CompanyIndex).ToList();
                if (lstHolidays != null && lstHolidays.Count > 0)
                {
                    var groupHolidays = lstHolidays.GroupBy(x => x.DoorIndex).Select(x => new { Key = x.Key, Value = x.ToList() }).ToList();
                    foreach (var item in groupHolidays)
                    {
                        var ListSerial = _dbContext.AC_DoorAndDevice.Where(x => x.DoorIndex == item.Key).Select(x => x.SerialNumber).ToList();

                        ListSerialCheckHardWareLicense(ListSerial, ref lsSerialHw);
                        if (lsSerialHw != null && lsSerialHw.Count > 0)
                        {

                            if (lsSerialHw.Count > 0)
                            {
                                var lstUser = new List<UserInfoOnMachine>();

                                var lstGroups = new List<AC_AccGroup>();
                                var lstTimezones = new List<AC_TimeZone>();
                                List<CommandResult> lstCmd = CreateListACCommand(_dbContext, lsSerialHw, CommandAction.UploadAccHoliday, new DateTime(2000, 1, 1), DateTime.Now, lstUser, lstGroups, item.Value, lstTimezones, commandRequest, true, GlobalParams.DevicePrivilege.SDKStandardRole);
                                CreateGroupCommand(_config.CompanyIndex, UpdatedUser.SYSTEM_AUTO.ToString(), CommandAction.UploadAccHoliday.ToString(), lstCmd, "");
                            }
                        }

                    }
                }

            }

        }

        void CreateGroupCommand(int pCompanyIndex, string pUserName, string pGroupName, List<CommandResult> pListCommands, string pEventType, string externalData = "")
        {
            CommandProcess.CreateGroupCommand(_dbContext, _cache, pCompanyIndex, pUserName, pGroupName, externalData, pListCommands, pEventType);
        }

        List<CommandResult> CreateListACCommand(EPAD_Context context, List<string> listSerial, CommandAction pAction, DateTime pFromTime, DateTime pToTime, List<UserInfoOnMachine> pListUsers, List<AC_AccGroup> accGroups, List<AC_AccHoliday> accHolidays, List<AC_TimeZone> timeZones, IC_CommandRequestDTO commandRequest, bool isOverwriteData, short privilege, string externalData = "")
        {
            return CommandProcess.CreateListACCommands(context, listSerial, pAction, externalData, pFromTime, pToTime, pListUsers, accGroups, accHolidays, timeZones, isOverwriteData, commandRequest, privilege);
        }

        public async Task DownloadLogStateLog()
        {
            string timePostCheck = DateTime.Now.ToHHmm();
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.DOWNLOAD_STATE_LOG.ToString() });
            List<IC_ConfigDTO> downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);

            if (downloadConfig != null)
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {
                    System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/log.txt", "DOWNLOAD_STATE_LOG");
                    DateTime toTime = DateTime.Now;
                    DateTime fromTime = toTime.AddDays((config.PreviousDays ?? 0) * -1);
                    // tải log từ 0h sáng ngày hiện tại
                    fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);

                    addedParams = new List<AddedParam>();
                    addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
                    var listService = _iC_ServiceLogic.GetMany(addedParams);
                    if (listService != null)
                    {
                        foreach (var service in listService)
                        {
                            List<string> lsSerialHw = new List<string>();

                            addedParams = new List<AddedParam>();
                            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
                            addedParams.Add(new AddedParam { Key = "ServiceIndex", Value = service.Index });
                            var listDeviceOfService = _iC_ServiceAndDeviceLogic.GetMany(addedParams);
                            if (listDeviceOfService != null)
                            {
                                ListSerialCheckHardWareLicense(listDeviceOfService.Select(e => e.SerialNumber).ToList(), ref lsSerialHw);

                                List<AddedParam> addedParamss = new List<AddedParam>();
                                addedParamss.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
                                addedParamss.Add(new AddedParam { Key = "SystemCommandStatus", Value = false });
                                addedParamss.Add(new AddedParam { Key = "CommandName", Value = CommandAction.DownloadStateLog });
                                addedParamss.Add(new AddedParam { Key = "ListSerialNumber", Value = lsSerialHw });
                                List<IC_SystemCommandDTO> listCommandHasExisted = _iC_SystemCommandLogic.GetMany(addedParamss);
                                if (listCommandHasExisted != null && listCommandHasExisted.Count > 0)
                                {
                                    var serials = listCommandHasExisted.Select(x => x.SerialNumber).ToList();
                                    lsSerialHw = lsSerialHw.Where(x => !serials.Contains(x)).ToList();
                                }
                                if (lsSerialHw == null || lsSerialHw.Count == 0)
                                {
                                    continue;
                                }

                                IC_CommandParamDTO comParam = new IC_CommandParamDTO();
                                comParam.ListSerialNumber = lsSerialHw;
                                comParam.Action = CommandAction.DownloadStateLog;
                                comParam.FromTime = fromTime;
                                comParam.ToTime = toTime;
                                comParam.ExternalData = "";
                                comParam.IsOverwriteData = false;
                                List<CommandResult> lstCmdDownload = _iC_CommandLogic.CreateListCommands(comParam);
                                if (lstCmdDownload != null)
                                {
                                    IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                                    grouComParam.CompanyIndex = config.CompanyIndex;
                                    grouComParam.ListCommand = lstCmdDownload;
                                    grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                                    grouComParam.GroupName = GroupName.DOWNLOAD_STATE_LOG.ToString();
                                    grouComParam.EventType = config.EventType;
                                    _iC_CommandLogic.CreateGroupCommands(grouComParam);
                                }
                            }
                        }
                    }

                }
            }
        }

        public void CreateCheckDeviceCapacityCommands()
        {
            var addedParams = new List<AddedParam>();
            var companyIndex = 2;
            if (_config != null)
            {
                companyIndex = _config.CompanyIndex;
            }
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex });
            var listService = _iC_ServiceLogic.GetMany(addedParams);
            if (listService != null && listService.Count() > 0)
            {
                List<string> lsSerialHw = new List<string>();

                addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListServiceIndex", Value = listService.Select(x => x.Index).ToList() });

                var listDeviceOfService = _iC_ServiceAndDeviceLogic.GetMany(addedParams);
                if (listDeviceOfService != null)
                {
                    ListSerialCheckHardWareLicense(listDeviceOfService.Select(e => e.SerialNumber).ToList(), ref lsSerialHw);

                    IC_CommandParamDTO comParam = new IC_CommandParamDTO();
                    comParam.ListSerialNumber = lsSerialHw;
                    comParam.Action = CommandAction.GetDeviceInfo;
                    comParam.FromTime = DateTime.Now;
                    comParam.ToTime = DateTime.Now;
                    comParam.ExternalData = "";
                    comParam.IsOverwriteData = false;
                    List<CommandResult> lstCmdDownload = _iC_CommandLogic.CreateListCommands(comParam);
                    if (lstCmdDownload != null)
                    {
                        IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                        grouComParam.CompanyIndex = _config.CompanyIndex;
                        grouComParam.ListCommand = lstCmdDownload;
                        grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                        grouComParam.GroupName = GroupName.DownloadLogFromToTime.ToString();
                        grouComParam.EventType = ConfigAuto.FULL_CAPACITY.ToString();
                        _iC_CommandLogic.CreateGroupCommands(grouComParam);
                    }
                }

            }
        }

        public async Task DeleteLogFromToTime()
        {
            string timePostCheck = DateTime.Now.ToHHmm();

            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.DELETE_LOG.ToString() });
            List<IC_ConfigDTO> downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);

            if (downloadConfig != null)
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {

                    List<string> timePos = config.TimePos.Split(new char[] { ';', '|', ',' }).ToList();
                    if (!timePos.Contains(timePostCheck))
                    {
                        //return;
                    }
                    else
                    {
                        //System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/log.txt", "DELETE_LOG");
                        DateTime pNow = DateTime.Now;
                        DateTime toTime = new DateTime(pNow.Year, pNow.Month, pNow.Day, 23, 59, 59);
                        DateTime fromTime = new DateTime(2000, 1, 1, 0, 0, 0);
                        // trước hiện tại bao nhiêu ngày
                        toTime = toTime.AddDays((config.PreviousDays ?? 0) * -1);

                        addedParams = new List<AddedParam>();
                        addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
                        var listService = _iC_ServiceLogic.GetMany(addedParams);
                        if (listService != null)
                        {
                            foreach (var service in listService)
                            {
                                List<string> lsSerialHw = new List<string>();

                                addedParams = new List<AddedParam>();
                                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
                                addedParams.Add(new AddedParam { Key = "ServiceIndex", Value = service.Index });
                                var listDeviceOfService = _iC_ServiceAndDeviceLogic.GetMany(addedParams);
                                if (listDeviceOfService != null)
                                {
                                    ListSerialCheckHardWareLicense(listDeviceOfService.Select(e => e.SerialNumber).ToList(), ref lsSerialHw);

                                    IC_CommandParamDTO comParam = new IC_CommandParamDTO();
                                    comParam.ListSerialNumber = lsSerialHw;
                                    comParam.Action = CommandAction.DeleteLogFromToTime;
                                    comParam.FromTime = fromTime;
                                    comParam.ToTime = toTime;
                                    comParam.ExternalData = "";
                                    comParam.IsOverwriteData = false;
                                    List<CommandResult> lstCmdDownload = _iC_CommandLogic.CreateListCommands(comParam);
                                    if (lstCmdDownload != null)
                                    {
                                        IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                                        grouComParam.CompanyIndex = config.CompanyIndex;
                                        grouComParam.ListCommand = lstCmdDownload;
                                        grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                                        grouComParam.GroupName = GroupName.DeleteLogFromToTime.ToString();
                                        grouComParam.EventType = config.EventType;
                                        _iC_CommandLogic.CreateGroupCommands(grouComParam);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var downloadConfigByGroupDevice = await _dbContext.IC_ConfigByGroupMachine.AsNoTracking().Where(x
                => x.CompanyIndex == 2 && x.EventType == ConfigAuto.DELETE_LOG.ToString()).ToListAsync();
            if (downloadConfigByGroupDevice.Count > 0)
            {
                var groupDeviceIndexes = downloadConfigByGroupDevice.Select(x => x.GroupDeviceIndex).ToList();
                var timePostList = downloadConfigByGroupDevice.SelectMany(x => x.TimePos.Split(new char[] { ';', '|', ',' }).ToList()).ToList();
                if (!timePostList.Contains(timePostCheck))
                {
                    //return;
                }
                else
                {
                    var listDeviceGroupIndex = downloadConfigByGroupDevice.Select(x => x.GroupDeviceIndex).ToList();
                    var listDeviceOfGroup = await _dbContext.IC_GroupDeviceDetails.AsNoTracking().Where(x
                            => listDeviceGroupIndex.Contains(x.GroupDeviceIndex)).ToListAsync();
                    foreach (var config in downloadConfigByGroupDevice)
                    {
                        //System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/log.txt", "DELETE_LOG");
                        DateTime toTime = DateTime.Now;
                        DateTime fromTime = toTime.AddDays((config.PreviousDays ?? 0) * -1);
                        // tải log từ 0h sáng ngày hiện tại
                        fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);

                        var deviceOfGroup = listDeviceOfGroup.Where(x
                            => x.GroupDeviceIndex == config.GroupDeviceIndex).ToList();
                        if (deviceOfGroup.Count > 0)
                        {
                            List<string> lsSerialHw = new List<string>();
                            var listSerial = deviceOfGroup.Select(x => x.SerialNumber).ToList();
                            ListSerialCheckHardWareLicense(listSerial, ref lsSerialHw);

                            IC_CommandParamDTO comParam = new IC_CommandParamDTO();
                            comParam.ListSerialNumber = lsSerialHw;
                            comParam.Action = CommandAction.DeleteLogFromToTime;
                            comParam.FromTime = fromTime;
                            comParam.ToTime = toTime;
                            comParam.ExternalData = "";
                            comParam.IsOverwriteData = false;
                            List<CommandResult> lstCmdDownload = _iC_CommandLogic.CreateListCommands(comParam);
                            if (lstCmdDownload != null)
                            {
                                IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                                grouComParam.CompanyIndex = config.CompanyIndex;
                                grouComParam.ListCommand = lstCmdDownload;
                                grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                                grouComParam.GroupName = GroupName.DeleteLogFromToTime.ToString();
                                grouComParam.EventType = config.EventType;
                                _iC_CommandLogic.CreateGroupCommands(grouComParam);
                            }
                        }
                    }
                }
            }
        }

        public async Task DownloadAllUser()
        {
            string timePostCheck = DateTime.Now.ToHHmm();

            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.DOWNLOAD_USER.ToString() });
            List<IC_ConfigDTO> downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);

            if (downloadConfig != null)
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {

                    List<string> timePos = config.TimePos.Split(new char[] { ';', '|', ',' }).ToList();
                    if (!timePos.Contains(timePostCheck))
                    {
                        //return;
                    }
                    else
                    {
                        System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/log.txt", "DOWNLOAD_USER");
                        List<string> lstSerialDownload = config.IntegrateLogParam.ListSerialNumber.Split(';').ToList();
                        List<string> lsSerialHw = new List<string>();
                        ListSerialCheckHardWareLicense(lstSerialDownload, ref lsSerialHw);

                        IC_CommandParamDTO comParam = new IC_CommandParamDTO();
                        comParam.ListSerialNumber = lsSerialHw;
                        comParam.Action = CommandAction.DownloadAllUserMaster;
                        comParam.FromTime = DateTime.Now;
                        comParam.ToTime = DateTime.Now;
                        comParam.ExternalData = "";
                        comParam.IsOverwriteData = config.IntegrateLogParam.IsOverwriteData;
                        List<CommandResult> lstCmdDownload = _iC_CommandLogic.CreateListCommands(comParam);
                        if (lstCmdDownload != null)
                        {
                            IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                            grouComParam.CompanyIndex = config.CompanyIndex;
                            grouComParam.ListCommand = lstCmdDownload;
                            grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                            grouComParam.GroupName = GroupName.DownloadAllUserMaster.ToString();
                            grouComParam.EventType = config.EventType;
                            _iC_CommandLogic.CreateGroupCommands(grouComParam);
                        }
                    }
                }
            }

            var downloadConfigByGroupDevice = await _dbContext.IC_ConfigByGroupMachine.AsNoTracking().Where(x
                => x.CompanyIndex == 2 && x.EventType == ConfigAuto.DOWNLOAD_USER.ToString()).ToListAsync();
            if (downloadConfigByGroupDevice.Count > 0)
            {
                var groupDeviceIndexes = downloadConfigByGroupDevice.Select(x => x.GroupDeviceIndex).ToList();
                var timePostList = downloadConfigByGroupDevice.SelectMany(x => x.TimePos.Split(new char[] { ';', '|', ',' }).ToList()).ToList();
                if (!timePostList.Contains(timePostCheck))
                {
                    //return;
                }
                else
                {
                    var listDeviceGroupIndex = downloadConfigByGroupDevice.Select(x => x.GroupDeviceIndex).ToList();
                    var listDeviceOfGroup = await _dbContext.IC_GroupDeviceDetails.AsNoTracking().Where(x
                            => listDeviceGroupIndex.Contains(x.GroupDeviceIndex)).ToListAsync();
                    foreach (var config in downloadConfigByGroupDevice)
                    {
                        var configIntegrateLogParam = !string.IsNullOrWhiteSpace(config.CustomField)
                            ? JsonConvert.DeserializeObject<IntegrateLogParam>(config.CustomField,
                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                            : new IntegrateLogParam();
                        System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/log.txt", "DOWNLOAD_USER");
                        DateTime toTime = DateTime.Now;
                        DateTime fromTime = toTime.AddDays((config.PreviousDays ?? 0) * -1);
                        // tải log từ 0h sáng ngày hiện tại
                        fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);

                        var deviceOfGroup = listDeviceOfGroup.Where(x
                            => x.GroupDeviceIndex == config.GroupDeviceIndex).ToList();
                        if (deviceOfGroup.Count > 0)
                        {
                            List<string> lsSerialHw = new List<string>();
                            var listSerial = deviceOfGroup.Select(x => x.SerialNumber).ToList();
                            ListSerialCheckHardWareLicense(listSerial, ref lsSerialHw);

                            IC_CommandParamDTO comParam = new IC_CommandParamDTO();
                            comParam.ListSerialNumber = lsSerialHw;
                            comParam.Action = CommandAction.DownloadAllUserMaster;
                            comParam.FromTime = DateTime.Now;
                            comParam.ToTime = DateTime.Now;
                            comParam.ExternalData = "";
                            comParam.IsOverwriteData = configIntegrateLogParam.IsOverwriteData;
                            List<CommandResult> lstCmdDownload = _iC_CommandLogic.CreateListCommands(comParam);
                            if (lstCmdDownload != null)
                            {
                                IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                                grouComParam.CompanyIndex = config.CompanyIndex;
                                grouComParam.ListCommand = lstCmdDownload;
                                grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                                grouComParam.GroupName = GroupName.DownloadAllUserMaster.ToString();
                                grouComParam.EventType = config.EventType;
                                _iC_CommandLogic.CreateGroupCommands(grouComParam);
                            }
                        }
                    }
                }
            }
        }

        public async Task RestartDevice()
        {
            string timePostCheck = DateTime.Now.ToHHmm();

            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.START_MACHINE.ToString() });
            List<IC_ConfigDTO> downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);

            if (downloadConfig != null)
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {

                    List<string> timePos = config.TimePos.Split(new char[] { ';', '|', ',' }).ToList();
                    if (!timePos.Contains(timePostCheck))
                    {
                        //return;
                    }
                    else
                    {
                        //System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/log.txt", "START_MACHINE");
                        DateTime toTime = DateTime.Now;
                        DateTime fromTime = toTime.AddDays((config.PreviousDays ?? 0) * -1);
                        // tải log từ 0h sáng ngày hiện tại
                        fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);

                        addedParams = new List<AddedParam>();
                        addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
                        var listService = _iC_ServiceLogic.GetMany(addedParams);
                        if (listService != null)
                        {
                            foreach (var service in listService)
                            {
                                List<string> lsSerialHw = new List<string>();

                                addedParams = new List<AddedParam>();
                                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
                                addedParams.Add(new AddedParam { Key = "ServiceIndex", Value = service.Index });
                                var listDeviceOfService = _iC_ServiceAndDeviceLogic.GetMany(addedParams);
                                var lstDevices = listDeviceOfService.Select(e => e.SerialNumber).ToList();
                                if (listDeviceOfService != null)
                                {
                                    var lstDevice = listDeviceOfService.Select(e => e.SerialNumber).ToList();
                                    ListSerialCheckHardWareLicense(lstDevice, ref lsSerialHw);
                                    IC_CommandParamDTO comParam = new IC_CommandParamDTO();
                                    comParam.ListSerialNumber = lsSerialHw;
                                    comParam.Action = CommandAction.RestartDevice;
                                    comParam.FromTime = fromTime;
                                    comParam.ToTime = toTime;
                                    comParam.ExternalData = "";
                                    comParam.IsOverwriteData = false;
                                    List<CommandResult> lstCmdDownload = _iC_CommandLogic.CreateListCommands(comParam);
                                    if (lstCmdDownload != null)
                                    {
                                        IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                                        grouComParam.CompanyIndex = config.CompanyIndex;
                                        grouComParam.ListCommand = lstCmdDownload;
                                        grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                                        grouComParam.GroupName = GroupName.RestartDevice.ToString();
                                        grouComParam.EventType = config.EventType;
                                        _iC_CommandLogic.CreateGroupCommands(grouComParam);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var downloadConfigByGroupDevice = await _dbContext.IC_ConfigByGroupMachine.AsNoTracking().Where(x
                => x.CompanyIndex == 2 && x.EventType == ConfigAuto.START_MACHINE.ToString()).ToListAsync();
            if (downloadConfigByGroupDevice.Count > 0)
            {
                var groupDeviceIndexes = downloadConfigByGroupDevice.Select(x => x.GroupDeviceIndex).ToList();
                var timePostList = downloadConfigByGroupDevice.SelectMany(x => x.TimePos.Split(new char[] { ';', '|', ',' }).ToList()).ToList();
                if (!timePostList.Contains(timePostCheck))
                {
                    //return;
                }
                else
                {
                    var listDeviceGroupIndex = downloadConfigByGroupDevice.Select(x => x.GroupDeviceIndex).ToList();
                    var listDeviceOfGroup = await _dbContext.IC_GroupDeviceDetails.AsNoTracking().Where(x
                            => listDeviceGroupIndex.Contains(x.GroupDeviceIndex)).ToListAsync();
                    foreach (var config in downloadConfigByGroupDevice)
                    {
                        //System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/log.txt", "START_MACHINE");
                        DateTime toTime = DateTime.Now;
                        DateTime fromTime = toTime.AddDays((config.PreviousDays ?? 0) * -1);
                        // tải log từ 0h sáng ngày hiện tại
                        fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);

                        var deviceOfGroup = listDeviceOfGroup.Where(x
                            => x.GroupDeviceIndex == config.GroupDeviceIndex).ToList();
                        if (deviceOfGroup.Count > 0)
                        {
                            List<string> lsSerialHw = new List<string>();
                            var listSerial = deviceOfGroup.Select(x => x.SerialNumber).ToList();
                            ListSerialCheckHardWareLicense(listSerial, ref lsSerialHw);

                            IC_CommandParamDTO comParam = new IC_CommandParamDTO();
                            comParam.ListSerialNumber = lsSerialHw;
                            comParam.Action = CommandAction.RestartDevice;
                            comParam.FromTime = fromTime;
                            comParam.ToTime = toTime;
                            comParam.ExternalData = "";
                            comParam.IsOverwriteData = false;
                            List<CommandResult> lstCmdDownload = _iC_CommandLogic.CreateListCommands(comParam);
                            if (lstCmdDownload != null)
                            {
                                IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                                grouComParam.CompanyIndex = config.CompanyIndex;
                                grouComParam.ListCommand = lstCmdDownload;
                                grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                                grouComParam.GroupName = GroupName.RestartDevice.ToString();
                                grouComParam.EventType = config.EventType;
                                _iC_CommandLogic.CreateGroupCommands(grouComParam);
                            }
                        }
                    }
                }
            }
        }

        public async Task UpdateGroupCommand()
        {
            List<CommandParam> listParams = new List<CommandParam>();
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "Excuted", Value = false });
            List<IC_CommandSystemGroupDTO> listCommandGroup = _iC_CommandSystemGroupLogic.GetMany(addedParams);
            if (listCommandGroup != null)
            {
                listCommandGroup = listCommandGroup.Where(e => e.CreatedDate.Value.AddMinutes(-1).ToHHmm().Contains(DateTime.Now.AddDays(-1).ToHHmm())).ToList();

                if (listCommandGroup != null)
                {
                    foreach (var systemGroup in listCommandGroup)
                    {
                        addedParams = new List<AddedParam>();
                        addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = systemGroup.CompanyIndex });
                        addedParams.Add(new AddedParam { Key = "GroupIndex", Value = systemGroup.Index });
                        var listSystemCommand = _iC_SystemCommandLogic.GetMany(addedParams);
                        if (listSystemCommand != null)
                        {
                            foreach (var systemCommand in listSystemCommand)
                            {
                                listParams.Add(new CommandParam()
                                {
                                    ID = systemCommand.Index.ToString(),
                                    SDKFuntion = systemCommand.Command,
                                    Status = CommandStatus.Success.ToString(),
                                    Error = "Expired"
                                });
                            }
                        }
                    }

                    var listGroupIndex = listCommandGroup.Select(t => t.Index.ToString()).ToList();
                    var companyInfo = CompanyInfo.GetFromCache(_cache, listCommandGroup.FirstOrDefault().CompanyIndex.ToString());
                    if (companyInfo != null)
                    {
                        var listGroupInParam = companyInfo.ListCommandGroups.Where(t => listGroupIndex.Contains(t.ID)).ToList();
                        var listGroupDeleteIndex = new List<string>();
                        for (int i = 0; i < listGroupInParam.Count; i++)
                        {
                            // xóa cmd trong company cache nếu status = success
                            UpdateCommandInCompanyCache(listParams, listGroupInParam[i].ID, companyInfo, DateTime.Now);
                            // nếu tất cả command trong group hoàn thành thì update group
                            var listCommandResult = listGroupInParam[i].ListCommand;

                            bool allFinished = true;
                            for (int j = 0; j < listCommandResult.Count; j++)
                            {
                                if (listCommandResult[j].Status == CommandStatus.UnExecute.ToString() || listCommandResult[j].Status == CommandStatus.Executing.ToString())
                                {
                                    allFinished = false;
                                    break;
                                }
                                else if (listCommandResult[j].Error != null && listCommandResult[j].Error != "")
                                {
                                    listGroupInParam[i].Errors.Add(listCommandResult[j].Error);
                                }
                            }
                            DeleteCommandInCompanyCache(listCommandResult, listGroupInParam[i].ID, companyInfo, DateTime.Now);
                            if (allFinished == true)
                            {
                                listGroupInParam[i].Excuted = true;
                                listGroupInParam[i].FinishedTime = DateTime.Now;
                                listGroupDeleteIndex.Add(listGroupInParam[i].ID);
                                int index = int.Parse(listGroupInParam[i].ID);
                                var groupModel = _dbContext.IC_CommandSystemGroup.Where(t => t.Index == index).FirstOrDefault();
                                if (groupModel != null)
                                {
                                    groupModel.Excuted = true;
                                    groupModel.UpdatedDate = DateTime.Now;
                                    groupModel.UpdatedUser = UpdatedUser.SYSTEM_AUTO.ToString();

                                }
                                var systemCommands = _dbContext.IC_SystemCommand.Where(t => t.GroupIndex.Equals(groupModel.Index) && t.Excuted.Equals(false)).ToList();
                                foreach (var systemCommand in systemCommands)
                                {
                                    systemCommand.Excuted = true;
                                    systemCommand.ExcutedTime = DateTime.Now;
                                    systemCommand.Error = "Expired";
                                    systemCommand.UpdatedUser = UpdatedUser.UserSystem.ToString();
                                }
                            }
                        }
                        await _dbContext.SaveChangesAsync();

                        // remove group in cache
                        for (int i = 0; i < listGroupDeleteIndex.Count; i++)
                        {
                            companyInfo.DeleteGroupById(listGroupDeleteIndex[i]);
                        }
                    }
                }
            }
        }

        public async Task CheckDeviceFullCapacity()
        {
            string timePostCheck = DateTime.Now.ToHHmm();
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.FULL_CAPACITY.ToString() });
            List<IC_ConfigDTO> downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            if (downloadConfig != null)
            {
                int maxPercent = 80;
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {
                    List<string> timePos = config.TimePos.Split(new char[] { ';', '|', ',' }).ToList();
                    if (!timePos.Contains(timePostCheck))
                    {
                        return;
                    }
                    else
                    {
                        addedParams = new List<AddedParam>();
                        addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
                        var listDevice = _iC_DeviceLogic.GetMany(addedParams);
                        string bodyEmail = "";
                        if (listDevice != null)
                        {
                            for (int i = 0; i < listDevice.Count; i++)
                            {
                                if (listDevice[i].UserCapacity != null && listDevice[i].UserCapacity.HasValue && listDevice[i].UserCapacity.Value > 0)
                                {
                                    int userCapacity = listDevice[i].UserCapacity.Value;
                                    int userCount = listDevice[i].UserCount == null ? 0 : listDevice[i].UserCount.Value;

                                    if (userCount > (userCapacity * maxPercent / 100))
                                    {
                                        // user > 80% capacity
                                        bodyEmail += $"Thiết bị {listDevice[i].AliasName}({listDevice[i].IPAddress}) đã sử dụng hơn {maxPercent}% ({userCount}/{userCapacity}) " +
                                            $"bộ nhớ lưu trữ dữ liệu nhân viên, vui lòng cho sao lưu và xóa bớt dữ liệu trước khi tiếp tục sử dụng.\n";
                                    }
                                }
                                if (listDevice[i].FingerCapacity != null && listDevice[i].FingerCapacity.HasValue && listDevice[i].FingerCapacity.Value > 0)
                                {
                                    int fingerCapacity = listDevice[i].FingerCapacity.Value;
                                    int fingerCount = listDevice[i].FingerCount == null ? 0 : listDevice[i].FingerCount.Value;

                                    if (fingerCount > (fingerCapacity * maxPercent / 100))
                                    {
                                        // finger > 80% capacity
                                        bodyEmail += $"Thiết bị {listDevice[i].AliasName}({listDevice[i].IPAddress}) đã sử dụng hơn {maxPercent}%({fingerCount}/{fingerCapacity}) " +
                                            $"bộ nhớ lưu trữ dữ liệu vân tay, vui lòng cho sao lưu và xóa bớt dữ liệu trước khi tiếp tục sử dụng.\n";
                                    }
                                }
                                if (listDevice[i].AttendanceLogCapacity != null && listDevice[i].AttendanceLogCapacity.HasValue && listDevice[i].AttendanceLogCapacity.Value > 0)
                                {
                                    int logCapacity = listDevice[i].AttendanceLogCapacity.Value;
                                    int attendanceLogCount = listDevice[i].AttendanceLogCount == null ? 0 : listDevice[i].AttendanceLogCount.Value;
                                    if (attendanceLogCount > (logCapacity * maxPercent / 100))
                                    {
                                        // attendance log > 80% capacity
                                        bodyEmail += $"Thiết bị {listDevice[i].AliasName}({listDevice[i].IPAddress}) đã sử dụng hơn {maxPercent}%({attendanceLogCount}/{logCapacity}) " +
                                            $"bộ nhớ lưu trữ dữ liệu điểm danh, vui lòng cho sao lưu và xóa bớt dữ liệu trước khi tiếp tục sử dụng.\n";
                                    }
                                }
                                if (listDevice[i].FaceCapacity != null && listDevice[i].FaceCapacity.HasValue && listDevice[i].FaceCapacity.Value > 0)
                                {
                                    int faceCapacity = listDevice[i].FaceCapacity.Value;
                                    int faceCount = listDevice[i].FaceCount == null ? 0 : listDevice[i].FaceCount.Value;
                                    if (faceCount > (faceCapacity * maxPercent / 100))
                                    {
                                        // face > 80% capacity
                                        bodyEmail += $"Thiết bị {listDevice[i].AliasName}({listDevice[i].IPAddress}) đã sử dụng hơn {maxPercent}%({faceCount}/{faceCapacity}) " +
                                            $"bộ nhớ lưu trữ dữ liệu khuôn mặt, vui lòng cho sao lưu và xóa bớt dữ liệu trước khi tiếp tục sử dụng.\n";
                                    }
                                }
                            }
                            if (config.Email.Trim() != "" && bodyEmail.Trim() != "")
                            {
                                string title = "Thông báo máy chấm công gần đầy bộ nhớ";
                                ////_logger.LogInformation("Start Send Email Check capacity: " + title.ToString());
                                var mailHistory = new IC_MailHistory();
                                mailHistory.Title = title;
                                mailHistory.Content = bodyEmail;
                                mailHistory.CompanyIndex = mCompanyIndex;
                                mailHistory.EmailTo = config.Email.Trim();
                                mailHistory.CreatedBy = "CheckDeviceCapacity";
                                mailHistory.CreatedDate = DateTime.Now;
                                mailHistory.Status = (short)MailHistoryType.Pending;
                                mailHistory.Times = 0;
                                _dbContext.IC_MailHistory.Add(mailHistory);
                                _dbContext.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        public void CreateEmailWhenDeviceOffline()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
                string timePostCheck = DateTime.Now.ToHHmm();
                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
                addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.SEND_MAIL_WHEN_DEVICE_OFFLINE.ToString() });
                List<IC_ConfigDTO> downloadConfig = _iC_ConfigLogic.GetMany(addedParams).Result;
                if (downloadConfig != null)
                {
                    var config = downloadConfig.FirstOrDefault();
                    if (config != null && !string.IsNullOrWhiteSpace(config.Email))
                    {
                        List<string> emailList = config.Email.Split(new char[] { ';', '|', ',' }).ToList();
                        if (emailList != null && emailList.Count > 0)
                        {
                            var listDeviceUnSendEmail = db.IC_Device.Where(x => x.CompanyIndex == 2
                                && !x.IsSendMailLastDisconnect).ToList();

                            var groupDevice = (from i in db.IC_GroupDeviceDetails.Where(x => x.CompanyIndex == mCompanyIndex)
                                               join a in db.IC_GroupDevice.Where(x => x.CompanyIndex == mCompanyIndex)
                                               on i.GroupDeviceIndex equals a.Index
                                               select new
                                               {
                                                   GroupDeviceName = a.Name,
                                                   SerialNumber = i.SerialNumber
                                               }).ToList();


                            if (listDeviceUnSendEmail != null && listDeviceUnSendEmail.Count > 0)
                            {
                                //var listDeviceProcess = _dbContext.IC_SystemCommand.Where(x => x.CompanyIndex == mCompanyIndex && x.Excuted == false && x.ExcutingServiceIndex > 0)
                                //    .AsEnumerable().GroupBy(x => x.SerialNumber);
                                //var deviceProcessing = listDeviceProcess.ToDictionary(x => x.Key, x => x.ToList());
                                var listOfflinedDeviceIP = new List<string>();
                                for (int i = 0; i < listDeviceUnSendEmail.Count; i++)
                                {
                                    if (IsDeviceOfflined(listDeviceUnSendEmail[i], _cache, null))
                                    {
                                        var name = groupDevice.FirstOrDefault(x => x.SerialNumber == listDeviceUnSendEmail[i].SerialNumber);
                                        if (name != null)
                                        {
                                            var ipAndSerialDevice = $"IP: {listDeviceUnSendEmail[i].IPAddress}:{listDeviceUnSendEmail[i].Port} - SerialNumber: {listDeviceUnSendEmail[i].SerialNumber} - Site: {name.GroupDeviceName} \n";
                                            listOfflinedDeviceIP.Add(ipAndSerialDevice);
                                        }
                                        else
                                        {
                                            var ipAndSerialDevice = $"IP: {listDeviceUnSendEmail[i].IPAddress}:{listDeviceUnSendEmail[i].Port} - SerialNumber: {listDeviceUnSendEmail[i].SerialNumber} - Site: unknow\n";
                                            listOfflinedDeviceIP.Add(ipAndSerialDevice);
                                        }
                                    }
                                }
                                if (listOfflinedDeviceIP != null && listOfflinedDeviceIP.Count > 0)
                                {
                                    string title = config.TitleEmailError;
                                    string bodyEmail = config.BodyEmailError + Environment.NewLine + String.Join("", listOfflinedDeviceIP);
                                    _logger.LogInformation("Start Create Email When device offlined: " + title.ToString());
                                    //var isSuccess = _emailProvider.SendEmailToMulti("", title, bodyEmail, config.Email.Trim());

                                    foreach (var device in listDeviceUnSendEmail)
                                    {
                                        device.IsSendMailLastDisconnect = true;
                                    }

                                    var mailHistory = new IC_MailHistory();
                                    mailHistory.Title = title;
                                    mailHistory.Content = bodyEmail;
                                    mailHistory.CompanyIndex = mCompanyIndex;
                                    mailHistory.EmailTo = config.Email.Trim();
                                    mailHistory.CreatedBy = "CheckDeviceOffline";
                                    mailHistory.CreatedDate = DateTime.Now;
                                    mailHistory.Status = (short)MailHistoryType.Pending;
                                    mailHistory.Times = 0;
                                    db.IC_MailHistory.Add(mailHistory);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }

                scope.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateEmailWhenDeviceOffline:", ex);
            }
        }

        public void AutomaticSendMail()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();

            var listUnSendEmail = db.IC_MailHistory.Where(x => x.CompanyIndex == mCompanyIndex
                && (x.Status == (short)MailHistoryType.Pending || (x.Status == (short)MailHistoryType.Failed
                && x.Times < mTimesResendMail))).ToList();
            if (listUnSendEmail != null && listUnSendEmail.Count > 0)
            {
                foreach (var unSendEmail in listUnSendEmail)
                {
                    var isSuccess = _emailProvider.SendEmailToMulti("", unSendEmail.Title, unSendEmail.Content, unSendEmail.EmailTo);
                    ++unSendEmail.Times;
                    unSendEmail.Status = isSuccess ? (short)MailHistoryType.Sent : (short)MailHistoryType.Failed;
                }
                db.SaveChanges();
            }

            scope.Dispose();
        }

        private bool IsDeviceOfflined(IC_Device pDevice, IMemoryCache pCache, Dictionary<string, List<IC_SystemCommand>> pProcessingDevice)
        {
            return CaculateTime(pDevice.LastConnection, DateTime.Now) >= ConfigObject.GetConfig(pCache).LimitedTimeConnection;
        }

        private double CaculateTime(DateTime? time1, DateTime time2)
        {
            var temp = new DateTime();
            if (time1.HasValue)
            {
                temp = time1.Value;
            }
            else
            {
                temp = new DateTime(2000, 1, 1, 0, 0, 0);
            }
            var time = new TimeSpan();
            time = time2 - temp;
            return time.TotalMinutes;
        }

        public async Task AutoRemoveStoppedWorkingEmployeesData()
        {
            var now = DateTime.Now;
            string timePostCheck = DateTime.Now.ToHHmm();
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            List<IC_ConfigDTO> downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            if (downloadConfig != null)
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {
                    if (config.IntegrateLogParam.RemoveStoppedWorkingEmployeesType == (short)RemoveStoppedWorkingEmployeesType.Day
                        && config.IntegrateLogParam.RemoveStoppedWorkingEmployeesDay.HasValue)
                    {
                        var query = from e in _dbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == 2)
                                    join wi in _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == 2)
                                    on e.EmployeeATID equals wi.EmployeeATID into empWi
                                    from empWiResult in empWi.DefaultIfEmpty()
                                    select new
                                    {
                                        Employee = e,
                                        WorkingInfo = empWiResult
                                    };

                        var exceptEmpIDs = query.Where(x => !x.WorkingInfo.ToDate.HasValue
                            || (x.WorkingInfo.ToDate.HasValue
                            && (x.WorkingInfo.ToDate.Value.Date > now.Date
                            || (x.WorkingInfo.ToDate.Value.Date <= now.Date
                            && x.WorkingInfo.ToDate.Value.AddDays(config.IntegrateLogParam.RemoveStoppedWorkingEmployeesDay.Value).Date > now.Date))))?.Select(x => x.Employee.EmployeeATID).ToList();

                        query = query.Where(x => x.WorkingInfo.Status == (short)TransferStatus.Approve
                                    && x.WorkingInfo.FromDate.Date <= now.Date
                                    && x.WorkingInfo.ToDate.HasValue
                                    && x.WorkingInfo.ToDate.Value.Date < now.Date
                                    && x.WorkingInfo.ToDate.Value.AddDays(config.IntegrateLogParam.RemoveStoppedWorkingEmployeesDay.Value).Date <= now.Date);

                        if (query != null && query.Count() > 0)
                        {
                            var employeeList = await query.Select(x => x.Employee).ToListAsync();
                            if (exceptEmpIDs != null && exceptEmpIDs.Count > 0)
                            {
                                employeeList = employeeList.Where(x => !exceptEmpIDs.Contains(x.EmployeeATID)).ToList();
                            }
                            var employeeATIDList = employeeList.Select(x => x.EmployeeATID).ToHashSet();
                            var userList = await _dbContext.HR_User.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                            var cardNumberInfoList = await _dbContext.HR_CardNumberInfo.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                            var workingInfoList = await _dbContext.IC_WorkingInfo.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                            var employeeTransferList = await _dbContext.IC_EmployeeTransfer.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                            var userMasterList = await _dbContext.IC_UserMaster.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                            _dbContext.HR_EmployeeInfo.RemoveRange(employeeList);
                            if (userList != null && userList.Count > 0)
                                _dbContext.HR_User.RemoveRange(userList);
                            if (cardNumberInfoList != null && cardNumberInfoList.Count > 0)
                                _dbContext.HR_CardNumberInfo.RemoveRange(cardNumberInfoList);
                            if (workingInfoList != null && workingInfoList.Count > 0)
                                _dbContext.IC_WorkingInfo.RemoveRange(workingInfoList);
                            if (employeeTransferList != null && employeeTransferList.Count > 0)
                                _dbContext.IC_EmployeeTransfer.RemoveRange(employeeTransferList);
                            if (userMasterList != null && userMasterList.Count > 0)
                                _dbContext.IC_UserMaster.RemoveRange(userMasterList);
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                    else if (config.IntegrateLogParam.RemoveStoppedWorkingEmployeesType == (short)RemoveStoppedWorkingEmployeesType.Week)
                    {
                        if (config.IntegrateLogParam.RemoveStoppedWorkingEmployeesWeek == (int)now.DayOfWeek
                            && (!config.IntegrateLogParam.RemoveStoppedWorkingEmployeesTime.HasValue
                            || (config.IntegrateLogParam.RemoveStoppedWorkingEmployeesTime.HasValue
                            && config.IntegrateLogParam.RemoveStoppedWorkingEmployeesTime.Value.TimeOfDay <= now.TimeOfDay)))
                        {
                            var query = from e in _dbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == 2)
                                        join wi in _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == 2)
                                        on e.EmployeeATID equals wi.EmployeeATID into empWi
                                        from empWiResult in empWi.DefaultIfEmpty()
                                        select new
                                        {
                                            Employee = e,
                                            WorkingInfo = empWiResult
                                        };

                            var exceptEmpIDs = query.Where(x => !x.WorkingInfo.ToDate.HasValue
                                || (x.WorkingInfo.ToDate.HasValue && x.WorkingInfo.ToDate.Value.Date > now.Date))?.Select(x => x.Employee.EmployeeATID).ToHashSet();

                            query = query.Where(x => x.WorkingInfo.Status == (short)TransferStatus.Approve
                                    && x.WorkingInfo.FromDate.Date <= now.Date
                                    && x.WorkingInfo.ToDate.HasValue
                                    && x.WorkingInfo.ToDate.Value < now);
                            //&& now <= x.WorkingInfo.ToDate.Value.AddDays(6));

                            if (query != null && query.Count() > 0)
                            {
                                var employeeList = await query.Select(x => x.Employee).ToListAsync();
                                if (exceptEmpIDs != null && exceptEmpIDs.Count > 0)
                                {
                                    employeeList = employeeList.Where(x => !exceptEmpIDs.Contains(x.EmployeeATID)).ToList();
                                }
                                var employeeATIDList = employeeList.Select(x => x.EmployeeATID).ToHashSet();
                                var userList = await _dbContext.HR_User.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                                var cardNumberInfoList = await _dbContext.HR_CardNumberInfo.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                                var workingInfoList = await _dbContext.IC_WorkingInfo.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                                var employeeTransferList = await _dbContext.IC_EmployeeTransfer.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                                var userMasterList = await _dbContext.IC_UserMaster.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                                _dbContext.HR_EmployeeInfo.RemoveRange(employeeList);
                                if (userList != null && userList.Count > 0)
                                    _dbContext.HR_User.RemoveRange(userList);
                                if (cardNumberInfoList != null && cardNumberInfoList.Count > 0)
                                    _dbContext.HR_CardNumberInfo.RemoveRange(cardNumberInfoList);
                                if (workingInfoList != null && workingInfoList.Count > 0)
                                    _dbContext.IC_WorkingInfo.RemoveRange(workingInfoList);
                                if (employeeTransferList != null && employeeTransferList.Count > 0)
                                    _dbContext.IC_EmployeeTransfer.RemoveRange(employeeTransferList);
                                if (userMasterList != null && userMasterList.Count > 0)
                                    _dbContext.IC_UserMaster.RemoveRange(userMasterList);
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                    }
                    else if (config.IntegrateLogParam.RemoveStoppedWorkingEmployeesType == (short)RemoveStoppedWorkingEmployeesType.Month)
                    {
                        if (config.IntegrateLogParam.RemoveStoppedWorkingEmployeesMonth == now.Date.Day
                            && (!config.IntegrateLogParam.RemoveStoppedWorkingEmployeesTime.HasValue
                            || (config.IntegrateLogParam.RemoveStoppedWorkingEmployeesTime.HasValue
                            && config.IntegrateLogParam.RemoveStoppedWorkingEmployeesTime.Value.TimeOfDay <= now.TimeOfDay)))
                        {
                            var query = from e in _dbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == 2)
                                        join wi in _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == 2)
                                        on e.EmployeeATID equals wi.EmployeeATID into empWi
                                        from empWiResult in empWi.DefaultIfEmpty()
                                        select new
                                        {
                                            Employee = e,
                                            WorkingInfo = empWiResult
                                        };

                            var exceptEmpIDs = query.Where(x => !x.WorkingInfo.ToDate.HasValue
                                || (x.WorkingInfo.ToDate.HasValue && x.WorkingInfo.ToDate.Value.Date > now.Date))?.Select(x => x.Employee.EmployeeATID).ToList();

                            query = query.Where(x => x.WorkingInfo.Status == (short)TransferStatus.Approve
                                && x.WorkingInfo.FromDate.Date <= now.Date
                                && x.WorkingInfo.ToDate.HasValue
                                && x.WorkingInfo.ToDate.Value < now);
                            //&& ((x.WorkingInfo.ToDate.Value.Day <= now.Day && x.WorkingInfo.ToDate.Value.Month <= now.Month 
                            //&& x.WorkingInfo.ToDate.Value.Year <= now.Year)
                            //|| (x.WorkingInfo.ToDate.Value.Day > now.Day && x.WorkingInfo.ToDate.Value.AddMonths(1).Month == now.Month 
                            //&& x.WorkingInfo.ToDate.Value.Year <= now.Year)));

                            if (query != null && query.Count() > 0)
                            {
                                var employeeList = await query.Select(x => x.Employee).ToListAsync();
                                if (exceptEmpIDs != null && exceptEmpIDs.Count > 0)
                                {
                                    employeeList = employeeList.Where(x => !exceptEmpIDs.Contains(x.EmployeeATID)).ToList();
                                }
                                var employeeATIDList = employeeList.Select(x => x.EmployeeATID).ToHashSet();
                                var userList = await _dbContext.HR_User.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                                var cardNumberInfoList = await _dbContext.HR_CardNumberInfo.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                                var workingInfoList = await _dbContext.IC_WorkingInfo.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                                var employeeTransferList = await _dbContext.IC_EmployeeTransfer.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                                var userMasterList = await _dbContext.IC_UserMaster.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
                                _dbContext.HR_EmployeeInfo.RemoveRange(employeeList);
                                if (userList != null && userList.Count > 0)
                                    _dbContext.HR_User.RemoveRange(userList);
                                if (cardNumberInfoList != null && cardNumberInfoList.Count > 0)
                                    _dbContext.HR_CardNumberInfo.RemoveRange(cardNumberInfoList);
                                if (workingInfoList != null && workingInfoList.Count > 0)
                                    _dbContext.IC_WorkingInfo.RemoveRange(workingInfoList);
                                if (employeeTransferList != null && employeeTransferList.Count > 0)
                                    _dbContext.IC_EmployeeTransfer.RemoveRange(employeeTransferList);
                                if (userMasterList != null && userMasterList.Count > 0)
                                    _dbContext.IC_UserMaster.RemoveRange(userMasterList);
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                    }
                }
            }
        }

        public void AddOrDeleteUser(IC_ConfigDTO config)
        {
            var addedParams = new List<AddedParam>();
            ConfigObject configFile = ConfigObject.GetConfig(_cache);
            List<CommandResult> lstCmd = new List<CommandResult>();
            var lstUploadUsersTmp = new List<IC_UploadUser>();
            var lstDeleteTmp = new List<IC_UploadUser>();
            //string externalData = "";
            if (configFile.IntegrateDBOther == false)
            {
                // transfer to new deparment 
                List<IC_EmployeeTransfer> lstTransfer = _dbContext.IC_EmployeeTransfer.Where(x => x.CompanyIndex == config.CompanyIndex && x.FromTime.Date == DateTime.Now.Date && x.IsSync == null && x.Status == (long)TransferStatus.Approve).ToList();
                var employeeInDepartment = _dbContext.AC_DepartmentAccessedGroup.Where(x => lstTransfer.Select(x => x.NewDepartment).Contains(x.DepartmentIndex)).ToList();

                foreach (IC_EmployeeTransfer emp in lstTransfer)
                {
                    if (emp.RemoveFromOldDepartment)
                    {
                        List<string> lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == config.CompanyIndex && x.DepartmentIndex == emp.OldDepartment).Select(x => x.SerialNumber).ToList();
                        if (config.IntegrateLogParam.IntegrateWhenNotInclareDepartment == true && lstSerial.Count == 0)
                        {
                            var serialNumbers = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == config.CompanyIndex).Select(x => x.SerialNumber).ToList();
                            lstSerial = _dbContext.IC_Device.Where(x => !serialNumbers.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                        }

                        List<string> lsSerialApp1 = new List<string>();
                        ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp1);
                        List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                        if (lsSerialApp1.Count > 0)
                        {
                            IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                            paramUserOnMachine.ListEmployeeaATID = new List<string>() { emp.EmployeeATID };
                            paramUserOnMachine.CompanyIndex = config.CompanyIndex;
                            paramUserOnMachine.ListSerialNumber = lsSerialApp1;
                            paramUserOnMachine.AuthenMode = "";
                            paramUserOnMachine.FullInfo = true;
                            lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                        }
                        if (lstUser.Count > 0)
                        {
                            IC_CommandParamDTO commandParam2 = new IC_CommandParamDTO();
                            commandParam2.ListSerialNumber = lsSerialApp1;
                            commandParam2.Action = CommandAction.DeleteUserById;
                            commandParam2.FromTime = DateTime.Now;
                            commandParam2.ToTime = DateTime.Now;
                            commandParam2.ListEmployee = lstUser;
                            commandParam2.IsOverwriteData = false;
                            commandParam2.ExternalData = emp.EmployeeATID;
                            List<CommandResult> lstCmdRemoveUser = _iC_CommandLogic.CreateListCommands(commandParam2);
                            lstCmd.AddRange(lstCmdRemoveUser);
                        }
                    }
                    if (emp.AddOnNewDepartment)
                    {
                        List<string> lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == config.CompanyIndex && x.DepartmentIndex == emp.NewDepartment).Select(x => x.SerialNumber).ToList();
                        if (config.IntegrateLogParam.IntegrateWhenNotInclareDepartment == true && lstSerial.Count == 0)
                        {
                            var serialNumbers = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == config.CompanyIndex).Select(x => x.SerialNumber).ToList();
                            lstSerial = _dbContext.IC_Device.Where(x => !serialNumbers.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                        }
                        List<string> lsSerialApp1 = new List<string>();
                        ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp1);
                        List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                        if (lsSerialApp1.Count > 0)
                        {
                            IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                            paramUserOnMachine.ListEmployeeaATID = new List<string>() { emp.EmployeeATID };
                            paramUserOnMachine.CompanyIndex = config.CompanyIndex;
                            paramUserOnMachine.ListSerialNumber = lsSerialApp1;
                            paramUserOnMachine.AuthenMode = "";
                            paramUserOnMachine.FullInfo = true;
                            lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                        }

                        if (lstUser.Count > 0)
                        {
                            IC_CommandParamDTO commandParam1 = new IC_CommandParamDTO();
                            commandParam1.ListSerialNumber = lsSerialApp1;
                            commandParam1.Action = CommandAction.UploadUsers;
                            commandParam1.FromTime = DateTime.Now;
                            commandParam1.ToTime = DateTime.Now;
                            commandParam1.ListEmployee = lstUser;
                            commandParam1.IsOverwriteData = false;
                            commandParam1.ExternalData = emp.EmployeeATID;
                            List<CommandResult> lstCmdAddUser = _iC_CommandLogic.CreateListCommands(commandParam1);
                            lstCmd.AddRange(lstCmdAddUser);
                        }
                    }
                    //transfer device AC
                    if (employeeInDepartment != null && employeeInDepartment.Count > 0)
                    {
                        //var employeeAccessGr = employeeInfoList.Where(x => employeeInDepartment.Select(z => z.DepartmentIndex).Contains(x.DepartmentIndex)).ToList();
                        foreach (var departmentAcc in employeeInDepartment)
                        {
                            var listUserAcc = lstTransfer.Where(x => x.NewDepartment == departmentAcc.DepartmentIndex).Select(x => x.EmployeeATID).ToList();
                            _iC_CommandLogic.UploadTimeZone(departmentAcc.GroupIndex, config.CompanyIndex);
                            _iC_CommandLogic.UploadUsers(departmentAcc.GroupIndex, listUserAcc, config.CompanyIndex);
                            _iC_CommandLogic.UploadACUsers(departmentAcc.GroupIndex, listUserAcc, config.CompanyIndex);
                        }
                    }

                    emp.IsSync = false;
                    emp.UpdatedDate = DateTime.Now;
                    emp.UpdatedUser = UpdatedUser.SYSTEM_AUTO.ToString();
                    _dbContext.SaveChanges();
                }

                // transfer back
                List<IC_EmployeeTransfer> lstTransferBack = _dbContext.IC_EmployeeTransfer.Where(x => x.CompanyIndex == config.CompanyIndex && x.ToTime.Date == DateTime.Now.Date && x.Status == (long)TransferStatus.Approve).ToList();
                var lstEmployeeATID = lstTransferBack.Select(x => x.EmployeeATID);
                var lstTransfers = _dbContext.IC_EmployeeTransfer.Where(x => x.CompanyIndex == config.CompanyIndex && x.Status == (long)TransferStatus.Approve && lstEmployeeATID.Contains(x.EmployeeATID)).ToList();
                foreach (IC_EmployeeTransfer emp in lstTransferBack)
                {
                    var checkEmployee = lstTransfers.FirstOrDefault(x => x.EmployeeATID == emp.EmployeeATID && emp.NewDepartment == x.NewDepartment && x.FromTime.Date == emp.ToTime.Date);
                    if (checkEmployee != null)
                    {
                        continue;
                    }
                    if (emp.AddOnNewDepartment)
                    {
                        List<string> lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == config.CompanyIndex && x.DepartmentIndex == emp.NewDepartment).Select(x => x.SerialNumber).ToList();
                        if (config.IntegrateLogParam.IntegrateWhenNotInclareDepartment == true && lstSerial.Count == 0)
                        {
                            var serialNumbers = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == config.CompanyIndex).Select(x => x.SerialNumber).ToList();
                            lstSerial = _dbContext.IC_Device.Where(x => !serialNumbers.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                        }
                        //Check licence
                        List<string> lsSerialApp1 = new List<string>();
                        ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp1);

                        List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                        if (lstSerial.Count > 0)
                        {
                            IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                            paramUserOnMachine.ListEmployeeaATID = new List<string>() { emp.EmployeeATID };
                            paramUserOnMachine.CompanyIndex = config.CompanyIndex;
                            paramUserOnMachine.ListSerialNumber = lsSerialApp1;
                            paramUserOnMachine.AuthenMode = "";
                            paramUserOnMachine.FullInfo = true;
                            lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                        }
                        IC_CommandParamDTO commandParam3 = new IC_CommandParamDTO();
                        commandParam3.ListSerialNumber = lsSerialApp1;
                        commandParam3.Action = CommandAction.DeleteUserById;
                        commandParam3.FromTime = DateTime.Now;
                        commandParam3.ToTime = DateTime.Now;
                        commandParam3.ListEmployee = lstUser;
                        commandParam3.IsOverwriteData = false;
                        List<CommandResult> lstCmdRemoveUser = _iC_CommandLogic.CreateListCommands(commandParam3);
                        lstCmd.AddRange(lstCmdRemoveUser);
                    }
                    if (emp.RemoveFromOldDepartment)
                    {
                        List<string> lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == config.CompanyIndex && x.DepartmentIndex == emp.OldDepartment).Select(x => x.SerialNumber).ToList();
                        if (config.IntegrateLogParam.IntegrateWhenNotInclareDepartment == true && lstSerial.Count == 0)
                        {
                            var serialNumbers = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == config.CompanyIndex).Select(x => x.SerialNumber).ToList();
                            lstSerial = _dbContext.IC_Device.Where(x => !serialNumbers.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                        }
                        List<string> lsSerialApp1 = new List<string>();
                        ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp1);
                        List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                        if (lsSerialApp1.Count > 0)
                        {
                            IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                            paramUserOnMachine.ListEmployeeaATID = new List<string>() { emp.EmployeeATID };
                            paramUserOnMachine.CompanyIndex = config.CompanyIndex;
                            paramUserOnMachine.ListSerialNumber = lsSerialApp1;
                            paramUserOnMachine.AuthenMode = "";
                            paramUserOnMachine.FullInfo = true;
                            lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                        }
                        IC_CommandParamDTO commandParam4 = new IC_CommandParamDTO();
                        commandParam4.ListSerialNumber = lsSerialApp1;
                        commandParam4.Action = CommandAction.UploadUsers;
                        commandParam4.FromTime = DateTime.Now;
                        commandParam4.ToTime = DateTime.Now;
                        commandParam4.ListEmployee = lstUser;
                        commandParam4.IsOverwriteData = false;
                        List<CommandResult> lstCmdAddUser = _iC_CommandLogic.CreateListCommands(commandParam4);
                        lstCmd.AddRange(lstCmdAddUser);
                    }
                }

                var isMondelez = ClientName.MONDELEZ.ToString() == _configClientName;

                // working info
                var listEmployeeATID = (from empw in _dbContext.IC_WorkingInfo
                                        join emp in _dbContext.HR_User
                                         on empw.EmployeeATID equals emp.EmployeeATID into emphw
                                        from empInfo in emphw.DefaultIfEmpty()
                                        where empw.IsSync == null && empInfo.CompanyIndex == config.CompanyIndex
                                        && (!isMondelez || (empw.DepartmentIndex != 0))
                                        select new IC_WorkingInfo { EmployeeATID = empInfo.EmployeeATID }).Select(x => x.EmployeeATID).Distinct().ToList();


                var listWorkingInfoAll = _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == config.CompanyIndex
                                  && x.FromDate.Date <= DateTime.Now && (!x.ToDate.HasValue || x.ToDate > DateTime.Now.Date)
                                   && (!isMondelez || (x.DepartmentIndex != 0))
                                  && listEmployeeATID.Contains(x.EmployeeATID) && x.Status == (long)TransferStatus.Approve).OrderBy(t => t.FromDate).ToList();

                var listDepartmentIndex = listWorkingInfoAll.Select(x => x.DepartmentIndex).ToHashSet();
                var listDeviceAndSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == config.CompanyIndex && listDepartmentIndex.Contains(x.DepartmentIndex))
                    .Select(x => new { DeviceSerial = x.SerialNumber, DepartmentIndex = x.DepartmentIndex }).ToList();
                var listAllSerial = _dbContext.IC_Device.Select(e => e.SerialNumber).ToList();
                var listSerial = new List<string>();
                if (listDeviceAndSerial != null && listDeviceAndSerial.Count > 0)
                {
                    listSerial = listDeviceAndSerial.Select(x => x.DeviceSerial).Distinct().ToList();
                }

                List<string> lsSerialApp = new List<string>();
                ListSerialCheckHardWareLicense(listSerial, ref lsSerialApp);

                listSerial = lsSerialApp;
                //else if (listSerial == null || listSerial.Count() == 0)
                //{
                //    listSerial = _dbContext.IC_Device.Select(e => e.SerialNumber).Distinct().ToList();
                //}
                var listSerialDefault = config.IntegrateLogParam.ListSerialNumber.Split(';').ToList();
                var listUser = new List<UserInfoOnMachine>();
                lsSerialApp.AddRange(listSerialDefault);
                if (lsSerialApp.Count > 0)
                {
                    IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = listEmployeeATID;
                    paramUserOnMachine.CompanyIndex = config.CompanyIndex;
                    paramUserOnMachine.ListSerialNumber = lsSerialApp;
                    paramUserOnMachine.AuthenMode = "";
                    paramUserOnMachine.FullInfo = true;
                    listUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                }

                if (listUser.Count > 0)
                {
                    var listServiceAndDevice = _iC_CommandLogic.GetServiceType(listSerial);
                    var serialNumbers = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == config.CompanyIndex).Select(x => x.SerialNumber).ToList();
                    var listSerialIgnore = _dbContext.IC_Device.Where(x => !serialNumbers.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();


                    for (int i = 0; i < listEmployeeATID.Count; i++)
                    {
                        List<IC_WorkingInfo> listWorkingByEmp = listWorkingInfoAll.FindAll(t => t.EmployeeATID == listEmployeeATID[i]).OrderBy(t => t.FromDate).ToList();
                        if (listWorkingByEmp.Count == 0) continue;
                        string indexWorking = "";
                        if (listWorkingByEmp.Count == 1)
                        {
                            // if sync column is null --> create command
                            if (listWorkingByEmp[0].IsSync == null && listWorkingByEmp[0].FromDate.Date <= DateTime.Now.Date)
                            {
                                indexWorking = listWorkingByEmp[0].Index.ToString();
                                var commandListSerial = new List<string>();
                                var deviceAndSerial = listDeviceAndSerial.Where(x => x.DepartmentIndex == listWorkingByEmp[0].DepartmentIndex).ToList();

                                if (config.IntegrateLogParam.IntegrateWhenNotInclareDepartment == true && (deviceAndSerial == null || deviceAndSerial.Count() == 0))
                                {
                                    commandListSerial = listSerialIgnore;
                                    if (commandListSerial == null || commandListSerial.Count() == 0)
                                    {
                                        continue;
                                    }

                                }
                                else
                                {
                                    if (deviceAndSerial != null && deviceAndSerial.Count > 0 || (listSerialDefault != null && listSerialDefault.Count > 0))
                                    {
                                        commandListSerial = deviceAndSerial.Select(x => x.DeviceSerial).ToList();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                commandListSerial.AddRange(listSerialDefault);
                                ListSerialCheckHardWareLicense(commandListSerial, ref lsSerialApp);
                                commandListSerial = lsSerialApp;

                                if (commandListSerial.Count > 0)
                                {
                                    var listUpload = listServiceAndDevice.Where(x => commandListSerial.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                                    if (listUpload.Count > 0)
                                    {
                                        lstUploadUsersTmp.AddRange(listUpload.Select(x => new IC_UploadUser { EmployeeATID = listWorkingByEmp[0].EmployeeATID, CompanyIndex = listWorkingByEmp[0].CompanyIndex, SerialNumber = x }).ToList());
                                        listWorkingByEmp[0].IsSync = false;
                                        _dbContext.IC_WorkingInfo.Update(listWorkingByEmp[0]);
                                    }
                                }
                                //var listCmdTemp = CreateAutoUploadUserCommand(listWorkingByEmp[0].CompanyIndex, listWorkingByEmp[0].DepartmentIndex,
                                //    listWorkingByEmp[0].EmployeeATID, DateTime.Now, DateTime.Now, indexWorking, false, commandListSerial, listUser, listServiceAndDevice);

                                //if (listCmdTemp.Count > 0)
                                //{
                                //    listWorkingByEmp[0].IsSync = false;
                                //    _dbContext.IC_WorkingInfo.Update(listWorkingByEmp[0]);
                                //    lstCmd.AddRange(listCmdTemp);
                                //}
                            }
                        }
                        else
                        {
                            // điều chuyển với working có fromdate <= ngày hiện tại, chỉ điều chuyển dòng có fromdate lớn nhất
                            IC_WorkingInfo workingNeedTransfer = listWorkingByEmp.FindAll(t => t.FromDate.Date <= DateTime.Now.Date)
                                .OrderByDescending(t => t.FromDate).FirstOrDefault();

                            if (workingNeedTransfer != null && workingNeedTransfer.IsSync == null)
                            {
                                long newDepartment = workingNeedTransfer.DepartmentIndex;
                                //tìm phòng ban cũ 
                                IC_WorkingInfo workingOld = listWorkingByEmp.FindAll(t => t.FromDate.Date <= workingNeedTransfer.FromDate.Date
                                     && t.Index != workingNeedTransfer.Index).OrderByDescending(t => t.FromDate).FirstOrDefault();

                                long oldDepartment = 0;
                                if (workingOld != null)
                                {
                                    oldDepartment = workingOld.DepartmentIndex;
                                }
                                // nếu phòng ban cũ = phòng ban mới --> finish
                                if (newDepartment == oldDepartment)
                                {
                                    workingNeedTransfer.IsSync = true;
                                    _dbContext.IC_WorkingInfo.Update(workingNeedTransfer);
                                    continue;
                                }
                                List<CommandResult> listCmdTemp = new List<CommandResult>();
                                //xóa phòng ban cũ
                                if (workingOld != null)
                                {
                                    var deleteCommandListSerial = new List<string>();
                                    var deleteCommandListDeviceAndSerial = listDeviceAndSerial.Where(x => x.DepartmentIndex == workingOld.DepartmentIndex).ToList();
                                    if (deleteCommandListDeviceAndSerial != null && deleteCommandListDeviceAndSerial.Count > 0)
                                    {
                                        deleteCommandListSerial = deleteCommandListDeviceAndSerial.Select(x => x.DeviceSerial).ToList();
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    ListSerialCheckHardWareLicense(deleteCommandListSerial, ref lsSerialApp);
                                    deleteCommandListSerial = lsSerialApp;
                                    deleteCommandListSerial.AddRange(listSerialDefault);
                                    if (deleteCommandListSerial.Count > 0)
                                    {
                                        var listUpload = listServiceAndDevice.Where(x => deleteCommandListSerial.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                                        if (listUpload.Count > 0)
                                        {
                                            lstDeleteTmp.AddRange(listUpload.Select(x => new IC_UploadUser { EmployeeATID = workingOld.EmployeeATID, CompanyIndex = workingNeedTransfer.CompanyIndex, SerialNumber = x }).ToList());
                                        }
                                    }

                                    //listCmdTemp.AddRange(CreateAutoDeleteUserCommand(workingNeedTransfer.CompanyIndex, workingOld.DepartmentIndex,
                                    //    workingOld.EmployeeATID, DateTime.Now, DateTime.Now, indexWorking, deleteCommandListSerial, listUser, listServiceAndDevice));

                                }
                                // thêm lên phòng ban mới
                                indexWorking = workingNeedTransfer.Index.ToString();
                                var commandListSerial = new List<string>();
                                var deviceAndSerial = listDeviceAndSerial.Where(x => x.DepartmentIndex == workingNeedTransfer.DepartmentIndex).ToList();
                                if (config.IntegrateLogParam.IntegrateWhenNotInclareDepartment == true && (deviceAndSerial == null || deviceAndSerial.Count() == 0))
                                {
                                    commandListSerial = listSerialIgnore;
                                    ListSerialCheckHardWareLicense(commandListSerial, ref lsSerialApp);
                                    commandListSerial = lsSerialApp;
                                    if (commandListSerial == null || commandListSerial.Count() == 0)
                                    {
                                        continue;
                                    }

                                }
                                else
                                {
                                    if (deviceAndSerial != null && deviceAndSerial.Count > 0 || (listSerialDefault != null && listSerialDefault.Count > 0))
                                    {
                                        commandListSerial = deviceAndSerial.Select(x => x.DeviceSerial).ToList();
                                        ListSerialCheckHardWareLicense(commandListSerial, ref lsSerialApp);
                                        commandListSerial = lsSerialApp;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                commandListSerial.AddRange(listSerialDefault);
                                if (commandListSerial.Count > 0)
                                {
                                    var listUpload = listServiceAndDevice.Where(x => commandListSerial.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                                    if (listUpload.Count > 0)
                                    {
                                        lstUploadUsersTmp.AddRange(listUpload.Select(x => new IC_UploadUser { EmployeeATID = workingNeedTransfer.EmployeeATID, CompanyIndex = workingNeedTransfer.CompanyIndex, SerialNumber = x }).ToList());
                                        workingNeedTransfer.IsSync = false;
                                        _dbContext.IC_WorkingInfo.Update(workingNeedTransfer);
                                    }
                                }

                                //listCmdTemp.AddRange(CreateAutoUploadUserCommand(workingNeedTransfer.CompanyIndex, workingNeedTransfer.DepartmentIndex,
                                //    workingNeedTransfer.EmployeeATID, DateTime.Now, DateTime.Now, indexWorking, false, commandListSerial, listUser, listServiceAndDevice));


                                //if (listCmdTemp.Count > 0)
                                //{
                                //    lstCmd.AddRange(listCmdTemp);
                                //}
                            }
                        }
                    }

                    if (lstUploadUsersTmp != null && lstUploadUsersTmp.Count > 0)
                    {
                        var groups = lstUploadUsersTmp.GroupBy(x => x.SerialNumber).Select(x => new { Key = x.Key, Value = x }).ToList();

                        foreach (var item in groups)
                        {
                            var tmps = CreateAutoUploadUserCommandNew(item.Value.Select(x => x.EmployeeATID).ToList(), DateTime.Now, DateTime.Now, null, false, new List<string> { item.Key }, listUser, listServiceAndDevice);
                            lstCmd.AddRange(tmps);
                        }
                    }

                    if (lstDeleteTmp != null && lstDeleteTmp.Count > 0)
                    {
                        var groups = lstDeleteTmp.Where(x => !lstUploadUsersTmp.Any(y => y.EmployeeATID == x.EmployeeATID && x.SerialNumber == y.EmployeeATID)).GroupBy(x => x.SerialNumber).Select(x => new { Key = x.Key, Value = x }).ToList();

                        foreach (var item in groups)
                        {
                            var tmps = CreateAutoDeleteUserCommandNew(item.Value.Select(x => x.EmployeeATID).ToList(), DateTime.Now, DateTime.Now, null, new List<string> { item.Key }, listUser, listServiceAndDevice);
                            lstCmd.AddRange(tmps);
                        }
                    }

                }

            }
            else
            {
                addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex });
                var listEmployee = _hR_EmployeeLogic.GetMany(addedParams);
                var listEmpATIDs = listEmployee.Select(u => u.EmployeeATID).ToList();

                List<HR_WorkingInfo> listWorking = _dbEzHRContext.HR_WorkingInfo.Where(t => t.CompanyIndex == _config.CompanyIndex
                    && listEmpATIDs.Contains(t.EmployeeATID)).ToList();

                for (int i = 0; i < listEmpATIDs.Count; i++)
                {
                    List<HR_WorkingInfo> listWorkingByEmp = listWorking.FindAll(t => t.EmployeeATID == listEmpATIDs[i]).OrderBy(t => t.FromDate).ToList();
                    if (listWorkingByEmp.Count == 0) continue;
                    string indexWorking = "";

                    if (listWorkingByEmp.Count == 1)
                    {
                        // if sync column is null --> create command
                        if (listWorkingByEmp[0].Synched == null && listWorkingByEmp[0].FromDate.Value.Date <= DateTime.Now.Date)
                        {
                            indexWorking = listWorkingByEmp[0].Index.ToString();
                            List<CommandResult> listCmdTemp = CreateUploadUserCommand(config.CompanyIndex, listWorkingByEmp[0].DepartmentIndex.Value,
                                listWorkingByEmp[0].EmployeeATID, DateTime.Now, DateTime.Now, indexWorking, false);

                            if (listCmdTemp.Count > 0)
                            {
                                listWorkingByEmp[0].Synched = 0;
                                lstCmd.AddRange(listCmdTemp);
                            }
                        }
                    }
                    else
                    {
                        // điều chuyển với working có fromdate <= ngày hiện tại, chỉ điều chuyển dòng có fromdate lớn nhất
                        HR_WorkingInfo workingNeedTransfer = listWorkingByEmp.FindAll(t => t.FromDate.Value.Date <= DateTime.Now.Date).OrderByDescending(t => t.FromDate)
                            .FirstOrDefault();

                        if (workingNeedTransfer != null && workingNeedTransfer.Synched == null)
                        {
                            long newDepartment = workingNeedTransfer.DepartmentIndex.Value;
                            //tìm phòng ban cũ 
                            HR_WorkingInfo workingOld = listWorkingByEmp.FindAll(t => t.FromDate.Value.Date <= workingNeedTransfer.FromDate.Value.Date
                                 && t.Index != workingNeedTransfer.Index).OrderByDescending(t => t.FromDate).FirstOrDefault();

                            long oldDepartment = 0;
                            if (workingOld != null)
                            {
                                oldDepartment = workingOld.DepartmentIndex.Value;
                            }
                            // nếu phòng ban cũ = phòng ban mới --> finish
                            if (newDepartment == oldDepartment)
                            {
                                workingNeedTransfer.Synched = 1;
                                continue;
                            }
                            List<CommandResult> listCmdTemp = new List<CommandResult>();
                            //xóa phòng ban cũ
                            if (workingOld != null)
                            {
                                listCmdTemp.AddRange(CreateDeleteUserCommand(config.CompanyIndex, workingOld.DepartmentIndex.Value,
                                    workingOld.EmployeeATID, DateTime.Now, DateTime.Now, indexWorking));
                            }
                            // thêm lên phòng ban mới
                            indexWorking = workingNeedTransfer.Index.ToString();
                            listCmdTemp.AddRange(CreateUploadUserCommand(config.CompanyIndex, workingNeedTransfer.DepartmentIndex.Value,
                                workingNeedTransfer.EmployeeATID, DateTime.Now, DateTime.Now, indexWorking, false));
                            workingNeedTransfer.Synched = 0;

                            if (listCmdTemp.Count > 0)
                            {
                                lstCmd.AddRange(listCmdTemp);
                            }
                        }
                    }
                }
            }


            if (lstCmd != null && lstCmd.Count > 0)
            {
                IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                grouComParam.CompanyIndex = config.CompanyIndex;
                grouComParam.ListCommand = lstCmd;
                grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                grouComParam.GroupName = GroupName.TransferEmployee.ToString();
                grouComParam.EventType = config.EventType;
                _iC_CommandLogic.CreateGroupCommands(grouComParam);
            }
            // tạo command xóa nv nghỉ việc
            lstCmd = new List<CommandResult>();
            lstCmd = CreateCommandDeleteEmployeeStopped(DateTime.Now, config);
            if (lstCmd != null && lstCmd.Count > 0)
            {
                IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                grouComParam.CompanyIndex = config.CompanyIndex;
                grouComParam.ListCommand = lstCmd;
                grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                grouComParam.GroupName = GroupName.DeleteEmployeeStopped.ToString();
                grouComParam.EventType = config.EventType;
                _iC_CommandLogic.CreateGroupCommands(grouComParam);
            }
            _dbEzHRContext.SaveChanges();
            _dbContext.SaveChanges();
        }

        public class IC_UploadUser
        {
            public string EmployeeATID { get; set; }
            public string SerialNumber { get; set; }
            public int CompanyIndex { get; set; }
        }

        public void AutoAddOrDeleteUser(DateTime now)
        {
            string timePostCheck = now.ToHHmm();
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER.ToString() });

            List<IC_ConfigDTO> downloadConfig = _iC_ConfigLogic.GetMany(addedParams).Result;
            if (downloadConfig != null)
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {
                    if (!config.TimePos.Contains(timePostCheck)) { return; }
                    else
                    {
                        AddOrDeleteUser(config);
                    }
                }
            }
        }

        public async Task AutoAddOrDeleteUserManual()
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER.ToString() });

            List<IC_ConfigDTO> downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            if (downloadConfig != null)
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {
                    if (config.TimePos.Count() == 0) { return; }
                    else
                    {
                        AddOrDeleteUser(config);
                    }
                }
            }
        }

        public void AutoAddOrDeleteCustomer(DateTime now)
        {
            string timePostCheck = now.ToHHmm();

            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER.ToString() });

            List<IC_ConfigDTO> downloadConfig = _iC_ConfigLogic.GetMany(addedParams).Result;
            if (downloadConfig != null)
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {
                    if (!config.TimePos.Contains(timePostCheck)) { return; }
                    else
                    {
                        Dictionary<string, List<UserInfoOnMachine>> dicListUserByDevice = new Dictionary<string, List<UserInfoOnMachine>>();
                        ConfigObject configFile = ConfigObject.GetConfig(_cache);
                        List<CommandResult> lstCmd = new List<CommandResult>();
                        //string externalData = "";
                        if (configFile.IntegrateDBOther == false)
                        {
                            var lstSerial = _dbContext.IC_Device.Where(t => t.CompanyIndex == config.CompanyIndex).Select(t => t.SerialNumber).ToList();

                            var lsSerialApp = new List<string>();
                            ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp);

                            var listCustomer = _dbContext.HR_CustomerInfo.Where(e => e.ToTime.Date == DateTime.Now.Date && e.CompanyIndex == config.CompanyIndex).Select(e => e.EmployeeATID).ToHashSet();
                            foreach (var customer in listCustomer)
                            {
                                var lstUser = new List<UserInfoOnMachine>();
                                if (lstSerial.Count > 0)
                                {
                                    var paramUserOnMachine = new IC_UserinfoOnMachineParam();
                                    paramUserOnMachine.ListEmployeeaATID = new List<string>() { customer };
                                    paramUserOnMachine.CompanyIndex = config.CompanyIndex;
                                    paramUserOnMachine.ListSerialNumber = lsSerialApp;
                                    paramUserOnMachine.AuthenMode = "";
                                    paramUserOnMachine.FullInfo = false;
                                    lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                                }

                                for (int i = 0; i < lsSerialApp.Count; i++)
                                {
                                    if (dicListUserByDevice.ContainsKey(lsSerialApp[i]) == false)
                                    {
                                        dicListUserByDevice.Add(lsSerialApp[i], lstUser);
                                    }
                                    else
                                    {
                                        dicListUserByDevice[lsSerialApp[i]].AddRange(lstUser);
                                    }
                                }
                            }

                            for (int i = 0; i < dicListUserByDevice.Count; i++)
                            {
                                var commandParam = new IC_CommandParamDTO();
                                commandParam.ListSerialNumber = new List<string>() { dicListUserByDevice.ElementAt(i).Key };
                                commandParam.ListEmployee = dicListUserByDevice[dicListUserByDevice.ElementAt(i).Key];
                                commandParam.Action = CommandAction.DeleteUserById;
                                commandParam.FromTime = DateTime.Now;
                                commandParam.ToTime = DateTime.Now;
                                commandParam.ExternalData = "";
                                commandParam.IsOverwriteData = false;
                                List<CommandResult> listCmdTemp = _iC_CommandLogic.CreateListCommands(commandParam);
                                lstCmd.AddRange(listCmdTemp);
                            }
                        }

                        if (lstCmd != null && lstCmd.Count > 0)
                        {
                            IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                            grouComParam.CompanyIndex = config.CompanyIndex;
                            grouComParam.ListCommand = lstCmd;
                            grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                            grouComParam.GroupName = GroupName.DeleteCustomer.ToString(); ;
                            grouComParam.EventType = config.EventType;
                            _iC_CommandLogic.CreateGroupCommands(grouComParam);
                        }
                        _dbEzHRContext.SaveChanges();
                        _dbContext.SaveChanges();
                    }
                }
            }
        }

        public async Task AutoSyncDeviceTime()
        {
            string timePostCheck = DateTime.Now.ToHHmm();
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.TIME_SYNC.ToString() });
            List<IC_ConfigDTO> downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            if (downloadConfig != null)
            {
                try
                {
                    var config = downloadConfig.FirstOrDefault();
                    if (config != null)
                    {
                        if (!config.TimePos.Contains(timePostCheck)) { return; }
                        else
                        {
                            List<CommandResult> lstCmd = new List<CommandResult>();

                            List<IC_Service> lstService = _dbContext.IC_Service.Where(x => x.CompanyIndex == config.CompanyIndex).ToList();
                            List<string> listDeviceSerial = _dbContext.IC_Device.Select(x => x.SerialNumber).ToList();

                            foreach (IC_Service service in lstService)
                            {
                                List<string> lsSerialHw = new List<string>();

                                List<string> lstSerial = _dbContext.IC_ServiceAndDevices.Where(x => x.ServiceIndex == service.Index
                                    && x.CompanyIndex == config.CompanyIndex && listDeviceSerial.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                                ListSerialCheckHardWareLicense(lstSerial, ref lsSerialHw);

                                IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                                commandParam.ListSerialNumber = lsSerialHw;
                                commandParam.Action = CommandAction.SetTimeDevice;
                                commandParam.FromTime = DateTime.Now;
                                commandParam.ToTime = DateTime.Now;
                                commandParam.ExternalData = "";
                                commandParam.IsOverwriteData = false;

                                List<CommandResult> listCmdByService = _iC_CommandLogic.CreateListCommands(commandParam);
                                lstCmd.AddRange(listCmdByService);
                            }

                            // Call add to cache here
                            IC_GroupCommandParamDTO groupComParam = new IC_GroupCommandParamDTO();
                            groupComParam.CompanyIndex = config.CompanyIndex;
                            groupComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                            groupComParam.GroupName = GroupName.SetTimeDevice.ToString();
                            groupComParam.ListCommand = lstCmd;
                            groupComParam.EventType = config.EventType;
                            _iC_CommandLogic.CreateGroupCommands(groupComParam);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error Sync Device log: " + ex.Message.ToString());
                }
            }
        }

        public async Task AutoSyncEmployeeToDatabase1Office(IC_ConfigDTO config)
        {
            if (!string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
            {
                var client = new HttpClient();
                var responseMessageGetData = await client.GetAsync(config.IntegrateLogParam.LinkAPIIntegrate + "/api/personnel/profile/gets?access_token=" + config.IntegrateLogParam.Token + "&limit=1000");
                responseMessageGetData.EnsureSuccessStatusCode();
                var result = await responseMessageGetData.Content.ReadAsStringAsync();
                var listEmployeeIntegrate = JsonConvert.DeserializeObject<IC_Employee1Office>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var employees = new List<IC_EmployeeIntegrate>();


                if (listEmployeeIntegrate != null && listEmployeeIntegrate.data != null)
                {
                    foreach (var item in listEmployeeIntegrate.data)
                    {
                        employees.Add(new IC_EmployeeIntegrate
                        {
                            Status = item.job_status == "Nghỉ việc" ? false : true,
                            DateQuit = !string.IsNullOrEmpty(item.job_date_out) ? DateTime.ParseExact(item.job_date_out, "dd/MM/yyyy", null) : (DateTime?)null,
                            DepartmentCode = item.department_api != null ? item.department_api.ToString() : "",
                            EmployeeATID = item.attendace_code,
                            EmployeeCode = item.code,
                            FullName = item.name,
                            UserType = 1,
                            PositionCode = item.job_title_api != null ? item.job_title_api.ToString() : "",
                            StartedDate = !string.IsNullOrEmpty(item.job_date_join) ? DateTime.ParseExact(item.job_date_join, "dd/MM/yyyy", null) : (DateTime?)null,
                        });
                    }
                    var client1 = new HttpClient();
                    client1.BaseAddress = new Uri(_config.IntegrateEmployeeLink);
                    client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                    var json = JsonConvert.SerializeObject(employees);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    try
                    {
                        HttpResponseMessage response = await client1.PostAsync("api/User/Post_User", content);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.EnsureSuccessStatusCode();
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"EMPLOYEE_INTEGRATE_TO_DATABASE: {ex}");
                    }
                }
            }
        }
        public async Task AutoSyncEmployeeToDatabaseFromFile(IC_ConfigDTO config)
        {
            var employees = new List<IC_EmployeeIntegrate>();
            if (!string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate) && !string.IsNullOrEmpty(config.IntegrateLogParam.UserName) && !string.IsNullOrEmpty(config.IntegrateLogParam.Password))
            {
                string remoteDirectory = "/outgoing/THS/Emp/";
                var departments = new List<IC_DepartmentIntegrate>();

                using (var sftp = new SftpClient(config.IntegrateLogParam.LinkAPIIntegrate, config.IntegrateLogParam.UserName, config.IntegrateLogParam.Password))
                {
                    sftp.Connect();
                    var files = sftp.ListDirectory(remoteDirectory);
                    foreach (var file in files)
                    {
                        string remoteFileName = file.Name;
                        if (remoteFileName != "empData.csv")
                        {
                            continue;
                        }
                        using (var remoteFileStream = sftp.OpenRead(remoteDirectory + remoteFileName))
                        {
                            using (var reader = new StreamReader(remoteFileStream))
                            {
                                var i = 0;

                                while (!reader.EndOfStream)
                                {
                                    var line = reader.ReadLine();
                                    if (i == 0)
                                    {
                                        i++;
                                        continue;
                                    }

                                    string[] values = line.Split(',');

                                    try
                                    {
                                        employees.Add(new IC_EmployeeIntegrate
                                        {
                                            EmployeeATID = values[0],
                                            EmployeeCode = values[1],
                                            FullName = values[2],
                                            DepartmentCode = values[3],
                                            PositionCode = values[4],
                                            Status = !string.IsNullOrEmpty(values[5]) ? Convert.ToBoolean(values[5]) : false,
                                            DateQuit = !string.IsNullOrEmpty(values[6]) ? Convert.ToDateTime(values[6]) : (DateTime?)null,
                                            UserType = !string.IsNullOrEmpty(values[9]) ? Convert.ToInt32(values[9]) : 0,
                                            CardNumber = values[10],
                                            Note = values[3] + "/" + values[11]
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"PostDepartmentToDatabaseFromFile:", ex);
                                    }
                                    i++;
                                }

                            }
                        }
                    }

                    var client1 = new HttpClient();
                    client1.BaseAddress = new Uri(_config.IntegrateEmployeeLink);
                    client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                    var json = JsonConvert.SerializeObject(employees);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    try
                    {
                        HttpResponseMessage response = await client1.PostAsync("api/User/Post_User", content);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.EnsureSuccessStatusCode();
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"EMPLOYEE_INTEGRATE_TO_DATABASE: {ex}");
                    }

                }

            }
        }

        public async Task AutoSyncLogToDatabase1Office(IC_ConfigDTO config, int previousDay)
        {
            string timePostCheck = DateTime.Now.ToHHmm();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            List<HR_User> listEmployee = db.HR_User.Where(x => x.EmployeeATID != "").ToList();
            DateTime toTime = DateTime.Now;
            DateTime fromTime = toTime.AddDays(-(config.PreviousDays.Value));
            if (previousDay != 0)
            {
                fromTime = toTime.AddDays(-(previousDay));
            }
            fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
            toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);

            List<IC_AttendanceLog> listLogDB = db.IC_AttendanceLog.Where(t => t.CompanyIndex == config.CompanyIndex
                    && t.CheckTime >= fromTime && t.CheckTime <= toTime).ToList();
            List<IC_EmployeeDTO> listNameOnMachine = new List<IC_EmployeeDTO>();
            var addedParams = new List<AddedParam>();
            if (_config.IntegrateDBOther == false)
            {
                var listEmployeeID = listLogDB.Select(e => e.EmployeeATID).Distinct().ToList();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = config.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeID });
                listNameOnMachine = _iC_EmployeeLogic.GetEmployeeList(addedParams).Select(e => new IC_EmployeeDTO { NameOnMachine = e.NameOnMachine, EmployeeATID = e.EmployeeATID }).ToList();
            }
            else
            {
                var listEmployeeID = listLogDB.Select(e => e.EmployeeATID).Distinct().ToList();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeID });
                listNameOnMachine = _hR_EmployeeLogic.GetMany(addedParams).Select(e => new IC_EmployeeDTO { NameOnMachine = e.NameOnMachine, EmployeeATID = e.EmployeeATID }).ToList();
                //listNameOnMachine = integrateContext.HR_Employee.Where(e => listEmployeeID.Contains(e.EmployeeATID)).Select(e => new IC_EmployeeDTO { NameOnMachine = e.NickName, EmployeeATID = e.EmployeeATID }).ToList();
            }

            var listDevice = db.IC_Device.ToList();

            IntegrateLogParam param = JsonConvert.DeserializeObject<IntegrateLogParam>(config.CustomField);
            IntegrateLogMongo logMongo = new IntegrateLogMongo();
            logMongo.IntegrateTime = DateTime.Now;
            logMongo.LogCount = listLogDB.Count;
            logMongo.Param = param;
            logMongo.Success = false;
            logMongo.CompanyIndex = config.CompanyIndex;

            if (listLogDB.Count > 1000)
            {
                var listSplitEmployeeID = CommonUtils.SplitList(listLogDB, 1000);
                foreach (var item in listSplitEmployeeID)
                {

                    var listLogParam = new List<AttendanceLog>();
                    for (int i = 0; i < item.Count; i++)
                    {
                        var nameOnMachine = listNameOnMachine.FirstOrDefault(e => e.EmployeeATID == item[i].EmployeeATID);
                        var log = new AttendanceLog();
                        if (nameOnMachine != null)
                        {
                            log.NameOnMachine = nameOnMachine.NameOnMachine;
                        }
                        HR_User employee = db.HR_User.FirstOrDefault(x => x.EmployeeATID == item[i].EmployeeATID);

                        log.EmployeeATID = item[i].EmployeeATID;
                        log.EmployeeCode = employee?.EmployeeCode;
                        log.SerialNumber = item[i].SerialNumber;
                        log.CheckTime = item[i].CheckTime;
                        log.VerifyMode = item[i].VerifyMode;

                        log.InOutMode = item[i].InOutMode;
                        log.WorkCode = item[i].WorkCode;
                        log.Reserve1 = item[i].Reserve1;
                        log.UpdatedDate = item[i].UpdatedDate;

                        IC_Device device = listDevice.Find(t => t.SerialNumber == log.SerialNumber);
                        if (device != null)
                        {
                            log.DeviceName = device.AliasName;
                            log.IPAddress = device.IPAddress;
                            log.Port = device.Port;
                            log.DeviceId = device.DeviceId;
                            log.DeviceNumber = device.DeviceNumber;
                        }

                        listLogParam.Add(log);
                    }

                    var client = new HttpClient();
                    client.BaseAddress = new Uri(_config.IntegrateEmployeeLink);
                    client.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                    var json = JsonConvert.SerializeObject(listLogParam);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync("api/AttendanceLog/Post_Attendancelog", content);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.EnsureSuccessStatusCode();
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"EMPLOYEE_INTEGRATE_TO_DATABASE: {ex}");
                    }
                }
            }
            else
            {

                var listLogParam = new List<AttendanceLog>();
                for (int i = 0; i < listLogDB.Count; i++)
                {
                    var nameOnMachine = listNameOnMachine.FirstOrDefault(e => e.EmployeeATID == listLogDB[i].EmployeeATID);
                    var log = new AttendanceLog();
                    if (nameOnMachine != null)
                    {
                        log.NameOnMachine = nameOnMachine.NameOnMachine;
                    }
                    HR_User employee = db.HR_User.FirstOrDefault(x => x.EmployeeATID == listLogDB[i].EmployeeATID);

                    log.EmployeeATID = listLogDB[i].EmployeeATID;
                    log.EmployeeCode = employee?.EmployeeCode;
                    log.SerialNumber = listLogDB[i].SerialNumber;
                    log.CheckTime = listLogDB[i].CheckTime;
                    log.VerifyMode = listLogDB[i].VerifyMode;

                    log.InOutMode = listLogDB[i].InOutMode;
                    log.WorkCode = listLogDB[i].WorkCode;
                    log.Reserve1 = listLogDB[i].Reserve1;
                    log.UpdatedDate = listLogDB[i].UpdatedDate;

                    IC_Device device = listDevice.Find(t => t.SerialNumber == log.SerialNumber);
                    if (device != null)
                    {
                        log.DeviceName = device.AliasName;
                        log.IPAddress = device.IPAddress;
                        log.Port = device.Port;
                        log.DeviceId = device.DeviceId;
                        log.DeviceNumber = device.DeviceNumber;
                    }

                    listLogParam.Add(log);
                }

                var client = new HttpClient();
                client.BaseAddress = new Uri(_config.IntegrateEmployeeLink);
                client.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                var json = JsonConvert.SerializeObject(listLogParam);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = await client.PostAsync("api/AttendanceLog/Post_Attendancelog", content);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"EMPLOYEE_INTEGRATE_TO_DATABASE: {ex}");
                }
            }

            try
            {
                var icConfig = new IC_Config();
                icConfig.AlwaysSend = config.AlwaysSend;
                icConfig.BodyEmailError = config.BodyEmailError;
                icConfig.BodyEmailSuccess = config.BodyEmailSuccess;
                icConfig.CompanyIndex = config.CompanyIndex;
                icConfig.CustomField = config.CustomField;
                icConfig.Email = config.Email;
                icConfig.EventType = config.EventType;
                _emailProvider.SendMailIntegrateLog(icConfig, listLogDB.Count(), new List<string>(), listLogDB.Count(), new List<string>());
            }
            catch (Exception ex)
            {
                logMongo.Error = ex.ToString();
            }

        }


        public async Task AutoSyncLogToFile(IC_ConfigDTO config, int previousDay)
        {
            if (!string.IsNullOrEmpty(config.IntegrateLogParam.WriteToFilePath) && !string.IsNullOrEmpty(config.IntegrateLogParam.UserName) && !string.IsNullOrEmpty(config.IntegrateLogParam.Password) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPI))
            {
                string timePostCheck = DateTime.Now.ToHHmm();
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
                List<HR_User> listEmployee = db.HR_User.Where(x => x.EmployeeATID != "").ToList();
                DateTime toTime = DateTime.Now;
                DateTime fromTime = toTime.AddDays(-(config.PreviousDays.Value));
                if (previousDay != 0)
                {
                    fromTime = toTime.AddDays(-(previousDay));
                }
                fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
                toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);

                List<IC_AttendanceLog> listLogDB = db.IC_AttendanceLog.Where(t => t.CompanyIndex == config.CompanyIndex
                        && t.CheckTime >= fromTime && t.CheckTime <= toTime).ToList();
                List<IC_EmployeeDTO> listNameOnMachine = new List<IC_EmployeeDTO>();
                var addedParams = new List<AddedParam>();
                if (_config.IntegrateDBOther == false)
                {
                    var listEmployeeID = listLogDB.Select(e => e.EmployeeATID).Distinct().ToList();
                    addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = config.CompanyIndex });
                    addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeID });
                    listNameOnMachine = _iC_EmployeeLogic.GetEmployeeList(addedParams).Select(e => new IC_EmployeeDTO { NameOnMachine = e.NameOnMachine, EmployeeATID = e.EmployeeATID }).ToList();
                }
                else
                {
                    var listEmployeeID = listLogDB.Select(e => e.EmployeeATID).Distinct().ToList();
                    addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex });
                    addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeID });
                    listNameOnMachine = _hR_EmployeeLogic.GetMany(addedParams).Select(e => new IC_EmployeeDTO { NameOnMachine = e.NameOnMachine, EmployeeATID = e.EmployeeATID }).ToList();
                    //listNameOnMachine = integrateContext.HR_Employee.Where(e => listEmployeeID.Contains(e.EmployeeATID)).Select(e => new IC_EmployeeDTO { NameOnMachine = e.NickName, EmployeeATID = e.EmployeeATID }).ToList();
                }

                var listDevice = db.IC_Device.Where(t => t.CompanyIndex == config.CompanyIndex).ToList();

                IntegrateLogParam param = JsonConvert.DeserializeObject<IntegrateLogParam>(config.CustomField);
                IntegrateLogMongo logMongo = new IntegrateLogMongo();
                logMongo.IntegrateTime = DateTime.Now;
                logMongo.LogCount = listLogDB.Count;
                logMongo.Param = param;
                logMongo.Success = false;
                logMongo.CompanyIndex = config.CompanyIndex;

                var logFile = listLogDB
                       .GroupBy(x => new { x.EmployeeATID, x.CheckTime.Date })
                 .Select(x => new
                 {
                     EmployeeId = x.Key.EmployeeATID,
                     InTime = x.OrderBy(x => x.CheckTime).FirstOrDefault().CheckTime.ToString("HH:mm:ss"),
                     OutTime = x.OrderByDescending(x => x.CheckTime).FirstOrDefault().CheckTime.ToString("HH:mm:ss"),
                     Date = x.Key.Date.ToString("yyyy-MM-dd")
                 });


                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                System.IO.File.WriteAllText(Path.Combine(sWebRootFolder, @"Files/timeLog.csv"), string.Empty);
                using (var w = new StreamWriter(Path.Combine(sWebRootFolder, @"Files/timeLog.csv")))
                {
                    var lineInit = "EmployeeATID,Date,StartTime,EndTime";
                    w.WriteLine(lineInit);
                    w.Flush();
                    foreach (var item in logFile)
                    {
                        var line = string.Format("{0},{1},{2},{3}", item.EmployeeId, item.Date, item.InTime, item.OutTime); ;
                        w.WriteLine(line);
                        w.Flush();
                    }
                }

                try
                {
                    using (var sftp = new SftpClient(config.IntegrateLogParam.LinkAPI, config.IntegrateLogParam.UserName, config.IntegrateLogParam.Password))
                    {
                        Console.WriteLine("Connect to server");
                        sftp.Connect();
                        Console.WriteLine("Creating FileStream object to stream a file");
                        using (FileStream fs = new FileStream(Path.Combine(sWebRootFolder, @"Files/timeLog.csv"), FileMode.Open))
                        {
                            sftp.BufferSize = 1024;
                            sftp.UploadFile(fs, config.IntegrateLogParam.WriteToFilePath, true);
                        }
                        sftp.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"AutoSyncLogToFile: {ex}");

                }

                try
                {
                    var icConfig = new IC_Config();
                    icConfig.AlwaysSend = config.AlwaysSend;
                    icConfig.BodyEmailError = config.BodyEmailError;
                    icConfig.BodyEmailSuccess = config.BodyEmailSuccess;
                    icConfig.CompanyIndex = config.CompanyIndex;
                    icConfig.CustomField = config.CustomField;
                    icConfig.Email = config.Email;
                    icConfig.EventType = config.EventType;
                    _emailProvider.SendMailIntegrateLog(icConfig, listLogDB.Count(), new List<string>(), listLogDB.Count(), new List<string>());
                }
                catch (Exception ex)
                {
                    logMongo.Error = ex.ToString();
                }
            }

        }

        public async Task AutoIntegrateLogConfig(IC_ConfigDTO config, string timePostCheck, bool isManual, int previousDays)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            IntegrateLogParam param = JsonConvert.DeserializeObject<IntegrateLogParam>(config.CustomField);
            if (isManual)
            {
                config.PreviousDays = previousDays;
            }

            if (param.SoftwareType == (int)SoftwareType.Standard || param.SoftwareType == null)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
                List<HR_User> listEmployee = db.HR_User.Where(x => x.EmployeeATID != "").ToList();
                DateTime toTime = DateTime.Now;
                DateTime fromTime = toTime.AddDays(-(config.PreviousDays.Value));

                fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
                toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);

                List<IC_AttendanceLog> listLogDB = db.IC_AttendanceLog.Where(t => t.CompanyIndex == config.CompanyIndex
                        && t.CheckTime >= fromTime && t.CheckTime <= toTime).ToList();
                List<IC_EmployeeDTO> listNameOnMachine = new List<IC_EmployeeDTO>();
                addedParams = new List<AddedParam>();
                if (_config.IntegrateDBOther == false)
                {
                    var listEmployeeID = listLogDB.Select(e => e.EmployeeATID).Distinct().ToList();
                    addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = config.CompanyIndex });
                    addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeID });
                    listNameOnMachine = _iC_EmployeeLogic.GetEmployeeList(addedParams).Select(e => new IC_EmployeeDTO { NameOnMachine = e.NameOnMachine, EmployeeATID = e.EmployeeATID }).ToList();
                }
                else
                {
                    var listEmployeeID = listLogDB.Select(e => e.EmployeeATID).Distinct().ToList();
                    addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex });
                    addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeID });
                    listNameOnMachine = _hR_EmployeeLogic.GetMany(addedParams).Select(e => new IC_EmployeeDTO { NameOnMachine = e.NameOnMachine, EmployeeATID = e.EmployeeATID }).ToList();
                    //listNameOnMachine = integrateContext.HR_Employee.Where(e => listEmployeeID.Contains(e.EmployeeATID)).Select(e => new IC_EmployeeDTO { NameOnMachine = e.NickName, EmployeeATID = e.EmployeeATID }).ToList();
                }

                var listDevice = db.IC_Device.Where(t => t.CompanyIndex == config.CompanyIndex).ToList();
                IntegrateLogMongo logMongo = new IntegrateLogMongo();
                logMongo.IntegrateTime = DateTime.Now;
                logMongo.LogCount = listLogDB.Count;
                logMongo.Param = param;
                logMongo.Success = false;
                logMongo.CompanyIndex = config.CompanyIndex;

                if (listLogDB.Count > 5000)
                {
                    var listSplitEmployeeID = CommonUtils.SplitList(listLogDB, 5000).ToList();
                    var isFirst = true;
                    for (int k = 0; k < listSplitEmployeeID.Count; k++)
                    {

                        var item = listSplitEmployeeID[k];
                        var now = DateTime.Now.ToString("hhMMss");
                        if (param.LinkAPI != null && param.LinkAPI != "")
                        {
                            string apiLink = param.LinkAPI;
                            apiLink = apiLink + (apiLink.EndsWith("/") ? "" : "/");

                            IntegrateTimeLogParam logParam = new IntegrateTimeLogParam();
                            logParam.WriteToDatabase = param.WriteToDatabase;
                            logParam.WriteToFile = param.WriteToFile;
                            logParam.IntegrateTime = now;
                            logParam.WriteToFilePath = param.WriteToFilePath;
                            var listLogParam = new List<AttendanceLog>();
                            var employeeATIDs = listLogDB.Select(x => x.EmployeeATID).Distinct();
                            var employeeLst = (from e in db.HR_User.Where(x => employeeATIDs.Contains(x.EmployeeATID))
                                               join w in db.IC_WorkingInfo.Where(w => w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date && (!w.ToDate.HasValue || w.ToDate.Value.Date >= DateTime.Now.Date))
                                  on e.EmployeeATID equals w.EmployeeATID
                                               join d in db.IC_Department.Where(x => x.IsInactive != true)
                                               on w.DepartmentIndex equals d.Index into deptGroup
                                               from dept in deptGroup.DefaultIfEmpty()
                                               select new
                                               {
                                                   EmployeeATID = e.EmployeeATID,
                                                   DepartmentID = w.DepartmentIndex,
                                                   FullName = e.FullName,
                                                   EmployeeCode = e.EmployeeCode,
                                                   DepartmentName = dept.Name
                                               }).ToList();
                            for (int i = 0; i < item.Count; i++)
                            {
                                var nameOnMachine = listNameOnMachine.FirstOrDefault(e => e.EmployeeATID == item[i].EmployeeATID);
                                var log = new AttendanceLog();
                                if (nameOnMachine != null)
                                {
                                    log.NameOnMachine = nameOnMachine.NameOnMachine;
                                }
                                var employee = employeeLst.FirstOrDefault(x => x.EmployeeATID == item[i].EmployeeATID);

                                log.EmployeeATID = item[i].EmployeeATID;
                                log.EmployeeCode = employee?.EmployeeCode;
                                log.SerialNumber = item[i].SerialNumber;
                                log.CheckTime = item[i].CheckTime;
                                log.VerifyMode = item[i].VerifyMode;

                                log.InOutMode = item[i].InOutMode;
                                log.WorkCode = item[i].WorkCode;
                                log.Reserve1 = item[i].Reserve1;
                                log.UpdatedDate = item[i].UpdatedDate;
                                log.FullName = employee?.FullName;
                                log.DepartmentName = employee?.DepartmentName;

                                IC_Device device = listDevice.Find(t => t.SerialNumber == log.SerialNumber);
                                if (device != null)
                                {
                                    log.DeviceName = device.AliasName;
                                    log.IPAddress = device.IPAddress;
                                    log.Port = device.Port;
                                    log.DeviceId = device.DeviceId;
                                    log.DeviceNumber = device.DeviceNumber;
                                }

                                listLogParam.Add(log);
                            }
                            logParam.ListLogs = listLogParam;
                            logParam.FileType = param.FileType ?? 0;
                            logParam.IsFirst = isFirst;
                            if (k == listSplitEmployeeID.Count - 1)
                            {
                                logParam.IsLast = true;
                            }
                            else
                            {
                                logParam.IsLast = false;
                            }

                            var client = new HttpClient();
                            client.BaseAddress = new Uri(apiLink);
                            var json = JsonConvert.SerializeObject(logParam);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            try
                            {
                                HttpResponseMessage response = await client.PostAsync("api/TA_Timelog/SaveListTimeLog", content);
                                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    logMongo.Success = true;
                                }
                                else
                                {
                                    logMongo.Data = await response.Content.ReadAsStringAsync();
                                    logMongo.Success = false;
                                }
                                string data = await response.Content.ReadAsStringAsync();
                                IntegrateLogResult logResult = JsonConvert.DeserializeObject<IntegrateLogResult>(data);

                                try
                                {
                                    var icConfig = new IC_Config();
                                    icConfig.AlwaysSend = config.AlwaysSend;
                                    icConfig.BodyEmailError = config.BodyEmailError;
                                    icConfig.BodyEmailSuccess = config.BodyEmailSuccess;
                                    icConfig.CompanyIndex = config.CompanyIndex;
                                    icConfig.CustomField = config.CustomField;
                                    icConfig.Email = config.Email;
                                    icConfig.EventType = config.EventType;
                                    _emailProvider.SendMailIntegrateLog(icConfig, logResult.SuccessDB, logResult.ErrorsDB, logResult.SuccessFile, logResult.ErrorsFile);
                                }
                                catch (Exception ex)
                                {
                                    logMongo.Error = ex.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                logMongo.Error = ex.ToString();
                            }
                        }
                        isFirst = false;
                    }
                }
                else
                {
                    if (param.LinkAPI != null && param.LinkAPI != "")
                    {
                        string apiLink = param.LinkAPI;
                        apiLink = apiLink + (apiLink.EndsWith("/") ? "" : "/");

                        IntegrateTimeLogParam logParam = new IntegrateTimeLogParam();
                        logParam.WriteToDatabase = param.WriteToDatabase;
                        logParam.WriteToFile = param.WriteToFile;
                        logParam.IntegrateTime = timePostCheck;
                        logParam.WriteToFilePath = param.WriteToFilePath;
                        logParam.FileType = param.FileType ?? 0;
                        logParam.IsFirst = true;
                        logParam.IsLast = true;
                        var listLogParam = new List<AttendanceLog>();
                        var employeeATIDs = listLogDB.Select(x => x.EmployeeATID).Distinct();
                        var employeeLst = (from e in db.HR_User.Where(x => employeeATIDs.Contains(x.EmployeeATID))
                                           join w in db.IC_WorkingInfo.Where(w => w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date && (!w.ToDate.HasValue || w.ToDate.Value.Date >= DateTime.Now.Date))
                                             on e.EmployeeATID equals w.EmployeeATID
                                           join d in db.IC_Department.Where(x => x.IsInactive != true)
                                           on w.DepartmentIndex equals d.Index into deptGroup
                                           from dept in deptGroup.DefaultIfEmpty()
                                           select new
                                           {
                                               EmployeeATID = e.EmployeeATID,
                                               DepartmentID = w.DepartmentIndex,
                                               FullName = e.FullName,
                                               EmployeeCode = e.EmployeeCode,
                                               DepartmentName = dept.Name
                                           }).ToList();
                        for (int i = 0; i < listLogDB.Count; i++)
                        {
                            var nameOnMachine = listNameOnMachine.FirstOrDefault(e => e.EmployeeATID == listLogDB[i].EmployeeATID);
                            var log = new AttendanceLog();
                            if (nameOnMachine != null)
                            {
                                log.NameOnMachine = nameOnMachine.NameOnMachine;
                            }
                            var employee = employeeLst.FirstOrDefault(x => x.EmployeeATID == listLogDB[i].EmployeeATID);

                            log.EmployeeATID = listLogDB[i].EmployeeATID;
                            log.EmployeeCode = employee?.EmployeeCode;
                            log.SerialNumber = listLogDB[i].SerialNumber;
                            log.CheckTime = listLogDB[i].CheckTime;
                            log.VerifyMode = listLogDB[i].VerifyMode;

                            log.InOutMode = listLogDB[i].InOutMode;
                            log.WorkCode = listLogDB[i].WorkCode;
                            log.Reserve1 = listLogDB[i].Reserve1;
                            log.UpdatedDate = listLogDB[i].UpdatedDate;
                            log.FullName = employee?.FullName;
                            log.DepartmentName = employee?.DepartmentName;

                            IC_Device device = listDevice.Find(t => t.SerialNumber == log.SerialNumber);
                            if (device != null)
                            {
                                log.DeviceName = device.AliasName;
                                log.IPAddress = device.IPAddress;
                                log.Port = device.Port;
                                log.DeviceId = device.DeviceId;
                                log.DeviceNumber = device.DeviceNumber;
                            }

                            listLogParam.Add(log);
                        }
                        logParam.ListLogs = listLogParam;

                        var client = new HttpClient();
                        client.BaseAddress = new Uri(apiLink);
                        var json = JsonConvert.SerializeObject(logParam);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        try
                        {
                            HttpResponseMessage response = await client.PostAsync("api/TA_Timelog/SaveListTimeLog", content);
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                logMongo.Success = true;
                            }
                            else
                            {
                                logMongo.Data = await response.Content.ReadAsStringAsync();
                                logMongo.Success = false;
                            }
                            string data = await response.Content.ReadAsStringAsync();
                            IntegrateLogResult logResult = JsonConvert.DeserializeObject<IntegrateLogResult>(data);

                            try
                            {
                                var icConfig = new IC_Config();
                                icConfig.AlwaysSend = config.AlwaysSend;
                                icConfig.BodyEmailError = config.BodyEmailError;
                                icConfig.BodyEmailSuccess = config.BodyEmailSuccess;
                                icConfig.CompanyIndex = config.CompanyIndex;
                                icConfig.CustomField = config.CustomField;
                                icConfig.Email = config.Email;
                                icConfig.EventType = config.EventType;
                                _emailProvider.SendMailIntegrateLog(icConfig, logResult.SuccessDB, logResult.ErrorsDB, logResult.SuccessFile, logResult.ErrorsFile);
                            }
                            catch (Exception ex)
                            {
                                logMongo.Error = ex.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            logMongo.Error = ex.ToString();
                        }
                    }
                }
                //ghi log 
                MongoDBHelper<IntegrateLogMongo> mongoObject = new MongoDBHelper<IntegrateLogMongo>("log_integrate", _cache);
                if (mongoObject.CheckUseMongoDB() == true)
                {
                    mongoObject.AddDataToCollection(logMongo, true);
                }
            }
            else if (param.SoftwareType == (int)SoftwareType.Office)
            {
                await AutoSyncLogToDatabase1Office(config, 0);
            }
            else if (param.SoftwareType == (int)SoftwareType.File)
            {
                await AutoSyncLogToFile(config, config.PreviousDays.Value);
            }
        }


        public async Task AutoIntegrateLog()
        {
            try
            {
                string timePostCheck = DateTime.Now.ToHHmm();
                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
                addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_LOG.ToString() });
                List<IC_ConfigDTO> downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                if (downloadConfig != null)
                {
                    var config = downloadConfig.FirstOrDefault();
                    if (config != null)
                    {
                        if (!config.TimePos.Contains(timePostCheck)) { return; }
                        else
                        {
                            await AutoIntegrateLogConfig(config, timePostCheck, false, 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error integrate log: " + ex.Message.ToString());
            }
        }


        public async Task AutoIntegrateLogManual(int previousDays)
        {
            try
            {
                string timePostCheck = DateTime.Now.ToHHmm();
                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
                addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_LOG.ToString() });
                List<IC_ConfigDTO> downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                if (downloadConfig != null)
                {
                    var config = downloadConfig.FirstOrDefault();
                    if (config != null)
                    {
                        await AutoIntegrateLogConfig(config, timePostCheck, true, previousDays);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error integrate log:  {ex}");
            }
        }


        public async Task AutoDeleteSystemCommand()
        {
            string timePostCheck = DateTime.Now.ToHHmm();
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.DELETE_SYSTEM_COMMAND.ToString() });
            List<IC_ConfigDTO> downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            if (downloadConfig != null)
            {
                try
                {
                    var config = downloadConfig.FirstOrDefault();
                    if (config != null)
                    {
                        if (config.IntegrateLogParam.AfterHours > 0)
                        {
                            //// TODO: In future will implement send mail when delete system command

                            List<IC_SystemCommand> listCommand = _dbContext.IC_SystemCommand.AsNoTracking().Where(t => t.Excuted == false).ToList();
                            listCommand = listCommand.Where(t => t.RequestedTime.HasValue && DateTime.Now.Subtract(t.RequestedTime.Value).TotalHours >= config.IntegrateLogParam.AfterHours).ToList();
                            if (listCommand != null && listCommand.Count > 0)
                            {
                                List<IC_SystemCommandDTO> deleteCommands = listCommand.Select(u => new IC_SystemCommandDTO
                                {
                                    Index = u.Index,
                                    GroupIndex = u.GroupIndex,
                                    SerialNumber = u.SerialNumber,
                                    EmployeeATIDs = u.EmployeeATIDs,
                                    ExcutingServiceIndex = u.ExcutingServiceIndex,
                                    Params = u.Params,
                                    Excuted = u.Excuted
                                }).ToList();
                                _iC_SystemCommandLogic.DeleteSystemCommandCacheAndDataBase(deleteCommands);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error delete system command log: {ex}");
                }
            }
        }

        public async Task AutoDeleteExecutedCommand()
        {
            DateTime oldestDate = DateTime.Now.Subtract(new TimeSpan(-3, 0, 0, 0, 0));
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            List<IC_CommandSystemGroup> listCommandGroup = await db.IC_CommandSystemGroup.Where(t => t.Excuted == true && t.CreatedDate.Value.Date <= oldestDate).ToListAsync();
            if (listCommandGroup.Count > 0)
            {
                List<int> listGroupIndex = listCommandGroup.Select(t => t.Index).ToList();
                List<IC_SystemCommand> listCommand = await db.IC_SystemCommand.Where(t => listGroupIndex.Contains(t.GroupIndex)).ToListAsync();
                //List<SystemCommandInfoMongo> listCommandInfo = new List<SystemCommandInfoMongo>();

                //mongoObject.AddListDataToCollection(listCommandInfo, false);
                db.IC_CommandSystemGroup.RemoveRange(listCommandGroup);
                db.IC_SystemCommand.RemoveRange(listCommand);
                await db.SaveChangesAsync();

            }
            scope.Dispose();
        }

        private List<CommandResult> CreateCommandDeleteEmployeeStopped(DateTime pNow, IC_ConfigDTO pConfig)
        {
            List<CommandResult> lstCmd = new List<CommandResult>();
            Dictionary<string, List<UserInfoOnMachine>> dicListUserByDevice = new Dictionary<string, List<UserInfoOnMachine>>();
            List<AddedParam> addedParams = new List<AddedParam>();

            List<IC_EmployeeDTO> listStopped = new List<IC_EmployeeDTO>();

            if (_config.IntegrateDBOther == false)
            {
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = pConfig.CompanyIndex });
                var listStopedWorkinginfo = _iC_EmployeeLogic.GetManyStoppedWorking(addedParams);
                if (listStopedWorkinginfo != null)
                {
                    listStopedWorkinginfo = listStopedWorkinginfo.Where(e => e.StoppedDate.HasValue && (e.StoppedDate.Value.Date == DateTime.Now.Date
                    || e.StoppedDate.Value.Date == DateTime.Now.Date.AddDays(-1))).ToList();
                    listStopped = listStopedWorkinginfo;
                }
            }
            else
            {
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "StartedDate", Value = DateTime.Now });
                var listStopedWorkinginfo = _hR_EmployeeLogic.GetManyStoppedWorking(addedParams);
                if (listStopedWorkinginfo != null)
                {
                    listStopped = listStopedWorkinginfo;
                }
            }
            var serialNumbers = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pConfig.CompanyIndex).Select(x => x.SerialNumber).ToList();
            foreach (var emp in listStopped)
            {
                List<string> lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pConfig.CompanyIndex && x.DepartmentIndex == emp.DepartmentIndex).Select(x => x.SerialNumber).ToList();
                if (pConfig.IntegrateLogParam.IntegrateWhenNotInclareDepartment == true && lstSerial.Count == 0)
                {
                    lstSerial = _dbContext.IC_Device.Where(x => !serialNumbers.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                }
                List<string> lsSerialHw = new List<string>();
                ListSerialCheckHardWareLicense(lstSerial, ref lsSerialHw);

                List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                if (lstSerial.Count > 0)
                {
                    IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = new List<string>() { emp.EmployeeATID };
                    paramUserOnMachine.CompanyIndex = pConfig.CompanyIndex;
                    paramUserOnMachine.ListSerialNumber = lsSerialHw;
                    paramUserOnMachine.AuthenMode = "";
                    paramUserOnMachine.FullInfo = true;
                    lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                }

                for (int i = 0; i < lsSerialHw.Count; i++)
                {
                    if (dicListUserByDevice.ContainsKey(lsSerialHw[i]) == false)
                    {
                        dicListUserByDevice.Add(lsSerialHw[i], lstUser);
                    }
                    else
                    {
                        var dictValue = dicListUserByDevice[lsSerialHw[i]];
                        foreach (var user in lstUser)
                        {
                            if (dictValue.Contains(user) == false)
                            {
                                dictValue.Add(user);
                            }
                        }

                        dicListUserByDevice[lstSerial[i]] = dictValue;
                    }
                }

            }



            for (int i = 0; i < dicListUserByDevice.Count; i++)
            {
                IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                commandParam.ListSerialNumber = new List<string>() { dicListUserByDevice.ElementAt(i).Key };
                commandParam.ListEmployee = dicListUserByDevice[dicListUserByDevice.ElementAt(i).Key];
                commandParam.Action = CommandAction.DeleteUserById;
                commandParam.FromTime = DateTime.Now;
                commandParam.ToTime = DateTime.Now;
                commandParam.ExternalData = "";
                commandParam.IsOverwriteData = false;

                List<CommandResult> listCmdTemp = _iC_CommandLogic.CreateListCommands(commandParam);
                lstCmd.AddRange(listCmdTemp);

            }

            return lstCmd;
        }
        private List<CommandResult> CreateAutoDeleteUserCommand(int pCompanyIndex, long pOldDepartment, string pEmp, DateTime pFromTime,
            DateTime pToTime, string pExternalData, List<string> lstSerial, List<UserInfoOnMachine> listUser,
            List<IC_ServiceAndDeviceDTO> listServiceAndDevice)
        {
            List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
            listUser = listUser.Where(x => x.UserID == pEmp).ToList();
            listServiceAndDevice = listServiceAndDevice.Where(x => lstSerial.Contains(x.SerialNumber)).ToList();
            if (lstSerial.Count > 0)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = new List<string>() { pEmp };
                paramUserOnMachine.CompanyIndex = pCompanyIndex;
                paramUserOnMachine.ListSerialNumber = lstSerial;
                paramUserOnMachine.AuthenMode = "";
                paramUserOnMachine.FullInfo = true;
                lstUser = listUser;
            }
            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
            commandParam.ListSerialNumber = lstSerial;
            commandParam.Action = CommandAction.DeleteUserById;
            commandParam.FromTime = pFromTime;
            commandParam.ToTime = pToTime;
            commandParam.ExternalData = pExternalData;
            commandParam.ListEmployee = lstUser;
            commandParam.IsOverwriteData = false;
            List<CommandResult> lstCmdRemoveUser = _iC_CommandLogic.CreateAutoListCommands(commandParam, listServiceAndDevice);
            return lstCmdRemoveUser;
        }

        private List<CommandResult> CreateAutoDeleteUserCommandNew(List<string> pEmp, DateTime pFromTime,
           DateTime pToTime, string pExternalData, List<string> lstSerial, List<UserInfoOnMachine> listUser,
           List<IC_ServiceAndDeviceDTO> listServiceAndDevice)
        {
            List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
            listUser = listUser.Where(x => pEmp.Contains(x.UserID) || pEmp.Contains(x.EmployeeATID)).ToList();
            if (lstSerial.Count > 0)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = pEmp;
                paramUserOnMachine.CompanyIndex = _config.CompanyIndex;
                paramUserOnMachine.ListSerialNumber = lstSerial;
                paramUserOnMachine.AuthenMode = "";
                paramUserOnMachine.FullInfo = true;
                lstUser = listUser;

                IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                commandParam.ListSerialNumber = lstSerial;
                commandParam.Action = CommandAction.DeleteUserById;
                commandParam.FromTime = pFromTime;
                commandParam.ToTime = pToTime;
                commandParam.ExternalData = pExternalData;
                commandParam.ListEmployee = lstUser;
                commandParam.IsOverwriteData = false;
                List<CommandResult> lstCmdRemoveUser = _iC_CommandLogic.CreateAutoListCommands(commandParam, listServiceAndDevice);
                return lstCmdRemoveUser;
            }
            return new List<CommandResult>();
        }

        private List<CommandResult> CreateDeleteUserCommand(int pCompanyIndex, long pOldDepartment, string pEmp, DateTime pFromTime, DateTime pToTime, string pExternalData)
        {
            List<string> lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pCompanyIndex && x.DepartmentIndex == pOldDepartment).Select(x => x.SerialNumber).ToList();
            List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
            var addOrDeleteUserConfig = _dbContext.IC_Config.FirstOrDefault(t => t.CompanyIndex == pCompanyIndex
                && t.EventType == ConfigAuto.ADD_OR_DELETE_USER.ToString());
            var param = JsonConvert.DeserializeObject<IntegrateLogParam>(addOrDeleteUserConfig.CustomField);
            if (param.IntegrateWhenNotInclareDepartment == true && lstSerial.Count == 0)
            {
                var serialNumbers = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => x.SerialNumber).ToList();
                lstSerial = _dbContext.IC_Device.Where(x => !serialNumbers.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
            }
            var lsSerialApp = new List<string>();
            ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp);
            lstSerial = lsSerialApp;
            if (lstSerial.Count > 0)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = new List<string>() { pEmp };
                paramUserOnMachine.CompanyIndex = pCompanyIndex;
                paramUserOnMachine.ListSerialNumber = lstSerial;
                paramUserOnMachine.AuthenMode = "";
                paramUserOnMachine.FullInfo = true;
                lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
            }
            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
            commandParam.ListSerialNumber = lstSerial;
            commandParam.Action = CommandAction.DeleteUserById;
            commandParam.FromTime = pFromTime;
            commandParam.ToTime = pToTime;
            commandParam.ExternalData = pExternalData;
            commandParam.ListEmployee = lstUser;
            commandParam.IsOverwriteData = false;
            List<CommandResult> lstCmdRemoveUser = _iC_CommandLogic.CreateListCommands(commandParam);
            return lstCmdRemoveUser;
        }
        public List<CommandResult> CreateAutoUploadUserCommand(int pCompanyIndex, long pNewDepartment, string pEmp, DateTime pFromTime,
            DateTime pToTime, string pExternalData, bool isOverwriteData, List<string> lstSerial, List<UserInfoOnMachine> listUser,
            List<IC_ServiceAndDeviceDTO> listServiceAndDevice)
        {
            List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
            listUser = listUser.Where(x => x.UserID == pEmp || x.EmployeeATID == pEmp).ToList();
            listServiceAndDevice = listServiceAndDevice.Where(x => lstSerial.Contains(x.SerialNumber)).ToList();
            if (lstSerial.Count > 0)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = new List<string>() { pEmp };
                paramUserOnMachine.CompanyIndex = pCompanyIndex;
                paramUserOnMachine.ListSerialNumber = lstSerial;
                paramUserOnMachine.AuthenMode = "";
                paramUserOnMachine.FullInfo = true;
                lstUser = listUser;
            }
            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
            commandParam.ListSerialNumber = lstSerial;
            commandParam.Action = CommandAction.UploadUsers;
            commandParam.FromTime = pFromTime;
            commandParam.ToTime = pToTime;
            commandParam.ExternalData = pExternalData;
            commandParam.ListEmployee = lstUser;
            commandParam.IsOverwriteData = isOverwriteData;
            List<CommandResult> lstCmdAddUser = _iC_CommandLogic.CreateAutoListCommands(commandParam, listServiceAndDevice);
            return lstCmdAddUser;
        }

        public List<CommandResult> CreateAutoUploadUserCommandNew(List<string> listUserUp, DateTime pFromTime,
           DateTime pToTime, string pExternalData, bool isOverwriteData, List<string> lstSerial, List<UserInfoOnMachine> listUser,
           List<IC_ServiceAndDeviceDTO> listServiceAndDevice)
        {
            List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
            listUser = listUser.Where(x => listUserUp.Contains(x.UserID) || listUserUp.Contains(x.EmployeeATID)).ToList();
            if (lstSerial.Count > 0)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = listUserUp;
                paramUserOnMachine.CompanyIndex = _config.CompanyIndex;
                paramUserOnMachine.ListSerialNumber = lstSerial;
                paramUserOnMachine.AuthenMode = "";
                paramUserOnMachine.FullInfo = true;
                lstUser = listUser;

                IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                commandParam.ListSerialNumber = lstSerial;
                commandParam.Action = CommandAction.UploadUsers;
                commandParam.FromTime = pFromTime;
                commandParam.ToTime = pToTime;
                commandParam.ExternalData = pExternalData;
                commandParam.ListEmployee = lstUser;
                commandParam.IsOverwriteData = isOverwriteData;
                List<CommandResult> lstCmdAddUser = _iC_CommandLogic.CreateAutoListCommands(commandParam, listServiceAndDevice);
                return lstCmdAddUser;
            }

            return new List<CommandResult>();

        }





        public List<CommandResult> CreateUploadUserCommand(int pCompanyIndex, long pNewDepartment, string pEmp, DateTime pFromTime, DateTime pToTime, string pExternalData, bool isOverwriteData)
        {
            var addOrDeleteUserConfig = _dbContext.IC_Config.FirstOrDefault(t => t.CompanyIndex == pCompanyIndex
                 && t.EventType == ConfigAuto.ADD_OR_DELETE_USER.ToString());
            var param = JsonConvert.DeserializeObject<IntegrateLogParam>(addOrDeleteUserConfig.CustomField);
            List<string> lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pCompanyIndex && x.DepartmentIndex == pNewDepartment)
                .Select(x => x.SerialNumber).ToList();
            if (param.IntegrateWhenNotInclareDepartment == true && lstSerial.Count == 0)
            {
                var serialNumbers = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => x.SerialNumber).ToList();
                lstSerial = _dbContext.IC_Device.Where(x => !serialNumbers.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
            }
            var lsSerialApp = new List<string>();
            ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp);
            lstSerial = lsSerialApp;
            //else if (lstSerial == null || lstSerial.Count() == 0)
            //{
            //    lstSerial = _dbContext.IC_Device.Select(e => e.SerialNumber).ToList();
            //}
            List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
            if (lstSerial.Count > 0)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = new List<string>() { pEmp };
                paramUserOnMachine.CompanyIndex = pCompanyIndex;
                paramUserOnMachine.ListSerialNumber = lstSerial;
                paramUserOnMachine.AuthenMode = "";
                paramUserOnMachine.FullInfo = true;
                lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
            }
            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
            commandParam.ListSerialNumber = lstSerial;
            commandParam.Action = CommandAction.UploadUsers;
            commandParam.FromTime = pFromTime;
            commandParam.ToTime = pToTime;
            commandParam.ExternalData = pExternalData;
            commandParam.ListEmployee = lstUser;
            commandParam.IsOverwriteData = isOverwriteData;
            List<CommandResult> lstCmdAddUser = _iC_CommandLogic.CreateListCommands(commandParam);
            return lstCmdAddUser;
        }

        public async Task AutoUpdateExpiratedTransferEmployee()
        {
            List<IC_EmployeeTransferDTO> listEmployeeTransfer = new List<IC_EmployeeTransferDTO>();

            //get employee transfer from 2 table anh update status
            List<IC_EmployeeTransfer> listEmpTransfer = _dbContext.IC_EmployeeTransfer.Where(t => t.Status == (short)TransferStatus.Pendding && t.FromTime.Date < DateTime.Now.Date).ToList();
            List<IC_WorkingInfo> listWorking = _dbContext.IC_WorkingInfo.Where(t => t.Status == (short)TransferStatus.Pendding && t.FromDate.Date < DateTime.Now.Date).ToList();
            for (int i = 0; i < listEmpTransfer.Count; i++)
            {
                listEmpTransfer[i].Status = (short)TransferStatus.Reject;
                listEmpTransfer[i].UpdatedDate = DateTime.Now;
                listEmpTransfer[i].UpdatedUser = UpdatedUser.SYSTEM_AUTO.ToString();
                listEmployeeTransfer.Add(new IC_EmployeeTransferDTO { CompanyIndex = listEmpTransfer[i].CompanyIndex, NewDepartment = listEmpTransfer[i].NewDepartment });
            }
            for (int i = 0; i < listWorking.Count; i++)
            {
                listWorking[i].Status = (short)TransferStatus.Reject;
                listWorking[i].UpdatedDate = DateTime.Now;
                listWorking[i].UpdatedUser = UpdatedUser.SYSTEM_AUTO.ToString();
                listEmployeeTransfer.Add(new IC_EmployeeTransferDTO { CompanyIndex = listWorking[i].CompanyIndex, NewDepartment = listWorking[i].DepartmentIndex });
            }
            _dbContext.IC_EmployeeTransfer.UpdateRange(listEmpTransfer);
            _dbContext.IC_WorkingInfo.UpdateRange(listWorking);

            // Add notification
            if (listEmployeeTransfer.Count > 0)
            {
                List<AddedParam> addedParams = new List<AddedParam>();
                List<IC_UserNotificationDTO> listUserNotify = new List<IC_UserNotificationDTO>();

                var listCountEmployee = listEmployeeTransfer.GroupBy(u => new { u.CompanyIndex, u.NewDepartment }).Select(c => new { c.Key, Count = c.Count() }).ToList();
                foreach (var employee in listCountEmployee)
                {
                    addedParams = new List<AddedParam>();
                    addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = employee.Key.CompanyIndex });
                    addedParams.Add(new AddedParam { Key = "DepartmentIndex", Value = employee.Key.NewDepartment });
                    listUserNotify.AddRange(_iC_UserNotificationLogic.GetListUserNotify(addedParams));

                    foreach (var item in listUserNotify)
                    {
                        item.Status = 0;// 0 is submit , 1 is approve, 2 is reject
                        item.Type = 3;
                        item.Message = JsonConvert.SerializeObject(new MessageBodyDTO
                        {
                            Message = employee.Count.ToString(),
                        });
                    }
                }
                _iC_UserNotificationLogic.CreateList(listUserNotify);
            }
            await _dbContext.SaveChangesAsync();
        }

        private async Task UpdateEmployeeIntegrate(List<EmployeeIntegrate> employeeIntegrates, int pCompanyIndex, EmployeeIntegrateResult result, string customerName)
        {
            var listEmployeeTobeSync = new List<IC_EmployeeDTO>();
            var listDeleeteEmployee = new List<IC_EmployeeDTO>();
            var historyEmployee = new IC_HistoryTrackingIntegrate();
            var historyWorkingInfo = new IC_HistoryTrackingIntegrate();
            historyEmployee.JobName = "IC_Employee";
            historyWorkingInfo.JobName = "IC_WorkingInfo";

            try
            {
                foreach (var item in employeeIntegrates)
                {
                    #region Employee Validation - Check valid card number
                    var validateInfo = new IC_EmployeeDTO
                    {
                        CardNumber = item.CardNumber,
                        EmployeeATID = item.EmployeeATID
                    };

                    var errorList = _iC_EmployeeLogic.ValidateEmployeeInfo(validateInfo);
                    if (errorList != null && errorList.Count > 0)
                    {
                        result.ListIndexError.Add(item.Index);
                        result.ListError.AddRange(errorList);
                        continue;
                    }
                    #endregion

                    #region Department - Insert or Update
                    var department = new IC_DepartmentDTO
                    {
                        Code = item.DepartmentCode,
                        Name = item.DepartmentName,
                        OrgUnitID = item.OrgUnitID,
                        OrgUnitParentNode = item.OrgUnitParentNode,
                        CompanyIndex = pCompanyIndex,
                        CreatedDate = DateTime.Today,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                    };
                    department = _iC_DepartmentLogic.CheckExistedOrCreate(department, customerName);
                    #endregion

                    #region Employee - Insert or Update
                    var employee = new IC_EmployeeDTO();
                    employee.EmployeeATID = item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
                    employee.DepartmentIndex = department.Index;
                    employee.EmployeeCode = item.EmployeeCode;
                    employee.FullName = item.FullName;
                    employee.CardNumber = item.CardNumber;
                    employee.CompanyIndex = pCompanyIndex;
                    employee.StoppedDate = item.StoppedDate;
                    employee.UpdatedDate = DateTime.Today;
                    employee.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();

                    if (item.Status == "D")
                    {
                        listDeleeteEmployee.Add(employee);
                        if (item.StoppedDate != null)
                            employee.StoppedDate = item.StoppedDate;
                    }
                    employee = await _iC_EmployeeLogic.SaveOrUpdateAsync(employee);

                    historyEmployee.DataNew += (short)(employee.IsInsert == true ? 1 : 0);
                    historyEmployee.DataUpdate += (short)(employee.IsUpdate == true ? 1 : 0);
                    #endregion

                    #region Working Info
                    var workingInfo = new IC_WorkingInfoDTO
                    {
                        EmployeeATID = item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'),
                        CompanyIndex = pCompanyIndex,
                        DepartmentIndex = department.Index,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString(),
                        Status = (short)TransferStatus.Approve,
                        ApprovedDate = DateTime.Today,
                        FromDate = item.UpdatedDate,
                        ToDate = item.StoppedDate,
                        IsManager = false,
                        IsSync = null,
                        PositionName = item.Position,
                    };
                    var workingInfoResult = _iC_WorkingInfoLogic.CheckUpdateOrInsert(workingInfo);

                    historyWorkingInfo.DataNew += (short)(workingInfoResult.IsInsert == true ? 1 : 0);
                    historyWorkingInfo.DataUpdate += (short)(workingInfoResult.IsUpdate == true ? 1 : 0);
                    #endregion

                    #region User Master
                    var userMaster = new IC_UserMasterDTO
                    {
                        EmployeeATID = employee.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'),
                        CompanyIndex = employee.CompanyIndex,
                        CardNumber = employee.CardNumber,
                        Privilege = 0,
                        NameOnMachine = employee.NameOnMachine,
                        CreatedDate = DateTime.Now,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                    };
                    _iC_UserMasterLogic.CheckExistedOrCreate(userMaster);
                    #endregion

                    #region Employee Info
                    var existedEmployee = await _dbContext.HR_EmployeeInfo
                        .FirstOrDefaultAsync(e => e.CompanyIndex == pCompanyIndex && e.EmployeeATID == item.EmployeeATID);
                    if (existedEmployee == null)
                    {
                        existedEmployee = new HR_EmployeeInfo
                        {
                            CompanyIndex = employee.CompanyIndex,
                            EmployeeATID = employee.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'),
                            JoinedDate = employee.JoinedDate,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                        };
                        await _dbContext.HR_EmployeeInfo.AddAsync(existedEmployee);
                        await _dbContext.SaveChangesAsync();
                    }
                    #endregion

                    #region Add data before sync
                    result.ListIndexSuccess.Add(item.Index);

                    if (workingInfoResult != null && !string.IsNullOrEmpty(workingInfoResult.EmployeeATID))
                    {
                        // add to list to be sync all device of deparment
                        var emplDelete = listDeleeteEmployee.FirstOrDefault(e => e.EmployeeATID == workingInfoResult.EmployeeATID);
                        if (emplDelete == null)
                        {
                            listEmployeeTobeSync.Add(new IC_EmployeeDTO
                            {
                                EmployeeATID = item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'),
                                CompanyIndex = pCompanyIndex
                            });
                        }
                    }
                    #endregion
                }

                var checkParent = _dbContext.IC_Department.Where(x => x.IsInactive != true).ToList();
                if (checkParent != null && checkParent.Count > 0)
                {
                    var childrenNode = checkParent.Where(x => x.OrgUnitParentNode != 0 && x.OrgUnitParentNode != null).ToList();
                    foreach (var item in childrenNode)
                    {
                        var parent = checkParent.FirstOrDefault(x => x.OVNID == item.OrgUnitParentNode);
                        if (parent != null)
                        {
                            item.ParentIndex = parent.Index;
                            _dbContext.IC_Department.Update(item);
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                }

                await AddHistoryTrackingIntegrate(historyEmployee);
                await AddHistoryTrackingIntegrate(historyWorkingInfo);

                #region Sync data
                var addedParams = new List<AddedParam>
                {
                    new AddedParam { Key = "CompanyIndex", Value = pCompanyIndex },
                    new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER }
                };
                var systemconfigs = await _iC_ConfigLogic.GetMany(addedParams);
                if (systemconfigs != null)
                {
                    var sysconfig = systemconfigs.FirstOrDefault();
                    if (sysconfig != null)
                    {
                        if (sysconfig.IntegrateLogParam.AutoIntegrate)
                        {
                            await _iC_CommandLogic.SyncWithEmployee(listEmployeeTobeSync.Select(u => u.EmployeeATID).ToList(), pCompanyIndex);
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }

        private async Task UpdateEmployeeIntegrateStandard(List<EmployeeIntegrate> employeeIntegrates, int pCompanyIndex, EmployeeIntegrateResult result, string customerName, IC_ConfigDTO config)
        {
            var listEmployeeTobeSync = new List<IC_EmployeeDTO>();
            var listDeleeteEmployee = new List<IC_EmployeeDTO>();
            var historyEmployee = new IC_HistoryTrackingIntegrate();
            var historyWorkingInfo = new IC_HistoryTrackingIntegrate();
            historyEmployee.JobName = "IC_Employee";
            historyWorkingInfo.JobName = "IC_WorkingInfo";

            try
            {
                var lstEmployeesATID = employeeIntegrates.Select(x => x.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0')).ToList();
                var lsthrCardInfo = _dbContext.HR_CardNumberInfo.Where(x => x.IsActive == true).ToList();
                var departmentCodes = employeeIntegrates.Select(x => x.DepartmentCode).ToList();
                var departments = _dbContext.IC_Department.Where(x => departmentCodes.Contains(x.Code)).ToList();
                var userMasters = _dbContext.IC_UserMaster.Where(x => lstEmployeesATID.Contains(x.EmployeeATID)).ToList();
                var employeeInfos = _dbContext.HR_EmployeeInfo.Where(x => lstEmployeesATID.Contains(x.EmployeeATID)).ToList();
                foreach (var item in employeeIntegrates)
                {
                    var department = departments.FirstOrDefault(x => x.Code == item.DepartmentCode);

                    #region Employee - Insert or Update
                    var employee = new IC_EmployeeDTO();
                    employee.EmployeeATID = item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
                    employee.DepartmentIndex = department != null ? department.Index : 0;
                    employee.EmployeeCode = item.EmployeeCode;
                    employee.FullName = item.FullName;
                    employee.CardNumber = item.CardNumber;
                    employee.CompanyIndex = pCompanyIndex;
                    employee.StoppedDate = item.StoppedDate;
                    employee.UpdatedDate = DateTime.Today;
                    employee.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                    employee.FromDate = item.FromDate;
                    employee.Note = item.Note;

                    if (item.StatusStandard == false)
                    {
                        if (item.StoppedDate != null)
                            employee.StoppedDate = item.StoppedDate;
                    }

                    employee = await _iC_EmployeeLogic.SaveOrUpdateAsync(employee);
                    historyEmployee.DataNew += (short)(employee.IsInsert == true ? 1 : 0);
                    historyEmployee.DataUpdate += (short)(employee.IsUpdate == true ? 1 : 0);
                    #endregion

                    #region Working Info
                    var workingInfo = new IC_WorkingInfoDTO
                    {
                        EmployeeATID = item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'),
                        CompanyIndex = pCompanyIndex,
                        DepartmentIndex = department != null ? department.Index : 0,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString(),
                        Status = (short)TransferStatus.Approve,
                        ApprovedDate = DateTime.Today,
                        FromDate = item.FromDate,
                        ToDate = item.StoppedDate,
                        IsManager = false,
                        IsSync = null,
                        PositionName = item.Position,
                    };

                    var workingInfoResult = _iC_WorkingInfoLogic.CheckUpdateOrInsert(workingInfo);
                    if (item.StatusStandard == false)
                    {
                        listDeleeteEmployee.Add(employee);
                    }


                    historyWorkingInfo.DataNew += (short)(workingInfoResult.IsInsert == true ? 1 : 0);
                    historyWorkingInfo.DataUpdate += (short)(workingInfoResult.IsUpdate == true ? 1 : 0);
                    #endregion

                    #region User Master
                    var userMaster = new IC_UserMasterDTO
                    {
                        EmployeeATID = employee.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'),
                        CompanyIndex = employee.CompanyIndex,
                        CardNumber = item.CardNumber,
                        Privilege = 0,
                        NameOnMachine = employee.FullName,
                        CreatedDate = DateTime.Now,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                    };
                    _iC_UserMasterLogic.CheckExistedOrCreateDto(userMaster, userMasters, _dbContext);
                    #endregion

                    #region Employee Info
                    var existedEmployee = employeeInfos
                        .FirstOrDefault(e => e.CompanyIndex == pCompanyIndex && e.EmployeeATID == employee.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'));
                    if (existedEmployee == null)
                    {
                        existedEmployee = new HR_EmployeeInfo
                        {
                            CompanyIndex = employee.CompanyIndex,
                            EmployeeATID = employee.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'),
                            JoinedDate = employee.JoinedDate,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                        };
                        await _dbContext.HR_EmployeeInfo.AddAsync(existedEmployee);
                    }
                    #endregion

                    var cardNumber = !string.IsNullOrEmpty(item.CardNumber) ? item.CardNumber : "0";

                    var hrCardInfo = new HR_CardNumberInfo
                    {
                        CardNumber = cardNumber.TrimStart(new Char[] { '0' }),
                        IsActive = true,
                        CompanyIndex = 2,
                        EmployeeATID = employee.EmployeeATID,

                    };

                    CheckCardActivedOrCreate(hrCardInfo, 2, lsthrCardInfo);

                    #region Add data before sync
                    result.ListIndexSuccess.Add(item.Index);

                    if (workingInfoResult != null && !string.IsNullOrEmpty(workingInfoResult.EmployeeATID) && workingInfoResult.IsNotChange != true)
                    {
                        // add to list to be sync all device of deparment
                        var emplDelete = listDeleeteEmployee.FirstOrDefault(e => e.EmployeeATID == workingInfoResult.EmployeeATID);
                        if (emplDelete == null)
                        {
                            listEmployeeTobeSync.Add(new IC_EmployeeDTO
                            {
                                EmployeeATID = item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'),
                                CompanyIndex = pCompanyIndex
                            });
                        }
                    }
                    #endregion
                }
                await _dbContext.SaveChangesAsync();

                var checkParent = _dbContext.IC_Department.Where(x => x.IsInactive != true).ToList();
                if (checkParent != null && checkParent.Count > 0)
                {
                    var childrenNode = checkParent.Where(x => x.OrgUnitParentNode != 0 && x.OrgUnitParentNode != null).ToList();
                    foreach (var item in childrenNode)
                    {
                        var parent = checkParent.FirstOrDefault(x => x.OVNID == item.OrgUnitParentNode);
                        if (parent != null)
                        {
                            item.ParentIndex = parent.Index;
                            _dbContext.IC_Department.Update(item);
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                }

                await AddHistoryTrackingIntegrate(historyEmployee);
                await AddHistoryTrackingIntegrate(historyWorkingInfo);

                //#region Sync data
                //var addedParams = new List<AddedParam>
                //{
                //    new AddedParam { Key = "CompanyIndex", Value = pCompanyIndex },
                //    new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER }
                //};
                //var systemconfigs = await _iC_ConfigLogic.GetMany(addedParams);
                //if (systemconfigs != null)
                //{
                //    var sysconfig = systemconfigs.FirstOrDefault();
                //    if (sysconfig != null)
                //    {
                //        if (sysconfig.IntegrateLogParam.AutoIntegrate)
                //        {
                //            await _iC_CommandLogic.SyncWithEmployee(listEmployeeTobeSync.Select(u => u.EmployeeATID).ToList(), pCompanyIndex);
                //        }
                //    }
                //}
                //#endregion
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }

        private async Task UpdateEmployeeIntegrate(List<IC_Employee_Integrate_AEON> listEmployeeIntegrateAVN, List<UserDepartmentMappingsDto> userDepartmentMappings, int pCompanyIndex, EmployeeIntegrateAEONResult result, string eventType)
        {
            var listEmployeeTobeSync = new List<IC_EmployeeDTO>();
            var listDeleeteEmployee = new List<IC_EmployeeDTO>();
            var employeeIds = listEmployeeIntegrateAVN.Select(x => x.SAPCode).ToList();
            var listEmployeeDB = _dbContext.HR_EmployeeInfo.Where(e => e.CompanyIndex == pCompanyIndex).ToList();
            var departments = _dbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).ToList();
            var users = _dbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex).ToList();
            var duplicate = listEmployeeIntegrateAVN.GroupBy(x => x.SAPCode).Select(x => new CheckDuplicate()
            {
                SAPCode = x.Key,
                IsAdd = false
            }).ToList();

            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = pCompanyIndex });
            addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
            List<IC_WorkingInfo> listWorkingInfo = _iC_WorkingInfoLogic.GetList(addedParams).OrderByDescending(u => u.FromDate).ToList();

            var userMasterLst = _dbContext.IC_UserMaster.Where(x => x.CompanyIndex == pCompanyIndex).ToList();

            foreach (var item in listEmployeeIntegrateAVN)
            {

                if (item.IsActivated != true)
                {
                    if (!listWorkingInfo.Any(x => x.EmployeeATID == item.SAPCode.PadLeft(_config.MaxLenghtEmployeeATID, '0')))
                    {
                        var existedd = users.FirstOrDefault(x => x.EmployeeATID == item.SAPCode.PadLeft(_config.MaxLenghtEmployeeATID, '0') && x.CompanyIndex == pCompanyIndex);
                        if (existedd != null)
                        {
                            existedd.UserName = "";
                            _dbContext.HR_User.Update(existedd);
                        }
                        continue;
                    }

                }

                var checkDuplicate = duplicate.FirstOrDefault(x => x.SAPCode == item.SAPCode);
                if (checkDuplicate != null)
                {
                    if (checkDuplicate.IsAdd)
                    {
                        continue;
                    }
                    else
                    {
                        checkDuplicate.IsAdd = true;
                    }
                }

                var departmentCodeObj = userDepartmentMappings.FirstOrDefault(x => x.UserId == item.SAPCode);


                var validateInfo = new IC_EmployeeDTO();
                validateInfo.EmployeeATID = item.SAPCode.PadLeft(_config.MaxLenghtEmployeeATID, '0');

                var existed = users.FirstOrDefault(x => x.EmployeeATID == item.SAPCode.PadLeft(_config.MaxLenghtEmployeeATID, '0') && x.CompanyIndex == pCompanyIndex);
                if (existed != null)
                {
                    existed.EmployeeCode = item.SAPCode;
                    existed.FullName = item.FullName;
                    existed.EmployeeType = (int)EmployeeType.Employee;
                    existed.UpdatedDate = DateTime.Now;
                    existed.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                    existed.UserName = item.IsActivated != true ? "" : item.LoginName;
                    _dbContext.HR_User.Update(existed);
                }
                else
                {
                    existed = new HR_User();
                    existed.EmployeeCode = item.SAPCode;
                    existed.FullName = item.FullName;
                    existed.CompanyIndex = pCompanyIndex;
                    existed.EmployeeType = (int)EmployeeType.Employee;
                    existed.EmployeeATID = item.SAPCode.PadLeft(_config.MaxLenghtEmployeeATID, '0');
                    existed.CreatedDate = DateTime.Now;
                    existed.UserName = item.IsActivated != true ? "" : item.LoginName;
                    _dbContext.HR_User.Add(existed);
                }

                // check this employee has working info or not
                var workingInfo = new IC_WorkingInfoDTO();
                workingInfo.EmployeeATID = item.SAPCode.PadLeft(_config.MaxLenghtEmployeeATID, '0'); ;
                workingInfo.CompanyIndex = pCompanyIndex;
                workingInfo.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                workingInfo.Status = (short)TransferStatus.Approve;
                workingInfo.ApprovedDate = DateTime.Today;
                workingInfo.FromDate = DateTime.Today;
                if (item.IsActivated != true)
                {
                    workingInfo.ToDate = DateTime.Today;
                }
                workingInfo.IsManager = false;
                workingInfo.IsSync = null;
                workingInfo.DepartmentIndex = departmentCodeObj != null ? departmentCodeObj.DepartmentDbId : 0;

                var updateWorkingResult = _iC_WorkingInfoLogic.CheckUpdateOrInsertDto(workingInfo, listWorkingInfo, _dbContext);

                var userMaster = new IC_UserMasterDTO();
                userMaster.EmployeeATID = existed.EmployeeATID;
                userMaster.CompanyIndex = existed.CompanyIndex;
                userMaster.Privilege = 0;
                userMaster.CreatedDate = DateTime.Now;
                userMaster.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                _iC_UserMasterLogic.CheckExistedOrCreateDto(userMaster, userMasterLst, _dbContext);

                var existedEmployee = listEmployeeDB.FirstOrDefault(e => e.EmployeeATID == item.SAPCode.PadLeft(_config.MaxLenghtEmployeeATID, '0'));
                if (existedEmployee != null)
                {
                    existedEmployee.UpdatedDate = DateTime.Now;
                    existedEmployee.Email = item.Email;
                    _dbContext.HR_EmployeeInfo.Update(existedEmployee);
                }
                else
                {
                    existedEmployee = new HR_EmployeeInfo();
                    existedEmployee.CompanyIndex = existed.CompanyIndex;
                    existedEmployee.EmployeeATID = existed.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
                    existedEmployee.UpdatedDate = DateTime.Now;
                    existedEmployee.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                    existedEmployee.Email = item.Email;
                    _dbContext.HR_EmployeeInfo.Add(existedEmployee);
                }

                result.ListIndexSuccess.Add(item.Id);

                if (updateWorkingResult != null && updateWorkingResult.EmployeeATID != null)
                {
                    // add to list to be sync all device of deparment
                    var emplDelete = listDeleeteEmployee.FirstOrDefault(e => e.EmployeeATID == updateWorkingResult.EmployeeATID);
                    if (emplDelete == null)
                    {
                        listEmployeeTobeSync.Add(new IC_EmployeeDTO
                        {
                            EmployeeATID = item.SAPCode,
                            CompanyIndex = pCompanyIndex
                        });
                    }
                }
            }
            var results = await _dbContext.SaveChangesAsync();
            if (results > 0)
            {
                _logger.LogInformation("UpdateEmployeeIntegrate successfully!!!");
            }

            var addedParamss = new List<AddedParam>();
            addedParamss.Add(new AddedParam { Key = "CompanyIndex", Value = pCompanyIndex });
            addedParamss.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER });
            var systemconfigs = await _iC_ConfigLogic.GetMany(addedParamss);
            if (systemconfigs != null)
            {
                var sysconfig = systemconfigs.FirstOrDefault();
                if (sysconfig != null)
                {
                    if (sysconfig.IntegrateLogParam.AutoIntegrate)
                    {
                        await _iC_CommandLogic.SyncWithEmployee(listEmployeeTobeSync.Select(u => u.EmployeeATID).ToList(), pCompanyIndex);
                    }
                }
            }
        }

        private void UpdateDepartmentIntegrate(DepartmentFromJson departmentFromJsonIntegrate, int pCompanyIndex)
        {
            var departments = departmentFromJsonIntegrate.Departments;
            var departmentInDb = _dbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && x.IsInactive != true).ToList();
            //var departmentCorrect = new List<AEONDepartmentMapping>();
            if (departments.Count > 0)
            {
                foreach (var department in departments)
                {
                    //var parentNode = GetParentNode(departments, department);
                    //departmentCorrect.Add(new AEONDepartmentMapping { DepartmentCode = department.Id, DepartmentFormatCode = parentNode.Id });
                    //if (department.Type != (int)AeonDepartmentValue.Department)
                    //{
                    //    continue;
                    //}

                    var item = departmentInDb.FirstOrDefault(x => x.Code == department.Id && x.CompanyIndex == pCompanyIndex);
                    if (item != null)
                    {
                        department.IsDb = item.Index;
                        item.Index = item.Index;
                        item.Code = department.Id;
                        item.Name = department.Name;
                        item.Location = department.PostitionName;
                        item.CompanyIndex = pCompanyIndex;
                        item.IsStore = department.IsStore;
                        item.JobGradeGrade = department.JobGradeGrade;
                        item.Type = department.Type;
                        item.UpdatedDate = DateTime.Now;
                    }
                    else
                    {
                        item = new IC_Department();
                    }
                    if (!string.IsNullOrEmpty(department.ParentId))
                    {
                        var parentItem = departments.FirstOrDefault(x => x.Id == department.ParentId);
                        if (parentItem != null)
                        {
                            item.ParentIndex = parentItem.IsDb;
                            department.IsDbParent = parentItem.IsDb;
                        }
                    }

                    if (item.Index > 0)
                    {
                        _dbContext.IC_Department.Update(item);
                    }
                    else
                    {
                        item.Code = department.Id;
                        item.Name = department.Name;
                        item.Location = department.PostitionName;
                        item.CompanyIndex = pCompanyIndex;
                        item.CreatedDate = DateTime.Now;
                        item.Type = department.Type;
                        item.IsStore = department.IsStore;
                        item.JobGradeGrade = department.JobGradeGrade;
                        _dbContext.IC_Department.Add(item);
                    }
                }
                var result = _dbContext.SaveChanges();
                if (result > 0)
                {
                    departmentInDb = _dbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).ToList();
                    var updateParentId = departments.Where(x => !string.IsNullOrEmpty(x.ParentId) && (x.IsDbParent == null || x.IsDbParent == 0)).ToList();
                    if (updateParentId != null && updateParentId.Count > 0)
                    {
                        for (int i = 0; i < updateParentId.Count; i++)
                        {
                            var item = departmentInDb.Where(x => x.Code == updateParentId[i].Id).FirstOrDefault();
                            var parentItem = departmentInDb.Where(x => x.Code == updateParentId[i].ParentId).FirstOrDefault();
                            if (item != null)
                            {
                                item.ParentIndex = parentItem.Index;
                                _dbContext.IC_Department.Update(item);
                            }
                        }
                        var resultDb = _dbContext.SaveChanges();
                        if (resultDb > 0)
                        {
                            _logger.LogInformation("Integrate Department sucessfully!!");
                        }
                    }

                    var departmentMapping = departmentFromJsonIntegrate.UserDepartmentMappings;
                    var lstDeptStore = departmentFromJsonIntegrate.Departments.Where(x => x.IsStore == true && x.JobGradeGrade == 5).ToList();
                    //if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
                    //{
                    //    lstDeptLevel1 = lstDeptLevel1.Where(x => user.ListDepartmentAssigned.Contains(x.Index)).ToList();
                    //}

                    var lstDpt = new List<string>();

                    for (int i = 0; i < lstDeptStore.Count; i++)
                    {
                        var dept = GetChildren(departmentFromJsonIntegrate.Departments, lstDeptStore[i].Id);
                        lstDpt.AddRange(dept.Select(x => x.Id));
                    }

                    lstDpt.AddRange(lstDeptStore.Select(x => x.Id));

                    var lstDeptHQCon = departmentFromJsonIntegrate.Departments.Where(x => !lstDpt.Contains(x.Id)).ToList();

                    var lstDeptHQ = departmentFromJsonIntegrate.Departments.Where(x => !lstDpt.Contains(x.Id) && x.IsStore == false && x.Type == 2).ToList();
                    var lstDeptHQId = lstDeptHQ.Select(x => x.Id).ToList();
                    for (int i = 0; i < departmentMapping.Count; i++)
                    {

                        var item = departmentInDb.FirstOrDefault(x => x.Code == departmentMapping[i].DepartmentId);
                        if (lstDpt.Contains(item.Code) && item.JobGradeGrade == 1)
                        {

                            var it = GetStoreDepartmentId(departmentFromJsonIntegrate.Departments, item.Code);
                            item = departmentInDb.FirstOrDefault(x => x.Code == it);
                            departmentMapping[i].DepartmentDbId = item.Index;
                        }
                        else if (lstDpt.Contains(item.Code) && item.JobGradeGrade > 1)
                        {
                            departmentMapping[i].DepartmentDbId = item.Index;
                        }
                        else if (lstDeptHQId.Contains(item.Code))
                        {

                            departmentMapping[i].DepartmentDbId = item.Index;

                        }
                        else if (!lstDeptHQId.Contains(item.Code))
                        {
                            var it = GetHQDepartmentId(lstDeptHQCon, item.Code, lstDeptHQId);
                            item = departmentInDb.FirstOrDefault(x => x.Code == it);
                            departmentMapping[i].DepartmentDbId = item.Index;
                        }
                    }
                }
            }
        }

        private string GetHQDepartmentId(List<IC_Department_Integrate_AEON_Dto> lstDeptHQCon, string id, List<string> lstDeptHQ)
        {
            var item = lstDeptHQCon.FirstOrDefault(x => x.Id == id);
            if (lstDeptHQ.Contains(item.Id))
            {
                return item.Id;
            }
            else
            {
                return GetHQDepartmentId(lstDeptHQCon, item.ParentId, lstDeptHQ);
            }
        }

        private string GetStoreDepartmentId(List<IC_Department_Integrate_AEON_Dto> lstDeptHQCon, string id)
        {
            var item = lstDeptHQCon.FirstOrDefault(x => x.Id == id);
            if (item.JobGradeGrade >= 2)
            {
                return item.Id;
            }
            else
            {
                return GetStoreDepartmentId(lstDeptHQCon, item.ParentId);
            }
        }


        private List<IC_Department_Integrate_AEON_Dto> GetChildren(List<IC_Department_Integrate_AEON_Dto> foos, string id)
        {
            return foos
                .Where(x => x.ParentId == id)
                .Union(foos.Where(x => x.ParentId == id)
                    .SelectMany(y => GetChildren(foos, y.Id))
                ).ToList();
        }


        private IC_Department_Integrate_AEON_Dto GetParentNode(List<IC_Department_Integrate_AEON_Dto> lst, IC_Department_Integrate_AEON_Dto node)
        {
            if (node != null && node.Type == (int)AeonDepartmentValue.Department)
            {
                return node;
            }
            if (node != null && !string.IsNullOrWhiteSpace(node.ParentId))
            {
                var parent = lst.FirstOrDefault(x => x.Id == node.ParentId);
                if (parent.Type == (int)AeonDepartmentValue.Department)
                {
                    return parent;
                }
                else
                {
                    return GetParentNode(lst, parent);
                }
            }
            else
            {
                return null;
            }
        }


        private void UpdateSyncDepartmentIntegrate(DepartmentFromJson departmentFromJsonIntegrate, int pCompanyIndex)
        {
            var departments = departmentFromJsonIntegrate.Departments;
            var departmentInDb = _dbContext.IC_DepartmemtAEONSync.Where(x => x.CompanyIndex == pCompanyIndex).ToList();
            if (departments.Count > 0)
            {
                foreach (var department in departments)
                {

                    var item = departmentInDb.FirstOrDefault(x => x.Code == department.Id && x.CompanyIndex == pCompanyIndex);
                    if (item != null)
                    {
                        department.IsDb = item.Index;
                        item.Index = item.Index;
                        item.Code = department.Id;
                        item.Name = department.Name;
                        item.Location = department.PostitionName;
                        item.CompanyIndex = pCompanyIndex;
                        item.IsStore = department.IsStore;
                        item.JobGradeGrade = department.JobGradeGrade;
                        item.Type = department.Type;
                        item.UpdatedDate = DateTime.Now;
                    }
                    else
                    {
                        item = new IC_DepartmemtAEONSync();
                    }
                    if (!string.IsNullOrEmpty(department.ParentId))
                    {
                        var parentItem = departments.FirstOrDefault(x => x.Id == department.ParentId);
                        if (parentItem != null)
                        {
                            item.ParentIndex = parentItem.IsDb;
                            department.IsDbParent = parentItem.IsDb;
                        }
                    }

                    if (item.Index > 0)
                    {
                        _dbContext.IC_DepartmemtAEONSync.Update(item);
                    }
                    else
                    {
                        item.Code = department.Id;
                        item.Name = department.Name;
                        item.Location = department.PostitionName;
                        item.CompanyIndex = pCompanyIndex;
                        item.CreatedDate = DateTime.Now;
                        item.Type = department.Type;
                        item.IsStore = department.IsStore;
                        item.JobGradeGrade = department.JobGradeGrade;
                        _dbContext.IC_DepartmemtAEONSync.Add(item);
                    }
                }
                var result = _dbContext.SaveChanges();
                if (result > 0)
                {
                    departmentInDb = _dbContext.IC_DepartmemtAEONSync.Where(x => x.CompanyIndex == pCompanyIndex).ToList();
                    var updateParentId = departments.Where(x => !string.IsNullOrEmpty(x.ParentId) && (x.IsDbParent == null || x.IsDbParent == 0)).ToList();
                    if (updateParentId != null && updateParentId.Count > 0)
                    {
                        for (int i = 0; i < updateParentId.Count; i++)
                        {
                            var item = departmentInDb.Where(x => x.Code == updateParentId[i].Id).FirstOrDefault();
                            var parentItem = departmentInDb.Where(x => x.Code == updateParentId[i].ParentId).FirstOrDefault();
                            if (item != null)
                            {
                                item.ParentIndex = parentItem.Index;
                                _dbContext.IC_DepartmemtAEONSync.Update(item);
                            }
                        }
                        var resultDb = _dbContext.SaveChanges();
                        if (resultDb > 0)
                        {
                            _logger.LogInformation("Integrate Department sucessfully!!");
                        }
                    }

                }
            }
        }

        public void UpdateCommandInCompanyCache(List<CommandParam> pListCommand, string pGroupIndex, CompanyInfo companyInfo, DateTime pNow)
        {
            for (int i = 0; i < pListCommand.Count; i++)
            {
                companyInfo.UpdateCommandById(pListCommand[i].ID, pGroupIndex, pListCommand[i].Status, pListCommand[i].Error, pNow);
            }
        }

        private void DeleteCommandInCompanyCache(List<CommandResult> pListCommand, string pGroupIndex, CompanyInfo companyInfo, DateTime pNow)
        {
            for (int i = 0; i < pListCommand.Count; i++)
            {
                if (pListCommand[i].Status == CommandStatus.Success.ToString() || pListCommand[i].Status == CommandStatus.Failed.ToString())
                {
                    companyInfo.DeleteCommandById(pListCommand[i].ID, pGroupIndex);
                }
            }
        }

        private bool ListSerialCheckHardWareLicense(List<string> lsSerial, ref List<string> lsSerialHw)
        {
            int dem = 0;
            foreach (var serial in lsSerial)
            {
                bool checkls = _cache.HaveHWLicense(serial);
                if (checkls)
                {
                    lsSerialHw.Add(serial);
                }
                else
                {
                    dem++;
                }
            }
            if (dem > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task AutoGetOVNDepartment()
        {
            string timePostCheck = DateTime.Now.ToHHmm();
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = 2 },
                new AddedParam { Key = "EventType", Value = ConfigAuto.EMPLOYEE_INTEGRATE.ToString() }
            };
            try
            {
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                if (downloadConfig != null && downloadConfig.Any())
                {
                    var config = downloadConfig.First();
                    if (config != null)
                    {
                        if (!config.TimePos.Contains(timePostCheck)) { return; }

                        //set the connection string
                        string connString = _thirdPartyIntegrationConfigurationService.GetConnectionString;

                        //sql connection object
                        using SqlConnection conn = new SqlConnection(connString);
                        var spInfo = new StoreProcedureInfo
                        {
                            Name = @"dbo.[SP_OVNDeptLst]",
                            Params = new List<Parameter>()
                        };

                        //define the SqlCommand object
                        var cmd = conn.CreateSqlCommand(spInfo);

                        //open connection
                        conn.Open();

                        //set the SqlCommand type to stored procedure and execute
                        cmd.CommandType = CommandType.StoredProcedure;

                        var dr = cmd.ExecuteReader();

                        //check if there are records
                        var departments = new List<IC_Department_Integrate_OVN>();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                var department = new IC_Department_Integrate_OVN();
                                department.ID = (int)dr.GetInt64(0);
                                department.ParentNodeID = (int)dr.SafeGetLong(1);
                                department.NameEN = dr.GetString(2);
                                department.Code = dr.SafeGetString(3);
                                department.OrgUnitID = dr.GetInt32(4);
                                department.AddedDate = DateTime.Now;

                                departments.Add(department);
                            }
                        }

                        //close data reader
                        dr.Close();

                        //close connection
                        conn.Close();
                        conn.Dispose();
                        //#endif

                        if (departments != null && departments.Any())
                        {
                            //clean IC_Employee_Integrate table
                            //_iC_Employee_IntegrateLogic.RemoveAll();
                            if (await _dbSyncContext.IC_Department_Integrate_OVN.AnyAsync())
                            {
                                var unitIDs = departments.Select(x => x.OrgUnitID).ToList();
                                var currentDepartments = _dbSyncContext.IC_Department_Integrate_OVN.Where(x => unitIDs.Contains(x.OrgUnitID)).ToList();
                                if (currentDepartments != null && currentDepartments.Count > 0)
                                {
                                    _dbSyncContext.IC_Department_Integrate_OVN.RemoveRange(currentDepartments);
                                    await _dbSyncContext.SaveChangesAsync();
                                }

                            }

                            await _dbSyncContext.IC_Department_Integrate_OVN.AddRangeAsync(departments);
                            await _dbSyncContext.SaveChangesAsync();

                            //Get all Department on Integrate table
                            var listDepartment = await _iC_Employee_IntegrateLogic.GetAllDepartment();
                            if (listDepartment != null && listDepartment.Count > 0)
                            {
                                var departmentListIntegrate = listDepartment.Select(u => new IC_Department_Integrate_OVN
                                {
                                    ID = u.ID,
                                    OrgUnitID = u.OrgUnitID,
                                    ParentNodeID = u.ParentNodeID,
                                    NameEN = u.NameEN,
                                    Code = u.Code
                                }).ToList();

                                var parentsLevel1 = departmentListIntegrate.Where(x => x.ParentNodeID == 0).Select(x => x.ID).Distinct().ToList();
                                var parentsLevelN = departmentListIntegrate.Where(x => x.ParentNodeID > 0).Select(x => x.ParentNodeID).Distinct().ToList();

                                parentsLevelN.AddRange(parentsLevel1);
                                parentsLevelN = parentsLevelN.Distinct().ToList();

                                var parentsUnitID = departmentListIntegrate.Where(x => parentsLevelN.Contains(x.ID)).Select(x => x.OrgUnitID).Distinct().ToList();
                                var children = departmentListIntegrate.Where(x => x.ParentNodeID > 0).Select(x => x.OrgUnitID).Distinct().ToList();

                                var ovnDepartmentIDs = departmentListIntegrate.Select(x => x.OrgUnitID).ToList();
                                var ovnDepartmentParentIds = departmentListIntegrate.Select(x => x.ParentNodeID).ToList();
                                var employeeIntegrationList = await _iC_Employee_IntegrateLogic.GetAllAsync();
                                var oldDepartmentList = await _iC_DepartmentLogic.OldDepartmentUpdate(2, ovnDepartmentIDs, employeeIntegrationList);

                                var history = new IC_HistoryTrackingIntegrate();
                                history.JobName = "AutoGetOVNDepartment IC_Department";
                                history.DataDelete = (short)oldDepartmentList;

                                children.AddRange(parentsUnitID);
                                children = children.Distinct().ToList();

                                int row = 1;
                                foreach (var id in children)
                                {
                                    var departmentDto = departmentListIntegrate.Where(x => x.OrgUnitID == id).Select(x => new IC_DepartmentDTO
                                    {
                                        Code = x.Code,
                                        Name = x.NameEN,
                                        OVNID = x.ID,
                                        OrgUnitID = x.OrgUnitID,
                                        OrgUnitParentNode = x.ParentNodeID,
                                        CompanyIndex = 2,
                                        CreatedDate = DateTime.Now,
                                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                                    }).FirstOrDefault();

                                    if (departmentDto != null)
                                    {
                                        departmentDto = await _iC_DepartmentLogic.CheckExistedOrCreateOVN(departmentDto, departmentListIntegrate, row);
                                        if (departmentDto != null)
                                        {
                                            if (departmentDto.IsInsert == true)
                                            {
                                                history.DataNew += 1;
                                            }
                                            if (departmentDto.IsUpdate == true)
                                            {
                                                history.DataUpdate += 1;
                                            }
                                        }
                                        row++;
                                    }
                                }

                                var checkParent = _dbContext.IC_Department.Where(x => x.IsInactive != true).ToList();
                                if (checkParent != null && checkParent.Count > 0)
                                {
                                    var childrenNode = checkParent.Where(x => x.OrgUnitParentNode != 0 && x.OrgUnitParentNode != null).ToList();
                                    foreach (var item in childrenNode)
                                    {
                                        var parent = checkParent.FirstOrDefault(x => x.OVNID == item.OrgUnitParentNode);
                                        if (parent != null)
                                        {
                                            item.ParentIndex = parent.Index;
                                            _dbContext.IC_Department.Update(item);
                                        }
                                    }

                                    await _dbContext.SaveChangesAsync();
                                }

                                await AddHistoryTrackingIntegrate(history);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoGetOVNDepartment: {ex}");
            }
        }

        public async Task AutoGetOVNEmployee()
        {
            // Get data from third party sooner setting 30 minutes to prepare for internal synchronization
            string timePostCheck = DateTime.Now.ToHHmm();
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = 2 },
                new AddedParam { Key = "EventType", Value = ConfigAuto.EMPLOYEE_INTEGRATE.ToString() }
            };
            try
            {
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                if (downloadConfig != null && downloadConfig.Any())
                {
                    var config = downloadConfig.First();
                    if (config != null)
                    {
                        if (!config.TimePos.Contains(timePostCheck)) { return; }

                        //set the connection string
                        string connString = _thirdPartyIntegrationConfigurationService.GetConnectionString;

                        //#if !DEBUG
                        //sql connection object
                        using SqlConnection conn = new SqlConnection(connString);
                        var spInfo = new StoreProcedureInfo
                        {
                            Name = @"dbo.[SP_GetEmployeeInfo]",
                            Params = new List<Parameter>
                            {
                                new Parameter
                                {
                                    Name = "@pLocalEmployeeID",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = null
                                },
                                new Parameter
                                {
                                    Name = "@pDepartmentCode",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = null
                                },
                            }
                        };

                        //define the SqlCommand object
                        var cmd = conn.CreateSqlCommand(spInfo);

                        //open connection
                        conn.Open();

                        //set the SqlCommand type to stored procedure and execute
                        cmd.CommandType = CommandType.StoredProcedure;

                        var dr = cmd.ExecuteReader();

                        //check if there are records
                        var employees = new List<IC_Employee_Integrate>();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                var employee = new IC_Employee_Integrate();
                                employee.EmployeeATID = dr.GetString(0);//LocalEmployeeID
                                employee.FullName = dr.GetString(1);//FullName
                                employee.DepartmentCode = dr.GetString(2);//DepartmentCode
                                employee.Department = dr.GetString(3);//DepartmentName
                                employee.Position = dr.GetString(4);//Position
                                employee.Status = dr.GetString(5);//ModifiedDate
                                employee.UpdatedDate = dr.GetDateTime(6);//ModifiedDate
                                employee.OrgUnitID = dr.GetInt32(7);//OrgUnitID
                                employee.OrgUnitParentNode = (int?)dr.GetInt64(8);//OrgUnitParentNode

                                if (dr.GetString(5).ToLower() == "resignation")
                                {
                                    employee.StoppedDate = dr.GetDateTime(6);
                                }
                                employees.Add(employee);
                            }
                            //_logger.LogError($"employees: {employees.Count}");
                        }

                        //close data reader
                        dr.Close();

                        //close connection
                        conn.Close();
                        conn.Dispose();
                        //#endif

                        //#if DEBUG
                        //                        var employees = new List<IC_Employee_Integrate>();
                        //                        var startId = 1000000;
                        //                        for (int i = 0; i < 10; i++)
                        //                        {
                        //                            startId++;
                        //                            employees.Add(new IC_Employee_Integrate
                        //                            {
                        //                                EmployeeATID = startId.ToString(),
                        //                                FullName = "Test " + startId,
                        //                                DepartmentCode = "DP01",
                        //                                Department = "Department 01",
                        //                                Position = "",
                        //                                Status = "New",
                        //                                UpdatedDate = DateTime.Now
                        //                            });
                        //                        }
                        //#endif

                        if (employees.Any())
                        {
                            var history = new IC_HistoryTrackingIntegrate();
                            history.JobName = "AutoGetOVNEmployee IC_Employee_Integrate";
                            employees = employees.Where(x => !x.Department.ToUpper().StartsWith("I_") && !x.Department.ToUpper().StartsWith("M_")).ToList();
                            if (employees != null && employees.Count() > 0)
                            {
                                //clean IC_Employee_Integrate table
                                //_iC_Employee_IntegrateLogic.RemoveAll();
                                if (await _dbSyncContext.IC_Employee_Integrate.AnyAsync())
                                {
                                    var employeeIds = employees.Select(x => x.EmployeeATID).ToList();
                                    var currentEmployees = _dbSyncContext.IC_Employee_Integrate.Where(x => employeeIds.Contains(x.EmployeeATID)).ToList();
                                    _dbSyncContext.IC_Employee_Integrate.RemoveRange(currentEmployees);
                                    await _dbSyncContext.SaveChangesAsync();
                                    history.DataDelete = (short)(currentEmployees != null ? currentEmployees.Count() : 0);
                                }

                                await _dbSyncContext.IC_Employee_Integrate.AddRangeAsync(employees);
                                await _dbSyncContext.SaveChangesAsync();

                                history.DataNew = (short)employees.Count();
                                _logger.LogError($"employees succeed: {employees.Count}");

                                await AddHistoryTrackingIntegrate(history);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoGetOVNEmployee: {ex}");
            }
        }

        public async Task AutoGetOVNEmployeeShift()
        {
            string timePostCheck = DateTime.Now.ToHHmm();
            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.EMPLOYEE_SHIFT_INTEGRATE.ToString() });
            try
            {
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                if (downloadConfig != null && downloadConfig.Any())
                {
                    var config = downloadConfig.First();
                    var param = JsonConvert.DeserializeObject<IntegrateOVNEmployeeShiftParam>(config.CustomField);

                    if (config != null)
                    {
                        if (!config.TimePos.Contains(timePostCheck)) { return; }

                        //Set the connection string
                        string connString = _thirdPartyIntegrationConfigurationService.GetConnectionString;

                        var from = DateTime.Parse(param.FromDate).ToString("M/d/yyyy");
                        var to = DateTime.Parse(param.ToDate).ToString("M/d/yyyy");

                        //Sql connection object
                        using SqlConnection conn = new SqlConnection(connString);
                        var spInfo = new StoreProcedureInfo
                        {
                            Name = @"dbo.[SP_GetEmployeeShift]",
                            Params = new List<Parameter>
                            {
                                new Parameter
                                {
                                    Name = "@pLocalEmployeeID",
                                    SqlDbType = SqlDbType.VarChar,
                                    Value = null
                                },
                                new Parameter
                                {
                                    Name = "@pDepartmentCode",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = null
                                },
                                new Parameter
                                {
                                    Name = "@pFromDate",
                                    SqlDbType = SqlDbType.VarChar,
                                    Value = from//"1/1/2000"
                                },
                                new Parameter
                                {
                                    Name = "@pToDate",
                                    SqlDbType = SqlDbType.VarChar,
                                    Value = to//DateTime.Now.AddYears(1).ToString("M/d/yyyy")
                                }
                            }
                        };

                        //Define the SqlCommand object
                        var cmd = conn.CreateSqlCommand(spInfo);

                        //Open connection
                        conn.Open();

                        //Set the SqlCommand type to stored procedure and execute
                        cmd.CommandType = CommandType.StoredProcedure;

                        var dr = cmd.ExecuteReader();

                        //Check if there are records
                        var employeeShiftData = new List<Employee_Shift_Integrate>();

                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                employeeShiftData.Add(new Employee_Shift_Integrate
                                {
                                    EmployeeId = dr.GetString(0),
                                    ShiftName = dr.GetString(1),
                                    ShiftDate = dr.GetDateTime(2),
                                    ShiftFromTime = dr.GetDateTime(3),
                                    ShiftToTime = dr.GetDateTime(4),
                                    ShiftApplyDate = dr.GetDateTime(5),
                                    ModifiedDate = dr.GetDateTime(6)
                                });
                            }
                        }

                        //Close data reader
                        dr.Close();

                        //Close connection
                        conn.Close();

                        if (employeeShiftData.Any())
                        {
                            //employeeShiftData = employeeShiftData.Where(x => x.ShiftDate >= DateTime.Now).ToList();
                            //if(employeeShiftData != null && employeeShiftData.Any())
                            //_logger.LogError($"Date: {from} --> {to} - Request: {employeeShiftData.Count()}");
                            await UpdateEmployeeShift(employeeShiftData);
                            //_logger.LogError($"employeeShiftData after update: {employeeShiftData.Count}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoGetOVNEmployeeShift: {ex}");
            }
        }

        public async Task AutoSyncEmployee(IC_ConfigDTO config)
        {
            try
            {
                var milliseconds = 90000;
                Thread.Sleep(milliseconds);
                // Using Link API
                if (!string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPI) && config.IntegrateLogParam.UsingDatabase == false)
                {
                    var client = new HttpClient();
                    var responseMessageGetData = await client.GetAsync(config.IntegrateLogParam.LinkAPI + "/api/IntergratedEmployee/GetData");
                    responseMessageGetData.EnsureSuccessStatusCode();
                    var result = await responseMessageGetData.Content.ReadAsStringAsync();
                    var listEmployeeIntegrate = JsonConvert.DeserializeObject<List<EmployeeIntegrate>>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (listEmployeeIntegrate != null && listEmployeeIntegrate.Count > 0)
                    {
                        var updateResult = new EmployeeIntegrateResult();
                        listEmployeeIntegrate = listEmployeeIntegrate.Distinct().ToList();
                        await UpdateEmployeeIntegrate(listEmployeeIntegrate, config.CompanyIndex, updateResult, "");
                        //write log
                        var strError = "";
                        for (int i = 0; i < updateResult.ListIndexError.Count; i++)
                        {
                            strError += $"{Environment.NewLine}Index: {updateResult.ListIndexError[i]}. Error: {updateResult.ListError[i]} ";
                        }

                        var content = JsonConvert.SerializeObject(updateResult);
                        var data = new StringContent(content, Encoding.UTF8, "application/json");
                        var responseMessageUpdateData = await client.PostAsync(config.IntegrateLogParam.LinkAPI + "/api/IntergratedEmployee/UpdateData", data);
                        responseMessageUpdateData.EnsureSuccessStatusCode();
                    }
                }
                else if (config.IntegrateLogParam.UsingDatabase)
                {
                    var listEmployeeSync = await _iC_Employee_IntegrateLogic.GetAllAsync();
                    if (listEmployeeSync != null && listEmployeeSync.Count > 0)
                    {
                        var employeeIntegrate = listEmployeeSync.Select(u => new EmployeeIntegrate
                        {
                            Index = u.Index,
                            EmployeeATID = u.EmployeeATID,
                            CardNumber = u.CardNumber,
                            DepartmentCode = u.DepartmentCode,
                            DepartmentName = u.Department,
                            EmployeeCode = u.EmployeeCode,
                            FullName = u.FullName,
                            Position = u.Position,
                            Status = u.Status,
                            StoppedDate = u.StoppedDate,
                            UpdatedDate = u.UpdatedDate,
                            OrgUnitID = u.OrgUnitID,
                            OrgUnitParentNode = u.OrgUnitParentNode
                        }).ToList();

                        var updateResult = new EmployeeIntegrateResult();
                        await UpdateEmployeeIntegrate(employeeIntegrate, config.CompanyIndex, updateResult, _configClientName);

                        //write log
                        var strError = "";
                        for (int i = 0; i < updateResult.ListIndexError.Count; i++)
                        {
                            strError += $"{Environment.NewLine}Index: {updateResult.ListIndexError[i]}. Error: {updateResult.ListError[i]} ";
                        }
                        //_iC_Employee_IntegrateLogic.Update(updateResult);
                    }
                    else
                    {
                        _logger.LogError($"listEmployeeSync: {listEmployeeSync.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sync employee log: {ex}");
            }
        }

        public async Task AutoSyncIntegrateEmloyee(DateTime pNow)
        {
            var timePostCheck = DateTime.Now.ToHHmm();
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = 2 },
                new AddedParam { Key = "EventType", Value = ConfigAuto.EMPLOYEE_INTEGRATE.ToString() }
            };
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            if (downloadConfig != null)
            {
                var config = downloadConfig.FirstOrDefault();
                try
                {
                    if (config != null)
                    {
                        if (!config.TimePos.Contains(timePostCheck)) { return; }
                        else
                        {
                            if (_configClientName == ClientName.AEON.ToString())
                            {
                                await AutoGetAEONDepartment(config);
                            }
                            else if (_configClientName == ClientName.AVN.ToString())
                            {
                                await AutoGetAVNIntegrate(pNow, config);
                            }
                            else if (_configClientName == ClientName.OVN.ToString())
                            {
                                await AutoSyncEmployee(config);
                            }
                            else
                            {
                                await AutoSyncEmployeeStandard(config);
                            }

                            if (config.AlwaysSend)
                            {
                                string title = config.TitleEmailSuccess;
                                string bodyEmail = config.BodyEmailSuccess;
                                var isSuccess = _emailProvider.SendEmailToMulti("", title, bodyEmail, config.Email.Trim());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (config.SendMailWhenError)
                    {
                        string title = config.TitleEmailError;
                        string bodyEmail = config.BodyEmailError;
                        var isSuccess = _emailProvider.SendEmailToMulti("", title, bodyEmail, config.Email.Trim());
                    }
                    _logger.LogError($"Error sync employee log: {ex}");
                }
            }
        }

        public async Task AutoSyncIntegrateEmloyeeManual(DateTime pNow)
        {
            var timePostCheck = DateTime.Now.ToHHmm();
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = 2 },
                new AddedParam { Key = "EventType", Value = ConfigAuto.EMPLOYEE_INTEGRATE.ToString() }
            };
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            if (downloadConfig != null)
            {

                var config = downloadConfig.FirstOrDefault();
                try
                {
                    if (config != null)
                    {
                        if (config.TimePos.Count() == 0) { return; }
                        if (_configClientName == ClientName.AEON.ToString())
                        {
                            await AutoGetAEONDepartment(config);
                        }
                        else if (_configClientName == ClientName.AVN.ToString())
                        {
                            await AutoGetAVNIntegrate(pNow, config);
                        }
                        else if (_configClientName == ClientName.OVN.ToString())
                        {
                            await AutoSyncEmployee(config);
                        }
                        else
                        {
                            await AutoSyncEmployeeStandard(config);
                        }
                        if (config.AlwaysSend)
                        {
                            string title = config.TitleEmailSuccess;
                            string bodyEmail = config.BodyEmailSuccess;
                            var isSuccess = _emailProvider.SendEmailToMulti("", title, bodyEmail, config.Email.Trim());
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (config.SendMailWhenError)
                    {
                        string title = config.TitleEmailError;
                        string bodyEmail = config.BodyEmailError;
                        var isSuccess = _emailProvider.SendEmailToMulti("", title, bodyEmail, config.Email.Trim());
                    }
                    _logger.LogError($"Error sync employee log: {ex}");
                }
            }
        }


        public async Task AutoSyncEmployeeStandard(IC_ConfigDTO config)

        {
            try
            {
                if (config.IntegrateLogParam.UsingDatabase)
                {
                    await AutoGetDepartmentStandard();

                    var listEmployeeSync = await _iC_Employee_IntegrateLogic.GetAllEmployeeIntegrateAsync();
                    if (listEmployeeSync != null && listEmployeeSync.Count > 0)
                    {
                        var employeeIntegrate = listEmployeeSync.Select(u => new EmployeeIntegrate
                        {
                            EmployeeATID = u.EmployeeATID,
                            CardNumber = u.CardNumber,
                            DepartmentCode = u.DepartmentCode,
                            EmployeeCode = u.EmployeeCode,
                            FullName = u.FullName,
                            Position = u.PositionCode,
                            StatusStandard = u.Status,
                            StoppedDate = u.DateQuit,
                            FromDate = u.StartedDate ?? DateTime.Now,
                            Note = u.Note
                        }).ToList();

                        var updateResult = new EmployeeIntegrateResult();
                        await UpdateEmployeeIntegrateStandard(employeeIntegrate, config.CompanyIndex, updateResult, _configClientName, config);
                        await AutoGetTransferUserStandard(config);
                        //write log
                        var strError = "";
                        for (int i = 0; i < updateResult.ListIndexError.Count; i++)
                        {
                            strError += $"{Environment.NewLine}Index: {updateResult.ListIndexError[i]}. Error: {updateResult.ListError[i]} ";
                        }
                        //_iC_Employee_IntegrateLogic.Update(updateResult);
                    }
                    else
                    {
                        _logger.LogError($"listEmployeeSync: {listEmployeeSync.Count}");
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sync employee log: {ex}");
            }

        }

        public async Task AutoGetAEONEmployee(List<UserDepartmentMappingsDto> userDepartmentMappings, IC_ConfigDTO config)
        {
            try
            {
                // Using Link API
                if (!string.IsNullOrWhiteSpace(_AppConfigAEONEmployee?.Domain) && !string.IsNullOrWhiteSpace(_AppConfigAEONEmployee?.Api) && !string.IsNullOrEmpty(_AppConfigAEONEmployee?.Token))
                {
                    List<WebAPIHeader> lstHeader = new List<WebAPIHeader>
                            {
                                new WebAPIHeader("Authorization", "Bearer " + _AppConfigAEONEmployee.Token)
                            };
                    HttpClient client = new HttpClient();
                    foreach (WebAPIHeader header in lstHeader)
                        client.DefaultRequestHeaders.Add(header.Name, header.Value);
                    string result = "";
                    HttpResponseMessage responseMessageGetData = await client.GetAsync(_AppConfigAEONEmployee.Domain + _AppConfigAEONEmployee.Api);
                    //responseMessageGetData.EnsureSuccessStatusCode();
                    result = await responseMessageGetData.Content.ReadAsStringAsync();
                    ItemEmployee itemEmployee = JsonConvert.DeserializeObject<ItemEmployee>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    if (itemEmployee != null && itemEmployee.Objects.Count > 0)
                    {
                        var listEmployeeIntegrate = itemEmployee.Objects.Data;
                        var updateResult = new EmployeeIntegrateAEONResult();
                        await UpdateEmployeeIntegrate(listEmployeeIntegrate, userDepartmentMappings, config.CompanyIndex, updateResult, config.EventType);
                        await AutoActiveESSAccount();
                        //write log
                        string strError = "";
                        for (int i = 0; i < updateResult.ListIndexError.Count; i++)
                        {
                            strError += $"{Environment.NewLine}Index: {updateResult.ListIndexError[i]}. Error: {updateResult.ListError[i]} ";
                        }

                        string content = JsonConvert.SerializeObject(updateResult);
                        var data = new StringContent(content, Encoding.UTF8, "application/json");
                        HttpResponseMessage responseMessageUpdateData = await client.PostAsync(config.IntegrateLogParam.LinkAPI + "/api/IntergratedEmployee/UpdateData", data);
                        responseMessageUpdateData.EnsureSuccessStatusCode();
                    }
                }
                else
                {
                    using (StreamReader r = new StreamReader("C:\\Users\\ADMIN\\Downloads\\response1.json"))
                    {
                        string json = r.ReadToEnd();
                        HttpClient client = new HttpClient();
                        ItemEmployee itemEmployee = JsonConvert.DeserializeObject<ItemEmployee>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                        if (itemEmployee != null && itemEmployee.Objects.Count > 0)
                        {
                            var listEmployeeIntegrate = itemEmployee.Objects.Data;
                            EmployeeIntegrateAEONResult updateResult = new EmployeeIntegrateAEONResult();
                            await UpdateEmployeeIntegrate(listEmployeeIntegrate, userDepartmentMappings, config.CompanyIndex, updateResult, config.EventType);
                            await AutoActiveESSAccount();
                            //write log
                            string strError = "";
                            for (int i = 0; i < updateResult.ListIndexError.Count; i++)
                            {
                                strError += $"{Environment.NewLine}Index: {updateResult.ListIndexError[i]}. Error: {updateResult.ListError[i]} ";
                            }

                            string content = JsonConvert.SerializeObject(updateResult);
                            var data = new StringContent(content, Encoding.UTF8, "application/json");
                            HttpResponseMessage responseMessageUpdateData = await client.PostAsync(config.IntegrateLogParam.LinkAPI + "/api/IntergratedEmployee/UpdateData", data);
                            responseMessageUpdateData.EnsureSuccessStatusCode();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sync AEON employee log: {ex}");
            }
        }



        public async Task AutoIntergrateAttLogAVN()
        {
            try
            {
                double previousHours = _configPreviousHoursIntergrateLog;
                var now = DateTime.Now;
                //Get previous data by hour
                //var fromDate = now.AddHours(-previousHours).ToString("yyyyMMdd HH:mm");
                var getAttendanceLogDBThirdParty = GetLogFromDBThirdParty();
                var getAttendanceLogDBHRPRO7 = GetLogFromDBHRPRO7();
                var getAttendanceLogDBHIK = GetLogFromDBHIK();


                var attendanceLogList = new List<IC_AttendanceLog>();
                attendanceLogList.AddRange(getAttendanceLogDBThirdParty);
                attendanceLogList.AddRange(getAttendanceLogDBHRPRO7);
                attendanceLogList.AddRange(getAttendanceLogDBHIK);


                attendanceLogList = attendanceLogList.GroupBy(x => new { x.EmployeeATID, x.CheckTime, x.InOutMode }).Select(x => x.First()).ToList();

                var timecheckLog = now.AddHours(-previousHours);
                var listAttendanceLog = _dbContext.IC_AttendanceLog.Where(x => x.CheckTime >= timecheckLog.Date).ToList();
                var employeeLst = (from us in _dbContext.HR_User.Where(x => !string.IsNullOrEmpty(x.EmployeeCode))
                                   join wi in _dbContext.HR_CardNumberInfo.Where(x => x.CardNumber != "0" && x.IsActive == true)
                                   on us.EmployeeATID equals wi.EmployeeATID into eWork
                                   from eWorkResult in eWork.DefaultIfEmpty()
                                   where eWorkResult != null
                                   select new
                                   {
                                       EmployeeATID = us.EmployeeATID,
                                       CardNumber = eWorkResult.CardNumber
                                   }).ToList();

                var exists = new List<IC_AttendanceLog>();
                foreach (var item in attendanceLogList)
                {
                    var time = item.CheckTime.FormatUnixTime();

                    //Compare data if don't exist then insert DB    
                    var logExist = listAttendanceLog.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID && x.CheckTime.FormatUnixTime() == time);
                    if (logExist == null)
                    {
                        logExist = new IC_AttendanceLog();
                        logExist.EmployeeATID = item.EmployeeATID;
                        logExist.CheckTime = time;
                        logExist.InOutMode = item.InOutMode;
                        if (item.SerialNumber != null)
                        {
                            logExist.SerialNumber = item.SerialNumber;
                        }
                        else if (!string.IsNullOrEmpty(_configNameSerialNumber_IN) && logExist.InOutMode == (short)(InOutMode.Input))
                        {
                            logExist.SerialNumber = _configNameSerialNumber_IN;
                        }
                        else if (!string.IsNullOrEmpty(_configNameSerialNumber_OUT) && logExist.InOutMode == (short)(InOutMode.Output))
                        {
                            logExist.SerialNumber = _configNameSerialNumber_OUT;
                        }
                        logExist.UpdatedDate = now;
                        logExist.CompanyIndex = 2;
                        logExist.UpdatedUser = item.UpdatedUser;
                        if (item.UpdatedUser == UpdatedUser.UserSystem.ToString())
                        {
                            var employee = employeeLst.FirstOrDefault(x => x.CardNumber != null && x.CardNumber == item.EmployeeATID.TrimStart(new Char[] { '0' }));
                            var card = employee != null ? employee.EmployeeATID : item.EmployeeATID;
                            var logExistIntegrate = listAttendanceLog.FirstOrDefault(x => x.EmployeeATID == card && x.CheckTime.FormatUnixTime() == time);
                            if (logExistIntegrate == null)
                            {
                                logExist.EmployeeATID = employee != null ? employee.EmployeeATID : item.EmployeeATID;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (exists.Count(x => x.EmployeeATID == logExist.EmployeeATID && x.CheckTime.FormatUnixTime() == logExist.CheckTime) > 0)
                        {
                            continue;
                        }

                        if (attendanceLogList.Count(x => x.EmployeeATID == logExist.EmployeeATID && x.CheckTime.FormatUnixTime() == logExist.CheckTime) > 1)
                        {
                            exists.Add(logExist);
                        }
                        try
                        {
                            _dbContext.IC_AttendanceLog.Add(logExist);
                        }
                        catch
                        {
                        }
                    }
                }
                var check = _dbContext.SaveChanges();
                var logSend = _dbContext.IC_AttendanceLog.Where(x => x.CheckTime >= timecheckLog && x.IsSend == null).ToList();
                if (check > 0 || (logSend != null && logSend.Count > 0))
                {
                    //Send attendancelog eGCS.
                    if (!string.IsNullOrEmpty(mLinkGCSMonitoringApi) && logSend.Count() > 0)
                    {
                        await SendTimeLogToGCS_AVN(logSend, mLinkGCSMonitoringApi + "/api/Business/AddAttendanceLogRealTime_AVN");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoGetIntergrateLog: {ex}");
            }
        }

        private async Task UpdateEmployeeShift(List<Employee_Shift_Integrate> request)
        {
            try
            {
                var history = new IC_HistoryTrackingIntegrate();
                history.JobName = "AutoGetOVNEmployeeShift IC_Employee_Shift_Integrate";

                var currentEmployeeShifts = await _dbSyncContext.IC_Employee_Shift_Integrate.ToListAsync();

                _dbSyncContext.IC_Employee_Shift_Integrate.RemoveRange(currentEmployeeShifts);
                history.DataDelete = (short)(currentEmployeeShifts != null ? currentEmployeeShifts.Count() : 0);
                var employeeShifts = new List<IC_Employee_Shift_Integrate>();
                foreach (var item in request)
                {
                    var employeeShift = _Mapper.Map<Employee_Shift_Integrate, IC_Employee_Shift_Integrate>(item);
                    await _dbSyncContext.IC_Employee_Shift_Integrate.AddAsync(employeeShift);
                    history.DataNew = history.DataNew++;
                }
                await _dbSyncContext.SaveChangesAsync();
                await AddHistoryTrackingIntegrate(history);
                //Handle on Shift data first and then process for Employee Shift, to make sure we have an Shift ID
                var newShifts = request.Select(x => x.ShiftName).Distinct().ToList();

                var existShifts = await _dbContext.IC_Shift.AsNoTracking().ToListAsync();

                var historyValueShift = new IC_HistoryTrackingIntegrate();
                historyValueShift.JobName = "AutoGetOVNEmployeeShift IC_Shift";

                foreach (var item in newShifts)
                {
                    if (!existShifts.Any(x => x.Name.ToLower() == item.ToLower()))
                    {
                        var es = request.OrderByDescending(x => x.ShiftApplyDate).FirstOrDefault(x => x.ShiftName == item);
                        var shift = new IC_Shift
                        {
                            Name = es.ShiftName,
                            StartTime = es.ShiftFromTime,
                            EndTime = es.ShiftToTime,
                            ApplyDate = es.ShiftApplyDate,
                            CompanyIndex = 2,
                            UpdatedDate = es.ModifiedDate,
                            ShiftDate = es.ShiftDate
                        };

                        _dbContext.IC_Shift.Add(shift);
                        await _dbContext.SaveChangesAsync();
                        existShifts.Add(shift);
                        historyValueShift.DataNew = historyValueShift.DataNew++;
                    }
                    //_logger.LogError($"item {item}");
                }
                await AddHistoryTrackingIntegrate(historyValueShift);
                var listEmployeeShifts = new List<IC_Employee_Shift>();
                //var listUpdatedShifts = await _employee_Shift_IntegrateLogic.GetAllShiftsAsync();

                foreach (var es in request)
                {
                    ////Delete Shifts are not available
                    //var deletedShifts = existShifts.Except(request, new ShiftNameEqualityComparer());
                    //if (deletedShifts.Any())
                    //{
                    //    //Delete related employee shifts
                    //    var deletedShiftIds = deletedShifts.Select(x => x.Id);
                    //    var listEmployeeShifts = await _dbContext.IC_Employee_Shift.Where(e => deletedShiftIds.Contains(e.IC_ShiftId)).ToListAsync();
                    //    _logger.LogError($"listEmployeeShifts {listEmployeeShifts.Count()}");

                    //    if (listEmployeeShifts.Any())
                    //    {
                    //        _dbContext.IC_Employee_Shift.RemoveRange(listEmployeeShifts);
                    //        await _dbContext.SaveChangesAsync();
                    //    }
                    //    _dbContext.IC_Shift.RemoveRange(deletedShifts);
                    //}

                    ////Update shifts have existed
                    //var modifiedShifts = request.Except(newShifts, new ShiftNameEqualityComparer());
                    //foreach (var shift in modifiedShifts)
                    //{
                    //    var existedShift = existShifts.FirstOrDefault(s => s.Name.Equals(shift.Name, StringComparison.OrdinalIgnoreCase));

                    //    if (existedShift != null)
                    //    {
                    //        existedShift.ShiftDate = shift.ShiftDate;
                    //        existedShift.StartTime = shift.StartTime;
                    //        existedShift.EndTime = shift.EndTime;
                    //        existedShift.ApplyDate = shift.ApplyDate;
                    //        existedShift.UpdatedDate = DateTime.Now;
                    //        _dbContext.Update(existedShift);
                    //    }
                    //}

                    listEmployeeShifts.Add(new IC_Employee_Shift
                    {
                        EmployeeATID = es.EmployeeId,
                        CompanyIndex = 2,
                        CreatedDate = DateTime.Now,
                        ShippedDate = es.ShiftDate,
                        ShiftFromTime = es.ShiftFromTime,
                        ShiftToTime = es.ShiftToTime,
                        ShiftApplyDate = es.ShiftApplyDate,
                        IC_ShiftId = existShifts.FirstOrDefault(x => x.Name.ToLower() == es.ShiftName.ToLower()).Id
                    });
                }

                await _employee_Shift_IntegrateLogic.UpdateEmployeeShiftsAsync(listEmployeeShifts, existShifts);
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateEmployeeShift {ex}");
            }
        }

        private async Task SendTimeLogToGCS_AVN(List<IC_AttendanceLog> logParam, string gcsEndpoint)
        {
            var client = _ClientFactory.CreateClient();
            string mCommunicateToken = _Configuration.GetValue<string>("CommunicateToken");
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, gcsEndpoint);
                request.Headers.Add("api-token", mCommunicateToken);
                var jsonData = JsonConvert.SerializeObject(logParam);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                //Update status when sending log is successful
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    double timer = _configPreviousHoursIntergrateLog;
                    var checkTime = DateTime.Now.AddHours(-timer);
                    var getLog = _dbContext.IC_AttendanceLog.Where(x => x.CheckTime >= checkTime && x.IsSend == null).ToList();
                    foreach (var item in logParam)
                    {
                        var logUpdate = getLog.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID && x.CheckTime == item.CheckTime && x.InOutMode == item.InOutMode);
                        if (logUpdate != null)
                        {
                            logUpdate.IsSend = true;
                        }
                    }
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendTimeLogToGCS_AVN: {ex}");
            }
        }

        public async Task AutoGetAEONDepartment(IC_ConfigDTO config)
        {
            try
            {

                if (!string.IsNullOrWhiteSpace(_AppConfigAEONDepartment?.Domain) && !string.IsNullOrWhiteSpace(_AppConfigAEONDepartment?.Api) && !string.IsNullOrEmpty(_AppConfigAEONDepartment?.Token))
                {
                    List<WebAPIHeader> lstHeader = new List<WebAPIHeader>
                            {
                                new WebAPIHeader("Authorization", "Bearer " + _AppConfigAEONDepartment.Token)
                            };
                    HttpClient client = new HttpClient();
                    foreach (WebAPIHeader header in lstHeader)
                        client.DefaultRequestHeaders.Add(header.Name, header.Value);
                    string result = "";
                    HttpResponseMessage responseMessageGetData = await client.GetAsync(_AppConfigAEONDepartment.Domain + _AppConfigAEONDepartment.Api);
                    //responseMessageGetData.EnsureSuccessStatusCode();
                    result = await responseMessageGetData.Content.ReadAsStringAsync();
                    var departmentFromJson = HandleDepartmentFromJson(result);
                    await OldDepartmentUpdateAEON(config.CompanyIndex, departmentFromJson.Departments);
                    UpdateDepartmentIntegrate(departmentFromJson, config.CompanyIndex);

                    //UpdateSyncDepartmentIntegrate(departmentFromJson, config.CompanyIndex);
                    AddDepartmentOnCache();
                    await AutoGetAEONEmployee(departmentFromJson.UserDepartmentMappings, config);
                }
                else
                {
                    using (StreamReader r = new StreamReader("C:\\Users\\ADMIN\\Downloads\\response.json"))
                    {
                        string json = r.ReadToEnd();
                        var departmentFromJson = HandleDepartmentFromJson(json);
                        await OldDepartmentUpdateAEON(config.CompanyIndex, departmentFromJson.Departments);
                        UpdateDepartmentIntegrate(departmentFromJson, config.CompanyIndex);
                        //UpdateSyncDepartmentIntegrate(departmentFromJson, config.CompanyIndex);
                        AddDepartmentOnCache();
                        await AutoGetAEONEmployee(departmentFromJson.UserDepartmentMappings, config);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sync AEON employee log: {ex}");
            }
        }

        public async Task AutoGetDataToDatabase(DateTime now)
        {
            string timePostCheck = now.ToHHmm();
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = 2 },
                new AddedParam { Key = "EventType", Value = ConfigAuto.EMPLOYEE_INTEGRATE_TO_DATABASE.ToString() }
            };
            try
            {
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                if (downloadConfig != null && downloadConfig.Any())
                {
                    var config = downloadConfig.First();
                    if (config != null)
                    {
                        if (!config.TimePos.Contains(timePostCheck)) { return; }

                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(config.CustomField);

                        if (param.SoftwareType == (int)SoftwareType.File)
                        {
                            try
                            {
                                await PostDepartmentToDatabaseFromFile(config);
                                await PostTransferUserToDatabaseFromFile(config);
                                await AutoSyncEmployeeToDatabaseFromFile(config);
                                if (config.AlwaysSend)
                                {
                                    string title = config.TitleEmailSuccess;
                                    string bodyEmail = config.BodyEmailSuccess;
                                    var isSuccess = _emailProvider.SendEmailToMulti("", title, bodyEmail, config.Email.Trim());
                                }
                            }
                            catch (Exception ex)
                            {
                                if (config.SendMailWhenError)
                                {
                                    string title = config.TitleEmailError;
                                    string bodyEmail = config.BodyEmailError;
                                    var isSuccess = _emailProvider.SendEmailToMulti("", title, bodyEmail, config.Email.Trim());
                                    _logger.LogError($"EMPLOYEE_INTEGRATE_TO_DATABASE: {ex}");
                                }

                            }

                        }
                        else if (param.SoftwareType == (int)SoftwareType.Office)
                        {
                            await PostDepartmentToDatabase1Office(config);
                            await AutoSyncEmployeeToDatabase1Office(config);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoGetOVNDepartment: {ex}");
            }
        }

        public async Task AutoGet1OfficeToDatabaseManual(DateTime now)
        {
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = 2 },
                new AddedParam { Key = "EventType", Value = ConfigAuto.EMPLOYEE_INTEGRATE_TO_DATABASE.ToString() }
            };
            try
            {
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                if (downloadConfig != null && downloadConfig.Any())
                {
                    var config = downloadConfig.First();
                    if (config != null)
                    {
                        if (config.TimePos.Count() == 0) { return; }

                        var param = JsonConvert.DeserializeObject<IntegrateLogParam>(config.CustomField);

                        if (param.SoftwareType == (int)SoftwareType.File)
                        {
                            try
                            {
                                await PostDepartmentToDatabaseFromFile(config);
                                await PostTransferUserToDatabaseFromFile(config);
                                await AutoSyncEmployeeToDatabaseFromFile(config);
                                if (config.AlwaysSend)
                                {
                                    string title = config.TitleEmailSuccess;
                                    string bodyEmail = config.BodyEmailSuccess;
                                    var isSuccess = _emailProvider.SendEmailToMulti("", title, bodyEmail, config.Email.Trim());
                                }
                            }
                            catch (Exception ex)
                            {
                                if (config.SendMailWhenError)
                                {
                                    string title = config.TitleEmailError;
                                    string bodyEmail = config.BodyEmailError;
                                    var isSuccess = _emailProvider.SendEmailToMulti("", title, bodyEmail, config.Email.Trim());
                                    _logger.LogError($"EMPLOYEE_INTEGRATE_TO_DATABASE: {ex}");
                                }

                            }
                        }
                        else if (param.SoftwareType == (int)SoftwareType.Office)
                        {
                            await PostDepartmentToDatabase1Office(config);
                            await AutoSyncEmployeeToDatabase1Office(config);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoGetOVNDepartment: {ex}");
            }
        }

        public async Task AutoPostLogFromDatabase(DateTime pNow)
        {
            string timePostCheck = pNow.ToHHmm();
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = 2 },
                new AddedParam { Key = "EventType", Value = ConfigAuto.LOG_INTEGRATE_TO_DATABASE.ToString() }
            };
            try
            {
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                if (downloadConfig != null && downloadConfig.Any())
                {
                    var config = downloadConfig.First();
                    if (config != null)
                    {
                        if (!config.TimePos.Contains(timePostCheck)) { return; }
                        await PostLogTo1Office(config);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoPostLogFromDatabase: {ex}");
            }
        }

        public async Task AutoPostLogFromDatabaseManual()
        {
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = 2 },
                new AddedParam { Key = "EventType", Value = ConfigAuto.LOG_INTEGRATE_TO_DATABASE.ToString() }
            };
            try
            {
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                if (downloadConfig != null && downloadConfig.Any())
                {
                    var config = downloadConfig.First();
                    if (config != null)
                    {
                        if (config.TimePos.Count() == 0) { return; }
                        await PostLogTo1Office(config);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoPostLogFromDatabase: {ex}");
            }
        }


        private async Task PostLogTo1Office(IC_ConfigDTO config)
        {
            try
            {
                var listLogIntegrate = await _iC_Employee_IntegrateLogic.GetAllAttendancelogNotSendAsync();
                if (listLogIntegrate != null && listLogIntegrate.Count > 0)
                {
                    var listIndex = listLogIntegrate.Select(x => x.Index).ToList();
                    var listEmployees = listLogIntegrate.Select(x => x.EmployeeATID).Distinct();
                    var employees = await _dbContext.HR_User.Where(x => listEmployees.Contains(x.EmployeeATID)).ToListAsync();
                    var listLog = new List<AttendancelogDataTo1Office>();
                    foreach (var item in listLogIntegrate)
                    {
                        var user = employees.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
                        if (user != null)
                        {
                            var log = new AttendancelogDataTo1Office()
                            {
                                code = user.EmployeeCode,
                                time = item.CheckTime.ToString("yyyy-MM-dd HH:mm:ss")
                            };
                            listLog.Add(log);
                        }
                    }
                    var listSplitLogDb = CommonUtils.SplitList(listLog, 300);
                    foreach (var item in listSplitLogDb)
                    {
                        var jsons = JsonConvert.SerializeObject(item);
                        var formContent = new FormUrlEncodedContent(new[]
                        {
                        new KeyValuePair<string, string>("key", config.IntegrateLogParam.Token),
                        new KeyValuePair<string, string>("data", jsons)
                    });


                        var client = new HttpClient();
                        client.Timeout = TimeSpan.FromMinutes(10);
                        var responseMessageGetData = await client.PostAsync(config.IntegrateLogParam.LinkAPIIntegrate + "/timekeep/attendance/service", formContent);
                        var result = await responseMessageGetData.Content.ReadAsStringAsync();
                        Thread.Sleep(15000);
                    }

                    if (config.AlwaysSend)
                    {
                        var icConfig = new IC_Config();
                        icConfig.AlwaysSend = config.AlwaysSend;
                        icConfig.BodyEmailError = config.BodyEmailError;
                        icConfig.BodyEmailSuccess = config.BodyEmailSuccess;
                        icConfig.CompanyIndex = config.CompanyIndex;
                        icConfig.CustomField = config.CustomField;
                        icConfig.Email = config.Email;
                        icConfig.EventType = config.EventType;
                        _emailProvider.SendMailIntegrateLog(icConfig, listLog.Count(), new List<string>(), listLog.Count(), new List<string>());

                    }

                }
                else
                {
                    _logger.LogError($"listEmployeeSync: {listLogIntegrate.Count}");
                }
            }
            catch (Exception ex)
            {
                if (config.SendMailWhenError)
                {
                    string title = config.TitleEmailError;
                    string bodyEmail = config.BodyEmailError;
                    var isSuccess = _emailProvider.SendEmailToMulti("", title, bodyEmail, config.Email.Trim());
                }
                _logger.LogError($"PostLogTo1Office: {ex}");
            }
        }


        private async Task PostDepartmentToDatabase1Office(IC_ConfigDTO config)
        {
            try
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(10);
                var responseMessageGetData = await client.GetAsync(config.IntegrateLogParam.LinkAPIIntegrate + "/api/admin/department/gets?access_token=" + config.IntegrateLogParam.Token + "&limit=10000");
                var result = await responseMessageGetData.Content.ReadAsStringAsync();
                var itemDepartment = JsonConvert.DeserializeObject<IC_Department1Office_Return>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var departments = new List<IC_DepartmentIntegrate>();


                if (itemDepartment != null && itemDepartment.Data != null)
                {
                    foreach (var item in itemDepartment.Data)
                    {
                        departments.Add(new IC_DepartmentIntegrate
                        {
                            Code = item.ID,
                            Name = item.Title,
                            ParentCode = item.ParentID,
                            Status = true
                        });
                    }
                    var client1 = new HttpClient();
                    client1.BaseAddress = new Uri(_config.IntegrateEmployeeLink);
                    client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                    var json = JsonConvert.SerializeObject(departments);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    try
                    {
                        HttpResponseMessage response = await client1.PostAsync("api/Department/Post_Department", content);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.EnsureSuccessStatusCode();
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"EMPLOYEE_INTEGRATE_TO_DATABASE: {ex}");
                    }

                    if (config.AlwaysSend)
                    {
                        string title = config.TitleEmailSuccess;
                        string bodyEmail = config.BodyEmailSuccess;
                        var isSuccess = _emailProvider.SendEmailToMulti("", title, bodyEmail, config.Email.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                if (config.SendMailWhenError)
                {
                    string title = config.TitleEmailError;
                    string bodyEmail = config.BodyEmailError;
                    var isSuccess = _emailProvider.SendEmailToMulti("", title, bodyEmail, config.Email.Trim());
                }
                _logger.LogError($"{ex}");
            }
        }

        private async Task PostDepartmentToDatabaseFromFile(IC_ConfigDTO config)
        {
            if (!string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate) && !string.IsNullOrEmpty(config.IntegrateLogParam.UserName) && !string.IsNullOrEmpty(config.IntegrateLogParam.Password))
            {
                string remoteDirectory = "/outgoing/THS/Zone/";
                var departments = new List<IC_DepartmentIntegrate>();

                using (var sftp = new SftpClient(config.IntegrateLogParam.LinkAPIIntegrate, config.IntegrateLogParam.UserName, config.IntegrateLogParam.Password))
                {
                    sftp.Connect();
                    var files = sftp.ListDirectory(remoteDirectory);
                    foreach (var file in files)
                    {
                        string remoteFileName = file.Name;
                        if (remoteFileName != "psv_zone.csv")
                        {
                            continue;
                        }
                        using (var remoteFileStream = sftp.OpenRead(remoteDirectory + remoteFileName))
                        {
                            using (var reader = new StreamReader(remoteFileStream))
                            {
                                var i = 0;
                                while (!reader.EndOfStream)
                                {
                                    var line = reader.ReadLine();
                                    if (i == 0)
                                    {
                                        i++;
                                        continue;
                                    }

                                    string[] values = line.Split(',');

                                    try
                                    {
                                        departments.Add(new IC_DepartmentIntegrate
                                        {
                                            Code = values[0],
                                            Name = values[1],
                                            ParentCode = values[2],
                                            Status = values[3] == "active" ? true : false
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"PostDepartmentToDatabaseFromFile:", ex);
                                    }
                                    i++;
                                }

                            }
                        }
                    }
                    var client1 = new HttpClient();
                    client1.BaseAddress = new Uri(_config.IntegrateEmployeeLink);
                    client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                    var json = JsonConvert.SerializeObject(departments);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    try
                    {
                        HttpResponseMessage response = await client1.PostAsync("api/Department/Post_Department", content);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"EMPLOYEE_INTEGRATE_TO_DATABASE: {ex}");
                    }
                }
            }
        }


        private async Task PostTransferUserToDatabaseFromFile(IC_ConfigDTO config)
        {
            if (!string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate) && !string.IsNullOrEmpty(config.IntegrateLogParam.UserName) && !string.IsNullOrEmpty(config.IntegrateLogParam.Password))
            {
                string remoteDirectory = "/outgoing/THS/Travel/";
                var transferUer = new List<IC_TransferUserIntegrate>();

                using (var sftp = new SftpClient(config.IntegrateLogParam.LinkAPIIntegrate, config.IntegrateLogParam.UserName, config.IntegrateLogParam.Password))
                {
                    sftp.Connect();
                    var files = sftp.ListDirectory(remoteDirectory);
                    foreach (var file in files)
                    {
                        string remoteFileName = file.Name;
                        if (remoteFileName != "empTravel.csv")
                        {
                            continue;
                        }
                        using (var remoteFileStream = sftp.OpenRead(remoteDirectory + remoteFileName))
                        {
                            using (var reader = new StreamReader(remoteFileStream))
                            {
                                var i = 0;
                                while (!reader.EndOfStream)
                                {
                                    var line = reader.ReadLine();
                                    if (i == 0)
                                    {
                                        i++;
                                        continue;
                                    }

                                    string[] values = line.Split(',');

                                    try
                                    {
                                        transferUer.Add(new IC_TransferUserIntegrate
                                        {
                                            EmployeeATID = values[0],
                                            DepartmentTo = values[1],
                                            FromDate = Convert.ToDateTime(values[2]),
                                            ToDate = !string.IsNullOrEmpty(values[3]) ? Convert.ToDateTime(values[3]) : (DateTime?)null
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"PostTransferUserToDatabaseFromFile:", ex);
                                    }
                                    i++;
                                }

                            }
                        }
                    }
                    var client1 = new HttpClient();
                    client1.BaseAddress = new Uri(_config.IntegrateEmployeeLink);
                    client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                    var json = JsonConvert.SerializeObject(transferUer);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    try
                    {
                        HttpResponseMessage response = await client1.PostAsync("api/TransferUser/Post_TransferUser", content);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.EnsureSuccessStatusCode();
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"EMPLOYEE_INTEGRATE_TO_DATABASE: {ex}");
                    }
                }
            }
        }
        public async Task<int> OldDepartmentUpdateAEON(int companyIndex, List<IC_Department_Integrate_AEON_Dto> employeeIntegrateList)
        {
            try
            {
                var ids = employeeIntegrateList.Select(x => x.Id).Distinct().ToList();
                int row = 0;
                var result = await _dbContext.IC_Department
                    .Where(u => u.CompanyIndex == companyIndex && !ids.Contains(u.Code) && u.Code != null)
                    .ToListAsync();

                if (result.Any())
                {
                    foreach (var item in result)
                    {

                        item.IsInactive = true;
                        item.UpdatedDate = DateTime.Now;

                        row++;
                    }
                    _dbContext.IC_Department.UpdateRange(result);
                    await _dbContext.SaveChangesAsync();
                }
                return row;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            return 0;
        }

        public void AddDepartmentOnCache()
        {
            var departmentList = _dbContext.IC_Department.ToList();

            var departmentIndexByConfig = departmentList.Where(x => x.IsStore == true && x.JobGradeGrade == 5).ToList();
            if (departmentIndexByConfig != null && departmentIndexByConfig.Count > 0)
            {
                var listDepartment = new List<IC_DepartmentParentModel>();
                foreach (var item in departmentIndexByConfig)
                {
                    var departmentChildren = GetChildren(departmentList, item.Index);
                    var iC_DepartmentParent = new IC_DepartmentParentModel()
                    {
                        DepartmentParentIndex = item.Index,
                        DepartmentParentName = item.Name,
                        DepartmentIndexList = departmentChildren.Select(x => x.Index).ToList()
                    };
                    listDepartment.Add(iC_DepartmentParent);
                }

                _cache.Set<List<IC_DepartmentParentModel>>("urn:IC_Department", listDepartment.ToList(), TimeSpan.FromHours(24));
            }
        }
        private List<IC_Department> GetChildren(List<IC_Department> foos, int? id)
        {
            return foos
                .Where(x => x.ParentIndex == id)
                .Union(foos.Where(x => x.ParentIndex == id)
                    .SelectMany(y => GetChildren(foos, y.Index))
                ).ToList();
        }

        private DepartmentFromJson HandleDepartmentFromJson(string json)
        {
            var itemEmployee = JsonConvert.DeserializeObject<DepartmentItem>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var itemDepartmentLst = new List<IC_Department_Integrate_AEON_Dto>();
            var item = itemEmployee.Objects.Data[0];
            var itemUserDepartmentLst = new List<UserDepartmentMappingsDto>();

            Queue<IC_Department_Integrate_AEON> q = new Queue<IC_Department_Integrate_AEON>();
            q.Enqueue(item);
            while (q.Count != 0)
            {
                int n = q.Count;

                while (n > 0)
                {
                    IC_Department_Integrate_AEON p = q.Peek();
                    q.Dequeue();
                    var department = _Mapper.Map<IC_Department_Integrate_AEON, IC_Department_Integrate_AEON_Dto>(p);
                    itemDepartmentLst.Add(department);
                    if (p.UserDepartmentMappings != null && p.UserDepartmentMappings.Count > 0)
                    {
                        for (int j = 0; j < p.UserDepartmentMappings.Count; j++)
                        {
                            if (p.UserDepartmentMappings[j].IsHeadCount == true)
                            {
                                var userDepartmentMapping = new UserDepartmentMappingsDto()
                                {
                                    DepartmentId = p.Id,
                                    UserId = p.UserDepartmentMappings[j].UserSAPCode
                                };
                                itemUserDepartmentLst.Add(userDepartmentMapping);
                            }

                        }
                    }
                    for (int i = 0; i < p.Items.Count; i++)
                    {
                        q.Enqueue(p.Items[i]);
                    }
                    n--;
                }
            }
            var DepartmentFromJson = new DepartmentFromJson()
            {
                Departments = itemDepartmentLst,
                UserDepartmentMappings = itemUserDepartmentLst
            };

            return DepartmentFromJson;

        }

        public void AutoUpdateLastConnectionOfFR05()
        {
            List<string> lsSerialNumber = new List<string>();
            var FR05Machine = _dbContext.IC_Device.Where(x => x.CompanyIndex == _config.CompanyIndex &&
           !string.IsNullOrEmpty(x.DeviceModel) && Convert.ToInt32(x.DeviceModel) == (int)ProducerEnum.FR05).ToList();

            foreach (var item in FR05Machine)
            {
                var checkItem = CheckPing(item);
                if (item.SerialNumber != null && item.SerialNumber != string.Empty && checkItem == true)
                {
                    lsSerialNumber.Add(item.SerialNumber);

                }
            }

            _iC_SystemCommandLogic.UpdateLastConnection(lsSerialNumber, _config.CompanyIndex);
        }


        private bool CheckPing(IC_Device item)
        {
            System.Net.NetworkInformation.Ping ping = new Ping();
            PingReply pingresult = ping.Send(item.IPAddress, 1000);
            bool pingIp = pingresult.Status.ToString().Equals("Success");
            return pingIp;
        }

        private List<IC_AttendanceLog> GetLogFromDBThirdParty()
        {
            try
            {
                //set the connection string
                string connString = _thirdPartyIntegrationConfigurationService.GetConnectionString;

                if (string.IsNullOrEmpty(connString))
                {
                    return new List<IC_AttendanceLog>();
                }

                //sql connection object
                using SqlConnection conn = new SqlConnection(connString);

                //Get config interval second in appsetting.json
                double previousHours = _configPreviousHoursIntergrateLog;

                var now = DateTime.Now;
                //Get previous data by hour
                var fromDate = now.AddHours(-previousHours).ToString("yyyyMMdd HH:mm");

                string querySelectLog = "SELECT [CARD_NO], [EVENT_TIME], [READER_STATE] FROM  [dbo].[ACC_TRANSACTION] WHERE [CARD_NO] <> '' " +
                    "AND READER_STATE IN (0,1) AND DEV_SN IN ('BRIC210660020', 'BRIC210660003') " +
                    "AND EVENT_TIME BETWEEN '" + fromDate + "' AND GETDATE() GROUP BY [CARD_NO], [EVENT_TIME], [READER_STATE]";
                SqlCommand command = new SqlCommand(querySelectLog, conn);
                conn.Open();

                SqlDataReader reader = command.ExecuteReader();
                var attendanceLogList = new List<IC_AttendanceLog>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var listLog = new IC_AttendanceLog();
                        listLog.EmployeeATID = (reader.GetString(0)).PadLeft(_config.MaxLenghtEmployeeATID, '0');
                        listLog.CheckTime = reader.GetDateTime(1);
                        listLog.InOutMode = reader.GetInt16(2);
                        listLog.UpdatedUser = UpdatedUser.UserSystem.ToString();
                        attendanceLogList.Add(listLog);
                    }
                }
                //close data reader
                reader.Close();
                //close SqlCommand
                conn.Close();
                return attendanceLogList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetLogFromDBThirdParty: {ex}");
                throw;
            }
        }

        private List<IC_AttendanceLog> GetLogFromDBHRPRO7()
        {
            try
            {
                var users = _dbContext.HR_User.Where(x => !string.IsNullOrEmpty(x.EmployeeCode)).ToList();
                //set the connection string
                string connString = _thirdPartyIntegrationConfigurationService.GetConnectionStringHRPRO7;
                if (string.IsNullOrEmpty(connString))
                {
                    return new List<IC_AttendanceLog>();
                }
                //sql connection object
                using SqlConnection conn = new SqlConnection(connString);

                //Get config interval second in appsetting.json
                double previousHours = _configPreviousHoursIntergrateLog;

                var now = DateTime.Now;
                //Get previous data by hour
                var fromDate = now.AddHours(-previousHours).ToString("yyyyMMdd HH:mm");

                string querySelectLog = "select A2.[EmployeeCode], [Time], [InOutMode] from TA_TimeLog a1 " +
                    "LEFT JOIN HR_Employee a2 on a1.EmployeeATID = a2.EmployeeATID WHERE A1.[EmployeeATID] <> ''  AND  A2.[EmployeeCode] <> '' " +
                    "AND TIME BETWEEN '" + fromDate + "' AND GETDATE()  GROUP BY A2.[EmployeeCode], [Time], [InOutMode]";
                SqlCommand command = new SqlCommand(querySelectLog, conn);
                conn.Open();

                SqlDataReader reader = command.ExecuteReader();
                var attendanceLogList = new List<IC_AttendanceLog>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var listLog = new IC_AttendanceLog();
                        listLog.EmployeeATID = reader.GetString(0);
                        listLog.CheckTime = reader.GetDateTime(1);
                        var user = users.FirstOrDefault(x => x.EmployeeCode == listLog.EmployeeATID);
                        if (user != null)
                        {
                            listLog.EmployeeATID = user.EmployeeATID;
                        }
                        if (reader.GetInt16(2) == 1)
                        {
                            listLog.InOutMode = (short)InOutMode.Input;
                        }
                        else if (reader.GetInt16(2) == 2)
                        {
                            listLog.InOutMode = (short)InOutMode.Output;
                        }
                        if (!string.IsNullOrEmpty(_configNameSerialNumberHRPRO7_IN) && listLog.InOutMode == (short)InOutMode.Input)
                        {
                            listLog.SerialNumber = _configNameSerialNumberHRPRO7_IN;
                        }
                        else if (!string.IsNullOrEmpty(_configNameSerialNumberHRPRO7_OUT) && listLog.InOutMode == (short)InOutMode.Output)
                        {
                            listLog.SerialNumber = _configNameSerialNumberHRPRO7_OUT;
                        }
                        listLog.UpdatedUser = "IntergrateLog";
                        attendanceLogList.Add(listLog);
                    }
                }
                //close data reader
                reader.Close();
                //close SqlCommand
                conn.Close();
                return attendanceLogList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetLogFromDBHRPRO7: {ex}");
                throw;
            }
        }
        private List<IC_AttendanceLog> GetLogFromDBHIK()
        {
            try
            {
                //set the connection string
                string connString = _thirdPartyIntegrationConfigurationService.GetConnectionStringHIK;
                if (string.IsNullOrEmpty(connString))
                {
                    return new List<IC_AttendanceLog>();
                }

                //sql connection object
                using SqlConnection conn = new SqlConnection(connString);

                //Get config interval second in appsetting.json
                double previousHours = _configPreviousHoursIntergrateLog;

                var now = DateTime.Now;
                //Get previous data by hour
                var fromDate = now.AddHours(-previousHours).ToString("yyyyMMdd HH:mm");

                string querySelectLog = "SELECT [ID], [DateTime], [DeviceName], [Serial] FROM  [dbo].[att] WHERE [ID] IS NOT NULL " +
                    "AND DateTime BETWEEN '" + fromDate + "' AND GETDATE() GROUP BY [ID], [DateTime], [DeviceName], [Serial]";
                SqlCommand command = new SqlCommand(querySelectLog, conn);
                conn.Open();

                SqlDataReader reader = command.ExecuteReader();
                var attendanceLogList = new List<IC_AttendanceLog>();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var listLog = new IC_AttendanceLog();
                        listLog.EmployeeATID = (reader.GetString(0)).PadLeft(_config.MaxLenghtEmployeeATID, '0'); ;
                        listLog.CheckTime = reader.GetDateTime(1);
                        if (reader.GetString(2) == "Check IN")
                        {
                            listLog.InOutMode = (short)InOutMode.Input;
                        }
                        else if (reader.GetString(2) == "CHECK OUT")
                        {
                            listLog.InOutMode = (short)InOutMode.Output;
                        }
                        listLog.SerialNumber = reader.GetString(2);
                        listLog.UpdatedUser = UpdatedUser.UserSystem.ToString();
                        attendanceLogList.Add(listLog);
                    }
                }
                //close data reader
                reader.Close();
                //close SqlCommand
                conn.Close();
                return attendanceLogList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetLogFromDBHIK: {ex}");
                throw;
            }
        }
        public async Task AddHistoryTrackingIntegrate(IC_HistoryTrackingIntegrate iC_HistoryTrackingIntegrate)
        {
            try
            {
                var historyData = new IC_HistoryTrackingIntegrate()
                {
                    JobName = iC_HistoryTrackingIntegrate.JobName,
                    RunTime = DateTime.Now,
                    DataNew = iC_HistoryTrackingIntegrate.DataNew,
                    DataUpdate = iC_HistoryTrackingIntegrate.DataUpdate,
                    DataDelete = iC_HistoryTrackingIntegrate.DataDelete,
                    CompanyIndex = _config.CompanyIndex,
                    Reason = iC_HistoryTrackingIntegrate.Reason,
                    IsSuccess = iC_HistoryTrackingIntegrate.IsSuccess
                };
                await _dbContext.IC_HistoryTrackingIntegrate.AddAsync(historyData);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }

        public async Task AutoGetAVNIntegrate(DateTime now, IC_ConfigDTO config)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_AppConfigAVN?.Domain))
                {
                    var token = await GetToken();
                    var lstHeader = new List<WebAPIHeader>
                                { new WebAPIHeader("Authorization", "Bearer " + token) };
                    var client = new HttpClient();
                    client.Timeout = TimeSpan.FromMinutes(10);
                    foreach (var header in lstHeader)
                        client.DefaultRequestHeaders.Add(header.Name, header.Value);

                    await AutoGetAVNDepartment(client, config);
                    await AutoGetAVNPosition(client, config);
                    await AutoGetAVNEmployee(client, config);
                    await AutoGetAVNShift(client, config);
                    await AutoGetAVNOverTimePlan(client, config);
                    await AutoGetAVNBussinessTravel(client, config);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sync AEON employee log: {ex}");
            }
        }

        public async Task AutoGetAVNIntegrateBusinessTravel(DateTime now)
        {
            var timePostCheck = now.ToHHmm();
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = 2 },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_EMPLOYEE_BUSINESS_TRAVEL.ToString() }
            };
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            try
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {
                    if (config.TimePos.Contains(timePostCheck))
                    {
                        if (!string.IsNullOrWhiteSpace(_AppConfigAVN?.Domain))
                        {
                            var token = await GetToken();
                            var lstHeader = new List<WebAPIHeader>
                                { new WebAPIHeader("Authorization", "Bearer " + token) };
                            var client = new HttpClient();
                            client.Timeout = TimeSpan.FromMinutes(10);
                            foreach (var header in lstHeader)
                                client.DefaultRequestHeaders.Add(header.Name, header.Value);

                            await AutoGetAVNOverTimePlan(client, config);
                            await AutoGetAVNBussinessTravel(client, config);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sync AEON employee log: {ex}");
            }
        }


        private async Task<string> GetToken()
        {
            LoginInfoAVN loginInfo = new LoginInfoAVN()
            {
                username = Convert.ToString(_AppConfigAVN.UserName),
                password = Convert.ToString(_AppConfigAVN.Password),
                grant_type = "password"
            };

            var json = JsonConvert.SerializeObject(loginInfo);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            try
            {
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.PostAsync(_AppConfigAVN.Domain + "/Token", new FormUrlEncodedContent(values));

                if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<TokenHeader>(await response.Content.ReadAsStringAsync());
                    return result.access_token;
                }
            }
            catch
            {
            }
            return "";
        }


        public class TokenHeader
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
            public string userName { get; set; }
        }

        static public dynamic ConvertJsonToObject(string pResult)
        {
            dynamic data = JsonConvert.DeserializeObject<object>(pResult, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return data;
        }

        static public string ConvertObjectToJson(object pObject)
        {
            string data = JsonConvert.SerializeObject(pObject);
            return data;
        }

        public class CheckDuplicateAVN
        {
            public string CodeAttendance { get; set; }
            public bool IsAdd { get; set; }
        }

        private async Task AutoGetAVNEmployee(HttpClient client, IC_ConfigDTO config)
        {
            var history = new IC_HistoryTrackingIntegrate
            {
                JobName = "AutoGetAVNEmployee",
                IsSuccess = true
            };

            HttpResponseMessage responseMessageGetData = null;
            var pageIndex = 1;
            var itemResult = new List<Hre_Profile>();
            try
            {
                do
                {
                    if (responseMessageGetData == null)
                    {
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Hre_Profile/GetListProfileCustom?CodeEmp=&OrgStructureCode=&PageIndex=" + pageIndex + "&PageSize=100000");
                    }
                    else if (responseMessageGetData != null && responseMessageGetData.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var token = await GetToken();
                        var lstHeader = new List<WebAPIHeader>
                            {
                                new WebAPIHeader("Authorization", "Bearer " + token)
                            };
                        client.DefaultRequestHeaders.Remove("Authorization");
                        foreach (var header in lstHeader)
                            client.DefaultRequestHeaders.Add(header.Name, header.Value);
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Hre_Profile/GetListProfileCustom?CodeEmp=&OrgStructureCode=&PageIndex=" + pageIndex + "&PageSize=100000");
                    }
                    else
                    {
                        var result = await responseMessageGetData.Content.ReadAsStringAsync();
                        var itemDepartment = JsonConvert.DeserializeObject<Hre_ProfileApiReturn>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });


                        if (itemDepartment != null && itemDepartment.data != null && itemDepartment.data.Count > 0)
                        {
                            itemResult.AddRange(itemDepartment.data);
                        }

                        if (itemDepartment.data.Count < 100000)
                        {
                            break;
                        }
                        else
                        {
                            responseMessageGetData = null;
                            pageIndex++;
                        }


                    }

                } while (true);
                //result = await responseMessageGetData.Content.ReadAsStringAsync();
                //var itemEmployee = JsonConvert.DeserializeObject<Hre_ProfileApiReturn>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                if (itemResult != null && itemResult.Count > 0)
                {
                    var listEmployeeTobeSync = new List<IC_EmployeeDTO>();
                    var listDeleteEmployee = new List<IC_EmployeeDTO>();
                    var employeeIds = itemResult.Where(x => x.CodeAttendance != null).Select(x => x.CodeAttendance.PadLeft(_config.MaxLenghtEmployeeATID, '0')).ToList();
                    var listEmployeeDb = _dbContext.HR_EmployeeInfo.Where(e => e.CompanyIndex == config.CompanyIndex).ToList();
                    var positionDb = _dbContext.HR_PositionInfo.Where(e => e.CompanyIndex == config.CompanyIndex).ToList();
                    var departments = _dbContext.IC_Department.Where(x => x.CompanyIndex == config.CompanyIndex).ToList();
                    var users = _dbContext.HR_User.Where(x => x.CompanyIndex == config.CompanyIndex).ToList();
                    var lsthrCardInfo = _dbContext.HR_CardNumberInfo.Where(x => x.IsActive == true).ToList();
                    var duplicate = itemResult.Where(x => x.CodeAttendance != null).GroupBy(x => x.CodeAttendance.PadLeft(_config.MaxLenghtEmployeeATID, '0')).Select(x => new CheckDuplicateAVN()
                    {
                        CodeAttendance = x.Key.PadLeft(_config.MaxLenghtEmployeeATID, '0'),
                        IsAdd = false
                    }).ToList();

                    List<AddedParam> addedParams = new List<AddedParam>();
                    addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = config.CompanyIndex });
                    List<IC_WorkingInfo> listWorkingInfo = _iC_WorkingInfoLogic.GetList(addedParams).OrderByDescending(u => u.FromDate).ToList();

                    var userMasterLst = _dbContext.IC_UserMaster.Where(x => x.CompanyIndex == config.CompanyIndex).ToList();

                    foreach (var item in itemResult)
                    {
                        if (string.IsNullOrEmpty(item.CodeAttendance))
                        {
                            continue;
                        }
                        if (item.StatusSyn == "Đã nghỉ việc" || item.Flag == "D")
                        {
                            if (!listWorkingInfo.Any(x => x.EmployeeATID == item.CodeAttendance.PadLeft(_config.MaxLenghtEmployeeATID, '0')))
                            {
                                continue;
                            }
                            if (itemResult.Any(x => x.CodeEmp == item.CodeEmp && x.StatusSyn != "Đã nghỉ việc" && x.Flag != "D"))
                            {
                                continue;
                            }
                        }

                        var departmentCode = new IC_Department();
                        var checkDuplicate = duplicate.FirstOrDefault(x => x.CodeAttendance == item.CodeAttendance.PadLeft(_config.MaxLenghtEmployeeATID, '0'));
                        if (checkDuplicate != null)
                        {
                            if (checkDuplicate.IsAdd)
                            {
                                continue;
                            }

                            checkDuplicate.IsAdd = true;
                        }


                        if (!string.IsNullOrEmpty(item.OrgStructureCode))
                        {
                            departmentCode = departments.FirstOrDefault(x => x.Code == item.OrgStructureCode);
                        }

                        var validateInfo = new IC_EmployeeDTO
                        {
                            EmployeeATID = item.CodeAttendance.PadLeft(_config.MaxLenghtEmployeeATID, '0')
                        };

                        _iC_EmployeeLogic.ValidateEmployeeInfo(validateInfo);

                        //if ((errorList != null && errorList.Count() > 0))
                        //{
                        //    result.ListIndexError.Add(item.Id);
                        //    result.ListError.AddRange(errorList);
                        //    continue;
                        //}

                        var existed = users.FirstOrDefault(x => x.EmployeeCode == item.CodeEmp && x.CompanyIndex == config.CompanyIndex);
                        if (existed == null)
                        {
                            existed = users.FirstOrDefault(x => x.EmployeeATID == item.CodeAttendance.PadLeft(_config.MaxLenghtEmployeeATID, '0') && x.CompanyIndex == config.CompanyIndex && (x.EmployeeCode == null || string.IsNullOrEmpty(x.EmployeeCode)));
                        }
                        if (existed != null)
                        {
                            existed.EmployeeCode = item.CodeEmp;
                            existed.FullName = item.ProfileName;
                            existed.EmployeeType = (int)EmployeeType.Employee;
                            existed.UpdatedDate = DateTime.Now;
                            existed.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                            _dbContext.HR_User.Update(existed);
                            history.DataUpdate++;
                        }
                        else
                        {
                            existed = new HR_User
                            {
                                EmployeeCode = item.CodeEmp,
                                FullName = item.ProfileName,
                                CompanyIndex = config.CompanyIndex,
                                EmployeeType = (int)EmployeeType.Employee,
                                EmployeeATID = item.CodeAttendance.PadLeft(_config.MaxLenghtEmployeeATID, '0'),
                                CreatedDate = DateTime.Now
                            };
                            _dbContext.HR_User.Add(existed);
                            history.DataNew++;
                        }

                        // check this employee has working info or not
                        var workingInfo = new IC_WorkingInfoDTO
                        {
                            EmployeeATID = existed.EmployeeATID,
                            CompanyIndex = config.CompanyIndex,
                            UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString(),
                            Status = (short)TransferStatus.Approve,
                            ApprovedDate = DateTime.Today,
                            FromDate = item.datecreate ?? DateTime.Now
                        };
                        if (item.StatusSyn == "Đã nghỉ việc" || item.Flag == "D")
                        {
                            workingInfo.ToDate = item.DateQuit ?? DateTime.Now.Date.AddDays(-1);
                        }
                        workingInfo.IsManager = false;
                        workingInfo.IsSync = null;
                        workingInfo.DepartmentIndex = departmentCode?.Index ?? 0;

                        if (!string.IsNullOrWhiteSpace(item.PositionCode))
                        {
                            var position = positionDb.FirstOrDefault(x => x.Code == item.PositionCode);
                            if (position != null)
                            {
                                workingInfo.PositionIndex = position.Index;
                            }
                        }

                        var updateWorkingResult = _iC_WorkingInfoLogic.CheckUpdateOrInsertDtoAVN(workingInfo, listWorkingInfo, _dbContext);

                        var userMaster = new IC_UserMasterDTO
                        {
                            EmployeeATID = existed.EmployeeATID,
                            CompanyIndex = existed.CompanyIndex,
                            Privilege = 0,
                            CreatedDate = DateTime.Now,
                            UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString(),
                            CardNumber = existed.EmployeeATID.TrimStart(new Char[] { '0' })
                        };
                        _iC_UserMasterLogic.CheckExistedOrCreateDto(userMaster, userMasterLst, _dbContext);

                        var existedEmployee = listEmployeeDb.FirstOrDefault(e => e.EmployeeATID == existed.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'));
                        if (existedEmployee != null)
                        {
                            existedEmployee.UpdatedDate = DateTime.Now;
                        }
                        else
                        {
                            existedEmployee = new HR_EmployeeInfo
                            {
                                CompanyIndex = existed.CompanyIndex,
                                EmployeeATID = existed.EmployeeATID,
                                UpdatedDate = DateTime.Now,
                                UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                            };
                            _dbContext.HR_EmployeeInfo.Add(existedEmployee);
                        }

                        var hrCardInfo = new HR_CardNumberInfo
                        {
                            CardNumber = item.CodeAttendance.TrimStart(new Char[] { '0' }),
                            IsActive = true,
                            CompanyIndex = 2,
                            EmployeeATID = existed.EmployeeATID,

                        };

                        CheckCardActivedOrCreate(hrCardInfo, 2, lsthrCardInfo);

                        if (updateWorkingResult != null && updateWorkingResult.EmployeeATID != null)
                        {
                            // add to list to be sync all device of deparment
                            var emplDelete = listDeleteEmployee.FirstOrDefault(e => e.EmployeeATID == updateWorkingResult.EmployeeATID);
                            if (emplDelete == null)
                            {
                                listEmployeeTobeSync.Add(new IC_EmployeeDTO
                                {
                                    EmployeeATID = existed.EmployeeATID,
                                    CompanyIndex = config.CompanyIndex
                                });
                            }
                        }
                    }
                    var results = await _dbContext.SaveChangesAsync();
                    if (results > 0)
                    {
                        _logger.LogInformation("UpdateEmployeeIntegrate successfully!!!");
                    }

                    var addedParamss = new List<AddedParam>
                {
                    new AddedParam { Key = "CompanyIndex", Value = config.CompanyIndex },
                    new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER }
                };
                    var systemconfigs = await _iC_ConfigLogic.GetMany(addedParamss);
                    if (systemconfigs != null)
                    {
                        var sysconfig = systemconfigs.FirstOrDefault();
                        if (sysconfig != null)
                        {
                            if (sysconfig.IntegrateLogParam.AutoIntegrate)
                            {
                                await _iC_CommandLogic.SyncWithEmployee(listEmployeeTobeSync.Select(u => u.EmployeeATID).ToList(), config.CompanyIndex);
                            }
                        }
                    }
                    await _ic_IntegrateLogic.SaveOrUpdateEmployeeIntegrate(itemResult);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"AutoGetAVNEmployee: {e}");
                history.Reason = e.Message;
                history.IsSuccess = false;
            }
            await AddHistoryTrackingIntegrate(history);
            await AutoActiveESSAccount();
        }

        private async Task AutoActiveESSAccount()
        {
            var client = new HttpClient();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, mLinkECMSApi + "/api/IC_UserAccountESS/ActiveAccountESSFromIntegrate");
                request.Headers.Add("api-token", mCommunicateToken);
                client.Timeout = TimeSpan.FromMinutes(10);
                var responseMessageGetData = await client.SendAsync(request);
                responseMessageGetData.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoActiveESSAccount: {mLinkECMSApi} {ex}");
            }
        }
        private async Task AutoGetAVNPosition(HttpClient client, IC_ConfigDTO config)
        {
            var history = new IC_HistoryTrackingIntegrate
            {
                JobName = "AutoGetAVNPosition",
                IsSuccess = true
            };
            try
            {
                HttpResponseMessage responseMessageGetData = null;
                do
                {
                    if (responseMessageGetData == null)
                    {
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Hre_Profile/GetPosition?PageIndex=1&PageSize=100000");
                    }
                    else if (responseMessageGetData.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var token = await GetToken();
                        var lstHeader = new List<WebAPIHeader>
                            {
                                new WebAPIHeader("Authorization", "Bearer " + token)
                            };
                        client.DefaultRequestHeaders.Remove("Authorization");
                        foreach (WebAPIHeader header in lstHeader)
                            client.DefaultRequestHeaders.Add(header.Name, header.Value);
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Hre_Profile/GetPosition?PageIndex=1&PageSize=100000");
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                //responseMessageGetData.EnsureSuccessStatusCode();
                var result = await responseMessageGetData.Content.ReadAsStringAsync();
                var itemPosition = JsonConvert.DeserializeObject<Cat_PositionApiResult>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                if (itemPosition != null && itemPosition.data != null && itemPosition.data.Any())
                {
                    foreach (var item in itemPosition.data)
                    {
                        var employeePosition = new HR_PositionInfoDTO { Name = string.IsNullOrEmpty(item.PositionName) ? "Nhân Viên" : item.PositionName, Code = item.Code, NameInEng = item.PositionEngName };
                        var resultPosition = await _hR_PositionInfoLogic.CheckExistedOrCreateAVN(employeePosition);

                        switch (resultPosition)
                        {
                            case true:
                                history.DataNew += 1;
                                break;
                            case false:
                                history.DataUpdate += 1;
                                break;
                        }
                    }

                }
                await _ic_IntegrateLogic.SaveOrUpdatePositionIntegrate(itemPosition.data);
            }
            catch (Exception ex)
            {
                history.Reason = ex.Message;
                history.IsSuccess = false;
                _logger.LogError($"AutoGetAVNPosition: {ex}");
            }
            await AddHistoryTrackingIntegrate(history);
        }

        private async Task AutoGetAVNEmployeeShift(HttpClient client, IC_ConfigDTO config)
        {
            var history = new IC_HistoryTrackingIntegrate
            {
                JobName = "AutoGetAVNEmployeeShift",
                DataNew = 0,
                DataUpdate = 0,
                IsSuccess = true
            };
            try
            {
                HttpResponseMessage responseMessageGetData = null;
                var now = DateTime.Now.ToyyyyMMdd();
                var param = JsonConvert.DeserializeObject<IntegrateOVNEmployeeShiftParam>(config.CustomField);
                string from = "";
                string to = "";
                DateTime fromTime = DateTime.Now;
                DateTime toTime = DateTime.Now;
                if (config.IntegrateLogParam.IsOverwriteData)
                {
                    from = (DateTime.Parse(param.FromDate)).ToyyyyMMdd();
                    to = (DateTime.Parse(param.ToDate)).ToyyyyMMdd();
                    fromTime = DateTime.Parse(param.FromDate);
                    toTime = DateTime.Parse(param.ToDate);
                }
                else
                {
                    fromTime = DateTime.Now;
                    toTime = fromTime.AddDays((config.PreviousDays ?? 0));
                    from = fromTime.ToyyyyMMdd();
                    to = toTime.ToyyyyMMdd();
                }
                do
                {
                    if (responseMessageGetData == null)
                    {
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Att_Roster/GetRosterCalendar?codeEmps=&dateStart=" + from + "&dateEnd=" + to);
                    }
                    else if (responseMessageGetData.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var token = await GetToken();
                        var lstHeader = new List<WebAPIHeader>
                            {
                                new WebAPIHeader("Authorization", "Bearer " + token)
                            };
                        client.DefaultRequestHeaders.Remove("Authorization");
                        foreach (WebAPIHeader header in lstHeader)
                            client.DefaultRequestHeaders.Add(header.Name, header.Value);
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Att_Roster/GetRosterCalendar?codeEmps=&dateStart=" + from + "&dateEnd=" + to);
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                var result = await responseMessageGetData.Content.ReadAsStringAsync();
                var itemPosition = JsonConvert.DeserializeObject<Att_RosterApiResult>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                await _ic_IntegrateLogic.SaveOrUpdateEmployeeShiftIntegrate(itemPosition.data);
                if (itemPosition != null && itemPosition.data != null && itemPosition.data.Any())
                {
                    var resultResponseDelete = await SendDeleteEmployeeShiftToECMSAPIAsync(fromTime.Date, toTime.Date);
                    history.DataDelete = resultResponseDelete.Data.DataDelete;
                    if (itemPosition.data.Count > 1000)
                    {
                        var listSplitEmployeeID = CommonUtils.SplitList(itemPosition.data, 1000);
                        foreach (var listEmployeeSplit in listSplitEmployeeID)
                        {
                            var employeeCodes = listEmployeeSplit.Select(x => x.CodeEmp).ToHashSet();
                            var listEmployee = (from e in _dbContext.HR_User.Where(x => employeeCodes.Contains(x.EmployeeCode))
                                                join w in _dbContext.IC_WorkingInfo.Where(w =>
                                                     w.Status == (short)TransferStatus.Approve
                                                    && w.FromDate.Date <= DateTime.Now.Date)
                                                on e.EmployeeATID equals w.EmployeeATID into workinginfo
                                                from wkinf in workinginfo.DefaultIfEmpty()
                                                where !wkinf.ToDate.HasValue || (wkinf.ToDate.HasValue && wkinf.ToDate.Value.Date >= DateTime.Now.Date)
                                                select new
                                                {
                                                    e.EmployeeATID,
                                                    e.EmployeeCode
                                                }).ToList();

                            foreach (var item in listEmployeeSplit)
                            {
                                var employee = listEmployee.FirstOrDefault(x => x.EmployeeCode == item.CodeEmp);
                                if (employee != null)
                                {
                                    item.CodeEmp = employee.EmployeeATID;
                                }
                            }
                            var resultResponse = SendEmployeeShiftToECMSAPIAsync(listEmployeeSplit);
                            await Task.WhenAll(resultResponse);

                            if (resultResponse != null && resultResponse.Result?.Data?.DataNew != null)
                            {
                                history.DataNew += resultResponse.Result.Data.DataNew;
                                history.DataUpdate += resultResponse.Result.Data.DataUpdate;
                            }
                        }
                    }
                    else
                    {
                        var employeeCodes = itemPosition.data.Select(x => x.CodeEmp).ToHashSet();
                        var listEmployee = (from e in _dbContext.HR_User.Where(x => employeeCodes.Contains(x.EmployeeCode))
                                            join w in _dbContext.IC_WorkingInfo.Where(w =>
                                                 w.Status == (short)TransferStatus.Approve
                                                && w.FromDate.Date <= DateTime.Now.Date)
                                            on e.EmployeeATID equals w.EmployeeATID into workinginfo
                                            from wkinf in workinginfo.DefaultIfEmpty()
                                            where !wkinf.ToDate.HasValue || (wkinf.ToDate.HasValue && wkinf.ToDate.Value.Date >= DateTime.Now.Date)
                                            select new
                                            {
                                                e.EmployeeATID,
                                                e.EmployeeCode
                                            }).ToList();
                        foreach (var item in itemPosition.data)
                        {
                            var employee = listEmployee.FirstOrDefault(x => x.EmployeeCode == item.CodeEmp);
                            if (employee != null)
                            {
                                item.CodeEmp = employee.EmployeeATID;
                            }
                        }
                        var resultResponse = await SendEmployeeShiftToECMSAPIAsync(itemPosition.data);
                        if (resultResponse != null)
                        {
                            history.DataNew = resultResponse.Data.DataNew;
                            history.DataUpdate = resultResponse.Data.DataUpdate;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoGetAVNEmployeeShift: {ex}");
            }
            await AddHistoryTrackingIntegrate(history);
        }

        private async Task AutoGetAVNOverTimePlan(HttpClient client, IC_ConfigDTO config)
        {
            var history = new IC_HistoryTrackingIntegrate
            {
                JobName = "AutoGetAVNOverTimePlan",
                IsSuccess = true
            };
            try
            {
                HttpResponseMessage responseMessageGetData = null;
                var now = DateTime.Now.ToyyyyMMdd();

                do
                {
                    if (responseMessageGetData == null)
                    {
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Att_OvertimePlan/GetListOTPlan?dateStart=" + now + "&dateEnd=" + now + "&codeATT=");
                    }
                    else if (responseMessageGetData.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var token = await GetToken();
                        var lstHeader = new List<WebAPIHeader>
                            {
                                new WebAPIHeader("Authorization", "Bearer " + token)
                            };
                        client.DefaultRequestHeaders.Remove("Authorization");
                        foreach (WebAPIHeader header in lstHeader)
                            client.DefaultRequestHeaders.Add(header.Name, header.Value);
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Att_OvertimePlan/GetListOTPlan?dateStart=" + now + "&dateEnd=" + now + "&codeATT=");
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                //responseMessageGetData.EnsureSuccessStatusCode();
                var result = await responseMessageGetData.Content.ReadAsStringAsync();
                var itemPosition = JsonConvert.DeserializeObject<Att_OverTimePlan_ApiResult>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                await _ic_IntegrateLogic.SaveOrUpdateOverTimePlanIntegrate(itemPosition.data);
                if (itemPosition != null && itemPosition.data != null && itemPosition.data.Any())
                {
                    var employeeCodes = itemPosition.data.Select(x => x.CodeEmp).ToHashSet();
                    var employees = _dbContext.HR_User.AsNoTracking().ToList();

                    foreach (var item in itemPosition.data)
                    {
                        var employee = employees.FirstOrDefault(x => x.EmployeeCode == item.CodeEmp);
                        if (employee != null)
                        {
                            item.CodeEmp = employee.EmployeeATID;
                        }
                    }
                    var resultResponse = await SendOverTimeToECMSAPIAsync(itemPosition.data);
                    if (resultResponse != null)
                    {
                        history.DataNew = resultResponse.Data.DataNew;
                        history.DataUpdate = resultResponse.Data.DataUpdate;
                    }
                }
            }
            catch (Exception ex)
            {
                history.Reason = ex.Message;
                history.IsSuccess = false;
                _logger.LogError($"AutoGetAVNOverTimePlan: {ex}");
            }
            await AddHistoryTrackingIntegrate(history);
        }

        private async Task AutoGetAVNOverTimePlanTime(HttpClient client, IC_ConfigDTO config)
        {
            var history = new IC_HistoryTrackingIntegrate
            {
                JobName = "AutoGetAVNOverTimePlan",
                IsSuccess = true
            };
            try
            {
                HttpResponseMessage responseMessageGetData = null;
                var now = DateTime.Now.ToyyyyMMdd();
                var param = JsonConvert.DeserializeObject<IntegrateOVNEmployeeShiftParam>(config.CustomField);
                string from = "";
                string to = "";
                DateTime fromTime = DateTime.Now;
                DateTime toTime = DateTime.Now;
                if (config.IntegrateLogParam.IsOverwriteData)
                {
                    from = (DateTime.Parse(param.FromDate)).ToyyyyMMdd();
                    to = (DateTime.Parse(param.ToDate)).ToyyyyMMdd();
                    fromTime = DateTime.Parse(param.FromDate);
                    toTime = DateTime.Parse(param.ToDate);
                }
                else
                {
                    fromTime = DateTime.Now;
                    toTime = fromTime.AddDays((config.PreviousDays ?? 0));
                    from = fromTime.ToyyyyMMdd();
                    to = toTime.ToyyyyMMdd();
                }
                do
                {
                    if (responseMessageGetData == null)
                    {
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Att_OvertimePlan/GetListOTPlan?dateStart=" + from + "&dateEnd=" + to + "&codeATT=");
                    }
                    else if (responseMessageGetData.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var token = await GetToken();
                        var lstHeader = new List<WebAPIHeader>
                            {
                                new WebAPIHeader("Authorization", "Bearer " + token)
                            };
                        client.DefaultRequestHeaders.Remove("Authorization");
                        foreach (WebAPIHeader header in lstHeader)
                            client.DefaultRequestHeaders.Add(header.Name, header.Value);
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Att_OvertimePlan/GetListOTPlan?dateStart=" + from + "&dateEnd=" + to + "&codeATT=");
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                //responseMessageGetData.EnsureSuccessStatusCode();
                var result = await responseMessageGetData.Content.ReadAsStringAsync();
                var itemPosition = JsonConvert.DeserializeObject<Att_OverTimePlan_ApiResult>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                await _ic_IntegrateLogic.SaveOrUpdateOverTimePlanIntegrate(itemPosition.data);
                if (itemPosition != null && itemPosition.data != null && itemPosition.data.Any())
                {
                    var employeeCodes = itemPosition.data.Select(x => x.CodeEmp).ToHashSet();
                    var employees = _dbContext.HR_User.AsNoTracking().ToList();

                    foreach (var item in itemPosition.data)
                    {
                        var employee = employees.FirstOrDefault(x => x.EmployeeCode == item.CodeEmp);
                        if (employee != null)
                        {
                            item.CodeEmp = employee.EmployeeATID;
                        }
                    }
                    var resultResponse = await SendOverTimeToECMSAPIAsync(itemPosition.data);
                    if (resultResponse != null)
                    {
                        history.DataNew = resultResponse.Data.DataNew;
                        history.DataUpdate = resultResponse.Data.DataUpdate;
                    }
                }
            }
            catch (Exception ex)
            {
                history.Reason = ex.Message;
                history.IsSuccess = false;
                _logger.LogError($"AutoGetAVNOverTimePlan: {ex}");
            }
            await AddHistoryTrackingIntegrate(history);
        }


        private async Task AutoGetAVNBussinessTravel(HttpClient client, IC_ConfigDTO config)
        {
            var history = new IC_HistoryTrackingIntegrate
            {
                JobName = "AutoGetAVNTA_BussinessTravel",
                IsSuccess = true
            };
            try
            {
                HttpResponseMessage responseMessageGetData = null;
                var now = DateTime.Now.ToyyyyMMdd();
                var nowD = DateTime.Now.Date;
                do
                {
                    if (responseMessageGetData == null)
                    {
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Att_BussinessTravel/GetListBussinessTravel?dateStart=" + now + "&dateEnd=" + now + "&CodeAttendance=");
                    }
                    else if (responseMessageGetData.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var token = await GetToken();
                        var lstHeader = new List<WebAPIHeader>
                            {
                                new WebAPIHeader("Authorization", "Bearer " + token)
                            };
                        client.DefaultRequestHeaders.Remove("Authorization");
                        foreach (WebAPIHeader header in lstHeader)
                            client.DefaultRequestHeaders.Add(header.Name, header.Value);
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Att_BussinessTravel/GetListBussinessTravel?dateStart=" + now + "&dateEnd=" + now + "&CodeAttendance=");
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                //responseMessageGetData.EnsureSuccessStatusCode();
                var result = await responseMessageGetData.Content.ReadAsStringAsync();
                var itemPosition = JsonConvert.DeserializeObject<Att_BusinessTravelApiResult>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                await _ic_IntegrateLogic.SaveOrUpdateBusinessTravelIntegrate(itemPosition.data);
                if (itemPosition != null && itemPosition.data != null && itemPosition.data.Any())
                {
                    itemPosition.data = itemPosition.data.Where(x => x.DateFrom.Date == DateTime.Now.Date).ToList();
                    var employeeATIDLst = _dbContext.IC_AttendanceLog.AsNoTracking().Where(x => x.CheckTime.Date == nowD && x.UpdatedUser == "IntergrateLog").Select(x => x.EmployeeATID).ToHashSet();
                    var employees = _dbContext.HR_User.AsNoTracking().Where(x => employeeATIDLst.Contains(x.EmployeeATID)).ToList();
                    foreach (var item in itemPosition.data)
                    {
                        var emp = employees.FirstOrDefault(x => x.EmployeeCode == item.CodeEmp);
                        if (item.CodeEmp != null && emp != null)
                        {
                            item.CodeAttendance = emp.EmployeeATID;
                            item.BusinessTripType = 0;
                        }
                        else
                        {
                            item.BusinessTripType = 1;
                        }

                        if (item.DurationType == DurationTypeValue.E_FIRSTHALFSHIFT.ToString())
                        {
                            item.DurationType = "1";
                        }
                        else if (item.DurationType == DurationTypeValue.E_LASTHALFSHIFT.ToString())
                        {
                            item.DurationType = "2";
                        }
                        else if (item.DurationType == DurationTypeValue.E_FULLSHIFT.ToString())
                        {
                            item.DurationType = "3";
                        }
                    }

                    var resultResponse = await SendBusinessTravelToECMSAPIAsync(itemPosition.data);
                    if (resultResponse != null)
                    {
                        history.DataNew = resultResponse.Data.DataNew;
                        history.DataUpdate = resultResponse.Data.DataUpdate;
                    }
                }
            }
            catch (Exception ex)
            {
                history.Reason = ex.Message;
                history.IsSuccess = false;
                _logger.LogError($"AutoGetAVNBussinessTravel: {ex}");
            }
            await AddHistoryTrackingIntegrate(history);
        }


        private async Task AutoGetAVNShift(HttpClient client, IC_ConfigDTO config)
        {
            var history = new IC_HistoryTrackingIntegrate
            {
                JobName = "AutoGetAVNShift",
                IsSuccess = true
            };
            try
            {
                HttpResponseMessage responseMessageGetData = null;

                do
                {
                    if (responseMessageGetData == null)
                    {
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Cat_Shift/GetListCatShift");
                    }
                    else if (responseMessageGetData.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var token = await GetToken();
                        var lstHeader = new List<WebAPIHeader>
                            {
                                new WebAPIHeader("Authorization", "Bearer " + token)
                            };
                        client.DefaultRequestHeaders.Remove("Authorization");
                        foreach (WebAPIHeader header in lstHeader)
                            client.DefaultRequestHeaders.Add(header.Name, header.Value);
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Cat_Shift/GetListCatShift");
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                //responseMessageGetData.EnsureSuccessStatusCode();
                var result = await responseMessageGetData.Content.ReadAsStringAsync();
                var itemPosition = JsonConvert.DeserializeObject<List<Cat_Shift>>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                if (itemPosition != null && itemPosition.Any())
                {
                    var resultResponse = await SendShiftToECMSAPIAsync(itemPosition);
                    if (resultResponse != null)
                    {
                        history.DataNew = resultResponse.Data.DataNew;
                        history.DataUpdate = resultResponse.Data.DataUpdate;
                    }
                }


                await _ic_IntegrateLogic.SaveOrUpdateShiftIntegrate(itemPosition);
            }
            catch (Exception ex)
            {
                history.Reason = ex.Message;
                history.IsSuccess = false;
                _logger.LogError($"AutoGetAVNShift: {ex}");
            }
            await AddHistoryTrackingIntegrate(history);
        }

        private async Task<ECMSResponse> SendShiftToECMSAPIAsync(List<Cat_Shift> requestData)
        {
            var client = new HttpClient();
            try
            {
                var jsonData = JsonConvert.SerializeObject(requestData);
                var request = new HttpRequestMessage(HttpMethod.Post, mLinkECMSApi + "/api/TA_Shift/ShiftIntegrateFromAVNePAD");
                request.Headers.Add("api-token", mCommunicateToken);
                client.Timeout = TimeSpan.FromMinutes(10);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var responseMessageGetData = await client.SendAsync(request);
                var result = await responseMessageGetData.Content.ReadAsStringAsync();
                var itemPosition = JsonConvert.DeserializeObject<ECMSResponse>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return itemPosition;

            }
            catch (Exception ex)
            {
                _logger.LogError($"SendShiftToECMSAPIAsync: {mLinkECMSApi} {ex}");
            }
            return null;
        }

        private async Task<ECMSResponse> SendEmployeeShiftToECMSAPIAsync(List<Att_Roster> requestData)
        {
            var client = new HttpClient();
            try
            {
                var jsonData = JsonConvert.SerializeObject(requestData);
                var request = new HttpRequestMessage(HttpMethod.Post, mLinkECMSApi + "/api/CM_EmployeeShift/SyncEmployeeShiftFromeAVNPAD");
                request.Headers.Add("api-token", mCommunicateToken);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var responseMessageGetData = await client.SendAsync(request);
                var result = await responseMessageGetData.Content.ReadAsStringAsync();
                var itemPosition = JsonConvert.DeserializeObject<ECMSResponse>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return itemPosition;

            }
            catch (Exception ex)
            {
                _logger.LogError($"SendEmployeeShiftToECMSAPIAsync: {mLinkECMSApi} {ex}");
            }
            return null;
        }

        private async Task<ECMSResponse> SendDeleteEmployeeShiftToECMSAPIAsync(DateTime FromDate, DateTime ToDate)
        {
            var client = new HttpClient();
            try
            {
                var requestData = new ECMSRequestTime();
                requestData.FromDate = FromDate;
                requestData.ToDate = ToDate;
                var jsonData = JsonConvert.SerializeObject(requestData);
                var request = new HttpRequestMessage(HttpMethod.Post, mLinkECMSApi + "/api/CM_EmployeeShift/DeleteEmployeeShiftFromeAVNPAD");
                request.Headers.Add("api-token", mCommunicateToken);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var responseMessageGetData = await client.SendAsync(request);
                var result = await responseMessageGetData.Content.ReadAsStringAsync();
                var itemPosition = JsonConvert.DeserializeObject<ECMSResponse>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return itemPosition;

            }
            catch (Exception ex)
            {
                _logger.LogError($"SendDeleteEmployeeShiftToECMSAPIAsync: {mLinkECMSApi} {ex}");
            }
            return null;
        }

        private async Task<ECMSResponse> SendOverTimeToECMSAPIAsync(List<Att_OverTimePlan> requestData)
        {
            var client = new HttpClient();
            try
            {
                var jsonData = JsonConvert.SerializeObject(requestData);
                var request = new HttpRequestMessage(HttpMethod.Post, mLinkECMSApi + "/api/TA_OverTime/TimeLogFromAVNEPAD");
                request.Headers.Add("api-token", mCommunicateToken);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var responseMessageGetData = await client.SendAsync(request);
                var result = await responseMessageGetData.Content.ReadAsStringAsync();
                var itemPosition = JsonConvert.DeserializeObject<ECMSResponse>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return itemPosition;

            }
            catch (Exception ex)
            {
                _logger.LogError($"SendOverTimeToECMSAPIAsync: {mLinkECMSApi} {ex}");
            }
            return null;
        }

        private async Task<ECMSResponse> SendBusinessTravelToECMSAPIAsync(List<Att_BusinessTravel> requestData)
        {
            var client = new HttpClient();
            try
            {
                var jsonData = JsonConvert.SerializeObject(requestData);
                var request = new HttpRequestMessage(HttpMethod.Post, mLinkECMSApi + "/api/TA_BussinessTravel/BussinessTravelFromAVNEPAD");
                request.Headers.Add("api-token", mCommunicateToken);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var responseMessageGetData = await client.SendAsync(request);
                var result = await responseMessageGetData.Content.ReadAsStringAsync();
                var itemPosition = JsonConvert.DeserializeObject<ECMSResponse>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return itemPosition;

            }
            catch (Exception ex)
            {
                _logger.LogError($"SendBusinessTravelToECMSAPIAsync: {mLinkECMSApi} {ex}");
            }
            return null;
        }


        private async Task AutoGetAVNDepartment(HttpClient client, IC_ConfigDTO config)
        {
            var history = new IC_HistoryTrackingIntegrate
            {
                JobName = "AutoGetAVNDepartment",
                IsSuccess = true
            };
            try
            {
                var pageIndex = 1;
                var itemResult = new List<Cat_OrgStructure>();
                HttpResponseMessage responseMessageGetData = null;
                do
                {
                    if (responseMessageGetData == null)
                    {
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Hre_Profile/GetOrgStructure?PageIndex=" + pageIndex + "&PageSize=100000&IsCurrentUpdate=0");
                    }
                    if (responseMessageGetData != null && responseMessageGetData.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var token = await GetToken();
                        var lstHeader = new List<WebAPIHeader>
                    {
                        new WebAPIHeader("Authorization", "Bearer " + token)
                    };
                        client.DefaultRequestHeaders.Remove("Authorization");
                        foreach (WebAPIHeader header in lstHeader)
                            client.DefaultRequestHeaders.Add(header.Name, header.Value);
                        responseMessageGetData = await client.GetAsync(_AppConfigAVN.Domain + "/api/Hre_Profile/GetOrgStructure?PageIndex=" + pageIndex + "&PageSize=100000&IsCurrentUpdate=0");
                    }
                    else
                    {
                        var result = await responseMessageGetData.Content.ReadAsStringAsync();
                        var itemDepartment = JsonConvert.DeserializeObject<Cat_OrgStructureApiResult>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });


                        if (itemDepartment != null && itemDepartment.data != null && itemDepartment.data.Count > 0)
                        {
                            itemResult.AddRange(itemDepartment.data);
                        }

                        if (itemDepartment.data.Count < 100000)
                        {
                            break;
                        }
                        else
                        {
                            responseMessageGetData = null;
                            pageIndex++;
                        }
                    }
                } while (true);

                if (itemResult.Count > 0)
                {
                    itemResult = itemResult.Where(x => x.StatusFormat != "0").ToList();
                    var oldDepartmentList = await _iC_DepartmentLogic.OldDepartmentUpdateAVN(2, itemResult);

                    history.DataDelete = (short)oldDepartmentList;

                    var row = 1;
                    foreach (var item in itemResult)
                    {
                        var departmentDto = new IC_DepartmentDTO()
                        {
                            Code = item.code,
                            Name = item.OrgStructureName,
                            ParentCode = item.ParentCode,
                            CompanyIndex = 2,
                            CreatedDate = DateTime.Now,
                            UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                        };

                        departmentDto = await _iC_DepartmentLogic.CheckExistedOrCreateAVN(departmentDto, row);
                        if (departmentDto != null)
                        {
                            if (departmentDto.IsInsert == true)
                            {
                                history.DataNew += 1;
                            }
                            if (departmentDto.IsUpdate == true)
                            {
                                history.DataUpdate += 1;
                            }
                        }
                        row++;
                    }

                    var checkParent = _dbContext.IC_Department.Where(x => x.IsInactive != true).ToList();
                    if (checkParent.Count > 0)
                    {
                        var childrenNode = checkParent.Where(x => !string.IsNullOrEmpty(x.Code) && !string.IsNullOrEmpty(x.ParentCode)).ToList();
                        foreach (var item in childrenNode)
                        {
                            var parent = checkParent.FirstOrDefault(x => x.Code == item.ParentCode);
                            if (parent == null) continue;
                            item.ParentIndex = parent.Index;
                            _dbContext.IC_Department.Update(item);
                        }

                        await _dbContext.SaveChangesAsync();
                    }
                    await _ic_IntegrateLogic.SaveOrUpdateDepartmentIntegrate(itemResult);
                }
            }
            catch (Exception ex)
            {
                history.Reason = ex.Message;
                history.IsSuccess = false;

                _logger.LogError($"AutoGetAVNDepartment: {ex}");
            }
            await AddHistoryTrackingIntegrate(history);
        }


        public async Task AutoGetDepartmentStandard()
        {
            var history = new IC_HistoryTrackingIntegrate
            {
                JobName = "AutoGetDepartmentStandard",
                IsSuccess = true
            };
            try
            {
                var itemResult = await _iC_Employee_IntegrateLogic.GetAllDepartmentIntegrateAsync();

                if (itemResult.Count > 0)
                {
                    var oldDepartmentList = await _iC_DepartmentLogic.OldDepartmentUpdateStandard(2, itemResult);

                    history.DataDelete = (short)oldDepartmentList;

                    var row = 1;
                    foreach (var item in itemResult)
                    {
                        var departmentDto = new IC_DepartmentDTO()
                        {
                            Code = item.Code,
                            Name = item.Name,
                            ParentCode = item.ParentCode,
                            CompanyIndex = 2,
                            CreatedDate = DateTime.Now,
                            UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                        };

                        departmentDto = await _iC_DepartmentLogic.CheckExistedOrCreateAVN(departmentDto, row);
                        if (departmentDto != null)
                        {
                            if (departmentDto.IsInsert == true)
                            {
                                history.DataNew += 1;
                            }
                            if (departmentDto.IsUpdate == true)
                            {
                                history.DataUpdate += 1;
                            }
                        }
                        row++;
                    }

                    var checkParent = _dbContext.IC_Department.Where(x => x.IsInactive != true).ToList();
                    if (checkParent.Count > 0)
                    {
                        var childrenNode = checkParent.Where(x => !string.IsNullOrEmpty(x.Code) && !string.IsNullOrEmpty(x.ParentCode)).ToList();
                        foreach (var item in childrenNode)
                        {
                            var parent = checkParent.FirstOrDefault(x => x.Code == item.ParentCode);
                            if (parent == null) continue;
                            item.ParentIndex = parent.Index;
                            _dbContext.IC_Department.Update(item);
                        }

                        await _dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                history.Reason = ex.Message;
                history.IsSuccess = false;

                _logger.LogError($"AutoGetAVNDepartment: {ex}");
            }
            await AddHistoryTrackingIntegrate(history);
        }


        public async Task AutoGetTransferUserStandard(IC_ConfigDTO config)
        {
            var history = new IC_HistoryTrackingIntegrate
            {
                JobName = "AutoGetTransferUserStandard",
                IsSuccess = true
            };
            try
            {
                var isSync = false;
                #region Sync data
                var addedParams = new List<AddedParam>
                {
                    new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                    new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER }
                };
                var systemconfigs = await _iC_ConfigLogic.GetMany(addedParams);
                if (systemconfigs != null)
                {
                    var sysconfig = systemconfigs.FirstOrDefault();
                    if (sysconfig != null)
                    {
                        if (sysconfig.IntegrateLogParam.AutoIntegrate)
                        {
                            isSync = true;
                        }
                    }
                }
                #endregion

                var itemResult = await _iC_Employee_IntegrateLogic.GetAllTransferUserIntegrateAsync();

                if (itemResult.Count > 0)
                {
                    var employeeLst = itemResult.Select(x => x.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0')).ToList();
                    var checkData = _dbContext.IC_WorkingInfo.Where(x => employeeLst.Contains(x.EmployeeATID) && x.CompanyIndex == _config.CompanyIndex).ToList();

                    foreach (var item in itemResult)
                    {
                        if (!string.IsNullOrEmpty(item.EmployeeATID) && !string.IsNullOrEmpty(item.DepartmentTo))
                        {
                            var department = _dbContext.IC_Department.FirstOrDefault(x => x.Code == item.DepartmentTo);
                            if (department == null)
                            {
                                continue;
                            }
                            if (item.ToDate != null)
                            {
                                var checkDataTime = _dbContext.IC_EmployeeTransfer.Where(t => t.EmployeeATID == item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0')
                               && (t.Status == (short)TransferStatus.Pendding || t.Status == (short)TransferStatus.Approve) &&
                              item.FromDate <= t.ToTime && item.ToDate >= t.FromTime).ToList();

                                if (checkDataTime != null && checkDataTime.Count > 0)
                                {
                                    continue;
                                }

                                var workingInfo = _dbContext.IC_WorkingInfo.FirstOrDefault(u => u.EmployeeATID == item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0')
                                             && u.Status == (short)TransferStatus.Approve && u.FromDate.Date <= DateTime.Now.Date
                                             && (!u.ToDate.HasValue || u.ToDate.Value.Date >= DateTime.Now.Date));

                                IC_EmployeeTransfer empTranfer = new IC_EmployeeTransfer();
                                empTranfer.EmployeeATID = item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
                                empTranfer.CompanyIndex = _config.CompanyIndex;
                                empTranfer.ToTime = item.ToDate.Value.AddDays(1);
                                empTranfer.FromTime = item.FromDate;
                                empTranfer.IsSync = null;
                                empTranfer.CreatedDate = DateTime.Today;
                                empTranfer.UpdatedDate = DateTime.Today;
                                empTranfer.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                                empTranfer.NewDepartment = department.Index;
                                empTranfer.OldDepartment = workingInfo == null ? 0 : workingInfo.DepartmentIndex;
                                empTranfer.Status = (short)ApproveStatus.Approved;
                                if (isSync)
                                {
                                    empTranfer.AddOnNewDepartment = true;
                                    empTranfer.RemoveFromOldDepartment = false;
                                }
                                _dbContext.IC_EmployeeTransfer.Add(empTranfer);

                            }
                            else
                            {

                                var checkEmployeeData = checkData.Where(x => x.EmployeeATID == item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0') &&
                               (item.FromDate.Date <= x.FromDate.Date || (x.ToDate.HasValue && item.FromDate.Date <= x.ToDate.Value.Date) || x.DepartmentIndex == 0) && (x.Status == (short)TransferStatus.Approve || x.Status == (short)TransferStatus.Pendding));
                                if (checkEmployeeData != null && checkEmployeeData.Count() > 0)
                                {
                                    var haveDepartment = checkEmployeeData.FirstOrDefault(u => u.DepartmentIndex > 0);
                                    if (haveDepartment != null)
                                    {
                                        continue;
                                    }
                                }

                                var noDeartment = checkEmployeeData.FirstOrDefault(u => u.DepartmentIndex == 0 && u.ToDate.HasValue);
                                if (noDeartment != null)
                                {
                                    noDeartment.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                                    noDeartment.DepartmentIndex = department.Index;
                                    noDeartment.Status = (short)TransferStatus.Approve;
                                    noDeartment.FromDate = DateTime.Today;
                                    noDeartment.UpdatedDate = DateTime.Today;
                                    _dbContext.IC_WorkingInfo.Update(noDeartment);
                                }
                                else
                                {
                                    IC_WorkingInfo workingInfo = new IC_WorkingInfo();
                                    workingInfo.CompanyIndex = _config.CompanyIndex;
                                    workingInfo.EmployeeATID = item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
                                    workingInfo.DepartmentIndex = department.Index;
                                    workingInfo.FromDate = item.FromDate;
                                    workingInfo.UpdatedDate = DateTime.Today;
                                    workingInfo.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                                    workingInfo.Status = (short)TransferStatus.Approve;
                                    _dbContext.IC_WorkingInfo.Add(workingInfo);

                                    var data = checkData.Where(u => u.CompanyIndex == _config.CompanyIndex && u.EmployeeATID == item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0') && u.ToDate == null && u.Status == (short)TransferStatus.Approve)
                                   .OrderBy(u => u.FromDate).ToList();
                                    if (data != null)
                                    {
                                        //ReOrderWorkingInfo(listWorkingInfo);
                                        for (int i = 0; i < data.Count; i++)
                                        {
                                            if (i == data.Count - 1)
                                            {
                                                data[i].ToDate = item.FromDate.AddDays(-1);
                                            }
                                            else
                                            {
                                                data[i].ToDate = data[i + 1].FromDate.AddDays(-1);
                                            }
                                            _dbContext.IC_WorkingInfo.Update(data[i]);
                                        }
                                    }
                                }


                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                history.Reason = ex.Message;
                history.IsSuccess = false;

                _logger.LogError($"AutoGetAVNDepartment: {ex}");
            }
            await AddHistoryTrackingIntegrate(history);
        }

        public void CheckCardActivedOrCreate(HR_CardNumberInfo hrCardInfo, int pCompanyIndex, List<HR_CardNumberInfo> lsthrCardInfo)
        {
            if (!string.IsNullOrWhiteSpace(hrCardInfo.CardNumber))
            {
                var existed = lsthrCardInfo.Where(e => e.CompanyIndex == pCompanyIndex && hrCardInfo.EmployeeATID == e.EmployeeATID && e.IsActive == true).ToList();

                if (existed != null && !existed.Select(x => x.CardNumber).Contains(hrCardInfo.CardNumber))
                {
                    foreach (var item in existed)
                    {
                        item.IsActive = false;
                        item.UpdatedDate = DateTime.Now;
                        _dbContext.HR_CardNumberInfo.Update(item);
                    }

                    var newCard = new HR_CardNumberInfo();
                    hrCardInfo.Index = 0;
                    newCard = _Mapper.Map<HR_CardNumberInfo>(hrCardInfo);
                    newCard.IsActive = true;
                    newCard.CreatedDate = DateTime.Now;
                    _dbContext.HR_CardNumberInfo.Add(newCard);
                }
                else if (existed == null)
                {
                    var item = new HR_CardNumberInfo();
                    item = _Mapper.Map<HR_CardNumberInfo>(hrCardInfo);
                    item.IsActive = true;
                    item.CreatedDate = DateTime.Now;
                    _dbContext.HR_CardNumberInfo.Add(item);
                }
            }

        }

        public async Task AutoGetAVNEmployeeShift(DateTime now)
        {
            var timePostCheck = now.ToHHmm();
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = 2 },
                new AddedParam { Key = "EventType", Value = ConfigAuto.EMPLOYEE_SHIFT_INTEGRATE.ToString() }
            };
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            try
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {
                    if (config.TimePos.Contains(timePostCheck))
                    {
                        if (!string.IsNullOrWhiteSpace(_AppConfigAVN?.Domain))
                        {
                            var token = await GetToken();
                            var lstHeader = new List<WebAPIHeader>
                                { new WebAPIHeader("Authorization", "Bearer " + token) };
                            var client = new HttpClient();
                            client.Timeout = TimeSpan.FromMinutes(10);
                            foreach (var header in lstHeader)
                                client.DefaultRequestHeaders.Add(header.Name, header.Value);

                            await AutoGetAVNShift(client, config);
                            await AutoGetAVNOverTimePlanTime(client, config);
                            await AutoGetAVNEmployeeShift(client, config);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoGetAVNEmployeeShift: {ex}");
            }
        }

        public void CreateHistoryOnlineOfflineMachine()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();

            var groupIndexes = db.IC_DeviceHistory.AsNoTracking().GroupBy(x => x.SerialNumber).Select(x => x.Max(y => y.Index)).ToList();
            var groupLastConnect = db.IC_DeviceHistory.AsNoTracking().Where(x => groupIndexes.Contains(x.Index)).ToList();
            var listDevice = db.IC_Device.AsNoTracking().Where(x => x.CompanyIndex == 2).ToList();

            if (listDevice != null && listDevice.Count > 0)
            {
                var isSave = false;
                foreach (var device in listDevice)
                {
                    var deviceHistory = groupLastConnect.FirstOrDefault(x => x.SerialNumber == device.SerialNumber);
                    if (device != null)
                    {
                        var isOffLine = IsDeviceOfflined(device, _cache, null);
                        var status = isOffLine ? (short)DeviceOnOffStatus.Offline : (short)DeviceOnOffStatus.Online;
                        if (deviceHistory == null)
                        {
                            deviceHistory = new IC_DeviceHistory();
                            deviceHistory.Date = device.LastConnection ?? DateTime.Now;
                            deviceHistory.Status = status;
                            deviceHistory.SerialNumber = device.SerialNumber;
                            deviceHistory.UpdatedDate = DateTime.Now;
                            db.IC_DeviceHistory.Add(deviceHistory);
                            isSave = true;
                        }
                        else if (status != deviceHistory.Status)
                        {
                            deviceHistory = new IC_DeviceHistory();
                            deviceHistory.Date = device.LastConnection ?? DateTime.Now;
                            deviceHistory.Status = status;
                            deviceHistory.SerialNumber = device.SerialNumber;
                            deviceHistory.UpdatedDate = DateTime.Now;
                            db.IC_DeviceHistory.Add(deviceHistory);
                            isSave = true;
                        }
                    }
                }
                if (isSave)
                {
                    db.SaveChanges();
                }

            }

            scope.Dispose();
        }

        public async Task IntegrateParking(DateTime now)
        {
            var timePostCheck = now.ToHHmm();
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = 2 },
                new AddedParam { Key = "EventType", Value = ConfigAuto.RE_PROCESSING_REGISTERCARD.ToString() }
            };
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);

            try
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {
                    if (config.TimePos.Contains(timePostCheck) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                    {
                        await IntegrateObject(config);
                        await IntegrateVehicleType(config);
                        await IntegrateDepartment(config);
                        await IntegrateCustomer(config);
                        await IntegrateVehicles(config);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoGetAVNEmployeeShift: {ex}");
            }
        }

        public async Task IntegrateParkingLog(DateTime now)
        {
            var timePostCheck = now.ToHHmm();
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = 2 },
                new AddedParam { Key = "EventType", Value = ConfigAuto.DOWNLOAD_PARKING_LOG.ToString() }
            };
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);

            try
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {
                    if (config.TimePos.Contains(timePostCheck) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                    {
                        await IntegrateLogParking(config);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoGetAVNEmployeeShift: {ex}");
            }
        }

        public async Task IntegrateLogParking(IC_ConfigDTO config)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();

                DateTime toTime = DateTime.Now;
                DateTime fromTime = toTime.AddDays(-(config.PreviousDays.Value));

                fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
                toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);

                var json = "time_start=" + fromTime.ToddMMyyyyHHmmss() + "&time_end=" + toTime.ToddMMyyyyHHmmss() + "&limit=100000&offset=0";
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                var byteContent = new ByteArrayContent(buffer);
                var client = new HttpClient();
                client.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                HttpResponseMessage response = await client.PostAsync("api/ths/v1/log_in_out/search_by_time", byteContent);

                var result = await response.Content.ReadAsStringAsync();
                var itemPosition = JsonConvert.DeserializeObject<IC_LogInOut>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var lstLog = db.IC_VehicleLog.Where(x => !string.IsNullOrEmpty(x.IntegrateString) && x.FromDate >= fromTime).ToList();
                var lstDevice = db.IC_Device.Where(x => x.DeviceModel == "PA" && x.ParkingType == (int)ParkingType.Lovad).ToList();

                if (itemPosition != null && itemPosition.LogList.Count > 0)
                {
                    foreach (var item in itemPosition.LogList)
                    {
                        if (lstLog.Any(x => x.IntegrateString == item.Id))
                        {
                            var data = lstLog.FirstOrDefault(x => x.IntegrateString == item.Id);
                            data.EmployeeATID = item.CustomerCode;
                            data.VehicleTypeId = !string.IsNullOrEmpty(item.VehicleCode) ? int.Parse(item.VehicleCode) : 0;
                            if (!string.IsNullOrEmpty(item.FromDate))
                            {
                                string[] formats = { "dd/MM/yyyy HH:mm:ss" };
                                var eTADay = new DateTime();

                                var convertFromDate = DateTime.TryParseExact(item.FromDate, formats,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out eTADay);

                                data.FromDate = eTADay;
                            }

                            if (!string.IsNullOrEmpty(item.ToDate))
                            {
                                string[] formats = { "dd/MM/yyyy HH:mm:ss" };
                                var eTADay = new DateTime();

                                var convertFromDate = DateTime.TryParseExact(item.ToDate, formats,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out eTADay);

                                data.ToDate = eTADay;
                            }

                            data.ComputerIn = item.LaneIn.ToString();


                            data.ComputerOut = item.LaneOut.ToString();


                            data.Reason = item.Reason;

                            db.IC_VehicleLog.Update(data);
                        }
                        else
                        {
                            var data = new IC_VehicleLog();
                            data.IntegrateString = item.Id;
                            data.EmployeeATID = item.CustomerCode;
                            data.VehicleTypeId = !string.IsNullOrEmpty(item.VehicleCode) ? int.Parse(item.VehicleCode) : 0;
                            if (!string.IsNullOrEmpty(item.FromDate))
                            {
                                string[] formats = { "dd/MM/yyyy HH:mm:ss" };
                                var eTADay = new DateTime();

                                var convertFromDate = DateTime.TryParseExact(item.FromDate, formats,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out eTADay);

                                data.FromDate = eTADay;
                            }

                            if (!string.IsNullOrEmpty(item.ToDate))
                            {
                                string[] formats = { "dd/MM/yyyy HH:mm:ss" };
                                var eTADay = new DateTime();

                                var convertFromDate = DateTime.TryParseExact(item.ToDate, formats,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out eTADay);

                                data.ToDate = eTADay;
                            }

                            data.ComputerIn = item.LaneIn.ToString();


                            data.ComputerOut = item.LaneOut.ToString();

                            data.Reason = item.Reason;

                            await db.IC_VehicleLog.AddAsync(data);
                        }
                    }

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateLogParking: {ex}");
            }

        }

        public async Task IntegrateObject(IC_ConfigDTO config)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            var lstContent = db.HR_UserType.Select(x => new IC_CustomerTypeParam
            {
                Code = x.UserTypeId.ToString(),
                Name = x.Name,
            }).ToList();

            foreach (var item in lstContent)
            {
                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                    var json = SerializeObjectToString(item);
                    var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                    var byteContent = new ByteArrayContent(buffer);
                    HttpResponseMessage response = await client.PostAsync("api/ths/v1/customer_type/save", byteContent);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateObject: {ex}");
                }
            }
        }

        public async Task IntegrateCustomer(IC_ConfigDTO config)
        {
            using var scope = _scopeFactory.CreateScope();
            var now = DateTime.Now;
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();

            var blackLists = db.GC_BlackList.Where(x => string.IsNullOrEmpty(x.ReasonRemove) && x.FromDate.Date <= now.Date && (x.ToDate == null || (x.ToDate != null && now.Date <= x.ToDate.Value.Date)));
            var employeeLst = (from e in db.HR_User
                               join w in db.IC_WorkingInfo.Where(w => w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date && (!w.ToDate.HasValue || w.ToDate.Value.Date >= DateTime.Now.Date))
                  on e.EmployeeATID equals w.EmployeeATID into WorkingIn
                               from worrk in WorkingIn.DefaultIfEmpty()

                               join d in db.IC_Department.Where(x => x.IsInactive != true)
                               on worrk.DepartmentIndex equals d.Index into deptGroup
                               from dept in deptGroup.DefaultIfEmpty()

                               select new IC_CustomerParam
                               {
                                   Code = e.EmployeeATID,
                                   CustomerTypeCode = (e.EmployeeType == null ? (int)EmployeeType.Employee : e.EmployeeType).ToString(),
                                   DeparmentCode = worrk != null && worrk.DepartmentIndex != 0 ? worrk.DepartmentIndex.ToString() : "0",
                                   Name = e.FullName,
                                   Blacklist = blackLists.Any(x => x.EmployeeATID == e.EmployeeATID || (!string.IsNullOrEmpty(x.Nric) && x.Nric == e.Nric)) ? 1 : 0
                               }).ToList();



            var listSplitEmployeeID = CommonUtils.SplitList(employeeLst, 500);
            foreach (var item in listSplitEmployeeID)
            {
                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                    var json = JsonConvert.SerializeObject(item);
                    json = "list_data=" + json;
                    var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                    var byteContent = new ByteArrayContent(buffer);
                    HttpResponseMessage response = await client.PostAsync("api/ths/v1/customer/save_list", byteContent);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                        var result = await response.Content.ReadAsStringAsync();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateCustomer: {ex}");
                }
            }
        }
        public async Task IntegrateVehicles(IC_ConfigDTO config)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            var employeeLst = (from emp in db.HR_User
                               join pkla in db.GC_EmployeeVehicle
                           on emp.EmployeeATID equals pkla.EmployeeATID
                           into pke
                               from pkeResult in pke.DefaultIfEmpty()

                               join wk in db.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                   && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                               on emp.EmployeeATID equals wk.EmployeeATID
                               into pkw
                               from pkwResult in pkw.DefaultIfEmpty()
                               where emp.EmployeeType != (int)EmployeeType.Guest
                               select new IC_CustomerVehicle
                               {
                                   Code = emp.EmployeeATID,
                                   VehicleBrand = pkeResult != null ? pkeResult.Branch : "",
                                   VehicleCode = "1",
                                   VehicleNumber = pkeResult != null ? pkeResult.Plate : "",
                                   DateStart = pkeResult != null ? pkeResult.FromDate.ToddMMyyyy() : (pkwResult != null ? pkwResult.FromDate.ToddMMyyyy() : DateTime.Now.ToddMMyyyy()),
                                   DateEnd = pkeResult != null && pkeResult.ToDate.HasValue ? pkeResult.ToDate.Value.ToddMMyyyy() : (pkwResult != null && pkwResult.ToDate.HasValue ? pkwResult.ToDate.Value.ToddMMyyyy() : ""),
                                   TimeStart = "00:00",
                                   TimeEnd = "23:59"
                               }).ToList();

            var employeeLst1 = (from emp in db.HR_User
                                join pkla in db.GC_CustomerVehicle
                            on emp.EmployeeATID equals pkla.EmployeeATID
                            into pke
                                from pkeResult in pke.DefaultIfEmpty()
                                join cus in db.HR_CustomerInfo
                              on emp.EmployeeATID equals cus.EmployeeATID
                              into cuss
                                from cusResult in cuss.DefaultIfEmpty()
                                where emp.EmployeeType == (int)EmployeeType.Guest
                                select new IC_CustomerVehicle
                                {
                                    Code = emp.EmployeeATID,
                                    VehicleBrand = pkeResult != null ? pkeResult.Branch : "",
                                    VehicleCode = "1",
                                    VehicleNumber = pkeResult != null ? pkeResult.Plate : "",
                                    DateStart = cusResult != null ? cusResult.FromTime.ToddMMyyyy() : "",
                                    DateEnd = cusResult != null ? cusResult.ToTime.ToddMMyyyy() : "",
                                    TimeStart = cusResult != null ? cusResult.FromTime.ToHHmm() : "",
                                    TimeEnd = cusResult != null ? cusResult.ToTime.ToHHmm() : ""
                                }).ToList();

            employeeLst.AddRange(employeeLst1);

            var listSplitEmployeeID = CommonUtils.SplitList(employeeLst, 500);
            foreach (var item in listSplitEmployeeID)
            {
                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                    var json = JsonConvert.SerializeObject(item);
                    json = "list_data=" + json;
                    var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                    var byteContent = new ByteArrayContent(buffer);
                    HttpResponseMessage response = await client.PostAsync("api/ths/v1/customer_vehicle/save_list", byteContent);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                        var result = await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateVehicles: {ex}");
                }
            }
        }
        public async Task IntegrateVehicleType(IC_ConfigDTO config)
        {
            var lstContent = new List<IC_CustomerTypeParam>()
            {
                new IC_CustomerTypeParam(){Code = "1", Name = "Xe máy"},
                new IC_CustomerTypeParam(){Code = "2", Name = "Xe đạp"},
                new IC_CustomerTypeParam(){Code = "3", Name = "Xe đạp điện"},
                new IC_CustomerTypeParam(){Code = "4", Name = "Xe oto"},
            };
            foreach (var item in lstContent)
            {
                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                    var json = SerializeObjectToString(item);
                    var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                    var byteContent = new ByteArrayContent(buffer);
                    HttpResponseMessage response = await client.PostAsync("api/ths/v1/vehicle/save", byteContent);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateVehicle: {ex}");
                }
            }
        }

        public async Task IntegrateDepartment(IC_ConfigDTO config)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            var lstContent = await db.IC_Department.Where(x => x.CompanyIndex == config.CompanyIndex && x.IsInactive != true).ToListAsync();
            foreach (var item in lstContent)
            {
                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                    var json = SerializeDepartmentToString(item);
                    var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                    var byteContent = new ByteArrayContent(buffer);
                    HttpResponseMessage response = await client.PostAsync("api/ths/v1/department/save", byteContent);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateVehicle: {ex}");
                }
            }
        }


        public string SerializeObjectToString(IC_CustomerTypeParam customerTypes)
        {
            var json = "code=" + customerTypes.Code + "&name=" + customerTypes.Name;
            return json;
        }

        public string SerializeDepartmentToString(IC_Department customerTypes)
        {
            var json = "code=" + customerTypes.Index + "&name=" + customerTypes.Name + "&parentcode=" + customerTypes.ParentIndex ?? "";
            return json;
        }

        public async Task IntegrateInfoToOffline(DateTime now)
        {
            var timePostCheck = now.ToHHmm();
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = 2 },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString() }
            };
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);

            try
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null)
                {
                    if (config.TimePos.Contains(timePostCheck) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                    {
                        await _hR_EmployeeLogic.IntegrateBlackListToOffline(null, null, config.IntegrateLogParam.LinkAPIIntegrate);
                        await _hR_EmployeeLogic.IntegrateCardToOffline(null, config.IntegrateLogParam.LinkAPIIntegrate);
                        await _hR_EmployeeLogic.IntegrateDepartmentToOffline(null, config.IntegrateLogParam.LinkAPIIntegrate);
                        await _hR_EmployeeLogic.IntegrateCustomerCardToOffline(null, config.IntegrateLogParam.LinkAPIIntegrate);
                        await _hR_EmployeeLogic.IntegrateUserToOffline(null, config.IntegrateLogParam.LinkAPIIntegrate);
                        await _hR_EmployeeLogic.IntegrateWorkingInfo(null, config.IntegrateLogParam.LinkAPIIntegrate);
                        await _hR_EmployeeLogic.IntegrateEmployeeToOffline(null, config.IntegrateLogParam.LinkAPIIntegrate);
                        await _hR_EmployeeLogic.IntegrateCustmerToOffline(null, config.IntegrateLogParam.LinkAPIIntegrate);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoGetAVNEmployeeShift: {ex}");
            }
        }

        public async Task AutoDeleteBlacklist(DateTime now)
        {
            string timePostCheck = DateTime.Now.ToHHmm();
            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.AUTO_DELETE_BLACKLIST.ToString() });
            try
            {
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                if (downloadConfig != null && downloadConfig.Any())
                {
                    var config = downloadConfig.First();
                    List<CommandResult> lstCmd = new List<CommandResult>();
                    if (config != null)
                    {
                        if (!config.TimePos.Contains(timePostCheck)) { return; }

                        var lstGroupDevice = config.IntegrateLogParam.ListSerialNumber.Split(';').ToList();
                        if (lstGroupDevice.Count == 0)
                        {
                            return;
                        }

                        var groupDevice = lstGroupDevice.Select(int.Parse).ToList();
                        var lstSerials = await _dbContext.IC_GroupDeviceDetails.Where(x => groupDevice.Contains(x.GroupDeviceIndex)).Select(x => x.SerialNumber).ToListAsync();
                        if (lstSerials == null || lstSerials.Count == 0)
                        {
                            return;
                        }
                        var deleteCommandListSerial = new List<string>();
                        ListSerialCheckHardWareLicense(lstSerials, ref deleteCommandListSerial);
                        if (deleteCommandListSerial != null && deleteCommandListSerial.Count > 0)
                        {
                            var blacklist = await _dbContext.GC_BlackList.Where(x => x.FromDate.Date == now.Date).ToListAsync();
                            if (blacklist != null && blacklist.Count > 0)
                            {
                                var lstBlacklistDelete = new List<string>();
                                var employee = blacklist.Where(x => !string.IsNullOrEmpty(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
                                var nric = blacklist.Where(x => string.IsNullOrEmpty(x.EmployeeATID)).Select(x => x.Nric).ToList();
                                if (nric != null && nric.Count > 0)
                                {
                                    var employeeLst = await (from e in _dbContext.HR_User
                                                             where nric.Contains(e.Nric) && (e.EmployeeType == (int)EmployeeType.Employee || e.EmployeeType == null)
                                                             select e.EmployeeATID).ToListAsync();

                                    if (employeeLst != null && employeeLst.Count > 0)
                                    {
                                        lstBlacklistDelete.AddRange(employeeLst);
                                    }

                                    var customerLst = await (from e in _dbContext.HR_User
                                                             join w in _dbContext.HR_CustomerInfo
                                                             on e.EmployeeATID equals w.EmployeeATID
                                                             where nric.Contains(e.Nric) && (e.EmployeeType == (int)EmployeeType.Guest)
                                                             select e.EmployeeATID).ToListAsync();
                                    if (customerLst != null && customerLst.Count > 0)
                                    {
                                        lstBlacklistDelete.AddRange(customerLst);
                                    }
                                }

                                if (employee != null && employee.Count > 0)
                                {
                                    lstBlacklistDelete.AddRange(employee);
                                }
                                var listUser = new List<UserInfoOnMachine>();
                                if (deleteCommandListSerial.Count > 0)
                                {
                                    IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                                    paramUserOnMachine.ListEmployeeaATID = lstBlacklistDelete;
                                    paramUserOnMachine.CompanyIndex = config.CompanyIndex;
                                    paramUserOnMachine.ListSerialNumber = deleteCommandListSerial;
                                    paramUserOnMachine.AuthenMode = "";
                                    paramUserOnMachine.FullInfo = true;
                                    listUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                                }
                                var listServiceAndDevice = _iC_CommandLogic.GetServiceType(deleteCommandListSerial);

                                var listUpload = listServiceAndDevice.Where(x => deleteCommandListSerial.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                                if (lstBlacklistDelete != null && lstBlacklistDelete.Count > 0)
                                {
                                    var cmd = CreateAutoDeleteUserCommandNew(lstBlacklistDelete.ToList(), DateTime.Now, DateTime.Now, null, deleteCommandListSerial, listUser, listServiceAndDevice);
                                    lstCmd.AddRange(cmd);
                                }
                            }
                        }

                        if (deleteCommandListSerial != null && deleteCommandListSerial.Count > 0)
                        {
                            var blacklist = await _dbContext.GC_BlackList.Where(x => x.ToDate != null && x.ToDate.Value.Date == now.Date.AddDays(-1)).ToListAsync();
                            if (blacklist != null && blacklist.Count > 0)
                            {
                                var lstBlacklistDelete = new List<string>();
                                var employee = blacklist.Where(x => !string.IsNullOrEmpty(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
                                var nric = blacklist.Where(x => string.IsNullOrEmpty(x.EmployeeATID)).Select(x => x.Nric).ToList();
                                if (nric != null && nric.Count > 0)
                                {
                                    var employeeLst = await (from e in _dbContext.HR_User
                                                             where nric.Contains(e.Nric) && (e.EmployeeType == (int)EmployeeType.Employee || e.EmployeeType == null)
                                                             select e.EmployeeATID).ToListAsync();

                                    if (employeeLst != null && employeeLst.Count > 0)
                                    {
                                        lstBlacklistDelete.AddRange(employeeLst);
                                    }

                                    var customerLst = await (from e in _dbContext.HR_User
                                                             join w in _dbContext.HR_CustomerInfo
                                                             on e.EmployeeATID equals w.EmployeeATID
                                                             where nric.Contains(e.Nric) && (e.EmployeeType == (int)EmployeeType.Guest)
                                                             select e.EmployeeATID).ToListAsync();
                                    if (customerLst != null && customerLst.Count > 0)
                                    {
                                        lstBlacklistDelete.AddRange(customerLst);
                                    }
                                }

                                if (employee != null && employee.Count > 0)
                                {
                                    lstBlacklistDelete.AddRange(employee);
                                }



                                var isMondelez = ClientName.MONDELEZ.ToString() == _configClientName;
                                if (lstBlacklistDelete.Count > 0)
                                {
                                    var listWorkingInfoAll = await _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == config.CompanyIndex
                                              && x.FromDate.Date <= DateTime.Now && (!x.ToDate.HasValue || x.ToDate > DateTime.Now.Date)
                                               && (!isMondelez || (x.DepartmentIndex != 0))
                                              && lstBlacklistDelete.Contains(x.EmployeeATID) && x.Status == (long)TransferStatus.Approve).OrderBy(t => t.FromDate).ToListAsync();

                                    var customerLst = (from e in listWorkingInfoAll
                                                       join w in _dbContext.IC_DepartmentAndDevice
                                                       on e.DepartmentIndex equals w.DepartmentIndex
                                                       where lstSerials.Contains(w.SerialNumber) && w.SerialNumber != null
                                                       select new
                                                       {
                                                           EmployeeATID = e.EmployeeATID,
                                                           SerialNumber = w.SerialNumber
                                                       }).ToList();

                                    var customerLstUp = customerLst.GroupBy(x => x.SerialNumber).Select(x => new { Key = x.Key, Value = x }).ToList();

                                    foreach (var item in customerLstUp)
                                    {
                                        var listUser = new List<UserInfoOnMachine>();

                                        IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                                        paramUserOnMachine.ListEmployeeaATID = item.Value.Select(x => x.EmployeeATID).ToList();
                                        paramUserOnMachine.CompanyIndex = config.CompanyIndex;
                                        paramUserOnMachine.ListSerialNumber = new List<string> { item.Key };
                                        paramUserOnMachine.AuthenMode = "";
                                        paramUserOnMachine.FullInfo = true;
                                        listUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);


                                        var listServiceAndDevice = _iC_CommandLogic.GetServiceType(new List<string> { item.Key });

                                        var listUpload = listServiceAndDevice.Where(x => x.SerialNumber == item.Key).Select(x => x.SerialNumber).ToList();
                                        if (lstBlacklistDelete != null && lstBlacklistDelete.Count > 0)
                                        {
                                            var cmd = CreateAutoUploadUserCommandNew(lstBlacklistDelete.ToList(), DateTime.Now, DateTime.Now, null, true, new List<string> { item.Key }, listUser, listServiceAndDevice);
                                            lstCmd.AddRange(cmd);
                                        }
                                    }
                                }
                            }
                        }

                        if (lstCmd != null && lstCmd.Count > 0)
                        {
                            IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                            grouComParam.CompanyIndex = config.CompanyIndex;
                            grouComParam.ListCommand = lstCmd;
                            grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                            grouComParam.GroupName = GroupName.TransferEmployee.ToString();
                            grouComParam.EventType = config.EventType;
                            _iC_CommandLogic.CreateGroupCommands(grouComParam);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoGetOVNEmployeeShift: {ex}");
            }
        }
    }

    public interface IIC_ScheduleAutoHostedLogic
    {
        void CreateHistoryOnlineOfflineMachine();
        Task DownloadLogFromToTime();
        Task DeleteLogFromToTime();
        Task DownloadAllUser();
        Task RestartDevice();
        Task CheckDeviceFullCapacity();
        void CreateEmailWhenDeviceOffline();
        void AutomaticSendMail();
        Task AutoRemoveStoppedWorkingEmployeesData();
        Task AutoSyncEmployeeToDatabase1Office(IC_ConfigDTO config);
        Task AutoSyncDeviceTime();
        Task AutoIntegrateLog();
        void AutoAddOrDeleteUser(DateTime pNow);
        void AutoAddOrDeleteCustomer(DateTime pNow);
        Task AutoDeleteSystemCommand();
        Task AutoDeleteExecutedCommand();
        Task AutoUpdateExpiratedTransferEmployee();
        Task AutoGetOVNDepartment();
        Task AutoGetOVNEmployee();
        Task AutoGetOVNEmployeeShift();
        Task AutoIntergrateAttLogAVN();
        Task AutoGetAEONEmployee(List<UserDepartmentMappingsDto> UserDepartmentMappings, IC_ConfigDTO config);
        void AutoUpdateLastConnectionOfFR05();
        void AddDepartmentOnCache();
        void CreateCheckDeviceCapacityCommands();
        Task AddHistoryTrackingIntegrate(IC_HistoryTrackingIntegrate iC_HistoryTrackingIntegrate);
        List<CommandResult> CreateUploadUserCommand(int pCompanyIndex, long pNewDepartment, string pEmp, DateTime pFromTime, DateTime pToTime, string pExternalData, bool isOverwriteData);
        List<CommandResult> CreateAutoUploadUserCommand(int pCompanyIndex, long pNewDepartment, string pEmp, DateTime pFromTime,
                DateTime pToTime, string pExternalData, bool isOverwriteData, List<string> lstSerial, List<UserInfoOnMachine> listUser, List<IC_ServiceAndDeviceDTO> listServiceAndDevice);
        Task AutoGetAVNEmployeeShift(DateTime now);
        Task AutoGetAVNIntegrateBusinessTravel(DateTime now);
        Task AutoGetDepartmentStandard();
        Task AutoGetDataToDatabase(DateTime now);
        Task AutoPostLogFromDatabase(DateTime now);
        Task AutoSyncIntegrateEmloyee(DateTime pNow);
        Task AutoSyncIntegrateEmloyeeManual(DateTime pNow);
        Task AutoGet1OfficeToDatabaseManual(DateTime now);
        Task AutoIntegrateLogManual(int previousDays);
        Task AutoPostLogFromDatabaseManual();
        Task IntegrateParking(DateTime now);
        Task AutoAddOrDeleteUserManual();
        void DeleteHolidayByDay();
        Task DownloadLogStateLog();
        Task IntegrateLogParking(IC_ConfigDTO config);
        Task IntegrateParkingLog(DateTime now);
        Task IntegrateInfoToOffline(DateTime now);
        Task AutoDeleteBlacklist(DateTime now);
    }
}

