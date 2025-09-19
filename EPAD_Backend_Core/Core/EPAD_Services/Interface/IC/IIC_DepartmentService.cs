using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_DepartmentService : IBaseServices<IC_Department, EPAD_Context>
    {
        List<IC_Department> GetAllDepartment(int companyIndex);
        List<IC_Department> GetAllActiveDepartment(int pCompanyIndex);
        List<IC_Department> GetDepartmentByListName(int pCompanyIndex, List<string> pListName);
        Task<List<IC_Department>> GetByCompanyIndex(int companyIndex);
        Task<List<IC_Department>> GetByCompanyIndexAndDepIds(int companyIndex, List<long> departmentIds);
        Task<List<IC_Department>> GetDepartmentByIds(List<long> departmentIds);
        Task<List<IC_Department>> GetActiveDepartmentByListIndex(List<long> departmentIds);
        Task<List<IC_Department>> GetActiveDepartment();
        Task<object> GetActiveDepartmentByPermission(UserInfo user);
        Task<object> GetActiveDepartmentAndDeviceByPermission(UserInfo user);
        string GetDeparmentNameByIdFromList(int parentIndex, List<IC_Department> departmentsList);
        Task<List<RegularDepartmentDataReponse>> GetDepartmentChildrentByName(string departmentName, int companyIndex);
        Task<List<int>> GetDepartmentNotIntegrate();
        Task<List<int>> GetDepartmentSecurity();
    }
}
