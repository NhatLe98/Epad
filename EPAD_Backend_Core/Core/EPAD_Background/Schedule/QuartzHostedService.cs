using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EPAD_Background.Schedule
{
    public class QuartzHostedService : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly IEnumerable<JobSchedule> _jobSchedules;

        public QuartzHostedService(ISchedulerFactory schedulerFactory, IJobFactory jobFactory, IEnumerable<JobSchedule> jobSchedules)
        {
            _schedulerFactory = schedulerFactory;
            _jobSchedules = jobSchedules;
            _jobFactory = jobFactory;
            
        }
        public IScheduler Scheduler { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine();
            Console.WriteLine("Job schedule is starting.");
            Console.ResetColor();

            Scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            Scheduler.JobFactory = _jobFactory;

            foreach (var jobSchedule in _jobSchedules)
            {
                if (jobSchedule.DoBackground == false) continue;
                var job = CreateJob(jobSchedule);
                var trigger = CreateTrigger(jobSchedule);

                await Scheduler.ScheduleJob(job, trigger, cancellationToken);
            }

            await Scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine();
            Console.WriteLine("Job schedule is stopping.");
            Console.ResetColor();
            await Scheduler?.Shutdown(cancellationToken);
        }

        private static IJobDetail CreateJob(JobSchedule schedule)
        {
            var jobType = schedule.JobType;
            return JobBuilder
                .Create(jobType)
                .WithIdentity($"epad.job.checkLicense.{jobType.Name}", "checkLicense")
                .WithDescription(jobType.Name)
                .Build();
        }

        private static ITrigger CreateTrigger(JobSchedule schedule)
        {
            if(schedule.CronExpression != "")
            {
                return TriggerBuilder
                    .Create()
                    .WithIdentity($"epad.trigger.checkLicense.{schedule.JobType.Name}", "checkLicense")
                    .StartAt(DateTime.Now.AddSeconds(2))
                    .WithCronSchedule(schedule.CronExpression)
                    .WithDescription(schedule.CronExpression)
                    .Build();
            }
            else
            {
                return TriggerBuilder
                    .Create()
                    .WithIdentity($"epad.trigger.checkLicense.{schedule.JobType.Name}", "checkLicense")
                    .StartAt(DateTime.Now.AddSeconds(2))
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(300) // 5 minutes
                        .RepeatForever())
                    .Build();
            }
        }
    }
}
