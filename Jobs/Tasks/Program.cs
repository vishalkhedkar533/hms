using CommonLibrary.mapping;
using Database;
using Quartz;
using Repository;

var builder = Host.CreateApplicationBuilder(args);

// Register logging and configuration are available by default via HostBuilder
builder.Services.AddLogging();

// Register mapping provider (loads mapping/*.json files like mappings.postgresql.json)
builder.Services.AddSingleton<IMappingProvider>(sp =>
{
    var env = sp.GetRequiredService<IHostEnvironment>();
    var mappingDir = Path.Combine(env.ContentRootPath, "mapping");
    return new FileMappingProvider(mappingDir);
});

// Register connection factory (NpgsqlConnectionFactory already exists in Database namespace)
builder.Services.AddSingleton<IConnectionFactory, ProviderConnectionFactory>();

// Register repository
builder.Services.AddScoped<IJobConfigRepository, JobConfigRepository>();

//// Register jobs so DI can resolve them
//builder.Services.AddScoped<SampleDbJob>();

// Register worker
builder.Services.AddHostedService<Tasks.Worker>();

// Register Quartz and use Microsoft DI to create job instances
//builder.Services.AddQuartz(q =>
//{
    // Use DI for job instantiation
    // Register the job with an identity
    //q.AddJob<SampleDbJob>(opts => opts.WithIdentity("sample-db-job", "default"));

    //// Create a simple trigger that starts immediately and repeats every 60 seconds
    //q.AddTrigger(opts => opts
    //    .ForJob("sample-db-job", "default")
    //    .WithIdentity("sample-db-trigger", "default")
    //    .StartNow()
    //    .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever())
    //);
//});

// Register Quartz hosted service (starts the scheduler)
//builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var host = builder.Build();
await host.RunAsync();
