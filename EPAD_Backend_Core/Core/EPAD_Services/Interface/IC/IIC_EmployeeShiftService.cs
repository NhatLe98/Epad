using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_EmployeeShiftService : IBaseServices<IC_Employee_Shift, EPAD_Context>
    {
        Task<List<IC_Employee_Shift>> GetAllEmployeeShifts(GetListEmployeeShiftRequest request, int companyIndex);
    }
}
