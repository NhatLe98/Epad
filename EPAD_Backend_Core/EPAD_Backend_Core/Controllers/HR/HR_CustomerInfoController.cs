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
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HR_CustomerInfoController : ApiControllerBase
    {
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
        private readonly IGC_BlackListService _GC_BlackListService;
        private readonly IIC_VehicleLogService _IC_VehicleLogService;
        private readonly IHR_EmployeeLogic _IHR_EmployeeLogic;
        ConfigObject _Config;
        private readonly string _configClientName;
        private readonly ILogger _logger;
        private readonly IIC_CommandService _iC_CommandService;
        private EPAD_Context _context;
        private static List<string> _usingRandomCustomerID = new List<string>();

        public HR_CustomerInfoController(IServiceProvider pProvider, ILoggerFactory loggerFactory, EPAD_Context context) : base(pProvider)
        {
            _context = TryResolve<EPAD_Context>();
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
            _GC_BlackListService = TryResolve<IGC_BlackListService>();
            _IC_VehicleLogService = TryResolve<IIC_VehicleLogService>();
            _configClientName = _Configuration.GetValue<string>("ClientName");
            _logger = loggerFactory.CreateLogger<HR_CustomerInfoController>();
            _iC_CommandService = TryResolve<IIC_CommandService>();
            _IHR_EmployeeLogic = TryResolve<IHR_EmployeeLogic>();
        }

        [Authorize]
        [ActionName("GetCustomerAtPage")]
        [HttpGet]
        public async Task<ActionResult<List<HR_CustomerInfoResult>>> Get([FromQuery] string[] employeeATID, [FromQuery] string filter, [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] int employeeType)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
            addedParams.Add(new AddedParam { Key = "PageIndex", Value = page });
            addedParams.Add(new AddedParam { Key = "PageSize", Value = pageSize });
            addedParams.Add(new AddedParam { Key = "ListContactATID", Value = employeeATID });
            addedParams.Add(new AddedParam { Key = "EmployeeType", Value = employeeType });
            var allCustomer = await _HR_CustomerInfoService.GetPage(addedParams, user.CompanyIndex);
            //var allCustomer = await _HR_CustomerInfoService.GetDataGrid(user.CompanyIndex, page, pageSize);
            return ApiOk(allCustomer);
        }

        [Authorize]
        [ActionName("GetCustomerAtPageAdvance")]
        [HttpPost]
        public async Task<ActionResult<List<HR_CustomerInfoResult>>> GetCustomerAtPageAdvance([FromBody] CustomerRequestModel param)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "Filter", Value = param.filter });
            addedParams.Add(new AddedParam { Key = "PageIndex", Value = param.page });
            addedParams.Add(new AddedParam { Key = "PageSize", Value = param.pageSize });
            addedParams.Add(new AddedParam { Key = "ListContactATID", Value = param.employeeATID });
            addedParams.Add(new AddedParam { Key = "EmployeeType", Value = param.employeeType });
            addedParams.Add(new AddedParam { Key = "FilterDepartment", Value = param.filterDepartments });

            var allCustomer = await _HR_CustomerInfoService.GetPageAdvance(addedParams, user.CompanyIndex, param.studentOfParent);
            //var allCustomer = await _HR_CustomerInfoService.GetDataGrid(user.CompanyIndex, page, pageSize);
            return ApiOk(allCustomer);
        }

        [Authorize]
        [ActionName("GetCustomerAtPageByEmployeeATID")]
        [HttpGet]
        public async Task<ActionResult<List<HR_CustomerInfoResult>>> GetCustomerAtPageByEmployeeATID([FromQuery] string[] employeeATID, [FromQuery] string filter, [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] int employeeType)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
            addedParams.Add(new AddedParam { Key = "PageIndex", Value = page });
            addedParams.Add(new AddedParam { Key = "PageSize", Value = pageSize });
            addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = employeeATID });
            addedParams.Add(new AddedParam { Key = "EmployeeType", Value = employeeType });
            var allCustomer = await _HR_CustomerInfoService.GetPage(addedParams, user.CompanyIndex);
            //var allCustomer = await _HR_CustomerInfoService.GetDataGrid(user.CompanyIndex, page, pageSize);
            return ApiOk(allCustomer);
        }

        [Authorize]
        [ActionName("GetCustomerAtPageAdvanceByEmployeeATID")]
        [HttpPost]
        public async Task<ActionResult<List<HR_CustomerInfoResult>>> GetCustomerAtPageAdvanceByEmployeeATID([FromBody] CustomerRequestModel param)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "Filter", Value = param.filter });
            addedParams.Add(new AddedParam { Key = "PageIndex", Value = param.page });
            addedParams.Add(new AddedParam { Key = "PageSize", Value = param.pageSize });
            addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = param.employeeATID });
            addedParams.Add(new AddedParam { Key = "EmployeeType", Value = param.employeeType });
            addedParams.Add(new AddedParam { Key = "FilterDepartment", Value = param.filterDepartments });

            var allCustomer = await _HR_CustomerInfoService.GetPageAdvance(addedParams, user.CompanyIndex, param.studentOfParent);
            //var allCustomer = await _HR_CustomerInfoService.GetDataGrid(user.CompanyIndex, page, pageSize);
            return ApiOk(allCustomer);
        }

        [Authorize]
        [ActionName("Get_HR_CustomerInfos")]
        [HttpGet]
        public async Task<ActionResult<List<HR_CustomerInfoResult>>> Get_HR_CustomerInfos()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_CustomerInfoService.GetAllCustomerInfo(new string[0], user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("GetNewestActiveCustomerInfo")]
        [HttpGet]
        public async Task<ActionResult<List<HR_CustomerInfoResult>>> GetNewestActiveCustomerInfo()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_CustomerInfoService.GetNewestActiveCustomerInfo(new string[0], user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("GetNewestCustomerInfo")]
        [HttpGet]
        public async Task<ActionResult<List<HR_CustomerInfoResult>>> GetNewestCustomerInfo()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_CustomerInfoService.GetNewestCustomerInfo(new string[0], user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("GetCustomerInfoExcludeExpired")]
        [HttpGet]
        public async Task<ActionResult<List<HR_CustomerInfoResult>>> GetCustomerInfoExcludeExpired()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_CustomerInfoService.GetCustomerInfoExcludeExpired(new string[0], user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("GetCustomerAndContractorInfo")]
        [HttpGet]
        public async Task<ActionResult<List<HR_CustomerInfoResult>>> GetCustomerAndContractorInfo()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_CustomerInfoService.GetAllCustomerAndContractorInfo(new string[0], user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Get_HR_CustomerInfo")]
        [HttpGet("{employeeATID}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Get_HR_CustomerInfo(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employee = await _HR_CustomerInfoService.GetCustomerInfo(employeeATID, user.CompanyIndex);
            return ApiOk(employee);
        }

        [Authorize]
        [ActionName("GetCustomerById")]
        [HttpGet]
        public async Task<ActionResult<HR_EmployeeInfoResult>> GetCustomerById(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employee = await _HR_CustomerInfoService.GetCustomerInfo(employeeATID, user.CompanyIndex);
            return ApiOk(employee);
        }

        [Authorize]
        [ActionName("Post_HR_CustomerInfo")]
        [HttpPost]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Post_HR_CustomerInfo([FromBody] HR_CustomerInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var randomId = string.Empty;
            if (value.EmployeeType == (short)EmployeeType.Guest && string.IsNullOrWhiteSpace(value.EmployeeATID))
            {
                var customerIdExisted = await _HR_UserService.GetAllEmployeeATID();
                if (_usingRandomCustomerID != null && _usingRandomCustomerID.Count > 0)
                {
                    customerIdExisted.AddRange(_usingRandomCustomerID);
                }
                var randomIds = _HR_CustomerInfoService.GenerateUniqueNumberStrings(1, _Config.MaxLenghtEmployeeATID,
                    _Config.AutoGenerateCustomerIDPrefix, customerIdExisted);
                if (randomIds != null && randomIds.Count > 0)
                {
                    _usingRandomCustomerID.AddRange(randomIds);
                    randomId = randomIds[0];
                    value.EmployeeATID = randomId;
                }
                else
                {
                    return ApiError("OutOfGenerateCustomerID");
                }
            }

            value.EmployeeATID = value.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0');

            var cardActive = await _HR_CardNumberInfoService.FirstOrDefaultAsync(x => !string.IsNullOrWhiteSpace(x.CardNumber) && x.CardNumber == value.CardNumber && x.IsActive == true && x.CompanyIndex == user.CompanyIndex);
            if (cardActive != null)
            {
                return ApiError("CardNumberExist");
            }
            var check = await _HR_UserService.FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex && x.EmployeeATID == value.EmployeeATID);
            if (check != null)
                return ApiError("EmployeeATIDExist");
            value.CompanyIndex = user.CompanyIndex;
            if (!string.IsNullOrWhiteSpace(_configClientName) && _configClientName.ToUpper() == ClientName.MONDELEZ.ToString())
            {
                var startTimeConvert = !string.IsNullOrWhiteSpace(value.StartTimeStr) ? DateTime.ParseExact(value.StartTimeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) : value.StartTime;
                var endTimeConvert = !string.IsNullOrWhiteSpace(value.EndTimeStr) ? DateTime.ParseExact(value.EndTimeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) : value.EndTime;
                value.FromTime = new DateTime(value.FromTime.Value.Year, value.FromTime.Value.Month, value.FromTime.Value.Day, startTimeConvert.Hour, startTimeConvert.Minute, startTimeConvert.Second);
                value.ToTime = value.ToTime != null ? new DateTime(value.ToTime.Value.Year, value.ToTime.Value.Month, value.ToTime.Value.Day, endTimeConvert.Hour, endTimeConvert.Minute, endTimeConvert.Second) : null;


            }

            var fromDate = value.FromTime.HasValue ? value.FromTime.Value : DateTime.Now;
            var dateNow = DateTime.Now;
            var employeeInBlackList = _GC_BlackListService.Any(x => x.Nric != null && x.Nric == value.NRIC && x.FromDate.Date <= dateNow.Date && (x.ToDate == null || (x.ToDate != null && dateNow.Date <= x.ToDate.Value.Date)));
            if (employeeInBlackList)
            {
                return ApiError("EmployeeInBlackList");
            }

            HR_User u = _Mapper.Map<HR_CustomerInfoResult, HR_User>(value);
            if (!u.Gender.HasValue)
                u.Gender = (short)GenderEnum.Other;

            if (u.EmployeeType == null)
            {
                u.EmployeeType = (int)EmployeeType.Guest;
            }

            HR_CustomerInfo e = _Mapper.Map<HR_CustomerInfoResult, HR_CustomerInfo>(value);
            IC_UserMasterDTO us = _Mapper.Map<HR_CustomerInfoResult, IC_UserMasterDTO>(value);
            HR_CardNumberInfo c = _Mapper.Map<HR_CustomerInfoResult, HR_CardNumberInfo>(value);
            HR_ContractorInfo cp = _Mapper.Map<HR_CustomerInfoResult, HR_ContractorInfo>(value);

            if (e.IdentityImage != null && e.IdentityImage.Length == 0)
            {
                e.IdentityImage = null;
            }

            BeginTransaction();
            try
            {
                await _HR_UserService.InsertAsync(u);
                await _HR_CustomerInfoService.InsertAsync(e);
                await _HR_CardNumberInfoService.CheckCardActivedOrCreate(c, user.CompanyIndex);

                if (u.EmployeeType == (int)EmployeeType.Contractor)
                {
                    var checkWork = await _IC_WorkingInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == value.EmployeeATID && x.CompanyIndex == user.CompanyIndex && x.Status == (short)TransferStatus.Approve
               && x.FromDate.Date <= DateTime.Now.Date && (!x.ToDate.HasValue || x.ToDate.Value.Date >= DateTime.Now.Date));
                    if (checkWork != null)
                    {
                        checkWork = _Mapper.Map(value, checkWork);
                        _IC_WorkingInfoService.Update(checkWork);
                    }
                    else
                    {
                        // Working info
                        var wk = new IC_WorkingInfo()
                        {
                            EmployeeATID = value.EmployeeATID,
                            FromDate = value.FromTime != null ? value.FromTime.Value : DateTime.Now,
                            ToDate = value?.ToTime,
                            DepartmentIndex = value.DepartmentIndex ?? 0,
                            Status = (short)TransferStatus.Approve,
                            ApprovedDate = DateTime.Now,
                            PositionIndex = value.PositionIndex,
                        };

                        await _IC_WorkingInfoService.InsertAsync(wk);
                    }

                }

                _IIC_UserMasterLogic.CheckExistedOrCreate(us);
                await SaveChangeAsync();
                CommitTransaction();

                var employee = await _HR_CustomerInfoService.GetCustomerInfo(u.EmployeeATID, user.CompanyIndex);
                await _IC_VehicleLogService.IntegrateEmployeeToLovad(new List<string>() { value.EmployeeATID });
                await _IHR_EmployeeLogic.IntegrateUserToOfflineCustomer(new List<string>() { value.EmployeeATID });

                //Add employee in department AC
                var employeeInDepartment = _DbContext.AC_DepartmentAccessedGroup.FirstOrDefault(x => value.DepartmentIndex == x.DepartmentIndex);
                if (employeeInDepartment != null)
                {
                    await _iC_CommandService.UploadTimeZone(employeeInDepartment.GroupIndex, user);
                    await _iC_CommandService.UploadUsers(employeeInDepartment.GroupIndex, new List<string>() { value.EmployeeATID }, user);
                    await _iC_CommandService.UploadACUsers(employeeInDepartment.GroupIndex, new List<string>() { value.EmployeeATID }, user);
                }
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
            finally
            {
                if (u.EmployeeType == (short)EmployeeType.Guest && !string.IsNullOrWhiteSpace(randomId)
                    && _usingRandomCustomerID.Contains(randomId))
                {
                    _usingRandomCustomerID.Remove(randomId);
                }
            }
        }

        [Authorize]
        [ActionName("DeleteCustomerFromExcel")]
        [HttpPost]
        public IActionResult DeleteCustomerFromExcel([FromBody] List<AddedParam> addedParams, [FromQuery] int userType)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var paramEmployee = addedParams.FirstOrDefault(e => e.Key == "ListEmployeeATID");
            var paramDeleteOndevice = addedParams.FirstOrDefault(e => e.Key == "IsDeleteOnDevice");
            var lsemployeeATID = new List<string>();
            var isDeleteOnDevice = false;
            if (paramEmployee != null)
            {
                lsemployeeATID = JsonConvert.DeserializeObject<List<string>>(paramEmployee.Value.ToString());
            }
            else
            {
                return BadRequest("");
            }
            var listPadleft = new List<string>();
            foreach (var item in lsemployeeATID)
            {
                listPadleft.Add(item.PadLeft(_Config.MaxLenghtEmployeeATID, '0'));
            }
            var existedEmployee = _DbContext.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listPadleft.Contains(e.EmployeeATID) && e.EmployeeType == userType).ToList();

            if (paramDeleteOndevice != null)
            {
                isDeleteOnDevice = Convert.ToBoolean(paramDeleteOndevice.Value);
                if (isDeleteOnDevice)
                {
                    var lsSerialHw = _DbContext.IC_Device.Where(e => e.CompanyIndex == user.CompanyIndex).Select(e => e.SerialNumber).ToList();

                    if (existedEmployee != null)
                    {
                        listPadleft = existedEmployee.Select(e => e.EmployeeATID).ToList();

                        IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                        paramUserOnMachine.ListEmployeeaATID = listPadleft;
                        paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                        paramUserOnMachine.AuthenMode = "";
                        paramUserOnMachine.FullInfo = false;
                        List<UserInfoOnMachine> lstUser = _IIC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                        IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                        commandParam.IsOverwriteData = false;
                        commandParam.Action = CommandAction.DeleteUserById;
                        commandParam.AuthenMode = "";
                        commandParam.FromTime = DateTime.Now;
                        commandParam.ToTime = DateTime.Now;
                        commandParam.ListEmployee = lstUser;
                        commandParam.ListSerialNumber = lsSerialHw;
                        commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                        commandParam.ExternalData = "";
                        List<CommandResult> lstCmd = _IIC_CommandLogic.CreateListCommands(commandParam);
                        if (lstCmd != null && lstCmd.Count() > 0)
                        {
                            IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                            groupCommand.CompanyIndex = user.CompanyIndex;
                            groupCommand.UserName = user.UserName;
                            groupCommand.ListCommand = lstCmd;
                            groupCommand.GroupName = GroupName.DeleteUserById.ToString();
                            groupCommand.EventType = "";
                            _IIC_CommandLogic.CreateGroupCommands(groupCommand);
                        }
                    }
                }
            }

            var listEmp = _DbContext.HR_User.Where(t => t.CompanyIndex == user.CompanyIndex && listPadleft.Contains(t.EmployeeATID)).ToArray();
            var listUserContactInfo = _DbContext.HR_EmployeeInfo.Where(t => t.CompanyIndex == user.CompanyIndex && listPadleft.Contains(t.EmployeeATID)).ToArray();
            var listUserMaster = _DbContext.IC_UserMaster.Where(x => x.CompanyIndex == user.CompanyIndex && listPadleft.Contains(x.EmployeeATID)).ToArray();

            try
            {
                _DbContext.HR_User.RemoveRange(listEmp);
                _DbContext.IC_UserMaster.RemoveRange(listUserMaster);
                _DbContext.HR_EmployeeInfo.RemoveRange(listUserContactInfo);
                _DbContext.SaveChanges();
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "HR_User";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Deleted;
                //audit.Description = AuditType.Deleted.ToString() + " From Excel " + listEmp.Count().ToString() + " Customer";
                audit.Description = AuditType.Deleted.ToString() + "CustomerFromExcel:/:" + listEmp.Count().ToString();
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);
                result = Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("AddCustomerFromExcel")]
        [HttpPost]
        public async Task<IActionResult> AddCustomerFromExcel([FromBody] List<IC_CustomerImportDTO> param, [FromQuery] int userType)
        {
            var randomIds = new List<string>();
            try
            {
                UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
                ConfigObject config = ConfigObject.GetConfig(cache);
                IActionResult result = Unauthorized();
                int departmentIndex = 0;
                if (user == null)
                {
                    return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
                }
                // validation data
                List<IC_CustomerImportDTO> listError = new List<IC_CustomerImportDTO>();

                if (param != null && param.Count > 0 && userType == (short)EmployeeType.Guest)
                {
                    var countNotHaveID = param.Count(x => string.IsNullOrWhiteSpace(x.EmployeeATID));
                    if (countNotHaveID > 0)
                    {
                        var customerIdExisted = await _HR_UserService.GetAllEmployeeATID();
                        if (_usingRandomCustomerID != null && _usingRandomCustomerID.Count > 0)
                        {
                            customerIdExisted.AddRange(_usingRandomCustomerID);
                        }
                        randomIds = _HR_CustomerInfoService.GenerateUniqueNumberStrings(countNotHaveID, _Config.MaxLenghtEmployeeATID,
                            _Config.AutoGenerateCustomerIDPrefix, customerIdExisted);
                        _usingRandomCustomerID.AddRange(randomIds);
                        var notHaveIdParam = param.Where(x => string.IsNullOrWhiteSpace(x.EmployeeATID)).ToList();
                        for (var i = 0; i < countNotHaveID; i++)
                        {
                            notHaveIdParam[i].EmployeeATID = randomIds[i];
                        }
                    }
                }

                if (_configClientName.ToUpper() == ClientName.MONDELEZ.ToString())
                {
                    listError = await _HR_CustomerInfoService.ValidationImportCustomer_KinhDo(param, userType, user);
                }
                else
                {
                    listError = await _HR_CustomerInfoService.ValidationImportCustomer(param, userType, user);
                }

                var message = "";

                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/CustomerImportError.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/CustomerImportError.xlsx"));
                if (userType == 2)
                {
                    if (listError != null && listError.Count() > 0)
                    {
                        var listEmployeeIDError = listError.Select(e => e.EmployeeATID).ToList();
                        param = param.Where(e => !listEmployeeIDError.Contains(e.EmployeeATID)).ToList();
                        message = listError.Count().ToString();

                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("CustomerError");
                            var currentRow = 1;
                            if (!string.IsNullOrEmpty(_configClientName) && _configClientName.ToUpper() == ClientName.MONDELEZ.ToString())
                            {
                                worksheet.Cell(currentRow, 1).Value = "Mã người dùng";
                                worksheet.Cell(currentRow, 2).Value = "Họ tên (*)";
                                worksheet.Cell(currentRow, 3).Value = "Tên trên máy";
                                worksheet.Cell(currentRow, 4).Value = "Giới tính";
                                worksheet.Cell(currentRow, 5).Value = "Ngày sinh (ngày/tháng/năm) (*)";
                                worksheet.Cell(currentRow, 6).Value = "CMND/CCCD/Passport (*)";
                                worksheet.Cell(currentRow, 7).Value = "Tên công ty";
                                worksheet.Cell(currentRow, 8).Value = "Số điện thoại";
                                worksheet.Cell(currentRow, 9).Value = "Email";
                                worksheet.Cell(currentRow, 10).Value = "Địa chỉ";
                                worksheet.Cell(currentRow, 11).Value = "Phòng ban liên hệ (*)";
                                worksheet.Cell(currentRow, 12).Value = "Người liên hệ (*)";
                                worksheet.Cell(currentRow, 13).Value = "Từ ngày (*)";
                                worksheet.Cell(currentRow, 14).Value = "Đến ngày (*)";
                                worksheet.Cell(currentRow, 15).Value = "Từ giờ (*)";
                                worksheet.Cell(currentRow, 16).Value = "Đến giờ (*)";
                                worksheet.Cell(currentRow, 17).Value = "Sử dụng điện thoại";
                                worksheet.Cell(currentRow, 18).Value = "Nội dung làm việc";
                                worksheet.Cell(currentRow, 19).Value = "Ghi chú";
                                worksheet.Cell(currentRow, 20).Value = "Lỗi";
                            }
                            else
                            {
                                worksheet.Cell(currentRow, 1).Value = "Mã người dùng";
                                worksheet.Cell(currentRow, 2).Value = "Họ tên (*)";
                                worksheet.Cell(currentRow, 3).Value = "Tên trên máy";
                                worksheet.Cell(currentRow, 4).Value = "Giới tính";
                                worksheet.Cell(currentRow, 5).Value = "Ngày sinh (ngày/tháng/năm)";
                                worksheet.Cell(currentRow, 6).Value = "CMND/CCCD/Passport";
                                worksheet.Cell(currentRow, 7).Value = "Tên công ty";
                                worksheet.Cell(currentRow, 8).Value = "Số điện thoại";
                                worksheet.Cell(currentRow, 9).Value = "Email";
                                worksheet.Cell(currentRow, 10).Value = "Địa chỉ";
                                worksheet.Cell(currentRow, 11).Value = "Phòng ban liên hệ (*)";
                                worksheet.Cell(currentRow, 12).Value = "Người liên hệ (*)";
                                worksheet.Cell(currentRow, 13).Value = "Từ ngày";
                                worksheet.Cell(currentRow, 14).Value = "Đến ngày";
                                worksheet.Cell(currentRow, 15).Value = "Từ giờ";
                                worksheet.Cell(currentRow, 16).Value = "Đến giờ";
                                worksheet.Cell(currentRow, 17).Value = "Sử dụng điện thoại";
                                worksheet.Cell(currentRow, 18).Value = "Nội dung làm việc";
                                worksheet.Cell(currentRow, 19).Value = "Ghi chú";
                                worksheet.Cell(currentRow, 20).Value = "Lỗi";
                            }

                            for (int i = 1; i < 21; i++)
                            {
                                worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                                worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Column(i).Width = 20;
                            }

                            foreach (var users in listError)
                            {
                                currentRow++;
                                worksheet.Cell(currentRow, 1).Value = users.EmployeeATID;
                                worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeATID.Length, '0');


                                worksheet.Cell(currentRow, 2).Value = users.FullName;
                                worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 3).Value = users.NameOnMachine;
                                worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 5).Value = users.Gender == 1 ? "Nam" : users.Gender == 0 ? "Nữ" : users.Gender == 2 ? "Khác" : "";
                                worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 5).Value = users.DateOfBirth;
                                worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 6).Value = users.Nric;
                                worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 7).Value = users.Company;
                                worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 8).Value = users.PhoneNumber;
                                worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 9).Value = users.Email;
                                worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 10).Value = users.Address;
                                worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 11).Value = users.ContactDepartment;
                                worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 12).Value = "'" + users.ContactPerson;
                                worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 13).Value = users.JoinedDate;
                                worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 14).Value = users.StoppedDate;
                                worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 15).Value = users.StartTime;
                                worksheet.Cell(currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 16).Value = users.EndTime;
                                worksheet.Cell(currentRow, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 17).Value = users.IsAllowPhone == 1 ? "x" : "";
                                worksheet.Cell(currentRow, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 18).Value = users.WorkingContent;
                                worksheet.Cell(currentRow, 18).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 19).Value = users.Note;
                                worksheet.Cell(currentRow, 19).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 20).Value = "Lỗi";
                                worksheet.Cell(currentRow, 20).Value = users.ErrorMessage;
                                worksheet.Cell(currentRow, 20).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            }

                            workbook.SaveAs(file.FullName);
                        }
                    }
                }
                else if (userType == 4)
                {
                    if (listError != null && listError.Count() > 0)
                    {
                        var listEmployeeIDError = listError.Select(e => e.EmployeeATID).ToList();
                        param = param.Where(e => !listEmployeeIDError.Contains(e.EmployeeATID)).ToList();
                        message = listError.Count().ToString();

                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("CustomerError");
                            var currentRow = 1;
                            worksheet.Cell(currentRow, 1).Value = "Mã người dùng (*)";
                            worksheet.Cell(currentRow, 2).Value = "Mã hệ thống";
                            worksheet.Cell(currentRow, 3).Value = "Họ tên (*)";
                            worksheet.Cell(currentRow, 4).Value = "Mã thẻ";
                            worksheet.Cell(currentRow, 5).Value = "Tên trên máy";
                            worksheet.Cell(currentRow, 6).Value = "Giới tính (Nam)";
                            worksheet.Cell(currentRow, 7).Value = "Địa chỉ";
                            worksheet.Cell(currentRow, 8).Value = "Ghi chú";
                            if (userType == (int)EmployeeType.Contractor)
                            {
                                worksheet.Cell(currentRow, 9).Value = "Phòng ban";
                                worksheet.Cell(currentRow, 10).Value = "Lỗi";
                            }
                            else if (userType == (int)EmployeeType.Parents)
                            {
                                worksheet.Cell(currentRow, 9).Value = "Học sinh (*)";
                                worksheet.Cell(currentRow, 10).Value = "Lỗi";
                            }
                            else
                            {
                                worksheet.Cell(currentRow, 9).Value = "Lỗi";
                            }


                            for (int i = 1; i < 11; i++)
                            {
                                worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                                worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Column(i).Width = 20;
                            }

                            foreach (var users in listError)
                            {
                                currentRow++;
                                worksheet.Cell(currentRow, 1).Value = users.EmployeeATID;
                                worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeATID.Length, '0');

                                worksheet.Cell(currentRow, 2).Value = users.EmployeeCode;
                                worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                if (!string.IsNullOrWhiteSpace(users.EmployeeCode))
                                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeCode.Length, '0');

                                worksheet.Cell(currentRow, 3).Value = users.FullName;
                                worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 4).Value = users.CardNumber;
                                worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                if (!string.IsNullOrWhiteSpace(users.CardNumber))
                                    worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "0".PadLeft(users.CardNumber.Length, '0');

                                worksheet.Cell(currentRow, 5).Value = users.NameOnMachine;
                                worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 6).Value = users.Gender;
                                worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 7).Value = users.Address;
                                worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 8).Value = users.Note;
                                worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                if (userType == (int)EmployeeType.Contractor)
                                {
                                    worksheet.Cell(currentRow, 9).Value = users.Department;
                                    worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                    worksheet.Cell(currentRow, 10).Value = "Lỗi";
                                    worksheet.Cell(currentRow, 10).Value = users.ErrorMessage;
                                    worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                }
                                else if (userType == (int)EmployeeType.Parents)
                                {
                                    worksheet.Cell(currentRow, 9).Value = "'" + users.StudentOfParent;
                                    worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                    worksheet.Cell(currentRow, 10).Value = "Lỗi";
                                    worksheet.Cell(currentRow, 10).Value = users.ErrorMessage;
                                    worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                }
                                else
                                {
                                    worksheet.Cell(currentRow, 9).Value = "Lỗi";
                                    worksheet.Cell(currentRow, 9).Value = users.ErrorMessage;
                                    worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                }
                            }

                            workbook.SaveAs(file.FullName);
                        }
                    }
                }
                else if (userType == (int)EmployeeType.Contractor)
                {
                    if (listError != null && listError.Count() > 0)
                    {
                        var listEmployeeIDError = listError.Select(e => e.EmployeeATID).ToList();
                        param = param.Where(e => !listEmployeeIDError.Contains(e.EmployeeATID)).ToList();
                        message = listError.Count().ToString();

                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("ContractorError");
                            var currentRow = 1;
                            if (!string.IsNullOrEmpty(_configClientName) && _configClientName.ToUpper() == ClientName.MONDELEZ.ToString())
                            {
                                worksheet.Cell(currentRow, 1).Value = "Mã người dùng (*)";
                                worksheet.Cell(currentRow, 2).Value = "Họ tên (*)";
                                worksheet.Cell(currentRow, 3).Value = "Tên trên máy";
                                worksheet.Cell(currentRow, 4).Value = "Giới tính";
                                worksheet.Cell(currentRow, 5).Value = "Ngày sinh (ngày/tháng/năm) (*)";
                                worksheet.Cell(currentRow, 6).Value = "CMND/CCCD/Passport (*)";
                                worksheet.Cell(currentRow, 7).Value = "Số điện thoại";
                                worksheet.Cell(currentRow, 8).Value = "Email";
                                worksheet.Cell(currentRow, 9).Value = "Địa chỉ";
                                worksheet.Cell(currentRow, 10).Value = "Phòng ban (*)";
                                worksheet.Cell(currentRow, 11).Value = "Chức vụ";
                                worksheet.Cell(currentRow, 12).Value = "Sử dụng điện thoại";
                                worksheet.Cell(currentRow, 13).Value = "Ngày bắt đầu";
                                worksheet.Cell(currentRow, 14).Value = "Ngày kết thúc";
                                worksheet.Cell(currentRow, 15).Value = "Mật khẩu";
                                worksheet.Cell(currentRow, 16).Value = "Mã thẻ";
                                worksheet.Cell(currentRow, 17).Value = "Lỗi";
                            }
                            else
                            {
                                worksheet.Cell(currentRow, 1).Value = "Mã người dùng (*)";
                                worksheet.Cell(currentRow, 2).Value = "Họ tên (*)";
                                worksheet.Cell(currentRow, 3).Value = "Tên trên máy";
                                worksheet.Cell(currentRow, 4).Value = "Giới tính";
                                worksheet.Cell(currentRow, 5).Value = "Ngày sinh (ngày/tháng/năm)";
                                worksheet.Cell(currentRow, 6).Value = "CMND/CCCD/Passport";
                                worksheet.Cell(currentRow, 7).Value = "Số điện thoại";
                                worksheet.Cell(currentRow, 8).Value = "Email";
                                worksheet.Cell(currentRow, 9).Value = "Địa chỉ";
                                worksheet.Cell(currentRow, 10).Value = "Phòng ban (*)";
                                worksheet.Cell(currentRow, 11).Value = "Chức vụ";
                                worksheet.Cell(currentRow, 12).Value = "Sử dụng điện thoại";
                                worksheet.Cell(currentRow, 13).Value = "Ngày bắt đầu";
                                worksheet.Cell(currentRow, 14).Value = "Ngày kết thúc";
                                worksheet.Cell(currentRow, 15).Value = "Mật khẩu";
                                worksheet.Cell(currentRow, 16).Value = "Mã thẻ";
                                worksheet.Cell(currentRow, 17).Value = "Lỗi";
                            }

                            for (int i = 1; i < 18; i++)
                            {
                                worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                                worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Column(i).Width = 20;
                            }

                            foreach (var users in listError)
                            {
                                currentRow++;
                                worksheet.Cell(currentRow, 1).Value = users.EmployeeATID;
                                worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeATID.Length, '0');


                                worksheet.Cell(currentRow, 2).Value = users.FullName;
                                worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 3).Value = users.NameOnMachine;
                                worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 5).Value = users.Gender == 1 ? "Nam" : users.Gender == 0 ? "Nữ" : users.Gender == 2 ? "Khác" : "";
                                worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 5).Value = users.DateOfBirth;
                                worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 6).Value = users.Nric;
                                worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                                worksheet.Cell(currentRow, 7).Value = users.PhoneNumber;
                                worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 8).Value = users.Email;
                                worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 9).Value = users.Address;
                                worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 10).Value = users.Department;
                                worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 11).Value = users.PositionName;
                                worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 12).Value = users.IsAllowPhone == 1 ? "x" : "";
                                worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                                worksheet.Cell(currentRow, 13).Value = users.JoinedDate;
                                worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 14).Value = users.StoppedDate;
                                worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                                worksheet.Cell(currentRow, 15).Value = users.Password;
                                worksheet.Cell(currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 16).Value = users.CardNumber;
                                worksheet.Cell(currentRow, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 17).Value = "Lỗi";
                                worksheet.Cell(currentRow, 17).Value = users.ErrorMessage;
                                worksheet.Cell(currentRow, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            }

                            workbook.SaveAs(file.FullName);
                        }
                    }
                }
                else
                {
                    if (listError != null && listError.Count() > 0)
                    {
                        var listEmployeeIDError = listError.Select(e => e.EmployeeATID).ToList();
                        param = param.Where(e => !listEmployeeIDError.Contains(e.EmployeeATID)).ToList();
                        message = listError.Count().ToString();

                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("CustomerError");
                            var currentRow = 1;
                            worksheet.Cell(currentRow, 1).Value = "Mã người dùng (*)";
                            worksheet.Cell(currentRow, 2).Value = "Mã hệ thống";
                            worksheet.Cell(currentRow, 3).Value = "Họ tên";
                            worksheet.Cell(currentRow, 4).Value = "Mã thẻ";
                            worksheet.Cell(currentRow, 5).Value = "Tên trên máy";
                            worksheet.Cell(currentRow, 6).Value = "Giới tính (Nam)";
                            worksheet.Cell(currentRow, 7).Value = "Địa chỉ";
                            worksheet.Cell(currentRow, 8).Value = "Ghi chú";
                            if (userType == (int)EmployeeType.Contractor)
                            {
                                worksheet.Cell(currentRow, 9).Value = "Phòng ban";
                                worksheet.Cell(currentRow, 10).Value = "Lỗi";
                            }
                            else
                            {
                                worksheet.Cell(currentRow, 9).Value = "Lỗi";
                            }


                            for (int i = 1; i < 10; i++)
                            {
                                worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                                worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Column(i).Width = 20;
                            }

                            foreach (var users in listError)
                            {
                                currentRow++;
                                worksheet.Cell(currentRow, 1).Value = users.EmployeeATID;
                                worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeATID.Length, '0');

                                worksheet.Cell(currentRow, 2).Value = users.EmployeeCode;
                                worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                if (!string.IsNullOrWhiteSpace(users.EmployeeCode))
                                    worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeCode.Length, '0');

                                worksheet.Cell(currentRow, 3).Value = users.FullName;
                                worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 4).Value = users.CardNumber;
                                worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                if (!string.IsNullOrWhiteSpace(users.CardNumber))
                                    worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "0".PadLeft(users.CardNumber.Length, '0');

                                worksheet.Cell(currentRow, 5).Value = users.NameOnMachine;
                                worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 6).Value = users.Gender;
                                worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 7).Value = users.Address;
                                worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 8).Value = users.Note;
                                worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                if (userType == (int)EmployeeType.Contractor)
                                {
                                    worksheet.Cell(currentRow, 9).Value = users.Department;
                                    worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                    worksheet.Cell(currentRow, 10).Value = "Lỗi";
                                    worksheet.Cell(currentRow, 10).Value = users.ErrorMessage;
                                    worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                }
                                else
                                {
                                    worksheet.Cell(currentRow, 9).Value = "Lỗi";
                                    worksheet.Cell(currentRow, 9).Value = users.ErrorMessage;
                                    worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                }
                            }

                            workbook.SaveAs(file.FullName);
                        }
                    }
                }

                var listEmployeeID = param.Select(e => e.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0')).ToList();

                var listEmployeeDB = new List<HR_User>();
                var listCustomerInfoDB = new List<HR_CustomerInfo>();
                var listUserMasterDB = new List<IC_UserMaster>();
                var listCardNumberDB = new List<HR_CardNumberInfo>();
                var listWorkingInfoDB = new List<IC_WorkingInfo>();

                if (listEmployeeID.Count > 5000)
                {
                    var listSplitEmployeeID = CommonUtils.SplitList(listEmployeeID, 5000);
                    foreach (var listEmployeeSplit in listSplitEmployeeID)
                    {
                        var resultEmployee = _DbContext.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultCustomerInfo = _DbContext.HR_CustomerInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultUserMaster = _DbContext.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultCardNumber = _DbContext.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();

                        listEmployeeDB.AddRange(resultEmployee);
                        listCustomerInfoDB.AddRange(resultCustomerInfo);
                        listUserMasterDB.AddRange(resultUserMaster);
                        listCardNumberDB.AddRange(resultCardNumber);
                        if (userType == (int)EmployeeType.Contractor)
                        {
                            var listWorkingInfo = _DbContext.IC_WorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)
                                && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date
                                && (e.ToDate == null || (e.ToDate.HasValue && e.ToDate.Value.Date > DateTime.Now.Date))).OrderByDescending(e => e.FromDate).ToList();
                            listWorkingInfoDB.AddRange(listWorkingInfo);
                        }

                    }
                }
                else
                {
                    listEmployeeDB = _DbContext.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listCustomerInfoDB = _DbContext.HR_CustomerInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listUserMasterDB = _DbContext.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listCardNumberDB = _DbContext.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();
                    if (userType == (int)EmployeeType.Contractor)
                    {
                        listWorkingInfoDB = _DbContext.IC_WorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)
                            && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date
                            && (e.ToDate == null || (e.ToDate.HasValue && e.ToDate.Value.Date > DateTime.Now.Date))).OrderByDescending(e => e.FromDate).ToList();
                    }
                }

                List<HR_User> listEmployee = new List<HR_User>();
                List<IC_UserMasterDTO> listUserMaster = new List<IC_UserMasterDTO>();

                //var listDepartment = param.GroupBy(e => e.Department).Select(e => e.First()).ToList();
                //List<IC_DepartmentDTO> listDepartmentCreate = listDepartment.Where(e => !string.IsNullOrEmpty(e.Department)).Select(e => new IC_DepartmentDTO
                //{
                //    Name = e.Department.Trim(),
                //    CompanyIndex = user.CompanyIndex,
                //    ParentIndex = 0,
                //    UpdatedDate = DateTime.Now,
                //    UpdatedUser = user.FullName,
                //    CreatedDate = DateTime.Now
                //}).ToList();

                //listDepartmentCreate = _iC_DepartmentLogic.CheckExistedOrCreateList(listDepartmentCreate);

                var departmentContactList = _DbContext.IC_Department.Where(e => param.Select(x => x.ContactDepartment).Contains(e.Name)).ToList();
                foreach (var item in param)
                {
                    try
                    {
                        //var classInfo = new HR_ClassInfo();
                        //classInfo.Index = Guid.NewGuid().ToString();
                        //if (listCreate != null)
                        //{
                        //    classInfo = listCreate.FirstOrDefault(e => !string.IsNullOrWhiteSpace(item.ClassCode) && e.Code.ToLower() == item.ClassCode.ToLower());
                        //    if (classInfo == null)
                        //    {
                        //        classInfo.Index = Guid.NewGuid().ToString();
                        //    }
                        //}

                        //var department = new IC_DepartmentDTO();
                        //department.Index = 0;
                        //if (listDepartmentCreate != null)
                        //{
                        //    department = listDepartmentCreate.FirstOrDefault(e => !string.IsNullOrWhiteSpace(item.Department) && e.Name.ToLower() == item.Department.ToLower());
                        //    if (department == null)
                        //    {
                        //        department = new IC_DepartmentDTO();
                        //        department.Index = 0;
                        //    }
                        //}

                        //var departmentContact = new IC_Department();
                        //if (departmentContactList != null && userType == (int)EmployeeType.Guest)
                        //{
                        //    departmentContact = departmentContactList.FirstOrDefault(e => !string.IsNullOrWhiteSpace(item.ContactDepartment) && e.Name.ToLower() == item.ContactDepartment.ToLower());
                        //    if (departmentContact == null)
                        //    {
                        //        departmentContact = new IC_Department();
                        //        departmentContact.Index = 0;
                        //    }
                        //}

                        item.EmployeeATID = item.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');

                        var existedEmployee = listEmployeeDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedEmployee != null)
                        {
                            existedEmployee.EmployeeCode = item.EmployeeCode;
                            existedEmployee.FullName = item.FullName;
                            existedEmployee.Gender = (short)item.Gender;
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.EmployeeType = userType;
                            existedEmployee.UpdatedUser = user.UserName;
                            existedEmployee.DayOfBirth = StringHelper.GetDateOfBirthEmployee("Day", item.DateOfBirth);
                            existedEmployee.MonthOfBirth = StringHelper.GetDateOfBirthEmployee("Month", item.DateOfBirth);
                            existedEmployee.YearOfBirth = StringHelper.GetDateOfBirthEmployee("Year", item.DateOfBirth);
                            existedEmployee.EmployeeType = userType;
                            _DbContext.HR_User.Update(existedEmployee);
                        }
                        else
                        {
                            existedEmployee = new HR_User();
                            existedEmployee.CompanyIndex = user.CompanyIndex;
                            existedEmployee.EmployeeATID = item.EmployeeATID;
                            existedEmployee.EmployeeCode = item.EmployeeCode;
                            existedEmployee.FullName = item.FullName;
                            existedEmployee.Gender = (short)item.Gender;
                            existedEmployee.EmployeeType = userType;
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            existedEmployee.CreatedDate = DateTime.Now;
                            existedEmployee.DayOfBirth = StringHelper.GetDateOfBirthEmployee("Day", item.DateOfBirth);
                            existedEmployee.MonthOfBirth = StringHelper.GetDateOfBirthEmployee("Month", item.DateOfBirth);
                            existedEmployee.YearOfBirth = StringHelper.GetDateOfBirthEmployee("Year", item.DateOfBirth);
                            _DbContext.HR_User.Add(existedEmployee);
                        }

                        string[] formats = { "dd/MM/yyyy" };
                        var joinedDate = new DateTime();
                        var endDate = new DateTime();
                        var now = DateTime.Now;

                        var convertFromDate = DateTime.TryParseExact(item.JoinedDate, formats,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out joinedDate);

                        var convertEndDate = DateTime.TryParseExact(item.StoppedDate, formats,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out endDate);

                        var startTime = new DateTime();
                        var endTime = new DateTime();
                        var convertStartTime = DateTime.TryParse(item.StartTime, out startTime);
                        var convertEndTime = DateTime.TryParse(item.EndTime, out endTime);

                        var student = listCustomerInfoDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (student != null)
                        {
                            student.Company = item.Company;
                            student.WorkingContent = item.WorkingContent;
                            student.CompanyIndex = user.CompanyIndex;
                            student.UpdatedDate = DateTime.Now;
                            student.UpdatedUser = user.UserName;
                            student.NRIC = item.Nric;
                            student.Address = item.Address;
                            student.Email = item.Email;
                            student.Phone = item.PhoneNumber;
                            student.ContactPerson = item.ContactPerson;
                            student.ContactDepartment = item.ContactDepartmentIndex.ToString();
                            student.IsAllowPhone = item.IsAllowPhone == 1 ? true : false;
                            _DbContext.HR_CustomerInfo.Update(student);
                        }
                        else
                        {

                            student = new HR_CustomerInfo();
                            student.EmployeeATID = item.EmployeeATID;
                            student.NRIC = item.Nric;
                            student.Address = item.Address;
                            student.Email = item.Email;
                            student.Phone = item.PhoneNumber;
                            student.ContactPerson = item.ContactPerson;
                            student.ContactDepartment = item.ContactDepartmentIndex.ToString();
                            student.IsAllowPhone = item.IsAllowPhone == 1 ? true : false;
                            student.Note = item.Note;
                            student.CompanyIndex = user.CompanyIndex;
                            student.UpdatedDate = DateTime.Now;
                            student.UpdatedUser = user.UserName;
                            student.FromTime = new DateTime(joinedDate.Year, joinedDate.Month, joinedDate.Day, startTime.Hour, startTime.Minute, startTime.Second);
                            student.ToTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, endTime.Hour, endTime.Minute, endTime.Second);
                            student.StudentOfParent = item.StudentOfParent;
                            student.Company = item.Company;
                            student.WorkingContent = item.WorkingContent;

                            _DbContext.HR_CustomerInfo.Add(student);
                        }

                        var listOtherCardNumberByEmpID = listCardNumberDB.Where(x => x.EmployeeATID == item.EmployeeATID
                            && x.CardNumber != item.CardNumber).ToList();
                        if (listOtherCardNumberByEmpID.Count > 0)
                        {
                            foreach (var card in listOtherCardNumberByEmpID)
                            {
                                card.IsActive = false;
                                card.UpdatedDate = DateTime.Now;
                                _DbContext.HR_CardNumberInfo.Update(card);
                            }
                        }

                        var existedCardNumber = listCardNumberDB.FirstOrDefault(e => e.CardNumber == item.CardNumber);
                        if (existedCardNumber != null)
                        {
                            if (existedEmployee.EmployeeATID == item.EmployeeATID)
                            {

                            }
                            else
                            {

                            }

                        }
                        else
                        {
                            existedCardNumber = new HR_CardNumberInfo();
                            existedCardNumber.EmployeeATID = existedEmployee.EmployeeATID;
                            existedCardNumber.CompanyIndex = existedEmployee.CompanyIndex;
                            existedCardNumber.CardNumber = item.CardNumber;
                            existedCardNumber.IsActive = true;
                            existedCardNumber.CreatedDate = DateTime.Now;
                            existedCardNumber.UpdatedDate = existedEmployee.UpdatedDate;
                            existedCardNumber.UpdatedUser = existedEmployee.UpdatedUser;
                            _DbContext.HR_CardNumberInfo.Add(existedCardNumber);
                        }


                        var existedUserMaster = listUserMasterDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedUserMaster == null)
                        {
                            existedUserMaster = new IC_UserMaster();
                            existedUserMaster.EmployeeATID = existedEmployee.EmployeeATID;
                            existedUserMaster.CompanyIndex = existedEmployee.CompanyIndex;
                            existedUserMaster.NameOnMachine = item.NameOnMachine;
                            existedUserMaster.CardNumber = item.CardNumber;
                            existedUserMaster.AuthenMode = AuthenMode.FullAccessRight.ToString();
                            existedUserMaster.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.CreatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            _DbContext.IC_UserMaster.Add(existedUserMaster);
                        }
                        else
                        {
                            existedUserMaster.NameOnMachine = item.NameOnMachine;
                            existedUserMaster.CardNumber = item.CardNumber;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            _DbContext.IC_UserMaster.Update(existedUserMaster);
                        }


                        if (userType == (int)EmployeeType.Contractor)
                        {
                            DateTime joined = new DateTime();
                            DateTime? stopped = null;
                            if (!string.IsNullOrWhiteSpace(item.JoinedDate) && convertFromDate)
                            {
                                joined = joinedDate;
                            }
                            if (!string.IsNullOrWhiteSpace(item.StoppedDate) && convertEndDate)
                            {

                                stopped = endDate;
                            }
                            var existedWorkingInfo = listWorkingInfoDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                            if (existedWorkingInfo == null)
                            {
                                existedWorkingInfo = new IC_WorkingInfo();
                                existedWorkingInfo.EmployeeATID = existedEmployee.EmployeeATID;
                                existedWorkingInfo.CompanyIndex = existedEmployee.CompanyIndex;
                                existedWorkingInfo.DepartmentIndex = item.DepartmentIndex;
                                existedWorkingInfo.PositionIndex = item.PositionIndex;
                                existedWorkingInfo.FromDate = joined;
                                existedWorkingInfo.ToDate = stopped;
                                existedWorkingInfo.IsManager = false;
                                existedWorkingInfo.ApprovedDate = DateTime.Now;
                                existedWorkingInfo.UpdatedUser = user.UserName;
                                existedWorkingInfo.Status = (short)TransferStatus.Approve;
                                _DbContext.IC_WorkingInfo.Add(existedWorkingInfo);
                            }
                            else if (existedWorkingInfo.DepartmentIndex == 0 && existedWorkingInfo.DepartmentIndex != item.DepartmentIndex && item.DepartmentIndex != 0)
                            {
                                existedWorkingInfo.DepartmentIndex = item.DepartmentIndex;
                                existedWorkingInfo.PositionIndex = item.PositionIndex;
                                existedWorkingInfo.IsSync = null;
                                existedWorkingInfo.Status = (short)TransferStatus.Approve;
                                existedWorkingInfo.FromDate = joined;
                                existedWorkingInfo.ToDate = stopped;
                                existedWorkingInfo.ApprovedDate = DateTime.Now;
                                existedWorkingInfo.UpdatedUser = user.UserName;
                                existedWorkingInfo.UpdatedDate = DateTime.Now;
                                _DbContext.IC_WorkingInfo.Update(existedWorkingInfo);
                            }
                            else if (existedWorkingInfo.PositionIndex != item.PositionIndex && item.PositionIndex != 0)
                            {
                                existedWorkingInfo.PositionIndex = item.PositionIndex;
                                existedWorkingInfo.Status = (short)TransferStatus.Approve;
                                existedWorkingInfo.ApprovedDate = DateTime.Now;
                                existedWorkingInfo.UpdatedUser = user.UserName;
                                existedWorkingInfo.UpdatedDate = DateTime.Now;
                            }
                            //else if (existedWorkingInfo.FromDate.Date < DateTime.Now.Date && ((existedWorkingInfo.DepartmentIndex > 0 && existedWorkingInfo.DepartmentIndex != department.Index && department.Index != 0)))
                            //{
                            //    var workingInfo = new IC_WorkingInfo();
                            //    workingInfo.EmployeeATID = existedEmployee.EmployeeATID;
                            //    workingInfo.CompanyIndex = existedEmployee.CompanyIndex;
                            //    workingInfo.DepartmentIndex = item.DepartmentIndex;
                            //    workingInfo.FromDate = DateTime.Now;
                            //    workingInfo.IsManager = false;
                            //    workingInfo.ApprovedDate = DateTime.Now;
                            //    workingInfo.UpdatedUser = user.UserName;
                            //    workingInfo.Status = (short)TransferStatus.Approve;
                            //    _DbContext.IC_WorkingInfo.Add(workingInfo);
                            //    //
                            //    existedWorkingInfo.ToDate = DateTime.Now.AddDays(-1);
                            //    existedWorkingInfo.ApprovedDate = DateTime.Now;
                            //    existedWorkingInfo.UpdatedUser = user.UserName;
                            //    existedWorkingInfo.UpdatedDate = DateTime.Now;
                            //    _DbContext.IC_WorkingInfo.Update(existedWorkingInfo);
                            //}
                        }

                    }
                    catch (Exception ex)
                    {
                    }

                    try
                    {
                        _DbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                    }



                    // Add audit log
                    IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                    audit.TableName = "HR_CustomerInfo";
                    audit.UserName = user.UserName;
                    audit.CompanyIndex = user.CompanyIndex;
                    audit.State = AuditType.Added;
                    //audit.Description = AuditType.Added.ToString() + " Import From Excel " + listEmployeeID.Count().ToString() + " HR_CustomerInfo";
                    audit.Description = AuditType.Added.ToString() + "CustomerFromExcel:/:" + listEmployeeID.Count().ToString();
                    audit.DateTime = DateTime.Now;
                    _IIC_AuditLogic.Create(audit);

                    //List<AddedParam> addedParams = new List<AddedParam>();
                    //addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                    //addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER });
                    //var systemconfigs = _IIC_ConfigLogic.GetMany(addedParams);
                    //if (systemconfigs != null)
                    //{
                    //    var sysconfig = systemconfigs.FirstOrDefault();
                    //    if (sysconfig != null)
                    //    {
                    //        if (sysconfig.IntegrateLogParam.AutoIntegrate)
                    //        {
                    //            _IIC_CommandLogic.SyncWithEmployee(listEmployeeID, user.CompanyIndex);
                    //        }
                    //    }
                    //}
                }
               
                var employeeATIDs = param.Select(x => x.EmployeeATID).ToList();
                if (employeeATIDs != null && employeeATIDs.Count > 0)
                {
                    await _IC_VehicleLogService.IntegrateEmployeeToLovad(employeeATIDs);
                    await _IHR_EmployeeLogic.IntegrateUserToOfflineCustomer(employeeATIDs);
                    //Add employee in department AC
                    var employeeInfoList = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeATIDs, DateTime.Now, user.CompanyIndex);
                    var employeeInDepartment = _DbContext.AC_DepartmentAccessedGroup.Where(x => employeeInfoList.Select(z => z.DepartmentIndex).Contains(x.DepartmentIndex)).ToList();
                    if (employeeInDepartment != null && employeeInDepartment.Count > 0)
                    {
                        //var employeeAccessGr = employeeInfoList.Where(x => employeeInDepartment.Select(z => z.DepartmentIndex).Contains(x.DepartmentIndex)).ToList();
                        foreach (var departmentAcc in employeeInDepartment)
                        {
                            var listUserAcc = employeeInfoList.Where(x => x.DepartmentIndex == departmentAcc.DepartmentIndex).Select(x => x.EmployeeATID).ToList();

                            await _iC_CommandService.UploadTimeZone(departmentAcc.GroupIndex, user);
                            await _iC_CommandService.UploadUsers(departmentAcc.GroupIndex, listUserAcc, user);
                            await _iC_CommandService.UploadACUsers(departmentAcc.GroupIndex, listUserAcc, user);
                        }
                    }
                }
         
             

                result = Ok(message);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return ApiError("ImportFromExcelError");
            }
            finally
            {
                if (userType == (short)EmployeeType.Guest && randomIds != null && randomIds.Count > 0 && _usingRandomCustomerID.Any(y
                    => randomIds.Contains(y)))
                {
                    _usingRandomCustomerID.RemoveAll(x => randomIds.Contains(x));
                }
            }
        }

        [Authorize]
        [ActionName("UpdateCustomerCardNumber")]
        [HttpPost]
        public async Task<ActionResult<HR_EmployeeInfoResult>> UpdateCustomerCardNumber([FromBody] HR_CustomerInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var cardActive = await _HR_CardNumberInfoService.CheckCardActiveOtherEmployee(new HR_CardNumberInfo
            { CardNumber = value.CardNumber, CompanyIndex = user.CompanyIndex, EmployeeATID = value.EmployeeATID }, user.CompanyIndex);
            if (cardActive)
            {
                return ApiError("CardNumberExist");
            }

            var c = await _HR_CardNumberInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == value.EmployeeATID
                && x.CompanyIndex == user.CompanyIndex && x.IsActive == true, true);

            try
            {
                c = _Mapper.Map(value, c);

                await _HR_CardNumberInfoService.AddOrUpdateNewCard(c, user);

                await _IHR_EmployeeLogic.IntegrateCardToOffline(new List<string>() { value.EmployeeATID }, null);

                return ApiOk();
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("ReturnOrDeleteCard")]
        [HttpPost]
        public async Task<ActionResult<HR_EmployeeInfoResult>> ReturnOrDeleteCard([FromQuery] string cardNumber)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _HR_CardNumberInfoService.ReturnOrDeleteCard(cardNumber, user);

            return ApiOk(result);
        }

        [Authorize]
        [ActionName("Put_HR_CustomerInfo")]
        [HttpPut("{employeeATID}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Put_HR_CustomerInfo(string employeeATID, [FromBody] HR_CustomerInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            if (!string.IsNullOrEmpty(value.CardNumber) && value.CardNumber != "0")
            {
                var cardActive = await _HR_CardNumberInfoService.CheckCardActiveOtherEmployee(new HR_CardNumberInfo { CardNumber = value.CardNumber, CompanyIndex = user.CompanyIndex, EmployeeATID = value.EmployeeATID }, user.CompanyIndex);
                if (cardActive)
                {
                    return ApiError("CardNumberExist");
                }
            }

            if (!string.IsNullOrWhiteSpace(_configClientName) && _configClientName.ToUpper() == ClientName.MONDELEZ.ToString())
            {
                var dateNow = DateTime.Now;
                var employeeInBlackList = _GC_BlackListService.Any(x => x.Nric == value.NRIC && x.FromDate.Date <= dateNow.Date
                                                                                                                         && (x.ToDate == null || (x.ToDate != null && dateNow.Date <= x.ToDate.Value.Date)));
                if (employeeInBlackList)
                {
                    return ApiError("EmployeeInBlackList");
                }
                var startTimeConvert = !string.IsNullOrWhiteSpace(value.StartTimeStr) ? DateTime.ParseExact(value.StartTimeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) : value.StartTime;
                var endTimeConvert = !string.IsNullOrWhiteSpace(value.EndTimeStr) ? DateTime.ParseExact(value.EndTimeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) : value.EndTime;

                value.FromTime = new DateTime(value.FromTime.Value.Year, value.FromTime.Value.Month, value.FromTime.Value.Day, startTimeConvert.Hour, startTimeConvert.Minute, startTimeConvert.Second);
                value.ToTime = value.ToTime != null ? new DateTime(value.ToTime.Value.Year, value.ToTime.Value.Month, value.ToTime.Value.Day, endTimeConvert.Hour, endTimeConvert.Minute, endTimeConvert.Second) : null;

                //value.FromTime = value.ToTime != null ? new DateTime(value.FromTime.Value.Year, value.FromTime.Value.Month, value.FromTime.Value.Day, value.StartTime.Hour, value.StartTime.Minute, value.StartTime.Second) : DateTime.Now;
                //value.ToTime = value.ToTime != null ? new DateTime(value.ToTime.Value.Year, value.ToTime.Value.Month, value.ToTime.Value.Day, value.EndTime.Hour, value.EndTime.Minute, value.EndTime.Second) : null;
            }

            value.CompanyIndex = user.CompanyIndex;
            var u = await _HR_UserService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex, true);
            var e = await _HR_CustomerInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex, true);
            var us = new IC_UserMasterDTO();
            var c = await _HR_CardNumberInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex && x.IsActive == true, true);
            var w = await _IC_WorkingInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex
                && x.Status == (short)TransferStatus.Approve
                && x.FromDate.Date <= DateTime.Now.Date && (!x.ToDate.HasValue || x.ToDate.Value.Date >= DateTime.Now.Date));
            if (w == null)
            {
                w = await _IC_WorkingInfoService.GetNewestDataByEmployeeATID(user.CompanyIndex, employeeATID);
            }

            BeginTransaction();
            try
            {
                u = _Mapper.Map(value, u);

                us = _Mapper.Map(value, us);
                c = _Mapper.Map(value, c);

                _HR_UserService.Update(u);
                if (e != null)
                {
                    e = _Mapper.Map(value, e);
                    if (e.FromTime == null)
                    {
                        e.FromTime = DateTime.Now;
                    }
                    if (e.IdentityImage != null && e.IdentityImage.Length == 0)
                    {
                        e.IdentityImage = null;
                    }
                    _HR_CustomerInfoService.Update(e);
                }

                if (w != null)
                {
                    w = _Mapper.Map(value, w);
                    _IC_WorkingInfoService.Update(w);
                }

                await _HR_CardNumberInfoService.CheckCardActivedOrCreate(c, user.CompanyIndex);
                await _IIC_UserMasterLogic.SaveAndOverwriteList(new List<IC_UserMasterDTO>() { us });
                await SaveChangeAsync();
                CommitTransaction();

                var employee = await _HR_CustomerInfoService.GetCustomerInfo(u.EmployeeATID, user.CompanyIndex);
                await _IC_VehicleLogService.IntegrateEmployeeToLovad(new List<string>() { value.EmployeeATID });
                await _IHR_EmployeeLogic.IntegrateUserToOfflineCustomer(new List<string>() { value.EmployeeATID });
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("Delete_HR_CustomerInfo")]
        [HttpDelete("{employeeATID}")]
        public async Task<IActionResult> Delete_HR_CustomerInfo(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            BeginTransaction();
            try
            {
                await _HR_CustomerInfoService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await _IC_WorkingInfoService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await _HR_UserService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await _HR_CardNumberInfoService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await _IC_UserMasterService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();


                await _IC_VehicleLogService.DeleteEmployeeToLovad(new List<string> { employeeATID });
                await _IHR_EmployeeLogic.DeleteUserToOffline(new List<string> { employeeATID });

                return ApiOk();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("DeleteCustomerMulti")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCustomerMulti([FromBody] string[] listEmployee)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var empLookup = listEmployee.ToHashSet();

            BeginTransaction();
            try
            {
                await _HR_CustomerInfoService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await _IC_WorkingInfoService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await _HR_UserService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await _HR_CardNumberInfoService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await _IC_UserMasterService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();

                await _IC_VehicleLogService.DeleteEmployeeToLovad(listEmployee.ToList());
                await _IHR_EmployeeLogic.DeleteUserToOffline(listEmployee.ToList());

                return ApiOk();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }


        [Authorize]
        [ActionName("InfoCustomerTemplateImport")]
        [HttpGet]
        public IActionResult InfoCustomerTemplateImport()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            try
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                var departmentList = _context.IC_Department.Where(x => x.IsContractorDepartment != true && x.IsDriverDepartment != true).Select(x => x.Name).OrderByDescending(x => x).ToList();
#if !DEBUG
                var folderDetails = Path.Combine(sWebRootFolder, @"epad/dist/CustomerImport_Guest_Mdl.xlsx");
                using (var workbook = new XLWorkbook(folderDetails))
                {
                    var worksheet = workbook.Worksheets;
                    IXLWorksheet worksheet1;
                    IXLWorksheet worksheet3;
                    var w1 = worksheet.TryGetWorksheet("DepartmentInfo", out worksheet1);
                    worksheet1.Cells().Clear();

                    string startDepartmentCell = "A1";
                    string endDepartmentCell = string.Empty;

                    for (int i = 0; i < departmentList.Count; i++)
                    {
                        if (i == (departmentList.Count - 1))
                        {
                            endDepartmentCell = "A" + (i + 1);
                        }
                        worksheet1.Cell("A" + (i + 1)).Value = departmentList[i];
                    }

                    var w = worksheet.TryGetWorksheet("Data", out worksheet3);
                    worksheet3.Range("K2:K10003").SetDataValidation().List(worksheet1.Range(startDepartmentCell + ":" + endDepartmentCell), true);
                    workbook.Save();
                }
#endif



                return ApiOk();
            }
            catch (Exception ex)
            {
                _logger.LogError("InfoCustomerTemplateImport: ", ex);
                return ApiOk();
            }

        }

        [Authorize]
        [ActionName("InfoContractorTemplateImport")]
        [HttpGet]
        public IActionResult InfoContractorTemplateImport()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            try
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                //Use local 
                var departmentList = _context.IC_Department.Where(x => x.IsContractorDepartment == true).Select(x => x.Name).OrderByDescending(x => x).ToList();
                var positionList = _context.HR_PositionInfo.Select(x => x.Name).OrderByDescending(x => x).ToList();
