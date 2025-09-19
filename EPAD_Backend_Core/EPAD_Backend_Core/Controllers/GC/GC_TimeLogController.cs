using EPAD_Backend_Core.Base;
using EPAD_Common.Enums;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GC_TimeLogController : ApiControllerBase
    {
        private IMemoryCache cache;
        IGC_TimeLogService _GC_TimeLogService;
        IGC_GatesService _GC_GatesService;
        IGC_Gates_LinesService _GC_Gates_LinesService;
        IGC_Rules_GeneralService _GC_Rules_GeneralService;
        private readonly IHR_UserService _HR_UserService;
        private readonly IGC_TruckDriverLogService _GC_TruckDriverLogService;
        private readonly IIC_PlanDockService _IC_PlanDockService;
        public GC_TimeLogController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _GC_TruckDriverLogService = TryResolve<IGC_TruckDriverLogService>();
            _IC_PlanDockService = TryResolve<IIC_PlanDockService>();
            _GC_TimeLogService = TryResolve<IGC_TimeLogService>();
            _GC_GatesService = TryResolve<IGC_GatesService>();
            _GC_Gates_LinesService = TryResolve<IGC_Gates_LinesService>();
            _GC_Rules_GeneralService = TryResolve<IGC_Rules_GeneralService>();
        }

        [Authorize]
        [ActionName("GetLogInGateMandatory")]
        [HttpGet]
        public async Task<IActionResult> GetLogInGateMandatory()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var gates = await _GC_GatesService.GetDataByCompanyIndex(user.CompanyIndex);
            var gatesIndex = gates.Select(x => x.Index).ToList();
            var linesData = await _GC_Gates_LinesService.GetDataByListGateIndexAndCompanyIndex(gatesIndex, user.CompanyIndex);
            var lines = linesData.Select(x => x.LineIndex).ToList();
            var listLog = await _GC_TimeLogService.GetLineValidLogs(user.CompanyIndex, lines);

            var listLogIn = listLog.Where(t => t.InOutMode == (short)GCSInOutMode.Input);
            var listLastTimeLogOut = listLog.Where(t => t.InOutMode == (short)GCSInOutMode.Output)
                .GroupBy(x => x.EmployeeATID).Select(x => new
                {
                    EmployeeATID = x.Key,
                    Time = x.Max(x => x.Time)
                }).ToList();

            var listLogInN = (from c in listLogIn
                              join d in listLastTimeLogOut
                              on c.EmployeeATID equals d.EmployeeATID into lstTime
                              from d in lstTime.DefaultIfEmpty()
                              where d != null ? (c.Time > d.Time) : true
                              select c).ToList();

            return ApiOk(listLogInN);
        }

        [Authorize]
        [ActionName("UpdateLogInGateMandatoryByRule")]
        [HttpPost]
        public async Task<IActionResult> UpdateLogInGateMandatoryByRule()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            await _GC_TimeLogService.UpdateLogInGateMandatoryByRule(user.CompanyIndex);
            return ApiOk();
        }

        [Authorize]
        [ActionName("GetWalkerMonitoringHistoryListByLineIndex")]
        [HttpGet]
        public async Task<IActionResult> GetWalkerMonitoringHistoryListByLineIndex(int lineIndex, int size)
        {
            var user = GetUserInfo();
            if (user == null)
            {
                return ApiUnauthorized();
            }

            var listTimeLog = _GC_TimeLogService.Where(t => t.CompanyIndex == user.CompanyIndex
                && t.LogType == EPAD_Common.Enums.LogType.Walker.ToString() && t.LineIndex == lineIndex)
                .OrderByDescending(x => x.Time).Take(size).ToList();

            var listTimelogIndex = listTimeLog.Select(t => t.Index).ToList();
            //  var listTimelog_Image = _GC_TimeLog_ImageService.Where(t => listTimelogIndex.Contains(t.TimeLogIndex)).ToList();

            // var listCustomer = _GC_CustomerService.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            var listData = new List<WalkerHistoryInfo>();
            var employeeATIDs = listTimeLog.Select(x => x.EmployeeATID).ToList();
            var employeeInfoList = await _HR_UserService.GetEmployeeCompactInfoByListEmpATID(employeeATIDs, user.CompanyIndex);
            var listDriver = await _IC_PlanDockService.GetPlanDockByListTripCode(employeeATIDs);
            var listExtraDriver = await _GC_TruckDriverLogService.GetExtraTruckDriverLogByListTripCode(employeeATIDs);
            foreach (var item in listTimeLog)
            {
                var data = new WalkerHistoryInfo();
                data.LogIndex = item.Index;
                data.EmployeeATID = item.EmployeeATID;
                data.CheckTime = item.Time;
                data.Success = item.Status == 0 || item.Status == (int)EMonitoringError.NoError;
                data.InOut = item.InOutMode ?? 0;
                data.Error = item.Error;
                data.LineIndex = item.LineIndex;
                data.Note = item.Note;

                data.CardNumber = item.CardNumber;
                data.ObjectType = item.ObjectAccessType;
                // var image = listTimelog_Image.FirstOrDefault(t => t.TimeLogIndex == item.Index);
                //  data.VerifyImage = GetImageInRow(image);
                data.ApproveStatus = item.ApproveStatus;

                if (item.ObjectAccessType == "Employee")
                {
                    if (employeeInfoList != null)
                    {
                        var empInfo = employeeInfoList.FirstOrDefault(t => t.EmployeeATID == item.EmployeeATID);
                        if (empInfo != null)
                        {
                            data.FullName = empInfo.FullName;
                            data.Department = empInfo.Department;
                            data.Position = empInfo.Position;
                            if (empInfo.Avatar != null && empInfo.Avatar.Length > 0) 
                            {
                                data.RegisterImage = Convert.ToBase64String(empInfo.Avatar);
                            }
                            data.CardNumber = empInfo.CardNumber;
                            if (empInfo.EmployeeType.HasValue)
                            { 
                                data.ObjectType = (empInfo.EmployeeType.Value).ToString();
                            }
                        }
                    }
                    if (listDriver.Any(y => y.TripId == data.EmployeeATID))
                    {
                        data.ObjectType = EmployeeType.Driver.ToString();
                        data.FullName = listDriver.FirstOrDefault(y => y.TripId == data.EmployeeATID).DriverName;
                        if (listExtraDriver.Any(y => y.TripCode == data.EmployeeATID && y.CardNumber == data.CardNumber))
                        {
                            data.ObjectType = "ExtraDriver";
                            data.FullName = listExtraDriver.FirstOrDefault(y 
                                => y.TripCode == data.EmployeeATID && y.CardNumber == data.CardNumber).ExtraDriverName;
                        }
                    }
                }
                else
                {
                    data.StudentCode = "";
                    data.Class = "";
                    data.ClassTeacher = "";
                }
                data.VerifyImage = GetLogVerifyImage(data);
                listData.Add(data);
            }

            return ApiOk(listData);

        }


        [Authorize]
        [ActionName("GetWalkerMonitoringHistoryByLogIndex")]
        [HttpGet]
        public async Task<IActionResult> GetWalkerMonitoringHistoryByLogIndex(int logIndex)
        {
            var user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
                return result;

            var log = _GC_TimeLogService.FirstOrDefault(t => t.CompanyIndex == user.CompanyIndex && t.Index == logIndex);
            var data = new WalkerHistoryInfo();
            if (log != null)
            {
                var listDriver = await _IC_PlanDockService.GetPlanDockByListTripCode(new List<string> { log.EmployeeATID });
                var listExtraDriver = await _GC_TruckDriverLogService.GetExtraTruckDriverLogByListTripCode(new List<string> { log.EmployeeATID });
                //var image = _GC_TimeLog_ImageService.FirstOrDefault(t => t.TimeLogIndex == log.Index);
                data.LogIndex = log.Index;
                data.EmployeeATID = log.EmployeeATID;
                data.CheckTime = log.Time;
                data.Success = log.Status == 0 || log.Status == (int)EMonitoringError.NoError;
                data.InOut = log.InOutMode ?? (short)InOutMode.Input;
                data.Error = log.Error;
                data.LineIndex = log.LineIndex;

                data.CardNumber = log.CardNumber;
                data.ObjectType = log.ObjectAccessType;
                //data.VerifyImage = GetImageInRow(image);
                data.ApproveStatus = log.ApproveStatus;
                if (log.ObjectAccessType == ObjectAccessType.Employee.ToString())
                {
                    var empInfos = await _HR_UserService.GetEmployeeCompactInfoByListEmpATID(new List<string> { log.EmployeeATID }, user.CompanyIndex);
                    var empInfo = empInfos?.FirstOrDefault();
                    if (empInfo != null)
                    {
                        data.FullName = empInfo.FullName;
                        data.Department = empInfo.Department;
                        data.Position = empInfo.Position;
                        if (empInfo.Avatar != null && empInfo.Avatar.Length > 0)
                        {
                            data.RegisterImage = Convert.ToBase64String(empInfo.Avatar);
                        }
                        data.CardNumber = empInfo.CardNumber;
                        if (empInfo.EmployeeType.HasValue)
                        {
                            data.ObjectType = (empInfo.EmployeeType.Value).ToString();
                        }
                    }
                    if (listDriver.Any(y => y.TripId == data.EmployeeATID))
                    {
                        data.ObjectType = EmployeeType.Driver.ToString();
                        data.FullName = listDriver.FirstOrDefault(y => y.TripId == data.EmployeeATID).DriverName;
                        if (listExtraDriver.Any(y => y.TripCode == data.EmployeeATID && y.CardNumber == data.CardNumber))
                        {
                            data.ObjectType = "ExtraDriver";
                            data.FullName = listExtraDriver.FirstOrDefault(y
                                => y.TripCode == data.EmployeeATID && y.CardNumber == data.CardNumber).ExtraDriverName;
                        }
                    }
                }
                else
                {
                    data.StudentCode = "";
                    data.Class = "";
                    data.ClassTeacher = "";
                }
                data.VerifyImage = GetLogVerifyImage(data);
            }
            return ApiOk(data);
        }

        private string GetLogVerifyImage(WalkerHistoryInfo data) 
        {
            var base64String = string.Empty;
            if (!Directory.Exists("Files/RealtimeImage"))
            {
                return String.Empty;
            }
            string[] imageFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "Files/RealtimeImage", "*.jpeg");

            if (imageFiles != null && imageFiles.Length > 0)
            {
                var filteredFiles = imageFiles
                    .FirstOrDefault(file => Path.GetFileName(file).Contains(data.CheckTime.ToString("ddMMyyyyHHmmss")));

                if (filteredFiles != null) 
                {
                    byte[] imageBytes = System.IO.File.ReadAllBytes(filteredFiles);
                    base64String = Convert.ToBase64String(imageBytes);
                }
            }

            return base64String;
        }
    }
}
