using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface ITA_ListLocationService : IBaseServices<TA_ListLocation, EPAD_Context>
    {
        DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter);
        Task<string> AddLocation(TA_ListLocation data, UserInfo user);
        Task<bool> UpdateLocation(TA_ListLocationDTO data, UserInfo user, TA_ListLocation existLocation);
        string DeleteLocation(List<int> listIndex);
        Task<TA_ListLocation> GetLocationByIndex(int locationIndex);
    }
}