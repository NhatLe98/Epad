using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using EPAD_Data.Entities.HR;
using IHR_EmployeeInfoService = EPAD_Services.Interface.IHR_EmployeeInfoService;
using EPAD_Data.Models.HR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;

namespace EPAD_Backend_Core.Controllers.HR
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HR_EmployeeInfoController : ApiControllerBase
    {
        private readonly IHR_EmployeeInfoService _HR_EmployeeInfoService;
        private readonly IIC_DepartmentService _IC_DepartmentService;
        private readonly IHR_UserService _HR_UserService;
        private readonly IHR_CardNumberInfoService _HR_CardNumberInfoService;
        private readonly IIC_WorkingInfoService _IC_WorkingInfoService;
        private readonly IIC_UserMasterService _IC_UserMasterService;
        private readonly IIC_EmployeeTransferService _IC_EmployeeTransferService;
        private readonly IIC_UserMasterLogic _IIC_UserMasterLogic;
        private readonly IHR_UserContactInfoService _HR_UserContactInfoService;
        private readonly IIC_CommandLogic _iC_CommandLogic;
        private readonly IIC_CommandService _iC_CommandService;
        private readonly IIC_ScheduleAutoHostedLogic _iC_ScheduleAutoHostedLogic;
        private readonly IGC_BlackListService _GC_BlackListService;
        private readonly IHR_EmployeeLogic _IHR_EmployeeLogic;
        private readonly ILogger _logger;
        protected readonly ILoggerFactory _LoggerFactory;
        private readonly string _configClientName;
        private string mLinkECMSApi;
        private string mCommunicateToken;
        private readonly EPAD_Context context;
        private readonly ezHR_Context otherContext;
        private readonly IMemoryCache cache;
        private readonly IIC_VehicleLogService _IC_VehicleLogService;
        protected readonly IIC_AuditLogic _IIC_AuditLogic;
        private readonly IIC_UserAuditService _IIC_UserAuditService;

        public HR_EmployeeInfoController(IServiceProvider pProvider, ILoggerFactory _loggerFactory) : base(pProvider)
        {
            _HR_UserService = TryResolve<IHR_UserService>();
            _IC_DepartmentService = TryResolve<IIC_DepartmentService>();
            _HR_EmployeeInfoService = TryResolve<IHR_EmployeeInfoService>();
            _HR_CardNumberInfoService = TryResolve<IHR_CardNumberInfoService>();
            _IC_WorkingInfoService = TryResolve<IIC_WorkingInfoService>();
            _IC_UserMasterService = TryResolve<IIC_UserMasterService>();
            _IC_EmployeeTransferService = TryResolve<IIC_EmployeeTransferService>();
            _IIC_UserMasterLogic = TryResolve<IIC_UserMasterLogic>();
            _HR_UserContactInfoService = TryResolve<IHR_UserContactInfoService>();
            _iC_CommandLogic = TryResolve<IIC_CommandLogic>();
            _iC_ScheduleAutoHostedLogic = TryResolve<IIC_ScheduleAutoHostedLogic>();
            _GC_BlackListService = TryResolve<IGC_BlackListService>();
            _logger = _loggerFactory.CreateLogger<HR_EmployeeInfoController>();
            _configClientName = _Configuration.GetValue<string>("ClientName").ToUpper();
            mLinkECMSApi = _Configuration.GetValue<string>("ECMSApi");
            mCommunicateToken = _Configuration.GetValue<string>("CommunicateToken");
            otherContext = TryResolve<ezHR_Context>();
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _IC_VehicleLogService = TryResolve<IIC_VehicleLogService>();
            _iC_CommandService = TryResolve<IIC_CommandService>();
            _IHR_EmployeeLogic = TryResolve<IHR_EmployeeLogic>();
            _IIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _IIC_UserAuditService = TryResolve<IIC_UserAuditService>();
        }

        [Authorize]
        [ActionName("GetEmployeesAtPage")]
        [HttpPost]
        public async Task<ActionResult<List<HR_EmployeeInfoResult>>> GetEmployeesAtPage([FromBody] HR_EmployeeInfoRequest employeeInfoRequest)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var addedParams = new List<AddedParam>();
            if (employeeInfoRequest.DepartmentIDs != null && employeeInfoRequest.DepartmentIDs.Count() > 0)
            {
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = employeeInfoRequest.DepartmentIDs });
            }
            else
            {
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
            }
            addedParams.Add(new AddedParam { Key = "Filter", Value = employeeInfoRequest.Filter });
            addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = employeeInfoRequest.ListWorkingStatus });
            addedParams.Add(new AddedParam { Key = "PageIndex", Value = employeeInfoRequest.Page });
            addedParams.Add(new AddedParam { Key = "PageSize", Value = employeeInfoRequest.PageSize });
            addedParams.Add(new AddedParam { Key = "FromDate", Value = employeeInfoRequest.FromDate });
            addedParams.Add(new AddedParam { Key = "ToDate", Value = employeeInfoRequest.ToDate });
            addedParams.Add(new AddedParam { Key = "UserType", Value = employeeInfoRequest.UserType });
            addedParams.Add(new AddedParam { Key = "ApproveStatus", Value = (long)TransferStatus.Approve });
            var allEmployee = await _HR_EmployeeInfoService.GetPage(addedParams, user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        // GET: api/<HR_EmployeeInfoController>
        [Authorize]
        [ActionName("GetEmployeeAtPage")]
        [HttpGet]
        public async Task<ActionResult<List<HR_EmployeeInfoResult>>> Get([FromQuery] string filter, [FromQuery] long[] d, [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] int? userType, [FromQuery] List<int> listWorkingStatus)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            //var allEmployee = await _HR_EmployeeInfoService.GetDataGrid("", d , user.CompanyIndex, page, pageSize);

            var addedParams = new List<AddedParam>();
            if (d != null && d.Count() > 0)
            {
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = d });
            }
            else
            {
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
            }
            addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
            addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = listWorkingStatus });
            addedParams.Add(new AddedParam { Key = "PageIndex", Value = page });
            addedParams.Add(new AddedParam { Key = "PageSize", Value = pageSize });
            addedParams.Add(new AddedParam { Key = "UserType", Value = userType });
            addedParams.Add(new AddedParam { Key = "ApproveStatus", Value = (long)TransferStatus.Approve });
            var allEmployee = await _HR_EmployeeInfoService.GetPage(addedParams, user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Get_HR_EmployeeInfos")]
        [HttpGet]
        public async Task<ActionResult<List<HR_EmployeeInfoResult>>> Get_HR_EmployeeInfos()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_EmployeeInfoService.GetAllEmployeeInfo(new string[0], user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("GetEmployeeInfoByUserDepartment")]
        [HttpGet]
        public async Task<ActionResult<List<HR_EmployeeInfoResult>>> GetEmployeeInfoByUserDepartment()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_EmployeeInfoService.GetEmployeeInfoByDepartment(user.ListDepartmentAssigned.ToList(), user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("GetEmployeeInfoByDepartment")]
        [HttpGet]
        public async Task<ActionResult<List<HR_EmployeeInfoResult>>> GetEmployeeInfoByDepartment(long departmentIndex)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_EmployeeInfoService.GetEmployeeInfoByDepartment(new List<long> { departmentIndex }, user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("GetEmployeeInfoByUserRootDepartment")]
        [HttpGet]
        public async Task<ActionResult<List<HR_EmployeeInfoResult>>> GetEmployeeInfoByUserRootDepartment()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_EmployeeInfoService.GetEmployeeInfoByDepartment(user.ListDepartmentAssigned.ToList(), user.CompanyIndex);
            var listEmployeeDepartmentIndex = allEmployee.Select(x => x.DepartmentIndex).ToList();
            var listActiveDepartment = await _IC_DepartmentService.GetActiveDepartment();
            var listEmployeeRootDepartment = listActiveDepartment.Where(x => !x.ParentIndex.HasValue || x.ParentIndex == 0).ToList();
            foreach (var employee in allEmployee)
            {
                if (employee.DepartmentIndex != 0 && !listEmployeeRootDepartment.Any(y => y.Index == employee.DepartmentIndex))
                {
                    var rootDepartmentIndex = FindRootDepartmentIndex(employee.DepartmentIndex, listActiveDepartment);
                    if (rootDepartmentIndex != employee.DepartmentIndex)
                    {
                        employee.DepartmentIndex = rootDepartmentIndex;
                        employee.DepartmentName = listActiveDepartment.FirstOrDefault(x => x.Index == rootDepartmentIndex)?.Name
                            ?? string.Empty;
                    }
                }
            }

            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("GetEmployeeInfoByRootDepartment")]
        [HttpGet]
        public async Task<ActionResult<List<HR_EmployeeInfoResult>>> GetEmployeeInfoByRootDepartment(long departmentIndex)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_EmployeeInfoService.GetEmployeeInfoByDepartment(user.ListDepartmentAssigned.ToList(), user.CompanyIndex);
            var listEmployeeDepartmentIndex = allEmployee.Select(x => x.DepartmentIndex).ToList();
            var listActiveDepartment = await _IC_DepartmentService.GetActiveDepartment();
            var listEmployeeRootDepartment = listActiveDepartment.Where(x => !x.ParentIndex.HasValue || x.ParentIndex == 0).ToList();
            foreach (var employee in allEmployee)
            {
                if (employee.DepartmentIndex != 0 && !listEmployeeRootDepartment.Any(y => y.Index == employee.DepartmentIndex))
                {
                    var rootDepartmentIndex = FindRootDepartmentIndex(employee.DepartmentIndex, listActiveDepartment);
                    if (rootDepartmentIndex != employee.DepartmentIndex)
                    {
                        employee.DepartmentIndex = rootDepartmentIndex;
                        employee.DepartmentName = listActiveDepartment.FirstOrDefault(x => x.Index == rootDepartmentIndex)?.Name
                            ?? string.Empty;
                    }
                }
            }

            allEmployee = allEmployee.Where(x => x.DepartmentIndex == departmentIndex).ToList();

            return allEmployee;
        }

        private long FindRootDepartmentIndex(long departmentIndex, List<IC_Department> listDepartment)
        {
            var result = departmentIndex;
            var department = listDepartment.FirstOrDefault(x => x.Index == departmentIndex);
            if (department != null && department.ParentIndex != null && department.ParentIndex > 0
                && department.ParentIndex != department.Index)
            {
                result = FindRootDepartmentIndex(department.ParentIndex.Value, listDepartment);
            }

            return result;
        }

        // This API is use only for VStar
        [Authorize]
        [ActionName("Get_HR_EmployeeInfoList")]
        [HttpGet]
        public async Task<ActionResult<List<VStarEmployeeInfoResult>>> Get_HR_EmployeeInfoList()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = new List<VStarEmployeeInfoResult>();
            if (_configClientName == ClientName.VSTAR.ToString())
            {
                allEmployee = await _HR_EmployeeInfoService.GetAllEmployeeInfoVStar(new string[0], user.CompanyIndex);
            }
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Get_HR_EmployeeInfoListExtend")]
        [HttpGet]
        public async Task<ActionResult<List<VStarEmployeeInfoResult>>> Get_HR_EmployeeInfoListExtend(long type)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = new List<VStarEmployeeInfoResult>();
            if (_configClientName == ClientName.VSTAR.ToString())
            {
                allEmployee = await _HR_EmployeeInfoService.GetAllEmployeeInfoVStarExtend(new string[0], user.CompanyIndex, type);
            }
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("GetEmployeeInfoByIDs")]
        [HttpPost]
        public async Task<ActionResult<List<EmployeeInfoResponse>>> GetEmployeeInfoByIDs([FromBody] string employeeIDs)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employeeIds = employeeIDs.Split(',').ToArray();
            var allEmployee = await _HR_EmployeeInfoService.GetEmployeeInfoByIDs(employeeIds, user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("GetAllEmployeeInfoByIds")]
        [HttpPost]
        public async Task<ActionResult<List<HR_EmployeeInfoResult>>> GetAllEmployeeInfoByIds([FromBody] List<string> ids)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            string[] employeeIds = ids.ToArray();
            var allEmployee = await _HR_EmployeeInfoService.GetAllEmployeeInfo(employeeIds, user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("GetEmployeeInfoByListId")]
        [HttpPost]
        public async Task<ActionResult<List<HR_EmployeeInfoResult>>> GetEmployeeInfoByListId([FromBody] List<string> ids)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            string[] employeeIds = ids.ToArray();
            var allEmployee = await _HR_EmployeeInfoService.GetEmployeeInfoByIds(employeeIds, user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        // GET api/<HR_EmployeeInfoController>/5
        [Authorize]
        [ActionName("Get_HR_EmployeeInfo")]
        [HttpGet("{employeeATID}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Get_HR_EmployeeInfo(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employee = await _HR_EmployeeInfoService.GetEmployeeInfo(employeeATID, user.CompanyIndex);
            return ApiOk(employee);
        }

        [Authorize]
        [ActionName("GetEmployeeInfoByIdAndCode")]
        [HttpGet]
        public async Task<ActionResult<HR_EmployeeInfoResult>> GetEmployeeInfoByIdAndCode(string id, string code)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employee = await _HR_EmployeeInfoService.GetEmployeeInfo(id, user.CompanyIndex, code);
            return ApiOk(employee);
        }

        // POST api/<HR_EmployeeInfoController>
        [Authorize]
        [ActionName("Post_HR_EmployeeInfo")]
        [HttpPost]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Post_HR_EmployeeInfo([FromBody] HR_EmployeeInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            if (!value.Gender.HasValue)
                value.Gender = (short)GenderEnum.Other;

            value.EmployeeATID = value.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0');

            var u = _Mapper.Map<HR_EmployeeInfoResult, HR_User>(value);
            if (_configClientName == ClientName.MAY.ToString() && value.PositionIndex != 0)
            {
                u.EmployeeType = int.Parse(value.PositionIndex.ToString());
            }
            else
            {
                u.EmployeeType = (int)EmployeeType.Employee;
            }

            var employeeInBlackList = _GC_BlackListService.Any(x => ((!string.IsNullOrWhiteSpace(value.EmployeeATID) && value.EmployeeATID == x.EmployeeATID) || (!string.IsNullOrEmpty(value.Nric) && x.Nric == value.Nric))
                                                                                                                      && x.FromDate.Date <= value.FromDate.Value.Date
                                                                                                                      && (x.ToDate == null || (x.ToDate != null && value.FromDate.Value.Date.Date <= x.ToDate.Value.Date)));
            if (employeeInBlackList)
            {
                return ApiError("EmployeeInBlackList");
            }

            var e = _Mapper.Map<HR_EmployeeInfoResult, HR_EmployeeInfo>(value);
            // card info
            var cardActive = await _HR_CardNumberInfoService.CheckCardActiveOtherEmployee(new HR_CardNumberInfo { CardNumber = value.CardNumber, CompanyIndex = user.CompanyIndex, EmployeeATID = value.EmployeeATID }, user.CompanyIndex);
            if (cardActive)
                return ApiError("CardNumberExist");

            var c = _Mapper.Map<HR_EmployeeInfoResult, HR_CardNumberInfo>(value);
            c.IsActive = true;

            // Working info
            var wk = new IC_WorkingInfo()
            {
                EmployeeATID = value.EmployeeATID,
                FromDate = value.FromDate ?? value.JoinedDate ?? DateTime.Now,
                PositionIndex = _configClientName == ClientName.MAY.ToString() ? 0 : value.PositionIndex,
                ToDate = value.ToDate,
                DepartmentIndex = value.DepartmentIndex,
                Status = (short)TransferStatus.Approve,
                ApprovedDate = DateTime.Now
            };
            // User master
            var us = new IC_UserMasterDTO
            {
                EmployeeATID = value.EmployeeATID,
                NameOnMachine = value.NameOnMachine,
                CompanyIndex = user.CompanyIndex,
                Password = value.Password,
                CreatedDate = DateTime.Now,
                FingerData0 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 0),
                FingerData1 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 1),
                FingerData2 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 2),
                FingerData3 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 3),
                FingerData4 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 4),
                FingerData5 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 5),
                FingerData6 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 6),
                FingerData7 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 7),
                FingerData8 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 8),
                FingerData9 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 9),
            };

            var check = _HR_UserService.Where(x => x.CompanyIndex == user.CompanyIndex && (value.EmployeeATID.Contains(x.EmployeeATID) || x.EmployeeATID == value.EmployeeATID || x.EmployeeATID == value.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0')
                || x.EmployeeATID == value.EmployeeATID.TrimStart(new Char[] { '0' }) || x.EmployeeATID.Contains(value.EmployeeATID))).ToList();

            check = check.Where(x => (x.EmployeeATID.EndsWith(value.EmployeeATID) && x.EmployeeATID.Replace(value.EmployeeATID, "0").All(x => x == '0'))
                || x.EmployeeATID.TrimStart(new Char[] { '0' }) == value.EmployeeATID.TrimStart(new Char[] { '0' })).ToList();

            if (check != null && check.Count > 0)
                return ApiError("EmployeeATIDExist");

            BeginTransaction();
            try
            {
                await _HR_UserService.InsertAsync(u);
                await _HR_EmployeeInfoService.InsertAsync(e);
                await _HR_CardNumberInfoService.CheckCardActivedOrCreate(c, user.CompanyIndex);
                await _IC_WorkingInfoService.InsertAsync(wk);
                //_HR_UserContactInfoService.Insert(listInfo);

                _IIC_UserMasterLogic.CheckExistedOrCreate(us);
                await SaveChangeAsync();

                //Add ContactInfo
                var contactInfo = value.HR_ContactInfo.ToList();
                var listInfo = new List<HR_UserContactInfo>();
                foreach (var p in contactInfo)
                {
                    var info = new HR_UserContactInfo()
                    {
                        UserIndex = value.EmployeeATID,
                        Name = p.Name,
                        Phone = p.Phone,
                        Email = p.Email,
                        UpdatedUser = user.UserName,
                        UpdatedDate = DateTime.Now,
                        CompanyIndex = user.CompanyIndex
                    };
                    listInfo.Add(info);
                }
                _DbContext.HR_UserContactInfo.AddRange(listInfo);

                _DbContext.SaveChanges();
                CommitTransaction();

                var addOrDeleteUserConfig = _DbContext.IC_Config.FirstOrDefault(t => t.CompanyIndex == user.CompanyIndex
                    && t.EventType == ConfigAuto.ADD_OR_DELETE_USER.ToString());
                if (addOrDeleteUserConfig != null && !string.IsNullOrWhiteSpace(addOrDeleteUserConfig.CustomField))
                {
                    var param = JsonConvert.DeserializeObject<IntegrateLogParam>(addOrDeleteUserConfig.CustomField);
                    var isAutoSync = param.AutoIntegrate;
                    if (isAutoSync)
                    {
                        wk.IsSync = false;
                        var commandResult = _iC_ScheduleAutoHostedLogic.CreateUploadUserCommand(user.CompanyIndex, value.DepartmentIndex,
                            value.EmployeeATID, wk.FromDate, wk.ToDate ?? DateTime.Now, wk.Index.ToString(), false);
                        if (commandResult != null)
                        {
                            IC_GroupCommandParamDTO grouComParam = new IC_GroupCommandParamDTO();
                            grouComParam.CompanyIndex = user.CompanyIndex;
                            grouComParam.ListCommand = commandResult;
                            grouComParam.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                            grouComParam.GroupName = GroupName.TransferEmployee.ToString();
                            grouComParam.EventType = CommandAction.UploadUsers.ToString();
                            _iC_CommandLogic.CreateGroupCommands(grouComParam);
                        }
                    }
                }
                _DbContext.SaveChanges();
                if (_configClientName == ClientName.AVN.ToString() || _configClientName == ClientName.AEON.ToString())
                {
                    await AutoActiveESSAccount();
                }

                var employee = await _HR_EmployeeInfoService.GetEmployeeInfo(u.EmployeeATID, user.CompanyIndex);

                await _IC_VehicleLogService.IntegrateEmployeeToLovad(new List<string>() { value.EmployeeATID });
                await _IHR_EmployeeLogic.IntegrateUserToOfflineEmployee(new List<string>() { value.EmployeeATID });
                await _IIC_UserAuditService.InsertAudit(new List<string>() { value.EmployeeATID });

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
        }


        private async Task AutoActiveESSAccount()
        {
            var client = new HttpClient();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, mLinkECMSApi + "/api/IC_UserAccountESS/ActiveAccountESSFromIntegrate");
                request.Headers.Add("api-token", mCommunicateToken);
                client.Timeout = TimeSpan.FromMinutes(10);
                var responseMessageGetData = await client.SendAsync(request);
                responseMessageGetData.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"AutoActiveESSAccount: {mLinkECMSApi} {ex}");
            }
        }

        // PUT api/<HR_EmployeeInfoController>/5
        [Authorize]
        [ActionName("Put_HR_EmployeeInfo")]
        [HttpPut("{employeeATID}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Put_HR_EmployeeInfo(string employeeATID,
            [FromBody] HR_EmployeeInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var cardActive = await _HR_CardNumberInfoService.CheckCardActiveOtherEmployee(new HR_CardNumberInfo { CardNumber = value.CardNumber, CompanyIndex = user.CompanyIndex, EmployeeATID = value.EmployeeATID }, user.CompanyIndex);
            if (cardActive)
                return ApiError("CardNumberExist");

            var dateNow = DateTime.Now;
            var employeeInBlackList = _GC_BlackListService.Any(x => x.Nric == value.Nric && x.FromDate.Date <= dateNow.Date
                                                                                                                     && (x.ToDate == null || (x.ToDate != null && dateNow.Date <= x.ToDate.Value.Date)));
            if (employeeInBlackList)
            {
                return ApiError("EmployeeInBlackList");
            }
            var u = await _HR_UserService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex, true);
            var e = await _HR_EmployeeInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex, true);
            //var w = await _IC_WorkingInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex && x.Status == (short)TransferStatus.Approve
            //    && x.FromDate.Date <= DateTime.Now.Date && (!x.ToDate.HasValue || x.ToDate.Value.Date >= DateTime.Now.Date));
            //var w = await _IC_WorkingInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex 
            //    && x.Status == (short)TransferStatus.Approve && x.FromDate.Date <= DateTime.Now.Date);
            var w = await _IC_WorkingInfoService.FirstOrDefaultAsync(x => x.Index == value.WorkingInfoIndex);
            var um = new IC_UserMasterDTO();
            var c = await _HR_CardNumberInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex && x.IsActive == true, true);

            BeginTransaction();
            try
            {
                var userType = u.EmployeeType;
                //u = _Mapper.Map(value, u);
                u = _Mapper.Map<HR_EmployeeInfoResult, HR_User>(value);
                e = _Mapper.Map(value, e);
                w = _Mapper.Map(value, w);
                if (_configClientName == ClientName.MAY.ToString())
                {
                    w.PositionIndex = 0;
                }
                um = _Mapper.Map(value, um);
                c = _Mapper.Map(value, c);

                um.FingerData0 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 0);
                um.FingerData1 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 1);
                um.FingerData2 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 2);
                um.FingerData3 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 3);
                um.FingerData4 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 4);
                um.FingerData5 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 5);
                um.FingerData6 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 6);
                um.FingerData7 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 7);
                um.FingerData8 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 8);
                um.FingerData9 = _IIC_UserMasterLogic.GetFingerDataListString(value.ListFinger, 9);

                //No need to change the type of data
                u.EmployeeType = userType;
                _HR_UserService.Update(u);
                _HR_EmployeeInfoService.Update(e);
                _IC_WorkingInfoService.Update(w);
                await _HR_CardNumberInfoService.CheckCardActivedOrCreate(c, user.CompanyIndex);
                await _IIC_UserMasterLogic.SaveAndOverwriteList(new List<IC_UserMasterDTO>() { um });
                await _HR_UserContactInfoService.DeleteAsync(x => x.UserIndex == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();

                //Add ContactInfo
                var contactInfo = value.HR_ContactInfo.ToList();
                var listInfo = new List<HR_UserContactInfo>();
                foreach (var p in contactInfo)
                {
                    var info = new HR_UserContactInfo()
                    {
                        UserIndex = value.EmployeeATID,
                        Name = p.Name,
                        Phone = p.Phone,
                        Email = p.Email,
                        UpdatedUser = user.UserName,
                        UpdatedDate = DateTime.Now,
                        CompanyIndex = user.CompanyIndex
                    };
                    listInfo.Add(info);
                }
                _DbContext.HR_UserContactInfo.AddRange(listInfo);
                _DbContext.SaveChanges();
                CommitTransaction();

                var employee = await _HR_EmployeeInfoService.GetEmployeeInfo(u.EmployeeATID, user.CompanyIndex);
                await _IC_VehicleLogService.IntegrateEmployeeToLovad(new List<string>() { value.EmployeeATID });
                await _IHR_EmployeeLogic.IntegrateUserToOfflineEmployee(new List<string>() { value.EmployeeATID });
                await _IIC_UserAuditService.InsertAudit(new List<string>() { value.EmployeeATID });
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        // DELETE api/<HR_EmployeeInfoController>/5
        [Authorize]
        [ActionName("Delete_HR_EmployeeInfo")]
        [HttpDelete("{employeeATID}")]
        public async Task<IActionResult> Delete_HR_EmployeeInfo(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            BeginTransaction();
            try
            {
                await _HR_EmployeeInfoService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await _HR_UserService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await _HR_CardNumberInfoService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await _IC_WorkingInfoService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await _IC_EmployeeTransferService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await _IC_UserMasterService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();
                await _IC_VehicleLogService.DeleteEmployeeToLovad(new List<string> { employeeATID });
                await _IHR_EmployeeLogic.DeleteUserToOffline(new List<string>() { employeeATID });
                await _IIC_UserAuditService.DeleteAudit(new List<string>() { employeeATID });
                return ApiOk();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("DeleteEmployeeMulti")]
        [HttpDelete]
        public async Task<IActionResult> DeleteEmployeeMulti([FromBody] string[] listEmployee)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var empLookup = listEmployee.ToHashSet();

            BeginTransaction();
            try
            {
                await _HR_EmployeeInfoService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await _HR_UserService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await _HR_CardNumberInfoService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await _IC_WorkingInfoService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await _IC_EmployeeTransferService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await _IC_UserMasterService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await _HR_UserContactInfoService.DeleteAsync(x => empLookup.Contains(x.UserIndex) && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();

                await _IC_VehicleLogService.DeleteEmployeeToLovad(listEmployee.ToList());
                await _IHR_EmployeeLogic.DeleteUserToOffline(listEmployee.ToList());
                await _IIC_UserAuditService.DeleteAudit(listEmployee.ToList());

                #region Demo save history
                //Add history
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "Quản lý người dùng";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Deleted;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + listEmployeeID.Count().ToString() + " HR_CustomerInfo";
                audit.Description = String.Join(", ", empLookup.ToList());
                audit.DateTime = DateTime.Now;
                _IIC_AuditLogic.Create(audit);
                #endregion
                return ApiOk();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("GetUserContactInfoById")]
        [HttpGet]
        public async Task<IActionResult> GetUserContactInfoById(string id)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employee = await _HR_UserContactInfoService.GetUserContactInfoById(id);
            return ApiOk(employee);
        }
    }
}
