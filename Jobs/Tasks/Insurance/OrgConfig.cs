using CommonLibrary.mapping;
using Dapper;
using Quartz;
using Repository;
using Tasks.Models;

namespace Tasks.Finance
{
    public class OrgConfig
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        private readonly IMappingProvider _mappingProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger<OrgConfig> _logger;
        private readonly IConnectionScope _connectionScope;

        private readonly int orgId;
        public JobKey jobKey;

        public OrgConfig(
            IJobExecutionContext jobExecutionContext,
            IMappingProvider mappingProvider,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            ILogger<OrgConfig> logger,
            IConnectionScope connectionScope)
        {
            _jobExecutionContext = jobExecutionContext;
            orgId = int.Parse(jobExecutionContext.JobDetail.JobDataMap.Values
                .Select((value, index) => new { value, index })
                .FirstOrDefault(x => x.index.Equals(
                    jobExecutionContext.JobDetail.JobDataMap.Keys
                    .Select((value, index) => new { value, index })
                    .FirstOrDefault(x => x.value == "orgId").index))
                .value.ToString());

            jobKey = jobExecutionContext.JobDetail.Key;
            _mappingProvider = mappingProvider ?? throw new ArgumentNullException(nameof(mappingProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionScope = connectionScope ?? throw new ArgumentNullException(nameof(connectionScope));
        }

        public async Task SetMonthlyConfiguration(JobExeHist jobExeHist, CancellationToken ct)
        {
            if (DateTime.UtcNow.Day != 1)
            {
                _logger.LogInformation("Skipping OrgConfig monthly setup. Today is not day 1. OrgId={OrgId}", jobExeHist.OrgId);
                return;
            }

            var monthSqlMapping = _mappingProvider.GetScriptForOperation("Organisation", "GenerateFinancialMonths")
                ?? throw new InvalidOperationException("Operation mapping for Organisation/GenerateFinancialMonths not found.");
            var quarterSqlMapping = _mappingProvider.GetScriptForOperation("Organisation", "GenerateFinancialQuarters")
                ?? throw new InvalidOperationException("Operation mapping for Organisation/GenerateFinancialQuarters not found.");

            var connectionString = _configuration.GetConnectionString(monthSqlMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{monthSqlMapping.ConnectionStringKey}' not found.");

            await using var conn = await _connectionScope.GetOpenConnectionAsync(connectionString);
            await using var tx = await conn.BeginTransactionAsync(ct);

            try
            {
                await conn.ExecuteAsync(
                    new CommandDefinition(
                        monthSqlMapping.Script,
                        new { p_organization_id = jobExeHist.OrgId },
                        tx,
                        cancellationToken: ct));

                await conn.ExecuteAsync(
                    new CommandDefinition(
                        quarterSqlMapping.Script,
                        new { p_organization_id = jobExeHist.OrgId },
                        tx,
                        cancellationToken: ct));

                await tx.CommitAsync(ct);
                _logger.LogInformation("Monthly period setup completed for OrgId={OrgId}", jobExeHist.OrgId);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "Monthly period setup failed for OrgId={OrgId}", jobExeHist.OrgId);
                throw;
            }
        }
    }
}
