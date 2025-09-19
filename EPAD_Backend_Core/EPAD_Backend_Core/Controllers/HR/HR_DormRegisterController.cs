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
    [Route("api/HR_DormRegister/[action]")]
    [ApiController]
    public class HR_DormRegisterController : ApiControllerBase
    {
        private EPAD_Context context;
        private ezHR_Context otherContext;
        private IMemoryCache cache;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHR_DormRoomService _IHR_DormRoomService;
        private readonly IHR_DormRegisterService _IHR_DormRegisterService;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        private List<long> Ids { get; set; }

        public HR_DormRegisterController(IServiceProvider provider, IHostingEnvironment hostingEnvironment) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            otherContext = TryResolve<ezHR_Context>();
            _IHR_DormRoomService = TryResolve<IHR_DormRoomService>();
            _IHR_DormRegisterService = TryResolve<IHR_DormRegisterService>();
            _hostingEnvironment = hostingEnvironment;
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
        }

        [Authorize]
        [ActionName("GetDormActivity")]
        [HttpGet]
        public async Task<IActionResult> GetDormActivity()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var dataResult = await _IHR_DormRegisterService.GetDormActivity(user);

            result = Ok(dataResult);
            return result;
        }

        [Authorize]
        [ActionName("GetDormRation")]
        [HttpGet]
        public async Task<IActionResult> GetDormRation()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var dataResult = await _IHR_DormRegisterService.GetDormRation(user);

            result = Ok(dataResult);
            return result;
        }

        [Authorize]
        [ActionName("GetDormLeaveType")]
        [HttpGet]
        public async Task<IActionResult> GetDormLeaveType()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var dataResult = await _IHR_DormRegisterService.GetDormLeaveType(user);

            result = Ok(dataResult);
            return result;
        }

        [Authorize]
        [ActionName("GetDormAccessMode")]
        [HttpGet]
        public IActionResult GetDormAccessMode()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var dataResult = _IHR_DormRegisterService.GetDormAccessMode();

            result = Ok(dataResult);
            return result;
        }

        [Authorize]
        [ActionName("GetDormRegisterAtPage")]
        [HttpPost]
        public async Task<IActionResult> GetDormRegisterAtPage(DormRegisterRequest requestParam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var dataResult = await _IHR_DormRegisterService.GetDormRegister(user, requestParam);

            result = Ok(dataResult);
            return result;
        }

        [Authorize]
        [ActionName("AddDormRegister")]
        [HttpPost]
        public async Task<IActionResult> AddDormRegister(DormRegisterViewModel requestParam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var format = "dd-MM-yyyy";
            if (!string.IsNullOrWhiteSpace(requestParam.FromDateString) && DateTime.TryParseExact(requestParam.FromDateString, format, null,
                    System.Globalization.DateTimeStyles.None, out DateTime fromDate))
            {
                requestParam.FromDate = fromDate;
            }
            else
            {
                return Conflict("FromDateWrongFormat");
            }

            if (!string.IsNullOrWhiteSpace(requestParam.ToDateString) && DateTime.TryParseExact(requestParam.ToDateString, format, null,
                    System.Globalization.DateTimeStyles.None, out DateTime toDate))
            {
                requestParam.ToDate = toDate;
            }
            else
            {
                return Conflict("ToDateWrongFormat");
            }

            if (!AreAllWeekendDates(requestParam.FromDate, requestParam.ToDate))
            { 
                return Conflict("PleaseSelectBetweenSaturdayAndSunday");
            }

            var listDate = ListDate(requestParam.FromDate, requestParam.ToDate);
            var existedByEmployeeATID = await _IHR_DormRegisterService.GetByEmployeATID(user, requestParam.EmployeeATID);
            if (existedByEmployeeATID != null && existedByEmployeeATID.Any(x => listDate.Contains(x.RegisterDate.Date)))
            {
                return Ok(new Tuple<string, object>("DormRegisterIsExist", 
                    existedByEmployeeATID.Where(x => listDate.Contains(x.RegisterDate.Date)).ToList()));
            }

            if (requestParam.StayInDorm)
            {
                var existedDormEmployeeCode = await _IHR_DormRegisterService.GetByDormEmployeeCode(user, requestParam.DormEmployeeCode);
                if (existedDormEmployeeCode != null && existedDormEmployeeCode.Any(y => y.EmployeeATID != requestParam.EmployeeATID))
                {
                    return Conflict("DormEmployeCodeIsExisted");
                }
            }

            var isSucess = await _IHR_DormRegisterService.AddDormRegister(user, requestParam);

            result = Ok(isSucess);
            return result;
        }

        [Authorize]
        [ActionName("UpdateDormRegister")]
        [HttpPost]
        public async Task<IActionResult> UpdateDormRegister(DormRegisterViewModel requestParam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var format = "dd-MM-yyyy";
            if (!string.IsNullOrWhiteSpace(requestParam.FromDateString) && DateTime.TryParseExact(requestParam.FromDateString, format, null,
                    System.Globalization.DateTimeStyles.None, out DateTime fromDate))
            {
                requestParam.FromDate = fromDate;
            }
            else
            {
                return Conflict("FromDateWrongFormat");
            }

            if (!string.IsNullOrWhiteSpace(requestParam.ToDateString) && DateTime.TryParseExact(requestParam.ToDateString, format, null,
                    System.Globalization.DateTimeStyles.None, out DateTime toDate))
            {
                requestParam.ToDate = toDate;
            }
            else
            {
                return Conflict("ToDateWrongFormat");
            }

            //if (!AreAllWeekendDates(requestParam.FromDate, requestParam.ToDate))
            //{
            //    return Conflict("PleaseSelectBetweenSaturdayAndSunday");
            //}

            //var listDate = ListDate(requestParam.FromDate, requestParam.ToDate);
            //var existedByEmployeeATID = await _IHR_DormRegisterService.GetByEmployeATID(user, requestParam.EmployeeATID);
            //if (existedByEmployeeATID != null && existedByEmployeeATID.Any(x => listDate.Contains(x.RegisterDate.Date)))
            //{
            //    return Ok(new Tuple<string, object>("DormRegisterIsExist",
            //        existedByEmployeeATID.Where(x => listDate.Contains(x.RegisterDate.Date)).ToList()));
            //}

            var existedDormRegister = _IHR_DormRegisterService.GetByIndex(requestParam.Index);
            if (existedDormRegister == null)
            {
                return Conflict("DormRegisterNotExist");
            }

            if (requestParam.StayInDorm)
            {
                var existedDormEmployeeCode = await _IHR_DormRegisterService.GetByDormEmployeeCode(user, requestParam.DormEmployeeCode);
                if (existedDormEmployeeCode != null && existedDormEmployeeCode.Any(y => y.EmployeeATID != requestParam.EmployeeATID))
                {
                    return Conflict("DormEmployeCodeIsExisted");
                }
            }

            var isSucess = await _IHR_DormRegisterService.UpdateDormRegister(user, requestParam);

            result = Ok(isSucess);
            return result;
        }

        [Authorize]
        [ActionName("DeleteDormRegister")]
        [HttpPost]
        public async Task<IActionResult> DeleteDormRegister(List<int> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var isSuccess = await _IHR_DormRegisterService.DeleteDormRegister(lsparam);

            result = Ok(isSuccess);
            return result;
        }

        [Authorize]
        [ActionName("AddDormRegisterFromExcel")]
        [HttpPost]
        public async Task<IActionResult> AddDormRegisterFromExcel(List<DormRegisterViewModel> lstImport)
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
                var listError = new List<DormRegisterViewModel>();

                listError = await _IHR_DormRegisterService.ValidationImportDormRegsiter(lstImport, user);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_DormRegister_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_DormRegister_Error.xlsx"));

                if (listError != null && listError.Count() > 0)
                {
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook(file.FullName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var currentRow = 3;
                        //worksheet.Cell(currentRow, 1).Value = "Mã ID (*)";
                        //worksheet.Cell(currentRow, 2).Value = "Họ tên";
                        //worksheet.Cell(currentRow, 3).Value = "Lớp";
                        //worksheet.Cell(currentRow, 4).Value = "Ngày (*)";
                        //worksheet.Cell(currentRow, 5).Value = "Lý do (*)";
                        //worksheet.Cell(currentRow, 6).Value = "Ghi chú";
                        //worksheet.Cell(currentRow, 7).Value = "Lỗi";

                        //for (int i = 1; i < 8; i++)
                        //{
                        //    worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                        //    worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        //    worksheet.Column(i).Width = 20;
                        //}

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

                            worksheet.Cell(currentRow, 2).Value = "'" + department.FullName;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 3).Value = "'" + department.DepartmentName;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = "'" + department.FromDateString;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = "'" + department.ToDateString;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = department.StayInDorm ? "X" : string.Empty;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = "'" + department.DormRoomName;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = "'" + department.DormEmployeeCode;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            if (department.DormRegisterRation != null && department.DormRegisterRation.Count > 0
                                && department.DormRegisterRation.Any(x => x.DormRationName == "Sáng"))
                            {
                                worksheet.Cell(currentRow, 9).Value = "X";
                                worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }

                            if (department.DormRegisterRation != null && department.DormRegisterRation.Count > 0
                                && department.DormRegisterRation.Any(x => x.DormRationName == "Trưa"))
                            {
                                worksheet.Cell(currentRow, 10).Value = "X";
                                worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }

                            if (department.DormRegisterRation != null && department.DormRegisterRation.Count > 0
                                && department.DormRegisterRation.Any(x => x.DormRationName == "Chiều"))
                            {
                                worksheet.Cell(currentRow, 11).Value = "X";
                                worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }

                            if (department.DormRegisterActivity != null && department.DormRegisterActivity.Count > 0
                                && department.DormRegisterActivity.Any(x => x.DormActivityName == "Siêu thị" 
                                && x.DormAccessMode == (short)DormAccessMode.In))
                            {
                                worksheet.Cell(currentRow, 12).Value = "X";
                                worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }

                            if (department.DormRegisterActivity != null && department.DormRegisterActivity.Count > 0
                                && department.DormRegisterActivity.Any(x => x.DormActivityName == "Siêu thị"
                                && x.DormAccessMode == (short)DormAccessMode.Out))
                            {
                                worksheet.Cell(currentRow, 13).Value = "X";
                                worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }

                            if (department.DormRegisterActivity != null && department.DormRegisterActivity.Count > 0
                                && department.DormRegisterActivity.Any(x => x.DormActivityName == "Đi lễ"
                                && x.DormAccessMode == (short)DormAccessMode.In))
                            {
                                worksheet.Cell(currentRow, 14).Value = "X";
                                worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }

                            if (department.DormRegisterActivity != null && department.DormRegisterActivity.Count > 0
                                && department.DormRegisterActivity.Any(x => x.DormActivityName == "Đi lễ"
                                && x.DormAccessMode == (short)DormAccessMode.Out))
                            {
                                worksheet.Cell(currentRow, 15).Value = "X";
                                worksheet.Cell(currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }

                            if (department.DormRegisterActivity != null && department.DormRegisterActivity.Count > 0
                                && department.DormRegisterActivity.Any(x => x.DormActivityName == "Bóng đá"
                                && x.DormAccessMode == (short)DormAccessMode.In))
                            {
                                worksheet.Cell(currentRow, 16).Value = "X";
                                worksheet.Cell(currentRow, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }

                            if (department.DormRegisterActivity != null && department.DormRegisterActivity.Count > 0
                                && department.DormRegisterActivity.Any(x => x.DormActivityName == "Bóng đá"
                                && x.DormAccessMode == (short)DormAccessMode.Out))
                            {
                                worksheet.Cell(currentRow, 17).Value = "X";
                                worksheet.Cell(currentRow, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }

                            worksheet.Cell(currentRow, 18).Value = department.DormLeaveName;
                            worksheet.Cell(currentRow, 18).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 19).Value = department.ErrorMessage;
                            worksheet.Cell(currentRow, 19).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
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

        private List<DateTime> ListDate(DateTime fromDate, DateTime toDate) 
        {
            var currentDate = fromDate;
            List<DateTime> weekendDates = new List<DateTime>();

            while (currentDate <= toDate)
            {
                weekendDates.Add(currentDate.Date);
                currentDate = currentDate.AddDays(1);                
            }

            return weekendDates;
        }

        private bool AreAllWeekendDates(DateTime fromDate, DateTime toDate)
        {
            var currentDate = fromDate;
            List<DateTime> weekendDates = new List<DateTime>();

            while (currentDate <= toDate)
            {
                weekendDates.Add(currentDate.Date);
                currentDate = currentDate.AddDays(1);
            }

            bool allWeekend = weekendDates.TrueForAll(IsWeekend);

            return allWeekend;
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}
