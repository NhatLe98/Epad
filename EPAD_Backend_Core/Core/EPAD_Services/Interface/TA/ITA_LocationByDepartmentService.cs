using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface ITA_LocationByDepartmentService : IBaseServices<TA_LocationByDepartment, EPAD_Context>
    {
        DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter);
        Task<string> AddListLocationByDepartment(TA_LocationByDepartmentDTO data, UserInfo user);
        Task<bool> UpdateLocationByDepartment(TA_LocationByDepartmentDTO data, UserInfo user);
        bool DeleteListLocationByDepartment(List<int> listIndex);
        Task<TA_LocationByDepartment> GetLocationByDepartmentByIndex(int locationIndex);
    }
}
