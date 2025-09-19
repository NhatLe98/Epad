using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_ExcusedLateEntryService : IBaseServices<HR_ExcusedLateEntry, EPAD_Context>
    {
        Task<DataGridClass> GetExcusedLateEntryAtPage(ExcusedLateEntryRequest requestParam, UserInfo user);
        Task<List<HR_ExcusedLateEntry>> GetByEmployeeATIDs(List<string> listEmployeeATIDs);
        Task<List<HR_ExcusedLateEntry>> GetByListIndex(List<int> index);
        Task<HR_ExcusedLateEntry> GetByIndex(int index);
        Task<bool> AddExcusedLateEntry(ExcusedLateEntryParam param, UserInfo user);
        Task<bool> UpdateExcusedLateEntry(ExcusedLateEntryParam param, UserInfo user);
        Task<bool> DeleteExcusedLateEntry(List<int> param);
        Task<List<ExcusedLateEntryImportParam>> ValidationImportExcusedLateEntry(List<ExcusedLateEntryImportParam> param, UserInfo user);
    }
}
