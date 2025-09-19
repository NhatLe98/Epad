using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface ITA_TimeAttendanceLogService : IBaseServices<TA_TimeAttendanceLog, EPAD_Context>
    {
        Task SendTimeLogToECMSAPIAsync(TA_TimeAttendanceProccess logParam);
        Task<DataGridClass> GetCaculateAttendanceData(SyntheticAttendanceRequest filter, int companyIndex);
    }
}
