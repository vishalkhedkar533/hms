using CommisionCalc.Infrastructure.Repository.Insurance;
using CommisionCalc.Insurance.Interfaces;
using CommisionCalc.Jobs;
using Npgsql; // Ensure this using directive is present
using Quartz; // Add this using directive
using System.Data;

var builder = Host.CreateApplicationBuilder(args);
// 1. Register Dapper Connection (Scoped)
switch (builder.Configuration.GetSection("DBType:DatabaseType").Value)
{
    case "PostgreSQL":
        builder.Services.AddScoped<IDbConnection>(sp =>
            new NpgsqlConnection(builder.Configuration.GetConnectionString("HMSContext")));
        break;
    default:
        break;
}

// 2. Register Repository
builder.Services.AddScoped<ICommissionConfig, CommissionConfig>();

// 3. Configure Quartz.NET
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("DataProcessingJob");

    q.AddJob<Job_ProcessCommission>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("Job_ProcessCommission")
        .WithCronSchedule("0 0/5 * * * ?")); // Every 5 minutes
});

// 4. Add Quartz Hosted Service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();
