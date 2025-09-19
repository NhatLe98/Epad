using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_EmployeeTypeService : IBaseServices<IC_EmployeeType, EPAD_Context>
    {
        Task<bool> AddEmployeeType(IC_EmployeeType param, UserInfo user);
        Task<bool> UpdateEmployeeType(IC_EmployeeType param, UserInfo user);
        Task<bool> DeleteEmployeeTypes(List<int> indexes, UserInfo user);
        Task<DataGridClass> GetDataByPage(int companyIndex, int page, string filter, int pageSize);
        Task<IC_EmployeeType> GetDataByIndex(int index);
        Task<List<IC_EmployeeType>> GetDataByCompanyIndex(int companyIndex);
        Task<List<IC_EmployeeType>> GetUsingEmployeeType(int companyIndex);
        Task<IC_EmployeeType> GetDataByNameAndCompanyIndex(string name, int companyIndex);
        Task<List<IC_EmployeeType>> GetDataByListNameAndCompanyIndex(List<string> name, int companyIndex);
        Task<IC_EmployeeType> GetDataByCodeAndCompanyIndex(string code, int companyIndex);
    }
}
