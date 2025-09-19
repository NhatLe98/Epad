using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EPAD_Common.Types;
using EPAD_Data.Models;

namespace EPAD_Services.Interface
{
    public interface ITA_AnnualLeaveService : IBaseServices<TA_AnnualLeave, EPAD_Context>
    {
        DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter, List<long> departments, List<string> employeeatids);
        Task<bool> AddAnnualLeave(TA_AnnualLeaveInsertParam data, UserInfo user);
        Task<bool> UpdateAnnualLeave(TA_AnnualLeaveInsertParam data, UserInfo user);
        Task<bool> DeleteAnnualLeave(int index);
        Task<List<TA_AnnualLeave>> GetAnnualLeaveEmployeeATID(string employeeATID, int companyIndex);
        Task<TA_AnnualLeave> GetAnnualLeaveByIndex(int index, int companyIndex);
        Task<bool> DeleteAnnualLeave(List<int> index);
        Task<List<TA_AnnualLeave>> GetAnnualLeaveEmployeeATIDs(List<string> employeeATIDs, int companyIndex);
        List<TA_AnnualLeaveImportParam> ValidationImportAnnualLeave(List<TA_AnnualLeaveImportParam> param, UserInfo user);
    }
}
