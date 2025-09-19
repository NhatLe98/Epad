using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using EPAD_Data.Models.Other;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_EmployeeInfoService : IBaseServices<HR_EmployeeInfo, EPAD_Context>
    {
        Task<List<EmployeeInfoResponse>> GetEmployeeInfoByIDs(string[] pEmployeeATIDs, int pCompanyIndex);
        Task<List<EmployeeInfoResponse>> GetEmployeeInfoByEmployeeATIDs(List<string> pEmployeeATIDs, int pCompanyIndex);
        public Task<List<HR_EmployeeInfoResult>> GetAllEmployeeInfo(string[] pEmployeeATIDs, int pCompanyIndex);
        public Task<List<HR_EmployeeInfoResult>> GetEmployeeInfoByIds(string[] pEmployeeATIDs, int pCompanyIndex);
        Task<List<HR_EmployeeInfoResult>> GetEmployeeInfoByDepartment(List<long> pDepartmentIndex, int pCompanyIndex);
        public Task<List<VStarEmployeeInfoResult>> GetAllEmployeeInfoVStar(string[] pEmployeeATIDs, int pCompanyIndex);
        public Task<HR_EmployeeInfoResult> GetEmployeeInfo(string pEmployeeATID, int pCompanyIndex, string employeeCode = "");
        public Task<DataGridClass> GetPage(List<AddedParam> addedParams, int cCompanyIndex);
        public Task CheckExistedOrCreateList(List<HR_EmployeeInfo> listHREmployeeInfo, int pCompanyIndex);
        public Task<List<VStarEmployeeInfoResult>> GetAllEmployeeInfoVStarExtend(string[] pEmployeeATIDs, int pCompanyIndex, long type);
        List<HR_DeparmentParent> GetChildrenToId(List<HR_Department> departments, long id, string name);
    }
}
