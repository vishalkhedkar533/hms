using Dapper;
using Database;
using Quartz;
using Repository;

namespace Jobs
{
    /// <summary>
    /// DI-created job that demonstrates using repository pattern + Dapper for DB work.
    /// The Worker schedules this job and supplies JobConfigId via JobDataMap.
    /// </summary>
    //public class SampleDbJob : IJob
    //{
    //    private readonly ILogger<SampleDbJob> _logger;
    //    private readonly IJobConfigRepository _jobConfigRepository;
    //    private readonly IConnectionFactory _connectionFactory;
    //    private readonly string _connectionString;

    //    public SampleDbJob(
    //        ILogger<SampleDbJob> logger,
    //        IJobConfigRepository jobConfigRepository,
    //        IConnectionFactory connectionFactory,
    //        IConfiguration configuration)
    //    {
    //        _logger = logger;
    //        _jobConfigRepository = jobConfigRepository;
    //        _connectionFactory = connectionFactory;
    //        _connectionString = configuration.GetConnectionString("HMSContext")
    //                            ?? configuration.GetValue<string>("ConnectionString")
    //                            ?? string.Empty;
    //    }

    //    public async Task Execute(IJobExecutionContext context)
    //    {
    //        var dataMap = context.JobDetail.JobDataMap;
    //        var jobConfigId = dataMap.ContainsKey("JobConfigId") ? dataMap.GetInt("JobConfigId") : 0;

    //        _logger.LogInformation("SampleDbJob starting. JobConfigId={JobConfigId} Time={Time}", jobConfigId, DateTime.UtcNow);

    //        try
    //        {
    //            Models.JobConfig? config = null;
    //            if (jobConfigId > 0)
    //            {
    //                config = await _jobConfigRepository.GetByIdAsync(jobConfigId);
    //                _logger.LogDebug("Loaded job config: {@Config}", config);
    //            }

    //            if (string.IsNullOrWhiteSpace(_connectionString))
    //            {
    //                _logger.LogWarning("No DB connection string configured; skipping DB work.");
    //                return;
    //            }

    //            await using var conn = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection(_connectionString);
    //            await conn.OpenAsync();

    //            // Example Dapper work - replace with your repository/CRUD logic.
    //            var result = await conn.QueryFirstOrDefaultAsync<int>("SELECT 1");
    //            _logger.LogInformation("SampleDbJob test query returned {Result}", result);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "SampleDbJob failed for JobConfigId={JobConfigId}", jobConfigId);
    //            throw;
    //        }
    //    }
    //}

}
