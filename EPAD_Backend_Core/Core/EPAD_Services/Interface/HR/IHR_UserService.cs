using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_UserService : IBaseServices<HR_User, EPAD_Context>
    {
        Task<List<string>> GetAllEmployeeATID();
        public Task<List<HR_User>> GetEmployeeLookup(int pCompanyIndex);
        public Task<List<HR_EmployeeInfoResult>> GetEmployeeAndDepartmentLookup(int pCompanyIndex);
        Task<List<HR_UserResult>> GetAllHR_UserAsync(int pCompanyIndex);
        Task<HR_UserResult> GetHR_UserByIDAsync(string pEmployeeATID, int pCompanyIndex);
        Task CheckExistedOrCreate(HR_User hrUser, int pCompanyIndex);
        Task<List<string>> CheckExistedOrCreateList(List<HR_User> listHRUser, int pCompanyIndex);
        Task<List<EmployeeFullInfo>> GetAllEmployeeCompactInfoByPermission(UserInfo user);
        Task<List<EmployeeFullInfo>> GetAllEmployeeCompactInfoByPermissionImprovePerformance(UserInfo user);
        Task<List<EmployeeFullInfo>> GetEmployeeCompactInfoByEmployeeATID(List<string> pEmployeeATIDs, DateTime pDate, int pCompanyIndex);
        Task<List<EmployeeFullInfo>> GetEmployeeCompactInfoByEmployeeCodes(List<string> pEmployeeIDs, int pCompanyIndex);
        Task<List<EmployeeFullInfo>> GetAllEmployeeCompactInfo(int pCompanyIndex);
        Task<List<EmployeeFullInfo>> GetAllEmployeeTypeUserCompactInfo(int pCompanyIndex);
        Task<List<EmployeeFullInfo>> GetFilterEmployeeTypeUserCompactInfo(int pCompanyIndex, List<string> employeeATIDs);
        Task<List<EmployeeFullInfo>> GetAllEmployeeCompactInfoByDate(DateTime pDate, int pCompanyIndex);
        Task<List<EmployeeFullInfo>> GetAllUserCompactInfo(int pCompanyIndex);
        Task<List<EmployeeFullInfo>> GetEmployeeCompactInfoByListEmail(List<string> listEmail, int pCompanyIndex);
        Task<List<EmployeeFullInfo>> GetEmployeeCompactInfoByListEmpATID(List<string> listEmpATID, int pCompanyIndex);
        Task<List<EmployeeFullInfo>> GetEmployeeCompactInfoByCardNumber(string cardNumber, int pCompanyIndex);
        List<EmployeeFullInfo> GetEmployeeCompactInfo(int pCompanyIndex, EmployeeType[] employeeTypes, string fields);
        Task<List<EmployeeFullInfo>> GetEmployeeByDepartmentIds(List<string> pDepartmentIDs, int pCompanyIndex);
        Task<List<HR_Department>> GetDepartmentList(int pCompanyIndex);
        Task<List<HR_Department>> GetDepartmentByIds(List<string> pDepartmentIds, int pCompanyIndex);
        Task CheckUserActivedOrCreate(HR_User hrUser, int pCompanyIndex);
        Task<List<HR_UserResult>> GetAllStudent(int pCompanyIndex);
        Task<HR_UserResult> GetStudentById(int pCompanyIndex, string pStudentId);
        Task<HR_UserResult> GetuserByCCCD(string cccd);
        Task<List<EmployeeFullInfo>> GetEmployeeByUserTypeIds(List<int?> pUserTypes, int pCompanyIndex);
        Tuple<bool, HashSet<string>> ShowStoppedWorkingEmployeesData();
        Task<List<UserBasicInfoReponse>> GetUserBasicInfo();
        Task<List<UserDetailInfoReponse>> GetUserDetailInfo();
        Task<List<UserContactInfoReponse>> GetUserContactInfo();
        Task<List<UserPersonalInfoReponse>> GetUserPersonalInfo();
        Task<List<EmployeeFullInfo>> GetAllNanny(DateTime pDate, int pCompanyIndex);
        Task<List<EmployeeFullInfo>> GetAllStudent(DateTime pDate, int pCompanyIndex);
        Task<List<EmployeeFullInfo>> GetEmployeeInfoAndDepartmentParentByListEmpATID(List<string> listEmpATID, int pCompanyIndex);
        Task<List<EmployeeFullInfo>> GetDriverCompactInfoByEmployeeATID(List<string> pEmployeeATIDs, DateTime pDate, int pCompanyIndex);
        Task<EmployeeFullInfo> GetDriverCompactInfoByEmployeeATIDOrCardId(string pEmployeeATIDs, string cardID, DateTime pDate, int pCompanyIndex);
    }
}
