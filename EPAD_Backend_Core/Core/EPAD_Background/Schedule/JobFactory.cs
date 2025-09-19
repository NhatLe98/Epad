using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Background.Schedule
{
    public class JobFactory : IJobFactory
    {
        private readonly IServiceProvider _container;
        public JobFactory(IServiceProvider container)
        {
            _container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            if (bundle == null) throw new ArgumentNullException(nameof(bundle));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));

            var jobDetail = bundle.JobDetail;
            var newJob = (IJob)_container.GetService(jobDetail.JobType);

            if (newJob == null)
                throw new SchedulerConfigException(string.Format("Failed to instantiate Job {0} of type {1}", jobDetail.Key, jobDetail.JobType));

            return newJob;
        }

        public void ReturnJob(IJob job)
        {
            if (job is IDisposable)
            {
                var disposableJob = (IDisposable)job;
                disposableJob.Dispose();
            }
        }
    }
}
