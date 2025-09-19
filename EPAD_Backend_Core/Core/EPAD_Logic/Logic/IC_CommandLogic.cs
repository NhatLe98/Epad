using EPAD_Common.Extensions;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic.MainProcess;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Logic
{
    public class IC_CommandLogic : IIC_CommandLogic
    {
        public EPAD_Context _dbContext;
        private ezHR_Context _integrateContext;
        private readonly ILogger _logger;
        private static IMemoryCache cache;
        private ConfigObject _config;
        private IIC_EmployeeLogic _iIC_EmployeeLogic;
        private IIC_EmployeeTransferLogic _IIC_EmployeeTransferLogic;
        private IIC_WorkingInfoLogic _IIC_WorkingInfoLogic;
        private IHR_WorkingInfoLogic _IHR_WorkingInfoLogic;
        private IHR_EmployeeLogic _IHR_EmployeeLogic;
        private IIC_UserMasterLogic _iC_UserMasterLogic;
        private IIC_AuditLogic _iIC_AuditLogic;

        public IC_CommandLogic(IMemoryCache _cache, IIC_EmployeeLogic iIC_EmployeeLogic,
            IIC_WorkingInfoLogic iIC_WorkingInfoLogic,
            EPAD_Context dbContext, ezHR_Context integrateContext,
            IHR_WorkingInfoLogic iHR_WorkingInfoLogic, IHR_EmployeeLogic iHR_EmployeeLogic,
            IIC_EmployeeTransferLogic iIC_EmployeeTransferLogic, ILoggerFactory loggerFactory,
            IIC_UserMasterLogic iC_UserMasterLogic, IIC_AuditLogic iC_AuditLogic)
        {
            cache = _cache;
            _dbContext = dbContext;
            _config = ConfigObject.GetConfig(cache);
            _logger = loggerFactory.CreateLogger<IC_CommandLogic>();
            _integrateContext = integrateContext;
            _iIC_EmployeeLogic = iIC_EmployeeLogic;
            _IIC_WorkingInfoLogic = iIC_WorkingInfoLogic;
            _IHR_WorkingInfoLogic = iHR_WorkingInfoLogic;
            _IHR_EmployeeLogic = iHR_EmployeeLogic;
            _IIC_EmployeeTransferLogic = iIC_EmployeeTransferLogic;
            _iC_UserMasterLogic = iC_UserMasterLogic;
            _iIC_AuditLogic = iC_AuditLogic;
        }

        public void TransferUser(List<IC_EmployeeTransferDTO> lstEmployee)
        {
            var lstCmd = new List<CommandResult>();
            var lstTransfer = lstEmployee.Where(u => u.TemporaryTransfer == true).ToList();
            if (lstTransfer.Count > 0)
            {
                lstCmd = CreateListCommandTransferUser_EPAD(lstTransfer);
            }

            lstTransfer = lstEmployee.Where(u => u.TemporaryTransfer == false).ToList();
            if (lstTransfer.Count > 0)
            {
                lstCmd.AddRange(CreateListCommandChangeDepartment_EPAD(lstTransfer));
            }
            _dbContext.SaveChanges();

            if (lstCmd.Count > 0)
            {
                var groupComParam = new IC_GroupCommandParamDTO();
                groupComParam.CompanyIndex = lstEmployee[0].CompanyIndex;
                groupComParam.ListCommand = lstCmd;
                groupComParam.GroupName = GroupName.TransferEmployee.ToString();
                groupComParam.ExternalData = "";
                groupComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                groupComParam.EventType = ConfigAuto.ADD_OR_DELETE_USER.ToString();
                CreateGroupCommands(groupComParam);
                //CreateGroupCommand(lstEmployee[0].CompanyIndex, UpdatedUser.SYSTEM_AUTO.ToString(), GroupName.TransferEmployee.ToString(), "", lstCmd, GlobalParams.ConfigAuto.ADD_OR_DELETE_USER.ToString());
            }
        }

        // TODO
        public void SyncTransferEmloyee(List<IC_EmployeeTransferDTO> lstEmployee)
        {
            DateTime toTime = DateTime.Now;
            DateTime fromTime = toTime;
            fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
            toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);

            var listCommand = new List<CommandResult>();
            var configFile = ConfigObject.GetConfig(cache);

            var lstTransfer = lstEmployee.Where(u => u.TemporaryTransfer == true).ToList();

            var addedParams = new List<AddedParam>();
            if (configFile.IntegrateDBOther == false)
            {
                if (lstTransfer.Count > 0)
                {
                    addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = lstTransfer[0].CompanyIndex });
                    addedParams.Add(new AddedParam { Key = "IsCurrentTransfer", Value = true });
                    addedParams.Add(new AddedParam { Key = "ListEmployeeID", Value = lstTransfer.Select(U => U.EmployeeATID).ToList() });
                    _IIC_EmployeeTransferLogic.GetMany(addedParams);
                }
            }

            if (listCommand.Count > 0)
            {
                var groupComParam = new IC_GroupCommandParamDTO();
                groupComParam.CompanyIndex = lstEmployee[0].CompanyIndex;
                groupComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                groupComParam.ListCommand = listCommand;
                groupComParam.GroupName = GroupName.TransferEmployee.ToString();
                groupComParam.ExternalData = "";
                groupComParam.EventType = ConfigAuto.ADD_OR_DELETE_USER.ToString();
                CreateGroupCommands(groupComParam);
                //CreateGroupCommand(lstEmployee[0].CompanyIndex, UpdatedUser.SYSTEM_AUTO.ToString(), GroupName.TransferEmployee.ToString(), "", listCommand, GlobalParams.ConfigAuto.ADD_OR_DELETE_USER.ToString());
            }
        }

        public void SyncWithDepartmentAndDevice(List<string> listSerialNumber, int departmentIndex, int companyIndex)
        {
            DateTime toTime = DateTime.Now;
            DateTime fromTime = toTime;
            fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
            toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);

            var configFile = ConfigObject.GetConfig(cache);
            var listCommand = new List<CommandResult>();
            var addedParams = new List<AddedParam>();

            if (configFile.IntegrateDBOther == false)
            {
                // Get all user of deparment 
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = companyIndex });
                addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
                addedParams.Add(new AddedParam { Key = "DepartmentIndex", Value = departmentIndex });

                var listWorkingInfo = _IIC_WorkingInfoLogic.GetMany(addedParams).ToList();

                if (listWorkingInfo != null && listWorkingInfo.Count > 0)
                {
                    var lstUser = new List<UserInfoOnMachine>();

                    // Create list user for list device
                    var paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = listWorkingInfo.Select(u => u.EmployeeATID).ToList();
                    paramUserOnMachine.CompanyIndex = companyIndex;
                    paramUserOnMachine.AuthenMode = "";
                    paramUserOnMachine.FullInfo = true;
                    lstUser = GetListUserInfoOnMachine(paramUserOnMachine);
                    var commandParam = new IC_CommandParamDTO();
                    commandParam.ListSerialNumber = listSerialNumber;
                    commandParam.Action = CommandAction.UploadUsers;
                    commandParam.ExternalData = "";
                    commandParam.FromTime = fromTime;
                    commandParam.ToTime = toTime;
                    commandParam.ListEmployee = lstUser;
                    commandParam.IsOverwriteData = false;
                    commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                    var lstCmdAddUser = CreateListCommands(commandParam);
                    //List<CommandResult> lstCmdAddUser = CommandProcess.CreateListCommands(_dbContext, listSerialNumber, GlobalParams.CommandAction.UploadUsers, "", fromTime, toTime, lstUser, false);
                    listCommand.AddRange(lstCmdAddUser);
                }
            }
            else
            {
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = configFile.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
                addedParams.Add(new AddedParam { Key = "DepartmentIndex", Value = departmentIndex });

                var listWorkingInfo = _IHR_WorkingInfoLogic.GetMany(addedParams).ToList();
                if (listWorkingInfo != null && listWorkingInfo.Count > 0)
                {
                    var lstUser = new List<UserInfoOnMachine>();
                    var paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = listWorkingInfo.Select(u => u.EmployeeATID).ToList();
                    paramUserOnMachine.CompanyIndex = configFile.CompanyIndex;
                    paramUserOnMachine.AuthenMode = "";
                    paramUserOnMachine.FullInfo = true;
                    lstUser = GetListUserInfoOnMachine(paramUserOnMachine);

                    var commandParam = new IC_CommandParamDTO();
                    commandParam.ListSerialNumber = listSerialNumber;
                    commandParam.Action = CommandAction.UploadUsers;
                    commandParam.ExternalData = "";
                    commandParam.FromTime = fromTime;
                    commandParam.ToTime = toTime;
                    commandParam.ListEmployee = lstUser;
                    commandParam.IsOverwriteData = false;
                    commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                    var lstCmdAddUser = CreateListCommands(commandParam);
                    // List<CommandResult> lstCmdAddUser = CreateListCommands( listSerialNumber, GlobalParams.CommandAction.UploadUsers, "", fromTime, toTime, lstUser, false, GlobalParams.DevicePrivilege.SDKStandardRole);
                    listCommand.AddRange(lstCmdAddUser);
                }
            }

            if (listCommand.Count > 0)
            {
                var groupComParam = new IC_GroupCommandParamDTO();
                groupComParam.CompanyIndex = companyIndex;
                groupComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                groupComParam.ListCommand = listCommand;
                groupComParam.GroupName = GroupName.TransferEmployee.ToString();
                groupComParam.ExternalData = "";
                groupComParam.EventType = ConfigAuto.ADD_OR_DELETE_USER.ToString();
                CreateGroupCommands(groupComParam);
                //CreateGroupCommand(companyIndex, UpdatedUser.SYSTEM_AUTO.ToString(), GroupName.TransferEmployee.ToString(), "", listCommand, GlobalParams.ConfigAuto.ADD_OR_DELETE_USER.ToString());
            }
            _dbContext.SaveChanges();
        }

        public async Task SyncWithEmployee(List<string> listEmployeeIndex, int companyIndex, string currentSerialNumber = null)
        {
            var toTime = DateTime.Now;
            var fromTime = DateTime.Now;
            fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
            toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);

            var configFile = ConfigObject.GetConfig(cache);
            var listCommand = new List<CommandResult>();
            var addedParams = new List<AddedParam>();

            if (configFile.IntegrateDBOther == false)
            {
                // Create Command for delete employee on old department
                listCommand.AddRange(CreateCommandForOldDepartment(listEmployeeIndex, companyIndex, currentSerialNumber, fromTime, toTime));

                // Create Command for new department
                listCommand.AddRange(CreateCommandForNewDepartment(listEmployeeIndex, companyIndex, currentSerialNumber, fromTime, toTime));
            }
            else
            {
                // TODO Need re-implement this function
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = configFile.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
                addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeIndex });

                var listWorkingInfo = _IHR_WorkingInfoLogic.GetMany(addedParams).ToList();
                if (listWorkingInfo != null && listWorkingInfo.Count > 0)
                {
                    foreach (var working in listWorkingInfo)
                    {
                        var listSerialNumber = _dbContext.IC_DepartmentAndDevice.Where(u => u.DepartmentIndex == working.DepartmentIndex).ToList();

                        var listEmpIndex = new List<string>();
                        listEmpIndex.Add(working.EmployeeATID);

                        var lstUser = new List<UserInfoOnMachine>();
                        var paramUserOnMachine = new IC_UserinfoOnMachineParam
                        {
                            ListEmployeeaATID = listEmpIndex,
                            CompanyIndex = configFile.CompanyIndex,
                            AuthenMode = "",
                            FullInfo = true
                        };
                        lstUser = GetListUserInfoOnMachine(paramUserOnMachine);

                        var commandParam = new IC_CommandParamDTO
                        {
                            ListSerialNumber = listSerialNumber.Select(u => u.SerialNumber).ToList(),
                            Action = CommandAction.UploadUsers,
                            ExternalData = working.Index.ToString(),
                            FromTime = fromTime,
                            ToTime = toTime,
                            ListEmployee = lstUser,
                            IsOverwriteData = false,
                            Privilege = GlobalParams.DevicePrivilege.SDKStandardRole
                        };
                        var lstCmdAddUser = CreateListCommands(commandParam);
                        //List<CommandResult> lstCmdAddUser = CommandProcess.CreateListCommands(_dbContext, listSerialNumber.Select(u => u.SerialNumber).ToList(), GlobalParams.CommandAction.UploadUsers, working.Index.ToString(), fromTime, toTime, lstUser, false, GlobalParams.DevicePrivilege.SDKStandardRole);
                        listCommand.AddRange(lstCmdAddUser);
                    }
                }
            }
            if (listCommand.Count > 0)
            {
                var groupComParam = new IC_GroupCommandParamDTO
                {
                    CompanyIndex = companyIndex,
                    ListCommand = listCommand,
                    GroupName = GroupName.TransferEmployee.ToString(),
                    ExternalData = "",
                    EventType = ConfigAuto.ADD_OR_DELETE_USER.ToString()
                };
                CreateGroupCommands(groupComParam);
                //CreateGroupCommand(companyIndex, UpdatedUser.SYSTEM_AUTO.ToString(), GroupName.TransferEmployee.ToString(), "", listCommand, GlobalParams.ConfigAuto.ADD_OR_DELETE_USER.ToString());
            }
            await _dbContext.SaveChangesAsync();
        }

        private List<CommandResult> CreateCommandForOldDepartment(List<string> listEmployeeIndex, int companyIndex, string currentSerialNumber, DateTime fromDate, DateTime toDate)
        {
            var lstCmd = new List<CommandResult>();

            // get all workinginfo of all employee need to sync
            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = companyIndex });
            addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeIndex });
            var listWorkingInfo = _IIC_WorkingInfoLogic.GetMany(addedParams).ToList();

            foreach (var empATID in listEmployeeIndex)
            {
                var listWorkingByEmp = listWorkingInfo.FindAll(t => t.EmployeeATID == empATID).OrderBy(t => t.FromDate).ToList();
                if (listWorkingByEmp.Count == 0) continue;

                if (listWorkingByEmp.Count > 1)
                {
                    // Tim ra current working department
                    var currentWorking = listWorkingInfo.FindAll(u => u.Status == (short)TransferStatus.Approve
                                                        && ((u.ToDate == null && DateTime.Today.Date >= u.FromDate.Date)
                                                            || u.ToDate != null && DateTime.Today.Date >= u.FromDate.Date && DateTime.Today.Date <= u.ToDate.Value.Date)).OrderByDescending(t => t.FromDate).FirstOrDefault();

                    if (currentWorking != null)
                    {
                        //tìm phòng ban cũ 
                        var workingOld = listWorkingInfo.FindAll(t => t.FromDate.Date < currentWorking.FromDate.Date
                                                       && t.Index != currentWorking.Index).OrderByDescending(t => t.FromDate).FirstOrDefault();

                        if (workingOld == null)
                        {
                            continue;
                        }
                        // nếu phòng ban cũ = phòng ban mới --> finish
                        if (currentWorking.DepartmentIndex == workingOld.DepartmentIndex)
                        {
                            continue;
                        }

                        var listCmdTemp = new List<CommandResult>();
                        //xóa phòng ban cũ
                        if (workingOld != null)
                        {
                            var lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == companyIndex && x.DepartmentIndex == workingOld.DepartmentIndex && x.SerialNumber != currentSerialNumber).Select(x => x.SerialNumber).ToList();

                            var lstUser = new List<UserInfoOnMachine>();
                            var paramUserOnMachine = new IC_UserinfoOnMachineParam();
                            paramUserOnMachine.ListEmployeeaATID = new List<string>() { workingOld.EmployeeATID };
                            paramUserOnMachine.CompanyIndex = companyIndex;
                            paramUserOnMachine.AuthenMode = "";
                            paramUserOnMachine.FullInfo = true;
                            lstUser = GetListUserInfoOnMachine(paramUserOnMachine);
                            // create delete employee on device on old department
                            var commandParam = new IC_CommandParamDTO();
                            commandParam.ListSerialNumber = lstSerial;
                            commandParam.Action = CommandAction.DeleteUserById;
                            commandParam.ExternalData = "";
                            commandParam.FromTime = fromDate;
                            commandParam.ToTime = toDate;
                            commandParam.ListEmployee = lstUser;
                            commandParam.IsOverwriteData = false;
                            commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            listCmdTemp = CreateListCommands(commandParam);

                            // listCmdTemp = CommandProcess.CreateListCommands(_dbContext, lstSerial, GlobalParams.CommandAction.DeleteUserById, "", fromDate, toDate, lstUser, false, GlobalParams.DevicePrivilege.SDKStandardRole);
                        }
                        if (listCmdTemp.Count > 0)
                        {
                            lstCmd.AddRange(listCmdTemp);
                        }
                    }
                }
            }
            return lstCmd;
        }
        private List<CommandResult> CreateCommandForNewDepartment(List<string> listEmployeeIndex, int companyIndex, string currentSerialNumber, DateTime fromDate, DateTime toDate)
        {
            var listCommand = new List<CommandResult>();
            var addedParams = new List<AddedParam>();
            var listSerial = new List<EmployeeSerial>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = companyIndex });
            addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
            addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeIndex });
            var listWorkingInfo = _IIC_WorkingInfoLogic.GetMany(addedParams).ToList();
            var listSerials = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == companyIndex).ToList();
            if (listWorkingInfo != null && listWorkingInfo.Count > 0)
            {
                foreach (var working in listWorkingInfo)
                {
                    var listSerialNumber = listSerials.Where(u => u.DepartmentIndex == working.DepartmentIndex && u.SerialNumber != currentSerialNumber).Select(e => e.SerialNumber).ToList();
                    //if (listSerialNumber == null || listSerialNumber.Count() == 0) {
                    //    listSerialNumber = _dbContext.IC_Device.Select(e => e.SerialNumber).ToList();
                    //}
                    if (listSerialNumber.Count > 0)
                    {
                        listSerial.Add(new EmployeeSerial
                        {
                            EmployeeATID = working.EmployeeATID,
                            SerialNumber = listSerialNumber
                        });
                    }
                }
                var serialNumber = listSerial.Select(x => x.SerialNumber).SelectMany(i => i).Distinct().ToList();
                foreach (var working in serialNumber)
                {
                    var listEmpIndex = listSerial.Where(x => x.SerialNumber.Contains(working)).Select(x => x.EmployeeATID).Distinct().ToList();

                    var lstUser = new List<UserInfoOnMachine>();
                    var paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = listEmpIndex;
                    paramUserOnMachine.CompanyIndex = companyIndex;
                    paramUserOnMachine.AuthenMode = "";
                    paramUserOnMachine.FullInfo = true;
                    lstUser = GetListUserInfoOnMachine(paramUserOnMachine);

                    var commandParam = new IC_CommandParamDTO();
                    commandParam.ListSerialNumber = new List<string> { working };
                    commandParam.Action = CommandAction.UploadUsers;
                    commandParam.FromTime = fromDate;
                    commandParam.ToTime = toDate;
                    commandParam.ListEmployee = lstUser;
                    commandParam.IsOverwriteData = false;
                    commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                    var lstCmdAddUser = CreateListCommands(commandParam);

                    //List<CommandResult> lstCmdAddUser = CommandProcess.CreateListCommands(_dbContext, listSerialNumber.Select(u => u.SerialNumber).ToList(), GlobalParams.CommandAction.UploadUsers, working.Index.ToString(), fromDate, toDate, lstUser, false, GlobalParams.DevicePrivilege.SDKStandardRole);
                    listCommand.AddRange(lstCmdAddUser);
                }
                // TODO maybe need to update issync status here when add to new department
                if (listCommand.Count > 0)
                {
                    addedParams.Add(new AddedParam { Key = "IsSync", Value = false });
                    addedParams.Add(new AddedParam { Key = "UpdatedDate", Value = DateTime.Today });
                    addedParams.Add(new AddedParam { Key = "UpdatedUser", Value = UpdatedUser.SYSTEM_AUTO.ToString() });
                    _IIC_WorkingInfoLogic.UpdateList(listWorkingInfo, addedParams);
                }
            }
            return listCommand;
        }

        public class EmployeeSerial
        {
            public string EmployeeATID { get; set; }
            public List<string> SerialNumber { get; set; }
        }

        private List<CommandResult> CreateCommandForOldDepartment_IntegrateOtherDB(List<string> listEmployeeIndex, int companyIndex, DateTime fromDate, DateTime toDate)
        {
            var lstCmd = new List<CommandResult>();

            // get all workinginfo of all employee need to sync
            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = companyIndex });
            addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeIndex });
            var listWorkingInfo = _IHR_WorkingInfoLogic.GetMany(addedParams).ToList();

            foreach (var empATID in listEmployeeIndex)
            {
                var listWorkingByEmp = listWorkingInfo.FindAll(t => t.EmployeeATID == empATID).OrderBy(t => t.FromDate).ToList();
                if (listWorkingByEmp.Count == 0) continue;

                if (listWorkingByEmp.Count > 1)
                {
                    // Tim ra current working department
                    var currentWorking = listWorkingInfo.FindAll(u => ((u.ToDate == null && DateTime.Today.Date >= u.FromDate.Value.Date)
                                                            || u.ToDate != null && DateTime.Today.Date >= u.FromDate.Value.Date && DateTime.Today.Date <= u.ToDate.Value.Date)).OrderByDescending(t => t.FromDate.Value).FirstOrDefault();

                    if (currentWorking != null)
                    {
                        //tìm phòng ban cũ 
                        var workingOld = listWorkingInfo.FindAll(t => t.FromDate.Value.Date < currentWorking.FromDate.Value.Date
                                                       && t.Index != currentWorking.Index).OrderByDescending(t => t.FromDate.Value).FirstOrDefault();

                        if (workingOld == null)
                        {
                            continue;
                        }
                        // nếu phòng ban cũ = phòng ban mới --> finish
                        if (currentWorking.DepartmentIndex == workingOld.DepartmentIndex)
                        {
                            continue;
                        }

                        var listCmdTemp = new List<CommandResult>();
                        //xóa phòng ban cũ
                        if (workingOld != null)
                        {
                            var lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == companyIndex && x.DepartmentIndex == workingOld.DepartmentIndex).Select(x => x.SerialNumber).ToList();

                            var lstUser = new List<UserInfoOnMachine>();
                            var paramUserOnMachine = new IC_UserinfoOnMachineParam();
                            paramUserOnMachine.ListEmployeeaATID = new List<string>() { workingOld.EmployeeATID };
                            paramUserOnMachine.CompanyIndex = companyIndex;
                            paramUserOnMachine.AuthenMode = "";
                            paramUserOnMachine.FullInfo = true;
                            lstUser = GetListUserInfoOnMachine(paramUserOnMachine);
                            // create delete employee on device on old department
                            var commandParam = new IC_CommandParamDTO();
                            commandParam.ListSerialNumber = lstSerial;
                            commandParam.Action = CommandAction.DeleteUserById;
                            commandParam.ExternalData = "";
                            commandParam.FromTime = fromDate;
                            commandParam.ToTime = toDate;
                            commandParam.ListEmployee = lstUser;
                            commandParam.IsOverwriteData = false;
                            commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            listCmdTemp = CreateListCommands(commandParam);
                            //listCmdTemp = CommandProcess.CreateListCommands(_dbContext, lstSerial, GlobalParams.CommandAction.DeleteUserById, "", fromDate, toDate, lstUser, false, GlobalParams.DevicePrivilege.SDKStandardRole);
                        }
                        if (listCmdTemp.Count > 0)
                        {
                            lstCmd.AddRange(listCmdTemp);
                        }
                    }
                }
            }
            return lstCmd;
        }
        private List<CommandResult> CreateCommandForNewDepartment_IntegrateOtherDB(List<string> listEmployeeIndex, int companyIndex, DateTime fromDate, DateTime toDate)
        {

            var listCommand = new List<CommandResult>();
            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = companyIndex });
            addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
            addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeIndex });
            var listWorkingInfo = _IHR_WorkingInfoLogic.GetMany(addedParams).ToList();

            if (listWorkingInfo != null && listWorkingInfo.Count > 0)
            {
                foreach (var working in listWorkingInfo)
                {
                    var listSerialNumber = _dbContext.IC_DepartmentAndDevice.Where(u => u.DepartmentIndex == working.DepartmentIndex).ToList();

                    var listEmpIndex = new List<string>();
                    listEmpIndex.Add(working.EmployeeATID);

                    var lstUser = new List<UserInfoOnMachine>();
                    IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = listEmpIndex;
                    paramUserOnMachine.CompanyIndex = companyIndex;
                    paramUserOnMachine.AuthenMode = "";
                    paramUserOnMachine.FullInfo = true;
                    lstUser = GetListUserInfoOnMachine(paramUserOnMachine);

                    IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                    commandParam.ListSerialNumber = listSerialNumber.Select(u => u.SerialNumber).ToList();
                    commandParam.Action = CommandAction.UploadUsers;
                    commandParam.ExternalData = working.Index.ToString();
                    commandParam.FromTime = fromDate;
                    commandParam.ToTime = toDate;
                    commandParam.ListEmployee = lstUser;
                    commandParam.IsOverwriteData = false;
                    commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                    var lstCmdAddUser = CreateListCommands(commandParam);
                    //List<CommandResult> lstCmdAddUser = CommandProcess.CreateListCommands(_dbContext, listSerialNumber.Select(u => u.SerialNumber).ToList(), GlobalParams.CommandAction.UploadUsers, working.Index.ToString(), fromDate, toDate, lstUser, false, GlobalParams.DevicePrivilege.SDKStandardRole);
                    listCommand.AddRange(lstCmdAddUser);
                }

                // TODO maybe need to update issync status here when add to new department
                if (listCommand.Count > 0)
                {
                    addedParams.Add(new AddedParam { Key = "IsSync", Value = false });
                    addedParams.Add(new AddedParam { Key = "UpdatedDate", Value = DateTime.Today });
                    addedParams.Add(new AddedParam { Key = "UpdatedUser", Value = UpdatedUser.SYSTEM_AUTO.ToString() });
                    _IHR_WorkingInfoLogic.UpdateList(listWorkingInfo, addedParams);
                }
            }
            return listCommand;
        }

        private List<CommandResult> CreateListCommandTransferUser_EPAD(List<IC_EmployeeTransferDTO> lstEmployee)
        {
            DateTime toTime = DateTime.Now;
            DateTime fromTime = toTime;

            fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
            toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);

            var lstCmd = new List<CommandResult>();

            var lstTransfer = _dbContext.IC_EmployeeTransfer.Where(x => x.CompanyIndex == lstEmployee[0].CompanyIndex && x.FromTime >= fromTime && x.FromTime <= toTime).ToList();
            lstTransfer = lstTransfer.Where(u => lstEmployee.Where(t => t.EmployeeATID.Contains(u.EmployeeATID)).Count() > 0).ToList();

            foreach (IC_EmployeeTransfer emp in lstTransfer)
            {
                if (emp.RemoveFromOldDepartment)
                {
                    var lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == lstEmployee[0].CompanyIndex && x.DepartmentIndex == emp.OldDepartment).Select(x => x.SerialNumber).ToList();
                    var addOrDeleteUserConfig = _dbContext.IC_Config.FirstOrDefault(t => t.CompanyIndex == lstEmployee[0].CompanyIndex
              && t.EventType == ConfigAuto.ADD_OR_DELETE_USER.ToString());
                    var param = JsonConvert.DeserializeObject<IntegrateLogParam>(addOrDeleteUserConfig.CustomField);
                    if (param.IntegrateWhenNotInclareDepartment == true && lstSerial.Count == 0)
                    {
                        var serialNumbers = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == lstEmployee[0].CompanyIndex).Select(x => x.SerialNumber).ToList();
                        lstSerial = _dbContext.IC_Device.Where(x => !serialNumbers.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                    }
                    var lsSerialApp = new List<string>();
                    ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp);
                    lstSerial = lsSerialApp;

                    //List<string> lsSerialApp = new List<string>();
                    var lstUser = new List<UserInfoOnMachine>();
                    if (lstSerial.Count > 0)
                    {
                        var paramUserOnMachine = new IC_UserinfoOnMachineParam();
                        paramUserOnMachine.ListEmployeeaATID = new List<string>() { emp.EmployeeATID };
                        paramUserOnMachine.CompanyIndex = lstEmployee[0].CompanyIndex;
                        paramUserOnMachine.AuthenMode = "";
                        paramUserOnMachine.ListSerialNumber = lstSerial;
                        paramUserOnMachine.FullInfo = true;
                        lstUser = GetListUserInfoOnMachine(paramUserOnMachine);
                    }
                    var commandParam = new IC_CommandParamDTO();
                    commandParam.ListSerialNumber = lstSerial;
                    commandParam.ListEmployee = lstUser;
                    commandParam.Action = CommandAction.DeleteUserById;
                    commandParam.FromTime = fromTime;
                    commandParam.ToTime = toTime;
                    commandParam.ExternalData = "";
                    commandParam.IsOverwriteData = false;
                    commandParam.Privilege = 0;
                    commandParam.ExternalData = emp.EmployeeATID;
                    var lstCmdRemoveUser = CreateListCommands(commandParam);
                    // List<CommandResult> lstCmdRemoveUser = CreateListCommand(lstSerial, GlobalParams.CommandAction.DeleteUserById, fromTime, toTime, lstUser, "");
                    lstCmd.AddRange(lstCmdRemoveUser);
                }
                if (emp.AddOnNewDepartment)
                {
                    var lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == lstEmployee[0].CompanyIndex && x.DepartmentIndex == emp.NewDepartment).Select(x => x.SerialNumber).ToList();
                    var addOrDeleteUserConfig = _dbContext.IC_Config.FirstOrDefault(t => t.CompanyIndex == lstEmployee[0].CompanyIndex
              && t.EventType == ConfigAuto.ADD_OR_DELETE_USER.ToString());
                    var param = JsonConvert.DeserializeObject<IntegrateLogParam>(addOrDeleteUserConfig.CustomField);
                    if (param.IntegrateWhenNotInclareDepartment == true && lstSerial.Count == 0)
                    {
                        var serialNumbers = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == lstEmployee[0].CompanyIndex).Select(x => x.SerialNumber).ToList();
                        lstSerial = _dbContext.IC_Device.Where(x => !serialNumbers.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                    }
                    var lsSerialApp = new List<string>();
                    ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp);
                    lstSerial = lsSerialApp;
                    //List<string> lsSerialApp = new List<string>();
                    var lstUser = new List<UserInfoOnMachine>();
                    if (lstSerial.Count > 0)
                    {
                        var paramUserOnMachine = new IC_UserinfoOnMachineParam();
                        paramUserOnMachine.ListEmployeeaATID = new List<string>() { emp.EmployeeATID };
                        paramUserOnMachine.CompanyIndex = lstEmployee[0].CompanyIndex;
                        paramUserOnMachine.AuthenMode = "";
                        paramUserOnMachine.ListSerialNumber = lstSerial;
                        paramUserOnMachine.FullInfo = true;
                        lstUser = GetListUserInfoOnMachine(paramUserOnMachine);
                    }
                    var commandParam = new IC_CommandParamDTO();
                    commandParam.ListSerialNumber = lstSerial;
                    commandParam.ListEmployee = lstUser;
                    commandParam.Action = CommandAction.UploadUsers;
                    commandParam.FromTime = fromTime;
                    commandParam.ToTime = toTime;
                    commandParam.ExternalData = "";
                    commandParam.IsOverwriteData = false;
                    commandParam.Privilege = 0;
                    commandParam.ExternalData = emp.EmployeeATID;
                    var lstCmdAddUser = CreateListCommands(commandParam);

                    //CreateListCommand(lstSerial, GlobalParams.CommandAction.UploadUsers, fromTime, toTime, lstUser, "");
                    lstCmd.AddRange(lstCmdAddUser);
                }

                emp.IsSync = false;
                emp.UpdatedDate = DateTime.Now;
                emp.UpdatedUser = UpdatedUser.SYSTEM_AUTO.ToString();
                _dbContext.SaveChanges();
            }
            return lstCmd;
        }

        private List<CommandResult> CreateListCommandChangeDepartment_EPAD(List<IC_EmployeeTransferDTO> lstEmployee)
        {
            DateTime toTime = DateTime.Now;
            DateTime fromTime = toTime;

            fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
            toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);

            var listEmployeeATID = lstEmployee.Select(u => u.EmployeeATID).ToList();

            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = lstEmployee[0].CompanyIndex });
            addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeATID });
            var listEmployee = _iIC_EmployeeLogic.GetEmployeeList(addedParams);

            var listWorkingInfo = _dbContext.IC_WorkingInfo.Where(u => u.CompanyIndex == lstEmployee[0].CompanyIndex && listEmployeeATID.Contains(u.EmployeeATID)).ToList();

            var listWorkingConvert = ConvertIC_WorkingInfoToWorkingInfoObject(listWorkingInfo);

            var lstCmd = CreateListCommandFromWorkingInfoData(listEmployeeATID, listWorkingConvert, toTime, lstEmployee[0].CompanyIndex, fromTime, toTime);

            //update IC_WorkingInfo 
            for (int i = 0; i < listWorkingConvert.Count; i++)
            {
                var workingInfoDB = listWorkingInfo.Where(x => x.Index.ToString() == listWorkingConvert[i].Index).FirstOrDefault();
                if (workingInfoDB != null)
                {
                    workingInfoDB.IsSync = listWorkingConvert[i].IsSync;
                }
            }
            return lstCmd;
        }

        private List<CommandResult> CreateListCommandChangeDepartment_EPAD(List<IC_EmployeeDTO> lstEmployee)
        {
            DateTime toTime = DateTime.Now;
            DateTime fromTime = toTime;

            fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
            toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);

            var listEmployeeATID = lstEmployee.Select(u => u.EmployeeATID).ToList();

            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = lstEmployee[0].CompanyIndex });
            addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeATID });
            var listEmployee = _iIC_EmployeeLogic.GetEmployeeList(addedParams);

            var listWorkingInfo = _dbContext.IC_WorkingInfo.Where(u => u.CompanyIndex == lstEmployee[0].CompanyIndex && listEmployeeATID.Contains(u.EmployeeATID)).ToList();

            var listWorkingConvert = ConvertIC_WorkingInfoToWorkingInfoObject(listWorkingInfo);

            var lstCmd = CreateListCommandFromWorkingInfoData(listEmployeeATID, listWorkingConvert, toTime, lstEmployee[0].CompanyIndex, fromTime, toTime);

            //update IC_WorkingInfo 
            for (int i = 0; i < listWorkingConvert.Count; i++)
            {
                var workingInfoDB = listWorkingInfo.Where(x => x.Index.ToString() == listWorkingConvert[i].Index).FirstOrDefault();
                if (workingInfoDB != null)
                {
                    workingInfoDB.IsSync = listWorkingConvert[i].IsSync;
                }
            }

            return lstCmd;
        }

        private List<CommandResult> CreateListCommandTransferUser_Integrate(DateTime pNow, ConfigObject pConfigFile)
        {
            DateTime toTime = pNow;
            DateTime fromTime = toTime;

            fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
            toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);

            var listEmployee = new List<IC_EmployeeDTO>();
            var addedParams = new List<AddedParam>();
            //using (var scope = _scopeFactory.CreateScope())
            //{
            //_IHR_EmployeeLogic = scope.ServiceProvider.GetRequiredService<IHR_EmployeeLogic>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = pConfigFile.CompanyIndex });
            listEmployee = _IHR_EmployeeLogic.GetMany(addedParams);
            // }


            var listEmpATIDs = listEmployee.Select(u => u.EmployeeATID).ToList();

            var listWorking = _integrateContext.HR_WorkingInfo.Where(t => t.CompanyIndex == pConfigFile.CompanyIndex
                && listEmpATIDs.Contains(t.EmployeeATID)).ToList();

            var listWorkingConvert = ConvertHR_WorkingInfoToWorkingInfoObject(listWorking);

            var lstCmd = CreateListCommandFromWorkingInfoData(listEmpATIDs, listWorkingConvert, pNow, pConfigFile.CompanyIndex, fromTime, toTime);

            //update HR_WorkingInfo 
            for (int i = 0; i < listWorkingConvert.Count; i++)
            {
                HR_WorkingInfo workingInfoDB = listWorking.FirstOrDefault(x => x.Index.ToString() == listWorkingConvert[i].Index);
                if (workingInfoDB != null)
                {
                    if (listWorkingConvert[i].IsSync == null)
                    {
                        workingInfoDB.Synched = null;
                    }
                    else
                    {
                        workingInfoDB.Synched = short.Parse(listWorkingConvert[i].IsSync.Value == false ? "0" : "1");
                    }
                }
            }


            return lstCmd;
        }

        private List<CommandResult> CreateCommandDeleteEmployeeStopped(DateTime pNow, IC_Config pConfig)
        {
            //List<HR_User> listEmp = _dbContext.HR_User.Where(t => t.CompanyIndex == pConfig.CompanyIndex
            //      && t.StoppedDate != null && t.StoppedDate.Value.Date <= pNow.Date).ToList();
            var listEmp = _dbContext.HR_User.Where(t => t.CompanyIndex == pConfig.CompanyIndex).ToList();
            var lstCmd = new List<CommandResult>();
            Dictionary<string, List<UserInfoOnMachine>> dicListUserByDevice = new Dictionary<string, List<UserInfoOnMachine>>();

            //foreach (HR_User emp in listEmp)
            //{
            //    List<string> lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pConfig.CompanyIndex && x.DepartmentIndex == emp.DepartmentIndex).Select(x => x.SerialNumber).ToList();
            //    if (lstSerial.Count == 0)
            //    {
            //        lstSerial = _dbContext.IC_Device.Where(t => t.CompanyIndex == pConfig.CompanyIndex).Select(t => t.SerialNumber).ToList();
            //    }
            //    List<string> lsSerialApp = new List<string>();
            //    ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp);
            //    List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
            //    if (lstSerial.Count > 0)
            //    {
            //        IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
            //        paramUserOnMachine.ListEmployeeaATID = new List<string>() { emp.EmployeeATID };
            //        paramUserOnMachine.CompanyIndex = pConfig.CompanyIndex;
            //        paramUserOnMachine.AuthenMode = "";
            //        paramUserOnMachine.ListSerialNumber = lstSerial;
            //        paramUserOnMachine.FullInfo = true;
            //        lstUser = GetListUserInfoOnMachine(paramUserOnMachine);
            //    }

            //    for (int i = 0; i < lstSerial.Count; i++)
            //    {
            //        if (dicListUserByDevice.ContainsKey(lstSerial[i]) == false)
            //        {
            //            dicListUserByDevice.Add(lstSerial[i], lstUser);
            //        }
            //        else
            //        {
            //            dicListUserByDevice[lstSerial[i]].AddRange(lstUser);
            //        }
            //    }
            //}

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
                commandParam.Privilege = 0;
                var listCmdTemp = CreateListCommands(commandParam);
                //List<CommandResult> listCmdTemp = CreateListCommand(new List<string>() { dicListUserByDevice.ElementAt(i).Key }, GlobalParams.CommandAction.DeleteUserById,
                //     DateTime.Now, DateTime.Now, dicListUserByDevice[dicListUserByDevice.ElementAt(i).Key], "");
                lstCmd.AddRange(listCmdTemp);
            }

            return lstCmd;
        }

        public bool ListSerialCheckHardWareLicense(List<string> lsSerial, ref List<string> lsSerialHw)
        {
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
        public List<CommandResult> CreateAutoListCommands(IC_CommandParamDTO commandParam, List<IC_ServiceAndDeviceDTO> listServiceAndDevice)
        {
            var listCommandService = new List<CommandResult>();
            var now = DateTime.Now;

            for (int i = 0; i < commandParam.ListSerialNumber.Count; i++)
            {
                var cmd = new CommandResult(commandParam.Action, CommandStatus.UnExecute);
                cmd.CommnadName = commandParam.CommandName;
                if (commandParam.Action == CommandAction.RESTART_SERVICE)
                {
                    cmd.SerialNumber = "Serial";
                    cmd.ExcutingServiceIndex = int.Parse(commandParam.ListSerialNumber[i]);
                }
                else
                {
                    cmd.SerialNumber = commandParam.ListSerialNumber[i];
                }
                // change privilege for push service only support 14 when using Push
                if (commandParam.ListEmployee != null && commandParam.ListEmployee.Count() > 0)
                {
                    if (listServiceAndDevice != null && listServiceAndDevice.Count() > 0)
                    {
                        var serviceType = listServiceAndDevice.FirstOrDefault(e => e.SerialNumber == commandParam.ListSerialNumber[i]);
                        if (serviceType != null)
                        {
                            if (serviceType != null && serviceType.ServiceType == GlobalParams.ServiceType.PUSHInterfaceService && commandParam.Privilege == GlobalParams.DevicePrivilege.SDKAdminRole)
                            {
                                commandParam.Privilege = GlobalParams.DevicePrivilege.PUSHAdminRole; // 14 is admin role using for push service update to device
                                foreach (var em in commandParam.ListEmployee)
                                {
                                    em.Privilege = commandParam.Privilege;
                                }
                            }
                            else
                            {
                                var listEmployeeAdmin = commandParam.ListEmployee.Where(x => x.Privilege == GlobalParams.DevicePrivilege.SDKAdminRole).Select(x => x.UserID).ToList();
                                if (serviceType != null && serviceType.ServiceType == GlobalParams.ServiceType.PUSHInterfaceService && listEmployeeAdmin != null && listEmployeeAdmin.Count() > 0)
                                {
                                    foreach (var item in listEmployeeAdmin)
                                    {
                                        var userAdmin = commandParam.ListEmployee.FirstOrDefault(x => x.UserID == item);
                                        if (userAdmin != null)
                                        {
                                            userAdmin.Privilege = GlobalParams.DevicePrivilege.PUSHAdminRole;
                                        }
                                    }
                                }
                            }


                        }
                    }
                }

                cmd.FromTime = commandParam.FromTime;
                cmd.ToTime = commandParam.ToTime;
                cmd.ListUsers = commandParam.ListEmployee;
                cmd.ExternalData = commandParam.ExternalData;
                cmd.CreatedTime = now;
                cmd.Status = CommandStatus.UnExecute.ToString();
                cmd.IsOverwriteData = commandParam.IsOverwriteData;
                if (commandParam.Action == CommandAction.UploadUsers || commandParam.Action == CommandAction.DeleteUserById)
                {
                    if (cmd.ListUsers != null && cmd.ListUsers.Count > 0)
                    {
                        listCommandService.Add(cmd);
                    }
                }
                else
                {
                    listCommandService.Add(cmd);
                }

            }

            return listCommandService;
        }
        public List<CommandResult> CreateListCommands(IC_CommandParamDTO commandParam)
        {
            var listCommandService = new List<CommandResult>();
            var now = DateTime.Now;

            for (int i = 0; i < commandParam.ListSerialNumber.Count; i++)
            {
                var listEmployee = ObjectExtensions.CopyToNewObject(commandParam.ListEmployee);
                var cmd = new CommandResult(commandParam.Action, CommandStatus.UnExecute);
                cmd.CommnadName = commandParam.CommandName;
                if (commandParam.Action == CommandAction.RESTART_SERVICE)
                {
                    cmd.SerialNumber = "Serial";
                    cmd.ExcutingServiceIndex = int.Parse(commandParam.ListSerialNumber[i]);
                }
                else
                {
                    cmd.SerialNumber = commandParam.ListSerialNumber[i];
                }
                // change privilege for push service only support 14 when using Push
                if (listEmployee != null && listEmployee.Count() > 0)
                {
                    var listServiceAndDevice = GetServiceType(commandParam.ListSerialNumber);
                    if (listServiceAndDevice != null && listServiceAndDevice.Count() > 0)
                    {
                        var serviceType = listServiceAndDevice.FirstOrDefault(e => e.SerialNumber == commandParam.ListSerialNumber[i]);
                        if (serviceType != null)
                        {
                            if (serviceType != null && serviceType.ServiceType == GlobalParams.ServiceType.PUSHInterfaceService && commandParam.Privilege == GlobalParams.DevicePrivilege.SDKAdminRole)
                            {
                                commandParam.Privilege = GlobalParams.DevicePrivilege.PUSHAdminRole; // 14 is admin role using for push service update to device
                                foreach (var em in listEmployee)
                                {
                                    em.Privilege = commandParam.Privilege;
                                }
                            }
                            else if (commandParam.Privilege == GlobalParams.DevicePrivilege.PUSHAdminRole || commandParam.Privilege == GlobalParams.DevicePrivilege.SDKAdminRole || commandParam.Privilege == GlobalParams.DevicePrivilege.SDKUserRegisterRole)
                            {
                                var listEmployeeAdmin = listEmployee.Where(x => x.Privilege == GlobalParams.DevicePrivilege.SDKAdminRole || x.Privilege == GlobalParams.DevicePrivilege.PUSHAdminRole || x.Privilege == GlobalParams.DevicePrivilege.SDKUserRegisterRole).Select(x => x.UserID).ToList();
                                if (serviceType != null && serviceType.ServiceType == GlobalParams.ServiceType.PUSHInterfaceService && listEmployeeAdmin != null && listEmployeeAdmin.Count() > 0)
                                {
                                    foreach (var item in listEmployeeAdmin)
                                    {
                                        var userAdmin = listEmployee.FirstOrDefault(x => x.UserID == item);
                                        if (userAdmin != null)
                                        {
                                            userAdmin.Privilege = GlobalParams.DevicePrivilege.PUSHAdminRole;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var listEmployeeAdmin = listEmployee.Where(x => x.Privilege
                                    == GlobalParams.DevicePrivilege.SDKAdminRole).Select(x => x.UserID).ToList();
                                foreach (var item in listEmployee.ToList())
                                {
                                    //item.Privilege = commandParam.Privilege;
                                    if (serviceType != null && serviceType.ServiceType == GlobalParams.ServiceType.PUSHInterfaceService
                                        && (item.Privilege == GlobalParams.DevicePrivilege.PUSHAdminRole
                                        || item.Privilege == GlobalParams.DevicePrivilege.SDKAdminRole
                                        || item.Privilege == GlobalParams.DevicePrivilege.SDKUserRegisterRole))
                                    {
                                        item.Privilege = GlobalParams.DevicePrivilege.PUSHAdminRole;
                                    }
                                }
                            }
                        }
                    }
                }

                cmd.FromTime = commandParam.FromTime;
                cmd.ToTime = commandParam.ToTime;
                cmd.ListUsers = listEmployee;
                cmd.ExternalData = commandParam.ExternalData;
                cmd.CreatedTime = now;
                cmd.Status = CommandStatus.UnExecute.ToString();
                cmd.IsOverwriteData = commandParam.IsOverwriteData;
                listCommandService.Add(cmd);
            }

            return listCommandService;
        }

        public void CreateGroupCommands(IC_GroupCommandParamDTO groupComParam)
        {
            var now = DateTime.Now;
            int groupIndex = 0;
            var listSerial = new List<string>();
            for (int i = 0; i < groupComParam.ListCommand.Count; i++)
            {
                listSerial.Add(groupComParam.ListCommand[i].SerialNumber);
            }
            #region create group on database
            var groupModel = new IC_CommandSystemGroup();
            groupModel.GroupName = groupComParam.GroupName;
            groupModel.Excuted = false;

            groupModel.EventType = groupComParam.EventType;
            groupModel.ExternalData = groupComParam.ExternalData;
            groupModel.CreatedDate = now;
            groupModel.UpdatedDate = now;
            groupModel.UpdatedUser = groupComParam.UserName;
            groupModel.CompanyIndex = groupComParam.CompanyIndex;

            _dbContext.IC_CommandSystemGroup.Add(groupModel);
            _dbContext.SaveChanges();
            groupIndex = groupModel.Index;
            #endregion
            #region create command on database

            var listDevice = _dbContext.IC_Device.Where(t => t.CompanyIndex == groupComParam.CompanyIndex && listSerial.Contains(t.SerialNumber)).ToList();
            var listCommand = new List<IC_SystemCommand>();

            for (int i = 0; i < groupComParam.ListCommand.Count; i++)
            {
                var device = listDevice.Find(t => t.SerialNumber == groupComParam.ListCommand[i].SerialNumber);
                if (device != null && device.IPAddress != null && device.IPAddress != "")
                {
                    var param = new CommandParamDB();
                    param.IPAddress = device.IPAddress;
                    param.Port = device.Port == null ? 4370 : device.Port.Value;
                    param.FromTime = groupComParam.ListCommand[i].FromTime;
                    param.ToTime = groupComParam.ListCommand[i].ToTime;
                    param.ListUsers = groupComParam.ListCommand[i].ListUsers;
                    param.AccHolidays = groupComParam.ListCommand[i].AccHolidays;
                    param.AccGroups = groupComParam.ListCommand[i].AccGroups;
                    param.TimeZones = groupComParam.ListCommand[i].TimeZones;
                    param.TimeZone = groupComParam.ListCommand[i].TimeZone;
                    param.Group = groupComParam.ListCommand[i].Group;

                    var command = CreateCommandModel(groupComParam.ListCommand[i].SerialNumber, groupComParam.ListCommand[i].Command, groupComParam.ListCommand[i].CommnadName,
                        param, groupIndex, 0, now, groupComParam.UserName, groupComParam.CompanyIndex, groupComParam.ListCommand[i].ExternalData, groupComParam.ListCommand[i].IsOverwriteData);
                    _dbContext.IC_SystemCommand.Add(command);
                    _dbContext.SaveChanges();
                    listCommand.Add(command);

                    groupComParam.ListCommand[i].IPAddress = device.IPAddress;
                    groupComParam.ListCommand[i].Port = device.Port == null ? 4370 : device.Port.Value;
                    groupComParam.ListCommand[i].GroupIndex = groupIndex.ToString();
                    groupComParam.ListCommand[i].ID = command.Index.ToString();
                }
                else
                {
                    // tạo command restart service
                    if (groupComParam.ListCommand[i].ExcutingServiceIndex != 0)
                    {
                        var command = CreateCommandModel(groupComParam.ListCommand[i].SerialNumber, groupComParam.ListCommand[i].Command, groupComParam.ListCommand[i].CommnadName,
                            new CommandParamDB(), groupIndex, groupComParam.ListCommand[i].ExcutingServiceIndex, now, groupComParam.UserName, groupComParam.CompanyIndex, groupComParam.ListCommand[i].ExternalData, groupComParam.ListCommand[i].IsOverwriteData);
                        _dbContext.IC_SystemCommand.Add(command);
                        _dbContext.SaveChanges();
                        listCommand.Add(command);

                        groupComParam.ListCommand[i].IPAddress = "0";
                        groupComParam.ListCommand[i].Port = 100;
                        groupComParam.ListCommand[i].GroupIndex = groupIndex.ToString();
                        groupComParam.ListCommand[i].ID = command.Index.ToString();
                    }
                }
            }

            #endregion
            #region add command to cache
            var groupCommand = new CommandGroup();
            groupCommand.ID = groupIndex.ToString();
            groupCommand.Name = groupComParam.GroupName;
            groupCommand.Excuted = false;
            groupCommand.ListCommand = groupComParam.ListCommand;
            groupCommand.EventType = groupComParam.EventType;

            var companyInfo = CompanyInfo.GetFromCache(cache, groupComParam.CompanyIndex.ToString());
            // lấy ds service đang login
            var listUser = companyInfo.GetListUserInfoIsService(cache);


            // cập nhật command cho các service ql
            groupComParam.ListCommand = RemoveCommandIsNullIPAddress(groupComParam.ListCommand);
            UpdateCommandForService(listUser, groupComParam.ListCommand);
            //cập nhật group command theo company
            AddGroupCommandsForCompany(companyInfo, groupComParam.CompanyIndex, groupCommand);
            #endregion
        }

        private IC_SystemCommand CreateCommandModel(string pSerial, string pCommand, string commandName, CommandParamDB pParam, int pGroupIndex, int pExcutingService,
           DateTime pNow, string pUserName, int pCompanyIndex, string pExternalData, bool isOverwriteData)
        {
            IC_SystemCommand command = new IC_SystemCommand();
            command.SerialNumber = pSerial;
            command.Command = pCommand.ToString();
            command.CommandName = string.IsNullOrWhiteSpace(commandName) ? pCommand.ToString() : commandName;

            var json = JsonConvert.SerializeObject(pParam);

            command.Params = json;
            command.EmployeeATIDs = pExternalData;
            command.RequestedTime = pNow;
            command.Excuted = false;

            command.CreatedDate = pNow;
            command.UpdatedDate = pNow;
            command.UpdatedUser = pUserName;
            command.CompanyIndex = pCompanyIndex;
            command.GroupIndex = pGroupIndex;
            command.ExcutingServiceIndex = pExcutingService;
            command.IsOverwriteData = isOverwriteData;

            return command;
        }
        private List<CommandResult> RemoveCommandIsNullIPAddress(List<CommandResult> pListCommand)
        {
            var listData = new List<CommandResult>();
            for (int i = 0; i < pListCommand.Count; i++)
            {
                if (pListCommand[i].IPAddress == "" || pListCommand[i].Port == 0)
                {
                    continue;
                }
                listData.Add(pListCommand[i]);
            }
            return listData;
        }

        private void UpdateCommandForService(List<UserInfo> pListUser, List<CommandResult> pListCommand)
        {
            for (int i = 0; i < pListCommand.Count; i++)
            {
                if (pListCommand[i].ExcutingServiceIndex == 0)
                {
                    // tìm service có quản lý device này
                    var listUser = pListUser.Where(t => t.ListDevice.Find(s => s.SerialNumber == pListCommand[i].SerialNumber) != null).ToList();
                    foreach (UserInfo user in listUser)
                    {
                        if (user.CheckCommandExists(pListCommand[i].Command, pListCommand[i].SerialNumber, pListCommand[i].IPAddress, pListCommand[i].CreatedTime) == false)
                        {
                            user.ListCommands.Add(pListCommand[i]);
                            pListCommand[i].ExcutingServiceIndex = user.Index;
                        }
                    }
                }
                else
                {
                    // command restart service
                    UserInfo user = pListUser.Where(t => t.Index == pListCommand[i].ExcutingServiceIndex).FirstOrDefault();
                    if (user != null)
                    {
                        if (user.CheckCommandExists(pListCommand[i].Command, pListCommand[i].SerialNumber, pListCommand[i].IPAddress, pListCommand[i].CreatedTime) == false)
                        {
                            user.ListCommands.Add(pListCommand[i]);
                        }
                    }
                }
            }
        }
        private void AddGroupCommandsForCompany(CompanyInfo companyInfo, int pCompany, CommandGroup pGroup)
        {
            if (companyInfo == null)
            {
                companyInfo = new CompanyInfo();
                companyInfo.AddToCache(cache, pCompany.ToString());
            }
            companyInfo.ListCommandGroups.Add(pGroup);
        }
        public List<IC_ServiceAndDeviceDTO> GetServiceType(List<string> listSerialNumber)
        {
            var query = (from sad in _dbContext.IC_ServiceAndDevices
                         join s in _dbContext.IC_Service
                          on sad.ServiceIndex equals s.Index
                         join d in _dbContext.IC_Device
                         on sad.SerialNumber equals d.SerialNumber
                         where listSerialNumber.Contains(sad.SerialNumber)
                         select new IC_ServiceAndDeviceDTO
                         {
                             CompanyIndex = sad.CompanyIndex,
                             ServiceType = s.ServiceType,
                             SerialNumber = sad.SerialNumber,
                             ServiceIndex = sad.ServiceIndex,
                             AliasName = d.AliasName,
                             IPAddress = d.IPAddress

                         }).AsQueryable();
            return query.ToList();
        }

        private List<WorkingInfoObject> ConvertIC_WorkingInfoToWorkingInfoObject(List<IC_WorkingInfo> pListData)
        {
            var listWorking = new List<WorkingInfoObject>();
            for (int i = 0; i < pListData.Count; i++)
            {
                WorkingInfoObject workingInfo = new WorkingInfoObject();
                workingInfo.EmployeeATID = pListData[i].EmployeeATID;
                workingInfo.FromDate = pListData[i].FromDate;
                workingInfo.ToDate = pListData[i].ToDate;
                workingInfo.IsSync = pListData[i].IsSync;
                workingInfo.Index = pListData[i].Index.ToString();
                workingInfo.DepartmentIndex = (int)pListData[i].DepartmentIndex;

                listWorking.Add(workingInfo);
            }
            return listWorking;
        }
        private List<WorkingInfoObject> ConvertHR_WorkingInfoToWorkingInfoObject(List<HR_WorkingInfo> pListData)
        {
            var listWorking = new List<WorkingInfoObject>();
            for (int i = 0; i < pListData.Count; i++)
            {
                WorkingInfoObject workingInfo = new WorkingInfoObject();
                workingInfo.EmployeeATID = pListData[i].EmployeeATID;
                workingInfo.FromDate = pListData[i].FromDate.Value;
                workingInfo.ToDate = pListData[i].ToDate;
                workingInfo.DepartmentIndex = (int)pListData[i].DepartmentIndex;
                if (pListData[i].Synched == null)
                {
                    workingInfo.IsSync = null;
                }
                else
                {
                    workingInfo.IsSync = pListData[i].Synched.Value == 0 ? false : true;
                }

                workingInfo.Index = pListData[i].Index.ToString();

                listWorking.Add(workingInfo);
            }
            return listWorking;
        }
        private List<CommandResult> CreateUploadUserCommand(int pCompanyIndex, long pNewDepartment, string pEmp, DateTime pFromTime, DateTime pToTime, string pExternalData, string authenMode)
        {
            var lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pCompanyIndex && x.DepartmentIndex == pNewDepartment)
                .Select(x => x.SerialNumber).ToList();
            var addOrDeleteUserConfig = _dbContext.IC_Config.FirstOrDefault(t => t.CompanyIndex == pCompanyIndex
                 && t.EventType == ConfigAuto.ADD_OR_DELETE_USER.ToString());
            var param = JsonConvert.DeserializeObject<IntegrateLogParam>(addOrDeleteUserConfig.CustomField);
            if (param.IntegrateWhenNotInclareDepartment == true && lstSerial.Count == 0)
            {
                var serialNumbers = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => x.SerialNumber).ToList();
                lstSerial = _dbContext.IC_Device.Where(x => !serialNumbers.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
            }
            var lstUser = new List<UserInfoOnMachine>();
            var lsSerialApp = new List<string>();
            ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp);
            lstSerial = lsSerialApp;
            if (lstSerial.Count > 0)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = new List<string>() { pEmp };
                paramUserOnMachine.CompanyIndex = pCompanyIndex;
                paramUserOnMachine.AuthenMode = authenMode;
                paramUserOnMachine.ListSerialNumber = lstSerial;
                paramUserOnMachine.FullInfo = true;
                lstUser = GetListUserInfoOnMachine(paramUserOnMachine);
            }
            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
            commandParam.ListSerialNumber = lstSerial;
            commandParam.ListEmployee = lstUser;
            commandParam.Action = CommandAction.UploadUsers;
            commandParam.FromTime = pFromTime;
            commandParam.ToTime = pToTime;
            commandParam.ExternalData = pExternalData;
            commandParam.IsOverwriteData = false;
            commandParam.Privilege = 0;
            var lstCmdAddUser = CreateListCommands(commandParam);
            //List<CommandResult> lstCmdAddUser = CreateListCommand(lstSerial, GlobalParams.CommandAction.UploadUsers, pFromTime, pToTime, lstUser, pExternalData);
            return lstCmdAddUser;
        }
        private List<CommandResult> CreateListCommandFromWorkingInfoData(List<string> pListEmpATIDs, List<WorkingInfoObject> pListWorkingInfo, DateTime pNow, int pCompanyIndex,
           DateTime pFromTimeConfig, DateTime pToTimeConfig)
        {
            var lstCmd = new List<CommandResult>();
            for (int i = 0; i < pListEmpATIDs.Count; i++)
            {
                var listWorkingByEmp = pListWorkingInfo.FindAll(t => t.EmployeeATID == pListEmpATIDs[i]).OrderBy(t => t.FromDate).ToList();
                if (listWorkingByEmp.Count == 0) continue;
                string indexWorking = "";

                if (listWorkingByEmp.Count == 1)
                {
                    // if sync column is null --> create command
                    if (listWorkingByEmp[0].IsSync == null && listWorkingByEmp[0].FromDate.Date <= pNow.Date)
                    {
                        indexWorking = listWorkingByEmp[0].Index;
                        var listCmdTemp = CreateUploadUserCommand(pCompanyIndex, listWorkingByEmp[0].DepartmentIndex,
                            listWorkingByEmp[0].EmployeeATID, pFromTimeConfig, pToTimeConfig, indexWorking, "");

                        if (listCmdTemp.Count > 0)
                        {
                            listWorkingByEmp[0].IsSync = false;
                            lstCmd.AddRange(listCmdTemp);
                        }
                    }
                }
                else
                {
                    // điều chuyển với working có fromdate <= ngày hiện tại, chỉ điều chuyển dòng có fromdate lớn nhất
                    WorkingInfoObject workingNeedTransfer = listWorkingByEmp.FindAll(t => t.FromDate.Date <= pNow.Date).OrderByDescending(t => t.FromDate)
                        .FirstOrDefault();

                    if (workingNeedTransfer != null && workingNeedTransfer.IsSync == null)
                    {
                        long newDepartment = workingNeedTransfer.DepartmentIndex;
                        //tìm phòng ban cũ 
                        WorkingInfoObject workingOld = listWorkingByEmp.FindAll(t => t.FromDate.Date <= workingNeedTransfer.FromDate.Date
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
                            continue;
                        }
                        var listCmdTemp = new List<CommandResult>();
                        //xóa phòng ban cũ
                        if (workingOld != null)
                        {
                            listCmdTemp.AddRange(CreateDeleteUserCommand(pCompanyIndex, workingOld.DepartmentIndex,
                                workingOld.EmployeeATID, pFromTimeConfig, pToTimeConfig, indexWorking));
                        }
                        // thêm lên phòng ban mới
                        indexWorking = workingNeedTransfer.Index;
                        listCmdTemp.AddRange(CreateUploadUserCommand(pCompanyIndex, workingNeedTransfer.DepartmentIndex,
                            workingNeedTransfer.EmployeeATID, pFromTimeConfig, pToTimeConfig, indexWorking, ""));
                        workingNeedTransfer.IsSync = false;

                        if (listCmdTemp.Count > 0)
                        {
                            lstCmd.AddRange(listCmdTemp);
                        }
                    }

                }
            }
            return lstCmd;
        }
        private List<CommandResult> CreateDeleteUserCommand(int pCompanyIndex, long pOldDepartment, string pEmp, DateTime pFromTime, DateTime pToTime, string pExternalData)
        {
            var lstSerial = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pCompanyIndex && x.DepartmentIndex == pOldDepartment).Select(x => x.SerialNumber).ToList();
            var addOrDeleteUserConfig = _dbContext.IC_Config.FirstOrDefault(t => t.CompanyIndex == pCompanyIndex
               && t.EventType == ConfigAuto.ADD_OR_DELETE_USER.ToString());
            var param = JsonConvert.DeserializeObject<IntegrateLogParam>(addOrDeleteUserConfig.CustomField);
            if (param.IntegrateWhenNotInclareDepartment == true && lstSerial.Count == 0)
            {
                var serialNumbers = _dbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => x.SerialNumber).ToList();
                lstSerial = _dbContext.IC_Device.Where(x => !serialNumbers.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
            }
            //Check licence
            var lsSerialApp = new List<string>();
            ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp);
            lstSerial = lsSerialApp;
            var lstUser = new List<UserInfoOnMachine>();
            if (lstSerial.Count > 0)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = new List<string>() { pEmp };
                paramUserOnMachine.CompanyIndex = pCompanyIndex;
                paramUserOnMachine.AuthenMode = "";
                paramUserOnMachine.FullInfo = true;
                lstUser = GetListUserInfoOnMachine(paramUserOnMachine);
            }
            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
            commandParam.ListSerialNumber = lstSerial;
            commandParam.ListEmployee = lstUser;
            commandParam.Action = CommandAction.DeleteUserById;
            commandParam.FromTime = pFromTime;
            commandParam.ToTime = pToTime;
            commandParam.ExternalData = pExternalData;
            commandParam.IsOverwriteData = false;
            commandParam.Privilege = 0;
            var lstCmdRemoveUser = CreateListCommands(commandParam);
            //List<CommandResult> lstCmdRemoveUser = CreateListCommand(lstSerial, GlobalParams.CommandAction.DeleteUserById, pFromTime, pToTime, lstUser, pExternalData);
            return lstCmdRemoveUser;
        }

        public List<UserInfoOnMachine> GetListUserInfoOnMachine(IC_UserinfoOnMachineParam param)
        {
            var lstUser = new List<UserInfoOnMachine>();
            var listUserMaster = new List<IC_UserMaster>();
            var listHREmployee = new List<HR_Employee>();
            //var listEmployee = new List<IC_Employee>();
            var listHRUser = new List<HR_User>();
            var listCardNumber = new List<HR_CardNumberInfo>();
            if (param.FullInfo)
            {
                listUserMaster = _dbContext.IC_UserMaster.Where(e => e.CompanyIndex == param.CompanyIndex && param.ListEmployeeaATID.Contains(e.EmployeeATID)).ToList();

                if (_config.IntegrateDBOther == true)
                {
                    listHREmployee = _integrateContext.HR_Employee.Where(e => e.CompanyIndex == _config.CompanyIndex && param.ListEmployeeaATID.Contains(e.EmployeeATID)).ToList();
                }
                else
                {
                    listHRUser = _dbContext.HR_User.Where(e => e.CompanyIndex == param.CompanyIndex 
                        && param.ListEmployeeaATID.Contains(e.EmployeeATID)).ToList();
                    var listCustomerUser = listHRUser.Where(x => x.EmployeeType == (short)EmployeeType.Guest).ToList();
                    var listCustomerID = listHRUser.Select(x => x.EmployeeATID).ToList();
                    listCardNumber = _dbContext.HR_CardNumberInfo.Where(e => e.CompanyIndex == param.CompanyIndex 
                        && param.ListEmployeeaATID.Contains(e.EmployeeATID) && !listCustomerID.Contains(e.EmployeeATID)
                        && e.IsActive == true).OrderByDescending(e => e.UpdatedDate).ToList();
                }
            }

            foreach (string empATID in param.ListEmployeeaATID)
            {
                UserInfoOnMachine userInfoOnMachine = new UserInfoOnMachine(empATID);
                userInfoOnMachine.EmployeeATID = empATID;

                if (!param.FullInfo)
                {
                    lstUser.Add(userInfoOnMachine);
                    continue;
                }
                // get card number in employee info first
                if (_config.IntegrateDBOther == true && listHREmployee != null)
                {
                    var hrEmployee = listHREmployee.FirstOrDefault(e => e.EmployeeATID == empATID);
                    if (hrEmployee != null)
                    {
                        userInfoOnMachine.CardNumber = hrEmployee.CardNumber;
                        userInfoOnMachine.NameOnDevice = hrEmployee.NickName.ConvertToUnSign3();
                    }
                }
                else if (listHRUser != null && listCardNumber != null)
                {
                    var cardInfo = listCardNumber.FirstOrDefault(e => e.EmployeeATID == empATID);
                    if (cardInfo != null)
                    {
                        userInfoOnMachine.CardNumber = cardInfo.CardNumber;
                    }
                    else
                    {
                        userInfoOnMachine.CardNumber = "0";
                    }
                }


                var userMaster = listUserMaster.FirstOrDefault(e => e.EmployeeATID == empATID);
                if (userMaster != null)
                {

                    //userInfoOnMachine.NameOnDevice = userMaster.NameOnMachine;
                    userInfoOnMachine.Privilege = userMaster.Privilege.HasValue ? userMaster.Privilege.Value : 0;

                    if (string.IsNullOrWhiteSpace(param.AuthenMode))
                    {
                        param.AuthenMode = string.IsNullOrWhiteSpace(userMaster.AuthenMode) ? AuthenMode.FullAccessRight.ToString() : userMaster.AuthenMode;
                    }

                    if (!string.IsNullOrWhiteSpace(userMaster.NameOnMachine) && !_config.IntegrateDBOther)
                        userInfoOnMachine.NameOnDevice = userMaster.NameOnMachine.ConvertToUnSign3();

                    // Allow authen by card number mode
                    if (param.AuthenMode.Contains(AuthenMode.CardNumber.ToString()) || param.AuthenMode.Contains(AuthenMode.FullAccessRight.ToString()))
                    {
                        //userInfoOnMachine.CardNumber = string.IsNullOrWhiteSpace(userMaster.CardNumber) ? "0" : userMaster.CardNumber;
                    }
                    else
                    {
                        userInfoOnMachine.CardNumber = "0";
                    }
                    // Allow authen by password mode
                    if (param.AuthenMode.Contains(AuthenMode.Password.ToString()) || param.AuthenMode.Contains(AuthenMode.FullAccessRight.ToString()))
                        userInfoOnMachine.PasswordOndevice = userMaster.Password;
                    else
                    {
                        userInfoOnMachine.PasswordOndevice = "";
                    }

                    // Allow authen by finger print mode
                    if (param.AuthenMode.Contains(AuthenMode.Finger.ToString()) || param.AuthenMode.Contains(AuthenMode.FullAccessRight.ToString()))
                    {
                        userInfoOnMachine.FingerPrints = _iC_UserMasterLogic.BuildFingerData(userMaster);
                    }
                    // Allow authen by face id mode
                    if (param.AuthenMode.Contains(AuthenMode.Face.ToString()) || param.AuthenMode.Contains(AuthenMode.FullAccessRight.ToString()))
                    {
                        // using face 1 or face 2 here
                        if (!string.IsNullOrEmpty(userMaster.FaceTemplate))
                            userInfoOnMachine.Face = new FaceInfo { FaceTemplate = userMaster.FaceTemplate };

                        if (!string.IsNullOrEmpty(userMaster.FaceV2_TemplateBIODATA) || !string.IsNullOrEmpty(userMaster.FaceV2_Content))
                        {
                            userInfoOnMachine.FaceInfoV2 = new FaceInfoV2
                            {
                                No = userMaster.FaceV2_No.HasValue ? userMaster.FaceV2_No.Value : 0,
                                Index = userMaster.FaceV2_Index.HasValue ? userMaster.FaceV2_Index.Value : 0,
                                Duress = userMaster.FaceV2_Duress.HasValue ? userMaster.FaceV2_Duress.Value : 0,
                                Type = userMaster.FaceV2_Type.HasValue ? userMaster.FaceV2_Type.Value : 9,
                                Valid = userMaster.FaceV2_Valid.HasValue ? userMaster.FaceV2_Valid.Value : 1,
                                MajorVer = userMaster.FaceV2_MajorVer.HasValue ? userMaster.FaceV2_MajorVer.Value : 5,
                                MinorVer = userMaster.FaceV2_MinorVer.HasValue ? userMaster.FaceV2_MinorVer.Value : 8,
                                Size = userMaster.FaceV2_Size.HasValue ? userMaster.FaceV2_Size.Value : !string.IsNullOrWhiteSpace(userMaster.FaceV2_Content) ? userMaster.FaceV2_Content.Length : !string.IsNullOrWhiteSpace(userMaster.FaceV2_TemplateBIODATA) ? userMaster.FaceV2_TemplateBIODATA.Length : 0,
                                Format = userMaster.FaceV2_Format.HasValue ? userMaster.FaceV2_Format.Value : 0,
                                //TemplateBIODATA = string.IsNullOrWhiteSpace(userMaster.FaceV2_TemplateBIODATA) ? "" : userMaster.FaceV2_TemplateBIODATA,
                                Content = string.IsNullOrWhiteSpace(userMaster.FaceV2_Content) ? userMaster.FaceV2_TemplateBIODATA : userMaster.FaceV2_Content
                            };
                        }

                    }
                }
                lstUser.Add(userInfoOnMachine);
            }
            return lstUser;
        }


        public void UploadACUsers(int groupIndex, List<string> listUser, int companyIndex)
        {
            var groupAcc = _dbContext.AC_AccGroup.Where(x => x.UID == groupIndex).ToList();
            var listSerial = _dbContext.AC_DoorAndDevice.Where(x => groupAcc.Select(x => x.DoorIndex).Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();

            List<string> lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(listSerial, ref lsSerialHw);
            var authenMode = new List<string> { "CardNumber" };
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                if (lsSerialHw.Count > 0)
                {

                    IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                    paramUserOnMachine.ListEmployeeaATID = listUser;
                    paramUserOnMachine.CompanyIndex = companyIndex;
                    paramUserOnMachine.AuthenMode = string.Join(",", authenMode);
                    paramUserOnMachine.ListSerialNumber = listSerial;
                    paramUserOnMachine.FullInfo = true;

                    List<UserInfoOnMachine> lstUser = GetListUserInfoOnMachine(paramUserOnMachine);

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

                    List<CommandResult> lstCmd = CreateListCommands(commandParam);

                    var timeZone = groupAcc.Select(x => x.Timezone).FirstOrDefault();
                    var timezoneDb = _dbContext.AC_TimeZone.FirstOrDefault(x => x.UID == timeZone);
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
                        groupCommand.CompanyIndex = companyIndex;
                        groupCommand.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                        groupCommand.ListCommand = lstCmd;
                        groupCommand.GroupName = GroupName.UploadACUsers.ToString();
                        groupCommand.EventType = "";
                        CreateGroupCommands(groupCommand);
                        for (int i = 0; i < lstCmd.Count; i++)
                        {
                            CommandResult command = lstCmd[i];
                            // Add audit log
                            var audit = new IC_AuditEntryDTO(null)
                            {
                                TableName = "IC_SystemCommand",
                                UserName = UpdatedUser.SYSTEM_AUTO.ToString(),
                                CompanyIndex = companyIndex,
                                State = AuditType.UploadACUsers,
                                Description = $"Upload {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                DescriptionEn = $"Upload {lstCmd[i].ListUsers?.Count ?? 0} users",
                                IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : 0,
                                Name = UpdatedUser.SYSTEM_AUTO.ToString(),
                                PageName = "AutoSynchUser",
                                Status = AuditStatus.Unexecuted,
                                DateTime = DateTime.Now
                            };
                            _iIC_AuditLogic.Create(audit);
                        }
                    }

                    AddUserMasterToHistory(listUser, groupAcc.Select(x => x.DoorIndex).ToList(), timeZone, companyIndex);
                    //CheckFR05(user.CompanyIndex, lstCmd).GetAwaiter().GetResult();
                    //List<CommandResult> lstCmd = CreateListCommand(context, lsSerialHw, GlobalParams.CommandAction.UploadUsers, new DateTime(2000, 1, 1), DateTime.Now, lstUser, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    //CreateGroupCommand(user.CompanyIndex, user.UserName, GlobalParams.CommandAction.UploadUsers.ToString(), lstCmd, "");
                }
            }
        }
        public void AddUserMasterToHistory(List<string> employeeATIDLst, List<int> doorLst, int timezone, int companyIndex)
        {
            if (employeeATIDLst != null && employeeATIDLst.Count > 0)
            {
                var employeesData = _dbContext.AC_UserMaster.Where(x => employeeATIDLst.Contains(x.EmployeeATID) && doorLst.Contains(x.DoorIndex)).ToList();
                foreach (var item in employeeATIDLst)
                {
                    foreach (var itemx in doorLst)
                    {
                        var updateData = new AC_UserMaster()
                        {
                            CompanyIndex = companyIndex,
                            DoorIndex = itemx,
                            EmployeeATID = item,
                            Timezone = timezone,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = UpdatedUser.SYSTEM_AUTO.ToString(),
                            Operation = (int)ACOperation.Sync
                        };
                        _dbContext.Add(updateData);
                    }
                }

                _dbContext.SaveChanges();
            }
        }

        public void UploadTimeZone(int groupIndex, int companyIndex)
        {
            var groupAcc = _dbContext.AC_AccGroup.Where(x => x.UID == groupIndex).ToList();
            var listSerial = _dbContext.AC_DoorAndDevice.Where(x => groupAcc.Select(x => x.DoorIndex).Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();

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
                        lstTimezones = _dbContext.AC_TimeZone.ToList();
                    }
                    else
                    {
                        var timeZone = timeZoneList.FirstOrDefault();
                        lstTimezones = _dbContext.AC_TimeZone.Where(x => x.UID == timeZone).ToList();
                    }

                    var param = new IC_CommandRequestDTO();
                    List<CommandResult> lstCmd = CreateListACCommand(_dbContext, lsSerialHw, CommandAction.UploadTimeZone, new DateTime(2000, 1, 1), DateTime.Now, lstUser, lstGroups, lstHolidays, lstTimezones, param, param.IsOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
                    CreateGroupCommand(companyIndex, UpdatedUser.SYSTEM_AUTO.ToString(), CommandAction.UploadTimeZone.ToString(), lstCmd, "");

                    // Add audit log
                    var audit = new IC_AuditEntryDTO(null)
                    {
                        TableName = "IC_SystemCommand",
                        UserName = UpdatedUser.SYSTEM_AUTO.ToString(),
                        CompanyIndex = companyIndex,
                        State = AuditType.UploadTimeZone,
                        Description = "Cập nhật Timezone",
                        DescriptionEn = "Update Timezone",
                        DateTime = DateTime.Now,
                        Status = AuditStatus.Completed,
                        Name = UpdatedUser.SYSTEM_AUTO.ToString()
                    };
                    _iIC_AuditLogic.Create(audit);
                }
            }
            else
            {
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null)
                {
                    UserName = UpdatedUser.SYSTEM_AUTO.ToString(),
                    TableName = "IC_SystemCommand",
                    CompanyIndex = companyIndex,
                    State = AuditType.UploadTimeZone,
                    Description = "Cập nhật Timezones xảy ra lỗi: Thiết bị không có bản quyền",
                    DescriptionEn = "update timezones occur error: Device don't have license",
                    DateTime = DateTime.Now,
                    Status = AuditStatus.Error,
                    Name = UpdatedUser.SYSTEM_AUTO.ToString()
                };
                _iIC_AuditLogic.Create(audit);
            }

        }

        public void UploadUsers(int groupIndex, List<string> listUser, int companyIndex)
        {
            var groupAcc = _dbContext.AC_AccGroup.Where(x => x.UID == groupIndex).ToList();
            var listSerial = _dbContext.AC_DoorAndDevice.Where(x => groupAcc.Select(x => x.DoorIndex).Contains(x.DoorIndex)).Select(x => x.SerialNumber).ToList();

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
                    paramUserOnMachine.CompanyIndex = companyIndex;
                    paramUserOnMachine.AuthenMode = string.Join(",", authenMode);
                    paramUserOnMachine.ListSerialNumber = listSerial;
                    paramUserOnMachine.FullInfo = true;

                    List<UserInfoOnMachine> lstUser = GetListUserInfoOnMachine(paramUserOnMachine);

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

                    List<CommandResult> lstCmd = CreateListCommands(commandParam);

                    if (lstCmd != null && lstCmd.Count() > 0)
                    {
                        IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                        groupCommand.CompanyIndex = companyIndex;
                        groupCommand.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                        groupCommand.ListCommand = lstCmd;
                        groupCommand.GroupName = GroupName.UploadUsers.ToString();
                        groupCommand.EventType = "";
                        CreateGroupCommands(groupCommand);
                        for (int i = 0; i < lstCmd.Count; i++)
                        {
                            CommandResult command = lstCmd[i];
                            // Add audit log
                            var audit = new IC_AuditEntryDTO(null)
                            {
                                TableName = "IC_SystemCommand",
                                UserName = UpdatedUser.SYSTEM_AUTO.ToString(),
                                CompanyIndex = companyIndex,
                                State = AuditType.UploadUsers,
                                Description = $"Upload {lstCmd[i]?.ListUsers?.Count ?? 0} người dùng",
                                DescriptionEn = $"Upload {lstCmd[i].ListUsers?.Count ?? 0} users",
                                IC_SystemCommandIndex = lstCmd[i]?.ID != null ? int.Parse(lstCmd[i]?.ID) : 0,
                                Name = UpdatedUser.SYSTEM_AUTO.ToString(),
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

        //    var department = (from eplDe in _dbContext.AC_DepartmentAccessedGroup.Where(x => departmentAccIndexes.Contains(x.Index))
        //                      join gr in _dbContext.AC_AccGroup.Where(x => x.CompanyIndex == user.CompanyIndex)
        //                      on eplDe.GroupIndex equals gr.UID into eGroup
        //                      from eGroupResult in eGroup.DefaultIfEmpty()
        //                      join doorDev in _dbContext.AC_DoorAndDevice.Where(x => x.CompanyIndex == user.CompanyIndex)
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
            CommandProcess.CreateGroupCommand(_dbContext, cache, pCompanyIndex, pUserName, pGroupName, externalData, pListCommands, pEventType);
        }


    }
    public interface IIC_CommandLogic
    {
        void TransferUser(List<IC_EmployeeTransferDTO> lstEmployee);
        void SyncWithDepartmentAndDevice(List<string> listSerialNumber, int departmentIndex, int companyIndex);
        Task SyncWithEmployee(List<string> listEmployeeIndex, int companyIndex, string currentSerialNumber = null);
        List<UserInfoOnMachine> GetListUserInfoOnMachine(IC_UserinfoOnMachineParam param);
        List<CommandResult> CreateListCommands(IC_CommandParamDTO commandParam);
        List<CommandResult> CreateAutoListCommands(IC_CommandParamDTO commandParam, List<IC_ServiceAndDeviceDTO> listServiceAndDevice);
        void CreateGroupCommands(IC_GroupCommandParamDTO groupComParam);
        public List<IC_ServiceAndDeviceDTO> GetServiceType(List<string> listSerialNumber);

        void UploadACUsers(int groupIndex, List<string> listUser, int companyIndex);
        void UploadTimeZone(int groupIndex, int companyIndex);
        void UploadUsers(int groupIndex, List<string> listUser, int companyIndex);
    }
}
