using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace EPAD_Logic
{
    public class IC_UserMasterLogic : IIC_UserMasterLogic
    {
        private EPAD_Context _dbContext;
        private IMemoryCache _iCache;
        private ConfigObject _config;
        IConfiguration _configuration;
        private IIC_WorkingInfoLogic _iC_WorkingInfoLogic;
        private IIC_CommandLogic _iC_CommandLogic;
        private string _configClientName;

        public IC_UserMasterLogic(EPAD_Context dbcontext, IMemoryCache iCache,
            IIC_WorkingInfoLogic iC_WorkingInfoLogic,
            IConfiguration configuration)
        {
            _configuration = configuration;
            _dbContext = dbcontext;
            _iCache = iCache;
            _config = ConfigObject.GetConfig(_iCache);
            _iC_WorkingInfoLogic = iC_WorkingInfoLogic;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public List<IC_UserMasterDTO> GetMany(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return null;

            var query = _dbContext.IC_UserMaster.AsQueryable();

            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "EmployeeATID":
                            if (p.Value != null)
                            {
                                string employeeId = p.Value.ToString();
                                query = query.Where(e => e.EmployeeATID == employeeId);
                            }
                            break;
                        case "ListEmployeeATID":
                            if (p.Value != null)
                            {
                                IList<string> listEmployeeATID = (IList<string>)p.Value;
                                query = query.Where(e => listEmployeeATID.Contains(e.EmployeeATID));
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(e => e.CompanyIndex == companyIndex);
                            }
                            break;
                    }
                }
            }
            query.OrderBy(e => e.EmployeeATID);
            var data = query.Select(e => new IC_UserMasterDTO
            {
                EmployeeATID = e.EmployeeATID,
                CompanyIndex = e.CompanyIndex,
                NameOnMachine = e.NameOnMachine,
                CardNumber = e.CardNumber,
                Password = e.Password,
                Privilege = e.Privilege,
                AuthenMode = e.AuthenMode,
                FingerData0 = e.FingerData0,
                FingerData1 = e.FingerData1,
                FingerData2 = e.FingerData2,
                FingerData3 = e.FingerData3,
                FingerData4 = e.FingerData4,
                FingerData5 = e.FingerData5,
                FingerData6 = e.FingerData6,
                FingerData7 = e.FingerData7,
                FingerData8 = e.FingerData8,
                FingerData9 = e.FingerData9,
                FingerVersion = e.FingerVersion,

                FaceIndex = e.FaceIndex,
                FaceTemplate = e.FaceTemplate,
                FaceVersion = e.FaceVersion,

                FaceV2_Index = e.FaceV2_Index,
                FaceV2_No = e.FaceV2_No,
                FaceV2_Format = e.FaceV2_Format,
                FaceV2_Duress = e.FaceV2_Duress,
                FaceV2_MajorVer = e.FaceV2_MajorVer,
                FaceV2_MinorVer = e.FaceV2_MinorVer,
                FaceV2_Size = e.FaceV2_Size,
                FaceV2_Type = e.FaceV2_Type,
                FaceV2_Valid = e.FaceV2_Valid,
                FaceV2_Content = e.FaceV2_Content,
                FaceV2_TemplateBIODATA = e.FaceV2_TemplateBIODATA,
                CreatedDate = e.CreatedDate,
                UpdatedDate = e.UpdatedDate,
                UpdatedUser = e.UpdatedUser

            }).ToList();
            return data;
        }

        // Using SQL query command for using DATALENGHT to get FACE Lenght
        public List<IC_EmployeeDTO> GetUserMasterInfoMany(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new List<IC_EmployeeDTO>();

            var addparamEmployeeTranfer = addedParams.FirstOrDefault(e => e.Key == "ListEmployeeTransferATID");


            string select = "SELECT distinct ";
            select += "\n" + "w.ToDate, u.EmployeeATID,w.DepartmentIndex, u.EmployeeCode, u.FullName, d.[Name] AS DepartmentName, ISNULL(us.NameOnMachine, '') as NameOnMachine, ISNULL(c.CardNumber, '') as CardNumber, ISNULL(us.Privilege, 0) AS Privilege, ISNULL(us.[Password], '') AS[Password],";
            select += "\n" + "(case when DATALENGTH(us.FaceTemplate) > 0  then ISNULL(DATALENGTH(us.FaceTemplate), 0) else ISNULL(DATALENGTH(us.FaceV2_Content), 0) end) AS FaceTemplate,";
            select += "\n" + "ISNULL(LEN(us.FingerData0), 0) as Finger1, ISNULL(LEN(us.FingerData1), 0) as Finger2, ISNULL(LEN(us.FingerData2), 0) as Finger3, ISNULL(LEN(us.FingerData3), 0) as Finger4, ISNULL(LEN(us.FingerData4), 0) as Finger5,";
            select += "\n" + "ISNULL(LEN(us.FingerData5), 0) as Finger6, ISNULL(LEN(us.FingerData6), 0) as Finger7, ISNULL(LEN(us.FingerData7), 0) as Finger8, ISNULL(LEN(us.FingerData8), 0) as Finger9, ISNULL(LEN(us.FingerData9), 0) as Finger10,'' AS AliasName, '' AS SerialNumber, '' AS IPAddress, u.UpdatedDate ";

            string from = "\n" + "FROM dbo.HR_User u ";
            from += "\n" + "LEFT JOIN dbo.HR_EmployeeInfo e ON u.EmployeeATID = e.EmployeeATID AND u.CompanyIndex = e.CompanyIndex ";
            from += "\n" + "LEFT JOIN dbo.HR_CustomerInfo ci ON ci.EmployeeATID = u.EmployeeATID AND ci.CompanyIndex = u.CompanyIndex ";
            from += "\n" + "LEFT JOIN dbo.HR_CardNumberInfo c ON u.EmployeeATID = c.EmployeeATID AND u.CompanyIndex = c.CompanyIndex AND c.IsActive = 1 ";
            from += "\n" + "LEFT JOIN dbo.IC_UserMaster us ON u.EmployeeATID = us.EmployeeATID AND u.CompanyIndex = us.CompanyIndex ";
            from += "\n" + "LEFT JOIN dbo.IC_WorkingInfo w ON u.EmployeeATID = w.EmployeeATID AND u.CompanyIndex = w.CompanyIndex ";
            from += "\n" + "LEFT JOIN dbo.IC_Department d ON w.DepartmentIndex = d.[Index] AND w.CompanyIndex = d.CompanyIndex ";
            string where = "\n WHERE (1=1) AND "; //+ $"WHERE (w.ToDate is null OR Datediff(day, w.ToDate, GetDate()) <= 0) AND ";

            DateTime maxToday = CommonUtils.GetMaxToday();
            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "Filter":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                string filter = param.Value.ToString();
                                var filterBy = filter.Split(" ").ToList();
                                where += " (u.FullName like '%" + filter + "%' OR ";
                                where += " u.EmployeeCode like '%" + filter + "%' OR ";
                                where += " u.EmployeeATID like '%" + filter + "%' OR ";
                                where += " d.[Name] like '%" + filter + "%' OR ";
                                where += " us.NameOnMachine like '%" + filter + "%' OR ";
                                if (filterBy.Count > 0)
                                {
                                    where += " u.EmployeeATID in ('" + string.Join("','", filterBy) + "') OR ";
                                    where += " u.FullName in ('" + string.Join("','", filterBy) + "') OR ";
                                }
                                where += " c.CardNumber like '%" + filter + "%') AND ";
                            }
                            break;
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(param.Value);
                                where += " u.CompanyIndex = " + companyIndex.ToString() + " AND ";
                            }
                            break;
                        case "DepartmentIndex":
                            if (param.Value != null)
                            {
                                int departmentID = Convert.ToInt32(param.Value);
                                where += "w.DepartmentIndex = " + departmentID + " AND ";
                            }
                            break;
                        case "ListDepartment":
                            if (param.Value != null)
                            {
                                IList<long> departments = (IList<long>)param.Value;
                                if (addparamEmployeeTranfer != null && addparamEmployeeTranfer.Value != null)
                                {
                                    IList<string> listEmployeeID = (IList<string>)addparamEmployeeTranfer.Value;
                                    where += "((w.DepartmentIndex IN (" + string.Join(",", departments) + ") OR ci.ContactDepartment IN (" + string.Join(",", departments) + ")) OR u.EmployeeATID IN ('" + string.Join("','", listEmployeeID) + "')) AND ";
                                }
                                else
                                {
                                    where += "(w.DepartmentIndex IN (" + string.Join(",", departments) + ") OR ci.ContactDepartment IN (" + string.Join(",", departments) + ")) AND ";
                                }
                            }
                            break;
                        case "ListEmployeeATID":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                IList<string> listEmployeeID = (IList<string>)param.Value;
                                where += "u.EmployeeATID IN ('" + string.Join("','", listEmployeeID) + "') AND ";
                            }
                            break;
                        case "EmployeeATID":
                            if (param.Value != null)
                            {
                                string employeeID = param.Value.ToString();
                                where += "u.EmployeeATID = '" + employeeID + "' AND ";
                            }
                            break;

                        case "IsCurrentWorking":
                            if (param.Value != null)
                            {
                                where += "(w.ToDate is null and Datediff(day, w.FromDate, GetDate()) >= 0 and w.Status = 1) OR (Datediff(day, w.ToDate, GetDate()) <= 0 AND Datediff(day, w.FromDate, GetDate()) >= 0) and w.Status = 1) AND ";
                            }
                            break;
                        case "IsCurrentWorkingAndNoDepartment":
                            if (param.Value != null)
                            {
                                where += "((w.ToDate is null and Datediff(day, w.FromDate, GetDate()) >= 0 and w.Status = 1) OR ((Datediff(day, w.ToDate, GetDate()) <= 0 AND Datediff(day, w.FromDate, GetDate()) >= 0) and w.Status = 1) OR  (w.DepartmentIndex is null OR w.DepartmentIndex = 0) ) AND ";
                            }
                            break;
                        case "IsWorking":
                            if (param.Value != null)
                            {
                                var listWorking = (List<int>)param.Value;
                                if (!listWorking.Contains((int)EmployeeStatusType.Working) || !listWorking.Contains((int)EmployeeStatusType.StopWorking))
                                {
                                    if (listWorking.Contains((int)EmployeeStatusType.Working)) // working
                                    {
                                        where += "(w.ToDate is null OR Datediff(day, w.ToDate, GetDate()) <= 0) AND ";
                                    }
                                    if (listWorking.Contains((int)EmployeeStatusType.StopWorking)) // stop working
                                    {
                                        where += "(Datediff(day, w.ToDate, GetDate()) >= 0) AND ";
                                    }
                                }
                            }
                            break;
                        case "UserType":
                            if (param.Value != null)
                            {
                                where += "(u.EmployeeType is NULL or u.EmployeeType = " + (int)param.Value + ") AND ";
                            }
                            break;
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
                                dataRow.EmployeeATID = (dtRow["EmployeeATID"] is DBNull) ? null : dtRow["EmployeeATID"].ToString();
                                dataRow.DepartmentIndex = (dtRow["DepartmentIndex"] is DBNull) ? 0 : Convert.ToInt32(dtRow["DepartmentIndex"].ToString());
                                dataRow.EmployeeCode = (dtRow["EmployeeCode"] is DBNull) ? null : dtRow["EmployeeCode"].ToString();
                                dataRow.FullName = (dtRow["FullName"] is DBNull) ? null : dtRow["FullName"].ToString();
                                dataRow.DepartmentName = (dtRow["DepartmentName"] is DBNull) ? "Không có phòng ban" : dtRow["DepartmentName"].ToString();
                                dataRow.NameOnMachine = (dtRow["NameOnMachine"] is DBNull) ? null : dtRow["NameOnMachine"].ToString();
                                dataRow.CardNumber = (dtRow["CardNumber"] is DBNull) ? null : dtRow["CardNumber"].ToString();
                                dataRow.Privilege = (dtRow["Privilege"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Privilege"].ToString());
                                dataRow.PrivilegeName = GetPrivilegeName(dataRow.Privilege.Value);
                                dataRow.Password = (dtRow["Password"] is DBNull) ? null : dtRow["Password"].ToString();
                                dataRow.FaceTemplate = (dtRow["FaceTemplate"] is DBNull) ? 0 : Convert.ToInt32(dtRow["FaceTemplate"].ToString());
                                dataRow.Finger1 = (dtRow["Finger1"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger1"].ToString());
                                dataRow.Finger2 = (dtRow["Finger2"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger2"].ToString());
                                dataRow.Finger3 = (dtRow["Finger3"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger3"].ToString());
                                dataRow.Finger4 = (dtRow["Finger4"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger4"].ToString());
                                dataRow.Finger5 = (dtRow["Finger5"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger5"].ToString());
                                dataRow.Finger6 = (dtRow["Finger6"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger6"].ToString());
                                dataRow.Finger7 = (dtRow["Finger7"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger7"].ToString());
                                dataRow.Finger8 = (dtRow["Finger8"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger8"].ToString());
                                dataRow.Finger9 = (dtRow["Finger9"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger9"].ToString());
                                dataRow.Finger10 = (dtRow["Finger10"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger10"].ToString());
                                dataRow.AliasName = (dtRow["AliasName"] is DBNull) ? null : dtRow["AliasName"].ToString();
                                dataRow.SerialNumber = (dtRow["SerialNumber"] is DBNull) ? null : dtRow["SerialNumber"].ToString();
                                dataRow.IPAddress = (dtRow["IPAddress"] is DBNull) ? null : dtRow["IPAddress"].ToString();
                                if (!(dtRow["UpdatedDate"] is DBNull))
                                {
                                    dataRow.UpdatedDate = DateTime.Parse(dtRow["UpdatedDate"].ToString());
                                }
                                dataRow.Status = (dtRow["ToDate"] is DBNull || DateTime.Parse(dtRow["ToDate"].ToString()) > DateTime.Now) ? "IsWorking" : "StoppedWork";
                                if (!(dtRow["ToDate"] is DBNull) && dataRow.Status.Contains("StoppedWork"))
                                {
                                    dataRow.Note = DateTime.Parse(dtRow["ToDate"].ToString()).ToddMMyyyyMinus();
                                }
                                resutlData.Add(dataRow);
                            }

                        }
                    }
                }

            }

            resutlData = resutlData.OrderBy(x => x.EmployeeATID).ThenByDescending(x => x.CardNumber).ToList();
            var result = new List<IC_EmployeeDTO>();
            int indexResult = 0;
            foreach (var x in resutlData)
            {
                if (!result.Select(x => x.EmployeeATID).Contains(x.EmployeeATID) ||
                    !result.Select(x => x.DepartmentIndex).Contains(x.DepartmentIndex) ||
                    !result.Select(x => x.ToDate).Contains(x.ToDate))
                {
                    result.Add(x);
                    x.Index = indexResult;
                    indexResult++;
                }
            }

            //resutlData = resutlData.GroupBy(x => x.EmployeeATID).Select(x => x.OrderByDescending(x => x.CardNumber).First()).ToList();
            return result;
        }

        public List<IC_EmployeeDTO> GetStudentMasterInfoMany(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new List<IC_EmployeeDTO>();


            string select = "SELECT distinct ";
            select += "\n" + "u.EmployeeATID,u.EmployeeCode, u.FullName, ISNULL(us.NameOnMachine, '') as NameOnMachine, ISNULL(c.CardNumber, '') as CardNumber, ";
            select += "\n" + "ISNULL(us.Privilege, 0) AS Privilege, ISNULL(us.[Password], '') AS [Password], cl.Name as ClassName, ";
            select += "\n" + "(case when DATALENGTH(us.FaceTemplate) > 0  then ISNULL(DATALENGTH(us.FaceTemplate), 0) else ISNULL(DATALENGTH(us.FaceV2_Content), 0) end) AS FaceTemplate,";
            select += "\n" + "ISNULL(LEN(us.FingerData0), 0) as Finger1, ISNULL(LEN(us.FingerData1), 0) as Finger2, ISNULL(LEN(us.FingerData2), 0) as Finger3, ISNULL(LEN(us.FingerData3), 0) as Finger4, ISNULL(LEN(us.FingerData4), 0) as Finger5,";
            select += "\n" + "ISNULL(LEN(us.FingerData5), 0) as Finger6, ISNULL(LEN(us.FingerData6), 0) as Finger7, ISNULL(LEN(us.FingerData7), 0) as Finger8, ISNULL(LEN(us.FingerData8), 0) as Finger9, ISNULL(LEN(us.FingerData9), 0) as Finger10,'' AS AliasName, '' AS SerialNumber ";


            string from = "\n" + "From HR_User u ";
            from += "\n" + "JOIN HR_StudentInfo s on u.EmployeeATID = s.EmployeeATID and u.CompanyIndex = s.CompanyIndex ";
            from += "\n" + "LEFT join HR_StudentClassInfo sli on s.EmployeeATID = sli.EmployeeATID and s.CompanyIndex = sli.CompanyIndex ";
            from += "\n" + "LEFT join HR_ClassInfo cl on sli.ClassInfoIndex = cl.[Index] and sli.CompanyIndex = cl.CompanyIndex ";
            from += "\n" + "LEFT JOIN IC_UserMaster us on u.EmployeeATID = us.EmployeeATID and u.CompanyIndex = us.CompanyIndex ";
            from += "\n" + "LEFT JOIN HR_CardNumberInfo c on u.EmployeeATID = c.EmployeeATID and u.CompanyIndex = c.CompanyIndex and c.IsActive = 1 ";

            string where = "\n" + $"WHERE 1 = 1 AND ";

            DateTime maxToday = CommonUtils.GetMaxToday();
            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "Filter":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                string filter = param.Value.ToString();
                                where += " (u.FullName like '%" + filter + "%' OR ";
                                where += " u.EmployeeCode like '%" + filter + "%' OR ";
                                where += " u.EmployeeATID like '%" + filter + "%' OR ";
                                where += " cl.Name like '%" + filter + "%' OR ";
                                where += " us.NameOnMachine like '%" + filter + "%' OR ";
                                where += " c.CardNumber like '%" + filter + "%') AND ";
                            }
                            break;
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(param.Value);
                                where += " u.CompanyIndex = " + companyIndex.ToString() + " AND ";
                            }
                            break;
                        case "ListEmployeeATID":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                IList<string> listEmployeeID = (IList<string>)param.Value;
                                where += "u.EmployeeATID IN ('" + string.Join("','", listEmployeeID) + "') AND ";
                            }
                            break;
                        case "EmployeeATID":
                            if (param.Value != null)
                            {
                                string employeeID = param.Value.ToString();
                                where += "u.EmployeeATID = '" + employeeID + "' AND ";
                            }
                            break;
                        case "ListClassIndex":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                IList<string> listLastIndex = (IList<string>)param.Value;
                                if (listLastIndex.Count() > 0)
                                    where += "cl.[Index] IN ('" + string.Join("','", listLastIndex) + "') AND ";
                            }
                            break;
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
                                dataRow.EmployeeATID = (dtRow["EmployeeATID"] is DBNull) ? null : dtRow["EmployeeATID"].ToString();
                                dataRow.EmployeeCode = (dtRow["EmployeeCode"] is DBNull) ? null : dtRow["EmployeeCode"].ToString();
                                dataRow.FullName = (dtRow["FullName"] is DBNull) ? null : dtRow["FullName"].ToString();
                                dataRow.DepartmentName = (dtRow["ClassName"] is DBNull) ? null : dtRow["ClassName"].ToString();
                                dataRow.NameOnMachine = (dtRow["NameOnMachine"] is DBNull) ? null : dtRow["NameOnMachine"].ToString();
                                dataRow.CardNumber = (dtRow["CardNumber"] is DBNull) ? null : dtRow["CardNumber"].ToString();
                                dataRow.Privilege = (dtRow["Privilege"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Privilege"].ToString());
                                dataRow.PrivilegeName = GetPrivilegeName(dataRow.Privilege.Value);
                                dataRow.Password = (dtRow["Password"] is DBNull) ? null : dtRow["Password"].ToString();
                                dataRow.FaceTemplate = (dtRow["FaceTemplate"] is DBNull) ? 0 : Convert.ToInt32(dtRow["FaceTemplate"].ToString());
                                dataRow.Finger1 = (dtRow["Finger1"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger1"].ToString());
                                dataRow.Finger2 = (dtRow["Finger2"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger2"].ToString());
                                dataRow.Finger3 = (dtRow["Finger3"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger3"].ToString());
                                dataRow.Finger4 = (dtRow["Finger4"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger4"].ToString());
                                dataRow.Finger5 = (dtRow["Finger5"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger5"].ToString());
                                dataRow.Finger6 = (dtRow["Finger6"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger6"].ToString());
                                dataRow.Finger7 = (dtRow["Finger7"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger7"].ToString());
                                dataRow.Finger8 = (dtRow["Finger8"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger8"].ToString());
                                dataRow.Finger9 = (dtRow["Finger9"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger9"].ToString());
                                dataRow.Finger10 = (dtRow["Finger10"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger10"].ToString());
                                dataRow.AliasName = (dtRow["AliasName"] is DBNull) ? null : dtRow["AliasName"].ToString();
                                dataRow.SerialNumber = (dtRow["SerialNumber"] is DBNull) ? null : dtRow["SerialNumber"].ToString();

                                resutlData.Add(dataRow);
                            }

                        }
                    }
                }

            }


            return resutlData;
        }

        public List<IC_EmployeeDTO> GetParentMasterInfoMany(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new List<IC_EmployeeDTO>();


            string select = "SELECT distinct ";
            select += "\n" + "u.EmployeeATID,u.EmployeeCode, u.FullName, ISNULL(us.NameOnMachine, '') as NameOnMachine, ISNULL(c.CardNumber, '') as CardNumber, ";
            select += "\n" + "ISNULL(us.Privilege, 0) AS Privilege, ISNULL(us.[Password], '') AS [Password], p.Students as StudentName, ";
            select += "\n" + "(case when DATALENGTH(us.FaceTemplate) > 0  then ISNULL(DATALENGTH(us.FaceTemplate), 0) else ISNULL(DATALENGTH(us.FaceV2_Content), 0) end) AS FaceTemplate,";
            select += "\n" + "ISNULL(LEN(us.FingerData0), 0) as Finger1, ISNULL(LEN(us.FingerData1), 0) as Finger2, ISNULL(LEN(us.FingerData2), 0) as Finger3, ISNULL(LEN(us.FingerData3), 0) as Finger4, ISNULL(LEN(us.FingerData4), 0) as Finger5,";
            select += "\n" + "ISNULL(LEN(us.FingerData5), 0) as Finger6, ISNULL(LEN(us.FingerData6), 0) as Finger7, ISNULL(LEN(us.FingerData7), 0) as Finger8, ISNULL(LEN(us.FingerData8), 0) as Finger9, ISNULL(LEN(us.FingerData9), 0) as Finger10,'' AS AliasName, '' AS SerialNumber ";


            string from = "\n" + "From HR_User u ";
            from += "\n" + "JOIN HR_ParentInfo p on u.EmployeeATID = p.EmployeeATID and u.CompanyIndex = p.CompanyIndex ";
            from += "\n" + "LEFT JOIN IC_UserMaster us on u.EmployeeATID = us.EmployeeATID and u.CompanyIndex = us.CompanyIndex ";
            from += "\n" + "LEFT JOIN HR_CardNumberInfo c on u.EmployeeATID = c.EmployeeATID and u.CompanyIndex = c.CompanyIndex and c.IsActive = 1 ";

            string where = "\n" + $"WHERE 1 = 1 AND ";

            DateTime maxToday = CommonUtils.GetMaxToday();
            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "Filter":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                string filter = param.Value.ToString();
                                where += " (u.FullName like '%" + filter + "%' OR ";
                                where += " u.EmployeeCode like '%" + filter + "%' OR ";
                                where += " u.EmployeeATID like '%" + filter + "%' OR ";
                                where += " p.Students like '%" + filter + "%' OR ";
                                where += " us.NameOnMachine like '%" + filter + "%' OR ";
                                where += " c.CardNumber like '%" + filter + "%') AND ";
                            }
                            break;
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(param.Value);
                                where += " u.CompanyIndex = " + companyIndex.ToString() + " AND ";
                            }
                            break;
                        case "ListEmployeeATID":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                IList<string> listEmployeeID = (IList<string>)param.Value;
                                where += "u.EmployeeATID IN ('" + string.Join("','", listEmployeeID) + "') AND ";
                            }
                            break;
                        case "EmployeeATID":
                            if (param.Value != null)
                            {
                                string employeeID = param.Value.ToString();
                                where += "u.EmployeeATID = '" + employeeID + "' AND ";
                            }
                            break;
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
                                dataRow.EmployeeATID = (dtRow["EmployeeATID"] is DBNull) ? null : dtRow["EmployeeATID"].ToString();
                                dataRow.EmployeeCode = (dtRow["EmployeeCode"] is DBNull) ? null : dtRow["EmployeeCode"].ToString();
                                dataRow.FullName = (dtRow["FullName"] is DBNull) ? null : dtRow["FullName"].ToString();
                                dataRow.DepartmentName = (dtRow["StudentName"] is DBNull) ? null : dtRow["StudentName"].ToString();
                                dataRow.NameOnMachine = (dtRow["NameOnMachine"] is DBNull) ? null : dtRow["NameOnMachine"].ToString();
                                dataRow.CardNumber = (dtRow["CardNumber"] is DBNull) ? null : dtRow["CardNumber"].ToString();
                                dataRow.Privilege = (dtRow["Privilege"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Privilege"].ToString());
                                dataRow.PrivilegeName = GetPrivilegeName(dataRow.Privilege.Value);
                                dataRow.Password = (dtRow["Password"] is DBNull) ? null : dtRow["Password"].ToString();
                                dataRow.FaceTemplate = (dtRow["FaceTemplate"] is DBNull) ? 0 : Convert.ToInt32(dtRow["FaceTemplate"].ToString());
                                dataRow.Finger1 = (dtRow["Finger1"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger1"].ToString());
                                dataRow.Finger2 = (dtRow["Finger2"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger2"].ToString());
                                dataRow.Finger3 = (dtRow["Finger3"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger3"].ToString());
                                dataRow.Finger4 = (dtRow["Finger4"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger4"].ToString());
                                dataRow.Finger5 = (dtRow["Finger5"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger5"].ToString());
                                dataRow.Finger6 = (dtRow["Finger6"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger6"].ToString());
                                dataRow.Finger7 = (dtRow["Finger7"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger7"].ToString());
                                dataRow.Finger8 = (dtRow["Finger8"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger8"].ToString());
                                dataRow.Finger9 = (dtRow["Finger9"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger9"].ToString());
                                dataRow.Finger10 = (dtRow["Finger10"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger10"].ToString());
                                dataRow.AliasName = (dtRow["AliasName"] is DBNull) ? null : dtRow["AliasName"].ToString();
                                dataRow.SerialNumber = (dtRow["SerialNumber"] is DBNull) ? null : dtRow["SerialNumber"].ToString();

                                resutlData.Add(dataRow);
                            }

                        }
                    }
                }

            }


            return resutlData;
        }

        public async Task<List<IC_EmployeeDTO>> GetCustomerMasterInfoMany(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new List<IC_EmployeeDTO>();


            string select = "SELECT distinct ";
            select += "\n" + "u.EmployeeATID,u.EmployeeCode, u.FullName, w.FromDate, w.ToDate, w.DepartmentIndex, ci.FromTime as CustomerFromTime, ci.ToTime as CustomerToTime, ci.ContactDepartment, ISNULL(us.NameOnMachine, '') as NameOnMachine, ISNULL(c.CardNumber, '') as CardNumber, ";
            select += "\n" + "ISNULL(us.Privilege, 0) AS Privilege, ISNULL(us.[Password], '') AS [Password], ci.NRIC, ";
            select += "\n" + "(case when DATALENGTH(us.FaceTemplate) > 0  then ISNULL(DATALENGTH(us.FaceTemplate), 0) else ISNULL(DATALENGTH(us.FaceV2_Content), 0) end) AS FaceTemplate,";
            select += "\n" + "ISNULL(LEN(us.FingerData0), 0) as Finger1, ISNULL(LEN(us.FingerData1), 0) as Finger2, ISNULL(LEN(us.FingerData2), 0) as Finger3, ISNULL(LEN(us.FingerData3), 0) as Finger4, ISNULL(LEN(us.FingerData4), 0) as Finger5,";
            select += "\n" + "ISNULL(LEN(us.FingerData5), 0) as Finger6, ISNULL(LEN(us.FingerData6), 0) as Finger7, ISNULL(LEN(us.FingerData7), 0) as Finger8, ISNULL(LEN(us.FingerData8), 0) as Finger9, ISNULL(LEN(us.FingerData9), 0) as Finger10,'' AS AliasName, '' AS SerialNumber ";


            string from = "\n" + "From HR_User u ";
            from += "\n" + "LEFT JOIN HR_CustomerInfo ci on u.EmployeeATID = ci.EmployeeATID and u.CompanyIndex = ci.CompanyIndex ";
            from += "\n" + "LEFT JOIN IC_UserMaster us on u.EmployeeATID = us.EmployeeATID and u.CompanyIndex = us.CompanyIndex ";
            from += "\n" + "LEFT JOIN HR_CardNumberInfo c on u.EmployeeATID = c.EmployeeATID and u.CompanyIndex = c.CompanyIndex and c.IsActive = 1 ";
            from += "\n" + "LEFT JOIN dbo.IC_WorkingInfo w ON u.EmployeeATID = w.EmployeeATID AND u.CompanyIndex = w.CompanyIndex ";

            string where = "\n" + $"WHERE 1 = 1 AND ";

            DateTime maxToday = CommonUtils.GetMaxToday();
            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "Filter":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                string filter = param.Value.ToString();
                                var filterBy = filter.Split(" ").ToList();
                                where += " (u.FullName like '%" + filter + "%' OR ";
                                where += " u.EmployeeCode like '%" + filter + "%' OR ";
                                where += " u.EmployeeATID like '%" + filter + "%' OR ";
                                where += " ci.NRIC like '%" + filter + "%' OR ";
                                where += " us.NameOnMachine like '%" + filter + "%' OR ";
                                if (filterBy.Count > 0)
                                {
                                    where += " u.EmployeeATID in ('" + string.Join("','", filterBy) + "') OR ";
                                }
                                where += " c.CardNumber like '%" + filter + "%') AND ";
                            }

                            break;
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(param.Value);
                                where += " u.CompanyIndex = " + companyIndex.ToString() + " AND ";
                            }
                            break;
                        case "ListEmployeeATID":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                IList<string> listEmployeeID = (IList<string>)param.Value;
                                where += "u.EmployeeATID IN ('" + string.Join("','", listEmployeeID) + "') AND ";
                            }
                            break;
                        case "EmployeeATID":
                            if (param.Value != null)
                            {
                                string employeeID = param.Value.ToString();
                                where += "u.EmployeeATID = '" + employeeID + "' AND ";
                            }
                            break;
                        case "ListDepartment":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                IList<long> departments = (IList<long>)param.Value;
                                where += "(w.DepartmentIndex IN (" + string.Join(",", departments) + ") " +
                                    "OR ci.ContactDepartment IN (" + string.Join(",", departments) + ")) AND ";
                            }
                            break;
                        case "EmployeeType":
                            if (param.Value != null)
                            {
                                var employeeTye = param.Value.ToString();
                                where += " u.EmployeeType = " + employeeTye.ToString() + " AND ";
                            }
                            break;
                        case "ListEmployeeType":
                            if (param.Value != null)
                            {
                                var listEmployeeType = (IList<int>)param.Value;
                                where += " u.EmployeeType IN (" + String.Join(",", listEmployeeType) + ") AND ";
                            }
                            break;
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
                                dataRow.EmployeeATID = (dtRow["EmployeeATID"] is DBNull) ? null : dtRow["EmployeeATID"].ToString();
                                dataRow.EmployeeCode = (dtRow["EmployeeCode"] is DBNull) ? null : dtRow["EmployeeCode"].ToString();
                                dataRow.FullName = (dtRow["FullName"] is DBNull) ? null : dtRow["FullName"].ToString();
                                //dataRow.DepartmentName = (dtRow["NRIC"] is DBNull) ? null : dtRow["NRIC"].ToString();
                                dataRow.DepartmentIndex = (dtRow["DepartmentIndex"] is DBNull) || string.IsNullOrWhiteSpace(dtRow["DepartmentIndex"].ToString()) ? (((dtRow["ContactDepartment"] is DBNull) || string.IsNullOrWhiteSpace(dtRow["ContactDepartment"].ToString())) ? null : (long?)(long.Parse(dtRow["ContactDepartment"].ToString()))) : (long?)dtRow["DepartmentIndex"];
                                dataRow.NameOnMachine = (dtRow["NameOnMachine"] is DBNull) ? null : dtRow["NameOnMachine"].ToString();
                                dataRow.CardNumber = (dtRow["CardNumber"] is DBNull) ? null : dtRow["CardNumber"].ToString();
                                dataRow.Privilege = (dtRow["Privilege"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Privilege"].ToString());
                                dataRow.PrivilegeName = GetPrivilegeName(dataRow.Privilege.Value);
                                dataRow.Password = (dtRow["Password"] is DBNull) ? null : dtRow["Password"].ToString();
                                dataRow.FaceTemplate = (dtRow["FaceTemplate"] is DBNull) ? 0 : Convert.ToInt32(dtRow["FaceTemplate"].ToString());
                                dataRow.Finger1 = (dtRow["Finger1"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger1"].ToString());
                                dataRow.Finger2 = (dtRow["Finger2"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger2"].ToString());
                                dataRow.Finger3 = (dtRow["Finger3"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger3"].ToString());
                                dataRow.Finger4 = (dtRow["Finger4"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger4"].ToString());
                                dataRow.Finger5 = (dtRow["Finger5"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger5"].ToString());
                                dataRow.Finger6 = (dtRow["Finger6"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger6"].ToString());
                                dataRow.Finger7 = (dtRow["Finger7"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger7"].ToString());
                                dataRow.Finger8 = (dtRow["Finger8"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger8"].ToString());
                                dataRow.Finger9 = (dtRow["Finger9"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger9"].ToString());
                                dataRow.Finger10 = (dtRow["Finger10"] is DBNull) ? 0 : Convert.ToInt32(dtRow["Finger10"].ToString());
                                dataRow.AliasName = (dtRow["AliasName"] is DBNull) ? null : dtRow["AliasName"].ToString();
                                dataRow.SerialNumber = (dtRow["SerialNumber"] is DBNull) ? null : dtRow["SerialNumber"].ToString();
                                dataRow.JoinedDate = (dtRow["FromDate"] is DBNull) ? ((dtRow["CustomerFromTime"] is DBNull) ? new DateTime() : ((DateTime)dtRow["CustomerFromTime"])) : ((DateTime)dtRow["FromDate"]);
                                dataRow.StoppedDate = (dtRow["ToDate"] is DBNull) ? ((dtRow["CustomerToTime"] is DBNull) ? null : ((DateTime?)dtRow["CustomerToTime"])) : ((DateTime?)dtRow["ToDate"]);

                                resutlData.Add(dataRow);
                            }

                        }
                    }
                }

            }

            var listDepartmentIndex = resutlData.Where(x => x.DepartmentIndex.HasValue).Select(x => x.DepartmentIndex.Value).ToList();
            var listDepartment = new List<IC_Department>();
            if (listDepartmentIndex.Count > 0)
            {
                listDepartment = await _dbContext.IC_Department.Where(x => listDepartmentIndex.Contains(x.Index)).ToListAsync();
            }

            resutlData.ForEach(x =>
            {
                if (!x.StoppedDate.HasValue || (x.StoppedDate.HasValue 
                    && (x.StoppedDate.Value == new DateTime() || x.StoppedDate.Value > DateTime.Now)))
                {
                    x.Status = "IsWorking";
                }
                else
                {
                    x.Status = "StoppedWork";
                }
                if (x.DepartmentIndex.HasValue)
                {
                    x.DepartmentName = listDepartment.FirstOrDefault(y => y.Index == x.DepartmentIndex)?.Name ?? string.Empty;
                }
            });

            return resutlData;
        }

        public IC_UserMasterDTO Get(int index)
        {
            return null;
        }

        public IC_UserMasterDTO GetExist(string employeeID, int companyIndex)
        {

            var data = _dbContext.IC_UserMaster.FirstOrDefault(e => e.EmployeeATID == employeeID && e.CompanyIndex == companyIndex);
            if (data != null)
                return ConvertToDTO(data);
            else
                return new IC_UserMasterDTO();
        }

        public List<IC_UserMasterDTO> UpdateList(List<IC_UserMasterDTO> listUserMaster)
        {

            var listExisted = _dbContext.IC_UserMaster.Where(e => listUserMaster.Where(u => u.EmployeeATID == e.EmployeeATID && u.CompanyIndex == e.CompanyIndex).Count() > 0).ToList();

            foreach (var user in listUserMaster)
            {
                var data = listExisted.FirstOrDefault(e => e.EmployeeATID == user.EmployeeATID);
                if (data != null)
                {
                    ConvertDTOToUpdateData(user, data);
                    _dbContext.Update(data);
                }
            }
            _dbContext.SaveChanges();
            return listUserMaster;
        }

        public List<IC_UserMasterDTO> SaveOrUpdateList(List<IC_UserMasterDTO> listUserMaster)
        {
            var listExisted = _dbContext.IC_UserMaster.Where(e => listUserMaster.Where(u => u.EmployeeATID == e.EmployeeATID && u.CompanyIndex == e.CompanyIndex).Count() > 0).ToList();
            foreach (var user in listUserMaster)
            {
                var data = listExisted.FirstOrDefault(e => e.EmployeeATID == user.EmployeeATID);
                if (data != null)
                {
                    ConvertDTOToUpdateData(user, data);
                    _dbContext.IC_UserMaster.Update(data);
                }
                else
                {
                    data = new IC_UserMaster();
                    ConvertDTOToNewData(user, data);
                    _dbContext.IC_UserMaster.Add(data);
                }
            }
            _dbContext.SaveChanges();
            return listUserMaster;
        }

        public async Task<List<IC_UserMasterDTO>> SaveAndAddMoreList(List<IC_UserMasterDTO> listUserMaster,
            List<UserSyncAuthMode> authModes = null,
            TargetDownloadUser targetDownloadUser = TargetDownloadUser.AllUser)
        {
            if (listUserMaster == null || listUserMaster.Count() == 0)
                return null;

            var listEmployeeATID = listUserMaster.Select(e => e.EmployeeATID).ToList();

            var listExisted = _dbContext.IC_UserMaster.Where(e => listEmployeeATID.Contains(e.EmployeeATID) && e.CompanyIndex == listUserMaster.First().CompanyIndex).ToList();
            foreach (var user in listUserMaster)
            {
                var data = listExisted.FirstOrDefault(e => e.EmployeeATID == user.EmployeeATID);
                if (data != null)
                {
                    if (targetDownloadUser == TargetDownloadUser.AllUser)
                    {
                        ConvertDTOToAddMore(user, data, authModes);
                        _dbContext.IC_UserMaster.Update(data);
                    }
                }
                else
                {
                    data = new IC_UserMaster();
                    ConvertDTOToNewData(user, data);
                    _dbContext.IC_UserMaster.Add(data);
                }
            }
            _dbContext.SaveChanges();
            return listUserMaster;
        }

        public async Task<List<IC_UserMasterDTO>> SaveAndOverwriteList(List<IC_UserMasterDTO> listUserMaster,
            List<UserSyncAuthMode> authModes = null,
            TargetDownloadUser targetDownloadUser = TargetDownloadUser.AllUser)
        {
            if (listUserMaster == null || listUserMaster.Count() == 0)
                return null;
            var listEmployeeATID = listUserMaster.Select(e => e.EmployeeATID).ToList();
            var listExisted = await _dbContext.IC_UserMaster.Where(e => listEmployeeATID.Contains(e.EmployeeATID) && e.CompanyIndex == listUserMaster.First().CompanyIndex).ToListAsync();
            var lstEmployeeATIDs = new List<string>();
            foreach (var user in listUserMaster)
            {
                var data = listExisted.FirstOrDefault(e => e.EmployeeATID == user.EmployeeATID);
                if (data != null)
                {
                    if (targetDownloadUser == TargetDownloadUser.AllUser)
                    {
                        if (user.FaceV2_Content != null && data.FaceV2_Content == null)
                        {
                            lstEmployeeATIDs.Add(user.EmployeeATID);
                        }
                        ConvertDTOToOverwrite(user, data, authModes);
                        _dbContext.IC_UserMaster.Update(data);
                    }

                }
                else
                {
                    data = new IC_UserMaster();
                    ConvertDTOToNewData(user, data, authModes);
                    if(user.FaceV2_Content != null)
                    {
                        lstEmployeeATIDs.Add(user.EmployeeATID);
                    }
                    await _dbContext.IC_UserMaster.AddAsync(data);
                }
            }
            await _dbContext.SaveChangesAsync();
            return listUserMaster;
        }

        public IC_UserMasterDTO Update(IC_UserMasterDTO userMaster)
        {

            var data = _dbContext.IC_UserMaster.FirstOrDefault(e => userMaster.EmployeeATID == e.EmployeeATID && userMaster.CompanyIndex == e.CompanyIndex);
            if (data != null)
            {
                ConvertDTOToUpdateData(userMaster, data);
                _dbContext.Update(data);
                _dbContext.SaveChanges();
            }

            return userMaster;
        }

        public IC_UserMasterDTO UpdateField(List<AddedParam> addParams)
        {
            AddedParam addParam = addParams.FirstOrDefault(a => a.Key == "EmployeeATID");
            AddedParam companyParam = addParams.FirstOrDefault(a => a.Key == "CompanyIndex");
            if (addParam == null)
                return null;
            addParams.Remove(addParam);
            var employeeID = addParam.Value.ToString();
            var companyIndex = Convert.ToInt32(companyParam.Value);
            var dataItem = _dbContext.IC_UserMaster.FirstOrDefault(e => e.EmployeeATID == employeeID && e.CompanyIndex == companyIndex);
            if (dataItem != null)
            {
                foreach (AddedParam p in addParams)
                {
                    switch (p.Key)
                    {
                        case "Password":
                            string password = string.Empty;
                            if (p.Value != null)
                            {
                                password = p.Value.ToString();
                            }
                            dataItem.Password = password;
                            break;
                        case "NameOnMachine":
                            string nameOnMachine = string.Empty;
                            if (p.Value != null)
                            {
                                nameOnMachine = p.Value.ToString();
                            }
                            dataItem.NameOnMachine = nameOnMachine;
                            break;
                        case "CardNumber":
                            string cardNumber = string.Empty;
                            if (p.Value != null)
                            {
                                cardNumber = p.Value.ToString();
                            }
                            dataItem.CardNumber = cardNumber;
                            break;
                        case "FingerVersion":
                            string fingerVersion = string.Empty;
                            if (p.Value != null)
                            {
                                fingerVersion = p.Value.ToString();
                            }
                            dataItem.FingerVersion = fingerVersion;
                            break;

                        case "FaceVersion":
                            string faceVersion = string.Empty;
                            if (p.Value != null)
                            {
                                faceVersion = p.Value.ToString();
                            }
                            dataItem.FaceVersion = faceVersion;
                            break;

                    }
                }
                _dbContext.IC_UserMaster.Update(dataItem);
            }
            _dbContext.SaveChanges();
            var dto = ConvertToDTO(dataItem);
            return dto;
        }

        public IC_UserMasterDTO SaveOrUpdate(IC_UserMasterDTO item)
        {
            IC_UserMaster dataItem = _dbContext.IC_UserMaster.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID && e.CompanyIndex == item.CompanyIndex);
            if (dataItem != null)
            {
                //set logic when update UserMaster
                dataItem.UpdatedDate = DateTime.Now;
            }
            else
            {
                //set logic when create new Department
                dataItem = new IC_UserMaster();
                dataItem.CreatedDate = DateTime.Now;
            }

            if (item.Index != 0)
            {
                ConvertDTOToUpdateData(item, dataItem);
                _dbContext.IC_UserMaster.Update(dataItem);
            }
            else
            {
                ConvertDTOToNewData(item, dataItem);
                _dbContext.IC_UserMaster.Add(dataItem);
            }
            _dbContext.SaveChanges();
            item = ConvertToDTO(dataItem);
            return item;
        }

        public string GetFingerData(List<FingerInfo> listFinger, int fingerIndex)
        {
            if (listFinger != null)
            {
                var finger = listFinger.FirstOrDefault(e => e.FingerIndex == fingerIndex);
                if (finger != null)
                {
                    return finger.FingerTemplate;
                }
            }
            return "";
        }

        public string GetFingerDataListString(List<string> listFinger, int fingerIndex)
        {
            if (listFinger != null)
            {
                var finger = listFinger[fingerIndex];
                if (finger != null)
                {
                    return finger;
                }
            }
            return "";
        }

        public int GetFingerDataLength(List<FingerInfo> listFinger, int fingerIndex)
        {
            if (listFinger != null)
            {
                var finger = listFinger.FirstOrDefault(e => e.FingerIndex == fingerIndex);
                if (finger != null)
                {
                    return finger.FingerTemplate == null ? 0 : finger.FingerTemplate.Length;
                }
            }
            return 0;
        }

        public List<FingerInfo> BuildFingerData(IC_UserMasterDTO data)
        {
            List<FingerInfo> listFinger = new List<FingerInfo>();
            if (!string.IsNullOrWhiteSpace(data.FingerData0))
                listFinger.Add(new FingerInfo { FingerIndex = 0, FingerTemplate = data.FingerData0 });
            if (!string.IsNullOrWhiteSpace(data.FingerData1))
                listFinger.Add(new FingerInfo { FingerIndex = 1, FingerTemplate = data.FingerData1 });
            if (!string.IsNullOrWhiteSpace(data.FingerData2))
                listFinger.Add(new FingerInfo { FingerIndex = 2, FingerTemplate = data.FingerData2 });
            if (!string.IsNullOrWhiteSpace(data.FingerData3))
                listFinger.Add(new FingerInfo { FingerIndex = 3, FingerTemplate = data.FingerData3 });
            if (!string.IsNullOrWhiteSpace(data.FingerData4))
                listFinger.Add(new FingerInfo { FingerIndex = 4, FingerTemplate = data.FingerData4 });
            if (!string.IsNullOrWhiteSpace(data.FingerData5))
                listFinger.Add(new FingerInfo { FingerIndex = 5, FingerTemplate = data.FingerData5 });
            if (!string.IsNullOrWhiteSpace(data.FingerData6))
                listFinger.Add(new FingerInfo { FingerIndex = 6, FingerTemplate = data.FingerData6 });
            if (!string.IsNullOrWhiteSpace(data.FingerData7))
                listFinger.Add(new FingerInfo { FingerIndex = 7, FingerTemplate = data.FingerData7 });
            if (!string.IsNullOrWhiteSpace(data.FingerData8))
                listFinger.Add(new FingerInfo { FingerIndex = 8, FingerTemplate = data.FingerData8 });
            if (!string.IsNullOrWhiteSpace(data.FingerData9))
                listFinger.Add(new FingerInfo { FingerIndex = 9, FingerTemplate = data.FingerData9 });
            return listFinger;
        }

        public List<FingerInfo> BuildFingerData(IC_UserMaster data)
        {
            List<FingerInfo> listFinger = new List<FingerInfo>();
            if (!string.IsNullOrWhiteSpace(data.FingerData0))
                listFinger.Add(new FingerInfo { FingerIndex = 0, FingerTemplate = data.FingerData0 });
            if (!string.IsNullOrWhiteSpace(data.FingerData1))
                listFinger.Add(new FingerInfo { FingerIndex = 1, FingerTemplate = data.FingerData1 });
            if (!string.IsNullOrWhiteSpace(data.FingerData2))
                listFinger.Add(new FingerInfo { FingerIndex = 2, FingerTemplate = data.FingerData2 });
            if (!string.IsNullOrWhiteSpace(data.FingerData3))
                listFinger.Add(new FingerInfo { FingerIndex = 3, FingerTemplate = data.FingerData3 });
            if (!string.IsNullOrWhiteSpace(data.FingerData4))
                listFinger.Add(new FingerInfo { FingerIndex = 4, FingerTemplate = data.FingerData4 });
            if (!string.IsNullOrWhiteSpace(data.FingerData5))
                listFinger.Add(new FingerInfo { FingerIndex = 5, FingerTemplate = data.FingerData5 });
            if (!string.IsNullOrWhiteSpace(data.FingerData6))
                listFinger.Add(new FingerInfo { FingerIndex = 6, FingerTemplate = data.FingerData6 });
            if (!string.IsNullOrWhiteSpace(data.FingerData7))
                listFinger.Add(new FingerInfo { FingerIndex = 7, FingerTemplate = data.FingerData7 });
            if (!string.IsNullOrWhiteSpace(data.FingerData8))
                listFinger.Add(new FingerInfo { FingerIndex = 8, FingerTemplate = data.FingerData8 });
            if (!string.IsNullOrWhiteSpace(data.FingerData9))
                listFinger.Add(new FingerInfo { FingerIndex = 9, FingerTemplate = data.FingerData9 });
            return listFinger;
        }
        // favorites function

        public UserInfoPram CheckCreateOrUpdate(UserInfoPram listUserInfo, UserInfo userInfo)
        {

            var listEmployeeATID = listUserInfo.ListUserInfo.Select(e => e.UserID).ToList();
            var listUserMasterInDB = _dbContext.IC_UserMaster.Where(e => listEmployeeATID.Contains(e.EmployeeATID) && e.CompanyIndex == userInfo.CompanyIndex).ToList();

            foreach (var userMaster in listUserInfo.ListUserInfo)
            {
                // add user master
                var existUserMaster = listUserMasterInDB.FirstOrDefault(e => e.EmployeeATID == userMaster.UserID);
                if (existUserMaster == null)
                {
                    existUserMaster = new IC_UserMaster();
                    if (existUserMaster.NameOnMachine.Length > 20)
                    {
                        existUserMaster.NameOnMachine = userMaster.NameOnDevice.Substring(0, 20);
                    }
                    existUserMaster.CardNumber = userMaster.CardNumber;
                    existUserMaster.Privilege = (short)userMaster.Privilege;
                    existUserMaster.CompanyIndex = userInfo.CompanyIndex;
                    existUserMaster.EmployeeATID = userMaster.UserID.PadLeft(_config.MaxLenghtEmployeeATID, '0'); ;
                    existUserMaster.CreatedDate = DateTime.Now;
                    existUserMaster.UpdatedUser = userInfo.UserName;
                    _dbContext.IC_UserMaster.Add(existUserMaster);
                }
                else
                {
                    if (existUserMaster.NameOnMachine.Length > 20)
                    {
                        existUserMaster.NameOnMachine = userMaster.NameOnDevice.Substring(0, 20);
                    }
                    existUserMaster.CardNumber = userMaster.CardNumber;
                    existUserMaster.NameOnMachine = userMaster.NameOnDevice;
                    existUserMaster.Privilege = (short)userMaster.Privilege;
                    existUserMaster.UpdatedUser = userInfo.UserName;
                    existUserMaster.UpdatedDate = DateTime.Now;
                    _dbContext.IC_UserMaster.Update(existUserMaster);
                }

            }
            _dbContext.SaveChanges();
            return listUserInfo;
        }

        public List<string> CheckExistedOrCreate(UserInfoPram listUserInfo, UserInfo userInfo)
        {

            var listEmployeeATID = listUserInfo.ListUserInfo.Select(e => e.UserID).ToList();
            var listUserMasterInDB = _dbContext.IC_UserMaster.Where(e => listEmployeeATID.Contains(e.EmployeeATID) && e.CompanyIndex == userInfo.CompanyIndex).ToList();
            var listEmployeeTobeSync = new List<string>();

            foreach (var userMaster in listUserInfo.ListUserInfo)
            {
                // add user master
                var tobeSync = false;
                var existUserMaster = listUserMasterInDB.FirstOrDefault(e => e.EmployeeATID == userMaster.UserID);
                if (existUserMaster == null)
                {
                    existUserMaster = new IC_UserMaster();
                    if (existUserMaster.NameOnMachine.Length > 20)
                    {
                        existUserMaster.NameOnMachine = userMaster.NameOnDevice.Substring(0, 20);
                    }

                    existUserMaster.CardNumber = userMaster.CardNumber;
                    existUserMaster.Privilege = (short)userMaster.Privilege;
                    existUserMaster.CompanyIndex = userInfo.CompanyIndex;
                    existUserMaster.EmployeeATID = userMaster.UserID.PadLeft(_config.MaxLenghtEmployeeATID, '0'); ;
                    existUserMaster.CreatedDate = DateTime.Now;
                    existUserMaster.UpdatedUser = userInfo.UserName;
                    _dbContext.IC_UserMaster.Add(existUserMaster);
                    tobeSync = true;
                }

                if (tobeSync)
                {
                    listEmployeeTobeSync.Add(userMaster.UserID);
                }
            }
            _dbContext.SaveChanges();

            return listEmployeeTobeSync;
        }

        public IC_UserMasterDTO CheckExistedOrCreateDto(IC_UserMasterDTO userInfo, List<IC_UserMaster> userMasterLst, EPAD_Context db)
        {
            var existUserMaster = userMasterLst.FirstOrDefault(e => userInfo.EmployeeATID == e.EmployeeATID && e.CompanyIndex == userInfo.CompanyIndex);

            // add user master
            if (existUserMaster == null)
            {
                existUserMaster = new IC_UserMaster();
                if (userInfo.UpdatedUser == UpdatedUser.AutoIntegrateEmployee.ToString() && !string.IsNullOrEmpty(userInfo.NameOnMachine) && string.IsNullOrEmpty(existUserMaster.NameOnMachine))
                {
                    var s = GetNameOnMachineFromName(userInfo.NameOnMachine);
                    existUserMaster.NameOnMachine = s;
                }
                else
                {
                    existUserMaster.NameOnMachine = string.IsNullOrWhiteSpace(userInfo.NameOnMachine) ? "" : userInfo.NameOnMachine;
                    if (existUserMaster.NameOnMachine != null && existUserMaster.NameOnMachine.Length > 20)
                    {
                        existUserMaster.NameOnMachine = existUserMaster.NameOnMachine.Substring(0, 20);
                    }

                }
                existUserMaster.CompanyIndex = userInfo.CompanyIndex;
                existUserMaster.EmployeeATID = userInfo.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
                existUserMaster.CardNumber = userInfo.CardNumber;
                existUserMaster.Privilege = userInfo.Privilege.HasValue ? (short)userInfo.Privilege : (short)0;
                existUserMaster.AuthenMode = AuthenMode.FullAccessRight.ToString();

                existUserMaster.FaceIndex = (short)(userInfo.FaceIndex.HasValue ? userInfo.FaceIndex : 50);
                existUserMaster.CreatedDate = DateTime.Now;
                existUserMaster.UpdatedUser = userInfo.UpdatedUser;
                existUserMaster.UpdatedDate = DateTime.Now;

                existUserMaster.FaceV2_Index = userInfo.FaceV2_Index;
                existUserMaster.FaceV2_No = userInfo.FaceV2_No;
                existUserMaster.FaceV2_Format = userInfo.FaceV2_Format;
                existUserMaster.FaceV2_Duress = userInfo.FaceV2_Duress;
                existUserMaster.FaceV2_MajorVer = userInfo.FaceV2_MajorVer;
                existUserMaster.FaceV2_MinorVer = userInfo.FaceV2_MinorVer;
                existUserMaster.FaceV2_Size = userInfo.FaceV2_Size;
                existUserMaster.FaceV2_Type = userInfo.FaceV2_Type;
                existUserMaster.FaceV2_Valid = userInfo.FaceV2_Valid;
                existUserMaster.FaceV2_TemplateBIODATA = userInfo.FaceV2_TemplateBIODATA;
                existUserMaster.FaceV2_Content = userInfo.FaceV2_Content;
                if (_configClientName == ClientName.AEON.ToString())
                {
                    existUserMaster.EmployeeATID = userInfo.EmployeeATID;
                }

                db.IC_UserMaster.Add(existUserMaster);
            }
            else
            {
                if (userInfo.UpdatedUser == UpdatedUser.AutoIntegrateEmployee.ToString() && !string.IsNullOrEmpty(userInfo.NameOnMachine) && string.IsNullOrEmpty(existUserMaster.NameOnMachine))
                {
                    var s = GetNameOnMachineFromName(userInfo.NameOnMachine);
                    existUserMaster.NameOnMachine = s;
                }
                else if (string.IsNullOrEmpty(existUserMaster.NameOnMachine))
                {
                    existUserMaster.NameOnMachine = string.IsNullOrWhiteSpace(userInfo.NameOnMachine) ? "" : userInfo.NameOnMachine;
                    if (existUserMaster.NameOnMachine != null && existUserMaster.NameOnMachine.Length > 20)
                    {
                        existUserMaster.NameOnMachine = existUserMaster.NameOnMachine.Substring(0, 20);
                    }
                }
                existUserMaster.CompanyIndex = userInfo.CompanyIndex;
                existUserMaster.EmployeeATID = userInfo.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');

                existUserMaster.AuthenMode = AuthenMode.FullAccessRight.ToString();
                existUserMaster.CreatedDate = DateTime.Now;
                existUserMaster.UpdatedUser = userInfo.UpdatedUser;
                existUserMaster.UpdatedDate = DateTime.Now;

                if (userInfo.UpdatedUser != UpdatedUser.AutoIntegrateEmployee.ToString())
                {
                    existUserMaster.CardNumber = userInfo.CardNumber;
                    existUserMaster.Privilege = userInfo.Privilege.HasValue ? (short)userInfo.Privilege : (short)0;
                    existUserMaster.FaceIndex = (short)(userInfo.FaceIndex.HasValue ? userInfo.FaceIndex : 50);
                    existUserMaster.FaceV2_Index = userInfo.FaceV2_Index;
                    existUserMaster.FaceV2_No = userInfo.FaceV2_No;
                    existUserMaster.FaceV2_Format = userInfo.FaceV2_Format;
                    existUserMaster.FaceV2_Duress = userInfo.FaceV2_Duress;
                    existUserMaster.FaceV2_MajorVer = userInfo.FaceV2_MajorVer;
                    existUserMaster.FaceV2_MinorVer = userInfo.FaceV2_MinorVer;
                    existUserMaster.FaceV2_Size = userInfo.FaceV2_Size;
                    existUserMaster.FaceV2_Type = userInfo.FaceV2_Type;
                    existUserMaster.FaceV2_Valid = userInfo.FaceV2_Valid;
                    existUserMaster.FaceV2_TemplateBIODATA = userInfo.FaceV2_TemplateBIODATA;
                    existUserMaster.FaceV2_Content = userInfo.FaceV2_Content;
                }

                if (_configClientName == ClientName.AEON.ToString())
                {
                    existUserMaster.EmployeeATID = userInfo.EmployeeATID;
                }

                db.IC_UserMaster.Update(existUserMaster);
            }

            userInfo = ConvertToDTO(existUserMaster);
            return userInfo;
        }

        public IC_UserMasterDTO CheckExistedOrCreate(IC_UserMasterDTO userInfo)
        {
            var existUserMaster = _dbContext.IC_UserMaster.FirstOrDefault(e => userInfo.EmployeeATID == e.EmployeeATID && e.CompanyIndex == userInfo.CompanyIndex);
            // add user master

            if (existUserMaster == null)
            {
                existUserMaster = new IC_UserMaster();
                if (userInfo.UpdatedUser == UpdatedUser.AutoIntegrateEmployee.ToString() && !string.IsNullOrEmpty(userInfo.NameOnMachine) && string.IsNullOrEmpty(existUserMaster.NameOnMachine))
                {
                    existUserMaster.NameOnMachine = GetNameOnMachineFromName(userInfo.NameOnMachine);
                }
                existUserMaster.NameOnMachine = string.IsNullOrWhiteSpace(userInfo.NameOnMachine) ? "" : userInfo.NameOnMachine;
                if (existUserMaster.NameOnMachine != null && existUserMaster.NameOnMachine.Length > 20)
                {
                    existUserMaster.NameOnMachine = existUserMaster.NameOnMachine.Substring(0, 20);
                }
                existUserMaster.CompanyIndex = userInfo.CompanyIndex;
                existUserMaster.EmployeeATID = userInfo.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
                existUserMaster.CardNumber = userInfo.CardNumber;
                existUserMaster.Privilege = userInfo.Privilege.HasValue ? (short)userInfo.Privilege : (short)0;
                existUserMaster.AuthenMode = AuthenMode.FullAccessRight.ToString();

                existUserMaster.FaceIndex = (short)(userInfo.FaceIndex.HasValue ? userInfo.FaceIndex : 50);
                existUserMaster.CreatedDate = DateTime.Now;
                existUserMaster.UpdatedUser = userInfo.UpdatedUser;
                existUserMaster.UpdatedDate = DateTime.Now;

                existUserMaster.FaceV2_Index = userInfo.FaceV2_Index;
                existUserMaster.FaceV2_No = userInfo.FaceV2_No;
                existUserMaster.FaceV2_Format = userInfo.FaceV2_Format;
                existUserMaster.FaceV2_Duress = userInfo.FaceV2_Duress;
                existUserMaster.FaceV2_MajorVer = userInfo.FaceV2_MajorVer;
                existUserMaster.FaceV2_MinorVer = userInfo.FaceV2_MinorVer;
                existUserMaster.FaceV2_Size = userInfo.FaceV2_Size;
                existUserMaster.FaceV2_Type = userInfo.FaceV2_Type;
                existUserMaster.FaceV2_Valid = userInfo.FaceV2_Valid;
                existUserMaster.FaceV2_TemplateBIODATA = userInfo.FaceV2_TemplateBIODATA;
                existUserMaster.FaceV2_Content = userInfo.FaceV2_Content;

                existUserMaster.FingerData0 = userInfo.FingerData0;
                existUserMaster.FingerData1 = userInfo.FingerData1;
                existUserMaster.FingerData2 = userInfo.FingerData2;
                existUserMaster.FingerData3 = userInfo.FingerData3;
                existUserMaster.FingerData4 = userInfo.FingerData4;
                existUserMaster.FingerData5 = userInfo.FingerData5;
                existUserMaster.FingerData6 = userInfo.FingerData6;
                existUserMaster.FingerData7 = userInfo.FingerData7;
                existUserMaster.FingerData8 = userInfo.FingerData8;
                existUserMaster.FingerData9 = userInfo.FingerData9;

                _dbContext.IC_UserMaster.Add(existUserMaster);
            }
            else if (userInfo.UpdatedUser != UpdatedUser.AutoIntegrateEmployee.ToString())
            {
                if (userInfo.UpdatedUser == UpdatedUser.AutoIntegrateEmployee.ToString() && !string.IsNullOrEmpty(userInfo.NameOnMachine) && string.IsNullOrEmpty(existUserMaster.NameOnMachine))
                {
                    existUserMaster.NameOnMachine = GetNameOnMachineFromName(userInfo.NameOnMachine);
                }
                existUserMaster.NameOnMachine = string.IsNullOrWhiteSpace(userInfo.NameOnMachine) ? "" : userInfo.NameOnMachine;
                if (existUserMaster.NameOnMachine != null && existUserMaster.NameOnMachine.Length > 20)
                {
                    existUserMaster.NameOnMachine = existUserMaster.NameOnMachine.Substring(0, 20);
                }
                existUserMaster.CompanyIndex = userInfo.CompanyIndex;
                existUserMaster.EmployeeATID = userInfo.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
                existUserMaster.CardNumber = userInfo.CardNumber;
                existUserMaster.Privilege = userInfo.Privilege.HasValue ? (short)userInfo.Privilege : (short)0;
                existUserMaster.AuthenMode = AuthenMode.FullAccessRight.ToString();

                existUserMaster.FaceIndex = (short)(userInfo.FaceIndex.HasValue ? userInfo.FaceIndex : 50);
                existUserMaster.CreatedDate = DateTime.Now;
                existUserMaster.UpdatedUser = userInfo.UpdatedUser;
                existUserMaster.UpdatedDate = DateTime.Now;

                existUserMaster.FaceV2_Index = userInfo.FaceV2_Index;
                existUserMaster.FaceV2_No = userInfo.FaceV2_No;
                existUserMaster.FaceV2_Format = userInfo.FaceV2_Format;
                existUserMaster.FaceV2_Duress = userInfo.FaceV2_Duress;
                existUserMaster.FaceV2_MajorVer = userInfo.FaceV2_MajorVer;
                existUserMaster.FaceV2_MinorVer = userInfo.FaceV2_MinorVer;
                existUserMaster.FaceV2_Size = userInfo.FaceV2_Size;
                existUserMaster.FaceV2_Type = userInfo.FaceV2_Type;
                existUserMaster.FaceV2_Valid = userInfo.FaceV2_Valid;
                existUserMaster.FaceV2_TemplateBIODATA = userInfo.FaceV2_TemplateBIODATA;
                existUserMaster.FaceV2_Content = userInfo.FaceV2_Content;

                existUserMaster.FingerData0 = userInfo.FingerData0;
                existUserMaster.FingerData1 = userInfo.FingerData1;
                existUserMaster.FingerData2 = userInfo.FingerData2;
                existUserMaster.FingerData3 = userInfo.FingerData3;
                existUserMaster.FingerData4 = userInfo.FingerData4;
                existUserMaster.FingerData5 = userInfo.FingerData5;
                existUserMaster.FingerData6 = userInfo.FingerData6;
                existUserMaster.FingerData7 = userInfo.FingerData7;
                existUserMaster.FingerData8 = userInfo.FingerData8;
                existUserMaster.FingerData9 = userInfo.FingerData9;

                _dbContext.IC_UserMaster.Update(existUserMaster);
            }
            _dbContext.SaveChanges();
            userInfo = ConvertToDTO(existUserMaster);
            return userInfo;
        }


        public string RemoveUnicode(string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
    "đ",
    "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
    "í","ì","ỉ","ĩ","ị",
    "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
    "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
    "ý","ỳ","ỷ","ỹ","ỵ",};
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
    "d",
    "e","e","e","e","e","e","e","e","e","e","e",
    "i","i","i","i","i",
    "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
    "u","u","u","u","u","u","u","u","u","u","u",
    "y","y","y","y","y",};
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text;
        }

        private string GetNameOnMachineFromName(string ht)
        {
            return RemoveUnicode(ht);
        }

        private string GetPrivilegeName(int privilege)
        {
            switch (privilege)
            {
                case GlobalParams.DevicePrivilege.PUSHAdminRole:
                case GlobalParams.DevicePrivilege.SDKAdminRole:
                    return "Quản trị viên"; // Recommend using language key instead of.
                case GlobalParams.DevicePrivilege.SDKUserRegisterRole:
                    return "Quyền đăng ký";
            }
            return "Người dùng";
        }

        private IC_UserMasterDTO ConvertToDTO(IC_UserMaster data)
        {
            var dto = new IC_UserMasterDTO();
            dto.EmployeeATID = data.EmployeeATID;
            dto.CompanyIndex = data.CompanyIndex;
            dto.NameOnMachine = data.NameOnMachine;
            dto.CardNumber = data.CardNumber;
            dto.Password = data.Password;
            dto.Privilege = data.Privilege;
            dto.AuthenMode = data.AuthenMode;

            dto.FingerData0 = data.FingerData0;
            dto.FingerData1 = data.FingerData1;
            dto.FingerData2 = data.FingerData2;
            dto.FingerData3 = data.FingerData3;
            dto.FingerData4 = data.FingerData4;
            dto.FingerData5 = data.FingerData5;
            dto.FingerData6 = data.FingerData6;
            dto.FingerData7 = data.FingerData7;
            dto.FingerData8 = data.FingerData8;
            dto.FingerData9 = data.FingerData9;
            dto.FingerVersion = data.FingerVersion;
            // face
            dto.FaceIndex = (short)(data.FaceIndex.HasValue ? data.FaceIndex : 50);
            dto.FaceTemplate = data.FaceTemplate;
            // face v2
            dto.FaceV2_Index = data.FaceV2_Index;
            dto.FaceV2_No = data.FaceV2_No;
            dto.FaceV2_Format = data.FaceV2_Format;
            dto.FaceV2_Duress = data.FaceV2_Duress;
            dto.FaceV2_MajorVer = data.FaceV2_MajorVer;
            dto.FaceV2_MinorVer = data.FaceV2_MinorVer;
            dto.FaceV2_Size = data.FaceV2_Size;
            dto.FaceV2_Type = data.FaceV2_Type;
            dto.FaceV2_Valid = data.FaceV2_Valid;
            dto.FaceV2_TemplateBIODATA = dto.FaceV2_TemplateBIODATA;
            dto.FaceV2_Content = dto.FaceV2_Content;
            dto.FaceVersion = data.FaceVersion;

            dto.CreatedDate = data.CreatedDate;
            dto.UpdatedDate = data.UpdatedDate;
            dto.UpdatedUser = data.UpdatedUser;
            return dto;
        }

        private void ConvertDTOToUpdateData(IC_UserMasterDTO dto, IC_UserMaster data)
        {
            data.NameOnMachine = dto.NameOnMachine;
            data.CardNumber = dto.CardNumber;
            data.Password = dto.Password;
            data.FingerVersion = dto.FingerVersion;
            data.AuthenMode = dto.AuthenMode;

            data.FingerData0 = dto.FingerData0;
            data.FingerData1 = dto.FingerData1;
            data.FingerData2 = dto.FingerData2;
            data.FingerData3 = dto.FingerData3;
            data.FingerData4 = dto.FingerData4;
            data.FingerData5 = dto.FingerData5;
            data.FingerData6 = dto.FingerData6;
            data.FingerData7 = dto.FingerData7;
            data.FingerData8 = dto.FingerData8;
            data.FingerData9 = dto.FingerData9;
            data.FingerVersion = dto.FingerVersion;
            // face
            data.FaceIndex = (short)dto.FaceIndex;
            data.FaceTemplate = dto.FaceTemplate;
            // face v2
            data.FaceV2_Index = dto.FaceV2_Index;
            data.FaceV2_No = dto.FaceV2_No;
            data.FaceV2_Format = dto.FaceV2_Format;
            data.FaceV2_Duress = dto.FaceV2_Duress;
            data.FaceV2_MajorVer = dto.FaceV2_MajorVer;
            data.FaceV2_MinorVer = dto.FaceV2_MinorVer;
            data.FaceV2_Size = dto.FaceV2_Size;
            data.FaceV2_Type = dto.FaceV2_Type;
            data.FaceV2_Valid = dto.FaceV2_Valid;
            data.FaceV2_TemplateBIODATA = dto.FaceV2_TemplateBIODATA;
            data.FaceV2_Content = dto.FaceV2_Content;
            data.FaceVersion = dto.FaceVersion;
            if (dto.Index > 0)
            {
                data.UpdatedDate = DateTime.Today;
            }
            else
            {
                data.CreatedDate = DateTime.Today;
            }
            data.UpdatedUser = dto.UpdatedUser;
        }

        private void ConvertDTOToNewData(IC_UserMasterDTO dto, IC_UserMaster data, List<UserSyncAuthMode> authModes = null)
        {

            data.EmployeeATID = dto.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
            data.CompanyIndex = dto.CompanyIndex;
            data.NameOnMachine = dto.NameOnMachine;
            if (authModes == null || authModes.Contains(UserSyncAuthMode.CardNumber))
            {
                data.CardNumber = dto.CardNumber;
            }
            if (authModes == null || authModes.Contains(UserSyncAuthMode.Password))
            {
                data.Password = dto.Password;
            }
            data.Privilege = dto.Privilege;
            data.AuthenMode = dto.AuthenMode;
            if (authModes == null || authModes.Contains(UserSyncAuthMode.Finger))
            {
                data.FingerData0 = dto.FingerData0;
                data.FingerData1 = dto.FingerData1;
                data.FingerData2 = dto.FingerData2;
                data.FingerData3 = dto.FingerData3;
                data.FingerData4 = dto.FingerData4;
                data.FingerData5 = dto.FingerData5;
                data.FingerData6 = dto.FingerData6;
                data.FingerData7 = dto.FingerData7;
                data.FingerData8 = dto.FingerData8;
                data.FingerData9 = dto.FingerData9;
                data.FingerVersion = dto.FingerVersion;
            }

            if (authModes == null || authModes.Contains(UserSyncAuthMode.Face))
            {
                // face
                data.FaceIndex = dto.FaceIndex.HasValue ? (short)dto.FaceIndex.Value : (short)50;
                data.FaceTemplate = dto.FaceTemplate;
                // face v2
                data.FaceV2_Index = dto.FaceV2_Index;
                data.FaceV2_No = dto.FaceV2_No;
                data.FaceV2_Format = dto.FaceV2_Format;
                data.FaceV2_Duress = dto.FaceV2_Duress;
                data.FaceV2_MajorVer = dto.FaceV2_MajorVer;
                data.FaceV2_MinorVer = dto.FaceV2_MinorVer;
                data.FaceV2_Size = dto.FaceV2_Size;
                data.FaceV2_Type = dto.FaceV2_Type;
                data.FaceV2_Valid = dto.FaceV2_Valid;
                data.FaceV2_TemplateBIODATA = dto.FaceV2_TemplateBIODATA;
                data.FaceV2_Content = dto.FaceV2_Content;
                data.FaceVersion = dto.FaceVersion;
            }

            if (dto.Index > 0)
            {
                data.UpdatedDate = DateTime.Today;
            }
            else
            {
                data.CreatedDate = DateTime.Today;
            }
            data.UpdatedUser = dto.UpdatedUser;
        }

        private void ConvertDTOToOverwrite(IC_UserMasterDTO dto, IC_UserMaster data, List<UserSyncAuthMode> authModes = null)
        {

            data.NameOnMachine = string.IsNullOrWhiteSpace(dto.NameOnMachine) ? data.NameOnMachine : dto.NameOnMachine;
            if (data.NameOnMachine != null && data.NameOnMachine.Length > 20)
            {
                data.NameOnMachine = data.NameOnMachine.Substring(0, 20);
            }
            if (authModes == null || authModes.Contains(UserSyncAuthMode.CardNumber))
            {
                data.CardNumber = string.IsNullOrWhiteSpace(dto.CardNumber) || data.CardNumber == "0" ? data.CardNumber : dto.CardNumber;
            }
            if (authModes == null || authModes.Contains(UserSyncAuthMode.Password))
            {
                data.Password = string.IsNullOrWhiteSpace(dto.Password) ? data.Password : dto.Password;
            }
            data.Privilege = !dto.Privilege.HasValue ? data.Privilege : dto.Privilege.Value;
            data.AuthenMode = string.IsNullOrWhiteSpace(dto.AuthenMode) ? data.AuthenMode : dto.AuthenMode;

            if (authModes == null || authModes.Contains(UserSyncAuthMode.Finger))
            {
                data.FingerData0 = string.IsNullOrWhiteSpace(dto.FingerData0) ? data.FingerData0 : dto.FingerData0;
                data.FingerData1 = string.IsNullOrWhiteSpace(dto.FingerData1) ? data.FingerData1 : dto.FingerData1;
                data.FingerData2 = string.IsNullOrWhiteSpace(dto.FingerData2) ? data.FingerData2 : dto.FingerData2;
                data.FingerData3 = string.IsNullOrWhiteSpace(dto.FingerData3) ? data.FingerData3 : dto.FingerData3;
                data.FingerData4 = string.IsNullOrWhiteSpace(dto.FingerData4) ? data.FingerData4 : dto.FingerData4;
                data.FingerData5 = string.IsNullOrWhiteSpace(dto.FingerData5) ? data.FingerData5 : dto.FingerData5;
                data.FingerData6 = string.IsNullOrWhiteSpace(dto.FingerData6) ? data.FingerData6 : dto.FingerData6;
                data.FingerData7 = string.IsNullOrWhiteSpace(dto.FingerData7) ? data.FingerData7 : dto.FingerData7;
                data.FingerData8 = string.IsNullOrWhiteSpace(dto.FingerData8) ? data.FingerData8 : dto.FingerData8;
                data.FingerData9 = string.IsNullOrWhiteSpace(dto.FingerData9) ? data.FingerData9 : dto.FingerData9;
                data.FingerVersion = string.IsNullOrWhiteSpace(dto.FingerVersion) ? data.FingerVersion : dto.FingerVersion;
            }

            if (authModes == null || authModes.Contains(UserSyncAuthMode.Face))
            {
                data.FaceTemplate = string.IsNullOrWhiteSpace(dto.FaceTemplate) ? data.FaceTemplate : dto.FaceTemplate;

                data.FaceV2_Index = !dto.FaceV2_Index.HasValue ? data.FaceV2_Index : dto.FaceV2_Index.Value;
                data.FaceV2_No = !dto.FaceV2_No.HasValue ? data.FaceV2_No : dto.FaceV2_No.Value;
                data.FaceV2_Format = !dto.FaceV2_Format.HasValue ? data.FaceV2_Format : dto.FaceV2_Format.Value;
                data.FaceV2_Duress = !dto.FaceV2_Duress.HasValue ? data.FaceV2_Duress : dto.FaceV2_Duress.Value;
                data.FaceV2_MajorVer = !dto.FaceV2_MajorVer.HasValue ? data.FaceV2_MajorVer : dto.FaceV2_MajorVer.Value;
                data.FaceV2_MinorVer = !dto.FaceV2_MinorVer.HasValue ? data.FaceV2_MinorVer : dto.FaceV2_MinorVer.Value;
                data.FaceV2_Size = !dto.FaceV2_Size.HasValue ? data.FaceV2_Size : dto.FaceV2_Size.Value;
                data.FaceV2_Type = !dto.FaceV2_Type.HasValue ? data.FaceV2_Type : dto.FaceV2_Type.Value;
                data.FaceV2_Valid = !dto.FaceV2_Valid.HasValue ? data.FaceV2_Valid : dto.FaceV2_Valid.Value;
                data.FaceV2_TemplateBIODATA = string.IsNullOrWhiteSpace(dto.FaceV2_TemplateBIODATA) ? data.FaceV2_TemplateBIODATA : dto.FaceV2_TemplateBIODATA;
                data.FaceV2_Content = string.IsNullOrWhiteSpace(dto.FaceV2_Content) ? data.FaceV2_Content : dto.FaceV2_Content;
                data.FaceVersion = string.IsNullOrWhiteSpace(dto.FaceVersion) ? data.FaceVersion : dto.FaceVersion;
            }


            if (dto.Index > 0)
            {
                data.UpdatedDate = DateTime.Today;
            }
            else
            {
                data.CreatedDate = DateTime.Today;
            }
            data.UpdatedUser = dto.UpdatedUser;
        }

        private void ConvertDTOToAddMore(IC_UserMasterDTO dto, IC_UserMaster data, List<UserSyncAuthMode> authModes = null)
        {

            data.NameOnMachine = string.IsNullOrWhiteSpace(data.NameOnMachine) ? dto.NameOnMachine : data.NameOnMachine;
            if (!string.IsNullOrWhiteSpace(data.NameOnMachine) && data.NameOnMachine.Length > 20)
            {
                data.NameOnMachine = data.NameOnMachine.Substring(0, 20);
            }

            if (authModes == null || authModes.Contains(UserSyncAuthMode.CardNumber))
            {
                data.CardNumber = string.IsNullOrWhiteSpace(data.CardNumber) || data.CardNumber == "0" ? dto.CardNumber : data.CardNumber;
            }
            if (authModes == null || authModes.Contains(UserSyncAuthMode.Password))
            {
                data.Password = string.IsNullOrWhiteSpace(data.Password) ? dto.Password : data.Password;
            }
            data.Privilege = !data.Privilege.HasValue ? dto.Privilege : data.Privilege;
            data.AuthenMode = string.IsNullOrWhiteSpace(data.AuthenMode) ? dto.AuthenMode : data.AuthenMode;

            if (authModes == null || authModes.Contains(UserSyncAuthMode.Finger))
            {
                data.FingerData0 = string.IsNullOrWhiteSpace(dto.FingerData0) ? data.FingerData0 : dto.FingerData0;
                data.FingerData1 = string.IsNullOrWhiteSpace(dto.FingerData1) ? data.FingerData1 : dto.FingerData1;
                data.FingerData2 = string.IsNullOrWhiteSpace(dto.FingerData2) ? data.FingerData2 : dto.FingerData2;
                data.FingerData3 = string.IsNullOrWhiteSpace(dto.FingerData3) ? data.FingerData3 : dto.FingerData3;
                data.FingerData4 = string.IsNullOrWhiteSpace(dto.FingerData4) ? data.FingerData4 : dto.FingerData4;
                data.FingerData5 = string.IsNullOrWhiteSpace(dto.FingerData5) ? data.FingerData5 : dto.FingerData5;
                data.FingerData6 = string.IsNullOrWhiteSpace(dto.FingerData6) ? data.FingerData6 : dto.FingerData6;
                data.FingerData7 = string.IsNullOrWhiteSpace(dto.FingerData7) ? data.FingerData7 : dto.FingerData7;
                data.FingerData8 = string.IsNullOrWhiteSpace(dto.FingerData8) ? data.FingerData8 : dto.FingerData8;
                data.FingerData9 = string.IsNullOrWhiteSpace(dto.FingerData9) ? data.FingerData9 : dto.FingerData9;
                data.FingerVersion = string.IsNullOrWhiteSpace(dto.FingerVersion) ? data.FingerVersion : dto.FingerVersion;
            }

            if (authModes == null || authModes.Contains(UserSyncAuthMode.Face))
            {
                data.FaceTemplate = string.IsNullOrWhiteSpace(data.FaceTemplate) ? dto.FaceTemplate : data.FaceTemplate;

                data.FaceV2_Index = !data.FaceV2_Index.HasValue ? dto.FaceV2_Index : data.FaceV2_Index.Value;
                data.FaceV2_No = !data.FaceV2_No.HasValue ? dto.FaceV2_No : data.FaceV2_No.Value;
                data.FaceV2_Format = !data.FaceV2_Format.HasValue ? dto.FaceV2_Format : data.FaceV2_Format.Value;
                data.FaceV2_Duress = !data.FaceV2_Duress.HasValue ? dto.FaceV2_Duress : data.FaceV2_Duress.Value;
                data.FaceV2_MajorVer = !data.FaceV2_MajorVer.HasValue ? dto.FaceV2_MajorVer : data.FaceV2_MajorVer.Value;
                data.FaceV2_MinorVer = !data.FaceV2_MinorVer.HasValue ? dto.FaceV2_MinorVer : data.FaceV2_MinorVer.Value;
                data.FaceV2_Size = !data.FaceV2_Size.HasValue ? dto.FaceV2_Size : data.FaceV2_Size.Value;
                data.FaceV2_Type = !data.FaceV2_Type.HasValue ? dto.FaceV2_Type : data.FaceV2_Type.Value;
                data.FaceV2_Valid = !data.FaceV2_Valid.HasValue ? dto.FaceV2_Valid : data.FaceV2_Valid.Value;
                data.FaceV2_TemplateBIODATA = string.IsNullOrWhiteSpace(data.FaceV2_TemplateBIODATA) ? dto.FaceV2_TemplateBIODATA : data.FaceV2_TemplateBIODATA;
                data.FaceV2_Content = string.IsNullOrWhiteSpace(data.FaceV2_Content) ? dto.FaceV2_Content : data.FaceV2_Content;
                data.FaceVersion = string.IsNullOrWhiteSpace(data.FaceVersion) ? dto.FaceVersion : data.FaceVersion;
            }


            if (dto.Index > 0)
            {
                data.UpdatedDate = DateTime.Today;
            }
            else
            {
                data.CreatedDate = DateTime.Today;
            }
            data.UpdatedUser = dto.UpdatedUser;
        }
    }

    public interface IIC_UserMasterLogic
    {
        List<IC_UserMasterDTO> GetMany(List<AddedParam> addedParams);
        IC_UserMasterDTO Get(int index);
        IC_UserMasterDTO GetExist(string employeeID, int companyIndex);
        IC_UserMasterDTO Update(IC_UserMasterDTO userMaster);
        IC_UserMasterDTO UpdateField(List<AddedParam> addParams);
        IC_UserMasterDTO SaveOrUpdate(IC_UserMasterDTO item);
        List<IC_UserMasterDTO> UpdateList(List<IC_UserMasterDTO> listUserMaster);
        List<IC_UserMasterDTO> SaveOrUpdateList(List<IC_UserMasterDTO> listUserMaster);
        Task<List<IC_UserMasterDTO>> SaveAndAddMoreList(List<IC_UserMasterDTO> listUserMaster,
            List<UserSyncAuthMode> authModes = null,
            TargetDownloadUser targetDownloadUser = TargetDownloadUser.AllUser);
        Task<List<IC_UserMasterDTO>> SaveAndOverwriteList(List<IC_UserMasterDTO> listUserMaster,
            List<UserSyncAuthMode> authModes = null,
            TargetDownloadUser targetDownloadUser = TargetDownloadUser.AllUser);
        string GetFingerData(List<FingerInfo> listFinger, int fingerIndex);
        int GetFingerDataLength(List<FingerInfo> listFinger, int fingerIndex);
        List<FingerInfo> BuildFingerData(IC_UserMasterDTO userMaster);
        List<FingerInfo> BuildFingerData(IC_UserMaster userMaster);
        List<string> CheckExistedOrCreate(UserInfoPram listUserInfo, UserInfo userInfo);
        List<IC_EmployeeDTO> GetUserMasterInfoMany(List<AddedParam> addedParams);
        List<IC_EmployeeDTO> GetStudentMasterInfoMany(List<AddedParam> addedParams);
        List<IC_EmployeeDTO> GetParentMasterInfoMany(List<AddedParam> addedParams);
        Task<List<IC_EmployeeDTO>> GetCustomerMasterInfoMany(List<AddedParam> addedParams);
        IC_UserMasterDTO CheckExistedOrCreate(IC_UserMasterDTO userInfo);
        string GetFingerDataListString(List<string> listFinger, int fingerIndex);
        IC_UserMasterDTO CheckExistedOrCreateDto(IC_UserMasterDTO userInfo, List<IC_UserMaster> userMasterLst, EPAD_Context db);
    }

}
