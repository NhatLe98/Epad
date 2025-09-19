using EPAD_Backend_Core.MainProcess;
using EPAD_Backend_Core.WebUtilitys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.IO;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using EPAD_Data.Models;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Logic;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Backend_Core.Models.DTOs;
using EPAD_Backend_Core.Base;
using EPAD_Common.Extensions;
using EPAD_Data.Models.IC;
using EPAD_Services.Interface;
using EPAD_Data.Entities.HR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using EPAD_Services.Impl;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/EmployeeInfo/[action]")]
    [ApiController]
    public class IC_EmployeeInfoController : ApiControllerBase
    {
        private EPAD_Context context;
        private ezHR_Context otherContext;
        private IMemoryCache cache;
        private ConfigObject _config;
        private readonly IHostingEnvironment _hostingEnvironment;
        private IIC_EmployeeLogic _IIC_EmployeeLogic;
        private IIC_EmployeeTransferLogic _iC_EmployeeTransferLogic;
        private IHR_EmployeeLogic _IHR_EmployeeLogic;
        private IIC_CommandLogic _IIC_CommandLogic;
        private IIC_WorkingInfoLogic _IIC_WorkingInfoLogic;
        private IIC_UserMasterLogic _iC_UserMasterLogic;
        private IIC_DepartmentLogic _iC_DepartmentLogic;
        private IIC_AuditLogic _iIC_AuditLogic;
        private IIC_ConfigLogic _iC_ConfigLogic;
        private IHR_PositionInfoService _IHR_PositionInfoService;
        private IIC_DepartmentService _IIC_DepartmentService;
        private IIC_EmployeeTypeService _IIC_EmployeeTypeService;
        private readonly string _configClientName;
        private readonly string departmentCodeConfig;
        private readonly string departmentNameConfig;
        private readonly ILogger _logger;
        private string mLinkECMSApi;
        private string mCommunicateToken;
        private IIC_ScheduleAutoHostedLogic _IIC_ScheduleAutoHostedLogic;
        private readonly IIC_VehicleLogService _IC_VehicleLogService;
        private readonly IIC_CommandService _iC_CommandService;
        private readonly IHR_UserService _HR_UserService;
        private readonly IIC_UserAuditService _IIC_UserAuditService;

        public IC_EmployeeInfoController(IServiceProvider provider, ILoggerFactory loggerFactory) : base(provider)
        {
            _Logger = _LoggerFactory.CreateLogger<IC_EmployeeInfoController>();
            context = TryResolve<EPAD_Context>();
            otherContext = TryResolve<ezHR_Context>();
            cache = TryResolve<IMemoryCache>();
            _config = ConfigObject.GetConfig(cache);
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _IIC_EmployeeLogic = TryResolve<IIC_EmployeeLogic>();
            _iC_EmployeeTransferLogic = TryResolve<IIC_EmployeeTransferLogic>();
            _IHR_EmployeeLogic = TryResolve<IHR_EmployeeLogic>();
            _IIC_CommandLogic = TryResolve<IIC_CommandLogic>();
            _IIC_WorkingInfoLogic = TryResolve<IIC_WorkingInfoLogic>();
            _iC_UserMasterLogic = TryResolve<IIC_UserMasterLogic>();
            _iC_DepartmentLogic = TryResolve<IIC_DepartmentLogic>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _iC_ConfigLogic = TryResolve<IIC_ConfigLogic>();
            _IHR_PositionInfoService = TryResolve<IHR_PositionInfoService>();
            _IIC_DepartmentService = TryResolve<IIC_DepartmentService>();
            _IIC_EmployeeTypeService = TryResolve<IIC_EmployeeTypeService>();
            _configClientName = _Configuration.GetValue<string>("ClientName").ToUpper();
            departmentCodeConfig = _Configuration.GetValue<string>("DEPARTMENT_CODE");
            departmentNameConfig = _Configuration.GetValue<string>("DEPARTMENT_NAME");
            _logger = loggerFactory.CreateLogger<IC_EmployeeInfoController>();
            mLinkECMSApi = _Configuration.GetValue<string>("ECMSApi");
            mCommunicateToken = _Configuration.GetValue<string>("CommunicateToken");
            _IIC_ScheduleAutoHostedLogic = TryResolve<IIC_ScheduleAutoHostedLogic>();
            _IC_VehicleLogService = TryResolve<IIC_VehicleLogService>();
            _iC_CommandService = TryResolve<IIC_CommandService>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _IIC_UserAuditService = TryResolve<IIC_UserAuditService>();
        }

        [Authorize]
        [ActionName("GetEmployeeAtPage")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeeAtPage([FromQuery] int page, [FromQuery] string filter, [FromQuery] long[] departmentIndex, [FromQuery] int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            ConfigObject config = ConfigObject.GetConfig(cache);
            DataGridClass dataGrid = null;
            DateTime now = DateTime.Now;
            List<AddedParam> addedParams = new List<AddedParam>();

            if (config.IntegrateDBOther == false)
            {
                addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "IsCurrentTransfer", Value = true });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
                List<IC_EmployeeTransferDTO> listEmTransfer = _iC_EmployeeTransferLogic.GetMany(addedParams);

                addedParams = new List<AddedParam>();
                if (listEmTransfer != null && listEmTransfer.Count > 0)
                {
                    addedParams.Add(new AddedParam { Key = "ListEmployeeTransferATID", Value = listEmTransfer.Select(e => e.EmployeeATID).ToList() });
                }
                if (departmentIndex != null && departmentIndex.Count() > 0)
                {
                    addedParams.Add(new AddedParam { Key = "ListDepartment", Value = departmentIndex });
                }
                else
                {
                    addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
                }
                addedParams.Add(new AddedParam { Key = "PageIndex", Value = page });
                addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
                addedParams.Add(new AddedParam { Key = "PageSize", Value = limit });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "TransferStatus", Value = TransferStatus.Approve });
                addedParams.Add(new AddedParam { Key = "IsCurrentWorkingAndNoDepartment", Value = true });
                ListDTOModel<IC_EmployeeDTO> listEmployee = _IIC_EmployeeLogic.GetPage(addedParams);

                listEmployee.Data = _IIC_EmployeeLogic.CheckCurrentDepartment(listEmployee.Data);

                var obj = from e in listEmployee.Data
                          select new
                          {
                              EmployeeATID = e.EmployeeATID,
                              EmployeeCode = e.EmployeeCode,
                              FullName = e.FullName,
                              Gender = e.Gender,
                              CardNumber = e.CardNumber,
                              NameOnMachine = e.NameOnMachine,
                              DepartmentIndex = e.DepartmentIndex,
                              _Gender = e.Gender != null ? e.Gender == (short)GenderEnum.Male ? "Nam" : "Nữ" : "Nam",
                              _DepartmentName = e.DepartmentName,
                              JoinedDate = e.JoinedDate,
                              ImageUpload = e.ImageUpload,
                              UpdatedDate = e.UpdatedDate
                          };

                dataGrid = new DataGridClass(listEmployee.TotalCount, obj);
            }
            else
            {
                //int countData = 0;
                //List<DALOther.HR_EmployeeReport> listEmployee = GetEmployeeFromOtherDB(config, filter, page, ref countData, user.PrivilegeIndex);
                addedParams = new List<AddedParam>();
                if (departmentIndex != null && departmentIndex.Count() > 0)
                {
                    addedParams.Add(new AddedParam { Key = "ListDepartment", Value = departmentIndex });
                }
                else
                {
                    addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
                }
                addedParams.Add(new AddedParam { Key = "PageIndex", Value = page });
                addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = config.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
                addedParams.Add(new AddedParam { Key = "PageSize", Value = limit });
                ListDTOModel<IC_EmployeeDTO> listHREmployee = _IHR_EmployeeLogic.GetPage(addedParams);
                var obj = from e in listHREmployee.Data
                          select new
                          {
                              EmployeeATID = e.EmployeeATID,
                              EmployeeCode = e.EmployeeCode,
                              FullName = e.FullName,
                              Gender = e.Gender,
                              CardNumber = e.CardNumber,
                              NameOnMachine = e.NameOnMachine,
                              DepartmentIndex = e.DepartmentIndex,
                              _Gender = e.Gender.HasValue ? e.Gender == (short)GenderEnum.Male ? "Nam" : "Nữ" : "Nam",
                              _DepartmentName = e.DepartmentName,
                              JoinedDate = e.JoinedDate,
                              UpdatedDate = e.UpdatedDate
                          };

                dataGrid = new DataGridClass(listHREmployee.TotalCount, obj);
            }

            result = Ok(dataGrid);
            return result;
        }

        private List<HR_EmployeeReport> GetEmployeeFromOtherDB(ConfigObject config, string filter, int page, ref int countData, int pAccountPrivilege)
        {
            var lstDeptIndex = context.IC_PrivilegeDepartment.Where(x => x.PrivilegeIndex == pAccountPrivilege && x.CompanyIndex == config.CompanyIndex)
                .Select(x => x.DepartmentIndex).ToList();

            string query = @"SELECT HR_Employee.EmployeeATID, HR_Employee.CompanyIndex, HR_Employee.EmployeeCode, HR_Employee.CardNumber,HR_Employee.LastName + ' ' + HR_Employee.MidName + ' ' + HR_Employee.FirstName AS FullName,
                HR_Employee.NickName, HR_Employee.Gender, '' as NameOnMachine,HR_WorkingInfo.DepartmentIndex as DepartmentIndex,dep.[Name] as DepartmentName, HR_Employee.JoinedDate 
                FROM HR_Employee 
                LEFT JOIN HR_WorkingInfo ON HR_WorkingInfo.EmployeeATID = HR_Employee.EmployeeATID and HR_WorkingInfo.CompanyIndex= HR_Employee.CompanyIndex 
                and ((HR_WorkingInfo.ToDate is null and  Datediff(day, HR_WorkingInfo.FromDate, getdate()) >= 0) 
                OR(Datediff(day, HR_WorkingInfo.ToDate, getdate()) <= 0 AND Datediff(day, HR_WorkingInfo.FromDate, getdate()) >= 0))
                LEFT OUTER JOIN HR_Department ON HR_Department.[Index] = HR_WorkingInfo.DepartmentIndex
                LEFT OUTER JOIN HR_Position ON HR_WorkingInfo.PositionIndex = HR_Position.[Index]
                LEFT OUTER JOIN HR_Titles ON HR_WorkingInfo.TitlesIndex = HR_Titles.[Index]
                and(HR_WorkingInfo.ToDate is null OR
                (Datediff(day, HR_WorkingInfo.ToDate, getdate()) <= 0 AND Datediff(day, HR_WorkingInfo.FromDate, getdate()) >= 0))
                left join HR_Department dep on dep.[Index]= HR_WorkingInfo.DepartmentIndex 
                WHERE (HR_Employee.MarkForDelete = 0) AND HR_Employee.CompanyIndex = @CompanyIndex ";
            List<HR_EmployeeReport> listEmployee;
            SqlParameter paramCompany = new SqlParameter("CompanyIndex", config.CompanyIndex);
            SqlParameter param = new SqlParameter("filter", "%" + filter + "%");
            countData = 0;
            if (page <= 1)
            {

                if (string.IsNullOrEmpty(filter) == false)
                {
                    query += " and (HR_Employee.EmployeeATID like @filter or HR_Employee.CardNumber like @filter " +
                    "or HR_Employee.Gender like @filter or dep.[Name] like @filter " +
                    "or HR_Employee.FirstName like @filter or HR_Employee.MidName like @filter or HR_Employee.LastName like @filter) ";
                    var iquery = otherContext.HR_EmployeeReport.FromSqlRaw(query, param, paramCompany).Where(x => x.DepartmentIndex == null || lstDeptIndex.Contains(x.DepartmentIndex.Value));
                    listEmployee = iquery.Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                    countData = iquery.Count();
                }
                else
                {
                    var iquery = otherContext.HR_EmployeeReport.FromSqlRaw(query, paramCompany).Where(x => x.DepartmentIndex == null || lstDeptIndex.Contains(x.DepartmentIndex.Value));
                    listEmployee = iquery.Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                    countData = iquery.Count();
                }

            }
            else
            {
                int fromRow = GlobalParams.ROWS_NUMBER_IN_PAGE * (page - 1);

                if (string.IsNullOrEmpty(filter) == false)
                {
                    query += " and (HR_Employee.EmployeeATID like @filter or HR_Employee.CardNumber like @filter " +
                     "or HR_Employee.Gender like @filter or dep.[Name] like @filter " +
                     "or HR_Employee.FirstName like @filter or HR_Employee.MidName like @filter or HR_Employee.LastName like @filter) ";
                    var iquery = otherContext.HR_EmployeeReport.FromSqlRaw(query, param, paramCompany).Where(x => x.DepartmentIndex == null || lstDeptIndex.Contains(x.DepartmentIndex.Value));
                    listEmployee = iquery.OrderBy(t => t.EmployeeATID).Skip(fromRow).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                    countData = iquery.Count();
                }
                else
                {
                    var iquery = otherContext.HR_EmployeeReport.FromSqlRaw(query, paramCompany).Where(x => x.DepartmentIndex == null || lstDeptIndex.Contains(x.DepartmentIndex.Value));
                    listEmployee = iquery.OrderBy(t => t.EmployeeATID).Skip(fromRow).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                    countData = iquery.Count();
                }

            }
            return listEmployee;
        }

        public static List<HR_EmployeeReport> GetAllEmployeeReport(ezHR_Context pContext, ConfigObject config)
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

            foreach (var item in listEmployee)
            {
                item.EmployeeATID = item.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
            }
            return listEmployee;
        }

        public static List<HR_EmployeeReport> GetEmployeeReportByEmps(ezHR_Context pContext, ConfigObject config, List<string> pListEmp)
        {
            string strEmps = "";
            for (int i = 0; i < pListEmp.Count; i++)
            {
                strEmps += "" + pListEmp[i] + ",";
            }
            if (strEmps.Length > 0)
            {
                strEmps = strEmps.Substring(0, strEmps.Length - 1);
            }
            else
            {
                strEmps = "''";
            }
            string query = "SELECT HR_Employee.EmployeeATID, HR_Employee.CompanyIndex, HR_Employee.EmployeeCode, HR_Employee.CardNumber,HR_Employee.LastName + ' ' + HR_Employee.MidName + ' ' + HR_Employee.FirstName AS FullName," +
                " HR_Employee.NickName, HR_Employee.Gender, '' as NameOnMachine,HR_WorkingInfo.DepartmentIndex as DepartmentIndex,dep.[Name] as DepartmentName, HR_Employee.JoinedDate  " +
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
                " WHERE HR_Employee.EmployeeATID in (" + strEmps + ") and HR_Employee.CompanyIndex = @CompanyIndex";
            SqlParameter paramCompany = new SqlParameter("CompanyIndex", config.CompanyIndex);

            List<HR_EmployeeReport> listEmployee;
            listEmployee = pContext.HR_EmployeeReport.FromSqlRaw(query, paramCompany).ToList();
            foreach (var item in listEmployee)
            {
                item.EmployeeATID = item.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
            }
            return listEmployee;
        }
        private List<HR_EmployeeReport> GetEmployeeReportFromDBEPAD(string filter, int page, ref int countData, DateTime date, int companyIndex, int pAccountPrivilege)
        {
            string query = @"SELECT HR_User.EmployeeATID, 
	                        HR_User.CompanyIndex, 
	                        HR_User.EmployeeCode, 
	                        HR_User.CardNumber,
	                        HR_User.FullName AS FullName,
                            '' as NickName, 
                            HR_User.Gender, 
                            HR_User.NameOnMachine as NameOnMachine,
                            IC_WorkingInfo.DepartmentIndex as DepartmentIndex,
                            dep.[Name] as DepartmentName 
                        FROM HR_User LEFT JOIN IC_WorkingInfo 
	                        ON IC_WorkingInfo.EmployeeATID = HR_User.EmployeeATID 
	                        and IC_WorkingInfo.CompanyIndex= HR_User.CompanyIndex 
                            and IC_WorkingInfo.[Status] = 1 
                        left join IC_Department dep 
                            on dep.[Index]= IC_WorkingInfo.DepartmentIndex 
                         
                        WHERE 
                        (
	                        HR_User.StoppedDate is null 
	                        or 
	                        HR_User.StoppedDate >  @date
                        ) 
                        AND HR_User.CompanyIndex = @CompanyIndex
                        AND 
                        (
	                        dep.[Index] IS NULL
	                        OR
	                        dep.[Index] IN (SELECT DepartmentIndex FROM IC_PrivilegeDepartment WHERE PrivilegeIndex = @PrivilegeIndex)
                        )
                         AND         
                        (
    	                     (IC_WorkingInfo.ToDate is null and  Datediff(day, IC_WorkingInfo.FromDate, @date) >= 0) 
                             OR
                             (Datediff(day, IC_WorkingInfo.ToDate,  @date) <= 0 AND Datediff(day, IC_WorkingInfo.FromDate,  @date) >= 0)
	                   )";
            SqlParameter param = new SqlParameter("filter", "%" + filter + "%");
            SqlParameter paramCompany = new SqlParameter("CompanyIndex", companyIndex);
            SqlParameter paramPrivilege = new SqlParameter("PrivilegeIndex", pAccountPrivilege);
            SqlParameter paramDate = new SqlParameter("date", date);

            List<HR_EmployeeReport> listEmployee;

            if (page <= 1)
            {

                if (string.IsNullOrEmpty(filter) == true)
                {
                    listEmployee = context.HR_EmployeeReport.FromSqlRaw(query, paramCompany, paramDate, paramPrivilege).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                    countData = context.HR_EmployeeReport.FromSqlRaw(query, paramCompany, paramDate, paramPrivilege).Count();
                }
                else
                {
                    query += " and (HR_User.EmployeeATID like @filter or HR_User.CardNumber like @filter " +
                        "or HR_User.Gender like @filter or dep.[Name] like @filter " +
                         "or HR_User.FullName like @filter) ";
                    listEmployee = context.HR_EmployeeReport.FromSqlRaw(query, param, paramCompany, paramDate, paramPrivilege).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                    countData = context.HR_EmployeeReport.FromSqlRaw(query, param, paramCompany, paramDate, paramPrivilege).Count();
                }

            }
            else
            {
                int fromRow = GlobalParams.ROWS_NUMBER_IN_PAGE * (page - 1);

                if (string.IsNullOrEmpty(filter) == true)
                {
                    listEmployee = context.HR_EmployeeReport.FromSqlRaw(query, paramCompany, paramDate, paramPrivilege).OrderBy(t => t.EmployeeATID).Skip(fromRow).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                    countData = context.HR_EmployeeReport.FromSqlRaw(query, paramCompany, paramDate, paramPrivilege).Count();


                }
                else
                {
                    query += " and (HR_User.EmployeeATID like @filter or HR_User.CardNumber like @filter " +
                        "or HR_User.Gender like @filter or dep.[Name] like @filter " +
                         "or HR_User.FullName like @filter) ";
                    listEmployee = context.HR_EmployeeReport.FromSqlRaw(query, param, paramCompany, paramDate, paramPrivilege).OrderBy(t => t.EmployeeATID)
                        .Skip(fromRow).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                    countData = context.HR_EmployeeReport.FromSqlRaw(query, param, paramCompany, paramDate, paramPrivilege).Count();
                }
            }
            return listEmployee;
        }
        public static List<HR_EmployeeReport> GetAllEmployeeReportFromDBEPAD(EPAD_Context context, DateTime date, int companyIndex)
        {
            string query = @"SELECT 
                            ie.EmployeeATID, 
                            ie.CompanyIndex, 
                            ie.EmployeeCode, 
                            ie.CardNumber,
                            ie.FullName AS FullName,
                            '' as NickName, 
                            ie.Gender,
                            ie.NameOnMachine as NameOnMachine,
                            dep.[Index] as DepartmentIndex,
                            dep.[Name] as DepartmentName 

                            FROM HR_User ie LEFT JOIN IC_WorkingInfo  iw
	                            ON iw.EmployeeATID = ie.EmployeeATID 
	                            and iw.CompanyIndex = ie.CompanyIndex

                            LEFT JOIN IC_Department dep 
	                            on dep.[Index]= iw.DepartmentIndex 

                            WHERE (ie.StoppedDate is null or ie.StoppedDate >  @date) 
                            AND ie.CompanyIndex = @CompanyIndex
                            AND Datediff(day, iw.FromDate, @date) >= 0
                            AND (iw.ToDate is NULL OR Datediff(day, iw.ToDate, @date) <= 0 )
                            AND iw.[Status] = 1
                            OR iw.DepartmentIndex IS NULL";
            SqlParameter paramCompany = new SqlParameter("CompanyIndex", companyIndex);
            SqlParameter paramDate = new SqlParameter("date", date);

            List<HR_EmployeeReport> listEmployee;
            listEmployee = context.HR_EmployeeReport.FromSqlRaw(query, paramCompany, paramDate).ToList();

            return listEmployee;
        }

        [Authorize]
        [ActionName("GetEmployeeAsTree")]
        [HttpGet]
        public IActionResult GetEmployeeAsTree(int? userTypeNotUse)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            List<AddedParam> addedParams = new List<AddedParam>();
            ConfigObject config = ConfigObject.GetConfig(cache);
            List<IC_EmployeeTreeDTO> listData = new List<IC_EmployeeTreeDTO>();
            DateTime now = DateTime.Now;
            //use db EPAD
            if (config.IntegrateDBOther == false)
            {
                IC_Company company = context.IC_Company.Where(t => t.Index == user.CompanyIndex).FirstOrDefault();
                List<IC_Department> listDep = context.IC_Department.Where(t => t.CompanyIndex == user.CompanyIndex && t.IsInactive != true).ToList();

                if (userTypeNotUse == (short)EmployeeType.Driver)
                {
                    listDep = listDep.Where(x => x.IsDriverDepartment != true).ToList();
                }
                addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "IsCurrentTransfer", Value = true });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
                List<IC_EmployeeTransferDTO> listEmTransfer = _iC_EmployeeTransferLogic.GetMany(addedParams);

                addedParams = new List<AddedParam>();
                if (listEmTransfer != null && listEmTransfer.Count > 0)
                {
                    addedParams.Add(new AddedParam { Key = "ListEmployeeTransferATID", Value = listEmTransfer.Select(e => e.EmployeeATID).ToList() });
                }
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
                addedParams.Add(new AddedParam { Key = "IsCurrentWorkingAndNoDepartment", Value = true });
                List<IC_EmployeeDTO> listEmployee = _IIC_EmployeeLogic.GetEmployeeList(addedParams); //GetAllEmployeeReportFromDBEPAD(context,now, user.CompanyIndex);

                if (userTypeNotUse != (short)EmployeeType.Driver)
                {
                    //Get driver info
                    var listEmployeeInfo = context.IC_PlanDock.Where(x => x.CompanyIndex == user.CompanyIndex)
                                                              .Select(driverInfo => new IC_EmployeeDTO
                                                              {
                                                                  EmployeeATID = driverInfo != null ? driverInfo.TripId : "",
                                                                  FullName = driverInfo != null ? driverInfo.DriverName : "",
                                                                  CompanyIndex = driverInfo.CompanyIndex,
                                                                  DepartmentIndex = driverInfo != null ? driverInfo.SupplierId : 0
                                                              }).ToList();
                    listEmployee.AddRange(listEmployeeInfo);
                }


                listEmployee = _IIC_EmployeeLogic.CheckCurrentDepartment(listEmployee);
                // chỉ hiển thị những phòng ban dc phân quyền
                var hsDept = user.ListDepartmentAssignedAndParent.ToHashSet();
                listDep = listDep.Where(t => hsDept.Contains(t.Index)).ToList();
                int id = 1; int level = 1;

                IC_EmployeeTreeDTO mainData = new IC_EmployeeTreeDTO();

                mainData.ID = -1; mainData.Code = company.Index.ToString(); mainData.Name = company.Name;
                mainData.Type = "Company"; mainData.Level = level;
                level++;

                List<IC_EmployeeTreeDTO> listChildrentForCompany = new List<IC_EmployeeTreeDTO>();
                List<IC_Department> listDepLV1 = listDep.Where(t => t.ParentIndex == null || t.ParentIndex == 0).ToList();
                //create phòng ban 'ko có phòng ban'
                listDepLV1.Add(new IC_Department()
                {
                    Index = 0,
                    Name = "Không có phòng ban",
                    Code = "",
                    ParentIndex = 0,
                    CompanyIndex = user.CompanyIndex
                });

                for (int i = 0; i < listDepLV1.Count; i++)
                {
                    IC_EmployeeTreeDTO currentDep = new IC_EmployeeTreeDTO();
                    currentDep.ID = id++; //listDepLV1[i].Index;//Convert.ToDecimal(id + "." + (i + 1));
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
                HR_Company company = otherContext.HR_Company.FirstOrDefault();
                if (company == null)
                {
                    return NoContent();
                }
                List<HR_Department> listDep = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex).ToList();
                List<HR_EmployeeReport> listEmployee = _IHR_EmployeeLogic.GetAllEmployeeReport();

                // chỉ hiển thị những phòng ban dc phân quyền
                listDep = listDep.Where(t => user.ListDepartmentAssignedAndParent.Contains(t.Index)).ToList();
                int id = 1; int level = 1;

                IC_EmployeeTreeDTO mainData = new IC_EmployeeTreeDTO();

                mainData.ID = -1; mainData.Code = company.Index.ToString(); mainData.Name = company.Name;
                mainData.Type = "Company"; mainData.Level = level;
                level++;
                List<IC_EmployeeTreeDTO> listChildrentForCompany = new List<IC_EmployeeTreeDTO>();
                List<HR_Department> listDepLV1 = listDep.Where(t => t.ParentIndex == null || t.ParentIndex == 0).ToList();
                listDepLV1.Add(new HR_Department()
                {
                    Index = id++,
                    Name = "Không có phòng ban",
                    NameInEng = "Not in department",
                    Code = "",
                    ParentIndex = 0,
                    CompanyIndex = user.CompanyIndex
                });
                for (int i = 0; i < listDepLV1.Count; i++)
                {
                    IC_EmployeeTreeDTO currentDep = new IC_EmployeeTreeDTO();
                    currentDep.ID = id++;// listDepLV1[i].Index;// Convert.ToDecimal(id + "." + (i + 1));
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

            result = Ok(listData);
            return result;
        }

        private List<IC_EmployeeTreeDTO> RecursiveGetChildrentDepartment(List<IC_Department> listDep, List<IC_EmployeeDTO> listEmployee, int pCurrentIndex, ref int pId, int pLevel)
        {
            List<IC_Department> listChildrent = listDep.Where(t => t.ParentIndex == pCurrentIndex).ToList();
            List<IC_EmployeeTreeDTO> listDepReturn = new List<IC_EmployeeTreeDTO>();
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
            List<IC_EmployeeTreeDTO> listEmpReturn = new List<IC_EmployeeTreeDTO>();
            for (int i = 0; i < listEmp.Count; i++)
            {
                IC_EmployeeTreeDTO currentEmp = new IC_EmployeeTreeDTO();
                currentEmp.EmployeeATID = listEmp[i].EmployeeATID;
                currentEmp.EmployeeCode = listEmp[i].EmployeeCode;
                currentEmp.NRIC = listEmp[i].NRIC;
                currentEmp.ID = pId++;//pId + "." + (i + 1); 
                currentEmp.Code = listEmp[i].EmployeeATID; ; currentEmp.Name = listEmp[i].EmployeeATID + "-" + listEmp[i].FullName;
                currentEmp.Type = "Employee"; currentEmp.Level = pLevel;
                if (listEmp[i].Gender != null)
                    currentEmp.Gender = listEmp[i].Gender.Value == (short)GenderEnum.Female ? "Female" : listEmp[i].Gender.Value == (short)GenderEnum.Male ? "Male" : "Other";

                listEmpReturn.Add(currentEmp);
            }
            return listEmpReturn;
        }

        private List<IC_EmployeeTreeDTO> RecursiveGetChildrentDepartment_DBOther(List<HR_Department> listDep, List<HR_EmployeeReport> listEmployee, int pCurrentIndex, ref int pId, int pLevel)
        {
            List<HR_Department> listChildrent = listDep.Where(t => t.ParentIndex == pCurrentIndex).ToList();
            List<IC_EmployeeTreeDTO> listDepReturn = new List<IC_EmployeeTreeDTO>();
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

        [Authorize]
        [ActionName("AddEmployee")]
        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody] IC_EmployeeParamDTO param)
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            var config = ConfigObject.GetConfig(cache);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (IC_EmployeeParamDTO)StringHelper.RemoveWhiteSpace(param);

            if (param.EmployeeATID == "" || param.FullName == "")
            {
                return BadRequest("PleaseFillAllRequiredFields");
            }

            var checkData = context.HR_User.Where(t => t.CompanyIndex == user.CompanyIndex && t.EmployeeATID == param.EmployeeATID).FirstOrDefault();
            if (checkData != null)
            {
                return Conflict("EmployeeATIDIsExist");
            }

            var employee = new HR_User();
            employee.EmployeeATID = param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
            employee.CompanyIndex = user.CompanyIndex;
            employee.EmployeeCode = param.EmployeeCode;
            employee.FullName = param.FullName;
            employee.Gender = param.Gender;
            employee.CreatedDate = DateTime.Now;
            employee.UpdatedDate = DateTime.Now;
            employee.UpdatedUser = user.UserName;
            //employee.JoinedDate = param.JoinedDate;
            //employee.CardNumber = param.CardNumber;
            //employee.NameOnMachine = param.NameOnMachine;
            //employee.DepartmentIndex = param.DepartmentIndex;
            context.HR_User.Add(employee);

            var userMaster = new IC_UserMasterDTO();
            userMaster.EmployeeATID = employee.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
            userMaster.CompanyIndex = employee.CompanyIndex;
            //userMaster.CardNumber = employee.CardNumber;
            //userMaster.NameOnMachine = employee.NameOnMachine;
            userMaster.Privilege = 0;
            userMaster.FaceIndex = 50;
            userMaster.CreatedDate = DateTime.Now;
            userMaster.UpdatedUser = employee.UpdatedUser;
            if (!string.IsNullOrWhiteSpace(param.ImageUpload))
            {
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
            if (param.ListFinger != null && param.ListFinger.Count > 0)
            {

                userMaster.FingerData0 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 0);
                userMaster.FingerData1 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 1);
                userMaster.FingerData2 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 2);
                userMaster.FingerData3 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 3);
                userMaster.FingerData4 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 4);
                userMaster.FingerData5 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 5);
                userMaster.FingerData6 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 6);
                userMaster.FingerData7 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 7);
                userMaster.FingerData8 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 8);
                userMaster.FingerData9 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 9);
            }
            _iC_UserMasterLogic.CheckExistedOrCreate(userMaster);

            var workingInfo = new IC_WorkingInfoDTO()
            {
                CompanyIndex = user.CompanyIndex,
                EmployeeATID = param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0'),
                DepartmentIndex = param.DepartmentIndex,
                FromDate = param.JoinedDate,
                ToDate = null,
                UpdatedDate = DateTime.Now,
                UpdatedUser = user.UserName,
                Status = (short)TransferStatus.Approve,
                PositionName = param.PositionName
            };
            _IIC_WorkingInfoLogic.CheckUpdateOrInsert(workingInfo);

            context.SaveChanges();

            // Add audit log
            var audit = new IC_AuditEntryDTO(null);
            audit.TableName = "HR_User";
            audit.UserName = user.UserName;
            audit.CompanyIndex = user.CompanyIndex;
            audit.State = AuditType.Added;
            //audit.Description = AuditType.Added.ToString() + "Employee: " + param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
            audit.Description = AuditType.Added.ToString() + "Employee:/:" + param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
            audit.DateTime = DateTime.Now;
            _iIC_AuditLogic.Create(audit);

            if (param.DepartmentIndex > 0)
            {
                var addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER });
                var systemconfigs = await _iC_ConfigLogic.GetMany(addedParams);
                if (systemconfigs != null)
                {
                    var sysconfig = systemconfigs.FirstOrDefault();
                    if (sysconfig != null)
                    {
                        if (sysconfig.IntegrateLogParam.AutoIntegrate)
                        {
                            await _IIC_CommandLogic.SyncWithEmployee(new List<string> { employee.EmployeeATID }, user.CompanyIndex);
                        }
                    }
                }
            }

            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("ImportImployee")]
        [HttpPost]
        public IActionResult ImportImployee([FromBody] List<IC_EmployeeParamDTO> listParams)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            ConfigObject config = ConfigObject.GetConfig(cache);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            List<HR_User> listData = context.HR_User.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            DateTime now = DateTime.Now;

            int rowSuccess = 0; int rowError = 0;
            int rowAdd = 0; int rowUpdate = 0;
            StringBuilder message = new StringBuilder();
            for (int i = 0; i < listParams.Count; i++)
            {
                HR_User employee = listData.Find(t => t.EmployeeATID == listParams[i].EmployeeATID);
                try
                {
                    var department = new IC_DepartmentDTO();
                    department.Name = listParams[i].DepartmentCode;
                    department.Location = "";
                    department.Description = "";
                    department.Code = listParams[i].DepartmentCode;
                    department.ParentIndex = null;
                    department.CompanyIndex = user.CompanyIndex;
                    department.CreatedDate = DateTime.Now;
                    department.UpdatedDate = DateTime.Now;
                    department.UpdatedUser = user.UserName;

                    department = _iC_DepartmentLogic.CheckExistedOrCreate(department, "");

                    if (employee != null)
                    {
                        employee.EmployeeCode = listParams[i].EmployeeCode;
                        employee.FullName = listParams[i].FullName;
                        employee.Gender = listParams[i].Gender;
                        //employee.DepartmentIndex = (int)department.Index;
                        //employee.NameOnMachine = listParams[i].NameOnMachine;
                        employee.UpdatedDate = now;
                        employee.UpdatedUser = user.UserName;

                        rowUpdate++;
                    }
                    else
                    {
                        employee = new HR_User();
                        employee.EmployeeATID = listParams[i].EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                        employee.CompanyIndex = user.CompanyIndex;
                        employee.EmployeeCode = listParams[i].EmployeeCode;
                        employee.FullName = listParams[i].FullName;
                        employee.Gender = listParams[i].Gender;
                        //employee.DepartmentIndex = (int)department.Index;
                        //employee.NameOnMachine = listParams[i].NameOnMachine;

                        employee.CreatedDate = now;

                        employee.UpdatedDate = now;
                        employee.UpdatedUser = user.UserName;

                        context.HR_User.Add(employee);
                        rowAdd++;
                    }

                    IC_WorkingInfoDTO workingInfo = new IC_WorkingInfoDTO()
                    {
                        CompanyIndex = user.CompanyIndex,
                        EmployeeATID = listParams[i].EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0'),
                        DepartmentIndex = (int)department.Index,
                        FromDate = now,
                        ToDate = null,
                        UpdatedDate = now,
                        UpdatedUser = user.UserName,
                        Status = (short)TransferStatus.Approve
                    };
                    _IIC_WorkingInfoLogic.CheckExistedOrCreate(workingInfo);

                    IC_UserMasterDTO userMaster = new IC_UserMasterDTO();
                    userMaster.EmployeeATID = employee.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                    userMaster.CompanyIndex = employee.CompanyIndex;
                    userMaster.Privilege = 0;
                    //userMaster.NameOnMachine = employee.NameOnMachine;
                    //userMaster.CardNumber = employee.CardNumber;
                    userMaster.CreatedDate = DateTime.Now;
                    userMaster.UpdatedUser = employee.UpdatedUser;
                    _iC_UserMasterLogic.CheckExistedOrCreate(userMaster);
                    rowSuccess++;
                }
                catch (Exception ex)
                {
                    rowError++;
                    message.Append("Lỗi tại dòng " + i + ". " + ex.Message);
                }

            }

            context.SaveChanges();
            message.Insert(0, "Số dòng thành công: " + rowSuccess + "\n"
                + "Số dòng thất bại: " + rowError + "\n"
                + "Số dòng thêm mới: " + rowAdd + "\n"
                + "Số dòng cập nhật: " + rowUpdate + "\n");

            result = Ok(message.ToString());
            return result;
        }

        [Authorize]
        [ActionName("UpdateEmployee")]
        [HttpPost]
        public async Task<IActionResult> UpdateEmployee([FromBody] IC_EmployeeParamDTO param)
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            var config = ConfigObject.GetConfig(cache);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (IC_EmployeeParamDTO)StringHelper.RemoveWhiteSpace(param);
            var employee = context.HR_User.Where(t => t.CompanyIndex == user.CompanyIndex && t.EmployeeATID == param.EmployeeATID).FirstOrDefault();
            if (employee == null)
            {
                return NotFound("EmployeeNotExist");
            }

            employee.EmployeeCode = param.EmployeeCode;
            employee.FullName = param.FullName;
            employee.Gender = param.Gender;
            employee.UpdatedDate = DateTime.Now;
            employee.UpdatedUser = user.UserName;
            //employee.CardNumber = param.CardNumber;
            //employee.NameOnMachine = param.NameOnMachine;
            //employee.DepartmentIndex = param.DepartmentIndex;
            //employee.JoinedDate = param.JoinedDate;
            context.SaveChanges();

            var userMaster = new IC_UserMasterDTO();
            userMaster.EmployeeATID = employee.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
            userMaster.CompanyIndex = employee.CompanyIndex;
            //userMaster.CardNumber = employee.CardNumber;
            //userMaster.NameOnMachine = employee.NameOnMachine;
            userMaster.UpdatedDate = DateTime.Now;
            userMaster.UpdatedUser = employee.UpdatedUser;
            if (!string.IsNullOrWhiteSpace(param.ImageUpload))
            {
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
            if (param.ListFinger != null && param.ListFinger.Count > 0)
            {
                userMaster.FingerData0 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 0);
                userMaster.FingerData1 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 1);
                userMaster.FingerData2 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 2);
                userMaster.FingerData3 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 3);
                userMaster.FingerData4 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 4);
                userMaster.FingerData5 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 5);
                userMaster.FingerData6 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 6);
                userMaster.FingerData7 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 7);
                userMaster.FingerData8 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 8);
                userMaster.FingerData9 = _iC_UserMasterLogic.GetFingerDataListString(param.ListFinger, 9);
            }
            await _iC_UserMasterLogic.SaveAndOverwriteList(new List<IC_UserMasterDTO> { userMaster });

            var workingInfo = new IC_WorkingInfoDTO()
            {
                CompanyIndex = user.CompanyIndex,
                EmployeeATID = param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0'),
                DepartmentIndex = param.DepartmentIndex,
                FromDate = param.JoinedDate,
                ToDate = null,
                UpdatedDate = DateTime.Now,
                UpdatedUser = user.UserName,
                Status = (int)TransferStatus.Approve
            };

            if (workingInfo.PositionIndex == 0)
                workingInfo.PositionName = param.PositionName;

            _IIC_WorkingInfoLogic.CheckUpdateOrInsert(workingInfo);

            // Add audit log
            var audit = new IC_AuditEntryDTO(null);
            audit.TableName = "HR_User";
            audit.UserName = user.UserName;
            audit.CompanyIndex = user.CompanyIndex;
            audit.State = AuditType.Modified;
            //audit.Description = AuditType.Modified.ToString() + "Employee: " + param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
            audit.Description = AuditType.Modified.ToString() + "Employee:/:" + param.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
            audit.DateTime = DateTime.Now;
            _iIC_AuditLogic.Create(audit);

            if (param.DepartmentIndex > 0)
            {
                var addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER });
                var systemconfigs = await _iC_ConfigLogic.GetMany(addedParams);
                if (systemconfigs != null)
                {
                    var sysconfig = systemconfigs.FirstOrDefault();
                    if (sysconfig != null)
                    {
                        if (sysconfig.IntegrateLogParam.AutoIntegrate)
                        {
                            await _IIC_CommandLogic.SyncWithEmployee(new List<string> { param.EmployeeATID }, user.CompanyIndex);
                        }
                    }
                }
            }

            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("DeleteEmployee")]
        [HttpPost]
        public IActionResult DeleteEmployee([FromBody] List<AddedParam> addedParams)
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
                listPadleft.Add(item.PadLeft(_config.MaxLenghtEmployeeATID, '0'));
            }
            var existedEmployee = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listPadleft.Contains(e.EmployeeATID)).ToList();
            if (paramDeleteOndevice != null)
            {
                isDeleteOnDevice = Convert.ToBoolean(paramDeleteOndevice.Value);
                if (isDeleteOnDevice)
                {
                    if (existedEmployee != null)
                    {
                        listPadleft = existedEmployee.Select(e => e.EmployeeATID).ToList();
                        var lsSerialHw = context.IC_Device.Where(e => e.CompanyIndex == user.CompanyIndex).Select(e => e.SerialNumber).ToList();

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

            var listEmp = context.HR_User.Where(t => t.CompanyIndex == user.CompanyIndex && listPadleft.Contains(t.EmployeeATID)).ToArray();
            var listWorkingInfo = context.IC_WorkingInfo.Where(x => x.CompanyIndex == user.CompanyIndex && listPadleft.Contains(x.EmployeeATID)).ToArray();
            var listWorkingTransfer = context.IC_EmployeeTransfer.Where(x => x.CompanyIndex == user.CompanyIndex && listPadleft.Contains(x.EmployeeATID)).ToArray();
            var listUserMaster = context.IC_UserMaster.Where(x => x.CompanyIndex == user.CompanyIndex && listPadleft.Contains(x.EmployeeATID)).ToArray();

            try
            {
                context.HR_User.RemoveRange(listEmp);
                context.IC_WorkingInfo.RemoveRange(listWorkingInfo);
                context.IC_EmployeeTransfer.RemoveRange(listWorkingTransfer);
                context.IC_UserMaster.RemoveRange(listUserMaster);
                context.SaveChanges();
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "HR_User";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Deleted;
                //audit.Description = AuditType.Deleted.ToString() + "Employee: " + listEmp.Select(e => e.EmployeeATID).ToString();
                audit.Description = AuditType.Deleted.ToString() + "Employee:/:" + String.Join(", ", listEmp.Select(e => e.EmployeeATID));
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);
                result = Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return result;
        }

        [Authorize]
        [ActionName("ExportTemplateICEmployee")]
        [HttpGet]
        public IActionResult ExportTemplateICEmployee()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            string sWebRootFolder = _hostingEnvironment.ContentRootPath;

            var folderDetails = Path.Combine(sWebRootFolder, @"epad/dist/Template_IC_Employee.xlsx");
            //Use local 
#if DEBUG
            folderDetails = Path.Combine(sWebRootFolder, @"epad/public/Template_IC_Employee.xlsx");
#endif

#if !DEBUG
            _IIC_EmployeeLogic.ExportTemplateICEmployee(folderDetails);
#endif

            return ApiOk();
        }

        [Authorize]
        [ActionName("AddEmployeeFromExcel")]
        [HttpPost]
        public async Task<IActionResult> AddEmployeeFromExcel([FromBody] List<IC_EmployeeImportDTO> param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            ConfigObject config = ConfigObject.GetConfig(cache);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            // validation data
            List<IC_EmployeeImportDTO> listError = new List<IC_EmployeeImportDTO>();
            listError = _IIC_EmployeeLogic.ValidationImportEmployee(param);
            var message = "";
            if (_configClientName.ToUpper() == ClientName.MAY.ToString())
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/EmployeesImportError_MAY.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/EmployeesImportError_MAY.xlsx"));
                if (listError != null && listError.Count() > 0)
                {
                    var listEmployeeIDError = listError.Select(e => e.EmployeeATID).ToList();
                    param = param.Where(e => !listEmployeeIDError.Contains(e.EmployeeATID)).ToList();
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("EmployeeError");
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Mã chấm ăn của khách hàng";
                        worksheet.Cell(currentRow, 2).Value = "Mã số khách hàng";
                        worksheet.Cell(currentRow, 3).Value = "Họ tên khách hàng";
                        worksheet.Cell(currentRow, 4).Value = "Mã code thẻ";
                        worksheet.Cell(currentRow, 5).Value = "Giới tính (Nam)";
                        worksheet.Cell(currentRow, 6).Value = "Đơn vị khách hàng";
                        worksheet.Cell(currentRow, 7).Value = "Đối tượng khách hàng";
                        worksheet.Cell(currentRow, 8).Value = "Ngày vào";
                        worksheet.Cell(currentRow, 9).Value = "Email";
                        worksheet.Cell(currentRow, 10).Value = "SDT";
                        worksheet.Cell(currentRow, 11).Value = "Địa chỉ";
                        worksheet.Cell(currentRow, 12).Value = "Ghi chú";
                        worksheet.Cell(currentRow, 13).Value = "Tên phụ huynh 1";
                        worksheet.Cell(currentRow, 14).Value = "Email phụ huynh 1";
                        worksheet.Cell(currentRow, 15).Value = "SDT phụ huynh 1";
                        worksheet.Cell(currentRow, 16).Value = "Tên phụ huynh 2";
                        worksheet.Cell(currentRow, 17).Value = "Email phụ huynh 2";
                        worksheet.Cell(currentRow, 18).Value = "SDT phụ huynh 2";
                        worksheet.Cell(currentRow, 19).Value = "Lỗi";
                        for (int i = 1; i < 12; i++)
                        {
                            worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Column(i).Width = 20;
                        }

                        foreach (var users in listError)
                        {
                            currentRow++;
                            //New template
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
                            worksheet.Cell(currentRow, 4).Value = users.CardNumber;

                            worksheet.Cell(currentRow, 5).Value = users.Gender;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = users.DepartmentName;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = users.Position;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = users.JoinedDate;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 9).Value = users.Email;
                            worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 10).Value = users.PhoneNumber;
                            worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 11).Value = users.Address;
                            worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 12).Value = users.Note;
                            worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 13).Value = users.ParentName1;
                            worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 14).Value = users.ParentEmail1;
                            worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 15).Value = users.ParentPhone1;
                            worksheet.Cell(currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 16).Value = users.ParentName2;
                            worksheet.Cell(currentRow, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 17).Value = users.ParentEmail2;
                            worksheet.Cell(currentRow, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 18).Value = users.ParentPhone2;
                            worksheet.Cell(currentRow, 18).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 19).Value = users.ErrorMessage;
                            worksheet.Cell(currentRow, 19).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        workbook.SaveAs(file.FullName);
                    }

                }

                var listDepartmet = param.GroupBy(e => e.DepartmentName).Select(e => e.First()).ToList();
                var listDepartmentName = param.Select(x => x.DepartmentName.Trim()).ToHashSet();
                var listDepartmentCreate = new List<IC_DepartmentDTO>();
                foreach (var departmentName in listDepartmentName)
                {
                    if (departmentName.Contains("/"))
                    {
                        var listSplitDepartmentName = departmentName.Split("/").Distinct().ToList();
                        for (var i = 0; i < listSplitDepartmentName.Count; i++)
                        {
                            if (listDepartmentCreate.Any(x => x.Name == listSplitDepartmentName[i]))
                            {
                                continue;
                            }
                            if (i == 0)
                            {
                                listDepartmentCreate.Add(new IC_DepartmentDTO
                                {
                                    Name = listSplitDepartmentName[i],
                                    CompanyIndex = user.CompanyIndex,
                                    ParentIndex = 0,
                                    ParentName = string.Empty,
                                    UpdatedDate = DateTime.Now,
                                    UpdatedUser = user.FullName,
                                    CreatedDate = DateTime.Now
                                });
                            }
                            else
                            {
                                listDepartmentCreate.Add(new IC_DepartmentDTO
                                {
                                    Name = listSplitDepartmentName[i],
                                    CompanyIndex = user.CompanyIndex,
                                    ParentIndex = 0,
                                    ParentName = listSplitDepartmentName[i - 1],
                                    UpdatedDate = DateTime.Now,
                                    UpdatedUser = user.FullName,
                                    CreatedDate = DateTime.Now
                                });
                            }
                        }
                    }
                    else
                    {
                        listDepartmentCreate.Add(new IC_DepartmentDTO
                        {
                            Name = departmentName,
                            CompanyIndex = user.CompanyIndex,
                            ParentIndex = 0,
                            ParentName = string.Empty,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = user.FullName,
                            CreatedDate = DateTime.Now
                        });
                    }
                }

                //listCreate = _iC_DepartmentLogic.CheckExistedOrCreateList(listCreate);
                var listCreate = _iC_DepartmentLogic.CheckExistedOrCreateListDepartmentFromImportMAY(listDepartmentCreate, user);
                //List<IC_DepartmentDTO> listCreate = listDepartmet.Where(e => !string.IsNullOrEmpty(e.DepartmentName)).Select(e => new IC_DepartmentDTO
                //{
                //    Name = e.DepartmentName.Trim(),
                //    CompanyIndex = user.CompanyIndex,
                //    ParentIndex = 0,
                //    UpdatedDate = DateTime.Now,
                //    UpdatedUser = user.FullName,
                //    CreatedDate = DateTime.Now
                //}).ToList();
                //listCreate = _iC_DepartmentLogic.CheckExistedOrCreateList(listCreate);

                //var listPosition = param.GroupBy(e => e.Position).Select(e => e.First()).Select(e => e.Position).ToList();
                //var listPositionDB = _IHR_PositionInfoService.GetAll(e => listPosition.Contains(e.Name)).ToList();

                var listEmployeeID = param.Select(e => e.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0')).ToList();
                var listUserType = context.HR_UserType.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();

                var listHRUserDB = new List<HR_User>();
                var listEmployeeDB = new List<HR_EmployeeInfo>();
                var listWorkingInfoDB = new List<IC_WorkingInfo>();
                var listUserMasterDB = new List<IC_UserMaster>();
                var listCardNumberDB = new List<HR_CardNumberInfo>();
                var listUserContactInfo = context.HR_UserContactInfo.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();

                if (listEmployeeID.Count > 5000)
                {
                    var listSplitEmployeeID = CommonUtils.SplitList(listEmployeeID, 5000);
                    //var a = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listSplitEm.Contains(e.EmployeeATID)) && e.EmployeeType == (int)EmployeeType.Employee).ToList();
                    var listHRUser = context.HR_User.Where(x => listEmployeeID.Contains(x.EmployeeATID)).ToList();
                    var listEmployeeInfo = context.HR_EmployeeInfo.Where(x => listEmployeeID.Contains(x.EmployeeATID)).ToList();
                    var listWorkingInfo = context.IC_WorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)
                          && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && e.ToDate == null).OrderByDescending(e => e.FromDate).ToList();
                    var listUserMaster = context.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    var resultCardNumber = context.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();

                    foreach (var listEmployeeSplit in listSplitEmployeeID)
                    {
                        var resultHRUser = listHRUser.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultEmployee = listEmployeeInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultWorkingInfo = listWorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)
                           && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && e.ToDate == null).OrderByDescending(e => e.FromDate).ToList();
                        var resultUserMaster = listUserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();

                        listHRUserDB.AddRange(resultHRUser);
                        listEmployeeDB.AddRange(resultEmployee);
                        listWorkingInfoDB.AddRange(resultWorkingInfo);
                        listUserMasterDB.AddRange(resultUserMaster);
                        listCardNumberDB.AddRange(resultCardNumber);
                    }
                }
                else
                {
                    listHRUserDB = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listEmployeeDB = context.HR_EmployeeInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listWorkingInfoDB = context.IC_WorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)
                       && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && e.ToDate == null).OrderByDescending(e => e.FromDate).ToList();
                    listUserMasterDB = context.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listCardNumberDB = context.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();
                }

                //List<HR_User> listUser = new List<HR_User>();
                //List<HR_EmployeeInfo> listEmployee = new List<HR_EmployeeInfo>();
                //List<IC_WorkingInfoDTO> listWorkingInfo = new List<IC_WorkingInfoDTO>();
                //List<IC_UserMasterDTO> listUserMaster = new List<IC_UserMasterDTO>();
                foreach (IC_EmployeeImportDTO item in param)
                {
                    try
                    {
                        var department = new IC_DepartmentDTO();
                        department.Index = 0;
                        var splitDepartmentNameList = new List<string>();
                        if (item.DepartmentName.Contains("/"))
                        {
                            splitDepartmentNameList = item.DepartmentName.Split("/").ToList();
                            item.DepartmentName = splitDepartmentNameList[splitDepartmentNameList.Count - 1];
                        }
                        if (listCreate != null)
                        {
                            //department = listCreate.FirstOrDefault(e => !string.IsNullOrWhiteSpace(item.DepartmentName) && e.Name.ToLower() == item.DepartmentName.ToLower());
                            var itemDepartment = listCreate.FirstOrDefault(e => !string.IsNullOrWhiteSpace(item.DepartmentName) && e.Name.ToLower() == item.DepartmentName.ToLower());
                            if (itemDepartment != null)
                            {
                                department = itemDepartment;
                            }
                            if (department != null && itemDepartment == null)
                            {
                                department.Index = 0;
                            }
                        }


                        item.EmployeeATID = item.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');

                        var existedUser = listHRUserDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        var userType = listUserType.FirstOrDefault(x => x.Name == item.Position);
                        if (existedUser != null)
                        {
                            existedUser.EmployeeCode = item.EmployeeCode;
                            existedUser.FullName = item.FullName;
                            existedUser.Gender = (short)item.Gender;
                            existedUser.DayOfBirth = StringHelper.GetDateOfBirthEmployee("Day", item.DateOfBirth);
                            existedUser.MonthOfBirth = StringHelper.GetDateOfBirthEmployee("Month", item.DateOfBirth);
                            existedUser.YearOfBirth = StringHelper.GetDateOfBirthEmployee("Year", item.DateOfBirth);
                            existedUser.UpdatedDate = DateTime.Now;
                            existedUser.UpdatedUser = user.UserName;
                            existedUser.Address = item.Address;
                            existedUser.Note = item.Note;
                            existedUser.EmployeeType = userType != null ? userType.UserTypeId : (int)EmployeeType.Employee;
                            context.HR_User.Update(existedUser);
                        }
                        else
                        {
                            existedUser = new HR_User();
                            existedUser.CompanyIndex = user.CompanyIndex;
                            existedUser.EmployeeATID = item.EmployeeATID;
                            existedUser.EmployeeCode = item.EmployeeCode;
                            existedUser.FullName = item.FullName;
                            existedUser.Gender = (short)item.Gender;
                            existedUser.DayOfBirth = StringHelper.GetDateOfBirthEmployee("Day", item.DateOfBirth);
                            existedUser.MonthOfBirth = StringHelper.GetDateOfBirthEmployee("Month", item.DateOfBirth);
                            existedUser.YearOfBirth = StringHelper.GetDateOfBirthEmployee("Year", item.DateOfBirth);
                            existedUser.UpdatedDate = DateTime.Now;
                            existedUser.UpdatedUser = user.UserName;
                            existedUser.CreatedDate = DateTime.Now;
                            existedUser.Address = item.Address;
                            existedUser.Note = item.Note;
                            existedUser.EmployeeType = userType != null ? userType.UserTypeId : (int)EmployeeType.Employee;
                            context.HR_User.Add(existedUser);
                        }

                        var existedEmployee = listEmployeeDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedEmployee != null)
                        {
                            existedEmployee.Email = item.Email;
                            existedEmployee.Phone = item.PhoneNumber;
                            existedEmployee.JoinedDate = string.IsNullOrWhiteSpace(item.JoinedDate) ? DateTime.Now : DateTime.ParseExact(item.JoinedDate, "dd/MM/yyyy", null);
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            context.HR_EmployeeInfo.Update(existedEmployee);
                        }
                        else
                        {
                            existedEmployee = new HR_EmployeeInfo();
                            existedEmployee.CompanyIndex = user.CompanyIndex;
                            existedEmployee.EmployeeATID = item.EmployeeATID;
                            existedEmployee.Email = item.Email;
                            existedEmployee.Phone = item.PhoneNumber;
                            existedEmployee.JoinedDate = string.IsNullOrWhiteSpace(item.JoinedDate) ? DateTime.Now : DateTime.ParseExact(item.JoinedDate, "dd/MM/yyyy", null);
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            context.HR_EmployeeInfo.Add(existedEmployee);
                        }

                        var existedCardNumber = listCardNumberDB.FirstOrDefault(e => e.CardNumber == item.CardNumber);
                        if (existedCardNumber == null)
                        {
                            existedCardNumber = new HR_CardNumberInfo();
                            existedCardNumber.EmployeeATID = existedUser.EmployeeATID;
                            existedCardNumber.CompanyIndex = existedUser.CompanyIndex;
                            existedCardNumber.CardNumber = item.CardNumber;
                            existedCardNumber.IsActive = true;
                            existedCardNumber.CreatedDate = DateTime.Now;
                            existedCardNumber.UpdatedDate = existedUser.UpdatedDate;
                            existedCardNumber.UpdatedUser = existedUser.UpdatedUser;
                            context.HR_CardNumberInfo.Add(existedCardNumber);

                        }

                        //var position = listPositionDB.FirstOrDefault(e => e.Name == item.Position);
                        var existedWorkingInfo = listWorkingInfoDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedWorkingInfo == null)
                        {
                            existedWorkingInfo = new IC_WorkingInfo();
                            existedWorkingInfo.EmployeeATID = existedUser.EmployeeATID;
                            existedWorkingInfo.CompanyIndex = existedUser.CompanyIndex;
                            existedWorkingInfo.DepartmentIndex = department.Index;
                            //existedWorkingInfo.PositionIndex = position != null ? position.Index : 0;
                            existedWorkingInfo.FromDate = DateTime.Now;
                            existedWorkingInfo.IsManager = false;
                            existedWorkingInfo.ApprovedDate = DateTime.Now;
                            existedWorkingInfo.UpdatedUser = user.UserName;
                            existedWorkingInfo.Status = (short)TransferStatus.Approve;
                            context.IC_WorkingInfo.Add(existedWorkingInfo);
                        }
                        else if (existedWorkingInfo.DepartmentIndex == 0 && existedWorkingInfo.DepartmentIndex != department.Index && department.Index != 0)
                        {
                            existedWorkingInfo.DepartmentIndex = department.Index;
                            //existedWorkingInfo.PositionIndex = position != null ? position.Index : 0;
                            existedWorkingInfo.IsSync = null;
                            existedWorkingInfo.Status = (short)TransferStatus.Approve;
                            existedWorkingInfo.FromDate = DateTime.Now;
                            existedWorkingInfo.ApprovedDate = DateTime.Now;
                            existedWorkingInfo.UpdatedUser = user.UserName;
                            existedWorkingInfo.UpdatedDate = DateTime.Now;
                            context.IC_WorkingInfo.Update(existedWorkingInfo);
                        }
                        else if (existedWorkingInfo.DepartmentIndex > 0 && existedWorkingInfo.FromDate.Date < DateTime.Now.Date && existedWorkingInfo.DepartmentIndex != department.Index && department.Index != 0)
                        {
                            var workingInfo = new IC_WorkingInfo();
                            workingInfo.EmployeeATID = existedUser.EmployeeATID;
                            workingInfo.CompanyIndex = existedUser.CompanyIndex;
                            workingInfo.DepartmentIndex = department.Index;
                            //workingInfo.PositionIndex = position != null ? position.Index : 0;
                            workingInfo.FromDate = DateTime.Now;
                            workingInfo.IsManager = false;
                            workingInfo.ApprovedDate = DateTime.Now;
                            workingInfo.UpdatedUser = user.UserName;
                            workingInfo.Status = (short)TransferStatus.Approve;
                            context.IC_WorkingInfo.Add(workingInfo);
                            //
                            existedWorkingInfo.ToDate = DateTime.Now.AddDays(-1);
                            //existedWorkingInfo.PositionIndex = position != null ? position.Index : 0;
                            existedWorkingInfo.ApprovedDate = DateTime.Now;
                            existedWorkingInfo.UpdatedUser = user.UserName;
                            existedWorkingInfo.UpdatedDate = DateTime.Now;
                            context.IC_WorkingInfo.Update(existedWorkingInfo);
                        }

                        var existedUserMaster = listUserMasterDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedUserMaster == null)
                        {
                            existedUserMaster = new IC_UserMaster();
                            existedUserMaster.EmployeeATID = existedUser.EmployeeATID;
                            existedUserMaster.CompanyIndex = existedUser.CompanyIndex;
                            existedUserMaster.NameOnMachine = item.NameOnMachine;
                            existedUserMaster.CardNumber = item.CardNumber;
                            existedUserMaster.Password = item.Password;
                            existedUserMaster.AuthenMode = AuthenMode.FullAccessRight.ToString();
                            existedUserMaster.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.CreatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            context.IC_UserMaster.Add(existedUserMaster);
                        }
                        else
                        {
                            existedUserMaster.NameOnMachine = item.NameOnMachine;
                            existedUserMaster.CardNumber = item.CardNumber;
                            existedUserMaster.Password = item.Password;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            context.IC_UserMaster.Update(existedUserMaster);
                        }
                        //Add tabll HR_UserContactInfo
                        var existedUserContactInfo = listUserContactInfo.Where(e => e.UserIndex == item.EmployeeATID).ToList();
                        var listInfo = new List<HR_UserContactInfo>();
                        if (!string.IsNullOrEmpty(item.ParentName1) || !string.IsNullOrEmpty(item.ParentEmail1) || !string.IsNullOrEmpty(item.ParentPhone1))
                        {
                            var info = new HR_UserContactInfo();
                            info.UserIndex = item.EmployeeATID;
                            info.Name = item.ParentName1;
                            info.Phone = item.ParentPhone1;
                            info.Email = item.ParentEmail1;
                            info.UpdatedUser = user.UserName;
                            info.UpdatedDate = DateTime.Now;
                            info.CompanyIndex = user.CompanyIndex;
                            listInfo.Add(info);
                        }

                        if (!string.IsNullOrEmpty(item.ParentName2) || !string.IsNullOrEmpty(item.ParentEmail2) || !string.IsNullOrEmpty(item.ParentPhone2))
                        {
                            var info = new HR_UserContactInfo();
                            info.UserIndex = item.EmployeeATID;
                            info.Name = item.ParentName2;
                            info.Phone = item.ParentPhone2;
                            info.Email = item.ParentEmail2;
                            info.UpdatedUser = user.UserName;
                            info.UpdatedDate = DateTime.Now;
                            info.CompanyIndex = user.CompanyIndex;
                            listInfo.Add(info);
                        }
                        if (existedUserContactInfo.Count() == 0)
                        {
                            context.HR_UserContactInfo.AddRange(listInfo);
                        }
                        else
                        {
                            context.HR_UserContactInfo.RemoveRange(existedUserContactInfo);
                            context.SaveChanges();
                            context.HR_UserContactInfo.AddRange(listInfo);
                        }

                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError($"AddEmployeeFromExcel: {ex}");
                        return BadRequest(ex.Message);
                    }
                }

                context.SaveChanges();
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "HR_User";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + listEmployeeID.Count().ToString() + " Employee";
                audit.Description = AuditType.Added.ToString() + "EmployeeFromExcel:/:" + listEmployeeID.Count().ToString();
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);

                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER });
                var systemconfigs = await _iC_ConfigLogic.GetMany(addedParams);
                if (systemconfigs != null)
                {
                    var sysconfig = systemconfigs.FirstOrDefault();
                    if (sysconfig != null)
                    {
                        if (sysconfig.IntegrateLogParam.AutoIntegrate)
                        {
                            await _IIC_CommandLogic.SyncWithEmployee(listEmployeeID, user.CompanyIndex);
                        }
                    }
                }
            }
            else if (_configClientName.ToUpper() == ClientName.VSTAR.ToString())
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/EmployeesImportError_VStar.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/EmployeesImportError_VStar.xlsx"));
                if (listError != null && listError.Count() > 0)
                {
                    var listEmployeeIDError = listError.Select(e => e.EmployeeATID).ToList();
                    param = param.Where(e => !listEmployeeIDError.Contains(e.EmployeeATID)).ToList();
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("EmployeeError");
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                        worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                        worksheet.Cell(currentRow, 3).Value = "Họ tên";
                        worksheet.Cell(currentRow, 4).Value = "Mã thẻ";
                        worksheet.Cell(currentRow, 5).Value = "Mật khẩu";
                        worksheet.Cell(currentRow, 6).Value = "Tên trên máy";
                        worksheet.Cell(currentRow, 7).Value = "Giới tính (Nam)";
                        worksheet.Cell(currentRow, 8).Value = "Phòng ban";
                        worksheet.Cell(currentRow, 9).Value = "Tổ";
                        worksheet.Cell(currentRow, 10).Value = "Chức vụ";
                        worksheet.Cell(currentRow, 11).Value = "Ngày vào (ngày/tháng/năm)";
                        worksheet.Cell(currentRow, 12).Value = "Ngày sinh (ngày/tháng/năm)";
                        worksheet.Cell(currentRow, 13).Value = "Email";
                        worksheet.Cell(currentRow, 14).Value = "Số DT";
                        worksheet.Cell(currentRow, 15).Value = "Lỗi";

                        for (int i = 1; i < 16; i++)
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
                            worksheet.Cell(currentRow, 5).Value = users.CardNumber;

                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            if (!string.IsNullOrWhiteSpace(users.Password))
                                worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "0".PadLeft(users.Password.Length, '0');

                            worksheet.Cell(currentRow, 6).Value = users.NameOnMachine;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = users.Gender;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = users.DepartmentName;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 9).Value = users.TeamName;
                            worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 10).Value = users.Position;
                            worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 11).Value = users.JoinedDate;
                            worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 12).Value = users.DateOfBirth;
                            worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 13).Value = users.Email;
                            worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 14).Value = users.PhoneNumber;
                            worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 15).Value = users.ErrorMessage;
                            worksheet.Cell(currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        workbook.SaveAs(file.FullName);
                    }

                }

                var listDepartment = param.GroupBy(e => e.DepartmentName).Select(e => e.First()).ToList();
                List<IC_DepartmentDTO> listDepartmentCreate = listDepartment.Where(e => !string.IsNullOrEmpty(e.DepartmentName)).Select(e => new IC_DepartmentDTO
                {
                    Name = e.DepartmentName.Trim(),
                    CompanyIndex = user.CompanyIndex,
                    ParentIndex = 0,
                    UpdatedDate = DateTime.Now,
                    UpdatedUser = user.FullName,
                    CreatedDate = DateTime.Now
                }).ToList();
                listDepartmentCreate = _iC_DepartmentLogic.CheckExistedOrCreateList(listDepartmentCreate);

                var listTeam = param.GroupBy(e => e.DepartmentName).Select(e => e.First()).ToList();
                List<HR_TeamInfo> listTeamCreate = listTeam.Where(e => !string.IsNullOrEmpty(e.TeamName)).Select(e => new HR_TeamInfo
                {
                    Name = e.TeamName.Trim(),
                    CompanyIndex = user.CompanyIndex
                }).ToList();
                listTeamCreate = _iC_DepartmentLogic.CheckExistedOrCreateList(listTeamCreate);

                var listPosition = param.GroupBy(e => e.Position).Select(e => e.First()).Select(e => e.Position).ToList();
                var listPositionDB = _IHR_PositionInfoService.GetAll(e => listPosition.Contains(e.Name)).ToList();

                var listEmployeeID = param.Select(e => e.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0')).ToList();

                var listHRUserDB = new List<HR_User>();
                var listEmployeeDB = new List<HR_EmployeeInfo>();
                var listWorkingInfoDB = new List<IC_WorkingInfo>();
                var listUserMasterDB = new List<IC_UserMaster>();
                var listCardNumberDB = new List<HR_CardNumberInfo>();


                if (listEmployeeID.Count > 5000)
                {
                    var listSplitEmployeeID = CommonUtils.SplitList(listEmployeeID, 5000);
                    //var a = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listSplitEm.Contains(e.EmployeeATID)) && e.EmployeeType == (int)EmployeeType.Employee).ToList();
                    var listHRUser = context.HR_User.Where(x => listEmployeeID.Contains(x.EmployeeATID)).ToList();
                    var listEmployeeInfo = context.HR_EmployeeInfo.Where(x => listEmployeeID.Contains(x.EmployeeATID)).ToList();
                    var listWorkingInfo = context.IC_WorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)
                          && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && e.ToDate == null).OrderByDescending(e => e.FromDate).ToList();
                    var listUserMaster = context.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    var resultCardNumber = context.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();

                    foreach (var listEmployeeSplit in listSplitEmployeeID)
                    {
                        var resultHRUser = listHRUser.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultEmployee = listEmployeeInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultWorkingInfo = listWorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)
                           && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && e.ToDate == null).OrderByDescending(e => e.FromDate).ToList();
                        var resultUserMaster = listUserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();

                        listHRUserDB.AddRange(resultHRUser);
                        listEmployeeDB.AddRange(resultEmployee);
                        listWorkingInfoDB.AddRange(resultWorkingInfo);
                        listUserMasterDB.AddRange(resultUserMaster);
                        listCardNumberDB.AddRange(resultCardNumber);
                    }
                }
                else
                {
                    listHRUserDB = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listEmployeeDB = context.HR_EmployeeInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listWorkingInfoDB = context.IC_WorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)
                       && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && e.ToDate == null).OrderByDescending(e => e.FromDate).ToList();
                    listUserMasterDB = context.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listCardNumberDB = context.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();
                }

                //List<HR_User> listUser = new List<HR_User>();
                //List<HR_EmployeeInfo> listEmployee = new List<HR_EmployeeInfo>();
                //List<IC_WorkingInfoDTO> listWorkingInfo = new List<IC_WorkingInfoDTO>();
                //List<IC_UserMasterDTO> listUserMaster = new List<IC_UserMasterDTO>();


                foreach (IC_EmployeeImportDTO item in param)
                {
                    try
                    {
                        var department = new IC_DepartmentDTO();
                        department.Index = 0;
                        if (listDepartmentCreate != null)
                        {
                            department = listDepartmentCreate.FirstOrDefault(e => !string.IsNullOrWhiteSpace(item.DepartmentName) && e.Name.ToLower() == item.DepartmentName.ToLower());
                            if (department == null)
                            {
                                department = new IC_DepartmentDTO();
                                department.Index = 0;
                            }
                        }

                        var team = new HR_TeamInfo();
                        team.Index = 0;
                        if (listTeamCreate != null)
                        {
                            team = listTeamCreate.FirstOrDefault(e => !string.IsNullOrWhiteSpace(item.TeamName) && e.Name.ToLower() == item.TeamName.ToLower());
                            if (team == null)
                            {
                                team.Index = 0;
                            }
                        }

                        item.EmployeeATID = item.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');

                        var existedUser = listHRUserDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedUser != null)
                        {
                            existedUser.EmployeeCode = item.EmployeeCode;
                            existedUser.FullName = item.FullName;
                            existedUser.Gender = (short)item.Gender;
                            existedUser.DayOfBirth = StringHelper.GetDateOfBirthEmployee("Day", item.DateOfBirth);
                            existedUser.MonthOfBirth = StringHelper.GetDateOfBirthEmployee("Month", item.DateOfBirth);
                            existedUser.YearOfBirth = StringHelper.GetDateOfBirthEmployee("Year", item.DateOfBirth);
                            existedUser.UpdatedDate = DateTime.Now;
                            existedUser.UpdatedUser = user.UserName;
                            context.HR_User.Update(existedUser);
                        }
                        else
                        {
                            existedUser = new HR_User();
                            existedUser.CompanyIndex = user.CompanyIndex;
                            existedUser.EmployeeATID = item.EmployeeATID;
                            existedUser.EmployeeCode = item.EmployeeCode;
                            existedUser.FullName = item.FullName;
                            existedUser.Gender = (short)item.Gender;
                            existedUser.DayOfBirth = StringHelper.GetDateOfBirthEmployee("Day", item.DateOfBirth);
                            existedUser.MonthOfBirth = StringHelper.GetDateOfBirthEmployee("Month", item.DateOfBirth);
                            existedUser.YearOfBirth = StringHelper.GetDateOfBirthEmployee("Year", item.DateOfBirth);
                            existedUser.EmployeeType = (int)EmployeeType.Employee;
                            existedUser.UpdatedDate = DateTime.Now;
                            existedUser.UpdatedUser = user.UserName;
                            existedUser.CreatedDate = DateTime.Now;
                            context.HR_User.Add(existedUser);
                        }

                        var existedEmployee = listEmployeeDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedEmployee != null)
                        {
                            existedEmployee.Email = item.Email;
                            existedEmployee.Phone = item.PhoneNumber;
                            existedEmployee.JoinedDate = string.IsNullOrWhiteSpace(item.JoinedDate) ? DateTime.Now : DateTime.ParseExact(item.JoinedDate, "dd/MM/yyyy", null);
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            context.HR_EmployeeInfo.Update(existedEmployee);
                        }
                        else
                        {
                            existedEmployee = new HR_EmployeeInfo();
                            existedEmployee.CompanyIndex = user.CompanyIndex;
                            existedEmployee.EmployeeATID = item.EmployeeATID;
                            existedEmployee.Email = item.Email;
                            existedEmployee.Phone = item.PhoneNumber;
                            existedEmployee.JoinedDate = string.IsNullOrWhiteSpace(item.JoinedDate) ? DateTime.Now : DateTime.ParseExact(item.JoinedDate, "dd/MM/yyyy", null);
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            context.HR_EmployeeInfo.Add(existedEmployee);
                        }

                        var existedCardNumber = listCardNumberDB.FirstOrDefault(e => e.CardNumber == item.CardNumber);
                        if (existedCardNumber == null)
                        {
                            var listCard = listCardNumberDB.Where(x => x.EmployeeATID == existedUser.EmployeeATID);
                            foreach (var itemCard in listCard)
                            {
                                itemCard.IsActive = false;
                                itemCard.UpdatedDate = DateTime.Now;
                                context.HR_CardNumberInfo.Update(itemCard);
                            }
                            existedCardNumber = new HR_CardNumberInfo();
                            existedCardNumber.EmployeeATID = existedUser.EmployeeATID;
                            existedCardNumber.CompanyIndex = existedUser.CompanyIndex;
                            existedCardNumber.CardNumber = item.CardNumber;
                            existedCardNumber.IsActive = true;
                            existedCardNumber.CreatedDate = DateTime.Now;
                            existedCardNumber.UpdatedDate = existedUser.UpdatedDate;
                            existedCardNumber.UpdatedUser = existedUser.UpdatedUser;
                            context.HR_CardNumberInfo.Add(existedCardNumber);
                        }

                        var position = listPositionDB.FirstOrDefault(e => e.Name == item.Position);
                        var existedWorkingInfo = listWorkingInfoDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedWorkingInfo == null)
                        {
                            existedWorkingInfo = new IC_WorkingInfo();
                            existedWorkingInfo.EmployeeATID = existedUser.EmployeeATID;
                            existedWorkingInfo.CompanyIndex = existedUser.CompanyIndex;
                            existedWorkingInfo.DepartmentIndex = department.Index;
                            existedWorkingInfo.TeamIndex = team.Index;
                            existedWorkingInfo.PositionIndex = position != null ? position.Index : 0;
                            existedWorkingInfo.FromDate = DateTime.Now;
                            existedWorkingInfo.IsManager = false;
                            existedWorkingInfo.ApprovedDate = DateTime.Now;
                            existedWorkingInfo.UpdatedUser = user.UserName;
                            existedWorkingInfo.Status = (short)TransferStatus.Approve;
                            context.IC_WorkingInfo.Add(existedWorkingInfo);
                        }
                        else if (existedWorkingInfo.DepartmentIndex == 0 && existedWorkingInfo.DepartmentIndex != department.Index && department.Index != 0)
                        {
                            existedWorkingInfo.DepartmentIndex = department.Index;
                            existedWorkingInfo.TeamIndex = team.Index;
                            existedWorkingInfo.PositionIndex = position != null ? position.Index : 0;
                            existedWorkingInfo.IsSync = null;
                            existedWorkingInfo.Status = (short)TransferStatus.Approve;
                            existedWorkingInfo.FromDate = DateTime.Now;
                            existedWorkingInfo.ApprovedDate = DateTime.Now;
                            existedWorkingInfo.UpdatedUser = user.UserName;
                            existedWorkingInfo.UpdatedDate = DateTime.Now;
                            context.IC_WorkingInfo.Update(existedWorkingInfo);
                        }
                        else if (existedWorkingInfo.FromDate.Date < DateTime.Now.Date && ((existedWorkingInfo.DepartmentIndex > 0 && existedWorkingInfo.DepartmentIndex != department.Index && department.Index != 0)
                            || (existedWorkingInfo.TeamIndex != team.Index && team.Index != 0)))
                        {
                            var workingInfo = new IC_WorkingInfo();
                            workingInfo.EmployeeATID = existedUser.EmployeeATID;
                            workingInfo.CompanyIndex = existedUser.CompanyIndex;
                            workingInfo.DepartmentIndex = department.Index;
                            workingInfo.TeamIndex = team.Index;
                            workingInfo.PositionIndex = position != null ? position.Index : 0;
                            workingInfo.FromDate = DateTime.Now;
                            workingInfo.IsManager = false;
                            workingInfo.ApprovedDate = DateTime.Now;
                            workingInfo.UpdatedUser = user.UserName;
                            workingInfo.Status = (short)TransferStatus.Approve;
                            context.IC_WorkingInfo.Add(workingInfo);
                            //
                            existedWorkingInfo.ToDate = DateTime.Now.AddDays(-1);
                            existedWorkingInfo.PositionIndex = position != null ? position.Index : 0;
                            existedWorkingInfo.ApprovedDate = DateTime.Now;
                            existedWorkingInfo.UpdatedUser = user.UserName;
                            existedWorkingInfo.UpdatedDate = DateTime.Now;
                            context.IC_WorkingInfo.Update(existedWorkingInfo);
                        }

                        var existedUserMaster = listUserMasterDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedUserMaster == null)
                        {
                            existedUserMaster = new IC_UserMaster();
                            existedUserMaster.EmployeeATID = existedUser.EmployeeATID;
                            existedUserMaster.CompanyIndex = existedUser.CompanyIndex;
                            existedUserMaster.NameOnMachine = item.NameOnMachine;
                            existedUserMaster.CardNumber = item.CardNumber;
                            existedUserMaster.Password = item.Password;
                            existedUserMaster.AuthenMode = AuthenMode.FullAccessRight.ToString();
                            existedUserMaster.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.CreatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            context.IC_UserMaster.Add(existedUserMaster);
                        }
                        else
                        {
                            existedUserMaster.NameOnMachine = item.NameOnMachine;
                            existedUserMaster.CardNumber = item.CardNumber;
                            existedUserMaster.Password = item.Password;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            context.IC_UserMaster.Update(existedUserMaster);
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
                context.SaveChanges();
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "HR_User";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + listEmployeeID.Count().ToString() + " Employee";
                audit.Description = AuditType.Added.ToString() + "EmployeeFromExcel:/:" + listEmployeeID.Count().ToString();
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);

                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER });
                var systemconfigs = await _iC_ConfigLogic.GetMany(addedParams);
                if (systemconfigs != null)
                {
                    var sysconfig = systemconfigs.FirstOrDefault();
                    if (sysconfig != null)
                    {
                        if (sysconfig.IntegrateLogParam.AutoIntegrate)
                        {
                            await _IIC_CommandLogic.SyncWithEmployee(listEmployeeID, user.CompanyIndex);
                        }
                    }
                }
            }
            else if (_configClientName.ToUpper() == ClientName.MONDELEZ.ToString())
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/EmployeesImportError.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/EmployeesImportError.xlsx"));
                if (listError != null && listError.Count() > 0)
                {
                    var listEmployeeIDError = listError.Select(e => e.EmployeeATID).ToList();
                    param = param.Where(e => !listEmployeeIDError.Contains(e.EmployeeATID)).ToList();
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("EmployeeError");
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                        worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                        worksheet.Cell(currentRow, 3).Value = "Họ tên (*)";
                        worksheet.Cell(currentRow, 4).Value = "Tên trên máy";
                        worksheet.Cell(currentRow, 5).Value = "Giới tính";
                        worksheet.Cell(currentRow, 6).Value = "Ngày sinh (ngày/tháng/năm) (*)";
                        worksheet.Cell(currentRow, 7).Value = "Số điện thoại";
                        worksheet.Cell(currentRow, 8).Value = "CMND/CCCD/Passport (*)";
                        worksheet.Cell(currentRow, 9).Value = "Email";
                        worksheet.Cell(currentRow, 10).Value = "Địa chỉ";
                        worksheet.Cell(currentRow, 11).Value = "Phòng ban (*)";
                        worksheet.Cell(currentRow, 12).Value = "Chức vụ";
                        worksheet.Cell(currentRow, 13).Value = "Loại nhân viên";
                        worksheet.Cell(currentRow, 14).Value = "Sử dụng điện thoại";
                        worksheet.Cell(currentRow, 15).Value = "Ngày vào";
                        worksheet.Cell(currentRow, 16).Value = "Ngày nghỉ";
                        worksheet.Cell(currentRow, 17).Value = "Mật khẩu";
                        worksheet.Cell(currentRow, 18).Value = "Mã thẻ";
                        worksheet.Cell(currentRow, 19).Value = "Lỗi";

                        for (int i = 1; i < 20; i++)
                        {
                            worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Column(i).Width = 30;
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

                            worksheet.Cell(currentRow, 4).Value = users.NameOnMachine;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = users.Gender == 1 ? "Nam" : users.Gender == 0 ? "Nữ" : users.Gender == 2 ? "Khác" : "";
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = users.DateOfBirth;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = users.PhoneNumber;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = users.Nric;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 9).Value = users.Email;
                            worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 10).Value = users.Address;
                            worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 11).Value = users.DepartmentName;
                            worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 12).Value = users.Position;
                            worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 13).Value = users.EmployeeTypeName;
                            worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 14).Value = users.IsAllowPhone == 1 ? "x" : "";
                            worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 15).Value = users.JoinedDate;
                            worksheet.Cell(currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 16).Value = users.StoppedDate;
                            worksheet.Cell(currentRow, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                            worksheet.Cell(currentRow, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            if (!string.IsNullOrWhiteSpace(users.Password))
                                worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = "0".PadLeft(users.Password.Length, '0');

                            worksheet.Cell(currentRow, 18).Value = users.CardNumber;
                            worksheet.Cell(currentRow, 18).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            if (!string.IsNullOrWhiteSpace(users.CardNumber))
                                worksheet.Cell(currentRow, 18).Style.NumberFormat.Format = "0".PadLeft(users.CardNumber.Length, '0');
                            worksheet.Cell(currentRow, 18).Value = users.CardNumber;

                            worksheet.Cell(currentRow, 19).Value = users.ErrorMessage;
                            worksheet.Cell(currentRow, 19).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        workbook.SaveAs(file.FullName);
                    }

                }


                //var listDepartment = param.GroupBy(e => e.DepartmentName).Select(e => e.First()).ToList();
                //List<IC_DepartmentDTO> listCreate = listDepartment.Where(e => !string.IsNullOrEmpty(e.DepartmentName)).Select(e => new IC_DepartmentDTO
                //{
                //    Name = e.DepartmentName.Trim(),
                //    CompanyIndex = user.CompanyIndex,
                //    ParentIndex = 0,
                //    UpdatedDate = DateTime.Now,
                //    UpdatedUser = user.FullName,
                //    CreatedDate = DateTime.Now
                //}).ToList();

                var listDepartmentName = param.Select(x => x.DepartmentName.Trim()).ToHashSet();

                var listAllDepartmentNameFromImport = new List<string>();
                foreach (var departmentName in listDepartmentName)
                {
                    if (departmentName.Contains("/"))
                    {
                        var listSplitDepartmentName = departmentName.Split("/").Distinct().ToList();
                        listAllDepartmentNameFromImport.AddRange(listSplitDepartmentName);
                    }
                    else
                    {
                        listAllDepartmentNameFromImport.Add(departmentName);
                    }
                }
                listAllDepartmentNameFromImport = listAllDepartmentNameFromImport.Distinct().ToList();
                //var listDepartmentDB = _IIC_DepartmentService.GetDepartmentByListName(user.CompanyIndex, listAllDepartmentNameFromImport);
                var listDepartmentDB = _IIC_DepartmentService.GetAllActiveDepartment(user.CompanyIndex);

                var listSingleDepartmentName = listDepartmentName.Where(x => !x.Contains("/")).ToList();
                var listTreeDepartmentName = listDepartmentName.Where(x => x.Contains("/")).ToList();

                if (listTreeDepartmentName != null && listTreeDepartmentName.Count > 0)
                {
                    foreach (var treeDepartmentName in listTreeDepartmentName)
                    {
                        var listSplitDepartmentName = treeDepartmentName.Split("/").Distinct().ToList();
                        listSingleDepartmentName.Add(listSplitDepartmentName[0]);
                    }
                }

                var listDepartmentCreate = new List<IC_DepartmentDTO>();
                var listSingleDepartmentCreate = new List<IC_DepartmentDTO>();
                var listTreeDepartmentCreate = new List<IC_DepartmentDTO>();

                foreach (var departmentName in listSingleDepartmentName)
                {
                    var name = departmentName;
                    var nowTicks = DateTime.Now.Ticks.ToString();
                    var code = nowTicks;
                    var index = 0;

                    var existedDBSingleDepartment = listDepartmentDB.FirstOrDefault(x => x.Name == name
                        && ((x.ParentIndex > 0) ? !listDepartmentDB.Any(y => y.Index == x.ParentIndex) : true)
                        //&& !listDepartmentDB.Any(y => y.ParentIndex == x.Index)
                        );
                    if (existedDBSingleDepartment != null)
                    {
                        code = existedDBSingleDepartment.Code;
                        index = existedDBSingleDepartment.Index;
                    }
                    else if (listDepartmentCreate.Any(x => x.Name == name))
                    {
                        code = listDepartmentCreate.FirstOrDefault(x => x.Name == name)?.Code ?? nowTicks;
                    }

                    if (string.IsNullOrWhiteSpace(code))
                    {
                        code = nowTicks;
                    }

                    var departmentDTO = new IC_DepartmentDTO
                    {
                        Name = name,
                        Code = code,
                        CompanyIndex = user.CompanyIndex,
                        ParentIndex = 0,
                        ParentCode = string.Empty,
                        ParentName = string.Empty,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.FullName,
                        CreatedDate = DateTime.Now,
                        Index = index,
                    };

                    if (listDepartmentCreate.Any(x => x.Code == code))
                    {
                        continue;
                    }

                    listDepartmentCreate.Add(departmentDTO);
                }

                foreach (var departmentName in listTreeDepartmentName)
                {
                    var parentCode = string.Empty;
                    var parentIndex = 0;
                    var listSplitDepartmentName = departmentName.Split("/").Distinct().ToList();
                    for (var i = 0; i < listSplitDepartmentName.Count; i++)
                    {
                        var name = listSplitDepartmentName[i];
                        var nowTicks = DateTime.Now.Ticks.ToString();
                        var code = nowTicks;
                        var index = -1;

                        var departmentDTO = new IC_DepartmentDTO();
                        if (i == 0)
                        {
                            var listExistedDepartment = listDepartmentDB.Where(x => x.Name == name).ToList();
                            var existedDepartment = listExistedDepartment.FirstOrDefault(x
                                => ((x.ParentIndex > 0) ? !listDepartmentDB.Any(y => y.Index == x.ParentIndex) : true)
                                //&& listDepartmentDB.Any(y => y.ParentIndex == x.Index)
                                );
                            if (existedDepartment != null)
                            {
                                code = existedDepartment.Code;
                                index = existedDepartment.Index;
                                if (string.IsNullOrWhiteSpace(code)
                                    && listDepartmentCreate.Any(y => y.Name == name && string.IsNullOrWhiteSpace(y.ParentCode)))
                                {
                                    code = listDepartmentCreate.FirstOrDefault(y => y.Name == name)?.Code ?? nowTicks;
                                }
                            }
                            else if (listDepartmentCreate.Any(y => y.Name == name && string.IsNullOrWhiteSpace(y.ParentCode)))
                            {
                                code = listDepartmentCreate.FirstOrDefault(y => y.Name == name)?.Code ?? nowTicks;
                            }

                            departmentDTO = new IC_DepartmentDTO
                            {
                                Name = listSplitDepartmentName[i],
                                Code = code,
                                CompanyIndex = user.CompanyIndex,
                                ParentIndex = 0,
                                ParentCode = string.Empty,
                                ParentName = string.Empty,
                                UpdatedDate = DateTime.Now,
                                UpdatedUser = user.FullName,
                                CreatedDate = DateTime.Now
                            };
                        }
                        else
                        {
                            var parentName = listSplitDepartmentName[i - 1];
                            var existedParentDepartment = listDepartmentDB.FirstOrDefault(x => x.Code == parentCode
                                || x.Index == parentIndex);
                            if (existedParentDepartment != null)
                            {
                                var existedDepartmentWithParent = listDepartmentDB.FirstOrDefault(x => x.Name == name
                                    && x.Index != existedParentDepartment.Index
                                    && x.ParentIndex == existedParentDepartment.Index);
                                if (existedDepartmentWithParent != null)
                                {
                                    code = existedDepartmentWithParent.Code;
                                    index = existedDepartmentWithParent.Index;
                                    if (string.IsNullOrWhiteSpace(code)
                                    && listDepartmentCreate.Any(y => y.Name == name && y.ParentCode == parentCode))
                                    {
                                        code = listDepartmentCreate.FirstOrDefault(y => y.Name == name
                                            && y.ParentCode == parentCode)?.Code ?? nowTicks;
                                    }
                                }
                            }
                            else if (listDepartmentCreate.Any(y => y.Name == name && y.ParentCode == parentCode))
                            {
                                code = listDepartmentCreate.FirstOrDefault(y => y.Name == name
                                    && y.ParentCode == parentCode)?.Code ?? nowTicks;
                            }

                            departmentDTO = new IC_DepartmentDTO
                            {
                                Name = listSplitDepartmentName[i],
                                Code = code,
                                CompanyIndex = user.CompanyIndex,
                                ParentIndex = 0,
                                ParentCode = parentCode,
                                ParentName = listSplitDepartmentName[i - 1],
                                UpdatedDate = DateTime.Now,
                                UpdatedUser = user.FullName,
                                CreatedDate = DateTime.Now
                            };
                        }

                        if (string.IsNullOrWhiteSpace(code))
                        {
                            code = nowTicks;
                        }

                        parentCode = code;
                        departmentDTO.Code = code;
                        parentIndex = index;
                        departmentDTO.Index = index;

                        if (index > 0 && listDepartmentCreate.Any(x => x.Index == index))
                        {
                            continue;
                        }
                        if (listDepartmentCreate.Any(x => x.Code == code))
                        {
                            continue;
                        }

                        listDepartmentCreate.Add(departmentDTO);
                    }
                }

                //if (listTreeDepartmentCreate != null && listTreeDepartmentCreate.Count > 0)
                //{
                //    listDepartmentCreate.AddRange(listTreeDepartmentCreate);
                //}

                //if (listSingleDepartmentCreate != null && listSingleDepartmentCreate.Count > 0)
                //{
                //    listDepartmentCreate.AddRange(listSingleDepartmentCreate);
                //}

                //listCreate = _iC_DepartmentLogic.CheckExistedOrCreateList(listCreate);
                var listCreate = _iC_DepartmentLogic.CheckExistedListDepartmentFromImport(listDepartmentCreate, user);

                var listPosition = param.GroupBy(e => e.Position).Select(e => e.First()).Select(e => e.Position).ToList();
                var listPositionDB = await _IHR_PositionInfoService.GetAllPositionInfo(user.CompanyIndex);

                var listEmployeeATIDs = param.Select(e => e.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0')).ToList();

                var listHRUserDB = new List<HR_User>();
                var listEmployeeDB = new List<HR_EmployeeInfo>();
                var listWorkingInfoDB = new List<IC_WorkingInfo>();
                var listUserMasterDB = new List<IC_UserMaster>();
                var listCardNumberDB = new List<HR_CardNumberInfo>();


                if (listEmployeeATIDs.Count > 5000)
                {
                    var listSplitEmployeeID = CommonUtils.SplitList(listEmployeeATIDs, 5000);
                    //var a = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listSplitEm.Contains(e.EmployeeATID)) &&
                    //).ToList();
                    var listHRUser = context.HR_User.Where(x => listEmployeeATIDs.Contains(x.EmployeeATID)).ToList();
                    var listEmployeeInfo = context.HR_EmployeeInfo.Where(x => listEmployeeATIDs.Contains(x.EmployeeATID)).ToList();
                    var listWorkingInfo = context.IC_WorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeATIDs.Contains(e.EmployeeATID)
                          && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && e.ToDate == null).OrderByDescending(e => e.FromDate).ToList();
                    var listUserMaster = context.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeATIDs.Contains(e.EmployeeATID)).ToList();
                    var resultCardNumber = context.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();

                    foreach (var listEmployeeSplit in listSplitEmployeeID)
                    {
                        var resultHRUser = listHRUser.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultEmployee = listEmployeeInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultWorkingInfo = listWorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)
                           && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && e.ToDate == null).OrderByDescending(e => e.FromDate).ToList();
                        var resultUserMaster = listUserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();

                        listHRUserDB.AddRange(resultHRUser);
                        listEmployeeDB.AddRange(resultEmployee);
                        listWorkingInfoDB.AddRange(resultWorkingInfo);
                        listUserMasterDB.AddRange(resultUserMaster);
                        listCardNumberDB.AddRange(resultCardNumber);
                    }
                }
                else
                {
                    listHRUserDB = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeATIDs.Contains(e.EmployeeATID)).ToList();
                    listEmployeeDB = context.HR_EmployeeInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeATIDs.Contains(e.EmployeeATID)).ToList();

                    listWorkingInfoDB = context.IC_WorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeATIDs.Contains(e.EmployeeATID)
                       && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date).OrderByDescending(e => e.FromDate).ToList();

                    listUserMasterDB = context.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeATIDs.Contains(e.EmployeeATID)).ToList();
                    listCardNumberDB = context.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();
                }

                //List<HR_User> listUser = new List<HR_User>();
                //List<HR_EmployeeInfo> listEmployee = new List<HR_EmployeeInfo>();
                //List<IC_WorkingInfoDTO> listWorkingInfo = new List<IC_WorkingInfoDTO>();
                //List<IC_UserMasterDTO> listUserMaster = new List<IC_UserMasterDTO>();
                var paramList = param.ToList();

                var listEmployeeTypeName = paramList.Select(x => x.EmployeeTypeName).Distinct().ToList();
                var listEmployeeType = new List<IC_EmployeeType>();
                if (listEmployeeTypeName != null && listEmployeeTypeName.Count > 0)
                {
                    listEmployeeType = await _IIC_EmployeeTypeService.GetDataByListNameAndCompanyIndex(listEmployeeTypeName,
                        user.CompanyIndex);
                }
                foreach (var paramItem in paramList)
                {
                    try
                    {
                        var department = new IC_DepartmentDTO();
                        department.Index = 0;
                        var splitDepartmentNameList = new List<string>();
                        if (paramItem.DepartmentName.Contains("/"))
                        {
                            splitDepartmentNameList = paramItem.DepartmentName.Split("/").ToList();
                            paramItem.DepartmentName = splitDepartmentNameList[splitDepartmentNameList.Count - 1];

                            if (listCreate != null)
                            {
                                var countLevelDepartment = 0;
                                var nextLevelChildDepartmentIndex = new List<long>();
                                var itemDepartment = new IC_DepartmentDTO();
                                while (countLevelDepartment < splitDepartmentNameList.Count)
                                {
                                    var departmentDTOs = new List<IC_DepartmentDTO>();
                                    if (!nextLevelChildDepartmentIndex?.Any() ?? false)
                                    {
                                        departmentDTOs = listCreate.Where(x => x.Name == splitDepartmentNameList[countLevelDepartment]).ToList();
                                        if (departmentDTOs?.Any() ?? false)
                                        {
                                            var listChildDepartment = listCreate.Where(x => departmentDTOs.Any(y => y.Index == x.ParentIndex)).ToList();
                                            nextLevelChildDepartmentIndex = listChildDepartment.Select(x => x.Index).ToList();
                                        }
                                    }
                                    else
                                    {
                                        departmentDTOs = listCreate.Where(x => x.Name == splitDepartmentNameList[countLevelDepartment]
                                            && nextLevelChildDepartmentIndex.Contains(x.Index)).ToList();
                                        if (departmentDTOs?.Any() ?? false)
                                        {
                                            var listChildDepartment = listCreate.Where(x => departmentDTOs.Any(y => y.Index == x.ParentIndex)).ToList();
                                            nextLevelChildDepartmentIndex = listChildDepartment.Select(x => x.Index).ToList();
                                        }
                                    }

                                    if (countLevelDepartment == (splitDepartmentNameList.Count - 1))
                                    {
                                        itemDepartment = departmentDTOs.FirstOrDefault(x => x.Name == splitDepartmentNameList[countLevelDepartment]);
                                        break;
                                    }

                                    if (nextLevelChildDepartmentIndex?.Any() ?? false)
                                    {
                                        countLevelDepartment++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                if (itemDepartment != null)
                                {
                                    department = itemDepartment;
                                }
                                if (department != null && itemDepartment == null)
                                {
                                    department.Index = 0;
                                }
                            }
                        }
                        else
                        {
                            department = listCreate.FirstOrDefault(e => !string.IsNullOrWhiteSpace(paramItem.DepartmentName)
                                && e.Name == paramItem.DepartmentName.Trim()
                                //&& (!e.ParentIndex.HasValue
                                //|| (e.ParentIndex.HasValue && e.ParentIndex.Value == 0))
                                //&& !listCreate.Any(y => y.ParentIndex == e.Index)
                                );
                        }

                        paramItem.EmployeeATID = paramItem.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');

                        var existedUser = listHRUserDB.FirstOrDefault(e => e.EmployeeATID == paramItem.EmployeeATID);
                        if (existedUser != null)
                        {
                            existedUser.EmployeeCode = paramItem.EmployeeCode;
                            existedUser.FullName = paramItem.FullName;
                            existedUser.Gender = (short)paramItem.Gender;
                            if (!string.IsNullOrWhiteSpace(paramItem.EmployeeTypeName))
                            {
                                var employeeType = listEmployeeType.FirstOrDefault(x => x.IsUsing && x.Name == paramItem.EmployeeTypeName);
                                existedUser.EmployeeTypeIndex = employeeType?.Index ?? null;
                            }
                            else
                            {
                                existedUser.EmployeeTypeIndex = null;
                            }
                            existedUser.DayOfBirth = StringHelper.GetDateOfBirthEmployee("Day", paramItem.DateOfBirth);
                            existedUser.MonthOfBirth = StringHelper.GetDateOfBirthEmployee("Month", paramItem.DateOfBirth);
                            existedUser.YearOfBirth = StringHelper.GetDateOfBirthEmployee("Year", paramItem.DateOfBirth);
                            existedUser.UpdatedDate = DateTime.Now;
                            existedUser.UpdatedUser = user.UserName;
                            existedUser.Note = paramItem.Note;
                            existedUser.Nric = paramItem.Nric;
                            existedUser.IsAllowPhone = paramItem.IsAllowPhone == 1 ? true : false;
                            existedUser.Address = paramItem.Address;
                            context.HR_User.Update(existedUser);
                        }
                        else
                        {
                            existedUser = new HR_User();
                            existedUser.CompanyIndex = user.CompanyIndex;
                            existedUser.EmployeeATID = paramItem.EmployeeATID;
                            existedUser.EmployeeCode = paramItem.EmployeeCode;
                            if (!string.IsNullOrWhiteSpace(paramItem.EmployeeTypeName))
                            {
                                var employeeType = listEmployeeType.FirstOrDefault(x => x.IsUsing && x.Name == paramItem.EmployeeTypeName);
                                existedUser.EmployeeTypeIndex = employeeType?.Index ?? null;
                            }
                            existedUser.FullName = paramItem.FullName;
                            existedUser.Gender = (short)paramItem.Gender;
                            existedUser.DayOfBirth = StringHelper.GetDateOfBirthEmployee("Day", paramItem.DateOfBirth);
                            existedUser.MonthOfBirth = StringHelper.GetDateOfBirthEmployee("Month", paramItem.DateOfBirth);
                            existedUser.YearOfBirth = StringHelper.GetDateOfBirthEmployee("Year", paramItem.DateOfBirth);
                            existedUser.EmployeeType = (int)EmployeeType.Employee;
                            existedUser.UpdatedDate = DateTime.Now;
                            existedUser.UpdatedUser = user.UserName;
                            existedUser.CreatedDate = DateTime.Now;
                            existedUser.Note = paramItem.Note;
                            existedUser.IsAllowPhone = paramItem.IsAllowPhone == 1 ? true : false;
                            existedUser.Nric = paramItem.Nric;
                            existedUser.Address = paramItem.Address;
                            context.HR_User.Add(existedUser);
                        }

                        var existedEmployee = listEmployeeDB.FirstOrDefault(e => e.EmployeeATID == paramItem.EmployeeATID);
                        if (existedEmployee != null)
                        {
                            existedEmployee.Email = paramItem.Email;
                            existedEmployee.Phone = paramItem.PhoneNumber;
                            existedEmployee.JoinedDate = string.IsNullOrWhiteSpace(paramItem.JoinedDate) ? DateTime.Now : DateTime.ParseExact(paramItem.JoinedDate, "dd/MM/yyyy", null);
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            context.HR_EmployeeInfo.Update(existedEmployee);
                        }
                        else
                        {
                            existedEmployee = new HR_EmployeeInfo();
                            existedEmployee.CompanyIndex = user.CompanyIndex;
                            existedEmployee.EmployeeATID = paramItem.EmployeeATID;
                            existedEmployee.Email = paramItem.Email;
                            existedEmployee.Phone = paramItem.PhoneNumber;
                            existedEmployee.JoinedDate = string.IsNullOrWhiteSpace(paramItem.JoinedDate) ? DateTime.Now : DateTime.ParseExact(paramItem.JoinedDate, "dd/MM/yyyy", null);
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            context.HR_EmployeeInfo.Add(existedEmployee);
                        }

                        var existedCardNumber = listCardNumberDB.FirstOrDefault(e => e.CardNumber == paramItem.CardNumber);
                        if (existedCardNumber == null)
                        {

                            var listCard = listCardNumberDB.Where(x => x.EmployeeATID == existedUser.EmployeeATID);
                            foreach (var itemCard in listCard)
                            {
                                itemCard.IsActive = false;
                                itemCard.UpdatedDate = DateTime.Now;
                                context.HR_CardNumberInfo.Update(itemCard);
                            }
                            existedCardNumber = new HR_CardNumberInfo();
                            existedCardNumber.EmployeeATID = existedUser.EmployeeATID;
                            existedCardNumber.CompanyIndex = existedUser.CompanyIndex;
                            existedCardNumber.CardNumber = paramItem.CardNumber;
                            existedCardNumber.IsActive = true;
                            existedCardNumber.CreatedDate = DateTime.Now;
                            existedCardNumber.UpdatedDate = existedUser.UpdatedDate;
                            existedCardNumber.UpdatedUser = existedUser.UpdatedUser;
                            context.HR_CardNumberInfo.Add(existedCardNumber);
                        }

                        var workingInfoFromDate = DateTime.Now;
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(paramItem.JoinedDate))
                            {
                                workingInfoFromDate = DateTime.ParseExact(paramItem.JoinedDate, "dd/MM/yyyy",
                                    System.Globalization.CultureInfo.InvariantCulture);
                            }
                        }
                        catch (Exception)
                        {
                            workingInfoFromDate = DateTime.Now;
                        }


                        var position = listPositionDB.FirstOrDefault(e => e.Name.ToLower().RemoveAccents() == paramItem.Position.ToLower().RemoveAccents());
                        var existedWorkingInfo = listWorkingInfoDB.FirstOrDefault(e => e.EmployeeATID == paramItem.EmployeeATID);
                        if (existedWorkingInfo == null)
                        {
                            existedWorkingInfo = new IC_WorkingInfo();
                            existedWorkingInfo.EmployeeATID = existedUser.EmployeeATID;
                            existedWorkingInfo.CompanyIndex = existedUser.CompanyIndex;
                            existedWorkingInfo.DepartmentIndex = department.Index;
                            existedWorkingInfo.PositionIndex = position != null ? position.Index : 0;
                            existedWorkingInfo.FromDate = workingInfoFromDate;
                            existedWorkingInfo.ToDate = !string.IsNullOrWhiteSpace(paramItem.StoppedDate) ? DateTime.ParseExact(paramItem.StoppedDate, "dd/MM/yyyy",
                                System.Globalization.CultureInfo.InvariantCulture) : null;
                            existedWorkingInfo.IsManager = false;
                            existedWorkingInfo.ApprovedDate = DateTime.Now;
                            existedWorkingInfo.UpdatedUser = user.UserName;
                            existedWorkingInfo.Status = (short)TransferStatus.Approve;
                            context.IC_WorkingInfo.Add(existedWorkingInfo);
                        }
                        else if (existedWorkingInfo.DepartmentIndex == 0 && existedWorkingInfo.DepartmentIndex != department.Index
                            && department.Index != 0)
                        {
                            existedWorkingInfo.DepartmentIndex = department.Index;
                            existedWorkingInfo.PositionIndex = position != null ? position.Index : 0;
                            existedWorkingInfo.IsSync = null;
                            existedWorkingInfo.Status = (short)TransferStatus.Approve;
                            existedWorkingInfo.FromDate = workingInfoFromDate;
                            existedWorkingInfo.ToDate = !string.IsNullOrWhiteSpace(paramItem.StoppedDate) ? DateTime.ParseExact(paramItem.StoppedDate, "dd/MM/yyyy",
                                System.Globalization.CultureInfo.InvariantCulture) : null;
                            existedWorkingInfo.ApprovedDate = DateTime.Now;
                            existedWorkingInfo.UpdatedUser = user.UserName;
                            existedWorkingInfo.UpdatedDate = DateTime.Now;
                            context.IC_WorkingInfo.Update(existedWorkingInfo);
                        }
                        //else if (existedWorkingInfo.DepartmentIndex > 0 && existedWorkingInfo.FromDate.Date <= DateTime.Now.Date
                        //    && existedWorkingInfo.DepartmentIndex != department.Index && department.Index != 0)
                        //{
                        //    var workingInfo = new IC_WorkingInfo();
                        //    workingInfo.EmployeeATID = existedUser.EmployeeATID;
                        //    workingInfo.CompanyIndex = existedUser.CompanyIndex;
                        //    workingInfo.DepartmentIndex = department.Index;
                        //    workingInfo.PositionIndex = position != null ? position.Index : 0;
                        //    workingInfo.FromDate = workingInfoFromDate;
                        //    workingInfo.IsManager = false;
                        //    workingInfo.ApprovedDate = DateTime.Now;
                        //    workingInfo.UpdatedUser = user.UserName;
                        //    workingInfo.Status = (short)TransferStatus.Approve;
                        //    context.IC_WorkingInfo.Add(workingInfo);
                        //    //
                        //    existedWorkingInfo.ToDate = DateTime.Now.AddDays(-1);
                        //    existedWorkingInfo.PositionIndex = position != null ? position.Index : 0;
                        //    existedWorkingInfo.ApprovedDate = DateTime.Now;
                        //    existedWorkingInfo.UpdatedUser = user.UserName;
                        //    existedWorkingInfo.UpdatedDate = DateTime.Now;
                        //    context.IC_WorkingInfo.Update(existedWorkingInfo);
                        //}
                        else if (existedWorkingInfo.DepartmentIndex > 0 && existedWorkingInfo.DepartmentIndex == department.Index && department.Index != 0)
                        {
                            existedWorkingInfo.EmployeeATID = existedUser.EmployeeATID;
                            existedWorkingInfo.CompanyIndex = existedUser.CompanyIndex;
                            existedWorkingInfo.ToDate = !string.IsNullOrWhiteSpace(paramItem.StoppedDate) ? DateTime.ParseExact(paramItem.StoppedDate, "dd/MM/yyyy",
                                System.Globalization.CultureInfo.InvariantCulture) : null;
                            existedWorkingInfo.ApprovedDate = DateTime.Now;
                            existedWorkingInfo.UpdatedUser = user.UserName;
                            existedWorkingInfo.Status = (short)TransferStatus.Approve;
                            context.IC_WorkingInfo.Update(existedWorkingInfo);
                        }

                        var existedUserMaster = listUserMasterDB.FirstOrDefault(e => e.EmployeeATID == paramItem.EmployeeATID);
                        if (existedUserMaster == null)
                        {
                            existedUserMaster = new IC_UserMaster();
                            existedUserMaster.EmployeeATID = existedUser.EmployeeATID;
                            existedUserMaster.CompanyIndex = existedUser.CompanyIndex;
                            existedUserMaster.NameOnMachine = paramItem.NameOnMachine;
                            existedUserMaster.CardNumber = paramItem.CardNumber;
                            existedUserMaster.Password = paramItem.Password;
                            existedUserMaster.AuthenMode = AuthenMode.FullAccessRight.ToString();
                            existedUserMaster.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.CreatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            context.IC_UserMaster.Add(existedUserMaster);
                        }
                        else
                        {
                            existedUserMaster.NameOnMachine = paramItem.NameOnMachine;
                            existedUserMaster.CardNumber = paramItem.CardNumber;
                            existedUserMaster.Password = paramItem.Password;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            context.IC_UserMaster.Update(existedUserMaster);
                        }
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError($"AddEmployeeFromExcel: {ex}");
                        return BadRequest(ex.Message);
                    }
                }
                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _Logger.LogError($"AddEmployeeFromExcel: {ex}");
                    return BadRequest(ex.Message);
                }
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "HR_User";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + listEmployeeATIDs.Count().ToString() + " Employee";
                audit.Description = AuditType.Added.ToString() + "EmployeeFromExcel:/:" + listEmployeeATIDs.Count().ToString();
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);

                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER });
                var systemconfigs = await _iC_ConfigLogic.GetMany(addedParams);
                if (systemconfigs != null)
                {
                    var sysconfig = systemconfigs.FirstOrDefault();
                    if (sysconfig != null)
                    {
                        if (sysconfig.IntegrateLogParam.AutoIntegrate)
                        {
                            await _IIC_CommandLogic.SyncWithEmployee(listEmployeeATIDs, user.CompanyIndex);
                        }
                    }
                }
            }
            else
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/EmployeesImportError.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/EmployeesImportError.xlsx"));
                if (listError != null && listError.Count() > 0)
                {
                    var listEmployeeIDError = listError.Select(e => e.EmployeeATID).ToList();
                    param = param.Where(e => !listEmployeeIDError.Contains(e.EmployeeATID)).ToList();
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("EmployeeError");
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                        worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                        worksheet.Cell(currentRow, 3).Value = "Họ tên (*)";
                        worksheet.Cell(currentRow, 4).Value = "Tên trên máy";
                        worksheet.Cell(currentRow, 5).Value = "Giới tính";
                        worksheet.Cell(currentRow, 6).Value = "Ngày sinh (ngày/tháng/năm)";
                        worksheet.Cell(currentRow, 7).Value = "Số điện thoại";
                        worksheet.Cell(currentRow, 8).Value = "CMND/CCCD/Passport";
                        worksheet.Cell(currentRow, 9).Value = "Email";
                        worksheet.Cell(currentRow, 10).Value = "Địa chỉ";
                        worksheet.Cell(currentRow, 11).Value = "Phòng ban (*)";
                        worksheet.Cell(currentRow, 12).Value = "Chức vụ";
                        worksheet.Cell(currentRow, 13).Value = "Loại nhân viên";
                        worksheet.Cell(currentRow, 14).Value = "Sử dụng điện thoại";
                        worksheet.Cell(currentRow, 15).Value = "Ngày vào";
                        worksheet.Cell(currentRow, 16).Value = "Ngày nghỉ";
                        worksheet.Cell(currentRow, 17).Value = "Mật khẩu";
                        worksheet.Cell(currentRow, 18).Value = "Mã thẻ";
                        worksheet.Cell(currentRow, 19).Value = "Lỗi";

                        for (int i = 1; i < 20; i++)
                        {
                            worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Column(i).Width = 30;
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

                            worksheet.Cell(currentRow, 4).Value = users.NameOnMachine;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = users.Gender == 1 ? "Nam" : users.Gender == 0 ? "Nữ" : users.Gender == 2 ? "Khác" : "";
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = users.DateOfBirth;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = users.PhoneNumber;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = users.Nric;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 9).Value = users.Email;
                            worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 10).Value = users.Address;
                            worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 11).Value = users.DepartmentName;
                            worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 12).Value = users.Position;
                            worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 13).Value = users.EmployeeTypeName;
                            worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 14).Value = users.IsAllowPhone == 1 ? "x" : "";
                            worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 15).Value = users.JoinedDate;
                            worksheet.Cell(currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 16).Value = users.StoppedDate;
                            worksheet.Cell(currentRow, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                            worksheet.Cell(currentRow, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            if (!string.IsNullOrWhiteSpace(users.Password))
                                worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = "0".PadLeft(users.Password.Length, '0');

                            worksheet.Cell(currentRow, 18).Value = users.CardNumber;
                            worksheet.Cell(currentRow, 18).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            if (!string.IsNullOrWhiteSpace(users.CardNumber))
                                worksheet.Cell(currentRow, 18).Style.NumberFormat.Format = "0".PadLeft(users.CardNumber.Length, '0');
                            worksheet.Cell(currentRow, 18).Value = users.CardNumber;

                            worksheet.Cell(currentRow, 19).Value = users.ErrorMessage;
                            worksheet.Cell(currentRow, 19).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        workbook.SaveAs(file.FullName);
                    }

                }


                //var listDepartment = param.GroupBy(e => e.DepartmentName).Select(e => e.First()).ToList();
                //List<IC_DepartmentDTO> listCreate = listDepartment.Where(e => !string.IsNullOrEmpty(e.DepartmentName)).Select(e => new IC_DepartmentDTO
                //{
                //    Name = e.DepartmentName.Trim(),
                //    CompanyIndex = user.CompanyIndex,
                //    ParentIndex = 0,
                //    UpdatedDate = DateTime.Now,
                //    UpdatedUser = user.FullName,
                //    CreatedDate = DateTime.Now
                //}).ToList();

                var listDepartmentName = param.Select(x => x.DepartmentName.Trim()).ToHashSet();

                var listAllDepartmentNameFromImport = new List<string>();
                foreach (var departmentName in listDepartmentName)
                {
                    if (departmentName.Contains("/"))
                    {
                        var listSplitDepartmentName = departmentName.Split("/").Distinct().ToList();
                        listAllDepartmentNameFromImport.AddRange(listSplitDepartmentName);
                    }
                    else
                    {
                        listAllDepartmentNameFromImport.Add(departmentName);
                    }
                }
                listAllDepartmentNameFromImport = listAllDepartmentNameFromImport.Distinct().ToList();
                //var listDepartmentDB = _IIC_DepartmentService.GetDepartmentByListName(user.CompanyIndex, listAllDepartmentNameFromImport);
                var listDepartmentDB = _IIC_DepartmentService.GetAllActiveDepartment(user.CompanyIndex);

                var listSingleDepartmentName = listDepartmentName.Where(x => !x.Contains("/")).ToList();
                var listTreeDepartmentName = listDepartmentName.Where(x => x.Contains("/")).ToList();

                if (listTreeDepartmentName != null && listTreeDepartmentName.Count > 0)
                {
                    foreach (var treeDepartmentName in listTreeDepartmentName)
                    {
                        var listSplitDepartmentName = treeDepartmentName.Split("/").Distinct().ToList();
                        listSingleDepartmentName.Add(listSplitDepartmentName[0]);
                    }
                }

                var listDepartmentCreate = new List<IC_DepartmentDTO>();
                var listSingleDepartmentCreate = new List<IC_DepartmentDTO>();
                var listTreeDepartmentCreate = new List<IC_DepartmentDTO>();

                foreach (var departmentName in listSingleDepartmentName)
                {
                    var name = departmentName;
                    var nowTicks = DateTime.Now.Ticks.ToString();
                    var code = nowTicks;
                    var index = 0;

                    var existedDBSingleDepartment = listDepartmentDB.FirstOrDefault(x => x.Name == name
                        && ((x.ParentIndex > 0) ? !listDepartmentDB.Any(y => y.Index == x.ParentIndex) : true)
                        //&& !listDepartmentDB.Any(y => y.ParentIndex == x.Index)
                        );
                    if (existedDBSingleDepartment != null)
                    {
                        code = existedDBSingleDepartment.Code;
                        index = existedDBSingleDepartment.Index;
                    }
                    else if (listDepartmentCreate.Any(x => x.Name == name))
                    {
                        code = listDepartmentCreate.FirstOrDefault(x => x.Name == name)?.Code ?? nowTicks;
                    }

                    if (string.IsNullOrWhiteSpace(code))
                    {
                        code = nowTicks;
                    }

                    var departmentDTO = new IC_DepartmentDTO
                    {
                        Name = name,
                        Code = code,
                        CompanyIndex = user.CompanyIndex,
                        ParentIndex = 0,
                        ParentCode = string.Empty,
                        ParentName = string.Empty,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.FullName,
                        CreatedDate = DateTime.Now,
                        Index = index,
                    };

                    if (listDepartmentCreate.Any(x => x.Code == code))
                    {
                        continue;
                    }

                    listDepartmentCreate.Add(departmentDTO);
                }

                foreach (var departmentName in listTreeDepartmentName)
                {
                    var parentCode = string.Empty;
                    var parentIndex = 0;
                    var listSplitDepartmentName = departmentName.Split("/").Distinct().ToList();
                    for (var i = 0; i < listSplitDepartmentName.Count; i++)
                    {
                        var name = listSplitDepartmentName[i];
                        var nowTicks = DateTime.Now.Ticks.ToString();
                        var code = nowTicks;
                        var index = -1;

                        var departmentDTO = new IC_DepartmentDTO();
                        if (i == 0)
                        {
                            var listExistedDepartment = listDepartmentDB.Where(x => x.Name == name).ToList();
                            var existedDepartment = listExistedDepartment.FirstOrDefault(x
                                => ((x.ParentIndex > 0) ? !listDepartmentDB.Any(y => y.Index == x.ParentIndex) : true)
                                //&& listDepartmentDB.Any(y => y.ParentIndex == x.Index)
                                );
                            if (existedDepartment != null)
                            {
                                code = existedDepartment.Code;
                                index = existedDepartment.Index;
                                if (string.IsNullOrWhiteSpace(code)
                                    && listDepartmentCreate.Any(y => y.Name == name && string.IsNullOrWhiteSpace(y.ParentCode)))
                                {
                                    code = listDepartmentCreate.FirstOrDefault(y => y.Name == name)?.Code ?? nowTicks;
                                }
                            }
                            else if (listDepartmentCreate.Any(y => y.Name == name && string.IsNullOrWhiteSpace(y.ParentCode)))
                            {
                                code = listDepartmentCreate.FirstOrDefault(y => y.Name == name)?.Code ?? nowTicks;
                            }

                            departmentDTO = new IC_DepartmentDTO
                            {
                                Name = listSplitDepartmentName[i],
                                Code = code,
                                CompanyIndex = user.CompanyIndex,
                                ParentIndex = 0,
                                ParentCode = string.Empty,
                                ParentName = string.Empty,
                                UpdatedDate = DateTime.Now,
                                UpdatedUser = user.FullName,
                                CreatedDate = DateTime.Now
                            };
                        }
                        else
                        {
                            var parentName = listSplitDepartmentName[i - 1];
                            var existedParentDepartment = listDepartmentDB.FirstOrDefault(x => x.Code == parentCode
                                || x.Index == parentIndex);
                            if (existedParentDepartment != null)
                            {
                                var existedDepartmentWithParent = listDepartmentDB.FirstOrDefault(x => x.Name == name
                                    && x.Index != existedParentDepartment.Index
                                    && x.ParentIndex == existedParentDepartment.Index);
                                if (existedDepartmentWithParent != null)
                                {
                                    code = existedDepartmentWithParent.Code;
                                    index = existedDepartmentWithParent.Index;
                                    if (string.IsNullOrWhiteSpace(code)
                                    && listDepartmentCreate.Any(y => y.Name == name && y.ParentCode == parentCode))
                                    {
                                        code = listDepartmentCreate.FirstOrDefault(y => y.Name == name
                                            && y.ParentCode == parentCode)?.Code ?? nowTicks;
                                    }
                                }
                            }
                            else if (listDepartmentCreate.Any(y => y.Name == name && y.ParentCode == parentCode))
                            {
                                code = listDepartmentCreate.FirstOrDefault(y => y.Name == name
                                    && y.ParentCode == parentCode)?.Code ?? nowTicks;
                            }

                            departmentDTO = new IC_DepartmentDTO
                            {
                                Name = listSplitDepartmentName[i],
                                Code = code,
                                CompanyIndex = user.CompanyIndex,
                                ParentIndex = 0,
                                ParentCode = parentCode,
                                ParentName = listSplitDepartmentName[i - 1],
                                UpdatedDate = DateTime.Now,
                                UpdatedUser = user.FullName,
                                CreatedDate = DateTime.Now
                            };
                        }

                        if (string.IsNullOrWhiteSpace(code))
                        {
                            code = nowTicks;
                        }

                        parentCode = code;
                        departmentDTO.Code = code;
                        parentIndex = index;
                        departmentDTO.Index = index;

                        if (index > 0 && listDepartmentCreate.Any(x => x.Index == index))
                        {
                            continue;
                        }
                        if (listDepartmentCreate.Any(x => x.Code == code))
                        {
                            continue;
                        }

                        listDepartmentCreate.Add(departmentDTO);
                    }
                }

                //if (listTreeDepartmentCreate != null && listTreeDepartmentCreate.Count > 0)
                //{
                //    listDepartmentCreate.AddRange(listTreeDepartmentCreate);
                //}

                //if (listSingleDepartmentCreate != null && listSingleDepartmentCreate.Count > 0)
                //{
                //    listDepartmentCreate.AddRange(listSingleDepartmentCreate);
                //}

                //listCreate = _iC_DepartmentLogic.CheckExistedOrCreateList(listCreate);

                var isAutoCreateDepartment = true;
                var ic_config = context.IC_Config.FirstOrDefault(t => t.EventType == ConfigAuto.CREATE_DEPARTMENT_IMPORT_EMPLOYEE.ToString());
                if (ic_config != null && ic_config.CustomField != null && ic_config.CustomField != "")
                {
                    var customField = JsonConvert.DeserializeObject<IntegrateLogParam>(ic_config.CustomField);
                    isAutoCreateDepartment = customField.AutoCreateDepartmentImportEmployee;
                }

                var listCreate = new List<IC_DepartmentDTO>();
                if (isAutoCreateDepartment == true)
                {
                    listCreate = _iC_DepartmentLogic.CheckExistedListDepartmentFromImport(listDepartmentCreate, user);
                }
                else
                {
                    listCreate = _iC_DepartmentLogic.CheckExistedOrCreateListDepartmentFromImport(listDepartmentCreate, user);
                }

                var listPosition = param.GroupBy(e => e.Position).Select(e => e.First()).Select(e => e.Position).ToList();
                var listPositionDB = await _IHR_PositionInfoService.GetAllPositionInfo(user.CompanyIndex);

                var listEmployeeATIDs = param.Select(e => e.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0')).ToList();

                var listHRUserDB = new List<HR_User>();
                var listEmployeeDB = new List<HR_EmployeeInfo>();
                var listWorkingInfoDB = new List<IC_WorkingInfo>();
                var listUserMasterDB = new List<IC_UserMaster>();
                var listCardNumberDB = new List<HR_CardNumberInfo>();


                if (listEmployeeATIDs.Count > 5000)
                {
                    var listSplitEmployeeID = CommonUtils.SplitList(listEmployeeATIDs, 5000);
                    //var a = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listSplitEm.Contains(e.EmployeeATID)) &&
                    //).ToList();
                    var listHRUser = context.HR_User.Where(x => listEmployeeATIDs.Contains(x.EmployeeATID)).ToList();
                    var listEmployeeInfo = context.HR_EmployeeInfo.Where(x => listEmployeeATIDs.Contains(x.EmployeeATID)).ToList();
                    var listWorkingInfo = context.IC_WorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeATIDs.Contains(e.EmployeeATID)
                          && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && e.ToDate == null).OrderByDescending(e => e.FromDate).ToList();
                    var listUserMaster = context.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeATIDs.Contains(e.EmployeeATID)).ToList();
                    var resultCardNumber = context.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();

                    foreach (var listEmployeeSplit in listSplitEmployeeID)
                    {
                        var resultHRUser = listHRUser.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultEmployee = listEmployeeInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultWorkingInfo = listWorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)
                           && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && e.ToDate == null).OrderByDescending(e => e.FromDate).ToList();
                        var resultUserMaster = listUserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();

                        listHRUserDB.AddRange(resultHRUser);
                        listEmployeeDB.AddRange(resultEmployee);
                        listWorkingInfoDB.AddRange(resultWorkingInfo);
                        listUserMasterDB.AddRange(resultUserMaster);
                        listCardNumberDB.AddRange(resultCardNumber);
                    }
                }
                else
                {
                    listHRUserDB = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeATIDs.Contains(e.EmployeeATID)).ToList();
                    listEmployeeDB = context.HR_EmployeeInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeATIDs.Contains(e.EmployeeATID)).ToList();

                    listWorkingInfoDB = context.IC_WorkingInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeATIDs.Contains(e.EmployeeATID)
                       && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date).OrderByDescending(e => e.FromDate).ToList();

                    listUserMasterDB = context.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeATIDs.Contains(e.EmployeeATID)).ToList();
                    listCardNumberDB = context.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();
                }

                //List<HR_User> listUser = new List<HR_User>();
                //List<HR_EmployeeInfo> listEmployee = new List<HR_EmployeeInfo>();
                //List<IC_WorkingInfoDTO> listWorkingInfo = new List<IC_WorkingInfoDTO>();
                //List<IC_UserMasterDTO> listUserMaster = new List<IC_UserMasterDTO>();
                var paramList = param.ToList();

                var listEmployeeTypeName = paramList.Select(x => x.EmployeeTypeName).Distinct().ToList();
                var listEmployeeType = new List<IC_EmployeeType>();
                if (listEmployeeTypeName != null && listEmployeeTypeName.Count > 0)
                {
                    listEmployeeType = await _IIC_EmployeeTypeService.GetDataByListNameAndCompanyIndex(listEmployeeTypeName,
                        user.CompanyIndex);
                }
                foreach (var importItem in paramList)
                {
                    try
                    {
                        var department = new IC_DepartmentDTO();
                        department.Index = 0;
                        var splitDepartmentNameList = new List<string>();
                        if (importItem.DepartmentName.Contains("/"))
                        {
                            splitDepartmentNameList = importItem.DepartmentName.Split("/").ToList();
                            importItem.DepartmentName = splitDepartmentNameList[splitDepartmentNameList.Count - 1];

                            if (listCreate != null)
                            {
                                var countLevelDepartment = 0;
                                var nextLevelChildDepartmentIndex = new List<long>();
                                var itemDepartment = new IC_DepartmentDTO();
                                while (countLevelDepartment < splitDepartmentNameList.Count)
                                {
                                    var departmentDTOs = new List<IC_DepartmentDTO>();
                                    if (!nextLevelChildDepartmentIndex?.Any() ?? false)
                                    {
                                        departmentDTOs = listCreate.Where(x => x.Name == splitDepartmentNameList[countLevelDepartment]).ToList();
                                        if (departmentDTOs?.Any() ?? false)
                                        {
                                            var listChildDepartment = listCreate.Where(x => departmentDTOs.Any(y => y.Index == x.ParentIndex)).ToList();
                                            nextLevelChildDepartmentIndex = listChildDepartment.Select(x => x.Index).ToList();
                                        }
                                    }
                                    else
                                    {
                                        departmentDTOs = listCreate.Where(x => x.Name == splitDepartmentNameList[countLevelDepartment]
                                            && nextLevelChildDepartmentIndex.Contains(x.Index)).ToList();
                                        if (departmentDTOs?.Any() ?? false)
                                        {
                                            var listChildDepartment = listCreate.Where(x => departmentDTOs.Any(y => y.Index == x.ParentIndex)).ToList();
                                            nextLevelChildDepartmentIndex = listChildDepartment.Select(x => x.Index).ToList();
                                        }
                                    }

                                    if (countLevelDepartment == (splitDepartmentNameList.Count - 1))
                                    {
                                        itemDepartment = departmentDTOs.FirstOrDefault(x => x.Name == splitDepartmentNameList[countLevelDepartment]);
                                        break;
                                    }

                                    if (nextLevelChildDepartmentIndex?.Any() ?? false)
                                    {
                                        countLevelDepartment++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                if (itemDepartment != null)
                                {
                                    department = itemDepartment;
                                }
                                if (department != null && itemDepartment == null)
                                {
                                    department.Index = 0;
                                }
                            }
                        }
                        else
                        {
                            department = listCreate.FirstOrDefault(e => !string.IsNullOrWhiteSpace(importItem.DepartmentName)
                                && e.Name == importItem.DepartmentName.Trim() && (!e.ParentIndex.HasValue
                                || (e.ParentIndex.HasValue && e.ParentIndex.Value == 0))
                                //&& !listCreate.Any(y => y.ParentIndex == e.Index)
                                );
                            var itemDepartment = listCreate.FirstOrDefault(e => !string.IsNullOrWhiteSpace(importItem.DepartmentName)
                                && e.Name == importItem.DepartmentName.Trim() && (!e.ParentIndex.HasValue
                                || (e.ParentIndex.HasValue && e.ParentIndex.Value == 0))
                                //&& !listCreate.Any(y => y.ParentIndex == e.Index)
                                );
                        }

                        importItem.EmployeeATID = importItem.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');

                        var existedUser = listHRUserDB.FirstOrDefault(e => e.EmployeeATID == importItem.EmployeeATID);
                        if (existedUser != null)
                        {
                            existedUser.EmployeeCode = importItem.EmployeeCode;
                            existedUser.FullName = importItem.FullName;
                            existedUser.Gender = (short)importItem.Gender;
                            if (!string.IsNullOrWhiteSpace(importItem.EmployeeTypeName))
                            {
                                var employeeType = listEmployeeType.FirstOrDefault(x => x.IsUsing && x.Name == importItem.EmployeeTypeName);
                                existedUser.EmployeeTypeIndex = employeeType?.Index ?? null;
                            }
                            else
                            {
                                existedUser.EmployeeTypeIndex = null;
                            }
                            existedUser.DayOfBirth = StringHelper.GetDateOfBirthEmployee("Day", importItem.DateOfBirth);
                            existedUser.MonthOfBirth = StringHelper.GetDateOfBirthEmployee("Month", importItem.DateOfBirth);
                            existedUser.YearOfBirth = StringHelper.GetDateOfBirthEmployee("Year", importItem.DateOfBirth);
                            existedUser.UpdatedDate = DateTime.Now;
                            existedUser.UpdatedUser = user.UserName;
                            existedUser.Note = importItem.Note;
                            existedUser.Nric = importItem.Nric;
                            existedUser.IsAllowPhone = importItem.IsAllowPhone == 1 ? true : false;
                            existedUser.Address = importItem.Address;
                            context.HR_User.Update(existedUser);
                        }
                        else
                        {
                            existedUser = new HR_User();
                            existedUser.CompanyIndex = user.CompanyIndex;
                            existedUser.EmployeeATID = importItem.EmployeeATID;
                            existedUser.EmployeeCode = importItem.EmployeeCode;
                            if (!string.IsNullOrWhiteSpace(importItem.EmployeeTypeName))
                            {
                                var employeeType = listEmployeeType.FirstOrDefault(x => x.IsUsing && x.Name == importItem.EmployeeTypeName);
                                existedUser.EmployeeTypeIndex = employeeType?.Index ?? null;
                            }
                            existedUser.FullName = importItem.FullName;
                            existedUser.Gender = (short)importItem.Gender;
                            existedUser.DayOfBirth = StringHelper.GetDateOfBirthEmployee("Day", importItem.DateOfBirth);
                            existedUser.MonthOfBirth = StringHelper.GetDateOfBirthEmployee("Month", importItem.DateOfBirth);
                            existedUser.YearOfBirth = StringHelper.GetDateOfBirthEmployee("Year", importItem.DateOfBirth);
                            existedUser.EmployeeType = (int)EmployeeType.Employee;
                            existedUser.UpdatedDate = DateTime.Now;
                            existedUser.UpdatedUser = user.UserName;
                            existedUser.CreatedDate = DateTime.Now;
                            existedUser.Note = importItem.Note;
                            existedUser.IsAllowPhone = importItem.IsAllowPhone == 1 ? true : false;
                            existedUser.Nric = importItem.Nric;
                            existedUser.Address = importItem.Address;
                            context.HR_User.Add(existedUser);
                        }

                        var existedEmployee = listEmployeeDB.FirstOrDefault(e => e.EmployeeATID == importItem.EmployeeATID);
                        if (existedEmployee != null)
                        {
                            existedEmployee.Email = importItem.Email;
                            existedEmployee.Phone = importItem.PhoneNumber;
                            existedEmployee.JoinedDate = string.IsNullOrWhiteSpace(importItem.JoinedDate) ? DateTime.Now : DateTime.ParseExact(importItem.JoinedDate, "dd/MM/yyyy", null);
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            context.HR_EmployeeInfo.Update(existedEmployee);
                        }
                        else
                        {
                            existedEmployee = new HR_EmployeeInfo();
                            existedEmployee.CompanyIndex = user.CompanyIndex;
                            existedEmployee.EmployeeATID = importItem.EmployeeATID;
                            existedEmployee.Email = importItem.Email;
                            existedEmployee.Phone = importItem.PhoneNumber;
                            existedEmployee.JoinedDate = string.IsNullOrWhiteSpace(importItem.JoinedDate) ? DateTime.Now : DateTime.ParseExact(importItem.JoinedDate, "dd/MM/yyyy", null);
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            context.HR_EmployeeInfo.Add(existedEmployee);
                        }

                        var existedCardNumber = listCardNumberDB.FirstOrDefault(e => e.CardNumber == importItem.CardNumber);
                        if (existedCardNumber == null)
                        {

                            var listCard = listCardNumberDB.Where(x => x.EmployeeATID == existedUser.EmployeeATID);
                            foreach (var itemCard in listCard)
                            {
                                itemCard.IsActive = false;
                                itemCard.UpdatedDate = DateTime.Now;
                                context.HR_CardNumberInfo.Update(itemCard);
                            }
                            existedCardNumber = new HR_CardNumberInfo();
                            existedCardNumber.EmployeeATID = existedUser.EmployeeATID;
                            existedCardNumber.CompanyIndex = existedUser.CompanyIndex;
                            existedCardNumber.CardNumber = importItem.CardNumber;
                            existedCardNumber.IsActive = true;
                            existedCardNumber.CreatedDate = DateTime.Now;
                            existedCardNumber.UpdatedDate = existedUser.UpdatedDate;
                            existedCardNumber.UpdatedUser = existedUser.UpdatedUser;
                            context.HR_CardNumberInfo.Add(existedCardNumber);
                        }

                        var workingInfoFromDate = DateTime.Now;
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(importItem.JoinedDate))
                            {
                                workingInfoFromDate = DateTime.ParseExact(importItem.JoinedDate, "dd/MM/yyyy",
                                    System.Globalization.CultureInfo.InvariantCulture);
                            }
                        }
                        catch (Exception)
                        {
                            workingInfoFromDate = DateTime.Now;
                        }


                        var position = listPositionDB.FirstOrDefault(e => e.Name.ToLower().RemoveAccents() == importItem.Position.ToLower().RemoveAccents());
                        var existedWorkingInfo = listWorkingInfoDB.FirstOrDefault(e => e.EmployeeATID == importItem.EmployeeATID);
                        if (existedWorkingInfo == null)
                        {
                            existedWorkingInfo = new IC_WorkingInfo();
                            existedWorkingInfo.EmployeeATID = existedUser.EmployeeATID;
                            existedWorkingInfo.CompanyIndex = existedUser.CompanyIndex;
                            existedWorkingInfo.DepartmentIndex = department.Index;
                            existedWorkingInfo.PositionIndex = position != null ? position.Index : 0;
                            existedWorkingInfo.FromDate = workingInfoFromDate;
                            existedWorkingInfo.ToDate = !string.IsNullOrWhiteSpace(importItem.StoppedDate) ? DateTime.ParseExact(importItem.StoppedDate, "dd/MM/yyyy",
                                System.Globalization.CultureInfo.InvariantCulture) : null;
                            existedWorkingInfo.IsManager = false;
                            existedWorkingInfo.ApprovedDate = DateTime.Now;
                            existedWorkingInfo.UpdatedUser = user.UserName;
                            existedWorkingInfo.Status = (short)TransferStatus.Approve;
                            context.IC_WorkingInfo.Add(existedWorkingInfo);
                        }
                        else if (existedWorkingInfo.DepartmentIndex == 0 && existedWorkingInfo.DepartmentIndex != department.Index
                            && department.Index != 0)
                        {
                            existedWorkingInfo.DepartmentIndex = department.Index;
                            existedWorkingInfo.PositionIndex = position != null ? position.Index : 0;
                            existedWorkingInfo.IsSync = null;
                            existedWorkingInfo.Status = (short)TransferStatus.Approve;
                            existedWorkingInfo.FromDate = workingInfoFromDate;
                            existedWorkingInfo.ToDate = !string.IsNullOrWhiteSpace(importItem.StoppedDate) ? DateTime.ParseExact(importItem.StoppedDate, "dd/MM/yyyy",
                                System.Globalization.CultureInfo.InvariantCulture) : null;
                            existedWorkingInfo.ApprovedDate = DateTime.Now;
                            existedWorkingInfo.UpdatedUser = user.UserName;
                            existedWorkingInfo.UpdatedDate = DateTime.Now;
                            context.IC_WorkingInfo.Update(existedWorkingInfo);
                        }
                        //else if (existedWorkingInfo.DepartmentIndex > 0 && existedWorkingInfo.FromDate.Date <= DateTime.Now.Date
                        //    && existedWorkingInfo.DepartmentIndex != department.Index && department.Index != 0)
                        //{
                        //    var workingInfo = new IC_WorkingInfo();
                        //    workingInfo.EmployeeATID = existedUser.EmployeeATID;
                        //    workingInfo.CompanyIndex = existedUser.CompanyIndex;
                        //    workingInfo.DepartmentIndex = department.Index;
                        //    workingInfo.PositionIndex = position != null ? position.Index : 0;
                        //    workingInfo.FromDate = workingInfoFromDate;
                        //    workingInfo.IsManager = false;
                        //    workingInfo.ApprovedDate = DateTime.Now;
                        //    workingInfo.UpdatedUser = user.UserName;
                        //    workingInfo.Status = (short)TransferStatus.Approve;
                        //    context.IC_WorkingInfo.Add(workingInfo);
                        //    //
                        //    existedWorkingInfo.ToDate = DateTime.Now.AddDays(-1);
                        //    existedWorkingInfo.PositionIndex = position != null ? position.Index : 0;
                        //    existedWorkingInfo.ApprovedDate = DateTime.Now;
                        //    existedWorkingInfo.UpdatedUser = user.UserName;
                        //    existedWorkingInfo.UpdatedDate = DateTime.Now;
                        //    context.IC_WorkingInfo.Update(existedWorkingInfo);
                        //}
                        else if (existedWorkingInfo.DepartmentIndex > 0 && existedWorkingInfo.DepartmentIndex == department.Index && department.Index != 0)
                        {
                            existedWorkingInfo.EmployeeATID = existedUser.EmployeeATID;
                            existedWorkingInfo.CompanyIndex = existedUser.CompanyIndex;
                            existedWorkingInfo.ToDate = !string.IsNullOrWhiteSpace(importItem.StoppedDate) ? DateTime.ParseExact(importItem.StoppedDate, "dd/MM/yyyy",
                                System.Globalization.CultureInfo.InvariantCulture) : null;
                            existedWorkingInfo.ApprovedDate = DateTime.Now;
                            existedWorkingInfo.UpdatedUser = user.UserName;
                            existedWorkingInfo.Status = (short)TransferStatus.Approve;
                            context.IC_WorkingInfo.Update(existedWorkingInfo);
                        }

                        var existedUserMaster = listUserMasterDB.FirstOrDefault(e => e.EmployeeATID == importItem.EmployeeATID);
                        if (existedUserMaster == null)
                        {
                            existedUserMaster = new IC_UserMaster();
                            existedUserMaster.EmployeeATID = existedUser.EmployeeATID;
                            existedUserMaster.CompanyIndex = existedUser.CompanyIndex;
                            existedUserMaster.NameOnMachine = importItem.NameOnMachine;
                            existedUserMaster.CardNumber = importItem.CardNumber;
                            existedUserMaster.Password = importItem.Password;
                            existedUserMaster.AuthenMode = AuthenMode.FullAccessRight.ToString();
                            existedUserMaster.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.CreatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            context.IC_UserMaster.Add(existedUserMaster);
                        }
                        else
                        {
                            existedUserMaster.NameOnMachine = importItem.NameOnMachine;
                            existedUserMaster.CardNumber = importItem.CardNumber;
                            existedUserMaster.Password = importItem.Password;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            context.IC_UserMaster.Update(existedUserMaster);
                        }
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError($"AddEmployeeFromExcel: {ex}");
                        return BadRequest(ex.Message);
                    }
                }
                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _Logger.LogError($"AddEmployeeFromExcel: {ex}");
                    return BadRequest(ex.Message);
                }
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "HR_User";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + listEmployeeATIDs.Count().ToString() + " Employee";
                audit.Description = AuditType.Added.ToString() + "EmployeeFromExcel:/:" + listEmployeeATIDs.Count().ToString();
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);

                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER });
                var systemconfigs = await _iC_ConfigLogic.GetMany(addedParams);
                if (systemconfigs != null)
                {
                    var sysconfig = systemconfigs.FirstOrDefault();
                    if (sysconfig != null)
                    {
                        if (sysconfig.IntegrateLogParam.AutoIntegrate)
                        {
                            await _IIC_CommandLogic.SyncWithEmployee(listEmployeeATIDs, user.CompanyIndex);
                        }
                    }
                }
            }
            if (_configClientName.ToUpper() == ClientName.AVN.ToString() || _configClientName == ClientName.AEON.ToString())
            {
                await AutoActiveESSAccount();
            }

            var employeeATIDs = param.Select(x => x.EmployeeATID).ToList();
            await _IC_VehicleLogService.IntegrateEmployeeToLovad(employeeATIDs);
            await _IHR_EmployeeLogic.IntegrateUserToOfflineEmployee(employeeATIDs);
            await _IIC_UserAuditService.InsertAudit(employeeATIDs);

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

            // Phân quyền theo phòng ban
            user.InitDepartmentAssignedAndParent(context, otherContext, cache);
            //_logger.LogError("Import employees from excel, count error: " + message);
            result = Ok(message);
            return result;
        }

        [Authorize]
        [ActionName("DeleteEmployeeFromExcel")]
        [HttpPost]
        public IActionResult DeleteEmployeeFromExcel([FromBody] List<AddedParam> addedParams)
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
                listPadleft.Add(item.PadLeft(_config.MaxLenghtEmployeeATID, '0'));
            }
            var existedEmployee = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listPadleft.Contains(e.EmployeeATID)).ToList();

            if (paramDeleteOndevice != null)
            {
                isDeleteOnDevice = Convert.ToBoolean(paramDeleteOndevice.Value);
                if (isDeleteOnDevice)
                {
                    var lsSerialHw = context.IC_Device.Where(e => e.CompanyIndex == user.CompanyIndex).Select(e => e.SerialNumber).ToList();

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

            var listEmp = context.HR_User.Where(t => t.CompanyIndex == user.CompanyIndex && listPadleft.Contains(t.EmployeeATID)).ToArray();
            var listUserContactInfo = context.HR_UserContactInfo.Where(t => t.CompanyIndex == user.CompanyIndex && listPadleft.Contains(t.UserIndex)).ToArray();
            var listWorkingInfo = context.IC_WorkingInfo.Where(x => x.CompanyIndex == user.CompanyIndex && listPadleft.Contains(x.EmployeeATID)).ToArray();
            var listWorkingTransfer = context.IC_EmployeeTransfer.Where(x => x.CompanyIndex == user.CompanyIndex && listPadleft.Contains(x.EmployeeATID)).ToArray();
            var listUserMaster = context.IC_UserMaster.Where(x => x.CompanyIndex == user.CompanyIndex && listPadleft.Contains(x.EmployeeATID)).ToArray();

            try
            {
                context.HR_User.RemoveRange(listEmp);
                context.HR_UserContactInfo.RemoveRange(listUserContactInfo);
                context.IC_WorkingInfo.RemoveRange(listWorkingInfo);
                context.IC_EmployeeTransfer.RemoveRange(listWorkingTransfer);
                context.IC_UserMaster.RemoveRange(listUserMaster);
                context.SaveChanges();
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "HR_User";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Deleted;
                //audit.Description = AuditType.Deleted.ToString() + " From Excel " + listEmp.Count().ToString() + " Employee";
                audit.Description = AuditType.Deleted.ToString() + "EmployeeFromExcel:/:" + listEmp.Count().ToString();
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
        [ActionName("ExportToExcel")]
        [HttpPost]
        public async Task<IActionResult> ExportToExcel([FromBody] List<AddedParam> addedParams, [FromQuery] int userType = 1)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                //return new byte[0];
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var config = ConfigObject.GetConfig(cache);

            var addedParamTrans = new List<AddedParam>();
            addedParamTrans.Add(new AddedParam { Key = "IsCurrentTransfer", Value = true });
            addedParamTrans.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            addedParamTrans.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
            addedParamTrans.Add(new AddedParam { Key = "UserType", Value = userType });

            List<IC_EmployeeTransferDTO> listEmTransfer = _iC_EmployeeTransferLogic.GetMany(addedParamTrans);
            if (listEmTransfer != null && listEmTransfer.Count > 0)
            {
                addedParams.Add(new AddedParam { Key = "ListEmployeeTransferATID", Value = listEmTransfer.Select(e => e.EmployeeATID).ToList() });
            }

            var paramListDepartment = addedParams.FirstOrDefault(e => e.Key == "ListDepartment");
            if (paramListDepartment != null)
            {
                addedParams.Remove(paramListDepartment);
                IList<long> departments = JsonConvert.DeserializeObject<IList<long>>(paramListDepartment.Value.ToString());
                if (departments != null && departments.Count() > 0)
                {
                    addedParams.Add(new AddedParam { Key = "ListDepartment", Value = departments });
                }
                else
                {
                    addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
                }
            }

            var paramIsWorking = addedParams.FirstOrDefault(e => e.Key == "IsWorking");
            if (paramIsWorking != null)
            {
                addedParams.Remove(paramIsWorking);
                IList<int> listStatusWorking = JsonConvert.DeserializeObject<IList<int>>(paramIsWorking.Value.ToString());
                if (listStatusWorking != null && listStatusWorking.Any())
                {
                    addedParams.Add(new AddedParam { Key = "IsWorking", Value = listStatusWorking });
                }
            }

            addedParams.Remove(paramIsWorking);
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            addedParams.Add(new AddedParam { Key = "TransferStatus", Value = TransferStatus.Approve });
            //addedParams.Add(new AddedParam { Key = "IsCurrentWorkingAndNoDepartment", Value = true });
            addedParams.Add(new AddedParam { Key = "UserType", Value = userType });
            addedParams.Add(new AddedParam { Key = "ApproveStatus", Value = (long)TransferStatus.Approve });
            //addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
            List<IC_EmployeeDTO> employees = null;
            if (config.IntegrateDBOther == false)
            {
                //employees = context.HR_User.Where(t => t.CompanyIndex == user.CompanyIndex).OrderBy(t => t.EmployeeATID).ToList();
                if (userType == 1)
                {
                    employees = _IIC_EmployeeLogic.GetManyExportEmployee(addedParams);
                }
                else
                {
                    employees = _IIC_EmployeeLogic.GetManyExport(addedParams);
                }

            }
            else
            {
                employees = _IIC_EmployeeLogic.GetManyExportEmployee(addedParams);
            }

            var listDepartmentIndex = employees
            .Select(x => x.ContactDepartment)
            .Select(dept =>
            {
                int.TryParse(dept, out int index);
                return index;
            })
            .ToList();

            var departmentInfo = context.IC_Department.Where(x => listDepartmentIndex.Contains(x.Index)).ToList();
            var employeeInfo = context.HR_User.Where(x => employees.Select(z => z.ContactPerson).Contains(x.EmployeeATID)).ToList();

            var obj = employees.Select(e => new
            {
                EmployeeATID = e.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'),
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                CardNumber = e.CardNumber,
                NameOnMachine = e.NameOnMachine,
                _Gender = e.Gender == 1 ? "x" : "",
                _DepartmentName = e.DepartmentName,
                Nric = e.NRIC,
                PositionName = e.PositionName,
                JoinedDate = e.JoinedDate.ToddMMyyyy(),
                StoppedDate = e.ToDate != null ? e.ToDate.Value.ToddMMyyyy() : "",
                Password = e.Password,
                BirthDay = e.BirthDay,
                PhoneNumber = e.PhoneNumber,
                Email = e.Email,
                Address = e.Address,
                EmployeeTypeName = e.EmployeeTypeName,
                IsAllowPhone = e.IsAllowPhone == true ? "x" : "",
                CompanyName = e.CompanyName,
                ContactDepartment = !string.IsNullOrWhiteSpace(e.ContactDepartment) ? departmentInfo.FirstOrDefault(x => x.Index == int.Parse(e.ContactDepartment))?.Name : "",
                ContactPerson = !string.IsNullOrWhiteSpace(e.ContactPerson) ? employeeInfo.FirstOrDefault(x => x.EmployeeATID == e.ContactPerson)?.FullName : "",
                FromDateStr = e.FromDateStr,
                ToDateStr = e.ToDateStr,
                FromTime = e.FromTime,
                ToTime = e.ToTime,
                Note = e.Note,
                WorkingContent = e.WorkingContent

            }).ToList();

            string sWebRootFolder = _hostingEnvironment.ContentRootPath;
            if (userType == (int)EmployeeType.Contractor)
            {
                string URL_Contractor = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Employee.xlsx");
                FileInfo file_Contractor = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Employee.xlsx"));

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Contractor");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Mã người dùng";
                    worksheet.Cell(currentRow, 2).Value = "Họ tên";
                    worksheet.Cell(currentRow, 3).Value = "Tên trên máy";
                    worksheet.Cell(currentRow, 4).Value = "Giới tính (Nam)";
                    worksheet.Cell(currentRow, 5).Value = "Ngày sinh (ngày/tháng/năm)";
                    worksheet.Cell(currentRow, 6).Value = "CMND/CCCD/Passport";
                    worksheet.Cell(currentRow, 7).Value = "Số điện thoại";
                    worksheet.Cell(currentRow, 8).Value = "Email";
                    worksheet.Cell(currentRow, 9).Value = "Địa chỉ";
                    worksheet.Cell(currentRow, 10).Value = "Phòng ban";
                    worksheet.Cell(currentRow, 11).Value = "Chức vụ";
                    worksheet.Cell(currentRow, 12).Value = "Sử dụng điện thoại";
                    worksheet.Cell(currentRow, 13).Value = "Ngày bắt đầu";
                    worksheet.Cell(currentRow, 14).Value = "Ngày kết thúc";
                    worksheet.Cell(currentRow, 15).Value = "Mã thẻ";

                    for (int i = 1; i < 16; i++)
                    {
                        worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                        worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Column(i).Width = 20;
                    }

                    foreach (var users in obj)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = users.EmployeeATID;
                        worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeATID.Length, '0');


                        worksheet.Cell(currentRow, 2).Value = users.FullName;
                        worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 3).Value = users.NameOnMachine;
                        worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 4).Value = users._Gender;
                        worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 5).Value = users.BirthDay;
                        worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 6).Value = users.Nric;
                        worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 7).Value = users.PhoneNumber;
                        worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 8).Value = users.Email;
                        worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 9).Value = users.Address;
                        worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 10).Value = users._DepartmentName;
                        worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 11).Value = users.PositionName;
                        worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 12).Value = users.IsAllowPhone;
                        worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 13).Value = users.JoinedDate;
                        worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 14).Value = users.StoppedDate;
                        worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                        worksheet.Cell(currentRow, 15).Value = users.CardNumber;
                        worksheet.Cell(currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        if (!string.IsNullOrWhiteSpace(users.CardNumber))
                            worksheet.Cell(currentRow, 15).Style.NumberFormat.Format = "0".PadLeft(users.CardNumber.Length, '0');
                        worksheet.Cell(currentRow, 15).Value = users.CardNumber;
                    }

                    //var workbookBytes = new byte[0];
                    //using (var ms = new MemoryStream())
                    //{
                    //    workbook.SaveAs(ms);
                    //    return workbookBytes = ms.ToArray();
                    //}
                    MemoryStream stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"Contractor{DateTime.Now.ToddMMyyyyHHmmss()}.xlsx"
                    };
                }
            }
            else if (userType == (int)EmployeeType.Guest)
            {
                string URL_Contractor = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Employee.xlsx");
                FileInfo file_Contractor = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Employee.xlsx"));

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Customer");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Mã người dùng (*)";
                    worksheet.Cell(currentRow, 2).Value = "Họ tên (*)";
                    worksheet.Cell(currentRow, 3).Value = "Tên trên máy";
                    worksheet.Cell(currentRow, 4).Value = "Giới tính (Nam)";
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

                    for (int i = 1; i < 20; i++)
                    {
                        worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                        worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Column(i).Width = 20;
                    }

                    foreach (var users in obj)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = users.EmployeeATID;
                        worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeATID.Length, '0');


                        worksheet.Cell(currentRow, 2).Value = users.FullName;
                        worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 3).Value = users.NameOnMachine;
                        worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 4).Value = users._Gender;
                        worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 5).Value = users.BirthDay;
                        worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 6).Value = users.Nric;
                        worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 7).Value = users.CompanyName;
                        worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 8).Value = users.PhoneNumber;
                        worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 9).Value = users.Email;
                        worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 10).Value = users.Address;
                        worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 11).Value = users.ContactDepartment;
                        worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 12).Value = users.ContactPerson;
                        worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 13).Value = users.FromDateStr;
                        worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 14).Value = users.ToDateStr;
                        worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 15).Value = users.FromTime;
                        worksheet.Cell(currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 16).Value = users.ToTime;
                        worksheet.Cell(currentRow, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 17).Value = users.IsAllowPhone;
                        worksheet.Cell(currentRow, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 18).Value = users.WorkingContent;
                        worksheet.Cell(currentRow, 18).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 19).Value = users.Note;
                        worksheet.Cell(currentRow, 19).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    //var workbookBytes = new byte[0];
                    //using (var ms = new MemoryStream())
                    //{
                    //    workbook.SaveAs(ms);
                    //    return workbookBytes = ms.ToArray();
                    //}
                    MemoryStream stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"Customer{DateTime.Now.ToddMMyyyyHHmmss()}.xlsx"
                    };
                }
            }
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Employee.xlsx");
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Employee.xlsx"));

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Employees");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                worksheet.Cell(currentRow, 3).Value = "Họ tên";
                worksheet.Cell(currentRow, 4).Value = "Tên trên máy";
                worksheet.Cell(currentRow, 5).Value = "Giới tính (Nam)";
                worksheet.Cell(currentRow, 6).Value = "Ngày sinh (ngày/tháng/năm)";
                worksheet.Cell(currentRow, 7).Value = "Số điện thoại";
                worksheet.Cell(currentRow, 8).Value = "CMND/CCCD/Passport";
                worksheet.Cell(currentRow, 9).Value = "Email";
                worksheet.Cell(currentRow, 10).Value = "Địa chỉ";
                worksheet.Cell(currentRow, 11).Value = "Phòng ban";
                worksheet.Cell(currentRow, 12).Value = "Chức vụ";
                worksheet.Cell(currentRow, 13).Value = "Loại nhân viên";
                worksheet.Cell(currentRow, 14).Value = "Sử dụng điện thoại";
                worksheet.Cell(currentRow, 15).Value = "Ngày vào";
                worksheet.Cell(currentRow, 16).Value = "Ngày nghỉ";
                worksheet.Cell(currentRow, 17).Value = "Mã thẻ";



                if (_configClientName == ClientName.MAY.ToString())
                {
                    worksheet.Cell(currentRow, 1).Value = "Mã chấm ăn (*)";
                    worksheet.Cell(currentRow, 2).Value = "Mã số khách hàng";
                }
                for (int i = 1; i < 18; i++)
                {
                    worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Column(i).Width = 20;
                }

                foreach (var users in obj)
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

                    worksheet.Cell(currentRow, 4).Value = users.NameOnMachine;
                    worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 5).Value = users._Gender;
                    worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 6).Value = users.BirthDay;
                    worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 7).Value = users.PhoneNumber;
                    worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 8).Value = users.Nric;
                    worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 9).Value = users.Email;
                    worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 10).Value = users.Address;
                    worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 11).Value = users._DepartmentName;
                    worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 12).Value = users.PositionName;
                    worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 13).Value = users.EmployeeTypeName;
                    worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 14).Value = users.IsAllowPhone;
                    worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 15).Value = users.JoinedDate;
                    worksheet.Cell(currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 16).Value = users.StoppedDate;
                    worksheet.Cell(currentRow, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 17).Value = users.CardNumber;
                    worksheet.Cell(currentRow, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    if (!string.IsNullOrWhiteSpace(users.CardNumber))
                        worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = "0".PadLeft(users.CardNumber.Length, '0');
                    worksheet.Cell(currentRow, 17).Value = users.CardNumber;
                }

                //var workbookBytes = new byte[0];
                //using (var ms = new MemoryStream())
                //{
                //    workbook.SaveAs(ms);
                //    return workbookBytes = ms.ToArray();
                //}
                MemoryStream stream = new MemoryStream();
                workbook.SaveAs(stream);
                return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"Employee_{DateTime.Now.ToddMMyyyyHHmmss()}.xlsx"
                };
            }
        }



        [Authorize]
        [ActionName("GetEmployeeLookup")]
        [HttpGet]
        public IActionResult GetEmployeeLookup()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            ConfigObject config = ConfigObject.GetConfig(cache);
            var rs = GetListEmployeeLookup(config, user.CompanyIndex, context, otherContext, departmentCodeConfig, departmentNameConfig);
            return Ok(rs);
        }

        [Authorize]
        [ActionName("GetEmployeeFinger")]
        [HttpGet]
        public IActionResult GetEmployeeFinger(string employeeATID)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            List<string> listUserMasterFinger = new List<string>();
            var userMaster = _iC_UserMasterLogic.GetExist(employeeATID, user.CompanyIndex);
            if (userMaster != null)
            {
                listUserMasterFinger.Add(userMaster.FingerData0);
                listUserMasterFinger.Add(userMaster.FingerData1);
                listUserMasterFinger.Add(userMaster.FingerData2);
                listUserMasterFinger.Add(userMaster.FingerData3);
                listUserMasterFinger.Add(userMaster.FingerData4);
                listUserMasterFinger.Add(userMaster.FingerData5);
                listUserMasterFinger.Add(userMaster.FingerData6);
                listUserMasterFinger.Add(userMaster.FingerData7);
                listUserMasterFinger.Add(userMaster.FingerData8);
                listUserMasterFinger.Add(userMaster.FingerData9);
            }
            return Ok(listUserMasterFinger);

        }



        [Authorize]
        [ActionName("RunIntegrate")]
        [HttpPost]
        public async Task<IActionResult> RunIntegrate()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            try
            {
                var now = DateTime.Now;
                await _IIC_ScheduleAutoHostedLogic.AutoGet1OfficeToDatabaseManual(now);
                await _IIC_ScheduleAutoHostedLogic.AutoSyncIntegrateEmloyeeManual(now);
            }
            catch (Exception ex)
            {
                return ApiError("Integrate Error " + ex.Message);
            }

            return ApiOk();

        }

        [Authorize]
        [ActionName("AutoAddOrDeleteUserManual")]
        [HttpPost]
        public async Task<IActionResult> AutoAddOrDeleteUserManual()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            try
            {
                var now = DateTime.Now;
                await _IIC_ScheduleAutoHostedLogic.AutoAddOrDeleteUserManual();
            }
            catch (Exception ex)
            {
                return ApiError("Integrate Error " + ex.Message);
            }

            return ApiOk();

        }

        [Authorize]
        [ActionName("InfoEmployeeTemplateImport")]
        [HttpGet]
        public IActionResult InfoEmployeeTemplateImport()
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
#if !DEBUG
                var folderDetails = Path.Combine(sWebRootFolder, @"epad/dist/Template_IC_Employee_Mdl.xlsx");
                var departmentList = context.IC_Department.Select(x => x.Name).OrderByDescending(x => x).ToList();
                var positionList = context.HR_PositionInfo.Select(x => x.Name).OrderByDescending(x => x).ToList();

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
                _logger.LogError("InfoEmployeeTemplateImport: ", ex);
                return ApiOk();
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
        private void CheckCardNumberInUserInfo(string pEmployeeATID, string pCardNumber, string UserNameDevice, int pCompanyIndex, string username, string password)
        {
            ConfigObject config = ConfigObject.GetConfig(cache);
            IC_UserInfo us = new IC_UserInfo();
            try
            {
                List<IC_UserInfo> userInfo = context.IC_UserInfo.Where(t => t.EmployeeATID == pEmployeeATID && t.CompanyIndex == pCompanyIndex).ToList();
                if (userInfo.Count == 0)
                {
                    us = new IC_UserInfo()
                    {
                        EmployeeATID = pEmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0'),
                        CompanyIndex = pCompanyIndex,
                        SerialNumber = "",
                        UserName = UserNameDevice,
                        Password = password != null ? StringHelper.SHA1(password) : "",
                        CardNumber = pCardNumber,
                        Privilege = 0,
                        Reserve1 = "",
                        Reserve2 = 0,
                        AuthenMode = AuthenMode.FullAccessRight.ToString(),
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
                        if (password != null)
                        {
                            item.Password = StringHelper.SHA1(password);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //context.SaveChanges();
        }

        private void CheckCardNumberInUserInfoImport(List<IC_UserMasterDTO> listEmp, int pCompanyIndex)
        {
            ConfigObject config = ConfigObject.GetConfig(cache);
            List<IC_UserInfo> listUserInfo = context.IC_UserInfo.Where(t => t.CompanyIndex == pCompanyIndex).ToList();
            foreach (IC_UserMasterDTO emp in listEmp)
            {
                IC_UserInfo userInfo = listUserInfo.Where(t => t.EmployeeATID == emp.EmployeeATID && t.SerialNumber == "").FirstOrDefault();
                if (userInfo == null)
                {
                    userInfo = new IC_UserInfo()
                    {
                        EmployeeATID = emp.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0'),
                        CompanyIndex = emp.CompanyIndex,
                        SerialNumber = "",
                        UserName = emp.NameOnMachine,
                        Password = "",
                        CardNumber = emp.CardNumber,
                        Privilege = 0,
                        Reserve1 = "",
                        Reserve2 = 0,
                        CreatedDate = DateTime.Now,
                        UpdatedUser = emp.UpdatedUser,
                        UpdatedDate = DateTime.Now
                    };
                    context.IC_UserInfo.Add(userInfo);
                }
                else
                {
                    userInfo.UserName = emp.NameOnMachine;
                    userInfo.CardNumber = emp.CardNumber;
                    userInfo.UpdatedDate = DateTime.Now;
                    userInfo.UpdatedUser = emp.UpdatedUser;
                }
            }

        }

        internal static List<IC_EmployeeLookupDTO> GetListEmployeeLookup(ConfigObject pConfig, int companyIndex, EPAD_Context context, ezHR_Context otherContext, string departmentCodeConfig, string departmentNameConfig)
        {
            var lstEmployeeLookup = new List<IC_EmployeeLookupDTO>();
            if (pConfig.IntegrateDBOther)
            {
                var lastWorking = otherContext.HR_WorkingInfo.AsEnumerable().Where(x => x.CompanyIndex == pConfig.CompanyIndex && x.FromDate <= DateTime.Now && (!x.ToDate.HasValue || x.ToDate >= DateTime.Now));
                lastWorking = lastWorking
                .OrderByDescending(n => n.FromDate)
                .GroupBy(n => n.EmployeeATID)
                .Select(n => n.FirstOrDefault());

                var output = from wi in lastWorking
                             join hd in otherContext.HR_Department.Where(x => x.CompanyIndex == pConfig.CompanyIndex)
                             on wi.DepartmentIndex equals hd.Index into hdc
                             from hd in hdc.AsEnumerable().DefaultIfEmpty()

                             join he in otherContext.HR_Employee.Where(x => x.CompanyIndex == pConfig.CompanyIndex)
                             on wi.EmployeeATID equals he.EmployeeATID into wic
                             from he in wic.AsEnumerable().DefaultIfEmpty()

                             select new IC_EmployeeLookupDTO()
                             {
                                 EmployeeATID = wi.EmployeeATID,
                                 DepartmentIndex = wi.DepartmentIndex,
                                 FullName = he != null ? $"{he.LastName} {he.MidName} {he.FirstName}" : "",
                                 Department = hd != null ? hd.Name : "",
                                 CardNumber = he != null ? he.CardNumber : "0",
                                 PositionName = "",
                                 IsDepartmentChildren = false
                             };
                lstEmployeeLookup = output.ToList();
            }
            else
            {
                var departments = context.IC_Department.Where(x => x.CompanyIndex == companyIndex && x.IsInactive != true);


                var output = from e in context.HR_User.Where(x => x.CompanyIndex == companyIndex)
                             join w in context.IC_WorkingInfo.Where(w => w.CompanyIndex == companyIndex
                             && w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date
                             && (!w.ToDate.HasValue || w.ToDate.Value.Date >= DateTime.Now.Date))
                             on e.EmployeeATID equals w.EmployeeATID
                             join d in departments
                             on w.DepartmentIndex equals d.Index into deptGroup
                             from dept in deptGroup.DefaultIfEmpty()
                             join p in context.HR_PositionInfo.Where(x => x.CompanyIndex == companyIndex)
                             on w.PositionIndex equals p.Index into position
                             from posi in position.DefaultIfEmpty()
                             join c in context.HR_CardNumberInfo.Where(x => x.CompanyIndex == companyIndex && x.IsActive == true)
                             on e.EmployeeATID equals c.EmployeeATID into card
                             from ci in card.DefaultIfEmpty()
                             select new IC_EmployeeLookupDTO()
                             {
                                 EmployeeATID = e.EmployeeATID,
                                 FullName = e.FullName,
                                 DepartmentIndex = w.DepartmentIndex,
                                 Department = dept.Name,
                                 PositionName = posi.Name,
                                 CardNumber = ci.CardNumber,
                                 PositionIndex = w.PositionIndex,
                             };
                lstEmployeeLookup = output.ToList();
            }
            return lstEmployeeLookup;
        }
    }

}

