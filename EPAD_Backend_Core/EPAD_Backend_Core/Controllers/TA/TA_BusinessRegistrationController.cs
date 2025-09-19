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
    public class TA_BusinessRegistrationController : ApiControllerBase
    {
        ITA_BusinessRegistrationService _TA_BusinessRegistrationService;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        private readonly IHostingEnvironment _hostingEnvironment;
        public TA_BusinessRegistrationController(IConfiguration configuration, IServiceProvider pServiceProvider, IHostingEnvironment hostingEnvironment) : base(pServiceProvider)
        {
            _TA_BusinessRegistrationService = TryResolve<ITA_BusinessRegistrationService>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _hostingEnvironment = hostingEnvironment;
        }

        [Authorize]
        [ActionName("GetBusinessType")]
        [HttpGet]
        public async Task<IActionResult> GetBusinessType()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = EnumExtension.GetListEnum<BusinessType>();
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetBusinessRegistration")]
        [HttpPost]
        public async Task<IActionResult> GetBusinessRegistration([FromBody] BusinessRegistrationModel param)
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

            var data = await _TA_BusinessRegistrationService.GetBusinessRegistration(param, user);

            return ApiOk(data);
        }

        [Authorize]
        [ActionName("AddBusinessRegistration")]
        [HttpPost]
        public async Task<IActionResult> AddBusinessRegistration([FromBody] BusinessRegistrationModel data)
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

            if (data.BusinessType == (short)BusinessType.BusinessFromToTime)
            {
                var timeFormat = "yyyy-MM-dd HH:mm";
                if (!string.IsNullOrWhiteSpace(data.FromTimeString))
                {
                    if (!DateTime.TryParseExact(data.FromTimeString, timeFormat, null, System.Globalization.DateTimeStyles.None,
                        out var time))
                    {
                        return ApiError("DateTimeWrongFormat");
                    }
                    data.FromTime = time;
                }
                else
                {
                    return ApiError("PlaeseSelectStartTime");
                }
                if (!string.IsNullOrWhiteSpace(data.ToTimeString))
                {
                    if (!DateTime.TryParseExact(data.ToTimeString, timeFormat, null, System.Globalization.DateTimeStyles.None,
                        out var time))
                    {
                        return ApiError("DateTimeWrongFormat");
                    }
                    data.ToTime = time;
                }
                else
                {
                    return ApiError("PleaseSelectEndTime");
                }
                if (data.FromTime > data.ToTime)
                {
                    return ApiError("StartTimeCannotLargerThanToTime");
                }
            }

            var listDate = DateTimeExtension.GetListDate(data.FromDate.Value, data.ToDate.Value);

            var lockAttendanceTimeValidDate = await _TA_BusinessRegistrationService.GetLockAttendanceTimeValidDate(user);
            if (listDate.Any(x => x.Date < lockAttendanceTimeValidDate.Date))
            {
                return ApiError("PassedLockAttendanceTime");
            }

            var checkError = await _TA_BusinessRegistrationService.CheckRuleBusinessRegister(data, user);
            if (checkError != null && checkError.Count > 0)
            {
                return ApiOk(checkError);
            }

            data.UpdatedUser = user.FullName;
            data.CreatedDate = DateTime.Now;
            data.UpdatedDate = DateTime.Now;

            var isSuccess = await _TA_BusinessRegistrationService.AddBusinessRegistration(data, user);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateBusinessRegistration")]
        [HttpPut]
        public async Task<IActionResult> UpdateBusinessRegistration([FromBody] BusinessRegistrationModel data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var format = "yyyy-MM-dd";
            if (!string.IsNullOrWhiteSpace(data.BusinessDateString))
            {
                if (!DateTime.TryParseExact(data.BusinessDateString, format, null, System.Globalization.DateTimeStyles.None,
                    out var time))
                {
                    return ApiError("DateTimeWrongFormat");
                }
                data.BusinessDate = time;
            }
            else
            {
                return ApiError("PleaseSelectDate");
            }

            if (data.BusinessType == (short)BusinessType.BusinessFromToTime)
            {
                var timeFormat = "yyyy-MM-dd HH:mm";
                if (!string.IsNullOrWhiteSpace(data.FromTimeString))
                {
                    if (!DateTime.TryParseExact(data.FromTimeString, timeFormat, null, System.Globalization.DateTimeStyles.None,
                        out var time))
                    {
                        return ApiError("DateTimeWrongFormat");
                    }
                    data.FromTime = time;
                }
                else
                {
                    return ApiError("PlaeseSelectStartTime");
                }
                if (!string.IsNullOrWhiteSpace(data.ToTimeString))
                {
                    if (!DateTime.TryParseExact(data.ToTimeString, timeFormat, null, System.Globalization.DateTimeStyles.None,
                        out var time))
                    {
                        return ApiError("DateTimeWrongFormat");
                    }
                    data.ToTime = time;
                }
                else
                {
                    return ApiError("PleaseSelectEndTime");
                }
                if (data.FromTime > data.ToTime)
                {
                    return ApiError("StartTimeCannotLargerThanToTime");
                }
            }

            data.FromDate = data.BusinessDate;
            data.ToDate = data.BusinessDate;

            var listDate = DateTimeExtension.GetListDate(data.FromDate.Value, data.ToDate.Value);

            var lockAttendanceTimeValidDate = await _TA_BusinessRegistrationService.GetLockAttendanceTimeValidDate(user);
            if (listDate.Any(x => x.Date < lockAttendanceTimeValidDate.Date))
            {
                return ApiError("PassedLockAttendanceTime");
            }

            var checkError = await _TA_BusinessRegistrationService.CheckRuleBusinessRegister(data, user);
            if (checkError != null && checkError.Count > 0)
            {
                return ApiOk(checkError);
            }

            data.UpdatedUser = user.FullName;
            data.UpdatedDate = DateTime.Now;

            var isSuccess = await _TA_BusinessRegistrationService.UpdateBusinessRegistration(data, user);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("DeleteBusinessRegistration")]
        [HttpDelete]
        public async Task<IActionResult> DeleteBusinessRegistration(List<int> index)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var leaveRegistration = await _TA_BusinessRegistrationService.GetBusinessRegistrationByListIndex(index);

            var lockAttendanceTimeValidDate = await _TA_BusinessRegistrationService.GetLockAttendanceTimeValidDate(user);
            if (leaveRegistration.Any(x => x.BusinessDate.Date < lockAttendanceTimeValidDate.Date))
            {
                return ApiError("PassedLockAttendanceTime");
            }

            var isSuccess = await _TA_BusinessRegistrationService.DeleteBusinessRegistration(index);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("AddBusinessRegistrationFromExcel")]
        [HttpPost]
        public async Task<IActionResult> AddBusinessRegistrationFromExcel([FromBody] List<BusinessRegistrationModel> data)
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
                var listError = new List<BusinessRegistrationModel>();

                listError = await _TA_BusinessRegistrationService.ValidationImportBusinessRegistration(data, user);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/MissionError.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/MissionError.xlsx"));

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
                        worksheet.Cell(currentRow, 6).Value = "Cả ngày";
                        worksheet.Cell(currentRow, 7).Value = "Từ giờ";
                        worksheet.Cell(currentRow, 8).Value = "Đến giờ";
                        worksheet.Cell(currentRow, 9).Value = "Nơi công tác";
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

                            worksheet.Cell(currentRow, 6).Value = department.BusinessType == (short)BusinessType.BusinessAllShift ? "x" : string.Empty;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = "'" + department.FromTimeString;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = "'" + department.ToTimeString;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 9).Value = department.Description;
                            worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 10).Value = department.WorkPlace;
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
                audit.TableName = "TA_BusinessRegistration";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + lstImport.Count().ToString() + " Department";
                audit.Description = AuditType.Added.ToString() + "BusinessRegistrationFromExcel:/:" + data.Count().ToString();
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
