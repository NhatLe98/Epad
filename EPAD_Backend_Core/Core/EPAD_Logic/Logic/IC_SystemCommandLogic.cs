using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EPAD_Common;
using EPAD_Common.Extensions;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic.MainProcess;
using EPAD_Logic.SendMail;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using static EPAD_Data.Models.IC_SignalRDTO;

namespace EPAD_Logic
{
    public class IC_SystemCommandLogic : IIC_SystemCommandLogic
    {
        private IEmailProvider emailProvider;
        private IMemoryCache _cache;
        private EPAD_Context _dbContext;
        private ezHR_Context _otherContext;
        private IIC_SignalRLogic _iC_SignalRLogic;
        public IC_SystemCommandLogic(EPAD_Context dbContext, ezHR_Context otherContext, IMemoryCache pCache,
            IEmailProvider pEmailProvider, IIC_SignalRLogic iC_SignalRLogic)
        {
            _dbContext = dbContext;
            emailProvider = pEmailProvider;
            _cache = pCache;
            _otherContext = otherContext;
            _iC_SignalRLogic = iC_SignalRLogic;
        }
        public bool Delete(IC_SystemCommandDTO systemcommand)
        {
            var result = _dbContext.IC_SystemCommand.FirstOrDefault(u => u.CompanyIndex == systemcommand.CompanyIndex && u.Index == systemcommand.Index);
            if (result != null)
            {
                _dbContext.Remove(result);
                _dbContext.SaveChanges();
                return true;
            }
            return false;
        }

        public bool Delete(List<int> listsystemcommand, int companyindex)
        {
            var result = _dbContext.IC_SystemCommand.Where(u => u.CompanyIndex == companyindex && listsystemcommand.Contains(u.Index)).ToList();
            if (result != null && result.Count > 0)
            {
                _dbContext.RemoveRange(result);
                _dbContext.SaveChanges();
                return true;
            }
            return false;
        }

