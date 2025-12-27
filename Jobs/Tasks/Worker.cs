using Jobs;
using Quartz;
using Quartz.Impl;
using Repository;

namespace Tasks
{
    public class Worker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Worker(IConfiguration configuration,
                      ILogger<Worker> logger,
                      IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker starting - initializing Quartz scheduler.");

            var factory = new StdSchedulerFactory();
            var scheduler = await factory.GetScheduler(stoppingToken);

            // Use DI-based JobFactory so jobs are resolved from IServiceProvider
            scheduler.JobFactory = new ServiceProviderJobFactory(_serviceProvider);

            // Create an async scope when accessing scoped services (e.g. repositories)
            IEnumerable<Models.JobConfig> configs;
            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var repo = scope.ServiceProvider.GetRequiredService<IJobConfigRepository>();
                configs = await repo.GetEnabledAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load job configurations. Scheduler will not start.");
                return;
            }

            foreach (var cfg in configs)
            {
                try
                {
                    var jobKey = new JobKey($"job-{cfg.Id}", "scheduler");

                    // build job detail using the generic ReflectionJob
                    var jobDetail = JobBuilder.Create(typeof(ReflectionJob))
                        .WithIdentity(jobKey)
                        .UsingJobData("TargetType", /* e.g. cfg.Job_Type or parsed from cfg.Parameters */ cfg.Job_Type ?? string.Empty)
                        .UsingJobData("TargetMethod", /* parse or supply method name */ "Run")
                        .UsingJobData("Args", cfg.Parameters ?? string.Empty) // pass JSON payload
                        .Build();

                    ITrigger? trigger = cfg.Trigger_Type?.ToLowerInvariant() switch
                    {
                        "cron" when !string.IsNullOrWhiteSpace(cfg.Cron_Expression) =>
                            TriggerBuilder.Create()
                                .WithIdentity($"trigger-{cfg.Id}", "scheduler")
                                .ForJob(jobKey)
                                .StartAt(cfg.Start_At ?? DateTimeOffset.UtcNow)
                                .EndAt(cfg.End_At)
                                .WithCronSchedule(cfg.Cron_Expression)
                                .Build(),

                        "interval" when cfg.Interval_Seconds.HasValue =>
                            TriggerBuilder.Create()
                                .WithIdentity($"trigger-{cfg.Id}", "scheduler")
                                .ForJob(jobKey)
                                .StartAt(cfg.Start_At ?? DateTimeOffset.UtcNow)
                                .EndAt(cfg.End_At)
                                .WithSimpleSchedule(x => x.WithIntervalInSeconds(cfg.Interval_Seconds.Value).RepeatForever())
                                .Build(),

                        "onetime" =>
                            TriggerBuilder.Create()
                                .WithIdentity($"trigger-{cfg.Id}", "scheduler")
                                .ForJob(jobKey)
                                .StartAt(cfg.Start_At ?? DateTimeOffset.UtcNow)
                                .Build(),

                        _ => null
                    };

                    if (trigger == null)
                    {
                        _logger.LogWarning("JobConfig {Id} has unsupported trigger configuration - skipping.", cfg.Id);
                        continue;
                    }

                    if (await scheduler.CheckExists(jobKey, stoppingToken))
                    {
                        await scheduler.DeleteJob(jobKey, stoppingToken);
                    }

                    await scheduler.ScheduleJob(jobDetail, trigger, stoppingToken);

                    _logger.LogInformation("Scheduled job {JobName} (Id={Id}) with trigger {TriggerKey}.", cfg.Job_Name, cfg.Id, trigger.Key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to schedule job config Id={Id}", cfg.Id);
                }
            }

            await scheduler.Start(stoppingToken);
            _logger.LogInformation("Quartz scheduler started, scheduled {Count} jobs.", configs?.Count() ?? 0);

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException) { /* expected on shutdown */ }
            finally
            {
                _logger.LogInformation("Stopping Quartz scheduler.");
                await scheduler.Shutdown(waitForJobsToComplete: true, cancellationToken: CancellationToken.None);
                _logger.LogInformation("Quartz scheduler stopped.");
            }
        }
    }
}
