using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface ITA_LocationByEmployeeService : IBaseServices<TA_LocationByEmployee, EPAD_Context>
    {
        DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter);
        Task<string> AddListLocationByEmployee(TA_LocationByEmployeeDTO data, UserInfo user);
        Task<bool> UpdateLocationByEmployee(TA_LocationByEmployeeDTO data, UserInfo user);
        bool DeleteListLocationByEmployee(List<int> listIndex);
        Task<TA_LocationByEmployee> GetLocationByEmployeeByIndex(int index);
        List<TA_LocationByEmployeeImportExcel> ValidationImportLocationByEmployee(List<TA_LocationByEmployeeImportExcel> param, UserInfo user);
    }
}
