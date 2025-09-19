using EPAD_Background.Schedule.Job;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace EPAD_Background.Schedule
{
    public static class ScheduleDI
    {
        public static IServiceCollection RegisterSchedule(this IServiceCollection services)
        {
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            services.AddSingleton<CheckWarningViolation>();
            services.AddSingleton(new JobSchedule(pJobType: typeof(CheckWarningViolation), pCronExpression: "0 * * ? * *", pDoBackground: true));

            services.AddSingleton<CheckLicense>();
            services.AddSingleton(new JobSchedule(pJobType: typeof(CheckLicense), pCronExpression: "", pDoBackground: true));

            services.AddSingleton<CheckHardwareLicense>();
            services.AddSingleton(new JobSchedule(typeof(CheckHardwareLicense), "", false));

            services.AddHostedService<QuartzHostedService>();
            services.AddSingleton<IScheduler>(provider =>
            {
                ISchedulerFactory schedFact = provider.GetService<ISchedulerFactory>();
                IScheduler scheduler = schedFact.GetScheduler().Result;
                scheduler.JobFactory = provider.GetService<IJobFactory>();
                return scheduler;
            });

            return services;
        }
    }
}
