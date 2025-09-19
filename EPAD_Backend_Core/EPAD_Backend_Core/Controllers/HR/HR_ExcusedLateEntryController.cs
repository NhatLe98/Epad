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
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HR_ExcusedLateEntryController : ApiControllerBase
    {
        private EPAD_Context context;
        private ezHR_Context otherContext;
        private IMemoryCache cache;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHR_ExcusedLateEntryService _IHR_ExcusedLateEntryService;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        private List<long> Ids { get; set; }

        public HR_ExcusedLateEntryController(IServiceProvider provider, IHostingEnvironment hostingEnvironment) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            otherContext = TryResolve<ezHR_Context>();
            _IHR_ExcusedLateEntryService = TryResolve<IHR_ExcusedLateEntryService>();
            _hostingEnvironment = hostingEnvironment;
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
        }

        [Authorize]
        [ActionName("GetExcusedLateEntryAtPage")]
        [HttpPost]
        public async Task<IActionResult> GetExcusedLateEntryAtPage(ExcusedLateEntryRequest requestParam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var dataResult = await _IHR_ExcusedLateEntryService.GetExcusedLateEntryAtPage(requestParam, user);
            
            result = Ok(dataResult);
            return result;
        }

        [Authorize]
        [ActionName("AddExcusedLateEntry")]
        [HttpPost]
        public async Task<IActionResult> AddExcusedLateEntry(ExcusedLateEntryParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var format = "yyyy-MM-dd";
            if (DateTime.TryParseExact(param.LateDateString, format, null,
                        System.Globalization.DateTimeStyles.None, out DateTime fromDate))
            {
                param.LateDate = fromDate;
            }
            else 
            {
                return Conflict("DateWrongFormat");
            }

            var listExcusedLateEntryByEmployeeATIDs = await _IHR_ExcusedLateEntryService.GetByEmployeeATIDs(param.EmployeeATIDs);
            if (listExcusedLateEntryByEmployeeATIDs != null && listExcusedLateEntryByEmployeeATIDs.Any(x
                => x.LateDate.Date == param.LateDate.Date && param.EmployeeATIDs.Contains(x.EmployeeATID)))
            { 
                return Ok(new Tuple<string, object>("ExcusedLateEntryIsExist", listExcusedLateEntryByEmployeeATIDs.Where(x
                => x.LateDate.Date == param.LateDate.Date && param.EmployeeATIDs.Contains(x.EmployeeATID))));
            }

            var isSuccess = await _IHR_ExcusedLateEntryService.AddExcusedLateEntry(param, user);

            return Ok(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateExcusedLateEntry")]
        [HttpPost]
        public async Task<IActionResult> UpdateExcusedLateEntry(ExcusedLateEntryParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var existedExcusedLateEntry = await _IHR_ExcusedLateEntryService.GetByIndex(param.Index);
            if (existedExcusedLateEntry == null)
            {
                return Conflict("ExcusedLateEntryNotExist");
            }

            var isSuccess = await _IHR_ExcusedLateEntryService.UpdateExcusedLateEntry(param, user);

            result = Ok(isSuccess);
            return result;
        }

        [Authorize]
        [ActionName("DeleteExcusedLateEntry")]
        [HttpPost]
        public async Task<IActionResult> DeleteExcusedLateEntry(List<int> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var isSuccess = await _IHR_ExcusedLateEntryService.DeleteExcusedLateEntry(lsparam);

            result = Ok(isSuccess);
            return result;
        }

        [Authorize]
        [ActionName("AddExcusedLateEntryFromExcel")]
        [HttpPost]
        public async Task<IActionResult> AddExcusedLateEntryFromExcel(List<ExcusedLateEntryImportParam> lstImport)
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
                List<ExcusedLateEntryImportParam> listError = new List<ExcusedLateEntryImportParam>();

                listError = await _IHR_ExcusedLateEntryService.ValidationImportExcusedLateEntry(lstImport, user);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_ExcusedLateEntry_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_ExcusedLateEntry_Error.xlsx"));

                if (listError != null && listError.Count() > 0)
                {
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("ExcusedLateEntryError");
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Mã ID (*)";
                        worksheet.Cell(currentRow, 2).Value = "Họ tên";
                        worksheet.Cell(currentRow, 3).Value = "Lớp";
                        worksheet.Cell(currentRow, 4).Value = "Ngày (*)";
                        worksheet.Cell(currentRow, 5).Value = "Ghi chú";
                        worksheet.Cell(currentRow, 6).Value = "Lỗi";

                        for (int i = 1; i < 7; i++)
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

                            worksheet.Cell(currentRow, 4).Value = department.LateDate;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = department.Description;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = department.ErrorMessage;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        }
                        workbook.SaveAs(file.FullName);
                    }
                }

                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "HR_ExcusedLateEntry";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + lstImport.Count().ToString() + " Department";
                audit.Description = AuditType.Added.ToString() + "ExcusedLateEntryFromExcel:/:" + lstImport.Count().ToString();
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
