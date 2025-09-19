
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Logic.MainProcess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using EPAD_Common.Extensions;
using EPAD_Common.Types;
using EPAD_Backend_Core.Base;
using System.Threading.Tasks;
using EPAD_Common.Utility;
using EPAD_Backend_Core.ApiClient;
using EPAD_Backend_Core.Models;
using Newtonsoft.Json;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data.Entities;
using EPAD_Services.FR05.Interface;
using Microsoft.EntityFrameworkCore;
using EPAD_Data.Models.TimeLog;
using EPAD_Data.Models.FR05;
using Microsoft.Extensions.Configuration;
using EPAD_Backend_Core.Provider;
using System.Net;
using System.Net.Http;
using static EPAD_Backend_Core.Provider.WebAPIUtility;
using EPAD_Services.Interface;
using EPAD_Services.Impl;
using EPAD_Data.Models.HR;
using DocumentFormat.OpenXml.Bibliography;
using NPOI.SS.Formula.Functions;
using EPAD_Data.Entities;
using ClosedXML.Excel;
using EPAD_Data.Models.IC;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Command/[action]")]
    [ApiController]
    public class IC_CommandController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private readonly ILogger _logger;
        private IIC_UserInfoLogic _iIC_UserInfoLogic;
        private IIC_ServiceAndDeviceLogic _iC_ServiceAndDeviceLogic;
        private IIC_SystemCommandLogic _iC_SystemCommandLogic;
        private IIC_UserMasterLogic _iC_UserMasterLogic;
        private IIC_CommandLogic _iC_CommandLogic;
        private IIC_AuditLogic _iIC_AuditLogic;
        private readonly PushNotificationClient _notificationClient;
        private readonly IFR05_ClientService _FR05_ClientService;
        private ConfigObject config;
        private FR05Config _FR05Config;
        private IIC_ServiceAndDevicesService _IIC_ServiceAndDevicesService;
        private ezHR_Context otherContext;
        private IAC_UserMasterService _IAC_UserMasterService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private IHR_UserService _IHR_UserService;

        public IC_CommandController(IServiceProvider provider, ILoggerFactory loggerFactory) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _logger = _LoggerFactory.CreateLogger<IC_CommandController>();
            _iIC_UserInfoLogic = TryResolve<IIC_UserInfoLogic>();
            _iC_ServiceAndDeviceLogic = TryResolve<IIC_ServiceAndDeviceLogic>();
            _iC_SystemCommandLogic = TryResolve<IIC_SystemCommandLogic>();
            _iC_UserMasterLogic = TryResolve<IIC_UserMasterLogic>();
            _iC_CommandLogic = TryResolve<IIC_CommandLogic>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _notificationClient = PushNotificationClient.GetInstance(cache);
            _FR05_ClientService = TryResolve<IFR05_ClientService>();
            config = ConfigObject.GetConfig(cache);
            _FR05Config = _Configuration.GetSection("FR05Config")
                            .Get<FR05Config>();
            _IIC_ServiceAndDevicesService = TryResolve<IIC_ServiceAndDevicesService>();
            otherContext = TryResolve<ezHR_Context>();
            _IAC_UserMasterService = TryResolve<IAC_UserMasterService>();
            _IHR_UserService = TryResolve<IHR_UserService>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
        }

        [Authorize]
        [ActionName("DownloadLogFromToTime")]
        [HttpPost]
        public IActionResult DownloadLogFromToTime([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListSerial == null || ((param.FromTime == null || param.ToTime == null) && param.IsDownloadFull == null))
            {
                return BadRequest("BadRequest");
            }
            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {

                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "SystemCommandStatus", Value = false });
                addedParams.Add(new AddedParam { Key = "CommandName", Value = param.IsDownloadFull.HasValue && param.IsDownloadFull.Value ? CommandAction.DownloadAllLog : CommandAction.DownloadLogFromToTime });
                addedParams.Add(new AddedParam { Key = "ListSerialNumber", Value = lsSerialHw });
                List<IC_SystemCommandDTO> listCommandHasExisted = _iC_SystemCommandLogic.GetMany(addedParams);
                if (listCommandHasExisted != null && listCommandHasExisted.Count > 0)
                {
                    var deviceSerialHasDuplicated = listCommandHasExisted.Select(x => x.SerialNumber).ToList();
                    var deviceNames = context.IC_Device.Where(x => deviceSerialHasDuplicated.Contains(x.SerialNumber)).Select(x => x.AliasName).ToList();
                    return BadRequest($"Lệnh đã tồn tại và chưa thực thi trên thiết bị {string.Join(", ", deviceNames)}. Mã lệnh: {string.Join(", ", listCommandHasExisted.Select(x => x.Index).ToList())}");
                }
                if (lsSerialHw.Count > 0)
                {
                    List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                    List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, param.IsDownloadFull.HasValue && param.IsDownloadFull.Value ? CommandAction.DownloadAllLog : CommandAction.DownloadLogFromToTime, param.FromTime.Value, param.ToTime.Value, lstUser, param.IsOverwriteData, 0);
                    CreateGroupCommand(user.CompanyIndex, user.UserName, param.IsDownloadFull.HasValue && param.IsDownloadFull.Value ? CommandAction.DownloadAllLog.ToString() : CommandAction.DownloadLogFromToTime.ToString(), lstCmd, "");

                    string listDeviceStr = string.Join(", ", param.ListSerial);
                    for (int i = 0; i < lstCmd.Count; i++)
                    {
                        string description, descriptionEn;
                        AuditType auditType;
                        if (param.IsDownloadFull.HasValue && param.IsDownloadFull.Value)
                        {
                            description = $"Tải dữ tất cả liệu điểm danh trên máy {lstCmd[i].SerialNumber}";
                            descriptionEn = $"Download all attendance log on machine {lstCmd[i].SerialNumber}";
                            auditType = AuditType.DownloadAllLog;
                        }
                        else
                        {
                            description = $"Tải dữ liệu điểm danh trên máy {lstCmd[i].SerialNumber} từ {param.FromTime.Value:dd/MM/yyyy} đến {param.ToTime.Value:dd/MM/yyyy}";
                            descriptionEn = $"Download attendance log on machine {lstCmd[i].SerialNumber} from {param.FromTime.Value:dd/MM/yyyy} to {param.ToTime.Value:dd/MM/yyyy}";
                            auditType = AuditType.DownloadLogFromToTime;
                        }

                        var audit = new IC_AuditEntryDTO(null)
                        {
                            TableName = "IC_SystemCommand",
                            UserName = user.UserName,
                            CompanyIndex = user.CompanyIndex,
                            State = auditType,
                            Description = description,
                            DescriptionEn = descriptionEn,
                            IC_SystemCommandIndex = lstCmd[i].ID != null ? int.Parse(lstCmd[i].ID) : null,
                            Name = user.FullName,
                            PageName = "DeviceInfo",
                            Status = AuditStatus.Unexecuted,
                            DateTime = DateTime.Now
                        };
                        _iIC_AuditLogic.Create(audit);
                    }
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }
            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("GetFR05SystemCommandNeedExecute")]
        [HttpPost]
        public async Task<IActionResult> GetFR05SystemCommandNeedExecute()
        {
            ConfigObject config = ConfigObject.GetConfig(cache);
            var checkTimeLimit = ConfigObject.GetConfig(cache).LimitedTimeConnection;
            List<string> lsSerialNumber = new List<string>();
            var FR05Machine = context.IC_Device.AsNoTracking().Where(x => x.CompanyIndex == 2 &&
               !string.IsNullOrEmpty(x.DeviceModel) && Convert.ToInt32(x.DeviceModel) == (int)ProducerEnum.FR05

            ).ToList();
            FR05Machine = FR05Machine.Where(x => CaculateTime(x.LastConnection, DateTime.Now) < checkTimeLimit).ToList();

            var now = DateTime.Now.AddMinutes(-5);

            if (FR05Machine.Count > 0)
            {
                lsSerialNumber.AddRange(FR05Machine.Select(x => x.SerialNumber));

                var commandsNeedExecute = context.IC_SystemCommand
                       .Where(x => (x.Excuted == false || x.ExcutedTime == null)
                           && x.IsActive
                           && (lsSerialNumber == null || lsSerialNumber.Contains(x.SerialNumber))
                           && ((x.UpdatedUser == "SYSTEM_AUTO") || x.RequestedTime < now)
                           )

                       .Select(cmd => new CommandResult
                       {
                           ID = cmd.Index.ToString(),
                           SerialNumber = cmd.SerialNumber,
                           Command = cmd.Command,
                           CreatedTime = cmd.CreatedDate ?? DateTime.Now,
                           ListUsers = JsonConvert.DeserializeObject<CommandParamDB>(cmd.Params).ListUsers,
                           Error = cmd.Error,
                           ExcutingServiceIndex = cmd.ExcutingServiceIndex,
                           GroupIndex = cmd.GroupIndex + "",
                           Status = "UnExecute",
                           IsOverwriteData = cmd.IsOverwriteData,
                           FromTime = JsonConvert.DeserializeObject<CommandParamDB>(cmd.Params).FromTime,
                           ToTime = JsonConvert.DeserializeObject<CommandParamDB>(cmd.Params).ToTime,
                       })
                       .ToList();

                for (int i = 0; i < commandsNeedExecute.Count; i++)
                {
                    var cmd = commandsNeedExecute[i];
                    var device = FR05Machine.FirstOrDefault(x => x.SerialNumber == cmd.SerialNumber);
                    cmd.Port = device?.Port ?? 0;
                    cmd.IPAddress = device?.IPAddress;
                    if (!string.IsNullOrEmpty(device?.DeviceModel))
                    {
                        int.TryParse(device.DeviceModel, out int deviceModel);
                        cmd.DeviceModel = deviceModel;
                    }
                }
                UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
                CompanyInfo companyInfo = CompanyInfo.GetFromCache(cache, user.CompanyIndex.ToString());
                foreach (var command in commandsNeedExecute)
                {
                    await _FR05_ClientService.ProcessCommand(command, user, config, companyInfo, true);
                }
            }


            return Ok();
        }


        [Authorize]
        [ActionName("DeleteLogFromToTime")]
        [HttpPost]
        public IActionResult DeleteLogFromToTime([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListSerial == null || param.FromTime == null || param.ToTime == null)
            {
                return BadRequest("BadRequest");
            }

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "SystemCommandStatus", Value = false });
                addedParams.Add(new AddedParam { Key = "CommandName", Value = param.IsDeleteAll.HasValue && param.IsDeleteAll.Value ? CommandAction.DeleteAllLog : CommandAction.DeleteLogFromToTime });
                addedParams.Add(new AddedParam { Key = "ListSerialNumber", Value = lsSerialHw });
                List<IC_SystemCommandDTO> listCommandHasExisted = _iC_SystemCommandLogic.GetMany(addedParams);
                if (listCommandHasExisted != null && listCommandHasExisted.Count > 0)
                {
                    var deviceSerialHasDuplicated = listCommandHasExisted.Select(x => x.SerialNumber).ToList();
                    var deviceNames = context.IC_Device.Where(x => deviceSerialHasDuplicated.Contains(x.SerialNumber)).Select(x => x.AliasName).ToList();
                    return BadRequest($"Lệnh đã tồn tại và chưa thực thi trên thiết bị {string.Join(", ", deviceNames)}. Mã lệnh: {string.Join(", ", listCommandHasExisted.Select(x => x.Index).ToList())}");
                }
                if (lsSerialHw.Count > 0)
                {
                    List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                    var lstCmd = new List<CommandResult>();
                    if (param.IsDeleteAll.HasValue && param.IsDeleteAll.Value)
                    {
                        lstCmd = CreateListCommand(context, lsSerialHw, CommandAction.DeleteAllLog, param.FromTime.Value, param.ToTime.Value, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                        CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.DeleteAllLog.ToString(), lstCmd, "");
                    }
                    else
                    {
                        lstCmd = CreateListCommand(context, lsSerialHw, CommandAction.DeleteLogFromToTime, param.FromTime.Value, param.ToTime.Value, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                        CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.DeleteLogFromToTime.ToString(), lstCmd, "");
                    }

                    lstCmd.ForEach(cmd =>
                    {
                        // Add audit log
                        IC_AuditEntryDTO audit = new(null)
                        {
                            TableName = "IC_SystemCommand",
                            UserName = user.UserName,
                            CompanyIndex = user.CompanyIndex,
                            State = AuditType.Deleted,
                            DateTime = DateTime.Now,
                            Name = user.FullName,
                            IC_SystemCommandIndex = cmd.ID != null ? int.Parse(cmd.ID) : null,
                            PageName = "",
                            Status = AuditStatus.Unexecuted,
                        };
                        if (param.IsDeleteAll.HasValue && param.IsDeleteAll.Value)
                        {
                            audit.Description = $"Xóa tất cả log điểm danh";
                            audit.DescriptionEn = $"Delete all attendance log";
                        }
                        else
                        {
                            audit.Description = $"Xóa log từ ngày {param.FromTime?.ToString("dd-MM-yyyy") ?? ""} tới ngày {param.ToTime?.ToString("dd-MM-yyyy") ?? ""}";
                            audit.DescriptionEn = $"Delete log from {param.FromTime?.ToString("yyyy-MM-dd") ?? ""} to {param.ToTime?.ToString("yyyy-MM-dd") ?? ""}";
                        }
                        _iIC_AuditLogic.Create(audit);
                    });
                }
            }
            else
            {
                IC_AuditEntryDTO audit = new(null)
                {
                    TableName = "IC_SystemCommand",
                    UserName = user.UserName,
                    CompanyIndex = user.CompanyIndex,
                    State = AuditType.Deleted,
                    DateTime = DateTime.Now,
                    Name = user.FullName,
                    Status = AuditStatus.Error
                };
                if (param.IsDeleteAll.HasValue && param.IsDeleteAll.Value)
                {
                    audit.Description = $"Xóa tất cả log điểm danh xảy ra lỗi: Thiết bị không có bản quyền";
                    audit.DescriptionEn = $"Delete all attendance log occur error: Device don't have license";
                }
                else
                {
                    audit.Description = $"Xóa log từ ngày {param.FromTime?.ToString("dd-MM-yyyy") ?? ""} tới ngày {param.ToTime?.ToString("dd-MM-yyyy") ?? ""} xảy ra lỗi: Thiết bị không có bản quyền";
                    audit.DescriptionEn = $"Delete log from {param.FromTime?.ToString("yyyy-MM-dd") ?? ""} to {param.ToTime?.ToString("yyyy-MM-dd") ?? ""} occur error: Device don't have license";
                }
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("SetDeviceTime")]
        [HttpPost]
        public IActionResult SetDeviceTime([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListSerial == null || param.ListSerial.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            var lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                var addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "SystemCommandStatus", Value = false });
                addedParams.Add(new AddedParam { Key = "CommandName", Value = CommandAction.SetTimeDevice });
                addedParams.Add(new AddedParam { Key = "ListSerialNumber", Value = lsSerialHw });
                addedParams.Add(new AddedParam { Key = "IsActive", Value = true });

                var listCommandHasExisted = _iC_SystemCommandLogic.GetMany(addedParams);

                if (listCommandHasExisted != null && listCommandHasExisted.Count > 0)
                {
                    //lsSerialHw = lsSerialHw.Where(u => listCommandHasExisted.Where(t => t.SerialNumber == u).Count() == 0).ToList();
                    var deviceSerialHasDuplicated = listCommandHasExisted.Select(x => x.SerialNumber).ToList();
                    var deviceNames = context.IC_Device.Where(x => deviceSerialHasDuplicated.Contains(x.SerialNumber)).Select(x => x.AliasName).ToList();

                    return BadRequest($"Lệnh đã tồn tại và chưa thực thi trên thiết bị {string.Join(", ", deviceNames)}. Mã lệnh: {string.Join(", ", listCommandHasExisted.Select(x => x.Index).ToList())}");
                }
                if (lsSerialHw.Count > 0)
                {
                    var lstUser = new List<UserInfoOnMachine>();
                    var lstCmd = CreateListCommand(context, lsSerialHw, CommandAction.SetTimeDevice, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);

                    CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.SetTimeDevice.ToString(), lstCmd, "");

                    lstCmd.ForEach(cmd =>
                    {
                        // Add audit log
                        IC_AuditEntryDTO audit = new(null)
                        {
                            TableName = "IC_SystemCommand",
                            UserName = user.UserName,
                            CompanyIndex = user.CompanyIndex,
                            State = AuditType.SetDeviceTime,
                            Description = $"Cập nhật thời gian thiết bị: {string.Join(", ", param.ListSerial)}",
                            DescriptionEn = $"Set device time: {string.Join(", ", param.ListSerial)}",
                            DateTime = DateTime.Now,
                            IC_SystemCommandIndex = cmd.ID != null ? int.Parse(cmd.ID) : null,
                            PageName = "DeviceInfo",
                            Status = AuditStatus.Unexecuted,
                        };

                        _iIC_AuditLogic.Create(audit);
                    });

                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return Ok();
        }

        [Authorize]
        [ActionName("DownloadAllUser")]
        [HttpPost]
        public IActionResult DownloadAllUser([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListSerial == null || param.ListSerial.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            List<string> lsSerialHw = new List<string>();
            ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0)
            {


                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "SystemCommandStatus", Value = false });
                addedParams.Add(new AddedParam { Key = "CommandName", Value = CommandAction.DownloadAllUser });
                addedParams.Add(new AddedParam { Key = "ListSerialNumber", Value = lsSerialHw });
                List<IC_SystemCommandDTO> listCommandHasExisted = _iC_SystemCommandLogic.GetMany(addedParams);
                if (listCommandHasExisted != null && listCommandHasExisted.Count > 0)
                {

                    var deviceSerialHasDuplicated = listCommandHasExisted.Select(x => x.SerialNumber).ToList();
                    var deviceNames = context.IC_Device.Where(x => deviceSerialHasDuplicated.Contains(x.SerialNumber)).Select(x => x.AliasName).ToList();
                    return BadRequest($"Lệnh đã tồn tại và chưa thực thi trên thiết bị {string.Join(", ", deviceNames)}. Mã lệnh: {string.Join(", ", listCommandHasExisted.Select(x => x.Index).ToList())}");
                }
                if (lsSerialHw.Count > 0)
                {
                    List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                    List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, CommandAction.DownloadAllUser, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.DownloadAllUser.ToString(), lstCmd, "");
                    // Delete first
                    _iIC_UserInfoLogic.Delete(param.ListSerial, user.CompanyIndex);

                    // Add audit log
                    IC_AuditEntryDTO audit = new(null)
                    {
                        TableName = "IC_SystemCommand",
                        UserName = user.UserName,
                        CompanyIndex = user.CompanyIndex,
                        State = AuditType.DownloadAllUser,
                        Description = "Tải tất cả thông tin nhân viên",
                        DescriptionEn = "Download all user",
                        DateTime = DateTime.Now,
                        Status = AuditStatus.Completed,
                        Name = user.FullName
                    };
                    _iIC_AuditLogic.Create(audit);
                }
            }
            else
            {
                IC_AuditEntryDTO audit = new(null)
                {
                    UserName = user.UserName,
                    TableName = "IC_SystemCommand",
                    CompanyIndex = user.CompanyIndex,
                    State = AuditType.DownloadUserById,
                    Description = "Tải tất cả thông tin nhân viên xảy ra lỗi: Thiết bị không có bản quyền",
                    DescriptionEn = "Download all user occur error: Device don't have license",
                    DateTime = DateTime.Now,
                    Status = AuditStatus.Error,
                    Name = user.FullName
                };
                _iIC_AuditLogic.Create(audit);
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("UploadTimeZone")]
        [HttpPost]
        public IActionResult UploadTimeZone([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            if (param.ListSerial == null || param.ListSerial.Count == 0)
            {
                if (param.IsUsingArea == true && param.AreaLst != null && param.AreaLst.Count > 0)
                {
                    var lstDoor = _DbContext.AC_AreaAndDoor.Where(x => param.AreaLst.Contains(x.AreaIndex)).Select(x => x.DoorIndex).ToList();
                    param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => lstDoor.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
                }
                else
                {
                    if (param.DoorLst == null || param.DoorLst.Count == 0)
                    {
                        param.ListSerial = _DbContext.AC_DoorAndDevice.Select(x => x.SerialNumber).Distinct()
                            .ToList();

                    }
                    else
                    {
                        param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => param.DoorLst.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();

                    }
                }
            }
            if (param.ListSerial.Count == 0)
            {
                return Ok();
            }

            List<string> lsSerialHw = new List<string>();
            ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0)
            {

                if (lsSerialHw.Count > 0)
                {
                    var lstUser = new List<UserInfoOnMachine>();
                    var lstHolidays = new List<AC_AccHoliday>();
                    var lstGroups = new List<AC_AccGroup>();
                    var lstTimezones = new List<AC_TimeZone>();
                    if (param.TimeZone == null || param.TimeZone.Count == 0)
                    {
                        lstTimezones = _DbContext.AC_TimeZone.AsNoTracking().ToList();
                    }
                    else
                    {
                        string timeZone = param.TimeZone.FirstOrDefault();
                        lstTimezones = _DbContext.AC_TimeZone.AsNoTracking().Where(x => x.UID == int.Parse(timeZone)).ToList();
                    }

                    List<CommandResult> lstCmd = CreateListACCommand(context, lsSerialHw, CommandAction.UploadTimeZone, new DateTime(2000, 1, 1), DateTime.Now, lstUser, lstGroups, lstHolidays, lstTimezones, param, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.UploadTimeZone.ToString(), lstCmd, "");

                    // Add audit log
                    IC_AuditEntryDTO audit = new(null)
                    {
                        TableName = "IC_SystemCommand",
                        UserName = user.UserName,
                        CompanyIndex = user.CompanyIndex,
                        State = AuditType.UploadTimeZone,
                        Description = "Cập nhật Timezone",
                        DescriptionEn = "Update Timezone",
                        DateTime = DateTime.Now,
                        Status = AuditStatus.Completed,
                        Name = user.FullName
                    };
                    _iIC_AuditLogic.Create(audit);
                }
            }
            else
            {
                IC_AuditEntryDTO audit = new(null)
                {
                    UserName = user.UserName,
                    TableName = "IC_SystemCommand",
                    CompanyIndex = user.CompanyIndex,
                    State = AuditType.UploadTimeZone,
                    Description = "Cập nhật Timezones xảy ra lỗi: Thiết bị không có bản quyền",
                    DescriptionEn = "update timezones occur error: Device don't have license",
                    DateTime = DateTime.Now,
                    Status = AuditStatus.Error,
                    Name = user.FullName
                };
                _iIC_AuditLogic.Create(audit);
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("DeleteTimezoneById")]
        [HttpPost]
        public IActionResult DeleteTimezoneById([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            if (param.IsUsingArea == true && param.AreaLst != null && param.AreaLst.Count > 0)
            {
                var lstDoor = _DbContext.AC_AreaAndDoor.Where(x => param.AreaLst.Contains(x.AreaIndex)).Select(x => x.DoorIndex).ToList();
                param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => lstDoor.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
            }
            else
            {
                if (param.DoorLst == null || param.DoorLst.Count == 0)
                {
                    param.ListSerial = _DbContext.AC_DoorAndDevice.Select(x => x.SerialNumber).Distinct()
                        .ToList();

                }
                else
                {
                    param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => param.DoorLst.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();

                }
            }
            List<string> lsSerialHw = new List<string>();
            ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0)
            {

                if (lsSerialHw.Count > 0)
                {
                    var lstUser = new List<UserInfoOnMachine>();
                    var lstHolidays = new List<AC_AccHoliday>();
                    var lstGroups = new List<AC_AccGroup>();
                    var lstTimezones = new List<AC_TimeZone>();
                    lstTimezones = param.TimeZone.Select(x => new AC_TimeZone { UIDIndex = x }).ToList();

                    List<CommandResult> lstCmd = CreateListACCommand(context, lsSerialHw, CommandAction.DeleteTimezoneById, new DateTime(2000, 1, 1), DateTime.Now, lstUser, lstGroups, lstHolidays, lstTimezones, param, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.DeleteTimezoneById.ToString(), lstCmd, "");

                    // Add audit log
                    IC_AuditEntryDTO audit = new(null)
                    {
                        TableName = "IC_SystemCommand",
                        UserName = user.UserName,
                        CompanyIndex = user.CompanyIndex,
                        State = AuditType.DeleteTimezoneById,
                        Description = "Xóa Timezone",
                        DescriptionEn = "Delete Timezone",
                        DateTime = DateTime.Now,
                        Status = AuditStatus.Completed,
                        Name = user.FullName
                    };
                    _iIC_AuditLogic.Create(audit);
                }
            }
            else
            {
                IC_AuditEntryDTO audit = new(null)
                {
                    UserName = user.UserName,
                    TableName = "IC_SystemCommand",
                    CompanyIndex = user.CompanyIndex,
                    State = AuditType.DeleteTimezoneById,
                    Description = "Cập nhật Timezones xảy ra lỗi: Thiết bị không có bản quyền",
                    DescriptionEn = "update timezones occur error: Device don't have license",
                    DateTime = DateTime.Now,
                    Status = AuditStatus.Error,
                    Name = user.FullName
                };
                _iIC_AuditLogic.Create(audit);
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("UploadAccHoliday")]
        [HttpPost]
        public IActionResult UploadAccHoliday([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }


            var lstHolidays = new List<AC_AccHoliday>();
            var timezones = new List<AC_TimeZone>();

            if (param.DoorLst != null && param.DoorLst.Count > 0)
            {
                lstHolidays = _DbContext.AC_AccHoliday.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex && param.DoorLst.Contains(x.DoorIndex)).ToList();
            }
            else
            {
                lstHolidays = _DbContext.AC_AccHoliday.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex && x.DoorIndex != 0).ToList();

            }
            lstHolidays = lstHolidays.Where(x => x.Loop || x.StartDate.Date >= DateTime.Now.Date).ToList();
            timezones = _DbContext.AC_TimeZone.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
            foreach (var item in lstHolidays)
            {
                if (item.TimeZone != 0)
                {
                    var timezone = timezones.FirstOrDefault(x => x.UID == item.TimeZone)?.UIDIndex;
                    if (!string.IsNullOrEmpty(timezone))
                    {
                        item.TimeZone = Convert.ToInt32(timezone);
                    }

                }
            }
            if (lstHolidays != null && lstHolidays.Count > 0)
            {
                var groupHolidays = lstHolidays.GroupBy(x => x.DoorIndex).Select(x => new { Key = x.Key, Value = x.ToList() }).ToList();
                foreach (var item in groupHolidays)
                {
                    var ListSerial = _DbContext.AC_DoorAndDevice.Where(x => x.DoorIndex == item.Key).Select(x => x.SerialNumber).ToList();

                    var lsSerialHw = new List<string>();
                    ListSerialCheckHardWareLicense(ListSerial, ref lsSerialHw);
                    if (lsSerialHw != null && lsSerialHw.Count > 0)
                    {

                        if (lsSerialHw.Count > 0)
                        {
                            var lstUser = new List<UserInfoOnMachine>();

                            var lstGroups = new List<AC_AccGroup>();
                            var lstTimezones = new List<AC_TimeZone>();
                            List<CommandResult> lstCmd = CreateListACCommand(context, lsSerialHw, CommandAction.UploadAccHoliday, new DateTime(2000, 1, 1), DateTime.Now, lstUser, lstGroups, item.Value, lstTimezones, param, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                            CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.UploadAccHoliday.ToString(), lstCmd, "");

                            // Add audit log
                            IC_AuditEntryDTO audit = new(null)
                            {
                                TableName = "IC_SystemCommand",
                                UserName = user.UserName,
                                CompanyIndex = user.CompanyIndex,
                                State = AuditType.UploadAccHoliday,
                                Description = "Cập nhật kì nghỉ",
                                DescriptionEn = "Update Holiday",
                                DateTime = DateTime.Now,
                                Status = AuditStatus.Completed,
                                Name = user.FullName
                            };
                            _iIC_AuditLogic.Create(audit);
                        }
                    }
                    else
                    {
                        IC_AuditEntryDTO audit = new(null)
                        {
                            UserName = user.UserName,
                            TableName = "IC_SystemCommand",
                            CompanyIndex = user.CompanyIndex,
                            State = AuditType.UploadTimeZone,
                            Description = "Cập nhật Kì nghỉ xảy ra lỗi: Thiết bị không có bản quyền",
                            DescriptionEn = "update Holiday occur error: Device don't have license",
                            DateTime = DateTime.Now,
                            Status = AuditStatus.Error,
                            Name = user.FullName
                        };
                        _iIC_AuditLogic.Create(audit);
                        return BadRequest("DeviceNotLicence");
                    }
                }
            }
            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("UploadAccGroup")]
        [HttpPost]
        public IActionResult UploadAccGroup([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListSerial == null || param.ListSerial.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            List<string> lsSerialHw = new List<string>();
            ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0)
            {

                if (lsSerialHw.Count > 0)
                {
                    var lstUser = new List<UserInfoOnMachine>();
                    var lstHolidays = new List<AC_AccHoliday>();
                    var lstGroups = _DbContext.AC_AccGroup.ToList();
                    var lstTimezones = new List<AC_TimeZone>();
                    List<CommandResult> lstCmd = CreateListACCommand(context, lsSerialHw, CommandAction.UploadAccGroup, new DateTime(2000, 1, 1), DateTime.Now, lstUser, lstGroups, lstHolidays, lstTimezones, param, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.UploadAccGroup.ToString(), lstCmd, "");

                    // Add audit log
                    IC_AuditEntryDTO audit = new(null)
                    {
                        TableName = "IC_SystemCommand",
                        UserName = user.UserName,
                        CompanyIndex = user.CompanyIndex,
                        State = AuditType.UploadAccGroup,
                        Description = "Cập nhật nhóm",
                        DescriptionEn = "Update Group",
                        DateTime = DateTime.Now,
                        Status = AuditStatus.Completed,
                        Name = user.FullName
                    };
                    _iIC_AuditLogic.Create(audit);
                }
            }
            else
            {
                IC_AuditEntryDTO audit = new(null)
                {
                    UserName = user.UserName,
                    TableName = "IC_SystemCommand",
                    CompanyIndex = user.CompanyIndex,
                    State = AuditType.UploadAccGroup,
                    Description = "Cập nhật Nhóm xảy ra lỗi: Thiết bị không có bản quyền",
                    DescriptionEn = "update Group occur error: Device don't have license",
                    DateTime = DateTime.Now,
                    Status = AuditStatus.Error,
                    Name = user.FullName
                };
                _iIC_AuditLogic.Create(audit);
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }


        [Authorize]
        [ActionName("DownloadAllUserMaster")]
        [HttpPost]
        public IActionResult DownloadAllUserMaster([FromBody] DownloadUserMasterRequest param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.SerialNumbers == null || param.SerialNumbers.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            List<string> lsSerialHw = new List<string>();
            ListSerialCheckHardWareLicense(param.SerialNumbers, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0)
            {
                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "SystemCommandStatus", Value = false });
                addedParams.Add(new AddedParam { Key = "CommandName", Value = CommandAction.DownloadAllUserMaster });
                addedParams.Add(new AddedParam { Key = "ListSerialNumber", Value = lsSerialHw });
                List<IC_SystemCommandDTO> listCommandHasExisted = _iC_SystemCommandLogic.GetMany(addedParams);
                if (listCommandHasExisted != null && listCommandHasExisted.Count > 0)
                {

                    var deviceSerialHasDuplicated = listCommandHasExisted.Select(x => x.SerialNumber).ToList();
                    var deviceNames = context.IC_Device.Where(x => deviceSerialHasDuplicated.Contains(x.SerialNumber)).Select(x => x.AliasName).ToList();
                    return BadRequest($"Lệnh đã tồn tại và chưa thực thi trên thiết bị {string.Join(", ", deviceNames)}. Mã lệnh: {string.Join(", ", listCommandHasExisted.Select(x => x.Index).ToList())}");
                }
                if (lsSerialHw.Count > 0)
                {
                    try
                    {
                        List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(new IC_CommandParamDTO
                        {
                            Action = CommandAction.DownloadAllUserMaster,
                            IsOverwriteData = param.IsOverwriteData,
                            ListEmployee = new List<UserInfoOnMachine>(),
                            ListSerialNumber = param.SerialNumbers,
                        });

                        if (lstCmd?.Count > 0)
                        {
                            var groupCommand = new IC_GroupCommandParamDTO
                            {
                                CompanyIndex = user.CompanyIndex,
                                UserName = user.UserName,
                                ListCommand = lstCmd,
                                GroupName = GroupName.DownloadAllUserMaster.ToString(),
                                EventType = "",
                                ExternalData = JsonConvert.SerializeObject(new
                                {
                                    AuthModes = string.Join(", ", param.AuthModes),
                                    TargetUser = param.TargetDownloadUser.ToString(),
                                }),
                            };
                            _iC_CommandLogic.CreateGroupCommands(groupCommand);
                            for (int i = 0; i < lstCmd.Count; i++)
                            {
                                // Add audit log
                                IC_AuditEntryDTO audit = new(null)
                                {
                                    TableName = "IC_SystemCommand",
                                    UserName = user.UserName,
                                    CompanyIndex = user.CompanyIndex,
                                    State = AuditType.DownloadAllUserMaster,
                                    DateTime = DateTime.Now,
                                    Name = user.FullName,
                                    IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                    PageName = "AutoSynchUser",
                                    Status = AuditStatus.Unexecuted,
                                };
                                _iIC_AuditLogic.Create(audit);
                            }

                        }
                        CheckFR05(user.CompanyIndex, lstCmd).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error at DownloadAllUserMaster:  " + ex.ToString());
                        return StatusCode(500, ex.ToString());
                    }
                }
            }
            else
            {
                IC_AuditEntryDTO audit = new(null)
                {
                    TableName = "IC_SystemCommand",
                    UserName = user.UserName,
                    CompanyIndex = user.CompanyIndex,
                    State = AuditType.DownloadAllUserMaster,
                    Description = "Tải tất cả thông tin nhân viên xảy ra lỗi: Thiết bị không có bản quyền",
                    DescriptionEn = "Download all user master occur error: Device don't have license",
                    DateTime = DateTime.Now,
                    Name = user.FullName,
                    Status = AuditStatus.Error
                };
                _iIC_AuditLogic.Create(audit);
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.SerialNumbers);
            return result;
        }

        [Authorize]
        [ActionName("DownloadUserById")]
        [HttpPost]
        public IActionResult DownloadUserById([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListSerial == null || param.ListSerial.Count == 0 || param.ListUser == null || param.ListUser.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListUser, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = param.ListUser;
                paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                paramUserOnMachine.AuthenMode = "";
                paramUserOnMachine.FullInfo = false;
                List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, CommandAction.DownloadUserById, DateTime.Now, DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.DownloadUserById.ToString(), lstCmd, "");

                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "IC_SystemCommand";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Command DownloadUserById ";
                audit.Description = AuditType.Added.ToString() + "Command:/:DownloadUserById";
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }
        [Authorize]
        [ActionName("DownloadUserMasterById")]
        [HttpPost]
        public IActionResult DownloadUserMasterById([FromBody] DownloadUserMasterRequest param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.SerialNumbers == null || param.SerialNumbers.Count == 0 || param.EmployeeATIDs == null || param.EmployeeATIDs.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.SerialNumbers, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = param.EmployeeATIDs;
                paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                paramUserOnMachine.AuthenMode = "";
                paramUserOnMachine.FullInfo = false;
                List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, CommandAction.DownloadUserMasterById, DateTime.Now, DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                CreateGroupCommand(
                    user.CompanyIndex,
                    user.UserName,
                    CommandAction.DownloadUserMasterById.ToString(),
                    lstCmd,
                    pEventType: "",
                    externalData: JsonConvert.SerializeObject(new
                    {
                        AuthModes = string.Join(", ", param.AuthModes),
                        TargetUser = param.TargetDownloadUser.ToString(),
                    })
                );

                foreach (var cmd in lstCmd)
                {
                    // Add audit log
                    IC_AuditEntryDTO audit = new(null)
                    {
                        TableName = "IC_SystemCommand",
                        UserName = user.UserName,
                        CompanyIndex = user.CompanyIndex,
                        State = AuditType.DownloadUserMasterById,
                        DateTime = DateTime.Now,
                        IC_SystemCommandIndex = int.Parse(cmd.ID),
                        PageName = "AutoSynchUser",
                        Status = AuditStatus.Unexecuted,
                    };

                    _iIC_AuditLogic.Create(audit);
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.SerialNumbers);
            return result;
        }

        [Authorize]
        [ActionName("DeleteAllUser")]
        [HttpPost]
        public IActionResult DeleteAllUser([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListSerial == null)
            {
                return BadRequest("BadRequest");
            }

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                List<CommandResult> lstCmd = CreateListCommand(
                    context,
                    lsSerialHw,
                    pAction: CommandAction.DeleteAllUser,
                    pFromTime: new DateTime(2000, 1, 1),
                    pToTime: DateTime.Now,
                    pListUsers: lstUser,
                    param.IsOverwriteData,
                    GlobalParams.DevicePrivilege.SDKStandardRole,
                    externalData: JsonConvert.SerializeObject(new
                    {
                        AuthenModes = param.AuthenMode
                    }));
                CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.DeleteAllUser.ToString(), lstCmd, "");
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "IC_SystemCommand";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Command DeleteAllUser ";
                audit.Description = AuditType.Added.ToString() + "Command:/:DeleteAllUser";
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("DeleteAllHoliday")]
        [HttpPost]
        public IActionResult DeleteAllHoliday([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListSerial == null)
            {
                return BadRequest("BadRequest");
            }
            param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => param.DoorLst.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                List<CommandResult> lstCmd = CreateListCommand(
                    context,
                    lsSerialHw,
                    pAction: CommandAction.DeleteAllHoliday,
                    pFromTime: new DateTime(2000, 1, 1),
                    pToTime: DateTime.Now,
                    pListUsers: lstUser,
                    param.IsOverwriteData,
                    GlobalParams.DevicePrivilege.SDKStandardRole,
                    externalData: JsonConvert.SerializeObject(new
                    {
                        AuthenModes = param.AuthenMode
                    }));
                CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.DeleteAllHoliday.ToString(), lstCmd, "");
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "IC_SystemCommand";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                audit.Description = AuditType.Added.ToString() + "Command:/:DeleteAllHoliday";
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }



        [Authorize]
        [ActionName("DeleteAllFingerPrint")]
        [HttpPost]
        public IActionResult DeleteAllFingerPrint([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListSerial == null || param.ListSerial.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, CommandAction.DeleteAllFingerPrint, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.DeleteAllFingerPrint.ToString(), lstCmd, "");
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "IC_SystemCommand";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Command DeleteAllFingerPrint ";
                audit.Description = AuditType.Added.ToString() + "Command:/:DeleteAllFingerPrint";
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("DeleteUserById")]
        [HttpPost]
        public IActionResult DeleteUserById([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListSerial == null || param.ListSerial.Count == 0 || param.ListUser == null || param.ListUser.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = param.ListUser;
                paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                paramUserOnMachine.AuthenMode = "";
                paramUserOnMachine.FullInfo = false;
                List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                List<CommandResult> lstCmd = CreateListCommand(
                    context,
                    lsSerialHw,
                    pAction: CommandAction.DeleteUserById,
                    pFromTime: new DateTime(2000, 1, 1),
                    pToTime: DateTime.Now,
                    pListUsers: lstUser,
                    param.IsOverwriteData,
                    privilege: GlobalParams.DevicePrivilege.SDKStandardRole,
                    externalData: JsonConvert.SerializeObject(new
                    {
                        AuthenModes = param.AuthenMode
                    }));
                CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.DeleteUserById.ToString(), lstCmd, "");
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "IC_SystemCommand";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Command DeleteUserById ";
                audit.Description = AuditType.Added.ToString() + "Command:/:DeleteUserById";
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("DeleteUserByIdFromUserId")]
        [HttpPost]
        public IActionResult DeleteUserByIdFromUserId([FromBody] string[] listEmployeeam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (listEmployeeam == null || listEmployeeam.Count() == 0)
            {
                return BadRequest("BadRequest");
            }

            List<string> lsSerialHw = new List<string>();
            var listSerial = _DbContext.IC_Device.Where(x => x.CompanyIndex == user.CompanyIndex).Select(x => x.SerialNumber).ToList();
            bool checkHw = ListSerialCheckHardWareLicense(listSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = listEmployeeam.ToList();
                paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                paramUserOnMachine.AuthenMode = "";
                paramUserOnMachine.FullInfo = false;
                List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                List<CommandResult> lstCmd = CreateListCommand(
                    context,
                    lsSerialHw,
                    pAction: CommandAction.DeleteUserById,
                    pFromTime: new DateTime(2000, 1, 1),
                    pToTime: DateTime.Now,
                    pListUsers: lstUser,
                    true,
                    privilege: GlobalParams.DevicePrivilege.SDKStandardRole,
                    externalData: JsonConvert.SerializeObject(new
                    {
                        AuthenModes = new List<string>()
                    }));
                CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.DeleteUserById.ToString(), lstCmd, "");
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "IC_SystemCommand";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Command DeleteUserById ";
                audit.Description = AuditType.Added.ToString() + "Command:/:DeleteUserById";
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(listSerial);
            return result;
        }

        [Authorize]
        [ActionName("UploadUsers")]
        [HttpPost]
        public async Task<IActionResult> UploadUsers([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListDepartment != null && param.ListDepartment.Count > 0)
            {

                var userListByDepartment = await _IHR_UserService.GetEmployeeByDepartmentIds(param.ListDepartment, user.CompanyIndex);

                var groupIndexList = _DbContext.AC_AccGroup.Where(x => param.DoorLst.Contains(x.DoorIndex)).ToList();

                var userAccess = _DbContext.AC_AccessedGroup.Where(x => userListByDepartment.Select(x => x.EmployeeATID).Contains(x.EmployeeATID) && groupIndexList.Select(x => x.UID).Contains(x.GroupIndex)).ToList();

                param.ListUser = userListByDepartment.Where(x => !userAccess.Select(z => z.EmployeeATID).Contains(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
            }
            if (param.DoorLst == null && param.AreaLst == null)
            {
                if (param.ListSerial == null || param.ListSerial.Count == 0 || param.ListUser == null || param.ListUser.Count == 0)
                {
                    return BadRequest("BadRequest");
                }
            }
            else
            {
                if (param.IsUsingArea == true)
                {
                    var lstDoor = _DbContext.AC_AreaAndDoor.Where(x => param.AreaLst.Contains(x.AreaIndex)).Select(x => x.DoorIndex).ToList();
                    param.DoorLst = lstDoor;
                    param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => lstDoor.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
                }
                else
                {
                    param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => param.DoorLst.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
                }
            }

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);

            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                //var listDevice = _IIC_ServiceAndDevicesService.GetAllBySerialNumbers(lsSerialHw.ToArray(), user.CompanyIndex);
                //List<AddedParam> addedParams = new List<AddedParam>();
                //addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                //addedParams.Add(new AddedParam { Key = "SystemCommandStatus", Value = false });
                //addedParams.Add(new AddedParam { Key = "CommandName", Value = StringHelper.GetCommandType(param.EmployeeType) });
                //addedParams.Add(new AddedParam { Key = "ListSerialNumber", Value = lsSerialHw });
                //List<IC_SystemCommandDTO> listCommandHasExisted = _iC_SystemCommandLogic.GetMany(addedParams);
                //if (listCommandHasExisted != null && listCommandHasExisted.Count > 0)
                //{
                //    lsSerialHw = lsSerialHw.Where(u => listCommandHasExisted.Where(t => t.SerialNumber == u).Count() == 0).ToList();
                //}

                if (lsSerialHw.Count > 0)
                {
                    IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = param.ListUser;
                    paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                    paramUserOnMachine.AuthenMode = string.Join(",", param.AuthenMode);
                    paramUserOnMachine.ListSerialNumber = param.ListSerial;
                    paramUserOnMachine.FullInfo = true;

                    List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                    IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                    commandParam.IsOverwriteData = false;
                    commandParam.Action = CommandAction.UploadUsers;
                    commandParam.CommandName = StringHelper.GetCommandType(param.EmployeeType);
                    commandParam.AuthenMode = string.Join(",", param.AuthenMode);
                    commandParam.FromTime = new DateTime(2000, 1, 1);
                    commandParam.ToTime = DateTime.Now;
                    commandParam.ListEmployee = lstUser;
                    commandParam.ListSerialNumber = lsSerialHw;
                    commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                    commandParam.ExternalData = JsonConvert.SerializeObject(new
                    {
                        AuthenModes = param.AuthenMode
                    });

                    List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                    if (lstCmd != null && lstCmd.Count() > 0)
                    {
                        IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                        groupCommand.CompanyIndex = user.CompanyIndex;
                        groupCommand.UserName = user.UserName;
                        groupCommand.ListCommand = lstCmd;
                        groupCommand.GroupName = GroupName.UploadUsers.ToString();
                        groupCommand.EventType = "";
                        _iC_CommandLogic.CreateGroupCommands(groupCommand);
                        for (int i = 0; i < lstCmd.Count; i++)
                        {
                            CommandResult command = lstCmd[i];
                            // Add audit log
                            var audit = new IC_AuditEntryDTO(null)
                            {
                                TableName = "IC_SystemCommand",
                                UserName = user.UserName,
                                CompanyIndex = user.CompanyIndex,
                                State = AuditType.UploadUsers,
                                Description = $"Upload {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                DescriptionEn = $"Upload {lstCmd[i].ListUsers?.Count ?? 0} users",
                                IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                Name = user.FullName,
                                PageName = "AutoSynchUser",
                                Status = AuditStatus.Unexecuted,
                                DateTime = DateTime.Now
                            };
                            _iIC_AuditLogic.Create(audit);
                        }
                    }
                    //CheckFR05(user.CompanyIndex, lstCmd).GetAwaiter().GetResult();
                    //List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, GlobalParams.CommandAction.UploadUsers, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    //CreateGroupCommand(user.CompanyIndex, user.UserName, GlobalParams.CommandAction.UploadUsers.ToString(), lstCmd, "");
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("UploadACUsers")]
        [HttpPost]
        public async Task<IActionResult> UploadACUsers([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            if (param.ListDepartment != null && param.ListDepartment.Count > 0)
            {
                var userListByDepartment = await _IHR_UserService.GetEmployeeByDepartmentIds(param.ListDepartment, user.CompanyIndex);

                var groupIndexList = _DbContext.AC_AccGroup.Where(x => param.DoorLst.Contains(x.DoorIndex)).ToList();

                var userAccess = _DbContext.AC_AccessedGroup.Where(x => userListByDepartment.Select(x => x.EmployeeATID).Contains(x.EmployeeATID) && groupIndexList.Select(x => x.UID).Contains(x.GroupIndex)).ToList();

                param.ListUser = userListByDepartment.Where(x => !userAccess.Select(z => z.EmployeeATID).Contains(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
            }

            if (param.ListUser == null || param.ListUser.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            if (param.IsUsingArea == true)
            {
                var lstDoor = _DbContext.AC_AreaAndDoor.Where(x => param.AreaLst.Contains(x.AreaIndex)).Select(x => x.DoorIndex).ToList();
                param.DoorLst = lstDoor;
                param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => lstDoor.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
            }
            else
            {
                param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => param.DoorLst.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
            }

            if (param.ListSerial.Count == 0)
            {
                return Ok();
            }

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            param.AuthenMode = new List<string> { "CardNumber" };
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                if (lsSerialHw.Count > 0)
                {

                    IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = param.ListUser;
                    paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                    paramUserOnMachine.AuthenMode = string.Join(",", param.AuthenMode);
                    paramUserOnMachine.ListSerialNumber = param.ListSerial;
                    paramUserOnMachine.FullInfo = true;

                    List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                    IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                    commandParam.IsOverwriteData = false;
                    commandParam.Action = CommandAction.UploadACUsers;
                    commandParam.CommandName = StringHelper.GetCommandType(param.EmployeeType);
                    commandParam.AuthenMode = string.Join(",", param.AuthenMode);
                    commandParam.FromTime = new DateTime(2000, 1, 1);
                    commandParam.ToTime = DateTime.Now;
                    commandParam.ListEmployee = lstUser;
                    commandParam.ListSerialNumber = lsSerialHw;
                    commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                    commandParam.ExternalData = JsonConvert.SerializeObject(new
                    {
                        AuthenModes = param.AuthenMode
                    });

                    List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                    string timeZone = param.TimeZone.FirstOrDefault();
                    var timezoneDb = _DbContext.AC_TimeZone.FirstOrDefault(x => x.UID == int.Parse(timeZone));
                    //var lstTimezone = timezoneDb.UIDIndex.Split(',').ToList();

                    //string group = param.Group.ToString();


                    //var timeZoneIns = "0001";
                    //foreach (var t in lstTimezone)
                    //{
                    //    timeZoneIns += t.PadLeft(4, '0');
                    //}


                    foreach (var item in lstCmd)
                    {
                        item.TimeZone = timezoneDb.UIDIndex;
                        item.Group = "1";
                    }


                    if (lstCmd != null && lstCmd.Count() > 0)
                    {
                        IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                        groupCommand.CompanyIndex = user.CompanyIndex;
                        groupCommand.UserName = user.UserName;
                        groupCommand.ListCommand = lstCmd;
                        groupCommand.GroupName = GroupName.UploadACUsers.ToString();
                        groupCommand.EventType = "";
                        _iC_CommandLogic.CreateGroupCommands(groupCommand);
                        for (int i = 0; i < lstCmd.Count; i++)
                        {
                            CommandResult command = lstCmd[i];
                            // Add audit log
                            var audit = new IC_AuditEntryDTO(null)
                            {
                                TableName = "IC_SystemCommand",
                                UserName = user.UserName,
                                CompanyIndex = user.CompanyIndex,
                                State = AuditType.UploadACUsers,
                                Description = $"Upload {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                DescriptionEn = $"Upload {lstCmd[i].ListUsers?.Count ?? 0} users",
                                IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                Name = user.FullName,
                                PageName = "AutoSynchUser",
                                Status = AuditStatus.Unexecuted,
                                DateTime = DateTime.Now
                            };
                            _iIC_AuditLogic.Create(audit);
                        }
                    }

                    _IAC_UserMasterService.AddUserMasterToHistory(param.ListUser, param.DoorLst, user, int.Parse(timeZone));
                    //CheckFR05(user.CompanyIndex, lstCmd).GetAwaiter().GetResult();
                    //List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, GlobalParams.CommandAction.UploadUsers, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    //CreateGroupCommand(user.CompanyIndex, user.UserName, GlobalParams.CommandAction.UploadUsers.ToString(), lstCmd, "");
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("UploadACUsersWhenUpdateDoor")]
        [HttpPost]
        public async Task<IActionResult> UploadACUsersWhenUpdateDoor([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            var group = _DbContext.AC_AccGroup.FirstOrDefault(x => x.UID == param.Group);
            if (group == null)
            {
                return BadRequest("BadRequest");
            }
            else
            {
                if (param.ListSerial == null || param.ListSerial.Count == 0)
                {
                    param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => group.DoorIndex == x.DoorIndex).Select(x => x.SerialNumber).ToList();
                }

                param.TimeZone = new List<string> { group.Timezone.ToString() };
                param.ListUser = _DbContext.AC_AccessedGroup.Where(x => x.GroupIndex == param.Group).Select(x => x.EmployeeATID).ToList();

                var userInDepartmentAcc = _DbContext.AC_DepartmentAccessedGroup.Where(x => x.GroupIndex == param.Group).ToList();
                var userListByDepartment = await _IHR_UserService.GetEmployeeByDepartmentIds(userInDepartmentAcc.Select(x => x.DepartmentIndex.ToString()).ToList(), user.CompanyIndex);
                var groupByDoor = _DbContext.AC_AccGroup.Where(x => x.DoorIndex == group.DoorIndex).ToList();
                var employeeAcc = _DbContext.AC_AccessedGroup.Where(x => groupByDoor.Select(z => z.UID).Contains(x.GroupIndex)).Select(x => x.EmployeeATID).ToList();
                var userAccessList = userListByDepartment.Where(x => !employeeAcc.Contains(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
                param.ListUser.AddRange(userAccessList);
            }

            if (param.ListUser == null || param.ListUser.Count == 0)
            {
                return Ok();
            }
            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            param.AuthenMode = new List<string> { "CardNumber" };
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {


                if (lsSerialHw.Count > 0)
                {

                    IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = param.ListUser;
                    paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                    paramUserOnMachine.AuthenMode = string.Join(",", param.AuthenMode);
                    paramUserOnMachine.ListSerialNumber = param.ListSerial;
                    paramUserOnMachine.FullInfo = true;

                    List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                    IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                    commandParam.IsOverwriteData = false;
                    commandParam.Action = CommandAction.UploadACUsers;
                    commandParam.CommandName = StringHelper.GetCommandType(param.EmployeeType);
                    commandParam.AuthenMode = string.Join(",", param.AuthenMode);
                    commandParam.FromTime = new DateTime(2000, 1, 1);
                    commandParam.ToTime = DateTime.Now;
                    commandParam.ListEmployee = lstUser;
                    commandParam.ListSerialNumber = lsSerialHw;
                    commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                    commandParam.ExternalData = JsonConvert.SerializeObject(new
                    {
                        AuthenModes = param.AuthenMode
                    });

                    List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                    string timeZone = param.TimeZone.FirstOrDefault();
                    var timezoneDb = _DbContext.AC_TimeZone.FirstOrDefault(x => x.UID == int.Parse(timeZone));

                    foreach (var item in lstCmd)
                    {
                        item.TimeZone = timezoneDb.UIDIndex;
                        item.Group = "1";
                    }


                    if (lstCmd != null && lstCmd.Count() > 0)
                    {
                        IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                        groupCommand.CompanyIndex = user.CompanyIndex;
                        groupCommand.UserName = user.UserName;
                        groupCommand.ListCommand = lstCmd;
                        groupCommand.GroupName = GroupName.UploadACUsers.ToString();
                        groupCommand.EventType = "";
                        _iC_CommandLogic.CreateGroupCommands(groupCommand);
                        for (int i = 0; i < lstCmd.Count; i++)
                        {
                            CommandResult command = lstCmd[i];
                            // Add audit log
                            var audit = new IC_AuditEntryDTO(null)
                            {
                                TableName = "IC_SystemCommand",
                                UserName = user.UserName,
                                CompanyIndex = user.CompanyIndex,
                                State = AuditType.UploadACUsers,
                                Description = $"Upload {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                DescriptionEn = $"Upload {lstCmd[i].ListUsers?.Count ?? 0} users",
                                IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                Name = user.FullName,
                                PageName = "AutoSynchUser",
                                Status = AuditStatus.Unexecuted,
                                DateTime = DateTime.Now
                            };
                            _iIC_AuditLogic.Create(audit);
                        }
                    }

                    _IAC_UserMasterService.AddUserMasterToHistory(param.ListUser, param.DoorLst, user, int.Parse(timeZone));
                    //CheckFR05(user.CompanyIndex, lstCmd).GetAwaiter().GetResult();
                    //List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, GlobalParams.CommandAction.UploadUsers, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    //CreateGroupCommand(user.CompanyIndex, user.UserName, GlobalParams.CommandAction.UploadUsers.ToString(), lstCmd, "");
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("UploadACUsersWhenUpdateDoorBySerial")]
        [HttpPost]
        public async Task<IActionResult> UploadACUsersWhenUpdateDoorBySerial([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            var lstGroup = _DbContext.AC_AccGroup.Where(x => param.DoorLst.Contains(x.DoorIndex)).ToList();
            foreach (var group in lstGroup)
            {
                if (group == null)
                {
                    return BadRequest("BadRequest");
                }
                else
                {
                    if (param.ListSerial == null || param.ListSerial.Count == 0)
                    {
                        param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => group.DoorIndex == x.DoorIndex).Select(x => x.SerialNumber).ToList();
                    }
                    param.TimeZone = new List<string> { group.Timezone.ToString() };
                    param.ListUser = _DbContext.AC_AccessedGroup.Where(x => x.GroupIndex == group.UID).Select(x => x.EmployeeATID).ToList();

                    var userInDepartmentAcc = _DbContext.AC_DepartmentAccessedGroup.Where(x => x.GroupIndex == group.UID).ToList();

                    var userListByDepartment = await _IHR_UserService.GetEmployeeByDepartmentIds(userInDepartmentAcc.Select(x => x.DepartmentIndex.ToString()).ToList(), user.CompanyIndex);

                    //var groupByDoor = _DbContext.AC_AccGroup.Where(x => x.DoorIndex == group.DoorIndex).ToList();
                    var employeeAcc = _DbContext.AC_AccessedGroup.Where(x => lstGroup.Select(z => z.UID).Contains(x.GroupIndex)).Select(x => x.EmployeeATID).ToList();
                    var userAccessList = userListByDepartment.Where(x => !employeeAcc.Contains(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
                    //var userAccessList = userListByDepartment.Where(x => !param.ListUser.Contains(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();

                    param.ListUser.AddRange(userAccessList);
                }

                if (param.ListUser == null || param.ListUser.Count == 0)
                {
                    return Ok();
                }
                List<string> lsSerialHw = new List<string>();
                bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
                param.AuthenMode = new List<string> { "CardNumber" };
                if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
                {


                    if (lsSerialHw.Count > 0)
                    {

                        IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                        paramUserOnMachine.ListEmployeeaATID = param.ListUser;
                        paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                        paramUserOnMachine.AuthenMode = string.Join(",", param.AuthenMode);
                        paramUserOnMachine.ListSerialNumber = param.ListSerial;
                        paramUserOnMachine.FullInfo = true;

                        List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                        IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                        commandParam.IsOverwriteData = false;
                        commandParam.Action = CommandAction.UploadACUsers;
                        commandParam.CommandName = StringHelper.GetCommandType(param.EmployeeType);
                        commandParam.AuthenMode = string.Join(",", param.AuthenMode);
                        commandParam.FromTime = new DateTime(2000, 1, 1);
                        commandParam.ToTime = DateTime.Now;
                        commandParam.ListEmployee = lstUser;
                        commandParam.ListSerialNumber = lsSerialHw;
                        commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                        commandParam.ExternalData = JsonConvert.SerializeObject(new
                        {
                            AuthenModes = param.AuthenMode
                        });

                        List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                        string timeZone = param.TimeZone.FirstOrDefault();
                        var timezoneDb = _DbContext.AC_TimeZone.FirstOrDefault(x => x.UID == int.Parse(timeZone));

                        foreach (var item in lstCmd)
                        {
                            item.TimeZone = timezoneDb.UIDIndex;
                            item.Group = "1";
                        }


                        if (lstCmd != null && lstCmd.Count() > 0)
                        {
                            IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                            groupCommand.CompanyIndex = user.CompanyIndex;
                            groupCommand.UserName = user.UserName;
                            groupCommand.ListCommand = lstCmd;
                            groupCommand.GroupName = GroupName.UploadACUsers.ToString();
                            groupCommand.EventType = "";
                            _iC_CommandLogic.CreateGroupCommands(groupCommand);
                            for (int i = 0; i < lstCmd.Count; i++)
                            {
                                CommandResult command = lstCmd[i];
                                // Add audit log
                                var audit = new IC_AuditEntryDTO(null)
                                {
                                    TableName = "IC_SystemCommand",
                                    UserName = user.UserName,
                                    CompanyIndex = user.CompanyIndex,
                                    State = AuditType.UploadACUsers,
                                    Description = $"Upload {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                    DescriptionEn = $"Upload {lstCmd[i].ListUsers?.Count ?? 0} users",
                                    IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                    Name = user.FullName,
                                    PageName = "AutoSynchUser",
                                    Status = AuditStatus.Unexecuted,
                                    DateTime = DateTime.Now
                                };
                                _iIC_AuditLogic.Create(audit);
                            }
                        }





                        if (lsSerialHw.Count > 0)
                        {
                            var lstAuthen = new List<string> { "CardNumber" };
                            paramUserOnMachine = new IC_UserinfoOnMachineParam();
                            paramUserOnMachine.ListEmployeeaATID = param.ListUser;
                            paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                            paramUserOnMachine.AuthenMode = string.Join(",", param.AuthenMode);
                            paramUserOnMachine.ListSerialNumber = param.ListSerial;
                            paramUserOnMachine.FullInfo = true;

                            lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                            commandParam = new IC_CommandParamDTO();
                            commandParam.IsOverwriteData = false;
                            commandParam.Action = CommandAction.UploadUsers;
                            commandParam.CommandName = StringHelper.GetCommandType(EmployeeType.Employee.ToInt());
                            commandParam.AuthenMode = string.Join(",", lstAuthen);
                            commandParam.FromTime = new DateTime(2000, 1, 1);
                            commandParam.ToTime = DateTime.Now;
                            commandParam.ListEmployee = lstUser;
                            commandParam.ListSerialNumber = lsSerialHw;
                            commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            commandParam.ExternalData = JsonConvert.SerializeObject(new
                            {
                                AuthenModes = new List<string>()
                            });

                            lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                            if (lstCmd != null && lstCmd.Count() > 0)
                            {
                                IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                                groupCommand.CompanyIndex = user.CompanyIndex;
                                groupCommand.UserName = user.UserName;
                                groupCommand.ListCommand = lstCmd;
                                groupCommand.GroupName = GroupName.UploadUsers.ToString();
                                groupCommand.EventType = "";
                                _iC_CommandLogic.CreateGroupCommands(groupCommand);
                                for (int i = 0; i < lstCmd.Count; i++)
                                {
                                    CommandResult command = lstCmd[i];
                                    // Add audit log
                                    var audit1 = new IC_AuditEntryDTO(null)
                                    {
                                        TableName = "IC_SystemCommand",
                                        UserName = user.UserName,
                                        CompanyIndex = user.CompanyIndex,
                                        State = AuditType.UploadUsers,
                                        Description = $"Upload {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                        DescriptionEn = $"Upload {lstCmd[i].ListUsers?.Count ?? 0} users",
                                        IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                        Name = user.FullName,
                                        PageName = "AutoSynchUser",
                                        Status = AuditStatus.Unexecuted,
                                        DateTime = DateTime.Now
                                    };
                                    _iIC_AuditLogic.Create(audit1);
                                }
                            }
                        }





                        _IAC_UserMasterService.AddUserMasterToHistory(param.ListUser, param.DoorLst, user, int.Parse(timeZone));
                        //CheckFR05(user.CompanyIndex, lstCmd).GetAwaiter().GetResult();
                        //List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, GlobalParams.CommandAction.UploadUsers, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                        //CreateGroupCommand(user.CompanyIndex, user.UserName, GlobalParams.CommandAction.UploadUsers.ToString(), lstCmd, "");
                    }
                }

            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("UploadACUserFromExcel")]
        [HttpPost]
        public IActionResult UploadACUserFromExcel(List<AC_UserImportDTO> lstImport)
        {
            try
            {
                UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
                ConfigObject config = ConfigObject.GetConfig(cache);
                IActionResult result = Unauthorized();
                if (user == null)
                {
                    return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
                }
                // validation data
                var listError = new List<AC_UserImportDTO>();

                var lstArea = _DbContext.AC_Area.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
                var lstDoor = _DbContext.AC_Door.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
                var lstTimezone = _DbContext.AC_TimeZone.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
                var lstAreaDoor = _DbContext.AC_AreaAndDoor.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
                var lstDoorDevice = _DbContext.AC_DoorAndDevice.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
                listError = ValidationImportACUser(lstImport, user, lstTimezone, lstArea, lstDoor, lstAreaDoor, lstDoorDevice);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/DepartmentImportError.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/DepartmentImportError.xlsx"));
                if (listError != null && listError.Count() > 0)
                {
                    var listDepartmentCodeError = listError.Select(e => e.EmployeeATID).ToList();
                    lstImport = lstImport.Where(e => !listDepartmentCodeError.Contains(e.EmployeeATID)).ToList();

                }

                lstImport = lstImport.Where(x => x.ListSerial != null && x.ListSerial.Count > 0).ToList();

                var lstDevice = lstImport.Select(x => x.ListSerial).SelectMany(l => l).Distinct().ToList();

                foreach (var item in lstDevice)
                {
                    var lstDev = new List<string>() { item };
                    var lstAuthen = new List<string> { "CardNumber" };
                    List<string> lsSerialHw = new List<string>();
                    bool checkHw = ListSerialCheckHardWareLicense(lstDev, ref lsSerialHw);
                    var lstUsers = lstImport.Where(x => x.ListSerial.Contains(item)).ToList();
                    if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
                    {


                        if (lsSerialHw.Count > 0)
                        {

                            IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                            paramUserOnMachine.ListEmployeeaATID = lstUsers.Select(x => x.EmployeeATID).ToList();
                            paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                            paramUserOnMachine.AuthenMode = string.Join(",", lstAuthen);
                            paramUserOnMachine.ListSerialNumber = lstDev;
                            paramUserOnMachine.FullInfo = true;

                            var lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                            foreach (var item1 in lstUser)
                            {
                                item1.GroupID = "1";
                                var userGroup = lstImport.FirstOrDefault(x => x.EmployeeATID == item1.EmployeeATID);
                                item1.TimeZone = userGroup.TimezoneUID;
                            }

                            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                            commandParam.IsOverwriteData = false;
                            commandParam.Action = CommandAction.UploadACUsersFromExcel;
                            commandParam.CommandName = StringHelper.GetCommandType(EmployeeType.Employee.ToInt());
                            commandParam.AuthenMode = string.Join(",", lstAuthen);
                            commandParam.FromTime = new DateTime(2000, 1, 1);
                            commandParam.ToTime = DateTime.Now;
                            commandParam.ListEmployee = lstUser;
                            commandParam.ListSerialNumber = lsSerialHw;
                            commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            commandParam.ExternalData = JsonConvert.SerializeObject(new
                            {
                                AuthenModes = lstAuthen
                            });

                            List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                            if (lstCmd != null && lstCmd.Count() > 0)
                            {
                                IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                                groupCommand.CompanyIndex = user.CompanyIndex;
                                groupCommand.UserName = user.UserName;
                                groupCommand.ListCommand = lstCmd;
                                groupCommand.GroupName = GroupName.UploadACUsersFromExcel.ToString();
                                groupCommand.EventType = "";
                                _iC_CommandLogic.CreateGroupCommands(groupCommand);
                                for (int i = 0; i < lstCmd.Count; i++)
                                {
                                    CommandResult command = lstCmd[i];
                                    // Add audit log
                                    var audit1 = new IC_AuditEntryDTO(null)
                                    {
                                        TableName = "IC_SystemCommand",
                                        UserName = user.UserName,
                                        CompanyIndex = user.CompanyIndex,
                                        State = AuditType.UploadACUsersFromExcel,
                                        Description = $"Upload {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                        DescriptionEn = $"Upload {lstCmd[i].ListUsers?.Count ?? 0} users",
                                        IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                        Name = user.FullName,
                                        PageName = "AutoSynchUser",
                                        Status = AuditStatus.Unexecuted,
                                        DateTime = DateTime.Now
                                    };
                                    _iIC_AuditLogic.Create(audit1);
                                }
                            }
                        }

                        _IAC_UserMasterService.AddUserMasterToHistoryFromExcel(lstImport, user);

                    }

                    if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw && lstUsers.Any(x => x.IsIntegrateToMachine))
                    {


                        if (lsSerialHw.Count > 0)
                        {
                            IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                            paramUserOnMachine.ListEmployeeaATID = lstUsers.Where(x => x.IsIntegrateToMachine).Select(x => x.EmployeeATID).ToList();
                            paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                            paramUserOnMachine.AuthenMode = null;
                            paramUserOnMachine.ListSerialNumber = lstDev;
                            paramUserOnMachine.FullInfo = true;

                            var lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                            foreach (var item1 in lstUser)
                            {
                                item1.GroupID = "1";
                                var userGroup = lstImport.FirstOrDefault(x => x.EmployeeATID == item1.EmployeeATID);
                                item1.TimeZone = userGroup.TimezoneUID;
                            }

                            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                            commandParam.IsOverwriteData = false;
                            commandParam.Action = CommandAction.UploadUsers;
                            commandParam.CommandName = StringHelper.GetCommandType(EmployeeType.Employee.ToInt());
                            commandParam.AuthenMode = string.Join(",", lstAuthen);
                            commandParam.FromTime = new DateTime(2000, 1, 1);
                            commandParam.ToTime = DateTime.Now;
                            commandParam.ListEmployee = lstUser;
                            commandParam.ListSerialNumber = lsSerialHw;
                            commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            commandParam.ExternalData = JsonConvert.SerializeObject(new
                            {
                                AuthenModes = new List<string>()
                            });

                            List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                            if (lstCmd != null && lstCmd.Count() > 0)
                            {
                                IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                                groupCommand.CompanyIndex = user.CompanyIndex;
                                groupCommand.UserName = user.UserName;
                                groupCommand.ListCommand = lstCmd;
                                groupCommand.GroupName = GroupName.UploadUsers.ToString();
                                groupCommand.EventType = "";
                                _iC_CommandLogic.CreateGroupCommands(groupCommand);
                                for (int i = 0; i < lstCmd.Count; i++)
                                {
                                    CommandResult command = lstCmd[i];
                                    // Add audit log
                                    var audit1 = new IC_AuditEntryDTO(null)
                                    {
                                        TableName = "IC_SystemCommand",
                                        UserName = user.UserName,
                                        CompanyIndex = user.CompanyIndex,
                                        State = AuditType.UploadUsers,
                                        Description = $"Upload {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                        DescriptionEn = $"Upload {lstCmd[i].ListUsers?.Count ?? 0} users",
                                        IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                        Name = user.FullName,
                                        PageName = "AutoSynchUser",
                                        Status = AuditStatus.Unexecuted,
                                        DateTime = DateTime.Now
                                    };
                                    _iIC_AuditLogic.Create(audit1);
                                }
                            }
                        }


                    }
                }

                if (lstImport.Count > 0 || (listError != null && listError.Count() > 0))
                {

                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("EmployeeSyncError");
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                        worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                        worksheet.Cell(currentRow, 3).Value = "Tên nhân viên";
                        worksheet.Cell(currentRow, 4).Value = "Phòng ban";
                        worksheet.Cell(currentRow, 5).Value = "Timezone (*)";
                        worksheet.Cell(currentRow, 6).Value = "Khu vực kiểm soát";
                        worksheet.Cell(currentRow, 7).Value = "Cửa kiểm soát";
                        worksheet.Cell(currentRow, 8).Value = "Lỗi";

                        for (int i = 1; i < 9; i++)
                        {
                            worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Column(i).Width = 20;
                        }

                        foreach (var department in listError)
                        {
                            currentRow++;
                            //New template
                            worksheet.Cell(currentRow, 1).Value = "'" + department.EmployeeATID;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 2).Value = department.EmployeeCode;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 3).Value = department.EmployeeName;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = department.DepartmentName;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = department.Timezone;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = department.AreaName;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = department.DoorName;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = department.ErrorMessage;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        }
                        workbook.SaveAs(file.FullName);
                    }
                }

                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "AC_SyncUser";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                audit.Description = AuditType.Added.ToString() + "SyncUserFromExcel:/:" + lstImport.Count().ToString();
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);

                result = Ok(message);
                return result;
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);

            }

        }

        [Authorize]
        [ActionName("DeleteACUsers")]
        [HttpPost]
        public IActionResult DeleteACUsers([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListUser == null || param.ListUser.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            if (param.IsUsingArea == true)
            {
                var lstDoor = _DbContext.AC_AreaAndDoor.Where(x => param.AreaLst.Contains(x.AreaIndex)).Select(x => x.DoorIndex).ToList();
                param.DoorLst = lstDoor;
                param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => lstDoor.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
            }
            else
            {
                param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => param.DoorLst.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
            }

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            param.AuthenMode = new List<string> { "CardNumber" };
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {

                if (lsSerialHw.Count > 0)
                {

                    IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = param.ListUser;
                    paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                    paramUserOnMachine.AuthenMode = string.Join(",", param.AuthenMode);
                    paramUserOnMachine.ListSerialNumber = param.ListSerial;
                    paramUserOnMachine.FullInfo = true;

                    List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                    IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                    commandParam.IsOverwriteData = false;
                    commandParam.Action = CommandAction.DeleteACUser;
                    commandParam.CommandName = StringHelper.GetCommandType(param.EmployeeType);
                    commandParam.AuthenMode = string.Join(",", param.AuthenMode);
                    commandParam.FromTime = new DateTime(2000, 1, 1);
                    commandParam.ToTime = DateTime.Now;
                    commandParam.ListEmployee = lstUser;
                    commandParam.ListSerialNumber = lsSerialHw;
                    commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                    commandParam.ExternalData = JsonConvert.SerializeObject(new
                    {
                        AuthenModes = param.AuthenMode
                    });

                    List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);
                    //var timeZoneIns = "000000000000000";
                    foreach (var item in lstCmd)
                    {
                        item.TimeZone = "1";
                        item.Group = "1";
                    }


                    if (lstCmd != null && lstCmd.Count() > 0)
                    {
                        IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                        groupCommand.CompanyIndex = user.CompanyIndex;
                        groupCommand.UserName = user.UserName;
                        groupCommand.ListCommand = lstCmd;
                        groupCommand.GroupName = GroupName.DeleteACUser.ToString();
                        groupCommand.EventType = "";
                        _iC_CommandLogic.CreateGroupCommands(groupCommand);
                        for (int i = 0; i < lstCmd.Count; i++)
                        {
                            CommandResult command = lstCmd[i];
                            // Add audit log
                            var audit = new IC_AuditEntryDTO(null)
                            {
                                TableName = "IC_SystemCommand",
                                UserName = user.UserName,
                                CompanyIndex = user.CompanyIndex,
                                State = AuditType.DeleteACUser,
                                Description = $"Xóa quyền truy cập {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                DescriptionEn = $"Delete access control {lstCmd[i].ListUsers?.Count ?? 0} users",
                                IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                Name = user.FullName,
                                PageName = "DeleteACUser",
                                Status = AuditStatus.Unexecuted,
                                DateTime = DateTime.Now
                            };
                            _iIC_AuditLogic.Create(audit);
                        }
                    }

                    _IAC_UserMasterService.DeleteUserMaster(param.ListUser, param.DoorLst, user);
                    //CheckFR05(user.CompanyIndex, lstCmd).GetAwaiter().GetResult();
                    //List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, GlobalParams.CommandAction.UploadUsers, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    //CreateGroupCommand(user.CompanyIndex, user.UserName, GlobalParams.CommandAction.UploadUsers.ToString(), lstCmd, "");
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }


        [Authorize]
        [ActionName("DeleteACUsersByDoor")]
        [HttpPost]
        public IActionResult DeleteACUsersByDoor([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            var employee = (from eplAcc in _DbContext.AC_AccessedGroup.Where(x => param.EmployeeAccessedGroup.Contains(x.Index))
                            join gr in _DbContext.AC_AccGroup.Where(x => x.CompanyIndex == user.CompanyIndex)
                            on eplAcc.GroupIndex equals gr.UID into eGroup
                            from eGroupResult in eGroup.DefaultIfEmpty()
                            join doorDev in _DbContext.AC_DoorAndDevice.Where(x => x.CompanyIndex == user.CompanyIndex)
                            on eGroupResult.DoorIndex equals doorDev.DoorIndex into eDoordev
                            from eDoordevResult in eDoordev.DefaultIfEmpty()
                            where ((param.ListUser != null && param.ListUser.Count > 0 && param.ListUser.Contains(eplAcc.EmployeeATID))
                            || param.ListUser == null || param.ListUser.Count == 0)
                            select new
                            {
                                EmployeeATID = eplAcc.EmployeeATID,
                                SerialNumber = eDoordevResult.SerialNumber,
                                DoorIndex = eDoordevResult.DoorIndex,
                            }).ToList();
            var devices = employee.Select(x => new { x.SerialNumber, x.DoorIndex }).Distinct().ToList();
            var deviceByDoor = devices.GroupBy(x => x.DoorIndex).Select(x => new { Key = x.Key, Value = x }).ToList();
            foreach (var device in deviceByDoor)
            {
                param.ListSerial = device.Value.Select(x => x.SerialNumber).ToList();
                param.ListUser = employee.Where(x => param.ListSerial.Contains(x.SerialNumber)).Select(x => x.EmployeeATID).ToList();

                List<string> lsSerialHw = new List<string>();
                bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
                param.AuthenMode = new List<string> { "CardNumber" };
                if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
                {


                    if (lsSerialHw.Count > 0)
                    {

                        IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                        paramUserOnMachine.ListEmployeeaATID = param.ListUser;
                        paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                        paramUserOnMachine.AuthenMode = string.Join(",", param.AuthenMode);
                        paramUserOnMachine.ListSerialNumber = param.ListSerial;
                        paramUserOnMachine.FullInfo = true;

                        List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                        IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                        commandParam.IsOverwriteData = false;
                        commandParam.Action = CommandAction.DeleteACUser;
                        commandParam.CommandName = StringHelper.GetCommandType(param.EmployeeType);
                        commandParam.AuthenMode = string.Join(",", param.AuthenMode);
                        commandParam.FromTime = new DateTime(2000, 1, 1);
                        commandParam.ToTime = DateTime.Now;
                        commandParam.ListEmployee = lstUser;
                        commandParam.ListSerialNumber = lsSerialHw;
                        commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                        commandParam.ExternalData = JsonConvert.SerializeObject(new
                        {
                            AuthenModes = param.AuthenMode
                        });

                        List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);
                        //var timeZoneIns = "000000000000000";
                        foreach (var item in lstCmd)
                        {
                            item.TimeZone = "1";
                            item.Group = "1";
                        }


                        if (lstCmd != null && lstCmd.Count() > 0)
                        {
                            IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                            groupCommand.CompanyIndex = user.CompanyIndex;
                            groupCommand.UserName = user.UserName;
                            groupCommand.ListCommand = lstCmd;
                            groupCommand.GroupName = GroupName.DeleteACUser.ToString();
                            groupCommand.EventType = "";
                            _iC_CommandLogic.CreateGroupCommands(groupCommand);
                            for (int i = 0; i < lstCmd.Count; i++)
                            {
                                CommandResult command = lstCmd[i];
                                // Add audit log
                                var audit = new IC_AuditEntryDTO(null)
                                {
                                    TableName = "IC_SystemCommand",
                                    UserName = user.UserName,
                                    CompanyIndex = user.CompanyIndex,
                                    State = AuditType.DeleteACUser,
                                    Description = $"Xóa quyền truy cập {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                    DescriptionEn = $"Delete access control {lstCmd[i].ListUsers?.Count ?? 0} users",
                                    IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                    Name = user.FullName,
                                    PageName = "DeleteACUser",
                                    Status = AuditStatus.Unexecuted,
                                    DateTime = DateTime.Now
                                };
                                _iIC_AuditLogic.Create(audit);
                            }
                        }

                        _IAC_UserMasterService.DeleteUserMaster(param.ListUser, new List<int> { device.Key }, user);
                        //CheckFR05(user.CompanyIndex, lstCmd).GetAwaiter().GetResult();
                        //List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, GlobalParams.CommandAction.UploadUsers, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                        //CreateGroupCommand(user.CompanyIndex, user.UserName, GlobalParams.CommandAction.UploadUsers.ToString(), lstCmd, "");
                    }

                }
                else
                {
                    return BadRequest("DeviceNotLicence");
                }
            }


            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("DeleteACDepartmentByDoor")]
        [HttpPost]
        public async Task<IActionResult> DeleteACDepartmentByDoor([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            var department = (from eplDe in _DbContext.AC_DepartmentAccessedGroup
           .Where(x => param.EmployeeAccessedGroup.Contains(x.Index))
                              join gr in _DbContext.AC_AccGroup
                              .Where(x => x.CompanyIndex == user.CompanyIndex)
                              on eplDe.GroupIndex equals gr.UID into eGroup
                              from eGroupResult in eGroup.DefaultIfEmpty()
                              join doorDev in _DbContext.AC_DoorAndDevice
                              .Where(x => x.CompanyIndex == user.CompanyIndex)
                              on eGroupResult.DoorIndex equals doorDev.DoorIndex into eDoordev
                              from eDoordevResult in eDoordev.DefaultIfEmpty()
                              where eGroupResult != null && eDoordevResult != null
                              select new
                              {
                                  DepartmentIndex = eplDe.DepartmentIndex,
                                  SerialNumber = eDoordevResult != null ? eDoordevResult.SerialNumber : string.Empty,  // Handle potential null SerialNumber
                                  DoorIndex = eDoordevResult != null ? eDoordevResult.DoorIndex : 0,                  // Handle potential null DoorIndex
                              }).ToList();

            var employeeInfoList = new List<EmployeeFullInfo>();
            var departmentAccList = department.Select(x => x.DepartmentIndex.ToString()).ToList();

            if (departmentAccList != null && departmentAccList.Count > 0)
            {
                employeeInfoList = await _IHR_UserService.GetEmployeeByDepartmentIds(departmentAccList, user.CompanyIndex);
            }

            var devices = department.Select(x => new { x.SerialNumber, x.DoorIndex }).Distinct().ToList();
            var deviceByDoor = devices.GroupBy(x => x.DoorIndex).Select(x => new { Key = x.Key, Value = x }).ToList();

            var employeeInAccessGroup = _DbContext.AC_AccessedGroup.Where(x => employeeInfoList.Select(z => z.EmployeeATID).Contains(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
            employeeInfoList = employeeInfoList.Where(x => !employeeInAccessGroup.Contains(x.EmployeeATID)).ToList();
            foreach (var device in deviceByDoor)
            {
                param.ListSerial = device.Value.Select(x => x.SerialNumber).ToList();
                var listDepartment = department.Where(x => param.ListSerial.Contains(x.SerialNumber)).Select(x => x.DepartmentIndex).ToList();
                param.ListUser = employeeInfoList.Where(x => listDepartment.Contains((int)x.DepartmentIndex)).Select(x => x.EmployeeATID).ToList();

                List<string> lsSerialHw = new List<string>();
                bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
                param.AuthenMode = new List<string> { "CardNumber" };
                if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
                {
                    if (lsSerialHw.Count > 0)
                    {

                        IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                        paramUserOnMachine.ListEmployeeaATID = param.ListUser;
                        paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                        paramUserOnMachine.AuthenMode = string.Join(",", param.AuthenMode);
                        paramUserOnMachine.ListSerialNumber = param.ListSerial;
                        paramUserOnMachine.FullInfo = true;

                        List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                        IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                        commandParam.IsOverwriteData = false;
                        commandParam.Action = CommandAction.DeleteACUser;
                        commandParam.CommandName = StringHelper.GetCommandType(param.EmployeeType);
                        commandParam.AuthenMode = string.Join(",", param.AuthenMode);
                        commandParam.FromTime = new DateTime(2000, 1, 1);
                        commandParam.ToTime = DateTime.Now;
                        commandParam.ListEmployee = lstUser;
                        commandParam.ListSerialNumber = lsSerialHw;
                        commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                        commandParam.ExternalData = JsonConvert.SerializeObject(new
                        {
                            AuthenModes = param.AuthenMode
                        });

                        List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);
                        //var timeZoneIns = "000000000000000";
                        foreach (var item in lstCmd)
                        {
                            item.TimeZone = "1";
                            item.Group = "1";
                        }


                        if (lstCmd != null && lstCmd.Count() > 0)
                        {
                            IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                            groupCommand.CompanyIndex = user.CompanyIndex;
                            groupCommand.UserName = user.UserName;
                            groupCommand.ListCommand = lstCmd;
                            groupCommand.GroupName = GroupName.DeleteACUser.ToString();
                            groupCommand.EventType = "";
                            _iC_CommandLogic.CreateGroupCommands(groupCommand);
                            for (int i = 0; i < lstCmd.Count; i++)
                            {
                                CommandResult command = lstCmd[i];
                                // Add audit log
                                var audit = new IC_AuditEntryDTO(null)
                                {
                                    TableName = "IC_SystemCommand",
                                    UserName = user.UserName,
                                    CompanyIndex = user.CompanyIndex,
                                    State = AuditType.DeleteACUser,
                                    Description = $"Xóa quyền truy cập {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                    DescriptionEn = $"Delete access control {lstCmd[i].ListUsers?.Count ?? 0} users",
                                    IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                    Name = user.FullName,
                                    PageName = "DeleteACUser",
                                    Status = AuditStatus.Unexecuted,
                                    DateTime = DateTime.Now
                                };
                                _iIC_AuditLogic.Create(audit);
                            }
                        }

                        _IAC_UserMasterService.DeleteUserMaster(param.ListUser, new List<int> { device.Key }, user);
                        //CheckFR05(user.CompanyIndex, lstCmd).GetAwaiter().GetResult();
                        //List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, GlobalParams.CommandAction.UploadUsers, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                        //CreateGroupCommand(user.CompanyIndex, user.UserName, GlobalParams.CommandAction.UploadUsers.ToString(), lstCmd, "");
                    }

                }
                else
                {
                    return BadRequest("DeviceNotLicence");
                }
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [AllowAnonymous]
        [ActionName("UploadUsersInterval")]
        [HttpPost]
        public IActionResult UploadUsersInterval([FromBody] IC_CommandRequestDTO param)
        {
            var config = ConfigObject.GetConfig(cache);
            var resultQuery = new List<EmployeeDepartment>();
            var serialNumbers = new List<string>();
            var serialNumbersLst = new List<IC_DepartmentAndDevice>();
            if (config.IntegrateDBOther)
            {
                var query = (from e in otherContext.HR_Employee.Where(x => x.CompanyIndex == _Config.CompanyIndex && x.UpdatedDate >= DateTime.Now.Date && !string.IsNullOrEmpty(x.CardNumber)).ToList()
                             join wi in otherContext.HR_WorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             on e.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()
                             join d in otherContext.HR_Department.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             on eWorkResult.DepartmentIndex ?? 0 equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()
                             where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec
                             select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName }).AsQueryable();
                var x = query.ToList();
                resultQuery = query.ToList().Select(x => new EmployeeDepartment
                {
                    EmployeeATID = x.Employee.EmployeeATID,
                    DepartmentID = x.Department?.Index

                }).ToList();


            }
            else
            {
                var departments = _DbContext.IC_Department.Where(x => x.CompanyIndex == _Config.CompanyIndex && x.IsInactive != true);

                resultQuery = (from e in _DbContext.HR_User.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                               join w in _DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == _Config.CompanyIndex
                               && w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date && (!w.ToDate.HasValue || w.ToDate.Value.Date >= DateTime.Now.Date))
                  on e.EmployeeATID equals w.EmployeeATID
                               join d in departments
                               on w.DepartmentIndex equals d.Index into deptGroup
                               from dept in deptGroup.DefaultIfEmpty()
                               select new EmployeeDepartment()
                               {
                                   EmployeeATID = e.EmployeeATID,
                                   DepartmentID = w.DepartmentIndex,
                               }).ToList();

            }
            if (param.IsDownloadFull == true)
            {
                serialNumbers = _DbContext.IC_Device.Where(x => x.CompanyIndex == _Config.CompanyIndex).Select(x => x.SerialNumber).ToList();
            }
            else
            {
                if (resultQuery != null && resultQuery.Count > 0)
                {
                    resultQuery = resultQuery.Where(x => x.DepartmentID != null && x.DepartmentID != 0).ToList();
                    var departmentlst = resultQuery.Select(x => x.DepartmentID).Distinct().ToList();
                    serialNumbersLst = _DbContext.IC_DepartmentAndDevice.Where(x => departmentlst.Contains(x.DepartmentIndex)).ToList();
                    serialNumbers = serialNumbersLst.Select(x => x.SerialNumber).Distinct().ToList();
                }
                else
                {
                    return Ok();
                }
            }

            foreach (var item in serialNumbers)
            {
                List<string> lsSerialHw = new List<string>();
                bool checkHw = ListSerialCheckHardWareLicense(new List<string>() { item }, ref lsSerialHw);
                if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
                {
                    var departmentCheck = serialNumbersLst.Where(x => x.SerialNumber == item).Select(x => x.DepartmentIndex).ToList();
                    List<long> longs = departmentCheck.Select(i => (long)i).ToList();
                    param.ListUser = resultQuery.Where(x => longs.Contains(x.DepartmentID.Value)).Select(x => x.EmployeeATID).ToList();
                    var listDevice = _IIC_ServiceAndDevicesService.GetAllBySerialNumbers(lsSerialHw.ToArray(), 2);
                    List<AddedParam> addedParams = new List<AddedParam>
                {
                    new AddedParam { Key = "CompanyIndex", Value = 2 },
                    new AddedParam { Key = "SystemCommandStatus", Value = false },
                    new AddedParam { Key = "CommandName", Value = StringHelper.GetCommandType(param.EmployeeType) },
                    new AddedParam { Key = "ListSerialNumber", Value = lsSerialHw }
                };
                    List<IC_SystemCommandDTO> listCommandHasExisted = _iC_SystemCommandLogic.GetMany(addedParams);
                    if (listCommandHasExisted != null && listCommandHasExisted.Count > 0)
                    {
                        lsSerialHw = lsSerialHw.Where(u => listCommandHasExisted.Where(t => t.SerialNumber == u).Count() == 0).ToList();
                    }

                    if (lsSerialHw.Count > 0)
                    {

                        IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                        paramUserOnMachine.ListEmployeeaATID = param.ListUser;
                        paramUserOnMachine.CompanyIndex = 2;
                        paramUserOnMachine.AuthenMode = string.Join(",", param.AuthenMode);
                        paramUserOnMachine.ListSerialNumber = param.ListSerial;
                        paramUserOnMachine.FullInfo = true;

                        List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                        IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                        commandParam.IsOverwriteData = false;
                        commandParam.Action = CommandAction.UploadUsers;
                        commandParam.CommandName = StringHelper.GetCommandType(param.EmployeeType);
                        commandParam.AuthenMode = string.Join(",", param.AuthenMode);
                        commandParam.FromTime = new DateTime(2000, 1, 1);
                        commandParam.ToTime = DateTime.Now;
                        commandParam.ListEmployee = lstUser;
                        commandParam.ListSerialNumber = lsSerialHw;
                        commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                        commandParam.ExternalData = JsonConvert.SerializeObject(new
                        {
                            AuthenModes = param.AuthenMode
                        });

                        List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                        if (lstCmd != null && lstCmd.Count() > 0)
                        {
                            IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                            groupCommand.CompanyIndex = 2;
                            groupCommand.UserName = "System";
                            groupCommand.ListCommand = lstCmd;
                            groupCommand.GroupName = GroupName.UploadUsers.ToString();
                            groupCommand.EventType = "";
                            _iC_CommandLogic.CreateGroupCommands(groupCommand);
                            for (int i = 0; i < lstCmd.Count; i++)
                            {
                                CommandResult command = lstCmd[i];
                                // Add audit log
                                var audit = new IC_AuditEntryDTO(null)
                                {
                                    TableName = "IC_SystemCommand",
                                    UserName = "System",
                                    CompanyIndex = 2,
                                    State = AuditType.UploadUsers,
                                    Description = $"Upload {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                    DescriptionEn = $"Upload {lstCmd[i].ListUsers?.Count ?? 0} users",
                                    IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                    Name = "System",
                                    PageName = "AutoSynchUser",
                                    Status = AuditStatus.Unexecuted,
                                    DateTime = DateTime.Now
                                };
                                _iIC_AuditLogic.Create(audit);
                            }
                        }

                        //List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, GlobalParams.CommandAction.UploadUsers, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                        //CreateGroupCommand(user.CompanyIndex, user.UserName, GlobalParams.CommandAction.UploadUsers.ToString(), lstCmd, "");
                    }
                }
                else
                {
                    continue;
                }


                _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            }
            return Ok();
        }

        [Authorize]
        [ActionName("UploadPrivilegeUsers")]
        [HttpPost]
        public async Task<IActionResult> UploadPrivilegeUsers([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListUser == null || param.ListUser.Count == 0 || param.ListSerial == null || param.ListSerial.Count == 0 || string.IsNullOrWhiteSpace(param.Privilege))
            {
                return BadRequest("BadRequest");
            }

            // get all employee info of devices list
            List<AddedParam> addedParam = new List<AddedParam>();
            addedParam.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            addedParam.Add(new AddedParam { Key = "ListEmployeeATID", Value = param.ListUser.Select(u => u).ToList() });
            var listUserMasterInfo = _iC_UserMasterLogic.GetMany(addedParam);

            // Get all service type of list device
            addedParam = new List<AddedParam>();
            addedParam.Add(new AddedParam { Key = "ListSerialNumber", Value = param.ListSerial.Select(u => u).ToList() });
            var listServiceType = _iC_ServiceAndDeviceLogic.GetMany(addedParam).ToList();

            if (listUserMasterInfo != null && listUserMasterInfo.Count() > 0)
            {
                List<string> lsSerialHw = new List<string>();
                bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);

                if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
                {
                    var privilege = string.IsNullOrWhiteSpace(param.Privilege) ? (short)0 : Convert.ToInt16(param.Privilege);
                    foreach (var em in listUserMasterInfo)
                    {
                        em.Privilege = privilege;
                    }
                    var data = await _iC_UserMasterLogic.SaveAndOverwriteList(listUserMasterInfo);

                    IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = param.ListUser;
                    paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                    paramUserOnMachine.AuthenMode = "";
                    paramUserOnMachine.FullInfo = true;
                    List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                    IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                    commandParam.ListSerialNumber = lsSerialHw;
                    commandParam.ListEmployee = lstUser;
                    commandParam.Action = CommandAction.UploadUsers;
                    commandParam.FromTime = new DateTime(2000, 1, 1);
                    commandParam.ToTime = DateTime.Now;
                    commandParam.ExternalData = "";
                    commandParam.IsOverwriteData = param.IsOverwriteData;
                    commandParam.Privilege = privilege;
                    List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);
                    //List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, GlobalParams.CommandAction.UploadUsers, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, privilege);
                    if (lstCmd != null && lstCmd.Count() > 0)
                    {
                        IC_GroupCommandParamDTO groupComParam = new IC_GroupCommandParamDTO();
                        groupComParam.CompanyIndex = user.CompanyIndex;
                        groupComParam.ListCommand = lstCmd;
                        groupComParam.UserName = user.UserName;
                        groupComParam.GroupName = GroupName.UploadUsers.ToString();
                        groupComParam.EventType = "";
                        _iC_CommandLogic.CreateGroupCommands(groupComParam);

                        // Add audit log
                        lstCmd.ForEach(cmd =>
                        {
                            var audit = new IC_AuditEntryDTO(null)
                            {
                                TableName = "IC_SystemCommand",
                                UserName = user.UserName,
                                CompanyIndex = user.CompanyIndex,
                                State = AuditType.UpdateUserPrivilege,
                                Description = $"Thay đổi quyền {cmd.ListUsers?.Count} người dùng",
                                DescriptionEn = $"Update privilege {cmd.ListUsers?.Count} users",
                                DateTime = DateTime.Now,
                                IC_SystemCommandIndex = cmd.ID != null ? int.Parse(cmd.ID) : null,
                                Name = user.FullName
                            };
                            _iIC_AuditLogic.Create(audit);
                        });
                    }

                }
                else
                {
                    return BadRequest("DeviceNotLicence");
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            await _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }



        [Authorize]
        [ActionName("GetDeviceInfo")]
        [HttpPost]
        public IActionResult GetDeviceInfo([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListSerial == null || param.ListSerial.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "SystemCommandStatus", Value = false });
                addedParams.Add(new AddedParam { Key = "CommandName", Value = CommandAction.GetDeviceInfo });
                addedParams.Add(new AddedParam { Key = "ListSerialNumber", Value = lsSerialHw });
                List<IC_SystemCommandDTO> listCommandHasExisted = _iC_SystemCommandLogic.GetMany(addedParams);
                if (listCommandHasExisted != null && listCommandHasExisted.Count > 0)
                {

                    lsSerialHw = lsSerialHw.Where(u => listCommandHasExisted.Where(t => t.SerialNumber == u).Count() == 0).ToList();
                }
                if (lsSerialHw.Count > 0)
                {
                    List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                    List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, CommandAction.GetDeviceInfo, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.GetDeviceInfo.ToString(), lstCmd, "");

                    // Add audit log
                    IC_AuditEntryDTO audit = new(null)
                    {
                        TableName = "IC_SystemCommand",
                        UserName = user.UserName,
                        CompanyIndex = user.CompanyIndex,
                        State = AuditType.GetDeviceInfo,
                        Description = $"Lấy thông tin thiết bị: {string.Join(", ", param.ListSerial)}",
                        DescriptionEn = $"Get device info: {string.Join(", ", param.ListSerial)}",
                        DateTime = DateTime.Now,
                        Status = AuditStatus.Completed
                    };
                    _iIC_AuditLogic.Create(audit);
                    _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
                    return Ok(lstCmd);
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            return result;
        }
        [Authorize]
        [ActionName("RestartDevice")]
        [HttpPost]
        public IActionResult RestartDevice([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (param.ListSerial == null || param.ListSerial.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "SystemCommandStatus", Value = false });
                addedParams.Add(new AddedParam { Key = "CommandName", Value = CommandAction.RestartDevice });
                addedParams.Add(new AddedParam { Key = "ListSerialNumber", Value = lsSerialHw });
                List<IC_SystemCommandDTO> listCommandHasExisted = _iC_SystemCommandLogic.GetMany(addedParams);
                if (listCommandHasExisted != null && listCommandHasExisted.Count > 0)
                {
                    //lsSerialHw = lsSerialHw.Where(u => listCommandHasExisted.Where(t => t.SerialNumber == u).Count() == 0).ToList();
                    var deviceSerialHasDuplicated = listCommandHasExisted.Select(x => x.SerialNumber).ToList();
                    var deviceNames = context.IC_Device.Where(x => deviceSerialHasDuplicated.Contains(x.SerialNumber)).Select(x => x.AliasName).ToList();
                    return BadRequest($"Lệnh đã tồn tại và chưa thực thi trên thiết bị {string.Join(", ", deviceNames)}. Mã lệnh: {string.Join(", ", listCommandHasExisted.Select(x => x.Index).ToList())}");
                }
                if (lsSerialHw.Count > 0)
                {
                    List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                    List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, CommandAction.RestartDevice, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.RestartDevice.ToString(), lstCmd, "");

                    // Add audit log
                    IC_AuditEntryDTO audit = new(null)
                    {
                        TableName = "IC_SystemCommand",
                        UserName = user.UserName,
                        CompanyIndex = user.CompanyIndex,
                        State = AuditType.RestartDevice,
                        Description = $"Khởi động thiết bị: {string.Join(", ", param.ListSerial)}",
                        DescriptionEn = $"Restart device: {string.Join(", ", param.ListSerial)}",
                        DateTime = DateTime.Now,
                        Status = AuditStatus.Completed,
                    };
                    _iIC_AuditLogic.Create(audit);
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("UnlockDoor")]
        [HttpPost]
        public IActionResult UnlockDoor([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => param.DoorLst.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "SystemCommandStatus", Value = false });
                addedParams.Add(new AddedParam { Key = "CommandName", Value = CommandAction.UnlockDoor });
                addedParams.Add(new AddedParam { Key = "ListSerialNumber", Value = lsSerialHw });
                List<IC_SystemCommandDTO> listCommandHasExisted = _iC_SystemCommandLogic.GetMany(addedParams);
                if (listCommandHasExisted != null && listCommandHasExisted.Count > 0)
                {
                    //lsSerialHw = lsSerialHw.Where(u => listCommandHasExisted.Where(t => t.SerialNumber == u).Count() == 0).ToList();
                    var deviceSerialHasDuplicated = listCommandHasExisted.Select(x => x.SerialNumber).ToList();
                    var deviceNames = context.IC_Device.Where(x => deviceSerialHasDuplicated.Contains(x.SerialNumber)).Select(x => x.AliasName).ToList();
                    return BadRequest($"Lệnh đã tồn tại và chưa thực thi trên thiết bị {string.Join(", ", deviceNames)}. Mã lệnh: {string.Join(", ", listCommandHasExisted.Select(x => x.Index).ToList())}");
                }
                if (lsSerialHw.Count > 0)
                {
                    List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                    List<CommandResult> lstCmd = CreateListACCommand(context, lsSerialHw, CommandAction.UnlockDoor, new DateTime(2000, 1, 1), DateTime.Now, lstUser, null, null, null, param, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.UnlockDoor.ToString(), lstCmd, "");

                    // Add audit log
                    IC_AuditEntryDTO audit = new(null)
                    {
                        TableName = "IC_SystemCommand",
                        UserName = user.UserName,
                        CompanyIndex = user.CompanyIndex,
                        State = AuditType.UnlockDoor,
                        Description = $"Mở cửa thiết bị: {string.Join(", ", param.ListSerial)}",
                        DescriptionEn = $"Unlock door device: {string.Join(", ", param.ListSerial)}",
                        DateTime = DateTime.Now,
                        Status = AuditStatus.Completed,
                    };
                    _iIC_AuditLogic.Create(audit);
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("SetDoorSetting")]
        [HttpPost]
        public IActionResult SetDoorSetting([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => param.DoorLst.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
            var doors = _DbContext.AC_Door.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
            var timezone = _DbContext.AC_TimeZone.FirstOrDefault(x => x.UID.ToString() == param.TimezoneStr);

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {

                if (lsSerialHw.Count > 0)
                {
                    List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
                    List<CommandResult> lstCmd = CreateListACCommand(context, lsSerialHw, CommandAction.SetDoorSetting, new DateTime(2000, 1, 1), DateTime.Now, lstUser, null, null, null, param, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);

                    foreach (var item in lstCmd)
                    {
                        item.TimeZone = timezone == null ? "0" : timezone.UIDIndex;
                    }
                    CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.SetDoorSetting.ToString(), lstCmd, "");

                    // Add audit log
                    IC_AuditEntryDTO audit = new(null)
                    {
                        TableName = "IC_SystemCommand",
                        UserName = user.UserName,
                        CompanyIndex = user.CompanyIndex,
                        State = AuditType.SetDoorSetting,
                        Description = $"Cập nhật thời gian mở cửa: {string.Join(", ", param.ListSerial)}",
                        DescriptionEn = $"Set door setting: {string.Join(", ", param.ListSerial)}",
                        DateTime = DateTime.Now,
                        Status = AuditStatus.Completed,
                    };
                    _iIC_AuditLogic.Create(audit);
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }

        [Authorize]
        [ActionName("UploadACAccessedEmployeeFromExcel")]
        [HttpPost]
        public IActionResult UploadACAccessedEmployeeFromExcel(List<AC_AccessedGroupImportDTO> lstImport)
        {
            try
            {
                UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
                ConfigObject config = ConfigObject.GetConfig(cache);
                IActionResult result = Unauthorized();
                if (user == null)
                {
                    return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
                }
                // validation data
                var listError = new List<AC_AccessedGroupImportDTO>();

                var lstGroups = _DbContext.AC_AccGroup.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
                var timezones = _DbContext.AC_TimeZone.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
                var lstDoorDevice = _DbContext.AC_DoorAndDevice.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
                listError = ValidationImportACAccessEmployee(lstImport, user, lstGroups, lstDoorDevice);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/ACAccessedEmployeeError.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/ACAccessedEmployeeError.xlsx"));
                if (listError != null && listError.Count() > 0)
                {
                    var listDepartmentCodeError = listError.Select(e => e.EmployeeATID).ToList();
                    lstImport = lstImport.Where(e => !listDepartmentCodeError.Contains(e.EmployeeATID)).ToList();
                }

                lstImport = lstImport.Where(x => x.ListDevices != null && x.ListDevices.Count > 0).ToList();

                var lstDevice = lstImport.Select(x => x.ListDevices).SelectMany(l => l).Distinct().ToList();

                foreach (var item in lstDevice)
                {
                    var lstDev = new List<string>() { item };
                    var lstAuthen = new List<string> { "CardNumber" };
                    List<string> lsSerialHw = new List<string>();
                    bool checkHw = ListSerialCheckHardWareLicense(lstDev, ref lsSerialHw);
                    var lstUsers = lstImport.Where(x => x.ListDevices.Contains(item)).ToList();
                    if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
                    {


                        if (lsSerialHw.Count > 0)
                        {

                            IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                            paramUserOnMachine.ListEmployeeaATID = lstUsers.Select(x => x.EmployeeATID).ToList();
                            paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                            paramUserOnMachine.AuthenMode = string.Join(",", lstAuthen);
                            paramUserOnMachine.ListSerialNumber = lstDev;
                            paramUserOnMachine.FullInfo = true;

                            var lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                            foreach (var item1 in lstUser)
                            {
                                item1.GroupID = "1";
                                var userGroup = lstImport.FirstOrDefault(x => x.EmployeeATID == item1.EmployeeATID);
                                var timezone = timezones.FirstOrDefault(x => x.UID.ToString() == userGroup.Timezone);
                                item1.TimeZone = timezone.UIDIndex;
                            }

                            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                            commandParam.IsOverwriteData = false;
                            commandParam.Action = CommandAction.UploadACUsersFromExcel;
                            commandParam.CommandName = StringHelper.GetCommandType(EmployeeType.Employee.ToInt());
                            commandParam.AuthenMode = string.Join(",", lstAuthen);
                            commandParam.FromTime = new DateTime(2000, 1, 1);
                            commandParam.ToTime = DateTime.Now;
                            commandParam.ListEmployee = lstUser;
                            commandParam.ListSerialNumber = lsSerialHw;
                            commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            commandParam.ExternalData = JsonConvert.SerializeObject(new
                            {
                                AuthenModes = lstAuthen
                            });

                            List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                            if (lstCmd != null && lstCmd.Count() > 0)
                            {
                                IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                                groupCommand.CompanyIndex = user.CompanyIndex;
                                groupCommand.UserName = user.UserName;
                                groupCommand.ListCommand = lstCmd;
                                groupCommand.GroupName = GroupName.UploadACUsersFromExcel.ToString();
                                groupCommand.EventType = "";
                                _iC_CommandLogic.CreateGroupCommands(groupCommand);
                                for (int i = 0; i < lstCmd.Count; i++)
                                {
                                    CommandResult command = lstCmd[i];
                                    // Add audit log
                                    var audit1 = new IC_AuditEntryDTO(null)
                                    {
                                        TableName = "IC_SystemCommand",
                                        UserName = user.UserName,
                                        CompanyIndex = user.CompanyIndex,
                                        State = AuditType.UploadACUsersFromExcel,
                                        Description = $"Upload {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                        DescriptionEn = $"Upload {lstCmd[i].ListUsers?.Count ?? 0} users",
                                        IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                        Name = user.FullName,
                                        PageName = "AutoSynchUser",
                                        Status = AuditStatus.Unexecuted,
                                        DateTime = DateTime.Now
                                    };
                                    _iIC_AuditLogic.Create(audit1);
                                }
                            }
                        }

                        _IAC_UserMasterService.AddUserMasterToHistoryFromExcel(lstImport, user);

                    }

                    if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw && lstUsers.Any(x => x.IsIntegrateToMachine))
                    {


                        if (lsSerialHw.Count > 0)
                        {
                            IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                            paramUserOnMachine.ListEmployeeaATID = lstUsers.Where(x => x.IsIntegrateToMachine).Select(x => x.EmployeeATID).ToList();
                            paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                            paramUserOnMachine.AuthenMode = null;
                            paramUserOnMachine.ListSerialNumber = lstDev;
                            paramUserOnMachine.FullInfo = true;

                            var lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
                            foreach (var item1 in lstUser)
                            {
                                item1.GroupID = "1";
                                var userGroup = lstImport.FirstOrDefault(x => x.EmployeeATID == item1.EmployeeATID);
                                var timezone = timezones.FirstOrDefault(x => x.UID.ToString() == userGroup.Timezone);
                                item1.TimeZone = timezone.UIDIndex;
                            }

                            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                            commandParam.IsOverwriteData = false;
                            commandParam.Action = CommandAction.UploadUsers;
                            commandParam.CommandName = StringHelper.GetCommandType(EmployeeType.Employee.ToInt());
                            commandParam.AuthenMode = string.Join(",", lstAuthen);
                            commandParam.FromTime = new DateTime(2000, 1, 1);
                            commandParam.ToTime = DateTime.Now;
                            commandParam.ListEmployee = lstUser;
                            commandParam.ListSerialNumber = lsSerialHw;
                            commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            commandParam.ExternalData = JsonConvert.SerializeObject(new
                            {
                                AuthenModes = new List<string>()
                            });

                            List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                            if (lstCmd != null && lstCmd.Count() > 0)
                            {
                                IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                                groupCommand.CompanyIndex = user.CompanyIndex;
                                groupCommand.UserName = user.UserName;
                                groupCommand.ListCommand = lstCmd;
                                groupCommand.GroupName = GroupName.UploadUsers.ToString();
                                groupCommand.EventType = "";
                                _iC_CommandLogic.CreateGroupCommands(groupCommand);
                                for (int i = 0; i < lstCmd.Count; i++)
                                {
                                    CommandResult command = lstCmd[i];
                                    // Add audit log
                                    var audit1 = new IC_AuditEntryDTO(null)
                                    {
                                        TableName = "IC_SystemCommand",
                                        UserName = user.UserName,
                                        CompanyIndex = user.CompanyIndex,
                                        State = AuditType.UploadUsers,
                                        Description = $"Upload {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                        DescriptionEn = $"Upload {lstCmd[i].ListUsers?.Count ?? 0} users",
                                        IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                        Name = user.FullName,
                                        PageName = "AutoSynchUser",
                                        Status = AuditStatus.Unexecuted,
                                        DateTime = DateTime.Now
                                    };
                                    _iIC_AuditLogic.Create(audit1);
                                }
                            }
                        }


                    }
                }


                foreach (var employeeAccess in lstImport)
                {
                    var groupIndex = lstGroups.FirstOrDefault(x => x.Name == employeeAccess.Group);
                    if (groupIndex != null)
                    {
                        var employee_AccessedGroup = new AC_AccessedGroup();
                        employee_AccessedGroup.EmployeeATID = employeeAccess.EmployeeATID;
                        employee_AccessedGroup.GroupIndex = groupIndex.UID;
                        employee_AccessedGroup.UpdatedDate = DateTime.Now;
                        employee_AccessedGroup.UpdatedUser = user.UserName;
                        employee_AccessedGroup.CompanyIndex = user.CompanyIndex;
                        context.AC_AccessedGroup.Add(employee_AccessedGroup);
                    }
                }
                context.SaveChanges();

                if (listError != null && listError.Count() > 0)
                {

                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("EmployeeSyncError");
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "MCC (*)";
                        worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                        worksheet.Cell(currentRow, 3).Value = "Họ tên";
                        worksheet.Cell(currentRow, 4).Value = "Nhóm truy cập (*)";
                        worksheet.Cell(currentRow, 5).Value = "Đồng bộ thông tin nhân viên lên thiết bị";
                        worksheet.Cell(currentRow, 6).Value = "Lỗi";

                        for (int i = 1; i < 7; i++)
                        {
                            worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Column(i).Width = 20;
                        }

                        foreach (var department in listError)
                        {
                            currentRow++;
                            //New template
                            worksheet.Cell(currentRow, 1).Value = "'" + department.EmployeeATID;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 2).Value = department.EmployeeCode;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 3).Value = department.EmployeeName;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = department.Group;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = department.IsIntegrateToMachine ? "x" : "";
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = department.ErrorMessage;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        }
                        workbook.SaveAs(file.FullName);
                    }
                }

                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "AC_SyncUser";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                audit.Description = AuditType.Added.ToString() + "SyncUserFromExcel:/:" + lstImport.Count().ToString();
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);

                result = Ok(message);
                return result;
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);

            }

        }


        [Authorize]
        [ActionName("RestartService")]
        [HttpPost]
        public async Task<IActionResult> RestartService([FromBody] List<int> listServiceIndex)
        {
            try
            {
                var user = UserInfo.GetFromCache(cache, User.Identity.Name);
                if (user == null)
                {
                    return Unauthorized("TokenExpired");
                }
                await _notificationClient.SendMessageToChannelAsync("restartService", JsonConvert.SerializeObject(listServiceIndex));
                var serviceNames = context.IC_Service.Where(x => listServiceIndex.Contains(x.Index)).Select(x => x.Name).ToList();
                string serviceNamesStr = string.Join(", ", serviceNames).TextOverflowEllipsis(100);
                // Add audit log
                IC_AuditEntryDTO audit = new(null)
                {
                    TableName = "IC_SystemCommand",
                    UserName = user.UserName,
                    CompanyIndex = user.CompanyIndex,
                    State = AuditType.RestartService,
                    Description = $"Khởi động lại {serviceNames.Count} service: {serviceNamesStr}",
                    DescriptionEn = $"Restart {serviceNames.Count} service: {serviceNamesStr}",
                    DateTime = DateTime.Now
                };
                _iIC_AuditLogic.Create(audit);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"RestartService {ex}");
                return BadRequest("Error: " + ex.Message);
            }
        }


        [Authorize]
        [ActionName("RestartServiceByDevice")]
        [HttpPost]
        public async Task<IActionResult> RestartService([FromBody] List<string> devices)
        {
            try
            {
                var user = UserInfo.GetFromCache(cache, User.Identity.Name);
                if (user == null)
                {
                    return Unauthorized("TokenExpired");
                }
                var services = await _IIC_ServiceAndDevicesService.GetAllBySerialNumbers(devices.ToArray(), user.CompanyIndex);
                var listServiceIndex = services.Select(x => x.ServiceIndex).ToList();
                await _notificationClient.SendMessageToChannelAsync("restartService", JsonConvert.SerializeObject(listServiceIndex));
                var serviceNames = context.IC_Service.Where(x => listServiceIndex.Contains(x.Index)).Select(x => x.Name).ToList();
                string serviceNamesStr = string.Join(", ", serviceNames).TextOverflowEllipsis(100);
                // Add audit log
                IC_AuditEntryDTO audit = new(null)
                {
                    TableName = "IC_SystemCommand",
                    UserName = user.UserName,
                    CompanyIndex = user.CompanyIndex,
                    State = AuditType.RestartService,
                    Description = $"Khởi động lại {serviceNames.Count} service: {serviceNamesStr}",
                    DescriptionEn = $"Restart {serviceNames.Count} service: {serviceNamesStr}",
                    DateTime = DateTime.Now
                };
                _iIC_AuditLogic.Create(audit);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"RestartService {ex}");
                return BadRequest("Error: " + ex.Message);
            }
        }

        [AllowAnonymous]
        [ActionName("CallbackHeartBeatFR05")]
        [HttpPost]
        public IActionResult CallbackHeartBeatFR05([FromForm] HeartbeatParam param)
        {
            List<string> lsSerialNumber = new List<string>();
            lsSerialNumber.Add(param.DeviceKey);
            _iC_SystemCommandLogic.UpdateLastConnection(lsSerialNumber, 2);
            return Ok();
        }


        [AllowAnonymous]
        [ActionName("CallbackAttendanceFR05")]
        [HttpPost]
        public async Task<IActionResult> CallbackAttendanceFR05([FromForm] AttendanceFR05Param param)
        {
            if (param.PersonId != "STRANGERBABY")
            {
                var device = context.IC_Device.AsNoTracking().FirstOrDefault(x => x.CompanyIndex == 2 && x.SerialNumber == param.DeviceKey);


                var timeLog = param.Time.ConvertUnixTimeToDateTime();
                var VerifiedMode = 15;
                if (param.Type.Contains("finger"))
                {
                    VerifiedMode = 1;
                }

                List<LogInfo> logInfos = new List<LogInfo>()
                {
                    new LogInfo()
                    {
                         UserId = param.PersonId,
                         InOutMode = device.DeviceStatus != null ? HandleLogInOutMode(device.DeviceStatus.Value) : 0,
                         Time = timeLog,
                         VerifiedMode = VerifiedMode
                    }
                };

                var attendanceLogPram = new AttendanceLogPram()
                {
                    ListAttendanceLog = logInfos,
                    SerialNumber = param.DeviceKey
                };

                await PostAttendanceRealTime(attendanceLogPram);
            }


            return Ok();
        }

        public int HandleLogInOutMode(int deviceStatus)
        {
            var inOutMode = deviceStatus;
            switch (deviceStatus)
            {
                case (int)DeviceStatus.Output: inOutMode = (int)InOutMode.Output; break;
                case (int)DeviceStatus.Input: inOutMode = (int)InOutMode.Input; break;
            }
            return inOutMode;
        }




        public async Task PostAttendanceRealTime(AttendanceLogPram pAttendanceLogPram)
        {
            string content = WebAPIUtility.ConvertObjectToJson(pAttendanceLogPram);
            HttpResponseMessage response = await GetResponseMessage(content, WebAPIMethod.POST, Convert.ToString(_FR05Config.Host) + "/api/AttendanceLog/AddAttendanceLogRealTime", _FR05Config, cache);

            if (response != null)
            {
                var pStatusCode = response.StatusCode;
            }
        }




        [NonAction]
        public bool ListSerialCheckHardWareLicense(List<string> lsSerial, ref List<string> lsSerialHw)
        {
            lsSerialHw = new List<string>();
            int dem = 0;
            foreach (var serial in lsSerial)
            {
                bool checkls = cache.HaveHWLicense(serial);
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


        internal List<CommandResult> CreateListCommand(EPAD_Context context, List<string> listSerial, CommandAction pAction, DateTime pFromTime, DateTime pToTime, List<UserInfoOnMachine> pListUsers, bool isOverwriteData, short privilege, string externalData = "")
        {
            return CommandProcess.CreateListCommands(context, listSerial, pAction, externalData, pFromTime, pToTime, pListUsers, isOverwriteData, privilege);
        }

        internal List<CommandResult> CreateListACCommand(EPAD_Context context, List<string> listSerial, CommandAction pAction, DateTime pFromTime, DateTime pToTime, List<UserInfoOnMachine> pListUsers, List<AC_AccGroup> accGroups, List<AC_AccHoliday> accHolidays, List<AC_TimeZone> timeZones, IC_CommandRequestDTO commandRequest, bool isOverwriteData, short privilege, string externalData = "")
        {
            return CommandProcess.CreateListACCommands(context, listSerial, pAction, externalData, pFromTime, pToTime, pListUsers, accGroups, accHolidays, timeZones, isOverwriteData, commandRequest, privilege);
        }

        internal void CreateGroupCommand(int pCompanyIndex, string pUserName, string pGroupName, List<CommandResult> pListCommands, string pEventType, string externalData = "")
        {
            CommandProcess.CreateGroupCommand(context, cache, pCompanyIndex, pUserName, pGroupName, externalData, pListCommands, pEventType);
        }

        [NonAction]
        public void IntegratedLogFromToTime(int pCompanyIndex, string pUserName, List<string> pListSerial, DateTime pFromTime, DateTime pToTime)
        {
            List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
            List<CommandResult> lstCmd = CreateListCommand(context, pListSerial, CommandAction.CallIntegratedLog, pFromTime, pToTime, lstUser, false, GlobalParams.DevicePrivilege.SDKStandardRole);
            CreateGroupCommand(pCompanyIndex, pUserName, CommandAction.CallIntegratedLog.ToString(), lstCmd, "");
        }

        [NonAction]
        public async Task CheckFR05(int pCompanyIndex, List<CommandResult> pListCommands)
        {
            Dictionary<string, IC_Device> deviceLookup = null;
            if (_Cache.TryGetValue("urn:Dictionary_IC_Device", out deviceLookup) == false)
            {
                deviceLookup = _DbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex).ToDictionarySafe(x => x.SerialNumber);
            }

            var listCheck = deviceLookup.Where(x => !string.IsNullOrEmpty(x.Value.DeviceModel) && NumberExtensions.IsNumber(x.Value.DeviceModel) && int.Parse(x.Value.DeviceModel) == (int)ProducerEnum.FR05).Select(x => x.Key).ToList();

            if (pListCommands.Any(x => listCheck.Contains(x.SerialNumber)))
            {
                UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
                CompanyInfo companyInfo = CompanyInfo.GetFromCache(cache, user.CompanyIndex.ToString());
                var listCommandFr05 = pListCommands.Where(x => listCheck.Contains(x.SerialNumber)).ToList();
                foreach (var command in listCommandFr05)
                {
                    await _FR05_ClientService.ProcessCommand(command, user, config, companyInfo, false);
                }
            }
        }

        [NonAction]
        private double CaculateTime(DateTime? time1, DateTime time2)
        {
            DateTime temp = new DateTime();
            if (time1.HasValue)
            {
                temp = time1.Value;
            }
            else
            {
                temp = new DateTime(2000, 1, 1, 0, 0, 0);
            }
            TimeSpan time = new TimeSpan();
            time = time2 - temp;
            return time.TotalMinutes;
        }

        [NonAction]
        public List<AC_UserImportDTO> ValidationImportACUser(List<AC_UserImportDTO> param, UserInfo user, List<AC_TimeZone> timezoneLst, List<AC_Area> areaLst, List<AC_Door> doorLst, List<AC_AreaAndDoor> areaDoorLst, List<AC_DoorAndDevice> doorDeviceLst)
        {
            var EmployeeATIDs = param.Select(x => x.EmployeeATID).Distinct().ToList();
            var lstEmployeeATIDs = _DbContext.HR_User.Where(x => x.CompanyIndex == user.CompanyIndex && EmployeeATIDs.Contains(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
            var errorList = new List<AC_UserImportDTO>();
            var checkExistEmployee = param.Where(x => !lstEmployeeATIDs.Contains(x.EmployeeATID)).ToList();

            foreach (var item in checkExistEmployee)
            {
                item.ErrorMessage = "Mã chấm công không tồn tại\r\n";
            }

            foreach (var item in param)
            {
                if (item.ErrorMessage == null)
                {
                    item.ErrorMessage = "";
                }
                var checkTimezone = timezoneLst.FirstOrDefault(x => x.Name == item.Timezone);
                if (checkTimezone == null)
                {
                    item.ErrorMessage += "Timezone không tồn tại\r\n";
                }
                else
                {
                    item.TimezoneIndex = checkTimezone.UID;
                    item.TimezoneUID = checkTimezone.UIDIndex;
                }

                if (string.IsNullOrEmpty(item.AreaName) && string.IsNullOrEmpty(item.DoorName))
                {
                    item.ErrorMessage += "Khu vực kiểm soát hoặc Cửa kiểm soát phải có dữ liệu\r\n";
                }
                else
                {
                    if (!string.IsNullOrEmpty(item.AreaName) && !string.IsNullOrEmpty(item.DoorName))
                    {
                        var areaNameLst = item.AreaName.Split(',').Select(x => x.Trim()).ToList();
                        item.AreaIndexLst = new List<int>();
                        item.DoorIndexLst = new List<int>();
                        var check = false;
                        foreach (var ite in areaNameLst)
                        {
                            var area = areaLst.FirstOrDefault(x => x.Name == ite);
                            if (area == null)
                            {
                                item.ErrorMessage += "Khu vực kiểm soát không tồn tại\r\n";
                                check = true;
                                break;
                            }
                            else
                            {
                                item.AreaIndexLst.Add(area.Index);
                            }
                        }
                        if (!check)
                        {
                            item.DoorIndexLst = areaDoorLst.Where(x => item.AreaIndexLst.Contains(x.AreaIndex)).Select(x => x.DoorIndex).ToList();
                            item.ListSerial = doorDeviceLst.Where(x => item.DoorIndexLst.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
                        }
                    }
                    else if (!string.IsNullOrEmpty(item.AreaName) && string.IsNullOrEmpty(item.DoorName))
                    {
                        var areaNameLst = item.AreaName.Split(',').Select(x => x.Trim()).ToList();
                        item.AreaIndexLst = new List<int>();
                        item.DoorIndexLst = new List<int>();
                        var check = false;
                        foreach (var ite in areaNameLst)
                        {
                            var area = areaLst.FirstOrDefault(x => x.Name == ite);
                            if (area == null)
                            {
                                item.ErrorMessage += "Khu vực kiểm soát không tồn tại\r\n";
                                check = true;
                                break;
                            }
                            else
                            {
                                item.AreaIndexLst.Add(area.Index);
                            }
                        }
                        if (!check)
                        {
                            item.DoorIndexLst = areaDoorLst.Where(x => item.AreaIndexLst.Contains(x.AreaIndex)).Select(x => x.DoorIndex).ToList();
                            item.ListSerial = doorDeviceLst.Where(x => item.DoorIndexLst.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
                        }
                    }
                    else if (string.IsNullOrEmpty(item.AreaName) && !string.IsNullOrEmpty(item.DoorName))
                    {
                        var doorNameLst = item.DoorName.Split(',').Select(x => x.Trim()).ToList();
                        item.DoorIndexLst = new List<int>();
                        var check = false;
                        var doors = new List<int>();
                        foreach (var ite in doorNameLst)
                        {
                            var door = doorLst.FirstOrDefault(x => x.Name == ite);
                            if (door == null)
                            {
                                item.ErrorMessage += "Cửa kiểm soát không tồn tại\r\n";
                                check = true;
                                break;
                            }
                            else
                            {
                                doors.Add(door.Index);
                            }
                        }

                        if (!check)
                        {
                            item.DoorIndexLst = doors;
                            item.ListSerial = doorDeviceLst.Where(x => item.DoorIndexLst.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
                        }
                    }

                    if (item.ListSerial != null && item.ListSerial.Count > 0)
                    {
                        var lstSeri = new List<string>();
                        var boo = ListSerialCheckHardWareLicense(item.ListSerial, ref lstSeri);
                        if (!boo)
                        {
                            item.ErrorMessage += "Thiết bị chưa có bản quyền\r\n";
                        }
                    }
                }
            }
            errorList = param.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            return errorList;
        }

        [NonAction]
        public List<AC_AccessedGroupImportDTO> ValidationImportACAccessEmployee(List<AC_AccessedGroupImportDTO> param, UserInfo user, List<AC_AccGroup> groups, List<AC_DoorAndDevice> doorDevicesLst)
        {
            var EmployeeATIDs = param.Select(x => x.EmployeeATID).Distinct().ToList();
            var lstEmployeeATIDs = _DbContext.HR_User.Where(x => x.CompanyIndex == user.CompanyIndex && EmployeeATIDs.Contains(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
            var accessedGroup = _DbContext.AC_AccessedGroup.Where(x => x.CompanyIndex == user.CompanyIndex && EmployeeATIDs.Contains(x.EmployeeATID)).ToList();
            var errorList = new List<AC_AccessedGroupImportDTO>();
            var checkExistEmployee = param.Where(x => !lstEmployeeATIDs.Contains(x.EmployeeATID)).ToList();

            foreach (var item in checkExistEmployee)
            {
                item.ErrorMessage = "Mã chấm công không tồn tại\r\n";
            }

            foreach (var item in param)
            {
                if (item.ErrorMessage == null)
                {
                    item.ErrorMessage = "";
                }
                var group = groups.FirstOrDefault(x => x.Name == item.Group);
                if (group == null)
                {
                    item.ErrorMessage += "Nhóm truy cập không tồn tại\r\n";
                }
                else
                {
                    item.GroupIndex = group.UID;
                    item.Timezone = group.Timezone.ToString();
                    item.DoorIndexLst = new List<int> { group.DoorIndex };
                    item.ListDevices = doorDevicesLst.Where(x => item.DoorIndexLst.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();

                    var accGroup = groups.Where(x => x.DoorIndex == group.DoorIndex).Select(x => x.UID).ToList();
                    var dummy = accessedGroup.Where(x => x.CompanyIndex == user.CompanyIndex && x.EmployeeATID == item.EmployeeATID && accGroup.Contains(x.GroupIndex)
                                                    ).FirstOrDefault();
                    if (dummy != null)
                    {
                        item.ErrorMessage += "Nhân viên đã được khai báo trước đó\r\n";
                    }
                }

                if (item.ListDevices != null && item.ListDevices.Count > 0)
                {
                    var lstSeri = new List<string>();
                    var boo = ListSerialCheckHardWareLicense(item.ListDevices, ref lstSeri);
                    if (!boo)
                    {
                        item.ErrorMessage += "Thiết bị chưa có bản quyền\r\n";
                    }
                }
            }
            errorList = param.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            return errorList;
        }

        [Authorize]
        [ActionName("UploadACByDepartment")]
        [HttpPost]
        public async Task<IActionResult> UploadACByDepartment([FromBody] IC_CommandRequestDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            var userListByDepartment = await _IHR_UserService.GetEmployeeByDepartmentIds(param.ListDepartment, user.CompanyIndex);
            param.ListUser = userListByDepartment.Select(x => x.EmployeeATID).ToList();
            if (param.ListUser == null || param.ListUser.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            if (param.IsUsingArea == true)
            {
                var lstDoor = _DbContext.AC_AreaAndDoor.Where(x => param.AreaLst.Contains(x.AreaIndex)).Select(x => x.DoorIndex).ToList();
                param.DoorLst = lstDoor;
                param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => lstDoor.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
            }
            else
            {
                param.ListSerial = _DbContext.AC_DoorAndDevice.Where(x => param.DoorLst.Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();
            }

            if (param.ListSerial.Count == 0)
            {
                return Ok();
            }

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            param.AuthenMode = new List<string> { "CardNumber" };
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {

                if (lsSerialHw.Count > 0)
                {

                    IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = param.ListUser;
                    paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                    paramUserOnMachine.AuthenMode = string.Join(",", param.AuthenMode);
                    paramUserOnMachine.ListSerialNumber = param.ListSerial;
                    paramUserOnMachine.FullInfo = true;

                    List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                    IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                    commandParam.IsOverwriteData = false;
                    commandParam.Action = CommandAction.UploadACUsers;
                    commandParam.CommandName = StringHelper.GetCommandType(param.EmployeeType);
                    commandParam.AuthenMode = string.Join(",", param.AuthenMode);
                    commandParam.FromTime = new DateTime(2000, 1, 1);
                    commandParam.ToTime = DateTime.Now;
                    commandParam.ListEmployee = lstUser;
                    commandParam.ListSerialNumber = lsSerialHw;
                    commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                    commandParam.ExternalData = JsonConvert.SerializeObject(new
                    {
                        AuthenModes = param.AuthenMode
                    });

                    List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                    string timeZone = param.TimeZone.FirstOrDefault();
                    var timezoneDb = _DbContext.AC_TimeZone.FirstOrDefault(x => x.UID == int.Parse(timeZone));
                    //var lstTimezone = timezoneDb.UIDIndex.Split(',').ToList();

                    //string group = param.Group.ToString();


                    //var timeZoneIns = "0001";
                    //foreach (var t in lstTimezone)
                    //{
                    //    timeZoneIns += t.PadLeft(4, '0');
                    //}


                    foreach (var item in lstCmd)
                    {
                        item.TimeZone = timezoneDb.UIDIndex;
                        item.Group = "1";
                    }


                    if (lstCmd != null && lstCmd.Count() > 0)
                    {
                        IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                        groupCommand.CompanyIndex = user.CompanyIndex;
                        groupCommand.UserName = user.UserName;
                        groupCommand.ListCommand = lstCmd;
                        groupCommand.GroupName = GroupName.UploadACUsers.ToString();
                        groupCommand.EventType = "";
                        _iC_CommandLogic.CreateGroupCommands(groupCommand);
                        for (int i = 0; i < lstCmd.Count; i++)
                        {
                            CommandResult command = lstCmd[i];
                            // Add audit log
                            var audit = new IC_AuditEntryDTO(null)
                            {
                                TableName = "IC_SystemCommand",
                                UserName = user.UserName,
                                CompanyIndex = user.CompanyIndex,
                                State = AuditType.UploadACUsers,
                                Description = $"Upload {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                DescriptionEn = $"Upload {lstCmd[i].ListUsers?.Count ?? 0} users",
                                IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
                                Name = user.FullName,
                                PageName = "AutoSynchUser",
                                Status = AuditStatus.Unexecuted,
                                DateTime = DateTime.Now
                            };
                            _iIC_AuditLogic.Create(audit);
                        }
                    }

                    _IAC_UserMasterService.AddUserMasterToHistory(param.ListUser, param.DoorLst, user, int.Parse(timeZone));
                    //CheckFR05(user.CompanyIndex, lstCmd).GetAwaiter().GetResult();
                    //List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, GlobalParams.CommandAction.UploadUsers, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    //CreateGroupCommand(user.CompanyIndex, user.UserName, GlobalParams.CommandAction.UploadUsers.ToString(), lstCmd, "");
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            result = Ok();
            _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
            return result;
        }


        public class EmployeeDepartment
        {
            public string EmployeeATID { get; set; }
            public long? DepartmentID { get; set; }
        }

        public class DepartmentAccessModel
        {
            public int DoorIndex { get; set; }
            public string SerialNumber { get; set; }
            public string DepartmentIndex { get; set; }
        }
    }


}
