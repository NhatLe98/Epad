using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GC_TruckDriverLogController : ApiControllerBase
    {
        private IMemoryCache cache;
        IGC_TruckDriverLogService _GC_TruckDriverLogService;
        IIC_PlanDockService _IC_PlanDockService;
        IHR_CustomerCardService _HR_CustomerCardService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private EPAD_Context _context;
        private readonly ILogger _logger;
        public GC_TruckDriverLogController(IServiceProvider pProvider, ILoggerFactory loggerFactory) : base(pProvider)
        {
            _context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _GC_TruckDriverLogService = TryResolve<IGC_TruckDriverLogService>();
            _IC_PlanDockService = TryResolve<IIC_PlanDockService>();
            _HR_CustomerCardService = TryResolve<IHR_CustomerCardService>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _logger = loggerFactory.CreateLogger<GC_TruckDriverLogController>();
        }

        [Authorize]
        [ActionName("GetAllCardNumber")]
        [HttpGet]
        public async Task<IActionResult> GetAllCardNumber()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var listCustomerCard = await _HR_CustomerCardService.GetAllCustomerCard(user);

            return ApiOk(listCustomerCard);
        }

        [Authorize]
        [ActionName("GetCardNumberById")]
        [HttpGet]
        public async Task<IActionResult> GetCardNumberById(string id)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var customerCard = await _HR_CustomerCardService.GetCustomerCardById(id, user);

            return ApiOk(customerCard);
        }

        [Authorize]
        [ActionName("GetCardNumberByNumber")]
        [HttpGet]
        public async Task<IActionResult> GetCardNumberByNumber(string number)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var customerCard = await _HR_CustomerCardService.GetCardNumberByNumber(number, user);

            return ApiOk(customerCard);
        }

        [Authorize]
        [ActionName("IsTruckDriverIn")]
        [HttpGet]
        public async Task<IActionResult> IsTruckDriverIn(string tripCode)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var truckDriverInfo = await _GC_TruckDriverLogService.GetPlanDockByTripCode(tripCode);
            if (truckDriverInfo != null)
            {
                var truckDriverLogs = await _GC_TruckDriverLogService.GetTruckDriverLogByTripCode(tripCode);
                if (truckDriverLogs.Count == 0)
                {
                    return ApiOk();
                }
                var isTruckDriverIn = truckDriverLogs.Count > 0 && truckDriverLogs.Any(x => x.InOutMode == (short)InOutMode.Input);
                return ApiOk(isTruckDriverIn);
            }

            return ApiError("TripCodeNotExist");
        }

        [Authorize]
        [ActionName("IsTruckDriverInNotOut")]
        [HttpGet]
        public async Task<IActionResult> IsTruckDriverInNotOut(string tripCode)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var truckDriverInfo = await _GC_TruckDriverLogService.GetPlanDockByTripCode(tripCode);
            if (truckDriverInfo != null)
            {
                var truckDriverLogs = await _GC_TruckDriverLogService.GetTruckDriverLogByTripCode(tripCode);
                if (truckDriverLogs.Count == 0)
                {
                    return ApiOk();
                }
                var isTruckDriverIn = truckDriverLogs.Count > 0 && truckDriverLogs.Any(x => x.InOutMode == (short)InOutMode.Input
                    && !truckDriverLogs.Any(y => y.TripCode == x.TripCode && y.InOutMode == (short)InOutMode.Output));

                //if (isTruckDriverIn)
                //{
                //    var checkDriveIn = truckDriverLogs.Count > 0 && truckDriverLogs.Any(x => x.InOutMode == (short)InOutMode.Input
                //  && truckDriverLogs.Any(y => y.TripCode == x.TripCode));
                //    if (checkDriveIn)
                //    {
                //        return ApiError("TripIsProvidedCard");
                //    }
                //}
                return ApiOk(isTruckDriverIn);
            }

            return ApiError("TripCodeNotExist");
        }

        [Authorize]
        [ActionName("IsTruckDriverOut")]
        [HttpGet]
        public async Task<IActionResult> IsTruckDriverOut(string tripCode)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var truckDriverInfo = await _GC_TruckDriverLogService.GetPlanDockByTripCode(tripCode);
            if (truckDriverInfo != null)
            {
                var truckDriverLogs = await _GC_TruckDriverLogService.GetTruckDriverLogByTripCode(tripCode);
                var isTruckDriverIn = truckDriverLogs.Any(x => x.InOutMode == (short)InOutMode.Output);
                return ApiOk(isTruckDriverIn);
            }

            return ApiError("TripCodeNotExist");
        }

        [Authorize]
        [ActionName("GetDriverByCCCD")]
        [HttpGet("{cccd}")]
        public async Task<ActionResult<HR_UserResult>> GetDriverByCCCD(string cccd)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                return await _GC_TruckDriverLogService.GetDriverByCCCD(cccd);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetDriverByCCCD: {ex}");
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("GetDriverByCardNumber")]
        [HttpGet("{cardNumber}")]
        public async Task<ActionResult<HR_UserResult>> GetDriverByCardNumber(string cardNumber)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                return await _GC_TruckDriverLogService.GetDriverByCardNumber(cardNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetDriverByCardNumber: {ex}");
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("ReturnCard")]
        [HttpPost]
        public async Task<IActionResult> ReturnCard([FromBody] ReturnDriverCardModel data)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var result = await _GC_TruckDriverLogService.ReturnDriverCard(data, user);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    return ApiError(result);
                }

                return ApiOk();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ReturnCard: {ex}");
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("GetTransitTruckDriverInfoByVehiclePlate")]
        [HttpGet]
        public async Task<IActionResult> GetTransitTruckDriverInfoByVehiclePlate(string vehiclePlate)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var listDockStatus = await _GC_TruckDriverLogService.GetAllStatusDock();
            var truckDriverInfo = await _GC_TruckDriverLogService.GetPlanDockByVehiclePlate(vehiclePlate);
            if (truckDriverInfo != null)
            {
                var data = new TruckDriverInfoModel().PopulateWith(truckDriverInfo);

                //var company = await _GC_TruckDriverLogService.GetCompanyByUser(user);
                //if (company != null)
                //{
                //    data.CompanyName = company.Name;
                //}

                var truckDriverLog = await _GC_TruckDriverLogService.GetTruckDriverLogByTripCode(truckDriverInfo.TripId);
                var truckExtraDriverLog = await _GC_TruckDriverLogService.GetExtraTruckDriverLogByTripCode(truckDriverInfo.TripId);
                if (truckDriverLog.Count > 0)
                {
                    data.IsActive = truckDriverLog.FirstOrDefault() != null ? !truckDriverLog.FirstOrDefault().IsInactive : false;
                    data.CardNumber = truckDriverLog.FirstOrDefault()?.CardNumber ?? string.Empty;
                    data.TimeIn = truckDriverLog.FirstOrDefault(x => x.InOutMode == (short)InOutMode.Input)?.Time;
                    data.TimeInString = truckDriverLog.Any(x => x.InOutMode == (short)InOutMode.Input)
                        ? truckDriverLog.FirstOrDefault(x => x.InOutMode == (short)InOutMode.Input).Time.ToddMMyyyyHHmmss()
                        : string.Empty;
                    data.TimeOut = truckDriverLog.FirstOrDefault(x => x.InOutMode == (short)InOutMode.Output)?.Time;
                    data.TimeOutString = truckDriverLog.Any(x => x.InOutMode == (short)InOutMode.Output)
                        ? truckDriverLog.FirstOrDefault(x => x.InOutMode == (short)InOutMode.Output).Time.ToddMMyyyyHHmmss()
                        : string.Empty;
                    data.ExtraDriver = truckExtraDriverLog.Select(x =>
                    {
                        var extraDriverModel = new ExtraTruckDriverLogModel().PopulateWith(x);
                        extraDriverModel.BirthDayString = extraDriverModel.BirthDay.ToddMMyyyyMinus();
                        return extraDriverModel;
                    }).ToList();
                }

                if (listDockStatus.Any(x => x.Key == data.StatusDock))
                {
                    data.StatusDockName = listDockStatus.FirstOrDefault(x => x.Key == data.StatusDock)?.Name ?? string.Empty;
                    if (data.StatusDockName == "Đăng tài" || data.StatusDockName == "Xe vào cổng") 
                    {
                        data.IsRegisDri = true;
                    }
                }

                return ApiOk(data);
            }

            return ApiError("VehiclePlateNotExist");
        }

        [Authorize]
        [ActionName("GetTruckDriverInfoByTripCode")]
        [HttpGet]
        public async Task<IActionResult> GetTruckDriverInfoByTripCode(string tripCode)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var listDockStatus = await _GC_TruckDriverLogService.GetAllStatusDock();
            var truckDriverInfo = await _GC_TruckDriverLogService.GetPlanDockByTripCode(tripCode);
            if (truckDriverInfo != null)
            {
                var data = new TruckDriverInfoModel().PopulateWith(truckDriverInfo);

                //var company = await _GC_TruckDriverLogService.GetCompanyByUser(user);
                //if (company != null)
                //{
                //    data.CompanyName = company.Name;
                //}

                var truckDriverLog = await _GC_TruckDriverLogService.GetTruckDriverLogByTripCode(tripCode);
                var truckExtraDriverLog = await _GC_TruckDriverLogService.GetExtraTruckDriverLogByTripCode(tripCode);
                if (truckDriverLog.Count > 0)
                {
                    data.IsActive = truckDriverLog.FirstOrDefault() != null ? !truckDriverLog.FirstOrDefault().IsInactive : false;
                    data.CardNumber = truckDriverLog.FirstOrDefault()?.CardNumber ?? string.Empty;
                    data.TimeIn = truckDriverLog.FirstOrDefault(x => x.InOutMode == (short)InOutMode.Input)?.Time;
                    data.TimeInString = truckDriverLog.Any(x => x.InOutMode == (short)InOutMode.Input)
                        ? truckDriverLog.FirstOrDefault(x => x.InOutMode == (short)InOutMode.Input).Time.ToddMMyyyyHHmmss()
                        : string.Empty;
                    data.TimeOut = truckDriverLog.FirstOrDefault(x => x.InOutMode == (short)InOutMode.Output)?.Time;
                    data.TimeOutString = truckDriverLog.Any(x => x.InOutMode == (short)InOutMode.Output)
                        ? truckDriverLog.FirstOrDefault(x => x.InOutMode == (short)InOutMode.Output).Time.ToddMMyyyyHHmmss()
                        : string.Empty;
                    data.ExtraDriver = truckExtraDriverLog.Select(x =>
                    {
                        var extraDriverModel = new ExtraTruckDriverLogModel().PopulateWith(x);
                        extraDriverModel.CardUserIndex = truckDriverInfo.Index;
                        extraDriverModel.BirthDayString = extraDriverModel.BirthDay.ToddMMyyyyMinus();
                        return extraDriverModel;
                    }).ToList();
                }

                if (listDockStatus.Any(x => x.Key == data.StatusDock))
                {
                    data.StatusDockName = listDockStatus.FirstOrDefault(x => x.Key == data.StatusDock)?.Name ?? string.Empty;
                    if (data.StatusDockName == "Đăng tài" || data.StatusDockName == "Xe vào cổng")
                    {
                        data.IsRegisDri = true;
                    }
                }

                return ApiOk(data);
            }

            return ApiError("TripCodeNotExist");
        }

        [Authorize]
        [ActionName("SaveTruckDriverLog")]
        [HttpPost]
        public async Task<IActionResult> SaveTruckDriverLog([FromBody] TruckDriverLogModel param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var format = "yyyy/MM/dd HH:mm:ss";
            var now = DateTime.Now;
            if (param.InOutMode == (short)InOutMode.Output)
            {
                var truckDriverLog = await _GC_TruckDriverLogService.GetTruckDriverLogByTripCode(param.TripCode);

                var isLogInExisted = truckDriverLog.Any(x
                    => x.TripCode == param.TripCode && x.InOutMode == (short)InOutMode.Input);
                if (!isLogInExisted)
                {
                    return ApiError("LogInNotExist");
                }

                var isLogOutExisted = truckDriverLog.Any(x
                    => x.TripCode == param.TripCode && x.InOutMode == (short)InOutMode.Output);
                if (isLogOutExisted)
                {
                    return ApiError("LogOutExisted");
                }

                if (!string.IsNullOrWhiteSpace(param.TimeString))
                {
                    param.TimeString = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                }

                if (!string.IsNullOrWhiteSpace(param.TimeString))
                {
                    var convertTimeOut = DateTime.TryParseExact(param.TimeString, format,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var time);

                    if (convertTimeOut)
                    {
                        var data = new GC_TruckDriverLog
                        {
                            TripCode = param.TripCode,
                            Time = time,
                            //Time = now,
                            CompanyIndex = user.CompanyIndex,
                            UpdatedDate = now,
                            UpdatedUser = user.FullName,
                            InOutMode = param.InOutMode,
                            MachineSerial = param.MachineSerial,
                            CardNumber = param.CardNumber,
                            IsException = param.IsException,
                            ReasonException = param.ReasonException
                        };
                        var isSuccess = await _GC_TruckDriverLogService.AddTruckDriverLog(data);
                        if (!isSuccess)
                        {
                            return ApiOk(!isSuccess);
                        }
                        return ApiOk(now);
                    }
                    else
                    {
                        return ApiError("DateTimeWrongFormat");
                    }
                }
                else
                {
                    var data = new GC_TruckDriverLog
                    {
                        TripCode = param.TripCode,
                        //Time = time,
                        Time = now,
                        CompanyIndex = user.CompanyIndex,
                        UpdatedDate = now,
                        UpdatedUser = user.FullName,
                        InOutMode = param.InOutMode,
                        MachineSerial = param.MachineSerial,
                        CardNumber = param.CardNumber,
                        IsException = param.IsException,
                        ReasonException = param.ReasonException
                    };
                    var isSuccess = await _GC_TruckDriverLogService.AddTruckDriverLog(data);
                    if (!isSuccess)
                    {
                        return ApiOk(!isSuccess);
                    }
                    return ApiOk(now);
                }
            }
            else
            {
                var isLogInExisted = await _DbContext.GC_TruckDriverLog.AnyAsync(x
                    => x.TripCode == param.TripCode);
                if (isLogInExisted)
                {
                    return ApiError("LogInExisted");
                }

                var data = new GC_TruckDriverLog
                {
                    TripCode = param.TripCode,
                    Time = now,
                    CompanyIndex = user.CompanyIndex,
                    UpdatedDate = now,
                    UpdatedUser = user.FullName,
                    InOutMode = param.InOutMode,
                    MachineSerial = param.MachineSerial,
                    CardNumber = param.CardNumber,
                    IsException = param.IsException,
                    ReasonException = param.ReasonException,
                };
                if (param.ExtraDriver != null && param.ExtraDriver.Count > 0)
                {
                    param.ExtraDriver.ForEach(x =>
                    {
                        x.UpdatedDate = now;
                        x.UpdatedUser = user.FullName;
                        var convertBirthday = DateTime.TryParseExact(x.BirthDayString, format,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var birthday);
                        if (convertBirthday)
                        {
                            x.BirthDay = birthday;
                        }
                    });
                }

                var driverExtraInfo = param.ExtraDriver.ToList();
                var blackList = _DbContext.GC_BlackList.Where(x => driverExtraInfo.Select(z => z.ExtraDriverCode).Contains(x.Nric)).ToList();

                string message = string.Empty;
                foreach (var item in driverExtraInfo)
                {
                    var checkExist = blackList.Any(x => x.Nric == item.ExtraDriverCode);
                    if (checkExist)
                    {
                        message += "<p>  - " + item.ExtraDriverName + " - " + item.ExtraDriverCode + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";
                    }
                }
                if (!string.IsNullOrWhiteSpace(message))
                {
                    return ApiOk(message);
                }

                var isSuccess = await _GC_TruckDriverLogService.AddTruckDriverLog(data, param.ExtraDriver);
                return ApiOk(isSuccess);
            }
        }

        [Authorize]
        [ActionName("SaveExtraTruckDriverLog")]
        [HttpPost]
        public async Task<IActionResult> SaveExtraTruckDriverLog([FromBody] List<ExtraTruckDriverLogModel> param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var format = "dd-MM-yyyy";
            if (param != null && param.Count > 0)
            {
                param.ForEach(x =>
                {
                    x.UpdatedDate = DateTime.Now;
                    x.UpdatedUser = user.FullName;
                    if (DateTime.TryParseExact(x.BirthDayString, format, null,
                        System.Globalization.DateTimeStyles.None, out DateTime date))
                    {
                        x.BirthDay = date;
                    }
                });
            }

            var driverExtraInfo = param.Select(x => x.ExtraDriverCode).ToList();
            var blackList = _DbContext.GC_BlackList.Where(x => driverExtraInfo.Contains(x.Nric)).ToList();

            string message = string.Empty;
            foreach (var item in param)
            {
                var checkExist = blackList.Any(x => x.Nric == item.ExtraDriverCode);
                if (checkExist)
                {
                    message += "<p>  - " + item.ExtraDriverName + " - " + item.ExtraDriverCode + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";
                }
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                return ApiOk(message);
            }
            var isSuccess = await _GC_TruckDriverLogService.SaveExtraTruckDriverLog(param);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("DeleteExtraTruckDriverLog")]
        [HttpDelete]
        public async Task<IActionResult> DeleteExtraTruckDriverLog(int index)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var isSuccess = await _GC_TruckDriverLogService.DeleteExtraTruckDriverLog(index);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("GetTruckMonitoringHistories")]
        [HttpPost]
        public async Task<IActionResult> GetTruckMonitoringHistories(FilterParams param)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var fromTime = DateTime.Parse(param.FromTime);
            var toTime = DateTime.Parse(param.ToTime);
            var historyModels = await _GC_TruckDriverLogService.GetHistoryData(user, fromTime, toTime, param.Filter);
            var historyGateData = _GC_TruckDriverLogService.GetPaginationList(historyModels, param.Page, param.PageSize);
            return ApiOk(historyGateData);

        }

        [Authorize]
        [ActionName("ExportTruckMonitoringHistories")]
        [HttpPost]
        public async Task<IActionResult> ExportTruckMonitoringHistories(FilterParams param)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var fromTime = DateTime.Parse(param.FromTime);
            var toTime = DateTime.Parse(param.ToTime);
            var dataForExport = await _GC_TruckDriverLogService.GetHistoryData(user, fromTime, toTime, param.Filter);


            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Log");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Mã chuyến";
                worksheet.Cell(currentRow, 2).Value = "Tên tài xế";
                worksheet.Cell(currentRow, 3).Value = "CCCD/ CMND";
                worksheet.Cell(currentRow, 4).Value = "SĐT";
                worksheet.Cell(currentRow, 5).Value = "Biển số";
                worksheet.Cell(currentRow, 6).Value = "Điểm lấy hàng";
                worksheet.Cell(currentRow, 7).Value = "Thời gian vào";
                worksheet.Cell(currentRow, 8).Value = "Thời gian ra";
                worksheet.Cell(currentRow, 9).Value = "Máy vào";
                worksheet.Cell(currentRow, 10).Value = "Máy ra";
                worksheet.Cell(currentRow, 11).Value = "Lý do ngoại lệ";

                for (int i = 1; i < 12; i++)
                {
                    worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Column(i).Width = 20;
                }
                var no = 1;

                foreach (var item in dataForExport)
                {
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "'" + item.TripCode;
                    worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 2).Value = item.DriverName;
                    worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 3).Value = "'" + item.DriverCode;
                    worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 4).Value = "'" + item.DriverPhone;
                    worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 5).Value = "'" + item.TrailerNumber;
                    worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 6).Value = item.LocationFrom;
                    worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 7).Value = "'" + item.TimeInString;
                    worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 8).Value = "'" + item.TimeOutString;
                    worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 9).Value = "'" + item.MachineNameIn;
                    worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 10).Value = "'" + item.MachineNameOut;
                    worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 11).Value = "'" + item.ReasonException;
                    worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    no++;
                }

                MemoryStream stream = new MemoryStream();
                workbook.SaveAs(stream);
                return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"TruckMonitoringHistory_{DateTime.Now.ToString("ddMMyyyyhhmmss")}.xlsx"
                };
            }
        }

        [Authorize]
        [ActionName("InfoTruckDriverTemplateImport")]
        [HttpGet]
        public IActionResult InfoTruckDriverTemplateImport()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            try
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                //Use local 
                var departmentList = _context.IC_Department.Where(x => x.IsDriverDepartment == true).Select(x => x.Name).OrderByDescending(x => x).ToList();
                var positionList = _context.HR_PositionInfo.Select(x => x.Name).OrderByDescending(x => x).ToList();
