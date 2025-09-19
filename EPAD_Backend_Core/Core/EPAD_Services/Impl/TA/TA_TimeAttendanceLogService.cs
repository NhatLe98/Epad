using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using EPAD_Services.Plugins;
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
using static EPAD_Common.Utility.AppUtils;

namespace EPAD_Services.Impl
{
    public class TA_TimeAttendanceLogService : BaseServices<TA_TimeAttendanceLog, EPAD_Context>, ITA_TimeAttendanceLogService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string mLinkCoreETA_Api;
        private readonly IHR_UserService _HR_UserService;
        public TA_TimeAttendanceLogService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<TA_ShiftService>();
            _configuration = configuration;

            mLinkCoreETA_Api = _configuration.GetValue<string>("CoreETA_Api");
            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
        }
        public async Task SendTimeLogToECMSAPIAsync(TA_TimeAttendanceProccess logParam)
        {
            var client = new HttpClient();
            try
            {
                var jsonData = JsonConvert.SerializeObject(logParam);
                var request = new HttpRequestMessage(HttpMethod.Post, mLinkCoreETA_Api + "/api/TimeAttendanceProccess/TimeAttendanceProccess");
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendTimeLogToECMSAPIAsync: {mLinkCoreETA_Api} {ex}");
            }
        }


        public async Task<DataGridClass> GetCaculateAttendanceData(SyntheticAttendanceRequest filter, int companyIndex)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;
            var attendanceLogData = _dbContext.TA_TimeAttendanceLog.Where(x => x.Date.Date >= filter.FromDate.Date && x.Date.Date <= filter.ToDate.Date && x.CompanyIndex == companyIndex).ToList();
            if (filter.EmployeeATIDs != null && filter.EmployeeATIDs.Count > 0)
            {
                attendanceLogData = attendanceLogData.Where(x => filter.EmployeeATIDs.Contains(x.EmployeeATID)).ToList();
            }
            var filterBy = new List<string>();
            if (filter != null)
            {
                filterBy = filter.Filter.Split(" ").Select(x => x.ToLower()).ToList();
            }
            
            var employeeATIDs = attendanceLogData.Select(x => x.EmployeeATID).ToList();
            var employeeList = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeATIDs, DateTime.Now, companyIndex);
            var shiftInfo = _dbContext.TA_Shift.Where(x => attendanceLogData.Select(x => x.ShiftIndex).Contains(x.Index)).ToList();
            var timeAttendanceLogList = new List<CaculateAttendanceDataReponse>();
            foreach (var employee in attendanceLogData)
            {
                var timeAttendanceLog = new CaculateAttendanceDataReponse();
                timeAttendanceLog.EmployeeATID = employee.EmployeeATID;
                timeAttendanceLog.DepartmentName = employeeList.FirstOrDefault(x => x.EmployeeATID == employee.EmployeeATID)?.Department;
                timeAttendanceLog.EmployeeCode = employeeList.FirstOrDefault(x => x.EmployeeATID == employee.EmployeeATID)?.EmployeeCode;
                timeAttendanceLog.FullName = employeeList.FirstOrDefault(x => x.EmployeeATID == employee.EmployeeATID)?.FullName;
                timeAttendanceLog.DepartmentIndex = employeeList.FirstOrDefault(x => x.EmployeeATID == employee.EmployeeATID)?.DepartmentIndex;
                timeAttendanceLog.ShiftName = shiftInfo.FirstOrDefault(x => x.Index == employee.ShiftIndex)?.Name;
                timeAttendanceLog.Date = employee.Date.ToddMMyyyy();
                timeAttendanceLog.CheckIn = employee.CheckIn != null ? employee.CheckIn.Value.ToHHmmss() : "";
                timeAttendanceLog.CheckOut = employee.CheckOut != null ? employee.CheckOut.Value.ToHHmmss() : "";
                timeAttendanceLog.TotalWorkingDay = employee.TotalWorkingDay;
                timeAttendanceLog.TotalWorkingTime = employee.TotalWorkingTime;
                timeAttendanceLog.TotalDayOff = employee.TotalDayOff;
                timeAttendanceLog.TotalHoliday = employee.TotalHoliday;
                timeAttendanceLog.TotalBusinessTrip = employee.TotalBusinessTrip;
                timeAttendanceLog.TotalWorkingTimeNormal = employee.TotalWorkingTimeNormal;
                timeAttendanceLog.TotalOverTimeNormal = employee.TotalOverTimeNormal;
                timeAttendanceLog.TotalOverTimeNightNormal = employee.TotalOverTimeNightNormal;
                timeAttendanceLog.TotalOverTimeDayOff = employee.TotalOverTimeDayOff;
                timeAttendanceLog.TotalOverTimeNightDayOff = employee.TotalOverTimeNightDayOff;
                timeAttendanceLog.TotalOverTimeHoliday = employee.TotalOverTimeHoliday;
                timeAttendanceLog.TotalOverTimeNightHoliday = employee.TotalOverTimeNightHoliday;
                timeAttendanceLog.TotalWorkingTimeNight = employee.TotalWorkingTimeNight;
                timeAttendanceLog.CheckInLate = employee.CheckInLate;
                timeAttendanceLog.CheckOutEarly = employee.CheckOutEarly;
                timeAttendanceLogList.Add(timeAttendanceLog);
            }
            timeAttendanceLogList = timeAttendanceLogList.Where(x => filterBy.Contains(x.EmployeeATID) 
            || filter.Filter.Contains(x.EmployeeATID)
             || (!string.IsNullOrWhiteSpace(x.EmployeeCode) && (filter.Filter.ToLower().Contains(x.EmployeeCode.ToLower())
             || filter.Filter.ToLower() == x.EmployeeCode.ToLower() || x.EmployeeCode.ToLower().Contains(filter.Filter.ToLower())))
             || (!string.IsNullOrWhiteSpace(x.FullName) && (filter.Filter.ToLower().Contains(x.FullName.ToLower())
             || filter.Filter.ToLower() == x.FullName.ToLower() || x.FullName.ToLower().Contains(filter.Filter.ToLower())))).ToList();
            if(filter.Departments != null && filter.Departments.Count > 0)
            {
                timeAttendanceLogList = timeAttendanceLogList.Where(x => x.DepartmentIndex != null && filter.Departments.Contains(x.DepartmentIndex.Value)).ToList();
            }
             countPage = timeAttendanceLogList.Count();
            dataGrid = new DataGridClass(countPage, timeAttendanceLogList);
            if (filter.Page <= 1)
            {
                var data = timeAttendanceLogList.Take(filter.PageSize).ToList();
                dataGrid = new DataGridClass(countPage, data);
            }
            else
            {
                int fromRow = filter.PageSize * (filter.Page - 1);
                var lsDevice = timeAttendanceLogList.Skip(fromRow).Take(filter.PageSize).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            return dataGrid;
        }
    }
}