        public List<IC_SystemCommandDTO> GetMany(List<AddedParam> addedParams)
        {
            if (addedParams == null || addedParams.Count == 0)
                return null;
            var query = _dbContext.IC_SystemCommand.AsQueryable();
            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "Filter":
                            if (p.Value != null)
                            {
                                string filter = p.Value.ToString();
                                query = query.Where(u => u.SerialNumber.Contains(filter)
                                        || u.CommandName.Contains(filter)
                                        || u.Error.Contains(filter));
                            }
                            break;
                        case "SystemCommandStatus":
                            if (p.Value != null)
                            {
                                bool excuted = Convert.ToBoolean(p.Value);
                                query = query.Where(u => u.Excuted == excuted);
                            }
                            break;
                        case "ListSystemCommandGroupIndex":
                            if (p.Value != null)
                            {
                                IList<int> listSystemCommandGroupIndex = (IList<int>)p.Value;
                                query = query.Where(u => listSystemCommandGroupIndex.Contains(u.GroupIndex));
                            }
                            break;
                        case "GroupIndex":
                            if (p.Value != null)
                            {
                                int groupIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.GroupIndex == groupIndex);
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "AfterHours":
                            if (p.Value != null)
                            {
                                int afterHours = Convert.ToInt32(p.Value);
                                query = query.Where(u => DateTime.Now.Subtract(u.CreatedDate.Value.Date).TotalHours >= afterHours);

                            }
                            break;
                        case "FromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.CreatedDate.Value.Date >= fromDate.Date);
                            }
                            break;
                        case "ToDate":
                            if (p.Value != null)
                            {
                                DateTime toDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.CreatedDate.Value.Date <= toDate.Date);
                            }
                            break;
                        case "CommandName":
                            if (p.Value != null)
                            {
                                string commandName = p.Value.ToString();
                                query = query.Where(u => u.CommandName == commandName);
                            }
                            break;
                        case "ListSerialNumber":
                            if (p.Value != null)
                            {
                                IList<string> listSerialNumber = (IList<string>)p.Value;
                                query = query.Where(u => listSerialNumber.Contains(u.SerialNumber));
                            }
                            break;
                    }
                }
            }
            query = query.OrderByDescending(u => u.CreatedDate);
            var data = query.AsEnumerable().Select(u => new IC_SystemCommandDTO
            {
                Index = u.Index,
                SerialNumber = u.SerialNumber,
                CommandName = u.CommandName,
                CompanyIndex = u.CompanyIndex,
                //ParamBody = JsonConvert.DeserializeObject(u.Params),
                CreatedDate = u.CreatedDate,
                Excuted = u.Excuted,
                ExcutedTime = u.ExcutedTime,
                SystemCommandStatus = GetStatus(u.Excuted),
                IsOverwriteData = u.IsOverwriteData
            }).ToList();

            return data;
        }

        public List<IC_SystemCommand> GetManyForUpdate(List<AddedParam> addedParams)
        {
            if (addedParams == null || addedParams.Count == 0)
                return null;
            var query = _dbContext.IC_SystemCommand.AsQueryable();
            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "Filter":
                            if (p.Value != null)
                            {
                                string filter = p.Value.ToString();
                                query = query.Where(u => u.SerialNumber.Contains(filter)
                                        || u.CommandName.Contains(filter)
                                        || u.Error.Contains(filter));
                            }
                            break;
                        case "SystemCommandStatus":
                            if (p.Value != null)
                            {
                                bool excuted = Convert.ToBoolean(p.Value);
                                query = query.Where(u => u.Excuted == excuted);
                            }
                            break;
                        case "ListSystemCommandGroupIndex":
                            if (p.Value != null)
                            {
                                IList<int> listSystemCommandGroupIndex = (IList<int>)p.Value;
                                query = query.Where(u => listSystemCommandGroupIndex.Contains(u.GroupIndex));
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "AfterHours":
                            if (p.Value != null)
                            {
                                int afterHours = Convert.ToInt32(p.Value);
                                query = query.Where(u => DateTime.Now.Subtract(u.CreatedDate.Value.Date).TotalHours >= afterHours);

                            }
                            break;
                        case "FromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.CreatedDate.Value.Date >= fromDate.Date);
                            }
                            break;
                        case "ToDate":
                            if (p.Value != null)
                            {
                                DateTime toDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.CreatedDate.Value.Date <= toDate.Date);
                            }
                            break;
                    }
                }
            }
            return query.OrderByDescending(u => u.CreatedDate).ToList();


        }

        public ListDTOModel<IC_SystemCommandDTO> GetPage(List<AddedParam> addedParams)
        {
            if (addedParams == null || addedParams.Count == 0)
                return null;

            var query = _dbContext.IC_SystemCommand.AsQueryable();
            var device = _dbContext.IC_Device.ToList();
            int pageIndex = 1;
            int pageSize = GlobalParams.ROWS_NUMBER_IN_PAGE;
            var pageIndexParam = addedParams.FirstOrDefault(u => u.Key == "PageIndex");
            var pageSizeParam = addedParams.FirstOrDefault(u => u.Key == "PageSize");
            if (pageIndexParam != null && pageIndexParam.Value != null)
            {
                pageIndex = Convert.ToInt32(pageIndexParam.Value);
            }
            if (pageSizeParam != null && pageSizeParam.Value != null)
            {
                pageSize = Convert.ToInt32(pageSizeParam.Value);
            }
            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "Filter":
                            if (p.Value != null)
                            {
                                string filter = p.Value.ToString();
                                query = query.Where(u => u.SerialNumber.Contains(filter)
                                        || u.CommandName.Contains(filter)
                                        || u.UpdatedUser.Contains(filter)
                                        || u.Error.Contains(filter));
                            }
                            break;
                        case "Excuted":
                            if (p.Value != null)
                            {
                                bool excuted = Convert.ToBoolean(p.Value);
                                query = query.Where(u => u.Excuted == excuted);
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "FromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.CreatedDate.Value.Date >= fromDate.Date);
                            }
                            break;
                        case "ToDate":
                            if (p.Value != null)
                            {
                                DateTime toDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.CreatedDate.Value.Date <= toDate.Date);
                            }
                            break;
                    }
                }
            }
            query = query.Where(x => x.IsActive);
            query = query.OrderByDescending(u => u.CreatedDate);

            ListDTOModel<IC_SystemCommandDTO> mv = new ListDTOModel<IC_SystemCommandDTO>();
            mv.TotalCount = query.Count();
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var data = query.Select(u => new IC_SystemCommandDTO
            {
                Index = u.Index,
                SerialNumber = u.SerialNumber,
               
                CommandName = u.Command, //Misc.GetCommandName(u.Command),
                CompanyIndex = u.CompanyIndex,
                //ParamBody = JsonConvert.DeserializeObject(u.Params),
                CreatedDate = u.CreatedDate,
                Excuted = u.Excuted,
                ExcutedTime = u.ExcutedTime,
                UpdatedDate = u.UpdatedDate,
                //SystemCommandStatus = u.ExcutedTime == null ? "Chờ thực hiện" : u.Excuted == false ? "Đang thực hiện" : "Đã thực hiện",//GetStatus(u.Excuted),
                GroupIndex = u.GroupIndex,
                IsOverwriteData = u.IsOverwriteData,
                UpdatedUser = u.UpdatedUser,
                Error = u.Error,
                IsActive = u.IsActive,
                IC_Audits = u.IC_Audits
                    .Where(x => x.Status == AuditStatus.Completed.ToString())
                    .Select(audit => new IC_AuditEntryDTO(null)
                    {
                        NumFailure = audit.NumFailure,
                        NumSuccess = audit.NumSuccess,
                        OldValuesString = audit.OldValues,
                        NewValuesString = audit.NewValues,
                    }).ToList(),
            }).ToList();
            data.ForEach(x =>
            {
                if (x.Excuted)
                {
                    x.SystemCommandStatus = "Đã thực hiện";
                }
                else
                {
                    x.SystemCommandStatus = "Chờ thực hiện";
                    if (x.ExcutedTime != null)
                    {
                        x.SystemCommandStatus = "Đang thực hiện";
                    }
                }
            });

            data = data.Select(u =>
            {
                u.DeviceName = device.FirstOrDefault(x => x.SerialNumber == u.SerialNumber).AliasName;
                return u;
            }).ToList();

            mv.PageIndex = pageIndex;
            mv.Data = data;
            return mv;
        }

        public void UpdateSystemCommandStatus(List<CommandParam> listParams, UserInfo userInfo)
        {
            ConfigObject config = ConfigObject.GetConfig(_cache);
            UserInfo user = userInfo;
            List<RemoteProcessLogObject> listLogs = new List<RemoteProcessLogObject>();
            DateTime now = DateTime.Now;
            List<string> listGroupIndex = new List<string>();
            CompanyInfo companyInfo = CompanyInfo.GetFromCache(_cache, user.CompanyIndex.ToString());

            for (int i = 0; i < listParams.Count; i++)
            {
                CommandResult cmd = new CommandResult();
                // cập nhật status cho cmd của service
                if (user.UpdateStatusCommand(int.Parse(listParams[i].ID), listParams[i].Status,
                    listParams[i].Error, dataSuccess: "", dataFailure: "",
                    ref cmd, _dbContext, _otherContext, config) == true)
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
            // cập nhật group command

            if (companyInfo != null)
            {

                List<CommandGroup> listGroupInParam = companyInfo.ListCommandGroups.Where(t => listGroupIndex.Contains(t.ID)).ToList();
                List<string> listGroupDeleteIndex = new List<string>();
                for (int g = 0; g < listGroupInParam.Count; g++)
                {
                    // xóa cmd trong company cache nếu status = success
                    UpdateCommandInCompanyCache(listParams, listGroupInParam[g].ID, companyInfo, now);
                    // nếu tất cả command trong group hoàn thành thì update group
                    List<CommandResult> listCommandResult = listGroupInParam[g].ListCommand;

                    bool allFinished = true;
                    for (int j = 0; j < listCommandResult.Count; j++)
                    {
                        if (listCommandResult[j].Status == CommandStatus.UnExecute.ToString() || listCommandResult[j].Status == CommandStatus.Executing.ToString())
                        {
                            allFinished = false;
                            break;
                        }
                        else if ((listCommandResult[j].Error != null && listCommandResult[j].Error != "") || listCommandResult[j].Status == CommandStatus.DeleteManual.ToString())
                        {
                            listGroupInParam[g].Errors.Add(listCommandResult[j].Error);
                        }

                    }
                    DeleteCommandInCompanyCache(listCommandResult, listGroupInParam[g].ID, companyInfo, now);

                    var checkallFinished = _dbContext.IC_SystemCommand.Where(t => t.GroupIndex.Equals(int.Parse(listGroupInParam[g].ID)) && t.Excuted == false).ToList();
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
                        listGroupInParam[g].Excuted = true;
                        listGroupInParam[g].FinishedTime = now;
                        listGroupDeleteIndex.Add(listGroupInParam[g].ID);
                        int index = int.Parse(listGroupInParam[g].ID);
                        IC_CommandSystemGroup groupModel = _dbContext.IC_CommandSystemGroup.Where(t => t.Index == index).FirstOrDefault();

                        if (groupModel != null)
                        {
                            groupModel.Excuted = true;
                            groupModel.UpdatedDate = now;

                            //Cập nhật IsSync trong IC_EmployeeTransfer
                            try
                            {
                                if (groupModel.GroupName.Equals(ConfigAuto.ADD_OR_DELETE_USER.ToString()))
                                {
                                    var systemCommands = _dbContext.IC_SystemCommand.Where(t => t.CompanyIndex.Equals(user.CompanyIndex) && t.GroupIndex.Equals(groupModel.Index)).ToList();
                                    if (systemCommands != null && systemCommands.Count > 0)
                                    {
                                        foreach (var systemCommand in systemCommands)
                                        {
                                            if (string.IsNullOrEmpty(systemCommand.Error))
                                            {
                                                CommandParamDB employees = JsonConvert.DeserializeObject<CommandParamDB>(systemCommand.Params, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                                                foreach (var employee in employees.ListUsers)
                                                {
                                                    IC_EmployeeTransfer employeeTransfer = _dbContext.IC_EmployeeTransfer.Where(t => t.CompanyIndex.Equals(user.CompanyIndex) && t.EmployeeATID.Equals(employee.UserID)).FirstOrDefault();
                                                    employeeTransfer.IsSync = true;
                                                }
                                            }
                                        }
                                    }
                                    _dbContext.SaveChanges();
                                }
                            }
                            catch
                            {

                            }

                            //groupModel.UpdatedUser = user.UserName;
                        }

                        try
                        {
                            var listSerialCommand = _dbContext.IC_SystemCommand.Where(t => t.CompanyIndex.Equals(groupModel.CompanyIndex) && t.GroupIndex.Equals(groupModel.Index) && t.Command.Equals(groupModel.GroupName)).ToList();

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

                            //List<IntergateLogParam> lsIntergateLog = listSerialCommand.Select(t => new { t.SerialNumber, JsonConvert.DeserializeObject<CommandParamDB>(t.Params).FromTime, JsonConvert.DeserializeObject<CommandParamDB>(t.Params).ToTime }).Cast<IntergateLogParam>().ToList();

                            //var listSerialCommandRestartService = context.IC_SystemCommand.Where(t => t.CompanyIndex.Equals(groupModel.CompanyIndex) && t.GroupIndex.Equals(groupModel.Index) && t.Command.Equals("RESTART_SERVICE"));

                            //listSerialCommand.AddRange(listSerialCommandRestartService);

                            List<SerialNumberCommandParam> listSerialCommandResult = new List<SerialNumberCommandParam>();

                            string notification = "";

                            foreach (var item in listSerialCommand)
                            {
                                GetResultCommand(item.Command, item.Error, ref notification, lsIntergateLog, user.CompanyIndex, listGroupInParam[g]);

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
                _dbContext.SaveChanges();
                // send mail process
                for (int i = 0; i < listGroupInParam.Count; i++)
                {
                    if (listGroupInParam[i].Excuted && listGroupInParam[i].EventType != "")
                    {
                        var checkConfig = _dbContext.IC_CommandSystemGroup.Where(t => t.Index.Equals(Convert.ToInt32(listGroupInParam[i].ID)) && t.UpdatedUser.Contains("SYSTEM_AUTO_")).FirstOrDefault();
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
            MongoDBHelper<RemoteProcessLogObject> mongoObject = new MongoDBHelper<RemoteProcessLogObject>("process_log", _cache);
            mongoObject.AddListDataToCollection(listLogs, true);

        }

        //public void PostPushNotification(int CompanyIndex, List<SerialNumberCommandParam> pNotification)
        //{
        //    Notification notification = new Notification()
        //    {
        //        CompanyIndex = CompanyIndex,
        //        Message = pNotification
        //    };
        //    var client = new HttpClient();
        //    client.Timeout = TimeSpan.FromSeconds(3);
        //    client.BaseAddress = new Uri(ConfigObject.GetConfig(_cache).PushNotificatioinLink);
        //    var json = JsonConvert.SerializeObject(notification);
        //    var content = new StringContent(json, Encoding.UTF8, "application/json");
        //    HttpResponseMessage response = client.PostAsync("api/Notification", content).Result;
        //}

        public void IntergateLogManual(List<IntergateLogParam> lsparam, int CompanyIndex)
        {
            IC_Config cfg = _dbContext.IC_Config.Where(x => x.EventType == ConfigAuto.INTEGRATE_LOG.ToString() && x.CompanyIndex == CompanyIndex).FirstOrDefault();
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
                    var listLogDB = _dbContext.IC_AttendanceLog.Where(t => t.CompanyIndex == cfg.CompanyIndex
                        && t.CheckTime >= item.FromTIme && t.CheckTime <= item.ToTime && t.SerialNumber == item.SerialNumber).ToList();
                    var listDevice = _dbContext.IC_Device.Where(t => t.CompanyIndex == CompanyIndex).ToList();

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
                        var log = new AttendanceLog();
                        var employee = _dbContext.HR_User.FirstOrDefault(x => x.EmployeeATID == listLogDB[i].EmployeeATID);

                        log.EmployeeATID = listLogDB[i].EmployeeATID;
                        log.EmployeeCode = employee?.EmployeeCode;
                        log.SerialNumber = listLogDB[i].SerialNumber;
                        log.CheckTime = listLogDB[i].CheckTime;
                        log.VerifyMode = listLogDB[i].VerifyMode;

                        log.InOutMode = listLogDB[i].InOutMode;
                        log.WorkCode = listLogDB[i].WorkCode;
                        log.Reserve1 = listLogDB[i].Reserve1;
                        log.UpdatedDate = listLogDB[i].UpdatedDate;

                        var device = listDevice.Find(t => t.SerialNumber == log.SerialNumber);
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
                MongoDBHelper<IntegrateLogMongo> mongoObject = new MongoDBHelper<IntegrateLogMongo>("log_integrate", _cache);
                mongoObject.AddDataToCollection(logMongo, true);
            }
        }

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
                            //Khi tải log manual thì tích hợp log
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
                default:
                    break;
            }
        }

        private string GetStatus(bool? excuted)
        {
            return excuted == null ? "Chờ thực hiện" : excuted == false ? "Đang thực hiện" : "Đã thực hiện";
        }
        private void DeleteCommandInCompanyCache(List<CommandResult> pListCommand, string pGroupIndex, CompanyInfo companyInfo, DateTime pNow)
        {
            for (int i = 0; i < pListCommand.Count; i++)
            {
                if (pListCommand[i].Status == CommandStatus.Success.ToString() || pListCommand[i].Status == CommandStatus.Failed.ToString() || pListCommand[i].Status == CommandStatus.DeleteManual.ToString())
                {
                    companyInfo.DeleteCommandById(pListCommand[i].ID, pGroupIndex);
                }
            }
        }
        private void UpdateCommandInCompanyCache(List<CommandParam> pListCommand, string pGroupIndex, CompanyInfo companyInfo, DateTime pNow)
        {
            for (int i = 0; i < pListCommand.Count; i++)
            {
                companyInfo.UpdateCommandById(pListCommand[i].ID, pGroupIndex, pListCommand[i].Status, pListCommand[i].Error, pNow);
            }
        }
        public void DeleteSystemCommandCacheAndDataBase(List<IC_SystemCommandDTO> listSystemCommand)
        {

            ConfigObject config = ConfigObject.GetConfig(_cache);
            CompanyInfo companyInfo = CompanyInfo.GetFromCache(_cache, config.CompanyIndex.ToString());
            List<RemoteProcessLogObject> listLogs = new List<RemoteProcessLogObject>();
            DateTime now = DateTime.Now;
            List<string> listGroupIndexSuccess = new List<string>();

            List<IC_SystemCommand> listDeleteSystemCommand = new List<IC_SystemCommand>();
            List<IC_CommandSystemGroup> listUpdateGroupCommand = new List<IC_CommandSystemGroup>();
            List<IC_CommandSystemGroup> listDeleteGroupCommand = new List<IC_CommandSystemGroup>();
            List<SystemCommandInfoMongo> listCommandInfoMongo = new List<SystemCommandInfoMongo>();

            var listGroupIndex = listSystemCommand.Select(u => u.GroupIndex).ToList();
            var listSystemCommandIndex = listSystemCommand.Select(u => u.Index).ToList();

            List<IC_CommandSystemGroup> listCommandGroup = _dbContext.IC_CommandSystemGroup.Where(t => listGroupIndex.Contains(t.Index) && config.CompanyIndex == t.CompanyIndex).ToList();
            List<IC_SystemCommand> listCurrentSystemCommand = _dbContext.IC_SystemCommand.Where(u => config.CompanyIndex == u.CompanyIndex && listGroupIndex.Contains(u.GroupIndex)).ToList();

            foreach (var group in listCommandGroup)
            {
                var listCommandByGroup = listCurrentSystemCommand.Where(t => t.GroupIndex == group.Index && listSystemCommandIndex.Contains(t.Index)).ToList();
                // add to log mongDB
                listCommandInfoMongo.Add(new SystemCommandInfoMongo() { Group = group, ListCommand = listCommandByGroup });
                // add to delete sys command
                listDeleteSystemCommand.AddRange(listCommandByGroup);

                // Find system command has no command in progress
                var listCommandInProgreeByGroup = listCurrentSystemCommand.Where(t => t.GroupIndex == group.Index && listSystemCommandIndex.Contains(t.Index) == false).ToList();

                if (listCommandInProgreeByGroup == null || listCommandInProgreeByGroup.Count() == 0)
                {
                    // ADD To this list and update this group is finished
                    listDeleteGroupCommand.Add(group);
                }
                else
                {
                    var listCommandHasFinished = listCommandInProgreeByGroup.Select(u => u.Excuted == false).Count();
                    // add to list update group status
                    if (listCommandHasFinished == 0)
                    {
                        group.Excuted = true;
                        listUpdateGroupCommand.Add(group);
                    }
                }
            }

            List<CommandResult> listCommandResult = new List<CommandResult>();
            listCommandResult = listDeleteSystemCommand.Select(u => new CommandResult
            {
                ID = u.Index.ToString(),
                GroupIndex = u.GroupIndex.ToString(),
                SerialNumber = u.SerialNumber
            }).ToList();

            // delete cmd của service
            CommandProcess.DeleteCommandForService(companyInfo, _cache, listCommandResult);
            // cập nhật status cho cmd của company
            CommandProcess.DeleteCommandForCompanyCache(companyInfo, _cache, listCommandResult);

            #region TODO check and revert data when delete command


            //Revert status command for IC_WorkingInfo
            //try
            //{
            //    foreach (var deleteComamnd in listDeleteSystemCommand)
            //    {
            //        if ((deleteComamnd.Command == GlobalParams.CommandAction.UploadUsers.ToString() || deleteComamnd.Command == GlobalParams.CommandAction.DeleteUserById.ToString())
            //            && deleteComamnd.EmployeeATIDs != "" && deleteComamnd.Excuted != true)
            //        {
            //            if (config.IntegrateDBOther == true)
            //            {
            //                UpdateHRWorkingInfo(_otherContext, deleteComamnd.EmployeeATIDs, false);
            //            }
            //            else
            //            {
            //                UpdateICWorkingInfo(_dbContext, deleteComamnd.EmployeeATIDs, false);
            //            }
            //        }
            //    }


            //    //foreach (var groupRevert in listUpdateGroupCommand)
            //    //{
            //    //    if (groupRevert.GroupName.Equals(ConfigAuto.ADD_OR_DELETE_USER.ToString()))
            //    //    {
            //    //        var systemCommands = _dbContext.IC_SystemCommand.Where(t => t.CompanyIndex.Equals(user.CompanyIndex) && t.GroupIndex.Equals(groupRevert.Index)).ToList();
            //    //        if (systemCommands != null && systemCommands.Count > 0)
            //    //        {
            //    //            foreach (var systemCommand in systemCommands)
            //    //            {
            //    //                if (string.IsNullOrEmpty(systemCommand.Error))
            //    //                {
            //    //                    CommandParamDB employees = JsonConvert.DeserializeObject<CommandParamDB>(systemCommand.Params, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            //    //                    foreach (var employee in employees.ListUsers)
            //    //                    {
            //    //                        Models.IC_EmployeeTransfer employeeTransfer = _dbContext.IC_EmployeeTransfer.Where(t => t.CompanyIndex.Equals(user.CompanyIndex) && t.EmployeeATID.Equals(employee.UserID)).FirstOrDefault();
            //    //                        employeeTransfer.IsSync = true;
            //    //                    }
            //    //                }
            //    //            }
            //    //        }
            //    //        _dbContext.SaveChanges();
            //    //    }
            //    //}
            //}
            //catch
            //{

            //}
            #endregion

            // Delete systemComamad
            if (listDeleteSystemCommand != null && listDeleteSystemCommand.Count() > 0)
            {
                var listDeleteSystemCommandIndex = listDeleteSystemCommand.Select(x => x.Index).ToHashSet();
                var listAuditSystemCommand = _dbContext.IC_Audit.Where(x => x.CompanyIndex == config.CompanyIndex && x.IC_SystemCommandIndex.HasValue
                    && listDeleteSystemCommandIndex.Contains(x.IC_SystemCommandIndex.Value)).ToList();
                if (listAuditSystemCommand != null && listAuditSystemCommand.Count > 0)
                {
                    _dbContext.IC_Audit.RemoveRange(listAuditSystemCommand);
                }
                _dbContext.IC_SystemCommand.RemoveRange(listDeleteSystemCommand);// Delete(listDeleteSystemCommand.Select(u => u.Index).ToList(), config.CompanyIndex);
            }

            // update Group Command status
            if (listUpdateGroupCommand != null && listUpdateGroupCommand.Count > 0)
            {
                _dbContext.IC_CommandSystemGroup.UpdateRange(listUpdateGroupCommand);
            }

            if (listDeleteGroupCommand != null && listDeleteGroupCommand.Count > 0)
            {
                _dbContext.IC_CommandSystemGroup.RemoveRange(listDeleteGroupCommand);

            }

            //// luu log to mongoDB
            //MongoDBHelper<SystemCommandInfoMongo> mongoObject = new MongoDBHelper<SystemCommandInfoMongo>("system_command", _cache);
            //if (mongoObject.CheckUseMongoDB() == true)
            //{
            //    mongoObject.AddListDataToCollection(listCommandInfoMongo, false);
            //}
            _dbContext.SaveChanges();
        }

        public bool RenewSystemCommand(int systemCommandIndex, UserInfo user)
        {
            bool success = false;

            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {

                    IC_SystemCommand oldCommand = _dbContext.IC_SystemCommand
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
                        context: _dbContext,
                        listSerial: new List<string> { newCommand.SerialNumber },
                        pAction: Enum.Parse<CommandAction>(newCommand.Command),
                        pExternalData: "",
                        pFromTime: new DateTime(2000, 1, 1),
                        pToTime: DateTime.Now, pListUsers: param.ListUsers,
                        isOverwriteData: newCommand.IsOverwriteData,
                        privilege: GlobalParams.DevicePrivilege.SDKStandardRole);

                    string groupName = _dbContext.IC_CommandSystemGroup
                        .FirstOrDefault(x => x.Index == newCommand.GroupIndex)?.GroupName ?? newCommand.CommandName;

                    CommandProcess.CreateGroupCommand(
                        context: _dbContext,
                        cache: _cache,
                        pCompanyIndex: newCommand.CompanyIndex,
                        pUserName: newCommand.UpdatedUser,
                        pGroupName: groupName,
                        pExternalData: "",
                        pListCommands: lstCmd,
                        pEventType: ""
                    );

                    IC_Audit firstAuditOfOldCommand = _dbContext.IC_Audit
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
                        _dbContext.IC_Audit.RemoveRange(_dbContext.IC_Audit.Where(x => x.IC_SystemCommandIndex == systemCommandIndex));
                        _dbContext.IC_SystemCommand.Remove(oldCommand);
                        _dbContext.IC_Audit.Add(auditOfNewCommand);
                        _dbContext.SaveChanges();
                    }



                    transaction.Commit();
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


        private void UpdateHRWorkingInfo(ezHR_Context pOtherContext, string pData, bool pSuccess)
        {

            long index = 0;
            long.TryParse(pData, out index);
            HR_WorkingInfo working = pOtherContext.HR_WorkingInfo.Where(t => t.Index == index).FirstOrDefault();
            if (working != null)
            {
                if (pSuccess == true)
                {
                    working.Synched = 1;
                }
                else
                {
                    working.Synched = null;
                }
                pOtherContext.SaveChanges();
            }

        }
        private void UpdateICWorkingInfo(EPAD_Context pContext, string WorkingIndex, bool pSuccess)
        {

            long index = 0;
            long.TryParse(WorkingIndex, out index);
            IC_WorkingInfo working = pContext.IC_WorkingInfo.Where(t => t.Index == index).FirstOrDefault();
            if (working != null)
            {
                if (pSuccess == true)
                {
                    working.IsSync = true;
                }
                else
                {
                    working.IsSync = null;
                }
                pContext.SaveChanges();
            }

        }

        public void UpdateLastConnection(List<string> lsSerialNumber, int CompanyIndex)
        {
            if (lsSerialNumber != null && lsSerialNumber.Count > 0)
            {
                var listDevice = _dbContext.IC_Device.Where(t => t.CompanyIndex == CompanyIndex && lsSerialNumber.Contains(t.SerialNumber)).ToList();
                if (listDevice != null && listDevice.Count > 0)
                {
                    foreach (var item in lsSerialNumber)
                    {
                        var device = listDevice.FirstOrDefault(x => x.SerialNumber == item);
                        if (device != null)
                        {
                            device.LastConnection = DateTime.Now;
                            if (!IsDeviceOfflined(device, _cache, null) && device.IsSendMailLastDisconnect)
                            {
                                device.IsSendMailLastDisconnect = false;
                            }
                        }
                    }
                    _dbContext.SaveChanges();
                }
            }
        }

        private bool IsDeviceOfflined(IC_Device pDevice, IMemoryCache pCache, Dictionary<string, List<IC_SystemCommand>> pProcessingDevice)
        {
            return CaculateTime(pDevice.LastConnection, DateTime.Now) >= ConfigObject.GetConfig(pCache).LimitedTimeConnection;
        }

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
    }

    public interface IIC_SystemCommandLogic
    {
        List<IC_SystemCommandDTO> GetMany(List<AddedParam> addedParams);
        List<IC_SystemCommand> GetManyForUpdate(List<AddedParam> addedParams);
        ListDTOModel<IC_SystemCommandDTO> GetPage(List<AddedParam> addedParams);
        bool Delete(IC_SystemCommandDTO systemcommand);
        bool Delete(List<int> listsystemcommand, int companyIndex);
        void GetResultCommand(string pCommand, string pError, ref string pResultNotification, List<IntergateLogParam> logParams, int CompanyIndex, CommandGroup command);
        void IntergateLogManual(List<IntergateLogParam> lsparam, int CompanyIndex);
        void UpdateSystemCommandStatus(List<CommandParam> listParams, UserInfo userInfo);
        void DeleteSystemCommandCacheAndDataBase(List<IC_SystemCommandDTO> listSystemCommand);
        bool RenewSystemCommand(int systemCommandIndex, UserInfo user);
        void UpdateLastConnection(List<string> lsSerialNumber, int CompanyIndex);
    }
}
