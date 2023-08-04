using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NewGlobe.Interview.RetrainingScheduler.Services.Scheduler;

public static class SchedulerServiceCollectionExtensions
{
    public static IServiceCollection AddScheduler(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISchedulerService, SchedulerService>();
        services.Configure<SchedulerConfiguration>(configuration);

        return services;
    }
}