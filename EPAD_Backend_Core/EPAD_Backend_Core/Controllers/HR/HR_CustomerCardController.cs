using Chilkat;
using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.Models.DTOs;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HR_CustomerCardController : ApiControllerBase
    {
        private readonly IHR_CustomerCardService _HR_CustomerCardService;
        private readonly IHR_CustomerInfoService _HR_CustomerInfoService;
        private readonly IHR_UserService _HR_UserService;
        private readonly IHR_CardNumberInfoService _HR_CardNumberInfoService;
        private readonly IIC_UserMasterService _IC_UserMasterService;
        private readonly IHR_ContractorInfoService _IHR_ContractorInfoService;
        private readonly IIC_UserMasterLogic _IIC_UserMasterLogic;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IIC_AuditLogic _IIC_AuditLogic;
        private readonly IIC_CommandLogic _IIC_CommandLogic;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        private IMemoryCache cache;
        private readonly IIC_WorkingInfoService _IC_WorkingInfoService;
        private readonly IIC_DepartmentLogic _iC_DepartmentLogic;
        ConfigObject _Config;

        public HR_CustomerCardController(IServiceProvider pProvider) : base(pProvider)
        {
            _HR_CustomerCardService = TryResolve<IHR_CustomerCardService>();
            _HR_CustomerInfoService = TryResolve<IHR_CustomerInfoService>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _HR_CardNumberInfoService = TryResolve<IHR_CardNumberInfoService>();
            _IC_UserMasterService = TryResolve<IIC_UserMasterService>();
            _IHR_ContractorInfoService = TryResolve<IHR_ContractorInfoService>();
            _IIC_UserMasterLogic = TryResolve<IIC_UserMasterLogic>();
            cache = TryResolve<IMemoryCache>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _IIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _Config = ConfigObject.GetConfig(cache);
            _IIC_CommandLogic = TryResolve<IIC_CommandLogic>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _IC_WorkingInfoService = TryResolve<IIC_WorkingInfoService>();
            _iC_DepartmentLogic = TryResolve<IIC_DepartmentLogic>();
        }

        [Authorize]
        [ActionName("GetCustomerCardRequirement")]
        [HttpGet]
        public IActionResult GetCustomerCardRequirement()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var maxLength = _Config.MaxLenghtEmployeeATID;
            var prefixID = _Config.AutoGenerateIDPrefix;
            dynamic result = new ExpandoObject();
            result.MaxLength = maxLength;
            result.PrefixID = prefixID;
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetCustomerCardAtPage")]
        [HttpGet]
        public async Task<IActionResult> GetCustomerCardAtPage([FromQuery] string filter, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
            addedParams.Add(new AddedParam { Key = "PageIndex", Value = page });
            addedParams.Add(new AddedParam { Key = "PageSize", Value = pageSize });
            var customerCard = await _HR_CustomerCardService.GetPage(addedParams, user);
            return ApiOk(customerCard);
        }

        [Authorize]
        [ActionName("GetAllCustomerCard")]
        [HttpGet]
        public async Task<IActionResult> GetAllCustomerCard()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allCustomerCard = await _HR_CustomerCardService.GetAllCustomerCard(user);
            return ApiOk(allCustomerCard);
        }

        [Authorize]
        [ActionName("SyncCustomerCardToDevice")]
        [HttpGet]
        public async Task<IActionResult> SyncCustomerCardToDevice()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allCustomerCard = await _HR_CustomerCardService.GetAllCustomerCard(user);
            if (allCustomerCard.Count > 0)
            {
                var result = await _HR_CustomerCardService.SyncCustomerCardToDevice(allCustomerCard, user);
                return ApiOk(result);
            }
            return ApiOk(new List<string>());
        }

        [Authorize]
        [ActionName("AddCustomerCard")]
        [HttpPost]
        public async Task<IActionResult> AddCustomerCard(CustomerCardModel param)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var isCardExisted = await _HR_CustomerCardService.IsCardNumberExisted(param.CardNumber, user);
            if (isCardExisted)
            {
                return ApiError("CardNumberExisted");
            }

            var isCardUsing = await _HR_CustomerCardService.IsCardNumberUsing(param.CardNumber, user);
            if (isCardUsing)
            {
                return ApiError("CardNumberExisted");
            }

            var isSuccess = await _HR_CustomerCardService.AddCustomerCard(param.CardNumber, param.IsSyncToDevice, user);
            

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("DeleteCustomerCard")]
        [HttpPost]
        public async Task<IActionResult> DeleteCustomerCard(List<int> listIndex)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var isCardUsing = await _HR_CustomerCardService.IsCardNumberUsing(listIndex, user);
            if (isCardUsing)
            {
                return ApiError("CardIsUsing");
            }

            var isSuccess = await _HR_CustomerCardService.DeleteCustomerCard(listIndex, user);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("AddCustomerCardFromExcel")]
        [HttpPost]
        public async Task<IActionResult> AddCustomerCardFromExcel(List<CustomerCardModel> lstImport)
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
                var listError = new List<CustomerCardModel>();

                listError = await _HR_CustomerCardService.ValidationImportCustomerCard(lstImport, user);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_HR_CustomerCard_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_HR_CustomerCard_Error.xlsx"));

                if (listError != null && listError.Count() > 0)
                {
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook(file.FullName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var currentRow = 1;
                        //worksheet.Cell(currentRow, 1).Value = "Mã thẻ (*)";
                        //worksheet.Cell(currentRow, 2).Value = "Đồng bộ lên máy";
                        //worksheet.Cell(currentRow, 3).Value = "Lỗi";

                        //for (int i = 1; i < 4; i++)
                        //{
                        //    worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                        //    worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        //    worksheet.Column(i).Width = 20;
                        //}

                        for (int i = (currentRow + 1); i <= worksheet.LastRowUsed().RowNumber(); i++)
                        {
                            worksheet.Row(i).Clear();
                        }

                        foreach (var item in listError)
                        {
                            currentRow++;
                            //New template
                            worksheet.Cell(currentRow, 1).Value = "'" + item.CardNumber;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 2).Value = item.IsSyncToDevice ? "x" : string.Empty;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 3).Value = "'" + item.ErrorMessage;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        workbook.SaveAs(file.FullName);
                    }
                }

                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "HR_CustomerCard";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + lstImport.Count().ToString() + " Department";
                audit.Description = AuditType.Added.ToString() + "AddCustomerCardFromExcel:/:" + lstImport.Count().ToString();
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
