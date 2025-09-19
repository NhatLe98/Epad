using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface ITA_LeaveDateTypeService : IBaseServices<TA_LeaveDateType, EPAD_Context>
    {
        Task<DataGridClass> GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter);
        Task<List<TA_LeaveDateType>> GetAllLeaveDateType(int companyIndex);
        Task<TA_LeaveDateType> GetLeaveDateTypeByIndex(int index);
        Task<List<TA_LeaveDateType>> GetLeaveDateTypeByName(string name, int companyIndex);
        Task<List<TA_LeaveDateType>> GetLeaveDateTypeByCode(string code, int companyIndex);
        Task<bool> AddLeaveDateType(TA_LeaveDateType data);
        Task<bool> UpdateLeaveDateType(TA_LeaveDateType data);
        Task<bool> DeleteLeaveDateType(List<int> index);
        Task<bool> IsLeaveDateTypeUsing(List<int> index);
    }
}
