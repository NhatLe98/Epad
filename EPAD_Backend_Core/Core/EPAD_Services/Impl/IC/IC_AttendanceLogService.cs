using DocumentFormat.OpenXml.Wordprocessing;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.TimeLog;
using EPAD_Logic;
using EPAD_Logic.SendMail;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using EPAD_Common.Enums;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using EPAD_Data.Sync_Entities;

namespace EPAD_Services.Impl
{
    public class IC_AttendanceLogService : BaseServices<IC_AttendanceLog, EPAD_Context>, IIC_AttendanceLogService
    {
        private double? defaultBodyTemperature;
        private ILogger _logger;
        private readonly ILoggerFactory _LoggerFactory;
        private readonly IIC_EmployeeService _IC_EmployeeService;
        private readonly IIC_EmployeeLogic _IC_EmployeeLogic;
        protected readonly IHttpClientFactory _ClientFactory;
        private readonly IIC_ConfigService _IC_ConfigService;
        private readonly IEmailProvider _emailProvider;
        private readonly IHR_UserService _HR_UserService;
        private readonly IHR_CustomerCardService _HR_CustomerCardService;
        private readonly IIC_DepartmentService _IC_DepartmentService;
        private readonly EPAD_Context _context;
        private readonly ezHR_Context _otherContext;
        private IConfiguration _Configuration;
        private string _configClientName;
        private readonly string _isUsingGroupDeviceName;
        private readonly IIC_ScheduleAutoHostedLogic _IC_ScheduleAutoHostedLogic;
        private ConfigObject _config;

        public IC_AttendanceLogService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory,
            IConfiguration configuration) : base(serviceProvider)
        {
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _logger = loggerFactory.CreateLogger<IC_DeviceService>();
            _IC_EmployeeService = serviceProvider.GetService<IIC_EmployeeService>();
            _IC_EmployeeLogic = serviceProvider.GetService<IIC_EmployeeLogic>();
            var configBodyTemperature = DbContext.IC_Config.FirstOrDefault(e => e.EventType == ConfigAuto.GENERAL_SYSTEM_CONFIG.ToString());
            if (configBodyTemperature == null || !configBodyTemperature.BodyTemperature.HasValue)
            {
                defaultBodyTemperature = 37.5;
            }
            else
            {
                defaultBodyTemperature = configBodyTemperature.BodyTemperature;
            }
            _ClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            _IC_ConfigService = serviceProvider.GetService<IIC_ConfigService>();
            _emailProvider = serviceProvider.GetService<IEmailProvider>();
            _context = serviceProvider.GetService<EPAD_Context>();
            _Configuration = configuration;
            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
            _HR_CustomerCardService = serviceProvider.GetService<IHR_CustomerCardService>();
            _IC_DepartmentService = serviceProvider.GetService<IIC_DepartmentService>();
            _configClientName = _Configuration.GetValue<string>("ClientName").ToUpper();
            _isUsingGroupDeviceName = _Configuration.GetValue<string>("IsUsingGroupDeviceName");
            _IC_ScheduleAutoHostedLogic = serviceProvider.GetService<IIC_ScheduleAutoHostedLogic>();
            _config = ConfigObject.GetConfig(_Cache);
            _otherContext = serviceProvider.GetService<ezHR_Context>();
        }

        public object GetLastedRealtimeAttendanceLog(ConfigObject config, UserInfo user)
        {
            IQueryable<IC_AttendanceLog> listLog;
            var privilegeMachineRealtime = _context.IC_PrivilegeMachineRealtime.FirstOrDefault(x => x.UserName == user.UserName);
            if (privilegeMachineRealtime != null)
            {
                var listPrivilegeDeviceSerial = privilegeMachineRealtime.DeviceSerial.Split(":/:").ToList();
                listLog = _context.IC_AttendanceLog.AsNoTracking().Where(t => t.CompanyIndex == user.CompanyIndex
                    && listPrivilegeDeviceSerial.Contains(t.SerialNumber))
                .OrderByDescending(t => t.CheckTime).Take(GlobalParams.ROWS_NUMBER_IN_REALTIME_PAGE);
            }
            else
            {
                listLog = _context.IC_AttendanceLog.AsNoTracking().Where(t => t.CompanyIndex == user.CompanyIndex)
                    .OrderByDescending(t => t.CheckTime).Take(GlobalParams.ROWS_NUMBER_IN_REALTIME_PAGE);
            }

            var lstEmployeeLookup = _IC_EmployeeLogic.GetListEmployeeLookup(config, user);
            var employeeLookup = lstEmployeeLookup.ToDictionarySafe(e => e.EmployeeATID);

            var listDataLog = (from log in listLog
                               join emp in _context.HR_User
                               on log.EmployeeATID equals emp.EmployeeATID into temp
                               from dummy in temp.DefaultIfEmpty()
                               join dev in _context.IC_Device
                               on log.SerialNumber equals dev.SerialNumber into devlog
                               from dl in devlog.DefaultIfEmpty()
                               select new
                               {
                                   EmployeeATID = log.EmployeeATID,
                                   FullName = dummy == null ? log.EmployeeATID : dummy.FullName,
                                   SerialNumber = log.SerialNumber,
                                   DeviceName = dl != null ? dl.AliasName : string.Empty,
                                   DeviceNumber = log.DeviceNumber,
                                   CheckTime = log.CheckTime,
                                   VerifyModeValue = log.VerifyMode,
                                   InOutModeValue = log.InOutMode,
                                   FaceMaskValue = log.FaceMask,
                                   BodyTemperatureValue = log.BodyTemperature,
                                   IsOverBodyTemperatureValue = log.BodyTemperature,
                                   VerifyMode = StringHelper.GetVerifyModeString(log.VerifyMode),
                                   InOutMode = StringHelper.GetInOutModeString(log.InOutMode),
                                   FaceMask = StringHelper.GetFaceMaskString(log.FaceMask),
                                   BodyTemperature = StringHelper.GetBodyTemperatureString(log.BodyTemperature),
                                   IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(log.BodyTemperature, defaultBodyTemperature.Value),
                                   Department = employeeLookup.ContainsKey(log.EmployeeATID) ? employeeLookup[log.EmployeeATID].Department : string.Empty,
                                   DepartmentIndex = employeeLookup.ContainsKey(log.EmployeeATID) ? employeeLookup[log.EmployeeATID].DepartmentIndex : 0,
                               }).ToList();

            var result = listDataLog.ToList();

