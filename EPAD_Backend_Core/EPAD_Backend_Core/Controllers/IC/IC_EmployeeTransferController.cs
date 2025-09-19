using EPAD_Backend_Core.WebUtilitys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data.Entities;
using EPAD_Backend_Core.Base;
using EPAD_Common.Extensions;
using EPAD_Services.Interface;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/EmployeeTransfer/[action]")]
    [ApiController]
    public class IC_EmployeeTransferController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private ConfigObject config;
        private IIC_CommandLogic _iIC_CommandLogic;
        private IIC_DepartmentLogic _iIC_DepartmentLogic;
        private IIC_WorkingInfoLogic _iIC_WorkingInfoLogic;
        private IIC_EmployeeTransferLogic _iIC_EmployeeTransferLogic;
        private IIC_UserNotificationLogic _iIC_UserNotificationLogic;
        private IIC_EmployeeLogic _IIC_EmployeeLogic;
        private readonly IHostingEnvironment _hostingEnvironment;
        private IIC_AuditLogic _iIC_AuditLogic;
        private IHR_EmployeeInfoService _HR_EmployeeInfoService;
        private IIC_CommandService _iC_CommandService;
        public IC_EmployeeTransferController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            config = ConfigObject.GetConfig(cache);
            _iIC_CommandLogic = TryResolve<IIC_CommandLogic>();
            _iIC_DepartmentLogic = TryResolve<IIC_DepartmentLogic>();
            _iIC_WorkingInfoLogic = TryResolve<IIC_WorkingInfoLogic>();
            _iIC_EmployeeTransferLogic = TryResolve<IIC_EmployeeTransferLogic>();
            _iIC_UserNotificationLogic = TryResolve<IIC_UserNotificationLogic>();
            _IIC_EmployeeLogic = TryResolve<IIC_EmployeeLogic>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _HR_EmployeeInfoService = TryResolve<IHR_EmployeeInfoService>();
            _iC_CommandService = TryResolve<IIC_CommandService>();
        }

        [Authorize]
        [ActionName("GetEmployeeTransferAtPage")]
        [HttpGet]
        public IActionResult GetEmployeeTransferAtPage(int page, string fromDate, string toDate, string filter, bool isPenddingApprove, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            DataGridClass dataGrid = null;
            //DateTime fromTime = DateTime.ParseExact(fromDate, "yyyy-MM-dd", null);
            //DateTime toTime = DateTime.ParseExact(toDate, "yyyy-MM-dd", null);
            int countPage = 0;

            List<IC_EmployeeTransferDTO> listResult = new List<IC_EmployeeTransferDTO>();

            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
            addedParams.Add(new AddedParam { Key = "PageIndex", Value = page });
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            addedParams.Add(new AddedParam { Key = "FromDate", Value = fromDate });
            addedParams.Add(new AddedParam { Key = "ToDate", Value = toDate });
            addedParams.Add(new AddedParam { Key = "IsPenddingApprove", Value = isPenddingApprove });
            addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
            addedParams.Add(new AddedParam { Key = "PageSize", Value = limit });
            var lsEmployeeTransfer = _iIC_EmployeeTransferLogic.GetPage(addedParams);

            var lsWorkingInfo = _iIC_WorkingInfoLogic.GetPageEmpTransfer(addedParams);

            foreach (var workinfo in lsWorkingInfo.Data.ToList())
            {
                if (lsEmployeeTransfer.Data.Any(e => e.EmployeeATID == workinfo.EmployeeATID
                                                && e.NewDepartment == workinfo.NewDepartment
                                                && e.FromTime == workinfo.FromTime
                                                && e.ToTime == workinfo.ToTime))
                {
                    lsWorkingInfo.Data.Remove(workinfo);
                }
            }

            listResult.AddRange(lsEmployeeTransfer.Data);
            listResult.AddRange(lsWorkingInfo.Data);
            //countPage = lsEmployeeTransfer.TotalCount > lsWorkingInfo.TotalCount ? lsEmployeeTransfer.TotalCount + (lsEmployeeTransfer.TotalCount - lsWorkingInfo.TotalCount) : lsWorkingInfo.TotalCount + (lsWorkingInfo.TotalCount - lsEmployeeTransfer.TotalCount);
            countPage = listResult.Count;
            //dataGrid = new DataGridClass(countPage, listResult);
            dataGrid = new DataGridClass(lsEmployeeTransfer.TotalCount + lsWorkingInfo.TotalCount, listResult);
            result = Ok(dataGrid);
            return result;
        }

        [Authorize]
        [ActionName("ExportEmployeeTransfer")]
        [HttpPost]
        public IActionResult ExportEmployeeTransfer([FromBody] List<AddedParam> addedParams)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                //return new byte[0];
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            DataGridClass dataGrid = null;
            int countPage = 0;

            List<IC_EmployeeTransferDTO> listResult = new List<IC_EmployeeTransferDTO>();

            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
            var lsEmployeeTransfer = _iIC_EmployeeTransferLogic.GetMany(addedParams);
            var lsWorkingInfo = _iIC_WorkingInfoLogic.GetManyEmpTransfer(addedParams);
            listResult.AddRange(lsEmployeeTransfer);
            listResult.AddRange(lsWorkingInfo);

            string sWebRootFolder = _hostingEnvironment.ContentRootPath;
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/EmployeeTransfer.xlsx");
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/EmployeeTransfer.xlsx"));

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Employees");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                worksheet.Cell(currentRow, 2).Value = "Họ tên";
                worksheet.Cell(currentRow, 3).Value = "Ngày chuyển đi";
                worksheet.Cell(currentRow, 4).Value = "Ngày trở lại";
                worksheet.Cell(currentRow, 5).Value = "Phòng ban mới";
                worksheet.Cell(currentRow, 6).Value = "Loại điều chuyển";
                worksheet.Cell(currentRow, 7).Value = "Ngày duyệt";
                worksheet.Cell(currentRow, 8).Value = "Người duyệt";
                worksheet.Cell(currentRow, 9).Value = "Trạng thái";
                worksheet.Cell(currentRow, 10).Value = "Diễn giải";

                for (int i = 1; i < 11; i++)
                {
                    worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Column(i).Width = 20;
                }

                foreach (var users in listResult)
                {
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = users.EmployeeATID;
                    worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeATID.Length, '0');

                    worksheet.Cell(currentRow, 2).Value = users.FullName;
                    worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 3).Value = users.FromTime;
                    worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 4).Value = users.ToTime;
                    worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 5).Value = users.NewDepartmentName;
                    worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 6).Value = users.TypeTemporaryTransfer;
                    worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 7).Value = users.TransferApprovedDate;
                    worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 8).Value = users.TransferApprovedUser;
                    worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 9).Value = users.TransferApproveStatus;
                    worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 10).Value = users.Description;
                    worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                try
                {
                    ////var workbookBytes = new byte[0];
                    ////using (var ms = new MemoryStream())
                    ////{
                    ////    workbook.SaveAs(ms);
                    ////    return workbookBytes = ms.ToArray();
                    ////}
                    //workbook.SaveAs(file.FullName);
                    //return Ok(URL);
                    MemoryStream stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"EmployeeTransfer_{DateTime.Now.ToddMMyyyyHHmmss()}.xlsx"
                    };
                }
                catch (Exception ex)
                {
                    return NotFound("TemplateError");
                }
            }
        }


        [Authorize]
        [ActionName("AddEmployeeTransfer")]
        [HttpPost]
        public IActionResult AddEmployeeTransfer([FromBody] EmployeeTransferParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            List<AddedParam> addedParams = new List<AddedParam>();

            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (EmployeeTransferParam)StringHelper.RemoveWhiteSpace(param);
            if (param.EmployeeATID == "")
            {
                return BadRequest("PleaseFillAllRequiredFields");
            }


            if (param.TemporaryTransfer)
            {
                var checkDataTime = context.IC_EmployeeTransfer.Where(t => t.CompanyIndex == user.CompanyIndex
               && t.EmployeeATID == param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0')
               && (t.Status == (short)TransferStatus.Pendding || t.Status == (short)TransferStatus.Approve) &&
              param.FromTime <= t.ToTime && param.ToTime >= t.FromTime).ToList();

                if (checkDataTime != null && checkDataTime.Count > 0)
                {
                    return BadRequest("TimeExists");
                }

                var workingInfo = context.IC_WorkingInfo.FirstOrDefault(u => u.CompanyIndex == user.CompanyIndex
                             && u.EmployeeATID == param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0')
                             && u.Status == (short)TransferStatus.Approve && u.FromDate.Date <= DateTime.Now.Date
                             && (!u.ToDate.HasValue || u.ToDate.Value.Date >= DateTime.Now.Date));

                if (workingInfo != null && workingInfo.ToDate != null && workingInfo.ToDate.Value.Date <= param.ToTime.Value.Date)
                {
                    var userInfoList = context.HR_User.FirstOrDefault(x => x.EmployeeATID == param.EmployeeATID);
                    var messageError = "<p>  - " + param.EmployeeATID + " " + userInfoList?.FullName + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";
                    if (string.IsNullOrWhiteSpace(messageError))
                    {
                        return BadRequest("WorkingInfoIsExists");
                    }
                    return ApiOk(messageError);
                }

                IC_EmployeeTransfer empTranfer = new IC_EmployeeTransfer();
                empTranfer.EmployeeATID = param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                empTranfer.CompanyIndex = user.CompanyIndex;
                empTranfer.ToTime = param.ToTime.Value;
                empTranfer.FromTime = param.FromTime;
                empTranfer.Description = param.Description;
                empTranfer.IsSync = null;
                empTranfer.CreatedDate = DateTime.Today;
                empTranfer.UpdatedDate = DateTime.Today;
                empTranfer.UpdatedUser = user.UserName;
                empTranfer.NewDepartment = param.NewDepartment;
                empTranfer.OldDepartment = workingInfo == null ? 0 : workingInfo.DepartmentIndex;
                empTranfer.AddOnNewDepartment = param.AddOnNewDepartment;
                empTranfer.RemoveFromOldDepartment = param.RemoveFromOldDepartment;
                context.IC_EmployeeTransfer.Add(empTranfer);

            }
            else
            {
                addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "EmployeeATID", Value = param.EmployeeATID });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "CheckExistFromDate", Value = param.FromTime });
                List<IC_WorkingInfoDTO> checkData = _iIC_WorkingInfoLogic.GetMany(addedParams).ToList();
                if (checkData != null && checkData.Count() > 0)
                {
                    var haveDepartment = checkData.Where(u => u.DepartmentIndex > 0).ToList();
                    if (haveDepartment != null && haveDepartment.Count > 0)
                    {
                        var userInfoList = context.HR_User.Where(x => haveDepartment.Select(z => z.EmployeeATID).Contains(x.EmployeeATID)).ToList();
                        var messageError = "";
                        foreach (var info in haveDepartment)
                        {
                            messageError += "<p>  - " + info.EmployeeATID + " " + userInfoList.FirstOrDefault(x => x.EmployeeATID == info.EmployeeATID)?.FullName + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";
                        }
                        return ApiOk(messageError);
                    }

                    //if (haveDepartment != null)
                    //{
                    //    return BadRequest("WorkingInfoIsExists");
                    //}
                }

                var noDeartment = checkData.FirstOrDefault(u => u.DepartmentIndex == 0);
                if (noDeartment != null)
                {
                    IC_WorkingInfo updateInfo = new IC_WorkingInfo();
                    updateInfo.Index = noDeartment.Index;
                    updateInfo.EmployeeATID = noDeartment.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                    updateInfo.CompanyIndex = noDeartment.CompanyIndex;
                    updateInfo.IsManager = noDeartment.IsManager;
                    updateInfo.UpdatedUser = user.UserName;
                    updateInfo.DepartmentIndex = param.NewDepartment;
                    updateInfo.Status = (short)TransferStatus.Pendding;
                    updateInfo.FromDate = DateTime.Today;
                    updateInfo.UpdatedDate = DateTime.Today;
                    context.IC_WorkingInfo.Update(updateInfo);
                }
                else
                {
                    IC_WorkingInfo workingInfo = new IC_WorkingInfo();
                    workingInfo.CompanyIndex = user.CompanyIndex;
                    workingInfo.EmployeeATID = param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                    workingInfo.DepartmentIndex = param.NewDepartment;
                    workingInfo.FromDate = param.FromTime;
                    workingInfo.UpdatedDate = DateTime.Today;
                    workingInfo.UpdatedUser = user.UserName;
                    context.IC_WorkingInfo.Add(workingInfo);
                }
            }


            // Add Notify 
            List<IC_UserNotificationDTO> listUserNotify = new List<IC_UserNotificationDTO>();

            addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            addedParams.Add(new AddedParam { Key = "DepartmentIndex", Value = param.NewDepartment });
            listUserNotify = _iIC_UserNotificationLogic.GetListUserNotify(addedParams);
            if (listUserNotify.Count() > 0)
            {
                foreach (var item in listUserNotify)
                {
                    item.Status = 0;
                    item.Type = 0; // 0 is submit employee transfer
                    item.Message = JsonConvert.SerializeObject(new MessageBodyDTO
                    {
                        Message = "1",
                        FromDate = param.FromTime,
                        ToDate = param.ToTime,
                        FromUser = user.FullName,
                    });
                }
                _iIC_UserNotificationLogic.CreateList(listUserNotify);
            }
            context.SaveChanges();
            // Add audit log
            IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
            audit.TableName = "IC_EmployeeTransfer";
            audit.UserName = user.UserName;
            audit.CompanyIndex = user.CompanyIndex;
            audit.State = AuditType.Added;
            //audit.Description = AuditType.Added.ToString() + " Transfer Employee " + param.EmployeeATID;
            audit.Description = AuditType.Added.ToString() + "TransferEmployee:/:" + param.EmployeeATID;
            audit.DateTime = DateTime.Now;
            _iIC_AuditLogic.Create(audit);


            result = Ok();
            return result;
        }
        [Authorize]
        [ActionName("AddEmployeesTransfer")]
        [HttpPost]
        public IActionResult AddEmployeesTransfer([FromBody] EmployeesTransferParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            List<AddedParam> addedParams = new List<AddedParam>();

            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (EmployeesTransferParam)StringHelper.RemoveWhiteSpace(param);
            if (param.EmployeeATID == "")
            {
                return BadRequest("PleaseFillAllRequiredFields");
            }

            List<IC_WorkingInfoDTO> checkData = new List<IC_WorkingInfoDTO>();
            // Check thời gian khi nhân viên điều chuyển phòng ban
            if (param.TemporaryTransfer)
            {
                var checkDataTime = context.IC_EmployeeTransfer.Where(t => t.CompanyIndex == user.CompanyIndex
                           && param.ArrEmployeeATID.Contains(t.EmployeeATID)
                           && (t.Status == (short)TransferStatus.Pendding || t.Status == (short)TransferStatus.Approve) &&
                          param.FromTime <= t.ToTime && param.ToTime >= t.FromTime).ToList();
                if (checkDataTime != null && checkDataTime.Count > 0)
                {
                    return Conflict("EmployeeTransferIsExist");
                }
            }
            else
            {
                addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = param.ArrEmployeeATID });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "CheckExistFromDate", Value = param.FromTime });
                checkData = _iIC_WorkingInfoLogic.GetMany(addedParams).ToList();
                if (checkData != null && checkData.Count() > 0)
                {
                    var haveDepartment = checkData.Where(u => u.DepartmentIndex > 0).ToList();
                    if (haveDepartment != null && haveDepartment.Count > 0)
                    {
                        var userInfoList = context.HR_User.Where(x => haveDepartment.Select(z => z.EmployeeATID).Contains(x.EmployeeATID)).ToList();
                        var messageError = "";
                        foreach (var info in haveDepartment)
                        {
                            messageError += "<p>  - " + info.EmployeeATID + " " + userInfoList.FirstOrDefault(x => x.EmployeeATID == info.EmployeeATID)?.FullName + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";
                        }
                        return ApiOk(messageError);
                    }
                    //var haveDepartment = checkData.FirstOrDefault(u => u.DepartmentIndex > 0);
                    //if (haveDepartment != null)
                    //{
                    //    return BadRequest("WorkingInfoIsExists");
                    //}
                }
            }

            var listWorkingInfo = context.IC_WorkingInfo.Where(u => u.CompanyIndex == user.CompanyIndex
                            && param.ArrEmployeeATID.Contains(u.EmployeeATID)
                            && u.Status == (short)TransferStatus.Approve
                            && (u.ToDate == null && DateTime.Now.Date >= u.FromDate.Date)
                                || (u.ToDate != null && DateTime.Now.Date >= u.FromDate.Date && DateTime.Now.Date <= u.ToDate.Value.Date)).ToList();

            var userInfoDetailList = context.HR_User.Where(x => param.ArrEmployeeATID.Contains(x.EmployeeATID)).ToList();
            var messageTemporaryTransferError = "";
            for (int i = 0; i < param.ArrEmployeeATID.Count; i++)
            {
                if (param.TemporaryTransfer)
                {
                    var employeeATID = param.ArrEmployeeATID[i].PadLeft(config.MaxLenghtEmployeeATID, '0');
                    var workingInfo = listWorkingInfo.FirstOrDefault(e => e.EmployeeATID == employeeATID);

                    if (workingInfo != null && workingInfo.ToDate != null && workingInfo.ToDate <= param.ToTime)
                    {
                        string fullName = userInfoDetailList.FirstOrDefault(x => x.EmployeeATID == employeeATID)?.FullName;
                        messageTemporaryTransferError += "<p>  - " + employeeATID + " " + fullName + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";
                        continue;
                    }

                    IC_EmployeeTransfer empTranfer = new IC_EmployeeTransfer();
                    empTranfer.EmployeeATID = param.ArrEmployeeATID[i].PadLeft(config.MaxLenghtEmployeeATID, '0');
                    empTranfer.CompanyIndex = user.CompanyIndex;
                    empTranfer.ToTime = param.ToTime.Value;
                    empTranfer.FromTime = param.FromTime;
                    empTranfer.Description = param.Description;
                    empTranfer.IsSync = null;
                    empTranfer.CreatedDate = DateTime.Now;
                    empTranfer.UpdatedDate = DateTime.Now;
                    empTranfer.UpdatedUser = user.UserName;
                    empTranfer.NewDepartment = param.NewDepartment;
                    empTranfer.OldDepartment = workingInfo == null ? 0 : workingInfo.DepartmentIndex;
                    empTranfer.AddOnNewDepartment = param.AddOnNewDepartment;
                    empTranfer.RemoveFromOldDepartment = param.RemoveFromOldDepartment;
                    context.IC_EmployeeTransfer.Add(empTranfer);

                }
                else
                {
                    var noDeartment = checkData.FirstOrDefault(u => u.DepartmentIndex == 0);
                    if (noDeartment != null)
                    {
                        IC_WorkingInfo updateInfo = new IC_WorkingInfo();
                        updateInfo.Index = noDeartment.Index;
                        updateInfo.EmployeeATID = noDeartment.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                        updateInfo.CompanyIndex = noDeartment.CompanyIndex;
                        updateInfo.IsManager = noDeartment.IsManager;
                        updateInfo.UpdatedUser = user.UserName;
                        updateInfo.DepartmentIndex = param.NewDepartment;
                        updateInfo.Status = (short)TransferStatus.Pendding;
                        updateInfo.FromDate = DateTime.Now;
                        updateInfo.UpdatedDate = DateTime.Now;
                        context.IC_WorkingInfo.Update(updateInfo);
                    }
                    else
                    {
                        IC_WorkingInfo workingInfo = new IC_WorkingInfo();
                        workingInfo.CompanyIndex = user.CompanyIndex;
                        workingInfo.EmployeeATID = param.ArrEmployeeATID[i].PadLeft(config.MaxLenghtEmployeeATID, '0');
                        workingInfo.DepartmentIndex = param.NewDepartment;
                        workingInfo.FromDate = param.FromTime;
                        workingInfo.UpdatedDate = DateTime.Now;
                        workingInfo.UpdatedUser = user.UserName;
                        context.IC_WorkingInfo.Add(workingInfo);
                    }

                }
            }
            if (!string.IsNullOrWhiteSpace(messageTemporaryTransferError))
            {
                return ApiOk(messageTemporaryTransferError);
            }
            // Add Notify 
            List<IC_UserNotificationDTO> listUserNotify = new List<IC_UserNotificationDTO>();

            addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            addedParams.Add(new AddedParam { Key = "DepartmentIndex", Value = param.NewDepartment });
            listUserNotify = _iIC_UserNotificationLogic.GetListUserNotify(addedParams);

            foreach (var item in listUserNotify)
            {
                item.Status = 0;
                item.Type = 0; //  0 is submit emaployee transfer
                item.Message = JsonConvert.SerializeObject(new MessageBodyDTO { Message = param.ArrEmployeeATID.Count().ToString(), FromDate = param.FromTime, ToDate = param.ToTime, FromUser = user.FullName });
            }
            _iIC_UserNotificationLogic.CreateList(listUserNotify);

            context.SaveChanges();
            // Add audit log
            IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
            audit.TableName = "IC_EmployeeTransfer";
            audit.UserName = user.UserName;
            audit.CompanyIndex = user.CompanyIndex;
            audit.State = AuditType.Added;
            //audit.Description = AuditType.Added.ToString() + " Transfer " + param.ArrEmployeeATID.Count().ToString() + " Employee";
            audit.Description = AuditType.Added.ToString() + "TransferEmployees:/:" + param.ArrEmployeeATID.Count.ToString();
            audit.DateTime = DateTime.Now;
            _iIC_AuditLogic.Create(audit);


            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("GetWaitingApproveList")]
        [HttpGet]
        public IActionResult GetWaitingApproveList()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
            var listEmployee = _IIC_EmployeeLogic.GetEmployeeList(addedParams);

            List<WaitingApproveResult> listData = new List<WaitingApproveResult>();
            List<IC_EmployeeTransfer> listEmpTransfer = new List<IC_EmployeeTransfer>();

            List<IC_Department> listDepartment = context.IC_Department.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            var listEmployeeATID = listEmployee.Select(e => e.EmployeeATID).ToList();
            if (listEmployeeATID.Count > 5000)
            {
                var listSplitEmployeeID = CommonUtils.SplitList(listEmployeeATID, 5000);
                foreach (var listEmployeeSplit in listSplitEmployeeID)
                {
                    var resultEmployee = context.IC_EmployeeTransfer.Where(t => t.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(t.EmployeeATID) && t.Status == 0).ToList();
                    listEmpTransfer.AddRange(resultEmployee);
                }
            }
            else
            {
                listEmpTransfer = context.IC_EmployeeTransfer.Where(t => t.CompanyIndex == user.CompanyIndex && listEmployeeATID.Contains(t.EmployeeATID) && t.Status == 0).ToList();
            }

            for (int i = 0; i < listEmpTransfer.Count; i++)
            {
                IC_EmployeeDTO emp = listEmployee.Where(t => t.EmployeeATID == listEmpTransfer[i].EmployeeATID).FirstOrDefault();
                IC_Department dep = listDepartment.Where(t => t.Index == listEmpTransfer[i].NewDepartment).FirstOrDefault();
                if (emp != null)
                {
                    WaitingApproveResult data = new WaitingApproveResult();
                    data.Index = 0;
                    data.EmployeeATID = listEmpTransfer[i].EmployeeATID;
                    data.CompanyIndex = listEmpTransfer[i].CompanyIndex;
                    data.FromDate = listEmpTransfer[i].FromTime;
                    data.NewDepartmentIndex = listEmpTransfer[i].NewDepartment;
                    data.Type = 1;

                    data.EmployeeName = emp.FullName;
                    data.NewDepartment = dep == null ? "" : dep.Name;
                    data.OldDepartment = emp.DepartmentName;
                    data.ToDate = listEmpTransfer[i].ToTime;
                    data.SuggestUser = listEmpTransfer[i].UpdatedUser;
                    data.SuggestDate = listEmpTransfer[i].UpdatedDate;

                    listData.Add(data);
                }
            }

            var listWorkingInfo = listEmployee.Where(u => u.TransferStatus == (short)TransferStatus.Pendding).ToList();
            for (int i = 0; i < listWorkingInfo.Count; i++)
            {
                IC_EmployeeDTO emp = listEmployee.Where(t => t.EmployeeATID == listWorkingInfo[i].EmployeeATID).FirstOrDefault();
                IC_Department dep = listDepartment.Where(t => t.Index == listWorkingInfo[i].DepartmentIndex).FirstOrDefault();

                WaitingApproveResult data = new WaitingApproveResult();
                data.Index = (int)listWorkingInfo[i].WorkingInfoIndex;
                data.EmployeeATID = listWorkingInfo[i].EmployeeATID;
                data.CompanyIndex = listWorkingInfo[i].CompanyIndex;
                data.FromDate = listWorkingInfo[i].FromDate.Value;
                data.NewDepartmentIndex = (int)listWorkingInfo[i].DepartmentIndex;
                data.Type = 2;

                data.EmployeeName = emp.FullName;
                data.NewDepartment = dep == null ? "" : dep.Name;
                data.OldDepartment = emp.DepartmentName;
                data.ToDate = listWorkingInfo[i].ToDate;
                data.SuggestUser = listWorkingInfo[i].UpdatedUser;
                data.SuggestDate = listWorkingInfo[i].UpdatedDate;

                listData.Add(data);
            }

            return Ok(listData);
        }

        [Authorize]
        [ActionName("DeleteApproveEvent")]
        [HttpPost]
        public IActionResult DeleteApproveEvent([FromBody] List<WaitingApproveResult> listParam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            List<IC_WorkingInfo> listWorkingInfo = context.IC_WorkingInfo.Where(t => t.CompanyIndex == user.CompanyIndex && t.Status == 0).ToList();
            List<IC_EmployeeTransfer> listEmpTransfer = context.IC_EmployeeTransfer.Where(t => t.CompanyIndex == user.CompanyIndex && t.Status == 0).ToList();
            try
            {
                for (int i = 0; i < listParam.Count; i++)
                {
                    // delete workinginfo
                    if (listParam[i].Index > 0)
                    {
                        IC_WorkingInfo workingInfo = listWorkingInfo.Where(t => t.Index == listParam[i].Index).FirstOrDefault();
                        if (workingInfo != null)
                        {
                            workingInfo.Status = 2;

                        }
                    }
                    else
                    {
                        IC_EmployeeTransfer empTrasnfer = listEmpTransfer.Where(t => t.EmployeeATID == listParam[i].EmployeeATID && t.FromTime.Date == listParam[i].FromDate.Date
                            && t.NewDepartment == listParam[i].NewDepartmentIndex).FirstOrDefault();
                        if (empTrasnfer != null)
                        {
                            empTrasnfer.Status = 2;

                        }

                    }
                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            return Ok();
        }

        [Authorize]
        [ActionName("DeleteEmployeeTransfer")]
        [HttpPost]
        public IActionResult DeleteEmployeeTransfer([FromBody] EmployeeTransferParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IC_EmployeeTransfer deleteData = context.IC_EmployeeTransfer
              .Where(t => t.CompanyIndex == user.CompanyIndex && t.EmployeeATID == param.EmployeeATID
              && t.FromTime == param.FromTime && t.NewDepartment == param.NewDepartment).FirstOrDefault();
            if (deleteData.IsSync.HasValue && deleteData.IsSync.Value == true)
            {
                return BadRequest("IsSynced");
            }
            context.IC_EmployeeTransfer.Remove(deleteData);

            context.SaveChanges();
            // Add audit log
            IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
            audit.TableName = "IC_EmployeeTransfer";
            audit.UserName = user.UserName;
            audit.CompanyIndex = user.CompanyIndex;
            audit.State = AuditType.Deleted;
            //audit.Description = AuditType.Deleted.ToString() + " Transfer Employee " + param.EmployeeATID.ToString();
            audit.Description = AuditType.Deleted.ToString() + "TransferEmployee:/:" + param.EmployeeATID.ToString();
            audit.DateTime = DateTime.Now;
            _iIC_AuditLogic.Create(audit);
            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("UpdateEmployeeTransfer")]
        [HttpPost]
        public IActionResult UpdateEmployeeTransfer([FromBody] EmployeeTransferParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (EmployeeTransferParam)StringHelper.RemoveWhiteSpace(param);

            IC_EmployeeTransfer updateData = context.IC_EmployeeTransfer
                .Where(t => t.CompanyIndex == user.CompanyIndex && t.EmployeeATID == param.EmployeeATID
                && t.FromTime == param.FromTime && t.NewDepartment == param.NewDepartment).FirstOrDefault();

            if (updateData != null)
            {
                if (updateData.IsSync.HasValue && updateData.IsSync.Value == true)
                {
                    return BadRequest("IsSynced");
                }
                updateData.EmployeeATID = param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                updateData.CompanyIndex = user.CompanyIndex;
                updateData.ToTime = param.ToTime.Value;
                updateData.FromTime = param.FromTime;
                updateData.IsSync = null;
                updateData.Description = param.Description;
                updateData.CreatedDate = DateTime.Now;
                updateData.UpdatedDate = DateTime.Now;
                updateData.UpdatedUser = user.UserName;
                updateData.NewDepartment = param.NewDepartment;
                updateData.AddOnNewDepartment = param.AddOnNewDepartment;
                updateData.RemoveFromOldDepartment = param.RemoveFromOldDepartment;

                context.SaveChanges();

                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "IC_EmployeeTransfer";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Modified;
                //audit.Description = AuditType.Modified.ToString() + " Transfer Employee " + param.EmployeeATID.ToString();
                audit.Description = AuditType.Modified.ToString() + "TransferEmployee:/:" + param.EmployeeATID.ToString();
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);
            }

            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("ApproveOrRejectEmployeeTransfer")]
        [HttpPost]
        public async Task<IActionResult> ApproveOrRejectEmployeeTransfer([FromBody] List<IC_EmployeeTransferDTO> lsparam)
        {
            // Note: Please follow step by step at below command code to understand logic need to do

            List<AddedParam> addedParams = new List<AddedParam>();
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            if (lsparam.FirstOrDefault(u => u.EmployeeATID == "") != null)
            {
                return BadRequest("PleaseFillAllRequiredFields");
            }

            // 1. Update Employee Transfer or  working info
            foreach (var item in lsparam)
            {
                item.CompanyIndex = user.CompanyIndex;
                var param = (IC_EmployeeTransferDTO)StringHelper.RemoveWhiteSpace(item);

                if (param.TemporaryTransfer)
                {
                    var employeeTransfer = context.IC_EmployeeTransfer.Where(t => t.FromTime == param.FromTime && t.ToTime == param.ToTime && t.CompanyIndex == user.CompanyIndex && t.EmployeeATID == param.EmployeeATID && t.Status == (short)TransferStatus.Pendding).FirstOrDefault();
                    if (employeeTransfer != null)
                    {

                        employeeTransfer.Status = param.Status;
                        employeeTransfer.ApprovedDate = DateTime.Now;
                        employeeTransfer.ApprovedUser = user.UserName;
                        context.IC_EmployeeTransfer.Update(employeeTransfer);
                    }
                }
                else
                {
                    var currentApproveItem = context.IC_WorkingInfo.FirstOrDefault(u => u.Index == param.WorkingInfoIndex && u.CompanyIndex == user.CompanyIndex && u.EmployeeATID == param.EmployeeATID && u.Status == (short)TransferStatus.Pendding);
                    if (currentApproveItem != null)
                    {
                        var departmentInfo = context.IC_Department.FirstOrDefault(x => x.Index == currentApproveItem.DepartmentIndex);
                        if (departmentInfo != null)
                        {
                            var employeeInfo = context.HR_User.FirstOrDefault(x => x.EmployeeATID == currentApproveItem.EmployeeATID && x.CompanyIndex == user.CompanyIndex);
                            if (departmentInfo.IsContractorDepartment == true)
                            {
                                employeeInfo.EmployeeType = (short)EmployeeType.Contractor;
                                context.HR_User.Update(employeeInfo);

                                var contractorExist = context.HR_CustomerInfo.FirstOrDefault(x => x.EmployeeATID == currentApproveItem.EmployeeATID && x.CompanyIndex == user.CompanyIndex);
                                if (contractorExist == null)
                                {
                                    var contractorNew = new HR_CustomerInfo();
                                    contractorNew.EmployeeATID = employeeInfo.EmployeeATID;
                                    contractorNew.NRIC = employeeInfo.Nric;
                                    contractorNew.Address = employeeInfo.Address;
                                    contractorNew.IsAllowPhone = employeeInfo.IsAllowPhone;
                                    contractorNew.Note = employeeInfo.Note;
                                    contractorNew.CompanyIndex = user.CompanyIndex;
                                    contractorNew.UpdatedDate = DateTime.Now;
                                    contractorNew.UpdatedUser = user.UserName;
                                    contractorNew.FromTime = DateTime.Now;
                                    contractorNew.ToTime = new DateTime();
                                    _DbContext.HR_CustomerInfo.Add(contractorNew);
                                }
                            }
                            if (employeeInfo.EmployeeType == (short)EmployeeType.Contractor && (departmentInfo.IsContractorDepartment == null || departmentInfo.IsContractorDepartment == false))
                            {
                                employeeInfo.EmployeeType = (short)EmployeeType.Employee;
                                context.HR_User.Update(employeeInfo);
                            }
                        }

                        currentApproveItem.Status = param.Status;
                        currentApproveItem.ApprovedDate = DateTime.Now;
                        currentApproveItem.ApprovedUser = user.UserName;
                        context.IC_WorkingInfo.Update(currentApproveItem);
                    }

                    if (param.Status == (short)TransferStatus.Reject)
                    {
                        List<IC_WorkingInfo> listWorkingInfo = context.IC_WorkingInfo.Where(u => u.CompanyIndex == user.CompanyIndex && u.EmployeeATID == param.EmployeeATID).ToList();
                        if (listWorkingInfo != null && listWorkingInfo.Count() == 1)
                        {
                            IC_WorkingInfo newWorking = new IC_WorkingInfo();
                            newWorking.CompanyIndex = user.CompanyIndex;
                            newWorking.EmployeeATID = param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                            newWorking.DepartmentIndex = 0;
                            newWorking.ApprovedDate = DateTime.Now;
                            newWorking.ApprovedUser = user.UserName;
                            newWorking.FromDate = DateTime.Now;
                            newWorking.IsManager = false;
                            newWorking.UpdatedUser = user.UserName;
                            newWorking.Status = (short)TransferStatus.Approve;
                            newWorking.IsSync = true;
                            context.IC_WorkingInfo.Add(newWorking);
                        }
                    }

                }
            }

            // 2  Add Notification to send to requester 
            List<IC_UserNotificationDTO> listUserNotify = new List<IC_UserNotificationDTO>();
            var listCountEmployee = lsparam.GroupBy(u => new { u.CompanyIndex, u.NewDepartment }).Select(c => new { c.Key, Count = c.Count() }).ToList();
            foreach (var employee in listCountEmployee)
            {
                addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "DepartmentIndex", Value = employee.Key.NewDepartment });
                listUserNotify.AddRange(_iIC_UserNotificationLogic.GetListUserNotify(addedParams));

                foreach (var item in listUserNotify)
                {
                    item.Status = 0;
                    item.Type = lsparam[0].Status; // 0 is submit , 1 is approve, 2 is reject
                    item.Message = JsonConvert.SerializeObject(new MessageBodyDTO
                    {
                        Message = employee.Count.ToString(),
                        ApproveDate = DateTime.Now,
                        Approver = user.UserName,
                    });
                }
            }

            _iIC_UserNotificationLogic.CreateList(listUserNotify);
            context.SaveChanges();

            //3. Rebase working fromdate and todate info after save approve working info
            foreach (var item in lsparam)
            {
                if (!item.TemporaryTransfer)
                {
                    _iIC_WorkingInfoLogic.ReBaseWorkingInfo(item.EmployeeATID, item.CompanyIndex);
                    _iIC_WorkingInfoLogic.SaveChange();
                }
            }

            //4. Create command sync to device
            if (lsparam[0].TransferNow)
            {
                _iIC_CommandLogic.TransferUser(lsparam);
            }

            //Add employee in department AC
            //var employeeInfoList = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeATIDs, DateTime.Now, user.CompanyIndex);
            var listDepartmetnTransferNew = lsparam.Select(x => x.NewDepartment).ToList();
            //var listDepartmetnTransferOld = lsparam.Select(x => x.NewDepartment).ToList();

            var employeeInDepartment = _DbContext.AC_DepartmentAccessedGroup.Where(x => listDepartmetnTransferNew.Contains(x.DepartmentIndex)).ToList();
            if (employeeInDepartment != null && employeeInDepartment.Count > 0 && lsparam[0].TransferNow)
            {
                //var employeeAccessGr = employeeInfoList.Where(x => employeeInDepartment.Select(z => z.DepartmentIndex).Contains(x.DepartmentIndex)).ToList();
                foreach (var departmentAcc in employeeInDepartment)
                {
                    var listUserAcc = lsparam.Where(x => x.NewDepartment == departmentAcc.DepartmentIndex).Select(x => x.EmployeeATID).ToList();
                    await _iC_CommandService.UploadTimeZone(departmentAcc.GroupIndex, user);
                    await _iC_CommandService.UploadUsers(departmentAcc.GroupIndex, listUserAcc, user);
                    await _iC_CommandService.UploadACUsers(departmentAcc.GroupIndex, listUserAcc, user);
                }
            }




            // Add audit log
            IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
            audit.TableName = "IC_EmployeeTransfer";
            audit.UserName = user.UserName;
            audit.CompanyIndex = user.CompanyIndex;
            audit.State = AuditType.Modified;
            //audit.Description = AuditType.Modified.ToString() + " Apparove or reject Employee " + lsparam.Count().ToString();
            audit.Description = AuditType.Modified.ToString() + "ApproveOrRejectTransferEmployee:/:" + lsparam.Count().ToString();
            audit.DateTime = DateTime.Now;
            _iIC_AuditLogic.Create(audit);
            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("UpdateEmployeeTransferNew")]
        [HttpPost]
        public IActionResult UpdateEmployeeTransferNew([FromBody] List<EmployeesTransfer> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            ConfigObject config = ConfigObject.GetConfig(cache);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            lsparam[0] = (EmployeesTransfer)StringHelper.RemoveWhiteSpace(lsparam[0]);

            IC_EmployeeTransfer checkUpdateData = context.IC_EmployeeTransfer
                .Where(t => t.CompanyIndex == user.CompanyIndex && t.EmployeeATID == lsparam[0].EmployeeATID
                && t.FromTime == lsparam[0].FromTime && t.NewDepartment == lsparam[0].NewDepartment).FirstOrDefault();
            long OldDepartment = checkUpdateData.OldDepartment.Value;
            if (checkUpdateData != null)
            {
                if (checkUpdateData.IsSync.HasValue && checkUpdateData.IsSync.Value == true)
                {
                    return BadRequest("IsSynced");
                }

                //check fromtime and totime có tồn tài trong khoảng thời gian nhân viên được điều chuyển không
                var checkDataTime = context.IC_EmployeeTransfer.Where(t => t.CompanyIndex == user.CompanyIndex
                && t.EmployeeATID == lsparam[0].EmployeeATID &&
               lsparam[1].FromTime <= t.ToTime && lsparam[1].ToTime >= t.FromTime).ToList();

                if (checkDataTime != null && checkDataTime.Count > 1)
                {
                    return BadRequest("TimeExists");
                }
                else if (checkDataTime != null && checkDataTime.Count == 1)
                {
                    if (checkUpdateData == checkDataTime.FirstOrDefault())
                    {
                        context.IC_EmployeeTransfer.Remove(checkUpdateData);
                        IC_EmployeeTransfer updateData = new IC_EmployeeTransfer();
                        updateData.EmployeeATID = lsparam[1].EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                        updateData.CompanyIndex = user.CompanyIndex;
                        updateData.ToTime = lsparam[1].ToTime;
                        updateData.FromTime = lsparam[1].FromTime;
                        updateData.IsSync = null;
                        updateData.Description = lsparam[1].Description;
                        updateData.CreatedDate = DateTime.Now;
                        updateData.UpdatedDate = DateTime.Now;
                        updateData.UpdatedUser = user.UserName;
                        updateData.OldDepartment = OldDepartment;
                        updateData.NewDepartment = lsparam[1].NewDepartment;
                        updateData.AddOnNewDepartment = lsparam[1].AddOnNewDepartment;
                        updateData.RemoveFromOldDepartment = lsparam[1].RemoveFromOldDepartment;
                        context.Add(updateData);
                    }
                    else
                    {
                        return BadRequest("TimeExists");
                    }
                }
                else
                {
                    context.IC_EmployeeTransfer.Remove(checkUpdateData);

                    IC_EmployeeTransfer updateData = new IC_EmployeeTransfer();
                    updateData.EmployeeATID = lsparam[1].EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                    updateData.CompanyIndex = user.CompanyIndex;
                    updateData.ToTime = lsparam[1].ToTime;
                    updateData.FromTime = lsparam[1].FromTime;
                    updateData.IsSync = null;
                    updateData.Description = lsparam[1].Description;
                    updateData.CreatedDate = DateTime.Now;
                    updateData.UpdatedDate = DateTime.Now;
                    updateData.UpdatedUser = user.UserName;
                    updateData.OldDepartment = OldDepartment;
                    updateData.NewDepartment = lsparam[1].NewDepartment;
                    updateData.AddOnNewDepartment = lsparam[1].AddOnNewDepartment;
                    updateData.RemoveFromOldDepartment = lsparam[1].RemoveFromOldDepartment;
                    context.Add(updateData);
                }


            }

            context.SaveChanges();
            // Add audit log
            IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
            audit.TableName = "IC_EmployeeTransfer";
            audit.UserName = user.UserName;
            audit.CompanyIndex = user.CompanyIndex;
            audit.State = AuditType.Modified;
            //audit.Description = AuditType.Modified.ToString() + " Employee " + lsparam.Count().ToString();
            audit.Description = AuditType.Modified.ToString() + "TransferEmployee:/:" + lsparam.Count().ToString();
            audit.DateTime = DateTime.Now;
            _iIC_AuditLogic.Create(audit);
            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("AddEmployeeTransferFromExcel")]
        [HttpPost]
        public ActionResult<string> AddEmployeeTransferFromExcel([FromBody] List<EmployeesTransferImportExcel> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            ActionResult result = Unauthorized();
            bool checkTimeExist = false;
            int departmentIndex = 0;
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            StringBuilder stringBuilder = new StringBuilder();
            DateTime _fromTime = DateTime.Now;
            DateTime _toTime = DateTime.Now;
            foreach (var param in lsparam)
            {
                try
                {
                    _fromTime = DateTime.ParseExact(param.FromTime, "dd/MM/yyyy", null);
                    _toTime = DateTime.ParseExact(param.ToTime, "dd/MM/yyyy", null);
                    if (_fromTime > _toTime)
                    {
                        stringBuilder.Append(param.EmployeeATID + ":" + "Lỗi thời gian ngày chuyển đi lớn hơn ngày trở lại").AppendLine();
                        continue;
                        //return BadRequest("TimeErrorFromExcel");
                    }
                }
                catch
                {
                    stringBuilder.Append(param.EmployeeATID + ":" + "Định dạng sai thời gian").AppendLine();
                    continue;
                    //return BadRequest("TimeErrorFromExcel");
                }

                //param = (EmployeesTransferImportExcel)PublicFunctions.RemoveWhiteSpace(param);
                if (param.EmployeeATID == "")
                {
                    return BadRequest("PleaseFillAllRequiredFields");
                }


                var checkDataTime = context.IC_EmployeeTransfer.Where(t => t.CompanyIndex == user.CompanyIndex
                       && t.EmployeeATID == param.EmployeeATID &&
                      _fromTime <= t.ToTime && _toTime >= t.FromTime).ToList();

                if (checkDataTime != null && checkDataTime.Count > 0)
                {
                    stringBuilder.Append(param.EmployeeATID + ":" + "Thông tin điều chuyển đã tồn tại trong khoảng thời gian này").AppendLine();
                    continue;
                    //return BadRequest("TimeExists");
                }
                IC_EmployeeTransfer checkData = context.IC_EmployeeTransfer.Where(t => t.CompanyIndex == user.CompanyIndex
                && t.EmployeeATID == param.EmployeeATID &&
                t.FromTime <= _fromTime && t.ToTime >= _toTime).FirstOrDefault();
                if (checkData != null)
                {
                    stringBuilder.Append(param.EmployeeATID + ":" + "Phòng ban điều chuyển đã tồn tại").AppendLine();
                    continue;
                    //return Conflict("EmployeeTransferIsExist");
                }

                if (param.NewDepartmentName.Contains(@"/") && string.IsNullOrEmpty(param.NewDepartmentName) == false)
                {

                    var value = param.NewDepartmentName.Split('/');
                    var dpParent = value[0];
                    var dpchildren = value[1];

                    var dpIndexTemp = (from dp in context.IC_Department
                                       join dpp in context.IC_Department on dp.ParentIndex equals dpp.Index
                                       where (dp.Name == dpchildren && dpp.Name == dpParent)
                                       select new { dp.Index }).Select(t => t.Index);
                    if (dpIndexTemp.Any())
                    {
                        departmentIndex = dpIndexTemp.First();
                    }
                    else
                    {
                        departmentIndex = 0;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(param.NewDepartmentName) == false)
                    {
                        departmentIndex = Convert.ToInt32(context.IC_Department.Where(t => t.Name.Equals(param.NewDepartmentName)).Select(t => t.Index).FirstOrDefault());
                    }
                    else
                    {
                        departmentIndex = 0;
                    }
                }
                HR_User employee = context.HR_User.Where(t => t.CompanyIndex == user.CompanyIndex && t.EmployeeATID == param.EmployeeATID).FirstOrDefault();
                if (employee != null)
                {
                    IC_EmployeeTransfer empTranfer = new IC_EmployeeTransfer();
                    empTranfer.EmployeeATID = param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                    empTranfer.CompanyIndex = user.CompanyIndex;
                    empTranfer.ToTime = _toTime;
                    empTranfer.FromTime = _fromTime;
                    empTranfer.IsSync = false;
                    empTranfer.Description = param.Description;
                    empTranfer.CreatedDate = DateTime.Now;
                    empTranfer.UpdatedDate = DateTime.Now;
                    empTranfer.UpdatedUser = user.UserName;
                    empTranfer.NewDepartment = departmentIndex;
                    empTranfer.OldDepartment = 0;//employee.DepartmentIndex;
                    empTranfer.AddOnNewDepartment = param.AddOnNewDepartment;
                    empTranfer.RemoveFromOldDepartment = param.RemoveFromOldDepartment;
                    context.IC_EmployeeTransfer.Add(empTranfer);
                }
                else
                {
                    stringBuilder.Append(param.EmployeeATID + ":" + "Nhân viên không tồn tại").AppendLine();
                    continue;
                }
            }

            if (string.IsNullOrEmpty(stringBuilder.ToString()))
            {
                context.SaveChanges();
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "IC_EmployeeTransfer";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Transfer Employee from Excel " + lsparam.Count().ToString();
                audit.Description = AuditType.Added.ToString() + "TransferEmployeesFromExcel:/:" + lsparam.Count().ToString();
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);
                result = Ok("Thêm nhân viên điều chuyển thành công");
            }
            else
            {
                result = Ok(stringBuilder.ToString());
            }
            return result;
        }

        [Authorize]
        [ActionName("AddEmployeesTransferFromExcel")]
        [HttpPost]
        public async Task<ActionResult<string>> AddEmployeesTransferFromExcel([FromBody] List<EmployeesTransferImportExcel> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var listParamEmployeeATID = lsparam.Select(x => x.EmployeeATID).Distinct().ToList();
            var listEmployee = await _HR_EmployeeInfoService.GetEmployeeInfoByEmployeeATIDs(listParamEmployeeATID, user.CompanyIndex);

            var listNewDepartmentName = new List<string>();
            foreach (var param in lsparam)
            {
                if (param.NewDepartmentName.Contains("/"))
                {
                    var splitDepartmentName = param.NewDepartmentName.Split("/").ToList();
                    listNewDepartmentName.AddRange(splitDepartmentName);
                }
                else
                {
                    listNewDepartmentName.Add(param.NewDepartmentName);
                }
            }
            var listNewDepartment = _iIC_DepartmentLogic.GetByNames(listNewDepartmentName);

            var listTemporaryTransferParam = lsparam.Where(x => !x.PernamentTransfer).ToList();
            var listTemporaryTransferEmployeeATID = new List<string>();
            if (listTemporaryTransferParam != null && listTemporaryTransferParam.Count > 0)
            {
                listTemporaryTransferEmployeeATID = listTemporaryTransferParam.Select(x => x.EmployeeATID).ToList();
                listTemporaryTransferEmployeeATID.AddRange(listTemporaryTransferParam.Select(x
                    => x.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0')).ToList());
            }
            var listPernamentTransferParam = lsparam.Where(x => x.PernamentTransfer).ToList();
            var listPernamentTransferEmployeeATID = new List<string>();
            if (listPernamentTransferParam != null && listPernamentTransferParam.Count > 0)
            {
                listPernamentTransferEmployeeATID = listPernamentTransferParam.Select(x => x.EmployeeATID).ToList();
                listPernamentTransferEmployeeATID.AddRange(listPernamentTransferParam.Select(x
                    => x.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0')).ToList());
            }

            var addedParams = new List<AddedParam>();
            var listExistedPernamentTransfer = new List<IC_WorkingInfoDTO>();
            var listExistedTemporaryTransfer = new List<IC_EmployeeTransferDTO>();
            var listTemporaryTransferWorkingInfo = new List<IC_WorkingInfoDTO>();

            if (listTemporaryTransferEmployeeATID != null && listTemporaryTransferEmployeeATID.Count > 0)
            {
                addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listTemporaryTransferEmployeeATID });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "IsApprovedOrPending", Value = true });
                listExistedTemporaryTransfer = _iIC_EmployeeTransferLogic.GetMany(addedParams);
                listTemporaryTransferWorkingInfo = _iIC_WorkingInfoLogic.GetMany(addedParams).ToList();
                if (listTemporaryTransferWorkingInfo != null && listTemporaryTransferWorkingInfo.Count > 0)
                {
                    listTemporaryTransferWorkingInfo = listTemporaryTransferWorkingInfo.Where(x => x.Status == (short)TransferStatus.Approve
                        && (x.ToDate == null && DateTime.Now.Date >= x.FromDate.Date)
                        || (x.ToDate != null && DateTime.Now.Date >= x.FromDate.Date
                        && DateTime.Now.Date <= x.ToDate.Value.Date)).ToList();
                }
            }

            if (listPernamentTransferEmployeeATID != null && listPernamentTransferEmployeeATID.Count > 0)
            {
                addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listPernamentTransferEmployeeATID });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "IsApprovedOrPending", Value = true });
                listExistedPernamentTransfer = _iIC_WorkingInfoLogic.GetMany(addedParams).ToList();
            }

            ActionResult result = Unauthorized();

            var listCreateEmployeeTransfer = new List<IC_EmployeeTransfer>();
            var listCreateWorkingInfo = new List<IC_WorkingInfo>();
            var listUpdateWorkingInfo = new List<IC_WorkingInfo>();

            StringBuilder stringBuilder = new StringBuilder();
            var fromTime = new DateTime();
            var toTime = new DateTime();
            for (int i = 0; i < lsparam.Count; i++)
            {
                if (lsparam[i].EmployeeATID == "")
                {
                    return BadRequest("PleaseFillAllRequiredFields");
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(lsparam[i].FromTime))
                    {
                        fromTime = DateTime.ParseExact(lsparam[i].FromTime, "dd/MM/yyyy", null);
                        if (fromTime.Date < DateTime.Now.Date)
                        {
                            stringBuilder.Append(i.ToString() + ":/:" + "FromDateCannotSmallerThanCurrentDate" + ":*:");
                        }
                    }
                    if (!lsparam[i].PernamentTransfer && !string.IsNullOrWhiteSpace(lsparam[i].ToTime))
                    {
                        toTime = DateTime.ParseExact(lsparam[i].ToTime, "dd/MM/yyyy", null);
                        if (toTime.Date < DateTime.Now.Date)
                        {
                            stringBuilder.Append(i.ToString() + ":/:" + "ToDateCannotSmallerThanCurrentDate" + ":*:");
                        }
                    }
                    if (!lsparam[i].PernamentTransfer && !string.IsNullOrWhiteSpace(lsparam[i].FromTime) && !string.IsNullOrWhiteSpace(lsparam[i].ToTime)
                        && fromTime > toTime)
                    {
                        stringBuilder.Append(i.ToString() + ":/:" + "StartDateCannotLargerThanEndDate" + ":*:");
                        continue;
                    }
                }
                catch
                {
                    stringBuilder.Append(i.ToString() + ":/:" + "WrongTimeFormat" + ":*:");
                    continue;
                }

                var checkDataTime = listExistedTemporaryTransfer.Where(t => t.EmployeeATID == lsparam[i].EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0')
                    && fromTime <= t.ToTime && toTime >= t.FromTime).ToList();

                if (checkDataTime != null && checkDataTime.Count > 0 && !lsparam[i].PernamentTransfer)
                {
                    stringBuilder.Append(i.ToString() + ":/:" + "EmployeeTransferExisted" + ":*:");
                    continue;
                }
                int departmentIndex = 0;
                if (lsparam[i].NewDepartmentName.Contains("/"))
                {
                    var splitDepartmentName = lsparam[i].NewDepartmentName.Split("/").ToList();
                    int parentIndex = 0;
                    for (var j = 0; j < splitDepartmentName.Count; j++)
                    {
                        var existedDepartment = new IC_DepartmentDTO();
                        if (j == 0)
                        {
                            existedDepartment = listNewDepartment.FirstOrDefault(x => x.Name == splitDepartmentName[j]
                                && (!x.ParentIndex.HasValue || (x.ParentIndex.HasValue && x.ParentIndex.Value == 0)));
                        }
                        else
                        {
                            existedDepartment = listNewDepartment.FirstOrDefault(x => x.Name == splitDepartmentName[j]
                                    && x.ParentIndex.HasValue && x.ParentIndex.Value == parentIndex);
                        }
                        if (existedDepartment == null)
                        {
                            break;
                        }
                        else if (existedDepartment != null && j < (splitDepartmentName.Count - 1))
                        {
                            parentIndex = (int)existedDepartment.Index;
                        }
                        else
                        {
                            departmentIndex = (int)existedDepartment.Index;
                        }
                    }
                }
                else
                {
                    var existedDepartment = listNewDepartment.FirstOrDefault(x => x.Name == lsparam[i].NewDepartmentName
                                && (!x.ParentIndex.HasValue || (x.ParentIndex.HasValue && x.ParentIndex.Value == 0)));
                    if (existedDepartment != null)
                    {
                        departmentIndex = (int)existedDepartment.Index;
                    }
                }

                if (departmentIndex <= 0)
                {
                    stringBuilder.Append(i.ToString() + ":/:" + "DepartmentNotExists" + ":*:");
                    continue;
                }

                var departmentDriver = listNewDepartment.FirstOrDefault(x => x.Index == departmentIndex);
                if (departmentDriver != null && departmentDriver.IsDriverDepartment == true)
                {
                    stringBuilder.Append(i.ToString() + ":/:" + "NotTransferToDepartmentDriver" + ":*:");
                    continue;
                }

                var employee = listEmployee.FirstOrDefault(t
                    => t.EmployeeID == lsparam[i].EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0'));
                if (employee != null)
                {
                    if (!lsparam[i].PernamentTransfer)
                    {
                        var oldWorkingInfo = listTemporaryTransferWorkingInfo.FirstOrDefault(x
                            => x.EmployeeATID == lsparam[i].EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0'));

                        if (oldWorkingInfo != null && oldWorkingInfo.ToDate != null && oldWorkingInfo.ToDate.Value <= toTime.Date)
                        {
                            stringBuilder.Append(i.ToString() + ":/:" + "Import_ThereHasBeenInformationAboutQuitting" + ":*:");
                        }
                        else
                        {
                            IC_EmployeeTransfer empTranfer = new IC_EmployeeTransfer();
                            empTranfer.EmployeeATID = lsparam[i].EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                            empTranfer.CompanyIndex = user.CompanyIndex;
                            empTranfer.ToTime = toTime;
                            empTranfer.FromTime = fromTime;
                            empTranfer.IsSync = false;
                            empTranfer.Description = lsparam[i].Description;
                            empTranfer.CreatedDate = DateTime.Now;
                            empTranfer.UpdatedDate = DateTime.Now;
                            empTranfer.UpdatedUser = user.UserName;
                            empTranfer.NewDepartment = departmentIndex;
                            empTranfer.OldDepartment = oldWorkingInfo?.DepartmentIndex ?? 0;
                            empTranfer.AddOnNewDepartment = lsparam[i].AddOnNewDepartment;
                            empTranfer.RemoveFromOldDepartment = lsparam[i].RemoveFromOldDepartment;
                            listCreateEmployeeTransfer.Add(empTranfer);
                        }
                    }
                    else
                    {
                        var checkExistToDate = listExistedPernamentTransfer.Where(x => x.EmployeeATID == lsparam[i].EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0')
                                                                                                        && (x.ToDate.HasValue && x.ToDate.Value.Date > fromTime.Date)).ToList();

                        var listExistedWorkingInfo = listExistedPernamentTransfer.Where(x => x.EmployeeATID == lsparam[i].EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0')
                            && fromTime.Date <= x.FromDate.Date
                            && ((x.ToDate.HasValue && fromTime.Date <= x.ToDate.Value.Date)
                            || !x.ToDate.HasValue || x.DepartmentIndex == 0)).ToList();

                        if (listExistedWorkingInfo != null && listExistedWorkingInfo.Count > 0
                            && listExistedWorkingInfo.Any(x => x.DepartmentIndex > 0))
                        {
                            stringBuilder.Append(i.ToString() + ":/:" + "WorkingInfoIsExists" + ":*:");
                        }
                        else if (checkExistToDate != null && checkExistToDate.Count > 0)
                        {
                            stringBuilder.Append(i.ToString() + ":/:" + "Import_ThereHasBeenInformationAboutQuitting" + ":*:");
                        }
                        else if (listExistedWorkingInfo != null && listExistedWorkingInfo.Count > 0
                            && listExistedWorkingInfo.Any(x => x.DepartmentIndex == 0))
                        {
                            var noDepartmentWorkingInfo = listExistedWorkingInfo.FirstOrDefault(x => x.DepartmentIndex == 0);
                            IC_WorkingInfo updateInfo = new IC_WorkingInfo();
                            updateInfo.Index = noDepartmentWorkingInfo.Index;
                            updateInfo.EmployeeATID = noDepartmentWorkingInfo.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                            updateInfo.CompanyIndex = noDepartmentWorkingInfo.CompanyIndex;
                            updateInfo.IsManager = noDepartmentWorkingInfo.IsManager;
                            updateInfo.UpdatedUser = user.UserName;
                            updateInfo.DepartmentIndex = departmentIndex;
                            updateInfo.Status = (short)TransferStatus.Pendding;
                            updateInfo.FromDate = fromTime;
                            updateInfo.UpdatedDate = DateTime.Now;
                            listUpdateWorkingInfo.Add(updateInfo);
                        }
                        else
                        {
                            IC_WorkingInfo workingInfo = new IC_WorkingInfo();
                            workingInfo.CompanyIndex = user.CompanyIndex;
                            workingInfo.EmployeeATID = lsparam[i].EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                            workingInfo.DepartmentIndex = departmentIndex;
                            workingInfo.FromDate = fromTime;
                            workingInfo.UpdatedDate = DateTime.Now;
                            workingInfo.UpdatedUser = user.UserName;
                            workingInfo.Status = (short)TransferStatus.Pendding;
                            listCreateWorkingInfo.Add(workingInfo);
                        }
                    }
                }
                else
                {
                    stringBuilder.Append(i.ToString() + ":/:" + "EmployeeNotExist" + ":*:");
                    continue;
                }
            }

            if (string.IsNullOrEmpty(stringBuilder.ToString()))
            {
                if (listCreateEmployeeTransfer != null && listCreateEmployeeTransfer.Count > 0)
                {
                    _iIC_EmployeeTransferLogic.CreateSave(listCreateEmployeeTransfer);
                }
                if (listCreateWorkingInfo != null && listCreateWorkingInfo.Count > 0)
                {
                    _iIC_WorkingInfoLogic.CreateSave(listCreateWorkingInfo);
                }
                if (listUpdateWorkingInfo != null && listUpdateWorkingInfo.Count > 0)
                {
                    _iIC_WorkingInfoLogic.UpdateSave(listUpdateWorkingInfo);
                }
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "IC_EmployeeTransfer";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                audit.Description = AuditType.Added.ToString() + "TransferEmployeesFromExcel:/:" + lsparam.Count().ToString();
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);
                result = Ok("AddEmployeeTransferSuccess");
            }
            else
            {
                result = Ok(stringBuilder.ToString());
            }
            return result;
        }

        [NonAction]
        public static List<DateTime> GetDates(DateTime fromTime, DateTime toTime)
        {
            return Enumerable.Range(0, 1 + toTime.Subtract(fromTime).Days).Select(offset => fromTime.AddDays(offset)).ToList(); // Load dates into a list
        }

        public class EmployeeTransferParam
        {
            public string EmployeeATID { get; set; }

            public int NewDepartment { get; set; }

            public DateTime FromTime { get; set; }

            public int CompanyIndex { get; set; }

            public int? OldDepartment { get; set; }

            public bool RemoveFromOldDepartment { get; set; }

            public bool AddOnNewDepartment { get; set; }

            public DateTime? ToTime { get; set; }

            public bool IsSync { get; set; }

            public string Description { get; set; }

            public DateTime? CreatedDate { get; set; }

            public DateTime? UpdatedDate { get; set; }

            public string UpdatedUser { get; set; }
            public bool TemporaryTransfer { get; set; }

            public short Status { get; set; }
        }

        public class EmployeesTransferParam
        {
            public List<string> ArrEmployeeATID { get; set; }
            public string EmployeeATID { get; set; }

            public int NewDepartment { get; set; }

            public DateTime FromTime { get; set; }

            public int? OldDepartment { get; set; }

            public bool RemoveFromOldDepartment { get; set; }

            public bool AddOnNewDepartment { get; set; }

            public DateTime? ToTime { get; set; }

            public bool IsSync { get; set; }

            public string Description { get; set; }

            public DateTime? CreatedDate { get; set; }

            public DateTime? UpdatedDate { get; set; }

            public string UpdatedUser { get; set; }

            public bool TemporaryTransfer { get; set; }

            public short Status { get; set; }
        }

        public class EmployeesTransferImportExcel
        {
            public string EmployeeATID { get; set; }
            public string NewDepartmentName { get; set; }
            public string FromTime { get; set; }
            public string ToTime { get; set; }
            public bool RemoveFromOldDepartment { get; set; }
            public bool AddOnNewDepartment { get; set; }
            public bool PernamentTransfer { get; set; }
            public string Description { get; set; }
        }

        public class EmployeesTransfer
        {
            public string EmployeeATID { get; set; }
            public int NewDepartment { get; set; }
            public DateTime FromTime { get; set; }
            public DateTime ToTime { get; set; }
            public string IsFromTime { get; set; }
            public string IsToTime { get; set; }
            public int OldDepartment { get; set; }
            public bool RemoveFromOldDepartment { get; set; }
            public bool AddOnNewDepartment { get; set; }
            public bool? IsSync { get; set; }
            public string Description { get; set; }
        }

        public class EmployeesTransferGrid
        {
            public string EmployeeATID { get; set; }
            public string FullName { get; set; }
            public string IsFromTime { get; set; }
            public string IsToTime { get; set; }
            public DateTime? FromTime { get; set; }
            public DateTime? ToTime { get; set; }
            public string Description { get; set; }
            public int NewDepartment { get; set; }
            public string NewDepartmentName { get; set; }
            public int? OldDepartment { get; set; }
            public string OldDepartmentName { get; set; }
            public string RemoveFromOldDepartmentName { get; set; }
            public bool? RemoveFromOldDepartment { get; set; }
            public bool? AddOnNewDepartment { get; set; }
            public string AddOnNewDepartmentName { get; set; }
            public string TypeTemporaryTransfer { get; set; }
            public string TransferApprovedDate { get; set; }
            public string TransferApprovedUser { get; set; }
            public short Status { get; set; }
            public bool TemporaryTransfer { get; set; }
        }

        public class WaitingApproveResult
        {
            public int Index { get; set; }
            public string EmployeeATID { get; set; }
            public int CompanyIndex { get; set; }
            public DateTime FromDate { get; set; }
            public int NewDepartmentIndex { get; set; }
            public short Type { get; set; }

            public string EmployeeName { get; set; }
            public string NewDepartment { get; set; }
            public string OldDepartment { get; set; }
            public DateTime? ToDate { get; set; }
            public string SuggestUser { get; set; }
            public DateTime? SuggestDate { get; set; }
            public bool IsChecked { get; set; }
            public WaitingApproveResult()
            {
                IsChecked = false;
            }
        }

    }
}
