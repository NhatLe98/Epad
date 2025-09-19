using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_HistoryTrackingIntegrateService : IBaseServices<IC_HistoryTrackingIntegrate, EPAD_Context>
    {
        Task<DataGridClass> GetHistoryTrackingIntegrateInfo(int pPageIndex, string filter, DateTime fromDate, DateTime toDate, int pCompanyIndex, int limit = 50);
    }
}
