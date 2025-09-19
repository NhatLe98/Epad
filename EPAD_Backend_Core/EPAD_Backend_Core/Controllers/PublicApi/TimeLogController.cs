using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using EPAD_Common.Extensions;
using System.Net;
using Microsoft.Extensions.Logging;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using EPAD_Services.Interface;
using EPAD_Data;

namespace EPAD_Backend_Core.Controllers.PublicApi
{
    [Route("api/v1/timelog/[action]")]
    [ApiController]
    public class TimeLogController : PublicBaseController
    {
        private readonly ILogger _logger;
        private IMemoryCache cache;
        private string mCommunicateToken;
        private readonly IIC_AttendanceLogService _IC_AttendanceLogService;
        protected ConfigObject _Config;
        private string _configClientName;
        private string _configGetDevices;

        public TimeLogController(IServiceProvider provider, ILogger<TimeLogController> logger, IMemoryCache pCache, IConfiguration configuration) : base(provider)
        {
            mCommunicateToken = configuration.GetValue<string>("CommunicateToken");
            _logger = logger;
            cache = pCache;
            _IC_AttendanceLogService = TryResolve<IIC_AttendanceLogService>();
            _Config = ConfigObject.GetConfig(cache);
            _configClientName = configuration.GetValue<string>("ClientName").ToUpper();
            _configGetDevices = configuration.GetValue<string>("GetDevices");
        }

