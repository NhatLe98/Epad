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
    public interface IAC_AccessedGroupService : IBaseServices<AC_AccessedGroup, EPAD_Context>
    {
        DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter, List<int> groups, List<long> departments);
        Task<AC_AccessedGroup> GetByEmployeeAndFromToDate(string pEmployeeATID, UserInfo user, int groupValue);
        Task<AC_AccessedGroup> GetByEmployeeAndFromToDateEdit(string pEmployeeATID, UserInfo user, int groupValue);
    }
}
