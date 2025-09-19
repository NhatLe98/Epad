using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IGC_TimeLogService : IBaseServices<GC_TimeLog,EPAD_Context>
    {
        Task AddTimeLog(GC_TimeLog timeLog);
        Task<List<GC_TimeLog>> GetLineValidLogs(int companyIndex, List<int> lines);
        Task UpdateLogInGateMandatoryByRule(int companyIndex);
        Task SaveChangeAsync();
        List<MonitoringGatesHistoryModel> GetHistoryData(List<long> departmentIndexes, List<string> employeeIndexes,
        UserInfo user, DateTime FromTime, DateTime ToTime, List<int> rulesWarningIndexes, string statusLog);
        DataGridClass GetPaginationList(IEnumerable<MonitoringGatesHistoryModel> histories, int page, int pageSize);
    }
}
