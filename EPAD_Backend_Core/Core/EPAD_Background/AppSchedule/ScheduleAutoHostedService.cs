using EPAD_Common;
using EPAD_Common.Utility;
using EPAD_Common.Extensions;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Logic.MainProcess;
using EPAD_Logic.SendMail;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EPAD_Common.Types;
using EPAD_Logic.Configuration;
using Microsoft.Extensions.Configuration;
using EPAD_Data.Models.WebAPIHeader;
using EPAD_Data.Models.FR05;
using EPAD_Data.Models.Other;
using EPAD_Services.Interface;
using ServiceStack.AsyncEx;

namespace EPAD_Background
{
    public class ScheduleAutoHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private Timer _timer1Minute;
        private Timer _timerCheck;
        private Timer _timer60Minute;
        private Timer _timer1Day;
        private Timer _timerIntervalSecond;
        private Timer _timerScheduleIntervalSecond;
        private Timer _timerScheduleIntervalSecondGetLog;
        private Timer _timerHalfMinute;
        private Timer _timer2Minute;
        private bool mIsProcess = false;
        private bool autoDeleteCommand = false;
        private string mLinkGCSMonitoringApi;
        private bool isUsingOnGcsEpadRealTime;

        private static IMemoryCache cache;
        private ConfigObject config;
        private IServiceScopeFactory _scopeFactory;
        private IEmailProvider emailProvider;
        private IHR_EmployeeLogic _IHR_EmployeeLogic;
        private IHR_EmployeeInfoLogic _IHR_EmployeeInfoLogic;
        private IHR_WorkingInfoLogic _IHR_WorkingInfoLogic;
        private IIC_EmployeeLogic _IIC_EmployeeLogic;
        private IIC_WorkingInfoLogic _IIC_WorkingInfoLogic;
        private IIC_CommandLogic _IIC_CommandLogic;
        private IIC_SystemCommandLogic _iC_SystemCommandLogic;
        private IIC_CommandSystemGroupLogic _iC_CommandSystemGroupLogic;
        private IIC_ConfigLogic _iIC_ConfigLogic;
        private IIC_Employee_IntegrateLogic _IC_Employee_IntegrateLogic;
        private IIC_UserMasterLogic _iC_UserMasterLogic;
        private IIC_DepartmentLogic _iC_DepartmentLogic;
        private IIC_DepartmentAndDeviceLogic _iC_DepartmentAndDeviceLogic;
        private IIC_UserInfoLogic _iC_UserInfoLogic;
        private IIC_ScheduleAutoHostedLogic _iC_ScheduleAutoHostedLogic;
        private IGC_TimeLogService _GC_TimeLogService;
        private IHR_CustomerInfoService _HR_CustomerInfoService;
        private IHttpClientFactory _ClientFactory;
        private IConfiguration _Configuration;
        private readonly IThirdPartyIntegrationConfigurationService _thirdPartyIntegrationConfigurationService;
        private string _configIntervalSecondIntergrateLog;
        private string _configIntervalSecondGetLog;
        private string _configScheduleIntervalSecond;
        private string _configClientName;
        private FR05Config _FR05Config;

        public ScheduleAutoHostedService(ILogger<ScheduleAutoHostedService> logger,
            IMemoryCache pCache, IServiceScopeFactory scopeFactory,
            IThirdPartyIntegrationConfigurationService thirdPartyIntegrationConfigurationService,
            IConfiguration configuration)
        {
            cache = pCache;
            config = ConfigObject.GetConfig(cache);
            _logger = logger;
            _scopeFactory = scopeFactory;
            _thirdPartyIntegrationConfigurationService = thirdPartyIntegrationConfigurationService;
            _Configuration = configuration;
            _configIntervalSecondIntergrateLog = _Configuration.GetValue<string>("IntervalSecond");
            _configIntervalSecondGetLog = _Configuration.GetValue<string>("IntervalGetLog");
            _configScheduleIntervalSecond = _Configuration.GetValue<string>("ScheduleIntervalSecond");
            _configClientName = _Configuration.GetValue<string>("ClientName").ToUpper();
            _FR05Config = _Configuration.GetSection("FR05Config").Get<FR05Config>();
            isUsingOnGcsEpadRealTime = Convert.ToBoolean(_Configuration.GetValue<string>("IsUsingOnEpad"));
        }

