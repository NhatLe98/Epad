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
    public interface IGC_Department_AccessedGroupService : IBaseServices<GC_Department_AccessedGroup, EPAD_Context>
    {
        Task<GC_Department_AccessedGroup> GetByDepartmentAndFromToDate(int pDepartmentIndex, DateTime pFromDate, DateTime? pToDate, UserInfo user);
        Task<DataGridClass> GetDepartmentAccessedGroup(int page, int pageSize, string filter, UserInfo user);
        Task<DataGridClass> GetDepartmentAccessedGroupByFilter(int page, int pageSize, string filter,
            DateTime? fromDate, DateTime? toDate, List<long> listDepartment, UserInfo user);
        //DepartmentAccessRule GetInfoByEmpATIDAndFromDate(string pDepartmentATID, DateTime pDate, int pCompanyIndex);
        Task<List<DepartmentAccessedGroupModel>> ImportDepartmentAccessedGroup(List<DepartmentAccessedGroupModel> param, UserInfo user);

        EmployeeAccessRule GetInfoDepartmentAccessedGroup(string pEmployeeATID, DateTime pDate, int pCompanyIndex);
        List<GC_Department_AccessedGroup> GetByListDepartmentAndFromToDate(List<int> departmentIndexList, DateTime pFromDate, DateTime? pToDate, UserInfo user);
    }
}
