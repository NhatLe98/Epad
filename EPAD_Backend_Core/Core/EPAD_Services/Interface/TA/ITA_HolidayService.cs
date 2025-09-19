using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface ITA_HolidayService : IBaseServices<TA_Holiday, EPAD_Context>
    {
        Task<DataGridClass> GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter);
        Task<List<TA_Holiday>> GetAllHoliday(int companyIndex);
        Task<TA_Holiday> GetHolidayByIndex(int index);
        Task<List<TA_Holiday>> GetHolidayByName(string name, int companyIndex);
        Task<List<TA_Holiday>> GetHolidayByCode(string code, int companyIndex);
        Task<bool> AddHoliday(TA_Holiday data);
        Task<bool> UpdateHoliday(TA_Holiday data);
        Task<bool> DeleteHoliday(List<int> index);
    }
}
