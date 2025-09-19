using EPAD_Data.Models.TimeLog;
using EPAD_Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using System.Linq;
using EPAD_Common;
using EPAD_Data.Entities;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using EPAD_Common.Extensions;
using ClosedXML.Excel;
using EPAD_Common.FileProvider;
using EPAD_Common.EmailProvider;
using EPAD_Common.Locales;
using System.IO;
using EPAD_Common.Enums;
using LogType = EPAD_Data.LogType;
using EPAD_Common.Clients;
using EPAD_Data.HTTPClient;
using EPAD_Services.Interface.EZHR;
using System.Data.SqlTypes;
using EPAD_Logic;

namespace EPAD_Services.Business
{
    public abstract class MonitoringProcess
    {
        protected readonly IServiceProvider _ServiceProvider;
        protected ILogger _logger;
        protected IMemoryCache _cache;
        protected ObjectAccessType mAccessType = ObjectAccessType.Employee;
        protected ConfigObject _Config;
        protected EmployeeFullInfo mEmpInfo;
        protected CheckRuleResult mResult;
        protected short mInOutMode = 0;
        protected int mLineIndex = 0;
        protected int mGateIndex = 0;
        protected int mdeviceIndex = 0;
        protected bool mIsLobbyLog = false;
        protected int? mAreaGroup = null;
        protected bool mParkingProcess = false;
        protected GC_Customer mCustomer;
        protected List<int> mArrViolation = new List<int>();
        protected List<GroupDeviceParam> mGroupDevices = new List<GroupDeviceParam>();
        protected readonly IHR_UserService _HR_UserService;
        protected readonly IGC_Rules_GeneralService _GC_Rules_GeneralService;
        protected readonly IGC_TimeLogService _GC_TimeLogService;
        protected readonly IIC_VehicleLogService _IC_VehicleLogService;
        protected readonly IGC_TruckDriverLogService _GC_TruckDriverLogService;
        protected readonly IGC_TimeLog_ImageService _GC_TimeLog_ImageService;
        protected readonly IIC_GroupDeviceService _IC_GroupDeviceService;
        protected readonly IGC_Employee_AccessedGroupService _GC_Employee_AccessedGroupService;
        protected readonly IGC_Department_AccessedGroupService _GC_Department_AccessedGroupService;
        protected readonly IGC_AreaGroupService _GC_AreaGroupService;
        protected readonly IGC_Rules_GeneralAccess_GatesService _GC_Rules_GeneralAccess_GatesService;
        protected readonly IGC_Rules_General_LogService _GC_Rules_General_LogService;
        protected readonly IGC_Lines_CheckInCameraService _GC_Lines_CheckInCameraService;
        protected readonly IGC_Lines_CheckOutCameraService _GC_Lines_CheckOutCameraService;
        protected readonly IIC_CameraService _IC_CameraService;
        protected readonly IGC_CustomerService _GC_CustomerService;
        protected readonly IGC_Rules_CustomerService _GC_Rules_CustomerService;
        protected readonly IGC_Rules_Customer_GatesService _GC_Rules_Customer_GatesService;
        protected readonly IHR_EmployeeInfoService _HR_EmployeeInfoService;
        protected readonly IEzhrApiClient _EzhrClient;
        protected readonly IIC_UserMasterService _IC_UserMasterService;

        private readonly IEmailSender _EmailSender;
        private readonly IStoreFileProvider _FileHandler;
        private readonly ILocales i18n;
        protected readonly string _configClientName;
        protected IGC_Lines_CheckInRelayControllerService _GC_Lines_CheckInRelayControllerService;
        protected IGC_Lines_CheckOutRelayControllerService _GC_Lines_CheckOutRelayControllerService;
        protected IIC_ClientTCPControllerLogic _IIC_ClientTCPControllerLogic;
        protected IIC_RelayControllerService _IIC_RelayControllerService;
        protected IIC_RelayControllerChannelService _IC_RelayControllerChannelService;
        protected IIC_ModbusReplayControllerLogic _IIC_ModbusReplayControllerLogic;

        public MonitoringProcess(IServiceProvider pServiceProvider, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _ServiceProvider = pServiceProvider;
            _logger = loggerFactory.CreateLogger<MonitoringProcess>();
            _cache = TryResolve<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_cache);
            _HR_UserService = TryResolve<IHR_UserService>();
            _GC_Rules_GeneralService = TryResolve<IGC_Rules_GeneralService>();
            _GC_TimeLogService = TryResolve<IGC_TimeLogService>();
            _IC_VehicleLogService = TryResolve<IIC_VehicleLogService>();
            _GC_TruckDriverLogService = TryResolve<IGC_TruckDriverLogService>();
            _GC_TimeLog_ImageService = TryResolve<IGC_TimeLog_ImageService>();
            _IC_GroupDeviceService = TryResolve<IIC_GroupDeviceService>();
            _GC_Employee_AccessedGroupService = TryResolve<IGC_Employee_AccessedGroupService>();
            _GC_Department_AccessedGroupService = TryResolve<IGC_Department_AccessedGroupService>();
            _GC_AreaGroupService = TryResolve<IGC_AreaGroupService>();
            _GC_Rules_GeneralAccess_GatesService = TryResolve<IGC_Rules_GeneralAccess_GatesService>();
            _GC_Rules_General_LogService = TryResolve<IGC_Rules_General_LogService>();
            _GC_Lines_CheckInCameraService = TryResolve<IGC_Lines_CheckInCameraService>();
            _GC_Lines_CheckOutCameraService = TryResolve<IGC_Lines_CheckOutCameraService>();
            _GC_CustomerService = TryResolve<IGC_CustomerService>();
            _IC_CameraService = TryResolve<IIC_CameraService>();
            _GC_Rules_CustomerService = TryResolve<IGC_Rules_CustomerService>();
            _GC_Rules_Customer_GatesService = TryResolve<IGC_Rules_Customer_GatesService>();
            _HR_EmployeeInfoService = TryResolve<IHR_EmployeeInfoService>();

            _EmailSender = TryResolve<IEmailSender>();
            _FileHandler = TryResolve<IStoreFileProvider>();
            _EzhrClient = TryResolve<IEzhrApiClient>();

            i18n = TryResolve<ILocales>();
            _configClientName = configuration.GetValue<string>("ClientName").ToUpper();
            _GC_Lines_CheckInRelayControllerService = TryResolve<IGC_Lines_CheckInRelayControllerService>();
            _GC_Lines_CheckOutRelayControllerService = TryResolve<IGC_Lines_CheckOutRelayControllerService>();
            _IIC_ClientTCPControllerLogic = TryResolve<IIC_ClientTCPControllerLogic>();
            _IIC_RelayControllerService = TryResolve<IIC_RelayControllerService>();
            _IC_RelayControllerChannelService = TryResolve<IIC_RelayControllerChannelService>();
            _IC_UserMasterService = TryResolve<IIC_UserMasterService>();
            _IIC_ModbusReplayControllerLogic = TryResolve<IIC_ModbusReplayControllerLogic>();
        }

        protected T TryResolve<T>()
        {
            return _ServiceProvider.GetService<T>();
        }

        public async Task<bool> MainProcess(AttendanceLogRealTime param, int lineIndex, short inOutByLine, DateTime now, string classProcess, UserInfo user)
        {
            // push notification is data processing
            await PushProgressInfo(lineIndex, classProcess, _Config.CompanyIndex);

            //mEmpInfo = _HR_EmployeeBasicInfoService.GetInfoByEmployeeATIDAndFromToDateAsync(param.EmployeeATID, now, now, _AdminConfiguration.CompanyIndex).Result;
            var lstEmployee = new List<EmployeeFullInfo>();
            if (_configClientName == ClientName.MONDELEZ.ToString() && param.EmployeeType == (int)EmployeeType.Driver)
            {
                //lstEmployee = await _HR_UserService.GetDriverCompactInfoByEmployeeATID(new List<string> { param.EmployeeATID }, DateTime.Now, param.CompanyIndex);
                var employee = await _HR_UserService.GetDriverCompactInfoByEmployeeATIDOrCardId(param.EmployeeATID, param.FullName, DateTime.Now, param.CompanyIndex);
                if (employee != null)
                {
                    lstEmployee = new List<EmployeeFullInfo> { employee };
                }
            }
            else
            {
                lstEmployee = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(new List<string> { param.EmployeeATID }, DateTime.Now, param.CompanyIndex);
            }

            mInOutMode = inOutByLine;
            mLineIndex = lineIndex;

            if (lstEmployee == null || lstEmployee.Count == 0)
            {
               
                await PushUnknowErrorToClient(mLineIndex, "EmployeeInfoNotRegistered", _Config.CompanyIndex);


                return false;
            }

            mEmpInfo = lstEmployee.FirstOrDefault();

            if(mEmpInfo.Avatar == null && param.EmployeeType != (int)EmployeeType.Driver && param.EmployeeType != (int)EmployeeType.Driver)
            {
                mEmpInfo.Avatar = await _IC_UserMasterService.GetFaceByEmployeeATID(mEmpInfo.EmployeeATID);
            }
            try
            {
                var general_rule = _GC_Rules_GeneralService.Where(x => x.CompanyIndex == param.CompanyIndex && x.IsUsing).FirstOrDefault();
                if (general_rule != null && general_rule.IsBypassRule.HasValue && general_rule.IsBypassRule.Value)
                {
                    mResult = CheckByPassRules(param, inOutByLine, now);
                }
                else
                {
                    mResult = CheckRules(param, inOutByLine, now, _Config.CompanyIndex);
                }

                var log = CreateTimeLogObject(param, now, _Config.CompanyIndex);

                var listImages = GetImages(inOutByLine, user);

                // Get other info from camera
                var listInfos = GetInfos(inOutByLine);

                if (mResult.GetSuccess() == true && general_rule.RunWithoutScreen == true)
                {
                    bool openControllerSuccess = await OpenController(lineIndex, inOutByLine, true, true, user.CompanyIndex);

                    if (openControllerSuccess == true)
                    {
                        log.InOutMode = inOutByLine;
                        log.ApproveStatus = (short)ApproveStatus.Approved;
                        log.UpdatedDate = DateTime.Now;
                        log.UpdatedUser = "SYSTEM_AUTO";
                        await _GC_TimeLogService.SaveChangeAsync();
                    }
                }
                // Push data result to realtime screen
                PushDataToClient(param, log, now, _Config.RealTimeServerLink, listImages, listInfos);
                if (general_rule != null && general_rule.IsBypassRule.HasValue && general_rule.IsBypassRule.Value)
                {
                    log.ApproveStatus = (short)ApproveStatus.Approved;
                    await SendLogToClientAsync(log);
                }

                await _GC_TimeLogService.AddTimeLog(log);

                await PushDataToClientCustomer(param, log, _Config.CompanyIndex);

                // Update process data base on process type
                UpdateLocalData(log.Index, param, now);

                // save image to database
                await CreateTimeLog_ImageObject(log.Index, listImages, listInfos, now);
            }
            catch (Exception ex)
            {
                await PushUnknowErrorToClient(mLineIndex, ex.Message, _Config.CompanyIndex);
                _logger.LogError($"MainProcess: {ex}");
                return false;
            }

            return true;
        }

        internal abstract CheckRuleResult CheckByPassRules(AttendanceLogRealTime param, short inOutByLine, DateTime now);
        internal abstract CheckRuleResult CheckRules(AttendanceLogRealTime param, short inOutByLine, DateTime now, int companyIndex);
        internal abstract GC_TimeLog CreateTimeLogObject(AttendanceLogRealTime param, DateTime now, int companyIndex);
        internal abstract List<string> GetImages(short pInOut, UserInfo user);
        internal abstract List<string> GetInfos(short pInOut);
        internal abstract bool PushDataToClient(AttendanceLogRealTime param, GC_TimeLog log, DateTime now, string link,
            List<string> verifyImages, List<string> verifyInfos);
        internal abstract void UpdateLocalData(long timeLogIndex, AttendanceLogRealTime param, DateTime now);

