using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface ITA_LeaveRegistrationService : IBaseServices<TA_LeaveRegistration, EPAD_Context>
    {
        Task<DataGridClass> GetLeaveRegistration(LeaveRegistrationModel param, UserInfo user);
        Task<List<TA_LeaveRegistration>> GetLeaveRegistrationByListIndex(List<int> index);
        Task<List<TA_BusinessRegistration>> GetBusinessRegistrationByEmployeeATIDsAndDates(List<string> employeeATIDs,
            List<DateTime> dates, int companyIndex);
        Task<bool> AddLeaveRegistration(LeaveRegistrationModel param, UserInfo user);
        Task<bool> UpdateLeaveRegistration(LeaveRegistrationModel param, UserInfo user);
        Task<bool> DeleteLeaveRegistration(List<int> indexex);
        Task<DateTime> GetLockAttendanceTimeValidDate(UserInfo user);
        Task<bool> IsPaidLeave(int index);
        Task<Dictionary<string, HashSet<string>>> CheckRuleLeaveRegister(LeaveRegistrationModel param, UserInfo user);
        bool ExportTemplateLeaveRegister(string folderDetails);
        Task<List<LeaveRegistrationModel>> ValidationImportLeaveRegistration(List<LeaveRegistrationModel> param, UserInfo user);
        Task<List<TA_LeaveDateType>> GetAllLeaveDateType();
    }
}
