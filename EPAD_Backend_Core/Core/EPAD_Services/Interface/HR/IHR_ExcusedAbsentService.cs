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
    public interface IHR_ExcusedAbsentService : IBaseServices<HR_ExcusedAbsent, EPAD_Context>
    {
        Task<DataGridClass> GetExcusedAbsentAtPage(ExcusedAbsentRequest requestParam, UserInfo user);
        Task<List<HR_ExcusedAbsentReason>> GetExcusedAbsentReason(UserInfo user);
        Task<List<HR_ExcusedAbsent>> GetByEmployeeATIDs(List<string> listEmployeeATIDs);
        Task<List<HR_ExcusedAbsent>> GetByListIndex(List<int> index);
        Task<HR_ExcusedAbsent> GetByIndex(int index);
        Task<bool> AddExcusedAbsent(ExcusedAbsentParam param, UserInfo user);
        Task<bool> UpdateExcusedAbsent(ExcusedAbsentParam param, UserInfo user);
        Task<bool> DeleteExcusedAbsent(List<int> param);
        bool ExportTemplateExcusedAbsent(string folderDetails);
        Task<List<ExcusedAbsentImportParam>> ValidationImportExcusedAbsent(List<ExcusedAbsentImportParam> param, UserInfo user);
    }
}
