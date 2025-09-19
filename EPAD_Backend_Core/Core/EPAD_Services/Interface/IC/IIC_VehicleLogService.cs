using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_VehicleLogService : IBaseServices<IC_VehicleLog, EPAD_Context>
    {
        Task IntegrateAttendanceLog(List<IC_VehicleLog> logs);
        Task IntegrateAttendanceLogError(List<IC_VehicleLog> logs);
        Task IntegrateEmployeeToLovad(List<string> employeeATIDs);
        Task DeleteEmployeeToLovad(List<string> employeeATIDs);
        List<IC_VehicleLog> GetLogInNotOutByEmployeeATID(string employeeATID);
    }
}
