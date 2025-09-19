using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.IC;
using EPAD_Logic;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using NPOI.POIFS.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/EmailDeclareGuest/[action]")]
    [ApiController]
    public class EmailDeclareGuestController : ApiControllerBase
    {
        private EPAD_Context context;
        private ezHR_Context otherContext;
        private IMemoryCache cache;
        private IIC_DepartmentLogic _iIC_DepartmentLogic;
        private readonly IIC_DepartmentService _IC_DepartmentService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        private readonly IConfiguration _configuration;
        private readonly string _configClientName;
        private readonly IHR_EmployeeLogic _IHR_EmployeeLogic;
        private readonly IHR_UserService _HR_UserService;
        private List<long> Ids { get; set; }

        public EmailDeclareGuestController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            otherContext = TryResolve<ezHR_Context>();
            _configuration = TryResolve<IConfiguration>();
            _iIC_DepartmentLogic = TryResolve<IIC_DepartmentLogic>();
            _IC_DepartmentService = TryResolve<IIC_DepartmentService>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
            _IHR_EmployeeLogic = TryResolve<IHR_EmployeeLogic>();
            _HR_UserService = TryResolve<IHR_UserService>();
        }

        [Authorize]
        [ActionName("GetEmailDeclareGuestAtPage")]
        [HttpGet]
        public async Task<IActionResult> GetEmailDeclareGuestAtPage(int page, string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var mailList = context.HR_EmailDeclareGuest.ToList();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var filterList = filter.Split(' ').ToList();
                if (filterList != null && filterList.Count > 0)
                {
                    mailList = mailList.Where(x => filterList.Contains(x.EmployeeATID) || filterList.Contains(x.EmailAddress)).ToList();
                }
            }

            if (page < 1) page = 1;
            var totalCount = mailList.Count();
            var listDataEmail = mailList.OrderBy(x => x.Index).Skip((page - 1) * limit).Take(limit).ToList();

            var employeeInfoList = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(listDataEmail.Select(x => x.EmployeeATID).ToList(), DateTime.Now, user.CompanyIndex);
            var listData = new List<EmailDeclareGuestParam>();
            foreach (var mail in listDataEmail)
            {
                var employeeInfo = employeeInfoList.Where(x => x.EmployeeATID == mail.EmployeeATID).FirstOrDefault();
                var data = new EmailDeclareGuestParam();
                data.Index = mail.Index;
                data.EmployeeATID = mail.EmployeeATID;
                data.FullName = employeeInfo?.FullName;
                data.EmailAddressList = mail.EmailAddress.Split(",").ToList();
                data.EmailAddress = mail.EmailAddress;
                data.DepartmentName = employeeInfo?.Department;
                data.Description = mail.Description;
                data.TimeUpdate = mail.UpdatedDate != null ? mail.UpdatedDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : "";
                listData.Add(data);
            }

            var gridClass = new DataGridClass(totalCount, listData);
            result = Ok(gridClass);
            return result;
        }


        [Authorize]
        [ActionName("AddEmailDeclareGuest")]
        [HttpPost]
        public IActionResult AddEmailDeclareGuest([FromBody] EmailDeclareGuestParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var emailDeclareGuest = context.HR_EmailDeclareGuest.Any(x => x.EmployeeATID == param.EmployeeATID);
            if (emailDeclareGuest)
            {
                return ApiConflict("EmailDeclareGuestOverlap");
            }

            //var emailList = param.EmailAddressList.Replace(" ", "").Split(",").ToList();

            var listEmailAllow = new List<string>();
            var listEmailDeclareGuest = context.HR_EmailDeclareGuest.Where(x => !string.IsNullOrEmpty(x.EmailAddress)).Select(x => x.EmailAddress).ToList();
            foreach (var email in listEmailDeclareGuest)
            {
                var emailAllowList = email.Replace(" ", "").Split(',').ToList();
                listEmailAllow.AddRange(emailAllowList);
            }

            var checkExistEmail = listEmailAllow.Any(x => param.EmailAddressList.Contains(x));
            if (checkExistEmail)
            {
                return ApiConflict("EmailDeclareGuestOverlap");
            }

            var saveData = new HR_EmailDeclareGuest();
            saveData.EmployeeATID = param.EmployeeATID;
            saveData.EmailAddress = string.Join(",", param.EmailAddressList);
            saveData.Description = param.Description;
            saveData.CompanyIndex = user.CompanyIndex;
            saveData.UpdatedDate = DateTime.Now;
            saveData.UpdatedUser = user.UserName;
            saveData.Description = param.Description;

            context.HR_EmailDeclareGuest.Add(saveData);
            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("UpdateEmailDeclareGuest")]
        [HttpPost]
        public async Task<IActionResult> UpdateEmailDeclareGuest(EmailDeclareGuestParam param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var emailDeclareGuest = context.HR_EmailDeclareGuest.Any(x => x.Index != param.Index && (x.EmployeeATID == param.EmployeeATID));
            if (emailDeclareGuest)
            {
                return ApiConflict("EmailDeclareGuestOverlap");
            }

            var listEmailAllow = new List<string>();
            var listEmailDeclareGuest = context.HR_EmailDeclareGuest.Where(x => x.Index != param.Index && !string.IsNullOrEmpty(x.EmailAddress)).Select(x => x.EmailAddress).ToList();
            foreach (var email in listEmailDeclareGuest)
            {
                var emailAllowList = email.Replace(" ", "").Split(',').ToList();
                listEmailAllow.AddRange(emailAllowList);
            }

            var checkExistEmail = listEmailAllow.Any(x => param.EmailAddressList.Contains(x));
            if (checkExistEmail)
            {
                return ApiConflict("EmailDeclareGuestOverlap");
            }


            var emailUpdate = context.HR_EmailDeclareGuest.FirstOrDefault(x => x.Index == param.Index);
            if (emailUpdate == null)
            {
                return ApiConflict("EmailDeclareGuestNotExist");
            }

            emailUpdate.UpdatedDate = DateTime.Now;
            emailUpdate.UpdatedUser = user.UserName;
            emailUpdate.Description = param.Description;
            emailUpdate.CompanyIndex = user.CompanyIndex;
            emailUpdate.EmailAddress = string.Join(",", param.EmailAddressList);
            context.HR_EmailDeclareGuest.Update(emailUpdate);
            context.SaveChanges();
            return ApiOk();
        }

        [Authorize]
        [ActionName("DeleteEmailDeclareGuest")]
        [HttpPost]
        public async Task<IActionResult> DeleteEmailDeclareGuest([FromBody] List<long> listIndex)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var emailList = await context.HR_EmailDeclareGuest.Where(x => listIndex.Contains(x.Index)).ToListAsync();
            if (emailList != null && emailList.Count > 0)
            {
                context.HR_EmailDeclareGuest.RemoveRange(emailList);
                context.SaveChanges();
            }
            return ApiOk();
        }

        [Authorize]
        [ActionName("AddEmailDeclareGuestFromExcel")]
        [HttpPost]
        public async Task<IActionResult> AddEmailDeclareGuestFromExcel(List<EmailDeclareGuestParam> param)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var message = "";

            //var importResult = await _GC_BlackListService.ImportBlackList(param, user);
            var errorImportList = new List<EmailDeclareGuestParam>();
            var result = param;
            var employeeATIDs = result.Select(x => x.EmployeeATID).ToHashSet();
            var employees = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeATIDs.ToList(), DateTime.Now, user.CompanyIndex);

            string[] formats = { "dd/MM/yyyy" };

            long index = 0;
            foreach (var itemImport in result)
            {
                ++index;
                itemImport.RowIndex = index;

                //-----------
                if (string.IsNullOrWhiteSpace(itemImport.EmployeeATID))
                {
                    itemImport.ErrorMessage += "Bắt buộc phải nhập mã người dùng\r\n";
                }
                else
                {
                    var employee = employees.FirstOrDefault(y => y.EmployeeATID == itemImport.EmployeeATID);
                    if (employee != null)
                    {
                        itemImport.FullName = employee.FullName;
                    }
                    else
                    {
                        itemImport.ErrorMessage += "Nhân viên không tồn tại\r\n";
                    }
                }


                if (string.IsNullOrWhiteSpace(itemImport.EmailAddress))
                {
                    itemImport.ErrorMessage += "Bắt buộc phải nhập địa chỉ email\r\n";
                }
                else
                {
                    bool isEmail = true;
                    var emailList = itemImport.EmailAddress.Replace(" ", "").Split(",").ToList();
                    foreach (var email in emailList)
                    {
                        isEmail = StringHelper.IsValidEmail(email);
                        if (!isEmail) { break; };
                    }
                    if (!isEmail)
                    {
                        itemImport.ErrorMessage += "Email không không đúng định dạng\r\n";
                    }
                }
            }

            var listEmpIDs = result.Select(x => x.EmployeeATID).ToHashSet();
            var existedEmailDeclareGuest = context.HR_EmailDeclareGuest.ToList();
            var noErrorParam = result.Where(x => string.IsNullOrWhiteSpace(x.ErrorMessage)).ToList();

            if (noErrorParam != null && noErrorParam.Count > 0)
            {
                var logExistInFile = new List<EmailDeclareGuestParam>();
                foreach (var itemMail in noErrorParam)
                {
                    var emailList = itemMail.EmailAddress.Replace(" ", "").Split(",").ToList();
                    var listEmailAllow = new List<string>();
                    var listEmailDeclareGuest = existedEmailDeclareGuest.Where(x => !string.IsNullOrEmpty(x.EmailAddress)).Select(x => x.EmailAddress).ToList();
                    foreach (var email in listEmailDeclareGuest)
                    {
                        var emailAllowList = email.Replace(" ", "").Split(',').ToList();
                        listEmailAllow.AddRange(emailAllowList);
                    }
                    var checkExistEmail = listEmailAllow.Any(x => emailList.Contains(x));
                    if (checkExistEmail)
                    {
                        itemMail.ErrorMessage += "Email đã được khai báo\r\n";
                        continue;
                    }

                    if (logExistInFile != null && logExistInFile.Count > 0)
                    {
                        var listEmailAllowInFile = new List<string>();
                        var listEmailDeclareGuestInFile = logExistInFile.Where(x => !string.IsNullOrEmpty(x.EmailAddress)).Select(x => x.EmailAddress).ToList();

                        foreach (var emailInFile in listEmailDeclareGuestInFile)
                        {
                            var emailAllowListInFile = emailInFile.Replace(" ", "").Split(',').ToList();
                            listEmailAllowInFile.AddRange(emailAllowListInFile);
                        }
                        var checkExistEmailInFile = listEmailAllowInFile.Any(x => emailList.Contains(x));
                        if (checkExistEmailInFile)
                        {
                            itemMail.ErrorMessage += "Email đã được khai báo trong tập tin\r\n";
                            continue;
                        }
                        if (logExistInFile.Any(y => y.RowIndex != itemMail.RowIndex && y.EmployeeATID == itemMail.EmployeeATID))
                        {
                            itemMail.ErrorMessage += "Dữ liệu khai báo bị trùng trong tập tin\r\n";
                            continue;
                        }
                    }

                    var existedEmailDeclareGuestList = existedEmailDeclareGuest.Where(x => x.EmployeeATID == itemMail.EmployeeATID).ToList();
                    if (existedEmailDeclareGuestList != null && existedEmailDeclareGuestList.Count > 0)
                    {
                        var checkExistData = existedEmailDeclareGuestList.FirstOrDefault(x => x.EmployeeATID == itemMail.EmployeeATID);
                        if (checkExistData != null)
                        {
                            checkExistData.EmailAddress = itemMail.EmailAddress.Replace(" ", "");
                            checkExistData.Description = itemMail.Description;
                            checkExistData.UpdatedDate = DateTime.Now;
                            checkExistData.UpdatedUser = user.UserName;
                            context.HR_EmailDeclareGuest.Update(checkExistData);
                        }
                    }
                    else
                    {
                        var saveData = new HR_EmailDeclareGuest();
                        saveData.EmployeeATID = itemMail.EmployeeATID;
                        saveData.EmailAddress = string.Join(",", itemMail.EmailAddress.Replace(" ", ""));
                        saveData.Description = itemMail.Description;
                        saveData.CompanyIndex = user.CompanyIndex;
                        saveData.UpdatedDate = DateTime.Now;
                        saveData.UpdatedUser = user.UserName;

                        context.HR_EmailDeclareGuest.Add(saveData);
                    }
                    logExistInFile.Add(itemMail);
                }
                await context.SaveChangesAsync();
            }


            //Export file error
            errorImportList = result.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            if (errorImportList != null && errorImportList.Any(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)))
            {
                message = errorImportList.Count().ToString();
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_EmailDeclareGuest_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_EmailDeclareGuest_Error.xlsx"));

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("DataError");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                    worksheet.Cell(currentRow, 2).Value = "Họ và tên";
                    worksheet.Cell(currentRow, 3).Value = "Phòng ban";
                    worksheet.Cell(currentRow, 4).Value = "Email (*)";
                    worksheet.Cell(currentRow, 5).Value = "Ghi chú";
                    worksheet.Cell(currentRow, 6).Value = "Lỗi";

                    for (int i = 1; i < 7; i++)
                    {
                        worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                        worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Column(i).Width = 20;
                    }

                    var errorResult = errorImportList.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).ToList();

                    foreach (var importRow in errorResult)
                    {
                        currentRow++;
                        //New template
                        worksheet.Cell(currentRow, 1).Value = "'" + importRow.EmployeeATID;
                        worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 2).Value = "'" + importRow.FullName;
                        worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 3).Value = importRow.DepartmentName;
                        worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 4).Value = importRow.EmailAddress;
                        worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 5).Value = importRow.Description;
                        worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 6).Value = importRow.ErrorMessage;
                        worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    workbook.SaveAs(file.FullName);
                }
            }

            return Ok(message);
        }
    }

    public class EmailDeclareGuestParam
    {
        public int Index { get; set; }
        public string EmployeeATID { get; set; }
        public string DepartmentName { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public List<string> EmailAddressList { get; set; }
        public string Description { get; set; }
        public string TimeUpdate { get; set; }
        public string ErrorMessage { get; set; }
        public long RowIndex { get; set; }
    }
}