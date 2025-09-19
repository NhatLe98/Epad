using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data.Entities;
using EPAD_Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EPAD_Data.Models;

namespace EPAD_Services.Interface
{
    public interface IAC_DepartmentAccessedGroupService : IBaseServices<AC_DepartmentAccessedGroup, EPAD_Context>
    {
        DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter, List<int> groups, List<long> departments);
        Task<AC_DepartmentAccessedGroup> GetByDepartmentAndFromToDate(int departmentIndex, UserInfo user, int groupValue);
        Task<AC_DepartmentAccessedGroup> GetByDepartmentAndFromToDateEdit(int departmentIndex, UserInfo user, int groupValue);
    }
}
