using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.FR05;
using EPAD_Data.Models.Other;
using EPAD_Data.Models.TimeLog;
using EPAD_Data.Models.WebAPIHeader;
using EPAD_Logic;
using EPAD_Services.FR05.Interface;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.FR05.Impl
{
    public class FR05_ClientService : IFR05_ClientService
    {
        private readonly IIC_DeviceMachineService _deviceMachineService;
        private readonly IIC_LogApiMachineService _logMachineService;
        private readonly IIC_UserApiMachineService _userMachineService;
        private readonly IIC_AttendanceLogService _attendanceLogService;
        private readonly IIC_UserInfoLogic _userInfoLogic;
        private readonly IIC_UserMasterService _IIC_UserMasterService;
        private readonly IIC_DeviceService _IC_DeviceService;
        private readonly IMemoryCache _cache;
        private readonly IServiceProvider _serviceProvider;
        private bool isAuto;
        private EPAD_Context _context { get; }
        private ezHR_Context _ezHR_Context { get; }
        private FR05Config _FR05Config;
        private IConfiguration _Configuration;
        public FR05_ClientService(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _deviceMachineService = _serviceProvider.GetService<IIC_DeviceMachineService>();
            _logMachineService = _serviceProvider.GetService<IIC_LogApiMachineService>();
            _userMachineService = _serviceProvider.GetService<IIC_UserApiMachineService>();
            _attendanceLogService = _serviceProvider.GetService<IIC_AttendanceLogService>();
            _userInfoLogic = _serviceProvider.GetService<IIC_UserInfoLogic>();
            _IIC_UserMasterService = _serviceProvider.GetService<IIC_UserMasterService>();
            _context = _serviceProvider.GetService<EPAD_Context>();
            _IC_DeviceService = _serviceProvider.GetService<IIC_DeviceService>();
            _cache = _serviceProvider.GetService<IMemoryCache>();
            _ezHR_Context = _serviceProvider.GetService<ezHR_Context>();
            _Configuration = configuration;
            _FR05Config = _Configuration.GetSection("FR05Config")
                          .Get<FR05Config>();
        }
        public async Task ProcessCommand(CommandResult command, UserInfo user, ConfigObject config, CompanyInfo companyInfo, bool isAuto)
        {
            this.isAuto = isAuto;
            var userIdSuceess = new List<string>();
            var userIdFail = new List<string>();
            var error = "";
            await UpdateCommandExecuting(int.Parse(command.ID));
            switch (command.Command)
            {
                case GlobalParams.ValueFunction.DownloadAllLog:

                    var logInfos = await _logMachineService.DownloadAllLog(command);
                    userIdSuceess = logInfos.userIdsSuccess;
                    userIdFail = logInfos.userIdsFailure;
                    error = logInfos.Error;
                    if (logInfos.LogInfos != null && logInfos.LogInfos.Count > 0)
                    {
                        var attendanceLogPram = new PostAttendanceLog()
                        {
                            ListAttendanceLog = logInfos.LogInfos,
                            SerialNumber = command.SerialNumber
                        };

                        //await _attendanceLogService.AddAttendanceLogByDevice(attendanceLogPram, user);
                        await CallDownloadLogApi(attendanceLogPram);
                    }
                    break;
                case GlobalParams.ValueFunction.DownloadLogFromToTime:

                    var logFromTime = await _logMachineService.DownloadLogFromToTime(command);
                    userIdSuceess = logFromTime.userIdsSuccess;
                    userIdFail = logFromTime.userIdsFailure;
                    error = logFromTime.Error;
                    var logInfoss = logFromTime.LogInfos.ToList();
                    var commandS = command.SerialNumber;
                    if (logFromTime.LogInfos != null && logFromTime.LogInfos.Count > 0)
                    {
                        var attendanceLogPram = new PostAttendanceLog()
                        {
                            ListAttendanceLog = logInfoss,
                            SerialNumber = commandS
                        };
                        await _attendanceLogService.AddAttendanceLogByDevice(attendanceLogPram, user);
                    }
                    break;
                case GlobalParams.ValueFunction.DeleteAllLog:

                    var logDelete = await _logMachineService.DeleteAllLog(command);
                    error = logDelete;

                    break;
                case GlobalParams.ValueFunction.DeleteLogFromToTime:
                    var deleteLog = await _logMachineService.DeleteLogFromToTime(command);
                    error = deleteLog;
                    break;
                case GlobalParams.ValueFunction.DownloadAllUser:
                    var usersDownload = await _userMachineService.DownloadAllUser(command, null);

                    userIdSuceess = usersDownload.UserIdsSuccess;
                    userIdFail = usersDownload.UserIdsFailed;
                    error = usersDownload.Error;
                    var userInfoTemp = CommonUtils.SplitList(usersDownload.UserInfos, 200);

                    foreach (var item in userInfoTemp)
                    {
                        UserInfoPram userInfoPram = new UserInfoPram()
                        {
                            ListUserInfo = item,
                            SerialNumber = command.SerialNumber
                        };

                        if (userInfoPram.ListUserInfo != null && userInfoPram.ListUserInfo.Count > 0)
                        {
                            await CallUserDownloadApi(userInfoPram);
                            //_userInfoLogic.CheckCreateOrUpdate(userInfoPram, user);
                        }
                    }
                    break;
                case GlobalParams.ValueFunction.DownloadUserById:
                    var userDownloadById = await _userMachineService.DownloadUserById(command, null);

                    userIdSuceess = userDownloadById.UserIdsSuccess;
                    userIdFail = userDownloadById.UserIdsFailed;
                    error = userDownloadById.Error;

                    if (userDownloadById.UserInfos != null && userDownloadById.UserInfos.Count > 0)
                    {
                        UserInfoPram userInfoPram = new UserInfoPram()
                        {
                            ListUserInfo = userDownloadById.UserInfos,
                            SerialNumber = command.SerialNumber
                        };
                        await CallUserDownloadApi(userInfoPram);
                        //_userInfoLogic.CheckCreateOrUpdate(userInfoPram, user);
                    }
                    break;
                case GlobalParams.ValueFunction.DownloadAllUserMaster:

                    var usersDownloadMaster = await _userMachineService.DownloadAllUser(command, null);

                    userIdSuceess = usersDownloadMaster.UserIdsSuccess;
                    userIdFail = usersDownloadMaster.UserIdsFailed;
                    error = usersDownloadMaster.Error;
                    var userInfoTempMaster = CommonUtils.SplitList(usersDownloadMaster.UserInfos, 200);

                    foreach (var item in userInfoTempMaster)
                    {
                        UserInfoPram userInfoPram = new UserInfoPram()
                        {
                            ListUserInfo = item,
                            SerialNumber = command.SerialNumber,
                            IsOverwriteData = command.IsOverwriteData,
                            SystemCommandIndex = int.Parse(command.ID),
                        };

                        if (userInfoPram.ListUserInfo != null && userInfoPram.ListUserInfo.Count > 0)
                        {
                            await CallUserMasterApi(userInfoPram);
                        }
                    }

                    break;
                case GlobalParams.ValueFunction.DownloadUserMasterById:
                    var usersDownloadMasterById = await _userMachineService.DownloadUserById(command, null);

                    userIdSuceess = usersDownloadMasterById.UserIdsSuccess;
                    userIdFail = usersDownloadMasterById.UserIdsFailed;
                    error = usersDownloadMasterById.Error;

                    UserInfoPram userInfoPramById = new UserInfoPram()
                    {
                        ListUserInfo = usersDownloadMasterById.UserInfos,
                        SerialNumber = command.SerialNumber,
                        IsOverwriteData = command.IsOverwriteData,
                        SystemCommandIndex = int.Parse(command.ID),
                    };

                    if (userInfoPramById.ListUserInfo != null && userInfoPramById.ListUserInfo.Count > 0)
                    {
                        await CallUserMasterApi(userInfoPramById);
                    }

                    break;
                case GlobalParams.ValueFunction.DeleteAllUser:
                    error = await _userMachineService.DeleteAllUser(command);
                    break;
                case GlobalParams.ValueFunction.DeleteAllFingerPrint:
                    //if (species == Species.SDK.ToString())
                    //{
                    //    user.DeleteAllFingerPrint(systemCommand, ref pErrorCode);
                    //}
                    //if (species == Species.UHF.ToString())
                    //{
                    //    //UHF_Device.DeleteAllFingerPrint(systemCommand, ref pErrorCode);
                    //}
                    break;
                case GlobalParams.ValueFunction.DeleteUserById:
                    var deleteById = await _userMachineService.DeleteUserById(command, null);
                    userIdSuceess = deleteById.UserIdsSuccess;
                    userIdFail = deleteById.UserIdsFailed;
                    error = deleteById.Error;

                    break;
                case GlobalParams.ValueFunction.UploadUsers:

                    var uploadUsers = await _userMachineService.UploadUsers(command, null);
                    userIdSuceess = uploadUsers.UserIdsSuccess;
                    userIdFail = uploadUsers.UserIdsFailed;
                    error = uploadUsers.Error;

                    break;
                case GlobalParams.ValueFunction.GetDeviceInfo:
                    var getDeviceInfo = await _deviceMachineService.GetDeviceInfo(command);
                    await _IC_DeviceService.UpdateDataDevice(getDeviceInfo, user.CompanyIndex);
                    break;
                case GlobalParams.ValueFunction.RestartDevice:
                    await _deviceMachineService.RestartDevice(command);
                    break;
                case GlobalParams.ValueFunction.SetTimeDevice:
                    await _deviceMachineService.SetDeviceTime(command);
                    break;
                default:
                    break;
            }

            string status = error == "" ? ValueResult.Success : ValueResult.Failure;
          

            UpdateStatusCommand(int.Parse(command.ID), status, error, JsonConvert.SerializeObject(userIdSuceess), JsonConvert.SerializeObject(userIdFail), _context, _ezHR_Context, config, command);

        }

        private async Task CallDownloadLogApi(PostAttendanceLog postAttendance)
        {
            var token = _cache.Get<string>($"tokenFR05");  

            if (string.IsNullOrEmpty(token))
            {
                token = await GetToken(_FR05Config);
                _cache.Set("tokenFR05", token);
            }

            HttpClient httpClient = new HttpClient();
            string content = JsonConvert.SerializeObject(postAttendance);
            var data = new StringContent(content, Encoding.UTF8, "application/json");
            List<WebAPIHeader> lstHeader = new List<WebAPIHeader>
            {
                new WebAPIHeader("Authorization", "Bearer " + token)
            };

            foreach (WebAPIHeader header in lstHeader)
                httpClient.DefaultRequestHeaders.Add(header.Name, header.Value);

            HttpResponseMessage response = await httpClient.PostAsync(_FR05Config.Host + "/api/AttendanceLog/AddAttendanceLogByDevice", data);
            var message = response.Content;
        }

        private async Task CallUserDownloadApi(UserInfoPram userInfos)
        {
            var token = _cache.Get<string>($"tokenFR05");
         

            if (string.IsNullOrEmpty(token))
            {
                token = await GetToken(_FR05Config);
                _cache.Set("tokenFR05", token);
            }

            HttpClient httpClient = new HttpClient();
            string content = JsonConvert.SerializeObject(userInfos);
            var data = new StringContent(content, Encoding.UTF8, "application/json");
            List<WebAPIHeader> lstHeader = new List<WebAPIHeader>
            {
                new WebAPIHeader("Authorization", "Bearer " + token)
            };

            foreach (WebAPIHeader header in lstHeader)
                httpClient.DefaultRequestHeaders.Add(header.Name, header.Value);

            HttpResponseMessage response = await httpClient.PostAsync(_FR05Config.Host + "/api/UserInfo/AddOrUpdateUserInfo", data);
            var message = response.Content;
        }

        private async Task CallUserMasterApi(UserInfoPram userInfos)
        {
            var token = _cache.Get<string>($"tokenFR05");
           

            if (string.IsNullOrEmpty(token))
            {
                token = await GetToken(_FR05Config);
                _cache.Set("tokenFR05", token);
            }

            HttpClient httpClient = new HttpClient();
            string content = JsonConvert.SerializeObject(userInfos);
            var data = new StringContent(content, Encoding.UTF8, "application/json");
            List<WebAPIHeader> lstHeader = new List<WebAPIHeader>
            {
                new WebAPIHeader("Authorization", "Bearer " + token)
            };

            foreach (WebAPIHeader header in lstHeader)
                httpClient.DefaultRequestHeaders.Add(header.Name, header.Value);

            HttpResponseMessage response = await httpClient.PostAsync(_FR05Config.Host + "/api/UserMaster/DownloadUserMasterSDK", data);
            var message = response.Content;
        }
        private async Task UpdateCommandExecuting(int commandId)
        {
            var token = _cache.Get<string>($"tokenFR05");
           

            if (string.IsNullOrEmpty(token))
            {
                token = await GetToken(_FR05Config);
                _cache.Set("tokenFR05", token);
            }

            HttpClient httpClient = new HttpClient();
            List<WebAPIHeader> lstHeader = new List<WebAPIHeader>
            {
                new WebAPIHeader("Authorization", "Bearer " + token)
            };

            foreach (WebAPIHeader header in lstHeader)
                httpClient.DefaultRequestHeaders.Add(header.Name, header.Value);

            HttpResponseMessage response = await httpClient.PostAsync(_FR05Config.Host + "/api/SystemCommand/UpdateCommandExecuting" + $"?systemCommandIndex={commandId}&firstUpdate={true}"
                , new StringContent(""));
            var message = response.Content;
        }

        public bool UpdateStatusCommand(int pId, string pStatus, string pError,
            string dataSuccess, string dataFailure,
            EPAD_Context context,
            ezHR_Context pOtherContext, ConfigObject pConfig, CommandResult command)
        {
            bool success = false;
            IC_SystemCommand cmdModel = context.IC_SystemCommand
                .Include(x => x.IC_Audits)
                .FirstOrDefault(t => t.Index == pId && t.IsActive);
            if (cmdModel == null)
                return success;

            DateTime now = DateTime.Now;



            if (command != null)
            {
                bool isDownloadAttendanceLogFail = command.Command == CommandAction.DownloadLogFromToTime.ToString() && pError == "Dữ liệu log đọc không thành công";
                if (pStatus == CommandStatus.Success.ToString())
                {
                    cmdModel.Excuted = true;
                    cmdModel.Error = "";
                    cmdModel.UpdatedDate = now;

                    #region Save executed command history to IC_Audit
                    var latestAudit = cmdModel.IC_Audits.LastOrDefault();
                    if (latestAudit != null)
                    {
                        var audit = new IC_Audit()
                        {
                            UserName = latestAudit?.UserName,
                            Name = latestAudit?.Name,
                            Description = latestAudit.Description,
                            DescriptionEn = latestAudit.DescriptionEn,
                            DateTime = DateTime.Now,
                            TableName = latestAudit.TableName,
                            PageName = latestAudit.PageName,
                            Status = AuditStatus.Completed.ToString(),
                            CompanyIndex = latestAudit.CompanyIndex,
                            IC_SystemCommandIndex = latestAudit.IC_SystemCommandIndex,
                            State = latestAudit.State
                        };

                        if (cmdModel.Command == CommandAction.DownloadLogFromToTime.ToString())
                        {
                            List<dynamic> logInfos = JsonConvert.DeserializeObject<List<dynamic>>(dataSuccess);
                            audit.NumSuccess = logInfos.Count();
                            audit.NewValues = dataSuccess;
                            if (isDownloadAttendanceLogFail)
                            {
                                audit.NumFailure = 0;
                                audit.Description = $"Thiết bị {command.SerialNumber} không có log điểm danh";
                                audit.DescriptionEn = $"Device {command.SerialNumber} has no attendance log";
                            }
                        }
                        else if (cmdModel.Command == CommandAction.SetTimeDevice.ToString()
                          || cmdModel.Command == CommandAction.DeleteLogFromToTime.ToString())
                        {
                            audit.NumFailure = null;
                            audit.NumSuccess = null;
                            audit.OldValues = null;
                            audit.NewValues = null;
                        }
                        else
                        {
                            // Trường hợp bình thường SDK_Interface trả về list userId
                            string[] userIdsFailure = JsonConvert.DeserializeObject<string[]>(dataFailure);
                            string[] userIdsSuccess = JsonConvert.DeserializeObject<string[]>(dataSuccess);
                            audit.NumFailure = userIdsFailure?.Length;
                            audit.NumSuccess = userIdsSuccess?.Length;
                        }
                        cmdModel.IC_Audits.Add(audit);
                    }
                    #endregion

                    context.SaveChanges();
                    //cập nhât working info

                    if ((cmdModel.Command == CommandAction.UploadUsers.ToString() || cmdModel.Command == CommandAction.DeleteUserById.ToString())
                        && cmdModel.EmployeeATIDs != "")
                    {
                        // kiểm tra xem còn lệnh thuộc workinginfo này chưa thực hiện ko

                        if (pConfig.IntegrateDBOther == true)
                        {
                            UpdateHRWorkingInfo(pOtherContext, cmdModel.EmployeeATIDs, true);
                        }
                        else
                        {
                            UpdateICWorkingInfo(context, cmdModel.EmployeeATIDs, true);
                        }

                    }
                }
                else if (pStatus == CommandStatus.DeleteManual.ToString())
                {
                    //cập nhât working info
                    if ((cmdModel.Command == CommandAction.UploadUsers.ToString() || cmdModel.Command == CommandAction.DeleteUserById.ToString())
                        && cmdModel.EmployeeATIDs != "")
                    {
                        if (pConfig.IntegrateDBOther == true)
                        {
                            UpdateHRWorkingInfo(pOtherContext, cmdModel.EmployeeATIDs, false);
                        }
                        else
                        {
                            UpdateICWorkingInfo(context, cmdModel.EmployeeATIDs, false);
                        }
                    }
                    context.SaveChanges();
                }
                else
                {
                    command.ErrorCounter++;
                    command.AppliedTime = DateTime.Now.AddMinutes(5);
                    command.Error = pError;

                    cmdModel.Error = pError;
                    cmdModel.UpdatedDate = now;
                    cmdModel.Excuted = true;

                    #region Add Audit about error of systemCommand
                    var latestAudit = cmdModel.IC_Audits.LastOrDefault();

                    var audit = new IC_Audit()
                    {
                        UserName = latestAudit?.UserName,
                        Name = latestAudit?.Name,
                        Description = pError,
                        DescriptionEn = pError,
                        DateTime = DateTime.Now,
                        TableName = latestAudit?.TableName,
                        PageName = latestAudit?.PageName,
                        Status = AuditStatus.Error.ToString(),
                        CompanyIndex = cmdModel.CompanyIndex,
                        IC_SystemCommandIndex = cmdModel.Index,
                        State = latestAudit?.State
                    };
                    cmdModel.IC_Audits.Add(audit);
                    #endregion

                    if (command.ErrorCounter >= 3)
                    {

                        //cập nhât working info
                        if ((cmdModel.Command == CommandAction.UploadUsers.ToString() || cmdModel.Command == CommandAction.DeleteUserById.ToString())
                            && cmdModel.EmployeeATIDs != "")
                        {
                            if (pConfig.IntegrateDBOther == true)
                            {
                                UpdateHRWorkingInfo(pOtherContext, cmdModel.EmployeeATIDs, false);
                            }
                            else
                            {
                                UpdateICWorkingInfo(context, cmdModel.EmployeeATIDs, false);
                            }
                        }
                    }
                    context.SaveChanges();
                }
            }
            success = true;
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
        private void UpdateICWorkingInfo(EPAD_Context pContext, string pData, bool pSuccess)
        {

            long index = 0;
            long.TryParse(pData, out index);
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

        public async Task<string> GetToken(FR05Config config)
        {
            dynamic result;
            LoginInfo loginInfo = new LoginInfo()
            {
                UserName = Convert.ToString(config.Username),
                Password = Convert.ToString(config.Password),
                ServiceId = Convert.ToString(config.ServiceId)
            };

            string content = JsonConvert.SerializeObject(loginInfo);
            StringContent data = new StringContent(content, Encoding.UTF8, "application/json");

            try
            {
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.PostAsync(config.Host + "/api/login", data);


                if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = ConvertJsonToObject(await response.Content.ReadAsStringAsync());

                    LoginApiInfo login = new LoginApiInfo()
                    {
                        AccessToken = result["access_token"],
                        TokenType = result["token_type"],
                        ExpiresIn = result["expires_in"]
                    };

                    return login.AccessToken;
                }
            }
            catch
            {
            }
            return "";
        }

        private dynamic ConvertJsonToObject(string pResult)
        {
            dynamic data = JsonConvert.DeserializeObject<object>(pResult, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return data;
        }

    }

}
