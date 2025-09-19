using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using System;
using System.Collections.Generic;
using System.Text;
using EPAD_Common.Types;
using EPAD_Data.Models;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface ITA_AjustAttendanceLogHistoryService : IBaseServices<TA_AjustAttendanceLogHistory, EPAD_Context>
    {
        DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter, List<long> departments, List<string> employeeatids, DateTime fromTime, DateTime toTime, List<int> operators);
        Task InsertAjustAttendanceLogHistory(TA_AjustAttendanceLogHistory log, UserInfo user);
    }
}