        protected async Task SendLogToClientAsync(GC_TimeLog log)
        {

            var preLog = _GC_TimeLogService.Where(e => e.EmployeeATID == log.EmployeeATID && e.Time != log.Time
                    && e.ApproveStatus == (short)ApproveStatus.Approved && e.CustomerIndex == log.CustomerIndex)
                    .OrderByDescending(e => e.Time)
                    .FirstOrDefault();
            if (preLog == null || (preLog != null && log.InOutMode.HasValue && preLog.InOutMode.HasValue
                && (log.InOutMode != preLog.InOutMode
                || (log.InOutMode == preLog.InOutMode && log.MachineSerial != preLog.MachineSerial))))
            {
                var data = new Dictionary<bool, GC_TimeLog>();
                data.Add(log.InOutMode == 1, log);

                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                client.BaseAddress = new Uri(_Config.RealTimeServerLink);
                var json = JsonConvert.SerializeObject(data.FirstOrDefault());
                try
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var request = await client.PostAsync("/api/PushAttendanceLog/SendData", content);
                    request.EnsureSuccessStatusCode();

                }
                catch (Exception ex)
                {
                    _logger.LogError($"SendData: {ex}");
                }
            }
        }

        internal EMonitoringError GetEMonitoringErrorByName(string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                return EMonitoringError.NoError;
            }
            return (EMonitoringError)Enum.Parse(typeof(EMonitoringError), error);
        }

        protected async Task SendWalkerData(WalkerInfo info)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            client.BaseAddress = new Uri(_Config.RealTimeServerLink);
            var json = JsonConvert.SerializeObject(info);
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = await client.PostAsync("/api/PushAttendanceLog/SendWalkerData", content);
                request.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                _logger.LogError($"SendData: {ex}");
            }
        }

        private async Task<bool> OpenController(int lineIndex, int inOutMode, bool autoOff, bool setOn, int companyIndex)
        {
            string error = "";
            if (lineIndex == 0) return false;

            if (inOutMode == 1)
            {
                var listCheckInController = _GC_Lines_CheckInRelayControllerService
                        .Where(t => t.CompanyIndex == companyIndex && t.LineIndex == lineIndex).ToList();

                var controllerList = _IIC_RelayControllerService.Where(t => listCheckInController.Select(x => x.RelayControllerIndex).Contains(t.Index)).ToList();

                var controllerChannelList = _IC_RelayControllerChannelService
                    .Where(t => listCheckInController.Select(x => x.RelayControllerIndex).Contains(t.RelayControllerIndex)).ToList();
                if (controllerChannelList.Count == 0)
                {
                    error = "ControllerNotFound";
                }
                for (int i = 0; i < listCheckInController.Count; i++)
                {
                    var channelIndex = setOn ? listCheckInController[i].OpenDoorChannelIndex : listCheckInController[i].FailAlarmChannelIndex;//Mở
                    var listChannel = new List<int>() { channelIndex };
                    var controller = controllerList.FirstOrDefault(x => x.Index == listCheckInController[i].RelayControllerIndex);
                    var controllerChannel = controllerChannelList.Where(x => x.RelayControllerIndex == listCheckInController[i].RelayControllerIndex).ToList();
                    double secondAutoClose = controllerChannel?.FirstOrDefault()?.NumberOfSecondsOff ?? 4;
                    if (controller.RelayType == RelayType.ModbusTCP.ToString())
                    {
                        if (await _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(controller.IpAddress, Convert.ToUInt16(controller.Port)))
                        {
                            var listChannelOp = new List<ChannelParam>();
                            listChannelOp.Add(new ChannelParam() { Index = Convert.ToInt16(channelIndex), ChannelStatus = setOn });

                            var result = await _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannelOp, secondAutoClose);
                            _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                        }
                        else
                        {
                            error = "ControllerConnectFail";
                        }

                        if (autoOff)
                        {
                            if (await _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(controller.IpAddress, Convert.ToUInt16(controller.Port)))
                            {
                                var listChannelOp = new List<ChannelParam>();
                                listChannelOp.Add(new ChannelParam() { Index = Convert.ToInt16(listCheckInController[i].FailAlarmChannelIndex) });

                                if (controllerChannel != null)
                                {
                                    var result = await _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannelOp, secondAutoClose);
                                    _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                                }
                            }
                            else
                            {
                                error = "ControllerConnectFail";
                            }
                        }
                    }
                    else
                    {
                        error = await _IIC_ClientTCPControllerLogic.SetOnAndAutoOffController(controller.IpAddress, controller.Port, listChannel, secondAutoClose);
                        if (autoOff)
                        {
                            error = await _IIC_ClientTCPControllerLogic.SetOnAndAutoOffController(controller.IpAddress, controller.Port, new List<int>() { listCheckInController[i].FailAlarmChannelIndex }, secondAutoClose);//Đóng lệnh
                        }
                        if (error != "") break;
                    }
                }
            }
            else
            {
                var listCheckInController = _GC_Lines_CheckOutRelayControllerService
                          .Where(t => t.CompanyIndex == companyIndex && t.LineIndex == lineIndex).ToList();

                var controllerList = _IIC_RelayControllerService.Where(t => listCheckInController.Select(x => x.RelayControllerIndex).Contains(t.Index)).ToList();

                var controllerChannelList = _IC_RelayControllerChannelService
                    .Where(t => listCheckInController.Select(x => x.RelayControllerIndex).Contains(t.RelayControllerIndex)).ToList();
                if (controllerChannelList.Count == 0)
                {
                    error = "ControllerNotFound";
                }
                for (int i = 0; i < listCheckInController.Count; i++)
                {
                    var channelIndex = setOn ? listCheckInController[i].OpenDoorChannelIndex : listCheckInController[i].FailAlarmChannelIndex;//Mở
                    var listChannel = new List<int>() { channelIndex };
                    var controller = controllerList.FirstOrDefault(x => x.Index == listCheckInController[i].RelayControllerIndex);
                    var controllerChannel = controllerChannelList.Where(x => x.RelayControllerIndex == listCheckInController[i].RelayControllerIndex).ToList();

                    double secondAutoClose = controllerChannel?.FirstOrDefault()?.NumberOfSecondsOff ?? 4;
                    //var listChannelIndex = listCheckInController.Select(x => (int)x.FailAlarmChannelIndex).Distinct().ToList();
                    //openControllerSuccess = 

                    if (controller.RelayType == RelayType.ModbusTCP.ToString())
                    {
                        if (await _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(controller.IpAddress, Convert.ToUInt16(controller.Port)))
                        {
                            var listChannelOp = new List<ChannelParam>();
                            listChannelOp.Add(new ChannelParam() { Index = Convert.ToInt16(channelIndex), ChannelStatus = setOn });

                            var result = await _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannelOp, secondAutoClose);
                            _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                        }
                        else
                        {
                            error = "ControllerConnectFail";
                        }

                        if (autoOff)
                        {
                            if (await _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(controller.IpAddress, Convert.ToUInt16(controller.Port)))
                            {
                                var listChannelOp = new List<ChannelParam>();
                                listChannelOp.Add(new ChannelParam() { Index = Convert.ToInt16(listCheckInController[i].FailAlarmChannelIndex) });

                                if (controllerChannel != null)
                                {
                                    var result = await _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannelOp, secondAutoClose);
                                    _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                                }
                            }
                            else
                            {
                                error = "ControllerConnectFail";
                            }
                        }
                    }
                    else
                    {
                        error = await _IIC_ClientTCPControllerLogic.SetOnAndAutoOffController(controller.IpAddress, controller.Port, listChannel, secondAutoClose);

                        //_EPADClient.SetOnAndAutoOffController(listCheckInController[i].RelayControllerIndex, listChannel, false, setOn).Result;
                        if (autoOff)
                        {
                            error = await _IIC_ClientTCPControllerLogic.SetOnAndAutoOffController(controller.IpAddress, controller.Port, new List<int>() { listCheckInController[i].FailAlarmChannelIndex }, secondAutoClose);//Đóng lệnh
                        }
                        if (error != "") break;
                    }
                       
                }
            }
            if (error != "")
            {
                return false;
            }
            return true;
        }

        public async Task CreateTimeLog_ImageObject(long pTimeLogIndex, List<string> pListImageLink, List<string> pListInfo, DateTime pNow)
        {
            if (pListImageLink == null)
            {
                pListImageLink = new List<string>();
            }
            if (pListInfo == null)
            {
                pListInfo = new List<string>();
            }

            var data = new GC_TimeLog_Image();
            data.TimeLogIndex = pTimeLogIndex;
            data.Image1 = GetDataInList(0, pListImageLink);
            data.Image2 = GetDataInList(1, pListImageLink);
            data.Image3 = GetDataInList(2, pListImageLink);
            data.Image4 = GetDataInList(3, pListImageLink);
            data.Image5 = GetDataInList(4, pListImageLink);

            data.Info1 = GetDataInList(0, pListInfo);
            data.Info2 = GetDataInList(1, pListInfo);
            data.Info3 = GetDataInList(2, pListInfo);
            data.Info4 = GetDataInList(3, pListInfo);
            data.Info5 = GetDataInList(4, pListInfo);

            data.CompanyIndex = _Config.CompanyIndex;
            data.UpdatedUser = "MonitoringAuto";
            data.UpdatedDate = pNow;

            await _GC_TimeLog_ImageService.AddTimeLogImage(data);
        }

        protected ObjectAccessType CheckObjectAccess()
        {
            if (mEmpInfo.Department == "Visitor")
            {
                return ObjectAccessType.Customer;
            }
            else
            {
                return ObjectAccessType.Employee;
            }
        }

        private string GetDataInList(int pIndex, List<string> pList)
        {
            string value = "";
            if (pList.Count > pIndex)
            {
                value = pList[pIndex];
            }
            if (value == null)
            {
                value = "";
            }
            return value;
        }

        protected CheckRuleResult CheckRuleForCustomer(string empATID, DateTime checkTime, int companyIndex)
        {

            var result = new CheckRuleResult();
            // lấy thông tin khách
            var listCustomer = _GC_CustomerService.Where(t => t.EmployeeATID == empATID
                 && t.CompanyIndex == companyIndex).ToList();
            // thẻ khách chưa đk
            if (listCustomer.Count != 0)
            {
                var customer = listCustomer.Where(t => t.FromTime <= checkTime && t.ToTime >= checkTime).FirstOrDefault();
                if (customer == null)
                {
                    customer = listCustomer.Where(t => t.FromTime <= checkTime && t.ExtensionTime != null && t.ExtensionTime.Value >= checkTime).FirstOrDefault();
                    if (customer != null)
                    {
                        result.MoreInfo = "InExtensionTime";
                    }
                    else
                    {
                        mCustomer = listCustomer[0];
                        mArrViolation.Add((int)EMonitoringError.OutOfWorkingTime);
                    }
                }
                else
                {
                    mCustomer = customer;
                }

                if (customer != null)
                {
                    // lấy quy định
                    var rule = _GC_Rules_CustomerService.FirstOrDefault(e => e.Index == customer.RulesCustomerIndex && e.CompanyIndex == customer.CompanyIndex);
                    if (rule != null)
                    {
                        // lấy ds khu vực dc truy cập
                        var listGates = _GC_Rules_Customer_GatesService.Where(t => t.CompanyIndex == companyIndex
                            && t.RulesCustomerIndex == rule.Index).ToList();

                        bool allow = false;
                        for (int i = 0; i < listGates.Count; i++)
                        {
                            string[] arrLines = listGates[i].LineIndexs.Split(',');
                            foreach (string item in arrLines)
                            {
                                if (item != "" && item == mLineIndex.ToString())
                                {
                                    allow = true;
                                    break;
                                }
                            }
                        }

                        if (allow == false)
                        {
                            mArrViolation.Add((int)EMonitoringError.EmployeeNotInAccessGroup);
                        }
                        // check number of access
                        var listLogs = _GC_TimeLogService.Where(t => t.CompanyIndex == companyIndex /*&& t.LogType == LogType.Walker.ToString()*/
                            && t.EmployeeATID == empATID && t.ApproveStatus == 1).ToList();
                        if (listLogs.Count >= rule.NumberOfConnect)
                        {
                            mArrViolation.Add((int)EMonitoringError.ExceedNumberOfAccess);
                        }
                    }
                    else
                    {
                        mArrViolation.Add((int)EMonitoringError.NotFoundCustomerRule);
                    }
                }
            }
            else
            {
                mArrViolation.Add((int)EMonitoringError.CardNotRegister);
            }

            result.SetStatus(GetpriorityViolation(mArrViolation)); // Lấy vi phạm có ưu tiên cao nhất
            return result;
        }

        private async Task<bool> PushDataToClientCustomer(AttendanceLogRealTime param, GC_TimeLog log, int companyIndex)
        {
            try
            {
                var info = new WalkerInfo();
                info.ObjectType = mAccessType.ToString();
                if (param.EmployeeType.HasValue)
                {
                    info.ObjectType = ((EmployeeType)param.EmployeeType.Value).ToString();
                    if (mEmpInfo.IsExtraDriver && param.EmployeeType.HasValue && param.EmployeeType.Value == (short)EmployeeType.Driver)
                    {
                        info.ObjectType = "Extra" + ((EmployeeType)param.EmployeeType.Value).ToString();
                    }
                }
                info.SetBasicInfo(param.EmployeeATID, mInOutMode, mResult.GetSuccess(), mResult.GetError(),
                    param.CheckTime, companyIndex, log.Index, mLineIndex, param.Avatar);

                if (mAccessType == ObjectAccessType.Employee)
                {
                    info.ListInfo.Add(new InfoDetail("EmployeeATID", mEmpInfo.EmployeeATID));
                    info.ListInfo.Add(new InfoDetail("FullName", mEmpInfo.FullName));
                    //info.ListInfo.Add(new InfoDetail("EmployeeCode", mEmpInfo.EmployeeCode));
                    info.ListInfo.Add(new InfoDetail("Department", mEmpInfo.Department));
                    //info.ListInfo.Add(new InfoDetail("Position", mEmpInfo.Position));
                    if (mEmpInfo.Avatar != null)
                    {
                        info.RegisterImage = Convert.ToBase64String(mEmpInfo.Avatar);
                    }


                    var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(3);
                    client.BaseAddress = new Uri(_Config.RealTimeServerLink);
                    var json = JsonConvert.SerializeObject(info);
                    try
                    {
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var request = await client.PostAsync("/api/Monitoring/SendCustomerWalkerLog", content);
                        request.EnsureSuccessStatusCode();

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"SendCustomerWalkerLog: {ex}");
                    }
                }
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError($"PushDataToClient: {ex}");
                return false;
            }
        }

        internal EMonitoringError GetpriorityViolation(List<int> mArrViolation)
        {
            if (mArrViolation.Count > 0)
            {
                var priorityViolation = mArrViolation.OrderBy(index => index).FirstOrDefault();
                return (EMonitoringError)priorityViolation;
            }
            return EMonitoringError.NoError;
        }

        internal EMonitoringError GetpriorityViolation(EMonitoringError eMonitoringError, EMonitoringError eMonitoringErrorOld)
        {
            if ((int)eMonitoringError > (int)eMonitoringErrorOld)
            {
                return eMonitoringErrorOld;
            }
            else
            {
                return eMonitoringError;
            }
        }


        internal async Task PushProgressInfo(int lineIndex, string classProcess, int companyIndex)
        {
            ProgressData progressInfo = new ProgressData();
            progressInfo.CompanyIndex = companyIndex;
            progressInfo.LineIndex = lineIndex;
            progressInfo.Form = classProcess;
            progressInfo.Data = "";

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            client.BaseAddress = new Uri(_Config.RealTimeServerLink);
            var json = JsonConvert.SerializeObject(progressInfo);
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = await client.PostAsync("/api/PushAttendanceLog/SendProgressData", content);
                request.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                _logger.LogError($"SendProgressData: {ex}");
            }
        }

        protected void GetDeviceGroups(int companyIndex)
        {
            mGroupDevices = _IC_GroupDeviceService.GetGroupDevice(companyIndex);
            if (mGroupDevices == null)
            {
                mGroupDevices = new List<GroupDeviceParam>();
            }
        }

        protected List<string> GetLobbyMachines()
        {
            var lobby = mGroupDevices.FirstOrDefault(e => e.Name.ToLower().Contains("sảnh") || e.Description.ToLower().Contains("sảnh"));
            if (lobby != null)
            {
                return lobby.ListMachine;
            }
            return null;
        }

        protected CheckRuleResult CheckRuleForEmployee(EmployeeAccessRule ruleData, string empATID, DateTime checkTime, int companyIndex, LogType logType)
        {
            var result = new CheckRuleResult();
            //var ruleData = _GC_Employee_AccessedGroupService.GetInfoByEmpATIDAndFromDate(empATID, checkTime, companyIndex);
            if (ruleData != null)
            {
                var leaveDay = new LeaveDayBasicInfoReponse();
                if (ruleData.AllowInLeaveDay)
                {
                    // ko cho phép vào nv nghỉ phép cả ngày
                    leaveDay = _EzhrClient.GetListEmployeeLeaveDay(empATID, checkTime).Result;
                    if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                    {
                        var check = leaveDay.Data.FirstOrDefault(x => x.Type == "WholeDay");
                        if (check != null)
                        {
                            if (mInOutMode == 1)
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeInLeaveTime);
                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeOutLeaveTime);
                            }

                        }

                        var checkFirstAndLastHalf = leaveDay.Data.Where(x => x.Type == "FirstHaft" || x.Type == "LastHaft");
                        if (checkFirstAndLastHalf != null && checkFirstAndLastHalf.Count() > 1)
                        {
                            if (mInOutMode == 1)
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeInLeaveTime);
                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeOutLeaveTime);
                            }
                        }
                    }
                }


                var missedLst = new MissionApprovedResultReponse();
                if (ruleData.AllowInMission)
                {
                    // ko cho phép vào nv đăng ký công tác cả ngày
                    missedLst = _EzhrClient.GetListMissionApproved(empATID, checkTime).Result;
                    if (missedLst != null && missedLst.Data != null && missedLst.Data.Count > 0)
                    {
                        var check = missedLst.Data.FirstOrDefault(x => x.From == null && x.To == null);
                        if (check != null)
                        {
                            if (mInOutMode == 1)
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeInMissionTime);
                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeOutMissionTime);
                            }

                        }

                    }
                }
                var verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                if (mInOutMode == 1)
                {
                    #region kiểm tra các quy định cấm vào
                    // kiểm tra ra vào trong giờ làm việc
                    var logOut = GetLogOutByShift(ruleData, empATID, approveStatus: 1, inOutMode: 2, checkTime, logType, companyIndex, isOrder: true);

                    // log vào đầu tiên
                    if (logOut == null)
                    {
                        int earlyMinutes = ruleData.MaxEarlyCheckInMinute;
                        int lateMinutes = ruleData.MaxLateCheckInMinute;
                        int earlyOutMinutes = ruleData.MaxEarlyCheckOutMinute;
                        int lateOutMinutes = ruleData.MaxLateCheckOutMinute;
                        var missionEarlyOutMinutes = 0;
                        var missionLateInMinutes = 0;
                        if (ruleData.AllowEarlyOutLateInMission)
                        {
                            missionEarlyOutMinutes = ruleData.MissionMaxEarlyCheckOutMinute;
                            missionLateInMinutes = ruleData.MissionMaxLateCheckInMinute;
                        }
                        var ruleTime = DateTime.MinValue;
                        var ruleTimeOut = DateTime.MinValue;


                        if (ruleData.AdjustByLateInEarlyOut == true)
                        {
                            var lateInEarlyOut = _EzhrClient.LateInEarlyOutApprovedResult(empATID, checkTime).Result;
                            if (lateInEarlyOut != null && lateInEarlyOut.Data != null && lateInEarlyOut.Data.Count > 0)
                            {
                                // add early out
                                earlyMinutes += lateInEarlyOut.Data[0].EarlyIn;
                                // add late out
                                lateMinutes += lateInEarlyOut.Data[0].LateIn;
                            }
                        }
                        if (ruleData.CheckInByShift == false)
                        {
                            if (ruleData.CheckInTime != null)
                            {
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, ruleData.CheckInTime.Value.Hour, ruleData.CheckInTime.Value.Minute, 0);
                                ruleTimeOut = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, ruleData.CheckOutTime.Value.Hour, ruleData.CheckOutTime.Value.Minute, 0);
                                verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                            }
                        }
                        else
                        {
                            var workingHours = _EzhrClient.CheckMaximumWorkingHoursAndOT(empATID, checkTime).Result;

                            if (workingHours != null && workingHours.Data != null && workingHours.Data.ShiftsInfo != null && workingHours.Data.ShiftsInfo.Count > 0)
                            {
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].StartTime.Hour, workingHours.Data.ShiftsInfo[0].StartTime.Minute, 0);
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].EndTime.Value.Hour, workingHours.Data.ShiftsInfo[0].EndTime.Value.Minute, 0);
                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeNotAssignSchedule);
                            }
                        }
                        if (ruleTime != DateTime.MinValue)
                        {
                            var isFirstHaftLeave = false;
                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var halfFirst = leaveDay.Data.Where(x => x.Type == "FirstHaft").ToList();
                                if (halfFirst != null && halfFirst.Count > 0)
                                {
                                    var beginLastHalfTime = ruleData.BeginLastHaftTime.Value;
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                    isFirstHaftLeave = true;
                                }
                            }

                            var endRuleTime = (DateTime)SqlDateTime.MinValue; ;
                            var isLastHaftLeave = false;
                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var halfLast = leaveDay.Data.Where(x => x.Type == "LastHaft").ToList();
                                if (halfLast != null && halfLast.Count > 0)
                                {
                                    var beginLastHalfTime = ruleData.EndFirstHaftTime.Value;
                                    endRuleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                    isLastHaftLeave = true;
                                }
                            }


                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var checkWithTime = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime <= ruleTime && x.ToTime >= ruleTime);
                                if (checkWithTime != null)
                                {
                                    var beginLastHalfTime = checkWithTime.ToTime;
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                }
                            }
                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var check = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime <= checkTime && x.ToTime.AddMinutes(-earlyMinutes) >= checkTime);
                                if (check != null)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeInLeaveTime);
                                }
                            }
                            //if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                            //{
                            //    var check = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null 
                            //        && x.From <= checkTime && x.To.Value.AddMinutes(-earlyMinutes) >= checkTime);
                            //    if (check != null)
                            //    {
                            //        mArrViolation.Add((int)EMonitoringError.EmployeeInMissionTime);
                            //    }
                            //}

                            var isInMissionTime = false;
                            if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                            {
                                var check = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null
                                    && x.From.Value.AddMinutes(-missionEarlyOutMinutes) <= checkTime
                                    && x.To.Value.AddMinutes(missionLateInMinutes) >= checkTime);
                                if (check != null)
                                {
                                    isInMissionTime = true;
                                }
                            }

                            //if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                            //{
                            //    var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null 
                            //        && x.From <= ruleTime && x.To >= ruleTime);
                            //    if (checkWithTime != null)
                            //    {
                            //        var beginLastHalfTime = checkWithTime.To.Value;
                            //        ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                            //    }
                            //}

                            if (!isInMissionTime)
                            {
                                if (verifyTime < ruleTime.AddMinutes(-earlyMinutes)
                                    && !isFirstHaftLeave)
                                {
                                    mArrViolation.Add((int)EMonitoringError.CheckInNotYet);
                                }
                                if (verifyTime < ruleTime.AddMinutes(-earlyMinutes) && isFirstHaftLeave)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeRegisteredFirstHaftLeave);
                                }
                                if ((!ruleData.AllowFreeInAndOutInTimeRange && verifyTime > ruleTime.AddMinutes(lateMinutes))
                                    || (ruleData.AllowFreeInAndOutInTimeRange && verifyTime > ruleTimeOut.AddMinutes(lateOutMinutes)))
                                {
                                    mArrViolation.Add((int)EMonitoringError.ExceedCheckInTime);
                                }
                                if (verifyTime > endRuleTime.AddMinutes(lateMinutes) && isLastHaftLeave)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeRegisteredLastHaftLeave);
                                }
                            }
                        }
                    }
                    else
                    {
                        int earlyMinutes = ruleData.MaxEarlyCheckInMinute;
                        int lateMinutes = ruleData.MaxLateCheckInMinute;
                        int earlyOutMinutes = ruleData.MaxEarlyCheckOutMinute;
                        int lateOutMinutes = ruleData.MaxLateCheckOutMinute;
                        var missionEarlyOutMinutes = 0;
                        var missionLateInMinutes = 0;
                        if (ruleData.AllowEarlyOutLateInMission)
                        {
                            missionEarlyOutMinutes = ruleData.MissionMaxEarlyCheckOutMinute;
                            missionLateInMinutes = ruleData.MissionMaxLateCheckInMinute;
                        }
                        var ruleTime = DateTime.MinValue;
                        var ruleTimeOut = DateTime.MinValue;
                        var checkValid = false;
                        var ruleError = DateTime.MinValue;

                        if (ruleData.AdjustByLateInEarlyOut == true)
                        {
                            var lateInEarlyOut = _EzhrClient.LateInEarlyOutApprovedResult(empATID, checkTime).Result;
                            if (lateInEarlyOut != null && lateInEarlyOut.Data != null && lateInEarlyOut.Data.Count > 0)
                            {
                                // add early out
                                earlyMinutes += lateInEarlyOut.Data[0].EarlyIn;
                                // add late out
                                lateMinutes += lateInEarlyOut.Data[0].LateIn;
                            }
                        }
                        if (ruleData.CheckInByShift == false)
                        {
                            if (ruleData.CheckInTime != null)
                            {
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, ruleData.CheckInTime.Value.Hour, ruleData.CheckInTime.Value.Minute, 0);
                                ruleTimeOut = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, ruleData.CheckOutTime.Value.Hour, ruleData.CheckOutTime.Value.Minute, 0);
                                verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                            }
                            if (ruleData.CheckOutTime != null)
                            {
                                ruleError = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, ruleData.CheckOutTime.Value.Hour, ruleData.CheckOutTime.Value.Minute, 0);

                            }
                        }
                        else
                        {
                            var workingHours = _EzhrClient.CheckMaximumWorkingHoursAndOT(empATID, checkTime).Result;

                            if (workingHours != null && workingHours.Data != null && workingHours.Data.ShiftsInfo != null && workingHours.Data.ShiftsInfo.Count > 0)
                            {
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].StartTime.Hour, workingHours.Data.ShiftsInfo[0].StartTime.Minute, 0);
                                ruleTimeOut = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].EndTime.Value.Hour, workingHours.Data.ShiftsInfo[0].EndTime.Value.Minute, 0);
                                ruleError = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].EndTime.Value.Hour, workingHours.Data.ShiftsInfo[0].EndTime.Value.Minute, 0);
                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeNotAssignSchedule);
                            }
                        }

                        var isFirstHaftLeave = false;
                        if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                        {
                            var halfFirst = leaveDay.Data.Where(x => x.Type == "FirstHaft").ToList();
                            if (halfFirst != null && halfFirst.Count > 0)
                            {
                                var beginLastHalfTime = ruleData.BeginLastHaftTime.Value;
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                isFirstHaftLeave = true;
                            }
                        }

                        var isLastHaftLeave = false;
                        if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                        {
                            var halfFirst = leaveDay.Data.Where(x => x.Type == "LastHaft").ToList();
                            if (halfFirst != null && halfFirst.Count > 0)
                            {
                                var beginLastHalfTime = ruleData.EndFirstHaftTime.Value;
                                ruleError = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                isLastHaftLeave = true;
                            }
                        }

                        if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                        {
                            var checkWithTime = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime <= checkTime && x.ToTime.AddMinutes(-earlyMinutes) >= checkTime);
                            if (checkWithTime != null)
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeInLeaveTime);
                                checkValid = true;
                            }
                        }

                        //if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                        //{
                        //    var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null
                        //        && x.From <= checkTime && x.To.Value.AddMinutes(-earlyMinutes) >= checkTime);
                        //    if (checkWithTime != null)
                        //    {
                        //        mArrViolation.Add((int)EMonitoringError.EmployeeInMissionTime);
                        //        checkValid = true;
                        //    }
                        //}

                        var isIsMissionTime = false;
                        if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                        {
                            var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null
                                && x.From.Value.AddMinutes(-missionEarlyOutMinutes) <= checkTime
                                && x.To.Value.AddMinutes(missionLateInMinutes) >= checkTime);
                            if (checkWithTime != null)
                            {
                                isIsMissionTime = true;
                            }
                        }

                        if (logOut.LeaveType != 0 && !checkValid)
                        {
                            if (logOut.LeaveType == (int)LeaveType.Leaveday)
                            {
                                var checkWithTime = leaveDay.Data.Where(x => x.Type == "WithTime" && checkTime >= x.ToTime.AddMinutes(-earlyMinutes)).OrderByDescending(x => x.ToTime).FirstOrDefault();
                                if (checkWithTime != null)
                                {
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkWithTime.ToTime.Hour, checkWithTime.ToTime.Minute, 0);
                                }
                            }
                            //else if (logOut.LeaveType == (int)LeaveType.Mission)
                            //{
                            //    var checkWithTime = missedLst.Data.Where(x => x.From != null && x.To != null
                            //        && x.To.Value.AddMinutes(-earlyMinutes) <= checkTime).OrderByDescending(x => x.To).FirstOrDefault();
                            //    if (checkWithTime != null)
                            //    {
                            //        ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkWithTime.To.Value.Hour, checkWithTime.To.Value.Minute, 0);
                            //    }
                            //}
                            if (!isIsMissionTime)
                            {
                                //if (verifyTime < ruleTime.AddMinutes(-earlyMinutes))
                                //{
                                //    if (isFirstHaftLeave)
                                //    {
                                //        mArrViolation.Add((int)EMonitoringError.EmployeeRegisteredFirstHaftLeave);
                                //    }
                                //    else
                                //    {
                                //        mArrViolation.Add((int)EMonitoringError.CheckInNotYet);
                                //    }
                                //}
                                //if (verifyTime > ruleTime.AddMinutes(lateMinutes))
                                //{
                                //    if (isLastHaftLeave)
                                //    {
                                //        mArrViolation.Add((int)EMonitoringError.EmployeeRegisteredLastHaftLeave);
                                //    }
                                //    else
                                //    {
                                //        mArrViolation.Add((int)EMonitoringError.ExceedCheckInTime);
                                //    }
                                //}
                                if (verifyTime < ruleTime.AddMinutes(-earlyMinutes) && !isFirstHaftLeave)
                                {
                                    mArrViolation.Add((int)EMonitoringError.CheckInNotYet);
                                }
                                if (verifyTime < ruleTime.AddMinutes(-earlyMinutes) && isFirstHaftLeave)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeRegisteredFirstHaftLeave);
                                }
                                if ((!ruleData.AllowFreeInAndOutInTimeRange && verifyTime > ruleTime.AddMinutes(lateMinutes))
                                    || (ruleData.AllowFreeInAndOutInTimeRange && verifyTime > ruleTimeOut.AddMinutes(lateOutMinutes)))
                                {
                                    mArrViolation.Add((int)EMonitoringError.ExceedCheckInTime);
                                }
                                if (verifyTime > ruleError.AddMinutes(lateMinutes) && isLastHaftLeave)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeRegisteredLastHaftLeave);
                                }
                            }

                        }
                        else if (!checkValid)
                        {
                            if (ruleData.AllowCheckOutInWorkingTime && checkTime.Date == logOut.Time.Date && !ruleData.AllowFreeInAndOutInTimeRange)
                            {
                                //if (checkTime.Subtract(logOut.Time).TotalMinutes > ruleData.MaxMinuteAllowOutsideInWorkingTime)
                                //{
                                //    mArrViolation.Add((int)EMonitoringError.ExceedMaxMinuteAllowOutsideInWorkingTime);
                                //}

                                var isExceedCheckInTime = true;
                                var isExceedMaxAllowInOut = true;

                                if (!string.IsNullOrWhiteSpace(ruleData.AllowCheckOutInWorkingTimeRange))
                                {
                                    var allowCheckOutInWorkingTimeRange = JsonConvert.DeserializeObject<List<AllowCheckOutInWorkingTimeRangeObject>>
                                        (ruleData.AllowCheckOutInWorkingTimeRange);

                                    for (var i = 0; i < allowCheckOutInWorkingTimeRange.Count; i++)
                                    {
                                        allowCheckOutInWorkingTimeRange[i].FromTime
                                            = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day,
                                            allowCheckOutInWorkingTimeRange[i].FromTime.Hour, allowCheckOutInWorkingTimeRange[i].FromTime.Minute, 0);
                                        allowCheckOutInWorkingTimeRange[i].ToTime
                                            = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day,
                                            allowCheckOutInWorkingTimeRange[i].ToTime.Hour, allowCheckOutInWorkingTimeRange[i].ToTime.Minute, 0);
                                        allowCheckOutInWorkingTimeRange[i].Index = i + 1;
                                    }

                                    var isValidIn = false;
                                    var isValidInWorkingTime = false;
                                    foreach (var timeRange in allowCheckOutInWorkingTimeRange)
                                    {
                                        var startTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, timeRange.FromTime.Hour, timeRange.FromTime.Minute, 0);
                                        var endTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, timeRange.ToTime.Hour, timeRange.ToTime.Minute, 0);

                                        if (allowCheckOutInWorkingTimeRange.Any(y => y.Index != timeRange.Index
                                            && logOut.Time >= y.FromTime && logOut.Time <= y.ToTime))
                                        {
                                            if ((checkTime < startTime || checkTime > endTime) && checkTime <= ruleTimeOut.AddMinutes(lateOutMinutes))
                                            {
                                                //isValidInWorkingTime = false;
                                                //break;
                                            }
                                            else
                                            {
                                                //isValidInWorkingTime = true;
                                            }

                                            if (checkTime > ruleTimeOut.AddMinutes(lateOutMinutes))
                                            {
                                                //isValidIn = false;
                                                //break;
                                            }
                                            else
                                            {
                                                //isValidIn = true;
                                            }

                                            if (checkTime >= ruleTime.AddMinutes(-earlyMinutes) && checkTime <= ruleTime.AddMinutes(lateMinutes))
                                            {
                                                isValidIn = true;
                                            }
                                        }
                                        else if (logOut.Time >= startTime && logOut.Time <= endTime)
                                        {
                                            if ((checkTime < startTime || checkTime > endTime) && checkTime <= ruleTimeOut.AddMinutes(lateOutMinutes))
                                            {
                                                //isValidInWorkingTime = false;
                                                //break;
                                            }
                                            else
                                            {
                                                isValidInWorkingTime = true;
                                            }

                                            if (((checkTime < startTime || checkTime > endTime) && checkTime > ruleTimeOut.AddMinutes(lateOutMinutes))
                                                || (checkTime > startTime && checkTime < endTime && checkTime > ruleTimeOut.AddMinutes(lateOutMinutes)))
                                            {
                                                //isValidIn = false;
                                                //break;
                                            }
                                            else
                                            {
                                                isValidIn = true;
                                            }
                                        }
                                        else
                                        {
                                            if ((checkTime < startTime || checkTime > endTime) && checkTime <= ruleTimeOut.AddMinutes(lateOutMinutes))
                                            {
                                                //isValidIn = false;
                                                //break;
                                            }
                                            else
                                            {
                                                isValidInWorkingTime = true;
                                            }

                                            if (((checkTime < startTime || checkTime > endTime) && checkTime > ruleTimeOut.AddMinutes(lateOutMinutes))
                                                || (checkTime > startTime && checkTime < endTime && checkTime > ruleTimeOut.AddMinutes(lateOutMinutes)))
                                            {
                                                //isValidIn = false;
                                                //break;
                                            }
                                            else
                                            {
                                                isValidIn = true;
                                            }
                                        }

                                        //if (logOut.Time.Date == checkTime.Date)
                                        //{
                                        //    if (logOut.Time >= startTime && logOut.Time <= endTime)
                                        //    {
                                        //        if (checkTime > endTime && checkTime <= ruleTimeOut.AddMinutes(lateOutMinutes))
                                        //        {
                                        //            mArrViolation.Add((int)EMonitoringError.ExceedMaxMinuteAllowOutsideInWorkingTime);
                                        //            break;
                                        //        }
                                        //        else if (checkTime > ruleTimeOut.AddMinutes(lateOutMinutes))
                                        //        {
                                        //            mArrViolation.Add((int)EMonitoringError.ExceedCheckInTime);
                                        //            break;
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        if ((checkTime < startTime || checkTime > endTime) && checkTime <= ruleTimeOut.AddMinutes(lateOutMinutes))
                                        //        {
                                        //            mArrViolation.Add((int)EMonitoringError.ExceedCheckInTime);
                                        //            break;
                                        //        }
                                        //        else if (checkTime > ruleTimeOut.AddMinutes(lateOutMinutes))
                                        //        {
                                        //            mArrViolation.Add((int)EMonitoringError.ExceedCheckInTime);
                                        //            break;
                                        //        }
                                        //    }
                                        //}
                                    }

                                    if (!isValidIn)
                                    {
                                        mArrViolation.Add((int)EMonitoringError.ExceedCheckInTime);
                                    }
                                    if (!isValidInWorkingTime)
                                    {
                                        mArrViolation.Add((int)EMonitoringError.ExceedMaxMinuteAllowOutsideInWorkingTime);
                                    }
                                }
                            }


                            if (!ruleData.AllowCheckOutInWorkingTime || (ruleData.AllowCheckOutInWorkingTime && checkTime.Date != logOut.Time.Date))
                            {
                                //if (verifyTime > ruleError.AddMinutes(lateMinutes))
                                //{
                                //    mArrViolation.Add((int)EMonitoringError.ExceedCheckInTime);
                                //}

                                if (verifyTime < ruleTime.AddMinutes(-earlyMinutes) && !isFirstHaftLeave)
                                {
                                    mArrViolation.Add((int)EMonitoringError.CheckInNotYet);
                                }
                                if (verifyTime < ruleTime.AddMinutes(-earlyMinutes) && isFirstHaftLeave)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeRegisteredFirstHaftLeave);
                                }
                                if ((!ruleData.AllowFreeInAndOutInTimeRange && verifyTime > ruleTime.AddMinutes(lateMinutes))
                                    || (ruleData.AllowFreeInAndOutInTimeRange && verifyTime > ruleTimeOut.AddMinutes(lateOutMinutes)))
                                {
                                    mArrViolation.Add((int)EMonitoringError.ExceedCheckInTime);
                                }
                                if (verifyTime > ruleError.AddMinutes(lateMinutes) && isLastHaftLeave)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeRegisteredLastHaftLeave);
                                }
                            }
                        }
                    }

                    #endregion
                }
                else
                {
                    // kiểm tra log vào
                    GC_TimeLog logIn = GetTimeLogByParams(empATID, approveStatus: 1, inOutMode: 1, checkTime, companyIndex);
                    var general_rule = _GC_Rules_GeneralService.FirstOrDefault(x => x.IsUsing);
                    if (logIn == null && general_rule?.IgnoreInLog == false)
                    {
                        mArrViolation.Add((int)EMonitoringError.CheckInLogNotExist);
                    }
                    else
                    {
                        int earlyMinutes = ruleData.MaxEarlyCheckOutMinute;
                        int lateMinutes = ruleData.MaxLateCheckOutMinute;
                        int earlyInMinutes = ruleData.MaxEarlyCheckInMinute;
                        int lateInMinutes = ruleData.MaxLateCheckInMinute;
                        var missionEarlyOutMinutes = 0;
                        var missionLateInMinutes = 0;
                        if (ruleData.AllowEarlyOutLateInMission)
                        {
                            missionEarlyOutMinutes = ruleData.MissionMaxEarlyCheckOutMinute;
                            missionLateInMinutes = ruleData.MissionMaxLateCheckInMinute;
                        }
                        DateTime ruleTime = DateTime.MinValue;
                        DateTime ruleInTime = DateTime.MinValue;

                        if (ruleData.AdjustByLateInEarlyOut == true)
                        {
                            var lateInEarlyOut = _EzhrClient.LateInEarlyOutApprovedResult(empATID, checkTime).Result;

                            if (lateInEarlyOut != null && lateInEarlyOut.Data != null && lateInEarlyOut.Data.Count > 0)
                            {
                                // add early out
                                earlyMinutes += lateInEarlyOut.Data[0].EarlyOut;
                                // add late out
                                lateMinutes += lateInEarlyOut.Data[0].LateOut;
                            }
                        }

                        // check ra theo thời gian chọn
                        if (ruleData.CheckOutByShift == false)
                        {
                            if (ruleData.CheckInTime != null)
                            {
                                ruleInTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, ruleData.CheckInTime.Value.Hour, ruleData.CheckInTime.Value.Minute, 0);
                            }
                            if (ruleData.CheckOutTime != null)
                            {
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, ruleData.CheckOutTime.Value.Hour, ruleData.CheckOutTime.Value.Minute, 0);
                                verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                            }
                        }
                        else
                        {
                            var workingHours = _EzhrClient.CheckMaximumWorkingHoursAndOT(empATID, checkTime).Result;

                            if (workingHours != null && workingHours.Data != null && workingHours.Data.ShiftsInfo != null && workingHours.Data.ShiftsInfo.Count > 0)
                            {
                                ruleInTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].StartTime.Hour, workingHours.Data.ShiftsInfo[0].StartTime.Minute, 0);
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].EndTime.Value.Hour, workingHours.Data.ShiftsInfo[0].EndTime.Value.Minute, 0);
                                verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeNotAssignSchedule);
                            }
                        }
                        if (ruleTime != DateTime.MinValue)
                        {
                            var isFirstHaftLeave = false;
                            var startRuleTime = (DateTime)SqlDateTime.MinValue;
                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var halfFirst = leaveDay.Data.Where(x => x.Type == "FirstHaft").ToList();
                                if (halfFirst != null && halfFirst.Count > 0)
                                {
                                    var beginLastHalfTime = ruleData.BeginLastHaftTime.Value;
                                    startRuleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                    isFirstHaftLeave = true;
                                }
                            }

                            var isLastHaftLeave = false;
                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var halfFirst = leaveDay.Data.Where(x => x.Type == "LastHaft").ToList();
                                if (halfFirst != null && halfFirst.Count > 0)
                                {
                                    var beginLastHalfTime = ruleData.EndFirstHaftTime.Value;
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                    isLastHaftLeave = true;
                                }
                            }

                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var checkWithTime = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime <= ruleTime && x.ToTime >= ruleTime);
                                if (checkWithTime != null)
                                {
                                    var beginLastHalfTime = checkWithTime.FromTime;
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                }
                            }

                            if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                            {
                                var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null
                                    && x.From.Value.AddMinutes(-missionEarlyOutMinutes) <= ruleTime
                                    && x.To.Value.AddMinutes(missionLateInMinutes) >= ruleTime);
                                if (checkWithTime != null)
                                {
                                    var beginLastHalfTime = checkWithTime.From.Value.AddMinutes(-missionEarlyOutMinutes);
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                }
                            }

                            var checkValid = false;

                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var checkWithTime = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime.AddMinutes(-earlyMinutes) <= checkTime && x.FromTime.AddMinutes(lateMinutes) >= checkTime);
                                if (checkWithTime != null)
                                {
                                    result.LeaveType = (int)LeaveType.Leaveday;
                                    checkValid = true;
                                }
                            }

                            if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                            {
                                //var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null 
                                //    && x.From.Value.AddMinutes(-earlyMinutes) <= checkTime 
                                //    && x.From.Value.AddMinutes(lateMinutes) >= checkTime);
                                var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null
                                    && x.From.Value.AddMinutes(-missionEarlyOutMinutes) <= checkTime
                                    && x.To.Value.AddMinutes(missionLateInMinutes) >= checkTime);
                                if (checkWithTime != null)
                                {
                                    result.LeaveType = (int)LeaveType.Mission;
                                    checkValid = true;
                                }
                            }

                            if (!ruleData.AllowCheckOutInWorkingTime)
                            {
                                //if (verifyTime < ruleTime.AddMinutes(-earlyMinutes) && !checkValid)
                                //{
                                //    if (isFirstHaftLeave)
                                //    {
                                //        mArrViolation.Add((int)EMonitoringError.EmployeeRegisteredFirstHaftLeave);
                                //    }
                                //    else
                                //    {
                                //        mArrViolation.Add((int)EMonitoringError.NotAllowBreakInOut);
                                //    }
                                //}
                                if (verifyTime < startRuleTime.AddMinutes(-earlyMinutes) && !checkValid && isFirstHaftLeave)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeRegisteredFirstHaftLeave);
                                }

                                if (!ruleData.AllowFreeInAndOutInTimeRange && verifyTime < ruleTime.AddMinutes(-earlyMinutes) && !checkValid)
                                {
                                    mArrViolation.Add((int)EMonitoringError.NotAllowBreakInOut);
                                }
                                else if (ruleData.AllowFreeInAndOutInTimeRange && verifyTime < ruleInTime.AddMinutes(-earlyInMinutes)
                                    && !checkValid)
                                {
                                    mArrViolation.Add((int)EMonitoringError.NotAllowBreakInOut);
                                }
                            }
                            else if (ruleData.AllowCheckOutInWorkingTime && !ruleData.AllowFreeInAndOutInTimeRange)
                            {
                                if (!string.IsNullOrWhiteSpace(ruleData.AllowCheckOutInWorkingTimeRange))
                                {
                                    var allowCheckOutInWorkingTimeRange = JsonConvert.DeserializeObject<List<AllowCheckOutInWorkingTimeRangeModel>>
                                        (ruleData.AllowCheckOutInWorkingTimeRange);

                                    var isInBreakOutTime = false;
                                    foreach (var timeRange in allowCheckOutInWorkingTimeRange)
                                    {
                                        var startTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, timeRange.FromTime.Hour, timeRange.FromTime.Minute, 0);
                                        var endTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, timeRange.ToTime.Hour, timeRange.ToTime.Minute, 0);

                                        if (checkTime >= startTime && checkTime <= endTime)
                                        {
                                            isInBreakOutTime = true;
                                        }
                                    }

                                    if (!isInBreakOutTime && verifyTime > ruleTime.AddMinutes(lateMinutes))
                                    {
                                        mArrViolation.Add((int)EMonitoringError.ExceedCheckOutTime);
                                    }
                                    else if (!isInBreakOutTime
                                        && !(checkTime >= ruleTime.AddMinutes(-earlyMinutes) && checkTime <= ruleTime.AddMinutes(lateMinutes)))
                                    {
                                        mArrViolation.Add((int)EMonitoringError.CheckOutNotYet);
                                    }
                                }
                                else
                                {
                                    mArrViolation.Add((int)EMonitoringError.NotAllowBreakInOut);
                                }
                            }

                            if (verifyTime > ruleTime.AddMinutes(lateMinutes) && !checkValid
                                && !isLastHaftLeave)
                            {
                                mArrViolation.Add((int)EMonitoringError.ExceedCheckOutTime);
                            }
                            if (verifyTime > ruleTime.AddMinutes(lateMinutes) && !checkValid && isLastHaftLeave)
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeRegisteredLastHaftLeave);
                            }
                        }
                    }
                }
            }
            else
            {
                mArrViolation.Add((int)EMonitoringError.NotFoundRule);
            }

            result.SetStatus(GetpriorityViolation(mArrViolation)); // Lấy vi phạm có ưu tiên cao nhất

            return result;
        }

        protected CheckRuleResult CheckRuleForNotEmployee(EmployeeAccessRule ruleData, string empATID, DateTime checkTime, int companyIndex, LogType logType)
        {
            var result = new CheckRuleResult();
            //var ruleData = _GC_Employee_AccessedGroupService.GetInfoByEmpATIDAndFromDate(empATID, checkTime, companyIndex);
            if (ruleData != null)
            {
                // kiểm tra log vào
                GC_TimeLog logIn = GetTimeLogByParams(empATID, approveStatus: 1, inOutMode: 1, checkTime, companyIndex);

                var general_rule = _GC_Rules_GeneralService.FirstOrDefault(x => x.IsUsing);
                if (logIn == null && general_rule?.IgnoreInLog == false)
                {
                    mArrViolation.Add((int)EMonitoringError.CheckInLogNotExist);
                }
            }
            else
            {
                mArrViolation.Add((int)EMonitoringError.NotFoundRule);
            }

            result.SetStatus(GetpriorityViolation(mArrViolation)); // Lấy vi phạm có ưu tiên cao nhất

            return result;
        }

        // Old flow CheckRuleForEmployee, cause slow performance
        protected CheckRuleResult CheckRuleForEmployee(string empATID, DateTime checkTime, int companyIndex, LogType logType)
        {
            var result = new CheckRuleResult();
            var ruleData = _GC_Employee_AccessedGroupService.GetInfoByEmpATIDAndFromDate(empATID, checkTime, companyIndex);
            if (ruleData != null)
            {
                var leaveDay = new LeaveDayBasicInfoReponse();
                if (ruleData.AllowInLeaveDay)
                {
                    // ko cho phép vào nv nghỉ phép cả ngày
                    leaveDay = _EzhrClient.GetListEmployeeLeaveDay(empATID, checkTime).Result;
                    if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                    {
                        var check = leaveDay.Data.FirstOrDefault(x => x.Type == "WholeDay");
                        if (check != null)
                        {
                            if (mInOutMode == 1)
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeInLeaveTime);
                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeOutLeaveTime);
                            }

                        }

                        var checkFirstAndLastHalf = leaveDay.Data.Where(x => x.Type == "FirstHaft" || x.Type == "LastHaft");
                        if (checkFirstAndLastHalf != null && checkFirstAndLastHalf.Count() > 1)
                        {
                            if (mInOutMode == 1)
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeInLeaveTime);
                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeOutLeaveTime);
                            }
                        }
                    }
                }


                var missedLst = new MissionApprovedResultReponse();
                if (ruleData.AllowInMission)
                {
                    // ko cho phép vào nv đăng ký công tác cả ngày
                    missedLst = _EzhrClient.GetListMissionApproved(empATID, checkTime).Result;
                    if (missedLst != null && missedLst.Data != null && missedLst.Data.Count > 0)
                    {
                        var check = missedLst.Data.FirstOrDefault(x => x.From == null && x.To == null);
                        if (check != null)
                        {
                            if (mInOutMode == 1)
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeInMissionTime);
                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeOutMissionTime);
                            }

                        }

                    }
                }
                var verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                if (mInOutMode == 1)
                {
                    #region kiểm tra các quy định cấm vào
                    // kiểm tra ra vào trong giờ làm việc
                    var logOut = GetLogOutByShift(ruleData, empATID, approveStatus: 1, inOutMode: 2, checkTime, logType, companyIndex, isOrder: true);

                    // log vào đầu tiên
                    if (logOut == null)
                    {
                        int earlyMinutes = ruleData.MaxEarlyCheckInMinute;
                        int lateMinutes = ruleData.MaxLateCheckInMinute;
                        var ruleTime = DateTime.MinValue;


                        if (ruleData.AdjustByLateInEarlyOut == true)
                        {
                            var lateInEarlyOut = _EzhrClient.LateInEarlyOutApprovedResult(empATID, checkTime).Result;
                            if (lateInEarlyOut != null && lateInEarlyOut.Data != null && lateInEarlyOut.Data.Count > 0)
                            {
                                // add early out
                                earlyMinutes += lateInEarlyOut.Data[0].EarlyIn;
                                // add late out
                                lateMinutes += lateInEarlyOut.Data[0].LateIn;
                            }
                        }
                        if (ruleData.CheckInByShift == false)
                        {
                            if (ruleData.CheckInTime != null)
                            {
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, ruleData.CheckInTime.Value.Hour, ruleData.CheckInTime.Value.Minute, 0);
                                verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                            }
                        }
                        else
                        {
                            var workingHours = _EzhrClient.CheckMaximumWorkingHoursAndOT(empATID, checkTime).Result;

                            if (workingHours != null && workingHours.Data != null && workingHours.Data.ShiftsInfo != null && workingHours.Data.ShiftsInfo.Count > 0)
                            {
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].StartTime.Hour, workingHours.Data.ShiftsInfo[0].StartTime.Minute, 0);

                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeNotAssignSchedule);
                            }
                        }
                        if (ruleTime != DateTime.MinValue)
                        {
                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var halfFirst = leaveDay.Data.Where(x => x.Type == "FirstHaft").ToList();
                                if (halfFirst != null && halfFirst.Count > 0)
                                {
                                    var beginLastHalfTime = ruleData.BeginLastHaftTime.Value;
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                }
                            }


                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var checkWithTime = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime <= ruleTime && x.ToTime >= ruleTime);
                                if (checkWithTime != null)
                                {
                                    var beginLastHalfTime = checkWithTime.ToTime;
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                }
                            }
                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var check = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime <= checkTime && x.ToTime.AddMinutes(-earlyMinutes) >= checkTime);
                                if (check != null)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeInLeaveTime);
                                }
                            }
                            if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                            {
                                var check = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null && x.From <= checkTime && x.To.Value.AddMinutes(-earlyMinutes) >= checkTime);
                                if (check != null)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeInMissionTime);
                                }
                            }

                            if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                            {
                                var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null && x.From <= ruleTime && x.To >= ruleTime);
                                if (checkWithTime != null)
                                {
                                    var beginLastHalfTime = checkWithTime.To.Value;
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                }
                            }

                            if (verifyTime < ruleTime.AddMinutes(-earlyMinutes))
                            {
                                mArrViolation.Add((int)EMonitoringError.CheckInNotYet);
                            }
                            if (verifyTime > ruleTime.AddMinutes(lateMinutes))
                            {
                                mArrViolation.Add((int)EMonitoringError.ExceedCheckInTime);
                            }
                        }
                    }
                    else
                    {
                        int earlyMinutes = ruleData.MaxEarlyCheckInMinute;
                        int lateMinutes = ruleData.MaxLateCheckInMinute;
                        var ruleTime = DateTime.MinValue;
                        var checkValid = false;
                        var ruleError = DateTime.MinValue;

                        if (ruleData.AdjustByLateInEarlyOut == true)
                        {
                            var lateInEarlyOut = _EzhrClient.LateInEarlyOutApprovedResult(empATID, checkTime).Result;
                            if (lateInEarlyOut != null && lateInEarlyOut.Data != null && lateInEarlyOut.Data.Count > 0)
                            {
                                // add early out
                                earlyMinutes += lateInEarlyOut.Data[0].EarlyIn;
                                // add late out
                                lateMinutes += lateInEarlyOut.Data[0].LateIn;
                            }
                        }
                        if (ruleData.CheckInByShift == false)
                        {
                            if (ruleData.CheckInTime != null)
                            {
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, ruleData.CheckInTime.Value.Hour, ruleData.CheckInTime.Value.Minute, 0);
                                verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                            }
                            if (ruleData.CheckOutTime != null)
                            {
                                ruleError = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, ruleData.CheckOutTime.Value.Hour, ruleData.CheckOutTime.Value.Minute, 0);

                            }
                        }
                        else
                        {
                            var workingHours = _EzhrClient.CheckMaximumWorkingHoursAndOT(empATID, checkTime).Result;

                            if (workingHours != null && workingHours.Data != null && workingHours.Data.ShiftsInfo != null && workingHours.Data.ShiftsInfo.Count > 0)
                            {
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].StartTime.Hour, workingHours.Data.ShiftsInfo[0].StartTime.Minute, 0);
                                ruleError = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].EndTime.Value.Hour, workingHours.Data.ShiftsInfo[0].EndTime.Value.Minute, 0);
                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeNotAssignSchedule);
                            }
                        }


                        if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                        {
                            var halfFirst = leaveDay.Data.Where(x => x.Type == "LastHaft").ToList();
                            if (halfFirst != null && halfFirst.Count > 0)
                            {
                                var beginLastHalfTime = ruleData.EndFirstHaftTime.Value;
                                ruleError = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                            }
                        }

                        if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                        {
                            var checkWithTime = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime <= checkTime && x.ToTime.AddMinutes(-earlyMinutes) >= checkTime);
                            if (checkWithTime != null)
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeInLeaveTime);
                                checkValid = true;
                            }
                        }

                        if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                        {
                            var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null && x.From <= checkTime && x.To.Value.AddMinutes(-earlyMinutes) >= checkTime);
                            if (checkWithTime != null)
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeInMissionTime);
                                checkValid = true;
                            }
                        }

                        if (logOut.LeaveType != 0 && !checkValid)
                        {
                            if (logOut.LeaveType == (int)LeaveType.Leaveday)
                            {
                                var checkWithTime = leaveDay.Data.Where(x => x.Type == "WithTime" && checkTime >= x.ToTime.AddMinutes(-earlyMinutes)).OrderByDescending(x => x.ToTime).FirstOrDefault();
                                if (checkWithTime != null)
                                {
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkWithTime.ToTime.Hour, checkWithTime.ToTime.Minute, 0);
                                }
                            }
                            else if (logOut.LeaveType == (int)LeaveType.Mission)
                            {
                                var checkWithTime = missedLst.Data.Where(x => x.From != null && x.To != null && x.To.Value.AddMinutes(-earlyMinutes) <= checkTime).OrderByDescending(x => x.To).FirstOrDefault();
                                if (checkWithTime != null)
                                {
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkWithTime.To.Value.Hour, checkWithTime.To.Value.Minute, 0);
                                }
                            }


                            if (verifyTime < ruleTime.AddMinutes(-earlyMinutes))
                            {
                                mArrViolation.Add((int)EMonitoringError.CheckInNotYet);
                            }
                            if (verifyTime > ruleTime.AddMinutes(lateMinutes))
                            {
                                mArrViolation.Add((int)EMonitoringError.ExceedCheckInTime);
                            }

                        }
                        else if (!checkValid)
                        {
                            if (ruleData.AllowCheckOutInWorkingTime)
                            {
                                if (checkTime.Subtract(logOut.Time).TotalMinutes > ruleData.MaxMinuteAllowOutsideInWorkingTime)
                                {
                                    mArrViolation.Add((int)EMonitoringError.ExceedMaxMinuteAllowOutsideInWorkingTime);
                                }
                            }

                            if (verifyTime > ruleError.AddMinutes(lateMinutes))
                            {
                                mArrViolation.Add((int)EMonitoringError.ExceedCheckOutTime);
                            }
                        }
                    }

                    #endregion
                }
                else
                {
                    // kiểm tra log vào
                    GC_TimeLog logIn = GetTimeLogByParams(empATID, approveStatus: 1, inOutMode: 1, checkTime, companyIndex);

                    var general_rule = _GC_Rules_GeneralService.FirstOrDefault(x => x.IsUsing);
                    if (logIn == null && general_rule?.IgnoreInLog == false)
                    {
                        mArrViolation.Add((int)EMonitoringError.CheckInLogNotExist);
                    }
                    else
                    {
                        int earlyMinutes = ruleData.MaxEarlyCheckOutMinute;
                        int lateMinutes = ruleData.MaxLateCheckOutMinute;
                        DateTime ruleTime = DateTime.MinValue;

                        if (ruleData.AdjustByLateInEarlyOut == true)
                        {
                            var lateInEarlyOut = _EzhrClient.LateInEarlyOutApprovedResult(empATID, checkTime).Result;

                            if (lateInEarlyOut != null && lateInEarlyOut.Data != null && lateInEarlyOut.Data.Count > 0)
                            {
                                // add early out
                                earlyMinutes += lateInEarlyOut.Data[0].EarlyOut;
                                // add late out
                                lateMinutes += lateInEarlyOut.Data[0].LateOut;
                            }
                        }

                        // check ra theo thời gian chọn
                        if (ruleData.CheckOutByShift == false)
                        {
                            if (ruleData.CheckOutTime != null)
                            {
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, ruleData.CheckOutTime.Value.Hour, ruleData.CheckOutTime.Value.Minute, 0);
                                verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                            }
                        }
                        else
                        {
                            var workingHours = _EzhrClient.CheckMaximumWorkingHoursAndOT(empATID, checkTime).Result;

                            if (workingHours != null && workingHours.Data != null && workingHours.Data.ShiftsInfo != null && workingHours.Data.ShiftsInfo.Count > 0)
                            {
                                ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].EndTime.Value.Hour, workingHours.Data.ShiftsInfo[0].EndTime.Value.Minute, 0);
                                verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeNotAssignSchedule);
                            }
                        }
                        if (ruleTime != DateTime.MinValue)
                        {
                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var halfFirst = leaveDay.Data.Where(x => x.Type == "LastHaft").ToList();
                                if (halfFirst != null && halfFirst.Count > 0)
                                {
                                    var beginLastHalfTime = ruleData.EndFirstHaftTime.Value;
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                }
                            }

                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var checkWithTime = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime <= ruleTime && x.ToTime >= ruleTime);
                                if (checkWithTime != null)
                                {
                                    var beginLastHalfTime = checkWithTime.FromTime;
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                }
                            }

                            if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                            {
                                var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null && x.From <= ruleTime && x.To >= ruleTime);
                                if (checkWithTime != null)
                                {
                                    var beginLastHalfTime = checkWithTime.From.Value;
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                }
                            }

                            var checkValid = false;

                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var checkWithTime = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime.AddMinutes(-earlyMinutes) <= checkTime && x.FromTime.AddMinutes(lateMinutes) >= checkTime);
                                if (checkWithTime != null)
                                {
                                    result.LeaveType = (int)LeaveType.Leaveday;
                                    checkValid = true;
                                }
                            }

                            if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                            {
                                var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null
                                    && x.From.Value.AddMinutes(-earlyMinutes) <= checkTime
                                    && x.From.Value.AddMinutes(lateMinutes) >= checkTime);
                                if (checkWithTime != null)
                                {
                                    result.LeaveType = (int)LeaveType.Mission;
                                    checkValid = true;
                                }
                            }
                            if (!ruleData.AllowCheckOutInWorkingTime)
                            {
                                if (verifyTime < ruleTime.AddMinutes(-earlyMinutes) && !checkValid)
                                {
                                    mArrViolation.Add((int)EMonitoringError.NotAllowBreakInOut);
                                }
                            }

                            if (verifyTime > ruleTime.AddMinutes(lateMinutes) && !checkValid)
                            {
                                mArrViolation.Add((int)EMonitoringError.ExceedCheckOutTime);
                            }
                        }
                    }
                }
            }
            else
            {
                var rulaDataByDepartmentAccessedgroup = _GC_Department_AccessedGroupService.GetInfoDepartmentAccessedGroup(empATID, checkTime, companyIndex);
                if (rulaDataByDepartmentAccessedgroup != null)
                {
                    var leaveDay = new LeaveDayBasicInfoReponse();
                    if (rulaDataByDepartmentAccessedgroup.AllowInLeaveDay)
                    {
                        // ko cho phép vào nv nghỉ phép cả ngày
                        leaveDay = _EzhrClient.GetListEmployeeLeaveDay(empATID, checkTime).Result;
                        if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                        {
                            var check = leaveDay.Data.FirstOrDefault(x => x.Type == "WholeDay");
                            if (check != null)
                            {
                                if (mInOutMode == 1)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeInLeaveTime);
                                }
                                else
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeOutLeaveTime);
                                }

                            }

                            var checkFirstAndLastHalf = leaveDay.Data.Where(x => x.Type == "FirstHaft" || x.Type == "LastHaft");
                            if (checkFirstAndLastHalf != null && checkFirstAndLastHalf.Count() > 1)
                            {
                                if (mInOutMode == 1)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeInLeaveTime);
                                }
                                else
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeOutLeaveTime);
                                }
                            }
                        }
                    }


                    var missedLst = new MissionApprovedResultReponse();
                    if (rulaDataByDepartmentAccessedgroup.AllowInMission)
                    {
                        // ko cho phép vào nv đăng ký công tác cả ngày
                        missedLst = _EzhrClient.GetListMissionApproved(empATID, checkTime).Result;
                        if (missedLst != null && missedLst.Data != null && missedLst.Data.Count > 0)
                        {
                            var check = missedLst.Data.FirstOrDefault(x => x.From == null && x.To == null);
                            if (check != null)
                            {
                                if (mInOutMode == 1)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeInMissionTime);
                                }
                                else
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeOutMissionTime);
                                }

                            }

                        }
                    }
                    var verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                    if (mInOutMode == 1)
                    {
                        #region kiểm tra các quy định cấm vào
                        // kiểm tra ra vào trong giờ làm việc
                        var logOut = GetLogOutByShift(rulaDataByDepartmentAccessedgroup, empATID, approveStatus: 1, inOutMode: 2, checkTime, logType, companyIndex, isOrder: true);

                        // log vào đầu tiên
                        if (logOut == null)
                        {
                            int earlyMinutes = rulaDataByDepartmentAccessedgroup.MaxEarlyCheckInMinute;
                            int lateMinutes = rulaDataByDepartmentAccessedgroup.MaxLateCheckInMinute;
                            var ruleTime = DateTime.MinValue;


                            if (rulaDataByDepartmentAccessedgroup.AdjustByLateInEarlyOut == true)
                            {
                                var lateInEarlyOut = _EzhrClient.LateInEarlyOutApprovedResult(empATID, checkTime).Result;
                                if (lateInEarlyOut != null && lateInEarlyOut.Data != null && lateInEarlyOut.Data.Count > 0)
                                {
                                    // add early out
                                    earlyMinutes += lateInEarlyOut.Data[0].EarlyIn;
                                    // add late out
                                    lateMinutes += lateInEarlyOut.Data[0].LateIn;
                                }
                            }
                            if (rulaDataByDepartmentAccessedgroup.CheckInByShift == false)
                            {
                                if (rulaDataByDepartmentAccessedgroup.CheckInTime != null)
                                {
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, rulaDataByDepartmentAccessedgroup.CheckInTime.Value.Hour, rulaDataByDepartmentAccessedgroup.CheckInTime.Value.Minute, 0);
                                    verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                                }
                            }
                            else
                            {
                                var workingHours = _EzhrClient.CheckMaximumWorkingHoursAndOT(empATID, checkTime).Result;

                                if (workingHours != null && workingHours.Data != null && workingHours.Data.ShiftsInfo != null && workingHours.Data.ShiftsInfo.Count > 0)
                                {
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].StartTime.Hour, workingHours.Data.ShiftsInfo[0].StartTime.Minute, 0);

                                }
                                else
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeNotAssignSchedule);
                                }
                            }
                            if (ruleTime != DateTime.MinValue)
                            {
                                if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                                {
                                    var halfFirst = leaveDay.Data.Where(x => x.Type == "FirstHaft").ToList();
                                    if (halfFirst != null && halfFirst.Count > 0)
                                    {
                                        var beginLastHalfTime = rulaDataByDepartmentAccessedgroup.BeginLastHaftTime.Value;
                                        ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                    }
                                }


                                if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                                {
                                    var checkWithTime = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime <= ruleTime && x.ToTime >= ruleTime);
                                    if (checkWithTime != null)
                                    {
                                        var beginLastHalfTime = checkWithTime.ToTime;
                                        ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                    }
                                }
                                if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                                {
                                    var check = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime <= checkTime && x.ToTime.AddMinutes(-earlyMinutes) >= checkTime);
                                    if (check != null)
                                    {
                                        mArrViolation.Add((int)EMonitoringError.EmployeeInLeaveTime);
                                    }
                                }
                                if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                                {
                                    var check = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null && x.From <= checkTime && x.To.Value.AddMinutes(-earlyMinutes) >= checkTime);
                                    if (check != null)
                                    {
                                        mArrViolation.Add((int)EMonitoringError.EmployeeInMissionTime);
                                    }
                                }

                                if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                                {
                                    var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null && x.From <= ruleTime && x.To >= ruleTime);
                                    if (checkWithTime != null)
                                    {
                                        var beginLastHalfTime = checkWithTime.To.Value;
                                        ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                    }
                                }

                                if (verifyTime < ruleTime.AddMinutes(-earlyMinutes))
                                {
                                    mArrViolation.Add((int)EMonitoringError.CheckInNotYet);
                                }
                                if (verifyTime > ruleTime.AddMinutes(lateMinutes))
                                {
                                    mArrViolation.Add((int)EMonitoringError.ExceedCheckInTime);
                                }
                            }
                        }
                        else
                        {
                            int earlyMinutes = rulaDataByDepartmentAccessedgroup.MaxEarlyCheckInMinute;
                            int lateMinutes = rulaDataByDepartmentAccessedgroup.MaxLateCheckInMinute;
                            var ruleTime = DateTime.MinValue;
                            var checkValid = false;
                            var ruleError = DateTime.MinValue;

                            if (rulaDataByDepartmentAccessedgroup.AdjustByLateInEarlyOut == true)
                            {
                                var lateInEarlyOut = _EzhrClient.LateInEarlyOutApprovedResult(empATID, checkTime).Result;
                                if (lateInEarlyOut != null && lateInEarlyOut.Data != null && lateInEarlyOut.Data.Count > 0)
                                {
                                    // add early out
                                    earlyMinutes += lateInEarlyOut.Data[0].EarlyIn;
                                    // add late out
                                    lateMinutes += lateInEarlyOut.Data[0].LateIn;
                                }
                            }
                            if (rulaDataByDepartmentAccessedgroup.CheckInByShift == false)
                            {
                                if (rulaDataByDepartmentAccessedgroup.CheckInTime != null)
                                {
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, rulaDataByDepartmentAccessedgroup.CheckInTime.Value.Hour, rulaDataByDepartmentAccessedgroup.CheckInTime.Value.Minute, 0);
                                    verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                                }
                                if (rulaDataByDepartmentAccessedgroup.CheckOutTime != null)
                                {
                                    ruleError = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, rulaDataByDepartmentAccessedgroup.CheckOutTime.Value.Hour, rulaDataByDepartmentAccessedgroup.CheckOutTime.Value.Minute, 0);

                                }
                            }
                            else
                            {
                                var workingHours = _EzhrClient.CheckMaximumWorkingHoursAndOT(empATID, checkTime).Result;

                                if (workingHours != null && workingHours.Data != null && workingHours.Data.ShiftsInfo != null && workingHours.Data.ShiftsInfo.Count > 0)
                                {
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].StartTime.Hour, workingHours.Data.ShiftsInfo[0].StartTime.Minute, 0);
                                    ruleError = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].EndTime.Value.Hour, workingHours.Data.ShiftsInfo[0].EndTime.Value.Minute, 0);
                                }
                                else
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeNotAssignSchedule);
                                }
                            }


                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var halfFirst = leaveDay.Data.Where(x => x.Type == "LastHaft").ToList();
                                if (halfFirst != null && halfFirst.Count > 0)
                                {
                                    var beginLastHalfTime = rulaDataByDepartmentAccessedgroup.EndFirstHaftTime.Value;
                                    ruleError = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                }
                            }

                            if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                            {
                                var checkWithTime = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime <= checkTime && x.ToTime.AddMinutes(-earlyMinutes) >= checkTime);
                                if (checkWithTime != null)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeInLeaveTime);
                                    checkValid = true;
                                }
                            }

                            if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                            {
                                var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null && x.From <= checkTime && x.To.Value.AddMinutes(-earlyMinutes) >= checkTime);
                                if (checkWithTime != null)
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeInMissionTime);
                                    checkValid = true;
                                }
                            }

                            if (logOut.LeaveType != 0 && !checkValid)
                            {
                                if (logOut.LeaveType == (int)LeaveType.Leaveday)
                                {
                                    var checkWithTime = leaveDay.Data.Where(x => x.Type == "WithTime" && checkTime >= x.ToTime.AddMinutes(-earlyMinutes)).OrderByDescending(x => x.ToTime).FirstOrDefault();
                                    if (checkWithTime != null)
                                    {
                                        ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkWithTime.ToTime.Hour, checkWithTime.ToTime.Minute, 0);
                                    }
                                }
                                else if (logOut.LeaveType == (int)LeaveType.Mission)
                                {
                                    var checkWithTime = missedLst.Data.Where(x => x.From != null && x.To != null && x.To.Value.AddMinutes(-earlyMinutes) <= checkTime).OrderByDescending(x => x.To).FirstOrDefault();
                                    if (checkWithTime != null)
                                    {
                                        ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkWithTime.To.Value.Hour, checkWithTime.To.Value.Minute, 0);
                                    }
                                }


                                if (verifyTime < ruleTime.AddMinutes(-earlyMinutes))
                                {
                                    mArrViolation.Add((int)EMonitoringError.CheckInNotYet);
                                }
                                if (verifyTime > ruleTime.AddMinutes(lateMinutes))
                                {
                                    mArrViolation.Add((int)EMonitoringError.ExceedCheckInTime);
                                }

                            }
                            else if (!checkValid)
                            {
                                if (rulaDataByDepartmentAccessedgroup.AllowCheckOutInWorkingTime)
                                {
                                    if (checkTime.Subtract(logOut.Time).TotalMinutes > rulaDataByDepartmentAccessedgroup.MaxMinuteAllowOutsideInWorkingTime)
                                    {
                                        mArrViolation.Add((int)EMonitoringError.ExceedMaxMinuteAllowOutsideInWorkingTime);
                                    }
                                }

                                if (verifyTime > ruleError.AddMinutes(lateMinutes))
                                {
                                    mArrViolation.Add((int)EMonitoringError.ExceedCheckOutTime);
                                }
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        // kiểm tra log vào
                        GC_TimeLog logIn = GetTimeLogByParams(empATID, approveStatus: 1, inOutMode: 1, checkTime, companyIndex);

                        var general_rule = _GC_Rules_GeneralService.FirstOrDefault(x => x.IsUsing);
                        if (logIn == null && general_rule?.IgnoreInLog == false)
                        {
                            mArrViolation.Add((int)EMonitoringError.CheckInLogNotExist);
                        }
                        else
                        {
                            int earlyMinutes = rulaDataByDepartmentAccessedgroup.MaxEarlyCheckOutMinute;
                            int lateMinutes = rulaDataByDepartmentAccessedgroup.MaxLateCheckOutMinute;
                            DateTime ruleTime = DateTime.MinValue;

                            if (rulaDataByDepartmentAccessedgroup.AdjustByLateInEarlyOut == true)
                            {
                                var lateInEarlyOut = _EzhrClient.LateInEarlyOutApprovedResult(empATID, checkTime).Result;

                                if (lateInEarlyOut != null && lateInEarlyOut.Data != null && lateInEarlyOut.Data.Count > 0)
                                {
                                    // add early out
                                    earlyMinutes += lateInEarlyOut.Data[0].EarlyOut;
                                    // add late out
                                    lateMinutes += lateInEarlyOut.Data[0].LateOut;
                                }
                            }

                            // check ra theo thời gian chọn
                            if (rulaDataByDepartmentAccessedgroup.CheckOutByShift == false)
                            {
                                if (rulaDataByDepartmentAccessedgroup.CheckOutTime != null)
                                {
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, rulaDataByDepartmentAccessedgroup.CheckOutTime.Value.Hour, rulaDataByDepartmentAccessedgroup.CheckOutTime.Value.Minute, 0);
                                    verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                                }
                            }
                            else
                            {
                                var workingHours = _EzhrClient.CheckMaximumWorkingHoursAndOT(empATID, checkTime).Result;

                                if (workingHours != null && workingHours.Data != null && workingHours.Data.ShiftsInfo != null && workingHours.Data.ShiftsInfo.Count > 0)
                                {
                                    ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, workingHours.Data.ShiftsInfo[0].EndTime.Value.Hour, workingHours.Data.ShiftsInfo[0].EndTime.Value.Minute, 0);
                                    verifyTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, checkTime.Hour, checkTime.Minute, 0);
                                }
                                else
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeNotAssignSchedule);
                                }
                            }
                            if (ruleTime != DateTime.MinValue)
                            {
                                if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                                {
                                    var halfFirst = leaveDay.Data.Where(x => x.Type == "LastHaft").ToList();
                                    if (halfFirst != null && halfFirst.Count > 0)
                                    {
                                        var beginLastHalfTime = rulaDataByDepartmentAccessedgroup.EndFirstHaftTime.Value;
                                        ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                    }
                                }

                                if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                                {
                                    var checkWithTime = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime <= ruleTime && x.ToTime >= ruleTime);
                                    if (checkWithTime != null)
                                    {
                                        var beginLastHalfTime = checkWithTime.FromTime;
                                        ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                    }
                                }

                                if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                                {
                                    var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null && x.From <= ruleTime && x.To >= ruleTime);
                                    if (checkWithTime != null)
                                    {
                                        var beginLastHalfTime = checkWithTime.From.Value;
                                        ruleTime = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, beginLastHalfTime.Hour, beginLastHalfTime.Minute, 0);
                                    }
                                }

                                var checkValid = false;

                                if (leaveDay != null && leaveDay.Data != null && leaveDay.Data.Count() > 0)
                                {
                                    var checkWithTime = leaveDay.Data.FirstOrDefault(x => x.Type == "WithTime" && x.FromTime.AddMinutes(-earlyMinutes) <= checkTime && x.FromTime.AddMinutes(lateMinutes) >= checkTime);
                                    if (checkWithTime != null)
                                    {
                                        result.LeaveType = (int)LeaveType.Leaveday;
                                        checkValid = true;
                                    }
                                }

                                if (missedLst != null && missedLst.Data != null && missedLst.Data.Count() > 0)
                                {
                                    var checkWithTime = missedLst.Data.FirstOrDefault(x => x.From != null && x.To != null
                                        && x.From.Value.AddMinutes(-earlyMinutes) <= checkTime
                                        && x.From.Value.AddMinutes(lateMinutes) >= checkTime);
                                    if (checkWithTime != null)
                                    {
                                        result.LeaveType = (int)LeaveType.Mission;
                                        checkValid = true;
                                    }
                                }
                                if (!rulaDataByDepartmentAccessedgroup.AllowCheckOutInWorkingTime)
                                {
                                    if (verifyTime < ruleTime.AddMinutes(-earlyMinutes) && !checkValid)
                                    {
                                        mArrViolation.Add((int)EMonitoringError.NotAllowBreakInOut);
                                    }
                                }

                                if (verifyTime > ruleTime.AddMinutes(lateMinutes) && !checkValid)
                                {
                                    mArrViolation.Add((int)EMonitoringError.ExceedCheckOutTime);
                                }
                            }
                        }
                    }
                }
                else
                {
                    mArrViolation.Add((int)EMonitoringError.NotFoundRule);
                }

            }

            result.SetStatus(GetpriorityViolation(mArrViolation)); // Lấy vi phạm có ưu tiên cao nhất

            return result;
        }

        protected GC_TimeLog GetTimeLogByParams(string employeeATID, int approveStatus, int inOutMode, DateTime checkTime, int companyIndex, bool isOrder = false)
        {
            //Log gần nhất (qua ngày)
            GC_TimeLog logResult = null;
            var logs = _GC_TimeLogService.Where(t => t.CompanyIndex == companyIndex && t.LogType == LogType.Walker.ToString() && t.EmployeeATID == employeeATID
                            && t.ApproveStatus == approveStatus && t.Time.Date >= checkTime.Date.AddDays(-7) && t.Time < checkTime); //lấy ds log gần nhất trong 7 ngày
            if (logs != null && logs.Any())
            {
                var log = logs.OrderByDescending(t => t.Time).FirstOrDefault(); //Lấy log gần nhất
                if (log != null
                    //&& log.InOutMode == inOutMode
                    ) //Check xem log gần nhất có phải log vào/ra như param ko
                {
                    logResult = log;
                }
            }
            logs = _GC_TimeLogService.Where(t => t.CompanyIndex == companyIndex && t.LogType == LogType.Parking.ToString() && t.EmployeeATID == employeeATID
                                    && t.ApproveStatus == approveStatus && t.Time.Date >= checkTime.Date.AddDays(-7) && t.Time < checkTime); // Check log nhà xe nếu như log đi bộ ko có
            if (logs != null && logs.Any())
            {
                var log = logs.OrderByDescending(t => t.Time).FirstOrDefault(); //Lấy log gần nhất
                if (log != null
                    //&& log.InOutMode == inOutMode
                    ) //Check xem log gần nhất có phải log vào/ra như param ko
                {
                    logResult = log;
                }
            }

            var vehicleLogs = _IC_VehicleLogService.Where(x => x.EmployeeATID == employeeATID &&
                ((!x.ToDate.HasValue && x.FromDate.HasValue && x.FromDate.Value.Date >= checkTime.Date.AddDays(-7) && x.FromDate.Value < checkTime)
                || (x.ToDate.HasValue && x.ToDate.Value.Date >= checkTime.Date.AddDays(-7) && x.ToDate.Value < checkTime)));
            if (vehicleLogs != null && vehicleLogs.Any())
            {
                var vehicleLog = vehicleLogs.OrderByDescending(x => x.FromDate.Value).FirstOrDefault();
                if (vehicleLog.ToDate.HasValue && (logResult == null || (logResult != null && (logResult.Time < vehicleLog.ToDate.Value))))
                {
                    logResult = new GC_TimeLog();
                    logResult.EmployeeATID = vehicleLog.EmployeeATID;
                    logResult.InOutMode = (short)GCSInOutMode.Output;
                    logResult.Time = vehicleLog.ToDate.Value;
                }
                else if (!vehicleLog.ToDate.HasValue && vehicleLog.FromDate.HasValue
                    && (logResult == null || (logResult != null && (logResult.Time < vehicleLog.FromDate.Value))))
                {
                    logResult = new GC_TimeLog();
                    logResult.EmployeeATID = vehicleLog.EmployeeATID;
                    logResult.InOutMode = (short)GCSInOutMode.Input;
                    logResult.Time = vehicleLog.FromDate.Value;
                }
            }

            if (mEmpInfo.EmployeeType.HasValue && mEmpInfo.EmployeeType.Value == EmployeeType.Driver && (logResult == null
                || (logResult != null && logResult.CardNumber != mEmpInfo.CardNumber))
                && !_GC_TimeLogService.Any(x => x.EmployeeATID == employeeATID && x.ApproveStatus == approveStatus
                && x.Time < checkTime && x.CardNumber == mEmpInfo.CardNumber && x.CompanyIndex == companyIndex))
            {
                var truckDriverLogs = _GC_TruckDriverLogService.GetActiveTruckDriverLogByTripCode(employeeATID);
                if (truckDriverLogs != null && truckDriverLogs.Count > 0
                    && truckDriverLogs.Any(x => x.InOutMode == (short)InOutMode.Input
                    && x.Time < checkTime && !truckDriverLogs.Any(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Output)))
                {
                    var truckDriverLog = truckDriverLogs.FirstOrDefault(x => x.InOutMode == (short)InOutMode.Input && x.Time < checkTime
                        && !truckDriverLogs.Any(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Output));
                    if (!mEmpInfo.IsExtraDriver)
                    {
                        if (truckDriverLog != null && truckDriverLog.CardNumber == mEmpInfo.CardNumber)
                        {
                            logResult = new GC_TimeLog();
                            logResult.EmployeeATID = truckDriverLog.TripCode;
                            logResult.InOutMode = (short)GCSInOutMode.Input;
                            logResult.Time = truckDriverLog.Time;
                        }
                    }
                    else
                    {
                        var extraTruckDriverLogs = _GC_TruckDriverLogService.GetActiveExtraTruckDriverLogByTripCode(employeeATID);
                        if (extraTruckDriverLogs != null && extraTruckDriverLogs.Any(x => x.CardNumber == mEmpInfo.CardNumber))
                        {
                            var extraTruckDriverLog = extraTruckDriverLogs.FirstOrDefault(x => x.CardNumber == mEmpInfo.CardNumber);
                            logResult = new GC_TimeLog();
                            logResult.EmployeeATID = extraTruckDriverLog.TripCode;
                            logResult.InOutMode = (short)GCSInOutMode.Input;
                            logResult.Time = truckDriverLog.Time;
                        }
                    }
                }
            }

            if (logResult != null && logResult.InOutMode != inOutMode)
            {
                logResult = null;
            }

            return logResult;
            //if (isOrder)
            //    return log.OrderByDescending(t => t.Time).FirstOrDefault();
            //return log.FirstOrDefault();
        }

        protected void CheckAreaGroup(AttendanceLogRealTime param, EmployeeAccessRule rule, int companyIndex)
        {
            GetDeviceGroups(companyIndex);
            var areaGroups = GetAreaGroups(param.CompanyIndex);// tat ca nhom khu vuc
            if (mGroupDevices != null)
            {
                var device = mGroupDevices.FirstOrDefault(e => e.ListMachine.Contains(param.SerialNumber)); // nhom thiet bi
                if (device != null)
                {
                    mdeviceIndex = device.Index;
                    var parentGeneralAreas = rule.AreaGroups.OrderBy(x => x.Priority).ToList(); // rule 
                    var areaGroup = areaGroups.Where(x => parentGeneralAreas.Any(y => y.AreaGroupIndex == x.Index)).ToList();// tim ra khu vuc dựa theo rule
                    if (!areaGroup.Any()) return;
                    var firstArea = areaGroup.First();
                    var lastArea = areaGroup.Last();

                    if (param.InOutMode == ((int)InOutMode.Output).ToString() && firstArea.DeviceGroups.Contains(device.Index)) // out at first area
                    {
                        //check for logged in actions
                        var timeLogins = GetListTimeLogByParams(param.EmployeeATID, approveStatus: 1, inOutMode: 1, param.CheckTime, companyIndex); // lay list log In vào cổng trong 1 ngày
                        if (timeLogins == null)
                        {
                            var err = firstArea.GetCheckError();
                            mArrViolation.Add((int)err);
                        }
                        else
                        {
                            //get devices for all timeLogs
                            var deviceLogInItems = GetDevicesForTimeLogs(timeLogins);
                            //verify check in for all areaGroup
                            foreach (var areaGroupFullInfo in areaGroup)
                            {
                                var isCheckIn =
                                    areaGroupFullInfo.DeviceGroups.Any(x => deviceLogInItems.Any(d => d.Index == x));
                                if (isCheckIn) continue;
                                var err = areaGroupFullInfo.GetCheckError();
                                mArrViolation.Add((int)err);
                                break;
                            }
                        }

                        //check for logged out actions
                        var timeLogouts = GetListTimeLogByParams(param.EmployeeATID, approveStatus: 1, inOutMode: 2, param.CheckTime, companyIndex); // lay list log Out trong 1 ngày
                        if (timeLogouts == null)
                        {
                            var err = lastArea.GetCheckError(true);
                            mArrViolation.Add((int)err);
                        }
                        else
                        {
                            //get devices for all timeLogs
                            var deviceLogOutItems = GetDevicesForTimeLogs(timeLogouts);
                            //verify check in for all areaGroup
                            areaGroup.Reverse();
                            foreach (var areaGroupFullInfo in areaGroup)
                            {
                                var isCheckOut =
                                    areaGroupFullInfo.DeviceGroups.Any(x => deviceLogOutItems.Any(d => d.Index == x));
                                if (isCheckOut) continue;
                                var err = areaGroupFullInfo.GetCheckError(true);
                                mArrViolation.Add((int)err);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private HashSet<GroupDeviceParam> GetDevicesForTimeLogs(List<GC_TimeLog> timeLogs)
        {
            var deviceItems = new HashSet<GroupDeviceParam>();
            foreach (var item in timeLogs)
            {
                // Get device from group
                var deviceItem = mGroupDevices.FirstOrDefault(e => e.ListMachine.Contains(item.MachineSerial)); // máy thuộc nhóm nào
                if (deviceItem != null)
                {
                    deviceItems.Add(deviceItem);
                }
            }

            return deviceItems;
        }


        protected List<GC_TimeLog> GetListTimeLogByParams(string employeeATID, int approveStatus, int inOutMode, DateTime checkTime, int companyIndex, bool isOrder = false)
        {
            var logs = _GC_TimeLogService.Where(t => t.CompanyIndex == companyIndex && t.LogType == LogType.Walker.ToString() && t.EmployeeATID == employeeATID
                            && t.ApproveStatus == approveStatus && t.Time.Date >= checkTime.Date.AddDays(-1) && t.Time < checkTime && t.InOutMode == inOutMode).ToList(); //lấy ds log gần nhất trong 1 ngày
            if (logs != null && logs.Any())
            {
                return logs;
            }
            logs = _GC_TimeLogService.Where(t => t.CompanyIndex == companyIndex && t.LogType == LogType.Parking.ToString() && t.EmployeeATID == employeeATID
                                    && t.ApproveStatus == approveStatus && t.Time.Date >= checkTime.Date.AddDays(-1) && t.Time < checkTime && t.InOutMode == inOutMode).ToList(); // Check log nhà xe nếu như log đi bộ ko có
            if (logs != null && logs.Any())
            {
                return logs;
            }

            return null;
        }

        protected void CheckAreaAccess(EmployeeAccessRule rule)
        {
            // lấy ds khu vực dc truy cập
            var listGates = _GC_Rules_GeneralAccess_GatesService.Where(t => t.CompanyIndex == _Config.CompanyIndex
                && t.RulesGeneralIndex == rule.GeneralAccessRuleIndex).ToList();

            bool allow = false;
            for (int i = 0; i < listGates.Count; i++)
            {
                string[] arrLines = listGates[i].LineIndexs.Split(',');
                foreach (string item in arrLines)
                {
                    if (item != "" && item == mLineIndex.ToString())
                    {
                        allow = true;
                        break;
                    }
                }
            }

            if (allow == false)
            {
                mArrViolation.Add((int)EMonitoringError.EmployeeNotInAccessGroup);
            }
        }

        protected IEnumerable<AreaGroupFullInfo> GetAreaGroups(int companyIndex)
        {
            return _GC_AreaGroupService.GetFullDataByCompanyIndex(companyIndex).Result;
        }

        protected GC_TimeLog GetLogOutByShift(EmployeeAccessRule ruleData, string empATID, int approveStatus, int inOutMode, DateTime checkTime, LogType logType, int companyIndex, bool isOrder = false)
        {
            // Lấy log trong khoảng thời gian làm việc theo quy định GCS
            var log = _GC_TimeLogService.Where(t => t.CompanyIndex == companyIndex && t.LogType == logType.ToString() && t.EmployeeATID == empATID
                            && t.ApproveStatus == approveStatus && t.InOutMode == inOutMode && t.Time < checkTime);
            if (log != null && log.Any())
            {
                if (mEmpInfo.EmployeeType.HasValue && mEmpInfo.EmployeeType.Value == EmployeeType.Driver)
                {
                    return log.Where(x => x.CardNumber == mEmpInfo.CardNumber).OrderByDescending(t => t.Time).FirstOrDefault();
                }
                else
                {
                    return log.OrderByDescending(t => t.Time).FirstOrDefault();
                }
            }
            return null;
        }

        public async Task<bool> PushUnknowErrorToClient(int lineIndex, string error, int companyIndex)
        {
            if (lineIndex == 0) return false;
            var param = new ExceptionErrorParam();
            param.CompanyIndex = companyIndex;
            param.LineIndex = lineIndex;
            param.Error = error;

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            client.BaseAddress = new Uri(_Config.RealTimeServerLink);
            var json = JsonConvert.SerializeObject(param);
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //var request = await client.PostAsync("/api/PushUnknowErrorToClient", content);
                var request = await client.PostAsync("/api/Monitoring/SendUnknowErrorData", content);
                request.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                _logger.LogError($"SendProgressData: {ex}");
                return false;
            }
            return true;
        }

        public async Task CreateAndSendMail(string pEmail, GC_TimeLog pTimeLog)
        {
            EASMailInfo mailInfo = new EASMailInfo();
            var timeLogs = new List<GC_TimeLog>();
            var timeLog = _GC_TimeLogService.FirstOrDefault(e => e.Index == pTimeLog.Index);
            if (timeLog != null)
            {
                timeLogs.Add(timeLog);
                mailInfo.StartDate = timeLog.Time;
                mailInfo.IsSendItNow = true;
                await SendMailForCurrentUser(pEmail, mailInfo, timeLogs);
            }
        }

        async Task SendMailForCurrentUser(string pEmail, EASMailInfo pMailInfo, IEnumerable<GC_TimeLog> pTimeLogs)
        {
            var filePath = await CreateExcelByTimeLogs(pTimeLogs);
            pMailInfo.FilePath = filePath;
            pMailInfo.ListEmail.Add(pEmail);

            SendMailReminder(pMailInfo);
        }

        private void SendMailReminder(EASMailInfo pMailInfo)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"StaticFiles/EmailTemplate";
            string title = System.IO.File.ReadAllText(path + "/RulesWarningTitle.txt");

            var curBody = System.IO.File.ReadAllText(path + "/RulesWarningBody.txt");
            curBody = curBody.Replace("<endDate>", pMailInfo.EndDate.ToString("dd/MM/yyyy HH:mm"));
            if (pMailInfo.IsSendItNow)
            {
                curBody = System.IO.File.ReadAllText(path + "/RulesWarningBody_SendItNow.txt");
                curBody = curBody.Replace("<startDate>", pMailInfo.StartDate.Value.ToString("dd/MM/yyyy HH:mm"));
            }
            else if (pMailInfo.IsSingleToDate)
            {
                curBody = curBody.Replace("<startDate>", "trước");
            }
            else
            {
                curBody = curBody.Replace("<startDate>", pMailInfo.StartDate.Value.ToString("dd/MM/yyyy HH:mm"));
            }
            var filePaths = new List<string>() { pMailInfo.FilePath };
            _EmailSender.SendEmailToMulti("", title, curBody, string.Join(',', pMailInfo.ListEmail), filePaths.ToArray());
        }

        private async Task<string> CreateExcelByTimeLogs(IEnumerable<GC_TimeLog> pTimeLogs)
        {
            try
            {

                XLWorkbook workbook = new XLWorkbook(); // Load existed AINF template
                var worksheet = workbook.AddWorksheet("DanhSachCacTruongHopCanhBao", 0);
                worksheet.RowHeight = 50;
                int startRow = 1;
                int No = 1;

                worksheet.Cell("A" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("A" + startRow).Value = "STT";

                worksheet.Cell("B" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("B" + startRow).Value = "Họ tên";

                worksheet.Cell("C" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("C" + startRow).Value = "Đối tượng";

                worksheet.Cell("D" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("D" + startRow).Value = "Mã chấm công";

                //worksheet.Cell("E" + startRow).DataType = XLDataType.Text;
                //worksheet.Cell("E" + startRow).Value = "CMND";

                worksheet.Cell("E" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("E" + startRow).Value = "Chức vụ";

                //worksheet.Cell("G" + startRow).DataType = XLDataType.Text;
                //worksheet.Cell("G" + startRow).Value = "Người liên hệ làm việc";



                worksheet.Cell("F" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("F" + startRow).Value = "Sự kiện cảnh báo";

                worksheet.Cell("G" + startRow).DataType = XLDataType.Text;
                worksheet.Cell("G" + startRow).Value = "Thời gian";
                startRow++;

                var timeLogs = new List<GC_TimeLog>();
                if (pTimeLogs != null && pTimeLogs.Count() > 0)
                {
                    timeLogs = pTimeLogs.ToList();
                }
                foreach (var timeLog in timeLogs)
                {
                    string fullName = "", employeeATID = "", nric = "", position = "", contactPerson = "";
                    if (timeLog.CustomerIndex == 0)
                    {
                        var currentUser = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(new List<string> { timeLog.EmployeeATID },
                            DateTime.Now, timeLog.CompanyIndex);
                        if (currentUser != null && currentUser.Count > 0)
                        {
                            employeeATID = timeLog.EmployeeATID;
                            fullName = currentUser[0].FullName;
                            position = currentUser[0].Position;
                        }
                    }
                    else
                    {
                        var customer = await _GC_CustomerService.GetDataByIndex(timeLog.CustomerIndex);
                        if (customer != null)
                        {
                            var contactUser = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(
                                new List<string> { customer.ContactPersonATIDs },
                                DateTime.Now, timeLog.CompanyIndex);
                            nric = customer.CustomerNRIC;
                            fullName = customer.CustomerName;
                            if (contactUser != null && contactUser.Count > 0)
                                contactPerson = contactUser[0].FullName;
                            if (customer.IsVip)
                            {
                                timeLog.ObjectAccessType = "VipCustomer";
                            }
                        }
                    }

                    worksheet.Cell("A" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("A" + startRow).Value = No;

                    worksheet.Cell("B" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("B" + startRow).Value = fullName;

                    worksheet.Cell("C" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("C" + startRow).Value = GetCheckedString(timeLog.CustomerIndex == 0 ? "Employee" : "Customer");

                    worksheet.Cell("D" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("D" + startRow).Value = "'" + employeeATID;

                    //worksheet.Cell("E" + startRow).DataType = XLDataType.Text;
                    //worksheet.Cell("E" + startRow).Value = nric;

                    worksheet.Cell("E" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("E" + startRow).Value = position;

                    //worksheet.Cell("G" + startRow).DataType = XLDataType.Text;
                    //worksheet.Cell("G" + startRow).Value = contactPerson;

                    worksheet.Cell("F" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("F" + startRow).Value = GetCheckedString(timeLog.Error);

                    worksheet.Cell("G" + startRow).DataType = XLDataType.Text;
                    worksheet.Cell("G" + startRow).Value = "'" + timeLog.Time.ToString("dd/MM/yyyy HH:mm");
                    //var image = GetStream("http://localhost:5000/Files/ImageFromCamera/7/101/2021-03-29/13/4211_9399.jpg");
                    //worksheet.AddPicture(image).MoveTo(worksheet.Cell("I" + startRow)).WithSize(50, 50);

                    No++;
                    startRow++;
                }

                using (MemoryStream stream = new MemoryStream())
                {

                    var path = "RulesWarningExcel/DanhSachCacTruongHopCanhBao/";
                    string filename = "DanhSachCacTruongHopCanhBao_" + DateTime.Now.ToFileTime() + ".xlsx";
                    workbook.SaveAs(stream);

                    var parentPath = _FileHandler.uploadAndUpdateUrlBase(stream.ToArray(), path, filename);
                    return parentPath;
                }


            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        protected string GetCheckedString(string key)
        {
            return i18n.GetCheckedStringWithLang(key, Language.VI);
        }
    }

    public class EASMailInfo
    {
        public bool IsSendItNow { get; set; }
        public bool IsSingleToDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string FilePath { get; set; }
        public List<string> ListEmail { get; set; }

        public EASMailInfo()
        {
            ListEmail = new List<string>();
        }
    }
}
