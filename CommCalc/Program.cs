using Hangfire;
using Hangfire.PostgreSql;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        var hangfireConn = configuration.GetConnectionString("Hangfire")
            ?? throw new InvalidOperationException("Missing connection string: ConnectionStrings:Hangfire");

        // Configure Hangfire to use PostgreSQL storage (production-grade persistence)
        services.AddHangfire(cfg =>
        {
            cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
               .UseSimpleAssemblyNameTypeSerializer()
               .UseRecommendedSerializerSettings()
               .UsePostgreSqlStorage(hangfireConn, new PostgreSqlStorageOptions
               {
                   SchemaName = "hangfire",
                   QueuePollInterval = TimeSpan.FromSeconds(15),
                   InvisibilityTimeout = TimeSpan.FromMinutes(5),
                   DistributedLockTimeout = TimeSpan.FromMinutes(1),
                   PrepareSchemaIfNecessary = true
               });
        });

        // Run Hangfire Server in this worker
        services.AddHangfireServer(options =>
        {
            options.ServerName = $"CommCalc-Hangfire-{Environment.MachineName}";
            // WorkerCount tuned for production: base on cores; you can override via config
            options.WorkerCount = Environment.ProcessorCount * 5;
            options.Queues = new[] { "critical", "default" };
        });

        // Background service to register recurring jobs and perform bootstrap tasks
        services.AddHostedService<HangfireBootstrapper>();
        // register processor + worker for Hangfire jobs
        services.AddScoped<CommCalc.Services.IChunkProcessor, CommCalc.Services.ChunkProcessor>();
        services.AddTransient<CommCalc.Jobs.IChunkWorker, CommCalc.Jobs.ChunkWorker>();
    })
    .Build();
await host.RunAsync();