using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Data;
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
    public class TA_ScheduleFixedByDepartmentController : ApiControllerBase
    {
        ITA_ScheduleFixedByDepartmentService _TA_ScheduleFixedByDepartmentService;
        private readonly IHostingEnvironment _hostingEnvironment;
        public TA_ScheduleFixedByDepartmentController(IConfiguration configuration, IServiceProvider pServiceProvider, IHostingEnvironment hostingEnvironment) : base(pServiceProvider)
        {
            _TA_ScheduleFixedByDepartmentService = TryResolve<ITA_ScheduleFixedByDepartmentService>();
            _hostingEnvironment = hostingEnvironment;
        }

        [Authorize]
        [ActionName("GetScheduleFixedByDepartmentAtPage")]
        [HttpPost]
        public async Task<IActionResult> GetScheduleFixedByDepartmentAtPage(ScheduleFixedByDepartmentRequest fixedByDepartmentRequest)
        {
            UserInfo user = GetUserInfo();
            var result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var data = await _TA_ScheduleFixedByDepartmentService.GetScheduleFixedByDepartment(fixedByDepartmentRequest.DepartmentIndexes, fixedByDepartmentRequest.FromDate, user.CompanyIndex, fixedByDepartmentRequest.Page, fixedByDepartmentRequest.PageSize);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("AddScheduleFixedByDepartment")]
        [HttpPost]
        public async Task<IActionResult> AddScheduleFixedByDepartment([FromBody] TA_ScheduleFixedByDepartmentDTO data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var scheduleFixedDepartmentExist = await _TA_ScheduleFixedByDepartmentService.CheckScheduleFixedByDepartmentExist(data, user.CompanyIndex);
            if (scheduleFixedDepartmentExist != string.Empty)
            {
                return ApiOk(scheduleFixedDepartmentExist);
            }

            var isSuccess = await _TA_ScheduleFixedByDepartmentService.AddScheduleFixedByDepartment(data, user.CompanyIndex);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateScheduleFixedByDepartment")]
        [HttpPost]
        public async Task<IActionResult> UpdateScheduleFixedByDepartment([FromBody] TA_ScheduleFixedByDepartmentDTO data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var dataExist = _TA_ScheduleFixedByDepartmentService.FirstOrDefault(x => x.Index == data.Id && x.CompanyIndex == user.CompanyIndex);
            if (dataExist == null)
            {
                return ApiError("ScheduleFixedByDepartmentNotExist");
            }

            var scheduleFixedDepartmentExist = await _TA_ScheduleFixedByDepartmentService.CheckScheduleFixedByDepartmentExist(data, user.CompanyIndex);
            if (scheduleFixedDepartmentExist != string.Empty)
            {
                return ApiOk(scheduleFixedDepartmentExist);
            }

            var isSuccess = await _TA_ScheduleFixedByDepartmentService.UpdateScheduleFixedByDepartment(dataExist, data, user.CompanyIndex);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("DeleteScheduleFixedByDepartment")]
        [HttpDelete]
        public IActionResult DeleteScheduleFixedByDepartment([FromBody] List<int> data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var deleteStatus = _TA_ScheduleFixedByDepartmentService.DeleteScheduleFixedByDepartment(data);
            if (!deleteStatus)
            {
                return ApiError("DeleteScheduleFixedByDepartmentFail");
            }
            return ApiOk();
        }


        [Authorize]
        [ActionName("AddScheduleFixedByDepartmentFromExcel")]
        [HttpPost]
        public async Task<IActionResult> AddScheduleFixedByDepartmentFromExcel([FromBody] List<ScheduleFixedByDepartmentImportExcel> data)
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
                var listError = new List<ScheduleFixedByDepartmentImportExcel>();

                listError = await _TA_ScheduleFixedByDepartmentService.AddScheduleDepartmentFromExcel(data, user);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_ScheduleDepartment_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_ScheduleDepartment_Error.xlsx"));

                if (listError != null && listError.Count() > 0)
                {
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook(file.FullName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Phòng ban (*)";
                        worksheet.Cell(currentRow, 2).Value = "Ngày áp dụng (*)";
                        worksheet.Cell(currentRow, 3).Value = "Đến ngày";
                        worksheet.Cell(currentRow, 4).Value = "Thứ hai";
                        worksheet.Cell(currentRow, 5).Value = "Thứ ba";
                        worksheet.Cell(currentRow, 6).Value = "Thứ tư";
                        worksheet.Cell(currentRow, 7).Value = "Thứ năm";
                        worksheet.Cell(currentRow, 8).Value = "Thứ sáu";
                        worksheet.Cell(currentRow, 9).Value = "Thứ bảy";
                        worksheet.Cell(currentRow, 10).Value = "Chủ nhật";

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
                            worksheet.Cell(currentRow, 1).Value = "'" + department.DepartmentName;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 2).Value = "'" + department.FromDateFormat;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 3).Value = department.ToDateFormat;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = "'" + department.MondayShift;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = department.TuesdayShift;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = "'" + department.WednesdayShift;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = "'" + department.ThursdayShift;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = department.FridayShift;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 9).Value = department.SaturdayShift;
                            worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 10).Value = department.SundayShift;
                            worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 11).Value = department.ErrorMessage;
                            worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(currentRow, 11).Style.Alignment.SetWrapText(true);
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
            var folderDetails = Path.Combine(sWebRootFolder, @"epad/dist/Template_ScheduleDepartment.xlsx");

            _TA_ScheduleFixedByDepartmentService.ExportInfoShift(folderDetails);
#endif
            return ApiOk();
        }

    }
}
