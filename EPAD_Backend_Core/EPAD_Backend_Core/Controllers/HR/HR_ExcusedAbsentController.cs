using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using EPAD_Data.Models.IC;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static EPAD_Backend_Core.Controllers.IC_ServiceController;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/HR_ExcusedAbsent/[action]")]
    [ApiController]
    public class HR_ExcusedAbsentController : ApiControllerBase
    {
        private EPAD_Context context;
        private ezHR_Context otherContext;
        private IMemoryCache cache;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHR_ExcusedAbsentService _iHR_ExcusedAbsentService;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        private List<long> Ids { get; set; }

        public HR_ExcusedAbsentController(IServiceProvider provider, IHostingEnvironment hostingEnvironment) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            otherContext = TryResolve<ezHR_Context>();
            _iHR_ExcusedAbsentService = TryResolve<IHR_ExcusedAbsentService>();
            _hostingEnvironment = hostingEnvironment;
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
        }

        [Authorize]
        [ActionName("GetExcusedAbsentAtPage")]
        [HttpPost]
        public async Task<IActionResult> GetExcusedAbsentAtPage(ExcusedAbsentRequest requestParam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var dataResult = await _iHR_ExcusedAbsentService.GetExcusedAbsentAtPage(requestParam, user);
            
            result = Ok(dataResult);
            return result;
        }

        [Authorize]
        [ActionName("GetExcusedAbsentReason")]
        [HttpGet]
        public async Task<IActionResult> GetExcusedAbsentReason()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var dataResult = await _iHR_ExcusedAbsentService.GetExcusedAbsentReason(user);

            result = Ok(dataResult);
            return result;
        }

        [Authorize]
        [ActionName("AddExcusedAbsent")]
        [HttpPost]
        public async Task<IActionResult> AddExcusedAbsent(ExcusedAbsentParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var format = "yyyy-MM-dd";
            if (DateTime.TryParseExact(param.AbsentDateString, format, null,
                        System.Globalization.DateTimeStyles.None, out DateTime fromDate))
            {
                param.AbsentDate = fromDate;
            }
            else
            {
                return Conflict("DateWrongFormat");
            }

            var listExcusedAbsentByEmployeeATIDs = await _iHR_ExcusedAbsentService.GetByEmployeeATIDs(param.EmployeeATIDs);
            if (listExcusedAbsentByEmployeeATIDs != null && listExcusedAbsentByEmployeeATIDs.Any(x
                => x.AbsentDate.Date == param.AbsentDate.Date && param.EmployeeATIDs.Contains(x.EmployeeATID)))
            { 
                return Ok(new Tuple<string, object>("ExcusedAbsentIsExist", listExcusedAbsentByEmployeeATIDs.Where(x
                => x.AbsentDate.Date == param.AbsentDate.Date && param.EmployeeATIDs.Contains(x.EmployeeATID))));
            }

            var isSuccess = await _iHR_ExcusedAbsentService.AddExcusedAbsent(param, user);

            return Ok(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateExcusedAbsent")]
        [HttpPost]
        public async Task<IActionResult> UpdateExcusedAbsent(ExcusedAbsentParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var existedExcusedAbsent = await _iHR_ExcusedAbsentService.GetByIndex(param.Index);
            if (existedExcusedAbsent == null)
            {
                return Conflict("ExcusedAbsentNotExist");
            }

            var isSuccess = await _iHR_ExcusedAbsentService.UpdateExcusedAbsent(param, user);

            result = Ok(isSuccess);
            return result;
        }

        [Authorize]
        [ActionName("DeleteExcusedAbsent")]
        [HttpPost]
        public async Task<IActionResult> DeleteExcusedAbsent(List<int> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var isSuccess = await _iHR_ExcusedAbsentService.DeleteExcusedAbsent(lsparam);

            result = Ok(isSuccess);
            return result;
        }

        [Authorize]
        [ActionName("ExportTemplateExcusedAbsentReason")]
        [HttpGet]
        public IActionResult ExportTemplateExcusedAbsentReason()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            string sWebRootFolder = _hostingEnvironment.ContentRootPath;

            var folderDetails = Path.Combine(sWebRootFolder, @"epad/dist/Template_NoAttendance.xlsx");
            //Use local 
#if DEBUG
            folderDetails = Path.Combine(sWebRootFolder, @"epad/public/Template_NoAttendance.xlsx");
#endif

#if !DEBUG
            _iHR_ExcusedAbsentService.ExportTemplateExcusedAbsent(folderDetails);
#endif

            return ApiOk();
        }

        [Authorize]
        [ActionName("AddExcusedAbsentFromExcel")]
        [HttpPost]
        public async Task<IActionResult> AddExcusedAbsentFromExcel(List<ExcusedAbsentImportParam> lstImport)
        {
            try
            {
                UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
                ConfigObject config = ConfigObject.GetConfig(cache);
                IActionResult result = Unauthorized();
                if (user == null)
                {
                    return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
                }
                // validation data
                List<ExcusedAbsentImportParam> listError = new List<ExcusedAbsentImportParam>();

                listError = await _iHR_ExcusedAbsentService.ValidationImportExcusedAbsent(lstImport, user);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_NoAttendance_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_NoAttendance_Error.xlsx"));

                if (listError != null && listError.Count() > 0)
                {
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("ExcusedAbsentError");
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Mã ID (*)";
                        worksheet.Cell(currentRow, 2).Value = "Họ tên";
                        worksheet.Cell(currentRow, 3).Value = "Lớp";
                        worksheet.Cell(currentRow, 4).Value = "Ngày (*)";
                        worksheet.Cell(currentRow, 5).Value = "Lý do (*)";
                        worksheet.Cell(currentRow, 6).Value = "Ghi chú";
                        worksheet.Cell(currentRow, 7).Value = "Lỗi";

                        for (int i = 1; i < 8; i++)
                        {
                            worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Column(i).Width = 20;
                        }

                        foreach (var department in listError)
                        {
                            currentRow++;
                            //New template
                            worksheet.Cell(currentRow, 1).Value = "'" + department.EmployeeATID;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 2).Value = department.FullName;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 3).Value = department.Class;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = department.AbsentDate;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = department.ExcusedAbsentReason;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = department.Description;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = department.ErrorMessage;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        }
                        workbook.SaveAs(file.FullName);
                    }
                }

                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "HR_ExcusedAbsent";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + lstImport.Count().ToString() + " Department";
                audit.Description = AuditType.Added.ToString() + "ExcusedAbsentFromExcel:/:" + lstImport.Count().ToString();
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
