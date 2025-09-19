using EPAD_Backend_Core.Base;
using EPAD_Background.Schedule.Job;
using EPAD_Common.FileProvider;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.Other;
using EPAD_Data.Models.TimeLog;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GCS_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RealTimeController : ApiControllerBase
    {
        #region Properties
        private ILogger _logger;

        private readonly IGC_TimeLogService _GC_TimeLogService;
        private readonly IGC_TimeLog_ImageService _GC_TimeLog_ImageService;

        private readonly IGC_GatesService _GC_GatesService;
        private EPAD_Context context;
        private readonly IGC_Gates_LinesService _GC_Gates_LinesService;
        private readonly IGC_Lines_CheckInCameraService _GC_Lines_CheckInCameraService;
        private readonly IGC_Lines_CheckOutCameraService _GC_Lines_CheckOutCameraService;
        private readonly IGC_Lines_CheckInRelayControllerService _GC_Lines_CheckInRelayControllerService;
        private readonly IGC_Lines_CheckOutRelayControllerService _GC_Lines_CheckOutRelayControllerService;
        private readonly IGC_CustomerService _GC_CustomerService;
        private readonly IGC_Employee_AccessedGroupService _GC_Employee_AccessedGroupService;
        private readonly IGC_Rules_WarningService _GC_Rules_WarningService;
        private readonly IIC_ControllerService _IC_ControllerService;
        private IIC_ClientTCPControllerLogic _IIC_ClientTCPControllerLogic;
        private IIC_ModbusReplayControllerLogic _IIC_ModbusReplayControllerLogic;

        private string _linkControllerApi;
        #endregion

        public RealTimeController(IServiceProvider pServiceProvider, IIC_ClientTCPControllerLogic iIC_ClientTCPControllerLogic, IIC_ModbusReplayControllerLogic iIC_ModbusReplayControllerLogic,
            IConfiguration configuration, EPAD_Context pContext, ILoggerFactory _loggerFactory) : base(pServiceProvider)
        {
            _logger = _loggerFactory.CreateLogger<RealTimeController>();

            context = pContext;
            _GC_GatesService = TryResolve<IGC_GatesService>();
            _GC_TimeLogService = TryResolve<IGC_TimeLogService>();
            _GC_TimeLog_ImageService = TryResolve<IGC_TimeLog_ImageService>();
            _GC_Rules_WarningService = TryResolve<IGC_Rules_WarningService>();
            _GC_Lines_CheckInRelayControllerService = TryResolve<IGC_Lines_CheckInRelayControllerService>();
            _GC_Lines_CheckOutRelayControllerService = TryResolve<IGC_Lines_CheckOutRelayControllerService>();
            _GC_CustomerService = TryResolve<IGC_CustomerService>();
            _GC_Gates_LinesService = TryResolve<IGC_Gates_LinesService>();
            _GC_Lines_CheckInCameraService = TryResolve<IGC_Lines_CheckInCameraService>();
            _GC_Lines_CheckOutCameraService = TryResolve<IGC_Lines_CheckOutCameraService>();
            _GC_Employee_AccessedGroupService = TryResolve<IGC_Employee_AccessedGroupService>();
            _IC_ControllerService = TryResolve<IIC_ControllerService>();
            _IIC_ClientTCPControllerLogic = iIC_ClientTCPControllerLogic;
            _IIC_ModbusReplayControllerLogic = iIC_ModbusReplayControllerLogic;
            _linkControllerApi = configuration.GetValue<string>("ControllerApi");
        }
        [Authorize]
        [ActionName("ConnectToServer")]
        [HttpGet]
        public bool ConnectToServer()
        {
            return true;
        }

        [ActionName("SaveImageFromBase64")]
        [HttpPost]
        public async Task<IActionResult> SaveImageFromBase64(ImageParam image)
        {
            var user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var saveImageDirectory = AppDomain.CurrentDomain.BaseDirectory + "Files/RealtimeImage";

            try
            {
                byte[] imageBytes = Convert.FromBase64String(image.base64);

                // Create the directory if it doesn't exist
                if (!Directory.Exists(saveImageDirectory))
                {
                    Directory.CreateDirectory(saveImageDirectory);
                }

                string filePath = Path.Combine(saveImageDirectory, image.name + "." + image.type);
                System.IO.File.WriteAllBytes(filePath, imageBytes);

            }
            catch (Exception ex)
            {
            }

            return ApiOk();
        }


        [ActionName("UpdateLogStatus")]
        [HttpPost]
        public async Task<IActionResult> UpdateLogStatus([FromBody] LogParam param)
        {
            var user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            param.UserName = user.UserName;

            var log = _GC_TimeLogService.Where(t => t.Index == param.Index).FirstOrDefault();
            if (log == null && !param.IsException)
            {
                return ApiOk("LogNotExists");
            }

            // open controller
            bool openControllerSuccess = await OpenController(param.LineIndex, param.InOut, false, param.OpenController, user.CompanyIndex);
            if (openControllerSuccess == false)
            {
                return ApiOk("OpenBarrierFailed");
            }

            if (param.OpenController)
            {
                try
                {
                    if (log != null)
                    {
                        log.InOutMode = (short)param.InOut;
                        log.ApproveStatus = (short)ApproveStatus.Approved;
                        log.Note = param.Note;
                        log.UpdatedDate = DateTime.Now;
                        log.UpdatedUser = param.UserName;
                        log.IsException = param.IsException;
                        log.ReasonException = param.ExceptionReason;
                        await _GC_TimeLogService.SaveChangeAsync();
                    }
                    else if (log == null && param.IsException)
                    {
                        log = new GC_TimeLog();
                        log.EmployeeATID = param.EmployeeATID;
                        log.MachineSerial = string.Empty;
                        log.InOutMode = (short)param.InOut;
                        log.ApproveStatus = (short)ApproveStatus.Approved;
                        log.Note = param.Note;
                        log.UpdatedDate = DateTime.Now;
                        log.UpdatedUser = param.UserName;
                        log.IsException = param.IsException;
                        log.ReasonException = param.ExceptionReason;
                        log.LineIndex = param.LineIndex;
                        log.CompanyIndex = user.CompanyIndex;
                        log.Action = "ADD";
                        log.SystemTime = DateTime.Now;
                        log.Time = DateTime.Now;
                        log.ObjectAccessType = "Employee";
                        log.LogType = LogType.Walker.ToString();
                        log.Status = (short)EMonitoringError.NoError;
                        log.UpdatedDate = DateTime.Now;
                        log.UpdatedUser = user.FullName;
                        log.CardNumber = param.CardNumber;
                        await _GC_TimeLogService.AddTimeLog(log);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                await SendLogToClientAsync(log);
            }
            return ApiOk();
        }

        [ActionName("UpdateLogStatusAuto")]
        [HttpPost]
        public async Task<IActionResult> UpdateLogStatusAuto([FromBody] LogParam param)
        {
            var user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            param.UserName = user.UserName;

            var log = _GC_TimeLogService.Where(t => t.Index == param.Index).FirstOrDefault();
            if (log == null)
            {
                return ApiError("LogNotExists");
            }
            // open controller auto mode
            bool openControllerSuccess = await OpenController(param.LineIndex, param.InOut, true, true, user.CompanyIndex);
            if (openControllerSuccess == false)
            {
                return ApiError("OpenBarrierFailed");
            }

            log.ApproveStatus = (short)ApproveStatus.Approved;
            log.Note = param.Note;
            log.UpdatedDate = DateTime.Now;
            log.UpdatedUser = param.UserName;
            await _GC_TimeLogService.SaveChangeAsync();

            await SendLogToClientAsync(log);

            return ApiOk();
        }

        [HttpPost]
        [ActionName("CallControllerByParam")]
        public async Task<IActionResult> CallControllerByParam(CallControllerParam data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var serialNumber = "";
            var timeLog = _GC_TimeLogService.FirstOrDefault(e => e.Index == data.TimeLogIndex);
            if (timeLog != null)
                serialNumber = timeLog.MachineSerial;
            var list = await _GC_Rules_WarningService.GetRulesWarningControllerChannel(data.Id, user.CompanyIndex);
            list = list.Where(x => string.IsNullOrWhiteSpace(x.SerialNumber)
                || (!string.IsNullOrWhiteSpace(x.SerialNumber) && x.SerialNumber == serialNumber)).ToList();
            var group = list.GroupBy(e => e.ControllerIndex);
            //_logger.LogError("Start loop: " + list.Count + "rulesWarningIndex: " + data.Id + " timelogIndex: " + data.TimeLogIndex);
            //var dummy = new List<CotrollerWarningRequestModel>();
            //var checkDuplicateList = new List<CotrollerWarningRequestModel>();
            var isAllCallSuccess = true;
            foreach (var item in group)
            {
                //_logger.LogError("Start loop item: " + item.Key.ToString());
                var listLines = item.Select(e => e.ChannelIndex).Distinct().ToList();

                var success = await CallController(item.Key, listLines, true, true);
                //_logger.LogError("End loop item: " + success);
                if (!success)
                {
                    isAllCallSuccess = false;
                }
            }

            return ApiOk(isAllCallSuccess);
        }


        private async Task<bool> CallController(int controllerIndex, List<int> channelIndexs, bool autoOff, bool setOn)
        {
            var controllerParam = new RelayControllerParam();
            controllerParam.ControllerIndex = controllerIndex;
            controllerParam.ListChannel = channelIndexs;
            controllerParam.AutoOff = true;
            controllerParam.SetOn = true;
            bool openControllerSuccess = await _IC_ControllerService.SetOnAndAutoOffController(controllerParam);

            return openControllerSuccess;
        }

        private async Task<bool> OpenController(int lineIndex, int inOutMode, bool autoOff, bool setOn, int companyIndex)
        {
            string error = "";
            if (lineIndex == 0) return false;
            if (inOutMode == 1)
            {
                var listCheckInController = _GC_Lines_CheckInRelayControllerService
                        .Where(t => t.CompanyIndex == companyIndex && t.LineIndex == lineIndex).ToList();

                var controllerList = context.IC_RelayController.Where(t => listCheckInController.Select(x => x.RelayControllerIndex).Contains(t.Index)).ToList();

                var controllerChannelList = context.IC_RelayControllerChannel
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

                            var result = await _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannelOp,
                               secondAutoClose);
                            //var result = _IIC_ModbusReplayControllerLogic.OpenChannel(listChannel);
                            _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                            //_logger.LogError($"SetOnAndAutoOffController false: {controllerChannel?.NumberOfSecondsOff} {result}");
                        }
                        if (autoOff)
                        {
                            if (await _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(controller.IpAddress, Convert.ToUInt16(controller.Port)))
                            {
                                var listChannelOp = new List<ChannelParam>();
                                listChannelOp.Add(new ChannelParam() { Index = Convert.ToInt16(listCheckInController[i].FailAlarmChannelIndex) });

                                if (controllerChannel != null)
                                {
                                    var result = await _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannelOp,
                                       secondAutoClose);
                                    _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                                    //_logger.LogError($"SetOnAndAutoOffController true: {controllerChannel.NumberOfSecondsOff} {result}");
                                }
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

                var controllerList = context.IC_RelayController.Where(t => listCheckInController.Select(x => x.RelayControllerIndex).Contains(t.Index)).ToList();

                var controllerChannelList = context.IC_RelayControllerChannel
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

                            var result = await _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannelOp,
                               secondAutoClose);
                            //var result = _IIC_ModbusReplayControllerLogic.OpenChannel(listChannel);
                            _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                            //_logger.LogError($"SetOnAndAutoOffController false: {controllerChannel?.NumberOfSecondsOff} {result}");
                        }
                        if (autoOff)
                        {
                            if (await _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(controller.IpAddress, Convert.ToUInt16(controller.Port)))
                            {
                                var listChannelOp = new List<ChannelParam>();
                                listChannelOp.Add(new ChannelParam() { Index = Convert.ToInt16(listCheckInController[i].FailAlarmChannelIndex) });

                                if (controllerChannel != null)
                                {
                                    var result = await _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannelOp,
                                       secondAutoClose);
                                    _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                                    //_logger.LogError($"SetOnAndAutoOffController true: {controllerChannel.NumberOfSecondsOff} {result}");
                                }
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


        private string GetImageInRow(GC_TimeLog_Image pImageRow)
        {
            string value = "";
            if (pImageRow == null)
            {
                return value;
            }
            if (pImageRow.Image1 != "")
            {
                value = pImageRow.Image1;
            }
            else if (pImageRow.Image2 != "")
            {
                value = pImageRow.Image2;
            }
            else if (pImageRow.Image3 != "")
            {
                value = pImageRow.Image3;
            }
            else if (pImageRow.Image4 != "")
            {
                value = pImageRow.Image4;
            }
            else if (pImageRow.Image5 != "")
            {
                value = pImageRow.Image5;
            }
            return value;
        }

        protected async Task SendLogToClientAsync(GC_TimeLog log)
        {

            var preLog = _GC_TimeLogService.Where(e => e.EmployeeATID == log.EmployeeATID && e.Time != log.Time
                    && e.ApproveStatus == (short)ApproveStatus.Approved && e.CustomerIndex == log.CustomerIndex)
                    .OrderByDescending(e => e.Time)
                    .FirstOrDefault();
            if (preLog == null || (preLog != null && log.InOutMode.HasValue && preLog.InOutMode.HasValue && log.InOutMode != preLog.InOutMode))
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

    }
}
