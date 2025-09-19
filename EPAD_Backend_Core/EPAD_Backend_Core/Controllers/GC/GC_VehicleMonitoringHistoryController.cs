using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using EPAD_Services.Plugins;
using EPAD_Common.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public class GC_VehicleMonitoringHistoryController : ApiControllerBase
    {
        IGC_VehicleLogService _GC_VehicleLogService;
        IGC_Rules_WarningService _GC_Rules_WarningService;
        public GC_VehicleMonitoringHistoryController(IServiceProvider pProvider) : base(pProvider)
        {
            _GC_VehicleLogService = TryResolve<IGC_VehicleLogService>();
            _GC_Rules_WarningService = TryResolve<IGC_Rules_WarningService>();
        }

        [Authorize]
        [ActionName("GetVehicleMonitoringHistories")]
        [HttpPost]
        public IActionResult GetVehicleMonitoringHistories(FilterParams param)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var fromTime = DateTime.Parse(param.FromTime);
            var toTime = DateTime.Parse(param.ToTime);
            var historyModels = _GC_VehicleLogService.GetHistoryData(param.DepartmentIndexes, param.EmployeeIndexes, user, fromTime, toTime, param.StatusLog, param.Filter);
            var historyGateData = _GC_VehicleLogService.GetPaginationList(historyModels, param.Page, param.PageSize);
            return ApiOk(historyGateData);

        }

        [Authorize]
        [ActionName("ExportVehicleHistory")]
        [HttpPost]
        public IActionResult ExportVehicleHistory(FilterParams param)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var fromTime = DateTime.Parse(param.FromTime);
            var toTime = DateTime.Parse(param.ToTime);
            var dataForExport = _GC_VehicleLogService.GetHistoryData(param.DepartmentIndexes, param.EmployeeIndexes, user, fromTime, toTime, param.StatusLog, param.Filter);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Log");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "STT";
                worksheet.Cell(currentRow, 2).Value = "Mã chấm công";
                worksheet.Cell(currentRow, 3).Value = "Mã nhân viên";
                worksheet.Cell(currentRow, 4).Value = "Họ tên";
                worksheet.Cell(currentRow, 5).Value = "Phòng ban";
                worksheet.Cell(currentRow, 6).Value = "Loại phương tiện";
                worksheet.Cell(currentRow, 7).Value = "Biển số";
                worksheet.Cell(currentRow, 8).Value = "Thời gian vào";
                worksheet.Cell(currentRow, 9).Value = "Thời gian ra";
                worksheet.Cell(currentRow, 10).Value = "Lý do";

                for (int i = 1; i < 11; i++)
                {
                    worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Column(i).Width = 20;
                }
                var no = 1;

                foreach (var users in dataForExport)
                {
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = no;
                    worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 2).Value = users.EmployeeATID;
                    worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeATID.Length, '0');

                    worksheet.Cell(currentRow, 3).Value = users.EmployeeCode;
                    worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                    worksheet.Cell(currentRow, 4).Value = users.CustomerName;
                    worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 5).Value = users.DepartmentName;
                    worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 6).Value = users.VehicleType;
                    worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 7).Value = users.Plate;
                    worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                    worksheet.Cell(currentRow, 8).Value = users.FromTime != null ? "'" + users.FromTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "";
                    worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 9).Value = users.ToTime != null ? "'" + users.ToTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "";
                    worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 10).Value = users.Reason;
                    worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    no++;
                }

                MemoryStream stream = new MemoryStream();
                workbook.SaveAs(stream);
                return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"Employee_{DateTime.Now.ToString("ddMMyyyyhhmmss")}.xlsx"
                };
            }
        }
    }
}
