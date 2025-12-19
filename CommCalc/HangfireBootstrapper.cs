using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal sealed class HangfireBootstrapper : BackgroundService
{
    private readonly ILogger<HangfireBootstrapper> _logger;

    public HangfireBootstrapper(ILogger<HangfireBootstrapper> logger) => _logger = logger;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Hangfire bootstrapper starting. Registering recurring jobs...");

        // Example recurring job registration. Replace with DI-resolved services as needed.
        RecurringJob.AddOrUpdate(
            "Example.DailyCleanup",
            () => ExampleJobs.DailyCleanupAsync(),
            Cron.Daily,
            TimeZoneInfo.Utc);

        _logger.LogInformation("Recurring jobs registered.");
        return Task.CompletedTask;
    }
}

public static class ExampleJobs
{
    // Keep job methods public and parameterless (or accept DI types if you use IServiceScope inside job)
    public static void DailyCleanupAsync()
    {
        // Minimal example; replace with real work
        Console.WriteLine($"[{DateTime.UtcNow:O}] Hangfire job: DailyCleanup executed.");
    }
}