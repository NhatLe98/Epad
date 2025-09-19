using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Data.Models;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TA_ScheduleFixedByEmployeeController : ApiControllerBase
    {
        ITA_ScheduleFixedByEmployeeService _TA_ScheduleFixedByEmployeeService;
        private readonly IHostingEnvironment _hostingEnvironment;
        public TA_ScheduleFixedByEmployeeController(IConfiguration configuration, IServiceProvider pServiceProvider, IHostingEnvironment hostingEnvironment) : base(pServiceProvider)
        {
            _TA_ScheduleFixedByEmployeeService = TryResolve<ITA_ScheduleFixedByEmployeeService>();
            _hostingEnvironment = hostingEnvironment;
        }

        [Authorize]
        [ActionName("GetScheduleFixedByEmployeeAtPage")]
        [HttpPost]
        public async Task<IActionResult> GetScheduleFixedByEmployeeAtPage(ScheduleFixedByEmployeeRequest fixedByEmployeeRequest)
        {
            UserInfo user = GetUserInfo();
            var result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var data = await _TA_ScheduleFixedByEmployeeService.GetScheduleFixedByEmployee(fixedByEmployeeRequest.EmployeeATIDs, fixedByEmployeeRequest.FromDate, user.CompanyIndex, fixedByEmployeeRequest.Page, fixedByEmployeeRequest.PageSize);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("AddScheduleFixedByEmployee")]
        [HttpPost]
        public async Task<IActionResult> AddScheduleFixedByEmployee([FromBody] TA_ScheduleFixedByEmployeeDTO data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var scheduleFixedDepartmentExist = await _TA_ScheduleFixedByEmployeeService.CheckScheduleFixedByEmployeeExist(data, user.CompanyIndex);
            if (scheduleFixedDepartmentExist != string.Empty)
            {
                return ApiOk(scheduleFixedDepartmentExist);
            }

            var isSuccess = await _TA_ScheduleFixedByEmployeeService.AddScheduleFixedByEmployee(data, user.CompanyIndex);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateScheduleFixedByEmployee")]
        [HttpPost]
        public async Task<IActionResult> UpdateScheduleFixedByEmployee([FromBody] TA_ScheduleFixedByEmployeeDTO data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var dataExist = _TA_ScheduleFixedByEmployeeService.FirstOrDefault(x => x.Index == data.Id && x.CompanyIndex == user.CompanyIndex);
            if (dataExist == null)
            {
                return ApiError("ScheduleFixedByEmployeeNotExist");
            }

            var scheduleFixedDepartmentExist = await _TA_ScheduleFixedByEmployeeService.CheckScheduleFixedByEmployeeExist(data, user.CompanyIndex);
            if (scheduleFixedDepartmentExist != string.Empty)
            {
                return ApiOk(scheduleFixedDepartmentExist);
            }

            var isSuccess = await _TA_ScheduleFixedByEmployeeService.UpdateScheduleFixedByEmployee(dataExist, data, user.CompanyIndex);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("DeleteScheduleFixedByEmployee")]
        [HttpDelete]
        public IActionResult DeleteScheduleFixedByEmployee([FromBody] List<int> data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var deleteStatus = _TA_ScheduleFixedByEmployeeService.DeleteScheduleFixedByEmployee(data);
            if (!deleteStatus)
            {
                return ApiError("DeleteScheduleFixedByEmployeeFail");
            }
            return ApiOk();
        }

        [Authorize]
        [ActionName("AddScheduleFixedByEmployeeFromExcel")]
        [HttpPost]
        public async Task<IActionResult> AddScheduleFixedByEmployeeFromExcel([FromBody] List<ScheduleFixedByEmployeeImportExcel> data)
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
                var listError = new List<ScheduleFixedByEmployeeImportExcel>();

                listError = await _TA_ScheduleFixedByEmployeeService.AddScheduleFixedByEmployeeFromExcel(data, user);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_ScheduleEmployee_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_ScheduleEmployee_Error.xlsx"));

                if (listError != null && listError.Count() > 0)
                {
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook(file.FullName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                        worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                        worksheet.Cell(currentRow, 3).Value = "Tên nhân viên";
                        worksheet.Cell(currentRow, 4).Value = "Ngày áp dụng (*)";
                        worksheet.Cell(currentRow, 5).Value = "Đến ngày";
                        worksheet.Cell(currentRow, 6).Value = "Thứ hai";
                        worksheet.Cell(currentRow, 7).Value = "Thứ ba";
                        worksheet.Cell(currentRow, 8).Value = "Thứ tư";
                        worksheet.Cell(currentRow, 9).Value = "Thứ năm";
                        worksheet.Cell(currentRow, 10).Value = "Thứ sáu";
                        worksheet.Cell(currentRow, 11).Value = "Thứ bảy";
                        worksheet.Cell(currentRow, 12).Value = "Chủ nhật";

                        for (int i = 1; i <= 10; i++)
                        {
                            worksheet.Cell(currentRow, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(currentRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        for (int i = (currentRow + 1); i <= worksheet.LastRowUsed().RowNumber(); i++)
                        {
                            worksheet.Row(i).Clear();
                        }

                        foreach (var department in listError)
                        {
                            currentRow++;
                            //New template
                            worksheet.Cell(currentRow, 1).Value = "'" + department.EmployeeATID;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 2).Value = "'" + department.EmployeeCode;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 3).Value = "'" + department.FullName;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = "'" + department.FromDateFormat;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = department.ToDateFormat;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = "'" + department.MondayShift;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = department.TuesdayShift;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = "'" + department.WednesdayShift;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 9).Value = "'" + department.ThursdayShift;
                            worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 10).Value = department.FridayShift;
                            worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 11).Value = department.SaturdayShift;
                            worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 12).Value = department.SundayShift;
                            worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 13).Value = department.ErrorMessage;
                            worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(currentRow, 13).Style.Alignment.SetWrapText(true);
                        }
                        worksheet.Columns().AdjustToContents();
                        worksheet.Rows().AdjustToContents();
                        workbook.SaveAs(file.FullName);
                    }
                }

                result = Ok(message);
                return result;
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
        }

        [ActionName("ExportInfoShift")]
        [HttpGet]
        public IActionResult ExportInfoShift()
        {
#if !DEBUG
            string sWebRootFolder = _hostingEnvironment.ContentRootPath;
            var folderDetails = Path.Combine(sWebRootFolder, @"epad/dist/Template_ScheduleEmployee.xlsx");

            _TA_ScheduleFixedByEmployeeService.ExportInfoShift(folderDetails);
#endif
            return ApiOk();
        }
    }
}
