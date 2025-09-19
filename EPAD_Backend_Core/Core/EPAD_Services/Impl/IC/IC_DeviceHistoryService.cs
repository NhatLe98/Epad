using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EPAD_Common.Types;
using EPAD_Repository.Interface;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace EPAD_Services.Impl
{
    public class IC_DeviceHistoryService : BaseServices<IC_DeviceHistory, EPAD_Context>, IIC_DeviceHistoryService
    {
        private IIC_DeviceService _IC_DeviceService;
        public IC_DeviceHistoryService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _IC_DeviceService = serviceProvider.GetService<IIC_DeviceService>();
        }

        public async Task<DataGridClass> GetDeviceHistory(int pPageIndex, string filter, DateTime fromDate, DateTime toDate, int pCompanyIndex, int limit = 50)
        {
            var obj = from att in DbContext.IC_DeviceHistory
                      join de in DbContext.IC_Device on att.SerialNumber equals de.SerialNumber into eDevice
                      from eDeviceResult in eDevice.DefaultIfEmpty()
                      where (att.Date >= fromDate && att.Date <= toDate)
                      && (!string.IsNullOrEmpty(filter) && att.SerialNumber.Contains(filter) || string.IsNullOrEmpty(filter))
                      select new
                      {
                         SerialNumber = att.SerialNumber,
                         Date = att.Date.ToString("dd-MM-yyyy hh:mm:ss"),
                         IP = eDeviceResult.IPAddress,
                         Status = att.Status == (short)DeviceOnOffStatus.Offline ? DeviceOnOffStatus.Offline.ToString() : DeviceOnOffStatus.Online.ToString(),
                         DateTimeFormat = att.Date
                      };

            if (pPageIndex <= 1)
            {
                pPageIndex = 1;
            }

            int fromRow = limit * (pPageIndex - 1);
            var lsAttendanceLog = obj.OrderByDescending(t => t.DateTimeFormat).Skip(fromRow).Take(limit).ToList();
            var dataGrid = new DataGridClass(obj.Count(), lsAttendanceLog);
            return await Task.FromResult(dataGrid);
        }

        public async Task<DataGridClass> GetDeviceHistoryLast7Days()
        {
            var obj = from de in DbContext.IC_Device
                      join att in DbContext.IC_DeviceHistory
                        .Where(x => x.Date.Date >= DateTime.Now.Date.AddDays(-6) && x.Date.Date <= DateTime.Now.Date) 
                      on de.SerialNumber equals att.SerialNumber into eDeviceHistory
                      from eDeviceHistoryResult in eDeviceHistory.DefaultIfEmpty()
                      select new
                      {
                          SerialNumber = de.SerialNumber,
                          DeviceName = de.AliasName,
                          Date = (eDeviceHistoryResult != null) ? eDeviceHistoryResult.Date.ToString("dd-MM-yyyy hh:mm:ss") : string.Empty,
                          IP = de.IPAddress,
                          Status = (eDeviceHistoryResult != null) ? (eDeviceHistoryResult.Status == (short)DeviceOnOffStatus.Offline
                            ? DeviceOnOffStatus.Offline.ToString() : DeviceOnOffStatus.Online.ToString()) : string.Empty,
                          DateTimeFormat = (eDeviceHistoryResult != null) ? eDeviceHistoryResult.Date : DateTime.Now.Date.AddDays(-7)
                      };

            var lsAttendanceLog = obj.OrderByDescending(t => t.DateTimeFormat).ToList();
            var dataGrid = new DataGridClass(obj.Count(), lsAttendanceLog);
            return await Task.FromResult(dataGrid);
        }
    }
}
