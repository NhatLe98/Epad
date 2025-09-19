using EPAD_Common;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Logic.MainProcess;
using EPAD_Logic.SendMail;
using EPAD_Services.FR05.Interface;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static EPAD_Data.Models.IC_SignalRDTO;

namespace EPAD_Services.Impl
{
    public class IC_SystemCommandService : BaseServices<IC_SystemCommand, EPAD_Context>, IIC_SystemCommandService
    {
        private ezHR_Context otherContext;
        private readonly IEmailProvider emailProvider;
        private IMemoryCache cache;
        private IIC_SignalRLogic _iC_SignalRLogic;
        private IFR05_ClientService _FR05_ClientService;
        public IC_SystemCommandService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            otherContext = serviceProvider.GetService<ezHR_Context>();
            emailProvider = serviceProvider.GetService<IEmailProvider>();
            cache = serviceProvider.GetService<IMemoryCache>();
            _iC_SignalRLogic = serviceProvider.GetService<IIC_SignalRLogic>();
            _FR05_ClientService = serviceProvider.GetService<IFR05_ClientService>();
        }


        public async Task AutoRunCommandOfFR05()
        {
            


        }

        public List<RemoteProcessLogObject> UpdateSystemCommandStatus(List<CommandParam> listParams, UserInfo user, ConfigObject config, CompanyInfo companyInfo)
        {
            List<RemoteProcessLogObject> listLogs = new List<RemoteProcessLogObject>();
            DateTime now = DateTime.Now;
            List<string> listGroupIndex = new List<string>();

            for (int i = 0; i < listParams.Count; i++)
            {
                CommandResult cmd = new CommandResult();
                // update status for new cmd
                if (user.UpdateStatusCommand(int.Parse(listParams[i].ID), listParams[i].Status, listParams[i].Error,
                    listParams[i].DataSuccess, listParams[i].DataFailure, ref cmd, DbContext, otherContext, config) == true)
                {
                    if (listGroupIndex.Contains(cmd.GroupIndex) == false)
                    {
                        listGroupIndex.Add(cmd.GroupIndex);
                    }
                    RemoteProcessLogObject log = new RemoteProcessLogObject(user, cmd, listParams[i].SDKFuntion, listParams[i].Status, now);
                    log.Action = "UpdateCommandStatus";
                    listLogs.Add(log);
                }
            }
            // update group command

            if (companyInfo != null)
            {

                List<CommandGroup> listGroupInParam = companyInfo.ListCommandGroups.Where(t => listGroupIndex.Contains(t.ID)).ToList();
                List<string> listGroupDeleteIndex = new List<string>();
                //CheckGroupHasError(ref listGroupInParam);
                for (int i = 0; i < listGroupInParam.Count; i++)
                {
                    // delete cmd in company cache if status = success
                    UpdateCommandInCompanyCache(listParams, listGroupInParam[i].ID, companyInfo, now);
                    // if all command in groud is success, update group
                    List<CommandResult> listCommandResult = listGroupInParam[i].ListCommand;

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
                    DeleteCommandInCompanyCache(listCommandResult, listGroupInParam[i].ID, companyInfo, now);

                    var checkallFinished = DbContext.IC_SystemCommand.Where(t => t.GroupIndex.Equals(int.Parse(listGroupInParam[i].ID)) && t.Excuted == false).ToList();
                    if (checkallFinished != null && checkallFinished.Count > 0)
                    {
                        allFinished = false;
                    }
                    else
                    {
                        allFinished = true;
                    }

                    if (allFinished == true)
                    {
                        listGroupInParam[i].Excuted = true;
                        listGroupInParam[i].FinishedTime = now;
                        listGroupDeleteIndex.Add(listGroupInParam[i].ID);
                        int index = int.Parse(listGroupInParam[i].ID);
                        IC_CommandSystemGroup groupModel = DbContext.IC_CommandSystemGroup.Where(t => t.Index == index).FirstOrDefault();

                        if (groupModel != null)
                        {
                            groupModel.Excuted = true;
                            groupModel.UpdatedDate = now;

                            //Update IsSync trong IC_EmployeeTransfer
                            try
                            {
                                if (groupModel.EventType.Equals(ConfigAuto.ADD_OR_DELETE_USER.ToString()))
                                {
                                    var systemCommands = DbContext.IC_SystemCommand.Where(t => t.CompanyIndex.Equals(user.CompanyIndex) && t.GroupIndex.Equals(groupModel.Index)).ToList();
                                    if (systemCommands != null && systemCommands.Count > 0)
                                    {
                                        foreach (var systemCommand in systemCommands)
                                        {
                                            if (string.IsNullOrEmpty(systemCommand.Error))
                                            {
                                                CommandParamDB employees = JsonConvert.DeserializeObject<CommandParamDB>(systemCommand.Params, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                                                foreach (var employee in employees.ListUsers)
                                                {
                                                    IC_EmployeeTransfer employeeTransfer = DbContext.IC_EmployeeTransfer.Where(t => t.CompanyIndex.Equals(user.CompanyIndex) && t.EmployeeATID.Equals(employee.UserID.PadLeft(config.MaxLenghtEmployeeATID, '0'))).FirstOrDefault();
                                                    if (employeeTransfer != null)
                                                        employeeTransfer.IsSync = true;
                                                }
                                            }
                                        }
                                    }
                                    DbContext.SaveChanges();
                                }
                            }
                            catch
                            {

                            }
                        }

                        try
                        {
                            var listSerialCommand = DbContext.IC_SystemCommand.Where(t => t.CompanyIndex.Equals(groupModel.CompanyIndex) && t.GroupIndex.Equals(groupModel.Index) && t.Command.Equals(groupModel.GroupName)).ToList();

                            List<IntergateLogParam> lsIntergateLog = new List<IntergateLogParam>();
                            foreach (var item in listSerialCommand)
                            {
                                lsIntergateLog.Add(new IntergateLogParam()
                                {
                                    SerialNumber = item.SerialNumber,
                                    FromTIme = JsonConvert.DeserializeObject<CommandParamDB>(item.Params).FromTime,
                                    ToTime = JsonConvert.DeserializeObject<CommandParamDB>(item.Params).ToTime,
                                    IPAddress = JsonConvert.DeserializeObject<CommandParamDB>(item.Params).IPAddress
                                });
                            }

                            List<SerialNumberCommandParam> listSerialCommandResult = new List<SerialNumberCommandParam>();

                            string notification = "";
                            var groupCommandIndexList = listSerialCommand.Select(x => x.GroupIndex).ToHashSet();
                            var groupCommandList = DbContext.IC_CommandSystemGroup.Where(x => groupCommandIndexList.Contains(x.Index)).ToList();

                            foreach (var item in listSerialCommand)
                            {
                                GetResultCommand(item.Command, item.Error, ref notification, lsIntergateLog, user.CompanyIndex, listGroupInParam[i]);

                                listSerialCommandResult.Add(new SerialNumberCommandParam()
                                {
                                    SerialNumber = item.SerialNumber,
                                    Result = notification,
                                    Erorr = item.Error,
                                    IPAddress = JsonConvert.DeserializeObject<CommandParamDB>(item.Params).IPAddress
                                });
                            }

                            if (listSerialCommandResult != null && listSerialCommandResult.Count > 0)
                            {
                                _iC_SignalRLogic.PostPushNotification(user.CompanyIndex, listSerialCommandResult);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                DbContext.SaveChanges();
                // send mail process
                for (int i = 0; i < listGroupInParam.Count; i++)
                {
                    if (listGroupInParam[i].Excuted && listGroupInParam[i].EventType != "")
                    {
                        var checkConfig = DbContext.IC_CommandSystemGroup.Where(t => t.Index.Equals(Convert.ToInt32(listGroupInParam[i].ID)) && t.UpdatedUser.Contains("SYSTEM_AUTO_")).FirstOrDefault();
                        if (checkConfig != null)
                        {
                            try
                            {
                                emailProvider.SendMailConfigProcessDoneGroup(listGroupInParam[i], checkConfig.UpdatedUser.Split('_')[2].ToString());
                            }
                            catch (Exception) { }
                        }
                        else
                        {
                            try
                            {
                                emailProvider.SendMailConfigProcessDone(listGroupInParam[i]);
                            }
                            catch (Exception) { }
                        }
                    }
                }
                // remove group in cache
                for (int i = 0; i < listGroupDeleteIndex.Count; i++)
                {
                    companyInfo.DeleteGroupById(listGroupDeleteIndex[i]);
                }
            }
            //ghi log 
            MongoDBHelper<RemoteProcessLogObject> mongoObject = new MongoDBHelper<RemoteProcessLogObject>("process_log", cache);
            mongoObject.AddListDataToCollection(listLogs, true);
            return listLogs;
        }

        public bool RenewSystemCommand(int systemCommandIndex, UserInfo user)
        {
            bool success = false;

            using (var transaction = DbContext.Database.BeginTransaction())
            {
                try
                {

                    IC_SystemCommand oldCommand = DbContext.IC_SystemCommand
                        .FirstOrDefault(cmd => cmd.Index == systemCommandIndex);
                    if (oldCommand == null)
                    {
                        throw new NullReferenceException("Lệnh không tồn tại trên hệ thống");
                    }
                    IC_SystemCommand newCommand = new IC_SystemCommand
                    {
                        Index = 0, //Primary key = 0 for INSERT INTO DATABASE
                        Excuted = false,
                        Error = "",
                        CreatedDate = DateTime.Now,
                        ExcutedTime = null,
                        UpdatedDate = null,
                        ExcutingServiceIndex = 0,
                        Command = oldCommand.Command,
                        CommandName = oldCommand.CommandName,
                        CompanyIndex = oldCommand.CompanyIndex,
                        EmployeeATIDs = oldCommand.EmployeeATIDs,
                        GroupIndex = oldCommand.GroupIndex,
                        IsOverwriteData = oldCommand.IsOverwriteData,
                        Params = oldCommand.Params,
                        RequestedTime = null,
                        SerialNumber = oldCommand.SerialNumber,
                        UpdatedUser = user.UserName,
                    };

                    var param = JsonConvert.DeserializeObject<CommandParamDB>(newCommand.Params);

                    List<CommandResult> lstCmd = CommandProcess.CreateListCommands(
                        context: DbContext,
                        listSerial: new List<string> { newCommand.SerialNumber },
                        pAction: Enum.Parse<CommandAction>(newCommand.Command),
                        pExternalData: "",
                        pFromTime: new DateTime(2000, 1, 1),
                        pToTime: DateTime.Now, pListUsers: param.ListUsers,
                        isOverwriteData: newCommand.IsOverwriteData,
                        privilege: GlobalParams.DevicePrivilege.SDKStandardRole);

                    string groupName = DbContext.IC_CommandSystemGroup
                        .FirstOrDefault(x => x.Index == newCommand.GroupIndex)?.GroupName ?? newCommand.CommandName;

                    CommandProcess.CreateGroupCommand(
                        context: DbContext,
                        cache: cache,
                        pCompanyIndex: newCommand.CompanyIndex,
                        pUserName: newCommand.UpdatedUser,
                        pGroupName: groupName,
                        pExternalData: "",
                        pListCommands: lstCmd,
                        pEventType: ""
                    );

                    IC_Audit firstAuditOfOldCommand = DbContext.IC_Audit
                        .FirstOrDefault(x => x.IC_SystemCommandIndex == systemCommandIndex);

                    if (firstAuditOfOldCommand != null)
                    {
                        int? newCommandIndex = null;
                        if (lstCmd[0]?.ID != null)
                        {
                            newCommandIndex = int.Parse(lstCmd[0].ID);
                        }

                        IC_Audit auditOfNewCommand = new IC_Audit
                        {
                            UserName = user.UserName,
                            IC_SystemCommandIndex = newCommandIndex,
                            Index = 0,
                            CompanyIndex = newCommand.CompanyIndex,
                            DateTime = DateTime.Now,
                            Description = firstAuditOfOldCommand.Description,
                            DescriptionEn = firstAuditOfOldCommand.DescriptionEn,
                            AffectedColumns = null,
                            KeyValues = null,
                            Name = firstAuditOfOldCommand.Name,
                            NewValues = null,
                            NumFailure = null,
                            NumSuccess = null,
                            OldValues = null,
                            PageName = firstAuditOfOldCommand.PageName,
                            State = firstAuditOfOldCommand.State,
                            Status = "Unexecuted",
                            TableName = firstAuditOfOldCommand.TableName,
                        };
                        DbContext.IC_Audit.RemoveRange(DbContext.IC_Audit.Where(x => x.IC_SystemCommandIndex == systemCommandIndex));
                        DbContext.IC_SystemCommand.Remove(oldCommand);
                        DbContext.IC_Audit.Add(auditOfNewCommand);
                        DbContext.SaveChanges();
                    }


                    transaction.Commit();

                    CheckFR05(newCommand.CompanyIndex, lstCmd, user).GetAwaiter().GetResult();
                    success = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }

            return success;
        }

        public async Task CheckFR05(int pCompanyIndex, List<CommandResult> pListCommands, UserInfo user)
        {
            ConfigObject config = ConfigObject.GetConfig(cache);
            Dictionary<string, IC_Device> deviceLookup = null;
            if (cache.TryGetValue("urn:Dictionary_IC_Device", out deviceLookup) == false)
            {
                deviceLookup = DbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex).ToDictionarySafe(x => x.SerialNumber);
            }

            var listCheck = deviceLookup.Where(x => !string.IsNullOrEmpty(x.Value.DeviceModel) && NumberExtensions.IsNumber(x.Value.DeviceModel) && int.Parse(x.Value.DeviceModel) == (int)ProducerEnum.FR05).Select(x => x.Key).ToList();

            if (pListCommands.Any(x => listCheck.Contains(x.SerialNumber)))
            {
                CompanyInfo companyInfo = CompanyInfo.GetFromCache(cache, user.CompanyIndex.ToString());
                var listCommandFr05 = pListCommands.Where(x => listCheck.Contains(x.SerialNumber)).ToList();
                foreach (var command in listCommandFr05)
                {
                    await _FR05_ClientService.ProcessCommand(command, user, config, companyInfo, false);
                }
            }
        }

        [NonAction]
        private void UpdateCommandInCompanyCache(List<CommandParam> pListCommand, string pGroupIndex, CompanyInfo companyInfo, DateTime pNow)
        {
            for (int i = 0; i < pListCommand.Count; i++)
            {
                companyInfo.UpdateCommandById(pListCommand[i].ID, pGroupIndex, pListCommand[i].Status, pListCommand[i].Error, pNow);
            }
        }


        [NonAction]
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

        [NonAction]
        private void CheckGroupHasError(ref List<CommandGroup> listGroupInParam)
        {
            for (int i = 0; i < listGroupInParam.Count; i++)
            {
                List<CommandResult> lstCmdResult = listGroupInParam[i].ListCommand;
                for (int ix = 0; ix < lstCmdResult.Count; ix++)
                {
                    if (lstCmdResult[i].Error != null && lstCmdResult[i].Error != "")
                    {
                        listGroupInParam[i].Errors.Add(lstCmdResult[i].Error);
                    }
                }
            }
        }

        [NonAction]
        public void GetResultCommand(string pCommand, string pError, ref string pResultNotification, List<IntergateLogParam> logParams, int CompanyIndex, CommandGroup command)
        {
            switch (pCommand)
            {
                case GlobalParams.ValueFunction.GetDeviceInfo:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "GetDeviceInfoSuccess";
                    }
                    else
                    {
                        pResultNotification = "GetDeviceInfoFail";
                    }
                    break;
                case GlobalParams.ValueFunction.DownloadLogFromToTime:
                    if (string.IsNullOrEmpty(pError))
                    {
                        try
                        {
                            //Khi download log manual, integrate log
                            if (command.Excuted && command.EventType == "")
                            {
                                IntergateLogManual(logParams, CompanyIndex);
                            }
                        }
                        catch
                        {

                        }
                        pResultNotification = "DownloadLogFromToTimeSuccess";

                    }
                    else
                    {
                        pResultNotification = "DownloadLogFromToTimeFail";
                    }
                    break;
                case GlobalParams.ValueFunction.DownloadAllLog:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "DownloadAllLogSuccess";
                    }
                    else
                    {
                        pResultNotification = "DownloadAllLogFail";
                    }
                    break;
                case GlobalParams.ValueFunction.UploadUsers:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "UploadUsersSuccess";
                    }
                    else
                    {
                        pResultNotification = "UploadUsersFail";
                    }
                    break;
                case GlobalParams.ValueFunction.DeleteAllUser:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "DeleteAllUsersSuccess";
                    }
                    else
                    {
                        pResultNotification = "DeleteAllUsersFail";
                    }
                    break;
                case GlobalParams.ValueFunction.DeleteLogFromToTime:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "DeleteLogFromToTimeSuccess";
                    }
                    else
                    {
                        pResultNotification = "DeleteLogFromToTimeFail";
                    }
                    break;
                case GlobalParams.ValueFunction.RestartDevice:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "RestartDeviceSuccess";
                    }
                    else
                    {
                        pResultNotification = "RestartDeviceFail";
                    }
                    break;
                case GlobalParams.ValueFunction.DownloadUserById:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "DownloadUserByIdSuccess";
                    }
                    else
                    {
                        pResultNotification = "DownloadUserByIdFail";
                    }
                    break;
                case GlobalParams.ValueFunction.DownloadAllUser:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "DownloadAllUserSuccess";
                    }
                    else
                    {
                        pResultNotification = "DownloadAllUserFail";
                    }
                    break;
                case GlobalParams.ValueFunction.DeleteAllLog:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "DeleteAllLogSuccess";
                    }
                    else
                    {
                        pResultNotification = "DeleteAllLogFail";
                    }
                    break;
                case GlobalParams.ValueFunction.DeleteUserById:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "DeleteUserByIdSuccess";
                    }
                    else
                    {
                        pResultNotification = "DeleteUserByIdFail";
                    }
                    break;
                case GlobalParams.ValueFunction.DeleteAllFingerPrint:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "DeleteAllFingerPrintSuccess";
                    }
                    else
                    {
                        pResultNotification = "DeleteAllFingerPrintFail";

                    }
                    break;
                case GlobalParams.ValueFunction.RESTART_SERVICE:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "RESTART_SERVICESuccess";
                    }
                    else
                    {
                        pResultNotification = "RESTART_SERVICEFail";

                    }
                    break;
                case GlobalParams.ValueFunction.DownloadAllUserMaster:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "DownloadAllUserMasterSuccess";
                    }
                    else
                    {
                        pResultNotification = "DownloadAllUserMasterFail";

                    }
                    break;
                case GlobalParams.ValueFunction.DownloadUserMasterById:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "DownloadUserMasterByIdSuccess";
                    }
                    else
                    {
                        pResultNotification = "DownloadUserMasterByIdFail";

                    }
                    break;
                case GlobalParams.ValueFunction.SetTimeDevice:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "SetTimeDeviceSuccess";
                    }
                    else
                    {
                        pResultNotification = "SetTimeDeviceFail";

                    }
                    break;
                case GlobalParams.ValueFunction.UploadTimeZone:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "UploadTimezoneSuccess";
                    }
                    else
                    {
                        pResultNotification = "UploadTimezoneFail";
                    }
                    break;
                case GlobalParams.ValueFunction.UploadAccGroup:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "UploadGroupSuccess";
                    }
                    else
                    {
                        pResultNotification = "UploadGroupFail";
                    }
                    break;
                case GlobalParams.ValueFunction.UploadAccHoliday:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "UploadHolidaySuccess";
                    }
                    else
                    {
                        pResultNotification = "UploadHolidayFail";
                    }
                    break;
                case GlobalParams.ValueFunction.UploadACUsers:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "UploadACUsersSuccess";
                    }
                    else
                    {
                        pResultNotification = "UploadACUsersFail";
                    }
                    break;
                case GlobalParams.ValueFunction.UnlockDoor:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "UnlockDoorSuccess";
                    }
                    else
                    {
                        pResultNotification = "UnlockDoorFail";
                    }
                    break;
                case GlobalParams.ValueFunction.DeleteACUser:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "DeleteACUserSuccess";
                    }
                    else
                    {
                        pResultNotification = "DeleteACUserFail";
                    }
                    break;
                case GlobalParams.ValueFunction.UploadACUsersFromExcel:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "UploadUsersSuccess";
                    }
                    else
                    {
                        pResultNotification = "UploadUsersFail";
                    }
                    break;
                case GlobalParams.ValueFunction.DeleteAllHoliday:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "DeleteAllHolidaySuccess";
                    }
                    else
                    {
                        pResultNotification = "DeleteAllHolidayFail";
                    }
                    break;
                case GlobalParams.ValueFunction.SetDoorSetting:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "SetDoorSettingSuccess";
                    }
                    else
                    {
                        pResultNotification = "SetDoorSettingFail";
                    }
                    break;
                case GlobalParams.ValueFunction.DeleteTimezoneById:
                    if (string.IsNullOrEmpty(pError))
                    {
                        pResultNotification = "DeleteTimezoneByIdSuccess";
                    }
                    else
                    {
                        pResultNotification = "DeleteTimezoneByIdFail";
                    }
                    break;
                default:
                    break;
            }
        }

        [NonAction]
        public void IntergateLogManual(List<IntergateLogParam> lsparam, int CompanyIndex)
        {
            IC_Config cfg = DbContext.IC_Config.Where(x => x.EventType == ConfigAuto.INTEGRATE_LOG.ToString() && x.CompanyIndex == CompanyIndex).FirstOrDefault();
            IntegrateLogParam param = JsonConvert.DeserializeObject<IntegrateLogParam>(cfg.CustomField);
            List<AttendanceLog> listLogParam = new List<AttendanceLog>();
            IntegrateTimeLogParam logParam = new IntegrateTimeLogParam();
            IntegrateLogMongo logMongo = new IntegrateLogMongo();
            if (param.LinkAPI != null && param.LinkAPI != "")
            {
                string apiLink = param.LinkAPI;
                apiLink = apiLink + (apiLink.EndsWith("/") ? "" : "/");
                foreach (var item in lsparam)
                {
                    List<IC_AttendanceLog> listLogDB = DbContext.IC_AttendanceLog.Where(t => t.CompanyIndex == cfg.CompanyIndex
                   && t.CheckTime >= item.FromTIme && t.CheckTime <= item.ToTime && t.SerialNumber == item.SerialNumber).ToList();
                    List<IC_Device> listDevice = DbContext.IC_Device.Where(t => t.CompanyIndex == CompanyIndex).ToList();

                    logMongo.IntegrateTime = DateTime.Now;
                    logMongo.LogCount = listLogDB.Count;
                    logMongo.Param = param;
                    logMongo.Success = false;
                    logMongo.CompanyIndex = cfg.CompanyIndex;

                    logParam.WriteToDatabase = param.WriteToDatabase;
                    logParam.WriteToFile = param.WriteToFile;
                    logParam.WriteToFilePath = param.WriteToFilePath;
                    logParam.IntegrateTime = $"{DateTime.Now.Hour.ToString().PadLeft(2, '0')}:{DateTime.Now.Minute.ToString().PadLeft(2, '0')}"; ;
                    for (int i = 0; i < listLogDB.Count; i++)
                    {
                        AttendanceLog log = new AttendanceLog();
                        var employee = DbContext.HR_User.FirstOrDefault(x => x.EmployeeATID == listLogDB[i].EmployeeATID);

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
                        }

                        listLogParam.Add(log);
                    }
                }

                logParam.ListLogs = listLogParam;

                var client = new HttpClient();
                client.BaseAddress = new Uri(apiLink);
                var json = JsonConvert.SerializeObject(logParam);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = client.PostAsync("api/TA_Timelog/SaveListTimeLog", content).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        logMongo.Success = true;
                    }
                    else
                    {
                        logMongo.Success = false;
                    }
                    string data = response.Content.ReadAsStringAsync().Result;
                    IntegrateLogResult logResult = JsonConvert.DeserializeObject<IntegrateLogResult>(data);

                    try
                    {
                        emailProvider.SendMailIntegrateLog(cfg, logResult.SuccessDB, logResult.ErrorsDB, logResult.SuccessFile, logResult.ErrorsFile);
                    }
                    catch (Exception) { }
                }
                catch (Exception ex)
                {
                    logMongo.Error = ex.Message;
                }

                //ghi log 
                MongoDBHelper<IntegrateLogMongo> mongoObject = new MongoDBHelper<IntegrateLogMongo>("log_integrate", cache);
                mongoObject.AddDataToCollection(logMongo, true);
            }
        }
        
    }
}
