using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IAC_DoorService : IBaseServices<AC_Door, EPAD_Context>
    {
        DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter);
        Task<List<AC_Door>> GetExistedDoorSetting(UserInfo user, List<int> param);
        Task<AC_Door> GetByIndex(int param);
        Task UpdateDoorSettings(UserInfo user, AC_DoorDTO param);
        Task UpdateDoorSetting(UserInfo user, AC_DoorDTO param);
        Task DeleteDoorSettings(UserInfo user, List<int> param);
    }
}
