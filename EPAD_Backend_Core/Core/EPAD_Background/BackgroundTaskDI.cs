using EPAD_Background.Queue;
using EPAD_Background.Schedule;
using Microsoft.Extensions.DependencyInjection;

namespace EPAD_Background
{
    public static class BackgroundTaskDI
    {
        public static IServiceCollection AddBackgroundTask(this IServiceCollection services)
        {
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHostedService<QueuedHostedService>();
            services.AddHostedService<ScheduleAutoHostedService>();

            services.RegisterSchedule();
            return services;
        }
    }
}
