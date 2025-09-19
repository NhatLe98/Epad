using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using EPAD_Backend_Core.Models.DTOs;
using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EPAD_Logic
{

    public class IC_EmployeeLogic : IIC_EmployeeLogic
    {
        private EPAD_Context _dbContext;
        private IMemoryCache _iCache;
        private ezHR_Context _ezHR_Context;
        IConfiguration _configuration;
        private ConfigObject _config;
        private IIC_EmployeeTransferLogic _IIC_EmployeeTransferLogic;
        private IIC_WorkingInfoLogic _iC_WorkingInfoLogic;
        private readonly ILogger _logger;
        private readonly string _configClientName;

        public IC_EmployeeLogic(EPAD_Context dbContext, ezHR_Context ezHR_Context, IMemoryCache cache, IIC_EmployeeTransferLogic iIC_EmployeeTransferLogic,
            IIC_WorkingInfoLogic iC_WorkingInfoLogic, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _iCache = cache;
            _config = ConfigObject.GetConfig(_iCache);
            _IIC_EmployeeTransferLogic = iIC_EmployeeTransferLogic;
            _iC_WorkingInfoLogic = iC_WorkingInfoLogic;
            _logger = loggerFactory.CreateLogger<IC_EmployeeLogic>();
            _configClientName = configuration.GetValue<string>("ClientName").ToUpper();
            _ezHR_Context = ezHR_Context;
        }

        public List<IC_EmployeeLookupDTO> GetListEmployeeLookup(ConfigObject pConfig, UserInfo pUser)
        {
            var lstEmployeeLookup = new List<IC_EmployeeLookupDTO>();
            if (pConfig.IntegrateDBOther)
            {
                var lastWorking = _ezHR_Context.HR_WorkingInfo.AsEnumerable().Where(x => x.CompanyIndex == pConfig.CompanyIndex && x.FromDate <= DateTime.Now && (!x.ToDate.HasValue || x.ToDate >= DateTime.Now));
                lastWorking = lastWorking
                .OrderByDescending(n => n.FromDate)
                .GroupBy(n => n.EmployeeATID)
                .Select(n => n.FirstOrDefault());

                var output = from wi in lastWorking
                             join hd in _ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pConfig.CompanyIndex)
                             on wi.DepartmentIndex equals hd.Index into hdc
                             from hd in hdc.AsEnumerable().DefaultIfEmpty()

                             join he in _ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pConfig.CompanyIndex)
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
                var departments = _dbContext.IC_Department.Where(x => x.CompanyIndex == pUser.CompanyIndex && x.IsInactive != true);


                var output = from e in _dbContext.HR_User.Where(x => x.CompanyIndex == pUser.CompanyIndex)
                             join w in _dbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pUser.CompanyIndex
                             && w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date && (!w.ToDate.HasValue || w.ToDate.Value.Date >= DateTime.Now.Date))
                             on e.EmployeeATID equals w.EmployeeATID
                             join d in departments
                             on w.DepartmentIndex equals d.Index into deptGroup
                             from dept in deptGroup.DefaultIfEmpty()
                             join p in _dbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pUser.CompanyIndex)
                             on w.PositionIndex equals p.Index into position
                             from posi in position.DefaultIfEmpty()
                             join c in _dbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pUser.CompanyIndex && x.IsActive == true)
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

        public async Task<HR_Employee> GetByEmployeeATIDAndCompanyIndex(string employeeATID, int companyIndex)
        {
            var employee = await _dbContext.HR_User.FirstOrDefaultAsync(e => e.EmployeeATID == employeeATID && e.CompanyIndex == companyIndex);
            var image = await _dbContext.IC_UserMaster.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == companyIndex);

            var dummy = new HR_Employee()
            {
                //CardNumber = employee.CardNumber,
                CompanyIndex = employee.CompanyIndex,
                EmployeeATID = employee.EmployeeATID,
                EmployeeCode = employee.EmployeeCode,
                LastName = employee.FullName,
                FirstName = "",
                MidName = "",
                Gender = employee.Gender != null && employee.Gender.Value == 1,
                Avatar = image?.FaceV2_Content ?? ""
            };
            return dummy;
        }

        public async Task<List<EmployeeFullInfo>> GetEmployeeCompactInfo(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new List<EmployeeFullInfo>();

            var query = (from e in _dbContext.HR_User
                         join wi in _dbContext.IC_WorkingInfo
                         on e.EmployeeATID equals wi.EmployeeATID into eWork
                         from eWorkResult in eWork.DefaultIfEmpty()

                         join d in _dbContext.IC_Department
                         on eWorkResult.DepartmentIndex equals d.Index into dWork
                         from dWorkResult in dWork.DefaultIfEmpty()

                         select new EmployeeFullInfo
                         {
                             CompanyIndex = e.CompanyIndex,
                             EmployeeATID = e.EmployeeATID,
                             EmployeeCode = e.EmployeeCode,
                             Avatar = null,
                             Department = dWorkResult.Name,
                             LastName = e.FullName,
                             FirstName = "",
                             MidName = "",
                             Gender = e.Gender != null && e.Gender.Value == 1,
                             //CardNumber = e.CardNumber,
                             DepartmentIndex = dWorkResult.Index,
                             FromDate = eWorkResult.FromDate,
                             ToDate = eWorkResult.ToDate,

                         }).AsQueryable();

            DateTime maxToday = CommonUtils.GetMaxToday();
            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(param.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "DepartmentIndex":
                            if (param.Value != null)
                            {
                                int departmentID = Convert.ToInt32(param.Value);
                                query = query.Where(u => u.DepartmentIndex.HasValue && u.DepartmentIndex.Value == departmentID);
                            }
                            break;
                        case "ListDepartment":
                            if (param.Value != null)
                            {
                                IList<long> departments = (IList<long>)param.Value;
                                query = query.Where(u => u.DepartmentIndex.HasValue && departments.Contains(u.DepartmentIndex.Value));
                            }
                            break;
                        case "ListEmployeeATID":
                            if (param.Value != null)
                            {
                                IList<string> listEmployeeID = (IList<string>)param.Value;
                                query = query.Where(u => listEmployeeID.Contains(u.EmployeeATID));
                            }
                            break;
                        case "EmployeeATID":
                            if (param.Value != null)
                            {
                                string employeeID = param.Value.ToString();
                                query = query.Where(u => u.EmployeeATID == employeeID);
                            }
                            break;
                    }
                }
            }
            var data = await query.ToListAsync();
            return data;
        }

        public List<IC_EmployeeDTO> GetEmployeeList(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new List<IC_EmployeeDTO>();

            var addparamEmployeeTranfer = addedParams.FirstOrDefault(e => e.Key == "ListEmployeeTransferATID");


            string select = "SELECT distinct ";
            select += "\n" + "e.CompanyIndex, e.EmployeeATID,e.EmployeeCode, e.Nric, e.Avatar, e.FullName,e.Gender, ISNULL(c.CardNumber, '') as CardNumber,ISNULL(us.NameOnMachine, '') as NameOnMachine, ";
            select += "\n" + "w.[Index] as WorkingInfoIndex, w.Status as TransferStatus,w.FromDate,w.ToDate,w.IsSync, w.DepartmentIndex,w.ToDate as StoppedDate, ";
            select += "\n" + "d.[Name] AS DepartmentName, d.[Code] AS DepartmentCode, p.[Name] AS PositionName ";


            string from = "\n" + "FROM dbo.HR_User e ";
            from += "\n" + "LEFT JOIN dbo.HR_CardNumberInfo c ON e.EmployeeATID = c.EmployeeATID AND e.CompanyIndex = c.CompanyIndex  AND c.IsActive = 1 ";
            from += "\n" + "LEFT JOIN dbo.IC_UserMaster us ON e.EmployeeATID = us.EmployeeATID AND e.CompanyIndex = us.CompanyIndex ";
            from += "\n" + "LEFT JOIN dbo.IC_WorkingInfo w ON w.EmployeeATID = e.EmployeeATID AND w.CompanyIndex = e.CompanyIndex ";
            from += "\n" + "LEFT JOIN dbo.IC_Department d ON w.DepartmentIndex = d.[Index] AND w.CompanyIndex = d.CompanyIndex ";
            from += "\n" + "LEFT JOIN dbo.IC_UserMaster u ON e.EmployeeATID = u.EmployeeATID AND e.CompanyIndex = u.CompanyIndex ";
            from += "\n" + "LEFT JOIN HR_PositionInfo p on p.[Index] = w.PositionIndex and p.CompanyIndex = w.CompanyIndex ";

            string where = "\n" + $"WHERE (w.ToDate is null OR Datediff(day, w.ToDate, GetDate()) <= 0) AND ";

            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    if (param.Value != null)
                    {
                        switch (param.Key)
                        {
                            case "CompanyIndex":
                                int companyIndex = Convert.ToInt32(param.Value);
                                where += " e.CompanyIndex = " + companyIndex.ToString() + " AND ";
                                break;
                            case "TransferStatus":
                                int approveStatus = Convert.ToInt32(param.Value);
                                where += " e.TransferStatus = " + approveStatus.ToString() + " AND ";
                                break;
                            case "DepartmentIndex":
                                int departmentID = Convert.ToInt32(param.Value);
                                where += "w.DepartmentIndex = " + departmentID + " AND ";
                                break;
                            case "ListDepartment":
                                IList<long> departments = (IList<long>)param.Value;
                                if (addparamEmployeeTranfer != null && addparamEmployeeTranfer.Value != null)
                                {
                                    IList<string> listEmployeeID = (IList<string>)addparamEmployeeTranfer.Value;
                                    where += "(w.DepartmentIndex IN (" + string.Join(",", departments) + ") OR e.EmployeeATID IN ('" + string.Join("','", listEmployeeID) + "')) AND ";
                                }
                                else
                                {
                                    where += "w.DepartmentIndex IN (" + string.Join(",", departments) + ") AND ";
                                }
                                break;
                            case "ListEmployeeATID":
                                if (!string.IsNullOrWhiteSpace(param.Value.ToString()))
                                {
                                    IList<string> listEmployeeID = (IList<string>)param.Value;
                                    where += "e.EmployeeATID IN ('" + string.Join("','", listEmployeeID) + "') AND ";
                                }
                                break;

                            case "EmployeeATID":
                                string employeeID = param.Value.ToString();
                                where += "e.EmployeeATID = '" + employeeID + "' AND ";
                                break;
                            case "IsSync":
                                bool isSync = Convert.ToBoolean(param.Value);
                                where += " e.IsSync = " + isSync.ToString() + " AND ";
                                break;
                            case "IsCurrentWorking":
                                where += "(w.ToDate is null and Datediff(day, w.FromDate, GetDate()) >= 0 and w.Status = 1) OR (Datediff(day, w.ToDate, GetDate()) <= 0 AND Datediff(day, w.FromDate, GetDate()) >= 0) and w.Status = 1) AND ";
                                break;
                            case "IsCurrentWorkingAndNoDepartment":
                                where += "((w.ToDate is null and Datediff(day, w.FromDate, GetDate()) >= 0 and w.Status = 1) OR ((Datediff(day, w.ToDate, GetDate()) <= 0 AND Datediff(day, w.FromDate, GetDate()) >= 0) and w.Status = 1) OR  (w.DepartmentIndex is null OR w.DepartmentIndex = 0) ) AND ";
                                break;
                        }
                    }
                }
            }

            where = where.Substring(0, where.LastIndexOf("AND"));
            var query = select + from + where;
            string conn = _configuration.GetConnectionString("connectionString");
            var resutlData = new List<IC_EmployeeDTO>();
            using (var connection = new SqlConnection(conn))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.CommandTimeout = 0;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var dataTable = new DataTable();
                            dataTable.Load(reader);
                            var index = 0;
                            foreach (DataRow dtRow in dataTable.Rows)
                            {
                                // On all tables' columns
                                var dataRow = new IC_EmployeeDTO();
                                dataRow.Index = index++;
                                dataRow.CompanyIndex = (dtRow["CompanyIndex"] is DBNull) ? 0 : Convert.ToInt32(dtRow["CompanyIndex"].ToString());
                                dataRow.WorkingInfoIndex = (dtRow["WorkingInfoIndex"] is DBNull) ? (long?)null : Convert.ToInt64(dtRow["WorkingInfoIndex"].ToString());
                                dataRow.EmployeeATID = (dtRow["EmployeeATID"] is DBNull) ? null : dtRow["EmployeeATID"].ToString();
                                dataRow.DepartmentIndex = (dtRow["DepartmentIndex"] is DBNull) ? 0 : Convert.ToInt32(dtRow["DepartmentIndex"].ToString());
                                dataRow.EmployeeCode = (dtRow["EmployeeCode"] is DBNull) ? null : dtRow["EmployeeCode"].ToString();
                                dataRow.FullName = (dtRow["FullName"] is DBNull) ? null : dtRow["FullName"].ToString();
                                dataRow.Gender = (dtRow["Gender"] is DBNull) ? (short?)null : Convert.ToInt16(dtRow["Gender"]);
                                dataRow.TransferStatus = (dtRow["TransferStatus"] is DBNull) ? (short?)null : Convert.ToInt16(dtRow["TransferStatus"]);
                                dataRow.DepartmentName = (dtRow["DepartmentName"] is DBNull) ? "Không có phòng ban" : dtRow["DepartmentName"].ToString();
                                dataRow.DepartmentCode = (dtRow["DepartmentCode"] is DBNull) ? "NoDepartment" : dtRow["DepartmentCode"].ToString();
                                dataRow.NameOnMachine = (dtRow["NameOnMachine"] is DBNull) ? null : dtRow["NameOnMachine"].ToString();
                                dataRow.CardNumber = (dtRow["CardNumber"] is DBNull) ? null : dtRow["CardNumber"].ToString();
                                dataRow.IsSync = (dtRow["IsSync"] is DBNull) ? (bool?)null : Convert.ToBoolean(dtRow["IsSync"]);
                                dataRow.FromDate = (dtRow["FromDate"] is DBNull) ? (DateTime?)null : DateTime.Parse(dtRow["FromDate"].ToString());
                                dataRow.ToDate = (dtRow["ToDate"] is DBNull) ? (DateTime?)null : DateTime.Parse(dtRow["ToDate"].ToString());
                                dataRow.StoppedDate = (dtRow["StoppedDate"] is DBNull) ? (DateTime?)null : DateTime.Parse(dtRow["StoppedDate"].ToString());
                                dataRow.PositionName = (dtRow["PositionName"] is DBNull) ? null : dtRow["PositionName"].ToString();
                                dataRow.Avatar = dataTable.Rows.Count == 1 ? ((dtRow["Avatar"] is DBNull) ? "" : StringHelper.ConvertByteArrayToBaseString((byte[])dtRow["Avatar"])) : "";
                                dataRow.NRIC = (dtRow["Nric"] is DBNull) ? null : dtRow["Nric"].ToString();
                                resutlData.Add(dataRow);
                            }
                        }
                    }
                }
            }

            return resutlData;
        }


        public List<IC_EmployeeDTO> GetManyStoppedWorking(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new List<IC_EmployeeDTO>();

            var addparamEmployeeTranfer = addedParams.FirstOrDefault(e => e.Key == "ListEmployeeTransferATID");

            var query = (
                         from e in _dbContext.HR_User
                         join wi in _dbContext.IC_WorkingInfo
                         on e.EmployeeATID equals wi.EmployeeATID into eWork
                         from eWorkResult in eWork.DefaultIfEmpty()

                         join wii in _dbContext.IC_WorkingInfo.Where(x => x.ToDate == null && x.Status == (short)TransferStatus.Approve)
                        on e.EmployeeATID equals wii.EmployeeATID into eiWork
                         from eiWorkResult in eiWork.DefaultIfEmpty()

                         join d in _dbContext.IC_Department
                         on eWorkResult.DepartmentIndex equals d.Index into dWork
                         from dWorkResult in dWork.DefaultIfEmpty()
                         where eWorkResult.ToDate != null && (eWorkResult.ToDate.Value.Date == DateTime.Now.Date
                         || eWorkResult.ToDate.Value.Date == DateTime.Now.Date.AddDays(-1)
                         )
                            && eWorkResult.Status == (short)TransferStatus.Approve
                            && eWorkResult.DepartmentIndex != eiWorkResult.DepartmentIndex
                         select new IC_EmployeeDTO
                         {
                             WorkingInfoIndex = eWorkResult.Index,
                             CompanyIndex = e.CompanyIndex,
                             EmployeeATID = e.EmployeeATID,
                             DepartmentIndex = dWorkResult.Index,
                             DepartmentName = dWorkResult.Name,
                             TransferStatus = eWorkResult.Status,
                             StoppedDate = eWorkResult.ToDate
                         }).AsQueryable();

            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "Filter":
                            if (param.Value != null)
                            {
                                string filter = param.Value.ToString();
                                query = query.Where(u => u.EmployeeATID.Contains(filter)
                                    || u.DepartmentName.Contains(filter)
                                    || u.FullName.Contains(filter)
                                    || u.CardNumber.Contains(filter)
                                    || (!string.IsNullOrEmpty(u.EmployeeCode) && u.EmployeeCode.Contains(filter)));
                            }
                            break;
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(param.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "IsCurrentWorking":
                            if (param.Value != null)
                            {
                                query = query.Where(u => u.FromDate.Value.Date <= DateTime.Now.Date && (!u.ToDate.HasValue || u.ToDate.Value.Date >= DateTime.Now.Date)
                                    && u.TransferStatus == (short)TransferStatus.Approve);
                            }
                            break;
                        case "IsCurrentWorkingAndNoDepartment":
                            if (param.Value != null)
                            {
                                query = query.Where(u => (u.FromDate.Value.Date <= DateTime.Now.Date && (!u.ToDate.HasValue || u.ToDate.Value.Date >= DateTime.Now.Date)
                                    && u.TransferStatus == (short)TransferStatus.Approve) || u.DepartmentIndex == 0 || u.DepartmentIndex == null);
                            }
                            break;
                        case "DepartmentIndex":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                int departmentIndex = Convert.ToInt32(param.Value);
                                if (departmentIndex == 0)
                                {
                                    query = query.Where(u => u.DepartmentIndex == null || u.DepartmentIndex == departmentIndex);
                                }
                                else
                                {
                                    query = query.Where(u => u.DepartmentIndex.HasValue && u.DepartmentIndex == departmentIndex);
                                }
                            }
                            break;
                        case "ListDepartment":
                            if (param.Value != null)
                            {
                                IList<long> departments = (IList<long>)param.Value;
                                if (addparamEmployeeTranfer != null && addparamEmployeeTranfer.Value != null)
                                {
                                    IList<string> listEmployeeATID = (IList<string>)addparamEmployeeTranfer.Value;
                                    query = query.Where(u => u.DepartmentIndex.HasValue && departments.Contains(u.DepartmentIndex.Value)
                                        || listEmployeeATID.Contains(u.EmployeeATID) || u.DepartmentIndex == null || u.DepartmentIndex == 0);
                                }
                                else
                                {
                                    query = query.Where(u => u.DepartmentIndex.HasValue && departments.Contains(u.DepartmentIndex.Value)
                                        || u.DepartmentIndex == null || u.DepartmentIndex == 0);
                                }
                            }
                            break;
                        case "TransferStatus":
                            if (param.Value != null)
                            {
                                int approveStatus = Convert.ToInt32(param.Value);
                                query = query.Where(u => u.TransferStatus == approveStatus);
                            }
                            break;
                    }
                }
            }

            query = query.OrderBy(u => u.EmployeeATID);

            var data = query.ToList();
            return data;
        }

        public List<IC_EmployeeDTO> GetManyExportEmployee(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new List<IC_EmployeeDTO>();

            var addparamEmployeeTranfer = addedParams.FirstOrDefault(e => e.Key == "ListEmployeeTransferATID");
            var queryParams = addedParams.FirstOrDefault(x => x.Key == "Filter").Value;

            if (_config.IntegrateDBOther == false)
            {
                string filter = "";
                var filterBy = new List<string>();
                if (queryParams != null)
                {
                    filter = queryParams.ToString();
                    filterBy = filter.Split(" ").ToList();
                }
                var hrusers = _dbContext.HR_User.Where(x => x.CompanyIndex == 2 && (x.EmployeeType == null || x.EmployeeType == (int)EmployeeType.Employee) && (queryParams == null || (x.EmployeeATID.Contains(filter)
                                || x.FullName.Contains(filter)
                                || (filterBy.Count > 0 && (filterBy.Contains(x.EmployeeATID) || filterBy.Count == 0))
                                || (!string.IsNullOrEmpty(x.EmployeeCode) && x.EmployeeCode.Contains(filter)))));
                var query = (from u in hrusers
                             join e in _dbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == 2)
                             on u.EmployeeATID equals e.EmployeeATID into eCheck
                             from eResult in eCheck.DefaultIfEmpty()

                             join ut in _dbContext.HR_UserType.Where(x => x.CompanyIndex == 2 && x.StatusId != (byte)RowStatus.Inactive)
                             on u.EmployeeType equals ut.UserTypeId into uType
                             from utResult in uType.DefaultIfEmpty()

                             join et in _dbContext.IC_EmployeeType.Where(x => x.CompanyIndex == 2
                                    && x.IsUsing)
                                 on u.EmployeeTypeIndex equals et.Index into eType
                             from etResult in eType.DefaultIfEmpty()

                             join wi in _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == 2)
                             on eResult.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()

                             join d in _dbContext.IC_Department.Where(x => x.CompanyIndex == 2 && x.IsInactive != true)
                             on eWorkResult.DepartmentIndex equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()

                             join d in _dbContext.HR_PositionInfo.Where(x => x.CompanyIndex == 2)
                             on eWorkResult.PositionIndex equals d.Index into dPos
                             from ePositionResult in dPos.DefaultIfEmpty()

                             join c in _dbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == 2 && x.IsActive == true)
                             on eResult.EmployeeATID equals c.EmployeeATID into cWork
                             from cResult in cWork.DefaultIfEmpty()

                             join us in _dbContext.IC_UserMaster.Where(x => x.CompanyIndex == 2)
                             on u.EmployeeATID equals us.EmployeeATID into usWork
                             from usWorkResult in usWork.DefaultIfEmpty()

                             select new
                             {
                                 User = u,
                                 UserType = utResult,
                                 EmployeeType = etResult,
                                 Employee = eResult,
                                 WorkingInfo = eWorkResult,
                                 Department = dWorkResult,
                                 CardInfo = cResult,
                                 UserMaster = usWorkResult,
                                 Position = ePositionResult
                             });

                if (addedParams != null)
                {
                    foreach (AddedParam param in addedParams)
                    {
                        switch (param.Key)
                        {
                            case "Filter":
                                if (param.Value != null)
                                {

                                    //query = query.Where(u =>

                                    //(u.CardInfo != null && u.CardInfo.CardNumber.Contains(filter)));

                                }
                                break;
                            case "DepartmentIndex":
                                if (param.Value != null)
                                {
                                    int departmentIndex = Convert.ToInt32(param.Value);
                                    query = query.Where(u => (u.WorkingInfo.DepartmentIndex) == departmentIndex);
                                }
                                break;
                            case "ListDepartment":
                                if (param.Value != null)
                                {
                                    IList<long> departments = (IList<long>)param.Value;
                                    query = query.Where(u => u.WorkingInfo != null && departments.Contains(u.WorkingInfo.DepartmentIndex));
                                }
                                break;
                            case "ListEmployeeATID":
                                if (param.Value != null)
                                {
                                    IList<string> listEmployeeID = JsonConvert.DeserializeObject<IList<string>>(param.Value.ToString());
                                    query = query.Where(u => listEmployeeID.Contains(u.User.EmployeeATID));
                                }
                                break;
                            case "EmployeeATID":
                                if (param.Value != null)
                                {
                                    string employeeID = param.Value.ToString();
                                    query = query.Where(u => u.User.EmployeeATID == employeeID);
                                }
                                break;
                            case "ApproveStatus":
                                if (param.Value != null)
                                {
                                    long approveStatus = Convert.ToInt64(param.Value.ToString());
                                    query = query.Where(u => u.WorkingInfo.Status == approveStatus);
                                }
                                break;
                            case "IsCurrentWorking":
                                if (param.Value != null)
                                {
                                    query = query.Where(u => (u.WorkingInfo.FromDate.Date <= DateTime.Now.Date
                                                              && (!u.WorkingInfo.ToDate.HasValue || u.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date)) || u.WorkingInfo.DepartmentIndex == 0);
                                }
                                break;

                            case "IsWorking":
                                if (param.Value != null)
                                {
                                    var listWorking = (IList<int>)param.Value;
                                    if (!listWorking.Contains((int)EmployeeStatusType.Working) || !listWorking.Contains((int)EmployeeStatusType.StopWorking))
                                    {
                                        if (listWorking.Contains((int)EmployeeStatusType.Working)) // working
                                        {
                                            query = query.Where(u => (u.WorkingInfo.FromDate.Date <= DateTime.Now.Date
                                                                      && (!u.WorkingInfo.ToDate.HasValue || u.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date))
                                                                     || u.WorkingInfo.DepartmentIndex == 0);
                                        }
                                        if (listWorking.Contains((int)EmployeeStatusType.StopWorking)) // stopped work
                                        {
                                            query = query.Where(u => (u.WorkingInfo.FromDate.Date <= DateTime.Now.Date
                                                                      && (u.WorkingInfo.ToDate.HasValue && u.WorkingInfo.ToDate.Value.Date <= DateTime.Now.Date)));
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }

                //query = query.OrderBy(u => u.EmployeeATID);

                var data = query.ToList().Select(x => new IC_EmployeeDTO
                {
                    WorkingInfoIndex = x.WorkingInfo?.Index,
                    CompanyIndex = x.User.CompanyIndex,
                    EmployeeATID = x.User.EmployeeATID,
                    EmployeeCode = x.User.EmployeeCode,
                    FullName = x.User.FullName,
                    Gender = x.User.Gender,
                    NameOnMachine = x.UserMaster?.NameOnMachine,
                    CardNumber = x.CardInfo?.CardNumber,
                    DepartmentIndex = x.WorkingInfo?.Index,
                    DepartmentName = x.Department?.Name,
                    TransferStatus = x.WorkingInfo?.Status,
                    ToDate = x.WorkingInfo?.ToDate,
                    FromDate = x.WorkingInfo?.FromDate,
                    UpdatedUser = x.WorkingInfo?.UpdatedUser,
                    UpdatedDate = x.WorkingInfo?.UpdatedDate,
                    JoinedDate = x.Employee.JoinedDate.HasValue ? x.Employee.JoinedDate.Value : DateTime.Now,
                    EmployeeTypeName = x?.EmployeeType?.Name,
                    BirthDay = new DateTime(
                    x.User.YearOfBirth != null ? x.User.YearOfBirth.Value != 0 ? x.User.YearOfBirth.Value : 1900 : 1900,
                    x.User.MonthOfBirth != null ? x.User.MonthOfBirth.Value != 0 ? x.User.MonthOfBirth.Value : 1 : 1,
                    x.User.DayOfBirth != null ? x.User.DayOfBirth.Value != 0 ? x.User.DayOfBirth.Value : 1 : 1).ToString("yyyy-MM-dd"),

                    PhoneNumber = x?.Employee?.Phone,
                    Email = x.Employee?.Email,
                    Address = x?.User?.Address,
                    IsAllowPhone = x.User.IsAllowPhone,
                    NRIC = x.User.Nric,
                    PositionName = x.Position?.Name
                    //ImageUpload = dMasterResult.FaceV2_Content

                }).OrderBy(x => x.EmployeeATID).ToList();
                return data;
            }
            else
            {

                var query = (from e in _ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == _config.CompanyIndex)
                             join wi in _ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == _config.CompanyIndex)
                             on e.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()

                             join d in _ezHR_Context.HR_Department.Where(x => x.CompanyIndex == _config.CompanyIndex)
                         on eWorkResult.DepartmentIndex equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()

                             join p in _ezHR_Context.HR_Position.Where(x => x.CompanyIndex == _config.CompanyIndex)
                             on eWorkResult.PositionIndex equals p.Index into pWork
                             from pWorkResult in pWork.DefaultIfEmpty()

                             join t in _ezHR_Context.HR_Titles.Where(x => x.CompanyIndex == _config.CompanyIndex)
                             on eWorkResult.TitlesIndex equals t.Index into tWork
                             from tWorkResult in tWork.DefaultIfEmpty()

                             join s in _ezHR_Context.HR_EmployeeStoppedWorkingInfo.Where(x => x.CompanyIndex == _config.CompanyIndex && x.ReturnedDate == null
                                 && x.StartedDate.Value.Date <= DateTime.Now)
                              on eWorkResult.EmployeeATID equals s.EmployeeATID into sWork
                             from sWorkResult in sWork.DefaultIfEmpty()

                             where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec
                               && sWorkResult.EmployeeATID == null
                             select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName }).AsQueryable();
                if (addedParams != null)
                {
                    foreach (AddedParam param in addedParams)
                    {
                        if (param.Value != null)
                        {
                            switch (param.Key)
                            {
                                case "Filter":
                                    string filter = param.Value.ToString();
                                    var filterBy = filter.Split(",").ToList();
                                    query = query.Where(u => u.EmployeeATID.Contains(filter)
                                    || (u.Department != null && u.Department.Name.Contains(filter))
                                    || u.FullName.Contains(filter)
                                    || (u.Employee.CardNumber != null && u.Employee.CardNumber.Contains(filter))
                                    || (!string.IsNullOrEmpty(u.Employee.EmployeeCode) && u.Employee.EmployeeCode.Contains(filter))
                                    || ((filterBy.Count > 0 && (filterBy.Contains(u.EmployeeATID) || filterBy.Contains(u.FullName))) || filterBy.Count == 0)
                                    );
                                    break;
                                case "DepartmentIndex":
                                    int departmentIndex = Convert.ToInt32(param.Value);
                                    query = query.Where(u => (u.WorkingInfo.DepartmentIndex) == departmentIndex);
                                    break;
                                case "ListDepartment":
                                    IList<long> departments = (IList<long>)param.Value;
                                    query = query.Where(u => u.WorkingInfo != null && u.WorkingInfo.DepartmentIndex.HasValue && departments.Contains(u.WorkingInfo.DepartmentIndex.Value));
                                    break;
                                case "ListEmployeeATID":
                                    IList<string> listEmployeeID = (IList<string>)param.Value;
                                    query = query.Where(u => listEmployeeID.Contains(u.EmployeeATID));
                                    break;
                                case "EmployeeATID":
                                    string employeeID = param.Value.ToString();
                                    query = query.Where(u => u.EmployeeATID == employeeID);
                                    break;
                                case "IsCurrentWorking":
                                    query = query.Where(u => (u.WorkingInfo.FromDate <= DateTime.Now.Date
                                        && (!u.WorkingInfo.ToDate.HasValue || u.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date)) || u.WorkingInfo.DepartmentIndex == 0);
                                    break;
                                case "FromDate":
                                    var fromDate = (DateTime)param.Value;
                                    query = query.Where(u => u.WorkingInfo.FromDate.HasValue
                                        && u.WorkingInfo.FromDate.Value.Date >= fromDate.Date);
                                    break;
                                case "ToDate":

                                    var toDate = (DateTime)param.Value;
                                    query = query.Where(u => u.WorkingInfo.FromDate.HasValue
                                        && u.WorkingInfo.FromDate.Value.Date <= toDate.Date);
                                    break;
                            }
                        }
                    }
                }
                var result = query.AsEnumerable().Select(x => new IC_EmployeeDTO
                {
                    WorkingInfoIndex = x.WorkingInfo?.Index,
                    CompanyIndex = x.Employee.CompanyIndex,
                    EmployeeATID = x.Employee.EmployeeATID,
                    EmployeeCode = x.Employee.EmployeeCode,
                    FullName = x.FullName,
                    Gender = x.Employee.Gender == true ? (short)GenderEnum.Male : (short)GenderEnum.Female,

                    CardNumber = x.Employee.CardNumber,
                    DepartmentIndex = x.WorkingInfo?.Index,
                    DepartmentName = x.Department?.Name,
                    ToDate = x.WorkingInfo?.ToDate,
                    FromDate = x.WorkingInfo?.FromDate,
                    UpdatedUser = x.WorkingInfo?.UpdatedUser,
                    UpdatedDate = x.WorkingInfo?.UpdatedDate,
                    JoinedDate = x.Employee.JoinedDate.HasValue ? x.Employee.JoinedDate.Value : DateTime.Now
                    //ImageUpload = dMasterResult.FaceV2_Content

                }).OrderBy(x => x.EmployeeATID).ToList();

                return result;
            }


        }

        public List<IC_EmployeeDTO> GetManyExport(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new List<IC_EmployeeDTO>();

            var addparamEmployeeTranfer = addedParams.FirstOrDefault(e => e.Key == "ListEmployeeTransferATID");

            string filter = "";
            var filterBy = new List<string>();
            var queryParams = addedParams.FirstOrDefault(x => x.Key == "Filter").Value;
            if (queryParams != null)
            {
                filter = queryParams.ToString();
                filterBy = filter.Split(" ").ToList();
            }
            var hrusers = _dbContext.HR_User.Where(x => x.CompanyIndex == 2 && (queryParams == null || (x.EmployeeATID.Contains(filter)
                                 || x.FullName.Contains(filter)
                                 || (filterBy.Count > 0 && (filterBy.Contains(x.EmployeeATID) || filterBy.Count == 0))
                                 || (!string.IsNullOrEmpty(x.EmployeeCode) && x.EmployeeCode.Contains(filter)))));
            var query = (from u in hrusers
                         join e in _dbContext.HR_CustomerInfo.Where(x => x.CompanyIndex == 2)
                        on u.EmployeeATID equals e.EmployeeATID into eCheck
                         from eResult in eCheck.DefaultIfEmpty()

                         join ut in _dbContext.HR_UserType.Where(x => x.CompanyIndex == 2 && x.StatusId != (byte)RowStatus.Inactive)
                         on u.EmployeeType equals ut.UserTypeId into uType
                         from utResult in uType.DefaultIfEmpty()

                         join c in _dbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == 2 && x.IsActive == true)
                         on eResult.EmployeeATID equals c.EmployeeATID into cWork
                         from cResult in cWork.DefaultIfEmpty()

                         join wi in _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == 2)
                             on u.EmployeeATID equals wi.EmployeeATID into eWork
                         from eWorkResult in eWork.DefaultIfEmpty()

                         join d in _dbContext.IC_Department.Where(x => x.CompanyIndex == 2 && x.IsInactive != true)
                             on eWorkResult.DepartmentIndex equals d.Index into dWork
                         from dWorkResult in dWork.DefaultIfEmpty()

                         join p in _dbContext.HR_PositionInfo.Where(x => x.CompanyIndex == 2)
                             on eWorkResult.PositionIndex equals p.Index into pWork
                         from pWorkResult in pWork.DefaultIfEmpty()

                         join us in _dbContext.IC_UserMaster.Where(x => x.CompanyIndex == 2)
                       on u.EmployeeATID equals us.EmployeeATID into usWork
                         from usWorkResult in usWork.DefaultIfEmpty()

                         select new
                         {
                             User = u,
                             UserType = utResult,
                             Employee = eResult,
                             Department = dWorkResult,
                             CardInfo = cResult,
                             UserMaster = usWorkResult,
                             WorkingInfo = eWorkResult,
                             Position = pWorkResult
                         });

            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "Filter":
                            if (param.Value != null)
                            {

                                //query = query.Where(u =>

                                //(u.CardInfo != null && u.CardInfo.CardNumber.Contains(filter)));

                            }
                            break;
                        case "DepartmentIndex":
                            if (param.Value != null)
                            {
                                int departmentIndex = Convert.ToInt32(param.Value);
                                //query = query.Where(u => (u.WorkingInfo.DepartmentIndex) == departmentIndex);
                            }
                            break;
                        case "ListDepartment":
                            if (param.Value != null)
                            {
                                IList<long> departments = (IList<long>)param.Value;
                                //query = query.Where(u => u.WorkingInfo != null && departments.Contains(u.WorkingInfo.DepartmentIndex));
                            }
                            break;
                        case "ListEmployeeATID":
                            if (param.Value != null)
                            {
                                IList<string> listEmployeeID = JsonConvert.DeserializeObject<IList<string>>(param.Value.ToString());
                                query = query.Where(u => listEmployeeID.Contains(u.User.EmployeeATID));
                            }
                            break;
                        case "EmployeeATID":
                            if (param.Value != null)
                            {
                                string employeeID = param.Value.ToString();
                                query = query.Where(u => u.User.EmployeeATID == employeeID);
                            }
                            break;
                        case "ApproveStatus":
                            if (param.Value != null)
                            {
                                long approveStatus = Convert.ToInt64(param.Value.ToString());
                                //query = query.Where(u => u.WorkingInfo.Status == approveStatus);
                            }
                            break;
                        case "IsCurrentWorking":
                            if (param.Value != null)
                            {
                                //query = query.Where(u => (u.WorkingInfo.FromDate.Date <= DateTime.Now.Date
                                //                          && (!u.WorkingInfo.ToDate.HasValue || u.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date)) || u.WorkingInfo.DepartmentIndex == 0);
                            }
                            break;
                        case "UserType":
                            if (param.Value != null)
                            {
                                query = query.Where(u => (u.User.EmployeeType != null && u.User.EmployeeType == (int)param.Value) || (u.UserType == null && (int)param.Value == 1));
                            }
                            break;
                        case "IsWorking":
                            if (param.Value != null)
                            {
                                //var listWorking = (IList<int>)param.Value;
                                //if (!listWorking.Contains((int)EmployeeStatusType.Working) || !listWorking.Contains((int)EmployeeStatusType.StopWorking))
                                //{
                                //    if (listWorking.Contains((int)EmployeeStatusType.Working)) // working
                                //    {
                                //        query = query.Where(u => (u.WorkingInfo.FromDate.Date <= DateTime.Now.Date
                                //                                  && (!u.WorkingInfo.ToDate.HasValue || u.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date))
                                //                                 || u.WorkingInfo.DepartmentIndex == 0);
                                //    }
                                //    if (listWorking.Contains((int)EmployeeStatusType.StopWorking)) // stopped work
                                //    {
                                //        query = query.Where(u => (u.WorkingInfo.FromDate.Date <= DateTime.Now.Date
                                //                                  && (u.WorkingInfo.ToDate.HasValue && u.WorkingInfo.ToDate.Value.Date <= DateTime.Now.Date)));
                                //    }
                                //}
                            }
                            break;
                    }
                }
            }

            //query = query.OrderBy(u => u.EmployeeATID);
            var data = query.ToList().Select(x => new IC_EmployeeDTO
            {
                //WorkingInfoIndex = x?.WorkingInfo?.Index,
                CompanyIndex = x.User.CompanyIndex,
                EmployeeATID = x.User.EmployeeATID,
                EmployeeCode = x.User.EmployeeCode,
                FullName = x.User.FullName,
                Gender = x.User.Gender,
                DepartmentName = x?.Department?.Name,
                NameOnMachine = x.UserMaster?.NameOnMachine,
                CardNumber = x.CardInfo?.CardNumber,
                BirthDay = new DateTime(
                    x.User.YearOfBirth != null ? x.User.YearOfBirth.Value != 0 ? x.User.YearOfBirth.Value : 1900 : 1900,
                    x.User.MonthOfBirth != null ? x.User.MonthOfBirth.Value != 0 ? x.User.MonthOfBirth.Value : 1 : 1,
                    x.User.DayOfBirth != null ? x.User.DayOfBirth.Value != 0 ? x.User.DayOfBirth.Value : 1 : 1).ToString("yyyy-MM-dd"),

                PhoneNumber = x?.Employee?.Phone,
                Email = x.Employee?.Email,
                Address = x?.Employee?.Address,
                IsAllowPhone = x.Employee?.IsAllowPhone ?? false,
                NRIC = x.Employee?.NRIC,
                //DepartmentIndex = x.WorkingInfo?.Index,
                //DepartmentName = x.Department?.Name,
                //TransferStatus = x.WorkingInfo?.Status,
                ToDate = x?.WorkingInfo?.ToDate,
                JoinedDate = x.WorkingInfo != null ? x.WorkingInfo.FromDate : new DateTime(),
                //UpdatedUser = x.WorkingInfo?.UpdatedUser,
                //UpdatedDate = x.WorkingInfo?.UpdatedDate,
                //JoinedDate = x.Employee.JoinedDate.HasValue ? x.Employee.JoinedDate.Value : DateTime.Now
                //ImageUpload = dMasterResult.FaceV2_Content
                FromDateStr = x.Employee?.FromTime.ToddMMyyyy(),
                ToDateStr = x.Employee?.ToTime.ToddMMyyyy(),
                FromTime = x.Employee?.FromTime.ToHHmmss(),
                ToTime = x.Employee?.ToTime.ToHHmmss(),
                WorkingContent = x?.Employee?.WorkingContent,
                Note = x?.Employee?.Note,
                ContactDepartment = x?.Employee?.ContactDepartment,
                ContactPerson = x?.Employee?.ContactPerson,
                CompanyName = x?.Employee?.Company,
                PositionName = x?.Position?.Name ?? string.Empty,

            }).OrderBy(x => x.EmployeeATID).ToList();
            return data;
        }

        public ListDTOModel<IC_EmployeeDTO> GetPage(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new ListDTOModel<IC_EmployeeDTO>();

            var query = (from e in _dbContext.HR_User
                         join eInfo in _dbContext.HR_EmployeeInfo
                         on e.EmployeeATID equals eInfo.EmployeeATID

                         join wi in _dbContext.IC_WorkingInfo
                         on e.EmployeeATID equals wi.EmployeeATID into eWork
                         from eWorkResult in eWork.DefaultIfEmpty()

                         join c in _dbContext.HR_CardNumberInfo.Where(e => e.IsActive == true)
                         on e.EmployeeATID equals c.EmployeeATID into cInfo
                         from cResult in cInfo.DefaultIfEmpty()

                         join d in _dbContext.IC_Department
                         on eWorkResult.DepartmentIndex equals d.Index into dWork
                         from dWorkResult in dWork.DefaultIfEmpty()

                         join m in _dbContext.IC_UserMaster
                         on eWorkResult.EmployeeATID equals m.EmployeeATID into dMaster
                         from dMasterResult in dMaster.DefaultIfEmpty()

                         where (!eWorkResult.ToDate.HasValue || eWorkResult.ToDate.Value.Date > DateTime.Now.Date)   // loc nhan vien chua nghi viec

                         select new IC_EmployeeDTO
                         {
                             WorkingInfoIndex = eWorkResult.Index,
                             CompanyIndex = e.CompanyIndex,
                             EmployeeATID = e.EmployeeATID,
                             EmployeeCode = e.EmployeeCode,
                             FullName = e.FullName,
                             Gender = e.Gender,
                             NameOnMachine = dMasterResult.NameOnMachine,
                             CardNumber = cResult.CardNumber,
                             DepartmentIndex = dWorkResult.Index,
                             DepartmentName = dWorkResult.Name,
                             TransferStatus = eWorkResult.Status,
                             ToDate = eWorkResult.ToDate,
                             FromDate = eWorkResult.FromDate,
                             UpdatedUser = e.UpdatedUser,
                             UpdatedDate = e.UpdatedDate,
                             JoinedDate = eInfo.JoinedDate.Value,
                             ImageUpload = dMasterResult.FaceV2_Content
                         }).AsQueryable();

            var addparamEmployeeTranfer = addedParams.FirstOrDefault(e => e.Key == "ListEmployeeTransferATID");
            int pageIndex = 1;
            int pageSize = GlobalParams.ROWS_NUMBER_IN_PAGE;
            var pageIndexParam = addedParams.FirstOrDefault(u => u.Key == "PageIndex");
            var pageSizeParam = addedParams.FirstOrDefault(u => u.Key == "PageSize");
            if (pageIndexParam != null && pageIndexParam.Value != null)
            {
                pageIndex = Convert.ToInt32(pageIndexParam.Value);
            }
            if (pageSizeParam != null && pageSizeParam.Value != null)
            {
                pageSize = Convert.ToInt32(pageSizeParam.Value);
            }
            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "Filter":
                            if (param.Value != null)
                            {
                                string filter = param.Value.ToString();
                                query = query.Where(u => u.EmployeeATID.Contains(filter)
                                    || u.DepartmentName.Contains(filter)
                                    || u.FullName.Contains(filter)
                                    || u.CardNumber.Contains(filter)
                                    || (!string.IsNullOrEmpty(u.EmployeeCode) && u.EmployeeCode.Contains(filter)));
                            }
                            break;
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(param.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "IsCurrentWorking":
                            if (param.Value != null)
                            {
                                query = query.Where(u => u.FromDate.Value.Date <= DateTime.Now.Date && (!u.ToDate.HasValue || u.ToDate.Value.Date >= DateTime.Now.Date)
                                 && u.TransferStatus == (short)TransferStatus.Approve);
                            }
                            break;
                        case "IsCurrentWorkingAndNoDepartment":
                            if (param.Value != null)
                            {
                                query = query.Where(u => u.FromDate.Value.Date <= DateTime.Now.Date && (!u.ToDate.HasValue || u.ToDate.Value.Date >= DateTime.Now.Date)
                                && u.TransferStatus == (short)TransferStatus.Approve);
                            }
                            break;
                        case "DepartmentIndex":
                            if (param.Value != null)
                            {
                                int departmentIndex = Convert.ToInt32(param.Value);
                                if (departmentIndex == 0)
                                {
                                    query = query.Where(u => u.DepartmentIndex == null || u.DepartmentIndex == departmentIndex);
                                }
                                else
                                {
                                    query = query.Where(u => u.DepartmentIndex.HasValue && u.DepartmentIndex == departmentIndex);
                                }
                            }
                            break;
                        case "ListDepartment":
                            if (param.Value != null)
                            {
                                IList<long> departments = (IList<long>)param.Value;
                                if (addparamEmployeeTranfer != null && addparamEmployeeTranfer.Value != null)
                                {
                                    IList<string> listEmployeeATID = (IList<string>)addparamEmployeeTranfer.Value;
                                    query = query.Where(u => u.DepartmentIndex.HasValue && departments.Contains(u.DepartmentIndex.Value)
                                        || listEmployeeATID.Contains(u.EmployeeATID) || u.DepartmentIndex == null || u.DepartmentIndex == 0);
                                }
                                else
                                {
                                    query = query.Where(u => u.DepartmentIndex.HasValue && departments.Contains(u.DepartmentIndex.Value)
                                        || u.DepartmentIndex == null || u.DepartmentIndex == 0);
                                }
                            }
                            break;
                        case "TransferStatus":
                            if (param.Value != null)
                            {
                                int approveStatus = Convert.ToInt32(param.Value);
                                query = query.Where(u => u.TransferStatus == approveStatus);
                            }
                            break;
                    }
                }
            }

            query = query.OrderBy(u => u.EmployeeATID);

            var mv = new ListDTOModel<IC_EmployeeDTO>();
            mv.TotalCount = query.Count();
            //mv.TotalCount = _dbContext.HR_User.Count();
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var data = query.ToList();
            mv.PageIndex = pageIndex;
            mv.Data = data;
            return mv;
        }

        public List<IC_EmployeeDTO> CheckCurrentDepartment(List<IC_EmployeeDTO> data)
        {
            if (data != null && data.Count > 0)
            {
                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = data.FirstOrDefault().CompanyIndex });
                addedParams.Add(new AddedParam { Key = "IsCurrentTransfer", Value = true });
                var listEmpTransfer = _IIC_EmployeeTransferLogic.GetMany(addedParams);
                if (listEmpTransfer != null && listEmpTransfer.Count > 0)
                {

                    foreach (var item in listEmpTransfer)
                    {
                        foreach (var emptf in data)
                        {
                            if (item.EmployeeATID == emptf.EmployeeATID)
                            {
                                emptf.DepartmentIndex = item.NewDepartment;
                                emptf.DepartmentName = item.NewDepartmentName;
                                emptf.DepartmentCode = item.NewDepartmentCode;
                                continue;
                            }
                        }
                    }
                }
            }
            return data;
        }

        public HR_User CheckUpdateOrInsert(HR_User employee)
        {
            if (employee == null)
                return null;

            var query = _dbContext.HR_User.AsQueryable();

            var empResult = query.Where(u => u.EmployeeATID == employee.EmployeeATID && u.CompanyIndex == employee.CompanyIndex).FirstOrDefault();

            if (empResult == null)
            {
                employee.EmployeeATID = employee.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
                //employee.JoinedDate = DateTime.Now;
                employee.CreatedDate = DateTime.Now;
                _dbContext.HR_User.Add(employee);
            }
            else
            {

                empResult.EmployeeCode = employee.EmployeeCode;
                empResult.FullName = employee.FullName;
                //empResult.DepartmentIndex = employee.DepartmentIndex;
                empResult.UpdatedDate = DateTime.Now;
                empResult.UpdatedUser = employee.UpdatedUser;
                _dbContext.HR_User.Update(empResult);
            }
            _dbContext.SaveChanges();
            return empResult;
        }

        public async Task<List<IC_EmployeeDTO>> SaveAndOverwriteList(List<IC_EmployeeDTO> listEmployee)
        {
            var listEmployeeATID = listEmployee.Select(e => e.EmployeeATID).ToList();
            var listExisted = _dbContext.HR_User
                .Where(e => listEmployeeATID.Contains(e.EmployeeATID) && listEmployee.FirstOrDefault().CompanyIndex == e.CompanyIndex)
                .ToList();
            if (listExisted != null)
            {
                bool isChanged = false;
                foreach (var dto in listEmployee)
                {
                    var entity = listExisted.FirstOrDefault(e => e.EmployeeATID == dto.EmployeeATID);
                    if (entity != null)
                    {
                        if (entity.EmployeeCode != dto.EmployeeCode || entity.FullName != dto.FullName || entity.Gender != dto.Gender)
                        {
                            ConvertDTOToDataForChanges(dto, entity);
                            _dbContext.HR_User.Update(entity);
                            isChanged = true;
                        }
                    }
                    else
                    {
                        entity = new HR_User();
                        ConvertDTOToDataNew(dto, entity);
                        _dbContext.HR_User.Add(entity);
                        isChanged = true;
                    }
                }
                if (isChanged)
                    await _dbContext.SaveChangesAsync();
            }
            return listEmployee;
        }

        public async Task<List<IC_EmployeeDTO>> SaveAndAddMoreList(List<IC_EmployeeDTO> listEmployee)
        {
            var listEmployeeATID = listEmployee.Select(e => e.EmployeeATID).ToList();
            var listExisted = _dbContext.HR_User.Where(e => listEmployeeATID.Contains(e.EmployeeATID)
                && listEmployee.FirstOrDefault().CompanyIndex == e.CompanyIndex).ToList();
            if (listExisted != null)
            {
                foreach (var emp in listEmployee)
                {
                    var data = listExisted.FirstOrDefault(e => e.EmployeeATID == emp.EmployeeATID);
                    if (data != null)
                    {
                        ConvertDTOToDataAddMore(emp, data);
                        _dbContext.HR_User.Update(data);
                    }
                    else
                    {
                        data = new HR_User();
                        ConvertDTOToDataNew(emp, data);
                        _dbContext.HR_User.Add(data);
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
            return listEmployee;
        }

        public List<string> ValidateEmployeeInfo(IC_EmployeeDTO emp)
        {
            var errors = new List<string>();
            int dataCheck = 0;
            //if (int.TryParse(emp.EmployeeATID, out dataCheck) == false)
            //{
            //    errors.Add("EmployeeATIDInvalid");
            //}
            if (!string.IsNullOrEmpty(emp.CardNumber) && emp.CardNumber.Trim() != "" && int.TryParse(emp.CardNumber, out dataCheck) == false)
            {
                errors.Add("CardNumberInvalid");
            }
            return errors;
        }

        public List<string> ValidateEmployeeAVNInfo(IC_EmployeeDTO emp)
        {
            var errors = new List<string>();
            int dataCheck = 0;
            if (int.TryParse(emp.EmployeeATID, out dataCheck) == false)
            {
                errors.Add("EmployeeATIDInvalid");
            }

            return errors;
        }

        public async Task<IC_EmployeeDTO> SaveOrUpdateAsync(IC_EmployeeDTO employee)
        {
            var entity = await _dbContext.HR_User
                .FirstOrDefaultAsync(e => employee.EmployeeATID == e.EmployeeATID && employee.CompanyIndex == e.CompanyIndex);
            if (entity != null)
            {
                if (employee.EmployeeCode != entity.EmployeeCode || employee.FullName != entity.FullName || employee.Gender != entity.Gender)
                {
                    ConvertDTOToDataForChanges(employee, entity);
                    entity.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                    entity.UpdatedDate = DateTime.Now;
                    if (_configClientName == ClientName.PSV.ToString())
                    {
                        var psv = await _dbContext.IC_EmployeeType.FirstOrDefaultAsync(x => x.Name == "PSV");
                        if (psv != null)
                        {
                            entity.EmployeeTypeIndex = psv.Index;
                        }
                    }
                    _dbContext.HR_User.Update(entity);
                    employee.IsUpdate = true;
                }
            }
            else
            {
                entity = new HR_User();
                ConvertDTOToDataNew(employee, entity);
                entity.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                entity.CreatedDate = DateTime.Now;
                if (_configClientName == ClientName.PSV.ToString())
                {
                    var psv = await _dbContext.IC_EmployeeType.FirstOrDefaultAsync(x => x.Name == "PSV");
                    if (psv != null)
                    {
                        entity.EmployeeTypeIndex = psv.Index;
                    }
                }
                _dbContext.HR_User.Add(entity);
                employee.IsInsert = true;
            }
            await _dbContext.SaveChangesAsync();
            return ConvertToDTO(entity);
        }

        public IC_EmployeeDTO SaveOrUpdateField(IC_EmployeeDTO employee)
        {
            var entity = _dbContext.HR_User
                .FirstOrDefault(e => employee.EmployeeATID == e.EmployeeATID && employee.CompanyIndex == e.CompanyIndex);
            if (entity != null)
            {
                if (employee.EmployeeCode != entity.EmployeeCode || employee.FullName != entity.FullName || employee.Gender != entity.Gender)
                {
                    ConvertDTOToDataForChanges(employee, entity);
                    _dbContext.HR_User.Update(entity);
                }
            }
            else
            {
                entity = new HR_User();
                ConvertDTOToDataNew(employee, entity);
                _dbContext.HR_User.Add(entity);
            }
            employee = ConvertToDTO(entity);
            _dbContext.SaveChanges();
            return employee;
        }

        public IC_EmployeeDTO Create(IC_EmployeeDTO employee)
        {
            var existed = _dbContext.HR_User.FirstOrDefault(e => employee.EmployeeATID == e.EmployeeATID && employee.CompanyIndex == e.CompanyIndex);
            if (existed == null)
            {
                ConvertDTOToDataNew(employee, existed);
                _dbContext.HR_User.Add(existed);
            }
            employee = ConvertToDTO(existed);
            _dbContext.SaveChanges();
            return employee;
        }

        public IC_EmployeeDTO CheckExistedOrCreate(IC_EmployeeDTO employee, UserInfo userInfo)
        {

            var existed = _dbContext.HR_User.FirstOrDefault(e => employee.EmployeeATID == e.EmployeeATID && employee.CompanyIndex == e.CompanyIndex);
            if (existed == null)
            {
                existed = new HR_User();
                ConvertDTOToDataNew(employee, existed);
                _dbContext.HR_User.Add(existed);
            }

            _dbContext.SaveChanges();
            return employee;
        }

        public IC_EmployeeDTO CheckCreateOrUpdate(IC_EmployeeDTO employee, UserInfo userInfo)
        {

            var existed = _dbContext.HR_User.FirstOrDefault(e => employee.EmployeeATID == e.EmployeeATID && employee.CompanyIndex == e.CompanyIndex);
            if (existed != null)
            {
                //existed.CardNumber = employee.CardNumber;
                //existed.NameOnMachine = employee.NameOnMachine;
                existed.UpdatedUser = employee.UpdatedUser;
                existed.EmployeeCode = employee.EmployeeCode;
                existed.UpdatedDate = DateTime.Now;
                _dbContext.HR_User.Update(existed);
            }
            else
            {
                existed = new HR_User();
                ConvertDTOToDataNew(employee, existed);
                _dbContext.HR_User.Add(existed);

                var workingInfo = new IC_WorkingInfo();
                workingInfo.EmployeeATID = employee.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
                workingInfo.CompanyIndex = employee.CompanyIndex;
                workingInfo.DepartmentIndex = 0;
                workingInfo.FromDate = employee.JoinedDate;
                workingInfo.Status = (short)TransferStatus.Approve;
                workingInfo.IsManager = false;
                workingInfo.UpdatedUser = employee.UpdatedUser;
                workingInfo.ToDate = employee.StoppedDate;
                _dbContext.IC_WorkingInfo.Add(workingInfo);
            }
            employee = ConvertToDTO(existed);
            _dbContext.SaveChanges();
            return employee;
        }

        public List<IC_EmployeeImportDTO> ValidationImportEmployee(List<IC_EmployeeImportDTO> param)
        {
            var listEmployeATIDDB = _dbContext.HR_User.Where(e => e.EmployeeType != (int)EmployeeType.Employee).Select(e => e.EmployeeATID).ToHashSet();
            var listCardNumber = _dbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == 2).ToList();
            List<IC_EmployeeImportDTO> errorList = new List<IC_EmployeeImportDTO>();
            string message = "";
            var checkDuplicate = param.GroupBy(x => x.EmployeeATID).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            var checkDuplicateCard = param.Where(x => x.CardNumber != "0" && !string.IsNullOrEmpty(x.CardNumber)).GroupBy(x => x.CardNumber).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            var checkMaxLength = param.Where(e => e.EmployeeATID.Length > 100
            || e.DepartmentName.Split("/").Any(x => x.Length > 200)
            || e.EmployeeCode.Length > 50
            || e.FullName.Length > 200
            || e.CardNumber.Length > 30
            || e.NameOnMachine.Length > 20
            || e.Email.Length > 200
            || e.PhoneNumber.Length > 50
            || (e.Nric != null && e.Nric.Length > 50)).ToList();
            var checkIsNull = new List<IC_EmployeeImportDTO>();
            var existCard = new List<IC_EmployeeImportDTO>();
            foreach (var item in param)
            {
                var card = listCardNumber.FirstOrDefault(x => x.CardNumber == item.CardNumber && item.CardNumber != "0" && !string.IsNullOrEmpty(item.CardNumber) && item.EmployeeATID != x.EmployeeATID && x.IsActive == true);
                if (card != null)
                {
                    item.ErrorMessage += "Đã tồn tại mã thẻ\r\n";
                    existCard.Add(item);
                }
                if (!string.IsNullOrWhiteSpace(item.EmployeeATID) && !item.EmployeeATID.All(char.IsDigit))
                {
                    item.ErrorMessage += "Mã chấm công chỉ chấp nhận kí tự số\r\n";
                }
            }
            errorList.AddRange(existCard);
            if (_configClientName.ToUpper() == ClientName.MAY.ToString())
            {
                checkIsNull = param.Where(e => string.IsNullOrWhiteSpace(e.EmployeeATID)
                    || string.IsNullOrWhiteSpace(e.DepartmentName) || string.IsNullOrWhiteSpace(e.CardNumber)).ToList();
            }
            else
            {
                checkIsNull = param.Where(e => string.IsNullOrWhiteSpace(e.EmployeeATID)
                        || string.IsNullOrWhiteSpace(e.DepartmentName)).ToList();
            }
            //var checkExisted = param.Where(e => listEmployeATIDDB.Contains(e.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'))).ToList();

            if (checkDuplicate != null && checkDuplicate.Count() > 0)
            {
                var duplicate = param.Where(e => checkDuplicate.Contains(e.EmployeeATID)).ToList();
                foreach (var item in duplicate)
                {
                    item.ErrorMessage = "Trùng mã nhân viên\r\n";
                }
                errorList.AddRange(duplicate);
            }
            if (checkMaxLength != null && checkMaxLength.Count() > 0)
            {
                foreach (var item in checkMaxLength)
                {
                    if (item.EmployeeATID.Length > 100) item.ErrorMessage += "Mã chấm công lớn hơn 50 ký tự" + "\r\n";
                    if (item.DepartmentName.Split("/").Any(x => x.Length > 200)) item.ErrorMessage += "Mã phòng ban lớn hơn 200 ký tự" + "\r\n";
                    if (item.EmployeeCode.Length > 50) item.ErrorMessage += "Mã nhân viên lớn hơn 50 ký tự" + "\r\n";
                    if (item.FullName.Length > 200) item.ErrorMessage += "Tên nhân viên lớn hơn 200 ký tự" + "\r\n";
                    if (item.CardNumber.Length > 30) item.ErrorMessage += "Mã thẻ lớn hơn 30 ký tự" + "\r\n";
                    if (item.NameOnMachine.Length > 20) item.ErrorMessage += "Tên trên máy lớn hơn 20 ký tự" + "\r\n";
                    if (item.Email.Length > 200) item.ErrorMessage += "Email lớn hơn 200 ký tự" + "\r\n";
                    if (item.PhoneNumber.Length > 50) item.ErrorMessage += "Số DT lớn hơn 50 ký tự" + "\r\n";
                    if (item.Nric != null && item.Nric.Length > 50) item.ErrorMessage += "CMND/CCCD/Passport lớn hơn 50 ký tự" + "\r\n";
                }
                errorList.AddRange(checkMaxLength);
            }
            if (checkIsNull != null && checkIsNull.Count() > 0)
            {
                foreach (var item in checkIsNull)
                {
                    if (string.IsNullOrEmpty(item.EmployeeATID)) item.ErrorMessage += "Mã chấm không được để trống\r\n";
                    if (string.IsNullOrEmpty(item.DepartmentName)) item.ErrorMessage += "Phòng ban không được để trống\r\n";

                    if (_configClientName.ToUpper() == ClientName.MAY.ToString()
                        && string.IsNullOrEmpty(item.CardNumber)) item.ErrorMessage += "Mã thẻ không được để trống\r\n";
                }

                errorList.AddRange(checkIsNull);
            }

            if (checkDuplicateCard.Any())
            {
                var duplicate = param.Where(e => checkDuplicateCard.Contains(e.CardNumber)).ToList();
                foreach (var item in duplicate)
                {
                    item.ErrorMessage += "Trùng mã thẻ trong file excel\r\n";
                }
                errorList.AddRange(duplicate);
            }


            var listEmployeeTypeName = param.Select(x => x.EmployeeTypeName).Distinct().ToList();
            var listEmployeeType = _dbContext.IC_EmployeeType.Where(x => listEmployeeTypeName.Contains(x.Name)).ToList();
            var listPositionDB = _dbContext.HR_PositionInfo.ToList();
            if (_configClientName.ToUpper() == ClientName.PSV.ToString())
            {
                foreach (var item in param)
                {
                    if (string.IsNullOrWhiteSpace(item.EmployeeTypeName))
                    {
                        item.ErrorMessage += "Loại nhân viên không được để trống\r\n";
                    }
                    else
                    {
                        var employeeType = listEmployeeType.FirstOrDefault(x => x.Name == item.EmployeeTypeName);
                        if (employeeType == null)
                        {
                            item.ErrorMessage += "Loại nhân viên không tồn tại\r\n";
                        }
                        else if (employeeType != null && !employeeType.IsUsing)
                        {
                            item.ErrorMessage += "Loại nhân viên không được sử dụng\r\n";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(item.ErrorMessage))
                    {
                        errorList.Add(item);
                    }
                }
            }
            else
            {
                var dataCheckBlackList = param.Where(x => !string.IsNullOrWhiteSpace(x.EmployeeATID) || !string.IsNullOrWhiteSpace(x.Nric)).ToList();
                var employeeInBlackList = _dbContext.GC_BlackList.Where(x => dataCheckBlackList.Select(z => z.EmployeeATID).Contains(x.EmployeeATID) || dataCheckBlackList.Select(z => z.Nric).Contains(x.Nric)).ToList();
                var departmentList = _dbContext.IC_Department.Where(x => param.Select(z => z.DepartmentName).Contains(x.Name)).ToList();
                foreach (var item in param)
                {

                    if (!string.IsNullOrWhiteSpace(item.Position))
                    {
                        var positionInfo = listPositionDB.FirstOrDefault(x => x.Name.ToLower().RemoveAccents() == item.Position.ToLower().RemoveAccents());
                        if (positionInfo == null)
                        {
                            item.ErrorMessage += "Chức vụ không tồn tại\r\n";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(item.EmployeeTypeName))
                    {
                        var employeeType = listEmployeeType.FirstOrDefault(x => x.Name == item.EmployeeTypeName);
                        if (employeeType == null)
                        {
                            item.ErrorMessage += "Loại nhân viên không tồn tại\r\n";
                        }
                        else if (employeeType != null && !employeeType.IsUsing)
                        {
                            item.ErrorMessage += "Loại nhân viên không được sử dụng\r\n";
                        }
                    }
                    //Check Birday 
                    if (_configClientName.ToUpper() == ClientName.MONDELEZ.ToString())
                    {
                        if (string.IsNullOrEmpty(item.Nric)) { item.ErrorMessage += "CMND/CCCD/Passport không được để trống\r\n"; }

                        if (!string.IsNullOrEmpty(item.Nric) && char.IsDigit(item.Nric[0]) && item.Nric.Length != 12)
                        {
                            item.ErrorMessage += "CCCD phải đúng 12 ký tự \r\n";
                        }

                        if (string.IsNullOrEmpty(item.DateOfBirth))
                        {
                            item.ErrorMessage += "Ngày sinh không được để trống\r\n";
                        }
                        else
                        {
                            string[] formats = { "dd/MM/yyyy" };
                            var birthDay = new DateTime();
                            var now = DateTime.Now;
                            var convertFromDate = DateTime.TryParseExact(item.DateOfBirth, formats,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out birthDay);
                            if (!convertFromDate)
                            {
                                item.ErrorMessage += "Ngày sinh không hợp lệ\r\n";
                            }
                            else
                            {
                                if (birthDay.AddYears(18).Date >= now.Date)
                                {
                                    item.ErrorMessage += "Nhân viên chưa đủ 18 tuổi\r\n";
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(item.DepartmentName))
                        {
                            var department = departmentList.FirstOrDefault(y => y.Name == item.DepartmentName);
                            if (department == null)
                            {
                                item.ErrorMessage += "Phòng ban không tồn tại\r\n";
                            }
                            else
                            {
                                if (department.IsContractorDepartment == true || department.IsDriverDepartment == true)
                                {
                                    item.ErrorMessage += "Phòng ban không hợp lệ\r\n";
                                }
                            }
                        }
                        else
                        {
                            item.ErrorMessage += "Phòng ban không được để trống\r\n";
                        }
                    }



                    //Check stopped day > start day
                    if (_configClientName.ToUpper() == ClientName.MONDELEZ.ToString() && !string.IsNullOrEmpty(item.StoppedDate))
                    {
                        string[] formats = { "dd/MM/yyyy" };
                        DateTime stoppedDate, joinedDate;
                        DateTime now = DateTime.Now;
                        if (!DateTime.TryParseExact(item.StoppedDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out stoppedDate))
                        {
                            item.ErrorMessage += "Ngày nghỉ không hợp lệ\r\n";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(item.JoinedDate))
                            {
                                if (stoppedDate.Date <= now.Date)
                                {
                                    item.ErrorMessage += "Ngày nghỉ phải lớn hơn ngày hiện tại\r\n";
                                }
                            }
                            else if (!DateTime.TryParseExact(item.JoinedDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out joinedDate))
                            {
                                item.ErrorMessage += "Ngày vào không hợp lệ\r\n";
                            }
                            else if (stoppedDate.Date < joinedDate.Date)
                            {
                                item.ErrorMessage += "Ngày nghỉ phải lớn hơn ngày vào\r\n";
                            }
                        }

                        var checkEmployeeInBlackList = employeeInBlackList.Any(x => ((!string.IsNullOrWhiteSpace(item.EmployeeATID) && item.EmployeeATID == x.EmployeeATID) || (!string.IsNullOrEmpty(item.Nric) && x.Nric == item.Nric))
                                                                                                          && x.FromDate.Date <= now.Date
                                                                                                          && (x.ToDate == null || (x.ToDate != null && now.Date <= x.ToDate.Value.Date)));
                        if (checkEmployeeInBlackList)
                        {
                            item.ErrorMessage += "Người dùng thuộc danh sách đen\r\n";
                        }
                    }


                    if (!string.IsNullOrWhiteSpace(item.ErrorMessage))
                    {
                        errorList.Add(item);
                    }
                }
            }

            //if (checkExisted != null && checkExisted.Count > 0)
            //{
            //    foreach (var item in checkExisted)
            //    {
            //        item.ErrorMessage += "Mã nhân viên đã tồn tại\r\n";
            //    }
            //    errorList.AddRange(checkExisted);
            //}
            errorList = errorList.Distinct().ToList();
            return errorList;
        }

        public bool ExportTemplateICEmployee(string folderDetails)
        {
            try
            {
                var employeeTypeList = _dbContext.IC_EmployeeType.Where(x => x.IsUsing).Select(x => x.Name).OrderByDescending(x => x).ToList();

                using (var workbook = new XLWorkbook(folderDetails))
                {
                    var worksheet = workbook.Worksheets;
                    IXLWorksheet worksheet1;
                    IXLWorksheet worksheet5;

                    var w1 = worksheet.TryGetWorksheet("EmployeeTypeData", out worksheet1);
                    worksheet1.Cells().Clear();
                    string startCanteenCell = "A1";
                    string endCanteenCell = string.Empty;
                    for (int i = 0; i < employeeTypeList.Count; i++)
                    {
                        if (i == (employeeTypeList.Count - 1))
                        {
                            endCanteenCell = "A" + (i + 1);
                        }
                        worksheet1.Cell("A" + (i + 1)).Value = employeeTypeList[i];
                    }

                    var w = worksheet.TryGetWorksheet("Sheet1", out worksheet5);
                    if (!string.IsNullOrWhiteSpace(startCanteenCell) && !string.IsNullOrWhiteSpace(endCanteenCell))
                        worksheet5.Range("F2:F10002").SetDataValidation().List(worksheet1.Range(startCanteenCell
                            + ":" + endCanteenCell), true);

                    workbook.Save();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("ExportInfoMealRegistration: " + ex.ToString());
                return false;
            }
        }

        private IC_EmployeeDTO ConvertToDTO(HR_User data)
        {
            var dto = new IC_EmployeeDTO
            {
                EmployeeATID = data.EmployeeATID,
                CompanyIndex = data.CompanyIndex,
                EmployeeCode = data.EmployeeCode,
                FullName = data.FullName,
                Gender = data.Gender,
                CreatedDate = data.CreatedDate,
                UpdatedDate = data.UpdatedDate,
                UpdatedUser = data.UpdatedUser
            };
            //dto.JoinedDate = data.JoinedDate;
            //dto.StoppedDate = data.StoppedDate;
            //dto.NameOnMachine = data.NameOnMachine;
            //dto.DepartmentIndex = data.DepartmentIndex;
            //dto.CardNumber = data.CardNumber;
            return dto;
        }

        private void ConvertDTOToDataNew(IC_EmployeeDTO dto, HR_User data)
        {
            data.EmployeeATID = dto.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
            data.CompanyIndex = dto.CompanyIndex;
            data.EmployeeCode = dto.EmployeeCode;
            data.FullName = dto.FullName;
            data.Gender = dto.Gender;
            data.CreatedDate = dto.CreatedDate;
            data.Note = dto.Note;
            data.EmployeeType = (int)EmployeeType.Employee;
        }

        private void ConvertDTOToDataAddMore(IC_EmployeeDTO dto, HR_User data)
        {
            //data.NameOnMachine = string.IsNullOrWhiteSpace(data.NameOnMachine) ? dto.NameOnMachine : data.NameOnMachine;
            //if (!string.IsNullOrWhiteSpace(data.NameOnMachine) && data.NameOnMachine.Length > 20)
            //{
            //    data.NameOnMachine = data.NameOnMachine.Substring(0, 20);
            //}
            //data.CardNumber = string.IsNullOrWhiteSpace(data.CardNumber) ? dto.CardNumber : data.CardNumber;
            //data.CardNumber = string.IsNullOrWhiteSpace(data.CardNumber) ? dto.CardNumber : data.CardNumber;
            //data.JoinedDate = string.IsNullOrWhiteSpace(data.JoinedDate.ToString()) ? dto.JoinedDate : data.JoinedDate;
            //data.StoppedDate = string.IsNullOrWhiteSpace(data.StoppedDate.ToString()) ? dto.StoppedDate : data.StoppedDate;
            //data.DepartmentIndex = string.IsNullOrWhiteSpace(data.DepartmentIndex.ToString()) ? (int)dto.DepartmentIndex.Value : data.DepartmentIndex;
            data.EmployeeCode = string.IsNullOrWhiteSpace(data.EmployeeCode) ? dto.EmployeeCode : data.EmployeeCode;
            data.FullName = string.IsNullOrWhiteSpace(data.FullName) ? dto.FullName : data.FullName;
            data.Gender = !data.Gender.HasValue ? dto.Gender : (short)data.Gender.Value;
            data.CreatedDate = dto.CreatedDate;
            data.UpdatedDate = dto.UpdatedDate;
            data.UpdatedUser = dto.UpdatedUser;
            data.EmployeeType = (int)EmployeeType.Employee;
        }

        private void ConvertDTOToDataForChanges(IC_EmployeeDTO dto, HR_User data)
        {
            //data.NameOnMachine = string.IsNullOrWhiteSpace(dto.NameOnMachine) ? data.NameOnMachine : dto.NameOnMachine;
            //if (!string.IsNullOrWhiteSpace(data.NameOnMachine) && data.NameOnMachine.Length > 20)
            //{
            //    data.NameOnMachine = data.NameOnMachine.Substring(0, 20);
            //}
            //data.CardNumber = string.IsNullOrWhiteSpace(dto.CardNumber) ? data.CardNumber : dto.CardNumber;
            //data.JoinedDate = string.IsNullOrWhiteSpace(dto.JoinedDate.ToString()) ? data.JoinedDate : dto.JoinedDate;
            //data.StoppedDate = string.IsNullOrWhiteSpace(dto.StoppedDate.ToString()) ? data.StoppedDate : dto.StoppedDate;
            //data.DepartmentIndex = !dto.DepartmentIndex.HasValue ? data.DepartmentIndex : (int)dto.DepartmentIndex.Value;

            if (!string.IsNullOrWhiteSpace(dto.EmployeeCode) && data.EmployeeCode != dto.EmployeeCode)
                data.EmployeeCode = dto.EmployeeCode;

            if (!string.IsNullOrWhiteSpace(dto.FullName) && data.FullName != dto.FullName)
                data.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.Note) && data.Note != dto.Note)
                data.Note = dto.Note;

            if (dto.Gender != null && data.Gender != dto.Gender.Value)
                data.Gender = dto.Gender.Value;

            data.UpdatedDate = dto.UpdatedDate;
            data.UpdatedUser = dto.UpdatedUser;
            data.EmployeeType = (int)EmployeeType.Employee;
        }
    }

    public interface IIC_EmployeeLogic
    {
        List<IC_EmployeeImportDTO> ValidationImportEmployee(List<IC_EmployeeImportDTO> param);
        bool ExportTemplateICEmployee(string folderDetails);
        HR_User CheckUpdateOrInsert(HR_User employee);
        List<IC_EmployeeDTO> CheckCurrentDepartment(List<IC_EmployeeDTO> data);
        List<IC_EmployeeDTO> GetEmployeeList(List<AddedParam> addedParams);
        ListDTOModel<IC_EmployeeDTO> GetPage(List<AddedParam> addedParams);
        Task<List<EmployeeFullInfo>> GetEmployeeCompactInfo(List<AddedParam> addedParams);
        Task<HR_Employee> GetByEmployeeATIDAndCompanyIndex(string employeeATID, int companyIndex);
        List<IC_EmployeeLookupDTO> GetListEmployeeLookup(ConfigObject pConfig, UserInfo pUser);
        IC_EmployeeDTO SaveOrUpdateField(IC_EmployeeDTO employee);
        Task<IC_EmployeeDTO> SaveOrUpdateAsync(IC_EmployeeDTO employee);
        List<string> ValidateEmployeeInfo(IC_EmployeeDTO emp);
        List<string> ValidateEmployeeAVNInfo(IC_EmployeeDTO emp);
        IC_EmployeeDTO CheckCreateOrUpdate(IC_EmployeeDTO employee, UserInfo userInfo);
        Task<List<IC_EmployeeDTO>> SaveAndOverwriteList(List<IC_EmployeeDTO> listEmployee);
        Task<List<IC_EmployeeDTO>> SaveAndAddMoreList(List<IC_EmployeeDTO> listEmployee);
        List<IC_EmployeeDTO> GetManyExport(List<AddedParam> addedParams);
        List<IC_EmployeeDTO> GetManyStoppedWorking(List<AddedParam> addedParams);
        List<IC_EmployeeDTO> GetManyExportEmployee(List<AddedParam> addedParams);
    }
}