#if !DEBUG
                var folderDetails = Path.Combine(sWebRootFolder, @"epad/dist/Template_Driver.xlsx");
                using (var workbook = new XLWorkbook(folderDetails))
                {
                    var worksheet = workbook.Worksheets;
                    IXLWorksheet worksheet1;
                    IXLWorksheet worksheet3;
                    var w1 = worksheet.TryGetWorksheet("DepartmentInfo", out worksheet1);
                    worksheet1.Cells().Clear();

                    string startDepartmentCell = "A1";
                    string endDepartmentCell = string.Empty;

                    for (int i = 0; i < departmentList.Count; i++)
                    {
                        if (i == (departmentList.Count - 1))
                        {
                            endDepartmentCell = "A" + (i + 1);
                        }
                        worksheet1.Cell("A" + (i + 1)).Value = departmentList[i];
                    }


                    var w = worksheet.TryGetWorksheet("Data", out worksheet3);
                    worksheet3.Range("N2:N10003").SetDataValidation().List(worksheet1.Range(startDepartmentCell + ":" + endDepartmentCell), true);
                    workbook.Save();
                }
#endif

                return ApiOk();
            }
            catch (Exception ex)
            {
                _logger.LogError("InfoTruckDriverTemplateImport", ex);
                return ApiOk();
            }

        }
    }
}
