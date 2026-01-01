using System.Text.Json;
using System.Collections.Concurrent;
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

        // snapshot of scheduled job signatures keyed by JobConfig.Id
        private readonly ConcurrentDictionary<int, string> _configSnapshot = new();

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

            // initial load + schedule
            await RefreshAllConfigsAsync(scheduler, stoppingToken);

            await scheduler.Start(stoppingToken);
            _logger.LogInformation("Quartz scheduler started, scheduled {Count} jobs.", _configSnapshot.Count);

            // Start a polling loop to detect job_config changes (no DB triggers required)
            _ = Task.Run(() => PollForConfigChangesAsync(scheduler, pollingIntervalSeconds: 15, stoppingToken), CancellationToken.None);

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

        private async Task PollForConfigChangesAsync(IScheduler scheduler, int pollingIntervalSeconds, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(pollingIntervalSeconds), stoppingToken);
                    await RefreshAllConfigsAsync(scheduler, stoppingToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while polling for job_config changes.");
                }
            }
        }

        private async Task RefreshAllConfigsAsync(IScheduler scheduler, CancellationToken stoppingToken)
        {
            IEnumerable<Models.JobConfig> configs;
            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var repo = scope.ServiceProvider.GetRequiredService<IJobConfigRepository>();
                configs = await repo.GetEnabledAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load job configurations.");
                return;
            }

            // Build new snapshot and compare
            var newSnapshot = new Dictionary<int, (string Signature, Models.JobConfig Config)>();
            foreach (var cfg in configs)
            {
                var sig = ComputeSignature(cfg);
                newSnapshot[cfg.Id] = (sig, cfg);
            }

            // Add or update
            foreach (var kv in newSnapshot)
            {
                var id = kv.Key;
                var sig = kv.Value.Signature;
                var cfg = kv.Value.Config;

                if (!_configSnapshot.TryGetValue(id, out var existingSig))
                {
                    // new job
                    _logger.LogInformation("Detected new job config Id={Id} -> scheduling.", id);
                    await UpsertJobAsync(scheduler, cfg, stoppingToken);
                    _configSnapshot[id] = sig;
                }
                else if (existingSig != sig)
                {
                    // changed job
                    _logger.LogInformation("Detected changed job config Id={Id} -> rescheduling.", id);
                    await UpsertJobAsync(scheduler, cfg, stoppingToken);
                    _configSnapshot[id] = sig;
                }
            }

            // Remove deleted
            var removed = _configSnapshot.Keys.Except(newSnapshot.Keys).ToList();
            foreach (var id in removed)
            {
                _logger.LogInformation("Detected removed job config Id={Id} -> unscheduling.", id);
                await RemoveJobAsync(scheduler, id, stoppingToken);
                _configSnapshot.TryRemove(id, out _);
            }
        }

        private static string ComputeSignature(Models.JobConfig cfg)
        {
            // include only fields that affect scheduling/behavior
            var obj = new
            {
                cfg.Job_Type,
                cfg.TargetMethod,
                cfg.Parameters,
                cfg.Trigger_Type,
                cfg.Cron_Expression,
                cfg.Interval_Seconds,
                cfg.Start_At,
                cfg.End_At
            };
            return JsonSerializer.Serialize(obj);
        }

        private async Task UpsertJobAsync(IScheduler scheduler, Models.JobConfig cfg, CancellationToken stoppingToken)
        {
            var jobKey = new JobKey($"{cfg.Id}", "scheduler");

            // build job detail using the ReflectionJob (use non-generic Create to avoid IJob type mismatches)
            var jobDetail = JobBuilder.Create(typeof(ReflectionJob))
                .WithIdentity(jobKey)
                .UsingJobData("TargetType", cfg.TargetType ?? string.Empty)
                .UsingJobData("TargetMethod", cfg.TargetMethod ?? "Run")
                .UsingJobData("Args", cfg.Parameters ?? string.Empty)
                .UsingJobData("orgId", cfg.orgid.ToString() ?? string.Empty)
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
                return;
            }

            if (await scheduler.CheckExists(jobKey, stoppingToken))
            {
                await scheduler.DeleteJob(jobKey, stoppingToken);
            }

            await scheduler.ScheduleJob(jobDetail, trigger, stoppingToken);
            _logger.LogInformation("Scheduled job {JobName} (Id={Id}).", cfg.Job_Name, cfg.Id);
        }

        private async Task RemoveJobAsync(IScheduler scheduler, int cfgId, CancellationToken stoppingToken)
        {
            var jobKey = new JobKey($"job-{cfgId}", "scheduler");
            if (await scheduler.CheckExists(jobKey, stoppingToken))
            {
                await scheduler.DeleteJob(jobKey, stoppingToken);
                _logger.LogInformation("Removed scheduled job Id={Id}.", cfgId);
            }
        }
    }
}