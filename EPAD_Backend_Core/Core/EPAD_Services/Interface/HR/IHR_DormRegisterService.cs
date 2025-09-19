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
    public interface IHR_DormRegisterService : IBaseServices<HR_DormRegister, EPAD_Context>
    {
        Task<List<HR_DormActivity>> GetDormActivity(UserInfo user);
        Task<List<HR_DormLeaveType>> GetDormLeaveType(UserInfo user);
        Task<List<HR_DormRation>> GetDormRation(UserInfo user);
        object GetDormAccessMode();
        Task<DataGridClass> GetDormRegister(UserInfo user, DormRegisterRequest requestParam);
        Task<List<HR_DormRegister>> GetByDormEmployeeCode(UserInfo user, string dormEmployeeCode);
        Task<HR_DormRegister> GetByIndex(int index);
        Task<List<HR_DormRegister>> GetByEmployeATID(UserInfo user, string employeeATID);
        Task<bool> AddDormRegister(UserInfo user, DormRegisterViewModel param);
        Task<bool> UpdateDormRegister(UserInfo user, DormRegisterViewModel param);
        Task<bool> DeleteDormRegister(List<int> param);
        Task<List<DormRegisterViewModel>> ValidationImportDormRegsiter(List<DormRegisterViewModel> param, UserInfo user);
    }
}