#if !DEBUG
                var folderDetails = Path.Combine(sWebRootFolder, @"epad/dist/CustomerImport_Contractor_Mdl.xlsx");
                using (var workbook = new XLWorkbook(folderDetails))
                {
                    var worksheet = workbook.Worksheets;
                    IXLWorksheet worksheet1;
                    IXLWorksheet worksheet2;
                    IXLWorksheet worksheet3;
                    var w1 = worksheet.TryGetWorksheet("DepartmentInfo", out worksheet1);
                    var w2 = worksheet.TryGetWorksheet("PositionInfo", out worksheet2);
                    worksheet1.Cells().Clear();
                    worksheet2.Cells().Clear();

                    string startDepartmentCell = "A1";
                    string endDepartmentCell = string.Empty;

                    for (int i = 0; i < departmentList.Count; i++)
                    {
                        if (i == (departmentList.Count - 1))
                        {
                            endDepartmentCell = "A" + (i + 1);
                        }
                        worksheet1.Cell("A" + (i + 1)).Value = departmentList[i];
                    }

                    string startPositionCell = "A1";
                    string endPositionCell = string.Empty;

                    for (int i = 0; i < positionList.Count; i++)
                    {
                        if (i == (positionList.Count - 1))
                        {
                            endPositionCell = "A" + (i + 1);
                        }
                        worksheet2.Cell("A" + (i + 1)).Value = positionList[i];
                    }

                    var w = worksheet.TryGetWorksheet("Data", out worksheet3);
                    worksheet3.Range("K2:K10003").SetDataValidation().List(worksheet1.Range(startDepartmentCell + ":" + endDepartmentCell), true);

                    worksheet3.Range("L2:L10003").SetDataValidation().List(worksheet2.Range(startPositionCell + ":" + endPositionCell), true);
                    workbook.Save();
                }
#endif

                return ApiOk();
            }
            catch (Exception ex)
            {
                _logger.LogError("InfoContractorTemplateImport", ex);
                return ApiOk();
            }

        }


        [ActionName("ImportDataFromGoogleSheet")]
        [HttpGet]
        public async Task<IActionResult> ImportDataFromGoogleSheet()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var randomIds = await _HR_CustomerInfoService.ImportDataFromGoogleSheet(user.CompanyIndex);
            return ApiOk(randomIds);
        }
    }
}
