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

    public class GC_GatesMonitoringHistoryController : ApiControllerBase
    {
        IGC_TimeLogService _GC_TimeLogService;
        IGC_Rules_WarningService _GC_Rules_WarningService;
        public GC_GatesMonitoringHistoryController(IServiceProvider pProvider) : base(pProvider)
        {
            _GC_TimeLogService = TryResolve<IGC_TimeLogService>();
            _GC_Rules_WarningService = TryResolve<IGC_Rules_WarningService>();
        }

        [Authorize]
        [ActionName("GetGatesMonitoringHistories")]
        [HttpPost]
        public IActionResult GetGatesMonitoringHistories(FilterParams param)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var fromTime = DateTime.Parse(param.FromTime);
            var toTime = DateTime.Parse(param.ToTime);
            var historyModels = _GC_TimeLogService.GetHistoryData(param.DepartmentIndexes, param.EmployeeIndexes, user, fromTime, toTime, param.RulesWarningIndexes, param.StatusLog);
            var historyGateData = _GC_TimeLogService.GetPaginationList(historyModels, param.Page, param.PageSize);
            return ApiOk(historyGateData);

        }

        [Authorize]
        [ActionName("ExportGatesHistory")]
        [HttpPost]
        public async Task<IActionResult> ExportGatesHistoryAsync(FilterParams param)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var fromTime = DateTime.Parse(param.FromTime);
            var toTime = DateTime.Parse(param.ToTime);
            var dataForExport = _GC_TimeLogService.GetHistoryData(param.DepartmentIndexes, param.EmployeeIndexes, user, fromTime, toTime, param.RulesWarningIndexes, param.StatusLog);
            var listRulesWarningGroup = await _GC_Rules_WarningService.GetRulesWarningGroupsByCompanyIndex(user.CompanyIndex);
            foreach (var itemExport in dataForExport)
            {
                if (itemExport.Note == "MissingLogOutAfterExpiredPresenceTime")
                    itemExport.Note = "Thiếu log ra sau thời gian quy định";
                if (!string.IsNullOrWhiteSpace(itemExport.Error) && listRulesWarningGroup != null && listRulesWarningGroup.Any(x => x.Code == itemExport.Error))
                    itemExport.Error = listRulesWarningGroup.FirstOrDefault(x => x.Code.Trim() == itemExport.Error.Trim()).Name;
                
                if (itemExport.StatusLog == CommandStatus.Success.ToString())
                {
                    itemExport.StatusLog = "Thành công";
                }
                else
                {
                    itemExport.StatusLog = "Thất bại";
                }

                if (itemExport.InOutMode == DeviceStatus.Input.ToString())
                {
                    itemExport.InOutMode = "Vào";
                }
                else
                {
                    itemExport.InOutMode = "Ra";
                }
            }


            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Log");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "STT";
                worksheet.Cell(currentRow, 2).Value = "Mã chấm công";
                worksheet.Cell(currentRow, 3).Value = "Họ tên";
                worksheet.Cell(currentRow, 4).Value = "Phòng ban";
                worksheet.Cell(currentRow, 5).Value = "Thời gian ghi nhận";
                worksheet.Cell(currentRow, 6).Value = "Chế độ vào ra";
                worksheet.Cell(currentRow, 7).Value = "Cổng";
                worksheet.Cell(currentRow, 8).Value = "Lối đi";
                worksheet.Cell(currentRow, 9).Value = "Trạng thái";
                worksheet.Cell(currentRow, 10).Value = "Thông tin vi phạm";
                worksheet.Cell(currentRow, 11).Value = "Ghi chú";
             
                for (int i = 1; i < 12; i++)
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

                    worksheet.Cell(currentRow, 3).Value = users.CustomerName;
                    worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                  

                    worksheet.Cell(currentRow, 4).Value = users.DepartmentName;
                    worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;



                    worksheet.Cell(currentRow, 5).Value = "'" + users.CheckTime.ToString("dd/MM/yyyy HH:mm:ss");
                    worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 6).Value = users.InOutMode;
                    worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 7).Value = users.GateName;
                    worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 8).Value = users.LineName;
                    worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 9).Value = users.StatusLog;
                    worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 10).Value = users.Error;
                    worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 11).Value = users.Note;
                    worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
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
