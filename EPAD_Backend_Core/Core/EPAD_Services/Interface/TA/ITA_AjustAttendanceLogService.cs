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
    public interface ITA_AjustAttendanceLogService : IBaseServices<TA_AjustAttendanceLog, EPAD_Context>
    {
        DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter, List<long> departments, List<string> employeeatids, DateTime fromDate, DateTime toDate);
        Task<string> InsertAjustAttendanceLog(TA_AjustAttendanceLogInsertDTO log, UserInfo user);
        Task<string> UpdateAjustAttendanceLog(TA_AjustAttendanceLogInsertDTO log, UserInfo user);
        Task<string> DeleteAjustAttendanceLog(List<TA_AjustAttendanceLogDTO> logs, UserInfo user);
        Task<string> UpdateAjustAttendanceLogLst(List<TA_AjustAttendanceLogDTO> logs, UserInfo user);
        Task<List<TA_AjustAttendanceLogImport>> ValidationImportAjustAttendanceLog(List<TA_AjustAttendanceLogImport> logs, UserInfo user);
        Task AddOrUpdateImportAjustAttendanceLog(List<TA_AjustAttendanceLogImport> logs, UserInfo user);
        Task<List<TA_AjustAttendanceLogImport>> CheckAjustAttendanceLogInDatabase(List<TA_AjustAttendanceLogImport> logs, UserInfo user);

    }
}
