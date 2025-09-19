using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IGC_Employee_AccessedGroupService : IBaseServices<GC_Employee_AccessedGroup, EPAD_Context>
    {
        Task<List<GC_Employee_AccessedGroup>> GetByEmployeeAndFromToDate(List<string> employeeATIDsList, DateTime pFromDate, DateTime? pToDate, UserInfo user);
        Task<DataGridClass> GetEmployeeAccessedGroup(int page, int pageSize, string filter, UserInfo user);
        Task<DataGridClass> GetEmployeeAccessedGroupByFilter(int page, int pageSize, string filter,
            DateTime? fromDate, DateTime? toDate, List<long> listDepartment, List<string> listEmployeeATID, UserInfo user);
        EmployeeAccessRule GetInfoByEmpATIDAndFromDate(string pEmployeeATID, DateTime pDate, int pCompanyIndex);
        Task<List<EmployeeAccessedGroupModel>> ImportEmployeeAccessedGroup(List<EmployeeAccessedGroupModel> param, UserInfo user);
        EmployeeAccessRule GetInfoByDriver(int pCompanyIndex, string employeeATID);
        EmployeeAccessRule GetInfoByGuest(int pCompanyIndex, string employeeATID);
    }
}
