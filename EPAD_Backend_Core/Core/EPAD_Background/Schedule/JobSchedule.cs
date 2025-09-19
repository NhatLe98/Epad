using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Background.Schedule
{
    public class JobSchedule
    {
        public Type JobType { get; }
        public string CronExpression { get; }
        public bool DoBackground { get; set; }
        public string Name { get; set; }
        public JobSchedule(Type pJobType, string pCronExpression, bool pDoBackground)
        {
            JobType = pJobType;
            CronExpression = pCronExpression;
            DoBackground = pDoBackground;
        }
    }
}
