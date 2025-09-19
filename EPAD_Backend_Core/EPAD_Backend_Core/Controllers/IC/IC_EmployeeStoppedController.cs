using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Data.Models.IC;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/IC_EmployeeStopped/[action]")]
    [ApiController]
    public class IC_EmployeeStoppedController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private IIC_DepartmentLogic _iIC_DepartmentLogic;
        private IIC_EmployeeStoppedService _IC_EmployeeStoppedService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        public IC_EmployeeStoppedController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _iIC_DepartmentLogic = TryResolve<IIC_DepartmentLogic>();
            _IC_EmployeeStoppedService = TryResolve<IIC_EmployeeStoppedService>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
        }
        [Authorize]
        [ActionName("GetEmployeeStopped")]
        [HttpGet]
        public IActionResult GetEmployeeStopped(int page, string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var result = _IC_EmployeeStoppedService.GetDataGrid(user.CompanyIndex, page, limit, filter);
            return ApiOk(result);
        }
        [Authorize]
        [ActionName("AddEmployeeStopped")]
        [HttpPost]
        public async Task<IActionResult> AddEmployeeStopped(IC_EmployeeStoppedDTO ic_EmployeeStopped)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return result;
            }
            var formats = "dd-MM-yyyy";
            if (DateTime.TryParseExact(ic_EmployeeStopped.StoppedDateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var stoppedDate) || !string.IsNullOrWhiteSpace(ic_EmployeeStopped.StoppedDateString))
            {
                ic_EmployeeStopped.StoppedDate = stoppedDate;
            }
            else
            {
                return ApiError("DateTimeWrongFormat");
            }
            var employeeStopped = await _IC_EmployeeStoppedService.AddEmployeeStopped(ic_EmployeeStopped, user);
            if (!string.IsNullOrEmpty(employeeStopped))
            {
                return ApiOk(employeeStopped);
            }
            return ApiOk();
        }
        [Authorize]
        [ActionName("UpdateEmployeeStopped")]
        [HttpPut]
        public async Task<IActionResult> UpdateEmployeeStopped(IC_EmployeeStoppedDTO ic_EmployeeStopped)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var existingEmployee = await _IC_EmployeeStoppedService.GetEmployeeStoppedByIndex(ic_EmployeeStopped.Index, user.CompanyIndex);
            if (existingEmployee == null)
            {
                return ApiError("EmployeeStoppedNotExist");
            }
            var isSuccess = await _IC_EmployeeStoppedService.UpdateEmployeeStopped(ic_EmployeeStopped, user, existingEmployee);

            return ApiOk(isSuccess);
        }
        [Authorize]
        [ActionName("DeleteEmployeeStopped")]
        [HttpDelete]
        public async Task<IActionResult> DeleteEmployeeStopped(List<int> listIndex)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var isSuccess = await _IC_EmployeeStoppedService.DeleteEmployeeStopped(listIndex);
            return ApiOk(isSuccess);
        }
        [Authorize]
        [ActionName("AddEmployeeStoppedFromExcel")]
        [HttpPost]
        public IActionResult AddEmployeeStoppedFromExcel([FromBody] List<IC_EmployeeStoppedImportDTO> lstImport)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            ConfigObject config = ConfigObject.GetConfig(cache);
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            try
            {
                var listError = new List<IC_EmployeeStoppedImportDTO>();
                listError = _IC_EmployeeStoppedService.ValidationImportEmployeeStopped(lstImport, user);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_EmployeeStopped_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_EmployeeStopped_Error.xlsx"));
                if (listError != null && listError.Count() > 0)
                {
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook(file.FullName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var currentRow = 1;

                        worksheet.Cell(currentRow, 1).Value = "MCC (*)";
                        worksheet.Cell(currentRow, 2).Value = "Phòng ban";
                        worksheet.Cell(currentRow, 3).Value = "Họ tên";
                        worksheet.Cell(currentRow, 4).Value = "Ngày nghỉ việc (*)";
                        worksheet.Cell(currentRow, 5).Value = "Lý do (*)";
                        worksheet.Cell(currentRow, 6).Value = "Lỗi";

                        for (int i = 1; i < 7; i++)
                        {
                            worksheet.Cell(currentRow, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(currentRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Column(i).Width = 20;
                        }

                        for (int i = (currentRow + 1); i <= worksheet.LastRowUsed().RowNumber(); i++)
                        {
                            worksheet.Row(i).Clear();
                        }

                        var departmentName = context.IC_Department
                            .Select(d => d.Name)
                            .FirstOrDefault();
                        var fullName = context.HR_User
                            .Select(u => u.FullName)
                            .FirstOrDefault();

                        foreach (var item in listError)
                        {
                            currentRow++;

                            worksheet.Cell(currentRow, 1).Value = "'" + item.EmployeeATID;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 2).Value = "'" + departmentName;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 3).Value = "'" + fullName;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = "'" + item.StoppedDate;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = "'" + item.Reason;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = "'" + item.ErrorMessage;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        worksheet.Columns().AdjustToContents();
                        worksheet.Rows().AdjustToContents();
                        workbook.SaveAs(file.FullName);
                    }
                }
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "IC_EmployeeStopped";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                audit.Description = AuditType.Added.ToString() + "AddEmployeeStoppedFromExcel:/:" + lstImport.Count().ToString();
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