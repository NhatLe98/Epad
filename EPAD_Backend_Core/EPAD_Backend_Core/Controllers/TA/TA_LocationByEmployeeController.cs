using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Data.Models.IC;
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
    public class TA_LocationByEmployeeController : ApiControllerBase
    {
        private IMemoryCache cache;
        private readonly ITA_LocationByEmployeeService _TA_LocationByEmployeeService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        public TA_LocationByEmployeeController(IConfiguration configuration, IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _TA_LocationByEmployeeService = TryResolve<ITA_LocationByEmployeeService>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
        }
        [Authorize]
        [ActionName("GetLocationByEmployeeAtPage")]
        [HttpGet]
        public IActionResult GetLocationByEmployeeAtPage(int page, int limit, string filter)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = _TA_LocationByEmployeeService.GetDataGrid(user.CompanyIndex, page, limit, filter);
            return ApiOk(result);
        }
        [Authorize]
        [ActionName("AddLocationByEmployee")]
        [HttpPost]
        public async Task<IActionResult> AddLocationByEmployee(TA_LocationByEmployeeDTO data)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = await _TA_LocationByEmployeeService.AddListLocationByEmployee(data, user);

            return ApiOk(result);
        }
        [Authorize]
        [ActionName("UpdateLocationByEmployee")]
        [HttpPut]
        public async Task<IActionResult> UpdateLocationByEmployee(TA_LocationByEmployeeDTO data)
        {
            UserInfo user = GetUserInfo();
            if (user == null)
            {
                return ApiUnauthorized();
            }

            var existEmployee = await _TA_LocationByEmployeeService.GetLocationByEmployeeByIndex(data.EmployeeIndexDTO);
            if (existEmployee == null)
            {
                return ApiError("UpdateLocationFail");
            }

            var isSuccess = await _TA_LocationByEmployeeService.UpdateLocationByEmployee(data, user);
            return ApiOk(isSuccess);
        }
        [Authorize]
        [ActionName("DeleteLocationByEmployee")]
        [HttpDelete]
        public IActionResult DeleteLocationByEmployee([FromBody] List<int> listIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var deleteEmployee = _TA_LocationByEmployeeService.DeleteListLocationByEmployee(listIndex);
            return ApiOk();
        }

        [Authorize]
        [ActionName("AddLocationByEmployeeFromExcel")]
        [HttpPost]
        public IActionResult AddLocationByEmployeeFromExcel([FromBody] List<TA_LocationByEmployeeImportExcel> lstImport)
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
                var listError = new List<TA_LocationByEmployeeImportExcel>();
                listError = _TA_LocationByEmployeeService.ValidationImportLocationByEmployee(lstImport, user);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_TA_LocationByEmployee.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_LocationByEmployee_Error.xlsx"));
                if (listError != null && listError.Count() > 0)
                {
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook(file.FullName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var currentRow = 1;

                        worksheet.Cell(currentRow, 1).Value = "MCC (*)";
                        worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                        worksheet.Cell(currentRow, 3).Value = "Tên nhân viên";
                        worksheet.Cell(currentRow, 4).Value = "Địa điểm (*)";
                        worksheet.Cell(currentRow, 5).Value = "Lỗi";

                        for (int i = 1; i < 6; i++)
                        {
                            worksheet.Cell(currentRow, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(currentRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Column(i).Width = 20;
                        }

                        for (int i = (currentRow + 1); i <= worksheet.LastRowUsed().RowNumber(); i++)
                        {
                            worksheet.Row(i).Clear();
                        }

                        foreach (var item in listError)
                        {
                            currentRow++;

                            worksheet.Cell(currentRow, 1).Value = "'" + item.EmployeeATID;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 2).Value = "'" + item.EmployeeCode;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 3).Value = "'" + item.FullName;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = "'" + item.LocationName;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = "'" + item.ErrorMessage;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        worksheet.Columns().AdjustToContents();
                        worksheet.Rows().AdjustToContents();
                        workbook.SaveAs(file.FullName);
                    }
                }
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "TA_LocationByEmloyee";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                audit.Description = AuditType.Added.ToString() + "AddLocationByEmployeeFromExcel:/:" + lstImport.Count().ToString();
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

        //CCCD///
        [HttpPost("parse-cccd")]
        public IActionResult ParseCccd([FromBody] QRCodeData data)
        {
            if (data == null)
            {
                return BadRequest("No data received.");
            }
            if (string.IsNullOrEmpty(data.QrData))
            {
                return BadRequest("QRData is empty");
            }

            var fields = data.QrData.Split('|');
            if (fields.Length != 7)
            {
                return BadRequest("Invalid QR code data format.");
            }

            var cccdInfo = new CccdInfo
            {
                CCCDNumber = fields[0],
                OldIDNumber = fields[1],
                FullName = fields[2],
                DateOfBirth = fields[3],
                Gender = fields[4],
                PermanentAddress = fields[5],
                IssueDate = fields[6]
            };
            return Ok(cccdInfo);
        }
        public class QRCodeData
        {
            public string QrData { get; set; }
        }
        public class CccdInfo
        {
            public string CCCDNumber { get; set; }
            public string OldIDNumber { get; set; }
            public string FullName { get; set; }
            public string DateOfBirth { get; set; }
            public string Gender { get; set; }
            public string PermanentAddress { get; set; }
            public string IssueDate { get; set; }
        }
    }
}