        private void InitServiceInject(IServiceScope scope)
        {
            _IHR_EmployeeLogic = scope.ServiceProvider.GetRequiredService<IHR_EmployeeLogic>();
            _IHR_EmployeeInfoLogic = scope.ServiceProvider.GetRequiredService<IHR_EmployeeInfoLogic>();
            _IC_Employee_IntegrateLogic = scope.ServiceProvider.GetRequiredService<IIC_Employee_IntegrateLogic>();
            _IHR_WorkingInfoLogic = scope.ServiceProvider.GetRequiredService<IHR_WorkingInfoLogic>();
            _IIC_EmployeeLogic = scope.ServiceProvider.GetRequiredService<IIC_EmployeeLogic>();
            _IIC_WorkingInfoLogic = scope.ServiceProvider.GetRequiredService<IIC_WorkingInfoLogic>();
            _IIC_CommandLogic = scope.ServiceProvider.GetRequiredService<IIC_CommandLogic>();
            _iC_CommandSystemGroupLogic = scope.ServiceProvider.GetRequiredService<IIC_CommandSystemGroupLogic>();
            _iC_SystemCommandLogic = scope.ServiceProvider.GetRequiredService<IIC_SystemCommandLogic>();
            _iIC_ConfigLogic = scope.ServiceProvider.GetRequiredService<IIC_ConfigLogic>();
            _iC_UserMasterLogic = scope.ServiceProvider.GetRequiredService<IIC_UserMasterLogic>();
            _iC_DepartmentLogic = scope.ServiceProvider.GetRequiredService<IIC_DepartmentLogic>();
            _iC_DepartmentAndDeviceLogic = scope.ServiceProvider.GetRequiredService<IIC_DepartmentAndDeviceLogic>();
            _iC_UserInfoLogic = scope.ServiceProvider.GetRequiredService<IIC_UserInfoLogic>();
            emailProvider = scope.ServiceProvider.GetRequiredService<IEmailProvider>();
            _iC_ScheduleAutoHostedLogic = scope.ServiceProvider.GetRequiredService<IIC_ScheduleAutoHostedLogic>();
            _GC_TimeLogService = scope.ServiceProvider.GetRequiredService<IGC_TimeLogService>();
            _HR_CustomerInfoService = scope.ServiceProvider.GetRequiredService<IHR_CustomerInfoService>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Thread.Sleep(1000);
            mIsProcess = false;
            autoDeleteCommand = false;
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("Background Service is starting.");
            Console.ResetColor();
            _timer1Minute = new Timer(DoWorkOneMinute, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            _timerCheck = new Timer(DoCheck, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            _timer60Minute = new Timer(DoWork60Minute, null, TimeSpan.Zero, TimeSpan.FromMinutes(60));
            //_timerHalfMinute = new Timer(DoWorkHalfMinute, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            //_timer2Minute = new Timer(DoWork2Minute, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
            if (!string.IsNullOrEmpty(_configIntervalSecondIntergrateLog) && _configClientName == ClientName.AVN.ToString())
            {
                double second = double.Parse(_configIntervalSecondIntergrateLog);
                _timerIntervalSecond = new Timer(DoWorkIntervalSecond, null, TimeSpan.Zero, TimeSpan.FromSeconds(second));
            }
            if (!string.IsNullOrEmpty(_configScheduleIntervalSecond))
            {
                double scheduleIntervalSecond = double.Parse(_configScheduleIntervalSecond);
                _timerScheduleIntervalSecond = new Timer(DoWorkScheduleIntervalSecond, null, TimeSpan.Zero, TimeSpan.FromSeconds(scheduleIntervalSecond));
            }
            if (!string.IsNullOrEmpty(_configIntervalSecondGetLog))
            {
                double scheduleIntervalSecond = double.Parse(_configIntervalSecondGetLog);
                _timerScheduleIntervalSecondGetLog = new Timer(DoWorkScheduleIntervalSecondLog, null, TimeSpan.Zero, TimeSpan.FromSeconds(scheduleIntervalSecond));
            }

            return Task.CompletedTask;
        }

        private void DoCheck(object state)
        {
            var scope = _scopeFactory.CreateScope();
            InitServiceInject(scope);
            try
            {
                try
                {
                    ImportDataFromGoogleSheet();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{ex}");
                }

                if (_timer1Minute == null)
                {
                    _timer1Minute = new Timer(DoWorkOneMinute, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
                }
                else if (_timer60Minute == null)
                {
                    _timer60Minute = new Timer(DoWork60Minute, null, TimeSpan.Zero, TimeSpan.FromMinutes(60));
                }
                else if (_configScheduleIntervalSecond == null && !string.IsNullOrEmpty(_configScheduleIntervalSecond))
                {
                    double scheduleIntervalSecond = double.Parse(_configScheduleIntervalSecond);
                    _timerScheduleIntervalSecond = new Timer(DoWorkScheduleIntervalSecond, null, TimeSpan.Zero, TimeSpan.FromSeconds(scheduleIntervalSecond));
                }
                else if (_timerScheduleIntervalSecondGetLog == null && !string.IsNullOrEmpty(_configIntervalSecondGetLog))
                {
                    double scheduleIntervalSecond = double.Parse(_configScheduleIntervalSecond);
                    _timerScheduleIntervalSecond = new Timer(DoWorkScheduleIntervalSecondLog, null, TimeSpan.Zero, TimeSpan.FromSeconds(scheduleIntervalSecond));
                }
                //else if (_timer2Minute == null)
                //{
                //    _timer2Minute = new Timer(DoWork2Minute, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }

        private void DoWorkHalfMinute(object state)
        {
            var scope = _scopeFactory.CreateScope();
            InitServiceInject(scope);
            //try { PingToFR05(); } catch (Exception es) { }
        }

        private void DoWork2Minute(object state)
        {
            var scope = _scopeFactory.CreateScope();
            InitServiceInject(scope);
            try
            {
                _timer2Minute.Change(Timeout.Infinite, Timeout.Infinite);
                _timer2Minute.Change(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
            }
            catch (Exception ex)
            {
                _timer2Minute.Change(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
            }
        }

        private void DoWorkOneMinute(object state)
        {
            var scope = _scopeFactory.CreateScope();
            InitServiceInject(scope);
            var now = DateTime.Now;

            var lstUser = new List<UserInfoOnMachine>();
            try
            {
                DownloadLogFromToTime(now, lstUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                DeleteHolidayByDay();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                DeleteLogFromToTime(now, lstUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                DownloadAllUser(now, lstUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                RestartDevice(now, lstUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                UpdateGroupCommand();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                TransferUser(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                IntegrateLog(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                OVNEmployeeIntegrate(now).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                IntegrateEmloyee(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }

            try
            {
                IntegrateLogFromDatabase(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }

            try
            {
                IntegrateEmloyeeToDatabase(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }

            try
            {
                IntegrateParking(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }

            try
            {
                IntegrateParkingLog(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }

            try
            {
                IntegrateInfoToOffline(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                AutoDeleteBlacklist(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }

            try
            {
                CheckDeviceFullCapacity(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                SyncDeviceTime(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                IntegrateBusinessTravel(now).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                AutomaticSendMail();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            if (isUsingOnGcsEpadRealTime)
            {
                try
                {
                    AutoUpdateLogInGateMandatoryByRule();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{ex}");
                }
            }
        }

        private void DoWork60Minute(object state)
        {
            var scope = _scopeFactory.CreateScope();
            InitServiceInject(scope);
            var now = DateTime.Now;
            //try 
            //{ 
            //    DoDeleteCommand(now); 
            //} 
            //catch (Exception ex) 
            //{ 
            //    _logger.LogError($"{ex}"); 
            //}
            try
            {
                AutoDeleteSystemCommand(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                UpdateStatusEvent(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                CreateCheckDeviceCapacityCommands();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            try
            {
                AutoRemoveStoppedWorkingEmployeesData();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }



        private void DoWorkIntervalSecond(object state)
        {
            var scope = _scopeFactory.CreateScope();
            InitServiceInject(scope);
            var now = DateTime.Now;
            try
            {
                IntegrateLogAVN(now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }

        private void DoWorkScheduleIntervalSecond(object state)
        {
            var scope = _scopeFactory.CreateScope();
            InitServiceInject(scope);
            try
            {
                SendEmailWhenDeviceOffline();
                CreateOnOffDeviceHistory();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }

        private void DoWorkScheduleIntervalSecondLog(object state)
        {
            var scope = _scopeFactory.CreateScope();
            InitServiceInject(scope);
            try
            {
                DownloadLogStateLog();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }

        private void DoDeleteCommand(object state)
        {
            // kiểm tra các command đã thực hiện và xóa trong db. ghi lên mongo
            DateTime now = DateTime.Now;
            if (mIsProcess == true)
            {
                return;
            }
            try
            {
                mIsProcess = true;
                _iC_ScheduleAutoHostedLogic.AutoDeleteExecutedCommand();

            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            mIsProcess = false;
        }

        private void UpdateStatusEvent(object state)
        {
            DateTime now = DateTime.Now;
            if (now.Hour == 1)
            {
                _iC_ScheduleAutoHostedLogic.AutoUpdateExpiratedTransferEmployee();
            }
        }

        private async Task CheckFR05Command()
        {
            var token = cache.Get<string>($"tokenFR05");


            if (string.IsNullOrEmpty(token))
            {
                token = await GetToken(_FR05Config);
                cache.Set("tokenFR05", token);
            }

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(_FR05Config.Host))
            {
                HttpClient httpClient = new HttpClient();
                string content = JsonConvert.SerializeObject(null);
                List<WebAPIHeader> lstHeader = new List<WebAPIHeader>
                {
                    new WebAPIHeader("Authorization", "Bearer " + token)
                };

                foreach (WebAPIHeader header in lstHeader)
                    httpClient.DefaultRequestHeaders.Add(header.Name, header.Value);

                HttpResponseMessage response = await httpClient.PostAsync(_FR05Config.Host + "/api/Command/GetFR05SystemCommandNeedExecute", null);
                var message = response.Content;
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

        private void AutoStartService(object state)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var epadContext = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
                    var listService = epadContext.IC_Service.Where(t => t.ServiceType == "SDKInterfaceService").ToList();

                    for (int i = 0; i < listService.Count; i++)
                    {
                        var service = new WindowsServiceHelper("SDK_Interface_" + listService[i].Index);
                        if (service.CheckServiceExists() == true && service.CheckServiceRunning() == false)
                        {
                            string error = "";
                            bool result = service.StartService(ref error);
                            if (result == false)
                            {
                                _logger.LogError("Start service failed. " + error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("AutoStartService error. " + ex.ToString());
            }
        }

        private void DoDeleteCache(object state)
        {
            try
            {
                UpdateGroupCommand();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine();
            Console.WriteLine("Background Service is stopping.");
            Console.ResetColor();
            _timer1Minute?.Change(Timeout.Infinite, 0);
            _timer60Minute?.Change(Timeout.Infinite, 0);
            _timerCheck?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer1Minute?.Dispose();
            _timerCheck?.Dispose();
            _timer60Minute?.Dispose();
        }

        private void DeleteHolidayByDay()
        {
            _iC_ScheduleAutoHostedLogic.DeleteHolidayByDay();
        }

        private void DownloadLogFromToTime(DateTime now, List<UserInfoOnMachine> lstUser)
        {
            _iC_ScheduleAutoHostedLogic.DownloadLogFromToTime().GetAwaiter().GetResult();
        }

        private void DownloadLogStateLog()
        {
            _iC_ScheduleAutoHostedLogic.DownloadLogStateLog();
        }


        //Note chậm
        private void AutoUpdateLogInGateMandatoryByRule()
        {
            _GC_TimeLogService.UpdateLogInGateMandatoryByRule(config.CompanyIndex);
        }

        private void DeleteLogFromToTime(DateTime now, List<UserInfoOnMachine> lstUser)
        {
            _iC_ScheduleAutoHostedLogic.DeleteLogFromToTime().GetAwaiter().GetResult();
        }

        public void DownloadAllUser(DateTime now, List<UserInfoOnMachine> lstUser)
        {
            _iC_ScheduleAutoHostedLogic.DownloadAllUser().GetAwaiter().GetResult();
        }

        public void DownloadUserById(DateTime now, List<UserInfoOnMachine> lstUser)
        {
            CreateGroupCommandForAction(now, ConfigAuto.DOWNLOAD_USER, CommandAction.DownloadUserMasterById, lstUser);
        }

        public void DeleteUserById(DateTime now, List<UserInfoOnMachine> lstUser)
        {
            CreateGroupCommandForAction(now, ConfigAuto.ADD_OR_DELETE_USER, CommandAction.DeleteUserById, lstUser);
        }

        public void UploadUsers(DateTime now, List<UserInfoOnMachine> lstUser)
        {
            CreateGroupCommandForAction(now, ConfigAuto.ADD_OR_DELETE_USER, CommandAction.UploadUsers, lstUser);
        }

        public void AutoDeleteBlacklist(DateTime now)
        {
            _iC_ScheduleAutoHostedLogic.AutoDeleteBlacklist(now);
        }

        public void RestartDevice(DateTime now, List<UserInfoOnMachine> lstUser)
        {
            _iC_ScheduleAutoHostedLogic.RestartDevice().GetAwaiter().GetResult();
        }

        public void TransferUser(DateTime pNow)
        {
            _iC_ScheduleAutoHostedLogic.AutoAddOrDeleteUser(pNow);
            _iC_ScheduleAutoHostedLogic.AutoAddOrDeleteCustomer(pNow);
        }

        public void AutoRemoveStoppedWorkingEmployeesData()
        {
            _iC_ScheduleAutoHostedLogic.AutoRemoveStoppedWorkingEmployeesData();
        }

        //private List<CommandResult> CreateCommandDeleteEmployeeStopped(DateTime pNow, IC_Config pConfig)
        //{
        //    List<CommandResult> lstCmd = new List<CommandResult>();
        //    Dictionary<string, List<UserInfoOnMachine>> dicListUserByDevice = new Dictionary<string, List<UserInfoOnMachine>>();
        //    List<AddedParam> addedParams = new List<AddedParam>();

        //    List<IC_EmployeeDTO> listStopped = new List<IC_EmployeeDTO>();

        //    if (config.IntegrateDBOther == false)
        //    {
        //        addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = pConfig.CompanyIndex });
        //        var listStopedWorkinginfo = _IIC_EmployeeLogic.GetManyStoppedWorking(addedParams);
        //        if (listStopedWorkinginfo != null)
        //        {
        //            listStopedWorkinginfo = listStopedWorkinginfo.Where(e => e.StoppedDate.HasValue && e.StoppedDate.Value.Date == DateTime.Now.Date).ToList();
        //            listStopped = listStopedWorkinginfo;
        //        }
        //    }
        //    else
        //    {
        //        addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = config.CompanyIndex });
        //        addedParams.Add(new AddedParam { Key = "StartedDate", Value = DateTime.Now });
        //        //addedParams.Add(new AddedParam { Key = "Synched", Value = null });
        //        var listStopedWorkinginfo =  _IHR_EmployeeLogic.GetManyStoppedWorking(addedParams).Result;
        //        if (listStopedWorkinginfo != null)
        //        {
        //            listStopped = listStopedWorkinginfo;
        //        }
        //    }


        //    foreach (var emp in listStopped)
        //    {
        //        List<string> lstSerial = db.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pConfig.CompanyIndex && x.DepartmentIndex == emp.DepartmentIndex).Select(x => x.SerialNumber).ToList();
        //        if (lstSerial.Count == 0)
        //        {
        //            lstSerial = db.IC_Device.Where(t => t.CompanyIndex == pConfig.CompanyIndex).Select(t => t.SerialNumber).ToList();
        //        }
        //        List<string> lsSerialApp = new List<string>();
        //        ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp);
        //        List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
        //        if (lstSerial.Count > 0)
        //        {
        //            IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
        //            paramUserOnMachine.ListEmployeeaATID = new List<string>() { emp.EmployeeATID };
        //            paramUserOnMachine.CompanyIndex = pConfig.CompanyIndex;
        //            paramUserOnMachine.ListSerialNumber = lstSerial;
        //            paramUserOnMachine.AuthenMode = "";
        //            paramUserOnMachine.FullInfo = true;
        //            lstUser = _IIC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
        //        }

        //        for (int i = 0; i < lstSerial.Count; i++)
        //        {
        //            if (dicListUserByDevice.ContainsKey(lstSerial[i]) == false)
        //            {
        //                dicListUserByDevice.Add(lstSerial[i], lstUser);
        //            }
        //            else
        //            {
        //                dicListUserByDevice[lstSerial[i]].AddRange(lstUser);
        //            }
        //        }

        //    }
        //    for (int i = 0; i < dicListUserByDevice.Count; i++)
        //    {
        //        IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
        //        commandParam.ListSerialNumber = new List<string>() { dicListUserByDevice.ElementAt(i).Key };
        //        commandParam.ListEmployee = dicListUserByDevice[dicListUserByDevice.ElementAt(i).Key];
        //        commandParam.Action = CommandAction.DeleteUserById;
        //        commandParam.FromTime = DateTime.Now;
        //        commandParam.ToTime = DateTime.Now;
        //        commandParam.ExternalData = "";
        //        commandParam.IsOverwriteData = false;
        //        List<CommandResult> listCmdTemp = _IIC_CommandLogic.CreateListCommands(commandParam);
        //        lstCmd.AddRange(listCmdTemp);
        //    }

        //    return lstCmd;
        //}
        //private List<CommandResult> CreateListCommandTransferUser_EPAD(DateTime pNow, IC_Config pConfig)
        //{
        //    DateTime toTime = pNow;
        //    DateTime fromTime = toTime.AddDays((pConfig.PreviousDays ?? 0) * -1);

        //    fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
        //    toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);

        //    SendMailAction sendMailAction = pConfig.SendMailWhenError ? SendMailAction.WhenError : pConfig.AlwaysSend ? SendMailAction.Always : SendMailAction.None;
        //    List<CommandResult> lstCmd = new List<CommandResult>();

        //    List<IC_EmployeeTransfer> lstTransfer = db.IC_EmployeeTransfer.Where(x => x.CompanyIndex == pConfig.CompanyIndex && x.FromTime >= fromTime && x.FromTime <= toTime && x.IsSync == false && x.Status == (long)TransferStatus.Approve).ToList();
        //    foreach (IC_EmployeeTransfer emp in lstTransfer)
        //    {
        //        if (emp.AddOnNewDepartment)
        //        {
        //            List<string> lstSerial = db.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pConfig.CompanyIndex && x.DepartmentIndex == emp.NewDepartment).Select(x => x.SerialNumber).ToList();
        //            List<string> lsSerialApp = new List<string>();
        //            ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp);
        //            List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
        //            if (lstSerial.Count > 0)
        //            {
        //                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
        //                paramUserOnMachine.ListEmployeeaATID = new List<string>() { emp.EmployeeATID };
        //                paramUserOnMachine.CompanyIndex = pConfig.CompanyIndex;
        //                paramUserOnMachine.ListSerialNumber = lstSerial;
        //                paramUserOnMachine.AuthenMode = "";
        //                paramUserOnMachine.FullInfo = true;
        //                lstUser = _IIC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
        //            }

        //            IC_CommandParamDTO commandParam1 = new IC_CommandParamDTO();
        //            commandParam1.ListSerialNumber = lsSerialApp;
        //            commandParam1.Action = CommandAction.UploadUsers;
        //            commandParam1.FromTime = fromTime;
        //            commandParam1.ToTime = toTime;
        //            commandParam1.ListEmployee = lstUser;
        //            commandParam1.IsOverwriteData = false;

        //            //List<CommandResult> lstCmdAddUser = CreateListCommand(db,lsSerialApp, CommandAction.UploadUsers, fromTime, toTime, lstUser, "", false);
        //            List<CommandResult> lstCmdAddUser = _IIC_CommandLogic.CreateListCommands(commandParam1);
        //            lstCmd.AddRange(lstCmdAddUser);
        //        }

        //        if (emp.RemoveFromOldDepartment)
        //        {
        //            List<string> lstSerial = db.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pConfig.CompanyIndex && x.DepartmentIndex == emp.OldDepartment).Select(x => x.SerialNumber).ToList();
        //            List<string> lsSerialApp = new List<string>();
        //            ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp);
        //            List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
        //            if (lstSerial.Count > 0)
        //            {
        //                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
        //                paramUserOnMachine.ListEmployeeaATID = new List<string>() { emp.EmployeeATID };
        //                paramUserOnMachine.CompanyIndex = pConfig.CompanyIndex;
        //                paramUserOnMachine.ListSerialNumber = lstSerial;
        //                paramUserOnMachine.AuthenMode = "";
        //                paramUserOnMachine.FullInfo = true;
        //                lstUser = _IIC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
        //            }
        //            IC_CommandParamDTO commandParam2 = new IC_CommandParamDTO();
        //            commandParam2.ListSerialNumber = lsSerialApp;
        //            commandParam2.Action = CommandAction.DeleteUserById;
        //            commandParam2.FromTime = fromTime;
        //            commandParam2.ToTime = toTime;
        //            commandParam2.ListEmployee = lstUser;
        //            commandParam2.IsOverwriteData = false;
        //            // List<CommandResult> lstCmdRemoveUser = CreateListCommand(db,lsSerialApp, CommandAction.DeleteUserById, fromTime, toTime, lstUser, "", false);
        //            List<CommandResult> lstCmdRemoveUser = _IIC_CommandLogic.CreateListCommands(commandParam2);
        //            lstCmd.AddRange(lstCmdRemoveUser);
        //        }
        //    }

        //    List<IC_EmployeeTransfer> lstTransferBack = db.IC_EmployeeTransfer.Where(x => x.CompanyIndex == pConfig.CompanyIndex && x.ToTime >= fromTime && x.ToTime <= toTime && x.Status == (long)TransferStatus.Approve).ToList();
        //    foreach (IC_EmployeeTransfer emp in lstTransferBack)
        //    {
        //        if (emp.AddOnNewDepartment)
        //        {
        //            List<string> lstSerial = db.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pConfig.CompanyIndex && x.DepartmentIndex == emp.OldDepartment).Select(x => x.SerialNumber).ToList();
        //            List<string> lsSerialApp = new List<string>();
        //            ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp);
        //            List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
        //            if (lstSerial.Count > 0)
        //            {
        //                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
        //                paramUserOnMachine.ListEmployeeaATID = new List<string>() { emp.EmployeeATID };
        //                paramUserOnMachine.CompanyIndex = pConfig.CompanyIndex;
        //                paramUserOnMachine.ListSerialNumber = lstSerial;
        //                paramUserOnMachine.AuthenMode = "";
        //                paramUserOnMachine.FullInfo = true;
        //                lstUser = _IIC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
        //            }
        //            IC_CommandParamDTO commandParam4 = new IC_CommandParamDTO();
        //            commandParam4.ListSerialNumber = lsSerialApp;
        //            commandParam4.Action = CommandAction.UploadUsers;
        //            commandParam4.FromTime = fromTime;
        //            commandParam4.ToTime = toTime;
        //            commandParam4.ListEmployee = lstUser;
        //            commandParam4.IsOverwriteData = false;
        //            //List<CommandResult> lstCmdAddUser = CreateListCommand(db,lsSerialApp, CommandAction.UploadUsers, fromTime, toTime, lstUser, "", false);
        //            List<CommandResult> lstCmdAddUser = _IIC_CommandLogic.CreateListCommands(commandParam4);
        //            lstCmd.AddRange(lstCmdAddUser);
        //        }

        //        if (emp.RemoveFromOldDepartment)
        //        {
        //            List<string> lstSerial = db.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pConfig.CompanyIndex && x.DepartmentIndex == emp.NewDepartment).Select(x => x.SerialNumber).ToList();
        //            //Check licence
        //            List<string> lsSerialApp = new List<string>();
        //            ListSerialCheckHardWareLicense(lstSerial, ref lsSerialApp);

        //            List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
        //            if (lstSerial.Count > 0)
        //            {
        //                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
        //                paramUserOnMachine.ListEmployeeaATID = new List<string>() { emp.EmployeeATID };
        //                paramUserOnMachine.CompanyIndex = pConfig.CompanyIndex;
        //                paramUserOnMachine.ListSerialNumber = lstSerial;
        //                paramUserOnMachine.AuthenMode = "";
        //                paramUserOnMachine.FullInfo = true;
        //                lstUser = _IIC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
        //            }
        //            IC_CommandParamDTO commandParam3 = new IC_CommandParamDTO();
        //            commandParam3.ListSerialNumber = lsSerialApp;
        //            commandParam3.Action = CommandAction.DeleteUserById;
        //            commandParam3.FromTime = fromTime;
        //            commandParam3.ToTime = toTime;
        //            commandParam3.ListEmployee = lstUser;
        //            commandParam3.IsOverwriteData = false;
        //            // List<CommandResult> lstCmdRemoveUser = CreateListCommand(db,lsSerialApp, CommandAction.DeleteUserById, fromTime, toTime, lstUser, "", false);
        //            List<CommandResult> lstCmdRemoveUser = _IIC_CommandLogic.CreateListCommands(commandParam3);
        //            lstCmd.AddRange(lstCmdRemoveUser);
        //        }
        //    }
        //    return lstCmd;
        //}

        //private List<CommandResult> CreateListCommandChangeDepartment_EPAD(DateTime pNow, IC_Config pConfig)
        //{
        //    DateTime toTime = pNow;
        //    DateTime fromTime = toTime.AddDays((pConfig.PreviousDays ?? 0) * -1);

        //    fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
        //    toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);


        //    List<string> listEmployeeATID = db.HR_User.Where(x => x.CompanyIndex == pConfig.CompanyIndex).Select(x => x.EmployeeATID).ToList();
        //    List<IC_WorkingInfo> listWorkingInfoAll = db.IC_WorkingInfo.Where(x => x.CompanyIndex == pConfig.CompanyIndex && listEmployeeATID.Contains(x.EmployeeATID) 
        //    && x.Status == (long)TransferStatus.Approve && x.FromDate.Date <= DateTime.Now.Date && (!x.ToDate.HasValue || x.ToDate.Value.Date >= DateTime.Now.Date)).ToList();

        //    List<WorkingInfoObject> listWorkingConvert = ConvertIC_WorkingInfoToWorkingInfoObject(listWorkingInfoAll);

        //    List<CommandResult> lstCmd = CreateListCommandFromWorkingInfoData(listEmployeeATID, listWorkingConvert, pNow, pConfig.CompanyIndex, fromTime, toTime);
        //    //update IC_WorkingInfo 
        //    for (int i = 0; i < listWorkingConvert.Count; i++)
        //    {
        //        IC_WorkingInfo workingInfoDB = listWorkingInfoAll.Where(x => x.Index.ToString() == listWorkingConvert[i].Index).FirstOrDefault();
        //        if (workingInfoDB != null)
        //        {
        //            workingInfoDB.IsSync = listWorkingConvert[i].IsSync;
        //        }
        //    }
        //    return lstCmd;
        //}

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
                            listWorkingByEmp[0].EmployeeATID, pFromTimeConfig, pToTimeConfig, indexWorking, false);

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
                    var workingNeedTransfer = listWorkingByEmp.FindAll(t => t.FromDate.Date <= pNow.Date).OrderByDescending(t => t.FromDate)
                        .FirstOrDefault();

                    if (workingNeedTransfer != null && workingNeedTransfer.IsSync == null)
                    {
                        long newDepartment = workingNeedTransfer.DepartmentIndex;
                        //tìm phòng ban cũ 
                        var workingOld = listWorkingByEmp.FindAll(t => t.FromDate.Date <= workingNeedTransfer.FromDate.Date
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
                        // thêm lên phòng ban mới
                        indexWorking = workingNeedTransfer.Index;
                        var listCmdTemp = CreateUploadUserCommand(pCompanyIndex, workingNeedTransfer.DepartmentIndex,
                            workingNeedTransfer.EmployeeATID, pFromTimeConfig, pToTimeConfig, indexWorking, false);
                        workingNeedTransfer.IsSync = false;
                        //xóa phòng ban cũ
                        if (workingOld != null)
                        {
                            listCmdTemp.AddRange(CreateDeleteUserCommand(pCompanyIndex, workingOld.DepartmentIndex,
                                workingOld.EmployeeATID, pFromTimeConfig, pToTimeConfig, indexWorking));
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

        private List<WorkingInfoObject> ConvertIC_WorkingInfoToWorkingInfoObject(List<IC_WorkingInfo> pListData)
        {
            List<WorkingInfoObject> listWorking = new List<WorkingInfoObject>();
            for (int i = 0; i < pListData.Count; i++)
            {
                WorkingInfoObject workingInfo = new WorkingInfoObject();
                workingInfo.EmployeeATID = pListData[i].EmployeeATID;
                workingInfo.FromDate = pListData[i].FromDate;
                workingInfo.ToDate = pListData[i].ToDate;
                workingInfo.IsSync = pListData[i].IsSync;
                workingInfo.Index = pListData[i].Index.ToString();
                workingInfo.DepartmentIndex = pListData[i].DepartmentIndex;

                listWorking.Add(workingInfo);
            }
            return listWorking;
        }

        private List<WorkingInfoObject> ConvertHR_WorkingInfoToWorkingInfoObject(List<HR_WorkingInfo> pListData)
        {
            List<WorkingInfoObject> listWorking = new List<WorkingInfoObject>();
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

        private List<CommandResult> CreateListCommandTransferUser_Integrate(DateTime pNow, IC_Config pConfig, ConfigObject pConfigFile)
        {
            DateTime toTime = pNow;
            DateTime fromTime = toTime.AddDays((pConfig.PreviousDays ?? 0) * -1);

            fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);
            toTime = new DateTime(toTime.Year, toTime.Month, toTime.Day, 23, 59, 59);

            List<IC_EmployeeDTO> listEmployee = new List<IC_EmployeeDTO>();
            List<AddedParam> addedParams = new List<AddedParam>();

            SendMailAction sendMailAction = pConfig.SendMailWhenError ? SendMailAction.WhenError : pConfig.AlwaysSend ? SendMailAction.Always : SendMailAction.None;

            //using (var scope = _scopeFactory.CreateScope())
            //{
            //_IHR_EmployeeLogic = scope.ServiceProvider.GetRequiredService<IHR_EmployeeLogic>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = pConfigFile.CompanyIndex });
            listEmployee = _IHR_EmployeeLogic.GetMany(addedParams);
            // }


            var listEmpATIDs = listEmployee.Select(u => u.EmployeeATID).ToList();

            using var scope = _scopeFactory.CreateScope();
            var integrateContext = scope.ServiceProvider.GetRequiredService<ezHR_Context>();

            List<HR_WorkingInfo> listWorking = integrateContext.HR_WorkingInfo.Where(t => t.CompanyIndex == pConfigFile.CompanyIndex
            && listEmpATIDs.Contains(t.EmployeeATID)).ToList();

            List<WorkingInfoObject> listWorkingConvert = ConvertHR_WorkingInfoToWorkingInfoObject(listWorking);

            var lstCmd = CreateListCommandFromWorkingInfoData(listEmpATIDs, listWorkingConvert, pNow, pConfig.CompanyIndex, fromTime, toTime);

            //update HR_WorkingInfo 
            for (int i = 0; i < listWorkingConvert.Count; i++)
            {
                HR_WorkingInfo workingInfoDB = listWorking.Where(x => x.Index.ToString() == listWorkingConvert[i].Index).FirstOrDefault();
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

        private List<CommandResult> CreateUploadUserCommand(int pCompanyIndex, long pNewDepartment, string pEmp, DateTime pFromTime, DateTime pToTime, string pExternalData, bool isOverwriteData)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            List<string> lstSerial = db.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pCompanyIndex && x.DepartmentIndex == pNewDepartment)
                .Select(x => x.SerialNumber).ToList();
            if (lstSerial == null || lstSerial.Count() == 0)
            {
                lstSerial = db.IC_Device.Select(e => e.SerialNumber).ToList();
            }
            List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
            if (lstSerial.Count > 0)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = new List<string>() { pEmp };
                paramUserOnMachine.CompanyIndex = pCompanyIndex;
                paramUserOnMachine.ListSerialNumber = lstSerial;
                paramUserOnMachine.AuthenMode = "";
                paramUserOnMachine.FullInfo = true;
                lstUser = _IIC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
            }
            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
            commandParam.ListSerialNumber = lstSerial;
            commandParam.Action = CommandAction.UploadUsers;
            commandParam.FromTime = pFromTime;
            commandParam.ToTime = pToTime;
            commandParam.ExternalData = pExternalData;
            commandParam.ListEmployee = lstUser;
            commandParam.IsOverwriteData = isOverwriteData;
            List<CommandResult> lstCmdAddUser = _IIC_CommandLogic.CreateListCommands(commandParam);
            return lstCmdAddUser;
        }

        private List<CommandResult> CreateDeleteUserCommand(int pCompanyIndex, long pOldDepartment, string pEmp, DateTime pFromTime, DateTime pToTime, string pExternalData)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            List<string> lstSerial = db.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pCompanyIndex && x.DepartmentIndex == pOldDepartment).Select(x => x.SerialNumber).ToList();
            List<UserInfoOnMachine> lstUser = new List<UserInfoOnMachine>();
            if (lstSerial.Count > 0)
            {
                IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                paramUserOnMachine.ListEmployeeaATID = new List<string>() { pEmp };
                paramUserOnMachine.CompanyIndex = pCompanyIndex;
                paramUserOnMachine.ListSerialNumber = lstSerial;
                paramUserOnMachine.AuthenMode = "";
                paramUserOnMachine.FullInfo = true;
                lstUser = _IIC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);
            }
            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
            commandParam.ListSerialNumber = lstSerial;
            commandParam.Action = CommandAction.DeleteUserById;
            commandParam.FromTime = pFromTime;
            commandParam.ToTime = pToTime;
            commandParam.ExternalData = pExternalData;
            commandParam.ListEmployee = lstUser;
            commandParam.IsOverwriteData = false;
            //List<CommandResult> lstCmdRemoveUser = CreateListCommand(db,lstSerial, CommandAction.DeleteUserById, pFromTime, pToTime, lstUser, pExternalData, false);
            List<CommandResult> lstCmdRemoveUser = _IIC_CommandLogic.CreateListCommands(commandParam);
            return lstCmdRemoveUser;
        }

        public void CreateGroupCommandForAction(DateTime pNow, ConfigAuto pCfgAuto, CommandAction pCmdAction, List<UserInfoOnMachine> lstUserInfo)
        {
            string timePostCheck = pNow.ToString("HH:mm");
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            List<IC_Config> lstCfg = db.IC_Config.Where(x => x.EventType == pCfgAuto.ToString()).ToList();
            foreach (IC_Config cfg in lstCfg)
            {
                List<string> timePos = cfg.TimePos.Split(new char[] { ';', '|', ',' }).ToList();
                if (!timePos.Contains(timePostCheck)) continue;
                System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/log.txt", "download");
                DateTime toTime = DateTime.Now;
                DateTime fromTime = toTime.AddDays((cfg.PreviousDays ?? 0) * -1);

                // tải log từ 0h sáng ngày hiện tại
                fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);

                // xóa log từ 2000-1-1 đến trước  23h59p59s ngày hiện tại PreviousDays ngày
                if (pCfgAuto == ConfigAuto.DELETE_LOG)
                {
                    fromTime = new DateTime(2000, 1, 1, 0, 0, 0);
                    toTime = new DateTime(pNow.Year, pNow.Month, pNow.Day, 23, 59, 59);
                    // trước hiện tại bao nhiêu ngày
                    toTime = toTime.AddDays((cfg.PreviousDays ?? 0) * -1);
                }

                bool isOverwriteData = false;
                List<string> lstSerialDownload = new List<string>();
                if (pCfgAuto == ConfigAuto.DOWNLOAD_USER)
                {
                    if (cfg.CustomField != null && cfg.CustomField != "")
                    {
                        object param = JsonConvert.DeserializeObject<object>(cfg.CustomField, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                        isOverwriteData = param.TryGetValue<bool>("IsOverwriteData");
                        lstSerialDownload = param.TryGetValue<string>("ListSerialNumber").ToString().Split(';').ToList();
                    }
                }


                SendMailAction sendMailAction = cfg.SendMailWhenError ? SendMailAction.WhenError : cfg.AlwaysSend ? SendMailAction.Always : SendMailAction.None;

                List<CommandResult> lstCmd = new List<CommandResult>();

                if (pCfgAuto == ConfigAuto.DOWNLOAD_USER)
                {
                    IC_CommandParamDTO comParam = new IC_CommandParamDTO();
                    comParam.ListSerialNumber = lstSerialDownload;
                    comParam.Action = pCmdAction;
                    comParam.FromTime = fromTime;
                    comParam.ToTime = toTime;
                    comParam.ExternalData = "";
                    comParam.IsOverwriteData = isOverwriteData;
                    List<CommandResult> lstCmdDownload = _IIC_CommandLogic.CreateListCommands(comParam);
                    lstCmd.AddRange(lstCmdDownload);
                }
                else
                {
                    List<IC_Service> lstService = db.IC_Service.Where(x => x.CompanyIndex == cfg.CompanyIndex).ToList();
                    foreach (IC_Service service in lstService)
                    {
                        List<string> lsSerialHw = new List<string>();

                        List<string> lstSerial = db.IC_ServiceAndDevices.Where(x => x.ServiceIndex == service.Index && x.CompanyIndex == cfg.CompanyIndex).Select(x => x.SerialNumber).ToList();
                        ListSerialCheckHardWareLicense(lstSerial, ref lsSerialHw);
                        _logger.LogError($"lsSerialHw " + String.Join(",", lsSerialHw));

                        IC_CommandParamDTO comParam = new IC_CommandParamDTO();
                        comParam.ListSerialNumber = lsSerialHw;
                        comParam.Action = pCmdAction;
                        comParam.FromTime = fromTime;
                        comParam.ToTime = toTime;
                        comParam.ExternalData = "";
                        comParam.IsOverwriteData = isOverwriteData;
                        List<CommandResult> lstCmdDownload = _IIC_CommandLogic.CreateListCommands(comParam);
                        lstCmd.AddRange(lstCmdDownload);

                        // command xóa log sau khi tải nếu có
                        if (pCfgAuto == ConfigAuto.DOWNLOAD_LOG && cfg.ProceedAfterEvent != "" && cfg.ProceedAfterEvent.Equals(true.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            comParam = new IC_CommandParamDTO();
                            comParam.ListSerialNumber = lsSerialHw;
                            comParam.Action = CommandAction.DeleteLogFromToTime;
                            comParam.FromTime = fromTime;
                            comParam.ToTime = toTime;
                            comParam.ExternalData = "";
                            comParam.IsOverwriteData = false;
                            //List<CommandResult> lstCmdDelete = CreateListCommand(db,lsSerialHw, CommandAction.DeleteLogFromToTime, fromTime, toTime, new List<UserInfoOnMachine>(), "", false);
                            List<CommandResult> lstCmdDelete = _IIC_CommandLogic.CreateListCommands(comParam);
                            lstCmd.AddRange(lstCmdDelete);
                        }
                    }
                }

                // Call add to cache here
                IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                grouComParam.CompanyIndex = cfg.CompanyIndex;
                grouComParam.ListCommand = lstCmd;
                grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                grouComParam.GroupName = pCmdAction.ToString();
                grouComParam.EventType = cfg.EventType;
                _IIC_CommandLogic.CreateGroupCommands(grouComParam);

            }
        }

        public void CreateGroupCommandForActionGroupDevice(DateTime pNow, ConfigAuto pCfgAuto, CommandAction pCmdAction, List<UserInfoOnMachine> lstUserInfo)
        {
            string timePostCheck = pNow.ToString("HH:mm");
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            List<IC_GroupDevice> lsgroupDevice = db.IC_GroupDevice.ToList();
            foreach (var groupDevice in lsgroupDevice)
            {
                List<IC_ConfigByGroupMachine> lstCfg = db.IC_ConfigByGroupMachine.Where(x => x.EventType == pCfgAuto.ToString() && x.GroupDeviceIndex == groupDevice.Index).ToList();

                foreach (IC_ConfigByGroupMachine cfg in lstCfg)
                {
                    List<string> timePos = cfg.TimePos.Split(new char[] { ';', '|', ',' }).ToList();
                    if (!timePos.Contains(timePostCheck)) continue;

                    DateTime toTime = DateTime.Now;
                    DateTime fromTime = toTime.AddDays((cfg.PreviousDays ?? 0) * -1);

                    // tải log từ 0h sáng ngày hiện tại
                    fromTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, 0, 0, 0);

                    // xóa log từ 2000-1-1 đến trước  23h59p59s ngày hiện tại PreviousDays ngày
                    if (pCfgAuto == ConfigAuto.DELETE_LOG)
                    {
                        fromTime = new DateTime(2000, 1, 1, 0, 0, 0);
                        toTime = new DateTime(pNow.Year, pNow.Month, pNow.Day, 23, 59, 59);
                        // trước hiện tại bao nhiêu ngày
                        toTime = toTime.AddDays((cfg.PreviousDays ?? 0) * -1);
                    }

                    bool isOverwriteData = false;
                    if (pCfgAuto == ConfigAuto.DOWNLOAD_USER)
                    {
                        if (cfg.CustomField != null && cfg.CustomField != "")
                        {
                            object param = JsonConvert.DeserializeObject<object>(cfg.CustomField, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                            isOverwriteData = param.TryGetValue<bool>("IsOverwriteData");
                        }
                    }

                    SendMailAction sendMailAction = cfg.SendMailWhenError ? SendMailAction.WhenError : cfg.AlwaysSend ? SendMailAction.Always : SendMailAction.None;

                    List<CommandResult> lstCmd = new List<CommandResult>();
                    //List<IC_Service> lstService = db.IC_Service.Where(x => x.CompanyIndex == cfg.CompanyIndex).ToList();
                    List<string> lstSerial = db.IC_GroupDeviceDetails.Where(x => x.GroupDeviceIndex == groupDevice.Index && x.CompanyIndex == cfg.CompanyIndex).Select(x => x.SerialNumber).ToList();

                    IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                    commandParam.ListSerialNumber = lstSerial;
                    commandParam.Action = pCmdAction;
                    commandParam.FromTime = fromTime;
                    commandParam.ToTime = toTime;
                    commandParam.ExternalData = "";
                    commandParam.IsOverwriteData = isOverwriteData;

                    //List<CommandResult> lstCmdDownload = CreateListCommand(db,lstSerial, pCmdAction, fromTime, toTime, new List<UserInfoOnMachine>(), "", isOverwriteData);
                    List<CommandResult> lstCmdDownload = _IIC_CommandLogic.CreateListCommands(commandParam);
                    lstCmd.AddRange(lstCmdDownload);
                    // command xóa log sau khi tải nếu có
                    if (pCfgAuto == ConfigAuto.DOWNLOAD_LOG && cfg.ProceedAfterEvent != "" && cfg.ProceedAfterEvent.Equals(true.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        commandParam = new IC_CommandParamDTO();
                        commandParam.ListSerialNumber = lstSerial;
                        commandParam.Action = CommandAction.DeleteLogFromToTime;
                        commandParam.FromTime = fromTime;
                        commandParam.ToTime = toTime;
                        commandParam.ExternalData = "";
                        commandParam.IsOverwriteData = false;
                        //List<CommandResult> lstCmdDelete = CreateListCommand(db,lstSerial, CommandAction.DeleteLogFromToTime, fromTime, toTime, new List<UserInfoOnMachine>(), "", false);
                        List<CommandResult> lstCmdDelete = _IIC_CommandLogic.CreateListCommands(commandParam);
                        lstCmd.AddRange(lstCmdDelete);
                    }
                    // Call add to cache here
                    IC_GroupCommandParamDTO groupComParam = new IC_GroupCommandParamDTO();
                    groupComParam.CompanyIndex = cfg.CompanyIndex;
                    groupComParam.GroupName = pCmdAction.ToString();
                    groupComParam.UserName = "SYSTEM_AUTO_" + groupDevice.Name;
                    groupComParam.ListCommand = lstCmd;
                    groupComParam.EventType = cfg.EventType;
                    _IIC_CommandLogic.CreateGroupCommands(groupComParam);
                    //CreateGroupCommand(cfg.CompanyIndex, "SYSTEM_AUTO_" + groupDevice.Name, pCmdAction.ToString(), "", lstCmd, cfg.EventType);
                }
            }
        }

        private void IntegrateLog(DateTime pNow)
        {
            _iC_ScheduleAutoHostedLogic.AutoIntegrateLog();
        }

        private void IntegrateLogAVN(DateTime pNow)
        {
            if (_configClientName == ClientName.AVN.ToString())
            {
                _iC_ScheduleAutoHostedLogic.AutoIntergrateAttLogAVN();
            }

        }

        private async Task OVNEmployeeIntegrate(DateTime pNow)
        {
            if (_configClientName == ClientName.OVN.ToString())
            {
                await _iC_ScheduleAutoHostedLogic.AutoGetOVNDepartment();
                var milliseconds = 60000;
                Thread.Sleep(milliseconds);
                await _iC_ScheduleAutoHostedLogic.AutoGetOVNEmployee();
                await _iC_ScheduleAutoHostedLogic.AutoGetOVNEmployeeShift();
            }

            if (_configClientName == ClientName.AVN.ToString())
            {
                await _iC_ScheduleAutoHostedLogic.AutoGetAVNEmployeeShift(pNow);
            }
        }

        private async Task IntegrateBusinessTravel(DateTime now)
        {

            if (_configClientName == ClientName.AVN.ToString())
            {
                await _iC_ScheduleAutoHostedLogic.AutoGetAVNIntegrateBusinessTravel(now);
            }
        }

        private void IntegrateEmloyee(DateTime pNow)
        {
            _iC_ScheduleAutoHostedLogic.AutoSyncIntegrateEmloyee(pNow);
        }

        private void IntegrateLogFromDatabase(DateTime pNow)
        {
            _iC_ScheduleAutoHostedLogic.AutoPostLogFromDatabase(pNow);
        }

        private void IntegrateEmloyeeToDatabase(DateTime pNow)
        {
            _iC_ScheduleAutoHostedLogic.AutoGetDataToDatabase(pNow);
        }

        private void IntegrateParking(DateTime pNow)
        {
            _iC_ScheduleAutoHostedLogic.IntegrateParking(pNow);
        }

        private void IntegrateInfoToOffline(DateTime pNow)
        {
            _iC_ScheduleAutoHostedLogic.IntegrateInfoToOffline(pNow);
        }


        private void IntegrateParkingLog(DateTime pNow)
        {
            _iC_ScheduleAutoHostedLogic.IntegrateParkingLog(pNow);
        }

        private void ImportDataFromGoogleSheet()
        {
            _HR_CustomerInfoService.ImportDataFromGoogleSheet(2);
        }

        private void AutoDeleteSystemCommand(object state)
        {
            if (autoDeleteCommand == true)
            {
                return;
            }
            else
            {
                autoDeleteCommand = true;
                _iC_ScheduleAutoHostedLogic.AutoDeleteSystemCommand();
                //using (IServiceScope scope = _scopeFactory.CreateScope())
                //{

                //    InitServiceInject(scope);
                //    var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();

                //    DateTime now = DateTime.Now;
                //    autoDeleteCommand = true;

                //    //List<AddedParam> addedParams = new List<AddedParam>();
                //    // List<IC_ConfigDTO> listconfig =  _iIC_ConfigLogic.GetMany(addedParams);

                //    IC_Config config = db.IC_Config.FirstOrDefault(u => u.EventType == ConfigAuto.DELETE_SYSTEM_COMMAND.ToString());
                //    if (config != null && config.CustomField != null)
                //    {
                //        IntegrateLogParam param = JsonConvert.DeserializeObject<IntegrateLogParam>(config.CustomField);
                //        if (param.AfterHours > 0)
                //        {

                //            List<IC_CommandSystemGroup> listCommandGroup = db.IC_CommandSystemGroup.AsEnumerable().Where(t => t.Excuted == false && now.Date.Subtract(t.CreatedDate.Value.Date).TotalHours >= param.AfterHours).ToList();
                //            if (listCommandGroup.Count > 0)
                //            {
                //                List<int> listGroupIndex = listCommandGroup.Select(t => t.Index).ToList();
                //                List<IC_SystemCommand> listCommand = db.IC_SystemCommand.Where(t => listGroupIndex.Contains(t.GroupIndex) && t.Excuted == false).ToList();
                //                List<IC_SystemCommandDTO> deleteCommands = listCommand.Select(u => new IC_SystemCommandDTO
                //                {
                //                    Index = u.Index,
                //                    GroupIndex = u.GroupIndex,
                //                    SerialNumber = u.SerialNumber,
                //                    EmployeeATIDs = u.EmployeeATIDs,
                //                    ExcutingServiceIndex = u.ExcutingServiceIndex,
                //                    Params = u.Params,
                //                    Excuted = u.Excuted
                //                }).ToList();
                //                ///_logger.LogError("Start sDeleteSystemCommandCacheAndDataBase: " + deleteCommands.Count().ToString());
                //                _iC_SystemCommandLogic.DeleteSystemCommandCacheAndDataBase(deleteCommands);
                //            }
                //        }
                //    }
                //}
            }
            autoDeleteCommand = false;
        }

        private string CreateUserInfoOnMachine_BasicInfo(int departmentIndex, int companyIndex, string empATID, string pEmpName,
            List<IC_DepartmentAndDevice> listDepartmentAndDevice, List<IC_Device> listDevice, ref Dictionary<string, List<UserInfoOnMachine>> dicListUserByDevice)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = companyIndex });
            addedParams.Add(new AddedParam { Key = "DepartmentIndex", Value = departmentIndex });
            var listDeviceByDepartment = _iC_DepartmentAndDeviceLogic.GetMany(addedParams);

            List<string> listSerial = new List<string>();
            string error = "";
            // if this department is no assign device then write to all devices
            if (listDeviceByDepartment.Count == 0)
            {
                listSerial = listDevice.Select(x => x.SerialNumber).ToList();
            }
            else
            {
                listSerial = listDeviceByDepartment.Select(x => x.SerialNumber).ToList();
            }

            UserInfoOnMachine userMachine = new UserInfoOnMachine();
            userMachine.UserID = int.Parse(empATID).ToString();
            userMachine.NameOnDevice = pEmpName;
            userMachine.Enable = true;
            for (int i = 0; i < listSerial.Count; i++)
            {
                List<UserInfoOnMachine> listUser = new List<UserInfoOnMachine>();
                if (dicListUserByDevice.ContainsKey(listSerial[i]) == false)
                {
                    listUser.Add(userMachine);
                    dicListUserByDevice.Add(listSerial[i], listUser);
                }
                else
                {
                    dicListUserByDevice[listSerial[i]].Add(userMachine);
                }
            }

            return error;
        }

        private void UpdateDataForDevice(Dictionary<string, List<UserInfoOnMachine>> dicListUserByDevice, List<IC_UserInfo> listUserInfo,
            int companyIndex, string eventType)
        {
            for (int i = 0; i < dicListUserByDevice.Count; i++)
            {
                List<UserInfoOnMachine> listUser = dicListUserByDevice[dicListUserByDevice.ElementAt(i).Key];
                foreach (UserInfoOnMachine item in listUser)
                {
                    IC_UserInfo userInfo = listUserInfo.FirstOrDefault(t => t.EmployeeATID == item.UserID);
                    if (userInfo != null)
                    {
                        item.PasswordOndevice = userInfo.Password;
                        item.CardNumber = userInfo.CardNumber;
                        item.Privilege = int.Parse(userInfo.Privilege == null ? "" : userInfo.Privilege.Value.ToString());
                    }
                }
                IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                commandParam.ListSerialNumber = new List<string>() { dicListUserByDevice.ElementAt(i).Key };
                commandParam.ListEmployee = listUser;
                commandParam.Action = CommandAction.UploadUsers;
                commandParam.FromTime = new DateTime(2000, 1, 1);
                commandParam.ToTime = DateTime.Now;
                commandParam.ExternalData = "";
                commandParam.IsOverwriteData = false;
                List<CommandResult> lstCmd = _IIC_CommandLogic.CreateListCommands(commandParam);

                //List<CommandResult> lstCmd = CreateListCommand(db,new List<string>() { dicListUserByDevice.ElementAt(i).Key }, CommandAction.UploadUsers,
                //    new DateTime(2000, 1, 1), DateTime.Now, listUser, "", false);

                IC_GroupCommandParamDTO groupComParam = new IC_GroupCommandParamDTO();
                groupComParam.CompanyIndex = companyIndex;
                groupComParam.UserName = UpdatedUser.AutoIntegrateEmployee.ToString();
                groupComParam.GroupName = GroupName.UploadUsers.ToString();
                groupComParam.ListCommand = lstCmd;
                groupComParam.EventType = eventType;
                _IIC_CommandLogic.CreateGroupCommands(groupComParam);
                // CreateGroupCommand(companyIndex, "AutoIntegrateEmployee", CommandAction.UploadUsers.ToString(), "", lstCmd, eventType);
            }
        }

        private async Task UpdateEmployeeIntegrate(List<EmployeeIntegrate> listParam, int pCompanyIndex, EmployeeIntegrateResult result, string eventType)
        {
            var listEmployeeTobeSync = new List<IC_EmployeeDTO>();

            for (int i = 0; i < listParam.Count; i++)
            {
                var validateInfo = new IC_EmployeeDTO();
                validateInfo.CardNumber = listParam[i].CardNumber;
                validateInfo.EmployeeATID = listParam[i].EmployeeATID;

                var errorList = _IIC_EmployeeLogic.ValidateEmployeeInfo(validateInfo);

                if (errorList != null && errorList.Count() > 0)
                {
                    result.ListIndexError.Add(listParam[i].Index);
                    result.ListError.AddRange(errorList);
                    continue;
                }

                var department = new IC_DepartmentDTO
                {
                    Code = listParam[i].DepartmentCode,
                    Name = listParam[i].DepartmentName,
                    CompanyIndex = pCompanyIndex,
                    CreatedDate = DateTime.Today,
                    UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                };
                department = _iC_DepartmentLogic.CheckExistedOrCreate(department, "");

                var employee = new IC_EmployeeDTO
                {
                    EmployeeATID = listParam[i].EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0'),
                    DepartmentIndex = department.Index,
                    EmployeeCode = listParam[i].EmployeeCode,
                    FullName = listParam[i].FullName,
                    CardNumber = listParam[i].CardNumber,
                    CompanyIndex = pCompanyIndex,
                    UpdatedDate = DateTime.Today,
                    UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                };
                if (listParam[i].Status == "D")
                {
                    if (listParam[i].StoppedDate != null)
                    {
                        employee.StoppedDate = listParam[i].StoppedDate;
                    }
                    else
                    {
                        employee.StoppedDate = DateTime.Today;
                    }
                }
                employee = await _IIC_EmployeeLogic.SaveOrUpdateAsync(employee);

                // check this employee has working info or not
                var workingInfo = new IC_WorkingInfoDTO();
                workingInfo.EmployeeATID = listParam[i].EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                workingInfo.CompanyIndex = pCompanyIndex;
                workingInfo.DepartmentIndex = Convert.ToInt32(department.Index);
                workingInfo.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                workingInfo.Status = (short)TransferStatus.Approve;
                workingInfo.ApprovedDate = DateTime.Today;
                workingInfo.FromDate = DateTime.Today;
                workingInfo.IsManager = false;
                workingInfo.IsSync = null;
                workingInfo.PositionName = listParam[i].Position;
                var updateWorkingResult = _IIC_WorkingInfoLogic.CheckUpdateOrInsert(workingInfo);

                var userMaster = new IC_UserMasterDTO();
                userMaster.EmployeeATID = employee.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                userMaster.CompanyIndex = employee.CompanyIndex;
                userMaster.CardNumber = employee.CardNumber;
                userMaster.Privilege = 0;
                userMaster.NameOnMachine = employee.NameOnMachine;
                userMaster.CreatedDate = DateTime.Now;
                userMaster.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                _iC_UserMasterLogic.CheckExistedOrCreate(userMaster);

                result.ListIndexSuccess.Add(listParam[i].Index);

                if (updateWorkingResult != null && updateWorkingResult.EmployeeATID != null)
                {
                    // add to list to be sync all device of deparment
                    listEmployeeTobeSync.Add(new IC_EmployeeDTO
                    {
                        EmployeeATID = listParam[i].EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0'),
                        CompanyIndex = pCompanyIndex
                    });
                }
            }
            //UpdateDataForDevice(dicListUserByDevice, listUserInfo, pCompanyIndex, eventType);

            await _IIC_CommandLogic.SyncWithEmployee(listEmployeeTobeSync.Select(u => u.EmployeeATID).ToList(), pCompanyIndex);
        }

        private int GetDepartmentIndexFromString(string pDepartment, List<IC_Department> pListDepartment, int pCompanyIndex, DateTime now, EPAD_Context context, ref string error)
        {
            string[] arrDep = pDepartment.Split('/');
            int parentIndex = 0;
            var listDepartmentIndexCreate = new List<int>();
            for (int i = 0; i < arrDep.Length; i++)
            {
                if (arrDep[i].Trim() == "")
                {
                    error = "DepartmentError";
                    break;
                }
                var department = pListDepartment.Where(t => t.Name.Equals(arrDep[i], StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                if (department == null)
                {
                    department = new IC_Department
                    {
                        Name = arrDep[i],
                        Location = "",
                        Description = "",
                        ParentIndex = parentIndex,
                        CompanyIndex = pCompanyIndex,
                        CreatedDate = now,
                        UpdatedDate = now,
                        UpdatedUser = UpdatedUser.IntegrateEmployee.ToString()
                    };

                    context.IC_Department.Add(department);
                    context.SaveChanges();
                    listDepartmentIndexCreate.Add(department.Index);
                }
                parentIndex = department.Index;
            }
            //revert
            if (error != "")
            {
                context.IC_Department.RemoveRange(context.IC_Department.Where(t => listDepartmentIndexCreate.Contains(t.Index)));
                context.SaveChanges();
            }
            return parentIndex;
        }

        private IC_UserInfo CreateNewUserInfo(string pEmpATID, string pEmpName, int pCompanyIndex, string pCardNumber, DateTime now)
        {
            IC_UserInfo userInfo = new IC_UserInfo();
            userInfo.EmployeeATID = pEmpATID;
            userInfo.CompanyIndex = pCompanyIndex;
            userInfo.SerialNumber = "";
            userInfo.UserName = "";
            userInfo.CardNumber = pCardNumber;
            userInfo.Privilege = 0;
            userInfo.Password = "";
            userInfo.Reserve1 = "";
            userInfo.Reserve2 = 0;
            userInfo.CreatedDate = now;
            userInfo.UpdatedDate = now;
            userInfo.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();

            return userInfo;
        }

        private void SendEmailWhenDeviceOffline()
        {
            _iC_ScheduleAutoHostedLogic.CreateEmailWhenDeviceOffline();
        }

        private void CreateOnOffDeviceHistory()
        {
            _iC_ScheduleAutoHostedLogic.CreateHistoryOnlineOfflineMachine();
        }

        private void CreateCheckDeviceCapacityCommands()
        {
            //_iC_ScheduleAutoHostedLogic.CreateCheckDeviceCapacityCommands();
        }

        private void AutomaticSendMail()
        {
            _iC_ScheduleAutoHostedLogic.AutomaticSendMail();
        }

        private void CheckDeviceFullCapacity(DateTime pNow)
        {
            _iC_ScheduleAutoHostedLogic.CheckDeviceFullCapacity();
            //using (var scope = _scopeFactory.CreateScope())
            //{
            //    _IHR_EmployeeLogic = scope.ServiceProvider.GetRequiredService<IHR_EmployeeLogic>();
            //    _IC_Employee_IntegrateLogic = scope.ServiceProvider.GetRequiredService<IIC_Employee_IntegrateLogic>();
            //    _IHR_WorkingInfoLogic = scope.ServiceProvider.GetRequiredService<IHR_WorkingInfoLogic>();
            //    _IIC_EmployeeLogic = scope.ServiceProvider.GetRequiredService<IIC_EmployeeLogic>();
            //    _IIC_WorkingInfoLogic = scope.ServiceProvider.GetRequiredService<IIC_WorkingInfoLogic>();
            //    _IIC_CommandLogic = scope.ServiceProvider.GetRequiredService<IIC_CommandLogic>();
            //    _iC_CommandSystemGroupLogic = scope.ServiceProvider.GetRequiredService<IIC_CommandSystemGroupLogic>();

            //    _iC_SystemCommandLogic = scope.ServiceProvider.GetRequiredService<IIC_SystemCommandLogic>();
            //    _iIC_ConfigLogic = scope.ServiceProvider.GetRequiredService<IIC_ConfigLogic>();
            //    _iC_UserMasterLogic = scope.ServiceProvider.GetRequiredService<IIC_UserMasterLogic>();
            //    _iC_DepartmentLogic = scope.ServiceProvider.GetRequiredService<IIC_DepartmentLogic>();
            //    _iC_DepartmentAndDeviceLogic = scope.ServiceProvider.GetRequiredService<IIC_DepartmentAndDeviceLogic>();
            //    _iC_UserInfoLogic = scope.ServiceProvider.GetRequiredService<IIC_UserInfoLogic>();
            //    emailProvider = scope.ServiceProvider.GetRequiredService<IEmailProvider>();
            //    db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            //    integrateContext = scope.ServiceProvider.GetRequiredService<ezHR_Context>();
            //    integrateCustomerContext = scope.ServiceProvider.GetRequiredService<Sync_Context>();
            //    string timePostCheck = pNow.ToString("HH:mm");
            //    List<IC_Config> lstCfg = db.IC_Config.Where(x => x.EventType == ConfigAuto.FULL_CAPACITY.ToString()).ToList();

            //    int maxPercent = 80;
            //    _logger.LogInformation("Start Check capacity: " + DateTime.Now.ToString());
            //    foreach (IC_Config cfg in lstCfg)
            //    {
            //        if (cfg.TimePos.Contains(pNow.ToString("HH:mm")) == false)
            //        {
            //            continue;
            //        }

            //        List<IC_Device> listDevice = db.IC_Device.Where(x => x.CompanyIndex == cfg.CompanyIndex).ToList();
            //        string bodyEmail = "";
            //        for (int i = 0; i < listDevice.Count; i++)
            //        {
            //            if (listDevice[i].UserCapacity != null && listDevice[i].UserCapacity.Value > 0)
            //            {
            //                int userCapacity = listDevice[i].UserCapacity.Value;
            //                int userCount = listDevice[i].UserCount == null ? 0 : listDevice[i].UserCount.Value;

            //                if (userCount > (userCapacity * maxPercent / 100))
            //                {
            //                    // user > 80% capacity
            //                    bodyEmail += $"Thiết bị {listDevice[i].AliasName}({listDevice[i].IPAddress}) đã sử dụng hơn {maxPercent}% ({userCount}/{userCapacity}) " +
            //                        $"bộ nhớ lưu trữ dữ liệu nhân viên, vui lòng cho sao lưu và xóa bớt dữ liệu trước khi tiếp tục sử dụng.\n";
            //                }
            //            }
            //            if (listDevice[i].FingerCapacity != null && listDevice[i].FingerCapacity.Value > 0)
            //            {
            //                int fingerCapacity = listDevice[i].FingerCapacity.Value;
            //                int fingerCount = listDevice[i].FingerCount == null ? 0 : listDevice[i].FingerCount.Value;

            //                if (fingerCount > (fingerCapacity * maxPercent / 100))
            //                {
            //                    // finger > 80% capacity
            //                    bodyEmail += $"Thiết bị {listDevice[i].AliasName}({listDevice[i].IPAddress}) đã sử dụng hơn {maxPercent}%({fingerCount}/{fingerCapacity}) " +
            //                        $"bộ nhớ lưu trữ dữ liệu vân tay, vui lòng cho sao lưu và xóa bớt dữ liệu trước khi tiếp tục sử dụng.\n";
            //                }
            //            }
            //            if (listDevice[i].AttendanceLogCapacity != null && listDevice[i].AttendanceLogCapacity.Value > 0)
            //            {
            //                int logCapacity = listDevice[i].AttendanceLogCapacity.Value;
            //                int attendanceLogCount = listDevice[i].AttendanceLogCount == null ? 0 : listDevice[i].AttendanceLogCount.Value;
            //                if (attendanceLogCount > (logCapacity * maxPercent / 100))
            //                {
            //                    // user > 80% capacity
            //                    bodyEmail += $"Thiết bị {listDevice[i].AliasName}({listDevice[i].IPAddress}) đã sử dụng hơn {maxPercent}%({attendanceLogCount}/{logCapacity}) " +
            //                        $"bộ nhớ lưu trữ dữ liệu dữ liệu điểm danh, vui lòng cho sao lưu và xóa bớt dữ liệu trước khi tiếp tục sử dụng.\n";
            //                }
            //            }
            //        }
            //        //send mail
            //        if (cfg.Email.Trim() != "" && bodyEmail.Trim() != "")
            //        {
            //            string title = "Thông báo máy chấm công gần đầy bộ nhớ";
            //            _logger.LogInformation("Start Send Email Check capacity: " + title.ToString());
            //            emailProvider.SendEmailToMulti("", title, bodyEmail, cfg.Email.Trim());

            //        }
            //    }
            //}
        }

        private void PingToFR05()
        {
            _iC_ScheduleAutoHostedLogic.AutoUpdateLastConnectionOfFR05();
        }

        private void SyncDeviceTime(DateTime pNow)
        {
            _iC_ScheduleAutoHostedLogic.AutoSyncDeviceTime();
            //using (var scope = _scopeFactory.CreateScope())
            //{
            //    _IHR_EmployeeLogic = scope.ServiceProvider.GetRequiredService<IHR_EmployeeLogic>();
            //    _IC_Employee_IntegrateLogic = scope.ServiceProvider.GetRequiredService<IIC_Employee_IntegrateLogic>();
            //    _IHR_WorkingInfoLogic = scope.ServiceProvider.GetRequiredService<IHR_WorkingInfoLogic>();
            //    _IIC_EmployeeLogic = scope.ServiceProvider.GetRequiredService<IIC_EmployeeLogic>();
            //    _IIC_WorkingInfoLogic = scope.ServiceProvider.GetRequiredService<IIC_WorkingInfoLogic>();
            //    _IIC_CommandLogic = scope.ServiceProvider.GetRequiredService<IIC_CommandLogic>();
            //    _iC_CommandSystemGroupLogic = scope.ServiceProvider.GetRequiredService<IIC_CommandSystemGroupLogic>();

            //    _iC_SystemCommandLogic = scope.ServiceProvider.GetRequiredService<IIC_SystemCommandLogic>();
            //    _iIC_ConfigLogic = scope.ServiceProvider.GetRequiredService<IIC_ConfigLogic>();
            //    _iC_UserMasterLogic = scope.ServiceProvider.GetRequiredService<IIC_UserMasterLogic>();
            //    _iC_DepartmentLogic = scope.ServiceProvider.GetRequiredService<IIC_DepartmentLogic>();
            //    _iC_DepartmentAndDeviceLogic = scope.ServiceProvider.GetRequiredService<IIC_DepartmentAndDeviceLogic>();
            //    _iC_UserInfoLogic = scope.ServiceProvider.GetRequiredService<IIC_UserInfoLogic>();
            //    emailProvider = scope.ServiceProvider.GetRequiredService<IEmailProvider>();
            //    db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            //    integrateContext = scope.ServiceProvider.GetRequiredService<ezHR_Context>();
            //    integrateCustomerContext = scope.ServiceProvider.GetRequiredService<Sync_Context>();
            //    string timePostCheck = pNow.ToString("HH:mm");
            //    List<IC_Config> lstCfg = db.IC_Config.Where(x => x.EventType == ConfigAuto.TIME_SYNC.ToString()).ToList();
            //    foreach (IC_Config cfg in lstCfg)
            //    {
            //        if (cfg.TimePos.Contains(pNow.ToString("HH:mm")) == false)
            //        {
            //            continue;
            //        }
            //        List<CommandResult> lstCmd = new List<CommandResult>();

            //        List<IC_Service> lstService = db.IC_Service.Where(x => x.CompanyIndex == cfg.CompanyIndex).ToList();
            //        List<string> listDeviceSerial = db.IC_Device.Where(x => x.CompanyIndex == cfg.CompanyIndex).Select(x => x.SerialNumber).ToList();

            //        foreach (IC_Service service in lstService)
            //        {
            //            List<string> lsSerialHw = new List<string>();

            //            List<string> lstSerial = db.IC_ServiceAndDevices.Where(x => x.ServiceIndex == service.Index
            //                && x.CompanyIndex == cfg.CompanyIndex && listDeviceSerial.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
            //            ListSerialCheckHardWareLicense(lstSerial, ref lsSerialHw);

            //            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
            //            commandParam.ListSerialNumber = lsSerialHw;
            //            commandParam.Action = CommandAction.SetTimeDevice;
            //            commandParam.FromTime = pNow;
            //            commandParam.ToTime = pNow;
            //            commandParam.ExternalData = "";
            //            commandParam.IsOverwriteData = false;

            //            //List<CommandResult> listCmdByService = CreateListCommand(db,lsSerialHw, CommandAction.SetTimeDevice, pNow, pNow, new List<UserInfoOnMachine>(), "", false);
            //            List<CommandResult> listCmdByService = _IIC_CommandLogic.CreateListCommands(commandParam);
            //            lstCmd.AddRange(listCmdByService);

            //        }

            //        // Call add to cache here
            //        IC_GroupCommandParamDTO groupComParam = new IC_GroupCommandParamDTO();
            //        groupComParam.CompanyIndex = cfg.CompanyIndex;
            //        groupComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
            //        groupComParam.GroupName = GroupName.SetTimeDevice.ToString();
            //        groupComParam.ListCommand = lstCmd;
            //        groupComParam.EventType = cfg.EventType;
            //        _IIC_CommandLogic.CreateGroupCommands(groupComParam);
            //        //CreateGroupCommand(cfg.CompanyIndex, UpdatedUser.SYSTEM_AUTO.ToString(), CommandAction.SetTimeDevice.ToString(), "", lstCmd, cfg.EventType);
            //    }
            //}
        }

        private List<CommandResult> CreateListCommand(EPAD_Context context, List<string> listSerial, CommandAction pAction, DateTime pFromTime, DateTime pToTime, List<UserInfoOnMachine> pListUsers, string pExternalData, bool isOverwriteData)
        {
            return CommandProcess.CreateListCommands(context, listSerial, pAction, pExternalData, pFromTime, pToTime, pListUsers, isOverwriteData, GlobalParams.DevicePrivilege.SDKStandardRole);
        }

        public void CreateGroupCommand(int pCompanyIndex, string pUserName, string pGroupName, string pExternalData, List<CommandResult> pListCommands, string pEventType)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            CommandProcess.CreateGroupCommand(db, cache, pCompanyIndex, pUserName, pGroupName, pExternalData, pListCommands, pEventType);
        }

        private List<IC_ServiceAndDevices> GetListServiceDevice(int companyIndex)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            List<IC_ServiceAndDevices> lstServiceDevice = (from s in db.IC_Service
                                                           join sd in db.IC_ServiceAndDevices
                                                           on s.Index equals sd.ServiceIndex
                                                           where s.CompanyIndex == companyIndex
                                                           select sd).ToList();
            return lstServiceDevice;
        }

        private void UpdateGroupCommand()
        {
            //_iC_ScheduleAutoHostedLogic.DownloadAllUser();

            ////using (var scope = _scopeFactory.CreateScope())
            ////{
            ////    _IHR_EmployeeLogic = scope.ServiceProvider.GetRequiredService<IHR_EmployeeLogic>();
            ////    _IC_Employee_IntegrateLogic = scope.ServiceProvider.GetRequiredService<IIC_Employee_IntegrateLogic>();
            ////    _IHR_WorkingInfoLogic = scope.ServiceProvider.GetRequiredService<IHR_WorkingInfoLogic>();
            ////    _IIC_EmployeeLogic = scope.ServiceProvider.GetRequiredService<IIC_EmployeeLogic>();
            ////    _IIC_WorkingInfoLogic = scope.ServiceProvider.GetRequiredService<IIC_WorkingInfoLogic>();
            ////    _IIC_CommandLogic = scope.ServiceProvider.GetRequiredService<IIC_CommandLogic>();
            ////    _iC_CommandSystemGroupLogic = scope.ServiceProvider.GetRequiredService<IIC_CommandSystemGroupLogic>();

            ////    _iC_SystemCommandLogic = scope.ServiceProvider.GetRequiredService<IIC_SystemCommandLogic>();
            ////    _iIC_ConfigLogic = scope.ServiceProvider.GetRequiredService<IIC_ConfigLogic>();
            ////    _iC_UserMasterLogic = scope.ServiceProvider.GetRequiredService<IIC_UserMasterLogic>();
            ////    _iC_DepartmentLogic = scope.ServiceProvider.GetRequiredService<IIC_DepartmentLogic>();
            ////    _iC_DepartmentAndDeviceLogic = scope.ServiceProvider.GetRequiredService<IIC_DepartmentAndDeviceLogic>();
            ////    _iC_UserInfoLogic = scope.ServiceProvider.GetRequiredService<IIC_UserInfoLogic>();
            ////    emailProvider = scope.ServiceProvider.GetRequiredService<IEmailProvider>();
            ////    db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            ////    integrateContext = scope.ServiceProvider.GetRequiredService<ezHR_Context>();
            ////    integrateCustomerContext = scope.ServiceProvider.GetRequiredService<Sync_Context>();
            ////    List<CommandParam> listParams = new List<CommandParam>();
            ////    List<IC_CommandSystemGroup> systemGroups = db.IC_CommandSystemGroup.Where(t => t.Excuted == false && t.CreatedDate.Value.AddMinutes(-1).ToString("HH:mm").Contains(DateTime.Now.AddDays(-1).ToString("HH:mm"))).ToList();
            ////    if (systemGroups != null && systemGroups.Count > 0)
            ////    {
            ////        string companyIndex = systemGroups.Select(t => t.CompanyIndex.ToString()).FirstOrDefault();

            ////        List<string> listGroupIndex = systemGroups.Select(t => t.Index.ToString()).ToList();
            ////        DateTime now = DateTime.Now;
            ////        CompanyInfo companyInfo = CompanyInfo.GetFromCache(cache, companyIndex);

            ////        foreach (var systemGroup in systemGroups)
            ////        {
            ////            List<IC_SystemCommand> systemCommands = db.IC_SystemCommand.Where(t => t.GroupIndex.Equals(systemGroup.Index)).ToList();
            ////            foreach (var systemCommand in systemCommands)
            ////            {
            ////                listParams.Add(new CommandParam()
            ////                {
            ////                    ID = systemCommand.Index.ToString(),
            ////                    SDKFuntion = systemCommand.Command,
            ////                    Status = CommandStatus.Success.ToString(),
            ////                    Error = "Expired"
            ////                });
            ////            }
            ////        }

            ////        if (companyInfo != null)
            ////        {
            ////            List<CommandGroup> listGroupInParam = companyInfo.ListCommandGroups.Where(t => listGroupIndex.Contains(t.ID)).ToList();
            ////            List<string> listGroupDeleteIndex = new List<string>();
            ////            //CheckGroupHasError(ref listGroupInParam);
            ////            for (int i = 0; i < listGroupInParam.Count; i++)
            ////            {
            ////                // xóa cmd trong company cache nếu status = success
            ////                UpdateCommandInCompanyCache(listParams, listGroupInParam[i].ID, companyInfo, now);
            ////                // nếu tất cả command trong group hoàn thành thì update group
            ////                List<CommandResult> listCommandResult = listGroupInParam[i].ListCommand;

            ////                bool allFinished = true;
            ////                for (int j = 0; j < listCommandResult.Count; j++)
            ////                {
            ////                    if (listCommandResult[j].Status == CommandStatus.UnExecute.ToString() || listCommandResult[j].Status == CommandStatus.Executing.ToString())
            ////                    {
            ////                        allFinished = false;
            ////                        break;
            ////                    }
            ////                    else if (listCommandResult[j].Error != null && listCommandResult[j].Error != "")
            ////                    {
            ////                        listGroupInParam[i].Errors.Add(listCommandResult[j].Error);
            ////                    }

            ////                }
            ////                DeleteCommandInCompanyCache(listCommandResult, listGroupInParam[i].ID, companyInfo, now);
            ////                if (allFinished == true)
            ////                {
            ////                    listGroupInParam[i].Excuted = true;
            ////                    listGroupInParam[i].FinishedTime = now;
            ////                    listGroupDeleteIndex.Add(listGroupInParam[i].ID);
            ////                    int index = int.Parse(listGroupInParam[i].ID);
            ////                    IC_CommandSystemGroup groupModel = db.IC_CommandSystemGroup.Where(t => t.Index == index).FirstOrDefault();
            ////                    if (groupModel != null)
            ////                    {
            ////                        groupModel.Excuted = true;
            ////                        groupModel.UpdatedDate = now;
            ////                        groupModel.UpdatedUser = UpdatedUser.UserSystem.ToString();

            ////                    }
            ////                    List<IC_SystemCommand> systemCommands = db.IC_SystemCommand.Where(t => t.GroupIndex.Equals(groupModel.Index) && t.Excuted.Equals(false)).ToList();
            ////                    foreach (var systemCommand in systemCommands)
            ////                    {
            ////                        systemCommand.Excuted = true;
            ////                        systemCommand.ExcutedTime = now;
            ////                        systemCommand.Error = "Expired";
            ////                        systemCommand.UpdatedUser = UpdatedUser.UserSystem.ToString();
            ////                    }
            ////                }
            ////            }
            ////            db.SaveChanges();

            ////            // remove group in cache
            ////            for (int i = 0; i < listGroupDeleteIndex.Count; i++)
            ////            {
            ////                companyInfo.DeleteGroupById(listGroupDeleteIndex[i]);
            ////            }
            ////        }
            ////    }
            ////}
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

    }

}
