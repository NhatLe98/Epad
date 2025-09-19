using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Common.Types;
using System.Threading.Tasks;
using System;

namespace EPAD_Services.Interface
{
    public interface IIC_DeviceHistoryService : IBaseServices<IC_DeviceHistory, EPAD_Context>
    {
        Task<DataGridClass> GetDeviceHistory(int pPageIndex, string filter, DateTime fromDate, DateTime toDate, int pCompanyIndex, int limit = 50);
        Task<DataGridClass> GetDeviceHistoryLast7Days();
    }
}
