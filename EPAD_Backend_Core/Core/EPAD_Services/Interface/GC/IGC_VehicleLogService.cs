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
    public interface IGC_VehicleLogService : IBaseServices<IC_VehicleLog,EPAD_Context>
    {
        List<VehicleHistoryModel> GetHistoryData(List<long> departmentIndexes, List<string> employeeIndexes,
       UserInfo user, DateTime FromTime, DateTime ToTime, string statusLog, string filter);
        DataGridClass GetPaginationList(IEnumerable<VehicleHistoryModel> histories, int page, int pageSize);
    }
}
