using CommonLibrary.mapping;
using Dapper;
using Database;
using MiniExcelLibs;
using Quartz;
using Repository;
using SharedModels.BackEndCalculation;
using Tasks.Models;

namespace Tasks.Insurance
{
    public class PremiumUploadExcel
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        private int orgId = 0;
        public JobKey jobKey;
        private readonly ILogger<PolicyExcelUpload> _logger;
        private readonly IMappingProvider _mappingProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IConnectionScope _connectionScope;
        private readonly IBinaryImportFactory _bulkOpsFactory;

        public PremiumUploadExcel(IJobExecutionContext jobExecutionContext,
        ILogger<PolicyExcelUpload> logger,
        IMappingProvider mappingProvider,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        IConnectionScope connectionScope,
        IBinaryImportFactory bulkOpsFactory)
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
            _logger = logger;
            _mappingProvider = mappingProvider ?? throw new ArgumentNullException(nameof(mappingProvider));

            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _connectionScope = connectionScope ?? throw new ArgumentNullException(nameof(connectionScope));

            _bulkOpsFactory = bulkOpsFactory ?? throw new ArgumentNullException(nameof(bulkOpsFactory));
        }

        public async Task UploadPremiumData(JobExeHist jobExeHist)
        {
            _logger.LogInformation("PremiumExcelUpload job started for OrgId={OrgId}", orgId);

            var operationMapping = _mappingProvider.GetScriptForOperation("Job", "Fetch")
                ?? throw new InvalidOperationException("Operation mapping for Job/Fetch not found.");

            var connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException("Connection string not found.");

            var conn = await _connectionScope.GetOpenConnectionAsync(connectionString);
            var token = CancellationToken.None;

            var tasksSql = _mappingProvider.GetScriptForOperation("insu_core.premium_collected", "GetPendingTasks")?.Script
                ?? throw new Exception("GetPendingTasks SQL missing");

            var tasks = await conn.QueryAsync<FileProcessingTask>(tasksSql);

            if (!tasks.Any())
            {
                _logger.LogInformation("No pending premium tasks found");
                return;
            }
            var chunkSql = _mappingProvider.GetScriptForOperation("Agent", "GetChunkSize")?.Script
                     ?? throw new Exception("SQL for GetChunkSize missing");
            int chunkSize = await conn.ExecuteScalarAsync<int>(chunkSql, new { key = "policy_update_chunk_size" });
            if (chunkSize <= 0) chunkSize = 1000;
            foreach (var task in tasks)
            {
                _logger.LogInformation("Processing File for OrgId={OrgId}", orgId);

                if (!File.Exists(task.FilePath))
                {
                    _logger.LogError("File not found: {FilePath}. Skipping TaskId={TaskId}", task.FilePath, task.Id);
                    continue;
                }
                var rows = MiniExcel.Query<PremiumCollected>(task.FilePath);
                int rowCount = 0;

                foreach (var batch in rows.Chunk(chunkSize))
                {
                    var batchList = batch.ToList();

                    foreach (var row in batchList)
                    {
                        rowCount++;
                        row.OrgId = (int)task.OrgId;

                        var errors = ValidatePremiumRow(row, rowCount);

                        row.Comments = errors.Any() ? "Rejected" : "Processed";
                        row.Reason = errors.Any() ? $"Row {rowCount}: {string.Join(" | ", errors)}" : null;
                    }

                    var bulkSql = _mappingProvider.GetScriptForOperation("insu_core.premium_collected", "BulkCopyTempPremium")?.Script
                        ?? throw new Exception("Bulk COPY SQL missing");

                    await using var writer = await _bulkOpsFactory.BeginBinaryImportAsync(conn, bulkSql, token);

                    foreach (var r in batchList)
                    {
                        writer.StartRow();
                        writer.Write(r.OrgId.ToString());
                        writer.Write(r.PolicyRef?.ToString() ?? "");
                        writer.Write(r.PremiumReceivedDt?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.PremiumType?.ToString() ?? "");
                        writer.Write(r.PremiumAmt?.ToString() ?? "");
                        writer.Write(r.PremCollectedYr?.ToString() ?? "");
                        writer.Write(r.PremCollectedQtr?.ToString() ?? "");
                        writer.Write(r.PremCollectedFinYr ?? "");
                        writer.Write(r.Comments ?? "");
                        writer.Write(r.Reason ?? "");
                    }

                    await writer.CompleteAsync(token);
                }

                var finalizeSql = _mappingProvider.GetScriptForOperation("insu_core.premium_collected", "FinalizePremium")?.Script;
                using var tx = await conn.BeginTransactionAsync(token);
                await conn.ExecuteAsync(finalizeSql, transaction: tx);
                await tx.CommitAsync(token);
            }

            _logger.LogInformation("Premium Excel Upload Job Finished");
        }
        private static List<string> ValidatePremiumRow(PremiumCollected row, int rowNo)
        {
            var errors = new List<string>();

            if (row.PremCollectedYr == null)
                errors.Add("PremCollectedYr is mandatory");

            if (row.PremCollectedQtr == null)
                errors.Add("PremCollectedQtr is mandatory");

            if (string.IsNullOrWhiteSpace(row.PremCollectedFinYr))
                errors.Add("PremCollectedFinYr is mandatory");

            if (row.PremiumAmt != null && row.PremiumAmt < 0)
                errors.Add("PremiumAmt cannot be negative");

            return errors;
        }
    }
}
