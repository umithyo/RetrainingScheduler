using System.IO.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewGlobe.Interview.RetrainingScheduler.Services;
using NewGlobe.Interview.RetrainingScheduler.Services.Scheduler;
using Serilog;

namespace NewGlobe.Interview.RetrainingScheduler;

public class Program
{
    public static async Task<int> Main(params string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .Enrich.WithMachineName()
                .CreateLogger();

        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddScheduler(configuration.GetSection("Scheduler"));
                    services.AddScoped<IFileSystem, FileSystem>();
                })
                .Build();

            var schedulerService = host.Services.GetRequiredService<ISchedulerService>();

            await schedulerService.ScheduleAsync();

            Log.Information("Application has started");

            await host.RunAsync();

            return 0;
        }
        catch (Exception e)
        {
            Log.Error("Application unexpectedly closed");
            return 1;
        }
    }
}