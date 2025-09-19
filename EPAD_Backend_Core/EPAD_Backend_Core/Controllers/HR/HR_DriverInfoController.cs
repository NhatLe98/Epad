using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data.Models.HR;
using EPAD_Data.Models;
using EPAD_Data;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using EPAD_Data.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using EPAD_Logic;
using System.IO;
using ClosedXML.Excel;
using EPAD_Backend_Core.WebUtilitys;
using Microsoft.Extensions.Caching.Memory;
using EPAD_Common.Extensions;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/HR_DriverInfo/[action]")]
    [ApiController]
    public class HR_DriverInfoController : ApiControllerBase
    {
        private readonly IHR_DriverInfoService _HR_DriverInfoService;
        private ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        private readonly IGC_BlackListService _GC_BlackListService;
        private IMemoryCache cache;

        public HR_DriverInfoController(IServiceProvider pProvider, ILoggerFactory loggerFactory) : base(pProvider)
        {
            _HR_DriverInfoService = TryResolve<IHR_DriverInfoService>();
            _logger = loggerFactory.CreateLogger<HR_DriverInfoController>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _GC_BlackListService = TryResolve<IGC_BlackListService>();
            cache = TryResolve<IMemoryCache>();
        }

        [Authorize]
        [ActionName("GetDriverAtPage")]
        [HttpPost]
        public async Task<ActionResult<DataGridClass>> GetDriverAtPage([FromBody] HR_DriverInfoParam employeeInfoRequest)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var allEmployee = await _HR_DriverInfoService.GetPage(employeeInfoRequest, user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Post_HR_DriverInfo")]
        [HttpPost]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Post_HR_DriverInfo([FromBody] HR_DriverInfoDTO value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            try
            {
                var checkExist = _HR_DriverInfoService.CheckExistDriverByTripId(value.TripId, user.CompanyIndex);
                if (checkExist)
                {
                    return ApiError("TripIdIsExist");
                }

                var dateNow = DateTime.Now;
                var employeeInBlackList = _GC_BlackListService.Any(x => x.Nric == value.DriverCode && x.FromDate.Date <= dateNow.Date
                                                                                                                         && (x.ToDate == null || (x.ToDate != null && dateNow.Date <= x.ToDate.Value.Date)));
                if (employeeInBlackList)
                {
                    return ApiError("EmployeeInBlackList");
                }

                await _HR_DriverInfoService.AddDriverInfo(value, user);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                return BadRequest();
            }

        }

        [Authorize]
        [ActionName("DeleteDriverInfo")]
        [HttpPost]
        public async Task<IActionResult> DeleteDriverInfo([FromBody] string[] listTripId)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var empLookup = listTripId.ToHashSet().ToList();
            try
            {

                await _HR_DriverInfoService.DeleteDriverInfoMulti(empLookup, user.CompanyIndex);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                return BadRequest();
            }

        }

        [Authorize]
        [ActionName("Put_HR_DriverInfo")]
        [HttpPost]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Put_HR_DriverInfo([FromBody] HR_DriverInfoDTO value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            try
            {
                var dateNow = DateTime.Now;
                var employeeInBlackList = _GC_BlackListService.Any(x => x.Nric == value.DriverCode && x.FromDate.Date <= dateNow.Date
                                                                                                                         && (x.ToDate == null || (x.ToDate != null && dateNow.Date <= x.ToDate.Value.Date)));
                if (employeeInBlackList)
                {
                    return ApiError("EmployeeInBlackList");
                }
                await _HR_DriverInfoService.UpdateDriverInfo(value, user);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                return BadRequest();
            }

        }

        [Authorize]
        [ActionName("AddDriveInfoFromExcel")]
        [HttpPost]
        public IActionResult AddDriveInfoFromExcel([FromBody] List<HR_DriveInfoImportParam> data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            try
            {
                // validation data
                var listError = new List<HR_DriveInfoImportParam>();

                listError = _HR_DriverInfoService.ValidationImportDriverInfo(data, user);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/DriverInfoError.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/DriverInfoError.xlsx"));

                if (listError != null && listError.Count() > 0)
                {
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook(file.FullName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Mã chuyến (*)";
                        worksheet.Cell(currentRow, 2).Value = "Mã đơn hàng (*)";
                        worksheet.Cell(currentRow, 3).Value = "Điểm nhận hàng (*)";
                        worksheet.Cell(currentRow, 4).Value = "Họ tên (*)";
                        worksheet.Cell(currentRow, 5).Value = "Biển số xe (*)";
                        worksheet.Cell(currentRow, 6).Value = "Số CCCD (*)";
                        worksheet.Cell(currentRow, 7).Value = "Số điện thoại (*)";
                        worksheet.Cell(currentRow, 8).Value = "Xe vãng lai";
                        worksheet.Cell(currentRow, 9).Value = "Thời gian dự kiến đến điểm lấy (*)";
                        worksheet.Cell(currentRow, 10).Value = "Thời gian vào Dock";
                        worksheet.Cell(currentRow, 11).Value = "Trạng thái xe";
                        worksheet.Cell(currentRow, 12).Value = "Loại";
                        worksheet.Cell(currentRow, 13).Value = "Nhà cung cấp";
                        worksheet.Cell(currentRow, 14).Value = "Lỗi";

                        for (int i = 1; i < 15; i++)
                        {
                            worksheet.Cell(currentRow, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(currentRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        for (int i = (currentRow + 1); i <= worksheet.LastRowUsed().RowNumber(); i++)
                        {
                            worksheet.Row(i).Clear();
                        }

                        foreach (var driverInfo in listError)
                        {
                            currentRow++;
                            //New template
                            worksheet.Cell(currentRow, 1).Value = "'" + driverInfo.TripId;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 2).Value = "'" + driverInfo.OrderCode;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 3).Value = driverInfo.LocationFrom;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = "'" + driverInfo.DriverName;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = "'" + driverInfo.TrailerNumber;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = "'" + driverInfo.DriverCode;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = driverInfo.DriverPhone;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = "'" + (driverInfo.Vc ? "VC" : "");
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 9).Value = "'" + driverInfo.EtaStr;
                            worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 10).Value = "'" + driverInfo.TimesDock;
                            worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 11).Value = "'" + driverInfo.StatusDockString;
                            worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 12).Value = "'" + driverInfo.Type;
                            worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 13).Value = "'" + driverInfo.Supplier;
                            worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 14).Value = driverInfo.ErrorMessage;
                            worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(currentRow, 14).Style.Alignment.SetWrapText(true);
                        }
                        worksheet.Columns().AdjustToContents();
                        worksheet.Rows().AdjustToContents();
                        workbook.SaveAs(file.FullName);
                    }
                }

                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "HR_DriverInfo";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + lstImport.Count().ToString() + " Department";
                audit.Description = AuditType.Added.ToString() + "AddDriverInfoFromExcel:/:" + data.Count().ToString();
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);

                result = Ok(message);
                return result;
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("ExportToExcel")]
        [HttpPost]
        public IActionResult ExportToExcel([FromBody] HR_DriverInfoParam employeeInfoRequest)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = Unauthorized();
            if (user == null)
            {
                //return new byte[0];
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var config = ConfigObject.GetConfig(cache);

            var driverInfos = _HR_DriverInfoService.GetManyExportDriver(employeeInfoRequest, user.CompanyIndex);

            string sWebRootFolder = _hostingEnvironment.ContentRootPath;
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/DriverInfo.xlsx");
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/DriverInfo.xlsx"));

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DriverInfo");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Mã chuyến (*)";
                worksheet.Cell(currentRow, 2).Value = "Mã đơn hàng";
                worksheet.Cell(currentRow, 3).Value = "Họ tên";
                worksheet.Cell(currentRow, 4).Value = "Biển số xe";
                worksheet.Cell(currentRow, 5).Value = "Số CCCD";
                worksheet.Cell(currentRow, 6).Value = "Xe vãng lai";
                worksheet.Cell(currentRow, 7).Value = "Số điện thoại";
                worksheet.Cell(currentRow, 8).Value = "Thời gian xe đăng tài";
                worksheet.Cell(currentRow, 9).Value = "Trạng thái xe";
                worksheet.Cell(currentRow, 10).Value = "Thời gian vào cổng";
                worksheet.Cell(currentRow, 11).Value = "Thời gian ra cổng";
                worksheet.Cell(currentRow, 12).Value = "Loại";
                worksheet.Cell(currentRow, 13).Value = "Nhà cung cấp";
                worksheet.Cell(currentRow, 14).Value = "Trạng thái";

                for (int i = 1; i < 15; i++)
                {
                    worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Column(i).Width = 20;
                }

                foreach (var driverInfo in driverInfos)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = driverInfo.TripId;
                    worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 2).Value = driverInfo.OrderCode;
                    worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 3).Value = driverInfo.DriverName;
                    worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 4).Value = driverInfo.TrailerNumber;
                    worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 5).Value = driverInfo.DriverCode;
                    worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 6).Value = driverInfo.Vc ? "x" : "";
                    worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 7).Value = driverInfo.DriverPhone;
                    worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 8).Value = driverInfo.TimesDock != null && driverInfo.TimesDock != DateTime.MinValue ? "'" + driverInfo.TimesDock.Value.ToString("dd/MM/yyyy HH:mm:ss") : "";
                    worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 9).Value = driverInfo.StatusDockString;
                    worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 10).Value = !string.IsNullOrEmpty(driverInfo.FromDate) ? "'" + driverInfo.FromDate : "";
                    worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 11).Value = !string.IsNullOrEmpty(driverInfo.ToDate) ? "'" + driverInfo.ToDate : "";
                    worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 12).Value = driverInfo.Type;
                    worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 13).Value = driverInfo.Supplier;
                    worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 14).Value = driverInfo.StatusString;
                    worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                MemoryStream stream = new MemoryStream();
                workbook.SaveAs(stream);
                return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"Driver_{DateTime.Now.ToddMMyyyyHHmmss()}.xlsx"
                };
            }
        }

    }
}
