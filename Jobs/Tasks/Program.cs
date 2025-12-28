using CommonLibrary.mapping;
using Database;
using Npgsql;
using Repository;
using System.Data.Common;
using Tasks.Database;

DbProviderFactories.RegisterFactory("Npgsql", NpgsqlFactory.Instance);

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

// Register scoped connection scope to reuse open connections within a DI scope
builder.Services.AddScoped<IConnectionScope, ConnectionScope>();

// Register repository
builder.Services.AddScoped<IJobConfigRepository, JobConfigRepository>();

// Example DI registrations to ensure ReflectionJob and dependencies can be resolved.
// Place these near other service registrations (before building the host).
builder.Services.AddTransient<Jobs.ReflectionJob>();

// (optional) register your factory in DI if you want to resolve it instead of constructing directly
builder.Services.AddSingleton<Quartz.Spi.IJobFactory, Jobs.ServiceProviderJobFactory>();

//// Register jobs so DI can resolve them
//builder.Services.AddScoped<SampleDbJob>();

// Register worker
builder.Services.AddHostedService<Tasks.Worker>();

var host = builder.Build();
await host.RunAsync();