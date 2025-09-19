using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class GC_Rules_Warning_EmailScheduleService : BaseServices<GC_Rules_Warning_EmailSchedule, EPAD_Context>, IGC_Rules_Warning_EmailScheduleService
    {
        public GC_Rules_Warning_EmailScheduleService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public void UpdateSchedule(GC_Rules_Warning_EmailSchedule schedule)
        {
            var data = DbContext.GC_Rules_Warning_EmailSchedules.FirstOrDefault(e => e.Index == schedule.Index);
            data.LatestDateSendMail = schedule.LatestDateSendMail;
            DbContext.Update(data);
            DbContext.SaveChanges();
        }

        public async Task<List<GC_Rules_Warning_EmailSchedule>> GetSchedulesByTimeAndCompanyIndex(TimeSpan time, int dayOfWeekIndex, int companyIndex)
        {
            var checkTime = new TimeSpan(time.Hours, time.Minutes, 0);
            return await DbContext.GC_Rules_Warning_EmailSchedules.Where(x => x.CompanyIndex == companyIndex && x.Time.CompareTo(checkTime) == 0 && x.DayOfWeekIndex == dayOfWeekIndex).ToListAsync();
        }
        public GC_Rules_Warning_EmailSchedule GetFromDateSendMail(GC_Rules_Warning_EmailSchedule schedule)
        {
            var schedules = DbContext.GC_Rules_Warning_EmailSchedules
                .Where(e => e.RulesWarningIndex == schedule.RulesWarningIndex && schedule.CompanyIndex == e.CompanyIndex)
                .OrderByDescending(e => e.DayOfWeekIndex)
                .ThenByDescending(e => e.Time);

            if (schedules.Count() == 1)
            {
                return schedule;
            }

            var stop = false;
            int index = 0;
            foreach (var item in schedules)
            {
                if (item.Index == schedule.Index)
                {
                    if (index == schedules.Count() - 1)
                    {
                        return schedules.FirstOrDefault();
                    }
                    stop = true;
                }
                if (stop)
                {
                    return item;
                }
                index++;
            }

            return null;
        }
    }
}
