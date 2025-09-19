using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Logic.MainProcess;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_CommandService : BaseServices<IC_Command, EPAD_Context>, IIC_CommandService
    {
        private readonly EPAD_Context _context;
        private IMemoryCache cache;
        private IIC_CommandLogic _iC_CommandLogic;
        private IIC_AuditLogic _iIC_AuditLogic;
        private IAC_UserMasterService _IAC_UserMasterService;
        private IHR_UserService _IHR_UserService;
        private ILogger _logger;
        public IC_CommandService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _context = serviceProvider.GetService<EPAD_Context>();
            cache = serviceProvider.GetService<IMemoryCache>();
            _iC_CommandLogic = serviceProvider.GetService<IIC_CommandLogic>();
            _iIC_AuditLogic = serviceProvider.GetService<IIC_AuditLogic>();
            _IAC_UserMasterService = serviceProvider.GetService<IAC_UserMasterService>();
            _IHR_UserService = serviceProvider.GetService<IHR_UserService>();
            _logger = loggerFactory.CreateLogger<IC_VehicleLogService>();
        }

        public async Task UploadACUsers(int groupIndex, List<string> listUser, UserInfo user)
        {
            var groupAcc = _context.AC_AccGroup.Where(x => x.UID == groupIndex).ToList();
            var listSerial = _context.AC_DoorAndDevice.Where(x => groupAcc.Select(x => x.DoorIndex).Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(listSerial, ref lsSerialHw);
            var authenMode = new List<string> { "CardNumber" };
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                if (lsSerialHw.Count > 0)
                {

                    IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = listUser;
                    paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                    paramUserOnMachine.AuthenMode = string.Join(",", authenMode);
                    paramUserOnMachine.ListSerialNumber = listSerial;
                    paramUserOnMachine.FullInfo = true;

                    List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                    IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                    commandParam.IsOverwriteData = false;
                    commandParam.Action = CommandAction.UploadACUsers;
                    commandParam.CommandName = StringHelper.GetCommandType((int)EmployeeType.Employee);
                    commandParam.AuthenMode = string.Join(",", authenMode);
                    commandParam.FromTime = new DateTime(2000, 1, 1);
                    commandParam.ToTime = DateTime.Now;
                    commandParam.ListEmployee = lstUser;
                    commandParam.ListSerialNumber = lsSerialHw;
                    commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                    commandParam.ExternalData = JsonConvert.SerializeObject(new
                    {
                        AuthenModes = authenMode
                    });

                    List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                    var timeZone = groupAcc.Select(x => x.Timezone).FirstOrDefault();
                    var timezoneDb = _context.AC_TimeZone.FirstOrDefault(x => x.UID == timeZone);
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
                                IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : 0,
                                Name = user.FullName,
                                PageName = "AutoSynchUser",
                                Status = AuditStatus.Unexecuted,
                                DateTime = DateTime.Now
                            };
                            _iIC_AuditLogic.Create(audit);
                        }
                    }

                    _IAC_UserMasterService.AddUserMasterToHistory(listUser, groupAcc.Select(x => x.DoorIndex).ToList(), user, timeZone);
                    //CheckFR05(user.CompanyIndex, lstCmd).GetAwaiter().GetResult();
                    //List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, GlobalParams.CommandAction.UploadUsers, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    //CreateGroupCommand(user.CompanyIndex, user.UserName, GlobalParams.CommandAction.UploadUsers.ToString(), lstCmd, "");
                }
            }

        }


        public async Task UploadTimeZone(int groupIndex, UserInfo user)
        {

            var groupAcc = _context.AC_AccGroup.Where(x => x.UID == groupIndex).ToList();
            var listSerial = _context.AC_DoorAndDevice.Where(x => groupAcc.Select(x => x.DoorIndex).Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();

            List<string> lsSerialHw = new List<string>();
            ListSerialCheckHardWareLicense(listSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0)
            {

                if (lsSerialHw.Count > 0)
                {
                    var lstUser = new List<UserInfoOnMachine>();
                    var lstHolidays = new List<AC_AccHoliday>();
                    var lstGroups = new List<AC_AccGroup>();
                    var lstTimezones = new List<AC_TimeZone>();
                    var timeZoneList = groupAcc.Select(x => x.Timezone).ToList();
                    if (timeZoneList == null || timeZoneList.Count == 0)
                    {
                        lstTimezones = _context.AC_TimeZone.ToList();
                    }
                    else
                    {
                        var timeZone = timeZoneList.FirstOrDefault();
                        lstTimezones = _context.AC_TimeZone.Where(x => x.UID == timeZone).ToList();
                    }

                    var param = new IC_CommandRequestDTO();
                    List<CommandResult> lstCmd = CreateListACCommand(_context, lsSerialHw, CommandAction.UploadTimeZone, new DateTime(2000, 1, 1), DateTime.Now, lstUser, lstGroups, lstHolidays, lstTimezones, param, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    CreateGroupCommand(user.CompanyIndex, user.UserName, CommandAction.UploadTimeZone.ToString(), lstCmd, "");

                    // Add audit log
                    var audit = new IC_AuditEntryDTO(null)
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
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null)
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
            }

        }

        public async Task UploadUsers(int groupIndex, List<string> listUser, UserInfo user)
        {
            var groupAcc = _context.AC_AccGroup.Where(x => x.UID == groupIndex).ToList();
            var listSerial = _context.AC_DoorAndDevice.Where(x => groupAcc.Select(x => x.DoorIndex).Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(listSerial, ref lsSerialHw);

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
                var authenMode = new List<string> { "CardNumber" };
                if (lsSerialHw.Count > 0)
                {
                    IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = listUser;
                    paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                    paramUserOnMachine.AuthenMode = string.Join(",", authenMode);
                    paramUserOnMachine.ListSerialNumber = listSerial;
                    paramUserOnMachine.FullInfo = true;

                    List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                    IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                    commandParam.IsOverwriteData = false;
                    commandParam.Action = CommandAction.UploadUsers;
                    commandParam.CommandName = StringHelper.GetCommandType((int)EmployeeType.Employee);
                    commandParam.AuthenMode = string.Join(",", authenMode);
                    commandParam.FromTime = new DateTime(2000, 1, 1);
                    commandParam.ToTime = DateTime.Now;
                    commandParam.ListEmployee = lstUser;
                    commandParam.ListSerialNumber = lsSerialHw;
                    commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                    commandParam.ExternalData = JsonConvert.SerializeObject(new
                    {
                        AuthenModes = authenMode
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
                                IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : 0,
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
        }

        //public async Task<IActionResult> DeleteACDepartmentByDoor(List<string> listDepartment, List<int> departmentAccIndexes, UserInfo user)
        //{
        //    var employeeInfoList = new List<EmployeeFullInfo>();
        //    if (listDepartment != null && listDepartment.Count > 0)
        //    {
        //        employeeInfoList = await _IHR_UserService.GetEmployeeByDepartmentIds(listDepartment, user.CompanyIndex);
        //    }

        //    var department = (from eplDe in _context.AC_DepartmentAccessedGroup.Where(x => departmentAccIndexes.Contains(x.Index))
        //                      join gr in _context.AC_AccGroup.Where(x => x.CompanyIndex == user.CompanyIndex)
        //                      on eplDe.GroupIndex equals gr.UID into eGroup
        //                      from eGroupResult in eGroup.DefaultIfEmpty()
        //                      join doorDev in _context.AC_DoorAndDevice.Where(x => x.CompanyIndex == user.CompanyIndex)
        //                      on eGroupResult.DoorIndex equals doorDev.DoorIndex into eDoordev
        //                      from eDoordevResult in eDoordev.DefaultIfEmpty()
        //                      where ((listDepartment != null && listDepartment.Count > 0 && listDepartment.Contains(eplDe.DepartmentIndex.ToString()))
        //                      || listDepartment == null || listDepartment.Count == 0)
        //                      select new
        //                      {
        //                          DepartmentIndex = eplDe.DepartmentIndex,
        //                          SerialNumber = eDoordevResult.SerialNumber,
        //                          DoorIndex = eDoordevResult.DoorIndex,
        //                      }).ToList();
        //    var devices = department.Select(x => new { x.SerialNumber, x.DoorIndex }).Distinct().ToList();
        //    var deviceByDoor = devices.GroupBy(x => x.DoorIndex).Select(x => new { Key = x.Key, Value = x }).ToList();

        //    var employeeInAccessGroup = _DbContext.AC_AccessedGroup.Where(x => employeeInfoList.Select(z => z.EmployeeATID).Contains(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
        //    employeeInfoList = employeeInfoList.Where(x => !employeeInAccessGroup.Contains(x.EmployeeATID)).ToList();

        //    foreach (var device in deviceByDoor)
        //    {
        //        param.ListSerial = device.Value.Select(x => x.SerialNumber).ToList();
        //        var listDepartment = department.Where(x => param.ListSerial.Contains(x.SerialNumber)).Select(x => x.DepartmentIndex).ToList();
        //        param.ListUser = employeeInfoList.Where(x => listDepartment.Contains((int)x.DepartmentIndex)).Select(x => x.EmployeeATID).ToList();

        //        List<string> lsSerialHw = new List<string>();
        //        bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
        //        param.AuthenMode = new List<string> { "CardNumber" };
        //        if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
        //        {
        //            if (lsSerialHw.Count > 0)
        //            {

        //                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
        //                paramUserOnMachine.ListEmployeeaATID = param.ListUser;
        //                paramUserOnMachine.CompanyIndex = user.CompanyIndex;
        //                paramUserOnMachine.AuthenMode = string.Join(",", param.AuthenMode);
        //                paramUserOnMachine.ListSerialNumber = param.ListSerial;
        //                paramUserOnMachine.FullInfo = true;

        //                List<UserInfoOnMachine> lstUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

        //                IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
        //                commandParam.IsOverwriteData = false;
        //                commandParam.Action = CommandAction.DeleteACUser;
        //                commandParam.CommandName = StringHelper.GetCommandType(param.EmployeeType);
        //                commandParam.AuthenMode = string.Join(",", param.AuthenMode);
        //                commandParam.FromTime = new DateTime(2000, 1, 1);
        //                commandParam.ToTime = DateTime.Now;
        //                commandParam.ListEmployee = lstUser;
        //                commandParam.ListSerialNumber = lsSerialHw;
        //                commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
        //                commandParam.ExternalData = JsonConvert.SerializeObject(new
        //                {
        //                    AuthenModes = param.AuthenMode
        //                });

        //                List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);
        //                //var timeZoneIns = "000000000000000";
        //                foreach (var item in lstCmd)
        //                {
        //                    item.TimeZone = "1";
        //                    item.Group = "1";
        //                }


        //                if (lstCmd != null && lstCmd.Count() > 0)
        //                {
        //                    IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
        //                    groupCommand.CompanyIndex = user.CompanyIndex;
        //                    groupCommand.UserName = user.UserName;
        //                    groupCommand.ListCommand = lstCmd;
        //                    groupCommand.GroupName = GroupName.DeleteACUser.ToString();
        //                    groupCommand.EventType = "";
        //                    _iC_CommandLogic.CreateGroupCommands(groupCommand);
        //                    for (int i = 0; i < lstCmd.Count; i++)
        //                    {
        //                        CommandResult command = lstCmd[i];
        //                        // Add audit log
        //                        var audit = new IC_AuditEntryDTO(null)
        //                        {
        //                            TableName = "IC_SystemCommand",
        //                            UserName = user.UserName,
        //                            CompanyIndex = user.CompanyIndex,
        //                            State = AuditType.DeleteACUser,
        //                            Description = $"Xóa quyền truy cập {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
        //                            DescriptionEn = $"Delete access control {lstCmd[i].ListUsers?.Count ?? 0} users",
        //                            IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : null,
        //                            Name = user.FullName,
        //                            PageName = "DeleteACUser",
        //                            Status = AuditStatus.Unexecuted,
        //                            DateTime = DateTime.Now
        //                        };
        //                        _iIC_AuditLogic.Create(audit);
        //                    }
        //                }

        //                _IAC_UserMasterService.DeleteUserMaster(param.ListUser, new List<int> { device.Key }, user);
        //                //CheckFR05(user.CompanyIndex, lstCmd).GetAwaiter().GetResult();
        //                //List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, GlobalParams.CommandAction.UploadUsers, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
        //                //CreateGroupCommand(user.CompanyIndex, user.UserName, GlobalParams.CommandAction.UploadUsers.ToString(), lstCmd, "");
        //            }

        //        }
        //        else
        //        {
        //            return BadRequest("DeviceNotLicence");
        //        }
        //    }

        //    result = Ok();
        //    _notificationClient.SendNotificationToSDKInterfaceAsync(param.ListSerial);
        //    return result;
        //}

        internal List<CommandResult> CreateListACCommand(EPAD_Context context, List<string> listSerial, CommandAction pAction, DateTime pFromTime, DateTime pToTime, List<UserInfoOnMachine> pListUsers, List<AC_AccGroup> accGroups, List<AC_AccHoliday> accHolidays, List<AC_TimeZone> timeZones, IC_CommandRequestDTO commandRequest, bool isOverwriteData, short privilege, string externalData = "")
        {
            return CommandProcess.CreateListACCommands(context, listSerial, pAction, externalData, pFromTime, pToTime, pListUsers, accGroups, accHolidays, timeZones, isOverwriteData, commandRequest, privilege);
        }

        internal void CreateGroupCommand(int pCompanyIndex, string pUserName, string pGroupName, List<CommandResult> pListCommands, string pEventType, string externalData = "")
        {
            CommandProcess.CreateGroupCommand(_context, cache, pCompanyIndex, pUserName, pGroupName, externalData, pListCommands, pEventType);
        }

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
    }
}
