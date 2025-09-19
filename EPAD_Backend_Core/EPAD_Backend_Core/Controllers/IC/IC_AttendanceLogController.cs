using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common;
using EPAD_Common.Extensions;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.IC;
using EPAD_Data.Models.Lovad;
using EPAD_Data.Models.TimeLog;
using EPAD_Logic;
using EPAD_Logic.SendMail;
using EPAD_Services.Business;
using EPAD_Services.Business.Parking;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static EPAD_Backend_Core.Provider.TokenProvider;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/AttendanceLog/[action]")]
    [ApiController]
    public class IC_AttendanceLogController : ApiControllerBase
    {
        private string mLinkGCSMonitoringApi;
        private string mLinkGCSACMonitoringApi;
        private string mLinkECMSApi;
        private string mLinkEMSApi;
        private string mCommunicateToken;
        private string mLinkICMSApi;
        private double? defaultBodyTemperature;
        private readonly IHostingEnvironment _hostingEnvironment;

        private bool isUsingOnGcsEpadRealTime;

        private readonly IIC_AttendanceLogService _IC_AttendanceLogService;
        private readonly IIC_ScheduleAutoHostedLogic _IC_ScheduleAutoHostedLogic;
        private readonly IHR_CustomerCardService _IHR_CustomerCardService;
        private readonly IIC_ConfigService _IC_ConfigService;
        private IEmailProvider _emailProvider;
        private readonly ezHR_Context otherContext;
        private readonly string departmentCodeConfig;
        private readonly string departmentNameConfig;
        private readonly string _configClientName;
        private readonly string _isUsingGroupDeviceName;
        private readonly string _isUsingHID;

        IMemoryCache _Cache;

        CustomerProcess _customerProcess;
        ParkingProcess _parkingProcess;
        public IC_AttendanceLogController(IServiceProvider provider) : base(provider)
        {
            _Logger = _LoggerFactory.CreateLogger<IC_AttendanceLogController>();
            _IC_AttendanceLogService = TryResolve<IIC_AttendanceLogService>();
            _IC_ConfigService = TryResolve<IIC_ConfigService>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _emailProvider = TryResolve<IEmailProvider>();
            otherContext = TryResolve<ezHR_Context>();
            _IHR_CustomerCardService = TryResolve<IHR_CustomerCardService>();

            _IC_ScheduleAutoHostedLogic = TryResolve<IIC_ScheduleAutoHostedLogic>();

            var configBodyTemperature = _IC_ConfigService.FirstOrDefault(e => e.EventType == ConfigAuto.GENERAL_SYSTEM_CONFIG.ToString());
            if (configBodyTemperature == null || !defaultBodyTemperature.HasValue)
            {
                defaultBodyTemperature = 37.5;
            }
            else
            {
                defaultBodyTemperature = configBodyTemperature.BodyTemperature;
            }

            mLinkICMSApi = _Configuration.GetValue<string>("ICMSApi");
            mLinkGCSMonitoringApi = _Configuration.GetValue<string>("GCSMonitoringApi");
            mLinkGCSACMonitoringApi = _Configuration.GetValue<string>("GCSACMonitoringApi");
            mLinkECMSApi = _Configuration.GetValue<string>("ECMSApi");
            mCommunicateToken = _Configuration.GetValue<string>("CommunicateToken");
            mLinkEMSApi = _Configuration.GetValue<string>("EMSApi");

            isUsingOnGcsEpadRealTime = Convert.ToBoolean(_Configuration.GetValue<string>("IsUsingOnEpad"));

            departmentCodeConfig = _Configuration.GetValue<string>("DEPARTMENT_CODE");
            departmentNameConfig = _Configuration.GetValue<string>("DEPARTMENT_NAME");
            _isUsingGroupDeviceName = _Configuration.GetValue<string>("IsUsingGroupDeviceName");
            _configClientName = _Configuration.GetValue<string>("ClientName").ToUpper();
            _isUsingHID = _Configuration.GetValue<string>("IsUsingHID");

            _Cache = TryResolve<IMemoryCache>();

            _customerProcess = TryResolve<CustomerProcess>();
            _parkingProcess = TryResolve<ParkingProcess>();
        }

        [Authorize]
        [ActionName("GetAtPageAttendanceLog")]
        [HttpPost]
        public async Task<IActionResult> GetAtPageAttendanceLogAsync(GetAttendanceLogInfo req)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            DateTime fromTime = req.fromDate.TryGetDateTime();
            DateTime toTime = req.toDate.TryGetDateTime();
            var dummy = await _IC_AttendanceLogService.GetDataGrid(req.filter, fromTime, toTime, req.employee, user.CompanyIndex, req.page, req.limit, user);
            return ApiOk(dummy);
        }

        [Authorize]
        [ActionName("GetLogLast7Days")]
        [HttpGet]
        public async Task<IActionResult> GetLogLast7Days()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_AttendanceLogService.GetLogLast7Days();
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetLogsByDoor")]
        [HttpGet]
        public async Task<IActionResult> GetLogsByDoor()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_AttendanceLogService.GetLogsByDoor();
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetRemainInLogs")]
        [HttpGet]
        public async Task<IActionResult> GetRemainInLogs()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_AttendanceLogService.GetRemainInLogs();
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetFullWorkingEmployeeByDepartment")]
        [HttpGet]
        public async Task<IActionResult> GetFullWorkingEmployeeByDepartment()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_AttendanceLogService.GetFullWorkingEmployeeByDepartment(user);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetFullWorkingEmployeeByRootDepartment")]
        [HttpGet]
        public async Task<IActionResult> GetFullWorkingEmployeeByRootDepartment()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_AttendanceLogService.GetFullWorkingEmployeeByRootDepartment(user);
            return ApiOk(result);
        }

        // ===== GET LOG FROM GC_TIMELOG =====
        [Authorize]
        [ActionName("GetTupleFullWorkingEmployeeByDepartment")]
        [HttpGet]
        public async Task<IActionResult> GetTupleFullWorkingEmployeeByDepartment()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_AttendanceLogService.GetTupleFullWorkingEmployeeByDepartment(user);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetTupleFullVehicleEmployeeByDepartment")]
        [HttpGet]
        public async Task<IActionResult> GetTupleFullVehicleEmployeeByDepartment()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_AttendanceLogService.GetTupleFullVehicleEmployeeByDepartment(user);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("UpdateLatestEmergencyAttendance")]
        [HttpPost]
        public async Task<IActionResult> UpdateLatestEmergencyAttendance()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            _IC_AttendanceLogService.UpdateLatestEmergencyAttendance();
            return ApiOk();
        }

        [Authorize]
        [ActionName("GetIntegratedVehicleLog")]
        [HttpGet]
        public async Task<IActionResult> GetIntegratedVehicleLog()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_AttendanceLogService.GetIntegratedVehicleLog(user);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetTruckDriverLog")]
        [HttpGet]
        public async Task<IActionResult> GetTruckDriverLog()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_AttendanceLogService.GetTruckDriverLog(user);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetEmergencyAndEvacuation")]
        [HttpGet]
        public async Task<IActionResult> GetEmergencyAndEvacuation()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_AttendanceLogService.GetEmergencyAndEvacuation(user);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetEmergencyLog")]
        [HttpGet]
        public async Task<IActionResult> GetEmergencyLog()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_AttendanceLogService.GetEmergencyLog(user);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetTupleFullWorkingEmployeeByRootDepartment")]
        [HttpGet]
        public async Task<IActionResult> GetTupleFullWorkingEmployeeByRootDepartment()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_AttendanceLogService.GetTupleFullWorkingEmployeeByRootDepartment(user);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetTupleFullWorkingEmployeeByUserType")]
        [HttpGet]
        public async Task<IActionResult> GetTupleFullWorkingEmployeeByUserType()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_AttendanceLogService.GetTupleFullWorkingEmployeeByUserType(user);
            return ApiOk(result);
        }
        // ===== END =====

        [Authorize]
        [ActionName("GetSystemDateTime")]
        [HttpGet]
        public IActionResult GetSystemDateTime()
        {
            return ApiOk(DateTime.Now);
        }

        [Authorize]
        [ActionName("AddAttendanceLog")]
        [HttpPost]
        public async Task<IActionResult> AddAttendanceLog([FromBody] PostAttendanceLog req)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var timeLogRequest = new TimeLogRequest();
            var cfg = await _IC_ConfigService.GetConfigByEventType(ConfigAuto.INTEGRATE_LOG_REALTIME.ToString(), user.CompanyIndex);
            var obtEmployeeList = new List<OverBodyTemparatureEmployeesList>();
            //_Logger.LogError($"req {req.ListAttendanceLog.Count}");
            _IC_AttendanceLogService.AddAttendanceLog(req, ref timeLogRequest, ref obtEmployeeList, user);
            await _IC_AttendanceLogService.SendTimeLogToAPIAsync(cfg, timeLogRequest);
            // send email when exists over body temparature employees
            _IC_AttendanceLogService.SendMailWhenHaveEmployeeOverTemp(obtEmployeeList);

            return Ok();
        }

        [Authorize]
        [ActionName("AddAttendanceLogByDevice")]
        [HttpPost]
        public async Task<IActionResult> AddAttendanceLogByDevice([FromBody] PostAttendanceLog req)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            try
            {
                await _IC_AttendanceLogService.AddAttendanceLogByDevice(req, user);
                return Ok();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [ActionName("AddAttendanceLogByDeviceFR05M")]
        [HttpPost]
        public async Task<IActionResult> AddAttendanceLogByDeviceFR05M([FromBody] PostAttendanceLog req)
        {

            try
            {
                await _IC_AttendanceLogService.AddAttendanceLogByDeviceFR05M(req);
                return Ok();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// This api submit from EMS portal in case teacher forgot to scan the finger/card for log tracking
        /// So, we don't have serial number and device, we just have room name
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [ActionName("AddAttendanceLogWithClassRoom")]
        [HttpPost]
        public async Task<IActionResult> AddAttendanceLogWithClassRoom([FromBody] AttendanceLogRequest request)
        {
            var user = UserInfo.GetFromCache(_Cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));

            Dictionary<string, IC_Device> deviceLookup = null;
            if (_Cache.TryGetValue("urn:Dictionary_IC_Device", out deviceLookup) == false)
            {
                var lstDevice = _DbContext.IC_Device.Where(x => x.CompanyIndex == user.CompanyIndex);
                _Cache.Set("urn:Dictionary_IC_Device", lstDevice.ToDictionarySafe(x => x.SerialNumber), TimeSpan.FromHours(6));
                deviceLookup = lstDevice.ToDictionarySafe(x => x.SerialNumber);
            }

            var timeLogRequest = new TimeLogRequest();
            IC_Config cfg = null;
            if (_Cache.TryGetValue("urn:IC_Config", out cfg) == false)
            {
                cfg = await _DbContext.IC_Config.FirstOrDefaultAsync(x => x.EventType == ConfigAuto.INTEGRATE_LOG_REALTIME.ToString());
                _Cache.Set("urn:IC_Config", cfg, TimeSpan.FromHours(2));
            }

            // có thể sử dụng giải pháp lookup ở client để thay thế nếu đoạn này quá chậm
            Dictionary<string, IC_EmployeeLookupDTO> employeeLookup = null;
            if (_Cache.TryGetValue("urn:Dictionary_EmployeeLookup", out employeeLookup) == false)
            {
                var lstEmployeeLookup = IC_EmployeeInfoController.GetListEmployeeLookup(_Config, user.CompanyIndex, _DbContext, otherContext, departmentCodeConfig, departmentNameConfig);
                _Cache.Set("urn:Dictionary_EmployeeLookup", lstEmployeeLookup.ToDictionarySafe(e => e.EmployeeATID), TimeSpan.FromDays(1));
            }

            var listEmployeeATID = request.ListAttendanceLog.Select(e => e.UserId.PadLeft(_Config.MaxLenghtEmployeeATID, '0')).ToHashSet();
            var listEmployeeFullName = new List<HR_User>();
            if (_Config.IntegrateDBOther)
            {
                listEmployeeFullName = await otherContext.HR_Employee.Where(e => listEmployeeATID.Contains(e.EmployeeATID) && user.CompanyIndex == e.CompanyIndex)
                    .Select(e => new HR_User { EmployeeATID = e.EmployeeATID, FullName = e.FirstName + " " + e.MidName + " " + e.LastName }).ToListAsync();
            }
            else
            {
                listEmployeeFullName = await _DbContext.HR_User.Where(e => listEmployeeATID.Contains(e.EmployeeATID) && user.CompanyIndex == e.CompanyIndex)
                    .Select(e => new HR_User { EmployeeATID = e.EmployeeATID, FullName = e.FullName }).ToListAsync();
            }

            try
            {
                var obtEmployeeList = new List<OverBodyTemparatureEmployeesList>();
                foreach (var item in request.ListAttendanceLog)
                {
                    var attendanceLog = new IC_AttendanceLogClassRoom
                    {
                        EmployeeATID = item.UserId.PadLeft(_Config.MaxLenghtEmployeeATID, '0'),
                        RoomId = request.RoomName,
                        CompanyIndex = user.CompanyIndex,
                        CheckTime = item.Time,
                        VerifyMode = Convert.ToInt16(item.VerifiedMode),
                        InOutMode = Convert.ToInt16(item.InOutMode),
                        WorkCode = 1,
                        FaceMask = item.FaceMask,
                        BodyTemperature = item.BodyTemperature,
                        Reserve1 = 0,
                        Function = "",
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.UserName,
                        //SerialNumber = request.SerialNumber,
                        //DeviceId = request.DeviceId
                    };
                    timeLogRequest.TimeLogs.Add(CreateTimeLogByRoomForRequest(attendanceLog));
                    try
                    {
                        _DbContext.IC_AttendanceLogClassRoom.Add(attendanceLog);

                        await _IC_AttendanceLogService.SendTimeLogToAPIAsync(cfg, timeLogRequest);

                        var fullName = listEmployeeFullName.FirstOrDefault(e => e.EmployeeATID == attendanceLog.EmployeeATID);

                        if (!string.IsNullOrEmpty(mLinkEMSApi))
                        {
                            var logRealTime = new AttendanceLogToEMS
                            {
                                WeekNumber = attendanceLog.CheckTime.GetWeekOfYear(),
                                WeekTimeLogDetails = new List<WeekTimeLogDetailRequest>
                                {
                                    new WeekTimeLogDetailRequest
                                    {
                                        EmployeeATID = attendanceLog.EmployeeATID,
                                        RoomId = attendanceLog.RoomId,
                                        CheckTime = attendanceLog.CheckTime,
                                        InOutMode = attendanceLog.InOutMode == 0,
                                        CompanyIndex = attendanceLog.CompanyIndex
                                    }
                                },
                            };
                            await SendTimeLogToEMSAPIAsync(logRealTime);
                        }
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError($"{ex}");
                        _DbContext.IC_AttendanceLogClassRoom.Remove(attendanceLog);
                    }
                }
                _DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Receive log error");
                result = StatusCode((int)HttpStatusCode.InternalServerError, ex.ToString());
            }
            return Ok();
        }

        [Authorize]
        [ActionName("AttendanceLogClassHourAddition")]
        [HttpPost]
        public async Task<IActionResult> AttendanceLogClassHourAddition([FromBody] List<ClassHourAttendanceLog> request)
        {
            var user = UserInfo.GetFromCache(_Cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));

            var attendanceLogs = new List<IC_AttendanceLog>();

            try
            {
                var timeLogRequest = new TimeLogRequest();
                IC_Config cfg = null;
                if (_Cache.TryGetValue("urn:IC_Config", out cfg) == false)
                {
                    cfg = await _DbContext.IC_Config.FirstOrDefaultAsync(x => x.EventType == ConfigAuto.INTEGRATE_LOG_REALTIME.ToString());
                    _Cache.Set("urn:IC_Config", cfg, TimeSpan.FromHours(2));
                }

                //Dictionary<string, IC_EmployeeLookupDTO> employeeLookup = null;
                //if (_Cache.TryGetValue("urn:Dictionary_EmployeeLookup", out employeeLookup) == false)
                //{
                //    var lstEmployeeLookup = IC_EmployeeInfoController.GetListEmployeeLookup(_Config, user, _DbContext, otherContext, departmentCodeConfig, departmentNameConfig);
                //    _Cache.Set("urn:Dictionary_EmployeeLookup", lstEmployeeLookup.ToDictionarySafe(e => e.EmployeeATID), TimeSpan.FromDays(1));
                //}

                var userIDs = request.Select(x => x.UserId).ToList();
                var userList = new List<HR_User>();
                if (_Config.IntegrateDBOther)
                {
                    userList = await otherContext.HR_Employee.Where(e => userIDs.Contains(e.EmployeeATID) && e.CompanyIndex == user.CompanyIndex)
                        .Select(e => new HR_User { EmployeeATID = e.EmployeeATID, FullName = e.FirstName + " " + e.MidName + " " + e.LastName })
                        .ToListAsync();
                }
                else
                {
                    userList = await _DbContext.HR_User.Where(e => userIDs.Contains(e.EmployeeATID) && e.CompanyIndex == user.CompanyIndex)
                        .Select(e => new HR_User { EmployeeATID = e.EmployeeATID, FullName = e.FullName })
                        .ToListAsync();
                }

                foreach (var log in request)
                {
                    foreach (DateTime date in EachDay(log.FromDate, log.ToDate))
                    {
                        var checkInTime = new DateTime(date.Year, date.Month, date.Day, log.TimeIn.Hour, log.TimeIn.Minute, log.TimeIn.Second);
                        var checkOutTime = new DateTime(date.Year, date.Month, date.Day, log.TimeOut.Hour, log.TimeOut.Minute, log.TimeOut.Second);
                        var attendanceLog = new IC_AttendanceLog
                        {
                            EmployeeATID = log.UserId.PadLeft(_Config.MaxLenghtEmployeeATID, '0'),
                            CompanyIndex = user.CompanyIndex,
                            CheckTime = checkInTime,
                            InOutMode = (short)InOutMode.Input,
                            RoomCode = log.RoomId,
                            SerialNumber = "",
                            VerifyMode = 4,//Card
                            WorkCode = 1,
                            Reserve1 = 0,
                            Function = "",
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = user.UserName
                        };
                        attendanceLogs.Add(attendanceLog);
                        timeLogRequest.TimeLogs.Add(CreateTimeLogForRequest(attendanceLog));
                        _DbContext.IC_AttendanceLog.Add(attendanceLog);

                        var attendanceLog1 = new IC_AttendanceLog
                        {
                            EmployeeATID = log.UserId.PadLeft(_Config.MaxLenghtEmployeeATID, '0'),
                            CompanyIndex = user.CompanyIndex,
                            CheckTime = checkOutTime,
                            InOutMode = (short)InOutMode.Output,
                            RoomCode = log.RoomId,
                            SerialNumber = "",
                            VerifyMode = 4,//Card
                            WorkCode = 1,
                            Reserve1 = 0,
                            Function = "",
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = user.UserName
                        };
                        attendanceLogs.Add(attendanceLog);
                        timeLogRequest.TimeLogs.Add(CreateTimeLogForRequest(attendanceLog));

                        _DbContext.IC_AttendanceLog.Add(attendanceLog1);

                        //await _IC_AttendanceLogService.SendTimeLogToAPIAsync(cfg, timeLogRequest);

                        if (!string.IsNullOrEmpty(mLinkEMSApi))
                        {
                            foreach (var item in attendanceLogs)
                            {
                                var logRealTime = new AttendanceLogToEMS
                                {
                                    WeekNumber = item.CheckTime.GetWeekOfYear(),
                                    WeekTimeLogDetails = new List<WeekTimeLogDetailRequest>
                                    {
                                        new WeekTimeLogDetailRequest
                                        {
                                            EmployeeATID = item.EmployeeATID,
                                            CheckTime = item.CheckTime,
                                            InOutMode = item.InOutMode == 0,
                                            CompanyIndex = item.CompanyIndex
                                        }
                                    }
                                };
                                await SendTimeLogToEMSAPIAsync(logRealTime);
                            }
                        }
                    }
                }
                _DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _Logger.LogError($"{ex}");
                _DbContext.IC_AttendanceLog.RemoveRange(attendanceLogs);
                //result = StatusCode((int)HttpStatusCode.InternalServerError, ex.ToString());
                return ApiError(ex.ToString());
            }
            return Ok();
        }

        [AllowAnonymous]
        [ActionName("Test")]
        [HttpPost]
        public async Task<IActionResult> Test()
        {
            var client = _ClientFactory.CreateClient();
            string mCommunicateToken = _Configuration.GetValue<string>("CommunicateToken");
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://103.98.160.202:4008/api/User/Get_HR_User_PersonalInfo");
                var basicAuthenticationValue =
      Convert.ToBase64String(
          Encoding.ASCII.GetBytes($"{"tinhhoa"}:{"Tinhhoa@123"}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {basicAuthenticationValue}");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

            }
            catch (Exception ex)
            {

            }
            return Ok();
        }

        [Authorize]
        [ActionName("AddAttendanceLogRealTime")]
        [HttpPost]
        public async Task<IActionResult> AddAttendanceLogRealTime([FromBody] AttendanceLogPram request)
        {
            var user = UserInfo.GetFromCache(_Cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));

            Dictionary<string, IC_Device> deviceLookup = null;
            if (_Cache.TryGetValue("urn:Dictionary_IC_Device", out deviceLookup) == false)
            {
                var lstDevice = _DbContext.IC_Device.Where(x => x.CompanyIndex == user.CompanyIndex);
                _Cache.Set("urn:Dictionary_IC_Device", lstDevice.ToDictionarySafe(x => x.SerialNumber), TimeSpan.FromHours(6));
                deviceLookup = lstDevice.ToDictionarySafe(x => x.SerialNumber);
            }
            var tasks = new List<Task>();
            var isUsingHid = Convert.ToBoolean(_isUsingHID);

            var requestDeviceModule = string.Empty;
            var parkingType = 0;
            var linkparking = string.Empty;
            var deviceId = "";
            var flow = 0;
            if (deviceLookup != null && deviceLookup.Count > 0)
            {
                requestDeviceModule = deviceLookup.ContainsKey(request.SerialNumber) ? deviceLookup[request.SerialNumber].DeviceModule : string.Empty;
                parkingType = deviceLookup.ContainsKey(request.SerialNumber) ? deviceLookup[request.SerialNumber].ParkingType : 0;
                linkparking = deviceLookup.ContainsKey(request.SerialNumber) ? deviceLookup[request.SerialNumber].Account : string.Empty;
                deviceId = deviceLookup.ContainsKey(request.SerialNumber) ? deviceLookup[request.SerialNumber].DeviceId : "";
                flow = deviceLookup.ContainsKey(request.SerialNumber) ? 
                    (deviceLookup[request.SerialNumber].DeviceStatus.HasValue ? deviceLookup[request.SerialNumber].DeviceStatus.Value : 0) : 0;
            }

            //SendVinParkingLog(request.)

            if (mLinkICMSApi != "" && deviceLookup != null)
            {
                foreach (var item in request.ListAttendanceLog)
                {
                    try
                    {
                        var icmsLog = new ICMSAttendanceLog()
                        {
                            EmployeeATID = item.UserId.PadLeft(_Config.MaxLenghtEmployeeATID, '0'),
                            AttState = Convert.ToInt16(item.InOutMode),
                            IsInValid = 0,
                            IP = deviceLookup.ContainsKey(request.SerialNumber) ? deviceLookup[request.SerialNumber].IPAddress : "",
                            VerifyMode = Convert.ToInt16(item.VerifiedMode),
                            Year = item.Time.Year,
                            Month = item.Time.Month,
                            Day = item.Time.Day,
                            Hour = item.Time.Hour,
                            Minute = item.Time.Minute,
                            Second = item.Time.Second
                        };
                        tasks.Add(SendTimeLogToICMSAPIAsync(icmsLog));
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError($"ICMSAttendanceLog: {ex}");
                    }
                }
            }

            var timeLogRequest = new TimeLogRequest();
            IC_Config cfg = null;
            //Test add config realtiem log
            if (_Cache.TryGetValue("urn:IC_Config", out cfg) == false)
            {
                cfg = await _DbContext.IC_Config.FirstOrDefaultAsync(x => x.EventType == ConfigAuto.INTEGRATE_LOG_REALTIME.ToString());
                _Cache.Set("urn:IC_Config", cfg, TimeSpan.FromHours(2));
            }

            // có thể sử dụng giải pháp lookup ở client để thay thế nếu đoạn này quá chậm
            Dictionary<string, IC_EmployeeLookupDTO> employeeLookup = null;
            if (_Cache.TryGetValue("urn:Dictionary_EmployeeLookup", out employeeLookup) == false)
            {
                var lstEmployeeLookup = IC_EmployeeInfoController.GetListEmployeeLookup(_Config, user.CompanyIndex, _DbContext, otherContext, departmentCodeConfig, departmentNameConfig);
                _Cache.Set("urn:Dictionary_EmployeeLookup", lstEmployeeLookup.ToDictionarySafe(e => e.EmployeeATID), TimeSpan.FromHours(2));
                _Cache.TryGetValue("urn:Dictionary_EmployeeLookup", out employeeLookup);
            }
            // Error when call real time
            //if (!string.IsNullOrEmpty(mLinkECMSApi))
            //  GetAttendanceLogInfo(Pram, user.CompanyIndex, user.UserName, cfg, employeeLookup);

            List<IC_DepartmentParentModel> departmentLookup = new List<IC_DepartmentParentModel>();

            if (_configClientName == ClientName.AEON.ToString())
            {
                if (_Cache.TryGetValue("urn:IC_Department", out departmentLookup) == false)
                {
                    _IC_ScheduleAutoHostedLogic.AddDepartmentOnCache();
                    _Cache.TryGetValue<List<IC_DepartmentParentModel>>("urn:IC_Department", out departmentLookup);

                }
            }

            var listEmployeeATID = request.ListAttendanceLog.Select(e => e.UserId.PadLeft(_Config.MaxLenghtEmployeeATID, '0')).ToList();
            var listTrimEmployeeATID = request.ListAttendanceLog.Select(e => e.UserId.TrimStart(new Char[] { '0' })).ToList();
            listEmployeeATID.AddRange(listTrimEmployeeATID);
            listEmployeeATID = listEmployeeATID.Distinct().ToList();
            var listEmployeeFullName = new List<HR_User>();
            if (_Config.IntegrateDBOther)
            {
                //listEmployeeFullName = await otherContext.HR_Employee.Where(e => listEmployeeATID.Contains(e.EmployeeATID) && user.CompanyIndex == e.CompanyIndex)
                //    .Select(e => new HR_User { EmployeeATID = e.EmployeeATID, FullName = e.FirstName + " " + e.MidName + " " + e.LastName }).ToListAsync();
                foreach (var employeeATID in listEmployeeATID)
                {
                    var employeeFullNameExisted = await otherContext.HR_Employee.FirstOrDefaultAsync(e => e.EmployeeATID.Contains(employeeATID) && user.CompanyIndex == e.CompanyIndex);
                    if (employeeFullNameExisted != null && (employeeFullNameExisted.EmployeeATID.EndsWith(employeeATID) && employeeFullNameExisted.EmployeeATID.Replace(employeeATID, "0").All(x => x == '0')))
                    {
                        listEmployeeFullName.Add(new HR_User
                        {
                            EmployeeATID = employeeFullNameExisted.EmployeeATID,
                            FullName = employeeFullNameExisted.FirstName + " "
                            + employeeFullNameExisted.MidName + " " + employeeFullNameExisted.LastName
                        });
                    }
                }
            }
            else
            {
                foreach (var employeeATID in listEmployeeATID)
                {
                    var employeeFullNameExisted = await _DbContext.HR_User.Where(e => e.EmployeeATID.Contains(employeeATID) && user.CompanyIndex == e.CompanyIndex).ToListAsync();
                    if (employeeFullNameExisted != null && employeeFullNameExisted.Count() > 0)
                    {
                        listEmployeeFullName.AddRange(employeeFullNameExisted.Select(x => new HR_User { EmployeeATID = x.EmployeeATID, FullName = x.FullName, EmployeeType = x.EmployeeType, Avatar = x.Avatar }));
                    }
                }
            }

            try
            {
                var obtEmployeeList = new List<OverBodyTemparatureEmployeesList>();
                var listLogs = new List<AttendanceLogRealTime>();
                var device = await _DbContext.IC_Device.FirstOrDefaultAsync(x => x.SerialNumber.ToLower() == request.SerialNumber.ToLower());
                foreach (var item in request.ListAttendanceLog)
                {
                    var existedLog = await _DbContext.IC_AttendanceLog
                        .FirstOrDefaultAsync(x => (x.EmployeeATID == item.UserId.PadLeft(_Config.MaxLenghtEmployeeATID, '0') || x.EmployeeATID == item.UserId
                            || x.EmployeeATID == item.UserId.TrimStart(new Char[] { '0' })) && x.SerialNumber == request.SerialNumber
                            && x.CompanyIndex == user.CompanyIndex && x.CheckTime == item.Time);

                    if (existedLog != null && (existedLog.EmployeeATID.EndsWith(item.UserId) && existedLog.EmployeeATID.Replace(item.UserId, "0").All(x => x == '0')))
                        return BadRequest("LogExists");
                    var fullName = listEmployeeFullName.FirstOrDefault(e => e.EmployeeATID == item.UserId.PadLeft(_Config.MaxLenghtEmployeeATID, '0'));

                    if (fullName == null)
                    {
                        fullName = listEmployeeFullName.FirstOrDefault(e => e.EmployeeATID.TrimStart(new Char[] { '0' }) == item.UserId.TrimStart(new Char[] { '0' }));
                    }

                    var attendanceLog = new IC_AttendanceLog
                    {
                        EmployeeATID = fullName != null ? fullName.EmployeeATID : item.UserId,
                        SerialNumber = request.SerialNumber,
                        DeviceId = device != null ? device.DeviceId : "",
                        CompanyIndex = user.CompanyIndex,
                        CheckTime = item.Time,
                        VerifyMode = Convert.ToInt16(item.VerifiedMode),
                        InOutMode = deviceLookup.ContainsKey(request.SerialNumber) && deviceLookup[request.SerialNumber].DeviceStatus != null && deviceLookup[request.SerialNumber].DeviceStatus != 0 ? Convert.ToInt16(deviceLookup[request.SerialNumber].DeviceStatus.Value - 1) : Convert.ToInt16(item.InOutMode),
                        WorkCode = 1,
                        FaceMask = item.FaceMask,
                        BodyTemperature = item.BodyTemperature,
                        Reserve1 = 0,
                        Function = "",
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.UserName,
                        IsSend = true
                    };

                    if (!string.IsNullOrEmpty(_isUsingGroupDeviceName) && Convert.ToBoolean(_isUsingGroupDeviceName))
                    {
                        var group = GetDeviceGroupName(user.CompanyIndex, request.SerialNumber);
                        if (group != null)
                        {
                            attendanceLog.Function = group.GroupDeviceName;
                        }
                    }

                    timeLogRequest.TimeLogs.Add(CreateTimeLogForRequest(attendanceLog));
                    try
                    {
                        tasks.Add(_IC_AttendanceLogService.SendTimeLogToAPIAsync(cfg, timeLogRequest));
                        var employeeWorkingInfo = employeeLookup != null ? employeeLookup.Any(x => x.Key.TrimStart(new Char[] { '0' }) == attendanceLog.EmployeeATID.TrimStart(new Char[] { '0' }))
                            ? employeeLookup.FirstOrDefault(x => x.Key.TrimStart(new Char[] { '0' }) == attendanceLog.EmployeeATID.TrimStart(new Char[] { '0' })).Value : null : null;

                        var logRealTime = new AttendanceLogRealTime();

                        logRealTime.EmployeeATID = attendanceLog.EmployeeATID;
                        logRealTime.SerialNumber = attendanceLog.SerialNumber;
                        logRealTime.DeviceNumber = attendanceLog.DeviceNumber;
                        logRealTime.DeviceId = attendanceLog.DeviceId;
                        logRealTime.CheckTime = attendanceLog.CheckTime;
                        logRealTime.VerifyMode = StringHelper.GetVerifyModeString(attendanceLog.VerifyMode.Value);
                        logRealTime.InOutMode = StringHelper.GetInOutModeString(attendanceLog.InOutMode);
                        logRealTime.InOutModeString = StringHelper.GetInOutModeString(attendanceLog.InOutMode);
                        logRealTime.FullName = fullName == null ? attendanceLog.EmployeeATID : fullName.FullName;
                        logRealTime.CompanyIndex = attendanceLog.CompanyIndex;
                        logRealTime.Department = employeeWorkingInfo != null ? employeeWorkingInfo.Department : "";
                        logRealTime.DepartmentIndex = employeeWorkingInfo != null ? employeeWorkingInfo.DepartmentIndex : 0;
                        logRealTime.FaceMask = attendanceLog.FaceMask.GetFaceMaskString();
                        logRealTime.BodyTemperature = attendanceLog.BodyTemperature.GetBodyTemperatureString();
                        logRealTime.IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(attendanceLog.BodyTemperature, defaultBodyTemperature.Value);
                        logRealTime.Avatar = fullName != null && fullName.Avatar != null ? Convert.ToBase64String(fullName.Avatar) : "";
                        logRealTime.EmployeeType = fullName?.EmployeeType;
                        logRealTime.PositionName = employeeWorkingInfo != null ? employeeWorkingInfo.PositionName : "";
                        logRealTime.PositionIndex = employeeWorkingInfo != null ? employeeWorkingInfo.PositionIndex : 0;
                        logRealTime.CardNumber = employeeWorkingInfo != null ? employeeWorkingInfo.CardNumber : "";
                        logRealTime.Nric = fullName != null ? fullName.Nric : "";

                        if (deviceLookup.ContainsKey(request.SerialNumber) && deviceLookup[request.SerialNumber].DeviceModule == "AC")
                        {
                            var deviceDetail = (from doordevice in _DbContext.AC_DoorAndDevice.Where(x => x.CompanyIndex == user.CompanyIndex && x.SerialNumber == attendanceLog.SerialNumber)

                                                join d in _DbContext.AC_Door.Where(x => x.CompanyIndex == user.CompanyIndex)
                                                   on doordevice.DoorIndex equals d.Index into dDoor
                                                from dResult in dDoor.DefaultIfEmpty()

                                                join p in _DbContext.AC_AreaAndDoor.Where(x => x.CompanyIndex == user.CompanyIndex)
                                                     on dResult.Index equals p.DoorIndex into pAreaDoor
                                                from pResult in pAreaDoor.DefaultIfEmpty()

                                                join a in _DbContext.AC_Area.Where(x => x.CompanyIndex == user.CompanyIndex)
                                                   on pResult.AreaIndex equals a.Index into aArea
                                                from aResult in aArea.DefaultIfEmpty()
                                                select new
                                                {
                                                    AreaName = aResult.Name,
                                                    DoorName = dResult.Name,
                                                    AreaIndex = aResult.Index,
                                                    DoorIndex = dResult.Index,
                                                }).FirstOrDefault();

                            if (deviceDetail != null)
                            {
                                logRealTime.AreaName = deviceDetail.AreaName;
                                logRealTime.DoorName = deviceDetail.DoorName;
                                logRealTime.AreaIndex = deviceDetail.AreaIndex;
                                logRealTime.DoorIndex = deviceDetail.DoorIndex;
                            }
                        }

                        if (employeeWorkingInfo != null && _configClientName != null && _configClientName == ClientName.AEON.ToString())
                        {
                            var isDepartmentChildren = departmentLookup?.FirstOrDefault(x => x.DepartmentParentIndex == employeeWorkingInfo.DepartmentIndex
                            || x.DepartmentIndexList.Contains((int)employeeWorkingInfo.DepartmentIndex));

                            //var checkDepartmentChildren = departmentList?.Any(x => x.Index == employeeWorkingInfo.DepartmentIndex);
                            if (isDepartmentChildren != null)
                            {
                                logRealTime.DepartmentNameSymbol = logRealTime.Department + " / " + isDepartmentChildren?.DepartmentParentName;
                            }
                            else if (logRealTime.DepartmentIndex == 0)
                            {
                                logRealTime.DepartmentNameSymbol = " / VS";
                            }
                            else
                            {
                                logRealTime.DepartmentNameSymbol = logRealTime.Department + " / HQ";
                            }
                        }

                        listLogs.Add(logRealTime);

                        if (requestDeviceModule == "PA" && parkingType == (int)ParkingType.Lovad && !string.IsNullOrEmpty(linkparking))
                        {
                            if (_configClientName == ClientName.MONDELEZ.ToString() && fullName == null)
                            {
                                var empl = await _IHR_CustomerCardService.GetEmployeeATIDUsingCard(logRealTime.EmployeeATID, user.CompanyIndex, logRealTime.CheckTime);
                                var realtime = ObjectExtensions.CopyToNewObject(logRealTime);
                                if (empl.UserType != null)
                                {
                                    realtime.EmployeeATID = empl.EmployeeATID;
                                    realtime.EmployeeType = empl.UserType;
                                }

                                tasks.Add(SendLogToLovad(realtime.EmployeeATID, realtime.CheckTime, flow , linkparking, Convert.ToInt32(deviceId)));
                            }
                            else
                            {
                                tasks.Add(SendLogToLovad(logRealTime.EmployeeATID, logRealTime.CheckTime, flow, linkparking, Convert.ToInt32(deviceId)));
                            }
                        }

                        if (isUsingOnGcsEpadRealTime)
                        {
                            if(!(requestDeviceModule == "PA" && parkingType == (int)ParkingType.Lovad))
                            {
                                if (_configClientName == ClientName.MONDELEZ.ToString() && fullName == null)
                                {
                                    var empl = await _IHR_CustomerCardService.GetEmployeeATIDUsingCard(logRealTime.EmployeeATID, user.CompanyIndex, logRealTime.CheckTime);
                                    var realtime = ObjectExtensions.CopyToNewObject(logRealTime);
                                    if (empl.UserType != null)
                                    {
                                        realtime.EmployeeATID = empl.EmployeeATID;
                                        realtime.CardNumber = empl.CardNumber;
                                        realtime.EmployeeType = empl.UserType;
                                    }
                                    tasks.Add(AddGCSAttendanceLogRealTime(realtime, user));
                                }
                                else
                                {
                                    tasks.Add(AddGCSAttendanceLogRealTime(logRealTime, user));
                                }

                            }
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(requestDeviceModule) || requestDeviceModule == "GCS"
                                || requestDeviceModule == "AC" || requestDeviceModule == "PA")
                            {
                                //Send log to GCS
                                if (!string.IsNullOrEmpty(mLinkGCSMonitoringApi))
                                    tasks.Add(SendTimeLogToGCSAPIAsync(logRealTime, mLinkGCSMonitoringApi + "/api/Business/AddAttendanceLogRealTime"));

                                //Send log to GCS AC 
                                if (!string.IsNullOrEmpty(mLinkGCSACMonitoringApi))
                                    tasks.Add(SendTimeLogToGCSAPIAsync(logRealTime, mLinkGCSMonitoringApi + mLinkGCSACMonitoringApi));
                            }
                        }

                        //Send log to ECMS
                        if (!string.IsNullOrEmpty(mLinkECMSApi) && (string.IsNullOrWhiteSpace(requestDeviceModule) || requestDeviceModule == "ICMS"))
                            tasks.Add(SendTimeLogToECMSAPIAsync(logRealTime));

                        //Send log to EMS
                        if (!string.IsNullOrEmpty(mLinkEMSApi))
                        {
                            var emsTimeLog = new AttendanceLogToEMS
                            {
                                WeekNumber = attendanceLog.CheckTime.GetWeekOfYear(),
                                WeekTimeLogDetails = new List<WeekTimeLogDetailRequest>
                                {
                                    new WeekTimeLogDetailRequest {
                                        EmployeeATID = attendanceLog.EmployeeATID,
                                        RoomId = attendanceLog.SerialNumber,
                                        CheckTime = attendanceLog.CheckTime,
                                        InOutMode = attendanceLog.InOutMode == 0 ? true : false,
                                        CompanyIndex = attendanceLog.CompanyIndex
                                    }
                                }
                            };
                            tasks.Add(SendTimeLogToEMSAPIAsync(emsTimeLog));
                        }

                        //Send email when IsOverBodyTemperature = true
                        if (logRealTime.IsOverBodyTemperature == true)
                        {
                            obtEmployeeList.Add(new OverBodyTemparatureEmployeesList()
                            {
                                EmployeeATID = logRealTime.EmployeeATID,
                                TimeLog = logRealTime.CheckTime.ToddMMyyyyHHmmss(),
                                BodyTemparature = logRealTime.BodyTemperature.ToString()
                            });
                        }

                        _DbContext.IC_AttendanceLog.Add(attendanceLog);
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError($"{ex}");
                        _DbContext.IC_AttendanceLog.Remove(attendanceLog);
                    }
                }

                result = Ok();

                // push notification
                if (listLogs.Count > 0 && _Config.RealTimeServerLink != null && !string.IsNullOrEmpty(_Config.RealTimeServerLink))
                {
                    var listLogSerialNumber = listLogs.Select(x => x.SerialNumber).ToHashSet();
                    var listLogsDevice = _DbContext.IC_Device.Where(x => listLogSerialNumber.Contains(x.SerialNumber)).ToList();
                    listLogs.ForEach(x =>
                    {
                        var logDevice = listLogsDevice.FirstOrDefault(x => listLogsDevice.Any(y => y.SerialNumber == x.SerialNumber));
                        if (logDevice != null)
                        {
                            x.DeviceName = logDevice.AliasName;
                        }
                    });
                    tasks.Add(SendPushAttendanceLog(listLogs));
                }
                else if (listLogs.Count == 0)
                {
                    result = BadRequest("LogExists");
                }
                tasks.Add(_DbContext.SaveChangesAsync());
                await Task.WhenAll(tasks);


                // send email when exists over body temparature employees
                _IC_AttendanceLogService.SendMailWhenHaveEmployeeOverTemp(obtEmployeeList);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Receive log error");
                result = StatusCode((int)HttpStatusCode.InternalServerError, ex.ToString());
            }
            return result;
        }

        private void SendVinParkingLog(string employeeATID)
        {
            try
            {
                var numberEmployeeATID = int.Parse(employeeATID);
                var text = "OnNewAttLog?" + numberEmployeeATID.ToString();
                var rootSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse("192.168.1.31");
                System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint(ipAdd, 1919);
                rootSocket.Connect(remoteEP);

                byte[] byData = System.Text.Encoding.ASCII.GetBytes(text);
                rootSocket.Send(byData);

                rootSocket.Close();
            }
            catch (Exception ex)
            {
                _Logger.LogError($"SendVinParkingLog: {ex}");
            }

        }

        private async Task SendLogToLovad(string employeeATID, DateTime date, int inout, string linkApi,int lan)
        {
            try
            {
                var log = new IC_SendLogToLovad()
                {
                    Code = employeeATID,
                    Lane = lan,
                    LogTime = date.ToddMMyyyyHHmmss(),
                    Flow = inout == 1 ? 3 : inout
                };
                var client = new HttpClient();
                client.BaseAddress = new Uri(linkApi);
                var json = JsonConvert.SerializeObject(log);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                var byteContent = new ByteArrayContent(buffer);
                HttpResponseMessage response = await client.PostAsync("", byteContent);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError($"SendLogToLovad: {ex}");
            }

        }

        [Authorize]
        [ActionName("AddAttendanceLogRealTimeOffline")]
        [HttpPost]
        public async Task<IActionResult> AddAttendanceLogRealTimeOffline([FromBody] AttendanceLogPram request)
        {
            var user = UserInfo.GetFromCache(_Cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            Dictionary<string, IC_Device> deviceLookup = null;
            if (_Cache.TryGetValue("urn:Dictionary_IC_Device", out deviceLookup) == false)
            {
                var lstDevice = _DbContext.IC_Device.ToList();
                _Cache.Set("urn:Dictionary_IC_Device", lstDevice.ToDictionarySafe(x => x.SerialNumber), TimeSpan.FromHours(6));
                deviceLookup = lstDevice.ToDictionarySafe(x => x.SerialNumber);
            }
            var tasks = new List<Task>();
            var isUsingHid = Convert.ToBoolean(_isUsingHID);

            var requestDeviceModule = string.Empty;
            var parkingType = 0;
            var linkparking = string.Empty;
            var deviceId = "";
            var flow = 0;
            if (deviceLookup != null && deviceLookup.Count > 0)
            {
                requestDeviceModule = deviceLookup.ContainsKey(request.SerialNumber) ? deviceLookup[request.SerialNumber].DeviceModule : string.Empty;
                parkingType = deviceLookup.ContainsKey(request.SerialNumber) ? deviceLookup[request.SerialNumber].ParkingType : 0;
                linkparking = deviceLookup.ContainsKey(request.SerialNumber) ? deviceLookup[request.SerialNumber].Account : string.Empty;
                deviceId = deviceLookup.ContainsKey(request.SerialNumber) ? deviceLookup[request.SerialNumber].DeviceId : "";
                flow = deviceLookup.ContainsKey(request.SerialNumber) ?
                    (deviceLookup[request.SerialNumber].DeviceStatus.HasValue ? deviceLookup[request.SerialNumber].DeviceStatus.Value : 0) : 0;
            }

            
            var timeLogRequest = new TimeLogRequest();
            IC_Config cfg = null;
            //Test add config realtiem log
            if (_Cache.TryGetValue("urn:IC_Config", out cfg) == false)
            {
                cfg = await _DbContext.IC_Config.FirstOrDefaultAsync(x => x.EventType == ConfigAuto.INTEGRATE_LOG_REALTIME.ToString());
                _Cache.Set("urn:IC_Config", cfg, TimeSpan.FromHours(2));
            }

            // có thể sử dụng giải pháp lookup ở client để thay thế nếu đoạn này quá chậm
            Dictionary<string, IC_EmployeeLookupDTO> employeeLookup = null;
            if (_Cache.TryGetValue("urn:Dictionary_EmployeeLookup", out employeeLookup) == false)
            {
                var lstEmployeeLookup = IC_EmployeeInfoController.GetListEmployeeLookup(_Config, _Config.CompanyIndex, _DbContext, otherContext, departmentCodeConfig, departmentNameConfig);
                _Cache.Set("urn:Dictionary_EmployeeLookup", lstEmployeeLookup.ToDictionarySafe(e => e.EmployeeATID), TimeSpan.FromHours(2));
                _Cache.TryGetValue("urn:Dictionary_EmployeeLookup", out employeeLookup);
            }
            // Error when call real time
            //if (!string.IsNullOrEmpty(mLinkECMSApi))
            //  GetAttendanceLogInfo(Pram, user.CompanyIndex, user.UserName, cfg, employeeLookup);

            List<IC_DepartmentParentModel> departmentLookup = new List<IC_DepartmentParentModel>();

          

            var listEmployeeATID = request.ListAttendanceLog.Select(e => e.UserId.PadLeft(_Config.MaxLenghtEmployeeATID, '0')).ToList();
            var listTrimEmployeeATID = request.ListAttendanceLog.Select(e => e.UserId.TrimStart(new Char[] { '0' })).ToList();
            listEmployeeATID.AddRange(listTrimEmployeeATID);
            listEmployeeATID = listEmployeeATID.Distinct().ToList();
            var listEmployeeFullName = new List<HR_User>();
            if (_Config.IntegrateDBOther)
            {
                //listEmployeeFullName = await otherContext.HR_Employee.Where(e => listEmployeeATID.Contains(e.EmployeeATID) && user.CompanyIndex == e.CompanyIndex)
                //    .Select(e => new HR_User { EmployeeATID = e.EmployeeATID, FullName = e.FirstName + " " + e.MidName + " " + e.LastName }).ToListAsync();
                foreach (var employeeATID in listEmployeeATID)
                {
                    var employeeFullNameExisted = await otherContext.HR_Employee.FirstOrDefaultAsync(e => e.EmployeeATID.Contains(employeeATID) && _Config.CompanyIndex == e.CompanyIndex);
                    if (employeeFullNameExisted != null && (employeeFullNameExisted.EmployeeATID.EndsWith(employeeATID) && employeeFullNameExisted.EmployeeATID.Replace(employeeATID, "0").All(x => x == '0')))
                    {
                        listEmployeeFullName.Add(new HR_User
                        {
                            EmployeeATID = employeeFullNameExisted.EmployeeATID,
                            FullName = employeeFullNameExisted.FirstName + " "
                            + employeeFullNameExisted.MidName + " " + employeeFullNameExisted.LastName
                        });
                    }
                }
            }
            else
            {
                foreach (var employeeATID in listEmployeeATID)
                {
                    var employeeFullNameExisted = await _DbContext.HR_User.Where(e => e.EmployeeATID.Contains(employeeATID) && _Config.CompanyIndex == e.CompanyIndex).ToListAsync();
                    if (employeeFullNameExisted != null && employeeFullNameExisted.Count() > 0)
                    {
                        listEmployeeFullName.AddRange(employeeFullNameExisted.Select(x => new HR_User { EmployeeATID = x.EmployeeATID, FullName = x.FullName, EmployeeType = x.EmployeeType, Avatar = x.Avatar }));
                    }
                }
            }

            try
            {
                var obtEmployeeList = new List<OverBodyTemparatureEmployeesList>();
                var listLogs = new List<AttendanceLogRealTime>();
                var device = await _DbContext.IC_Device.FirstOrDefaultAsync(x => x.SerialNumber.ToLower() == request.SerialNumber.ToLower());
                foreach (var item in request.ListAttendanceLog)
                {
                    var existedLog = await _DbContext.IC_AttendanceLog
                        .FirstOrDefaultAsync(x => (x.EmployeeATID == item.UserId.PadLeft(_Config.MaxLenghtEmployeeATID, '0') || x.EmployeeATID == item.UserId
                            || x.EmployeeATID == item.UserId.TrimStart(new Char[] { '0' })) && x.SerialNumber == request.SerialNumber
                            && x.CompanyIndex == _Config.CompanyIndex && x.CheckTime == item.Time);

                    if (existedLog != null && (existedLog.EmployeeATID.EndsWith(item.UserId) && existedLog.EmployeeATID.Replace(item.UserId, "0").All(x => x == '0')))
                        return BadRequest("LogExists");
                    var fullName = listEmployeeFullName.FirstOrDefault(e => e.EmployeeATID == item.UserId.PadLeft(_Config.MaxLenghtEmployeeATID, '0'));

                    if (fullName == null)
                    {
                        fullName = listEmployeeFullName.FirstOrDefault(e => e.EmployeeATID.TrimStart(new Char[] { '0' }) == item.UserId.TrimStart(new Char[] { '0' }));
                    }

                    var attendanceLog = new IC_AttendanceLog
                    {
                        EmployeeATID = fullName != null ? fullName.EmployeeATID : item.UserId,
                        SerialNumber = request.SerialNumber,
                        DeviceId = device != null ? device.DeviceId : "",
                        CompanyIndex = _Config.CompanyIndex,
                        CheckTime = item.Time,
                        VerifyMode = Convert.ToInt16(item.VerifiedMode),
                        InOutMode = deviceLookup.ContainsKey(request.SerialNumber) && deviceLookup[request.SerialNumber].DeviceStatus != null && deviceLookup[request.SerialNumber].DeviceStatus != 0 ? Convert.ToInt16(deviceLookup[request.SerialNumber].DeviceStatus.Value - 1) : Convert.ToInt16(item.InOutMode),
                        WorkCode = 1,
                        FaceMask = item.FaceMask,
                        BodyTemperature = item.BodyTemperature,
                        Reserve1 = 0,
                        Function = "",
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = "Service",
                        IsSend = true
                    };

                    if (!string.IsNullOrEmpty(_isUsingGroupDeviceName) && Convert.ToBoolean(_isUsingGroupDeviceName))
                    {
                        var group = GetDeviceGroupName(_Config.CompanyIndex, request.SerialNumber);
                        if (group != null)
                        {
                            attendanceLog.Function = group.GroupDeviceName;
                        }
                    }

                    timeLogRequest.TimeLogs.Add(CreateTimeLogForRequest(attendanceLog));
                    try
                    {
                        tasks.Add(_IC_AttendanceLogService.SendTimeLogToAPIAsync(cfg, timeLogRequest));
                        var employeeWorkingInfo = employeeLookup != null ? employeeLookup.Any(x => x.Key.TrimStart(new Char[] { '0' }) == attendanceLog.EmployeeATID.TrimStart(new Char[] { '0' }))
                            ? employeeLookup.FirstOrDefault(x => x.Key.TrimStart(new Char[] { '0' }) == attendanceLog.EmployeeATID.TrimStart(new Char[] { '0' })).Value : null : null;

                        var logRealTime = new AttendanceLogRealTime();

                        logRealTime.EmployeeATID = attendanceLog.EmployeeATID;
                        logRealTime.SerialNumber = attendanceLog.SerialNumber;
                        logRealTime.DeviceNumber = attendanceLog.DeviceNumber;
                        logRealTime.DeviceId = attendanceLog.DeviceId;
                        logRealTime.CheckTime = attendanceLog.CheckTime;
                        logRealTime.VerifyMode = StringHelper.GetVerifyModeString(attendanceLog.VerifyMode.Value);
                        logRealTime.InOutMode = StringHelper.GetInOutModeString(attendanceLog.InOutMode);
                        logRealTime.InOutModeString = StringHelper.GetInOutModeString(attendanceLog.InOutMode);
                        logRealTime.FullName = fullName == null ? attendanceLog.EmployeeATID : fullName.FullName;
                        logRealTime.CompanyIndex = attendanceLog.CompanyIndex;
                        logRealTime.Department = employeeWorkingInfo != null ? employeeWorkingInfo.Department : "";
                        logRealTime.DepartmentIndex = employeeWorkingInfo != null ? employeeWorkingInfo.DepartmentIndex : 0;
                        logRealTime.FaceMask = attendanceLog.FaceMask.GetFaceMaskString();
                        logRealTime.BodyTemperature = attendanceLog.BodyTemperature.GetBodyTemperatureString();
                        logRealTime.IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(attendanceLog.BodyTemperature, defaultBodyTemperature.Value);
                        logRealTime.Avatar = fullName != null && fullName.Avatar != null ? Convert.ToBase64String(fullName.Avatar) : "";
                        logRealTime.EmployeeType = fullName?.EmployeeType;
                        logRealTime.PositionName = employeeWorkingInfo != null ? employeeWorkingInfo.PositionName : "";
                        logRealTime.PositionIndex = employeeWorkingInfo != null ? employeeWorkingInfo.PositionIndex : 0;
                        logRealTime.CardNumber = employeeWorkingInfo != null ? employeeWorkingInfo.CardNumber : "";
                        logRealTime.Nric = fullName != null ? fullName.Nric : "";

                    
                        listLogs.Add(logRealTime);

                        if (requestDeviceModule == "PA" && parkingType == (int)ParkingType.Lovad && !string.IsNullOrEmpty(linkparking))
                        {
                            if (_configClientName == ClientName.MONDELEZ.ToString() && fullName == null)
                            {
                                var empl = await _IHR_CustomerCardService.GetEmployeeATIDUsingCard(logRealTime.EmployeeATID, _Config.CompanyIndex, logRealTime.CheckTime);
                                var realtime = ObjectExtensions.CopyToNewObject(logRealTime);
                                if (empl.UserType != null)
                                {
                                    realtime.EmployeeATID = empl.EmployeeATID;
                                    realtime.EmployeeType = empl.UserType;
                                }

                                tasks.Add(SendLogToLovad(realtime.EmployeeATID, realtime.CheckTime, flow, linkparking, Convert.ToInt32(deviceId)));
                            }
                            else
                            {
                                tasks.Add(SendLogToLovad(logRealTime.EmployeeATID, logRealTime.CheckTime, flow, linkparking, Convert.ToInt32(deviceId)));
                            }
                        }

                        if (isUsingOnGcsEpadRealTime)
                        {
                            if (!(requestDeviceModule == "PA" && parkingType == (int)ParkingType.Lovad))
                            {
                                if (_configClientName == ClientName.MONDELEZ.ToString() && fullName == null)
                                {
                                    var empl = await _IHR_CustomerCardService.GetEmployeeATIDUsingCard(logRealTime.EmployeeATID, _Config.CompanyIndex, logRealTime.CheckTime);
                                    var realtime = ObjectExtensions.CopyToNewObject(logRealTime);
                                    if (empl.UserType != null)
                                    {
                                        realtime.EmployeeATID = empl.EmployeeATID;
                                        realtime.CardNumber = empl.CardNumber;
                                        realtime.EmployeeType = empl.UserType;
                                    }
                                    tasks.Add(AddGCSAttendanceLogRealTime(realtime, user));
                                }
                                else
                                {
                                    tasks.Add(AddGCSAttendanceLogRealTime(logRealTime, user));
                                }
                            }
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(requestDeviceModule) || requestDeviceModule == "GCS"
                                || requestDeviceModule == "AC" || requestDeviceModule == "PA")
                            {
                                //Send log to GCS
                                if (!string.IsNullOrEmpty(mLinkGCSMonitoringApi))
                                    tasks.Add(SendTimeLogToGCSAPIAsync(logRealTime, mLinkGCSMonitoringApi + "/api/Business/AddAttendanceLogRealTime"));

                                //Send log to GCS AC 
                                if (!string.IsNullOrEmpty(mLinkGCSACMonitoringApi))
                                    tasks.Add(SendTimeLogToGCSAPIAsync(logRealTime, mLinkGCSMonitoringApi + mLinkGCSACMonitoringApi));
                            }
                        }



                        //Send email when IsOverBodyTemperature = true
                        if (logRealTime.IsOverBodyTemperature == true)
                        {
                            obtEmployeeList.Add(new OverBodyTemparatureEmployeesList()
                            {
                                EmployeeATID = logRealTime.EmployeeATID,
                                TimeLog = logRealTime.CheckTime.ToddMMyyyyHHmmss(),
                                BodyTemparature = logRealTime.BodyTemperature.ToString()
                            });
                        }

                        _DbContext.IC_AttendanceLog.Add(attendanceLog);
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError($"{ex}");
                        _DbContext.IC_AttendanceLog.Remove(attendanceLog);
                    }
                }

                result = Ok();

                // push notification
                if (listLogs.Count > 0 && _Config.RealTimeServerLink != null && !string.IsNullOrEmpty(_Config.RealTimeServerLink))
                {
                    var listLogSerialNumber = listLogs.Select(x => x.SerialNumber).ToHashSet();
                    var listLogsDevice = _DbContext.IC_Device.Where(x => listLogSerialNumber.Contains(x.SerialNumber)).ToList();
                    listLogs.ForEach(x =>
                    {
                        var logDevice = listLogsDevice.FirstOrDefault(x => listLogsDevice.Any(y => y.SerialNumber == x.SerialNumber));
                        if (logDevice != null)
                        {
                            x.DeviceName = logDevice.AliasName;
                        }
                    });
                    tasks.Add(SendPushAttendanceLog(listLogs));
                }
                else if (listLogs.Count == 0)
                {
                    result = BadRequest("LogExists");
                }
                tasks.Add(_DbContext.SaveChangesAsync());
                await Task.WhenAll(tasks);


                // send email when exists over body temparature employees
                _IC_AttendanceLogService.SendMailWhenHaveEmployeeOverTemp(obtEmployeeList);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Receive log error");
                result = StatusCode((int)HttpStatusCode.InternalServerError, ex.ToString());
            }
            return result;
        }

        

        private async Task AddGCSAttendanceLogRealTime([FromBody] AttendanceLogRealTime param, UserInfo user)
        {

            // Get list device info for monitoring
            var listMonitoringDevice = new List<MonitoringDevice>();
            _Cache.TryGetValue("MonitoringDeviceList", out listMonitoringDevice);
            int lineIndex = 0;
            short inOutModeByLine = 1;
            bool isParkingLog = false;

            // Can't load device list
            if (listMonitoringDevice == null)
            {
                _Logger.LogError("CannotLoadDeviceByLineFromGCS");
            }

            // Get line info
            if (listMonitoringDevice.Any())
            {
                var monitoringDevice = listMonitoringDevice.FirstOrDefault(t => t.CompanyIndex == _Config.CompanyIndex);
                if (monitoringDevice == null)
                {
                    _Logger.LogError("Cannot get device by companyindex");
                }

                if (monitoringDevice.LineInDeviceSerialList.ContainsKey(param.SerialNumber) == true)
                {
                    lineIndex = monitoringDevice.LineInDeviceSerialList[param.SerialNumber];
                    inOutModeByLine = 1;
                }
                else if (monitoringDevice.LineOutDeviceSerialList.ContainsKey(param.SerialNumber) == true)
                {
                    lineIndex = monitoringDevice.LineOutDeviceSerialList[param.SerialNumber];
                    inOutModeByLine = 2;
                }
                else if (monitoringDevice.LineInputDeviceSerialList.ContainsKey(param.SerialNumber) == true)
                {
                    lineIndex = monitoringDevice.LineInputDeviceSerialList[param.SerialNumber];
                    inOutModeByLine = 3;
                }
                else
                {
                    _Logger.LogError("Not found device by line");
                    return;
                }

                if (monitoringDevice.ListLineParking.Contains(lineIndex))
                    isParkingLog = true;
            }

            var now = DateTime.Now;
            // check log is employee or customer
            int maxATID = _Cache.Get<int>("MaxATID_" + _Config.CompanyIndex);
            //param.EmployeeATID = param.EmployeeATID.PadLeft(maxATID, '0');

            if (isParkingLog)
                await _parkingProcess.MainProcess(param, lineIndex, inOutModeByLine, now, "ParkingMonitoring", user);
            else
                await _customerProcess.MainProcess(param, lineIndex, inOutModeByLine, now, "WalkerMonitoring", user);

        }

        private async Task SendPushAttendanceLog(List<AttendanceLogRealTime> listLogs)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            client.BaseAddress = new Uri(_Config.RealTimeServerLink);
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            var json = JsonConvert.SerializeObject(listLogs, jsonSettings);
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = await client.PostAsync("/api/PushAttendanceLog/Post", content);
                request.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                _Logger.LogError($"PushAttendanceLog: {ex}");
            }
        }

        private async Task SendTimeLogToGCSAPIAsync(AttendanceLogRealTime logParam, string gcsEndpoint)
        {
            var client = _ClientFactory.CreateClient();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, gcsEndpoint);
                request.Headers.Add("api-token", mCommunicateToken);
                logParam.InOutMode = StringHelper.ConvertToString(logParam.InOutMode);
                var jsonData = JsonConvert.SerializeObject(logParam);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _Logger.LogError($"SendTimeLogToGCSAPIAsync: {ex}");
            }
        }

        public IC_DeviceGroupDTO GetDeviceGroupName(int mCompanyIndex, string serialNumber)
        {
            var groupDevice = (from i in _DbContext.IC_GroupDeviceDetails.Where(x => x.CompanyIndex == mCompanyIndex && x.SerialNumber == serialNumber)
                               join a in _DbContext.IC_GroupDevice.Where(x => x.CompanyIndex == mCompanyIndex)
                               on i.GroupDeviceIndex equals a.Index
                               select new IC_DeviceGroupDTO
                               {
                                   GroupDeviceName = a.Name,
                                   SerialNumber = i.SerialNumber
                               }).FirstOrDefault();
            return groupDevice;
        }

        private async Task GetAttendanceLogInfo(AttendanceLogPram paramLog, int companyIndex, string userName, IC_Config cfg, Dictionary<string, IC_EmployeeLookupDTO> employeeLookup)
        {

            var listLogs = new List<AttendanceLogRealTime>();
            foreach (var item in paramLog.ListAttendanceLog)
            {
                var attendanceLog = new IC_AttendanceLog();
                attendanceLog.EmployeeATID = item.UserId.PadLeft(_Config.MaxLenghtEmployeeATID, '0');
                attendanceLog.SerialNumber = paramLog.SerialNumber;
                attendanceLog.CompanyIndex = companyIndex;
                attendanceLog.CheckTime = item.Time;
                attendanceLog.VerifyMode = Convert.ToInt16(item.VerifiedMode);
                attendanceLog.InOutMode = Convert.ToInt16(item.InOutMode);
                attendanceLog.WorkCode = 1;
                attendanceLog.FaceMask = item.FaceMask;
                attendanceLog.BodyTemperature = item.BodyTemperature;
                attendanceLog.Reserve1 = 0;
                attendanceLog.Function = "";
                attendanceLog.UpdatedDate = DateTime.Now;
                attendanceLog.UpdatedUser = userName;

                var fullname = _DbContext.HR_User.Where(t => t.EmployeeATID.Equals(item.UserId.PadLeft(_Config.MaxLenghtEmployeeATID, '0')))
                    .Select(t => t.FullName).FirstOrDefault();

                var timeLogRequest = new TimeLogRequest();
                timeLogRequest.TimeLogs.Add(CreateTimeLogForRequest(attendanceLog));
                try
                {
                    _DbContext.IC_AttendanceLog.Add(attendanceLog);

                    await _IC_AttendanceLogService.SendTimeLogToAPIAsync(cfg, timeLogRequest);

                    var logRealTime = new AttendanceLogRealTime();
                    logRealTime.EmployeeATID = attendanceLog.EmployeeATID;
                    logRealTime.SerialNumber = attendanceLog.SerialNumber;
                    logRealTime.DeviceNumber = attendanceLog.DeviceNumber;
                    logRealTime.CheckTime = attendanceLog.CheckTime;
                    logRealTime.VerifyMode = StringHelper.GetVerifyModeString(attendanceLog.VerifyMode.Value);
                    logRealTime.InOutMode = StringHelper.GetInOutModeString(attendanceLog.InOutMode);
                    logRealTime.FullName = string.IsNullOrEmpty(fullname) ? attendanceLog.EmployeeATID : fullname;
                    logRealTime.CompanyIndex = attendanceLog.CompanyIndex;
                    logRealTime.Department = employeeLookup != null ? employeeLookup.ContainsKey(attendanceLog.EmployeeATID) ? employeeLookup[attendanceLog.EmployeeATID].Department : "" : "";
                    logRealTime.DepartmentIndex = employeeLookup != null ? employeeLookup.ContainsKey(attendanceLog.EmployeeATID) ? employeeLookup[attendanceLog.EmployeeATID].DepartmentIndex : 0 : 0;

                    listLogs.Add(logRealTime);

                    //Send log to ECMS
                    //SendTimeLogToECMSAPIAsync(logRealTime);
                }
                catch (Exception ex)
                {
                    _Logger.LogError($"GetAttendanceLogInfo: {ex}");
                    _DbContext.IC_AttendanceLog.Remove(attendanceLog);
                }
                _DbContext.SaveChanges();
            }
        }

        private async Task<HttpResponseMessage> SendTimeLogToEMSAPIAsync(AttendanceLogToEMS logParam)
        {
            var client = _ClientFactory.CreateClient();
            try
            {
                //var jsonData = JsonConvert.SerializeObject(logParam);
                //var request = new HttpRequestMessage(HttpMethod.Post, mLinkEMSApi + "/api/timelogs/imports");
                //request.Headers.Add("api-token", mCommunicateToken);
                //request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                //var response = await client.SendAsync(request);
                //response.EnsureSuccessStatusCode();

                var json = JsonConvert.SerializeObject(logParam);
                HttpContent inputContent = new StringContent(json, Encoding.UTF8, "application/json");
                return await client.PostAsync(mLinkEMSApi + "/api/timelogs/imports", inputContent);
                // response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _Logger.LogError($"SendTimeLogToEMSAPIAsync: {mLinkEMSApi} {ex}");
                return null;
            }
        }

        private async Task SendTimeLogToECMSAPIAsync(AttendanceLogRealTime logParam)
        {
            var client = new HttpClient();
            try
            {

                logParam.InOutMode = logParam.InOutMode.ToLower();
                var jsonData = JsonConvert.SerializeObject(logParam);
                var request = new HttpRequestMessage(HttpMethod.Post, mLinkECMSApi + "/api/Business/AddAttendanceLogRealTime");
                request.Headers.Add("api-token", mCommunicateToken);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                _Logger.LogError($"SendTimeLogToECMSAPIAsync: {mLinkECMSApi} {ex}");
            }
        }

        private async Task SendTimeLogToICMSAPIAsync(ICMSAttendanceLog logParam)
        {
            var client = _ClientFactory.CreateClient();
            try
            {
                var jsonData = JsonConvert.SerializeObject(logParam);

                var request = new HttpRequestMessage(HttpMethod.Post, mLinkICMSApi + "/api/AttendanceLog/AttendanceLog");
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _Logger.LogError($"SendTimeLogToICMSAPIAsync: {ex}");
            }
        }

        [Authorize]
        [ActionName("ImportAttendanceLog")]
        [HttpPost]
        public IActionResult ImportAttendanceLog([FromForm] IFormFile file)
        {
            UserInfo user = UserInfo.GetFromCache(_Cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            return Ok();
        }

        [Authorize]
        [ActionName("ExportAttendanceLog")]
        [HttpPost]
        public async Task<IActionResult> ExportAttendanceLog([FromBody] List<AddedParam> addedParams)
        {
            UserInfo user = UserInfo.GetFromCache(_Cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                //return new byte[0];
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });

            //DateTime fromTime = req.fromDate.TryGetDateTime();
            //DateTime toTime = req.toDate.TryGetDateTime();
            var dummy = await _IC_AttendanceLogService.GetMany(addedParams);
            var uniqueName = "AttendanceLog-" + Guid.NewGuid().ToString() + ".xlsx";
            string sWebRootFolder = _hostingEnvironment.ContentRootPath;
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Temp/" + uniqueName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Temp/" + uniqueName));

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("AttendanceLogs");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                worksheet.Cell(currentRow, 3).Value = "Họ tên";
                worksheet.Cell(currentRow, 4).Value = "Thời điểm";
                worksheet.Cell(currentRow, 5).Value = "Tên trên máy";
                worksheet.Cell(currentRow, 6).Value = "Số Serial";
                worksheet.Cell(currentRow, 7).Value = "Phòng ban";
                worksheet.Cell(currentRow, 8).Value = "Vào ra";
                worksheet.Cell(currentRow, 9).Value = "Chế độ";
                worksheet.Cell(currentRow, 10).Value = "Khẩu trang";
                worksheet.Cell(currentRow, 11).Value = "Thân nhiệt";

                for (int i = 1; i < 12; i++)
                {
                    worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Column(i).Width = 20;
                }

                foreach (var log in dummy)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = log.EmployeeATID;
                    worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0".PadLeft(log.EmployeeATID.Length, '0');

                    worksheet.Cell(currentRow, 2).Value = log.EmployeeCode;
                    worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    if (!string.IsNullOrWhiteSpace(log.EmployeeCode))
                        worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "0".PadLeft(log.EmployeeCode.Length, '0');

                    worksheet.Cell(currentRow, 3).Value = log.FullName;
                    worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    worksheet.Cell(currentRow, 4).Style.DateFormat.Format = "dd-MM-yyyy HH:mm:ss";
                    if (log.Time.Day <= 12)
                    {
                        worksheet.Cell(currentRow, 4).Value = log.Time.ToString("MM-dd-yyyy HH:mm:ss");
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 4).Value = log.Time.ToString("dd-MM-yyyy HH:mm:ss");
                    }
                    worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 5).Value = log.AliasName;
                    worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 6).Value = log.SerialNumber;
                    worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 7).Value = log.DepartmentName;
                    worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 8).Value = log.InOutMode;
                    worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 9).Value = log.VerifyMode;
                    worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 10).Value = log.FaceMask;
                    worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 11).Value = log.BodyTemperature;
                    worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                workbook.SaveAs(file.FullName);
                return Ok(URL);
            }
        }

        [Authorize]
        [ActionName("DeleteAttendanceLogTemp")]
        [HttpPost]
        public IActionResult DeleteAttendanceLogTemp()
        {
            string sWebRootFolder = _hostingEnvironment.ContentRootPath;
            string[] files = Directory.GetFiles(sWebRootFolder + @"/Files/Temp/", "*.xlsx");
            foreach (var item in files)
            {
                using (Stream stream = new FileStream(item, FileMode.Open))
                {
                    DateTime modifi = System.IO.File.GetLastWriteTime(item);
                    if (modifi.AddMinutes(1) > DateTime.Now)
                    {
                        System.IO.File.Delete(item);
                    }
                }
            }
            return ApiOk();
        }

        [Authorize]
        [ActionName("GetLastedAttendanceLog")]
        [HttpGet]
        public IActionResult GetLastedAttendanceLog()
        {
            UserInfo user = UserInfo.GetFromCache(_Cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            var listLog = _DbContext.IC_AttendanceLog.AsNoTracking().Where(t => t.CompanyIndex == user.CompanyIndex)
                .OrderByDescending(t => t.CheckTime).Take(GlobalParams.ROWS_NUMBER_IN_PAGE);

            var lstEmployeeLookup = IC_EmployeeInfoController.GetListEmployeeLookup(_Config, user.CompanyIndex, _DbContext, otherContext, departmentCodeConfig, departmentNameConfig);
            var employeeLookup = lstEmployeeLookup.ToDictionarySafe(e => e.EmployeeATID);

            try
            {
                var listDataLog = from log in listLog.ToList()
                                  join emp in _DbContext.HR_User
                                  on log.EmployeeATID equals emp.EmployeeATID into temp
                                  from dummy in temp.DefaultIfEmpty()
                                  join dev in _DbContext.IC_Device
                                  on log.SerialNumber equals dev.SerialNumber into devlog
                                  from dl in devlog.DefaultIfEmpty()
                                  select new
                                  {
                                      EmployeeATID = log.EmployeeATID,
                                      FullName = dummy == null ? log.EmployeeATID : dummy.FullName,
                                      SerialNumber = log.SerialNumber,
                                      DeviceName = dl?.AliasName ?? string.Empty,
                                      DeviceNumber = log.DeviceNumber,
                                      CheckTime = log.CheckTime,
                                      VerifyMode = StringHelper.GetVerifyModeString(log.VerifyMode),
                                      InOutMode = StringHelper.GetInOutModeString(log.InOutMode),
                                      FaceMask = StringHelper.GetFaceMaskString(log.FaceMask),
                                      BodyTemperature = StringHelper.GetBodyTemperatureString(log.BodyTemperature),
                                      IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(log.BodyTemperature, defaultBodyTemperature.Value),
                                      Department = employeeLookup.ContainsKey(log.EmployeeATID) ? employeeLookup[log.EmployeeATID]?.Department : "",
                                      DepartmentIndex = employeeLookup.ContainsKey(log.EmployeeATID) ? employeeLookup[log.EmployeeATID]?.DepartmentIndex : 0,
                                  };
                listDataLog = listDataLog.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Contains(x.DepartmentIndex ?? 0)) || x.DepartmentIndex.HasValue == false);

                result = Ok(listDataLog);
            }
            catch (Exception ex)
            {
                result = StatusCode(500, ex.ToString());
            }

            return result;
        }

        [Authorize]
        [ActionName("GetLastedACAttendanceLog")]
        [HttpGet]
        public IActionResult GetLastedACAttendanceLog()
        {
            UserInfo user = UserInfo.GetFromCache(_Cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            var listLog = _DbContext.IC_AttendanceLog.AsNoTracking().Where(t => t.CompanyIndex == user.CompanyIndex)
                .OrderByDescending(t => t.CheckTime).Take(GlobalParams.ROWS_NUMBER_IN_PAGE);

            var lstEmployeeLookup = IC_EmployeeInfoController.GetListEmployeeLookup(_Config, user.CompanyIndex, _DbContext, otherContext, departmentCodeConfig, departmentNameConfig);
            var employeeLookup = lstEmployeeLookup.ToDictionarySafe(e => e.EmployeeATID);

            try
            {
                var listDataLog = from log in listLog.ToList()
                                  join emp in _DbContext.HR_User
                                  on log.EmployeeATID equals emp.EmployeeATID into temp
                                  from dummy in temp.DefaultIfEmpty()
                                  join dev in _DbContext.IC_Device
                                  on log.SerialNumber equals dev.SerialNumber into devlog
                                  from dl in devlog.DefaultIfEmpty()

                                  join dd in _DbContext.AC_DoorAndDevice.Where(x => x.CompanyIndex == user.CompanyIndex)
                                 on log.SerialNumber equals dd.SerialNumber into ddDoor
                                  from ddResult in ddDoor.DefaultIfEmpty()

                                  join d in _DbContext.AC_Door.Where(x => x.CompanyIndex == user.CompanyIndex)
                                 on ddResult?.DoorIndex equals d.Index into dDoor
                                  from dResult in dDoor.DefaultIfEmpty()

                                  join p in _DbContext.AC_AreaAndDoor.Where(x => x.CompanyIndex == user.CompanyIndex)
                                       on dResult?.Index equals p.DoorIndex into pAreaDoor
                                  from pResult in pAreaDoor.DefaultIfEmpty()

                                  join a in _DbContext.AC_Area.Where(x => x.CompanyIndex == user.CompanyIndex)
                                     on pResult?.AreaIndex equals a.Index into aArea
                                  from aResult in aArea.DefaultIfEmpty()
                                  where dResult != null
                                  select new
                                  {
                                      EmployeeATID = log.EmployeeATID,
                                      FullName = dummy == null ? log.EmployeeATID : dummy.FullName,
                                      SerialNumber = log.SerialNumber,
                                      DeviceName = dl?.AliasName ?? string.Empty,
                                      DeviceNumber = log.DeviceNumber,
                                      CheckTime = log.CheckTime,
                                      VerifyMode = StringHelper.GetVerifyModeString(log.VerifyMode),
                                      InOutMode = StringHelper.GetInOutModeString(log.InOutMode),
                                      FaceMask = StringHelper.GetFaceMaskString(log.FaceMask),
                                      BodyTemperature = StringHelper.GetBodyTemperatureString(log.BodyTemperature),
                                      IsOverBodyTemperature = StringHelper.GetIsOverBodyTemperature(log.BodyTemperature, defaultBodyTemperature.Value),
                                      Department = employeeLookup.ContainsKey(log.EmployeeATID) ? employeeLookup[log.EmployeeATID]?.Department : "",
                                      DepartmentIndex = employeeLookup.ContainsKey(log.EmployeeATID) ? employeeLookup[log.EmployeeATID]?.DepartmentIndex : 0,
                                      AreaName = aResult != null ? aResult.Name : string.Empty,
                                      DoorName = dResult != null ? dResult.Name : string.Empty,
                                      AreaIndex = aResult != null ? aResult.Index : 0,
                                      DoorIndex = dResult != null ? dResult.Index : 0
                                  };
                listDataLog = listDataLog.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Contains(x.DepartmentIndex ?? 0)) || x.DepartmentIndex.HasValue == false);

                result = Ok(listDataLog);
            }
            catch (Exception ex)
            {
                result = StatusCode(500, ex.ToString());
            }

            return result;
        }

        [Authorize]
        [ActionName("GetLastedRealtimeAttendanceLog")]
        [HttpGet]
        public IActionResult GetLastedRealtimeAttendanceLog()
        {
            UserInfo user = UserInfo.GetFromCache(_Cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            try
            {
                var listDataLog = _IC_AttendanceLogService.GetLastedRealtimeAttendanceLog(_Config, user);

                result = Ok(listDataLog);
            }
            catch (Exception ex)
            {
                result = StatusCode(500, ex.ToString());
            }

            return result;
        }

        [Authorize]
        [ActionName("RunIntegrateLogManual")]
        [HttpPost]
        public async Task<IActionResult> RunIntegrate(int previousDay)
        {
            UserInfo user = UserInfo.GetFromCache(_Cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            try
            {
                await _IC_ScheduleAutoHostedLogic.AutoIntegrateLogManual(previousDay);
                await _IC_ScheduleAutoHostedLogic.AutoPostLogFromDatabaseManual();
            }
            catch (Exception ex)
            {
                return ApiError("Integrate Error " + ex.Message);
            }

            return ApiOk();

        }

        [Authorize]
        [ActionName("GetLastedACOpenDoor")]
        [HttpGet]
        public IActionResult GetLastedACOpenDoor()
        {
            UserInfo user = UserInfo.GetFromCache(_Cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            try
            {
                var listDataLog = _IC_AttendanceLogService.GetLastedACOpenDoor(_Config, user);

                result = Ok(listDataLog);
            }
            catch (Exception ex)
            {
                result = StatusCode(500, ex.ToString());
            }

            return result;
        }

        [Authorize]
        [ActionName("GetDoorStatus")]
        [HttpGet]
        public async Task<IActionResult> GetDoorStatus()
        {
            UserInfo user = UserInfo.GetFromCache(_Cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            try
            {
                var listDataLog = await _IC_AttendanceLogService.GetDoorStatus();

                result = Ok(listDataLog);
            }
            catch (Exception ex)
            {
                result = StatusCode(500, ex.ToString());
            }

            return result;
        }


        [Authorize]
        [ActionName("GetAttendanceLogByFilter")]
        [HttpPost]
        public async Task<IActionResult> GetAttendanceLogByFilter(GetAttendanceLogInfo req)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            DateTime fromTime = req.fromDate.TryGetDateTime();
            DateTime toTime = req.toDate.TryGetDateTime();
            var attendanceLog = await _IC_AttendanceLogService.GetAttendanceLog(req.filter, fromTime, toTime, req.employee, user.CompanyIndex, req.page, req.limit, req.ListDevice);
            return ApiOk(attendanceLog);
        }


        [Authorize]
        [ActionName("GetACAttendanceLogByFilter")]
        [HttpPost]
        public async Task<IActionResult> GetACAttendanceLogByFilter(GetACAttendanceLogInfo req)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            DateTime fromTime = req.fromDate.TryGetDateTime();
            DateTime toTime = req.toDate.TryGetDateTime();
            var attendanceLog = await _IC_AttendanceLogService.GetACAttendanceLog(req.filter, fromTime, toTime, req.departmentIds, user.CompanyIndex, req.page, req.limit, req.listArea, req.listDoor);
            return ApiOk(attendanceLog);
        }

        [Authorize]
        [ActionName("GetAttendanceLogByDeviceInCanteen")]
        [HttpPost]
        public IActionResult GetAttendanceLogByDeviceInCanteen(GetAttendanceLogInfo req)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            DateTime fromTime = req.fromDate.TryGetDateTime();
            DateTime toTime = req.toDate.TryGetDateTime();
            var attendanceLog = _IC_AttendanceLogService.GetAttendanceLogByDeviceInCanteen(fromTime, toTime, user.CompanyIndex);
            return ApiOk(attendanceLog);
        }
        private TimeLog CreateTimeLogByRoomForRequest(IC_AttendanceLogClassRoom attendanceLog)
        {
            var tl = new TimeLog();
            tl.EmployeeATID = attendanceLog.EmployeeATID;
            tl.Time = attendanceLog.CheckTime;
            tl.MachineSerial = attendanceLog.RoomId;
            tl.InOutMode = attendanceLog.InOutMode;
            tl.SpecifiedMode = attendanceLog.VerifyMode.Value;
            tl.Action = "EPAD";

            return tl;
        }

        private TimeLog CreateTimeLogForRequest(IC_AttendanceLog attendanceLog)
        {
            var tl = new TimeLog();
            tl.EmployeeATID = attendanceLog.EmployeeATID;
            tl.Time = attendanceLog.CheckTime;
            tl.MachineSerial = attendanceLog.SerialNumber;
            tl.DeviceNumber = attendanceLog.DeviceNumber;
            tl.InOutMode = attendanceLog.InOutMode;
            tl.SpecifiedMode = attendanceLog.VerifyMode.Value;
            tl.Action = "EPAD";
            return tl;
        }

        private IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

    }
}
