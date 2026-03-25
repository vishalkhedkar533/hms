using CommonLibrary.mapping;
using Dapper;
using Quartz;
using Repository;
using Tasks.Models;

namespace Tasks.Insurance
{
    public class DashboardConfig
    {
        private readonly IMappingProvider _mappingProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger<DashboardConfig> _logger;
        private readonly IConnectionScope _connectionScope;

        public DashboardConfig(
            IJobExecutionContext jobExecutionContext,
            IMappingProvider mappingProvider,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            ILogger<DashboardConfig> logger,
            IConnectionScope connectionScope)
        {
            _mappingProvider = mappingProvider ?? throw new ArgumentNullException(nameof(mappingProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionScope = connectionScope ?? throw new ArgumentNullException(nameof(connectionScope));
        }

        public async Task SyncDashboardFromAgent(JobExeHist jobExeHist, CancellationToken ct)
        {
            var mapping = _mappingProvider.GetScriptForOperation("Dashboard", "UpsertFromAgent")
                ?? throw new InvalidOperationException("Operation mapping for Dashboard/UpsertFromAgent not found.");

            var connectionString = _configuration.GetConnectionString(mapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{mapping.ConnectionStringKey}' not found.");

            await using var conn = await _connectionScope.GetOpenConnectionAsync(connectionString);
            await using var tx = await conn.BeginTransactionAsync(ct);

            try
            {
                var affected = await conn.ExecuteAsync(
                    new CommandDefinition(
                        mapping.Script,
                        new { OrgId = jobExeHist.OrgId },
                        tx,
                        cancellationToken: ct));

                await tx.CommitAsync(ct);
                _logger.LogInformation("Dashboard sync completed. OrgId={OrgId}, Affected={Affected}", jobExeHist.OrgId, affected);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "Dashboard sync failed for OrgId={OrgId}", jobExeHist.OrgId);
                throw;
            }
        }
    }
}
