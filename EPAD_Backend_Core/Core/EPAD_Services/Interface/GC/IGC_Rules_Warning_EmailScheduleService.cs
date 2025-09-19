using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IGC_Rules_Warning_EmailScheduleService : IBaseServices<GC_Rules_Warning_EmailSchedule, EPAD_Context>
    {
        Task<List<GC_Rules_Warning_EmailSchedule>> GetSchedulesByTimeAndCompanyIndex(TimeSpan time, int dayOfWeekIndex, int companyIndex);
        GC_Rules_Warning_EmailSchedule GetFromDateSendMail(GC_Rules_Warning_EmailSchedule schedule);
        void UpdateSchedule(GC_Rules_Warning_EmailSchedule schedule);

    }
}
