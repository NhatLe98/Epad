using Chilkat;
using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TA_AnnualLeaveController : ApiControllerBase
    {
        private IMemoryCache cache;
        private readonly ITA_AnnualLeaveService _TA_AnnualLeaveService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        private readonly IHR_UserService _HR_UserService;
        public TA_AnnualLeaveController(IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _TA_AnnualLeaveService = TryResolve<ITA_AnnualLeaveService>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _HR_UserService = TryResolve<IHR_UserService>();
        }

        [Authorize]
        [ActionName("GetAnnualLeaveAtPage")]
        [HttpPost]
        public IActionResult GetAnnualLeaveAtPage([FromBody] TA_AnnualLeaveParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = _TA_AnnualLeaveService.GetDataGrid(user.CompanyIndex, param.page, param.limit, param.filter, param.departments, param.employeeatids);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("AddAnnualLeave")]
        [HttpPost]
        public async Task<IActionResult> AddAnnualLeave([FromBody] TA_AnnualLeaveInsertParam data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var employeeExist = "";
            var userInfoList = _HR_UserService.Where(x => data.EmployeeATIDs.Contains(x.EmployeeATID)).ToList();
            var employeeATIDs = data.EmployeeATIDs;

            var nameExist = await _TA_AnnualLeaveService.GetAnnualLeaveEmployeeATIDs(data.EmployeeATIDs, user.CompanyIndex);

            if (nameExist != null && nameExist.Count > 0)
            {
                foreach (var employeeATID in nameExist)
                {
                    employeeExist += "<p>  - " + employeeATID.EmployeeATID + " " + userInfoList.FirstOrDefault(x => x.EmployeeATID == employeeATID.EmployeeATID)?.FullName + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";

                }
                return ApiOk(employeeExist);
            }

            var isSuccess = await _TA_AnnualLeaveService.AddAnnualLeave(data, user);

            return Ok();
        }

        [Authorize]
        [ActionName("UpdateAnnualLeave")]
        [HttpPost]
        public async Task<IActionResult> UpdateAnnualLeave([FromBody] TA_AnnualLeaveInsertParam data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var dataExist = await _TA_AnnualLeaveService.GetAnnualLeaveByIndex(data.Index, user.CompanyIndex);
            if (dataExist == null)
            {
                return ApiError("AnnualLeaveNotExist");
            }

            var isSuccess = await _TA_AnnualLeaveService.UpdateAnnualLeave(data, user);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("DeleteAnnualLeave")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAnnualLeave(List<int> index)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var isSuccess = await _TA_AnnualLeaveService.DeleteAnnualLeave(index);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("AddAnnualLeaveFromExcel")]
        [HttpPost]
        public IActionResult AddAnnualLeaveFromExcel([FromBody] List<TA_AnnualLeaveImportParam> data)
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
                var listError = new List<TA_AnnualLeaveImportParam>();

                listError = _TA_AnnualLeaveService.ValidationImportAnnualLeave(data, user);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/AnnualLeaveError.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/AnnualLeaveError.xlsx"));

                if (listError != null && listError.Count() > 0)
                {
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook(file.FullName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "MCC (*)";
                        worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                        worksheet.Cell(currentRow, 3).Value = "Họ tên";
                        worksheet.Cell(currentRow, 4).Value = "Phép năm (*)";
                        worksheet.Cell(currentRow, 5).Value = "Lỗi";

                        for (int i = 1; i < 6; i++)
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

                            worksheet.Cell(currentRow, 3).Value = department.EmployeeName;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = "'" + department.AnnualLeave;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = department.ErrorMessage;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(currentRow, 5).Style.Alignment.SetWrapText(true);
                        }
                        worksheet.Columns().AdjustToContents();
                        worksheet.Rows().AdjustToContents();
                        workbook.SaveAs(file.FullName);
                    }
                }

                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "TA_AnnualLeave";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + lstImport.Count().ToString() + " Department";
                audit.Description = AuditType.Added.ToString() + "AddAnnualLeaveFromExcel:/:" + data.Count().ToString();
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
