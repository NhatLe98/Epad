using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
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
using static EPAD_Common.Utility.AppUtils;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TA_LeaveRegistrationController : ApiControllerBase
    {
        ITA_LeaveRegistrationService _TA_LeaveRegistrationService;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        private readonly IHostingEnvironment _hostingEnvironment;
        public TA_LeaveRegistrationController(IConfiguration configuration, IServiceProvider pServiceProvider, IHostingEnvironment hostingEnvironment) : base(pServiceProvider)
        {
            _TA_LeaveRegistrationService = TryResolve<ITA_LeaveRegistrationService>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _hostingEnvironment = hostingEnvironment;
        }

        [Authorize]
        [ActionName("GetLeaveDateType")]
        [HttpGet]
        public async Task<IActionResult> GetLeaveDateType()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = await _TA_LeaveRegistrationService.GetAllLeaveDateType();
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetLeaveDurationType")]
        [HttpGet]
        public async Task<IActionResult> GetLeaveDurationType()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = EnumExtension.GetListEnum<LeaveDurationType>();
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetHaftLeaveType")]
        [HttpGet]
        public async Task<IActionResult> GetHaftLeaveType()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = EnumExtension.GetListEnum<HaftLeaveType>();
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetLeaveRegistration")]
        [HttpPost]
        public async Task<IActionResult> GetLeaveRegistration([FromBody] LeaveRegistrationModel param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var format = "yyyy-MM-dd";
            if (!string.IsNullOrWhiteSpace(param.FromDateString))
            {
                if (!DateTime.TryParseExact(param.FromDateString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                param.FromDate = time;
            }
            
            if (!string.IsNullOrWhiteSpace(param.ToDateString))
            {
                if (!DateTime.TryParseExact(param.ToDateString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                param.ToDate = time;
            }  

            var data = await _TA_LeaveRegistrationService.GetLeaveRegistration(param, user);

            return ApiOk(data);
        }

        [Authorize]
        [ActionName("AddLeaveRegistration")]
        [HttpPost]
        public async Task<IActionResult> AddLeaveRegistration([FromBody] LeaveRegistrationModel data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var format = "yyyy-MM-dd";
            if (!string.IsNullOrWhiteSpace(data.FromDateString))
            {
                if (!DateTime.TryParseExact(data.FromDateString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.FromDate = time;
            }
            else
            {
                return ApiError("PleaseSelectFromDate");
            }
            if (!string.IsNullOrWhiteSpace(data.ToDateString))
            {
                if (!DateTime.TryParseExact(data.ToDateString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.ToDate = time;
            }
            else
            {
                return ApiError("PleaseSelectToDate");
            }

            if (data.FromDate.HasValue && data.ToDate.HasValue && data.FromDate.Value.Date > data.ToDate.Value.Date)
            {
                return ApiError("FromDateCannotLargerThanToDate");
            }

            var listDate = DateTimeExtension.GetListDate(data.FromDate.Value, data.ToDate.Value);

            var lockAttendanceTimeValidDate = await _TA_LeaveRegistrationService.GetLockAttendanceTimeValidDate(user);
            if (listDate.Any(x => x.Date < lockAttendanceTimeValidDate.Date))
            {
                return ApiError("PassedLockAttendanceTime");
            }

            var checkError = await _TA_LeaveRegistrationService.CheckRuleLeaveRegister(data, user);
            if (checkError != null && checkError.Count > 0)
            {
                return ApiOk(checkError);
            }

            data.UpdatedUser = user.FullName;
            data.CreatedDate = DateTime.Now;
            data.UpdatedDate = DateTime.Now;

            var isSuccess = await _TA_LeaveRegistrationService.AddLeaveRegistration(data, user);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateLeaveRegistration")]
        [HttpPut]
        public async Task<IActionResult> UpdateLeaveRegistration([FromBody] LeaveRegistrationModel data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var format = "yyyy-MM-dd";
            if (!string.IsNullOrWhiteSpace(data.LeaveDateString))
            {
                if (!DateTime.TryParseExact(data.LeaveDateString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.LeaveDate = time;
            }
            else
            {
                return ApiError("PleaseSelectDate");
            }

            data.FromDate = data.LeaveDate;
            data.ToDate = data.LeaveDate;

            var listDate = DateTimeExtension.GetListDate(data.FromDate.Value, data.ToDate.Value);

            var lockAttendanceTimeValidDate = await _TA_LeaveRegistrationService.GetLockAttendanceTimeValidDate(user);
            if (listDate.Any(x => x.Date < lockAttendanceTimeValidDate.Date))
            {
                return ApiError("PassedLockAttendanceTime");
            }

            var checkError = await _TA_LeaveRegistrationService.CheckRuleLeaveRegister(data, user);
            if (checkError != null && checkError.Count > 0)
            {
                return ApiOk(checkError);
            }

            data.UpdatedUser = user.FullName;
            data.UpdatedDate = DateTime.Now;

            var isSuccess = await _TA_LeaveRegistrationService.UpdateLeaveRegistration(data, user);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("DeleteLeaveRegistration")]
        [HttpDelete]
        public async Task<IActionResult> DeleteLeaveRegistration(List<int> index)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var leaveRegistration = await _TA_LeaveRegistrationService.GetLeaveRegistrationByListIndex(index);

            var lockAttendanceTimeValidDate = await _TA_LeaveRegistrationService.GetLockAttendanceTimeValidDate(user);
            if (leaveRegistration.Any(x => x.LeaveDate.Date < lockAttendanceTimeValidDate.Date))
            {
                return ApiError("PassedLockAttendanceTime");
            }

            var isSuccess = await _TA_LeaveRegistrationService.DeleteLeaveRegistration(index);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("ExportTemplateLeaveRegistration")]
        [HttpGet]
        public IActionResult ExportTemplateLeaveRegistration()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            string sWebRootFolder = _hostingEnvironment.ContentRootPath;

            var folderDetails = Path.Combine(sWebRootFolder, @"epad/dist/EmployeeLeaveDay.xlsx");
            //Use local 
#if DEBUG
            folderDetails = Path.Combine(sWebRootFolder, @"epad/public/EmployeeLeaveDay.xlsx");
#endif

#if !DEBUG
            _TA_LeaveRegistrationService.ExportTemplateLeaveRegister(folderDetails);
#endif

            return ApiOk();
        }

        [Authorize]
        [ActionName("AddLeaveRegistrationFromExcel")]
        [HttpPost]
        public async Task<IActionResult> AddLeaveRegistrationFromExcel([FromBody] List<LeaveRegistrationModel> data)
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
                var listError = new List<LeaveRegistrationModel>();

                listError = await _TA_LeaveRegistrationService.ValidationImportLeaveRegistration(data, user);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/EmployeeLeaveDayError.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/EmployeeLeaveDayError.xlsx"));

                if (listError != null && listError.Count() > 0)
                {
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook(file.FullName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var currentRow = 2;
                        worksheet.Cell(currentRow, 1).Value = "MCC (*)";
                        worksheet.Cell(currentRow, 2).Value = "MNV";
                        worksheet.Cell(currentRow, 3).Value = "Họ tên";
                        worksheet.Cell(currentRow, 4).Value = "Từ ngày (*)";
                        worksheet.Cell(currentRow, 5).Value = "Đến ngày (*)";
                        worksheet.Cell(currentRow, 6).Value = "Loại ngày nghỉ (*)";
                        worksheet.Cell(currentRow, 7).Value = "Cả ngày";
                        worksheet.Cell(currentRow, 8).Value = "Đầu ca";
                        worksheet.Cell(currentRow, 9).Value = "Cuối ca";
                        worksheet.Cell(currentRow, 10).Value = "Lý do";
                        worksheet.Cell(currentRow, 11).Value = "Lỗi";

                        for (int i = 1; i < 12; i++)
                        {
                            worksheet.Cell(currentRow, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(currentRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            //worksheet.Column(i).Width = 15;
                            //if (i == 11)
                            //{ 
                            //    worksheet.Column(i).Width = 50;
                            //}
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

                            worksheet.Cell(currentRow, 3).Value = department.FullName;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = "'" + department.FromDateString;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = "'" + department.ToDateString;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = department.LeaveDateTypeName;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = department.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift ? "x" : string.Empty;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = department.FirstHaftLeave ? "x" : string.Empty;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 9).Value = department.LastHaftLeave ? "x" : string.Empty;
                            worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 10).Value = department.Description;
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

                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "TA_LeaveRegistration";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + lstImport.Count().ToString() + " Department";
                audit.Description = AuditType.Added.ToString() + "LeaveRegistrationFromExcel:/:" + data.Count().ToString();
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
