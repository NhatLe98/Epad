using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EPAD_Data.Models.HR;
using EPAD_Common.Extensions;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HR_UserController : ApiControllerBase
    {
        private readonly IHR_UserService _HR_UserService;
        private readonly IIC_PlanDockService _IC_PlanDockService;
        private readonly IGC_TruckDriverLogService _GC_TruckDriverLogService;
        private readonly IIC_DepartmentService _IC_DepartmentService;
        private readonly IIC_CompanyService _IC_CompanyService;
        private readonly IIC_WorkingInfoService _IC_WorkingInfoService;
        private readonly IIC_EmployeeTransferLogic _iC_EmployeeTransferLogic;
        private readonly IIC_EmployeeLogic _IIC_EmployeeLogic;
        private readonly ILogger _logger;
        private IMemoryCache cache;
        private ConfigObject _config;
        private EPAD_Context context;
        private readonly string _configClientName;
        private readonly ezHR_Context otherContext;
        private readonly string departmentCodeConfig;
        private readonly string departmentNameConfig;

        public HR_UserController(IServiceProvider pProvider) : base(pProvider)
        {
            context = TryResolve<EPAD_Context>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _IC_PlanDockService = TryResolve<IIC_PlanDockService>();
            _GC_TruckDriverLogService = TryResolve<IGC_TruckDriverLogService>();
            _IC_DepartmentService = TryResolve<IIC_DepartmentService>();
            _IC_CompanyService = TryResolve<IIC_CompanyService>();
            _IC_WorkingInfoService = TryResolve<IIC_WorkingInfoService>();
            _iC_EmployeeTransferLogic = TryResolve<IIC_EmployeeTransferLogic>();
            _IIC_EmployeeLogic = TryResolve<IIC_EmployeeLogic>();
            _logger = _LoggerFactory.CreateLogger<HR_UserController>();
            cache = TryResolve<IMemoryCache>();
            _config = ConfigObject.GetConfig(cache);
            _configClientName = _Configuration.GetValue<string>("ClientName").ToUpper();
            otherContext = TryResolve<ezHR_Context>();
            departmentCodeConfig = _Configuration.GetValue<string>("DEPARTMENT_CODE");
            departmentNameConfig = _Configuration.GetValue<string>("DEPARTMENT_NAME");
        }

        [Authorize]
        [ActionName("GetAllEmployeeCompactInfoByPermission")]
        [HttpGet]
        public async Task<ActionResult<EmployeeFullInfo>> GetAllEmployeeCompactInfoByPermission()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var employee = await _HR_UserService.GetAllEmployeeCompactInfoByPermission(user);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllEmployeeCompactInfo: {ex}");
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("GetAllNote")]
        [HttpGet]
        public async Task<ActionResult<HR_User_Note_List>> GetAllNote()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var employee = (await _DbContext.HR_User_Note.ToListAsync()).Select(x => new HR_User_Note_List { Area = x.Area, Note = x.Note}).GroupBy(x => x.Note).Select(x => x.FirstOrDefault()).ToList();
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllEmployeeCompactInfo: {ex}");
                return ApiError(ex.Message);
            }
        }


        [Authorize]
        [ActionName("GetAllEmployeeCompactInfoByPermissionImprovePerformance")]
        [HttpGet]
        public async Task<ActionResult<EmployeeFullInfo>> GetAllEmployeeCompactInfoByPermissionImprovePerformance()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var employee = await _HR_UserService.GetAllEmployeeCompactInfoByPermissionImprovePerformance(user);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllEmployeeCompactInfo: {ex}");
                return ApiError(ex.Message);
            }
        }

        //[Authorize]
        [ActionName("GetEmployeeLookup")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeeLookup()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var allEmployee = await _HR_UserService.GetEmployeeLookup(user.CompanyIndex);
                var rs = allEmployee.Select(x => _Mapper.Map<HR_UserLookupInfo>(x));

                return ApiOk(rs);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeLookup: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetEmployeeAndDepartmentLookup")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeeAndDepartmentLookup()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var allEmployee = await _HR_UserService.GetEmployeeAndDepartmentLookup(user.CompanyIndex);
                var rs = allEmployee.Select(x => _Mapper.Map<HR_UserLookupInfo>(x));

                return ApiOk(rs);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeLookup: {ex}");
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("GetAllStudent")]
        [HttpGet]
        public async Task<ActionResult<List<HR_UserResult>>> GetAllStudent()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var dummy = await _HR_UserService.GetAllStudent(user.CompanyIndex);
                return ApiOk(dummy);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllStudent: {ex}");
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("GetStudentById")]
        [HttpGet("{studentId}")]
        public async Task<ActionResult<HR_UserResult>> GetStudentById(string studentId)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var employee = await _HR_UserService.GetStudentById(user.CompanyIndex, studentId);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetStudentById: {ex}");
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("GetUserByCCCD")]
        [HttpGet("{cccd}")]
        public async Task<ActionResult<HR_UserResult>> GetUserByCCCD(string cccd)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var existedUser = await _HR_UserService.GetuserByCCCD(cccd);
                if (existedUser != null)
                {
                    var result = existedUser;
                    return result;
                }
                else 
                {
                    //var planDock = await _IC_PlanDockService.GetPlanDockByDriverCode(cccd);
                    //var extraDriverByNRIC = await _GC_TruckDriverLogService.GetExtraTruckDriverLogByExtraDriverCode(cccd);
                    //if (planDock != null)
                    //{
                    //var truckDriverLog = await _GC_TruckDriverLogService.GetTruckDriverLogByTripCode(planDock.TripId);
                    //    var result = new HR_UserResult();
                    //    result.EmployeeATID = planDock.TripId;
                    //    result.EmployeeCode = planDock.TripId;
                    //    result.FullName = planDock.DriverName;
                    //    result.Nric = planDock.DriverCode;
                    //    result.CardNumber = truckDriverLog.FirstOrDefault()?.CardNumber ?? string.Empty;
                    //    result.EmployeeType = (short)EmployeeType.Driver;
                    //    result.IsExpired = truckDriverLog.Count == 0 ? null : (truckDriverLog.Any(x => x.InOutMode == (short)InOutMode.Input)
                    //        && truckDriverLog.Any(x => x.InOutMode == (short)InOutMode.Output));
                    //    return result;
                    //}
                    //else if (extraDriverByNRIC != null)
                    //{
                    //    var truckDriverLog = await _GC_TruckDriverLogService.GetTruckDriverLogByTripCode(extraDriverByNRIC.TripCode);
                    //    var result = new HR_UserResult();
                    //    result.EmployeeATID = extraDriverByNRIC.TripCode;
                    //    result.EmployeeCode = extraDriverByNRIC.TripCode;
                    //    result.FullName = extraDriverByNRIC.ExtraDriverName;
                    //    result.Nric = extraDriverByNRIC.ExtraDriverCode;
                    //    result.CardNumber = extraDriverByNRIC.CardNumber;
                    //    result.EmployeeType = (short)EmployeeType.Driver;
                    //    result.IsExpired = truckDriverLog.Count == 0 ? null : (truckDriverLog.Any(x => x.InOutMode == (short)InOutMode.Input)
                    //        && truckDriverLog.Any(x => x.InOutMode == (short)InOutMode.Output));
                    //    return result;
                    //}
                    //return null;

                    return await _GC_TruckDriverLogService.GetDriverByCCCD(cccd);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetStudentById: {ex}");
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("Get_HR_Users")]
        [HttpGet]
        public async Task<ActionResult<List<HR_UserResult>>> Get_HR_Users()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                //var allEmployee = await _HR_UserService.GetAllAsync(x => x.CompanyIndex == user.CompanyIndex);
                var dummy = await _HR_UserService.GetAllHR_UserAsync(user.CompanyIndex);
                if (_configClientName.ToUpper() == ClientName.VSTAR.ToString())
                {
                    dummy.All(x =>
                    {
                        x.Avatar = x.Avatar != null ? x.Avatar : "";
                        x.EmployeeType = (!x.EmployeeType.HasValue || x.EmployeeType == null) ? 0 : x.EmployeeType;
                        x.Gender = null;
                        x.DayOfBirth = null;
                        x.MonthOfBirth = null;
                        x.YearOfBirth = null;
                        x.CreatedDate = null;
                        x.UpdatedUser = null;
                        x.Address = null;
                        x.Description = null;
                        x.Note = null;
                        return true;
                    });
                }
                return ApiOk(dummy);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get_HR_Users: {ex}");
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("Get_HR_User")]
        [HttpGet("{employeeATID}")]
        public async Task<ActionResult<HR_UserResult>> Get_HR_User(string employeeATID)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var employee = await _HR_UserService.GetHR_UserByIDAsync(employeeATID, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get_HR_Users: {ex}");
                return ApiError(ex.Message);
            }
        }

        #region Consider to use those methods with new tree on FE
        [ActionName("GetEmployeeAsTree")]
        [HttpGet]
        public async Task<ActionResult<List<EmployeeTree>>> GetEmployeeAsTree()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var addedParams = new List<AddedParam>();
            var listData = new List<EmployeeTree>();
            var now = DateTime.Now;
            //use db EPAD
            //if (config.IntegrateDBOther == false)
            {
                var company = await context.IC_Company.FirstOrDefaultAsync(t => t.Index == user.CompanyIndex);
                var listDep = context.IC_Department.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();

                addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "IsCurrentTransfer", Value = true });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
                var listEmTransfer = _iC_EmployeeTransferLogic.GetMany(addedParams);

                addedParams = new List<AddedParam>();
                if (listEmTransfer != null && listEmTransfer.Count > 0)
                {
                    addedParams.Add(new AddedParam { Key = "ListEmployeeTransferATID", Value = listEmTransfer.Select(e => e.EmployeeATID).ToList() });
                }
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
                addedParams.Add(new AddedParam { Key = "IsCurrentWorkingAndNoDepartment", Value = true });
                var listEmployee = _IIC_EmployeeLogic.GetEmployeeList(addedParams);

                // GET Employee Transfer


                listEmployee = _IIC_EmployeeLogic.CheckCurrentDepartment(listEmployee);
                // chỉ hiển thị những phòng ban dc phân quyền
                /// TODO need to check permission from request
                //var hsDept = user.ListDepartmentAssignedAndParent.ToHashSet();
                //listDep = listDep.Where(t => hsDept.Contains(t.Index)).ToList();
                int id = 1; int level = 1;

                var mainData = new EmployeeTree();

                mainData.ID = id.ToString(); mainData.Code = company.Index.ToString(); mainData.Name = company.Name;
                mainData.Type = "Company"; mainData.Level = level;
                level++;

                //var listChildrentForCompany = new List<EmployeeTree>();
                //var listDepLV1 = listDep.Where(t => t.ParentIndex == null || t.ParentIndex == 0).ToList();
                ////create phòng ban 'ko có phòng ban'
                //listDepLV1.Add(new IC_Department()
                //{
                //    Index = 0,
                //    Name = "Không có phòng ban",
                //    Code = "",
                //    ParentIndex = 0,
                //    CompanyIndex = user.CompanyIndex
                //});

                //for (int i = 0; i < listDepLV1.Count; i++)
                //{
                //    var currentDep = new EmployeeTree();
                //    //currentDep.ID = id++; //listDepLV1[i].Index;//Convert.ToDecimal(id + "." + (i + 1));
                //    currentDep.ID = id + "." + (i + 1);
                //    currentDep.Code = listDepLV1[i].Code; currentDep.Name = listDepLV1[i].Name;
                //    currentDep.Type = "Department"; currentDep.Level = level;
                //    currentDep.ListChildrent = new List<EmployeeTree>();
                //    if (listDepLV1[i].Index > 0)
                //    {
                //        currentDep.ListChildrent = RecursiveGetChildrentDepartment(listDep, listEmployee, listDepLV1[i].Index, ref id, level + 1);
                //    }

                //    currentDep.ListChildrent.AddRange(GetListEmployeeByDepartmentIndex(listEmployee, listDepLV1[i].Index, ref id, level + 1));

                //    listChildrentForCompany.Add(currentDep);
                //}
                //mainData.ListChildrent = listChildrentForCompany;
                listData.Add(mainData);
            }
            //else //use other db
            //{
            //    HR_Company company = otherContext.HR_Company.Where(t => t.Index == config.CompanyIndex).FirstOrDefault();
            //    if (company == null)
            //    {
            //        return NoContent();
            //    }
            //    List<HR_Department> listDep = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex).ToList();
            //    List<HR_EmployeeReport> listEmployee = GetAllEmployeeReport(otherContext, config);
            //    // chỉ hiển thị những phòng ban dc phân quyền
            //    listDep = listDep.Where(t => user.ListDepartmentAssignedAndParent.Contains(t.Index)).ToList();
            //    int id = 1; int level = 1;

            //    IC_EmployeeTreeDTO mainData = new IC_EmployeeTreeDTO();

            //    mainData.ID = -1; mainData.Code = company.Index.ToString(); mainData.Name = company.Name;
            //    mainData.Type = "Company"; mainData.Level = level;
            //    level++;
            //    List<IC_EmployeeTreeDTO> listChildrentForCompany = new List<IC_EmployeeTreeDTO>();
            //    List<HR_Department> listDepLV1 = listDep.Where(t => t.ParentIndex == null || t.ParentIndex == 0).ToList();
            //    listDepLV1.Add(new HR_Department()
            //    {
            //        Index = id++,
            //        Name = "Không có phòng ban",
            //        NameInEng = "Not in department",
            //        Code = "",
            //        ParentIndex = 0,
            //        CompanyIndex = user.CompanyIndex
            //    });
            //    for (int i = 0; i < listDepLV1.Count; i++)
            //    {
            //        IC_EmployeeTreeDTO currentDep = new IC_EmployeeTreeDTO();
            //        currentDep.ID = id++;// listDepLV1[i].Index;// Convert.ToDecimal(id + "." + (i + 1));
            //        currentDep.Code = listDepLV1[i].Code; currentDep.Name = listDepLV1[i].Name;
            //        currentDep.Type = "Department"; currentDep.Level = level;
            //        if (listDepLV1[i].Index > 0)
            //        {
            //            currentDep.ListChildrent = RecursiveGetChildrentDepartment_DBOther(listDep, listEmployee, int.Parse(listDepLV1[i].Index.ToString()), ref id, level + 1);
            //        }
            //        currentDep.ListChildrent.AddRange(GetListEmployeeByDepartmentIndex_DBOther(listEmployee, int.Parse(listDepLV1[i].Index.ToString()), ref id, level + 1));

            //        listChildrentForCompany.Add(currentDep);
            //    }
            //    mainData.ListChildrent = listChildrentForCompany;
            //    listData.Add(mainData);
            //}

            return Ok(listData);
        }

        private List<EmployeeTree> RecursiveGetChildrentDepartment(List<IC_Department> listDep, List<IC_EmployeeDTO> listEmployee, int pCurrentIndex, ref int pId, int pLevel)
        {
            List<IC_Department> listChildren = listDep.Where(t => t.ParentIndex == pCurrentIndex).ToList();
            var listDepReturn = new List<EmployeeTree>();
            if (listChildren.Count > 0)
            {
                for (int i = 0; i < listChildren.Count; i++)
                {
                    var currentDep = new EmployeeTree();
                    //currentDep.ID = pId++;//listChildren[i].Index;//Convert.ToDecimal(pId + "." + (i + 1)); 
                    currentDep.ID = pId + "." + (i + 1);
                    currentDep.Code = listChildren[i].Code; ; currentDep.Name = listChildren[i].Name;
                    currentDep.Type = "Department"; currentDep.Level = pLevel;
                    currentDep.ListChildrent = RecursiveGetChildrentDepartment(listDep, listEmployee, listChildren[i].Index, ref pId, pLevel + 1);
                    currentDep.ListChildrent.AddRange(GetListEmployeeByDepartmentIndex(listEmployee, listChildren[i].Index, ref pId, pLevel + 1));

                    listDepReturn.Add(currentDep);
                }
            }

            return listDepReturn;
        }

        private List<EmployeeTree> GetListEmployeeByDepartmentIndex(List<IC_EmployeeDTO> listEmployee, int pDepIndex, ref int pId, int pLevel)
        {
            List<IC_EmployeeDTO> listEmp;
            if (pDepIndex > 0)
                listEmp = listEmployee.Where(t => t.DepartmentIndex == pDepIndex).ToList();
            else
                listEmp = listEmployee.Where(t => t.DepartmentIndex == 0 || t.DepartmentIndex == null).ToList();

            var listEmpReturn = new List<EmployeeTree>();
            for (int i = 0; i < listEmp.Count; i++)
            {
                var currentEmp = new EmployeeTree();
                //currentEmp.EmployeeATID = listEmp[i].EmployeeATID;
                //currentEmp.ID = pId++;//pId + "." + (i + 1);
                currentEmp.ID = pId + "." + (i + 1);
                currentEmp.Code = listEmp[i].EmployeeATID; ; currentEmp.Name = listEmp[i].EmployeeATID + "-" + listEmp[i].FullName;
                currentEmp.Type = "Employee"; currentEmp.Level = pLevel;
                //if (listEmp[i].Gender != null)
                //    currentEmp.Gender = listEmp[i].Gender.Value == (short)GenderEnum.Female ? "Female" : listEmp[i].Gender.Value == (short)GenderEnum.Male ? "Male" : "Other";

                listEmpReturn.Add(currentEmp);
            }
            return listEmpReturn;
        }

        private List<IC_EmployeeTreeDTO> RecursiveGetChildrentDepartment_DBOther(List<HR_Department> listDep, List<HR_EmployeeReport> listEmployee, int pCurrentIndex, ref int pId, int pLevel)
        {
            List<HR_Department> listChildren = listDep.Where(t => t.ParentIndex == pCurrentIndex).ToList();
            List<IC_EmployeeTreeDTO> listDepReturn = new List<IC_EmployeeTreeDTO>();
            if (listChildren.Count > 0)
            {
                for (int i = 0; i < listChildren.Count; i++)
                {
                    IC_EmployeeTreeDTO currentDep = new IC_EmployeeTreeDTO();
                    currentDep.ID = pId++;//listChildren[i].Index;//Convert.ToDecimal(pId + "." + (i + 1)); 
                    currentDep.Code = listChildren[i].Code; ; currentDep.Name = listChildren[i].Name;
                    currentDep.Type = "Department"; currentDep.Level = pLevel;
                    currentDep.ListChildrent = RecursiveGetChildrentDepartment_DBOther(listDep, listEmployee, int.Parse(listChildren[i].Index.ToString()), ref pId, pLevel + 1);
                    currentDep.ListChildrent.AddRange(GetListEmployeeByDepartmentIndex_DBOther(listEmployee, int.Parse(listChildren[i].Index.ToString()), ref pId, pLevel + 1));

                    listDepReturn.Add(currentDep);
                }
            }

            return listDepReturn;
        }

        private List<IC_EmployeeTreeDTO> GetListEmployeeByDepartmentIndex_DBOther(List<HR_EmployeeReport> listEmployee, int pDepIndex, ref int pId, int pLevel)
        {
            List<HR_EmployeeReport> listEmp = new List<HR_EmployeeReport>();
            if (pDepIndex == 0)
            {
                listEmp = listEmployee.Where(t => t.DepartmentIndex == null || t.DepartmentIndex.Value == 0).ToList();
            }
            else
            {
                listEmp = listEmployee.Where(t => t.DepartmentIndex == pDepIndex).ToList();
            }

            List<IC_EmployeeTreeDTO> listEmpReturn = new List<IC_EmployeeTreeDTO>();
            for (int i = 0; i < listEmp.Count; i++)
            {
                IC_EmployeeTreeDTO currentEmp = new IC_EmployeeTreeDTO();
                currentEmp.EmployeeATID = listEmp[i].EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
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

        [ActionName("GetEmployeeTree")]
        [HttpGet]
        public async Task<ActionResult<List<EmployeeTree>>> GetEmployeeTree()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var listData = new List<EmployeeTree>();
                int id = 1; int level = 1;
                var mainData = new EmployeeTree();
                mainData.ID = id.ToString();
                mainData.IndexID = id.ToString() + ":/:" + user.CompanyIndex.ToString();
                mainData.Code = user.CompanyIndex.ToString();
                mainData.Name = (await _IC_CompanyService.GetAllAsync()).Where(t => t.Index.Equals(user.CompanyIndex)).FirstOrDefault()?.Name;
                mainData.Type = "Company";
                mainData.Level = level;
                level++;
                var listDep = await _IC_DepartmentService.GetByCompanyIndex(user.CompanyIndex);
                var listEmployee = await _HR_UserService.GetEmployeeLookup(user.CompanyIndex);
                var resultShowStoppedEmp = _HR_UserService.ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1 && resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                {
                    listEmployee = listEmployee.Where(x => (x.EmployeeType.HasValue && x.EmployeeType.Value != (short)EmployeeType.Employee)
                        || ((!x.EmployeeType.HasValue || (x.EmployeeType.HasValue && x.EmployeeType.Value == (short)EmployeeType.Employee)) 
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID))).ToList();
                }
                var listWorkingInfo = await _IC_WorkingInfoService.GetDataByCompanyIndex(user.CompanyIndex, listEmployee.Select(x => x.EmployeeATID).ToList());

                var listChildrentForCompany = new List<EmployeeTree>();
                var listDepLV1 = listDep.Where(t => t.ParentIndex == null || t.ParentIndex == 0).ToList();
                for (int i = 0; i < listDepLV1.Count; i++)
                {
                    var currentDep = new EmployeeTree();
                    currentDep.ID = id + "." + (i + 1);
                    currentDep.IndexID = currentDep.ID + ":/:" + listDepLV1[i].Index.ToString();
                    currentDep.Code = listDepLV1[i].Code; currentDep.Name = listDepLV1[i].Name;
                    currentDep.Level = level;
                    currentDep.Type = "Department";
                    currentDep.ListChildrent = RecursiveGetChildrentDepartment(listDep, listEmployee, listWorkingInfo, listDepLV1[i].Index, currentDep.ID, level + 1, user.CompanyIndex);
                    var listChildrenEmployee = GetListEmployeeByDepartmentIndex(listEmployee, listWorkingInfo, listDepLV1[i].Index, currentDep.ID, level + 1, user.CompanyIndex);
                    if (listChildrenEmployee != null && listChildrenEmployee.Count > 0)
                    {
                        listChildrenEmployee.ForEach(x =>
                        {
                            x.IndexID = listDepLV1[i].Index.ToString() + ":/:" + x.ID;
                        });
                    }
                    currentDep.ListChildrent.AddRange(listChildrenEmployee);

                    listChildrentForCompany.Add(currentDep);
                }
                mainData.ListChildrent = listChildrentForCompany.Distinct().ToList();
                listData.Add(mainData);

                return ApiOk(listData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeTree: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetEmployeeTreeByIdAndRoleNew")]
        [HttpGet]
        public async Task<ActionResult<List<EmployeeTreeNew>>> GetEmployeeTreeByIdAndRoleNew(string index)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var listData = new List<EmployeeTreeNew>();
                int id = 1; int level = 1;
                var mainData = new EmployeeTreeNew();
                mainData.id = id.ToString(); mainData.Code = user.CompanyIndex.ToString();
                mainData.label = (await _IC_CompanyService.GetAllAsync()).Where(t => t.Index.Equals(user.CompanyIndex)).FirstOrDefault()?.Name;
                mainData.Type = "Company";
                mainData.Level = level;
                level++;
                var result = index.Split(',').ToList().Select(long.Parse).ToList();
                var listDep = await _IC_DepartmentService.GetByCompanyIndexAndDepIds(user.CompanyIndex, result);
                var listEmployee = await _HR_UserService.GetEmployeeLookup(user.CompanyIndex);
                var listWorkingInfo = await _IC_WorkingInfoService.GetDataByCompanyIndex(user.CompanyIndex, listEmployee.Select(x => x.EmployeeATID).ToList());

                var listChildrentForCompany = new List<EmployeeTreeNew>();
                for (int i = 0; i < listDep.Count; i++)
                {
                    var currentDep = new EmployeeTreeNew();
                    currentDep.id = id + "." + (i + 1);
                    currentDep.Code = listDep[i].Code; currentDep.label = listDep[i].Name;
                    currentDep.Level = level;
                    currentDep.Type = "Department";
                    currentDep.children = RecursiveGetChildrentDepartmentNew(listDep, listEmployee, listWorkingInfo, listDep[i].Index, currentDep.id, level + 1, user.CompanyIndex);
                    currentDep.children.AddRange(GetListEmployeeByDepartmentIndexNew(listEmployee, listWorkingInfo, listDep[i].Index, currentDep.id, level + 1, user.CompanyIndex));

                    listChildrentForCompany.Add(currentDep);
                }
                mainData.children = listChildrentForCompany.Distinct().ToList();
                listData.Add(mainData);

                return ApiOk(listData);

            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeTreeByIdAndRoleNew: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetEmployeeTreeByIdAndRole")]
        [HttpGet]
        public async Task<ActionResult<List<EmployeeTree>>> GetEmployeeTreeByIdAndRole(string index)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var listData = new List<EmployeeTree>();
                int id = 1; int level = 1;
                var mainData = new EmployeeTree();
                mainData.ID = id.ToString(); mainData.Code = user.CompanyIndex.ToString();
                mainData.Name = (await _IC_CompanyService.GetAllAsync()).Where(t => t.Index.Equals(user.CompanyIndex)).FirstOrDefault()?.Name;
                mainData.Type = "Company";
                mainData.Level = level;
                level++;
                var result = index?.Split(',').ToList().Select(long.Parse).ToList() ?? new List<long>();
                var listDep = await _IC_DepartmentService.GetByCompanyIndexAndDepIds(user.CompanyIndex, result);
                var listEmployee = await _HR_UserService.GetEmployeeLookup(user.CompanyIndex);
                var employeeIDs = listEmployee.Select(x => x.EmployeeATID).ToList();
                var listWorkingInfo = await _IC_WorkingInfoService.GetDataByCompanyIndex(user.CompanyIndex, employeeIDs, result);

                var listChildrentForCompany = new List<EmployeeTree>();
                //var listDepLV1 = listDep.Where(t => t.ParentIndex == null || t.ParentIndex == 0).ToList();
                for (int i = 0; i < listDep.Count; i++)
                {
                    var currentDep = new EmployeeTree();
                    currentDep.ID = id + "." + (i + 1);
                    currentDep.Code = listDep[i].Code; currentDep.Name = listDep[i].Name;
                    currentDep.Level = level;
                    currentDep.Type = "Department";
                    currentDep.ListChildrent = RecursiveGetChildrentDepartment(listDep, listEmployee, listWorkingInfo, listDep[i].Index, currentDep.ID, level + 1, user.CompanyIndex);
                    currentDep.ListChildrent.AddRange(GetListEmployeeByDepartmentIndex(listEmployee, listWorkingInfo, listDep[i].Index, currentDep.ID, level + 1, user.CompanyIndex));

                    listChildrentForCompany.Add(currentDep);
                }
                mainData.ListChildrent = listChildrentForCompany.Distinct().ToList();
                listData.Add(mainData);

                return ApiOk(listData);

            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeTree: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetDepartmentTree")]
        [HttpGet]
        public async Task<ActionResult<List<DepartmentTree>>> GetDepartmentTree()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var listData = new List<DepartmentTree>();
                int id = 1; int level = 1;
                var mainData = new DepartmentTree();

                mainData.Index = id.ToString();
                mainData.Code = user.CompanyIndex.ToString();
                mainData.Name = (await _IC_CompanyService.GetAllAsync()).Where(t => t.Index.Equals(user.CompanyIndex)).FirstOrDefault().Name;
                mainData.Type = "Company";
                mainData.Level = level;
                level++;
                var listDep = await _IC_DepartmentService.GetByCompanyIndex(user.CompanyIndex);

                var listChildrentForCompany = new List<DepartmentTree>();
                var listDepLV1 = listDep.Where(t => t.ParentIndex == null || t.ParentIndex == 0).ToList();
                for (int i = 0; i < listDepLV1.Count; i++)
                {
                    var currentDep = new DepartmentTree
                    {
                        //ID = id + "." + (i + 1),
                        Index = listDepLV1[i].Index.ToString(),
                        Code = listDepLV1[i].Code,
                        Name = listDepLV1[i].Name,
                        Level = level,
                        Type = "Department"
                    };

                    listChildrentForCompany.Add(currentDep);
                }
                mainData.ListChildrent = listChildrentForCompany;
                listData.Add(mainData);

                return ApiOk(listData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetDepartmentTree: {ex}");
                return ApiError(ex.Message);
            }
        }

        private List<EmployeeTreeNew> RecursiveGetChildrentDepartmentNew(List<IC_Department> listDep, List<HR_User> listEmployee, List<IC_WorkingInfo> listWorkingInfo, long pCurrentIndex, string pId, int pLevel, int companyIndex)
        {
            try
            {
                var listChildren = listDep.Where(t => t.ParentIndex == pCurrentIndex).ToList();
                var listDepReturn = new List<EmployeeTreeNew>();
                if (listChildren.Count > 0)
                {
                    for (int i = 0; i < listChildren.Count; i++)
                    {
                        var currentDep = new EmployeeTreeNew();
                        currentDep.id = (i + 1).ToString();
                        currentDep.Code = listChildren[i].Code;
                        currentDep.label = listChildren[i].Name;
                        currentDep.Level = pLevel;
                        currentDep.Type = "Department";
                        currentDep.children = RecursiveGetChildrentDepartmentNew(listDep, listEmployee, listWorkingInfo, int.Parse(listChildren[i].Index.ToString()), currentDep.id, pLevel + 1, companyIndex);
                        currentDep.children.AddRange(GetListEmployeeByDepartmentIndexNew(listEmployee, listWorkingInfo, int.Parse(listChildren[i].Index.ToString()), currentDep.id, pLevel + 1, companyIndex));

                        listDepReturn.Add(currentDep);
                    }
                }

                return listDepReturn.Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"RecursiveGetChildrentDepartment: {ex}");
                return null;
            }
        }

        private List<EmployeeTreeNew> GetListEmployeeByDepartmentIndexNew(List<HR_User> listEmployee, List<IC_WorkingInfo> listWorkingInfo, long pDepIndex, string pId, int pLevel, int companyIndex)
        {
            try
            {
                var now = DateTime.Now;
                var listEmp = new List<HR_User>();
                var workingInfos = listWorkingInfo.Where(t => t.DepartmentIndex == pDepIndex).ToList();
                foreach (var item in workingInfos)
                {
                    var employee = listEmployee.FirstOrDefault(t => t.EmployeeATID.Equals(item.EmployeeATID));
                    if (employee != null)
                    {
                        listEmp.Add(employee);
                    }
                }
                listEmp = listEmp.Distinct().ToList();
                //List<HR_User> listEmp = listEmployee.Where(t => t. == pDepIndex).ToList();
                var listEmpReturn = new List<EmployeeTreeNew>();
                for (int i = 0; i < listEmp.Count; i++)
                {
                    var currentEmp = new EmployeeTreeNew();
                    currentEmp.id = listEmp[i].EmployeeATID;
                    // currentEmp.ID = pId + "." + (i + 1); 
                    currentEmp.Code = listEmp[i].EmployeeATID;
                    currentEmp.label = listEmp[i].FullName;
                    currentEmp.Level = pLevel;
                    currentEmp.Type = "Employee";
                    currentEmp.children = new List<EmployeeTreeNew>();
                    listEmpReturn.Add(currentEmp);
                }
                return listEmpReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetListEmployeeByDepartmentIndexNew: {ex}");
                return null;
            }
        }

        private List<EmployeeTree> RecursiveGetChildrentDepartment(List<IC_Department> listDep, List<HR_User> listEmployee, List<IC_WorkingInfo> listWorkingInfo, long pCurrentIndex, string pId, int pLevel, int companyIndex)
        {
            try
            {
                var listChildren = listDep.Where(t => t.ParentIndex == pCurrentIndex).ToList();
                var listDepReturn = new List<EmployeeTree>();
                if (listChildren.Count > 0)
                {
                    for (int i = 0; i < listChildren.Count; i++)
                    {
                        var currentDep = new EmployeeTree();
                        currentDep.ID = (i + 1).ToString();
                        currentDep.IndexID = currentDep.ID + ":/:" + listChildren[i].Index.ToString();
                        currentDep.Code = listChildren[i].Code;
                        currentDep.Name = listChildren[i].Name;
                        currentDep.Level = pLevel;
                        currentDep.Type = "Department";
                        currentDep.ListChildrent = RecursiveGetChildrentDepartment(listDep, listEmployee, listWorkingInfo, int.Parse(listChildren[i].Index.ToString()), currentDep.ID, pLevel + 1, companyIndex);
                        var listChildrenEmployee = GetListEmployeeByDepartmentIndex(listEmployee, listWorkingInfo, listChildren[i].Index, currentDep.ID, pLevel + 1, companyIndex);
                        if (listChildrenEmployee != null && listChildrenEmployee.Count > 0)
                        {
                            listChildrenEmployee.ForEach(x =>
                            {
                                x.IndexID = listChildren[i].Index.ToString() + ":/:" + x.ID;
                            });
                        }
                        currentDep.ListChildrent.AddRange(listChildrenEmployee);

                        listDepReturn.Add(currentDep);
                    }
                }

                return listDepReturn.Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"RecursiveGetChildrentDepartment: {ex}");
                return null;
            }
        }

        private List<EmployeeTree> GetListEmployeeByDepartmentIndex(List<HR_User> listEmployee, List<IC_WorkingInfo> listWorkingInfo, long pDepIndex, string pId, int pLevel, int companyIndex)
        {
            try
            {
                var now = DateTime.Now;
                var listEmp = new List<HR_User>();
                var workingInfos = listWorkingInfo.Where(t => t.DepartmentIndex == pDepIndex).ToList();
                foreach (var item in workingInfos)
                {
                    var employee = listEmployee.FirstOrDefault(t => t.EmployeeATID.Equals(item.EmployeeATID));
                    if (employee != null)
                    {
                        listEmp.Add(employee);
                    }
                }
                listEmp = listEmp.Distinct().ToList();
                //List<HR_User> listEmp = listEmployee.Where(t => t. == pDepIndex).ToList();
                var listEmpReturn = new List<EmployeeTree>();
                for (int i = 0; i < listEmp.Count; i++)
                {
                    var currentEmp = new EmployeeTree();
                    currentEmp.ID = listEmp[i].EmployeeATID;
                    // currentEmp.ID = pId + "." + (i + 1); 
                    currentEmp.Code = listEmp[i].EmployeeATID;
                    currentEmp.Name = listEmp[i].FullName;
                    currentEmp.Level = pLevel;
                    currentEmp.Type = "Employee";
                    currentEmp.ListChildrent = new List<EmployeeTree>();
                    listEmpReturn.Add(currentEmp);
                }
                return listEmpReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetListEmployeeByDepartmentIndex: {ex}");
                return null;
            }
        }

        [ActionName("GetEmployeeIsVisitorAsTree")]
        [HttpGet]
        public async Task<ActionResult<List<EmployeeTree>>> GetEmployeeIsVisitorAsTree()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var listData = new List<EmployeeTree>();
                int id = 1; int level = 1;

                var listDep = await _IC_DepartmentService.GetByCompanyIndex(user.CompanyIndex);
                var listEmployee = await _HR_UserService.GetEmployeeLookup(user.CompanyIndex);
                var listWorkingInfo = await _IC_WorkingInfoService.GetDataByCompanyIndex(user.CompanyIndex);

                var listChildrentForCompany = new List<EmployeeTree>();
                var listDepLV1 = listDep.Where(t => t.Code == "Visitor").ToList();
                for (int i = 0; i < listDepLV1.Count; i++)
                {
                    var currentDep = new EmployeeTree();
                    currentDep.ID = id + "." + (i + 1);
                    currentDep.Code = listDepLV1[i].Code; currentDep.Name = listDepLV1[i].Name;
                    currentDep.Level = level;
                    currentDep.Type = "Department";
                    currentDep.ListChildrent = RecursiveGetChildrentDepartment(listDep, listEmployee, listWorkingInfo, listDepLV1[i].Index, currentDep.ID, level + 1, user.CompanyIndex);
                    currentDep.ListChildrent.AddRange(GetListEmployeeByDepartmentIndex(listEmployee, listWorkingInfo, listDepLV1[i].Index, currentDep.ID, level + 1, user.CompanyIndex));

                    listChildrentForCompany.Add(currentDep);
                }

                listData.AddRange(listChildrentForCompany);

                return ApiOk(listData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeIsVisitorAsTree: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetEmployeeCompactInfoByEmployeeATID")]
        [HttpPost]
        public async Task<ActionResult<EmployeeFullInfo>> GetEmployeeCompactInfoByEmployeeATID([FromBody] List<string> pEmployeeATIDs)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                if (pEmployeeATIDs == null || pEmployeeATIDs.Count() == 0)
                    return ApiError("Employee ID was empty!");

                var employee = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(pEmployeeATIDs, new DateTime(), user.CompanyIndex);

                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeCompactInfoByEmployeeATID: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetRootDepartmentEmployeeCompactInfoByEmployeeATID")]
        [HttpPost]
        public async Task<ActionResult<EmployeeFullInfo>> GetRootDepartmentEmployeeCompactInfoByEmployeeATID([FromBody] List<string> pEmployeeATIDs)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                if (pEmployeeATIDs == null || pEmployeeATIDs.Count() == 0)
                    return ApiError("Employee ID was empty!");

                var employee = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(pEmployeeATIDs, new DateTime(), user.CompanyIndex);
                var listActiveDepartment = await _IC_DepartmentService.GetActiveDepartment();
                var listEmployeeRootDepartment = listActiveDepartment.Where(x => !x.ParentIndex.HasValue || x.ParentIndex == 0).ToList();
                foreach (var emp in employee)
                {
                    if (emp.DepartmentIndex.HasValue && emp.DepartmentIndex != 0 && !listEmployeeRootDepartment.Any(y => y.Index == emp.DepartmentIndex))
                    {
                        var rootDepartmentIndex = FindRootDepartmentIndex(emp.DepartmentIndex.Value, listActiveDepartment);
                        if (rootDepartmentIndex != emp.DepartmentIndex)
                        {
                            emp.DepartmentIndex = rootDepartmentIndex;
                            emp.Department = listActiveDepartment.FirstOrDefault(x => x.Index == rootDepartmentIndex)?.Name
                                ?? string.Empty;
                        }
                    }
                }

                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeCompactInfoByEmployeeATID: {ex}");
                return ApiError(ex.Message);
            }
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

        [ActionName("GetEmployeeCompactInfoByEmployeeCodes")]
        [HttpPost]
        public async Task<ActionResult<EmployeeFullInfo>> GetEmployeeCompactInfoByEmployeeCodes([FromBody] List<string> pEmployeeATIDs)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                if (pEmployeeATIDs == null || pEmployeeATIDs.Count() == 0)
                    return ApiError("Employee ATIDs was empty!");

                var employee = await _HR_UserService.GetEmployeeCompactInfoByEmployeeCodes(pEmployeeATIDs, user.CompanyIndex);

                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeCompactInfoByEmployeeIDs: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetAllEmployeeCompactInfo")]
        [HttpGet]
        public async Task<ActionResult<EmployeeFullInfo>> GetAllEmployeeCompactInfo()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var employee = await _HR_UserService.GetAllEmployeeCompactInfo(user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllEmployeeCompactInfo: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetAllEmployeeTypeUserCompactInfo")]
        [HttpGet]
        public async Task<ActionResult<EmployeeFullInfo>> GetAllEmployeeTypeUserCompactInfo()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var employee = await _HR_UserService.GetAllEmployeeTypeUserCompactInfo(user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllEmployeeCompactInfo: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetRootDepartmentAllEmployeeCompactInfo")]
        [HttpGet]
        public async Task<ActionResult<EmployeeFullInfo>> GetRootDepartmentAllEmployeeCompactInfo()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var employee = await _HR_UserService.GetAllEmployeeCompactInfo(user.CompanyIndex);
                var listActiveDepartment = await _IC_DepartmentService.GetActiveDepartment();
                var listEmployeeRootDepartment = listActiveDepartment.Where(x => !x.ParentIndex.HasValue || x.ParentIndex == 0).ToList();
                foreach (var emp in employee)
                {
                    if (emp.DepartmentIndex.HasValue && emp.DepartmentIndex != 0 && !listEmployeeRootDepartment.Any(y => y.Index == emp.DepartmentIndex))
                    {
                        var rootDepartmentIndex = FindRootDepartmentIndex(emp.DepartmentIndex.Value, listActiveDepartment);
                        if (rootDepartmentIndex != emp.DepartmentIndex)
                        {
                            emp.DepartmentIndex = rootDepartmentIndex;
                            emp.Department = listActiveDepartment.FirstOrDefault(x => x.Index == rootDepartmentIndex)?.Name
                                ?? string.Empty;
                        }
                    }
                }

                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllEmployeeCompactInfo: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetAllEmployeeCompactInfoByDate")]
        [HttpPost]
        public async Task<ActionResult<EmployeeFullInfo>> GetAllEmployeeCompactInfoByDate([FromBody] DateTime pDate)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var employee = await _HR_UserService.GetAllEmployeeCompactInfoByDate(pDate, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllEmployeeCompactInfoByDate: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetAllNanny")]
        [HttpPost]
        public async Task<ActionResult<EmployeeFullInfo>> GetAllNanny([FromBody] DateTime pDate)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var employee = await _HR_UserService.GetAllNanny(pDate, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllNanny: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetAllStudents")]
        [HttpPost]
        public async Task<ActionResult<EmployeeFullInfo>> GetAllStudents([FromBody] DateTime pDate)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var employee = await _HR_UserService.GetAllStudent(pDate, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllStudent: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetAllUserCompactInfo")]
        [HttpGet]
        public async Task<ActionResult<EmployeeFullInfo>> GetAllUserCompactInfo()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var employee = await _HR_UserService.GetAllUserCompactInfo(user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllEmployeeCompactInfo: {ex}");
                return ApiError(ex.Message);
            }
        }

        /// <summary>
        ///     Lấy thông tin của employees
        /// </summary>
        /// <param name="type"> 0 || 1: Nhân viên, 2: Khách, 3: Học sinh, 4: Phụ huynh, 5: bảo mẫu, 6: Nhà thầu, 7: Giáo viên</param>
        /// <param name="fields">Mảng các property của dữ liệu sẽ trả về cần lấy, example: Avatar,FullName,Gender</param>
        /// <returns></returns>
        [ActionName("GetEmployeeList")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<EmployeeFullInfo>))]
        [HttpGet]
        public IActionResult GetEmployeeCompactInfo([FromQuery] EmployeeType[] type,
            [FromQuery] string fields)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                List<EmployeeFullInfo> employee = _HR_UserService
                    .GetEmployeeCompactInfo(user.CompanyIndex, type, fields);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeCompactInfo: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetEmployeeByDepartmentIds")]
        [HttpGet]
        public async Task<ActionResult<List<EmployeeFullInfo>>> GetEmployeeByDepartmentIds(string pDepartmentIds)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var Ids = pDepartmentIds.Split(',').ToList();
                var employee = await _HR_UserService.GetEmployeeByDepartmentIds(Ids, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeByDepartmentIds: {ex}");
                return ApiError(ex.Message);
            }
        }


        [ActionName("GetEmployeeByDepartmentLstIds")]
        [HttpPost]
        public async Task<ActionResult<List<EmployeeFullInfo>>> GetEmployeeByDepartmentLstIds([FromBody] List<string> pDepartmentIds)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

              
                var employee = await _HR_UserService.GetEmployeeByDepartmentIds(pDepartmentIds, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeByDepartmentIds: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetEmployeeCompactInfoByListEmail")]
        [HttpPost]
        public async Task<ActionResult<List<EmployeeFullInfo>>> GetEmployeeCompactInfoByListEmail([FromBody] string listEmail)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var emailList = listEmail.Split(',').ToList();
                var employee = await _HR_UserService.GetEmployeeCompactInfoByListEmail(emailList, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeByDepartmentIds: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetEmployeeCompactInfoByListEmpATID")]
        [HttpPost]
        public async Task<ActionResult<List<EmployeeFullInfo>>> GetEmployeeCompactInfoByListEmpATID([FromBody] string listEmpATID)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var listID = listEmpATID.Split(',').ToList();
                var employee = await _HR_UserService.GetEmployeeCompactInfoByListEmpATID(listID, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeByDepartmentIds: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetEmployeeInfoAndDepartmentParentByListEmpATID")]
        [HttpPost]
        public async Task<ActionResult<List<EmployeeFullInfo>>> GetEmployeeInfoAndDepartmentParentByListEmpATID([FromBody] string listEmpATID)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var listID = listEmpATID.Split(',').ToList();
                var employee = await _HR_UserService.GetEmployeeInfoAndDepartmentParentByListEmpATID(listID, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeByDepartmentIds: {ex}");
                return ApiError(ex.Message);
            }
        }


        [ActionName("GetEmployeeInfoByCardNumber")]
        [HttpPost]
        public async Task<ActionResult<List<EmployeeFullInfo>>> GetEmployeeInfoByCardNumber([FromBody] string cardNumber)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var employee = await _HR_UserService.GetEmployeeCompactInfoByCardNumber(cardNumber, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeByDepartmentIds: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetEmployeeByUserTypeIds")]
        [HttpGet]
        public async Task<ActionResult<List<EmployeeFullInfo>>> GetEmployeeByUserTypeIds(string pUserTypes)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var Ids = pUserTypes.Split(',').Select(int.Parse).ToList();
                var userTypes = new List<int?>();
                foreach (var item in Ids)
                {
                    userTypes.Add(item);
                }
                var employee = await _HR_UserService.GetEmployeeByUserTypeIds(userTypes, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeByUserTypeIds: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetDepartmentList")]
        [HttpGet]
        public async Task<ActionResult<List<HR_Department>>> GetDepartmentList()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var result = await _HR_UserService.GetDepartmentList(user.CompanyIndex);
                return ApiOk(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetDepartmentList: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetDepartmentByIds")]
        [HttpGet]
        public async Task<ActionResult<List<HR_Department>>> GetDepartmentByIds(string pDepartmentIds)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var Ids = pDepartmentIds.Split(',').ToList();
                var result = await _HR_UserService.GetDepartmentByIds(Ids, user.CompanyIndex);
                return ApiOk(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetDepartmentByIds: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("GetDepartmentListByIDs")]
        [HttpPost]
        public async Task<ActionResult<List<HR_Department>>> GetDepartmentListByIDs([FromBody] List<string> pDepartmentIds)
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                if (pDepartmentIds == null || pDepartmentIds.Count() == 0)
                    return ApiError("Employee ID was empty!");

                var result = await _HR_UserService.GetDepartmentByIds(pDepartmentIds, user.CompanyIndex);
                return ApiOk(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetDepartmentByIds: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("Get_HR_User_BasicInfo")]
        [HttpGet]
        public async Task<IActionResult> GetUser_BasicInfo()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();

                var allEmployee = await _HR_UserService.GetUserBasicInfo();
                return ApiOk(allEmployee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get_HR_User_BasicInfo: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("Get_HR_User_DetailInfo")]
        [HttpGet]
        public async Task<IActionResult> GetHR_User_DetailInfo()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();
                var allEmployee = await _HR_UserService.GetUserDetailInfo();
                return ApiOk(allEmployee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get_HR_User_DetailInfo: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("Get_HR_User_ContactInfo")]
        [HttpGet]
        public async Task<IActionResult> Get_HR_User_ContactInfo()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();
                var allEmployee = await _HR_UserService.GetUserContactInfo();
                return ApiOk(allEmployee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get_HR_User_ContactInfo: {ex}");
                return ApiError(ex.Message);
            }
        }

        [ActionName("Get_HR_User_PersonalInfo")]
        [HttpGet]
        public async Task<IActionResult> Get_HR_User_PersonalInfo()
        {
            try
            {
                var user = GetUserInfo();
                if (user == null) return ApiUnauthorized();
                var allEmployee = await _HR_UserService.GetUserPersonalInfo();
                return ApiOk(allEmployee);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get_HR_User_PersonalInfo: {ex}");
                return ApiError(ex.Message);
            }
        }
    }
}
