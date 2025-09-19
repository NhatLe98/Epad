using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface ITA_RulesShiftService : IBaseServices<TA_Rules_Shift, EPAD_Context>
    {
        Task<List<TA_Rules_ShiftDTO>> GetAllRulesShift(int companyIndex);
        Task<TA_Rules_Shift> GetRulesShiftByIndex(int index);
        Task<List<TA_Rules_Shift>> GetRulesShiftByName(string name, int companyIndex);
        Task<bool> AddRulesShift(TA_Rules_ShiftDTO data);
        Task<bool> UpdateRulesShift(TA_Rules_ShiftDTO data);
        Task<bool> DeleteRulesShift(int index);
        Task<bool> IsRuleUsing(int index);
    }
}
