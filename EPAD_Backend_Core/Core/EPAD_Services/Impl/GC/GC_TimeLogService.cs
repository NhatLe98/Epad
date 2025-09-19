using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace EPAD_Services.Impl
{
    public class GC_TimeLogService : BaseServices<GC_TimeLog, EPAD_Context>, IGC_TimeLogService
    {
        private IServiceScopeFactory _scopeFactory;
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        IGC_GatesService _GC_GatesService;
        IGC_Gates_LinesService _GC_Gates_LinesService;
        IGC_Rules_GeneralService _GC_Rules_GeneralService;
        IHR_EmployeeInfoService _HR_EmployeeInfoService;
        public GC_TimeLogService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration, IServiceScopeFactory scopeFactory) : base(serviceProvider)
        {
            _scopeFactory = scopeFactory;
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_TimeLogService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
            _GC_GatesService = serviceProvider.GetService<IGC_GatesService>();
            _GC_Gates_LinesService = serviceProvider.GetService<IGC_Gates_LinesService>();
            _GC_Rules_GeneralService = serviceProvider.GetService<IGC_Rules_GeneralService>();
            _HR_EmployeeInfoService = serviceProvider.GetService<IHR_EmployeeInfoService>();
        }

        public async Task AddTimeLog(GC_TimeLog timeLog)
        {
            await DbContext.GC_TimeLog.AddAsync(timeLog);
            await SaveChangeAsync();
        }

        public async Task SaveChangeAsync()
        {
            await DbContext.SaveChangesAsync();
        }

        public DataGridClass GetPaginationList(IEnumerable<MonitoringGatesHistoryModel> histories, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            if (skip < 0)
            {
                skip = 0;
            }
            int countTotal = histories.Count();
            var dummy = histories.Skip(skip).Take(pageSize).ToList();
            DataGridClass grid = new DataGridClass(countTotal, dummy);
            return grid;
        }

        public async Task<List<GC_TimeLog>> GetLineValidLogs(int companyIndex, List<int> lines)
        {
            var result = await _dbContext.GC_TimeLog.Where(t => t.CompanyIndex == companyIndex
                && t.ApproveStatus == (short)ApproveStatus.Approved && lines.Contains(t.LineIndex)
                && !(t.Note != null && t.Note.Contains("MissingLogOutAfterExpiredPresenceTime"))).ToListAsync();

            return result;
        }

        public async Task UpdateLogInGateMandatoryByRule(int companyIndex)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EPAD_Context>();

            var gates = await _GC_GatesService.GetDataByCompanyIndex(companyIndex);
            var gatesIndexes = gates.Select(x => x.Index).ToList();
            var lines = _GC_Gates_LinesService.Where(e => gatesIndexes.Contains(e.GateIndex)).Select(e => e.LineIndex).ToList();

            var listLog = await db.GC_TimeLog.Where(t => t.CompanyIndex == companyIndex
                && t.ApproveStatus == (short)ApproveStatus.Approved && lines.Contains(t.LineIndex)
                ).ToListAsync();
            var listExpiredPresenceTimeLogIn = listLog.Where(t => t.InOutMode == (short)GCSInOutMode.Input);
            var listExpiredLastTimeLogOut = listLog.Where(t => t.InOutMode == (short)GCSInOutMode.Output)
                .GroupBy(x => x.EmployeeATID).Select(x => new
                {
                    EmployeeATID = x.Key,
                    Time = x.Max(x => x.Time)
                }).ToList();

            var listLogInNotOut = (from c in listExpiredPresenceTimeLogIn
                                   join d in listExpiredLastTimeLogOut
                              on c.EmployeeATID equals d.EmployeeATID into lstTime
                                   from d in lstTime.DefaultIfEmpty()
                                   where d != null ? (c.Time > d.Time) : true
                                   select c).ToList();

            var listWorkingOverTimeLog = listLog.Where(t => t.CompanyIndex == companyIndex
                && t.ApproveStatus == (short)ApproveStatus.Approved && lines.Contains(t.LineIndex)
                 && t.Error != "WorkingOverTime"
                ).ToList();

            var listLogIn = listWorkingOverTimeLog.Where(t => t.InOutMode == (short)GCSInOutMode.Input);
            var listLastTimeLogOut = listWorkingOverTimeLog.Where(t => t.InOutMode == (short)GCSInOutMode.Output)
                .GroupBy(x => x.EmployeeATID).Select(x => new
                {
                    EmployeeATID = x.Key,
                    Time = x.Max(x => x.Time)
                }).ToList();

            var listLogInNotOutOverTime = (from c in listLogIn
                                           join d in listLastTimeLogOut
                                           on c.EmployeeATID equals d.EmployeeATID into lstTime
                                           from d in lstTime.DefaultIfEmpty()
                                           where d != null ? (c.Time > d.Time) : true
                                           select c).ToList();

            var now = DateTime.Now;

            var listGeneralRules = await _GC_Rules_GeneralService.GetRulesGeneralByCompanyIndex(companyIndex);
            if (listGeneralRules.Count() > 0)
            {
                var rule = listGeneralRules.Where(x => x.IsUsing).FirstOrDefault();
                if (rule != null && rule.MaxAttendanceTime > 0 && listLogInNotOutOverTime.Count > 0)
                {
                    foreach (var item in listLogInNotOutOverTime)
                    {
                        if (now.Subtract(item.Time).TotalMinutes > Convert.ToDouble(rule.MaxAttendanceTime * 60))
                        {
                            var logInItem = listLogIn.FirstOrDefault(x => x.Index == item.Index);
                            if (logInItem != null)
                            {
                                logInItem.Error = "WorkingOverTime";
                                logInItem.Status = (short)EMonitoringError.WorkingOverTime;
                            }
                        }
                    }
                }

                if (rule != null && rule.PresenceTrackingTime > 0)
                {
                    if (listLogInNotOut.Count > 0)
                    {
                        foreach (var item in listLogInNotOut)
                        {
                            if (now.Subtract(item.Time).TotalMinutes > Convert.ToDouble(rule.PresenceTrackingTime * 60) && rule.PresenceTrackingTime > 0)
                            {
                                var logInItem = listExpiredPresenceTimeLogIn.FirstOrDefault(x => x.Index == item.Index);
                                if (logInItem != null)
                                {
                                    logInItem.Note = "MissingLogOutAfterExpiredPresenceTime";
                                }
                            }
                        }
                    }

                    var truckDriverInLogs = await db.GC_TruckDriverLog.Where(x
                        => x.CompanyIndex == companyIndex && x.InOutMode == (short)InOutMode.Input && (string.IsNullOrWhiteSpace(x.Note)
                        || (!string.IsNullOrWhiteSpace(x.Note) && x.Note != "MissingLogOutAfterExpiredPresenceTime") 
                        && x.Time < now.AddMinutes(-(rule.PresenceTrackingTime * 60)))).ToListAsync();

                    var truckDriverOutLogs = new List<GC_TruckDriverLog>();
                    var truckDriverInNotOutLogs = new List<GC_TruckDriverLog>();
                    if (truckDriverInLogs.Count > 0)
                    {
                        var truckDriverInTripCodes = truckDriverInLogs.Select(x => x.TripCode).ToList();
                        truckDriverOutLogs = await db.GC_TruckDriverLog.Where(x
                            => x.CompanyIndex == companyIndex && x.InOutMode == (short)InOutMode.Output
                            && truckDriverInTripCodes.Contains(x.TripCode)).ToListAsync();
                        if (truckDriverOutLogs != null && truckDriverOutLogs.Count > 0)
                        {
                            truckDriverInNotOutLogs = truckDriverInLogs.Where(x
                                => !truckDriverOutLogs.Any(y => y.TripCode == x.TripCode)).ToList();
                            //truckDriverInNotOutLogs = (from c in truckDriverInLogs
                            //                           join d in truckDriverOutLogs
                            //                           on c.TripCode equals d.TripCode into lstTime
                            //                           from d in lstTime.DefaultIfEmpty()
                            //                           where d != null ? (c.Time > d.Time) : true
                            //                           select c).ToList();
                        }
                        else
                        {
                            truckDriverInNotOutLogs = truckDriverInLogs;
                        }
                    }

                    var vehicleLogs = await db.IC_VehicleLog.Where(x
                        => x.FromDate.HasValue && !x.ToDate.HasValue
                        && (string.IsNullOrWhiteSpace(x.Note)
                        || (!string.IsNullOrWhiteSpace(x.Note) && x.Note != "MissingLogOutAfterExpiredPresenceTime")
                        && x.FromDate.Value < now.AddMinutes(-(rule.PresenceTrackingTime * 60)))).ToListAsync();

                    if (truckDriverInNotOutLogs.Count > 0)
                    {
                        foreach (var item in truckDriverInNotOutLogs)
                        {
                            if (now.Subtract(item.Time).TotalMinutes > Convert.ToDouble(rule.PresenceTrackingTime * 60) && rule.PresenceTrackingTime > 0)
                            {
                                var logInItem = truckDriverInNotOutLogs.FirstOrDefault(x => x.Index == item.Index);
                                if (logInItem != null)
                                {
                                    logInItem.Note = "MissingLogOutAfterExpiredPresenceTime";
                                }
                            }
                        }
                    }
                    if (vehicleLogs.Count > 0)
                    {
                        foreach (var item in vehicleLogs)
                        {
                            if (now.Subtract(item.FromDate.Value).TotalMinutes > Convert.ToDouble(rule.PresenceTrackingTime * 60) && rule.PresenceTrackingTime > 0)
                            {
                                var logInItem = vehicleLogs.FirstOrDefault(x => x.Index == item.Index);
                                if (logInItem != null)
                                {
                                    logInItem.Note = "MissingLogOutAfterExpiredPresenceTime";
                                }
                            }
                        }
                    }
                }
                //_dbContext.GC_TimeLog.UpdateRange(listLogInNotOutOverTime);
                //_dbContext.GC_TimeLog.UpdateRange(listLogInNotOut);
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {

                }
                await SendSuccessUpdateLogToClientAsync(companyIndex, _Config.CompanyIndex.ToString());
            }
        }

        protected async Task SendSuccessUpdateLogToClientAsync(int companyIndex, string logContent)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            client.BaseAddress = new Uri(_Config.RealTimeServerLink);
            try
            {
                var content = new StringContent(logContent, Encoding.UTF8, "application/json");
                var request = await client.PostAsync("/api/PushAttendanceLog/SendSuccessUpdateLog", content);
                request.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                //_logger.LogError($"SendData: {ex}");
            }
        }

        public List<MonitoringGatesHistoryModel> GetHistoryData(List<long> departmentIndexes, List<string> employeeIndexes, UserInfo user, DateTime FromTime, DateTime ToTime, List<int> rulesWarningIndexes, string statusLog)
        {
            //var rulewarning = _DBContext.GC_Rules_Warning.ToList();
            var rulewarning = DbContext.GC_Rules_Warning.Where(x => rulesWarningIndexes.Contains(x.Index)).Select(x => x.RulesWarningGroupIndex).ToList();
            var ruleWarningGroup = DbContext.GC_Rules_WarningGroup.Where(x => rulewarning.Contains(x.Index)).Select(x => x.Code).ToList();

            var histories = DbContext.GC_TimeLog
              .Where(e => e.CompanyIndex == user.CompanyIndex
              && ((employeeIndexes != null && employeeIndexes.Count > 0) ? employeeIndexes.Contains(e.EmployeeATID) : e.EmployeeATID != null)
              && ((ruleWarningGroup != null && ruleWarningGroup.Count > 0) ? ruleWarningGroup.Contains(e.Error) : e.Error != null)
              && (e.Time >= FromTime && e.Time <= ToTime)).ToList();
            if (!string.IsNullOrEmpty(statusLog))
            {
                if (statusLog == CommandStatus.Success.ToString())
                {
                    histories = histories.Where(x => x.Status == 0 || x.Status == (short)EMonitoringError.NoError).ToList();
                }
                else
                {
                    histories = histories.Where(x => x.Status != 0 && x.Status != (short)EMonitoringError.NoError).ToList();
                }
            }
            var getAllEmployeeATIDInLog = histories.Select(x => x.EmployeeATID).ToList();
            var gateList = DbContext.GC_Gates.ToList();
            var lineList = DbContext.GC_Lines.ToList();
            var gateLine = DbContext.GC_Gates_Lines.ToList();
            var employeeInfoList = _HR_EmployeeInfoService.GetAllEmployeeInfo(getAllEmployeeATIDInLog.ToArray(), user.CompanyIndex).Result;
            var driverList = DbContext.IC_PlanDock.AsNoTracking().Where(x => getAllEmployeeATIDInLog.Contains(x.TripId)).ToList();
            var driverLogList = DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => getAllEmployeeATIDInLog.Contains(x.TripCode)).ToList();
            var extraDriverList = DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x => getAllEmployeeATIDInLog.Contains(x.TripCode)).ToList();
            var driverHistories = ObjectExtensions.CopyToNewObject(histories).Where(x
                => driverList.Any(y => y.TripId == x.EmployeeATID) || extraDriverList.Any(y => y.TripCode == x.EmployeeATID)).ToList();
            if (user != null && user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
            {
                employeeInfoList = employeeInfoList.Where(x => user.ListDepartmentAssigned.Contains(x.DepartmentIndex)).ToList();
                histories = histories.Where(x => employeeInfoList.Any(y => y.EmployeeATID == x.EmployeeATID)).ToList();
            }
            if ((employeeIndexes == null || (employeeIndexes != null && employeeIndexes.Count == 0)) && departmentIndexes != null
                && departmentIndexes.Count > 0)
            {
                employeeInfoList = employeeInfoList.Where(x => departmentIndexes.Contains(x.DepartmentIndex)).ToList();
                histories = histories.Where(x => employeeInfoList.Any(y => y.EmployeeATID == x.EmployeeATID)).ToList();
            }

            if (driverHistories != null && driverHistories.Count > 0)
            {
                histories.AddRange(driverHistories);
            }

            var historyData = new List<MonitoringGatesHistoryModel>();

            foreach (var item in histories)
            {
                var employeeInfo = employeeInfoList.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
                var gateLineInfo = gateLine.FirstOrDefault(x => x.LineIndex == item?.LineIndex);
                var gateInfo = gateList.FirstOrDefault(x => x.Index == gateLineInfo?.GateIndex);
                var lineInfo = lineList.FirstOrDefault(x => x.Index == item.LineIndex);
                var driverInfo = driverList.FirstOrDefault(x => x.TripId == item.EmployeeATID);
                var driverLogInfo = driverLogList.FirstOrDefault(x => x.TripCode == item.EmployeeATID && x.CardNumber == item.CardNumber);
                var extraDriverInfo = extraDriverList.FirstOrDefault(x => x.TripCode == item.EmployeeATID && x.CardNumber == item.CardNumber);

                var data = new MonitoringGatesHistoryModel()
                {
                    EmployeeATID = employeeInfo?.EmployeeATID ?? item.EmployeeATID,
                    EmployeeCode = employeeInfo?.EmployeeCode ?? item.EmployeeATID,
                    CustomerName = employeeInfo?.FullName ?? ((driverLogInfo != null && driverInfo != null) ? driverInfo?.DriverName 
                    : (extraDriverInfo != null ? extraDriverInfo.ExtraDriverName : string.Empty)),
                    DepartmentName = employeeInfo?.DepartmentName ?? "",
                    CheckTime = item.Time,
                    GateIndex = (int)(gateInfo?.Index ?? item.GateIndex),
                    GateName = gateInfo?.Name ?? "",
                    LineIndex = (int)(lineInfo?.Index ?? item.LineIndex),
                    LineName = lineInfo?.Name ?? "",
                    VerifyMode = item.VerifyMode,
                    CardNumber = employeeInfo?.CardNumber ?? item.CardNumber,
                    PhoneNumber = employeeInfo?.Phone,
                    //CustomerImage = employeeInfo?.Avatar,
                    Error = item.Error,
                    Note = item.Note,
                    StatusLog = item.Status == 0 || item.Status == (int)EMonitoringError.NoError ? CommandStatus.Success.ToString() : CommandStatus.Failed.ToString()
                };
                if (item.InOutMode == (short)DeviceStatus.Input)
                {
                    data.InOutMode = InOutMode.Input.ToString();
                }
                else
                {
                    data.InOutMode = InOutMode.Output.ToString();
                }
                historyData.Add(data);
            }
            return historyData;
        }
    }

}
