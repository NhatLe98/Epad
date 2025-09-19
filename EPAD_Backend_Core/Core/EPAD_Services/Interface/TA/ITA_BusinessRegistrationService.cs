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
    public interface ITA_BusinessRegistrationService : IBaseServices<TA_BusinessRegistration, EPAD_Context>
    {
        Task<DataGridClass> GetBusinessRegistration(BusinessRegistrationModel param, UserInfo user);
        Task<List<TA_BusinessRegistration>> GetBusinessRegistrationByListIndex(List<int> index);
        Task<List<TA_LeaveRegistration>> GetLeaveRegistrationByEmployeeATIDsAndDates(List<string> employeeATIDs,
            List<DateTime> dates, int companyIndex);
        Task<bool> AddBusinessRegistration(BusinessRegistrationModel param, UserInfo user);
        Task<bool> UpdateBusinessRegistration(BusinessRegistrationModel param, UserInfo user);
        Task<bool> DeleteBusinessRegistration(List<int> indexex);
        Task<DateTime> GetLockAttendanceTimeValidDate(UserInfo user);
        Task<Dictionary<string, HashSet<string>>> CheckRuleBusinessRegister(BusinessRegistrationModel param, UserInfo user);
        Task<List<BusinessRegistrationModel>> ValidationImportBusinessRegistration(List<BusinessRegistrationModel> param, UserInfo user);
    }
}
