using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_ClassInfoService : IBaseServices<HR_ClassInfo, EPAD_Context>
    {
        Task<HR_ClassInfoResult> GetAllClassInfoByClassID(string classID, int companyIndex);
        Task<HR_ClassInfo> GetAllClassInfoByClassCode(string code, int companyIndex);
        Task<List<HR_ClassInfoResult>> GetAllClassInfo(string[] classIDs, int companyIndex);
        Task<List<HR_ClassInfoResult>> GetClassInfoByNanny(string employeeATID, int companyIndex);
        Task<DataGridClass> GetPage(List<AddedParam> addedParam, int pCompanyIndex);
        Task<List<HR_ClassInfo>> CheckExistedOrCreate(List<HR_ClassInfo> listClassInfo, int pCompanyIndex);
    }
}
