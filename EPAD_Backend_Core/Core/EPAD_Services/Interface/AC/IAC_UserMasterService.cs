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
    public interface IAC_UserMasterService : IBaseServices<AC_UserMaster, EPAD_Context>
    {
        void DeleteUserMaster(List<string> employeeATIDLst, List<int> doorLst, UserInfo user);
        void AddUserMasterToHistory(List<string> employeeATIDLst, List<int> doorLst, UserInfo user, int timezone);
        object GetACOperation();
        Task<DataGridClass> GetACSync(string pFilter, DateTime fromDate, DateTime toDate, List<long> pDepartmentIds, 
            int pCompanyIndex, int pPageIndex, int pPageSize, List<int> listDoor, List<int> listArea, int viewMode, List<int> viewOperation);
        void AddUserMasterToHistoryFromExcel(List<AC_UserImportDTO> listImport, UserInfo user);
        void AddUserMasterToHistoryFromExcel(List<AC_AccessedGroupImportDTO> listImport, UserInfo user);
    }
}
