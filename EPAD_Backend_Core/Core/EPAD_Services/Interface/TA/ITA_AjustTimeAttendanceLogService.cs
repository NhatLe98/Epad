using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Services.Interface
{
    public interface ITA_AjustTimeAttendanceLogService : IBaseServices<TA_AjustTimeAttendanceLog, EPAD_Context>
    {
        List<dynamic> GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter, List<long> departments, List<string> employeeatids, DateTime fromDate, DateTime toDate);
        void UpdateAjustTimeAttendanceLog(AjustTimeAttendanceLogInsertParam logs, UserInfo user);
        List<ComboboxItem> GetAllRegistrationType();
        List<dynamic> GetSyntheticDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter, List<long> departments, List<string> employeeatids, DateTime fromDate, DateTime toDate, int filterByType);
    }
}