        [ActionName("GetAttendanceLogByEmployeeId")]
        [HttpGet]
        public ActionResult<BaseResult<AttendanceLogResult>> GetAttendanceLogByEmployeeId([FromQuery] string employeeId, [FromQuery] string fromTime, [FromQuery] string toTime, [FromQuery] string serialNumber)
        {
            var user = GetCurrentUser();
            if (user == null) return ApiUnauthorize();

            try
            {
                var from = fromTime.TryGetDateTime();
                from = from.AddMinutes(-1);
                var to = toTime.TryGetDateTime();
                var allLog = _epadContext.IC_AttendanceLog
                    .Where(x => x.CompanyIndex == user.CompanyIndex && x.EmployeeATID == employeeId
                    && x.CheckTime > from && x.CheckTime < to && x.SerialNumber == serialNumber);

                var result = allLog.Select(x => new AttendanceLogResult
                {
                    EmployeeATID = employeeId,
                    CheckTime = x.CheckTime,
                    InOutMode = x.InOutMode,
                    VerifyMode = x.VerifyMode ?? 0,
                    MachineSerial = x.SerialNumber,
                    DeviceId = x.DeviceId
                }).OrderByDescending(x => x.CheckTime).FirstOrDefault();



                return Ok(new { statusCode = HttpStatusCode.OK, MessageDetail = "", responseData = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { statusCode = HttpStatusCode.BadRequest, MessageDetail = ex.Message, responseData = "" });
            }
        }

        [ActionName("GetAttendanceLogByEmployeeIdAndListSerial")]
        [HttpGet]
        public ActionResult<BaseResult<AttendanceLogResult>> GetAttendanceLogByEmployeeIdAndListSerial
            ([FromQuery] string employeeId, [FromQuery] string fromTime, [FromQuery] string toTime, [FromQuery] string serialNumber)
        {
            var user = GetCurrentUser();
            if (user == null) return ApiUnauthorize();

            try
            {
                if (!string.IsNullOrEmpty(serialNumber))
                {
                    var employeeIds = new List<string> { employeeId };
                    var listEmployeeATID = employeeIds.Select(e => e.PadLeft(_Config.MaxLenghtEmployeeATID, '0')).ToList();
                    var listTrimEmployeeATID = employeeIds.Select(e => e.TrimStart(new Char[] { '0' })).ToList();
                    listEmployeeATID.AddRange(listTrimEmployeeATID);
                    listEmployeeATID = listEmployeeATID.Distinct().ToList();

                    string[] serialNumberLst = serialNumber.Split(',');
                    var from = fromTime.TryGetDateTime();
                    from = from.AddMinutes(-1);
                    var to = toTime.TryGetDateTime();
                    var allLog = _epadContext.IC_AttendanceLog
                        .Where(x => x.CompanyIndex == user.CompanyIndex && listEmployeeATID.Contains(x.EmployeeATID)
                        && x.CheckTime > from && x.CheckTime < to && serialNumberLst.Contains(x.SerialNumber));

                    var result = allLog.Select(x => new AttendanceLogResult
                    {
                        EmployeeATID = employeeId,
                        CheckTime = x.CheckTime,
                        InOutMode = x.InOutMode,
                        VerifyMode = x.VerifyMode ?? 0,
                        MachineSerial = x.SerialNumber,
                        DeviceId = x.DeviceId
                    }).OrderByDescending(x => x.CheckTime).FirstOrDefault();


                    return Ok(new { statusCode = HttpStatusCode.OK, MessageDetail = "", responseData = result });
                }
                else
                {
                    return Ok(new { statusCode = HttpStatusCode.OK, MessageDetail = "", responseData = new List<AttendanceLogResult>() });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { statusCode = HttpStatusCode.BadRequest, MessageDetail = ex.Message, responseData = "" });
            }
        }

        [ActionName("GetAttendanceLogByEmployeeIdsAndDateTime")]
        [HttpPost]
        public ActionResult<BaseResult<List<AttendanceLogResult>>> GetAttendanceLogByEmployeeIdsAndDateTime([FromBody] string[] employeeIds, [FromQuery] string dateTime)
        {
            var user = GetCurrentUser();
            if (user == null) return ApiUnauthorize();

            try
            {
                var date = dateTime.TryGetDateTime();
                var employeeIdLst = employeeIds.ToList();
                var allLog = _IC_AttendanceLogService.GetAttendanceLogByEmployeeATIDsAndDateTime(employeeIdLst, date, user);

                var result = allLog.Select(x => new AttendanceLogResult
                {
                    EmployeeATID = x.EmployeeATID,
                    CheckTime = x.CheckTime,
                    InOutMode = x.InOutMode,
                    VerifyMode = x.VerifyMode ?? 0,
                    MachineSerial = x.SerialNumber,
                    DeviceId = x.DeviceId
                }).OrderByDescending(x => x.CheckTime).ToList();

                return Ok(new { statusCode = HttpStatusCode.OK, MessageDetail = "", responseData = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(DateTime.Now.ToString() + ": " + ex.ToString());
                return BadRequest(new { statusCode = HttpStatusCode.BadRequest, MessageDetail = ex.Message, responseData = "" });
            }
        }

        [ActionName("GetAttendanceLogByTime")]
        [HttpPost]
        public ActionResult<BaseResult<List<AttendanceLogResult>>> GetAttendanceLogByTime([FromBody] GetAttendanceLogByTimeRequest request)
        {
            var user = GetCurrentUser();
            if (user == null) return ApiUnauthorize();

            try
            {
                var date = request.Time.TryGetDateTime();
                var employeeIdLst = request.pEmployeeATIDs.ToList();
                var allLog = _IC_AttendanceLogService.GetAttendanceLogByEmployeeATIDsAndTime(employeeIdLst, date, user, request.AttendanceLogByTimes, request.AttendanceLogByBVTimes);

                var result = allLog.Select(x => new AttendanceLogResult
                {
                    EmployeeATID = x.EmployeeATID,
                    CheckTime = x.CheckTime,
                    InOutMode = x.InOutMode,
                    VerifyMode = x.VerifyMode ?? 0,
                    MachineSerial = x.SerialNumber,
                    DeviceId = x.DeviceId
                }).OrderByDescending(x => x.CheckTime).ToList();

                return Ok(new { statusCode = HttpStatusCode.OK, MessageDetail = "", responseData = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(DateTime.Now.ToString() + ": " + ex.ToString());
                return BadRequest(new { statusCode = HttpStatusCode.BadRequest, MessageDetail = ex.Message, responseData = "" });
            }
        }

        [ActionName("GetAttendanceLog")]
        [HttpGet]
        public ActionResult<BaseResult<AttendanceLogResult>> GetAttendanceLog([FromQuery] string[] e, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var user = GetCurrentUser();
#if DEBUG
            user = new EPAD_Data.Models.UserInfo("admin@tinhhoasolutions.com") { CompanyIndex = 2 };
#endif
            if (user == null) return ApiUnauthorize();

            try
            {
                var pFromDate = fromDate.HasValue ? fromDate.Value : DateTime.MinValue;
                var pToDate = toDate.HasValue ? toDate.Value : DateTime.MaxValue;
                var empLookup = e.ToHashSet();
                var devices = _epadContext.IC_Device.Where(x => x.CompanyIndex == user.CompanyIndex);
                var deviceLookupBySerial = devices.ToDictionarySafe(x => x.SerialNumber);
                var allLog = _epadContext.IC_AttendanceLog.Where(x => x.CompanyIndex == user.CompanyIndex
                    && x.CheckTime.Date >= pFromDate.Date && x.CheckTime.Date <= pToDate.Date);

                if (e.Length > 0)
                    allLog = allLog.Where(x => empLookup.Contains(x.EmployeeATID));

                var responseData = allLog.Select(x => new AttendanceLogResult
                {
                    EmployeeATID = x.EmployeeATID,
                    CheckTime = x.CheckTime,
                    InOutMode = x.InOutMode,
                    VerifyMode = x.VerifyMode ?? 0,
                    MachineSerial = x.SerialNumber,
                    DeviceId = x.DeviceId,
                    Device = deviceLookupBySerial.ContainsKey(x.SerialNumber) ? deviceLookupBySerial[x.SerialNumber].AliasName : "Old Device",

                }).ToList();

                var employeeIds = responseData.Select(x => x.EmployeeATID).ToList();

                var employees = _epadContext.HR_User.Where(x => employeeIds.Contains(x.EmployeeATID)).ToList();
                foreach (var item in responseData)
                {
                    item.EmployeeCode = employees.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID)?.EmployeeCode ?? "";
                }

                return Ok(new { statusCode = HttpStatusCode.OK, MessageDetail = "", responseData = responseData });
            }
            catch (Exception ex)
            {
                return BadRequest(new { statusCode = HttpStatusCode.BadRequest, MessageDetail = ex.Message, responseData = "" });
            }
        }


        [ActionName("GetAttendanceLogByPrivateToken")]
        [HttpGet]
        public ActionResult<BaseResult<AttendanceLogResult>> GetAttendanceLogByPrivateToken([FromQuery] string[] e, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {

            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return Ok(new CameraANPRResult(false, "Token invalid", "", "", ""));
            }

            var config = ConfigObject.GetConfig(cache);

            try
            {
                var pFromDate = fromDate.HasValue ? fromDate.Value : DateTime.MinValue;
                var pToDate = toDate.HasValue ? toDate.Value : DateTime.MaxValue;
                var empLookup = e.ToHashSet();
                var devices = _epadContext.IC_Device.Where(x => x.CompanyIndex == config.CompanyIndex);
                var deviceLookupBySerial = devices.ToDictionarySafe(x => x.SerialNumber);
                var getDevices = new List<string>();
                if (_configClientName == ClientName.VSTAR.ToString() && !string.IsNullOrEmpty(_configGetDevices))
                {
                    getDevices = _configGetDevices.Split(",").ToList();
                    getDevices = devices.Where(x => getDevices.Contains(x.AliasName)).Select(x => x.SerialNumber).ToList();
                }
                var allLog = _epadContext.IC_AttendanceLog.Where(x => x.CompanyIndex == config.CompanyIndex
                && x.CheckTime.Date >= pFromDate.Date && x.CheckTime.Date <= pToDate.Date && (getDevices.Count() == 0 || getDevices.Contains(x.SerialNumber) || x.SerialNumber == ""));

                if (e.Length > 0)
                    allLog = allLog.Where(x => empLookup.Contains(x.EmployeeATID));

                var responseData = allLog.Select(x => new AttendanceLogResult
                {
                    EmployeeATID = x.EmployeeATID,
                    CheckTime = x.CheckTime,
                    InOutMode = x.InOutMode,
                    VerifyMode = x.VerifyMode ?? 0,
                    MachineSerial = x.SerialNumber,
                    DeviceId = x.DeviceId,
                    Device = deviceLookupBySerial.ContainsKey(x.SerialNumber) ? deviceLookupBySerial[x.SerialNumber].AliasName : "Old Device",
                    CheckTimeFormat = x.CheckTime.ToString("yyyyMMddHHmmss"),
                    RoomCode = (string.IsNullOrEmpty(x.RoomCode) ? (deviceLookupBySerial.ContainsKey(x.SerialNumber) ? deviceLookupBySerial[x.SerialNumber].AliasName : "Old Device") : x.RoomCode)
                }).ToList();
                var employeeIds = responseData.Select((x) => x.EmployeeATID).ToList();
                var employees = _epadContext.HR_User.Where((x) => employeeIds.Contains(x.EmployeeATID)).ToList();
                foreach (var item in responseData)
                {
                    item.EmployeeCode = employees.FirstOrDefault((x) => x.EmployeeATID == item.EmployeeATID)?.EmployeeCode ?? "";
                }

                return Ok(new { statusCode = HttpStatusCode.OK, MessageDetail = "", responseData = responseData });
            }
            catch (Exception ex)
            {
                return BadRequest(new { statusCode = HttpStatusCode.BadRequest, MessageDetail = ex.Message, responseData = "" });
            }
        }

        [ActionName("GetAttendanceLogByEmployeeIdByPrivateToken")]
        [HttpGet]
        public ActionResult<BaseResult<AttendanceLogResult>> GetAttendanceLogByEmployeeIdByPrivateToken([FromQuery] string employeeId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return Ok(new CameraANPRResult(false, "Token invalid", "", "", ""));
            }

            try
            {
                var config = ConfigObject.GetConfig(cache);
                var pFromDate = fromDate.HasValue ? fromDate.Value : DateTime.MinValue;
                var pToDate = toDate.HasValue ? toDate.Value : DateTime.MaxValue;

                var result = (from a in _epadContext.IC_AttendanceLog
                              join b in _epadContext.HR_User on a.EmployeeATID equals b.EmployeeATID into b_Check
                              from b in b_Check.DefaultIfEmpty()
                              where a.CompanyIndex == config.CompanyIndex && b.EmployeeCode == employeeId
                              && a.CheckTime >= pFromDate && a.CheckTime <= pToDate

                              select new AttendanceLogResult
                              {
                                  EmployeeATID = a.EmployeeATID,
                                  CheckTime = a.CheckTime,
                                  InOutMode = a.InOutMode,
                                  VerifyMode = a.VerifyMode ?? 0,
                                  MachineSerial = a.SerialNumber,
                                  DeviceId = a.DeviceId,
                                  CheckTimeFormat = a.CheckTime.ToString("yyyyMMddHHmmss"),
                                  EmployeeCode = b.EmployeeCode
                              }).ToList();

                return Ok(new { statusCode = HttpStatusCode.OK, MessageDetail = "", responseData = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { statusCode = HttpStatusCode.BadRequest, MessageDetail = ex.Message, responseData = "" });
            }
        }

        [ActionName("GetAttendanceLogByRoom")]
        [HttpGet]
        public ActionResult<BaseResult<AttendanceLogResult>> GetAttendanceLogByRoom([FromQuery] string[] e, [FromQuery] string[] r, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var user = GetCurrentUser();
#if DEBUG
            user = new EPAD_Data.Models.UserInfo("admin@tinhhoasolutions.com") { CompanyIndex = 2 };
#endif
            if (user == null) return ApiUnauthorize();

            try
            {
                var pFromDate = fromDate.HasValue ? fromDate.Value : DateTime.MinValue;
                var pToDate = toDate.HasValue ? toDate.Value : DateTime.MaxValue;

                var devices = _epadContext.IC_Device.AsEnumerable().Where(x => x.CompanyIndex == user.CompanyIndex && !string.IsNullOrEmpty(x.AliasName));
                if (r.Length > 0)
                {
                    var r1 = r.Select(x => x.ToLower()).ToHashSet();
                    devices = devices.Where(x => r1.Contains(x.AliasName.ToLower()));
                }
                var deviceLookup = devices.ToDictionarySafe(x => x.AliasName.ToLower());
                var deviceLookupBySerial = devices.ToDictionarySafe(x => x.SerialNumber);

                var empLookup = e.ToHashSet();
                var allLog = _epadContext.IC_AttendanceLogClassRoom.AsEnumerable().Where(x => x.CompanyIndex == user.CompanyIndex
                    && x.CheckTime.Date >= pFromDate.Date && x.CheckTime.Date <= pToDate.Date
                    && deviceLookupBySerial.ContainsKey(x.SerialNumber));

                if (e.Length > 0)
                {
                    allLog = allLog.Where(x => empLookup.Contains(x.EmployeeATID));
                }

                var responseData = allLog.Select(x => new
                {
                    x.EmployeeATID,
                    x.CheckTime,
                    x.InOutMode,
                    x.VerifyMode,
                    MachineSerial = x.SerialNumber,
                    Device = deviceLookupBySerial.ContainsKey(x.SerialNumber) ? deviceLookupBySerial[x.SerialNumber].AliasName : "Old Device",
                    DeviceId = string.IsNullOrEmpty(x.DeviceId) ? string.Empty : x.DeviceId,
                    RoomCode = x.RoomId
                }).ToList();

                return Ok(new { statusCode = HttpStatusCode.OK, MessageDetail = "", responseData = responseData });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                return BadRequest(new { statusCode = HttpStatusCode.BadRequest, MessageDetail = ex.Message, responseData = "" });
            }
        }
    }

    public class AttendanceLogResult
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime CheckTime { get; set; }
        public short InOutMode { get; set; }
        public short VerifyMode { get; set; }
        public string Device { get; set; }
        public string MachineSerial { get; set; }
        public string DeviceId { get; set; }
        public string RoomCode { get; set; }
        public string CheckTimeFormat { get; set; }
    }

    public class BaseResult<T>
    {
        public string Status { get; set; }
        public string MessageCode { get; set; }
        public string MesageDetail { get; set; }
        public T Data { get; set; }
    }

    public class GetAttendanceLogByTimeRequest
    {
        public List<GetAttendanceLogByTime> AttendanceLogByTimes { get; set; }
        public string Time { get; set; }
        public List<string> pEmployeeATIDs { get; set; }
        public List<GetAttendanceLogByTime> AttendanceLogByBVTimes { get; set; }
    }
}
