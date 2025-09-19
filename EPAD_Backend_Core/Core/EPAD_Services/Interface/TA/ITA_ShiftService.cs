using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface ITA_ShiftService : IBaseServices<TA_Shift, EPAD_Context>
    {
        Task<List<TA_Shift>> GetAllShift(int companyIndex);
        Task<TA_Shift> GetShiftByIndex(int index);
        Task<List<TA_Shift>> GetShiftByName(string name, int companyIndex);
        Task<List<TA_Shift>> GetShiftByCode(string code, int companyIndex);
        Task<bool> AddShift(TA_Shift data);
        Task<bool> UpdateShift(TA_Shift data);
        Task<bool> DeleteShift(int index);
        Task<bool> IsShiftUsing(int index);
    }
}
