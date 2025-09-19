using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_HistoryTrackingIntegrateService : BaseServices<IC_HistoryTrackingIntegrate, EPAD_Context>, IIC_HistoryTrackingIntegrateService
    {
        public IC_HistoryTrackingIntegrateService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<DataGridClass> GetHistoryTrackingIntegrateInfo(int pPageIndex, string filter, DateTime fromDate, DateTime toDate, int pCompanyIndex, int limit = 50)
        {
            var obj = from att in DbContext.IC_HistoryTrackingIntegrate.Where(x => x.CompanyIndex == pCompanyIndex)
                      where (att.RunTime >= fromDate && att.RunTime <= toDate)
                      && (!string.IsNullOrEmpty(filter) && att.JobName.Contains(filter) || string.IsNullOrEmpty(filter))
                      select new 
                      {
                          JobName = att.JobName,
                          DataDelete = att.DataDelete,
                          DataNew = att.DataNew,
                          DataUpdate = att.DataUpdate,
                          RunTimeString = att.RunTime.ToddMMyyyyHHmmss(),
                          Total = att.DataNew + att.DataUpdate,
                          RunTime = att.RunTime,
                          IsSuccessString = att.IsSuccess ? "Thành công" : "Thất bại"
                      };

            if (pPageIndex <= 1)
            {
                pPageIndex = 1;
            }

            int fromRow = limit * (pPageIndex - 1);
            var lsAttendanceLog = obj.OrderBy(t => t.RunTime).Skip(fromRow).Take(limit).ToList();
            var dataGrid = new DataGridClass(obj.Count(), lsAttendanceLog);
            return await Task.FromResult(dataGrid);
        }
    }
}