            result = result.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Contains(x.DepartmentIndex ?? 0))
                || x.DepartmentIndex.HasValue == false).ToList();

            return listDataLog;
        }

        public List<IC_AttendanceLog> GetAttendanceLogByEmployeeATIDsAndDateTime(List<string> employeeATIDs, DateTime dateTime, UserInfo user)
        {
            return _context.IC_AttendanceLog
                    .Where(x => x.CompanyIndex == user.CompanyIndex && ((employeeATIDs != null && employeeATIDs.Count() > 0) ? employeeATIDs.Contains(x.EmployeeATID) : true)
                    && x.CheckTime.Date == dateTime.Date).ToList();
        }

        public List<IC_AttendanceLog> GetAttendanceLogByEmployeeATIDsAndTime(List<string> employeeATIDs, DateTime dateTime, UserInfo user, List<GetAttendanceLogByTime> contractorTime, List<GetAttendanceLogByTime> securityTime)
        {
            var log = _context.IC_AttendanceLog
                    .AsNoTracking()
                    .Where(x => x.CompanyIndex == user.CompanyIndex && ((employeeATIDs != null && employeeATIDs.Count() > 0) ? employeeATIDs.Contains(x.EmployeeATID) : true)
                    && x.CheckTime.Date == dateTime.Date).ToList();

            if (contractorTime != null && contractorTime.Count > 0)
            {
                var departments = _IC_DepartmentService.GetDepartmentNotIntegrate().Result;
                var departmentsLst = departments.ConvertAll<string>(x => x.ToString());
                var contractors = _HR_UserService.GetEmployeeByDepartmentIds(departmentsLst, user.CompanyIndex).Result;

                var ids = contractors.Select(x => x.EmployeeATID).ToHashSet();
                var logContractor = log.Where(x => ids.Contains(x.EmployeeATID)).ToList();
                log = log.Where(x => !ids.Contains(x.EmployeeATID)).ToList();
                var logFinal = new List<IC_AttendanceLog>();
                foreach (var item in contractorTime)
                {
                    var fromTime = item.FromTime.TryGetDateTime();
                    var toTime = item.ToTime.TryGetDateTime();

                    var logGet = logContractor.Where(x => x.CheckTime >= fromTime && x.CheckTime <= toTime).ToList();
                    logFinal.AddRange(logGet);
                }

                logFinal = logFinal.GroupBy(x => x.Index).Select(x => x.First()).ToList();
                log.AddRange(logFinal);

            }

            if (securityTime != null && securityTime.Count > 0)
            {
                var secudepartments = _IC_DepartmentService.GetDepartmentSecurity().Result;
                var secusLst = secudepartments.ConvertAll<string>(x => x.ToString());
                var security = _HR_UserService.GetEmployeeByDepartmentIds(secusLst, user.CompanyIndex).Result;

                var ids = security.Select(x => x.EmployeeATID).ToHashSet();
                var logSecu = log.Where(x => ids.Contains(x.EmployeeATID)).ToList();
                log = log.Where(x => !ids.Contains(x.EmployeeATID)).ToList();
                var logFinal = new List<IC_AttendanceLog>();
                foreach (var item in securityTime)
                {
                    var fromTime = item.FromTime.TryGetDateTime();
                    var toTime = item.ToTime.TryGetDateTime();

                    var logGet = logSecu.Where(x => x.CheckTime >= fromTime && x.CheckTime <= toTime).ToList();
                    logFinal.AddRange(logGet);
                }

                logFinal = logFinal.GroupBy(x => x.Index).Select(x => x.First()).ToList();
                log.AddRange(logFinal);
            }

            return log;

        }

        public void AddAttendanceLog(PostAttendanceLog req, ref TimeLogRequest timeLogRequest, ref List<OverBodyTemparatureEmployeesList> obtEmployeeList, UserInfo user)
        {
            var device = DbContext.IC_Device.FirstOrDefault(x => x.SerialNumber.ToLower() == req.SerialNumber.ToLower());

            foreach (var item in req.ListAttendanceLog)
            {
                try
                {
                    var attendanceLog = new IC_AttendanceLog();
                    attendanceLog.EmployeeATID = item.UserId.GetNormalATID();
                    attendanceLog.SerialNumber = req.SerialNumber;
                    attendanceLog.DeviceId = device.DeviceId;
                    attendanceLog.CompanyIndex = user.CompanyIndex;
                    attendanceLog.CheckTime = item.Time;
                    attendanceLog.VerifyMode = Convert.ToInt16(item.VerifiedMode);
                    attendanceLog.InOutMode = Convert.ToInt16(item.InOutMode);
                    attendanceLog.WorkCode = 1;
                    attendanceLog.Reserve1 = 0;
                    attendanceLog.FaceMask = item.FaceMask;
                    attendanceLog.BodyTemperature = item.BodyTemperature;
                    attendanceLog.Function = "";
                    attendanceLog.UpdatedDate = DateTime.Now;
                    attendanceLog.UpdatedUser = user.UserName;

                    //check over body temp
                    if (StringHelper.GetIsOverBodyTemperature(attendanceLog.BodyTemperature, defaultBodyTemperature.Value))
                    {
                        obtEmployeeList.Add(new OverBodyTemparatureEmployeesList()
                        {
                            EmployeeATID = attendanceLog.EmployeeATID,
                            TimeLog = attendanceLog.CheckTime.ToddMMyyyyHHmmss(),
                            BodyTemparature = attendanceLog.BodyTemperature.ToString()
                        });
                    }

                    try
                    {
                        DbContext.IC_AttendanceLog.Add(attendanceLog);

                    }
                    catch
                    {
                        DbContext.IC_AttendanceLog.Remove(attendanceLog);
                    }
                    //_DbContext.IC_AttendanceLogData.Add(attendanceLog);
                    timeLogRequest.TimeLogs.Add(CreateTimeLogForRequest(attendanceLog));
                    //await SendTimeLogToAPIAsync(cfg, timeLogRequest);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"AddAttendanceLog: " + ex.Message);
                }
            }
            DbContext.SaveChanges();
        }

        public async Task<DataGridClass> GetDataGrid(string pFilter, DateTime fromDate, DateTime toDate, List<string> pEmpATIDs, int pCompanyIndex, int pPageIndex, int pPageSize, UserInfo user)
        {
            if (!_config.IntegrateDBOther)
            {
                var obj = from att in DbContext.IC_AttendanceLog.Where(x => x.CompanyIndex == pCompanyIndex)
                          join dv in DbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex)
                            on att.SerialNumber equals dv.SerialNumber
                          join emp in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                            on att.EmployeeATID equals emp.EmployeeATID

                          join wi in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on att.EmployeeATID equals wi.EmployeeATID into eWork

                          from eWorkResult in eWork.DefaultIfEmpty()

                          join dp in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                            on eWorkResult.DepartmentIndex equals dp.Index into temp
                          from dummy in temp.DefaultIfEmpty()
                              //join dp in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                              //  on emp.DepartmentIndex equals dp.Index into temp
                              //from dummy in temp.DefaultIfEmpty()
                          where (att.CheckTime >= fromDate && att.CheckTime <= toDate)
                          && (string.IsNullOrEmpty(pFilter)
                                ? emp.EmployeeATID.Contains("")
                                : (
                                       att.EmployeeATID.Contains(pFilter)
                                    || (!string.IsNullOrEmpty(emp.EmployeeCode) && emp.EmployeeCode.Contains(pFilter))
                                    || emp.FullName.Contains(pFilter)
                                    //|| dummy.Name.Contains(pFilter)
                                    || att.SerialNumber.Contains(pFilter)
                                ))
                          && ((pEmpATIDs != null && pEmpATIDs.Count > 0) ? pEmpATIDs.Any(y => y.Equals(att.EmployeeATID)) : emp.EmployeeATID.Contains(""))
                          select new AttendanceLogInfoResult()
                          {
                              EmployeeATID = att.EmployeeATID,
                              EmployeeCode = emp.EmployeeCode,
                              FullName = emp.FullName,
                              Time = att.CheckTime,
                              SerialNumber = att.SerialNumber,
                              DeviceNumber = att.DeviceNumber,
                              AliasName = dv.AliasName,
                              DepartmentName = dummy == null ? "" : dummy.Name,
                              InOutMode = att.InOutMode.GetInOutModeString() == "In" ? "Vào" : "Ra",
                              VerifyMode = att.VerifyMode.GetVerifyModeString(),
                              FaceMask = att.FaceMask.GetFaceMaskString(),
                              BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                              IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                          };

                if (pPageIndex <= 1)
                {
                    pPageIndex = 1;
                }

                int fromRow = pPageSize * (pPageIndex - 1);
                var lsAttendanceLog = obj.OrderBy(t => t.Time).Skip(fromRow).Take(pPageSize).ToList();
                var dataGrid = new DataGridClass(obj.Count(), lsAttendanceLog);
                return await Task.FromResult(dataGrid);
            }
            else
            {
                var userDepartmentAssignedList = user.ListDepartmentAssigned.ToHashSet();
                var obj = (from att in DbContext.IC_AttendanceLog.Where(x => x.CompanyIndex == pCompanyIndex)
                           join dv in DbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex)
                             on att.SerialNumber equals dv.SerialNumber
                           where (att.CheckTime >= fromDate && att.CheckTime <= toDate)
                           && (string.IsNullOrEmpty(pFilter)
                                 ? att.EmployeeATID.Contains("")
                                 : (
                                        att.EmployeeATID.Contains(pFilter)
                                     || att.SerialNumber.Contains(pFilter)
                                 ))
                           && ((pEmpATIDs != null && pEmpATIDs.Count > 0) ? pEmpATIDs.Any(y => y.Equals(att.EmployeeATID)) : att.EmployeeATID.Contains(""))
                           select new AttendanceLogInfoResult()
                           {
                               EmployeeATID = att.EmployeeATID,
                               Time = att.CheckTime,
                               SerialNumber = att.SerialNumber,
                               DeviceNumber = att.DeviceNumber,
                               AliasName = dv.AliasName,
                               //DepartmentName = dummy == null ? "" : dummy.Name,
                               InOutMode = att.InOutMode.GetInOutModeString() == "In" ? "Vào" : "Ra",
                               VerifyMode = att.VerifyMode.GetVerifyModeString(),
                               FaceMask = att.FaceMask.GetFaceMaskString(),
                               BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                               IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value)
                           }).ToList();


                obj = (from att in obj
                       join e in _otherContext.HR_Employee.Where(x => x.CompanyIndex == _config.CompanyIndex)
                       on att.EmployeeATID equals e.EmployeeATID into eEmployee
                       from eEmployeeResult in eEmployee.DefaultIfEmpty()
                       join wi in _otherContext.HR_WorkingInfo.Where(x => x.CompanyIndex == _config.CompanyIndex)
                       on eEmployeeResult.EmployeeATID equals wi.EmployeeATID into eWork
                       from eWorkResult in eWork.DefaultIfEmpty()
                       join d in _otherContext.HR_Department.Where(x => x.CompanyIndex == _config.CompanyIndex)
                       on eWorkResult.DepartmentIndex ?? 0 equals d.Index into dWork
                       from dWorkResult in dWork.DefaultIfEmpty()

                       where
                             eEmployeeResult != null &&
                            (string.IsNullOrEmpty(pFilter)
                                 ? att.EmployeeATID.Contains("")
                                 : (
                                        att.EmployeeATID.Contains(pFilter)
                                     || (!string.IsNullOrEmpty(eEmployeeResult.EmployeeCode) && eEmployeeResult.EmployeeCode.Contains(pFilter))
                                     //|| dummy.Name.Contains(pFilter)
                                     || att.SerialNumber.Contains(pFilter)
                                 ))
                           && ((pEmpATIDs != null && pEmpATIDs.Count > 0) ? pEmpATIDs.Any(y => y.Equals(att.EmployeeATID)) : eEmployeeResult.EmployeeATID.Contains(""))
                        && (dWorkResult == null || userDepartmentAssignedList.Contains(dWorkResult.Index))
                       select new AttendanceLogInfoResult()
                       {
                           EmployeeATID = att.EmployeeATID,
                           EmployeeCode = eEmployeeResult.EmployeeCode,
                           FullName = $"{eEmployeeResult.LastName} {eEmployeeResult.MidName} {eEmployeeResult.FirstName}",
                           Time = att.Time,
                           SerialNumber = att.SerialNumber,
                           DeviceNumber = att.DeviceNumber,
                           AliasName = att.AliasName,
                           DepartmentName = dWorkResult == null ? "" : dWorkResult.Name,
                           InOutMode = att.InOutMode,
                           VerifyMode = att.VerifyMode,
                           FaceMask = att.FaceMask,
                           BodyTemperature = att.BodyTemperature,
                           IsOverBodyTemperature = att.IsOverBodyTemperature,
                       }).ToList();

                if (pPageIndex <= 1)
                {
                    pPageIndex = 1;
                }

                int fromRow = pPageSize * (pPageIndex - 1);
                var lsAttendanceLog = obj.OrderBy(t => t.Time).Skip(fromRow).Take(pPageSize).ToList();


                var dataGrid = new DataGridClass(obj.Count(), lsAttendanceLog);
                return await Task.FromResult(dataGrid);
            }
        }

        public async Task<List<AttendanceLogInfoResult>> GetMany(List<AddedParam> addedParams)
        {
            if (addedParams == null || addedParams.Count == 0)
                return null;
            var paramCompanyIndex = addedParams.FirstOrDefault(e => e.Key == "CompanyIndex");
            if (paramCompanyIndex == null)
                return null;

            var paramFromDate = addedParams.FirstOrDefault(e => e.Key == "FromDate");
            var fromDate = (DateTime?)null;

            if (paramFromDate != null)
            {
                fromDate = Convert.ToDateTime(paramFromDate.Value);
            }

            var paramToDate = addedParams.FirstOrDefault(e => e.Key == "ToDate");
            var toDate = (DateTime?)null;

            if (paramToDate != null)
            {
                toDate = Convert.ToDateTime(paramToDate.Value);
            }


            var companyIndex = Convert.ToInt32(paramCompanyIndex.Value);
            if (!_config.IntegrateDBOther)
            {
                var query = (from att in DbContext.IC_AttendanceLog.Where(x => x.CompanyIndex == companyIndex)
                             join dv in DbContext.IC_Device.Where(x => x.CompanyIndex == companyIndex)
                               on att.SerialNumber equals dv.SerialNumber
                             join emp in DbContext.HR_User.Where(x => x.CompanyIndex == companyIndex)
                               on att.EmployeeATID equals emp.EmployeeATID
                             join wi in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == companyIndex)
                                    on att.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()
                             join dp in DbContext.IC_Department.Where(x => x.CompanyIndex == companyIndex)
                               on eWorkResult.DepartmentIndex equals dp.Index into temp
                             from dummy in temp.DefaultIfEmpty()

                             select new AttendanceLogInfoResult()
                             {
                                 CompanyIndex = att.CompanyIndex,
                                 EmployeeATID = att.EmployeeATID,
                                 EmployeeCode = emp.EmployeeCode,
                                 FullName = emp.FullName,
                                 Time = att.CheckTime,
                                 SerialNumber = att.SerialNumber,
                                 DeviceNumber = att.DeviceNumber,
                                 AliasName = dv.AliasName,
                                 DepartmentName = dummy == null ? "" : dummy.Name,
                                 InOutMode = att.InOutMode.GetInOutModeString() == "In" ? "Vào" : "Ra",
                                 VerifyMode = att.VerifyMode.GetVerifyModeString(),
                                 FaceMask = att.FaceMask.GetFaceMaskString(),
                                 BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                 IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value)
                             }).AsEnumerable();
                if (addedParams != null)
                {
                    foreach (AddedParam param in addedParams)
                    {
                        switch (param.Key)
                        {
                            case "Filter":
                                if (param.Value != null)
                                {
                                    string filter = param.Value.ToString();
                                    query = query.Where(u => u.SerialNumber.Contains(filter)
                                    || u.AliasName.Contains(filter)
                                    || u.DepartmentName.Contains(filter)
                                    || (!string.IsNullOrEmpty(u.EmployeeCode) && u.EmployeeCode.Contains(filter))
                                    || u.FullName.Contains(filter)
                                    || u.FaceMask.Contains(filter)
                                    || u.InOutMode.Contains(filter)
                                    || u.EmployeeATID.Contains(filter)
                                    || u.VerifyMode.Contains(filter)
                                    || u.BodyTemperature.Contains(filter));
                                }
                                break;
                            case "CompanyIndex":
                                if (param.Value != null)
                                {
                                    int comIndex = Convert.ToInt32(param.Value);
                                    query = query.Where(u => u.CompanyIndex == comIndex);
                                }
                                break;
                            case "FromDate":
                                if (param.Value != null)
                                {
                                    var fromdate = Convert.ToDateTime(param.Value.ToString());
                                    query = query.Where(u => u.Time.Date >= fromdate.Date);
                                }
                                break;
                            case "ToDate":
                                if (param.Value != null)
                                {
                                    var todate = Convert.ToDateTime(param.Value.ToString());
                                    query = query.Where(u => u.Time.Date <= todate.Date);
                                }
                                break;
                            case "ListEmployeeATID":
                                if (param.Value != null)
                                {
                                    var listEmployeeID = JsonConvert.DeserializeObject<List<string>>(param.Value.ToString());
                                    //IList<string> listEmployeeID = (IList<string>)param.Value;
                                    if (listEmployeeID.Count > 0)
                                    {
                                        query = query.Where(u => listEmployeeID.Contains(u.EmployeeATID));
                                    }

                                }
                                break;
                            case "EmployeeATID":
                                if (param.Value != null)
                                {
                                    string employeeID = param.Value.ToString();
                                    query = query.Where(u => u.EmployeeATID == employeeID);
                                }
                                break;
                        }
                    }

                }
                var data = query.AsEnumerable().OrderByDescending(x => x.Time).ToList();

                return data;
            }
            else
            {
                var query = (from att in DbContext.IC_AttendanceLog.Where(x => x.CompanyIndex == companyIndex)
                             join dv in DbContext.IC_Device.Where(x => x.CompanyIndex == companyIndex)
                               on att.SerialNumber equals dv.SerialNumber
                             where (att.CheckTime >= fromDate && att.CheckTime <= toDate)
                             select new AttendanceLogInfoResult()
                             {
                                 EmployeeATID = att.EmployeeATID,
                                 Time = att.CheckTime,
                                 SerialNumber = att.SerialNumber,
                                 DeviceNumber = att.DeviceNumber,
                                 AliasName = dv.AliasName,
                                 //DepartmentName = dummy == null ? "" : dummy.Name,
                                 InOutMode = att.InOutMode.GetInOutModeString() == "In" ? "Vào" : "Ra",
                                 VerifyMode = att.VerifyMode.GetVerifyModeString(),
                                 FaceMask = att.FaceMask.GetFaceMaskString(),
                                 BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                 IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value)
                             }).ToList();


                query = (from att in query
                         join e in _otherContext.HR_Employee.Where(x => x.CompanyIndex == _config.CompanyIndex)
                       on att.EmployeeATID equals e.EmployeeATID into eEmployee
                         from eEmployeeResult in eEmployee.DefaultIfEmpty()
                         join wi in _otherContext.HR_WorkingInfo.Where(x => x.CompanyIndex == _config.CompanyIndex)
                         on eEmployeeResult.EmployeeATID equals wi.EmployeeATID into eWork
                         from eWorkResult in eWork.DefaultIfEmpty()
                         join d in _otherContext.HR_Department.Where(x => x.CompanyIndex == _config.CompanyIndex)
                         on eWorkResult.DepartmentIndex ?? 0 equals d.Index into dWork
                         from dWorkResult in dWork.DefaultIfEmpty()
                         select new AttendanceLogInfoResult()
                         {
                             EmployeeATID = att.EmployeeATID,
                             EmployeeCode = eEmployeeResult.EmployeeCode,
                             FullName = $"{eEmployeeResult.LastName} {eEmployeeResult.MidName} {eEmployeeResult.FirstName}",
                             Time = att.Time,
                             SerialNumber = att.SerialNumber,
                             DeviceNumber = att.DeviceNumber,
                             AliasName = att.AliasName,
                             DepartmentName = dWorkResult == null ? "" : dWorkResult.Name,
                             InOutMode = att.InOutMode,
                             VerifyMode = att.VerifyMode,
                             FaceMask = att.FaceMask,
                             BodyTemperature = att.BodyTemperature,
                             IsOverBodyTemperature = att.IsOverBodyTemperature,
                         }).ToList();

                if (addedParams != null)
                {
                    foreach (AddedParam param in addedParams)
                    {
                        switch (param.Key)
                        {
                            case "Filter":
                                if (param.Value != null)
                                {
                                    string filter = param.Value.ToString();
                                    query = query.Where(u => u.SerialNumber.Contains(filter)
                                    || u.AliasName.Contains(filter)
                                    || u.DepartmentName.Contains(filter)
                                    || (!string.IsNullOrEmpty(u.EmployeeCode) && u.EmployeeCode.Contains(filter))
                                    || u.FullName.Contains(filter)
                                    || u.FaceMask.Contains(filter)
                                    || u.InOutMode.Contains(filter)
                                    || u.EmployeeATID.Contains(filter)
                                    || u.VerifyMode.Contains(filter)
                                    || u.BodyTemperature.Contains(filter)).ToList();
                                }
                                break;
                            case "FromDate":
                                if (param.Value != null)
                                {
                                    var fromdate = Convert.ToDateTime(param.Value.ToString());
                                    query = query.Where(u => u.Time.Date >= fromdate.Date).ToList();
                                }
                                break;
                            case "ToDate":
                                if (param.Value != null)
                                {
                                    var todate = Convert.ToDateTime(param.Value.ToString());
                                    query = query.Where(u => u.Time.Date <= todate.Date).ToList();
                                }
                                break;
                            case "ListEmployeeATID":
                                if (param.Value != null)
                                {
                                    IList<string> listEmployeeID = (IList<string>)param.Value;
                                    query = query.Where(u => listEmployeeID.Contains(u.EmployeeATID)).ToList();
                                }
                                break;
                            case "EmployeeATID":
                                if (param.Value != null)
                                {
                                    string employeeID = param.Value.ToString();
                                    query = query.Where(u => u.EmployeeATID == employeeID).ToList();
                                }
                                break;
                        }
                    }

                }
                var data = query.AsEnumerable().OrderByDescending(x => x.Time).ToList();

                return data;

            }
        }

        public TimeLog CreateTimeLogForRequest(IC_AttendanceLog attendanceLog)
        {
            var tl = new TimeLog();
            tl.EmployeeATID = attendanceLog.EmployeeATID;
            tl.Time = attendanceLog.CheckTime;
            tl.MachineSerial = attendanceLog.SerialNumber;
            tl.DeviceNumber = attendanceLog.DeviceNumber;
            tl.DeviceId = attendanceLog.DeviceId;
            tl.InOutMode = attendanceLog.InOutMode;
            tl.SpecifiedMode = attendanceLog.VerifyMode.Value;
            tl.Action = "EPAD";

            return tl;
        }

        public async Task SendTimeLogToAPIAsync(IC_Config cfg, TimeLogRequest timeLogRequest)
        {
            try
            {
                if (cfg == null) return;

                // Custome for toyota , use EmployeeCode
                if (_configClientName == ClientName.TOYOTA.ToString())
                {
                    if (timeLogRequest.TimeLogs.Count > 0)
                    {
                        for (int i = 0; i < timeLogRequest.TimeLogs.Count; i++)
                        {
                            var employee = _IC_EmployeeService.FirstOrDefault(x => x.EmployeeATID == timeLogRequest.TimeLogs[0].EmployeeATID);
                            timeLogRequest.TimeLogs[0].EmployeeATID = timeLogRequest.TimeLogs[0].EmployeeCode = employee != null
                                ? employee.EmployeeCode : timeLogRequest.TimeLogs[0].EmployeeATID;
                        }
                    }
                }
                var employees = timeLogRequest.TimeLogs.Select(x => x.EmployeeATID).Distinct().ToList();

                var listEmployee = DbContext.HR_User.Where(x => employees.Contains(x.EmployeeATID)).ToList();
                foreach (var item in timeLogRequest.TimeLogs)
                {
                    var employee = listEmployee.FirstOrDefault(x => listEmployee.Any(e => x.EmployeeATID == e.EmployeeATID));
                    item.EmployeeCode = employee?.EmployeeCode;
                }
                string[] cfgSplit = cfg.ProceedAfterEvent.Split('|');
                if (cfgSplit.Length < 1) return;
                if (cfgSplit.Length > 0 && bool.Parse(cfgSplit[0]) == false) return;

                string apiLink = cfgSplit[1].Trim();
                apiLink = apiLink + (apiLink.EndsWith("/") ? "" : "/") + "api/TA_TimeLog/AddTimeLog";
                if (timeLogRequest.TimeLogs.Count > 5000)
                {
                    var listSplitEmployeeID = CommonUtils.SplitList(timeLogRequest.TimeLogs, 5000);
                    foreach (var item in listSplitEmployeeID)
                    {
                        var jsonData = JsonConvert.SerializeObject(listSplitEmployeeID);
                        var request = new HttpRequestMessage(HttpMethod.Post, apiLink);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                        var client = _ClientFactory.CreateClient();

                        var response = await client.SendAsync(request);
                    }
                }
                else
                {
                    var jsonData = JsonConvert.SerializeObject(timeLogRequest);
                    var request = new HttpRequestMessage(HttpMethod.Post, apiLink);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    var client = _ClientFactory.CreateClient();

                    var response = await client.SendAsync(request);
                }


                //var mongoObject = new MongoDBHelper<TimeLogRequestLog>("timelog_request", _Cache);
                //mongoObject.AddDataToCollection(new TimeLogRequestLog { TimeLogReq = timeLogRequest, StatusCode = response.StatusCode, Content = response.ReasonPhrase }, true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }

        public async Task AddAttendanceLogByDevice(PostAttendanceLog req, UserInfo user)
        {
            var timeLogRequest = new TimeLogRequest();
            var cfg = await _IC_ConfigService.GetConfigByEventType(ConfigAuto.INTEGRATE_LOG_REALTIME.ToString(), user.CompanyIndex);
            var obtEmployeeList = new List<OverBodyTemparatureEmployeesList>();
            _logger.LogError($"req {req.ListAttendanceLog.Count}");
            var device = DbContext.IC_Device.FirstOrDefault(x => x.CompanyIndex == user.CompanyIndex && x.SerialNumber == req.SerialNumber);

            if (req.ListAttendanceLog != null && req.ListAttendanceLog.Count > 0)
            {
                var DateList = req.ListAttendanceLog.Select(x => x.Time).ToList();
                var fromDate = DateList.Min().Date;
                var toDate = DateList.Max().Date.AddDays(1);

                var checkExistLst = DbContext.IC_AttendanceLog.AsNoTracking().Where(x => x.CheckTime >= fromDate && x.CheckTime <= toDate && x.SerialNumber == req.SerialNumber).ToList();
                req.ListAttendanceLog = req.ListAttendanceLog.Where(x => !checkExistLst.Any(e => e.EmployeeATID == x.UserId.GetNormalATID()
                                                                    && e.CheckTime == x.Time
                                                                    && e.SerialNumber == req.SerialNumber
                                                                   )).ToList();
            }

            var function = "";

            if (!string.IsNullOrEmpty(_isUsingGroupDeviceName) && Convert.ToBoolean(_isUsingGroupDeviceName))
            {
                var group = GetDeviceGroupName(user.CompanyIndex, req.SerialNumber);
                if (group != null)
                {
                    function = group.GroupDeviceName;
                }
            }


            if (req.ListAttendanceLog != null)
            {
                foreach (var item in req.ListAttendanceLog)
                {
                    try
                    {
                        IC_AttendanceLog attendanceLog = new IC_AttendanceLog();
                        attendanceLog.EmployeeATID = item.UserId.GetNormalATID();
                        attendanceLog.SerialNumber = req.SerialNumber;
                        attendanceLog.CompanyIndex = user.CompanyIndex;
                        attendanceLog.CheckTime = item.Time;
                        attendanceLog.VerifyMode = Convert.ToInt16(item.VerifiedMode);
                        attendanceLog.InOutMode = device != null && device.DeviceStatus != null && device.DeviceStatus != 0 ? Convert.ToInt16(device.DeviceStatus.Value - 1) : Convert.ToInt16(item.InOutMode);
                        attendanceLog.WorkCode = 1;
                        attendanceLog.Reserve1 = 0;
                        attendanceLog.FaceMask = item.FaceMask;
                        attendanceLog.BodyTemperature = item.BodyTemperature;
                        attendanceLog.Function = function;
                        attendanceLog.UpdatedDate = DateTime.Now;
                        attendanceLog.UpdatedUser = user.UserName;

                        try
                        {
                            DbContext.IC_AttendanceLog.Add(attendanceLog);

                        }
                        catch
                        {
                            DbContext.IC_AttendanceLog.Remove(attendanceLog);
                        }

                        DbContext.SaveChanges();

                        timeLogRequest.TimeLogs.Add(CreateTimeLogForRequest(attendanceLog));
                        await SendTimeLogToAPIAsync(cfg, timeLogRequest);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"AddAttendanceLog: " + ex.Message);
                    }
                }
            }
            //AddAttendanceLog(req, ref timeLogRequest, ref obtEmployeeList, user);
            //await SendTimeLogToAPIAsync(cfg, timeLogRequest);
            // send email when exists over body temparature employees
            //SendMailWhenHaveEmployeeOverTemp(obtEmployeeList);
        }
        public IC_DeviceGroupDTO GetDeviceGroupName(int mCompanyIndex, string serialNumber)
        {
            var groupDevice = (from i in DbContext.IC_GroupDeviceDetails.Where(x => x.CompanyIndex == mCompanyIndex && x.SerialNumber == serialNumber)
                               join a in DbContext.IC_GroupDevice.Where(x => x.CompanyIndex == mCompanyIndex)
                               on i.GroupDeviceIndex equals a.Index
                               select new IC_DeviceGroupDTO
                               {
                                   GroupDeviceName = a.Name,
                                   SerialNumber = i.SerialNumber
                               }).FirstOrDefault();
            return groupDevice;
        }

        public async Task AddAttendanceLogByDeviceFR05M(PostAttendanceLog req)
        {
            var timeLogRequest = new TimeLogRequest();
            var cfg = await _IC_ConfigService.GetConfigByEventType(ConfigAuto.INTEGRATE_LOG_REALTIME.ToString(), 2);
            var obtEmployeeList = new List<OverBodyTemparatureEmployeesList>();
            _logger.LogError($"req {req.ListAttendanceLog.Count}");

            if (req.ListAttendanceLog != null && req.ListAttendanceLog.Count > 0)
            {
                var DateList = req.ListAttendanceLog.Select(x => x.Time).ToList();
                var fromDate = DateList.Min().Date;
                var toDate = DateList.Max().Date.AddDays(1);

                var checkExistLst = DbContext.IC_AttendanceLog.AsNoTracking().Where(x => x.CheckTime >= fromDate && x.CheckTime <= toDate).ToList();
                req.ListAttendanceLog = req.ListAttendanceLog.Where(x => !checkExistLst.Any(e => e.EmployeeATID == x.UserId.GetNormalATID()
                                                                    && e.CheckTime == x.Time
                                                                    && e.SerialNumber == req.SerialNumber
                                                                   )).ToList();
            }

            if (req.ListAttendanceLog != null)
            {
                foreach (var item in req.ListAttendanceLog)
                {
                    try
                    {
                        IC_AttendanceLog attendanceLog = new IC_AttendanceLog();
                        attendanceLog.EmployeeATID = item.UserId.GetNormalATID();
                        attendanceLog.SerialNumber = req.SerialNumber;
                        attendanceLog.CompanyIndex = 2;
                        attendanceLog.CheckTime = item.Time;
                        attendanceLog.VerifyMode = Convert.ToInt16(item.VerifiedMode);
                        attendanceLog.InOutMode = Convert.ToInt16(item.InOutMode);
                        attendanceLog.WorkCode = 1;
                        attendanceLog.Reserve1 = 0;
                        attendanceLog.FaceMask = item.FaceMask;
                        attendanceLog.BodyTemperature = item.BodyTemperature;
                        attendanceLog.Function = "";
                        attendanceLog.UpdatedDate = DateTime.Now;
                        attendanceLog.UpdatedUser = "Service";

                        try
                        {
                            DbContext.IC_AttendanceLog.Add(attendanceLog);
                        }
                        catch
                        {
                            DbContext.IC_AttendanceLog.Remove(attendanceLog);
                        }

                        timeLogRequest.TimeLogs.Add(CreateTimeLogForRequest(attendanceLog));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"AddAttendanceLog: " + ex.Message);
                    }
                }
            }


            DbContext.SaveChanges();
            //AddAttendanceLog(req, ref timeLogRequest, ref obtEmployeeList, user);
            //await SendTimeLogToAPIAsync(cfg, timeLogRequest);
            // send email when exists over body temparature employees
            //SendMailWhenHaveEmployeeOverTemp(obtEmployeeList);
        }

        public void SendMailWhenHaveEmployeeOverTemp(List<OverBodyTemparatureEmployeesList> obtEmployeeList)
        {
            if (obtEmployeeList.Count > 0)
            {
                var configBodyTemperature = _IC_ConfigService.FirstOrDefault(e => e.EventType == ConfigAuto.GENERAL_SYSTEM_CONFIG.ToString());
                if (configBodyTemperature != null)
                {
                    string mailTo = configBodyTemperature.Email;
                    string subjectMessage = configBodyTemperature.TitleEmailError;
                    string bodyMessage = configBodyTemperature.BodyEmailError;
                    foreach (var obtEmployee in obtEmployeeList)
                    {
                        bodyMessage += $"\n MCC {obtEmployee.EmployeeATID} - {obtEmployee.BodyTemparature} độ - {obtEmployee.TimeLog}";
                    }
                    bool is_success = _emailProvider.SendEmailToMulti("", subjectMessage, bodyMessage, mailTo);
                }
            }
        }

        public async Task<DataGridClass> GetAttendanceLog(string pFilter, DateTime fromDate, DateTime toDate, List<string> pEmpATIDs, int pCompanyIndex, int pPageIndex, int pPageSize, List<string> listDevice)
        {
            var attendanceLog = DbContext.IC_AttendanceLog.Where(x => x.CompanyIndex == pCompanyIndex && (x.CheckTime >= fromDate && x.CheckTime <= toDate));
            //var attendanceLogIntegrate = DbContext.IC_AttendancelogIntegrate.Where(x => x.CompanyIndex == pCompanyIndex && (x.CheckTime >= fromDate && x.CheckTime <= toDate)).ToList();
            //var allLogMapp = _Mapper.Map<List<IC_AttendanceLog>>(attendanceLogIntegrate);
            //attendanceLog.AddRange(allLogMapp);

            attendanceLog = attendanceLog.GroupBy(x => new { x.EmployeeATID, x.CheckTime }).Select(x => x.First());

            var obj = from att in attendanceLog
                      join dv in DbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex)
                        on att.SerialNumber equals dv.SerialNumber

                      join wi in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on att.EmployeeATID equals wi.EmployeeATID into eWork

                      from eWorkResult in eWork.DefaultIfEmpty()
                      join emp in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                        on att.EmployeeATID equals emp.EmployeeATID

                      join dp in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                        on eWorkResult.DepartmentIndex equals dp.Index into temp
                      from dummy in temp.DefaultIfEmpty()

                      join poInfo in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on eWorkResult.PositionIndex equals poInfo.Index into item
                      from position in item.DefaultIfEmpty()

                      join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                            on att.EmployeeATID equals c.EmployeeATID into cWork
                      from cResult in cWork.DefaultIfEmpty()

                      where
                       (string.IsNullOrEmpty(pFilter)
                            ? emp.EmployeeATID.Contains("")
                            : (
                                   att.EmployeeATID.Contains(pFilter)
                                || (!string.IsNullOrEmpty(emp.EmployeeCode) && emp.EmployeeCode.Contains(pFilter))
                                || emp.FullName.Contains(pFilter)
                                //|| dummy.Name.Contains(pFilter)
                                || att.SerialNumber.Contains(pFilter)
                            ))
                      && ((pEmpATIDs != null && pEmpATIDs.Count > 0) ? pEmpATIDs.Any(y => y.Equals(att.EmployeeATID)) : emp.EmployeeATID.Contains(""))
                      select new AttendanceLogInfoResult()
                      {
                          EmployeeATID = att.EmployeeATID,
                          EmployeeCode = emp.EmployeeCode,
                          FullName = emp.FullName,
                          Time = att.CheckTime,
                          SerialNumber = att.SerialNumber,
                          DeviceNumber = att.DeviceNumber,
                          AliasName = dv.AliasName,
                          IPAddress = dv.IPAddress,
                          DepartmentName = dummy == null ? "" : dummy.Name,
                          PositionName = position == null ? "" : position.Name,
                          InOutMode = att.InOutMode.GetInOutModeString() == "In" ? "Vào" : "Ra",
                          VerifyMode = att.VerifyMode.GetVerifyModeString(),
                          FaceMask = att.FaceMask.GetFaceMaskString(),
                          BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                          IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                          CardNumber = cResult == null ? "" : cResult.CardNumber
                      };

            if (pPageIndex <= 1)
            {
                pPageIndex = 1;
            }

            int fromRow = pPageSize * (pPageIndex - 1);
            obj = obj.Where(x => !listDevice.Contains(x.SerialNumber));
            var lsAttendanceLog = obj.OrderByDescending(t => t.Time).Skip(fromRow).Take(pPageSize).ToList();
            var dataGrid = new DataGridClass(obj.Count(), lsAttendanceLog);
            return await Task.FromResult(dataGrid);
        }

        public async Task<Dictionary<string, object>> GetLogLast7Days()
        {
            var attendanceLogs = from att in DbContext.IC_AttendanceLog.Where(x
                                    => (x.CheckTime.Date >= DateTime.Now.Date.AddDays(-6) && x.CheckTime.Date <= DateTime.Now.Date))
                                 join u in DbContext.HR_User
                                 on att.EmployeeATID equals u.EmployeeATID
                                 into logUserInfo
                                 from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                 join d in DbContext.IC_Device
                                 on att.SerialNumber equals d.SerialNumber
                                 into logDeviceInfo
                                 from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                 join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                 on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                 from empCard in employeeCard.DefaultIfEmpty()
                                 select new
                                 {
                                     EmployeeATID = att.EmployeeATID,
                                     EmployeeCode = logUserInfoResult != null ? logUserInfoResult.EmployeeCode : string.Empty,
                                     FullName = logUserInfoResult != null ? logUserInfoResult.FullName : string.Empty,
                                     SerialNumber = att.SerialNumber,
                                     DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                     Time = att.CheckTime,
                                     InOutMode = att.InOutMode.GetInOutModeString(),
                                     VerifyMode = att.VerifyMode.GetVerifyModeString(),
                                     FaceMask = att.FaceMask.GetFaceMaskString(),
                                     BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                     IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                     CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                                     TimeString = att.CheckTime.ToString("dd/MM/yyy HH:mm:ss")
                                 };
            var listAttendanceLog = attendanceLogs.ToList();
            var groupAttendanceLog = listAttendanceLog.GroupBy(x => x.Time.Date.ToString("dd/MM/yyyy")).ToList();

            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddDays(-6);
            var list7Days = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                                        .Select(offset => startDate.AddDays(offset)).ToList();

            var result = new Dictionary<string, object>();
            foreach (var date in list7Days)
            {
                var key = date.ToString("dd/MM/yyyy");
                var listLogsOfDate = groupAttendanceLog.FirstOrDefault(x => x.Key == key)?.ToList();
                result.Add(key, listLogsOfDate);
            }

            return result;
        }

        public async Task<object> GetRemainInLogs()
        {
            var attendanceLogs = from att in DbContext.IC_AttendanceLog.Where(x => x.CheckTime.Date == DateTime.Now.Date)
                                 join u in DbContext.HR_User
                                 on att.EmployeeATID equals u.EmployeeATID
                                 into logUserInfo
                                 from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                 join w in DbContext.IC_WorkingInfo
                                 on logUserInfoResult.EmployeeATID equals w.EmployeeATID
                                 into workingInfo
                                 from workingInfoResult in workingInfo.DefaultIfEmpty()
                                 join dep in DbContext.IC_Department
                                 on workingInfoResult.DepartmentIndex equals dep.Index
                                 into departmentInfo
                                 from departmentInfoResult in departmentInfo.DefaultIfEmpty()
                                 join d in DbContext.IC_Device
                                 on att.SerialNumber equals d.SerialNumber
                                 into logDeviceInfo
                                 from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                 join p in DbContext.HR_PositionInfo
                                 on workingInfoResult.PositionIndex equals p.Index
                                 into positionInfo
                                 from positionInfoResult in positionInfo.DefaultIfEmpty()
                                 join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                 on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                 from empCard in employeeCard.DefaultIfEmpty()
                                 select new
                                 {
                                     EmployeeATID = att.EmployeeATID,
                                     EmployeeCode = logUserInfoResult != null ? logUserInfoResult.EmployeeCode : string.Empty,
                                     FullName = logUserInfoResult != null ? logUserInfoResult.FullName : string.Empty,
                                     SerialNumber = att.SerialNumber,
                                     DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                     DepartmentIndex = workingInfoResult != null ? workingInfoResult.DepartmentIndex : 0,
                                     DepartmentName = departmentInfoResult != null ? departmentInfoResult.Name : "NoDepartment",
                                     PositionIndex = workingInfoResult != null ? workingInfoResult.PositionIndex : 0,
                                     PositionName = positionInfoResult != null ? positionInfoResult.Name : string.Empty,
                                     Time = att.CheckTime,
                                     InOutMode = att.InOutMode.GetInOutModeString(),
                                     VerifyMode = att.VerifyMode.GetVerifyModeString(),
                                     FaceMask = att.FaceMask.GetFaceMaskString(),
                                     BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                     IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                     CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                                     TimeString = att.CheckTime.ToString("dd-MM-yyy HH:mm:ss")
                                 };

            var listAttendanceLog = await attendanceLogs.ToListAsync();

            var listInLogs = listAttendanceLog.Where(x => x.InOutMode == "In" || x.InOutMode == "BreakIn").ToList();
            var listOutLogs = listAttendanceLog.Where(x => x.InOutMode == "Out" || x.InOutMode == "BreakOut").ToList();
            var listRemainInLogs = new List<object>();

            // Build lookup of latest logs by id
            var latestInLogs = listInLogs.GroupBy(x => x.EmployeeATID)
                                         .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Time).First());

            var latestOutLogs = listOutLogs.GroupBy(x => x.EmployeeATID)
                                           .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Time).First());

            // Filter ids upfront
            var ids = latestInLogs.Keys.Union(latestOutLogs.Keys);

            // Simplified loop
            foreach (var id in ids)
            {
                if (latestInLogs.TryGetValue(id, out var inLog1) &&
                    !latestOutLogs.TryGetValue(id, out var outLog1))
                {
                    listRemainInLogs.Add(inLog1);
                }
                else if (latestInLogs.TryGetValue(id, out var inLog2) &&
                    (latestOutLogs.TryGetValue(id, out var outLog2) && inLog2.Time > outLog2.Time))
                {
                    listRemainInLogs.Add(inLog2);
                }
            }

            return listRemainInLogs;
        }


        public async Task<object> GetEmergencyAndEvacuation(UserInfo user)
        {
            var listCustomerCard = await DbContext.HR_CustomerCard.Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
            var listCustomerCardId = listCustomerCard.Select(x => x.CardID).ToList();

            var listPlanDock = await DbContext.IC_PlanDock.Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
            var listTripCode = listPlanDock.Select(x => x.TripId).ToList();

            var listGeneralRules = await DbContext.GC_Rules_General.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
            var rule = listGeneralRules.Where(x => x.IsUsing).FirstOrDefault();
            var isCheckRule = false;
            var now = DateTime.Now;
            if (rule != null && rule.PresenceTrackingTime > 0)
            {
                isCheckRule = true;
            }
            var lstReturn = new List<EmergencyAndEvacuation>();
            var lstReturn1 = new List<EmergencyAndEvacuation>();

            var attendanceLogs = from att in DbContext.GC_TimeLog.Where(x => x.ApproveStatus == (short)ApproveStatus.Approved)
                                 join u in DbContext.HR_User
                                 on att.EmployeeATID equals u.EmployeeATID
                                 //into logUserInfo
                                 //from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                 join ut in DbContext.HR_UserType
                                 on u.EmployeeType equals ut.UserTypeId
                                 into uutInfo
                                 from uutResult in uutInfo.DefaultIfEmpty()
                                 join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                 && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                                 on att.EmployeeATID equals wi.EmployeeATID
                                 into attInfo
                                 from attResult in attInfo.DefaultIfEmpty()
                                 join dp in DbContext.IC_Department
                                 on attResult.DepartmentIndex equals dp.Index into dep
                                 from depResult in dep.DefaultIfEmpty()
                                 join d in DbContext.IC_Device.Where(x => x.DeviceModule != "ICMS")
                                 on att.MachineSerial equals d.SerialNumber
                                 into logDeviceInfo
                                 from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                 join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                 on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                 from empCard in employeeCard.DefaultIfEmpty()
                                 where isCheckRule == false || (now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= att.Time)
                                 select new WorkingLogResult
                                 {
                                     EmployeeATID = att.EmployeeATID,
                                     EmployeeCode = u != null ? u.EmployeeCode : string.Empty,
                                     FullName = u != null ? u.FullName : string.Empty,
                                     UserType = u != null
                                         ? (u.EmployeeType.HasValue
                                         ? u.EmployeeType.Value : (short)EmployeeType.Employee)
                                         : (short)EmployeeType.Employee,
                                     UserTypeName = uutResult != null ? uutResult.Name : "Nhân viên",
                                     DepartmentIndex = attResult != null ? attResult.DepartmentIndex : 0,
                                     RootDepartment = (depResult == null || (depResult != null && (!depResult.ParentIndex.HasValue
                                        || (depResult.ParentIndex.HasValue && (depResult.ParentIndex.Value == 0 || depResult.ParentIndex.Value == depResult.Index))))),
                                     DepartmentName = depResult != null ? depResult.Name : "NoDepartment",
                                     SerialNumber = att.MachineSerial,
                                     DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                     Time = att.Time,
                                     InOutMode = att.InOutMode.Value.GetGCSInOutModeString(),
                                     VerifyMode = att.VerifyMode,
                                     CardNumber = empCard != null ? empCard.CardNumber : att.CardNumber,
                                     TimeString = att.Time.ToString("dd/MM/yyy HH:mm:ss")
                                 };



            var vehicleLogs = from att in DbContext.GC_TruckDriverLog
                              join u in DbContext.IC_PlanDock
                              on att.TripCode equals u.TripId
                              into logUserInfo
                              from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                              join d in DbContext.IC_Device.Where(x => x.DeviceModule != "ICMS")
                              on att.MachineSerial equals d.SerialNumber
                              into logDeviceInfo
                              from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                              join c in DbContext.HR_CustomerCard
                              on att.CardNumber equals c.CardNumber into employeeCard
                              from empCard in employeeCard.DefaultIfEmpty()
                              where isCheckRule == false || (now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= att.Time)
                              select new WorkingLogResult
                              {
                                  EmployeeATID = att.TripCode,
                                  //EmployeeATID = logUserInfoResult != null ? logUserInfoResult.TripId : string.Empty,
                                  EmployeeCode = logUserInfoResult != null ? logUserInfoResult.DriverCode : string.Empty,
                                  FullName = logUserInfoResult != null ? logUserInfoResult.DriverName : string.Empty,
                                  UserType = (short)EmployeeType.Driver,
                                  UserTypeName = "Tài xế",
                                  SerialNumber = att.MachineSerial,
                                  DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                  Time = att.Time,
                                  InOutMode = att.InOutMode.Value.GetInOutModeString(),
                                  CardNumber = empCard != null ? empCard.CardNumber : att.CardNumber,
                                  TimeString = att.Time.ToString("dd/MM/yyy HH:mm:ss"),
                              };

            var integratedVehicleInNotOutLogs =
                        from att in DbContext.IC_VehicleLog
                        join u in DbContext.HR_User on att.EmployeeATID equals u.EmployeeATID
                        join ut in DbContext.HR_UserType on u.EmployeeType equals ut.UserTypeId into uutInfo
                        from uutResult in uutInfo.DefaultIfEmpty()
                        join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                        && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                        on att.EmployeeATID equals wi.EmployeeATID into attInfo
                        from attResult in attInfo.DefaultIfEmpty()
                        join dp in DbContext.IC_Department on attResult.DepartmentIndex equals dp.Index into dep
                        from depResult in dep.DefaultIfEmpty()
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                        on att.EmployeeATID equals c.EmployeeATID into employeeCard
                        from empCard in employeeCard.DefaultIfEmpty()

                        where !listCustomerCardId.Contains(att.EmployeeATID)
                            && !listTripCode.Contains(att.EmployeeATID)
                            && (!string.IsNullOrWhiteSpace(att.ComputerIn)
                                || (!string.IsNullOrWhiteSpace(att.ComputerOut) && att.ComputerOut != "-1"))
                        && isCheckRule == false || (now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= att.FromDate.Value
                            || (att.ToDate.HasValue && now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= att.ToDate.Value))
                        select new WorkingLogResult
                        {
                            EmployeeATID = att.EmployeeATID,
                            EmployeeCode = u != null ? u.EmployeeCode : string.Empty,
                            FullName = u != null ? u.FullName : string.Empty,
                            UserType = u != null
                                         ? (u.EmployeeType.HasValue
                                         ? u.EmployeeType.Value : (short)EmployeeType.Employee)
                                         : (short)EmployeeType.Employee,
                            UserTypeName = uutResult != null ? uutResult.Name : "Nhân viên",
                            DepartmentIndex = attResult != null ? attResult.DepartmentIndex : 0,
                            RootDepartment = (depResult == null || (depResult != null && (!depResult.ParentIndex.HasValue
                                        || (depResult.ParentIndex.HasValue && (depResult.ParentIndex.Value == 0 || depResult.ParentIndex.Value == depResult.Index))))),
                            DepartmentName = depResult != null ? depResult.Name : "NoDepartment",
                            SerialNumber = att.ComputerIn,
                            DeviceName = att.ComputerIn,
                            //DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                            Time = att.FromDate.Value,
                            FromDate = att.FromDate,
                            ToDate = att.ToDate,
                            ComputerIn = att.ComputerIn,
                            ComputerOut = att.ComputerOut,
                            VerifyMode = EPAD_Common.Enums.DeviceType.Card.ToString(),
                            //FaceMask = att.FaceMask.GetFaceMaskString(),
                            //BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                            //IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                            CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                            TimeString = att.FromDate.Value.ToString("dd/MM/yyy HH:mm:ss")
                        };
            var listAttendanceLog = await attendanceLogs.ToListAsync();
            var listVehicleLog = await vehicleLogs.ToListAsync();
            var listIntegratedVehicleInNotOutLogs = await integratedVehicleInNotOutLogs.ToListAsync();

            var listComputerID = new List<string>();
            var listIntegratedVehicleInLogs = listIntegratedVehicleInNotOutLogs.Where(x => !string.IsNullOrWhiteSpace(x.ComputerIn)
                && x.FromDate.HasValue).Select(x
                => new WorkingLogResult
                {
                    EmployeeATID = x.EmployeeATID,
                    EmployeeCode = x.EmployeeCode,
                    FullName = x.FullName,
                    UserType = x.UserType,
                    UserTypeName = x.UserTypeName,
                    DepartmentIndex = x.DepartmentIndex,
                    RootDepartment = x.RootDepartment,
                    DepartmentName = x.DepartmentName,
                    SerialNumber = x.ComputerIn,
                    DeviceName = x.ComputerIn,
                    Time = x.FromDate.Value,
                    InOutMode = "In",
                    VerifyMode = EPAD_Common.Enums.DeviceType.Card.ToString(),
                    CardNumber = x.CardNumber,
                    TimeString = x.TimeString,
                    IsIntegratedLog = true
                }).ToList();
            if (listIntegratedVehicleInLogs != null && listIntegratedVehicleInLogs.Count > 0)
            {
                listIntegratedVehicleInLogs = listIntegratedVehicleInLogs.GroupBy(x
                    => new { x.EmployeeATID, x.Time }).Select(x => x.FirstOrDefault()).ToList();
                listComputerID.AddRange(listIntegratedVehicleInLogs.Where(x
                    => !string.IsNullOrWhiteSpace(x.SerialNumber)).Select(x => x.SerialNumber).ToList());
            }

            var listIntegratedVehicleOutLogs = listIntegratedVehicleInNotOutLogs.Where(x => !string.IsNullOrWhiteSpace(x.ComputerOut)
                && x.ComputerOut != "-1" && x.ToDate.HasValue).Select(x
                => new WorkingLogResult
                {
                    EmployeeATID = x.EmployeeATID,
                    EmployeeCode = x.EmployeeCode,
                    FullName = x.FullName,
                    UserType = x.UserType,
                    UserTypeName = x.UserTypeName,
                    DepartmentIndex = x.DepartmentIndex,
                    RootDepartment = x.RootDepartment,
                    DepartmentName = x.DepartmentName,
                    SerialNumber = x.ComputerOut,
                    DeviceName = x.ComputerOut,
                    Time = x.ToDate.Value,
                    InOutMode = "Out",
                    VerifyMode = EPAD_Common.Enums.DeviceType.Card.ToString(),
                    CardNumber = x.CardNumber,
                    TimeString = x.TimeString,
                    IsIntegratedLog = true
                }).ToList();

            if (listIntegratedVehicleOutLogs != null && listIntegratedVehicleOutLogs.Count > 0)
            {
                listIntegratedVehicleOutLogs = listIntegratedVehicleOutLogs.GroupBy(x
                    => new { x.EmployeeATID, x.Time }).Select(x => x.FirstOrDefault()).ToList();
                listComputerID.AddRange(listIntegratedVehicleOutLogs.Where(x
                    => !string.IsNullOrWhiteSpace(x.SerialNumber)).Select(x => x.SerialNumber).ToList());
            }

            var listIntegratedDeviceByComputerID = new List<IC_Device>();
            var listIntegratedLineDeviceIn = new List<GC_Lines_CheckInDevice>();
            var listIntegratedLineDeviceOut = new List<GC_Lines_CheckOutDevice>();
            var listIntegratedGateLine = new List<GC_Gates_Lines>();
            var listIntegratedGate = new List<GC_Gates>();
            if (listComputerID.Count > 0)
            {
                listIntegratedDeviceByComputerID = await DbContext.IC_Device.AsNoTracking().Where(x
                    => !string.IsNullOrWhiteSpace(x.DeviceId) && x.DeviceModule == "PA" && listComputerID.Contains(x.DeviceId)).ToListAsync();
                if (listIntegratedDeviceByComputerID.Count > 0)
                {
                    var listIntegratedDeviceSerial = listIntegratedDeviceByComputerID.Where(x
                        => !string.IsNullOrWhiteSpace(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                    if (listIntegratedDeviceSerial.Count > 0)
                    {
                        listIntegratedLineDeviceIn = await DbContext.GC_Lines_CheckInDevice.AsNoTracking().Where(x
                            => listIntegratedDeviceSerial.Contains(x.CheckInDeviceSerial)).ToListAsync();
                        listIntegratedLineDeviceOut = await DbContext.GC_Lines_CheckOutDevice.AsNoTracking().Where(x
                            => listIntegratedDeviceSerial.Contains(x.CheckOutDeviceSerial)).ToListAsync();
                        var listIntegratedLineIndex = new List<int>();
                        if (listIntegratedLineDeviceIn.Count > 0)
                        {
                            listIntegratedLineIndex.AddRange(listIntegratedLineDeviceIn.Select(x => x.LineIndex));
                        }
                        if (listIntegratedLineDeviceOut.Count > 0)
                        {
                            listIntegratedLineIndex.AddRange(listIntegratedLineDeviceOut.Select(x => x.LineIndex));
                        }
                        if (listIntegratedLineIndex.Count > 0)
                        {
                            listIntegratedGateLine = await DbContext.GC_Gates_Lines.AsNoTracking().Where(x
                                => listIntegratedLineIndex.Contains(x.LineIndex)).ToListAsync();
                            if (listIntegratedGateLine.Count > 0)
                            {
                                var listIntegratedGateIndex = listIntegratedGateLine.Select(x => x.GateIndex).ToList();
                                listIntegratedGate = await DbContext.GC_Gates.AsNoTracking().Where(x
                                    => listIntegratedGateIndex.Contains(x.Index)).ToListAsync();
                            }
                        }
                    }
                }
            }

            if (listIntegratedVehicleInLogs != null && listIntegratedVehicleInLogs.Count > 0)
            {
                foreach (var integratedLogIn in listIntegratedVehicleInLogs)
                {
                    integratedLogIn.GateName = new List<string>();
                    if (listIntegratedDeviceByComputerID.Any(x => x.DeviceId == integratedLogIn.SerialNumber
                        && x.DeviceStatus.HasValue
                        && (x.DeviceStatus == (short)DeviceStatus.Input || x.DeviceStatus == (short)DeviceStatus.MiddleInt)))
                    {
                        var listIntegratedLogInDevice = listIntegratedDeviceByComputerID.Where(x
                            => x.DeviceId == integratedLogIn.SerialNumber
                            && x.DeviceStatus.HasValue
                            && (x.DeviceStatus == (short)DeviceStatus.Input || x.DeviceStatus == (short)DeviceStatus.MiddleInt)).ToList();

                        if ((listIntegratedLineDeviceIn.Any(x
                                => listIntegratedLogInDevice.Any(y => y.SerialNumber == x.CheckInDeviceSerial))
                            || listIntegratedLineDeviceOut.Any(x
                                => listIntegratedLogInDevice.Any(y => y.SerialNumber == x.CheckOutDeviceSerial))))
                        {
                            var listIntegratedLogInLineIndex = listIntegratedLineDeviceIn.Where(x
                                => listIntegratedLogInDevice.Any(y => y.SerialNumber == x.CheckInDeviceSerial)).Select(x
                                    => x.LineIndex).ToList() ?? new List<int>();
                            listIntegratedLogInLineIndex.AddRange(listIntegratedLineDeviceOut.Where(x
                                => listIntegratedLogInDevice.Any(y => y.SerialNumber == x.CheckOutDeviceSerial)).Select(x
                                    => x.LineIndex).ToList() ?? new List<int>());
                            if (listIntegratedGateLine.Any(x => listIntegratedLogInLineIndex.Contains(x.LineIndex)))
                            {
                                var listIntegratedLogInGateIndex = listIntegratedGateLine.Where(x
                                    => listIntegratedLogInLineIndex.Contains(x.LineIndex)).Select(x => x.GateIndex).ToList();
                                var listIntegratedLogInGate = listIntegratedGate.Where(x
                                    => listIntegratedLogInGateIndex.Contains(x.Index)).ToList();
                                if (listIntegratedLogInGate.Count > 0)
                                {
                                    integratedLogIn.GateName = new List<string> { listIntegratedLogInGate.FirstOrDefault().Name };
                                }
                            }
                        }
                        else
                        {
                            integratedLogIn.GateName = new List<string> { listIntegratedLogInDevice.FirstOrDefault().AliasName };
                        }
                    }
                }
                listAttendanceLog.AddRange(listIntegratedVehicleInLogs);
            }

            if (listIntegratedVehicleOutLogs != null && listIntegratedVehicleOutLogs.Count > 0)
            {
                foreach (var integratedLogOut in listIntegratedVehicleOutLogs)
                {
                    integratedLogOut.GateName = new List<string>();
                    if (listIntegratedDeviceByComputerID.Any(x => x.DeviceId == integratedLogOut.SerialNumber
                        && x.DeviceStatus.HasValue
                        && (x.DeviceStatus == (short)DeviceStatus.Output || x.DeviceStatus == (short)DeviceStatus.MiddleOut)))
                    {
                        var listIntegratedLogOutDevice = listIntegratedDeviceByComputerID.Where(x
                            => x.DeviceId == integratedLogOut.SerialNumber
                            && x.DeviceStatus.HasValue
                            && (x.DeviceStatus == (short)DeviceStatus.Output || x.DeviceStatus == (short)DeviceStatus.MiddleOut)).ToList();

                        if ((listIntegratedLineDeviceIn.Any(x
                                => listIntegratedLogOutDevice.Any(y => y.SerialNumber == x.CheckInDeviceSerial))
                            || listIntegratedLineDeviceOut.Any(x
                                => listIntegratedLogOutDevice.Any(y => y.SerialNumber == x.CheckOutDeviceSerial))))
                        {
                            var listIntegratedLogOutLineIndex = listIntegratedLineDeviceIn.Where(x
                                => listIntegratedLogOutDevice.Any(y => y.SerialNumber == x.CheckInDeviceSerial)).Select(x
                                    => x.LineIndex).ToList() ?? new List<int>();
                            listIntegratedLogOutLineIndex.AddRange(listIntegratedLineDeviceOut.Where(x
                                => listIntegratedLogOutDevice.Any(y => y.SerialNumber == x.CheckOutDeviceSerial)).Select(x
                                    => x.LineIndex).ToList() ?? new List<int>());
                            if (listIntegratedGateLine.Any(x => listIntegratedLogOutLineIndex.Contains(x.LineIndex)))
                            {
                                var listIntegratedLogOutGateIndex = listIntegratedGateLine.Where(x
                                    => listIntegratedLogOutLineIndex.Contains(x.LineIndex)).Select(x => x.GateIndex).ToList();
                                var listIntegratedLogOutGate = listIntegratedGate.Where(x
                                    => listIntegratedLogOutGateIndex.Contains(x.Index)).ToList();
                                if (listIntegratedLogOutGate.Count > 0)
                                {
                                    integratedLogOut.GateName = new List<string> { listIntegratedLogOutGate.FirstOrDefault().Name };
                                }
                            }
                        }
                        else
                        {
                            integratedLogOut.GateName = new List<string> { listIntegratedLogOutDevice.FirstOrDefault().AliasName };
                        }
                    }
                }
                listAttendanceLog.AddRange(listIntegratedVehicleOutLogs);
            }

            var isInActiveVehicle = await DbContext.GC_TruckDriverLog.Where(x => x.IsInactive).ToListAsync();
            var isInActiveVehicleCode = isInActiveVehicle.Select(x => x.TripCode).ToList();
            //listAttendanceLog = listAttendanceLog.Where(x => !isInActiveVehicleCode.Contains(x.EmployeeATID)).ToList();
            listAttendanceLog = listAttendanceLog.Where(x => !isInActiveVehicle.Any(y
                => y.TripCode == x.EmployeeATID && y.CardNumber == x.CardNumber)).ToList();

            //listAttendanceLog = listAttendanceLog.Where(x => !listVehicleLog.Any(y => y.EmployeeATID == x.EmployeeATID)).ToList();
            if (listVehicleLog.Count > 0)
            {
                //listVehicleLog = listVehicleLog.Where(x => !isInActiveVehicleCode.Contains(x.EmployeeATID)).ToList();
                listVehicleLog = listVehicleLog.Where(x => !isInActiveVehicle.Any(y
                    => y.TripCode == x.EmployeeATID && y.CardNumber == x.CardNumber)).ToList();
                listAttendanceLog.AddRange(listVehicleLog);
                var listInVehicleTripCode = listVehicleLog.Select(x => x.EmployeeATID).ToList();
                var listInVehicleLog = await DbContext.GC_TimeLog.AsNoTracking().Where(x
                    => listInVehicleTripCode.Contains(x.EmployeeATID)).ToListAsync();
                listInVehicleLog = listInVehicleLog.Where(x => listVehicleLog.Any(y
                    => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber)
                    && x.ApproveStatus == (short)ApproveStatus.Approved).ToList();
                if (listInVehicleLog.Count > 0)
                {
                    var listVehicleAttendanceLog = listInVehicleLog.Select(x => new WorkingLogResult
                    {

                        EmployeeATID = x.EmployeeATID,
                        //EmployeeATID = logUserInfoResult != null ? logUserInfoResult.TripId : string.Empty,
                        EmployeeCode = listVehicleLog.FirstOrDefault(y
                            => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber)?.EmployeeCode ?? string.Empty,
                        FullName = listVehicleLog.FirstOrDefault(y
                            => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber)?.FullName ?? string.Empty,
                        UserType = (short)EmployeeType.Driver,
                        UserTypeName = "Tài xế",
                        SerialNumber = x.MachineSerial,
                        Time = x.Time,
                        InOutMode = x.InOutMode.Value.GetGCSInOutModeString(),
                        CardNumber = x.CardNumber,
                        TimeString = x.Time.ToString("dd/MM/yyy HH:mm:ss"),
                    });
                    listAttendanceLog.AddRange(listVehicleAttendanceLog);
                }
            }

            var listVehicleTripCode = listAttendanceLog.Select(x => x.EmployeeATID).ToList();
            if (isInActiveVehicle.Count > 0)
            {
                listVehicleTripCode.AddRange(isInActiveVehicleCode);
            }
            var existedExtraTruckDriver = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                => listVehicleTripCode.Contains(x.TripCode) && !x.IsInactive).ToListAsync();
            var existedExtraTruckDriverTripCode = existedExtraTruckDriver.Select(x => x.TripCode).ToList();
            var existedTruckDriver = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x
                => existedExtraTruckDriverTripCode.Contains(x.TripCode)
                //&& !x.IsInactive
                ).ToListAsync();
            var existedTruckDriverSerial = existedTruckDriver.Select(x => x.MachineSerial).ToList();
            var deviceList = await DbContext.IC_Device.AsNoTracking().Where(x
                => existedTruckDriverSerial.Contains(x.SerialNumber)).ToListAsync();

            if (existedExtraTruckDriver != null && existedExtraTruckDriver.Count > 0)
            {
                var listExtraVehicleLog = existedExtraTruckDriver.Select(x => new WorkingLogResult
                {
                    EmployeeATID = x.TripCode,
                    //EmployeeATID = logUserInfoResult != null ? logUserInfoResult.TripId : string.Empty,
                    EmployeeCode = x.ExtraDriverCode,
                    FullName = x.ExtraDriverName,
                    UserType = (short)EmployeeType.Driver,
                    UserTypeName = "Phụ xế",
                    SerialNumber = existedTruckDriver.Any(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input)
                        ? existedTruckDriver.FirstOrDefault(y => y.TripCode == x.TripCode
                        && y.InOutMode == (short)InOutMode.Input).MachineSerial : string.Empty,
                    Time = existedTruckDriver.Any(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input)
                        ? existedTruckDriver.FirstOrDefault(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input).Time
                        : new DateTime(),
                    InOutMode = "In",
                    CardNumber = x.CardNumber,
                    TimeString = (existedTruckDriver.Any(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input)
                        ? existedTruckDriver.FirstOrDefault(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input).Time
                        : new DateTime()).ToString("dd/MM/yyy HH:mm:ss"),
                    IsExtraDriver = true
                }).ToList();
                listExtraVehicleLog.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.SerialNumber) && deviceList.Any(y => y.SerialNumber == x.SerialNumber))
                    {
                        x.DeviceName = deviceList.FirstOrDefault(y => y.SerialNumber == x.SerialNumber).AliasName;
                    }
                });
                listAttendanceLog.AddRange(listExtraVehicleLog);

                var listInVehicleTripCode = listExtraVehicleLog.Select(x => x.EmployeeATID).ToList();
                var listInVehicleLog = await DbContext.GC_TimeLog.AsNoTracking().Where(x
                    => listInVehicleTripCode.Contains(x.EmployeeATID)).ToListAsync();
                listInVehicleLog = listInVehicleLog.Where(x => listExtraVehicleLog.Any(y
                    => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber)
                    && x.ApproveStatus == (short)ApproveStatus.Approved).ToList();
                if (listInVehicleLog.Count > 0)
                {
                    var listVehicleAttendanceLog = listInVehicleLog.Select(x => new WorkingLogResult
                    {

                        EmployeeATID = x.EmployeeATID,
                        //EmployeeATID = logUserInfoResult != null ? logUserInfoResult.TripId : string.Empty,
                        EmployeeCode = listExtraVehicleLog.FirstOrDefault(y
                            => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber)?.EmployeeCode ?? string.Empty,
                        FullName = listExtraVehicleLog.FirstOrDefault(y
                            => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber)?.FullName ?? string.Empty,
                        UserType = (short)EmployeeType.Driver,
                        UserTypeName = "Phụ xế",
                        SerialNumber = x.MachineSerial,
                        Time = x.Time,
                        InOutMode = x.InOutMode.Value.GetGCSInOutModeString(),
                        CardNumber = x.CardNumber,
                        TimeString = x.Time.ToString("dd/MM/yyy HH:mm:ss"),
                        IsExtraDriver = true
                    });
                    listAttendanceLog.AddRange(listVehicleAttendanceLog);
                }
            }

            var listInAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "In" || x.InOutMode == "BreakIn").ToList();
            var listOutAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "Out" || x.InOutMode == "BreakOut").ToList();

            var listInNotOutAttendanceLogs = listInAttendanceLog.Where(x
                => !listOutAttendanceLog.Any(y => y.EmployeeATID == x.EmployeeATID && y.Time >= x.Time
                && y.CardNumber == x.CardNumber)).ToList();


            listInNotOutAttendanceLogs = listInNotOutAttendanceLogs.OrderByDescending(x => x.Time).GroupBy(x
                => new { x.EmployeeATID, x.CardNumber }).Select(x => x.FirstOrDefault()).ToList();
            listInNotOutAttendanceLogs.RemoveAll(x => x == null);

            var listFilterAttendanceLog = new List<WorkingLogResult>();
            listFilterAttendanceLog.AddRange(listInNotOutAttendanceLogs);

            listFilterAttendanceLog = listFilterAttendanceLog.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null
                && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();

            var listEmployeeATID = listFilterAttendanceLog.Select(x => x.EmployeeATID).ToList();
            listEmployeeATID = listEmployeeATID.Where(x => existedTruckDriver.Any(y => y.TripCode == x && !y.IsInactive)).ToList();
            var existedTruckDriverLog = await DbContext.IC_PlanDock.AsNoTracking().Where(x
                => listEmployeeATID.Contains(x.TripId)).ToListAsync();
            var existedExtraTruckDriverLog = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                => listEmployeeATID.Contains(x.TripCode) && !x.IsInactive).ToListAsync();

            listFilterAttendanceLog.ForEach(x =>
            {
                if (existedTruckDriverLog.Any(y => y.TripId == x.EmployeeATID))
                {
                    x.UserType = (short)EmployeeType.Driver;
                    x.FullName = existedTruckDriverLog.FirstOrDefault(y => y.TripId == x.EmployeeATID).DriverName;
                    x.UserTypeName = "Tài xế";
                }
                if (existedExtraTruckDriverLog.Any(y => y.TripCode == x.EmployeeATID && y.CardNumber == x.CardNumber))
                {
                    x.UserType = (short)EmployeeType.Driver;
                    x.FullName = existedExtraTruckDriverLog.FirstOrDefault(y => y.TripCode == x.EmployeeATID && y.CardNumber == x.CardNumber).ExtraDriverName;
                    x.UserTypeName = "Phụ xế";
                    x.IsExtraDriver = true;
                }
            });

            var listSerialNumber = new List<string>();
            if (listFilterAttendanceLog != null && listFilterAttendanceLog.Count > 0)
            {
                listSerialNumber = listFilterAttendanceLog.Select(x => x.SerialNumber).Distinct().ToList();
            }
            var listLineIndex = new List<int>();
            var listACDoorIndex = new List<int>();
            var listLineIn = new List<GC_Lines_CheckInDevice>();
            var listLineOut = new List<GC_Lines_CheckOutDevice>();
            var listACDoorAndDevice = new List<AC_DoorAndDevice>();
            if (listSerialNumber != null && listSerialNumber.Count > 0)
            {
                listLineIn = await DbContext.GC_Lines_CheckInDevice.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.CheckInDeviceSerial)).ToListAsync();
                listLineOut = await DbContext.GC_Lines_CheckOutDevice.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.CheckOutDeviceSerial)).ToListAsync();
                listACDoorAndDevice = await DbContext.AC_DoorAndDevice.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.SerialNumber)).ToListAsync();
                if (listLineIn != null && listLineIn.Count > 0)
                {
                    listLineIndex.AddRange(listLineIn.Select(x => x.LineIndex).ToList());
                }
                if (listLineOut != null && listLineOut.Count > 0)
                {
                    listLineIndex.AddRange(listLineOut.Select(x => x.LineIndex).ToList());
                }
                if (listACDoorAndDevice != null && listACDoorAndDevice.Count > 0)
                {
                    listACDoorIndex.AddRange(listACDoorAndDevice.Select(x => x.DoorIndex).ToList());
                }
            }

            var listGroupDeviceIndex = new List<int>();
            var listGroupDeviceDetails = new List<IC_GroupDeviceDetails>();
            if (listSerialNumber != null && listSerialNumber.Count > 0)
            {
                listGroupDeviceDetails = await DbContext.IC_GroupDeviceDetails.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.SerialNumber)).ToListAsync();
                if (listGroupDeviceDetails != null && listGroupDeviceDetails.Count > 0)
                {
                    listGroupDeviceIndex = listGroupDeviceDetails.Select(x => x.GroupDeviceIndex).Distinct().ToList();
                }
            }

            var listAreaGroupIndex = new List<int>();
            var listAreaGroupDetail = new List<GC_AreaGroup_GroupDevice>();
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listAreaGroupDetail = await DbContext.GC_AreaGroup_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.DeviceGroupIndex)).ToListAsync();
                if (listAreaGroupDetail != null && listAreaGroupDetail.Count > 0)
                {
                    listAreaGroupIndex = listAreaGroupDetail.Select(x => x.AreaGroupIndex).Distinct().ToList();
                }
            }

            var listGateIndex = new List<int>();
            var listGateLine = new List<GC_Gates_Lines>();
            if (listLineIndex != null && listLineIndex.Count > 0)
            {
                listGateLine = await DbContext.GC_Gates_Lines.AsNoTracking().Where(x
                    => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                if (listGateLine != null && listGateLine.Count > 0)
                {
                    listGateIndex = listGateLine.Select(x => x.GateIndex).Distinct().ToList();
                }
            }

            var listLine = new List<GC_Lines>();
            var listGate = new List<GC_Gates>();
            var listACDoor = new List<AC_Door>();
            var listGroupDevice = new List<IC_GroupDevice>();
            var listAreaGroup = new List<GC_AreaGroup>();
            if (listLineIndex != null && listLineIndex.Count > 0)
            {
                listLine = await DbContext.GC_Lines.AsNoTracking().Where(x
                    => listLineIndex.Contains(x.Index)).ToListAsync();
            }
            if (listGateIndex != null && listGateIndex.Count > 0)
            {
                listGate = await DbContext.GC_Gates.AsNoTracking().Where(x
                    => listGateIndex.Contains(x.Index)).ToListAsync();
            }
            if (listACDoorIndex != null && listACDoorIndex.Count > 0)
            {
                listACDoor = await DbContext.AC_Door.AsNoTracking().Where(x
                    => listACDoorIndex.Contains(x.Index)).ToListAsync();
            }
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listGroupDevice = await DbContext.IC_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.Index)).ToListAsync();
            }
            if (listAreaGroupIndex != null && listAreaGroupIndex.Count > 0)
            {
                listAreaGroup = await DbContext.GC_AreaGroup.AsNoTracking().Where(x
                    => listAreaGroupIndex.Contains(x.Index)).ToListAsync();
            }

            var logLine = new GC_Lines();
            var logGate = new List<GC_Gates>();
            var logGroupDevice = new List<IC_GroupDevice>();
            var logAreaGroup = new List<GC_AreaGroup>();
            listFilterAttendanceLog.ForEach(x =>
            {
                if (!x.IsIntegratedLog)
                {
                    logLine = new GC_Lines();
                    logGate = new List<GC_Gates>();
                    logGroupDevice = new List<IC_GroupDevice>();
                    logAreaGroup = new List<GC_AreaGroup>();

                    var logLineIn = listLineIn.FirstOrDefault(y => y.CheckInDeviceSerial == x.SerialNumber);
                    var logLineOut = listLineOut.FirstOrDefault(y => y.CheckOutDeviceSerial == x.SerialNumber);
                    var logLineIndex = 0;
                    if (logLineIn != null)
                    {
                        logLine = listLine.FirstOrDefault(y => y.Index == logLineIn.LineIndex);
                        logLineIndex = logLineIn.LineIndex;
                        x.LineIndex = logLine.Index;
                        x.LineName = logLine.Name;
                    }
                    else if (logLineOut != null)
                    {
                        logLine = listLine.FirstOrDefault(y => y.Index == logLineOut.LineIndex);
                        logLineIndex = logLineOut.LineIndex;
                        x.LineIndex = logLine.Index;
                        x.LineName = logLine.Name;
                    }
                    var logGateLine = listGateLine.Where(y => y.LineIndex == logLineIndex).ToList();
                    var logACDoorAndDevice = listACDoorAndDevice.FirstOrDefault(y => x.SerialNumber == y.SerialNumber);
                    if (logGateLine != null && logGateLine.Count > 0)
                    {
                        var gateIndex = logGateLine.Select(y => y.GateIndex).ToList();
                        logGate = listGate.Where(y => gateIndex.Contains(y.Index)).ToList();
                        x.GateIndex = logGate.Select(x => x.Index).ToList();
                        x.GateName = logGate.Select(x => x.Name).ToList();
                    }
                    else if (logACDoorAndDevice != null)
                    {
                        var doorIndex = logACDoorAndDevice.DoorIndex;
                        var door = listACDoor.FirstOrDefault(x => x.Index == doorIndex);
                        x.GateIndex = door != null ? new List<int> { door.Index } : new List<int>();
                        x.GateName = door != null ? new List<string> { door.Name } : new List<string>();
                    }
                    else
                    {
                        x.GateIndex = new List<int>();
                        x.GateName = new List<string> { x.DeviceName };
                    }

                    var logGroupDeviceIndex = listGroupDeviceDetails.Where(y => y.SerialNumber == x.SerialNumber)
                        .Select(x => x.GroupDeviceIndex).ToList();
                    if (logGroupDeviceIndex != null && logGroupDeviceIndex.Count > 0)
                    {
                        x.GroupDeviceIndex = logGroupDeviceIndex;
                        logGroupDevice = listGroupDevice.Where(g => listAreaGroupIndex.Contains(g.Index)).ToList();
                        if (logGroupDevice != null && logGroupDevice.Count > 0)
                        {
                            x.GroupDeviceName = logGroupDevice.Select(x => x.Name).ToList();
                        }

                        var logAreaGroupIndex = listAreaGroupDetail.Where(y => logGroupDeviceIndex.Contains(y.DeviceGroupIndex))
                            .Select(x => x.AreaGroupIndex).ToList();
                        if (logAreaGroupIndex != null && logAreaGroupIndex.Count > 0)
                        {
                            x.AreaGroupIndex = logAreaGroupIndex;
                            logAreaGroup = listAreaGroup.Where(g => logAreaGroupIndex.Contains(g.Index)).ToList();
                            if (logAreaGroup != null && logAreaGroup.Count > 0)
                            {
                                x.AreaGroupName = logAreaGroup.Select(x => x.Name).ToList();
                            }
                        }
                    }
                }
            });

            if (rule != null && rule.PresenceTrackingTime > 0)
            {

                listFilterAttendanceLog = listFilterAttendanceLog.Where(x
                    => now.Subtract(x.Time).TotalMinutes <= Convert.ToDouble(rule.PresenceTrackingTime * 60)).ToList();
            }
            var listEmployeeLogs = listFilterAttendanceLog.Where(x => x.UserType == (short)EmployeeType.Employee).GroupBy(x => new { x.DepartmentIndex, x.DepartmentName }).Select(x => new EmergencyDepartmentCountModel { DepartmentIndex = x.Key.DepartmentIndex, DepartmentName = x.Key.DepartmentName, Count = x.Count() }).ToList();
            var listCustomerLogs = listFilterAttendanceLog.Where(x => x.UserType == (short)EmployeeType.Guest).GroupBy(x => new { x.DepartmentIndex, x.DepartmentName }).Select(x => new EmergencyDepartmentCountModel  { DepartmentIndex = x.Key.DepartmentIndex, DepartmentName = x.Key.DepartmentName, Count = x.Count() }).ToList();
            var listContractorLogs = listFilterAttendanceLog.Where(x => x.UserType == (short)EmployeeType.Contractor).GroupBy(x => new { x.DepartmentIndex, x.DepartmentName }).Select(x => new EmergencyDepartmentCountModel  { DepartmentIndex = x.Key.DepartmentIndex, DepartmentName = x.Key.DepartmentName, Count = x.Count() }).ToList();
            var listDriverLogs = listFilterAttendanceLog.Where(x => x.UserType == (short)EmployeeType.Driver && !x.IsExtraDriver).GroupBy(x => new { x.DepartmentIndex, x.DepartmentName }).Select(x => new EmergencyDepartmentCountModel { DepartmentIndex = x.Key.DepartmentIndex, DepartmentName = x.Key.DepartmentName, Count = x.Count() }).ToList();
            var listExtraDriverLogs = listFilterAttendanceLog.Where(x => x.UserType == (short)EmployeeType.Driver && x.IsExtraDriver).GroupBy(x => new { x.DepartmentIndex, x.DepartmentName }).Select(x => new EmergencyDepartmentCountModel { DepartmentIndex = x.Key.DepartmentIndex, DepartmentName = x.Key.DepartmentName, Count = x.Count() }).ToList();

            
            try
            {
                var latestEmergencyAttendance = new DateTime();
                var stringResult = GetLatestEmergencyAttendance();
                //_logger.LogInformation("GetEmergencyLog" + stringResult);
                if (!string.IsNullOrWhiteSpace(stringResult))
                {
                    var format = "dd/MM/yyyy HH:mm:ss";
                    if (DateTime.TryParseExact(stringResult, format,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out var time))
                    {
                        latestEmergencyAttendance = time;
                    }
                }

                var emergencyPrefixMachineName = _config?.EmergencyPrefixMachineName != null ? _config.EmergencyPrefixMachineName : string.Empty;
                //_logger.LogInformation("GetEmergencyLog" + emergencyPrefixMachineName);

                if (!string.IsNullOrWhiteSpace(emergencyPrefixMachineName))
                {
                    var listEmergencyMachine = await DbContext.IC_Device.AsNoTracking().Where(x
                        => x.CompanyIndex == user.CompanyIndex && x.AliasName.StartsWith(emergencyPrefixMachineName)).ToListAsync();
                    var listEmergencyMachineSerial = listEmergencyMachine.Select(x => x.SerialNumber).ToList();
                    var attendanceLogs1 = from att in DbContext.IC_AttendanceLog.Where(x
                                            => x.CompanyIndex == user.CompanyIndex
                                            && x.CheckTime > latestEmergencyAttendance
                                            && listEmergencyMachineSerial.Contains(x.SerialNumber))
                                          join u in DbContext.HR_User
                                          on att.EmployeeATID equals u.EmployeeATID
                                          //into logUserInfo
                                          //from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                          join cc in DbContext.HR_CustomerCard
                                          on att.EmployeeATID equals cc.CardID
                                          into logCustomerCardInfo
                                          from logCustomerCardResult in logCustomerCardInfo.DefaultIfEmpty()
                                          join t in DbContext.GC_TruckDriverLog.Where(x => !x.IsInactive)
                                          on logCustomerCardResult.CardNumber equals t.CardNumber
                                          into logTruckDriverInfo
                                          from logTruckDriverResult in logTruckDriverInfo.DefaultIfEmpty()
                                          join p in DbContext.IC_PlanDock
                                          on logTruckDriverResult.TripCode equals p.TripId
                                          into planDockInfo
                                          from planDockResult in planDockInfo.DefaultIfEmpty()
                                          join et in DbContext.GC_TruckExtraDriverLog.Where(x => !x.IsInactive)
                                          on logCustomerCardResult.CardNumber equals et.CardNumber
                                          into logExtraTruckDriverInfo
                                          from logExtraTruckDriverResult in logExtraTruckDriverInfo.DefaultIfEmpty()
                                          join ut in DbContext.HR_UserType
                                          on u.EmployeeType equals ut.UserTypeId
                                          into uutInfo
                                          from uutResult in uutInfo.DefaultIfEmpty()
                                          join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                             && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                                          on att.EmployeeATID equals wi.EmployeeATID
                                          into attInfo
                                          from attResult in attInfo.DefaultIfEmpty()
                                          join dp in DbContext.IC_Department
                                          on attResult.DepartmentIndex equals dp.Index into dep
                                          from depResult in dep.DefaultIfEmpty()
                                          join d in DbContext.IC_Device
                                          //.Where(x => x.DeviceModule != "ICMS")
                                          on att.SerialNumber equals d.SerialNumber
                                          into logDeviceInfo
                                          from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                          join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                          on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                          from empCard in employeeCard.DefaultIfEmpty()
                                          select new WorkingLogResult
                                          {
                                              EmployeeATID = (logCustomerCardResult != null
                                                 ? (logTruckDriverResult != null ? logTruckDriverResult.TripCode
                                                 : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.TripCode : att.EmployeeATID)) : att.EmployeeATID),
                                              EmployeeCode = u != null ? u.EmployeeCode : string.Empty,
                                              //FullName = logUserInfoResult != null ? logUserInfoResult.FullName : string.Empty,
                                              FullName = (planDockResult != null ? planDockResult.DriverName
                                                 : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.ExtraDriverName
                                                 : (u != null ? u.FullName : string.Empty))),
                                              UserType = u != null
                                                  ? (u.EmployeeType.HasValue
                                                  ? u.EmployeeType.Value : (short)EmployeeType.Employee)
                                                  : ((logCustomerCardResult != null && (logTruckDriverResult != null || logExtraTruckDriverResult != null))
                                                  ? (short)EmployeeType.Driver : (short)EmployeeType.Employee),
                                              IsExtraDriver = (u == null && logCustomerCardResult != null
                                                 && logTruckDriverResult == null && logExtraTruckDriverResult != null),
                                              UserTypeName = uutResult != null ? uutResult.Name
                                                 : (logCustomerCardResult != null
                                                 ? (logTruckDriverResult != null ? "Tài xế"
                                                 : (logExtraTruckDriverResult != null ? "Phụ xế" : "Nhân viên")) : "Nhân viên"),
                                              DepartmentIndex = attResult != null ? attResult.DepartmentIndex : 0,
                                              RootDepartment = (depResult == null || (depResult != null && (!depResult.ParentIndex.HasValue
                                                 || (depResult.ParentIndex.HasValue && (depResult.ParentIndex.Value == 0 || depResult.ParentIndex.Value == depResult.Index))))),
                                              DepartmentName = depResult != null ? depResult.Name : "NoDepartment",
                                              SerialNumber = att.SerialNumber,
                                              DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                              Time = att.CheckTime,
                                              InOutMode = att.InOutMode.GetInOutModeString(),
                                              VerifyMode = att.VerifyMode.GetVerifyModeString(),
                                              //FaceMask = att.FaceMask.GetFaceMaskString(),
                                              //BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                              //IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                              //CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                                              CardNumber = (logCustomerCardResult != null
                                                 ? (logTruckDriverResult != null ? logTruckDriverResult.CardNumber
                                                 : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.CardNumber
                                                 : (empCard != null ? empCard.CardNumber : string.Empty))) : (empCard != null ? empCard.CardNumber : string.Empty)),
                                              TimeString = att.CheckTime.ToString("dd/MM/yyy HH:mm:ss")
                                          };

                    var listAttendanceLog1 = await attendanceLogs1.ToListAsync();

                    var activeDriverLog = await DbContext.GC_TruckDriverLog.Where(x => !x.IsInactive && x.InOutMode == (short)InOutMode.Input)
                        .Select(x => x.CardNumber).ToListAsync();
                    var activeExtraDriverLog = await DbContext.GC_TruckExtraDriverLog.Where(x => !x.IsInactive)
                        .Select(x => x.CardNumber).ToListAsync();
                    var activeDriverCardNumber = new List<string>();
                    if (activeDriverLog.Count > 0)
                    {
                        activeDriverCardNumber.AddRange(activeDriverLog);
                    }
                    if (activeExtraDriverLog.Count > 0)
                    {
                        activeDriverCardNumber.AddRange(activeExtraDriverLog);
                    }
                    if (activeDriverCardNumber.Count > 0)
                    {
                        var activeDriverCardId = await DbContext.HR_CustomerCard.Where(x
                            => activeDriverCardNumber.Contains(x.CardNumber)).Select(x => x.CardID).ToListAsync();
                        if (activeDriverCardId.Count > 0)
                        {
                            var activeDriverAttendanceLogs = from att in DbContext.IC_AttendanceLog.Where(x
                                            => x.CompanyIndex == user.CompanyIndex
                                            && x.CheckTime > latestEmergencyAttendance
                                            && listEmergencyMachineSerial.Contains(x.SerialNumber)
                                            && activeDriverCardId.Contains(x.EmployeeATID))
                                                             join cc in DbContext.HR_CustomerCard
                                                             on att.EmployeeATID equals cc.CardID
                                                             into logCustomerCardInfo
                                                             from logCustomerCardResult in logCustomerCardInfo.DefaultIfEmpty()
                                                             join t in DbContext.GC_TruckDriverLog.Where(x => !x.IsInactive)
                                                             on logCustomerCardResult.CardNumber equals t.CardNumber
                                                             into logTruckDriverInfo
                                                             from logTruckDriverResult in logTruckDriverInfo.DefaultIfEmpty()
                                                             join p in DbContext.IC_PlanDock
                                                             on logTruckDriverResult.TripCode equals p.TripId
                                                             into planDockInfo
                                                             from planDockResult in planDockInfo.DefaultIfEmpty()
                                                             join et in DbContext.GC_TruckExtraDriverLog.Where(x => !x.IsInactive)
                                                             on logCustomerCardResult.CardNumber equals et.CardNumber
                                                             into logExtraTruckDriverInfo
                                                             from logExtraTruckDriverResult in logExtraTruckDriverInfo.DefaultIfEmpty()
                                                             join d in DbContext.IC_Device
                                                             //.Where(x => x.DeviceModule != "ICMS")
                                                             on att.SerialNumber equals d.SerialNumber
                                                             into logDeviceInfo
                                                             from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                                             join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                                             on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                                             from empCard in employeeCard.DefaultIfEmpty()
                                                             select new WorkingLogResult
                                                             {
                                                                 EmployeeATID = (logCustomerCardResult != null
                                                                    ? (logTruckDriverResult != null ? logTruckDriverResult.TripCode
                                                                    : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.TripCode : att.EmployeeATID)) : att.EmployeeATID),
                                                                 EmployeeCode = (logCustomerCardResult != null
                                                                    ? (logTruckDriverResult != null ? logTruckDriverResult.TripCode
                                                                    : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.TripCode : att.EmployeeATID)) : att.EmployeeATID),
                                                                 //FullName = logUserInfoResult != null ? logUserInfoResult.FullName : string.Empty,
                                                                 FullName = (planDockResult != null ? planDockResult.DriverName
                                                                    : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.ExtraDriverName
                                                                    : string.Empty)),
                                                                 UserType = (logCustomerCardResult != null && (logTruckDriverResult != null || logExtraTruckDriverResult != null))
                                                                     ? (short)EmployeeType.Driver : (short)EmployeeType.Employee,
                                                                 IsExtraDriver = (logCustomerCardResult != null
                                                                    && logTruckDriverResult == null && logExtraTruckDriverResult != null),
                                                                 UserTypeName = logCustomerCardResult != null
                                                                    ? (logTruckDriverResult != null ? "Tài xế"
                                                                    : (logExtraTruckDriverResult != null ? "Phụ xế" : "Nhân viên")) : "Nhân viên",
                                                                 SerialNumber = att.SerialNumber,
                                                                 DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                                                 Time = att.CheckTime,
                                                                 InOutMode = att.InOutMode.GetInOutModeString(),
                                                                 VerifyMode = att.VerifyMode.GetVerifyModeString(),
                                                                 //FaceMask = att.FaceMask.GetFaceMaskString(),
                                                                 //BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                                                 //IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                                                 //CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                                                                 CardNumber = (logCustomerCardResult != null
                                                                    ? (logTruckDriverResult != null ? logTruckDriverResult.CardNumber
                                                                    : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.CardNumber
                                                                    : (empCard != null ? empCard.CardNumber : string.Empty))) : (empCard != null ? empCard.CardNumber : string.Empty)),
                                                                 TimeString = att.CheckTime.ToString("dd/MM/yyy HH:mm:ss")
                                                             };
                            var resultActiveDriverAttendanceLogs = await activeDriverAttendanceLogs.ToListAsync();
                            if (resultActiveDriverAttendanceLogs.Count() > 0)
                            {
                                listAttendanceLog1.AddRange(resultActiveDriverAttendanceLogs);
                            }
                        }
                    }

                    if (rule != null && rule.PresenceTrackingTime > 0)
                    {

                        listAttendanceLog1 = listAttendanceLog1.Where(x
                            => now.Subtract(x.Time).TotalMinutes <= Convert.ToDouble(rule.PresenceTrackingTime * 60)).ToList();
                    }

                    listAttendanceLog1 = listAttendanceLog1.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                        => x.FirstOrDefault()).ToList();
                    listAttendanceLog1.RemoveAll(x => x == null);

                    listAttendanceLog1 = listAttendanceLog1.Where(x => x.DepartmentIndex == 0
                        || (user.ListDepartmentAssigned != null
                        && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();

                    var listCustomerCard1 = await _HR_CustomerCardService.GetAllCustomerCard(user);
                    if (listCustomerCard1 != null && listCustomerCard1.Count > 0)
                    {
                        var listUsingCustomerCard = listCustomerCard1.Where(x => x.Status).ToList();
                        var cardIdToUserCodeMap = listUsingCustomerCard.ToDictionary(card => card.CardID, card => card.UserCode);

                        listAttendanceLog1.ForEach(x =>
                        {
                            if (cardIdToUserCodeMap.ContainsKey(x.EmployeeATID))
                            {
                                x.EmployeeATID = cardIdToUserCodeMap[x.EmployeeATID];
                            }
                        });
                    }

                    var listEmergency = listAttendanceLog1.Where(x => x.UserType == (short)EmployeeType.Employee).GroupBy(x => new { x.DepartmentIndex, x.DepartmentName }).Select(x => new { DepartmentIndex = x.Key.DepartmentIndex, DepartmentName = x.Key.DepartmentName, Count = x.Count() }).ToList();
                    foreach (var item in listEmployeeLogs)
                    {
                        var count = listEmergency.FirstOrDefault(x => x.DepartmentIndex == item.DepartmentIndex );
                        var attendance = count != null ? count.Count : 0;
                        var emer = new EmergencyAndEvacuation()
                        {
                            DepartmentIndex = item.DepartmentIndex,
                            DepartmentName = item.DepartmentName,
                            InCom = item.Count,
                            Attendance = attendance,
                            Absent = item.Count - attendance
                        };
                        lstReturn.Add(emer);
                    }

                    var listOtherLogs = listCustomerLogs;
                    listOtherLogs.AddRange(listContractorLogs);
                    listOtherLogs.AddRange(listDriverLogs);
                    listOtherLogs.AddRange(listExtraDriverLogs);
                    listOtherLogs = listOtherLogs.GroupBy(x => new { x.DepartmentIndex, x.DepartmentName }).Select(x => new EmergencyDepartmentCountModel { DepartmentIndex = x.Key.DepartmentIndex, DepartmentName = x.Key.DepartmentName, Count = x.Select(y => y.Count).Sum() }).ToList();

                     listEmergency = listAttendanceLog1.Where(x => x.UserType != (short)EmployeeType.Employee).GroupBy(x => new { x.DepartmentIndex, x.DepartmentName }).Select(x => new { DepartmentIndex = x.Key.DepartmentIndex, DepartmentName = x.Key.DepartmentName, Count = x.Count() }).ToList();

                    foreach (var item in listOtherLogs)
                    {
                        var count = listEmergency.FirstOrDefault(x => x.DepartmentIndex == item.DepartmentIndex);
                        var attendance = count != null ? count.Count : 0;
                        var emer = new EmergencyAndEvacuation()
                        {
                            DepartmentIndex = item.DepartmentIndex,
                            DepartmentName = item.DepartmentName,
                            InCom = item.Count,
                            Attendance = attendance,
                            Absent = item.Count - attendance
                        };
                        lstReturn1.Add(emer);
                    }

                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmergencyLog: " + ex);
            }

            return new
            {
                EmployeeEmergencyList = lstReturn,
                OtherEmergencyList = lstReturn1
            };
        }



        public async Task<Tuple<object, object, object>> GetLogsByDoor()
        {
            var attendanceLogs = from att in DbContext.IC_AttendanceLog.Where(x
                                    => x.CheckTime.Date == DateTime.Now.Date)
                                 join u in DbContext.HR_User
                                 on att.EmployeeATID equals u.EmployeeATID
                                 into logUserInfo
                                 from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                 join d in DbContext.IC_Device
                                 on att.SerialNumber equals d.SerialNumber
                                 into logDeviceInfo
                                 from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                 join dad in DbContext.AC_DoorAndDevice
                                 on att.SerialNumber equals dad.SerialNumber
                                 into doorAndDevice
                                 from doorAndDeviceResult in doorAndDevice.DefaultIfEmpty()
                                 join door in DbContext.AC_Door
                                 on doorAndDeviceResult.DoorIndex equals door.Index
                                 into doorData
                                 from doorResult in doorData.DefaultIfEmpty()
                                 join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                 on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                 from empCard in employeeCard.DefaultIfEmpty()
                                 select new
                                 {
                                     EmployeeATID = att.EmployeeATID,
                                     EmployeeCode = logUserInfoResult != null ? logUserInfoResult.EmployeeCode : string.Empty,
                                     FullName = logUserInfoResult != null ? logUserInfoResult.FullName : string.Empty,
                                     SerialNumber = att.SerialNumber,
                                     DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                     DoorIndex = doorAndDeviceResult != null ? doorAndDeviceResult.DoorIndex : 0,
                                     DoorName = doorResult != null ? doorResult.Name : string.Empty,
                                     Time = att.CheckTime,
                                     InOutMode = att.InOutMode.GetInOutModeString(),
                                     VerifyMode = att.VerifyMode.GetVerifyModeString(),
                                     FaceMask = att.FaceMask.GetFaceMaskString(),
                                     BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                     IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                     CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                                     TimeString = att.CheckTime.ToString("dd/MM/yyy HH:mm:ss")
                                 };
            var listAttendanceLog = await attendanceLogs.ToListAsync();

            var listInLogs = listAttendanceLog.Where(x => x.InOutMode == "In" || x.InOutMode == "BreakIn").ToList();
            var listOutLogs = listAttendanceLog.Where(x => x.InOutMode == "Out" || x.InOutMode == "BreakOut").ToList();
            var listRemainInLogs = new List<object>();

            var listSerial = listInLogs.Select(x => x.SerialNumber).ToList();
            listSerial.AddRange(listOutLogs.Select(x => x.SerialNumber).ToList());
            listSerial = listSerial.Distinct().ToList();

            var ids = listInLogs.Select(a => a.EmployeeATID).Union(listOutLogs.Select(b => b.EmployeeATID)).Distinct().ToList();

            foreach (var id in ids)
            {
                foreach (var serial in listSerial)
                {
                    var inLogs = listInLogs.Where(a => a.EmployeeATID == id && a.SerialNumber == serial).ToList();
                    var outLog = listOutLogs.Where(b => b.EmployeeATID == id && b.SerialNumber == serial).OrderByDescending(a => a.Time).FirstOrDefault();

                    if (outLog != null)
                    {
                        foreach (var inLog in inLogs)
                        {
                            if (inLog.Time > outLog.Time)
                            {
                                listRemainInLogs.Add(inLog);
                            }
                        }
                    }
                    else
                    {
                        listRemainInLogs.AddRange(inLogs);
                    }
                }
            }

            listInLogs = listInLogs.OrderByDescending(x => x.Time).ToList();
            listOutLogs = listOutLogs.OrderByDescending(x => x.Time).ToList();
            listRemainInLogs = (List<object>)listRemainInLogs.OrderByDescending(x => x.TryGetValue<DateTime>("Time")).ToList();

            return new Tuple<object, object, object>(listInLogs, listOutLogs, listRemainInLogs);
        }

        public async Task<List<WorkingLogResult>> GetWorkingEmployeeByDepartment()
        {
            try
            {
                var attendanceLogs = from att in DbContext.IC_AttendanceLog.Where(x
                        => x.CheckTime.Date == DateTime.Now.Date)
                                     join u in DbContext.HR_User
                                     on att.EmployeeATID equals u.EmployeeATID
                                     into logUserInfo
                                     from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                     join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                     && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                                    on att.EmployeeATID equals wi.EmployeeATID
                                     join dp in DbContext.IC_Department
                                    on wi.DepartmentIndex equals dp.Index into dep
                                     from depResult in dep.DefaultIfEmpty()
                                     join d in DbContext.IC_Device.Where(x => x.DeviceModule != "ICMS")
                                     on att.SerialNumber equals d.SerialNumber
                                     into logDeviceInfo
                                     from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                     join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                     on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                     from empCard in employeeCard.DefaultIfEmpty()
                                     select new WorkingLogResult
                                     {
                                         EmployeeATID = att.EmployeeATID,
                                         EmployeeCode = logUserInfoResult != null ? logUserInfoResult.EmployeeCode : string.Empty,
                                         FullName = logUserInfoResult != null ? logUserInfoResult.FullName : string.Empty,
                                         DepartmentIndex = wi.DepartmentIndex,
                                         RootDepartment = (depResult == null || (depResult != null && (!depResult.ParentIndex.HasValue
                                            || (depResult.ParentIndex.HasValue && (depResult.ParentIndex.Value == 0 || depResult.ParentIndex.Value == depResult.Index))))),
                                         DepartmentName = depResult != null ? depResult.Name : "NoDepartment",
                                         SerialNumber = att.SerialNumber,
                                         DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                         Time = att.CheckTime,
                                         InOutMode = att.InOutMode.GetInOutModeString(),
                                         VerifyMode = att.VerifyMode.GetVerifyModeString(),
                                         FaceMask = att.FaceMask.GetFaceMaskString(),
                                         BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                         IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                         CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                                         TimeString = att.CheckTime.ToString("dd/MM/yyy HH:mm:ss")
                                     };

                var listAttendanceLog = await attendanceLogs.ToListAsync();
                if (listAttendanceLog != null && listAttendanceLog.Count > 0)
                {
                    listAttendanceLog = listAttendanceLog.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                        => x.FirstOrDefault(y => y.InOutMode == "In" || y.InOutMode == "BreakIn")).ToList();
                    listAttendanceLog.RemoveAll(x => x == null);
                }

                return listAttendanceLog;
            }
            catch (Exception ex)
            {
                return new List<WorkingLogResult>();
            }
        }

        public async Task<Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>>
            GetTupleFullVehicleEmployeeByDepartment(UserInfo user)
        {
            //==== CURRENTLY ONLY GET LOG FRON INTEGRATE LOG, WE NEED TO GET LOG FROM GC_TIMELOG TOO (LOG OF MONITORING)====
            var attendanceLogs = from att in DbContext.IC_VehicleLog.Where(x
                                    => (x.FromDate.HasValue && x.FromDate.Value.Date == DateTime.Now.Date)
                                    || x.ToDate.HasValue && x.ToDate.Value.Date == DateTime.Now.Date)
                                     //join ev in DbContext.GC_EmployeeVehicle
                                     //on att.EmployeeATID equals ev.EmployeeATID
                                 join u in DbContext.HR_User
                                 on att.EmployeeATID equals u.EmployeeATID
                                 into logUserInfo
                                 from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                 join ut in DbContext.HR_UserType
                                 on logUserInfoResult.EmployeeType equals ut.UserTypeId into uutInfo
                                 from uutResult in uutInfo.DefaultIfEmpty()
                                 join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                 && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                                 on att.EmployeeATID equals wi.EmployeeATID
                                 into attInfo
                                 from attResult in attInfo.DefaultIfEmpty()
                                 join dp in DbContext.IC_Department
                                 on attResult.DepartmentIndex equals dp.Index into dep
                                 from depResult in dep.DefaultIfEmpty()
                                     //join d in DbContext.IC_Device.Where(x => x.DeviceModule != "ICMS")
                                     //on att.MachineSerial equals d.SerialNumber
                                     //into logDeviceInfo
                                     //from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                 join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                 on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                 from empCard in employeeCard.DefaultIfEmpty()
                                 select new WorkingLogResult
                                 {
                                     EmployeeATID = att.EmployeeATID,
                                     EmployeeCode = logUserInfoResult != null ? logUserInfoResult.EmployeeCode : string.Empty,
                                     FullName = logUserInfoResult != null ? logUserInfoResult.FullName : string.Empty,
                                     UserType = logUserInfoResult != null
                                         ? (logUserInfoResult.EmployeeType.HasValue
                                         ? logUserInfoResult.EmployeeType.Value : (short)EmployeeType.Employee)
                                         : (short)EmployeeType.Employee,
                                     UserTypeName = uutResult != null ? uutResult.Name : "Nhân viên",
                                     DepartmentIndex = attResult != null ? attResult.DepartmentIndex : 0,
                                     RootDepartment = (depResult == null || (depResult != null && (!depResult.ParentIndex.HasValue
                                        || (depResult.ParentIndex.HasValue && (depResult.ParentIndex.Value == 0 || depResult.ParentIndex.Value == depResult.Index))))),
                                     DepartmentName = depResult != null ? depResult.Name : "NoDepartment",
                                     FromDate = att.FromDate,
                                     ToDate = att.ToDate,
                                     Plate = att.Plate,
                                     //SerialNumber = att.MachineSerial,
                                     //DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                     //Time = att.Time,
                                     //InOutMode = att.InOutMode.Value.GetGCSInOutModeString(),
                                     //VerifyMode = att.VerifyMode,
                                     //FaceMask = att.FaceMask.GetFaceMaskString(),
                                     //BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                     //IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                     CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                                     //TimeString = att.Time.ToString("dd/MM/yyy HH:mm:ss")
                                 };

            var listAttendanceLog = await attendanceLogs.ToListAsync();

            //var listInAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "In" || x.InOutMode == "BreakIn").ToList();
            //var listOutAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "Out" || x.InOutMode == "BreakOut").ToList();
            var listInAttendanceLog = new List<WorkingLogResult>();
            var listOutAttendanceLog = new List<WorkingLogResult>();

            listAttendanceLog.ForEach(x =>
            {
                if (x.FromDate.HasValue && x.FromDate.Value.Date == DateTime.Now.Date)
                {
                    var logIn = ObjectExtensions.CopyToNewObject(x);
                    logIn.InOutMode = "In";
                    logIn.Time = x.FromDate.Value;
                    logIn.TimeString = x.FromDate.Value.ToddMMyyyyHHmmss();
                    listInAttendanceLog.Add(logIn);
                }
                if (x.ToDate.HasValue && x.ToDate.Value.Date == DateTime.Now.Date)
                {
                    var logOut = ObjectExtensions.CopyToNewObject(x);
                    logOut.InOutMode = "Out";
                    logOut.Time = x.ToDate.Value;
                    logOut.TimeString = x.ToDate.Value.ToddMMyyyyHHmmss();
                    listOutAttendanceLog.Add(logOut);
                }
            });

            listInAttendanceLog = listInAttendanceLog.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                => x.FirstOrDefault()).ToList();
            listInAttendanceLog.RemoveAll(x => x == null);
            listOutAttendanceLog = listOutAttendanceLog.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                => x.FirstOrDefault()).ToList();
            listOutAttendanceLog.RemoveAll(x => x == null);

            var listFilterAttendanceLog = new List<WorkingLogResult>();
            listFilterAttendanceLog.AddRange(listInAttendanceLog);
            listFilterAttendanceLog.AddRange(listOutAttendanceLog);

            var listLogEmployeeATID = listFilterAttendanceLog.Select(x => x.EmployeeATID).Distinct().ToList();
            var listEmployeeVehicle = new List<GC_EmployeeVehicle>();
            if (listLogEmployeeATID != null && listLogEmployeeATID.Count > 0)
            {
                listEmployeeVehicle = await DbContext.GC_EmployeeVehicle.AsNoTracking().Where(x
                    => listLogEmployeeATID.Contains(x.EmployeeATID)).ToListAsync();
            }

            var logVehicle = new GC_EmployeeVehicle();
            listFilterAttendanceLog.ForEach(x =>
            {
                logVehicle = new GC_EmployeeVehicle();

                if (listEmployeeVehicle != null && listEmployeeVehicle.Count > 0)
                {
                    logVehicle = listEmployeeVehicle.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID
                        && y.FromDate.Date >= DateTime.Now.Date && (!y.ToDate.HasValue || (y.ToDate.HasValue
                        && y.ToDate.Value.Date <= DateTime.Now.Date)) && x.Plate == y.Plate);
                    if (logVehicle != null)
                    {
                        x.VehicleType = logVehicle.Type;
                        x.VehicleTypeName = logVehicle.Type.GetVehicleTypeName();
                        x.Plate = logVehicle.Plate;
                    }
                }
            });

            listFilterAttendanceLog = listFilterAttendanceLog.Where(x => x.VehicleType.HasValue).ToList();

            listFilterAttendanceLog = listFilterAttendanceLog.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null
                && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();

            var listLogIn = listFilterAttendanceLog.Where(x => x.InOutMode == "In" || x.InOutMode == "BreakIn").ToList();
            var listLogOut = listFilterAttendanceLog.Where(x => x.InOutMode == "Out" || x.InOutMode == "BreakOut").ToList();
            var listLogRemain = listLogIn.Where(x => !listLogOut.Any(y => y.EmployeeATID == x.EmployeeATID)
                || listLogOut.Any(y => y.EmployeeATID == x.EmployeeATID && y.Time < x.Time)).ToList();

            var result = new Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>(
                listLogIn, listLogOut, listLogRemain);

            return result;
        }

        public async Task<Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>>
            GetTupleFullWorkingEmployeeByDepartment(UserInfo user)
        {
            var attendanceLogs = from att in DbContext.GC_TimeLog.Where(x
                                    => x.Time.Date == DateTime.Now.Date)
                                 join u in DbContext.HR_User
                                 on att.EmployeeATID equals u.EmployeeATID
                                 into logUserInfo
                                 from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                 join ut in DbContext.HR_UserType
                                 on logUserInfoResult.EmployeeType equals ut.UserTypeId into uutInfo
                                 from uutResult in uutInfo.DefaultIfEmpty()
                                 join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                 && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                                 on att.EmployeeATID equals wi.EmployeeATID
                                 into attInfo
                                 from attResult in attInfo.DefaultIfEmpty()
                                 join dp in DbContext.IC_Department
                                 on attResult.DepartmentIndex equals dp.Index into dep
                                 from depResult in dep.DefaultIfEmpty()
                                 join d in DbContext.IC_Device.Where(x => x.DeviceModule != "ICMS")
                                 on att.MachineSerial equals d.SerialNumber
                                 into logDeviceInfo
                                 from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                 join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                 on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                 from empCard in employeeCard.DefaultIfEmpty()
                                 select new WorkingLogResult
                                 {
                                     EmployeeATID = att.EmployeeATID,
                                     EmployeeCode = logUserInfoResult != null ? logUserInfoResult.EmployeeCode : string.Empty,
                                     FullName = logUserInfoResult != null ? logUserInfoResult.FullName : string.Empty,
                                     UserType = logUserInfoResult != null
                                         ? (logUserInfoResult.EmployeeType.HasValue
                                         ? logUserInfoResult.EmployeeType.Value : (short)EmployeeType.Employee)
                                         : (short)EmployeeType.Employee,
                                     UserTypeName = uutResult != null ? uutResult.Name : "Nhân viên",
                                     DepartmentIndex = attResult != null ? attResult.DepartmentIndex : 0,
                                     RootDepartment = (depResult == null || (depResult != null && (!depResult.ParentIndex.HasValue
                                        || (depResult.ParentIndex.HasValue && (depResult.ParentIndex.Value == 0 || depResult.ParentIndex.Value == depResult.Index))))),
                                     DepartmentName = depResult != null ? depResult.Name : "NoDepartment",
                                     SerialNumber = att.MachineSerial,
                                     DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                     Time = att.Time,
                                     InOutMode = att.InOutMode.Value.GetGCSInOutModeString(),
                                     VerifyMode = att.VerifyMode,
                                     //FaceMask = att.FaceMask.GetFaceMaskString(),
                                     //BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                     //IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                     CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                                     TimeString = att.Time.ToString("dd/MM/yyy HH:mm:ss")
                                 };

            var listAttendanceLog = await attendanceLogs.ToListAsync();

            var listInAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "In" || x.InOutMode == "BreakIn").ToList();
            var listOutAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "Out" || x.InOutMode == "BreakOut").ToList();

            listInAttendanceLog = listInAttendanceLog.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                => x.FirstOrDefault()).ToList();
            listInAttendanceLog.RemoveAll(x => x == null);
            listOutAttendanceLog = listOutAttendanceLog.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                => x.FirstOrDefault()).ToList();
            listOutAttendanceLog.RemoveAll(x => x == null);

            var listFilterAttendanceLog = new List<WorkingLogResult>();
            listFilterAttendanceLog.AddRange(listInAttendanceLog);
            listFilterAttendanceLog.AddRange(listOutAttendanceLog);

            var listSerialNumber = new List<string>();
            if (listFilterAttendanceLog != null && listFilterAttendanceLog.Count > 0)
            {
                listSerialNumber = listFilterAttendanceLog.Select(x => x.SerialNumber).Distinct().ToList();
            }
            var listLineIndex = new List<int>();
            var listLineIn = new List<GC_Lines_CheckInDevice>();
            var listLineOut = new List<GC_Lines_CheckOutDevice>();
            if (listSerialNumber != null && listSerialNumber.Count > 0)
            {
                listLineIn = await DbContext.GC_Lines_CheckInDevice.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.CheckInDeviceSerial)).ToListAsync();
                listLineOut = await DbContext.GC_Lines_CheckOutDevice.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.CheckOutDeviceSerial)).ToListAsync();
                if (listLineIn != null && listLineIn.Count > 0)
                {
                    listLineIndex.AddRange(listLineIn.Select(x => x.LineIndex).ToList());
                }
                if (listLineOut != null && listLineOut.Count > 0)
                {
                    listLineIndex.AddRange(listLineOut.Select(x => x.LineIndex).ToList());
                }
            }

            var listGroupDeviceIndex = new List<int>();
            var listGroupDeviceDetails = new List<IC_GroupDeviceDetails>();
            if (listSerialNumber != null && listSerialNumber.Count > 0)
            {
                listGroupDeviceDetails = await DbContext.IC_GroupDeviceDetails.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.SerialNumber)).ToListAsync();
                if (listGroupDeviceDetails != null && listGroupDeviceDetails.Count > 0)
                {
                    listGroupDeviceIndex = listGroupDeviceDetails.Select(x => x.GroupDeviceIndex).Distinct().ToList();
                }
            }

            var listAreaGroupIndex = new List<int>();
            var listAreaGroupDetail = new List<GC_AreaGroup_GroupDevice>();
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listAreaGroupDetail = await DbContext.GC_AreaGroup_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.DeviceGroupIndex)).ToListAsync();
                if (listAreaGroupDetail != null && listAreaGroupDetail.Count > 0)
                {
                    listAreaGroupIndex = listAreaGroupDetail.Select(x => x.AreaGroupIndex).Distinct().ToList();
                }
            }

            var listGateIndex = new List<int>();
            var listGateLine = new List<GC_Gates_Lines>();
            if (listLineIndex != null && listLineIndex.Count > 0)
            {
                listGateLine = await DbContext.GC_Gates_Lines.AsNoTracking().Where(x
                    => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                if (listGateLine != null && listGateLine.Count > 0)
                {
                    listGateIndex = listGateLine.Select(x => x.GateIndex).Distinct().ToList();
                }
            }

            var listLine = new List<GC_Lines>();
            var listGate = new List<GC_Gates>();
            var listGroupDevice = new List<IC_GroupDevice>();
            var listAreaGroup = new List<GC_AreaGroup>();
            if (listLineIndex != null && listLineIndex.Count > 0)
            {
                listLine = await DbContext.GC_Lines.AsNoTracking().Where(x
                    => listLineIndex.Contains(x.Index)).ToListAsync();
            }
            if (listGateIndex != null && listGateIndex.Count > 0)
            {
                listGate = await DbContext.GC_Gates.AsNoTracking().Where(x
                    => listGateIndex.Contains(x.Index)).ToListAsync();
            }
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listGroupDevice = await DbContext.IC_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.Index)).ToListAsync();
            }
            if (listAreaGroupIndex != null && listAreaGroupIndex.Count > 0)
            {
                listAreaGroup = await DbContext.GC_AreaGroup.AsNoTracking().Where(x
                    => listAreaGroupIndex.Contains(x.Index)).ToListAsync();
            }

            var logLine = new GC_Lines();
            var logGate = new List<GC_Gates>();
            var logGroupDevice = new List<IC_GroupDevice>();
            var logAreaGroup = new List<GC_AreaGroup>();
            listFilterAttendanceLog.ForEach(x =>
            {
                logLine = new GC_Lines();
                logGate = new List<GC_Gates>();
                logGroupDevice = new List<IC_GroupDevice>();
                logAreaGroup = new List<GC_AreaGroup>();

                var logLineIn = listLineIn.FirstOrDefault(y => y.CheckInDeviceSerial == x.SerialNumber);
                var logLineOut = listLineOut.FirstOrDefault(y => y.CheckOutDeviceSerial == x.SerialNumber);
                var logLineIndex = 0;
                if (logLineIn != null)
                {
                    logLine = listLine.FirstOrDefault(y => y.Index == logLineIn.LineIndex);
                    logLineIndex = logLineIn.LineIndex;
                    x.LineIndex = logLine.Index;
                    x.LineName = logLine.Name;
                }
                else if (logLineOut != null)
                {
                    logLine = listLine.FirstOrDefault(y => y.Index == logLineOut.LineIndex);
                    logLineIndex = logLineOut.LineIndex;
                    x.LineIndex = logLine.Index;
                    x.LineName = logLine.Name;
                }
                var logGateLine = listGateLine.Where(y => y.LineIndex == logLineIndex).ToList();
                if (logGateLine != null && logGateLine.Count > 0)
                {
                    var gateIndex = logGateLine.Select(y => y.GateIndex).ToList();
                    logGate = listGate.Where(y => gateIndex.Contains(y.Index)).ToList();
                    x.GateIndex = logGate.Select(x => x.Index).ToList();
                    x.GateName = logGate.Select(x => x.Name).ToList();
                }

                var logGroupDeviceIndex = listGroupDeviceDetails.Where(y => y.SerialNumber == x.SerialNumber)
                    .Select(x => x.GroupDeviceIndex).ToList();
                if (logGroupDeviceIndex != null && logGroupDeviceIndex.Count > 0)
                {
                    x.GroupDeviceIndex = logGroupDeviceIndex;
                    logGroupDevice = listGroupDevice.Where(g => listAreaGroupIndex.Contains(g.Index)).ToList();
                    if (logGroupDevice != null && logGroupDevice.Count > 0)
                    {
                        x.GroupDeviceName = logGroupDevice.Select(x => x.Name).ToList();
                    }

                    var logAreaGroupIndex = listAreaGroupDetail.Where(y => logGroupDeviceIndex.Contains(y.DeviceGroupIndex))
                        .Select(x => x.AreaGroupIndex).ToList();
                    if (logAreaGroupIndex != null && logAreaGroupIndex.Count > 0)
                    {
                        x.AreaGroupIndex = logAreaGroupIndex;
                        logAreaGroup = listAreaGroup.Where(g => logAreaGroupIndex.Contains(g.Index)).ToList();
                        if (logAreaGroup != null && logAreaGroup.Count > 0)
                        {
                            x.AreaGroupName = logAreaGroup.Select(x => x.Name).ToList();
                        }
                    }
                }
            });

            listFilterAttendanceLog = listFilterAttendanceLog.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null
                && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();

            var listLogIn = listFilterAttendanceLog.Where(x => x.InOutMode == "In" || x.InOutMode == "BreakIn").ToList();
            var listLogOut = listFilterAttendanceLog.Where(x => x.InOutMode == "Out" || x.InOutMode == "BreakOut").ToList();
            var listLogRemain = listLogIn.Where(x => !listLogOut.Any(y => y.EmployeeATID == x.EmployeeATID)
                || listLogOut.Any(y => y.EmployeeATID == x.EmployeeATID && y.Time < x.Time)).ToList();

            var result = new Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>(
                listLogIn, listLogOut, listLogRemain);

            return result;
        }

        public async Task<List<WorkingLogResult>> GetFullWorkingEmployeeByDepartment(UserInfo user)
        {
            var attendanceLogs = from att in DbContext.IC_AttendanceLog.Where(x
                                    => x.CheckTime.Date == DateTime.Now.Date)
                                 join u in DbContext.HR_User
                                 on att.EmployeeATID equals u.EmployeeATID
                                 into logUserInfo
                                 from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                 join ut in DbContext.HR_UserType
                                 on logUserInfoResult.EmployeeType equals ut.UserTypeId into uutInfo
                                 from uutResult in uutInfo.DefaultIfEmpty()
                                 join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                 && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                                 on att.EmployeeATID equals wi.EmployeeATID
                                 join dp in DbContext.IC_Department
                                 on wi.DepartmentIndex equals dp.Index into dep
                                 from depResult in dep.DefaultIfEmpty()
                                 join d in DbContext.IC_Device.Where(x => x.DeviceModule != "ICMS")
                                 on att.SerialNumber equals d.SerialNumber
                                 into logDeviceInfo
                                 from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                 join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                 on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                 from empCard in employeeCard.DefaultIfEmpty()
                                 select new WorkingLogResult
                                 {
                                     EmployeeATID = att.EmployeeATID,
                                     EmployeeCode = logUserInfoResult != null ? logUserInfoResult.EmployeeCode : string.Empty,
                                     FullName = logUserInfoResult != null ? logUserInfoResult.FullName : string.Empty,
                                     UserType = logUserInfoResult != null
                                         ? (logUserInfoResult.EmployeeType.HasValue
                                         ? logUserInfoResult.EmployeeType.Value : (short)EmployeeType.Employee)
                                         : (short)EmployeeType.Employee,
                                     UserTypeName = uutResult != null ? uutResult.Name : "Nhân viên",
                                     DepartmentIndex = wi.DepartmentIndex,
                                     RootDepartment = (depResult == null || (depResult != null && (!depResult.ParentIndex.HasValue
                                        || (depResult.ParentIndex.HasValue && (depResult.ParentIndex.Value == 0 || depResult.ParentIndex.Value == depResult.Index))))),
                                     DepartmentName = depResult != null ? depResult.Name : "NoDepartment",
                                     SerialNumber = att.SerialNumber,
                                     DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                     Time = att.CheckTime,
                                     InOutMode = att.InOutMode.GetInOutModeString(),
                                     VerifyMode = att.VerifyMode.GetVerifyModeString(),
                                     FaceMask = att.FaceMask.GetFaceMaskString(),
                                     BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                     IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                     CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                                     TimeString = att.CheckTime.ToString("dd/MM/yyy HH:mm:ss")
                                 };

            var listAttendanceLog = await attendanceLogs.ToListAsync();

            var listInAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "In" || x.InOutMode == "BreakIn").ToList();
            var listOutAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "Out" || x.InOutMode == "BreakOut").ToList();

            listInAttendanceLog = listInAttendanceLog.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                => x.FirstOrDefault()).ToList();
            listInAttendanceLog.RemoveAll(x => x == null);
            listOutAttendanceLog = listOutAttendanceLog.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                => x.FirstOrDefault()).ToList();
            listOutAttendanceLog.RemoveAll(x => x == null);

            var listFilterAttendanceLog = new List<WorkingLogResult>();
            listFilterAttendanceLog.AddRange(listInAttendanceLog);
            listFilterAttendanceLog.AddRange(listOutAttendanceLog);

            var listSerialNumber = new List<string>();
            if (listFilterAttendanceLog != null && listFilterAttendanceLog.Count > 0)
            {
                listSerialNumber = listFilterAttendanceLog.Select(x => x.SerialNumber).Distinct().ToList();
            }
            var listGroupDeviceIndex = new List<int>();
            var listGroupDeviceDetails = new List<IC_GroupDeviceDetails>();
            if (listSerialNumber != null && listSerialNumber.Count > 0)
            {
                listGroupDeviceDetails = await DbContext.IC_GroupDeviceDetails.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.SerialNumber)).ToListAsync();
                if (listGroupDeviceDetails != null && listGroupDeviceDetails.Count > 0)
                {
                    listGroupDeviceIndex = listGroupDeviceDetails.Select(x => x.GroupDeviceIndex).Distinct().ToList();
                }
            }
            var listAreaGroupIndex = new List<int>();
            var listAreaGroupDetail = new List<GC_AreaGroup_GroupDevice>();
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listAreaGroupDetail = await DbContext.GC_AreaGroup_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.DeviceGroupIndex)).ToListAsync();
                if (listAreaGroupDetail != null && listAreaGroupDetail.Count > 0)
                {
                    listAreaGroupIndex = listAreaGroupDetail.Select(x => x.AreaGroupIndex).Distinct().ToList();
                }
            }

            var listGroupDevice = new List<IC_GroupDevice>();
            var listAreaGroup = new List<GC_AreaGroup>();
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listGroupDevice = await DbContext.IC_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.Index)).ToListAsync();
            }
            if (listAreaGroupIndex != null && listAreaGroupIndex.Count > 0)
            {
                listAreaGroup = await DbContext.GC_AreaGroup.AsNoTracking().Where(x
                    => listAreaGroupIndex.Contains(x.Index)).ToListAsync();
            }

            var logGroupDevice = new List<IC_GroupDevice>();
            var logAreaGroup = new List<GC_AreaGroup>();
            listFilterAttendanceLog.ForEach(x =>
            {
                logGroupDevice = new List<IC_GroupDevice>();
                logAreaGroup = new List<GC_AreaGroup>();

                var logGroupDeviceIndex = listGroupDeviceDetails.Where(y => y.SerialNumber == x.SerialNumber)
                    .Select(x => x.GroupDeviceIndex).ToList();
                if (logGroupDeviceIndex != null && logGroupDeviceIndex.Count > 0)
                {
                    x.GroupDeviceIndex = logGroupDeviceIndex;
                    logGroupDevice = listGroupDevice.Where(g => listAreaGroupIndex.Contains(g.Index)).ToList();
                    if (logGroupDevice != null && logGroupDevice.Count > 0)
                    {
                        x.GroupDeviceName = logGroupDevice.Select(x => x.Name).ToList();
                    }

                    var logAreaGroupIndex = listAreaGroupDetail.Where(y => logGroupDeviceIndex.Contains(y.DeviceGroupIndex))
                        .Select(x => x.AreaGroupIndex).ToList();
                    if (logAreaGroupIndex != null && logAreaGroupIndex.Count > 0)
                    {
                        x.AreaGroupIndex = logAreaGroupIndex;
                        logAreaGroup = listAreaGroup.Where(g => logAreaGroupIndex.Contains(g.Index)).ToList();
                        if (logAreaGroup != null && logAreaGroup.Count > 0)
                        {
                            x.AreaGroupName = logAreaGroup.Select(x => x.Name).ToList();
                        }
                    }
                }
            });

            listFilterAttendanceLog = listFilterAttendanceLog.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null
                && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();

            return listFilterAttendanceLog;
        }

        public void UpdateLatestEmergencyAttendance()
        {
            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/latestEmergencyAttendance.txt",
                DateTime.Now.ToddMMyyyyHHmmss());
        }

        public string GetLatestEmergencyAttendance()
        {
            if (!System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Files/latestEmergencyAttendance.txt"))
            {
                return string.Empty;
            }
            return System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/latestEmergencyAttendance.txt");
        }

        public async Task<Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>>
            GetIntegratedVehicleLog(UserInfo user)
        {
            var listGeneralRules = await DbContext.GC_Rules_General.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
            //var rule = listGeneralRules.Where(x => x.IsUsing).FirstOrDefault();
            var attendanceLogs = from att in DbContext.IC_VehicleLog
                                 .Where(x =>
                                    (!string.IsNullOrWhiteSpace(x.ComputerIn)
                                    || (!string.IsNullOrWhiteSpace(x.ComputerOut) && x.ComputerOut != "-1"))
                                    // string.IsNullOrWhiteSpace(x.Note)
                                    //|| (!string.IsNullOrWhiteSpace(x.Note) && !x.Note.Contains("MissingLogOutAfterExpiredPresenceTime"))
                                    )
                                 join u in DbContext.HR_User
                                    on att.EmployeeATID equals u.EmployeeATID
                                 //into logUserInfo
                                 //from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                 join ut in DbContext.HR_UserType
                                 on u.EmployeeType equals ut.UserTypeId
                                 into uutInfo
                                 from uutResult in uutInfo.DefaultIfEmpty()
                                 join wi in DbContext.IC_WorkingInfo.Where(x => x.Status == (short)TransferStatus.Approve && x.FromDate.Date <= DateTime.Now.Date
                                 && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                                 on att.EmployeeATID equals wi.EmployeeATID
                                 into attInfo
                                 from attResult in attInfo.DefaultIfEmpty()
                                 join dp in DbContext.IC_Department
                                 on attResult.DepartmentIndex equals dp.Index into dep
                                 from depResult in dep.DefaultIfEmpty()
                                     //join d in DbContext.IC_Device.Where(x => x.DeviceModule != "ICMS")
                                     //on att.MachineSerial equals d.SerialNumber
                                     //into logDeviceInfo
                                     //from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                 join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                 on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                 from empCard in employeeCard.DefaultIfEmpty()
                                 select new
                                 {
                                     EmployeeATID = att.EmployeeATID,
                                     EmployeeCode = u.EmployeeCode,
                                     FullName = u.FullName,
                                     FromDate = att.FromDate,
                                     ToDate = att.ToDate,
                                     UserType = u.EmployeeType,
                                     UserTypeName = uutResult != null ? uutResult.Name : "Nhân viên",
                                     DepartmentIndex = attResult != null ? attResult.DepartmentIndex : 0,
                                     RootDepartment = (depResult == null || (depResult != null && (!depResult.ParentIndex.HasValue
                                        || (depResult.ParentIndex.HasValue && (depResult.ParentIndex.Value == 0
                                        || depResult.ParentIndex.Value == depResult.Index))))),
                                     DepartmentName = depResult != null ? depResult.Name : "NoDepartment",
                                     Plate = att.Plate,
                                     ComputerIn = att.ComputerIn,
                                     ComputerOut = att.ComputerOut,
                                     CardNumber = att.CardNumber
                                 };
            //select new
            //{
            //    att,
            //    logUserInfoResult = u,
            //    depResult,
            //    attResult,
            //    uutResult
            //};

            //if (rule != null && rule.PresenceTrackingTime > 0)
            //{
            //    var now = DateTime.Now;
            //    attendanceLogs = attendanceLogs.Where(x
            //        => (!x.att.FromDate.HasValue || now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.att.FromDate.Value) ||
            //        (!x.att.ToDate.HasValue || now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.att.ToDate.Value));
            //}

            var listAttendanceLog = await attendanceLogs.ToListAsync();

            var listInAttendanceLogs = new List<WorkingLogResult>();
            listInAttendanceLogs = listAttendanceLog.Select(x =>
            {
                if (x.FromDate.HasValue)
                {
                    return new WorkingLogResult
                    {
                        EmployeeATID = x.EmployeeATID,
                        EmployeeCode = x != null ? x.EmployeeCode : string.Empty,
                        FullName = x.FullName,
                        UserType = x.UserType.HasValue ? x.UserType.Value : (short)EmployeeType.Employee,
                        UserTypeName = x.UserTypeName,
                        DepartmentIndex = x.DepartmentIndex,
                        RootDepartment = x.RootDepartment,
                        DepartmentName = x.DepartmentName,
                        Plate = x.Plate,
                        GateName = new List<string> { x.ComputerIn },
                        SerialNumber = x.ComputerIn,
                        DeviceName = x.ComputerIn,
                        //DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                        Time = x.FromDate.Value,
                        InOutMode = "In",
                        CardNumber = x.CardNumber,
                        TimeString = x.FromDate.Value.ToString("dd/MM/yyy HH:mm:ss")
                    };
                }
                return null;
            }).ToList();
            listInAttendanceLogs = listInAttendanceLogs.Where(x => x != null).ToList();

            var listOutAttendanceLogs = new List<WorkingLogResult>();
            listOutAttendanceLogs = listAttendanceLog.Select(x =>
            {
                if (x.ToDate.HasValue)
                {
                    return new WorkingLogResult
                    {
                        EmployeeATID = x.EmployeeATID,
                        EmployeeCode = x != null ? x.EmployeeCode : string.Empty,
                        FullName = x.FullName,
                        UserType = x.UserType.HasValue ? x.UserType.Value : (short)EmployeeType.Employee,
                        UserTypeName = x.UserTypeName,
                        DepartmentIndex = x.DepartmentIndex,
                        RootDepartment = x.RootDepartment,
                        DepartmentName = x.DepartmentName,
                        Plate = x.Plate,
                        GateName = new List<string> { x.ComputerOut },
                        SerialNumber = x.ComputerOut,
                        DeviceName = x.ComputerOut,
                        //DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                        Time = x.ToDate.Value,
                        InOutMode = "Out",
                        //VerifyMode = att.VerifyMode,
                        //FaceMask = att.FaceMask.GetFaceMaskString(),
                        //BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                        //IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                        CardNumber = x.CardNumber,
                        TimeString = x.ToDate.Value.ToString("dd/MM/yyy HH:mm:ss")
                    };
                }
                return null;
            }).ToList();
            listOutAttendanceLogs = listOutAttendanceLogs.Where(x => x != null).ToList();

            listInAttendanceLogs = listInAttendanceLogs.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null
                && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();
            listInAttendanceLogs = listInAttendanceLogs.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                    => x.FirstOrDefault()).ToList();
            listInAttendanceLogs = listInAttendanceLogs.Where(x => x != null).ToList();

            listOutAttendanceLogs = listOutAttendanceLogs.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null
                && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();
            listOutAttendanceLogs = listOutAttendanceLogs.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                    => x.FirstOrDefault()).ToList();
            listOutAttendanceLogs = listOutAttendanceLogs.Where(x => x != null).ToList();

            var listInNotOutLogs = listInAttendanceLogs.Where(x
                 => !listOutAttendanceLogs.Any(y => y.EmployeeATID == x.EmployeeATID && y.Time >= x.Time)).ToList();

            listInAttendanceLogs = listInAttendanceLogs.Where(x => x.Time.Date == DateTime.Now.Date).ToList();
            listOutAttendanceLogs = listOutAttendanceLogs.Where(x => x.Time.Date == DateTime.Now.Date).ToList();

            //if (rule != null && rule.PresenceTrackingTime > 0)
            //{
            //    var now = DateTime.Now;
            //    listInNotOutLogs = listInNotOutLogs.Where(x
            //        => now.Subtract(x.Time).TotalMinutes <= Convert.ToDouble(rule.PresenceTrackingTime * 60)).ToList();
            //}

            var listLogToday = new List<WorkingLogResult>();
            listLogToday.AddRange(listInAttendanceLogs);
            listLogToday.AddRange(listOutAttendanceLogs);

            var listEmployeeLogs = listLogToday.Where(x => x.UserType == (short)EmployeeType.Employee).ToList();
            var listCustomerLogs = listLogToday.Where(x => x.UserType == (short)EmployeeType.Guest).ToList();
            var listContractorLogs = listLogToday.Where(x => x.UserType == (short)EmployeeType.Contractor).ToList();



            var result = new Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>(
                listEmployeeLogs, listCustomerLogs, listContractorLogs, listInNotOutLogs);

            return result;
        }

        public async Task<Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>>
            GetTruckDriverLog(UserInfo user)
        {
            var company = await DbContext.IC_Company.AsNoTracking().FirstOrDefaultAsync(x => x.Index == user.CompanyIndex);
            var listGeneralRules = await DbContext.GC_Rules_General.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
            var rule = listGeneralRules.Where(x => x.IsUsing).FirstOrDefault();
            var attendanceLogs = from att in DbContext.GC_TruckDriverLog.Where(x
                                    => x.CompanyIndex == user.CompanyIndex
                                    //&& (string.IsNullOrWhiteSpace(x.Note)
                                    //|| (!string.IsNullOrWhiteSpace(x.Note) && !x.Note.Contains("MissingLogOutAfterExpiredPresenceTime"))
                                    //)
                                    )
                                 join u in DbContext.IC_PlanDock
                                 on att.TripCode equals u.TripId
                                 into logUserInfo
                                 from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                 join cc in DbContext.HR_CustomerCard
                                 on att.TripCode equals cc.CardID
                                 into logCustomerCardInfo
                                 from logCustomerCardResult in logCustomerCardInfo.DefaultIfEmpty()
                                 join d in DbContext.IC_Device
                                 .Where(x => x.DeviceModule != "ICMS")
                                 on att.MachineSerial equals d.SerialNumber
                                 into logDeviceInfo
                                 from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                 select new WorkingLogResult
                                 {
                                     EmployeeATID = att.TripCode,
                                     EmployeeCode = logUserInfoResult != null ? logUserInfoResult.DriverCode : string.Empty,
                                     FullName = logUserInfoResult != null ? logUserInfoResult.DriverName : string.Empty,
                                     UserType = (short)EmployeeType.Driver,
                                     UserTypeName = "Tài xế",
                                     SerialNumber = att.MachineSerial,
                                     DepartmentName = company != null ? company.Name : string.Empty,
                                     Plate = logUserInfoResult.TrailerNumber,
                                     DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                     Time = att.Time,
                                     InOutMode = att.InOutMode.HasValue ? att.InOutMode.Value.GetInOutModeString() : "In",
                                     VerifyMode = "Card",
                                     CardNumber = att.CardNumber,
                                     TimeString = att.Time.ToString("dd/MM/yyy HH:mm:ss")
                                 };

            ////if (rule != null && rule.PresenceTrackingTime > 0)
            ////{
            ////    var now = DateTime.Now;
            ////    attendanceLogs = attendanceLogs.Where(x
            ////        => now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.Time);
            ////}

            var listAttendanceLog = await attendanceLogs.ToListAsync();

            //if (rule != null && rule.PresenceTrackingTime > 0)
            //{
            //    var now = DateTime.Now;
            //    listAttendanceLog = listAttendanceLog.Where(x
            //        => now.Subtract(x.Time).TotalMinutes <= Convert.ToDouble(rule.PresenceTrackingTime * 60)).ToList();
            //}

            listAttendanceLog = listAttendanceLog.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null
                && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();

            var listSerialNumber = new List<string>();
            if (listAttendanceLog != null && listAttendanceLog.Count > 0)
            {
                listSerialNumber = listAttendanceLog.Select(x => x.SerialNumber).Distinct().ToList();
            }
            var listLineIndex = new List<int>();
            var listLineIn = new List<GC_Lines_CheckInDevice>();
            var listLineOut = new List<GC_Lines_CheckOutDevice>();
            if (listSerialNumber != null && listSerialNumber.Count > 0)
            {
                listLineIn = await DbContext.GC_Lines_CheckInDevice.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.CheckInDeviceSerial)).ToListAsync();
                listLineOut = await DbContext.GC_Lines_CheckOutDevice.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.CheckOutDeviceSerial)).ToListAsync();
                if (listLineIn != null && listLineIn.Count > 0)
                {
                    listLineIndex.AddRange(listLineIn.Select(x => x.LineIndex).ToList());
                }
                if (listLineOut != null && listLineOut.Count > 0)
                {
                    listLineIndex.AddRange(listLineOut.Select(x => x.LineIndex).ToList());
                }
            }

            var listGroupDeviceIndex = new List<int>();
            var listGroupDeviceDetails = new List<IC_GroupDeviceDetails>();
            if (listSerialNumber != null && listSerialNumber.Count > 0)
            {
                listGroupDeviceDetails = await DbContext.IC_GroupDeviceDetails.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.SerialNumber)).ToListAsync();
                if (listGroupDeviceDetails != null && listGroupDeviceDetails.Count > 0)
                {
                    listGroupDeviceIndex = listGroupDeviceDetails.Select(x => x.GroupDeviceIndex).Distinct().ToList();
                }
            }

            var listAreaGroupIndex = new List<int>();
            var listAreaGroupDetail = new List<GC_AreaGroup_GroupDevice>();
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listAreaGroupDetail = await DbContext.GC_AreaGroup_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.DeviceGroupIndex)).ToListAsync();
                if (listAreaGroupDetail != null && listAreaGroupDetail.Count > 0)
                {
                    listAreaGroupIndex = listAreaGroupDetail.Select(x => x.AreaGroupIndex).Distinct().ToList();
                }
            }

            var listGateIndex = new List<int>();
            var listGateLine = new List<GC_Gates_Lines>();
            if (listLineIndex != null && listLineIndex.Count > 0)
            {
                listGateLine = await DbContext.GC_Gates_Lines.AsNoTracking().Where(x
                    => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                if (listGateLine != null && listGateLine.Count > 0)
                {
                    listGateIndex = listGateLine.Select(x => x.GateIndex).Distinct().ToList();
                }
            }

            var listLine = new List<GC_Lines>();
            var listGate = new List<GC_Gates>();
            var listGroupDevice = new List<IC_GroupDevice>();
            var listAreaGroup = new List<GC_AreaGroup>();
            if (listLineIndex != null && listLineIndex.Count > 0)
            {
                listLine = await DbContext.GC_Lines.AsNoTracking().Where(x
                    => listLineIndex.Contains(x.Index)).ToListAsync();
            }
            if (listGateIndex != null && listGateIndex.Count > 0)
            {
                listGate = await DbContext.GC_Gates.AsNoTracking().Where(x
                    => listGateIndex.Contains(x.Index)).ToListAsync();
            }
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listGroupDevice = await DbContext.IC_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.Index)).ToListAsync();
            }
            if (listAreaGroupIndex != null && listAreaGroupIndex.Count > 0)
            {
                listAreaGroup = await DbContext.GC_AreaGroup.AsNoTracking().Where(x
                    => listAreaGroupIndex.Contains(x.Index)).ToListAsync();
            }

            var logLine = new GC_Lines();
            var logGate = new List<GC_Gates>();
            var logGroupDevice = new List<IC_GroupDevice>();
            var logAreaGroup = new List<GC_AreaGroup>();
            listAttendanceLog.ForEach(x =>
            {
                logLine = new GC_Lines();
                logGate = new List<GC_Gates>();
                logGroupDevice = new List<IC_GroupDevice>();
                logAreaGroup = new List<GC_AreaGroup>();

                var logLineIn = listLineIn.FirstOrDefault(y => y.CheckInDeviceSerial == x.SerialNumber);
                var logLineOut = listLineOut.FirstOrDefault(y => y.CheckOutDeviceSerial == x.SerialNumber);
                var logLineIndex = 0;
                if (logLineIn != null)
                {
                    logLine = listLine.FirstOrDefault(y => y.Index == logLineIn.LineIndex);
                    logLineIndex = logLineIn.LineIndex;
                    x.LineIndex = logLine.Index;
                    x.LineName = logLine.Name;
                }
                else if (logLineOut != null)
                {
                    logLine = listLine.FirstOrDefault(y => y.Index == logLineOut.LineIndex);
                    logLineIndex = logLineOut.LineIndex;
                    x.LineIndex = logLine.Index;
                    x.LineName = logLine.Name;
                }
                var logGateLine = listGateLine.Where(y => y.LineIndex == logLineIndex).ToList();
                if (logGateLine != null && logGateLine.Count > 0)
                {
                    var gateIndex = logGateLine.Select(y => y.GateIndex).ToList();
                    logGate = listGate.Where(y => gateIndex.Contains(y.Index)).ToList();
                    x.GateIndex = logGate.Select(x => x.Index).ToList();
                    x.GateName = logGate.Select(x => x.Name).ToList();
                }

                var logGroupDeviceIndex = listGroupDeviceDetails.Where(y => y.SerialNumber == x.SerialNumber)
                    .Select(x => x.GroupDeviceIndex).ToList();
                if (logGroupDeviceIndex != null && logGroupDeviceIndex.Count > 0)
                {
                    x.GroupDeviceIndex = logGroupDeviceIndex;
                    logGroupDevice = listGroupDevice.Where(g => listAreaGroupIndex.Contains(g.Index)).ToList();
                    if (logGroupDevice != null && logGroupDevice.Count > 0)
                    {
                        x.GroupDeviceName = logGroupDevice.Select(x => x.Name).ToList();
                    }

                    var logAreaGroupIndex = listAreaGroupDetail.Where(y => logGroupDeviceIndex.Contains(y.DeviceGroupIndex))
                        .Select(x => x.AreaGroupIndex).ToList();
                    if (logAreaGroupIndex != null && logAreaGroupIndex.Count > 0)
                    {
                        x.AreaGroupIndex = logAreaGroupIndex;
                        logAreaGroup = listAreaGroup.Where(g => logAreaGroupIndex.Contains(g.Index)).ToList();
                        if (logAreaGroup != null && logAreaGroup.Count > 0)
                        {
                            x.AreaGroupName = logAreaGroup.Select(x => x.Name).ToList();
                        }
                    }
                }
            });

            var listInAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "In" || x.InOutMode == "BreakIn").ToList();
            var listOutAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "Out" || x.InOutMode == "BreakOut").ToList();

            var listLogRemain = listInAttendanceLog.Where(x => !listOutAttendanceLog.Any(y => y.EmployeeATID == x.EmployeeATID)).ToList();

            var listTodayInLog = listInAttendanceLog.Where(x => x.Time.Date == DateTime.Now.Date).ToList();
            var listTodayOutLog = listOutAttendanceLog.Where(x => x.Time.Date == DateTime.Now.Date).ToList();

            var result = new Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>(
                listAttendanceLog, listTodayInLog, listTodayOutLog, listLogRemain);
            return result;
        }

        public async Task<List<WorkingLogResult>>
            GetEmergencyLog(UserInfo user)
        {
            try
            {
                var latestEmergencyAttendance = new DateTime();
                var stringResult = GetLatestEmergencyAttendance();
                //_logger.LogInformation("GetEmergencyLog" + stringResult);
                if (!string.IsNullOrWhiteSpace(stringResult))
                {
                    var format = "dd/MM/yyyy HH:mm:ss";
                    if (DateTime.TryParseExact(stringResult, format,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out var time))
                    {
                        latestEmergencyAttendance = time;
                    }
                }
                var listGeneralRules = await DbContext.GC_Rules_General.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
                var rule = listGeneralRules.Where(x => x.IsUsing).FirstOrDefault();

                var emergencyPrefixMachineName = _config?.EmergencyPrefixMachineName != null ? _config.EmergencyPrefixMachineName : string.Empty;
                //_logger.LogInformation("GetEmergencyLog" + emergencyPrefixMachineName);

                if (!string.IsNullOrWhiteSpace(emergencyPrefixMachineName))
                {
                    var listEmergencyMachine = await DbContext.IC_Device.AsNoTracking().Where(x
                        => x.CompanyIndex == user.CompanyIndex && x.AliasName.StartsWith(emergencyPrefixMachineName)).ToListAsync();
                    var listEmergencyMachineSerial = listEmergencyMachine.Select(x => x.SerialNumber).ToList();
                    var attendanceLogs = from att in DbContext.IC_AttendanceLog.Where(x
                                            => x.CompanyIndex == user.CompanyIndex
                                            && x.CheckTime > latestEmergencyAttendance
                                            && listEmergencyMachineSerial.Contains(x.SerialNumber))
                                         join u in DbContext.HR_User
                                         on att.EmployeeATID equals u.EmployeeATID
                                         //into logUserInfo
                                         //from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                         join cc in DbContext.HR_CustomerCard
                                         on att.EmployeeATID equals cc.CardID
                                         into logCustomerCardInfo
                                         from logCustomerCardResult in logCustomerCardInfo.DefaultIfEmpty()
                                         join t in DbContext.GC_TruckDriverLog.Where(x => !x.IsInactive)
                                         on logCustomerCardResult.CardNumber equals t.CardNumber
                                         into logTruckDriverInfo
                                         from logTruckDriverResult in logTruckDriverInfo.DefaultIfEmpty()
                                         join p in DbContext.IC_PlanDock
                                         on logTruckDriverResult.TripCode equals p.TripId
                                         into planDockInfo
                                         from planDockResult in planDockInfo.DefaultIfEmpty()
                                         join et in DbContext.GC_TruckExtraDriverLog.Where(x => !x.IsInactive)
                                         on logCustomerCardResult.CardNumber equals et.CardNumber
                                         into logExtraTruckDriverInfo
                                         from logExtraTruckDriverResult in logExtraTruckDriverInfo.DefaultIfEmpty()
                                         join ut in DbContext.HR_UserType
                                         on u.EmployeeType equals ut.UserTypeId
                                         into uutInfo
                                         from uutResult in uutInfo.DefaultIfEmpty()
                                         join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                            && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                                         on att.EmployeeATID equals wi.EmployeeATID
                                         into attInfo
                                         from attResult in attInfo.DefaultIfEmpty()
                                         join dp in DbContext.IC_Department
                                         on attResult.DepartmentIndex equals dp.Index into dep
                                         from depResult in dep.DefaultIfEmpty()
                                         join d in DbContext.IC_Device
                                         //.Where(x => x.DeviceModule != "ICMS")
                                         on att.SerialNumber equals d.SerialNumber
                                         into logDeviceInfo
                                         from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                         join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                         on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                         from empCard in employeeCard.DefaultIfEmpty()
                                         select new WorkingLogResult
                                         {
                                             EmployeeATID = (logCustomerCardResult != null
                                                ? (logTruckDriverResult != null ? logTruckDriverResult.TripCode
                                                : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.TripCode : att.EmployeeATID)) : att.EmployeeATID),
                                             EmployeeCode = u != null ? u.EmployeeCode : string.Empty,
                                             //FullName = logUserInfoResult != null ? logUserInfoResult.FullName : string.Empty,
                                             FullName = (planDockResult != null ? planDockResult.DriverName
                                                : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.ExtraDriverName
                                                : (u != null ? u.FullName : string.Empty))),
                                             UserType = u != null
                                                 ? (u.EmployeeType.HasValue
                                                 ? u.EmployeeType.Value : (short)EmployeeType.Employee)
                                                 : ((logCustomerCardResult != null && (logTruckDriverResult != null || logExtraTruckDriverResult != null))
                                                 ? (short)EmployeeType.Driver : (short)EmployeeType.Employee),
                                             IsExtraDriver = (u == null && logCustomerCardResult != null
                                                && logTruckDriverResult == null && logExtraTruckDriverResult != null),
                                             UserTypeName = uutResult != null ? uutResult.Name
                                                : (logCustomerCardResult != null
                                                ? (logTruckDriverResult != null ? "Tài xế"
                                                : (logExtraTruckDriverResult != null ? "Phụ xế" : "Nhân viên")) : "Nhân viên"),
                                             DepartmentIndex = attResult != null ? attResult.DepartmentIndex : 0,
                                             RootDepartment = (depResult == null || (depResult != null && (!depResult.ParentIndex.HasValue
                                                || (depResult.ParentIndex.HasValue && (depResult.ParentIndex.Value == 0 || depResult.ParentIndex.Value == depResult.Index))))),
                                             DepartmentName = depResult != null ? depResult.Name : "NoDepartment",
                                             SerialNumber = att.SerialNumber,
                                             DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                             Time = att.CheckTime,
                                             InOutMode = att.InOutMode.GetInOutModeString(),
                                             VerifyMode = att.VerifyMode.GetVerifyModeString(),
                                             //FaceMask = att.FaceMask.GetFaceMaskString(),
                                             //BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                             //IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                             //CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                                             CardNumber = (logCustomerCardResult != null
                                                ? (logTruckDriverResult != null ? logTruckDriverResult.CardNumber
                                                : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.CardNumber
                                                : (empCard != null ? empCard.CardNumber : string.Empty))) : (empCard != null ? empCard.CardNumber : string.Empty)),
                                             TimeString = att.CheckTime.ToString("dd/MM/yyy HH:mm:ss")
                                         };

                    var listAttendanceLog = await attendanceLogs.ToListAsync();

                    var activeDriverLog = await DbContext.GC_TruckDriverLog.Where(x => !x.IsInactive && x.InOutMode == (short)InOutMode.Input)
                        .Select(x => x.CardNumber).ToListAsync();
                    var activeExtraDriverLog = await DbContext.GC_TruckExtraDriverLog.Where(x => !x.IsInactive)
                        .Select(x => x.CardNumber).ToListAsync();
                    var activeDriverCardNumber = new List<string>();
                    if (activeDriverLog.Count > 0)
                    {
                        activeDriverCardNumber.AddRange(activeDriverLog);
                    }
                    if (activeExtraDriverLog.Count > 0)
                    {
                        activeDriverCardNumber.AddRange(activeExtraDriverLog);
                    }
                    if (activeDriverCardNumber.Count > 0)
                    {
                        var activeDriverCardId = await DbContext.HR_CustomerCard.Where(x
                            => activeDriverCardNumber.Contains(x.CardNumber)).Select(x => x.CardID).ToListAsync();
                        if (activeDriverCardId.Count > 0)
                        {
                            var activeDriverAttendanceLogs = from att in DbContext.IC_AttendanceLog.Where(x
                                            => x.CompanyIndex == user.CompanyIndex
                                            && x.CheckTime > latestEmergencyAttendance
                                            && listEmergencyMachineSerial.Contains(x.SerialNumber)
                                            && activeDriverCardId.Contains(x.EmployeeATID))
                                                             join cc in DbContext.HR_CustomerCard
                                                             on att.EmployeeATID equals cc.CardID
                                                             into logCustomerCardInfo
                                                             from logCustomerCardResult in logCustomerCardInfo.DefaultIfEmpty()
                                                             join t in DbContext.GC_TruckDriverLog.Where(x => !x.IsInactive)
                                                             on logCustomerCardResult.CardNumber equals t.CardNumber
                                                             into logTruckDriverInfo
                                                             from logTruckDriverResult in logTruckDriverInfo.DefaultIfEmpty()
                                                             join p in DbContext.IC_PlanDock
                                                             on logTruckDriverResult.TripCode equals p.TripId
                                                             into planDockInfo
                                                             from planDockResult in planDockInfo.DefaultIfEmpty()
                                                             join et in DbContext.GC_TruckExtraDriverLog.Where(x => !x.IsInactive)
                                                             on logCustomerCardResult.CardNumber equals et.CardNumber
                                                             into logExtraTruckDriverInfo
                                                             from logExtraTruckDriverResult in logExtraTruckDriverInfo.DefaultIfEmpty()
                                                             join d in DbContext.IC_Device
                                                             //.Where(x => x.DeviceModule != "ICMS")
                                                             on att.SerialNumber equals d.SerialNumber
                                                             into logDeviceInfo
                                                             from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                                             join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                                             on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                                             from empCard in employeeCard.DefaultIfEmpty()
                                                             select new WorkingLogResult
                                                             {
                                                                 EmployeeATID = (logCustomerCardResult != null
                                                                    ? (logTruckDriverResult != null ? logTruckDriverResult.TripCode
                                                                    : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.TripCode : att.EmployeeATID)) : att.EmployeeATID),
                                                                 EmployeeCode = (logCustomerCardResult != null
                                                                    ? (logTruckDriverResult != null ? logTruckDriverResult.TripCode
                                                                    : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.TripCode : att.EmployeeATID)) : att.EmployeeATID),
                                                                 //FullName = logUserInfoResult != null ? logUserInfoResult.FullName : string.Empty,
                                                                 FullName = (planDockResult != null ? planDockResult.DriverName
                                                                    : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.ExtraDriverName
                                                                    : string.Empty)),
                                                                 UserType = (logCustomerCardResult != null && (logTruckDriverResult != null || logExtraTruckDriverResult != null))
                                                                     ? (short)EmployeeType.Driver : (short)EmployeeType.Employee,
                                                                 IsExtraDriver = (logCustomerCardResult != null
                                                                    && logTruckDriverResult == null && logExtraTruckDriverResult != null),
                                                                 UserTypeName = logCustomerCardResult != null
                                                                    ? (logTruckDriverResult != null ? "Tài xế"
                                                                    : (logExtraTruckDriverResult != null ? "Phụ xế" : "Nhân viên")) : "Nhân viên",
                                                                 SerialNumber = att.SerialNumber,
                                                                 DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                                                 Time = att.CheckTime,
                                                                 InOutMode = att.InOutMode.GetInOutModeString(),
                                                                 VerifyMode = att.VerifyMode.GetVerifyModeString(),
                                                                 //FaceMask = att.FaceMask.GetFaceMaskString(),
                                                                 //BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                                                 //IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                                                 //CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                                                                 CardNumber = (logCustomerCardResult != null
                                                                    ? (logTruckDriverResult != null ? logTruckDriverResult.CardNumber
                                                                    : (logExtraTruckDriverResult != null ? logExtraTruckDriverResult.CardNumber
                                                                    : (empCard != null ? empCard.CardNumber : string.Empty))) : (empCard != null ? empCard.CardNumber : string.Empty)),
                                                                 TimeString = att.CheckTime.ToString("dd/MM/yyy HH:mm:ss")
                                                             };
                            var resultActiveDriverAttendanceLogs = await activeDriverAttendanceLogs.ToListAsync();
                            if (resultActiveDriverAttendanceLogs.Count() > 0)
                            {
                                listAttendanceLog.AddRange(resultActiveDriverAttendanceLogs);
                            }
                        }
                    }

                    if (rule != null && rule.PresenceTrackingTime > 0)
                    {
                        var now = DateTime.Now;
                        listAttendanceLog = listAttendanceLog.Where(x
                            => now.Subtract(x.Time).TotalMinutes <= Convert.ToDouble(rule.PresenceTrackingTime * 60)).ToList();
                    }

                    listAttendanceLog = listAttendanceLog.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                        => x.FirstOrDefault()).ToList();
                    listAttendanceLog.RemoveAll(x => x == null);

                    listAttendanceLog = listAttendanceLog.Where(x => x.DepartmentIndex == 0
                        || (user.ListDepartmentAssigned != null
                        && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();

                    var listCustomerCard = await _HR_CustomerCardService.GetAllCustomerCard(user);
                    if (listCustomerCard != null && listCustomerCard.Count > 0)
                    {
                        var listUsingCustomerCard = listCustomerCard.Where(x => x.Status).ToList();
                        var cardIdToUserCodeMap = listUsingCustomerCard.ToDictionary(card => card.CardID, card => card.UserCode);

                        listAttendanceLog.ForEach(x =>
                        {
                            if (cardIdToUserCodeMap.ContainsKey(x.EmployeeATID))
                            {
                                x.EmployeeATID = cardIdToUserCodeMap[x.EmployeeATID];
                            }
                        });
                    }

                    var listSerialNumber = new List<string>();
                    if (listAttendanceLog != null && listAttendanceLog.Count > 0)
                    {
                        listSerialNumber = listAttendanceLog.Select(x => x.SerialNumber).Distinct().ToList();
                    }
                    var listLineIndex = new List<int>();
                    var listLineIn = new List<GC_Lines_CheckInDevice>();
                    var listLineOut = new List<GC_Lines_CheckOutDevice>();
                    if (listSerialNumber != null && listSerialNumber.Count > 0)
                    {
                        listLineIn = await DbContext.GC_Lines_CheckInDevice.AsNoTracking().Where(x
                            => listSerialNumber.Contains(x.CheckInDeviceSerial)).ToListAsync();
                        listLineOut = await DbContext.GC_Lines_CheckOutDevice.AsNoTracking().Where(x
                            => listSerialNumber.Contains(x.CheckOutDeviceSerial)).ToListAsync();
                        if (listLineIn != null && listLineIn.Count > 0)
                        {
                            listLineIndex.AddRange(listLineIn.Select(x => x.LineIndex).ToList());
                        }
                        if (listLineOut != null && listLineOut.Count > 0)
                        {
                            listLineIndex.AddRange(listLineOut.Select(x => x.LineIndex).ToList());
                        }
                    }

                    var listGroupDeviceIndex = new List<int>();
                    var listGroupDeviceDetails = new List<IC_GroupDeviceDetails>();
                    if (listSerialNumber != null && listSerialNumber.Count > 0)
                    {
                        listGroupDeviceDetails = await DbContext.IC_GroupDeviceDetails.AsNoTracking().Where(x
                            => listSerialNumber.Contains(x.SerialNumber)).ToListAsync();
                        if (listGroupDeviceDetails != null && listGroupDeviceDetails.Count > 0)
                        {
                            listGroupDeviceIndex = listGroupDeviceDetails.Select(x => x.GroupDeviceIndex).Distinct().ToList();
                        }
                    }

                    var listAreaGroupIndex = new List<int>();
                    var listAreaGroupDetail = new List<GC_AreaGroup_GroupDevice>();
                    if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
                    {
                        listAreaGroupDetail = await DbContext.GC_AreaGroup_GroupDevice.AsNoTracking().Where(x
                            => listGroupDeviceIndex.Contains(x.DeviceGroupIndex)).ToListAsync();
                        if (listAreaGroupDetail != null && listAreaGroupDetail.Count > 0)
                        {
                            listAreaGroupIndex = listAreaGroupDetail.Select(x => x.AreaGroupIndex).Distinct().ToList();
                        }
                    }

                    var listGateIndex = new List<int>();
                    var listGateLine = new List<GC_Gates_Lines>();
                    if (listLineIndex != null && listLineIndex.Count > 0)
                    {
                        listGateLine = await DbContext.GC_Gates_Lines.AsNoTracking().Where(x
                            => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                        if (listGateLine != null && listGateLine.Count > 0)
                        {
                            listGateIndex = listGateLine.Select(x => x.GateIndex).Distinct().ToList();
                        }
                    }

                    var listLine = new List<GC_Lines>();
                    var listGate = new List<GC_Gates>();
                    var listGroupDevice = new List<IC_GroupDevice>();
                    var listAreaGroup = new List<GC_AreaGroup>();
                    if (listLineIndex != null && listLineIndex.Count > 0)
                    {
                        listLine = await DbContext.GC_Lines.AsNoTracking().Where(x
                            => listLineIndex.Contains(x.Index)).ToListAsync();
                    }
                    if (listGateIndex != null && listGateIndex.Count > 0)
                    {
                        listGate = await DbContext.GC_Gates.AsNoTracking().Where(x
                            => listGateIndex.Contains(x.Index)).ToListAsync();
                    }
                    if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
                    {
                        listGroupDevice = await DbContext.IC_GroupDevice.AsNoTracking().Where(x
                            => listGroupDeviceIndex.Contains(x.Index)).ToListAsync();
                    }
                    if (listAreaGroupIndex != null && listAreaGroupIndex.Count > 0)
                    {
                        listAreaGroup = await DbContext.GC_AreaGroup.AsNoTracking().Where(x
                            => listAreaGroupIndex.Contains(x.Index)).ToListAsync();
                    }

                    var logLine = new GC_Lines();
                    var logGate = new List<GC_Gates>();
                    var logGroupDevice = new List<IC_GroupDevice>();
                    var logAreaGroup = new List<GC_AreaGroup>();
                    listAttendanceLog.ForEach(x =>
                    {
                        logLine = new GC_Lines();
                        logGate = new List<GC_Gates>();
                        logGroupDevice = new List<IC_GroupDevice>();
                        logAreaGroup = new List<GC_AreaGroup>();

                        var logLineIn = listLineIn.FirstOrDefault(y => y.CheckInDeviceSerial == x.SerialNumber);
                        var logLineOut = listLineOut.FirstOrDefault(y => y.CheckOutDeviceSerial == x.SerialNumber);
                        var logLineIndex = 0;
                        if (logLineIn != null)
                        {
                            logLine = listLine.FirstOrDefault(y => y.Index == logLineIn.LineIndex);
                            logLineIndex = logLineIn.LineIndex;
                            x.LineIndex = logLine.Index;
                            x.LineName = logLine.Name;
                        }
                        else if (logLineOut != null)
                        {
                            logLine = listLine.FirstOrDefault(y => y.Index == logLineOut.LineIndex);
                            logLineIndex = logLineOut.LineIndex;
                            x.LineIndex = logLine.Index;
                            x.LineName = logLine.Name;
                        }
                        var logGateLine = listGateLine.Where(y => y.LineIndex == logLineIndex).ToList();
                        if (logGateLine != null && logGateLine.Count > 0)
                        {
                            var gateIndex = logGateLine.Select(y => y.GateIndex).ToList();
                            logGate = listGate.Where(y => gateIndex.Contains(y.Index)).ToList();
                            x.GateIndex = logGate.Select(x => x.Index).ToList();
                            x.GateName = logGate.Select(x => x.Name).ToList();
                        }

                        var logGroupDeviceIndex = listGroupDeviceDetails.Where(y => y.SerialNumber == x.SerialNumber)
                            .Select(x => x.GroupDeviceIndex).ToList();
                        if (logGroupDeviceIndex != null && logGroupDeviceIndex.Count > 0)
                        {
                            x.GroupDeviceIndex = logGroupDeviceIndex;
                            logGroupDevice = listGroupDevice.Where(g => listAreaGroupIndex.Contains(g.Index)).ToList();
                            if (logGroupDevice != null && logGroupDevice.Count > 0)
                            {
                                x.GroupDeviceName = logGroupDevice.Select(x => x.Name).ToList();
                            }

                            var logAreaGroupIndex = listAreaGroupDetail.Where(y => logGroupDeviceIndex.Contains(y.DeviceGroupIndex))
                                .Select(x => x.AreaGroupIndex).ToList();
                            if (logAreaGroupIndex != null && logAreaGroupIndex.Count > 0)
                            {
                                x.AreaGroupIndex = logAreaGroupIndex;
                                logAreaGroup = listAreaGroup.Where(g => logAreaGroupIndex.Contains(g.Index)).ToList();
                                if (logAreaGroup != null && logAreaGroup.Count > 0)
                                {
                                    x.AreaGroupName = logAreaGroup.Select(x => x.Name).ToList();
                                }
                            }
                        }
                    });

                    return listAttendanceLog;
                }
                return new List<WorkingLogResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmergencyLog: " + ex);
                return new List<WorkingLogResult>();
            }
        }

        public async Task<dynamic> GetTupleFullWorkingEmployeeByUserType(UserInfo user)
        {
            var listCustomerCard = await DbContext.HR_CustomerCard.Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
            var listCustomerCardId = listCustomerCard.Select(x => x.CardID).ToList();

            var listPlanDock = await DbContext.IC_PlanDock.Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
            var listTripCode = listPlanDock.Select(x => x.TripId).ToList();

            var listGeneralRules = await DbContext.GC_Rules_General.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
            var rule = listGeneralRules.Where(x => x.IsUsing).FirstOrDefault();

            var presenTrackingTime = 0;
            if (rule != null && rule.PresenceTrackingTime > 0)
            {
                presenTrackingTime = rule.PresenceTrackingTime;
            }

            var timeLogQuery = DbContext.GC_TimeLog.AsNoTracking()
            .Where(x =>
                (presenTrackingTime == 0 ||
                 (presenTrackingTime > 0 && DateTime.Now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.Time)) &&
                x.ApproveStatus == (short)ApproveStatus.Approved)
            .Select(att => new
            {
                att.EmployeeATID,
                att.MachineSerial,
                att.Time,
                att.InOutMode,
                att.VerifyMode,
                att.CardNumber
            });

            //var attendanceLogs = from att in DbContext.GC_TimeLog.AsNoTracking().Where(x
            //                        =>
            //                        //!listCustomerCardId.Contains(x.EmployeeATID) 
            //                        //&& !listTripCode.Contains(x.EmployeeATID)
            //                        //&& 
            //                        (presenTrackingTime == 0
            //                            || (presenTrackingTime > 0 && DateTime.Now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.Time))
            //                        &&
            //                        x.ApproveStatus == (short)ApproveStatus.Approved
            //                        //&& (string.IsNullOrWhiteSpace(x.Note)
            //                        //|| (!string.IsNullOrWhiteSpace(x.Note) && !x.Note.Contains("MissingLogOutAfterExpiredPresenceTime"))
            //                        //)
            //                        )
            var attendanceLogs = from att in timeLogQuery
                                 join u in DbContext.HR_User
                                 on att.EmployeeATID equals u.EmployeeATID
                                 //into logUserInfo
                                 //from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                 join ut in DbContext.HR_UserType
                                 on u.EmployeeType equals ut.UserTypeId
                                 into uutInfo
                                 from uutResult in uutInfo.DefaultIfEmpty()
                                 join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                 && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                                 on att.EmployeeATID equals wi.EmployeeATID
                                 into attInfo
                                 from attResult in attInfo.DefaultIfEmpty()
                                 join dp in DbContext.IC_Department
                                 on attResult.DepartmentIndex equals dp.Index into dep
                                 from depResult in dep.DefaultIfEmpty()
                                 join d in DbContext.IC_Device.Where(x => x.DeviceModule != "ICMS")
                                 on att.MachineSerial equals d.SerialNumber
                                 into logDeviceInfo
                                 from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                 join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                 on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                 from empCard in employeeCard.DefaultIfEmpty()
                                 select new WorkingLogResult
                                 {
                                     EmployeeATID = att.EmployeeATID,
                                     EmployeeCode = u != null ? u.EmployeeCode : string.Empty,
                                     FullName = u != null ? u.FullName : string.Empty,
                                     UserType = u != null
                                         ? (u.EmployeeType.HasValue
                                         ? u.EmployeeType.Value : (short)EmployeeType.Employee)
                                         : (short)EmployeeType.Employee,
                                     UserTypeName = uutResult != null ? uutResult.Name : "Nhân viên",
                                     DepartmentIndex = attResult != null ? attResult.DepartmentIndex : 0,
                                     RootDepartment = (depResult == null || (depResult != null && (!depResult.ParentIndex.HasValue
                                        || (depResult.ParentIndex.HasValue && (depResult.ParentIndex.Value == 0 || depResult.ParentIndex.Value == depResult.Index))))),
                                     DepartmentName = depResult != null ? depResult.Name : "NoDepartment",
                                     SerialNumber = att.MachineSerial,
                                     DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                     Time = att.Time,
                                     InOutMode = att.InOutMode.Value.GetGCSInOutModeString(),
                                     VerifyMode = att.VerifyMode,
                                     //FaceMask = att.FaceMask.GetFaceMaskString(),
                                     //BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                     //IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                     CardNumber = empCard != null ? empCard.CardNumber : att.CardNumber,
                                     TimeString = att.Time.ToString("dd/MM/yyy HH:mm:ss")
                                 };

            //if (rule != null && rule.PresenceTrackingTime > 0)
            //{
            //    var now = DateTime.Now;
            //    attendanceLogs = attendanceLogs.Where(x
            //        => now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.Time);
            //}

            var vehicleLogs = from att in DbContext.GC_TruckDriverLog
                                  .Where(x
                                  => (presenTrackingTime == 0
                                        || (presenTrackingTime > 0 && DateTime.Now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.Time))
                                  //=> (string.IsNullOrWhiteSpace(x.Note)
                                  //|| (!string.IsNullOrWhiteSpace(x.Note) && !x.Note.Contains("MissingLogOutAfterExpiredPresenceTime"))
                                  //)
                                  )
                              join u in DbContext.IC_PlanDock
                              on att.TripCode equals u.TripId
                              into logUserInfo
                              from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                              join d in DbContext.IC_Device.Where(x => x.DeviceModule != "ICMS")
                              on att.MachineSerial equals d.SerialNumber
                              into logDeviceInfo
                              from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                              join c in DbContext.HR_CustomerCard
                              on att.CardNumber equals c.CardNumber into employeeCard
                              from empCard in employeeCard.DefaultIfEmpty()
                              select new WorkingLogResult
                              {
                                  EmployeeATID = att.TripCode,
                                  //EmployeeATID = logUserInfoResult != null ? logUserInfoResult.TripId : string.Empty,
                                  EmployeeCode = logUserInfoResult != null ? logUserInfoResult.DriverCode : string.Empty,
                                  FullName = logUserInfoResult != null ? logUserInfoResult.DriverName : string.Empty,
                                  UserType = (short)EmployeeType.Driver,
                                  UserTypeName = "Tài xế",
                                  SerialNumber = att.MachineSerial,
                                  DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                  Time = att.Time,
                                  InOutMode = att.InOutMode.Value.GetInOutModeString(),
                                  CardNumber = empCard != null ? empCard.CardNumber : att.CardNumber,
                                  TimeString = att.Time.ToString("dd/MM/yyy HH:mm:ss"),
                              };

            //if (rule != null && rule.PresenceTrackingTime > 0)
            //{
            //    var now = DateTime.Now;
            //    vehicleLogs = vehicleLogs.Where(x
            //        => now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.Time);
            //}

            var integratedVehicleLogQuery = DbContext.IC_VehicleLog.AsNoTracking().Where(x =>
                        (presenTrackingTime == 0
                            || (presenTrackingTime > 0 && (DateTime.Now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.FromDate.Value
                                || (x.ToDate.HasValue && DateTime.Now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.ToDate.Value)))))
                .Select(x => new
                {
                    x.EmployeeATID,
                    x.FromDate,
                    x.ToDate,
                    x.ComputerIn,
                    x.ComputerOut
                });

            var integratedVehicleInNotOutLogs =
                        //from att in DbContext.IC_VehicleLog.Where(x
                        //                    => !listCustomerCardId.Contains(x.EmployeeATID) && !listTripCode.Contains(x.EmployeeATID)
                        //                    && (!string.IsNullOrWhiteSpace(x.ComputerIn)
                        //                    || (!string.IsNullOrWhiteSpace(x.ComputerOut) && x.ComputerOut != "-1"))
                        //                    //&& !string.IsNullOrWhiteSpace(x.ComputerIn) && string.IsNullOrWhiteSpace(x.ComputerOut)
                        //                    //&& x.FromDate.HasValue 
                        //                    //&& !x.ToDate.HasValue
                        //                    //&& (string.IsNullOrWhiteSpace(x.Note)
                        //                    //|| (!string.IsNullOrWhiteSpace(x.Note) && !x.Note.Contains("MissingLogOutAfterExpiredPresenceTime"))
                        //                    //)
                        //                    )
                        //join u in DbContext.HR_User
                        //on att.EmployeeATID equals u.EmployeeATID
                        ////into logUserInfo
                        ////from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                        //join ut in DbContext.HR_UserType
                        //on u.EmployeeType equals ut.UserTypeId
                        //into uutInfo
                        //from uutResult in uutInfo.DefaultIfEmpty()
                        //join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                        //&& (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                        //on att.EmployeeATID equals wi.EmployeeATID
                        //into attInfo
                        //from attResult in attInfo.DefaultIfEmpty()
                        //join dp in DbContext.IC_Department
                        //on attResult.DepartmentIndex equals dp.Index into dep
                        //from depResult in dep.DefaultIfEmpty()
                        //    //join d in DbContext.IC_Device.Where(x => x.DeviceModule != "ICMS")
                        //    //on att.ComputerIn equals d.SerialNumber
                        //    //into logDeviceInfo
                        //    //from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                        //join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                        //on att.EmployeeATID equals c.EmployeeATID into employeeCard
                        //from empCard in employeeCard.DefaultIfEmpty()



                        //from att in DbContext.IC_VehicleLog.AsNoTracking().Where(x =>
                        //(presenTrackingTime == 0
                        //    || (presenTrackingTime > 0 && (DateTime.Now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.FromDate.Value
                        //        || (x.ToDate.HasValue && DateTime.Now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.ToDate.Value)))))
                        from att in integratedVehicleLogQuery
                        join u in DbContext.HR_User on att.EmployeeATID equals u.EmployeeATID
                        join ut in DbContext.HR_UserType on u.EmployeeType equals ut.UserTypeId into uutInfo
                        from uutResult in uutInfo.DefaultIfEmpty()
                        join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                        && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                        on att.EmployeeATID equals wi.EmployeeATID into attInfo
                        from attResult in attInfo.DefaultIfEmpty()
                        join dp in DbContext.IC_Department on attResult.DepartmentIndex equals dp.Index into dep
                        from depResult in dep.DefaultIfEmpty()
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                        on att.EmployeeATID equals c.EmployeeATID into employeeCard
                        from empCard in employeeCard.DefaultIfEmpty()

                        where !listCustomerCardId.Contains(att.EmployeeATID)
                            && !listTripCode.Contains(att.EmployeeATID)
                            && (!string.IsNullOrWhiteSpace(att.ComputerIn)
                                || (!string.IsNullOrWhiteSpace(att.ComputerOut) && att.ComputerOut != "-1"))
                        //&& (attResult == null || (attResult.FromDate.Date <= DateTime.Now.Date
                        //    && (!attResult.ToDate.HasValue
                        //        ||  attResult.ToDate.Value.Date > DateTime.Now.Date)))

                        select new WorkingLogResult
                        {
                            EmployeeATID = att.EmployeeATID,
                            EmployeeCode = u != null ? u.EmployeeCode : string.Empty,
                            FullName = u != null ? u.FullName : string.Empty,
                            UserType = u != null
                                         ? (u.EmployeeType.HasValue
                                         ? u.EmployeeType.Value : (short)EmployeeType.Employee)
                                         : (short)EmployeeType.Employee,
                            UserTypeName = uutResult != null ? uutResult.Name : "Nhân viên",
                            DepartmentIndex = attResult != null ? attResult.DepartmentIndex : 0,
                            RootDepartment = (depResult == null || (depResult != null && (!depResult.ParentIndex.HasValue
                                        || (depResult.ParentIndex.HasValue && (depResult.ParentIndex.Value == 0 || depResult.ParentIndex.Value == depResult.Index))))),
                            DepartmentName = depResult != null ? depResult.Name : "NoDepartment",
                            SerialNumber = att.ComputerIn,
                            DeviceName = att.ComputerIn,
                            //DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                            Time = att.FromDate.Value,
                            FromDate = att.FromDate,
                            ToDate = att.ToDate,
                            ComputerIn = att.ComputerIn,
                            ComputerOut = att.ComputerOut,
                            VerifyMode = EPAD_Common.Enums.DeviceType.Card.ToString(),
                            //FaceMask = att.FaceMask.GetFaceMaskString(),
                            //BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                            //IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                            CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                            TimeString = att.FromDate.Value.ToString("dd/MM/yyy HH:mm:ss")
                        };

            //if (rule != null && rule.PresenceTrackingTime > 0)
            //{
            //    var now = DateTime.Now;
            //    integratedVehicleInNotOutLogs = integratedVehicleInNotOutLogs.Where(x
            //        => now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.FromDate.Value
            //        || (x.ToDate.HasValue && now.AddMinutes(-(rule.PresenceTrackingTime * 60)) <= x.ToDate.Value));
            //}

            var listAttendanceLog = await attendanceLogs.ToListAsync();
            var listVehicleLog = await vehicleLogs.ToListAsync();
            var listIntegratedVehicleInNotOutLogs = await integratedVehicleInNotOutLogs.ToListAsync();

            //listIntegratedVehicleInNotOutLogs = listIntegratedVehicleInNotOutLogs.Where(x => x.EmployeeATID == "00041266").ToList();

            //listIntegratedVehicleInNotOutLogs = listIntegratedVehicleInNotOutLogs
            //    .OrderByDescending(x => x.FromDate)
            //    .GroupBy(x => x.EmployeeATID).SelectMany(x => x.ToList().GroupBy(y => y.FromDate)
            //    .Select(y => y.ToList()).FirstOrDefault()).ToList();

            ////if (listIntegratedVehicleInNotOutLogs.Count > 0)
            ////{
            ////    listIntegratedVehicleInNotOutLogs = listIntegratedVehicleInNotOutLogs.Where(x => !x.ToDate.HasValue).ToList();
            ////}

            var listComputerID = new List<string>();
            var listIntegratedVehicleInLogs = listIntegratedVehicleInNotOutLogs.Where(x => !string.IsNullOrWhiteSpace(x.ComputerIn)
                && x.FromDate.HasValue).Select(x
                => new WorkingLogResult
                {
                    EmployeeATID = x.EmployeeATID,
                    EmployeeCode = x.EmployeeCode,
                    FullName = x.FullName,
                    UserType = x.UserType,
                    UserTypeName = x.UserTypeName,
                    DepartmentIndex = x.DepartmentIndex,
                    RootDepartment = x.RootDepartment,
                    DepartmentName = x.DepartmentName,
                    SerialNumber = x.ComputerIn,
                    DeviceName = x.ComputerIn,
                    Time = x.FromDate.Value,
                    InOutMode = "In",
                    VerifyMode = EPAD_Common.Enums.DeviceType.Card.ToString(),
                    CardNumber = x.CardNumber,
                    TimeString = x.TimeString,
                    IsIntegratedLog = true
                }).ToList();
            if (listIntegratedVehicleInLogs != null && listIntegratedVehicleInLogs.Count > 0)
            {
                listIntegratedVehicleInLogs = listIntegratedVehicleInLogs.GroupBy(x
                    => new { x.EmployeeATID, x.Time }).Select(x => x.FirstOrDefault()).ToList();
                listComputerID.AddRange(listIntegratedVehicleInLogs.Where(x
                    => !string.IsNullOrWhiteSpace(x.SerialNumber)).Select(x => x.SerialNumber).ToList());
            }

            var listIntegratedVehicleOutLogs = listIntegratedVehicleInNotOutLogs.Where(x => !string.IsNullOrWhiteSpace(x.ComputerOut)
                && x.ComputerOut != "-1" && x.ToDate.HasValue).Select(x
                => new WorkingLogResult
                {
                    EmployeeATID = x.EmployeeATID,
                    EmployeeCode = x.EmployeeCode,
                    FullName = x.FullName,
                    UserType = x.UserType,
                    UserTypeName = x.UserTypeName,
                    DepartmentIndex = x.DepartmentIndex,
                    RootDepartment = x.RootDepartment,
                    DepartmentName = x.DepartmentName,
                    SerialNumber = x.ComputerOut,
                    DeviceName = x.ComputerOut,
                    Time = x.ToDate.Value,
                    InOutMode = "Out",
                    VerifyMode = EPAD_Common.Enums.DeviceType.Card.ToString(),
                    CardNumber = x.CardNumber,
                    TimeString = x.TimeString,
                    IsIntegratedLog = true
                }).ToList();

            if (listIntegratedVehicleOutLogs != null && listIntegratedVehicleOutLogs.Count > 0)
            {
                listIntegratedVehicleOutLogs = listIntegratedVehicleOutLogs.GroupBy(x
                    => new { x.EmployeeATID, x.Time }).Select(x => x.FirstOrDefault()).ToList();
                listComputerID.AddRange(listIntegratedVehicleOutLogs.Where(x
                    => !string.IsNullOrWhiteSpace(x.SerialNumber)).Select(x => x.SerialNumber).ToList());
            }

            var listIntegratedDeviceByComputerID = new List<IC_Device>();
            var listIntegratedLineDeviceIn = new List<GC_Lines_CheckInDevice>();
            var listIntegratedLineDeviceOut = new List<GC_Lines_CheckOutDevice>();
            var listIntegratedGateLine = new List<GC_Gates_Lines>();
            var listIntegratedGate = new List<GC_Gates>();
            if (listComputerID.Count > 0)
            {
                listIntegratedDeviceByComputerID = await DbContext.IC_Device.AsNoTracking().Where(x
                    => !string.IsNullOrWhiteSpace(x.DeviceId) && x.DeviceModule == "PA" && listComputerID.Contains(x.DeviceId)).ToListAsync();
                if (listIntegratedDeviceByComputerID.Count > 0)
                {
                    var listIntegratedDeviceSerial = listIntegratedDeviceByComputerID.Where(x
                        => !string.IsNullOrWhiteSpace(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
                    if (listIntegratedDeviceSerial.Count > 0)
                    {
                        listIntegratedLineDeviceIn = await DbContext.GC_Lines_CheckInDevice.AsNoTracking().Where(x
                            => listIntegratedDeviceSerial.Contains(x.CheckInDeviceSerial)).ToListAsync();
                        listIntegratedLineDeviceOut = await DbContext.GC_Lines_CheckOutDevice.AsNoTracking().Where(x
                            => listIntegratedDeviceSerial.Contains(x.CheckOutDeviceSerial)).ToListAsync();
                        var listIntegratedLineIndex = new List<int>();
                        if (listIntegratedLineDeviceIn.Count > 0)
                        {
                            listIntegratedLineIndex.AddRange(listIntegratedLineDeviceIn.Select(x => x.LineIndex));
                        }
                        if (listIntegratedLineDeviceOut.Count > 0)
                        {
                            listIntegratedLineIndex.AddRange(listIntegratedLineDeviceOut.Select(x => x.LineIndex));
                        }
                        if (listIntegratedLineIndex.Count > 0)
                        {
                            listIntegratedGateLine = await DbContext.GC_Gates_Lines.AsNoTracking().Where(x
                                => listIntegratedLineIndex.Contains(x.LineIndex)).ToListAsync();
                            if (listIntegratedGateLine.Count > 0)
                            {
                                var listIntegratedGateIndex = listIntegratedGateLine.Select(x => x.GateIndex).ToList();
                                listIntegratedGate = await DbContext.GC_Gates.AsNoTracking().Where(x
                                    => listIntegratedGateIndex.Contains(x.Index)).ToListAsync();
                            }
                        }
                    }
                }
            }

            if (listIntegratedVehicleInLogs != null && listIntegratedVehicleInLogs.Count > 0)
            {
                foreach (var integratedLogIn in listIntegratedVehicleInLogs)
                {
                    integratedLogIn.GateName = new List<string>();
                    if (listIntegratedDeviceByComputerID.Any(x => x.DeviceId == integratedLogIn.SerialNumber
                        && x.DeviceStatus.HasValue
                        && (x.DeviceStatus == (short)DeviceStatus.Input || x.DeviceStatus == (short)DeviceStatus.MiddleInt)))
                    {
                        var listIntegratedLogInDevice = listIntegratedDeviceByComputerID.Where(x
                            => x.DeviceId == integratedLogIn.SerialNumber
                            && x.DeviceStatus.HasValue
                            && (x.DeviceStatus == (short)DeviceStatus.Input || x.DeviceStatus == (short)DeviceStatus.MiddleInt)).ToList();

                        if ((listIntegratedLineDeviceIn.Any(x
                                => listIntegratedLogInDevice.Any(y => y.SerialNumber == x.CheckInDeviceSerial))
                            || listIntegratedLineDeviceOut.Any(x
                                => listIntegratedLogInDevice.Any(y => y.SerialNumber == x.CheckOutDeviceSerial))))
                        {
                            var listIntegratedLogInLineIndex = listIntegratedLineDeviceIn.Where(x
                                => listIntegratedLogInDevice.Any(y => y.SerialNumber == x.CheckInDeviceSerial)).Select(x
                                    => x.LineIndex).ToList() ?? new List<int>();
                            listIntegratedLogInLineIndex.AddRange(listIntegratedLineDeviceOut.Where(x
                                => listIntegratedLogInDevice.Any(y => y.SerialNumber == x.CheckOutDeviceSerial)).Select(x
                                    => x.LineIndex).ToList() ?? new List<int>());
                            if (listIntegratedGateLine.Any(x => listIntegratedLogInLineIndex.Contains(x.LineIndex)))
                            {
                                var listIntegratedLogInGateIndex = listIntegratedGateLine.Where(x
                                    => listIntegratedLogInLineIndex.Contains(x.LineIndex)).Select(x => x.GateIndex).ToList();
                                var listIntegratedLogInGate = listIntegratedGate.Where(x
                                    => listIntegratedLogInGateIndex.Contains(x.Index)).ToList();
                                if (listIntegratedLogInGate.Count > 0)
                                {
                                    integratedLogIn.GateName = new List<string> { listIntegratedLogInGate.FirstOrDefault().Name };
                                }
                            }
                        }
                        else
                        {
                            integratedLogIn.GateName = new List<string> { listIntegratedLogInDevice.FirstOrDefault().AliasName };
                        }
                    }
                }
                listAttendanceLog.AddRange(listIntegratedVehicleInLogs);
            }

            if (listIntegratedVehicleOutLogs != null && listIntegratedVehicleOutLogs.Count > 0)
            {
                foreach (var integratedLogOut in listIntegratedVehicleOutLogs)
                {
                    integratedLogOut.GateName = new List<string>();
                    if (listIntegratedDeviceByComputerID.Any(x => x.DeviceId == integratedLogOut.SerialNumber
                        && x.DeviceStatus.HasValue
                        && (x.DeviceStatus == (short)DeviceStatus.Output || x.DeviceStatus == (short)DeviceStatus.MiddleOut)))
                    {
                        var listIntegratedLogOutDevice = listIntegratedDeviceByComputerID.Where(x
                            => x.DeviceId == integratedLogOut.SerialNumber
                            && x.DeviceStatus.HasValue
                            && (x.DeviceStatus == (short)DeviceStatus.Output || x.DeviceStatus == (short)DeviceStatus.MiddleOut)).ToList();

                        if ((listIntegratedLineDeviceIn.Any(x
                                => listIntegratedLogOutDevice.Any(y => y.SerialNumber == x.CheckInDeviceSerial))
                            || listIntegratedLineDeviceOut.Any(x
                                => listIntegratedLogOutDevice.Any(y => y.SerialNumber == x.CheckOutDeviceSerial))))
                        {
                            var listIntegratedLogOutLineIndex = listIntegratedLineDeviceIn.Where(x
                                => listIntegratedLogOutDevice.Any(y => y.SerialNumber == x.CheckInDeviceSerial)).Select(x
                                    => x.LineIndex).ToList() ?? new List<int>();
                            listIntegratedLogOutLineIndex.AddRange(listIntegratedLineDeviceOut.Where(x
                                => listIntegratedLogOutDevice.Any(y => y.SerialNumber == x.CheckOutDeviceSerial)).Select(x
                                    => x.LineIndex).ToList() ?? new List<int>());
                            if (listIntegratedGateLine.Any(x => listIntegratedLogOutLineIndex.Contains(x.LineIndex)))
                            {
                                var listIntegratedLogOutGateIndex = listIntegratedGateLine.Where(x
                                    => listIntegratedLogOutLineIndex.Contains(x.LineIndex)).Select(x => x.GateIndex).ToList();
                                var listIntegratedLogOutGate = listIntegratedGate.Where(x
                                    => listIntegratedLogOutGateIndex.Contains(x.Index)).ToList();
                                if (listIntegratedLogOutGate.Count > 0)
                                {
                                    integratedLogOut.GateName = new List<string> { listIntegratedLogOutGate.FirstOrDefault().Name };
                                }
                            }
                        }
                        else
                        {
                            integratedLogOut.GateName = new List<string> { listIntegratedLogOutDevice.FirstOrDefault().AliasName };
                        }
                    }
                }
                listAttendanceLog.AddRange(listIntegratedVehicleOutLogs);
            }

            var isInActiveVehicle = await DbContext.GC_TruckDriverLog.Where(x => x.IsInactive).ToListAsync();
            var isInActiveVehicleCode = isInActiveVehicle.Select(x => x.TripCode).ToList();
            //listAttendanceLog = listAttendanceLog.Where(x => !isInActiveVehicleCode.Contains(x.EmployeeATID)).ToList();
            listAttendanceLog = listAttendanceLog.Where(x => !isInActiveVehicle.Any(y
                => y.TripCode == x.EmployeeATID && y.CardNumber == x.CardNumber)).ToList();

            //listAttendanceLog = listAttendanceLog.Where(x => !listVehicleLog.Any(y => y.EmployeeATID == x.EmployeeATID)).ToList();
            if (listVehicleLog.Count > 0)
            {
                //listVehicleLog = listVehicleLog.Where(x => !isInActiveVehicleCode.Contains(x.EmployeeATID)).ToList();
                listVehicleLog = listVehicleLog.Where(x => !isInActiveVehicle.Any(y
                    => y.TripCode == x.EmployeeATID && y.CardNumber == x.CardNumber)).ToList();
                listAttendanceLog.AddRange(listVehicleLog);
                var listInVehicleTripCode = listVehicleLog.Select(x => x.EmployeeATID).ToList();
                var listInVehicleLog = await DbContext.GC_TimeLog.AsNoTracking().Where(x
                    => listInVehicleTripCode.Contains(x.EmployeeATID)).ToListAsync();
                listInVehicleLog = listInVehicleLog.Where(x => listVehicleLog.Any(y
                    => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber)
                    && x.ApproveStatus == (short)ApproveStatus.Approved).ToList();
                if (listInVehicleLog.Count > 0)
                {
                    var listVehicleAttendanceLog = listInVehicleLog.Select(x => new WorkingLogResult
                    {

                        EmployeeATID = x.EmployeeATID,
                        //EmployeeATID = logUserInfoResult != null ? logUserInfoResult.TripId : string.Empty,
                        EmployeeCode = listVehicleLog.FirstOrDefault(y
                            => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber)?.EmployeeCode ?? string.Empty,
                        FullName = listVehicleLog.FirstOrDefault(y
                            => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber)?.FullName ?? string.Empty,
                        UserType = (short)EmployeeType.Driver,
                        UserTypeName = "Tài xế",
                        SerialNumber = x.MachineSerial,
                        Time = x.Time,
                        InOutMode = x.InOutMode.Value.GetGCSInOutModeString(),
                        CardNumber = x.CardNumber,
                        TimeString = x.Time.ToString("dd/MM/yyy HH:mm:ss"),
                    });
                    listAttendanceLog.AddRange(listVehicleAttendanceLog);
                }
            }

            var listVehicleTripCode = listAttendanceLog.Select(x => x.EmployeeATID).ToList();
            if (isInActiveVehicle.Count > 0)
            {
                listVehicleTripCode.AddRange(isInActiveVehicleCode);
            }
            var existedExtraTruckDriver = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                => listVehicleTripCode.Contains(x.TripCode) && !x.IsInactive).ToListAsync();
            var existedExtraTruckDriverTripCode = existedExtraTruckDriver.Select(x => x.TripCode).ToList();
            var existedTruckDriver = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x
                => existedExtraTruckDriverTripCode.Contains(x.TripCode)
                //&& !x.IsInactive
                ).ToListAsync();
            var existedTruckDriverSerial = existedTruckDriver.Select(x => x.MachineSerial).ToList();
            var deviceList = await DbContext.IC_Device.AsNoTracking().Where(x
                => existedTruckDriverSerial.Contains(x.SerialNumber)).ToListAsync();

            if (existedExtraTruckDriver != null && existedExtraTruckDriver.Count > 0)
            {
                var listExtraVehicleLog = existedExtraTruckDriver.Select(x => new WorkingLogResult
                {
                    EmployeeATID = x.TripCode,
                    //EmployeeATID = logUserInfoResult != null ? logUserInfoResult.TripId : string.Empty,
                    EmployeeCode = x.ExtraDriverCode,
                    FullName = x.ExtraDriverName,
                    UserType = (short)EmployeeType.Driver,
                    UserTypeName = "Phụ xế",
                    SerialNumber = existedTruckDriver.Any(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input)
                        ? existedTruckDriver.FirstOrDefault(y => y.TripCode == x.TripCode
                        && y.InOutMode == (short)InOutMode.Input).MachineSerial : string.Empty,
                    Time = existedTruckDriver.Any(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input)
                        ? existedTruckDriver.FirstOrDefault(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input).Time
                        : new DateTime(),
                    InOutMode = "In",
                    CardNumber = x.CardNumber,
                    TimeString = (existedTruckDriver.Any(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input)
                        ? existedTruckDriver.FirstOrDefault(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input).Time
                        : new DateTime()).ToString("dd/MM/yyy HH:mm:ss"),
                    IsExtraDriver = true
                }).ToList();
                listExtraVehicleLog.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.SerialNumber) && deviceList.Any(y => y.SerialNumber == x.SerialNumber))
                    {
                        x.DeviceName = deviceList.FirstOrDefault(y => y.SerialNumber == x.SerialNumber).AliasName;
                    }
                });
                listAttendanceLog.AddRange(listExtraVehicleLog);

                var listInVehicleTripCode = listExtraVehicleLog.Select(x => x.EmployeeATID).ToList();
                var listInVehicleLog = await DbContext.GC_TimeLog.AsNoTracking().Where(x
                    => listInVehicleTripCode.Contains(x.EmployeeATID)).ToListAsync();
                listInVehicleLog = listInVehicleLog.Where(x => listExtraVehicleLog.Any(y
                    => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber)
                    && x.ApproveStatus == (short)ApproveStatus.Approved).ToList();
                if (listInVehicleLog.Count > 0)
                {
                    var listVehicleAttendanceLog = listInVehicleLog.Select(x => new WorkingLogResult
                    {

                        EmployeeATID = x.EmployeeATID,
                        //EmployeeATID = logUserInfoResult != null ? logUserInfoResult.TripId : string.Empty,
                        EmployeeCode = listExtraVehicleLog.FirstOrDefault(y
                            => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber)?.EmployeeCode ?? string.Empty,
                        FullName = listExtraVehicleLog.FirstOrDefault(y
                            => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber)?.FullName ?? string.Empty,
                        UserType = (short)EmployeeType.Driver,
                        UserTypeName = "Phụ xế",
                        SerialNumber = x.MachineSerial,
                        Time = x.Time,
                        InOutMode = x.InOutMode.Value.GetGCSInOutModeString(),
                        CardNumber = x.CardNumber,
                        TimeString = x.Time.ToString("dd/MM/yyy HH:mm:ss"),
                        IsExtraDriver = true
                    });
                    listAttendanceLog.AddRange(listVehicleAttendanceLog);
                }
            }

            var listInAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "In" || x.InOutMode == "BreakIn").ToList();
            var listOutAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "Out" || x.InOutMode == "BreakOut").ToList();

            var listInNotOutAttendanceLogs = listInAttendanceLog.Where(x
                => !listOutAttendanceLog.Any(y => y.EmployeeATID == x.EmployeeATID && y.Time >= x.Time
                && y.CardNumber == x.CardNumber)).ToList();

            //if (existedExtraTruckDriver != null && existedExtraTruckDriver.Count > 0)
            //{
            //    var listExtraVehicleLog = existedExtraTruckDriver.Select(x => new WorkingLogResult
            //    {
            //        EmployeeATID = x.TripCode,
            //        //EmployeeATID = logUserInfoResult != null ? logUserInfoResult.TripId : string.Empty,
            //        EmployeeCode = x.ExtraDriverCode,
            //        FullName = x.ExtraDriverName,
            //        UserType = (short)EmployeeType.Driver,
            //        UserTypeName = "Phụ xế",
            //        SerialNumber = existedTruckDriver.Any(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input)
            //            ? existedTruckDriver.FirstOrDefault(y => y.TripCode == x.TripCode 
            //            && y.InOutMode == (short)InOutMode.Input).MachineSerial : string.Empty,
            //        Time = existedTruckDriver.Any(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input)
            //            ? existedTruckDriver.FirstOrDefault(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input).Time 
            //            : new DateTime(),
            //        InOutMode = "In",
            //        CardNumber = x.CardNumber,
            //        TimeString = (existedTruckDriver.Any(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input)
            //            ? existedTruckDriver.FirstOrDefault(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Input).Time 
            //            : new DateTime()).ToString("dd/MM/yyy HH:mm:ss"),
            //        IsExtraDriver = true
            //    }).ToList();
            //    listExtraVehicleLog.ForEach(x =>
            //    {
            //        if (!string.IsNullOrEmpty(x.SerialNumber) && deviceList.Any(y => y.SerialNumber == x.SerialNumber))
            //        {
            //            x.DeviceName = deviceList.FirstOrDefault(y => y.SerialNumber == x.SerialNumber).AliasName;
            //        }
            //    });
            //    listInNotOutAttendanceLogs.AddRange(listExtraVehicleLog);
            //}

            //if (listIntegratedVehicleInNotOutLogs.Count > 0) 
            //{
            //    var filterListIntegratedInNotOut = listIntegratedVehicleInNotOutLogs.Where(x
            //        => !listOutAttendanceLog.Any(y => y.EmployeeATID == x.EmployeeATID && y.Time > x.Time)
            //        //&& (!listInNotOutAttendanceLogs.Any(y => y.EmployeeATID == x.EmployeeATID)
            //        //|| listInNotOutAttendanceLogs.Any(y => y.EmployeeATID == x.EmployeeATID && x.Time > y.Time))
            //        ).ToList();
            //    if (filterListIntegratedInNotOut != null && filterListIntegratedInNotOut.Count > 0)
            //    {
            //        listInNotOutAttendanceLogs.AddRange(filterListIntegratedInNotOut);
            //    }
            //}         

            //if (listVehicleLog.Count > 0)
            //{
            //    listVehicleLog = listVehicleLog.Where(x => !listVehicleLog.Any(y => y.EmployeeATID == x.EmployeeATID
            //        && (y.InOutMode == "Out" || y.InOutMode == "Out"))).ToList();

            //    if (listVehicleLog != null && listVehicleLog.Count > 0)
            //    {
            //        var filterListVehicleLog = listVehicleLog.Where(x
            //            => !listOutAttendanceLog.Any(y => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber && y.Time > x.Time))
            //            .ToList();

            //        if (filterListVehicleLog != null && filterListVehicleLog.Count > 0)
            //        {
            //            listInNotOutAttendanceLogs.AddRange(filterListVehicleLog);
            //        }

            //        var vehicleLogTripCode = listVehicleLog.Select(x => x.EmployeeATID).ToList();
            //        var listExtraDriverLog = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
            //            => vehicleLogTripCode.Contains(x.TripCode)).ToListAsync();

            //        if (listExtraDriverLog != null && listExtraDriverLog.Count > 0)
            //        {
            //            var listExtraVehicleLog = listExtraDriverLog.Select(x => new WorkingLogResult
            //            {
            //                EmployeeATID = x.TripCode,
            //                EmployeeCode = x.ExtraDriverCode,
            //                FullName = x.ExtraDriverName,
            //                UserType = (short)EmployeeType.Driver,
            //                UserTypeName = "Phụ xế",
            //                IsExtraDriver = true,
            //                SerialNumber = listVehicleLog.FirstOrDefault(y => y.EmployeeATID == x.TripCode)?.SerialNumber ?? string.Empty,
            //                DeviceName = listVehicleLog.FirstOrDefault(y => y.EmployeeATID == x.TripCode)?.DeviceName ?? string.Empty,
            //                Time = listVehicleLog.FirstOrDefault(y => y.EmployeeATID == x.TripCode)?.Time ?? new DateTime(),
            //                InOutMode = "In",
            //                CardNumber = x.CardNumber,
            //                TimeString = (listVehicleLog.FirstOrDefault(y => y.EmployeeATID == x.TripCode)?.Time ?? new DateTime())
            //                    .ToString("dd/MM/yyy HH:mm:ss"),
            //            });

            //            var filterListExtraVehicleLog = listExtraVehicleLog.Where(x
            //                => !listOutAttendanceLog.Any(y => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber && y.Time > x.Time))
            //                .ToList();

            //            if (filterListExtraVehicleLog != null && filterListExtraVehicleLog.Count > 0)
            //            {
            //                listInNotOutAttendanceLogs.ForEach(x =>
            //                {
            //                    if (filterListExtraVehicleLog.Any(y => y.EmployeeATID == x.EmployeeATID && y.CardNumber == x.CardNumber))
            //                    {
            //                        x.IsExtraDriver = true;
            //                    }
            //                });
            //                listInNotOutAttendanceLogs.AddRange(filterListExtraVehicleLog);
            //            }
            //        }
            //    }
            //}

            listInNotOutAttendanceLogs = listInNotOutAttendanceLogs.OrderByDescending(x => x.Time).GroupBy(x
                => new { x.EmployeeATID, x.CardNumber }).Select(x => x.FirstOrDefault()).ToList();
            listInNotOutAttendanceLogs.RemoveAll(x => x == null);

            var listFilterAttendanceLog = new List<WorkingLogResult>();
            listFilterAttendanceLog.AddRange(listInNotOutAttendanceLogs);

            listFilterAttendanceLog = listFilterAttendanceLog.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null
                && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();

            var listEmployeeATID = listFilterAttendanceLog.Select(x => x.EmployeeATID).ToList();
            listEmployeeATID = listEmployeeATID.Where(x => existedTruckDriver.Any(y => y.TripCode == x && !y.IsInactive)).ToList();
            var existedTruckDriverLog = await DbContext.IC_PlanDock.AsNoTracking().Where(x
                => listEmployeeATID.Contains(x.TripId)).ToListAsync();
            var existedExtraTruckDriverLog = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                => listEmployeeATID.Contains(x.TripCode) && !x.IsInactive).ToListAsync();

            listFilterAttendanceLog.ForEach(x =>
            {
                if (existedTruckDriverLog.Any(y => y.TripId == x.EmployeeATID))
                {
                    x.UserType = (short)EmployeeType.Driver;
                    x.FullName = existedTruckDriverLog.FirstOrDefault(y => y.TripId == x.EmployeeATID).DriverName;
                    x.UserTypeName = "Tài xế";
                }
                if (existedExtraTruckDriverLog.Any(y => y.TripCode == x.EmployeeATID && y.CardNumber == x.CardNumber))
                {
                    x.UserType = (short)EmployeeType.Driver;
                    x.FullName = existedExtraTruckDriverLog.FirstOrDefault(y => y.TripCode == x.EmployeeATID && y.CardNumber == x.CardNumber).ExtraDriverName;
                    x.UserTypeName = "Phụ xế";
                    x.IsExtraDriver = true;
                }
            });

            var listSerialNumber = new List<string>();
            if (listFilterAttendanceLog != null && listFilterAttendanceLog.Count > 0)
            {
                listSerialNumber = listFilterAttendanceLog.Select(x => x.SerialNumber).Distinct().ToList();
            }
            var listLineIndex = new List<int>();
            var listACDoorIndex = new List<int>();
            var listLineIn = new List<GC_Lines_CheckInDevice>();
            var listLineOut = new List<GC_Lines_CheckOutDevice>();
            var listACDoorAndDevice = new List<AC_DoorAndDevice>();
            if (listSerialNumber != null && listSerialNumber.Count > 0)
            {
                listLineIn = await DbContext.GC_Lines_CheckInDevice.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.CheckInDeviceSerial)).ToListAsync();
                listLineOut = await DbContext.GC_Lines_CheckOutDevice.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.CheckOutDeviceSerial)).ToListAsync();
                listACDoorAndDevice = await DbContext.AC_DoorAndDevice.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.SerialNumber)).ToListAsync();
                if (listLineIn != null && listLineIn.Count > 0)
                {
                    listLineIndex.AddRange(listLineIn.Select(x => x.LineIndex).ToList());
                }
                if (listLineOut != null && listLineOut.Count > 0)
                {
                    listLineIndex.AddRange(listLineOut.Select(x => x.LineIndex).ToList());
                }
                if (listACDoorAndDevice != null && listACDoorAndDevice.Count > 0)
                {
                    listACDoorIndex.AddRange(listACDoorAndDevice.Select(x => x.DoorIndex).ToList());
                }
            }

            var listGroupDeviceIndex = new List<int>();
            var listGroupDeviceDetails = new List<IC_GroupDeviceDetails>();
            if (listSerialNumber != null && listSerialNumber.Count > 0)
            {
                listGroupDeviceDetails = await DbContext.IC_GroupDeviceDetails.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.SerialNumber)).ToListAsync();
                if (listGroupDeviceDetails != null && listGroupDeviceDetails.Count > 0)
                {
                    listGroupDeviceIndex = listGroupDeviceDetails.Select(x => x.GroupDeviceIndex).Distinct().ToList();
                }
            }

            var listAreaGroupIndex = new List<int>();
            var listAreaGroupDetail = new List<GC_AreaGroup_GroupDevice>();
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listAreaGroupDetail = await DbContext.GC_AreaGroup_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.DeviceGroupIndex)).ToListAsync();
                if (listAreaGroupDetail != null && listAreaGroupDetail.Count > 0)
                {
                    listAreaGroupIndex = listAreaGroupDetail.Select(x => x.AreaGroupIndex).Distinct().ToList();
                }
            }

            var listGateIndex = new List<int>();
            var listGateLine = new List<GC_Gates_Lines>();
            if (listLineIndex != null && listLineIndex.Count > 0)
            {
                listGateLine = await DbContext.GC_Gates_Lines.AsNoTracking().Where(x
                    => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                if (listGateLine != null && listGateLine.Count > 0)
                {
                    listGateIndex = listGateLine.Select(x => x.GateIndex).Distinct().ToList();
                }
            }

            var listLine = new List<GC_Lines>();
            var listGate = new List<GC_Gates>();
            var listACDoor = new List<AC_Door>();
            var listGroupDevice = new List<IC_GroupDevice>();
            var listAreaGroup = new List<GC_AreaGroup>();
            if (listLineIndex != null && listLineIndex.Count > 0)
            {
                listLine = await DbContext.GC_Lines.AsNoTracking().Where(x
                    => listLineIndex.Contains(x.Index)).ToListAsync();
            }
            if (listGateIndex != null && listGateIndex.Count > 0)
            {
                listGate = await DbContext.GC_Gates.AsNoTracking().Where(x
                    => listGateIndex.Contains(x.Index)).ToListAsync();
            }
            if (listACDoorIndex != null && listACDoorIndex.Count > 0)
            {
                listACDoor = await DbContext.AC_Door.AsNoTracking().Where(x
                    => listACDoorIndex.Contains(x.Index)).ToListAsync();
            }
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listGroupDevice = await DbContext.IC_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.Index)).ToListAsync();
            }
            if (listAreaGroupIndex != null && listAreaGroupIndex.Count > 0)
            {
                listAreaGroup = await DbContext.GC_AreaGroup.AsNoTracking().Where(x
                    => listAreaGroupIndex.Contains(x.Index)).ToListAsync();
            }

            var logLine = new GC_Lines();
            var logGate = new List<GC_Gates>();
            var logGroupDevice = new List<IC_GroupDevice>();
            var logAreaGroup = new List<GC_AreaGroup>();
            listFilterAttendanceLog.ForEach(x =>
            {
                if (!x.IsIntegratedLog)
                {
                    logLine = new GC_Lines();
                    logGate = new List<GC_Gates>();
                    logGroupDevice = new List<IC_GroupDevice>();
                    logAreaGroup = new List<GC_AreaGroup>();

                    var logLineIn = listLineIn.FirstOrDefault(y => y.CheckInDeviceSerial == x.SerialNumber);
                    var logLineOut = listLineOut.FirstOrDefault(y => y.CheckOutDeviceSerial == x.SerialNumber);
                    var logLineIndex = 0;
                    if (logLineIn != null)
                    {
                        logLine = listLine.FirstOrDefault(y => y.Index == logLineIn.LineIndex);
                        logLineIndex = logLineIn.LineIndex;
                        x.LineIndex = logLine.Index;
                        x.LineName = logLine.Name;
                    }
                    else if (logLineOut != null)
                    {
                        logLine = listLine.FirstOrDefault(y => y.Index == logLineOut.LineIndex);
                        logLineIndex = logLineOut.LineIndex;
                        x.LineIndex = logLine.Index;
                        x.LineName = logLine.Name;
                    }
                    var logGateLine = listGateLine.Where(y => y.LineIndex == logLineIndex).ToList();
                    var logACDoorAndDevice = listACDoorAndDevice.FirstOrDefault(y => x.SerialNumber == y.SerialNumber);
                    if (logGateLine != null && logGateLine.Count > 0)
                    {
                        var gateIndex = logGateLine.Select(y => y.GateIndex).ToList();
                        logGate = listGate.Where(y => gateIndex.Contains(y.Index)).ToList();
                        x.GateIndex = logGate.Select(x => x.Index).ToList();
                        x.GateName = logGate.Select(x => x.Name).ToList();
                    }
                    else if (logACDoorAndDevice != null)
                    {
                        var doorIndex = logACDoorAndDevice.DoorIndex;
                        var door = listACDoor.FirstOrDefault(x => x.Index == doorIndex);
                        x.GateIndex = door != null ? new List<int> { door.Index } : new List<int>();
                        x.GateName = door != null ? new List<string> { door.Name } : new List<string>();
                    }
                    else
                    {
                        x.GateIndex = new List<int>();
                        x.GateName = new List<string> { x.DeviceName };
                    }

                    var logGroupDeviceIndex = listGroupDeviceDetails.Where(y => y.SerialNumber == x.SerialNumber)
                        .Select(x => x.GroupDeviceIndex).ToList();
                    if (logGroupDeviceIndex != null && logGroupDeviceIndex.Count > 0)
                    {
                        x.GroupDeviceIndex = logGroupDeviceIndex;
                        logGroupDevice = listGroupDevice.Where(g => listAreaGroupIndex.Contains(g.Index)).ToList();
                        if (logGroupDevice != null && logGroupDevice.Count > 0)
                        {
                            x.GroupDeviceName = logGroupDevice.Select(x => x.Name).ToList();
                        }

                        var logAreaGroupIndex = listAreaGroupDetail.Where(y => logGroupDeviceIndex.Contains(y.DeviceGroupIndex))
                            .Select(x => x.AreaGroupIndex).ToList();
                        if (logAreaGroupIndex != null && logAreaGroupIndex.Count > 0)
                        {
                            x.AreaGroupIndex = logAreaGroupIndex;
                            logAreaGroup = listAreaGroup.Where(g => logAreaGroupIndex.Contains(g.Index)).ToList();
                            if (logAreaGroup != null && logAreaGroup.Count > 0)
                            {
                                x.AreaGroupName = logAreaGroup.Select(x => x.Name).ToList();
                            }
                        }
                    }
                }
            });

            if (rule != null && rule.PresenceTrackingTime > 0)
            {
                var now = DateTime.Now;
                listFilterAttendanceLog = listFilterAttendanceLog.Where(x
                    => now.Subtract(x.Time).TotalMinutes <= Convert.ToDouble(rule.PresenceTrackingTime * 60)).ToList();
            }
            var contractorDepartments = DbContext.IC_Department.Where(x => x.IsContractorDepartment == true).ToList();
            var parentContractorDepartment = contractorDepartments.FirstOrDefault(x => x.ParentIndex == null || x.ParentIndex == 0);
            var isMondelezContractor = false;
            var listFirstContractorDepartment = new List<long>();
            var listSecondContractorDepartment = new List<long>();
            var firstDepartment = "";
            var secondDepartment = "";
            if (parentContractorDepartment != null )
            {
                var lstDepartmentChild = contractorDepartments.Where(x => parentContractorDepartment.Index == x.ParentIndex).ToList();
                if(lstDepartmentChild != null && lstDepartmentChild.Count == 2)
                {
                    firstDepartment = lstDepartmentChild[0].Name;
                    secondDepartment = lstDepartmentChild[1].Name;
                    isMondelezContractor = true;
                    var alllistFirstContractorDepartment = GetChildren(contractorDepartments, lstDepartmentChild[0].Index);
                    listFirstContractorDepartment = alllistFirstContractorDepartment.Select(x => (long)x.Index).ToList();
                    var alllistSecondContractorDepartment = GetChildren(contractorDepartments, lstDepartmentChild[1].Index);
                    listSecondContractorDepartment = alllistSecondContractorDepartment.Select(x => (long)x.Index).ToList();
                }
            }

            var listEmployeeLogs = listFilterAttendanceLog.Where(x => x.UserType == (short)EmployeeType.Employee).ToList();
            var listCustomerLogs = listFilterAttendanceLog.Where(x => x.UserType == (short)EmployeeType.Guest).ToList();
            var listContractorLogs = listFilterAttendanceLog.Where(x => x.UserType == (short)EmployeeType.Contractor).ToList();
            var listDriverLogs = listFilterAttendanceLog.Where(x => x.UserType == (short)EmployeeType.Driver && !x.IsExtraDriver).ToList();
            var listExtraDriverLogs = listFilterAttendanceLog.Where(x => x.UserType == (short)EmployeeType.Driver && x.IsExtraDriver).ToList();

            if (isMondelezContractor)
            {
                var listFirstContractorLogs = new List<WorkingLogResult>();
                var listSecondContractorLogs = new List<WorkingLogResult>();
                var lstUserType = new List<string> { "Employee", "Customer", firstDepartment,secondDepartment, "Driver", "ExtraDriver" };
                listFirstContractorLogs = listContractorLogs.Where(x => listFirstContractorDepartment.Contains(x.DepartmentIndex)).ToList();
                listSecondContractorLogs = listContractorLogs.Where(x => listSecondContractorDepartment.Contains(x.DepartmentIndex)).ToList();
                var result = new Tuple<List<string>, List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>(
             lstUserType,listEmployeeLogs, listCustomerLogs, listFirstContractorLogs, listSecondContractorLogs, listDriverLogs, listExtraDriverLogs);

                return result;
            }
            else
            {
                var lstUserType = new List<string> { "Employee", "Customer", "Contractor", "Driver", "ExtraDriver" };
                var result = new Tuple<List<string>, List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>(
           lstUserType, listEmployeeLogs, listCustomerLogs, listContractorLogs, listDriverLogs, listExtraDriverLogs);

                return result;
            }
        }

        private List<IC_Department> GetChildren(List<IC_Department> foos, long id)
        {
            return foos
                .Where(x => x.ParentIndex == id)
                .Union(foos.Where(x => x.ParentIndex == id)
                    .SelectMany(y => GetChildren(foos, y.Index))
                ).ToList();
        }

        public async Task<Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>>
            GetTupleFullWorkingEmployeeByRootDepartment(UserInfo user)
        {
            var attendanceLogs = from att in DbContext.GC_TimeLog.Where(x
                                    => x.Time.Date == DateTime.Now.Date)
                                 join u in DbContext.HR_User
                                 on att.EmployeeATID equals u.EmployeeATID
                                 into logUserInfo
                                 from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                 join ut in DbContext.HR_UserType
                                 on logUserInfoResult.EmployeeType equals ut.UserTypeId
                                 into uutInfo
                                 from uutResult in uutInfo.DefaultIfEmpty()
                                 join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                 && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                                 on att.EmployeeATID equals wi.EmployeeATID
                                 into attInfo
                                 from attResult in attInfo.DefaultIfEmpty()
                                 join dp in DbContext.IC_Department
                                 on attResult.DepartmentIndex equals dp.Index into dep
                                 from depResult in dep.DefaultIfEmpty()
                                 join d in DbContext.IC_Device.Where(x => x.DeviceModule != "ICMS")
                                 on att.MachineSerial equals d.SerialNumber
                                 into logDeviceInfo
                                 from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                 join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                 on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                 from empCard in employeeCard.DefaultIfEmpty()
                                 select new WorkingLogResult
                                 {
                                     EmployeeATID = att.EmployeeATID,
                                     EmployeeCode = logUserInfoResult != null ? logUserInfoResult.EmployeeCode : string.Empty,
                                     FullName = logUserInfoResult != null ? logUserInfoResult.FullName : string.Empty,
                                     UserType = logUserInfoResult != null
                                         ? (logUserInfoResult.EmployeeType.HasValue
                                         ? logUserInfoResult.EmployeeType.Value : (short)EmployeeType.Employee)
                                         : (short)EmployeeType.Employee,
                                     UserTypeName = uutResult != null ? uutResult.Name : "Nhân viên",
                                     DepartmentIndex = attResult != null ? attResult.DepartmentIndex : 0,
                                     RootDepartment = (depResult == null || (depResult != null && (!depResult.ParentIndex.HasValue
                                        || (depResult.ParentIndex.HasValue && (depResult.ParentIndex.Value == 0 || depResult.ParentIndex.Value == depResult.Index))))),
                                     DepartmentName = depResult != null ? depResult.Name : "NoDepartment",
                                     SerialNumber = att.MachineSerial,
                                     DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                     Time = att.Time,
                                     InOutMode = att.InOutMode.Value.GetGCSInOutModeString(),
                                     VerifyMode = att.VerifyMode,
                                     //FaceMask = att.FaceMask.GetFaceMaskString(),
                                     //BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                     //IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                     CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                                     TimeString = att.Time.ToString("dd/MM/yyy HH:mm:ss")
                                 };

            var listAttendanceLog = await attendanceLogs.ToListAsync();

            var listInAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "In" || x.InOutMode == "BreakIn").ToList();
            var listOutAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "Out" || x.InOutMode == "BreakOut").ToList();

            listInAttendanceLog = listInAttendanceLog.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                => x.FirstOrDefault()).ToList();
            listInAttendanceLog.RemoveAll(x => x == null);
            listOutAttendanceLog = listOutAttendanceLog.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                => x.FirstOrDefault()).ToList();
            listOutAttendanceLog.RemoveAll(x => x == null);

            var listFilterAttendanceLog = new List<WorkingLogResult>();
            listFilterAttendanceLog.AddRange(listInAttendanceLog);
            listFilterAttendanceLog.AddRange(listOutAttendanceLog);

            var listSerialNumber = new List<string>();
            if (listFilterAttendanceLog != null && listFilterAttendanceLog.Count > 0)
            {
                listSerialNumber = listFilterAttendanceLog.Select(x => x.SerialNumber).Distinct().ToList();
            }
            var listGroupDeviceIndex = new List<int>();
            var listGroupDeviceDetails = new List<IC_GroupDeviceDetails>();
            if (listSerialNumber != null && listSerialNumber.Count > 0)
            {
                listGroupDeviceDetails = await DbContext.IC_GroupDeviceDetails.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.SerialNumber)).ToListAsync();
                if (listGroupDeviceDetails != null && listGroupDeviceDetails.Count > 0)
                {
                    listGroupDeviceIndex = listGroupDeviceDetails.Select(x => x.GroupDeviceIndex).Distinct().ToList();
                }
            }
            var listAreaGroupIndex = new List<int>();
            var listAreaGroupDetail = new List<GC_AreaGroup_GroupDevice>();
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listAreaGroupDetail = await DbContext.GC_AreaGroup_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.DeviceGroupIndex)).ToListAsync();
                if (listAreaGroupDetail != null && listAreaGroupDetail.Count > 0)
                {
                    listAreaGroupIndex = listAreaGroupDetail.Select(x => x.AreaGroupIndex).Distinct().ToList();
                }
            }

            var listGroupDevice = new List<IC_GroupDevice>();
            var listAreaGroup = new List<GC_AreaGroup>();
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listGroupDevice = await DbContext.IC_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.Index)).ToListAsync();
            }
            if (listAreaGroupIndex != null && listAreaGroupIndex.Count > 0)
            {
                listAreaGroup = await DbContext.GC_AreaGroup.AsNoTracking().Where(x
                    => listAreaGroupIndex.Contains(x.Index)).ToListAsync();
            }

            var logGroupDevice = new List<IC_GroupDevice>();
            var logAreaGroup = new List<GC_AreaGroup>();

            var allDepartment = await DbContext.IC_Department.AsNoTracking().ToListAsync();
            listFilterAttendanceLog.ForEach(x =>
            {
                logGroupDevice = new List<IC_GroupDevice>();
                logAreaGroup = new List<GC_AreaGroup>();

                var logGroupDeviceIndex = listGroupDeviceDetails.Where(y => y.SerialNumber == x.SerialNumber)
                    .Select(x => x.GroupDeviceIndex).ToList();
                if (logGroupDeviceIndex != null && logGroupDeviceIndex.Count > 0)
                {
                    x.GroupDeviceIndex = logGroupDeviceIndex;
                    logGroupDevice = listGroupDevice.Where(g => listAreaGroupIndex.Contains(g.Index)).ToList();
                    if (logGroupDevice != null && logGroupDevice.Count > 0)
                    {
                        x.GroupDeviceName = logGroupDevice.Select(x => x.Name).ToList();
                    }

                    var logAreaGroupIndex = listAreaGroupDetail.Where(y => logGroupDeviceIndex.Contains(y.DeviceGroupIndex))
                        .Select(x => x.AreaGroupIndex).ToList();
                    if (logAreaGroupIndex != null && logAreaGroupIndex.Count > 0)
                    {
                        x.AreaGroupIndex = logAreaGroupIndex;
                        logAreaGroup = listAreaGroup.Where(g => logAreaGroupIndex.Contains(g.Index)).ToList();
                        if (logAreaGroup != null && logAreaGroup.Count > 0)
                        {
                            x.AreaGroupName = logAreaGroup.Select(x => x.Name).ToList();
                        }
                    }
                }

                var rootDepartmentIndex = FindRootDepartmentIndex(x.DepartmentIndex, allDepartment);
                if (rootDepartmentIndex != 0 && rootDepartmentIndex != x.DepartmentIndex)
                {
                    x.DepartmentIndex = rootDepartmentIndex;
                    x.DepartmentName = allDepartment.FirstOrDefault(x => x.Index == rootDepartmentIndex)?.Name ?? "NoDepartment";
                }
            });

            listFilterAttendanceLog = listFilterAttendanceLog.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null
                && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();

            var listLogIn = listFilterAttendanceLog.Where(x => x.InOutMode == "In" || x.InOutMode == "BreakIn").ToList();
            var listLogOut = listFilterAttendanceLog.Where(x => x.InOutMode == "Out" || x.InOutMode == "BreakOut").ToList();
            var listLogRemain = listLogIn.Where(x => !listLogOut.Any(y => y.EmployeeATID == x.EmployeeATID)
                || listLogOut.Any(y => y.EmployeeATID == x.EmployeeATID && y.Time < x.Time)).ToList();

            var result = new Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>(
                listLogIn, listLogOut, listLogRemain);

            return result;
        }

        public async Task<List<WorkingLogResult>> GetFullWorkingEmployeeByRootDepartment(UserInfo user)
        {
            var attendanceLogs = from att in DbContext.IC_AttendanceLog.Where(x
                                    => x.CheckTime.Date == DateTime.Now.Date)
                                 join u in DbContext.HR_User
                                 on att.EmployeeATID equals u.EmployeeATID
                                 into logUserInfo
                                 from logUserInfoResult in logUserInfo.DefaultIfEmpty()
                                 join ut in DbContext.HR_UserType
                                 on logUserInfoResult.EmployeeType equals ut.UserTypeId
                                 into uutInfo
                                 from uutResult in uutInfo.DefaultIfEmpty()
                                 join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                 && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                                 on att.EmployeeATID equals wi.EmployeeATID
                                 join dp in DbContext.IC_Department
                                 on wi.DepartmentIndex equals dp.Index into dep
                                 from depResult in dep.DefaultIfEmpty()
                                 join d in DbContext.IC_Device.Where(x => x.DeviceModule != "ICMS")
                                 on att.SerialNumber equals d.SerialNumber
                                 into logDeviceInfo
                                 from logDeviceInfoResult in logDeviceInfo.DefaultIfEmpty()
                                 join c in DbContext.HR_CardNumberInfo.Where(x => x.IsActive == true)
                                 on att.EmployeeATID equals c.EmployeeATID into employeeCard
                                 from empCard in employeeCard.DefaultIfEmpty()
                                 select new WorkingLogResult
                                 {
                                     EmployeeATID = att.EmployeeATID,
                                     EmployeeCode = logUserInfoResult != null ? logUserInfoResult.EmployeeCode : string.Empty,
                                     FullName = logUserInfoResult != null ? logUserInfoResult.FullName : string.Empty,
                                     UserType = logUserInfoResult != null
                                         ? (logUserInfoResult.EmployeeType.HasValue
                                         ? logUserInfoResult.EmployeeType.Value : (short)EmployeeType.Employee)
                                         : (short)EmployeeType.Employee,
                                     UserTypeName = uutResult != null ? uutResult.Name : "Nhân viên",
                                     DepartmentIndex = wi.DepartmentIndex,
                                     RootDepartment = (depResult == null || (depResult != null && (!depResult.ParentIndex.HasValue
                                        || (depResult.ParentIndex.HasValue && (depResult.ParentIndex.Value == 0 || depResult.ParentIndex.Value == depResult.Index))))),
                                     DepartmentName = depResult != null ? depResult.Name : "NoDepartment",
                                     SerialNumber = att.SerialNumber,
                                     DeviceName = logDeviceInfoResult != null ? logDeviceInfoResult.AliasName : string.Empty,
                                     Time = att.CheckTime,
                                     InOutMode = att.InOutMode.GetInOutModeString(),
                                     VerifyMode = att.VerifyMode.GetVerifyModeString(),
                                     FaceMask = att.FaceMask.GetFaceMaskString(),
                                     BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                     IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                     CardNumber = empCard != null ? empCard.CardNumber : string.Empty,
                                     TimeString = att.CheckTime.ToString("dd/MM/yyy HH:mm:ss")
                                 };

            var listAttendanceLog = await attendanceLogs.ToListAsync();

            var listInAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "In" || x.InOutMode == "BreakIn").ToList();
            var listOutAttendanceLog = listAttendanceLog.Where(x => x.InOutMode == "Out" || x.InOutMode == "BreakOut").ToList();

            listInAttendanceLog = listInAttendanceLog.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                => x.FirstOrDefault()).ToList();
            listInAttendanceLog.RemoveAll(x => x == null);
            listOutAttendanceLog = listOutAttendanceLog.OrderByDescending(x => x.Time).GroupBy(x => x.EmployeeATID).Select(x
                => x.FirstOrDefault()).ToList();
            listOutAttendanceLog.RemoveAll(x => x == null);

            var listFilterAttendanceLog = new List<WorkingLogResult>();
            listFilterAttendanceLog.AddRange(listInAttendanceLog);
            listFilterAttendanceLog.AddRange(listOutAttendanceLog);

            var listSerialNumber = new List<string>();
            if (listFilterAttendanceLog != null && listFilterAttendanceLog.Count > 0)
            {
                listSerialNumber = listFilterAttendanceLog.Select(x => x.SerialNumber).Distinct().ToList();
            }
            var listGroupDeviceIndex = new List<int>();
            var listGroupDeviceDetails = new List<IC_GroupDeviceDetails>();
            if (listSerialNumber != null && listSerialNumber.Count > 0)
            {
                listGroupDeviceDetails = await DbContext.IC_GroupDeviceDetails.AsNoTracking().Where(x
                    => listSerialNumber.Contains(x.SerialNumber)).ToListAsync();
                if (listGroupDeviceDetails != null && listGroupDeviceDetails.Count > 0)
                {
                    listGroupDeviceIndex = listGroupDeviceDetails.Select(x => x.GroupDeviceIndex).Distinct().ToList();
                }
            }
            var listAreaGroupIndex = new List<int>();
            var listAreaGroupDetail = new List<GC_AreaGroup_GroupDevice>();
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listAreaGroupDetail = await DbContext.GC_AreaGroup_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.DeviceGroupIndex)).ToListAsync();
                if (listAreaGroupDetail != null && listAreaGroupDetail.Count > 0)
                {
                    listAreaGroupIndex = listAreaGroupDetail.Select(x => x.AreaGroupIndex).Distinct().ToList();
                }
            }

            var listGroupDevice = new List<IC_GroupDevice>();
            var listAreaGroup = new List<GC_AreaGroup>();
            if (listGroupDeviceIndex != null && listGroupDeviceIndex.Count > 0)
            {
                listGroupDevice = await DbContext.IC_GroupDevice.AsNoTracking().Where(x
                    => listGroupDeviceIndex.Contains(x.Index)).ToListAsync();
            }
            if (listAreaGroupIndex != null && listAreaGroupIndex.Count > 0)
            {
                listAreaGroup = await DbContext.GC_AreaGroup.AsNoTracking().Where(x
                    => listAreaGroupIndex.Contains(x.Index)).ToListAsync();
            }

            var logGroupDevice = new List<IC_GroupDevice>();
            var logAreaGroup = new List<GC_AreaGroup>();

            var allDepartment = await DbContext.IC_Department.AsNoTracking().ToListAsync();
            listFilterAttendanceLog.ForEach(x =>
            {
                logGroupDevice = new List<IC_GroupDevice>();
                logAreaGroup = new List<GC_AreaGroup>();

                var logGroupDeviceIndex = listGroupDeviceDetails.Where(y => y.SerialNumber == x.SerialNumber)
                    .Select(x => x.GroupDeviceIndex).ToList();
                if (logGroupDeviceIndex != null && logGroupDeviceIndex.Count > 0)
                {
                    x.GroupDeviceIndex = logGroupDeviceIndex;
                    logGroupDevice = listGroupDevice.Where(g => listAreaGroupIndex.Contains(g.Index)).ToList();
                    if (logGroupDevice != null && logGroupDevice.Count > 0)
                    {
                        x.GroupDeviceName = logGroupDevice.Select(x => x.Name).ToList();
                    }

                    var logAreaGroupIndex = listAreaGroupDetail.Where(y => logGroupDeviceIndex.Contains(y.DeviceGroupIndex))
                        .Select(x => x.AreaGroupIndex).ToList();
                    if (logAreaGroupIndex != null && logAreaGroupIndex.Count > 0)
                    {
                        x.AreaGroupIndex = logAreaGroupIndex;
                        logAreaGroup = listAreaGroup.Where(g => logAreaGroupIndex.Contains(g.Index)).ToList();
                        if (logAreaGroup != null && logAreaGroup.Count > 0)
                        {
                            x.AreaGroupName = logAreaGroup.Select(x => x.Name).ToList();
                        }
                    }
                }

                var rootDepartmentIndex = FindRootDepartmentIndex(x.DepartmentIndex, allDepartment);
                if (rootDepartmentIndex != 0 && rootDepartmentIndex != x.DepartmentIndex)
                {
                    x.DepartmentIndex = rootDepartmentIndex;
                    x.DepartmentName = allDepartment.FirstOrDefault(x => x.Index == rootDepartmentIndex)?.Name ?? "NoDepartment";
                }
            });

            listFilterAttendanceLog = listFilterAttendanceLog.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null
                && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();

            return listFilterAttendanceLog;
        }

        private long FindRootDepartmentIndex(long departmentIndex, List<IC_Department> listDepartment)
        {
            var result = departmentIndex;
            var department = listDepartment.FirstOrDefault(x => x.Index == departmentIndex);
            if (department != null && department.ParentIndex != null && department.ParentIndex > 0
                && department.ParentIndex != department.Index)
            {
                result = FindRootDepartmentIndex(department.ParentIndex.Value, listDepartment);
            }

            return result;
        }

        public List<AttendanceLogInfoResult> GetAttendanceLogByDeviceInCanteen(DateTime fromDate, DateTime toDate, int pCompanyIndex)
        {

            var attendanceLog = DbContext.IC_AttendanceLog.Where(x => x.CompanyIndex == pCompanyIndex && (x.CheckTime.Date >= fromDate.Date && x.CheckTime.Date <= toDate.Date));
            var attendanceLogs = from att in attendanceLog
                                 join dv in DbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex)
                        on att.SerialNumber equals dv.SerialNumber

                                 join emp in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                                            on att.EmployeeATID equals emp.EmployeeATID

                                 join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                 && w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date)
                                 on emp.EmployeeATID equals w.EmployeeATID into workingInfoGroup
                                 from wrk in workingInfoGroup.DefaultIfEmpty()

                                 join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on wrk.PositionIndex equals p.Index into positionGroup
                                 from pst in positionGroup.DefaultIfEmpty()

                                 join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on wrk.DepartmentIndex equals d.Index into deptGroup
                                 from dept in deptGroup.DefaultIfEmpty()
                                 join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                                 on emp.EmployeeATID equals c.EmployeeATID into employeeCard
                                 from empCard in employeeCard.DefaultIfEmpty()

                                 select new AttendanceLogInfoResult()
                                 {
                                     EmployeeATID = att.EmployeeATID,
                                     EmployeeCode = emp.EmployeeCode,
                                     FullName = emp.FullName,
                                     Time = att.CheckTime,
                                     CompanyIndex = pCompanyIndex,
                                     SerialNumber = att.SerialNumber,
                                     DeviceNumber = att.DeviceNumber,
                                     DeviceId = att.DeviceId,
                                     AliasName = dv.AliasName,
                                     IPAddress = dv.IPAddress,
                                     DepartmentName = dept == null ? "" : dept.Name,
                                     PositionName = pst == null ? "" : pst.Name,
                                     PositionIndex = pst == null ? 0 : pst.Index,
                                     VerifyMode = att.VerifyMode.GetVerifyModeString(),
                                     FaceMask = att.FaceMask.GetFaceMaskString(),
                                     BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                                     IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                                     CardNumber = empCard == null ? "" : empCard.CardNumber
                                 };
            return attendanceLogs.ToList();
        }

        public async Task<DataGridClass> GetACAttendanceLog(string pFilter, DateTime fromDate, DateTime toDate, List<long> pDepartmentIds, int pCompanyIndex, int pPageIndex, int pPageSize, List<int> listArea, List<int> listDoor)
        {
            var attendanceLog = DbContext.IC_AttendanceLog.Where(x => x.CompanyIndex == pCompanyIndex && (x.CheckTime >= fromDate && x.CheckTime <= toDate));
            //var attendanceLogIntegrate = DbContext.IC_AttendancelogIntegrate.Where(x => x.CompanyIndex == pCompanyIndex && (x.CheckTime >= fromDate && x.CheckTime <= toDate)).ToList();
            //var allLogMapp = _Mapper.Map<List<IC_AttendanceLog>>(attendanceLogIntegrate);
            //attendanceLog.AddRange(allLogMapp);
            var filterBy = new List<string>();
            if (pFilter != null)
            {
                filterBy = pFilter.Split(" ").ToList();
            }

            var obj = from att in attendanceLog
                      join dv in DbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex)
                        on att.SerialNumber equals dv.SerialNumber

                      join wi in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on att.EmployeeATID equals wi.EmployeeATID into eWork

                      from eWorkResult in eWork.DefaultIfEmpty()
                      join emp in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                        on att.EmployeeATID equals emp.EmployeeATID

                      join dp in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                        on eWorkResult.DepartmentIndex equals dp.Index into temp
                      from dummy in temp.DefaultIfEmpty()

                      join poInfo in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on eWorkResult.PositionIndex equals poInfo.Index into item
                      from position in item.DefaultIfEmpty()

                      join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                            on att.EmployeeATID equals c.EmployeeATID into cWork
                      from cResult in cWork.DefaultIfEmpty()

                      join k in DbContext.AC_DoorAndDevice.Where(x => x.CompanyIndex == pCompanyIndex)
                            on att.SerialNumber equals k.SerialNumber into kDoorDevice
                      from kResult in kDoorDevice.DefaultIfEmpty()

                      join d in DbContext.AC_Door.Where(x => x.CompanyIndex == pCompanyIndex)
                         on kResult.DoorIndex equals d.Index into dDoor
                      from dResult in dDoor.DefaultIfEmpty()

                      join p in DbContext.AC_AreaAndDoor.Where(x => x.CompanyIndex == pCompanyIndex)
                           on dResult.Index equals p.DoorIndex into pAreaDoor
                      from pResult in pAreaDoor.DefaultIfEmpty()

                      join a in DbContext.AC_Area.Where(x => x.CompanyIndex == pCompanyIndex)
                         on pResult.AreaIndex equals a.Index into aArea
                      from aResult in aArea.DefaultIfEmpty()

                      where
                       (string.IsNullOrEmpty(pFilter)
                            ? emp.EmployeeATID.Contains("")
                            : (
                                   att.EmployeeATID.Contains(pFilter)
                                || (!string.IsNullOrEmpty(emp.EmployeeCode) && emp.EmployeeCode.Contains(pFilter))
                                || emp.FullName.Contains(pFilter)
                                //|| dummy.Name.Contains(pFilter)
                                || att.SerialNumber.Contains(pFilter)
                                || filterBy.Contains(att.EmployeeATID)
                            ))
                        && dv.DeviceModule == "AC"
                      && ((listArea != null && listArea.Count > 0) ? listArea.Any(y => y.Equals(aResult.Index)) : true)
                      && ((pDepartmentIds != null && pDepartmentIds.Count > 0) ? pDepartmentIds.Any(y => y.Equals(eWorkResult.DepartmentIndex)) : true)
                       && ((listDoor != null && listDoor.Count > 0) ? listDoor.Any(y => y.Equals(dResult.Index)) : true)
                      select new AttendanceACLogInfoResult()
                      {
                          EmployeeATID = att.EmployeeATID,
                          EmployeeCode = emp.EmployeeCode,
                          FullName = emp.FullName,
                          Time = att.CheckTime,
                          SerialNumber = att.SerialNumber,
                          DeviceNumber = att.DeviceNumber,
                          AliasName = dv.AliasName,
                          IPAddress = dv.IPAddress,
                          DepartmentName = dummy == null ? "" : dummy.Name,
                          PositionName = position == null ? "" : position.Name,
                          InOutMode = att.InOutMode.GetInOutModeString() == "In" ? "Vào" : "Ra",
                          VerifyMode = att.VerifyMode.GetVerifyModeString(),
                          FaceMask = att.FaceMask.GetFaceMaskString(),
                          BodyTemperature = att.BodyTemperature.GetBodyTemperatureString(),
                          IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(att.BodyTemperature, defaultBodyTemperature.Value),
                          CardNumber = cResult == null ? "" : cResult.CardNumber,
                          AreaName = aResult == null ? "" : aResult.Name,
                          DoorName = dResult == null ? "" : dResult.Name,
                          TimeString = att.CheckTime.ToString("dd/MM/yyyy hh:mm:ss")
                      };

            if (pPageIndex <= 1)
            {
                pPageIndex = 1;
            }

            int fromRow = pPageSize * (pPageIndex - 1);
            var lsAttendanceLog = obj.OrderByDescending(t => t.Time).Skip(fromRow).Take(pPageSize).ToList();
            var dataGrid = new DataGridClass(obj.Count(), lsAttendanceLog);
            return await Task.FromResult(dataGrid);
        }

        public object GetLastedACOpenDoor(ConfigObject config, UserInfo user)
        {
            var listLog = _context.AC_StateLog.AsNoTracking()
          .OrderByDescending(t => t.Time).Take(GlobalParams.ROWS_NUMBER_IN_REALTIME_PAGE);

            var lstEmployeeLookup = _IC_EmployeeLogic.GetListEmployeeLookup(config, user);
            var employeeLookup = lstEmployeeLookup.ToDictionarySafe(e => e.EmployeeATID);

            var listDataLog = (from log in listLog
                               join doorDevice in _context.AC_DoorAndDevice.Where(x => x.CompanyIndex == user.CompanyIndex)
                                on log.SerialNumber equals doorDevice.SerialNumber into temp
                               from dummy in temp.DefaultIfEmpty()
                               join dev in _context.AC_Door.Where(x => x.CompanyIndex == user.CompanyIndex)
                               on dummy.DoorIndex equals dev.Index into devlog
                               from dl in devlog.DefaultIfEmpty()

                               join devi in _context.IC_Device.Where(x => x.CompanyIndex == user.CompanyIndex)
                            on log.SerialNumber equals devi.SerialNumber into deviLog
                               from dll in deviLog.DefaultIfEmpty()
                               select new AC_ACOpenDoor
                               {
                                   Optime = log.Time.ToString("dd/MM/yyyy hh:mm:ss"),
                                   DeviceName = dll != null ? dll.AliasName : "",
                                   DoorName = dl != null ? dl.Name : "",
                                   Status = log.Sensor == "01" ? "Mở và đóng cửa" : "Mở cửa"
                               }).ToList();

            var result = listDataLog.ToList();
            return listDataLog;
        }

        public async Task<List<AC_DoorStatus>> GetDoorStatus()
        {
            var groupIndexes = await _context.AC_StateLog.AsNoTracking().GroupBy(x => x.SerialNumber).Select(x => x.Max(y => y.Index)).ToListAsync();
            var groupLastConnect = await _context.AC_StateLog.AsNoTracking().Where(x => groupIndexes.Contains(x.Index)).ToListAsync();
            var serialNumberDoor = await _context.AC_DoorAndDevice.AsNoTracking().ToListAsync();

            var result = (from lastConnect in groupLastConnect
                          join serial in serialNumberDoor on lastConnect.SerialNumber equals serial.SerialNumber into serialLog
                          from se in serialLog.DefaultIfEmpty()
                          where se != null
                          select new
                          {
                              SerialNumber = lastConnect.SerialNumber,
                              DoorIndex = se.DoorIndex,
                              Sensor = lastConnect.Sensor,
                              Time = lastConnect.Time
                          }).ToList();

            return result.GroupBy(x => x.DoorIndex).Select(x => x.OrderByDescending(g => g.Time).FirstOrDefault()).Select(x => new AC_DoorStatus
            {
                DoorIndex = x.DoorIndex,
                Status = x.Sensor == "02" ? true : false
            }).ToList();

        }

        public async Task RunIntegrateLogManual(int previousDay)
        {
            await _IC_ScheduleAutoHostedLogic.AutoIntegrateLogManual(previousDay);
        }

    }
}
