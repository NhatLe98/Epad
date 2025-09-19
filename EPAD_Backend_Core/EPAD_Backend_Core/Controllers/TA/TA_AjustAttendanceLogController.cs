using Chilkat;
using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Enums;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
    public class TA_AjustAttendanceLogController : ApiControllerBase
    {
        private IMemoryCache cache;
        ITA_AjustAttendanceLogService _TA_AjustAttendanceLogService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        public TA_AjustAttendanceLogController(IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _TA_AjustAttendanceLogService = TryResolve<ITA_AjustAttendanceLogService>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
        }

        [Authorize]
        [ActionName("GetAjustAttendanceLogAtPage")]
        [HttpPost]
        public IActionResult GetAjustAttendanceLogAtPage([FromBody] TA_AjustAttendanceLogParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var fromDate = Convert.ToDateTime(param.FromDate);
            var toDate = Convert.ToDateTime(param.ToDate);
            var result = _TA_AjustAttendanceLogService.GetDataGrid(user.CompanyIndex, param.Page, param.Limit, param.Filter, param.Departments, param.EmployeeATIDs, fromDate, toDate);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("AddAjustAttendanceLog")]
        [HttpPost]
        public async Task<IActionResult> AddAjustAttendanceLog([FromBody] TA_AjustAttendanceLogInsertDTO data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var check = await _TA_AjustAttendanceLogService.InsertAjustAttendanceLog(data, user);
            if (!string.IsNullOrEmpty(check))
            {
                return ApiError(check);
            }

            return ApiOk();
        }

        [Authorize]
        [ActionName("UpdateAjustAttendanceLog")]
        [HttpPost]
        public async Task<IActionResult> UpdateAjustAttendanceLog([FromBody] TA_AjustAttendanceLogInsertDTO data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var check = await _TA_AjustAttendanceLogService.UpdateAjustAttendanceLog(data, user);
            if (!string.IsNullOrEmpty(check))
            {
                return ApiError(check);
            }

            return ApiOk();
        }


        [Authorize]
        [ActionName("UpdateAjustAttendanceLogLst")]
        [HttpPost]
        public async Task<IActionResult> UpdateAjustAttendanceLogLst([FromBody] List<TA_AjustAttendanceLogDTO> data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var check = await _TA_AjustAttendanceLogService.UpdateAjustAttendanceLogLst(data, user);
            if (!string.IsNullOrEmpty(check))
            {
                return ApiOk(check);
            }

            return ApiOk();
        }

        [Authorize]
        [ActionName("DeleteAjustAttendanceLog")]
        [HttpPost]
        public async Task<IActionResult> DeleteAjustAttendanceLog([FromBody] List<TA_AjustAttendanceLogDTO> data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var check = await _TA_AjustAttendanceLogService.DeleteAjustAttendanceLog(data, user);
            if (!string.IsNullOrEmpty(check))
            {
                return ApiError(check);
            }

            return ApiOk();
        }

        [Authorize]
        [ActionName("AddAjustAttendanceLogFromExcel")]
        [HttpPost]
        public async Task<IActionResult> AddAjustAttendanceLogFromExcel([FromBody] List<TA_AjustAttendanceLogImport> data)
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
                var listError = new List<TA_AjustAttendanceLogImport>();
                var listDataSave = new List<TA_AjustAttendanceLogImport>();

                listError = await _TA_AjustAttendanceLogService.ValidationImportAjustAttendanceLog(data, user);
                listError =  await _TA_AjustAttendanceLogService.CheckAjustAttendanceLogInDatabase(listError, user);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/AjustAttendanceLogError.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/AjustAttendanceLogError.xlsx"));

                if (listError != null && listError.Count() > 0)
                {
                    listDataSave = listError.Where(x => string.IsNullOrEmpty(x.Error)).ToList();
                    data = listError.Where(x => !string.IsNullOrEmpty(x.Error)).ToList();
                    message = data.Count().ToString();

                    using (var workbook = new XLWorkbook(file.FullName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var currentRow = 2;
                        worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                        worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                        worksheet.Cell(currentRow, 3).Value = "Tên nhân viên";
                        worksheet.Cell(currentRow, 4).Value = "Ngày (*)";
                        worksheet.Cell(currentRow, 5).Value = "Vào";
                        worksheet.Cell(currentRow, 6).Value = "Ra";
                        worksheet.Cell(currentRow, 7).Value = "Chế độ điểm danh";
                        worksheet.Cell(currentRow, 8).Value = "Thiết bị";
                        worksheet.Cell(currentRow, 9).Value = "Ghi chú";
                        worksheet.Cell(currentRow, 10).Value = "Lỗi";

                        for (int i = 1; i < 11; i++)
                        {
                            worksheet.Cell(currentRow, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(currentRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        for (int i = (currentRow + 1); i <= worksheet.LastRowUsed().RowNumber(); i++)
                        {
                            worksheet.Row(i).Clear();
                        }

                        foreach (var department in data)
                        {
                            currentRow++;
                            //New template
                            worksheet.Cell(currentRow, 1).Value = "'" + department.EmployeeATID;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 2).Value = "'" + department.EmployeeCode;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 3).Value = department.FullName;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = "'" + department.Date;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = "'" + department.In;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = "'" + department.Out;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = department.VerifyMode;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = department.SerialNumber;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 9).Value = department.Note;
                            worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 10).Value = department.Error;
                            worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        worksheet.Columns().AdjustToContents();
                        worksheet.Rows().AdjustToContents();
                        workbook.SaveAs(file.FullName);
                    }
                }

                await _TA_AjustAttendanceLogService.AddOrUpdateImportAjustAttendanceLog(listDataSave, user);

                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "TA_AjustAttendanceLog";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + lstImport.Count().ToString() + " Department";
                audit.Description = AuditType.Added.ToString() + "AjustAttendanceFromExcel:/:" + listDataSave.Count().ToString();
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
    }
}
