using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Common.Extensions;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic.MainProcess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EPAD_Logic;
using EPAD_Data.Models.IC;
using Microsoft.EntityFrameworkCore;
using EPAD_Services.Interface;
using EPAD_Backend_Core.Base;
using IHR_EmployeeInfoService = EPAD_Services.Interface.IHR_EmployeeInfoService;
using Newtonsoft.Json;
using System.Timers;
using System.Threading;
using EPAD_Backend_Core.ApiClient;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class V1Controller : ApiControllerBase
    {
        #region Contructor and Properties
        private EPAD_Context context;
        private IMemoryCache cache;
        private readonly ILogger _logger;
        private string mCommunicateToken;
        private string _linkControllerApi;
        private string _linkStreamCameraFormat;
        private int esdMinuteLooping;
        private readonly IIC_DeviceService _IC_DeviceService;
        private readonly IIC_AttendanceLogService _IC_AttendanceLogService;
        private IIC_SystemCommandLogic _iC_SystemCommandLogic;
        private IIC_UserMasterLogic _iC_UserMasterLogic;
        private IIC_WorkingInfoLogic _IIC_WorkingInfoLogic;
        private IIC_CommandLogic _IIC_CommandLogic;
        private IIC_DepartmentService _IC_DepartmentService;
        private IIC_DepartmentLogic _iIC_DepartmentLogic;
        private ezHR_Context otherContext;
        private readonly IIC_WorkingInfoService _IC_WorkingInfoService;
        private IIC_EmployeeLogic _IIC_EmployeeLogic;
        private IHR_EmployeeLogic _IHR_EmployeeLogic;
        private IIC_ModbusReplayControllerLogic _IIC_ModbusReplayControllerLogic;
        private IIC_ClientTCPControllerLogic _IIC_ClientTCPControllerLogic;
        private readonly IHR_CardNumberInfoService _HR_CardNumberInfoService;
        private readonly IHR_UserService _HR_UserService;
        private readonly IIC_EmployeeTransferService _IC_EmployeeTransferService;
        private readonly IHR_EmployeeInfoService _HR_EmployeeInfoService;
        private readonly IIC_UserMasterService _IC_UserMasterService;
        private readonly IHR_CustomerInfoService _HR_CustomerInfoService;
        private readonly IIC_UserMasterLogic _IIC_UserMasterLogic;
        private readonly IConfiguration _configuration;
        private readonly PushNotificationClient _notificationClient;
        private string _clientName;

        public V1Controller(IConfiguration configuration, EPAD_Context pContext, IMemoryCache pCache, ILoggerFactory loggerFactory, IIC_SystemCommandLogic iC_SystemCommandLogic, IIC_CommandLogic iIC_CommandLogic, IIC_WorkingInfoLogic iIC_WorkingInfoLogic, IIC_DepartmentLogic iC_DepartmentLogic, ezHR_Context pOtherContext,
            IIC_EmployeeLogic iIC_EmployeeLogic, IHR_EmployeeLogic iHR_EmployeeLogic, IIC_ModbusReplayControllerLogic iIIC_ModbusReplayControllerLogic, IIC_ClientTCPControllerLogic iIC_ClientTCPControllerLogic,
            IIC_UserMasterLogic iC_UserMasterLogic, IServiceProvider provider) : base(provider)
        {
            context = pContext;
            cache = pCache;
            _HR_CustomerInfoService = TryResolve<IHR_CustomerInfoService>();
            _IIC_UserMasterLogic = TryResolve<IIC_UserMasterLogic>();
            _IC_DeviceService = TryResolve<IIC_DeviceService>();
            _IC_AttendanceLogService = TryResolve<IIC_AttendanceLogService>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _IC_EmployeeTransferService = TryResolve<IIC_EmployeeTransferService>();
            _IC_DepartmentService = TryResolve<IIC_DepartmentService>();
            _IC_WorkingInfoService = TryResolve<IIC_WorkingInfoService>();
            _HR_EmployeeInfoService = TryResolve<IHR_EmployeeInfoService>();
            _IC_UserMasterService = TryResolve<IIC_UserMasterService>();
            _HR_CardNumberInfoService = TryResolve<IHR_CardNumberInfoService>();
            mCommunicateToken = configuration.GetValue<string>("CommunicateToken");
            _linkControllerApi = configuration.GetValue<string>("ControllerApi");
            _linkStreamCameraFormat = configuration.GetValue<string>("LinkStreamCameraFormat");
            _logger = loggerFactory.CreateLogger<V1Controller>();
            _iC_SystemCommandLogic = iC_SystemCommandLogic;
            _iC_UserMasterLogic = iC_UserMasterLogic;
            _IIC_WorkingInfoLogic = iIC_WorkingInfoLogic;
            _IIC_CommandLogic = iIC_CommandLogic;
            _iIC_DepartmentLogic = iC_DepartmentLogic;
            otherContext = pOtherContext;
            _IIC_EmployeeLogic = iIC_EmployeeLogic;
            _IHR_EmployeeLogic = iHR_EmployeeLogic;
            _IIC_ModbusReplayControllerLogic = iIIC_ModbusReplayControllerLogic;
            _IIC_ClientTCPControllerLogic = iIC_ClientTCPControllerLogic;
            esdMinuteLooping = configuration.GetValue<int>("ESDTimeLooping");
            _notificationClient = PushNotificationClient.GetInstance(cache);
            _clientName = _Configuration.GetValue<string>("ClientName").ToUpper();
        }
        #endregion

        #region Mondelez
        [ActionName("GetAllDepartments")]
        [HttpGet]
        public List<IC_Department> GetAllDepartments()
        {
            var list = new List<IC_Department>();
            var listDep = context.IC_Department.Where(x => x.IsInactive != true).ToList();
            list.AddRange(listDep);
            return list;
        }

        [ActionName("GetDeviceAllPrivilege")]
        [HttpGet]
        public async Task<IActionResult> GetDeviceAllPrivilege()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var devs = await _IC_DeviceService.GetAllAsync(x => x.CompanyIndex == user.CompanyIndex);
            var result = devs.Select(dev => _Mapper.Map<IC_Device, ComboboxItem>(dev));

            return ApiOk(result);
        }

        [ActionName("GetDeviceByDeviceModule")]
        [HttpPost]
        public async Task<IActionResult> GetDeviceByDeviceModule([FromBody] List<string> pListDeviceModule)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var devs = await _IC_DeviceService.GetListDeviceByDeviceModule(pListDeviceModule, user.CompanyIndex);
            //var result = devs.Select(dev => _Mapper.Map<IC_Device, ComboboxItem>(dev));
            var result = devs;

            return ApiOk(result);
        }

        protected UnauthorizedObjectResult ApiUnauthorized(string pMessageCode = "TokenExpired")
        {
            return Unauthorized(pMessageCode);
        }

        [ActionName("GetByEmployeeATIDAndCompanyIndex")]
        [HttpGet]
        public async Task<IActionResult> GetByEmployeeATIDAndCompanyIndex(string employeeATID, int companyIndex)
        {
            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return Ok(new CameraANPRResult(false, "Token invalid", "", "", ""));
            }

            var config = ConfigObject.GetConfig(cache);
            HR_Employee data = null;

            if (config.IntegrateDBOther == true)
            {
                data = await _IHR_EmployeeLogic.GetByEmployeeATIDAndCompanyIndex(employeeATID, companyIndex);
            }
            if (data == null)
            {
                data = await _IIC_EmployeeLogic.GetByEmployeeATIDAndCompanyIndex(employeeATID, companyIndex);
            }

            return Ok(data);
        }

        [ActionName("GetEmployeeByEmployeeATID")]
        [HttpGet]
        public IActionResult GetEmployeeByEmployeeATID(string employeeATID, int companyIndex)
        {
            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return Ok(new CameraANPRResult(false, "Token invalid", "", "", ""));
            }

            var config = ConfigObject.GetConfig(cache);
            EmployeeBasicInfo data = null;
            var now = DateTime.Now;
            var addedParams = new List<AddedParam>();

            if (config.IntegrateDBOther == true)
            {
                addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = new List<string>() { employeeATID } });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = companyIndex });
                var listHREmployee = _IHR_EmployeeLogic.GetMany(addedParams);
                var objOtherDb = from e in listHREmployee
                                 select new EmployeeBasicInfo
                                 {
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     FullName = e.FullName,
                                     DepartmentIndex = e.DepartmentIndex,
                                     DepartmentName = e.DepartmentName,
                                     DepartmentCode = e.DepartmentCode,
                                     JoinedDate = e.JoinedDate,
                                     Avatar = e.Avatar
                                 };

                data = objOtherDb.FirstOrDefault();
            }
            if (data == null)
            {
                addedParams.Add(new AddedParam { Key = "EmployeeATID", Value = employeeATID });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = companyIndex });
                var listEmployee = _IIC_EmployeeLogic.GetEmployeeList(addedParams);

                listEmployee = _IIC_EmployeeLogic.CheckCurrentDepartment(listEmployee);

                var obj = from e in listEmployee
                          select new EmployeeBasicInfo
                          {
                              EmployeeATID = e.EmployeeATID,
                              EmployeeCode = e.EmployeeCode,
                              FullName = e.FullName,
                              DepartmentIndex = e.DepartmentIndex,
                              DepartmentName = e.DepartmentName,
                              DepartmentCode = e.DepartmentCode,
                              JoinedDate = e.JoinedDate,
                              Avatar = e.Avatar,
                              PositionName = e.PositionName,
                          };

                data = obj.FirstOrDefault();
            }
            return Ok(data);
        }

        [ActionName("GetEmployeeCompactInfoByCompanyIndexAndDate")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeeCompactInfoByCompanyIndexAndDate(int companyIndex)
        {
            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return Ok(new CameraANPRResult(false, "Token invalid", "", "", ""));
            }

            var config = ConfigObject.GetConfig(cache);
            List<EmployeeFullInfo> data = null;
            var addedParams = new List<AddedParam>();

            if (config.IntegrateDBOther == true)
            {
                addedParams = new List<AddedParam>();
                //addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = new List<string>() { employeeATID } });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = companyIndex });
                var listHREmployee = await _IHR_EmployeeLogic.GetEmployeeCompactInfo(companyIndex);

                data = listHREmployee;
            }
            if (data == null)
            {
                //addedParams.Add(new AddedParam { Key = "EmployeeATID", Value = employeeATID });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = companyIndex });
                var listEmployee = await _IIC_EmployeeLogic.GetEmployeeCompactInfo(addedParams);

                //listEmployee = _IIC_EmployeeLogic.CheckCurrentDepartment(listEmployee);
                data = listEmployee;

            }
            return Ok(data);
        }

        [ActionName("GetEmployeeAsTree")]
        [HttpGet]
        public IActionResult GetEmployeeAsTree(int companyIndex)
        {
            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return Ok(new CameraANPRResult(false, "Token invalid", "", "", ""));
            }

            var addedParams = new List<AddedParam>();
            var config = ConfigObject.GetConfig(cache);
            var listData = new List<IC_EmployeeTreeDTO>();
            var now = DateTime.Now;
            //use db EPAD
            if (config.IntegrateDBOther == false)
            {
                var company = context.IC_Company.Where(t => t.Index == companyIndex).FirstOrDefault();
                var listDep = context.IC_Department.Where(t => t.CompanyIndex == companyIndex && t.IsInactive != true).ToList();

                addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = companyIndex });
                addedParams.Add(new AddedParam { Key = "IsCurrentWorkingAndNoDepartment", Value = true });
                var listEmployee = _IIC_EmployeeLogic.GetEmployeeList(addedParams); //GetAllEmployeeReportFromDBEPAD(context,now, user.CompanyIndex);

                //// chỉ hiển thị những phòng ban dc phân quyền
                //var hsDept = user.ListDepartmentAssignedAndParent.ToHashSet();
                //listDep = listDep.Where(t => hsDept.Contains(t.Index)).ToList();
                int id = 1; int level = 1;

                var mainData = new IC_EmployeeTreeDTO();

                mainData.ID = -1; mainData.Code = company.Index.ToString(); mainData.Name = company.Name;
                mainData.Type = "Company"; mainData.Level = level;
                level++;

                var listChildrentForCompany = new List<IC_EmployeeTreeDTO>();
                var listDepLV1 = listDep.Where(t => t.ParentIndex == null || t.ParentIndex == 0).ToList();
                //create phòng ban 'ko có phòng ban'
                listDepLV1.Add(new IC_Department()
                {
                    Index = 0,
                    Name = "Không có phòng ban",
                    Code = "",
                    ParentIndex = 0,
                    CompanyIndex = companyIndex
                });

                for (int i = 0; i < listDepLV1.Count; i++)
                {
                    var currentDep = new IC_EmployeeTreeDTO();
                    currentDep.ID = id++;
                    currentDep.Code = listDepLV1[i].Code; currentDep.Name = listDepLV1[i].Name;
                    currentDep.Type = "Department"; currentDep.Level = level;
                    currentDep.ListChildrent = new List<IC_EmployeeTreeDTO>();
                    if (listDepLV1[i].Index > 0)
                    {
                        currentDep.ListChildrent = RecursiveGetChildrentDepartment(listDep, listEmployee, listDepLV1[i].Index, ref id, level + 1);
                    }

                    currentDep.ListChildrent.AddRange(GetListEmployeeByDepartmentIndex(listEmployee, listDepLV1[i].Index, ref id, level + 1));

                    listChildrentForCompany.Add(currentDep);
                }
                mainData.ListChildrent = listChildrentForCompany;
                listData.Add(mainData);
            }
            else //use other db
            {
                var company = otherContext.HR_Company.Where(t => t.Index == config.CompanyIndex).FirstOrDefault();
                if (company == null)
                {
                    return NoContent();
                }
                var listDep = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex).ToList();
                var listEmployee = GetAllEmployeeReport(otherContext, config);
                //// chỉ hiển thị những phòng ban dc phân quyền
                //listDep = listDep.Where(t => user.ListDepartmentAssignedAndParent.Contains(t.Index)).ToList();
                int id = 1; int level = 1;

                var mainData = new IC_EmployeeTreeDTO();

                mainData.ID = -1; mainData.Code = company.Index.ToString(); mainData.Name = company.Name;
                mainData.Type = "Company"; mainData.Level = level;
                level++;
                var listChildrentForCompany = new List<IC_EmployeeTreeDTO>();
                var listDepLV1 = listDep.Where(t => t.ParentIndex == null || t.ParentIndex == 0).ToList();
                listDepLV1.Add(new HR_Department()
                {
                    Index = 0,
                    Name = "Không có phòng ban",
                    NameInEng = "Not in department",
                    Code = "",
                    ParentIndex = 0,
                    CompanyIndex = companyIndex
                });
                for (int i = 0; i < listDepLV1.Count; i++)
                {
                    var currentDep = new IC_EmployeeTreeDTO();
                    currentDep.ID = id++; //id + "." + (i + 1);
                    currentDep.Code = listDepLV1[i].Code; currentDep.Name = listDepLV1[i].Name;
                    currentDep.Type = "Department"; currentDep.Level = level;
                    if (listDepLV1[i].Index > 0)
                    {
                        currentDep.ListChildrent = RecursiveGetChildrentDepartment_DBOther(listDep, listEmployee, int.Parse(listDepLV1[i].Index.ToString()), ref id, level + 1);
                    }
                    currentDep.ListChildrent.AddRange(GetListEmployeeByDepartmentIndex_DBOther(listEmployee, int.Parse(listDepLV1[i].Index.ToString()), ref id, level + 1));

                    listChildrentForCompany.Add(currentDep);
                }
                mainData.ListChildrent = listChildrentForCompany;
                listData.Add(mainData);
            }

            return Ok(listData);
        }

        private static List<HR_EmployeeReport> GetAllEmployeeReport(ezHR_Context pContext, ConfigObject config)
        {
            string query =
                "SELECT HR_Employee.EmployeeATID, HR_Employee.CompanyIndex, HR_Employee.EmployeeCode, HR_Employee.CardNumber,HR_Employee.LastName + ' ' + HR_Employee.MidName + ' ' + HR_Employee.FirstName AS FullName," +
                " HR_Employee.NickName, HR_Employee.Gender, '' as NameOnMachine,HR_WorkingInfo.DepartmentIndex as DepartmentIndex,dep.[Name] as DepartmentName, HR_Employee.JoinedDate " +
                " FROM HR_Employee" +
                " LEFT JOIN HR_WorkingInfo ON HR_WorkingInfo.EmployeeATID = HR_Employee.EmployeeATID" +
                " and ((HR_WorkingInfo.ToDate is null and  Datediff(day, HR_WorkingInfo.FromDate, getdate()) >= 0)" +
                " OR(Datediff(day, HR_WorkingInfo.ToDate, getdate()) <= 0 AND Datediff(day, HR_WorkingInfo.FromDate, getdate()) >= 0))" +
                " LEFT OUTER JOIN HR_Department ON HR_Department.[Index] = HR_WorkingInfo.DepartmentIndex" +
                " LEFT OUTER JOIN HR_Position ON HR_WorkingInfo.PositionIndex = HR_Position.[Index]" +
                " LEFT OUTER JOIN HR_Titles ON HR_WorkingInfo.TitlesIndex = HR_Titles.[Index]" +
                " and(HR_WorkingInfo.ToDate is null OR" +
                " (Datediff(day, HR_WorkingInfo.ToDate, getdate()) <= 0 AND Datediff(day, HR_WorkingInfo.FromDate, getdate()) >= 0))" +
                " left join HR_Department dep on dep.[Index]= HR_WorkingInfo.DepartmentIndex " +
                " WHERE (HR_Employee.MarkForDelete = 0) AND HR_Employee.CompanyIndex = " + config.CompanyIndex;
            List<HR_EmployeeReport> listEmployee;
            listEmployee = pContext.HR_EmployeeReport.FromSqlRaw(query).ToList();
            return listEmployee;
        }
        private List<IC_EmployeeTreeDTO> GetListEmployeeByDepartmentIndex(List<IC_EmployeeDTO> listEmployee, int pDepIndex, ref int pId, int pLevel)
        {
            List<IC_EmployeeDTO> listEmp;
            if (pDepIndex > 0)
            {
                listEmp = listEmployee.Where(t => t.DepartmentIndex == pDepIndex).ToList();
            }
            else
            {
                listEmp = listEmployee.Where(t => t.DepartmentIndex == 0 || t.DepartmentIndex == null).ToList();
            }
            var listEmpReturn = new List<IC_EmployeeTreeDTO>();
            for (int i = 0; i < listEmp.Count; i++)
            {
                IC_EmployeeTreeDTO currentEmp = new IC_EmployeeTreeDTO();
                currentEmp.EmployeeATID = listEmp[i].EmployeeATID;
                currentEmp.ID = pId++;//pId + "." + (i + 1); 
                currentEmp.Code = listEmp[i].EmployeeATID; ; currentEmp.Name = listEmp[i].EmployeeATID + "-" + listEmp[i].FullName;
                currentEmp.Type = "Employee"; currentEmp.Level = pLevel;
                if (listEmp[i].Gender != null)
                    currentEmp.Gender = listEmp[i].Gender.Value == (short)GenderEnum.Female ? "Female" : listEmp[i].Gender.Value == (short)GenderEnum.Male ? "Male" : "Other";

                listEmpReturn.Add(currentEmp);
            }
            return listEmpReturn;
        }
        private List<IC_EmployeeTreeDTO> RecursiveGetChildrentDepartment(List<IC_Department> listDep, List<IC_EmployeeDTO> listEmployee, int pCurrentIndex, ref int pId, int pLevel)
        {
            var listChildrent = listDep.Where(t => t.ParentIndex == pCurrentIndex).ToList();
            var listDepReturn = new List<IC_EmployeeTreeDTO>();
            if (listChildrent.Count > 0)
            {
                for (int i = 0; i < listChildrent.Count; i++)
                {
                    IC_EmployeeTreeDTO currentDep = new IC_EmployeeTreeDTO();
                    currentDep.ID = pId++;//listChildrent[i].Index;//Convert.ToDecimal(pId + "." + (i + 1)); 
                    currentDep.Code = listChildrent[i].Code; ; currentDep.Name = listChildrent[i].Name;
                    currentDep.Type = "Department"; currentDep.Level = pLevel;
                    currentDep.ListChildrent = RecursiveGetChildrentDepartment(listDep, listEmployee, listChildrent[i].Index, ref pId, pLevel + 1);
                    currentDep.ListChildrent.AddRange(GetListEmployeeByDepartmentIndex(listEmployee, listChildrent[i].Index, ref pId, pLevel + 1));

                    listDepReturn.Add(currentDep);
                }
            }

            return listDepReturn;
        }
        private List<IC_EmployeeTreeDTO> RecursiveGetChildrentDepartment_DBOther(List<HR_Department> listDep, List<HR_EmployeeReport> listEmployee, int pCurrentIndex, ref int pId, int pLevel)
        {
            var listChildrent = listDep.Where(t => t.ParentIndex == pCurrentIndex).ToList();
            var listDepReturn = new List<IC_EmployeeTreeDTO>();
            if (listChildrent.Count > 0)
            {
                for (int i = 0; i < listChildrent.Count; i++)
                {
                    IC_EmployeeTreeDTO currentDep = new IC_EmployeeTreeDTO();
                    currentDep.ID = pId++;//listChildrent[i].Index;//Convert.ToDecimal(pId + "." + (i + 1)); 
                    currentDep.Code = listChildrent[i].Code; ; currentDep.Name = listChildrent[i].Name;
                    currentDep.Type = "Department"; currentDep.Level = pLevel;
                    currentDep.ListChildrent = RecursiveGetChildrentDepartment_DBOther(listDep, listEmployee, int.Parse(listChildrent[i].Index.ToString()), ref pId, pLevel + 1);
                    currentDep.ListChildrent.AddRange(GetListEmployeeByDepartmentIndex_DBOther(listEmployee, int.Parse(listChildrent[i].Index.ToString()), ref pId, pLevel + 1));

                    listDepReturn.Add(currentDep);
                }
            }

            return listDepReturn;
        }
        private List<IC_EmployeeTreeDTO> GetListEmployeeByDepartmentIndex_DBOther(List<HR_EmployeeReport> listEmployee, int pDepIndex, ref int pId, int pLevel)
        {
            var listEmp = new List<HR_EmployeeReport>();
            if (pDepIndex == 0)
            {
                listEmp = listEmployee.Where(t => t.DepartmentIndex == null || t.DepartmentIndex.Value == 0).ToList();
            }
            else
            {
                listEmp = listEmployee.Where(t => t.DepartmentIndex == pDepIndex).ToList();
            }

            var listEmpReturn = new List<IC_EmployeeTreeDTO>();
            for (int i = 0; i < listEmp.Count; i++)
            {
                IC_EmployeeTreeDTO currentEmp = new IC_EmployeeTreeDTO();
                currentEmp.EmployeeATID = listEmp[i].EmployeeATID;
                currentEmp.ID = pId++;//pId + "." + (i + 1); 
                currentEmp.Code = listEmp[i].EmployeeATID; ; currentEmp.Name = "(" + listEmp[i].EmployeeATID + ")" + listEmp[i].FullName;
                currentEmp.Type = "Employee"; currentEmp.Level = pLevel;
                if (listEmp[i].Gender != null)
                    currentEmp.Gender = listEmp[i].Gender.Value == false ? "Female" : "Male";

                listEmpReturn.Add(currentEmp);
            }
            return listEmpReturn;
        }
        #endregion

        [ActionName("DownloadLog")]
        [HttpPost]
        public IActionResult DownloadLog([FromBody] DownloadLogRequest request)
        {
            var keyHash = StringHelper.SHA256(request.Key);
            var token = context.IC_AccessToken.FirstOrDefault(x => x.AccessToken == keyHash);
            if (token == null) return Unauthorized("MSG_InvalidAccessTokenKey");

            var lstUser = new List<UserInfoOnMachine>();
            var lstCmd = CommandProcess.CreateListCommands(context, request.Serials, CommandAction.DownloadLogFromToTime, "", request.FromDate, request.ToDate, lstUser, false, GlobalParams.DevicePrivilege.SDKStandardRole);
            CommandProcess.CreateGroupCommand(context: context,
                cache: cache,
                pCompanyIndex: token.CompanyIndex,
                pUserName: token.UpdatedUser,
                pGroupName: CommandAction.DownloadLogFromToTime.ToString(),
                pExternalData: "",
                pListCommands: lstCmd,
                pEventType: "");
            return Ok();
        }

        [ActionName("GetDeviceAll")]
        [HttpGet]
        public IActionResult GetDeviceAll(string key, int companyIndex)
        {
            var keyHash = StringHelper.SHA256(key);
            string token = cache.Get("CommunicateToken").ToString();
            //var token = context.IC_AccessToken.FirstOrDefault(x => x.AccessToken == keyHash);
            if (token != key)
            {
                return Unauthorized("MSG_InvalidAccessTokenKey");
            }

            var data = context.IC_Device.Where(t => t.CompanyIndex == companyIndex)
                .Select(t => new { SerialNumber = t.SerialNumber, AliasName = t.AliasName }).ToList();

            return Ok(data);
        }

        [ActionName("GetEmployeeInfoByRootDepartment")]
        [HttpGet]
        public async Task<ActionResult<List<HR_EmployeeInfoResult>>> GetEmployeeInfoByRootDepartment(int companyIndex)
        {
            var listAllActiveDepartment = _IC_DepartmentService.GetAllActiveDepartment(companyIndex);
            var listActiveDepartmentIndex = listAllActiveDepartment.Select(x => (long)x.Index).ToList();
            listActiveDepartmentIndex = FilterExistRootDepartment(listActiveDepartmentIndex, listAllActiveDepartment);
            var allEmployee = await _HR_EmployeeInfoService.GetEmployeeInfoByDepartment(listActiveDepartmentIndex, companyIndex);
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

        [ActionName("GetWorkingEmployeeByDepartment")]
        [HttpGet]
        public async Task<ActionResult<object>> GetWorkingEmployeeByDepartment()
        {
            var result = await _IC_AttendanceLogService.GetWorkingEmployeeByDepartment();

            return ApiOk(result);
        }

        [ActionName("GetWorkingEmployeeByRootDepartment")]
        [HttpGet]
        public async Task<ActionResult<object>> GetWorkingEmployeeByRootDepartment()
        {
            var result = await _IC_AttendanceLogService.GetWorkingEmployeeByDepartment();
            var listActiveDepartment = await _IC_DepartmentService.GetActiveDepartment();
            var listEmployeeRootDepartment = listActiveDepartment.Where(x => !x.ParentIndex.HasValue || x.ParentIndex == 0).ToList();
            foreach (var employee in result)
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

            return ApiOk(result);
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

        private List<long> FilterExistRootDepartment(List<long> listIndex, List<IC_Department> listDepartment)
        {
            var result = listIndex;
            var reFilter = false;

            foreach (var department in listDepartment)
            {
                if (department.ParentIndex.HasValue && !listDepartment.Any(y => y.Index == department.ParentIndex.Value))
                {
                    result = result.Where(x => x != department.Index).ToList();
                    listDepartment = listDepartment.Where(x => x.Index != department.Index).ToList();
                    reFilter = true;
                }
            }
            if (reFilter)
            {
                result = FilterExistRootDepartment(result, listDepartment);
            }

            return result;
        }

        [ActionName("GetAllDepartment_Public")]
        [HttpGet]
        public IActionResult GetAllDepartment_Public(int companyIndex)
        {
            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return BadRequest("Token invalid");
            }

            var data = context.IC_Department.Where(t => t.CompanyIndex == companyIndex && t.IsInactive != true)
                .Select(t => new { t.Index, t.Name, t.Code, t.ParentIndex })
                .ToList();

            return Ok(data);
        }

        [ActionName("GetAllDevice_Public")]
        [HttpGet]
        public IActionResult GetAllDevice_Public(int companyIndex)
        {
            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return BadRequest("Token invalid");
            }

            var data = context.IC_Device.Where(t => t.CompanyIndex == companyIndex)
                .Select(t => new { SerialNumber = t.SerialNumber, AliasName = t.AliasName, IPAddress = t.IPAddress, Port = t.Port })
                .ToList();

            return Ok(data);
        }

        [ActionName("GetGroupDevice_Public")]
        [HttpGet]
        public IActionResult GetGroupDevice_Public(string key, int companyIndex)
        {
            var keyHash = StringHelper.SHA256(key);
            string token = cache.Get("CommunicateToken").ToString();
            //var token = context.IC_AccessToken.FirstOrDefault(x => x.AccessToken == keyHash);
            if (token != key)
            {
                return Unauthorized("MSG_InvalidAccessTokenKey");
            }
            var groupDeviceDetails = context.IC_GroupDeviceDetails.AsEnumerable()
                .Where(t => t.CompanyIndex == companyIndex).ToList();
            var groupDevices = context.IC_GroupDevice.AsEnumerable()
                .Where(t => t.CompanyIndex == companyIndex).Select(e => new
                {
                    e.Index,
                    e.Name,
                    e.Description,
                    ListMachine = groupDeviceDetails.Where(d => d.GroupDeviceIndex == e.Index).Select(d => d.SerialNumber).ToList()
                }).ToList();

            return Ok(groupDevices);
        }

        [ActionName("GetAllController_Public")]
        [HttpGet]
        public IActionResult GetAllController_Public(int companyIndex, bool isInput)
        {
            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return BadRequest("Token invalid");
            }

            var listController = context.IC_RelayController.Where(t => t.CompanyIndex == companyIndex
                && (isInput == false || isInput && (t.SignalType == (byte)SignalStatus.Input || t.SignalType == (byte)SignalStatus.InOutput)))
                .ToList();
            var listChannel = context.IC_RelayControllerChannel.Where(t => t.CompanyIndex == companyIndex).ToList();

            var listData = new List<ControllerParam>();
            for (int i = 0; i < listController.Count; i++)
            {
                var data = new ControllerParam();
                data.Index = listController[i].Index;
                data.Name = listController[i].Name;
                data.IPAddress = listController[i].IpAddress;
                data.Port = listController[i].Port;
                data.Description = listController[i].Description;
                data.SignalType = listController[i].SignalType;
                data.ListChannel = new List<ChannelParam>();
                data.RelayType = listController[i].RelayType;
                var listChannelByController = listChannel.Where(t => t.RelayControllerIndex == data.Index).ToList();
                foreach (var item in listChannelByController)
                {
                    data.ListChannel.Add(new ChannelParam() { Index = item.ChannelIndex, NumberOfSecondsOff = item.NumberOfSecondsOff, SignalType = item.SignalType });
                }

                listData.Add(data);
            }

            IActionResult result = Ok(listData);
            return result;
        }

        [ActionName("GetAllCamera_Public")]
        [HttpGet]
        public IActionResult GetAllCamera_Public(int companyIndex)
        {
            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return BadRequest("Token invalid");
            }

            var listCamera = context.IC_Camera.Where(t => t.CompanyIndex == companyIndex).ToList();


            var listData = new List<CameraPublicResult>();
            for (int i = 0; i < listCamera.Count; i++)
            {
                var data = new CameraPublicResult();
                data.Index = listCamera[i].Index;
                data.Name = listCamera[i].Name;
                data.IpAddress = listCamera[i].IpAddress;
                data.Port = listCamera[i].Port;
                data.Serial = listCamera[i].Serial;
                data.Type = listCamera[i].Type;

                listData.Add(data);
            }

            IActionResult result = Ok(listData);
            return result;
        }

        [ActionName("GetServerTime")]
        [HttpGet]
        public IActionResult GetServerTime()
        {
            DateTime now = DateTime.Now;
            return Ok(now);
        }

        [ActionName("SetOnOffAccessControl")]
        [HttpPost]
        public async Task<IActionResult> SetOnOffAccessControl([FromBody] RelayControllerParam param)
        {
            try
            {
                string token = "";
                for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
                {
                    if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                    {
                        token = HttpContext.Request.Headers.Values.ElementAt(i)[0];
                        break;
                    }
                }

                if (token != mCommunicateToken)
                {
                    return BadRequest("Token invalid");
                }

                var controller = context.IC_RelayController.FirstOrDefault(t => t.Index == param.ControllerIndex);
                if (controller == null)
                {
                    return BadRequest("ControllerIndexNotExists");
                }
                var controllerChannel = context.IC_RelayControllerChannel
                    .Where(t => t.RelayControllerIndex == param.ControllerIndex && param.ListChannel.Contains(t.ChannelIndex)).ToList();

                //if (param?.Second > 0)
                //    controllerChannel.NumberOfSecondsOff = param.Second;

                string error = "";
                var isValid = true;
                //_logger.LogError($"Type {controller.RelayType} - {param.AutoOff} {param.ControllerIndex} - {string.Join(',', param.ListChannel)} - {param.SetOn}");

                if (controller.RelayType == RelayType.ModbusTCP.ToString())
                {
                    var controllerStatus = await _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(controller.IpAddress, Convert.ToUInt16(controller.Port));
                    //_logger.LogError($"controllerStatus: {controllerStatus}");

                    if (controllerStatus)
                    {
                        var listChannelInput = new List<ChannelParam>
                        {
                            new ChannelParam() { Index = 1 },
                            new ChannelParam() { Index = 2 },
                            new ChannelParam() { Index = 3 },
                            new ChannelParam() { Index = 4 },
                            new ChannelParam() { Index = 5 },
                            new ChannelParam() { Index = 6 },
                            new ChannelParam() { Index = 7 },
                            new ChannelParam() { Index = 8 }
                        };
                        var param1 = new ControllerParam
                        {
                            RelayType = RelayType.ModbusTCP.ToString(),
                            ListChannel = listChannelInput
                        };

                        var esdMillisecondsLooping = esdMinuteLooping * 1000;

                        if ((param.ChannelInputGood > 0 || param.ChannelInputNotGood > 0) && esdMinuteLooping > 0)
                        {
                            var goodTime = 0;
                            for (int i = 0; i < 1; i--)
                            {
                                goodTime += 100;
                                Thread.Sleep(30);
                                var inputResult = _IIC_ModbusReplayControllerLogic.GetChannelInputStatus(param1);
                                if (inputResult.ListChannel.Any(x => x.ChannelStatus == true))
                                {
                                    var channelIndex = inputResult.ListChannel.FirstOrDefault(x => x.ChannelStatus).Index;
                                    if (param.ChannelInputGood == channelIndex)
                                    {
                                        i = 100;
                                        foreach (var item in param.ListChannel)
                                        {
                                            var listChannel = new List<ChannelParam>
                                            {
                                                new ChannelParam() { Index = Convert.ToInt16(item), ChannelStatus = param.SetOn }
                                            };
                                            //_logger.LogError($"param.ListChannel: {item} - {param.SetOn}");
                                            var channel = controllerChannel.FirstOrDefault(x => x.ChannelIndex == item);
                                            double secondAutoClose;
                                            if (param?.Second > 0)
                                            {
                                                secondAutoClose = param.Second;
                                            }
                                            else
                                            {
                                                secondAutoClose = channel?.NumberOfSecondsOff ?? 4;
                                            }
                                            var result = _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannel, secondAutoClose);
                                        }
                                        _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                                        return Ok();
                                    }
                                    else if (param.ChannelInputNotGood == channelIndex)
                                    {
                                        i = 100;
                                        return StatusCode(400, "ChannelInputNotGood");
                                    }
                                }
                                if (goodTime == esdMillisecondsLooping)
                                {
                                    i = 100;
                                }
                            }
                            isValid = false;
                        }
                        else
                        {
                            foreach (var item in param.ListChannel)
                            {
                                var listChannel = new List<ChannelParam>
                                {
                                    new ChannelParam() { Index = Convert.ToInt16(item), ChannelStatus = param.SetOn }
                                };
                                //_logger.LogError($"param.ListChannel: {item} - {param.SetOn}");
                                var channel = controllerChannel.FirstOrDefault(x => x.ChannelIndex == item);
                                double secondAutoClose;
                                if (param?.Second > 0)
                                {
                                    secondAutoClose = param.Second;
                                }
                                else
                                {
                                    secondAutoClose = channel?.NumberOfSecondsOff ?? 4;
                                }
                                var result = await _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannel, secondAutoClose);
                            }
                            _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                            return Ok();
                        }

                        //var result = _IIC_ModbusReplayControllerLogic.OpenChannel(listChannel);
                        //_logger.LogError($"SetOnAndAutoOffController: {controllerChannel?.NumberOfSecondsOff} {result}");
                    }
                }
                else
                {
                    //if (param.AutoOff)
                    //{
                    double secondAutoClose;
                    if (param?.Second > 0)
                    {
                        secondAutoClose = param.Second;
                    }
                    else
                    {
                        secondAutoClose = controllerChannel?.FirstOrDefault()?.NumberOfSecondsOff ?? 4;
                    }
                    error = await _IIC_ClientTCPControllerLogic.SetOnAndAutoOffController(controller.IpAddress, controller.Port, param.ListChannel, secondAutoClose);
                    //ControllerProcess.SetOnAndAutoOffController(_linkControllerApi, controller.IpAddress, controller.Port, param.ListChannel, controllerChannel == null ? 4 : controllerChannel.NumberOfSecondsOff);
                    //_logger.LogError($"error456:{error} - {_linkControllerApi} - NumberOfSecondsOff:{controllerChannel?.NumberOfSecondsOff}");
                    _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                    return Ok();
                    //}
                    //else
                    //{
                    //    //ControllerProcess.SetOnOffController(_linkControllerApi, controller.IpAddress, controller.Port, param.ListChannel, param.SetOn);
                    //    error = _IIC_ClientTCPControllerLogic.SetOnOffController(_linkControllerApi, controller.IpAddress, controller.Port, param.ListChannel, param.SetOn);
                    //    _logger.LogError($"error456:{error} {_linkControllerApi} IP:{controller.IpAddress} {string.Join(",", param.ListChannel.ToArray())} SetOn:{param.SetOn}");
                    //}
                    //_logger.LogError($"ClientTCP: {error}");
                }
                if (error != "")
                {
                    _logger.LogError($"Controller: {error}");
                    return StatusCode(500, error);
                }

                if (isValid == false)
                    return NotFound();
                else
                    return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"SetOnAndAutoOffController: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [ActionName("SetOnOffAccessControlVstar")]
        [HttpPost]
        public async Task<IActionResult> SetOnOffAccessControlVstar([FromBody] RelayControllerParamVstar param)
        {
            try
            {
                string token = "";
                for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
                {
                    if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                    {
                        token = HttpContext.Request.Headers.Values.ElementAt(i)[0];
                        break;
                    }
                }

                if (token != mCommunicateToken)
                {
                    return BadRequest("Token invalid");
                }

                var controller = context.IC_RelayController.FirstOrDefault(t => t.Index == param.ControllerIndex);
                if (controller == null)
                {
                    return BadRequest("ControllerIndexNotExists");
                }
                var controllerChannel = context.IC_RelayControllerChannel
                    .Where(t => t.RelayControllerIndex == param.ControllerIndex && param.ListChannel.Contains(t.ChannelIndex)).ToList();

                //if (param?.Second > 0)
                //    controllerChannel.NumberOfSecondsOff = param.Second;

                string error = "";
                var isValid = true;
                //_logger.LogError($"Type {controller.RelayType} - {param.AutoOff} {param.ControllerIndex} - {string.Join(',', param.ListChannel)} - {param.SetOn}");

                if (controller.RelayType == RelayType.ModbusTCP.ToString())
                {
                    var controllerStatus = _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(controller.IpAddress, Convert.ToUInt16(controller.Port));
                    //_logger.LogError($"controllerStatus: {controllerStatus}");

                    if (await controllerStatus)
                    {
                        var listChannelInput = new List<ChannelParam>
                        {
                            new ChannelParam() { Index = 1 },
                            new ChannelParam() { Index = 2 },
                            new ChannelParam() { Index = 3 },
                            new ChannelParam() { Index = 4 },
                            new ChannelParam() { Index = 5 },
                            new ChannelParam() { Index = 6 },
                            new ChannelParam() { Index = 7 },
                            new ChannelParam() { Index = 8 }
                        };
                        var param1 = new ControllerParam
                        {
                            RelayType = RelayType.ModbusTCP.ToString(),
                            ListChannel = listChannelInput
                        };

                        var esdMillisecondsLooping = esdMinuteLooping * 1000;

                        if ((param.ChannelInputGood > 0 || param.ChannelInputNotGood > 0) && esdMinuteLooping > 0)
                        {
                            var goodTime = 0;
                            for (int i = 0; i < 1; i--)
                            {
                                goodTime += 100;
                                Thread.Sleep(30);
                                var inputResult = _IIC_ModbusReplayControllerLogic.GetChannelInputStatus(param1);
                                if (inputResult.ListChannel.Any(x => x.ChannelStatus == true))
                                {
                                    var channelIndex = inputResult.ListChannel.FirstOrDefault(x => x.ChannelStatus).Index;
                                    if (param.ChannelInputGood == channelIndex)
                                    {
                                        i = 100;
                                        foreach (var item in param.ListChannel)
                                        {
                                            var listChannel = new List<ChannelParam>
                                            {
                                                new ChannelParam() { Index = Convert.ToInt16(item), ChannelStatus = param.SetOn }
                                            };
                                            //_logger.LogError($"param.ListChannel: {item} - {param.SetOn}");
                                            var channel = controllerChannel.FirstOrDefault(x => x.ChannelIndex == item);
                                            double secondAutoClose;
                                            if (param?.Second > 0)
                                            {
                                                secondAutoClose = param.Second;
                                            }
                                            else
                                            {
                                                secondAutoClose = channel?.NumberOfSecondsOff ?? 4;
                                            }
                                            var result = _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannel, secondAutoClose);
                                        }
                                        _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                                        return Ok();
                                    }
                                    else if (param.ChannelInputNotGood == channelIndex)
                                    {
                                        i = 100;
                                        return StatusCode(400, "ChannelInputNotGood");
                                    }
                                }
                                if (goodTime == esdMillisecondsLooping)
                                {
                                    i = 100;
                                }
                            }
                            isValid = false;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(_clientName) && _clientName.ToLower() == ClientName.VSTAR.ToString().ToLower())
                            {
                                if (param.AccessTime == 1)
                                {
                                    var listAutoClose = new List<ChannelParam>
                                    {
                                        new ChannelParam() { Index = Convert.ToInt16(param.RelayControllerChannel.ChannelOpen), ChannelStatus = true },
                                        //new ChannelParam() {Index = Convert.ToInt16(param.RelayControllerChannel.ChannelClose), ChannelStatus = true },
                                    };
                                    var tasks = new List<Task>();
                                    foreach (var item in listAutoClose)
                                    {
                                        var channel = controllerChannel.FirstOrDefault(x => x.ChannelIndex == item.Index);
                                        double secondAutoClose;
                                        if (param?.Second > 0)
                                        {
                                            secondAutoClose = param.Second;
                                        }
                                        else
                                        {
                                            secondAutoClose = channel?.NumberOfSecondsOff ?? 4;
                                        }
                                        tasks.Add(_IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listAutoClose, secondAutoClose));
                                    }
                                    var listAutoClose1 = new List<ChannelParam>
                                    {
                                        new ChannelParam() { Index = Convert.ToInt16(param.RelayControllerChannel.ChannelLockOpen), ChannelStatus = false },
                                    };
                                    var channel1 = controllerChannel.FirstOrDefault(x => x.ChannelIndex == param.RelayControllerChannel.ChannelLockOpen);
                                    double secondAutoClose1;
                                    if (param?.Second > 0)
                                    {
                                        secondAutoClose1 = param.Second;
                                    }
                                    else
                                    {
                                        secondAutoClose1 = channel1?.NumberOfSecondsOff ?? 4;
                                    }
                                    tasks.Add(_IIC_ModbusReplayControllerLogic.SetOffAndAutoOnController(listAutoClose1, secondAutoClose1));
                                    var listChannelss = new List<ChannelParam>
                                    {
                                        new ChannelParam() {Index = Convert.ToInt16(param.RelayControllerChannel.ChannelClose), ChannelStatus = false },
                                    };

                                     _IIC_ModbusReplayControllerLogic.OpenChannel(listChannelss);
                                    await Task.WhenAll(tasks);

                                    var listChannels = new List<ChannelParam>
                                    {
                                        new ChannelParam() {Index = Convert.ToInt16(param.RelayControllerChannel.ChannelClose), ChannelStatus = true },
                                    };

                                    var result = _IIC_ModbusReplayControllerLogic.OpenChannel(listChannels);
                                }
                                else if (param.AccessTime == 2)
                                {
                                    var listChannels = new List<ChannelParam>
                                    {
                                        new ChannelParam() { Index = Convert.ToInt16(param.RelayControllerChannel.ChannelOpen), ChannelStatus = true },
                                        new ChannelParam() {Index = Convert.ToInt16(param.RelayControllerChannel.ChannelClose), ChannelStatus = false },
                                        new ChannelParam() {Index = Convert.ToInt16(param.RelayControllerChannel.ChannelLockOpen), ChannelStatus = false },
                                    };

                                    var result = _IIC_ModbusReplayControllerLogic.OpenChannel(listChannels);
                                }
                                else if (param.AccessTime == 3)
                                {
                                    var listChannels = new List<ChannelParam>
                                    {
                                        new ChannelParam() { Index = Convert.ToInt16(param.RelayControllerChannel.ChannelOpen), ChannelStatus = false },
                                        new ChannelParam() {Index = Convert.ToInt16(param.RelayControllerChannel.ChannelClose), ChannelStatus = true },
                                        new ChannelParam() {Index = Convert.ToInt16(param.RelayControllerChannel.ChannelLockOpen), ChannelStatus = true },
                                    };

                                    var result = _IIC_ModbusReplayControllerLogic.OpenChannel(listChannels);
                                }

                                _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                            }
                            else
                            {
                                foreach (var item in param.ListChannel)
                                {
                                    var listChannel = new List<ChannelParam>
                                {
                                    new ChannelParam() { Index = Convert.ToInt16(item), ChannelStatus = param.SetOn }
                                };
                                    //_logger.LogError($"param.ListChannel: {item} - {param.SetOn}");
                                    var channel = controllerChannel.FirstOrDefault(x => x.ChannelIndex == item);
                                    double secondAutoClose;
                                    if (param?.Second > 0)
                                    {
                                        secondAutoClose = param.Second;
                                    }
                                    else
                                    {
                                        secondAutoClose = channel?.NumberOfSecondsOff ?? 4;
                                    }
                                    var result = await _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannel, secondAutoClose);
                                }
                                _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                            }

                            return Ok();
                        }

                        //var result = _IIC_ModbusReplayControllerLogic.OpenChannel(listChannel);
                        //_logger.LogError($"SetOnAndAutoOffController: {controllerChannel?.NumberOfSecondsOff} {result}");
                    }
                }
                else
                {
                    //if (param.AutoOff)
                    //{
                    double secondAutoClose;
                    if (param?.Second > 0)
                    {
                        secondAutoClose = param.Second;
                    }
                    else
                    {
                        secondAutoClose = controllerChannel?.FirstOrDefault()?.NumberOfSecondsOff ?? 4;
                    }
                    error = await _IIC_ClientTCPControllerLogic.SetOnAndAutoOffController(controller.IpAddress, controller.Port, param.ListChannel, secondAutoClose);
            
                    return Ok();
                    //}
                    //else
                    //{
                    //    //ControllerProcess.SetOnOffController(_linkControllerApi, controller.IpAddress, controller.Port, param.ListChannel, param.SetOn);
                    //    error = _IIC_ClientTCPControllerLogic.SetOnOffController(_linkControllerApi, controller.IpAddress, controller.Port, param.ListChannel, param.SetOn);
                    //    _logger.LogError($"error456:{error} {_linkControllerApi} IP:{controller.IpAddress} {string.Join(",", param.ListChannel.ToArray())} SetOn:{param.SetOn}");
                    //}
                    //_logger.LogError($"ClientTCP: {error}");
                }
                if (error != "")
                {
                    _logger.LogError($"Controller: {error}");
                    return StatusCode(500, error);
                }

                if (isValid == false)
                    return NotFound();
                else
                    return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"SetOnAndAutoOffController: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [ActionName("SetOnAndAutoOffController")]
        [HttpPost]
        public async Task<IActionResult> SetOnAndAutoOffController([FromBody] RelayControllerParam param)
        {
            try
            {
                string token = "";
                for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
                {
                    if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                    {
                        token = HttpContext.Request.Headers.Values.ElementAt(i)[0];
                        break;
                    }
                }

                if (token != mCommunicateToken)
                {
                    return BadRequest("Token invalid");
                }

                var controller = context.IC_RelayController.FirstOrDefault(t => t.Index == param.ControllerIndex);
                if (controller == null)
                {
                    return BadRequest("ControllerIndexNotExists");
                }
                var controllerChannel = context.IC_RelayControllerChannel
                    .FirstOrDefault(t => t.RelayControllerIndex == param.ControllerIndex && param.ListChannel.Contains(t.ChannelIndex));

                string error = "";

                //_logger.LogError($"Type {controller.RelayType} - {param.AutoOff} {param.ControllerIndex} - {string.Join(',', param.ListChannel)} - {param.SetOn}");

                if (controller.RelayType == RelayType.ModbusTCP.ToString())
                {
                    //_logger.LogError("param.AutoOff");
                    if (param.AutoOff)
                    {
                        if (await _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(controller.IpAddress, Convert.ToUInt16(controller.Port)))
                        {
                            var listChannel = new List<ChannelParam>();
                            foreach (var item in param.ListChannel)
                            {
                                listChannel.Add(new ChannelParam() { Index = Convert.ToInt16(item) });
                            }

                            if (controllerChannel != null)
                            {
                                var result = await _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannel, controllerChannel?.NumberOfSecondsOff ?? 4);
                                _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                                //_logger.LogError($"SetOnAndAutoOffController true: {controllerChannel.NumberOfSecondsOff} {result}");
                            }
                        }
                    }
                    else
                    {
                        if (await _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(controller.IpAddress, Convert.ToUInt16(controller.Port)))
                        {
                            var listChannel = new List<ChannelParam>();
                            foreach (var item in param.ListChannel)
                            {
                                listChannel.Add(new ChannelParam() { Index = Convert.ToInt16(item), ChannelStatus = param.SetOn });
                                //_logger.LogError($"param.ListChannel: {item} - {param.SetOn}");
                            }
                            var result = _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannel, controllerChannel?.NumberOfSecondsOff ?? 4);
                            //var result = _IIC_ModbusReplayControllerLogic.OpenChannel(listChannel);
                            _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                            //_logger.LogError($"SetOnAndAutoOffController false: {controllerChannel?.NumberOfSecondsOff} {result}");
                        }
                    }
                }
                else
                {
                    _logger.LogError($"_linkControllerApi {_linkControllerApi}");
                    if (param.AutoOff)
                    {

                        error = await _IIC_ClientTCPControllerLogic.SetOnAndAutoOffController(controller.IpAddress, controller.Port, param.ListChannel, controllerChannel?.NumberOfSecondsOff ?? 4);
                        //ControllerProcess.SetOnAndAutoOffController(_linkControllerApi, controller.IpAddress, controller.Port, param.ListChannel, controllerChannel == null ? 4 : controllerChannel.NumberOfSecondsOff);
                        _logger.LogError($"error123:{error} - {_linkControllerApi} - NumberOfSecondsOff:{controllerChannel?.NumberOfSecondsOff}");
                    }
                    else
                    {
                        //ControllerProcess.SetOnOffController(_linkControllerApi, controller.IpAddress, controller.Port, param.ListChannel, param.SetOn);
                        error = await _IIC_ClientTCPControllerLogic.SetOnOffController(controller.IpAddress, controller.Port, param.ListChannel, param.SetOn);
                        _logger.LogError($"error456:{error} {_linkControllerApi} IP:{controller.IpAddress} {string.Join(",", param.ListChannel.ToArray())} SetOn:{param.SetOn}");
                    }
                }
                if (error != "")
                {
                    _logger.LogError("Controller: " + error);
                    return StatusCode(500, error);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("SetOnAndAutoOffController: " + ex);
                return Ok(ex.Message);
            }
        }

        [ActionName("GetCameraPictureByCameraIndex")]
        [HttpGet]
        public IActionResult GetCameraPictureByCameraIndex(int cameraIndex, string option)
        {
            try
            {
                string token = "";
                for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
                {
                    if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                    {
                        token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                        break;
                    }
                }

                if (token != mCommunicateToken)
                {
                    return Ok(new CameraPictureResult { Success = false, Error = "Token invalid", Link = "" });
                }

                IC_Camera cameraInfo = context.IC_Camera.Where(t => t.Index == cameraIndex).FirstOrDefault();
                if (cameraInfo == null)
                {
                    return Ok(new CameraPictureResult { Success = false, Error = "Camera index invalid", Link = "" });
                }
                string error = "";
                var result = new CameraPictureResult { Success = false, Error = "", Link = "" };
                var now = DateTime.Now;
                if (option.ToLower() == "link")
                {
                    var stream = PublicFunctions.GetStreamImageFromCamera(cameraInfo.IpAddress, cameraInfo.Port.ToString(),
                        cameraInfo.UserName, cameraInfo.Password, "1", 3, ref error);
#if !DEBUG
                    if (error != "")
                    {
                        result.Error = error;
                        result.Success = false;
                    }
                    else
                    {
                        result.Success = true;
                        string picturePath = PublicFunctions.CreateLinkImageCameraFromStream(stream, cameraInfo.Index, "101", now, null, ref error);
                        if (error != "")
                        {
                            result.Error = error;
                            result.Success = false;
                        }
                        else
                        {
                            string link = this.Request.Scheme + "://" + this.Request.Host.Value + "/" + picturePath;
                            result.Link = link;
                        }
                    }
#endif
#if DEBUG
                    result.Success = true;
                    string picturePath = PublicFunctions.CreateLinkImageCameraFromStream(stream, cameraInfo.Index, "101", now, null, ref error);
                    string link = this.Request.Scheme + "://" + this.Request.Host.Value + "/" + picturePath;
                    result.Link = link;
#endif
                }
                else
                {
                    Stream stream = PublicFunctions.GetStreamImageFromCamera(cameraInfo.IpAddress, cameraInfo.Port.ToString(),
                        cameraInfo.UserName, cameraInfo.Password, "101", 3, ref error);
                    if (error != "")
                    {
                        result.Error = error;
                        result.Success = false;
                    }
                    else
                    {
                        string str = "";
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);

                            str = Convert.ToBase64String(memoryStream.ToArray());
                        }
                        result.Success = true;
                        result.Link = "data:image/png;base64," + str;
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetCameraPictureByCameraIndex: {ex}");
                return Ok("");
            }
        }

        [ActionName("GetCameraStreamLink")]
        [HttpPost]
        public IActionResult GetCameraStreamLink([FromBody] List<int> listCameraIndex)
        {
            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return StatusCode(500, "Token invalid");
            }

            var listCamera = context.IC_Camera.Where(t => listCameraIndex.Contains(t.Index)).ToList();
            var listData = new List<CameraStreamLinkResult>();
            for (int i = 0; i < listCamera.Count; i++)
            {
                var data = new CameraStreamLinkResult();
                data.Index = listCamera[i].Index;
                data.Type = listCamera[i].Type;
                data.Link = _linkStreamCameraFormat.Replace("{user}", listCamera[i].UserName).Replace("{pass}", listCamera[i].Password)
                    .Replace("{ip}", listCamera[i].IpAddress).Replace("{port}", listCamera[i].Port.ToString()).Replace("{channelID}", "102");

                listData.Add(data);
            }

            return Ok(listData);
        }

        [ActionName("GetANPRCamera")]
        [HttpGet]
        public IActionResult GetANPRCamera(int cameraIndex, string option)
        {
            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return Ok(new CameraANPRResult(false, "Token invalid", "", "", ""));
            }

            var cameraInfo = context.IC_Camera.Where(t => t.Index == cameraIndex).FirstOrDefault();
            if (cameraInfo == null)
            {
                return Ok(new CameraANPRResult(false, "CameraIndexInvalid", "", "", ""));
            }
            if (cameraInfo.Type != "ANPR")
            {
                return Ok(new CameraANPRResult(false, "CameraNotIsANPR", "", "", ""));
            }

            string ip = cameraInfo.IpAddress;
            string port = cameraInfo.Port.ToString();
            string channel = "101";
            string cameraUser = cameraInfo.UserName;
            string cameraPass = cameraInfo.Password;
            DateTime now = DateTime.Now;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://{ip}:{port}/ISAPI/Traffic/MNPR/channels/101");
            request.Credentials = new NetworkCredential(cameraUser, cameraPass);
            request.Method = "GET";

            bool success = false;
            string error = "";
            string licensePlate = "";
            string pictureLink = "";
            string pictureLPRLink = "";
            try
            {
                var result = request.GetResponse();
                Stream stream = result.GetResponseStream();
                var arrData = ConvertToByteArray(stream);

                Chilkat.Mime mime = new Chilkat.Mime();
                bool successLoad = mime.LoadMimeBytes(arrData);
                if (successLoad == false)
                {
                    return Ok(new CameraANPRResult(false, mime.LastErrorText, "", "", ""));
                }
                Chilkat.Mime xmlPart = null;
                xmlPart = mime.GetPart(0);
                XmlDocument doc = new XmlDocument();
                string xml = Encoding.UTF8.GetString(xmlPart.GetBodyBinary());
                xml = xml.Replace("\0", "");
                doc.LoadXml(xml);

                XmlNodeList node = doc.GetElementsByTagName("licensePlate");
                licensePlate = node[0].InnerText;

                int count = mime.NumParts;
                int indexData = 1;
                if (count == 3)
                {
                    indexData = 2;
                }
                Chilkat.Mime pic = null;

                pic = mime.GetPart(indexData);

                string rootPath = AppDomain.CurrentDomain.BaseDirectory;
                string folderPath = $"Files/ImageFromCamera/{cameraIndex }/{channel}/{now.ToString("yyyy-MM-dd")}/{now.ToString("HH")}";

                if (Directory.Exists(rootPath + folderPath) == false)
                {
                    Directory.CreateDirectory(rootPath + folderPath);
                }
                string filePath = "";
                filePath = "detection_" + DateTime.Now.ToString("mmss_ffff") + ".jpg";
                pic.SaveBody($"{rootPath}/{folderPath}/{filePath}");
                pictureLink = $"{this.Request.Scheme}://{this.Request.Host.Value}/{folderPath}/{filePath}";

                indexData = 1;
                pic = mime.GetPart(indexData);
                if (licensePlate != "unknown" && licensePlate != "" && pic != null)
                {
                    filePath = "licensePlate_" + DateTime.Now.ToString("mmss_ffff") + ".jpg";
                    pic.SaveBody($"{rootPath}/{folderPath}/{filePath}");
                    pictureLPRLink = $"{this.Request.Scheme}://{this.Request.Host.Value}/{folderPath}/{filePath}";
                }
                success = true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError($"{ex}");
            }
            return Ok(new CameraANPRResult(success, error, pictureLink, pictureLPRLink, licensePlate));
        }

        // GET api/<HR_EmployeeInfoController>/5
        //[Authorize]
        [ActionName("Get_HR_EmployeeInfo")]
        [HttpGet]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Get_HR_EmployeeInfo(string employeeATID)
        {
            _logger.LogError($"V1 Get_HR_EmployeeInfo employeeATID {employeeATID}");
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employee = await _HR_EmployeeInfoService.GetEmployeeInfo(employeeATID, user.CompanyIndex);
            return ApiOk(employee);
        }

        [ActionName("UploadUserByMealCard")]
        [HttpPost]
        public async Task<IActionResult> UploadUserByMealCard([FromBody] CommandRequestPublic param)
        {
            param.AuthenMode = new List<string>() { "FullAccessRight" };
            var visitorDepartmentIndex = GetVisitorDepartmentIndex(param.CompanyIndex);
            var defaultDepartmentECMSConfig = context.IC_Config.FirstOrDefault(t => t.EventType == ConfigAuto.ECMS_DEFAULT_MEAL_CARD_DEPARTMENT.ToString());
            if (defaultDepartmentECMSConfig != null && !string.IsNullOrWhiteSpace(defaultDepartmentECMSConfig.CustomField))
            {
                var deserializeParam = JsonConvert.DeserializeObject<IntegrateLogParam>(defaultDepartmentECMSConfig.CustomField);
                visitorDepartmentIndex = deserializeParam.DepartmentIndex.HasValue ? deserializeParam.DepartmentIndex.Value : 0;
            }
            var employeeParam = new IC_EmployeeParamDTO()
            {
                CardNumber = param.CardNumber,
                EmployeeATID = param.CustomerID,
                EmployeeCode = param.RegisterCode,
                FullName = param.CustomerName,
                Gender = (short)(param.Gender ? 1 : 0),
                JoinedDate = param.RegisterTime,
                NameOnMachine = param.CustomerName,
                DepartmentIndex = visitorDepartmentIndex,
                ImageUpload = param.CustomerFaceBase64,
                Password = "",
                DepartmentCode = "Cards",
                PositionName = param.PositionName
            };

            var isAddSuccess = await AddEmployeeByMealCard(employeeParam, param.CompanyIndex, param.Action);
            if (!isAddSuccess)
                return BadRequest("Can't add card");

            return Ok();
        }

        private async Task<bool> AddEmployeeByMealCard(IC_EmployeeParamDTO param, int companyIndex, string action)
        {
            var check = GetUserInfo();
            if (check == null) return false;

            var checkuser = await _HR_UserService.FirstOrDefaultAsync(x => x.CompanyIndex == companyIndex && x.EmployeeATID == param.EmployeeATID);
            if (checkuser != null)
                return false;

            param.EmployeeATID = param.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0');

            var user = new HR_User()
            {
                EmployeeATID = param.EmployeeATID,
                CompanyIndex = companyIndex,
                FullName = param.FullName,
                Gender = param.Gender,
                EmployeeType = (int)EmployeeType.Employee,
                CreatedDate = param.JoinedDate,
                UpdatedDate = param.JoinedDate,
                UpdatedUser = action
            };

            var employeeInfo = new HR_EmployeeInfo()
            {
                EmployeeATID = param.EmployeeATID,
                CompanyIndex = companyIndex,
                JoinedDate = param.JoinedDate,
                UpdatedDate = param.JoinedDate,
                UpdatedUser = action
            };

            var userMaster = new IC_UserMasterDTO()
            {
                EmployeeATID = param.EmployeeATID,
                CompanyIndex = companyIndex,
                CardNumber = param.CardNumber,
                NameOnMachine = param.NameOnMachine,
                Privilege = 0,
                FaceIndex = 50,
                CreatedDate = param.JoinedDate,
                UpdatedDate = param.JoinedDate,
                UpdatedUser = action
            };
            if (!string.IsNullOrWhiteSpace(param.ImageUpload))
            {
                param.ImageUpload = param.ImageUpload.ToString().Substring(param.ImageUpload.ToString().IndexOf(',') + 1);
                userMaster.FaceV2_TemplateBIODATA = param.ImageUpload;
                userMaster.FaceV2_Content = param.ImageUpload;
                userMaster.FaceV2_Size = param.ImageUpload.Length;
                userMaster.FaceV2_Type = 9;
                userMaster.FaceV2_No = 0;
                userMaster.FaceV2_MajorVer = 5;
                userMaster.FaceV2_MinorVer = 8;
                userMaster.FaceV2_Valid = 1;
                userMaster.FaceV2_Format = 0;
                userMaster.FaceV2_Index = 0;
                userMaster.FaceV2_Duress = 0;
            }

            var cardActive = await _HR_CardNumberInfoService.CheckCardActiveOtherEmployee(new HR_CardNumberInfo
            { CardNumber = param.CardNumber, CompanyIndex = companyIndex, EmployeeATID = param.EmployeeATID }, companyIndex);

            if (cardActive)
            {
                return false;
            }
            var cardNumberInfo = new HR_CardNumberInfo()
            {
                EmployeeATID = param.EmployeeATID,
                CardNumber = param.CardNumber,
                IsActive = true,
                CreatedDate = DateTime.Now,
                UpdatedDate = param.JoinedDate,
                CompanyIndex = companyIndex,
                UpdatedUser = action
            };

            var workingInfo = new IC_WorkingInfoDTO()
            {
                CompanyIndex = companyIndex,
                EmployeeATID = param.EmployeeATID,
                DepartmentIndex = param.DepartmentIndex,
                FromDate = param.JoinedDate,
                ToDate = null,
                Status = (short)TransferStatus.Approve,
                UpdatedDate = param.JoinedDate,
                UpdatedUser = action,
                PositionName = param.PositionName
            };
            try
            {
                context.HR_User.Add(user);
                context.HR_EmployeeInfo.Add(employeeInfo);
                context.HR_CardNumberInfo.Add(cardNumberInfo);

                _IIC_WorkingInfoLogic.CheckUpdateOrInsert(workingInfo);
                _iC_UserMasterLogic.CheckExistedOrCreate(userMaster);

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddEmployeeByMealCard: {ex}");
                return false;
            }
        }

        [ActionName("DeleteUserByMealCard")]
        [HttpPost]
        public async Task<IActionResult> DeleteUserByMealCard([FromBody] DeleteEmployeeModel param)
        {
            try
            {
                var empLookup = param.ListemployeeATID.ToHashSet();
                await _HR_EmployeeInfoService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == param.CompanyIndex);
                await _HR_UserService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == param.CompanyIndex);
                await _HR_CardNumberInfoService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == param.CompanyIndex);
                await _IC_WorkingInfoService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == param.CompanyIndex);
                await _IC_EmployeeTransferService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == param.CompanyIndex);
                await _IC_UserMasterService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == param.CompanyIndex);

                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteUser: {ex}");
                return BadRequest("BadRequest");
            }
        }

        [ActionName("UpdateUserByMealCard")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserByMealCard([FromBody] UpdateEmployeeModel param)
        {
            try
            {
                var dataUser = await context.HR_User.FirstOrDefaultAsync(x => x.CompanyIndex == param.CompanyIndex && x.EmployeeATID == param.EmployeeATID);
                dataUser.FullName = param.CustomerName;
                dataUser.UpdatedDate = DateTime.Now;
                dataUser.UpdatedUser = param.UpdatedUser;

                context.HR_User.Update(dataUser);

                var dataUserMaster = await context.IC_UserMaster.FirstOrDefaultAsync(x => x.CompanyIndex == param.CompanyIndex && x.EmployeeATID == param.EmployeeATID);
                dataUserMaster.NameOnMachine = param.CustomerName;
                dataUserMaster.UpdatedDate = DateTime.Now;
                dataUserMaster.UpdatedUser = param.UpdatedUser;

                context.IC_UserMaster.Update(dataUserMaster);

                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateUserByMealCard: {ex}");
                return BadRequest("BadRequest");
            }
        }

        [ActionName("UploadUsers")]
        [HttpPost]
        public async Task<IActionResult> UploadUsers([FromBody] CommandRequestPublic param)
        {
            //param.CustomerFaceBase64 // base64 ảnh đăng ký face . Nếu trống => ko có ảnh
            param.AuthenMode = new List<string>() { "FullAccessRight" };
            var visitorDepartmentIndex = GetVisitorDepartmentIndex(param.CompanyIndex);
            var employeeParam = new IC_EmployeeParamDTO()
            {
                CardNumber = param.CardNumber,
                EmployeeATID = param.CustomerID,
                EmployeeCode = param.RegisterCode,
                FullName = param.CustomerName,
                Gender = (short)(param.Gender ? 1 : 0),
                JoinedDate = param.RegisterTime,
                NameOnMachine = param.CustomerName,
                DepartmentIndex = visitorDepartmentIndex,
                ImageUpload = param.CustomerFaceBase64,
                Password = "",
                DepartmentCode = "Visitor",
            };
            var isAddSuccess = await AddEmployee(employeeParam, param.CompanyIndex, param.UserName);

            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return StatusCode(500, "Token invalid");
            }
            if (param.ListSerial == null || param.ListSerial.Count == 0 || param.ListUser == null || param.ListUser.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            var lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                var addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = param.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "SystemCommandStatus", Value = false });
                addedParams.Add(new AddedParam { Key = "CommandName", Value = CommandAction.UploadUsers });
                addedParams.Add(new AddedParam { Key = "ListSerialNumber", Value = lsSerialHw });
                var listCommandHasExisted = _iC_SystemCommandLogic.GetMany(addedParams);
                if (listCommandHasExisted != null && listCommandHasExisted.Count > 0)
                {
                    lsSerialHw = lsSerialHw.Where(u => listCommandHasExisted.Where(t => t.SerialNumber == u).Count() == 0).ToList();
                }
                if (lsSerialHw.Count > 0)
                {
                    var paramUserOnMachine = new IC_UserinfoOnMachineParam()
                    {
                        ListEmployeeaATID = param.ListUser,
                        CompanyIndex = param.CompanyIndex,
                        AuthenMode = string.Join(",", param.AuthenMode),
                        ListSerialNumber = param.ListSerial,
                        FullInfo = true
                    };

                    var lstUser = _IIC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                    var commandParam = new IC_CommandParamDTO()
                    {
                        IsOverwriteData = false,
                        Action = CommandAction.UploadUsers,
                        AuthenMode = string.Join(",", param.AuthenMode),
                        FromTime = new DateTime(2000, 1, 1),
                        ToTime = DateTime.Now,
                        ListEmployee = lstUser,
                        ListSerialNumber = lsSerialHw,
                        Privilege = GlobalParams.DevicePrivilege.SDKStandardRole,
                        ExternalData = "",
                    };

                    var lstCmd = _IIC_CommandLogic.CreateListCommands(commandParam);

                    if (lstCmd != null && lstCmd.Count() > 0)
                    {
                        var groupCommand = new IC_GroupCommandParamDTO();
                        groupCommand.CompanyIndex = param.CompanyIndex;
                        groupCommand.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                        groupCommand.ListCommand = lstCmd;
                        groupCommand.GroupName = GroupName.UploadUsers.ToString();
                        groupCommand.EventType = "";
                        _IIC_CommandLogic.CreateGroupCommands(groupCommand);
                    }
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            return Ok();
        }

        private int GetVisitorDepartmentIndex(int companyIndex)
        {
            ConfigObject config = ConfigObject.GetConfig(cache);
            if (config.IntegrateDBOther == false)
            {
                var department = context.IC_Department.FirstOrDefault(t => t.CompanyIndex == companyIndex && t.Code == "Visitor" && t.IsInactive != true);
                return department != null ? department.Index : 0;
            }
            else
            {
                var department = otherContext.HR_Department.FirstOrDefault(t => t.CompanyIndex == companyIndex && t.Code == "Visitor");
                return department != null ? Convert.ToInt32(department.Index) : 0;
            }
        }

        private async Task<bool> AddEmployee(IC_EmployeeParamDTO param, int companyIndex, string userName)
        {
            param = (IC_EmployeeParamDTO)StringHelper.RemoveWhiteSpace(param);

            var checkData = context.HR_User.Where(t => t.CompanyIndex == companyIndex && t.EmployeeATID == param.EmployeeATID).FirstOrDefault();
            if (checkData != null)
            {
                checkData.FullName = param.FullName;
                checkData.Gender = param.Gender;
                checkData.UpdatedDate = DateTime.Now;
                checkData.UpdatedUser = userName;
                context.HR_User.Update(checkData);

                this.CheckCardNumberInUserInfo(param.EmployeeATID, param.CardNumber, param.NameOnMachine, companyIndex, userName);

                context.SaveChanges();

                var userMaster = new IC_UserMasterDTO();
                userMaster.EmployeeATID = checkData.EmployeeATID;/*.PadLeft(config.MaxLenghtEmployeeATID, '0');*/
                userMaster.CompanyIndex = checkData.CompanyIndex;
                userMaster.CardNumber = param.CardNumber;
                userMaster.NameOnMachine = param.NameOnMachine;
                userMaster.UpdatedDate = DateTime.Now;
                userMaster.UpdatedUser = checkData.UpdatedUser;
                if (!string.IsNullOrWhiteSpace(param.ImageUpload))
                {
                    param.ImageUpload = param.ImageUpload.ToString().Substring(param.ImageUpload.ToString().IndexOf(',') + 1);
                    userMaster.FaceV2_TemplateBIODATA = param.ImageUpload;
                    userMaster.FaceV2_Content = param.ImageUpload;
                    userMaster.FaceV2_Size = param.ImageUpload.Length;
                    userMaster.FaceV2_Type = 9;
                    userMaster.FaceV2_No = 0;
                    userMaster.FaceV2_MajorVer = 5;
                    userMaster.FaceV2_MinorVer = 8;
                    userMaster.FaceV2_Valid = 1;
                    userMaster.FaceV2_Format = 0;
                    userMaster.FaceV2_Index = 0;
                    userMaster.FaceV2_Duress = 0;
                }
                await _iC_UserMasterLogic.SaveAndAddMoreList(new List<IC_UserMasterDTO> { userMaster });

                var workingInfo = new IC_WorkingInfoDTO()
                {
                    CompanyIndex = companyIndex,
                    EmployeeATID = param.EmployeeATID,
                    DepartmentIndex = param.DepartmentIndex,
                    FromDate = param.JoinedDate,
                    ToDate = null,
                    UpdatedDate = DateTime.Now,
                    UpdatedUser = userName,
                    Status = (short)TransferStatus.Approve,
                    PositionName = param.PositionName
                };
                _IIC_WorkingInfoLogic.CheckUpdateOrInsert(workingInfo);

                if (param.DepartmentIndex > 0)
                {
                    await _IIC_CommandLogic.SyncWithEmployee(new List<string> { param.EmployeeATID }, companyIndex);
                }
            }
            else
            {
                var employee = new HR_User();
                employee.EmployeeATID = param.EmployeeATID;
                employee.CompanyIndex = companyIndex;
                employee.EmployeeCode = param.EmployeeCode;
                employee.FullName = param.FullName;
                //employee.CardNumber = param.CardNumber;
                //employee.NameOnMachine = param.NameOnMachine;
                employee.Gender = param.Gender;
                //employee.DepartmentIndex = param.DepartmentIndex;
                employee.CreatedDate = DateTime.Now;
                employee.UpdatedDate = DateTime.Now;
                employee.UpdatedUser = userName;
                //employee.JoinedDate = param.JoinedDate;
                context.HR_User.Add(employee);

                this.CheckCardNumberInUserInfo(param.EmployeeATID, param.CardNumber, param.NameOnMachine, companyIndex, userName);

                context.SaveChanges();

                var userMaster = new IC_UserMasterDTO();
                userMaster.EmployeeATID = employee.EmployeeATID;/*.PadLeft(config.MaxLenghtEmployeeATID, '0');*/
                userMaster.CompanyIndex = employee.CompanyIndex;
                //userMaster.CardNumber = employee.CardNumber;
                //userMaster.NameOnMachine = employee.NameOnMachine;
                userMaster.Privilege = 0;
                userMaster.FaceIndex = 50;
                userMaster.CreatedDate = DateTime.Now;
                userMaster.UpdatedUser = employee.UpdatedUser;
                if (!string.IsNullOrWhiteSpace(param.ImageUpload))
                {
                    param.ImageUpload = param.ImageUpload.ToString().Substring(param.ImageUpload.ToString().IndexOf(',') + 1);
                    userMaster.FaceV2_TemplateBIODATA = param.ImageUpload;
                    userMaster.FaceV2_Content = param.ImageUpload;
                    userMaster.FaceV2_Size = param.ImageUpload.Length;
                    userMaster.FaceV2_Type = 9;
                    userMaster.FaceV2_No = 0;
                    userMaster.FaceV2_MajorVer = 5;
                    userMaster.FaceV2_MinorVer = 8;
                    userMaster.FaceV2_Valid = 1;
                    userMaster.FaceV2_Format = 0;
                    userMaster.FaceV2_Index = 0;
                    userMaster.FaceV2_Duress = 0;
                }
                _iC_UserMasterLogic.CheckExistedOrCreate(userMaster);

                var workingInfo = new IC_WorkingInfoDTO()
                {
                    CompanyIndex = companyIndex,
                    EmployeeATID = param.EmployeeATID,
                    DepartmentIndex = param.DepartmentIndex,
                    FromDate = param.JoinedDate,
                    ToDate = null,
                    UpdatedDate = DateTime.Now,
                    UpdatedUser = userName,
                    Status = (short)TransferStatus.Approve,
                    PositionName = param.PositionName
                };
                _IIC_WorkingInfoLogic.CheckUpdateOrInsert(workingInfo);

                if (param.DepartmentIndex > 0)
                {
                    await _IIC_CommandLogic.SyncWithEmployee(new List<string> { param.EmployeeATID }, companyIndex);
                }

            }
            await context.SaveChangesAsync();
            return true;
        }

        private void CheckCardNumberInUserInfo(string pEmployeeATID, string pCardNumber, string UserNameDevice, int pCompanyIndex, string username)
        {
            IC_UserInfo us = new IC_UserInfo();
            try
            {
                var userInfo = context.IC_UserInfo.Where(t => t.EmployeeATID == pEmployeeATID && t.CompanyIndex == pCompanyIndex).ToList();
                if (userInfo.Count == 0)
                {
                    us = new IC_UserInfo()
                    {
                        EmployeeATID = pEmployeeATID,
                        CompanyIndex = pCompanyIndex,
                        SerialNumber = "",
                        UserName = UserNameDevice,
                        Password = "",
                        CardNumber = pCardNumber,
                        Privilege = 0,
                        Reserve1 = "",
                        Reserve2 = 0,
                        UpdatedDate = DateTime.Now,
                        CreatedDate = DateTime.Now,
                        UpdatedUser = username
                    };
                    context.IC_UserInfo.Add(us);
                }
                else
                {
                    foreach (var item in userInfo)
                    {
                        item.UserName = UserNameDevice;
                        item.CardNumber = pCardNumber;
                        item.UpdatedDate = DateTime.Now;
                        item.UpdatedUser = username;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            //context.SaveChanges();
        }

        private void CreateGroupCommand(int pCompanyIndex, string pUserName, string pGroupName, List<CommandResult> pListCommands, string pEventType)
        {
            CommandProcess.CreateGroupCommand(context, cache, pCompanyIndex, pUserName, pGroupName, "", pListCommands, pEventType);
        }

        private List<CommandResult> CreateListCommand(EPAD_Context context, List<string> listSerial, CommandAction pAction, DateTime pFromTime, DateTime pToTime, List<UserInfoOnMachine> pListUsers)
        {
            return CommandProcess.CreateListCommands(context, listSerial, pAction, "", pFromTime, pToTime, pListUsers, false);
        }

        private List<UserInfoOnMachine> GetListUserInfoOnMachine(List<string> pEmps, int pCompanyIndex, bool pFullInfo, string pMachineSerial)
        {
            EPAD_Context db = context;
            var lstUser = new List<UserInfoOnMachine>();
            string empATID = "";
            string serial = "";
            foreach (string user in pEmps)
            {
                if (user.Split('-').Length > 1)
                {
                    empATID = user.Split('-')[0];
                    serial = user.Split('-')[1];
                }
                else
                {
                    empATID = user;
                    serial = "";
                }

                UserInfoOnMachine userInfoOnMachine = new UserInfoOnMachine(empATID);
                if (!pFullInfo)
                {
                    lstUser.Add(userInfoOnMachine);
                    continue;
                }
                IC_UserInfo userInfo = db.IC_UserInfo.FirstOrDefault(x => x.EmployeeATID == empATID && x.SerialNumber == serial);
                if (userInfo != null)
                {
                    userInfoOnMachine.CardNumber = userInfo.CardNumber == null ? "0" : userInfo.CardNumber;
                    userInfoOnMachine.PasswordOndevice = userInfo.Password == null ? "" : userInfo.Password;
                    userInfoOnMachine.NameOnDevice = userInfo.UserName == null ? "" : userInfo.UserName;
                    if (userInfo.Privilege != null)
                        userInfoOnMachine.Privilege = userInfo.Privilege.Value;
                }
                else
                {
                    userInfoOnMachine.CardNumber = "0";
                }

                var lstFinger = (from f in db.IC_UserFinger
                                 where f.CompanyIndex == pCompanyIndex && f.EmployeeATID == empATID && f.SerialNumber == serial
                                 group new { FingerIndex = f.FingerIndex, FingerTemplate = f.FingerData } by f.SerialNumber into gr
                                 select new { SerialNumber = gr.Key, FingerInfos = gr.ToList() }).ToList();
                var firstFingerInfo = lstFinger.FirstOrDefault();
                if (firstFingerInfo != null)
                {
                    var lstFingerInfo = (from fi in firstFingerInfo.FingerInfos
                                         select new FingerInfo()
                                         {
                                             FingerIndex = fi.FingerIndex,
                                             FingerTemplate = fi.FingerTemplate
                                         }).ToList();

                    userInfoOnMachine.FingerPrints = lstFingerInfo;
                }

                var faceTemplate = db.IC_UserFaceTemplate.FirstOrDefault(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == empATID && x.SerialNumber == serial);
                if (faceTemplate != null)
                {
                    var faceInfo = new FaceInfo()
                    {
                        FaceTemplate = faceTemplate.FaceTemplate
                    };
                    userInfoOnMachine.Face = faceInfo;
                }

                var faceTemplateV2 = db.IC_UserFaceTemplate_v2.FirstOrDefault(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == empATID && x.SerialNumber == serial && x.SerialNumber == serial);
                if (faceTemplateV2 != null && faceTemplateV2.Content != null)
                {
                    if (faceTemplateV2.TemplateBIODATA == null)
                    {
                        faceTemplateV2.TemplateBIODATA = "";
                    }
                    var faceInfoV2 = new FaceInfoV2();
                    faceInfoV2.No = faceTemplateV2.No;
                    faceInfoV2.Index = faceTemplateV2.Index;
                    faceInfoV2.Valid = faceTemplateV2.Valid;
                    faceInfoV2.Duress = faceTemplateV2.Duress;
                    faceInfoV2.Type = faceTemplateV2.Type;
                    faceInfoV2.MajorVer = faceTemplateV2.MajorVer;
                    faceInfoV2.MinorVer = faceTemplateV2.MinorVer;
                    faceInfoV2.Format = faceTemplateV2.Format;
                    faceInfoV2.TemplateBIODATA = faceTemplateV2.TemplateBIODATA;
                    faceInfoV2.Size = faceTemplateV2.Size;
                    faceInfoV2.Content = faceTemplateV2.Content;
                    userInfoOnMachine.FaceInfoV2 = faceInfoV2;
                }
                lstUser.Add(userInfoOnMachine);
            }
            return lstUser;
        }

        private bool ListSerialCheckHardWareLicense(List<string> lsSerial, ref List<string> lsSerialHw)
        {
            int dem = 0;
            foreach (var serial in lsSerial)
            {
                bool checkls = cache.HaveHWLicense(serial);
                if (checkls)
                {
                    lsSerialHw.Add(serial);
                }
                else
                {
                    dem++;
                }
            }
            if (dem > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private Byte[] ConvertToByteArray(Stream sourceStream)
        {
            var bufferSize = 2048;
            var buffer = new byte[bufferSize];
            var result = new List<byte>();
            try
            {
                var readBytes = 0;

                while ((readBytes = sourceStream.Read(buffer)) != 0)
                {
                    for (int i = 0; i < readBytes; i++)
                    {
                        result.Add(buffer[i]);
                    }
                }
            }
            catch (IOException ex)
            {
            }
            return result.ToArray();
        }

        [ActionName("UploadCustomer")]
        [HttpPost]
        public async Task<IActionResult> UploadCustomer([FromBody] CommandCustomerRequest param)
        {
            //param.CustomerFaceBase64 // base64 ảnh đăng ký face . Nếu trống => ko có ảnh
            param.AuthenMode = new List<string>() { "FullAccessRight" };
            var visitorDepartmentIndex = GetVisitorDepartmentIndex(param.CompanyIndex);

            var employeeParam = new HR_CustomerInfoResult()
            {
                CardNumber = param.CardNumber,
                EmployeeATID = param.EmployeeATID,
                EmployeeCode = param.RegisterCode,
                FullName = param.CustomerName,
                Gender = (short)(param.Gender ? 1 : 0),
                NameOnMachine = param.CustomerName,
                Phone = param.Phone,
                Address = param.Address,
                BikeDescription = param.BikeDescription,
                BikeModel = param.BikeModel,
                BikePlate = param.BikePlate,
                BikeType = (short)param.BikeType,
                Company = param.Company,
                CompanyIndex = param.CompanyIndex,
                ContactPerson = param.CustomerID,
                ContactPersonATIDs = param.ContactPersonATIDs,
                CreatedDate = param.CreatedDate,
                Email = param.Email,
                ExtensionTime = param.ExtensionTime,
                FromTime = param.FromTime,
                IsVIP = param.IsVIP,
                LicensePlateBackImage = param.LicensePlateBackImage,
                LicensePlateFrontImage = param.LicensePlateFrontImage,
                NRICBackImage = param.NRICBackImage,
                NRICFrontImage = param.NRICFrontImage,
                NumberOfContactPerson = param.NumberOfContactPerson,
                Password = "",
                RegisterCode = param.RegisterCode,
                ToTime = param.ToTime,
                RegisterTime = param.RegisterTime,
                RulesCustomerIndex = param.RulesCustomerIndex,
                UpdatedDate = param.UpdatedDate,
                UpdatedUser = param.UpdatedUser,
                WorkContent = param.WorkContent,
                GoInSystem = true,
                DataStorageTime = param.DataStorageTime,
                AccompanyingPersonList = null,
                CustomerID = param.CustomerID,
                Avatar = param.CustomerFaceImage,
                NRIC = param.NRIC,
                EmployeeType = (int)EmployeeType.Guest,
                IdentityImage = param.IdentityImage,
            };

            var isAddSuccess = await AddCustomer(employeeParam);

            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return StatusCode(500, "Token invalid");
            }
            if (param.ListSerial == null || param.ListSerial.Count == 0 || param.ListUser == null || param.ListUser.Count == 0)
            {
                return BadRequest("BadRequest");
            }

            var lsSerialHw = new List<string>();
            bool checkHw = ListSerialCheckHardWareLicense(param.ListSerial, ref lsSerialHw);
            if (lsSerialHw != null && lsSerialHw.Count > 0 && checkHw)
            {
                var addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = param.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "SystemCommandStatus", Value = false });
                addedParams.Add(new AddedParam { Key = "CommandName", Value = CommandAction.UploadUsers });
                addedParams.Add(new AddedParam { Key = "ListSerialNumber", Value = lsSerialHw });
                var listCommandHasExisted = _iC_SystemCommandLogic.GetMany(addedParams);
                if (listCommandHasExisted != null && listCommandHasExisted.Count > 0)
                {
                    lsSerialHw = lsSerialHw.Where(u => listCommandHasExisted.Where(t => t.SerialNumber == u).Count() == 0).ToList();
                }
                if (lsSerialHw.Count > 0)
                {
                    var paramUserOnMachine = new IC_UserinfoOnMachineParam()
                    {
                        ListEmployeeaATID = param.ListUser,
                        CompanyIndex = param.CompanyIndex,
                        AuthenMode = string.Join(",", param.AuthenMode),
                        ListSerialNumber = param.ListSerial,
                        FullInfo = true
                    };

                    var lstUser = _IIC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                    var commandParam = new IC_CommandParamDTO()
                    {
                        IsOverwriteData = false,
                        Action = CommandAction.UploadUsers,
                        AuthenMode = string.Join(",", param.AuthenMode),
                        FromTime = new DateTime(2000, 1, 1),
                        ToTime = DateTime.Now,
                        ListEmployee = lstUser,
                        ListSerialNumber = lsSerialHw,
                        Privilege = GlobalParams.DevicePrivilege.SDKStandardRole,
                        ExternalData = "",
                    };

                    var lstCmd = _IIC_CommandLogic.CreateListCommands(commandParam);

                    if (lstCmd != null && lstCmd.Count() > 0)
                    {
                        var groupCommand = new IC_GroupCommandParamDTO();
                        groupCommand.CompanyIndex = param.CompanyIndex;
                        groupCommand.UserName = UpdatedUser.SYSTEM_AUTO.ToString();
                        groupCommand.ListCommand = lstCmd;
                        groupCommand.GroupName = GroupName.UploadUsers.ToString();
                        groupCommand.EventType = "";
                        _IIC_CommandLogic.CreateGroupCommands(groupCommand);
                    }
                }
            }
            else
            {
                return BadRequest("DeviceNotLicence");
            }

            return Ok();
        }

        private async Task<bool> AddCustomer(HR_CustomerInfoResult value)
        {
            var checkuser = GetUserInfo();
            if (checkuser == null) return false;

            value.EmployeeATID = value.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0');

            //var cardActive = await _HR_CardNumberInfoService.FirstOrDefaultAsync(x => x.CardNumber == value.CardNumber && x.IsActive == true && x.CompanyIndex == checkuser.CompanyIndex);
            //if (cardActive != null)
            //{
            //    return false;
            //}
            //var check = await _HR_UserService.FirstOrDefaultAsync(x => x.CompanyIndex == checkuser.CompanyIndex && x.EmployeeATID == value.EmployeeATID);
            //if (check != null)
            //    return false;

            value.CompanyIndex = checkuser.CompanyIndex;

            var cardNumberInfo = _Mapper.Map<HR_CustomerInfoResult, HR_CardNumberInfo>(value);
            var customerInfo = _Mapper.Map<HR_CustomerInfoResult, HR_CustomerInfo>(value);

            var user = new HR_User()
            {
                EmployeeATID = value.EmployeeATID,
                CompanyIndex = value.CompanyIndex,
                FullName = value.FullName,
                CreatedDate = value.CreatedDate,
                EmployeeCode = value.EmployeeCode,
                Gender = value.Gender,
                UpdatedDate = value.UpdatedDate,
                UpdatedUser = value.UpdatedUser,
                EmployeeType = (int)EmployeeType.Guest
            };

            var userMaster = new IC_UserMasterDTO()
            {
                AuthenMode = "FullAccessRight",
                CardNumber = value.CardNumber,
                UpdatedUser = value.UpdatedUser,
                CompanyIndex = value.CompanyIndex,
                CreatedDate = value.CreatedDate,
                EmployeeATID = value.EmployeeATID,
                NameOnMachine = value.NameOnMachine,
                Password = "",
                UpdatedDate = value.UpdatedDate,
                Privilege = 0,
                FaceIndex = 50
            };
            if (!string.IsNullOrWhiteSpace(value.Avatar))
            {
                value.Avatar = value.Avatar.ToString().Substring(value.Avatar.ToString().IndexOf(',') + 1);
                userMaster.FaceV2_TemplateBIODATA = value.Avatar;
                userMaster.FaceV2_Content = value.Avatar;
                userMaster.FaceV2_Size = value.Avatar.Length;
                userMaster.FaceV2_Type = 9;
                userMaster.FaceV2_No = 0;
                userMaster.FaceV2_MajorVer = 5;
                userMaster.FaceV2_MinorVer = 8;
                userMaster.FaceV2_Valid = 1;
                userMaster.FaceV2_Format = 0;
                userMaster.FaceV2_Index = 0;
                userMaster.FaceV2_Duress = 0;
            }
            try
            {
                await _HR_UserService.CheckUserActivedOrCreate(user, checkuser.CompanyIndex);
                //context.HR_CustomerInfo.Add(customerInfo);
                await _HR_CustomerInfoService.CheckCustomerInfoActivedOrCreate(customerInfo, checkuser.CompanyIndex);
                await _HR_CardNumberInfoService.CheckCardNumberInforActivedOrCreate(cardNumberInfo, checkuser.CompanyIndex);
                _IIC_UserMasterLogic.CheckExistedOrCreate(userMaster);
                await SaveChangeAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddCustomer: {ex}");
                return false;
            }
        }

        [ActionName("CheckExistingMealCardByIdentityNumberAndCardNumber")]
        [HttpGet]
        public async Task<IActionResult> CheckExistingMealCardByIdentityNumberAndCardNumber(string pIdentityNumber, string pCardNumber)
        {
            var result1 = 0; var result2 = 0;
            var identity = await _DbContext.HR_CardNumberInfo.Where(e => e.EmployeeATID == pIdentityNumber).AnyAsync();
            if (identity)
                result1 = 1;
            var cardNumber = await _DbContext.HR_CardNumberInfo.Where(e => e.CardNumber == pCardNumber).AnyAsync();
            if (cardNumber)
                result2 = 2;

            var total = result1 + result2;

            return ApiOk(total);
        }
    }

    #region Request parameters
    public class CommandRequestPublic : IC_CommandRequestDTO
    {
        public int CompanyIndex { get; set; }
        public string UserName { get; set; }
        public string RegisterCode { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CardNumber { get; set; }
        public bool Gender { get; set; }
        public DateTime RegisterTime { get; set; }
        public string CustomerFaceBase64 { get; set; }
        public string PositionName { get; set; }
    }

    public class DownloadLogRequest
    {
        public List<string> Serials { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Key { get; set; }
    }

    public class RelayControllerParam
    {
        public int ControllerIndex { get; set; }
        public List<int> ListChannel { get; set; }
        public bool SetOn { get; set; }
        public bool AutoOff { get; set; }
        public short Second { get; set; }
        public short ChannelInputGood { get; set; }
        public short ChannelInputNotGood { get; set; }
    }

    public class RelayControllerParamVstar
    {
        public int ControllerIndex { get; set; }
        public List<int> ListChannel { get; set; }
        public bool SetOn { get; set; }
        public bool AutoOff { get; set; }
        public short Second { get; set; }
        public short ChannelInputGood { get; set; }
        public short ChannelInputNotGood { get; set; }
        public short AccessTime { get; set; }
        public RelayControllerChannel RelayControllerChannel { get; set; }
    }

    public class RelayControllerChannel
    {
        public int ChannelOpen { get; set; }
        public int ChannelClose { get; set; }
        public int ChannelLockOpen { get; set; }
        public int ChannelLockClose { get; set; }
    }

    public class CameraPublicResult
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Serial { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Type { get; set; }
    }

    public class CameraPictureResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public string Link { get; set; }
    }

    public class CameraStreamLinkResult
    {
        public int Index { get; set; }
        public string Link { get; set; }
        public string Type { get; set; }
    }

    public class CameraANPRResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public string Picture { get; set; }
        public string LCR_Picture { get; set; }
        public string LCR { get; set; }
        public CameraANPRResult(bool pSuccess, string pError, string pPicture, string pLCR_Picture, string pLCR)
        {
            Success = pSuccess;
            Error = pError;
            Picture = pPicture;
            LCR_Picture = pLCR_Picture;
            LCR = pLCR;
        }
    }
    public class GC_Customer
    {
        public int Index { get; set; }
        public string EmployeeATID { get; set; }
        public string RegisterCode { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNRIC { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerCompany { get; set; }
        public bool Gender { get; set; }
        public string CustomerAddress { get; set; }
        public bool IsVip { get; set; }
        public int DataStorageTime { get; set; }


        public string ContactPersonATIDs { get; set; }
        public string AccompanyingPersonList { get; set; }
        public short NumberOfContactPerson { get; set; }
        public DateTime RegisterTime { get; set; }
        //[Column(TypeName = "varchar(30)")]
        //public string CardNumber { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public DateTime? ExtensionTime { get; set; }
        public string WorkContent { get; set; }

        public short BikeType { get; set; }
        public string BikeModel { get; set; }
        public string BikePlate { get; set; }
        public string BikeDescription { get; set; }

        public string CustomerImage { get; set; }
        public string NRICFrontImage { get; set; }
        public string NRICBackImage { get; set; }
        public string LicensePlateFrontImage { get; set; }
        public string LicensePlateBackImage { get; set; }

        public bool? GoInSystem { get; set; }
        public int RulesCustomerIndex { get; set; }
        public int CompanyIndex { get; set; }
    }
    public class EmployeeBasicInfo
    {
        public string Avatar { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public bool? Gender { get; set; }
        public DateTime? JoinedDate { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public long? DepartmentIndex { get; set; }
        public string PositionName { get; set; }
        public int CompanyIndex { get; set; }
    }
    #endregion
}