using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_Rules_InOutTimeService : IBaseServices<HR_Rules_InOutTime, EPAD_Context>
    {
        List<Rules_InOutTimeParam> GetAllRules(UserInfo user);
        Task<HR_Rules_InOutTime> GetByDate(DateTime date, UserInfo user);
        Task<HR_Rules_InOutTime> GetByIndex(int index);
        Task<bool> AddRuleInOutTime(Rules_InOutTimeParam param, UserInfo user);
        Task<bool> UpdateRuleInOutTime(Rules_InOutTimeParam param, UserInfo user);
        Task<bool> DeleteRuleInOutTime(int index);
    }
}
