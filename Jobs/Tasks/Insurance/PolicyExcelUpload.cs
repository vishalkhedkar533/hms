using CommonLibrary.mapping;
using Dapper;
using Database;
using MiniExcelLibs;
using Quartz;
using Repository;
using System.Text.RegularExpressions;
using Tasks.Models;
using Tasks.Models.DB;

namespace Tasks.Insurance
{
    public class PolicyExcelUpload
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        private int orgId = 0;
        public JobKey jobKey;
        private readonly ILogger<PolicyExcelUpload> _logger;
        private readonly IMappingProvider _mappingProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IConnectionScope _connectionScope;
        private readonly IBinaryImportFactory _bulkOpsFactory;


        public PolicyExcelUpload(IJobExecutionContext jobExecutionContext,
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
        public async Task UploadPolicyData()
        {
            _logger.LogInformation("PolicyExcelUpload job started for OrgId={OrgId}", orgId);

            var operationMapping = _mappingProvider.GetScriptForOperation("Job", "Fetch")
                ?? throw new InvalidOperationException("Operation mapping for Job/Fetch not found.");

            var connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

            var conn = await _connectionScope.GetOpenConnectionAsync(connectionString);
            var token = CancellationToken.None;

            var tasksSql = _mappingProvider.GetScriptForOperation("Policy", "GetPendingTasks")?.Script
                ?? throw new Exception("SQL for GetPendingTasks missing");
            var tasks = await conn.QueryAsync<FileProcessingTask>(tasksSql);

            if (!tasks.Any())
            {
                _logger.LogInformation("No pending agent create tasks found");
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

                var rows = MiniExcel.Query<Ins_Policy>(task.FilePath);
                int rowCount = 0;

                foreach (var batch in rows.Chunk(chunkSize))
                {
                    var batchList = batch.ToList();
                    foreach (var row in batchList)
                    {
                        rowCount++;
                        row.OrgId = (int)task.OrgId;

                        var errors = ValidatePolicyRow(row, rowCount);

                        row.Comments = errors.Any() ? "Rejected" : "Processed";
                        row.Reason = errors.Any() ? $"Row {rowCount}: {string.Join(" | ", errors)}" : null;
                    }

                    _logger.LogInformation("Start Process Inserting Data From Excel");

                    var bulkSql = _mappingProvider.GetScriptForOperation("Policy", "BulkCopyTempPolicy")?.Script;
                    if (string.IsNullOrWhiteSpace(bulkSql))
                        throw new Exception("Bulk COPY SQL for BulkCopy TempPolicy missing");

                    await using var writer = await _bulkOpsFactory.BeginBinaryImportAsync(conn, bulkSql, token);
                    foreach (var r in batchList)
                    {
                        writer.StartRow();

                        writer.Write(r.OrgId.ToString());
                        writer.Write(r.PolicyNo ?? "");
                        writer.Write(r.PolicySuffix ?? "");
                        writer.Write(r.RiskStartDt?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.RiskEndDt?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.PolicyTerm?.ToString() ?? "");
                        writer.Write(r.PremPayingTerm?.ToString() ?? "");
                        writer.Write(r.ProposerClientId ?? "");
                        writer.Write(r.LifeInsuredClientId ?? "");
                        writer.Write(r.AgentId?.ToString() ?? "");
                        writer.Write(r.IsStaffPolicy?.ToString() ?? "");
                        writer.Write(r.PolicySourceCode?.ToString() ?? "");
                        writer.Write(r.InsuredPan ?? "");
                        writer.Write(r.ProposerPan ?? "");
                        writer.Write(r.InsuredDob?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.ProposerDob?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.LoginDt?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.InsuredGender?.ToString() ?? "");
                        writer.Write(r.ProposerGender?.ToString() ?? "");
                        writer.Write(r.MaturityAgeInMonths?.ToString() ?? "");
                        writer.Write(r.ModalBasePremium?.ToString() ?? "");
                        writer.Write(r.ModalBaseRiderPremium?.ToString() ?? "");
                        writer.Write(r.Comments ?? "");
                        writer.Write(r.Reason ?? "");
                    }

                    await writer.CompleteAsync(token);

                    _logger.LogInformation("Execel Data Successfully Inserted Into Temp Policy Table");
                }

                var finalizeSql = _mappingProvider.GetScriptForOperation("Policy", "FinalizePolicy")?.Script;
                using (var tx = await conn.BeginTransactionAsync(token))
                {
                    await conn.ExecuteAsync(finalizeSql, transaction: tx);
                    await tx.CommitAsync(token);
                }
            }
            _logger.LogInformation("Policy Execel Upload Job Finished");

            _logger.LogInformation("Policy Excel Upload Job Finished. Checking for rejected rows...");

            // Step 1: Get rejected rows from temp table
            var exportSql = _mappingProvider.GetScriptForOperation("Policy", "ExportRejectedPolicy")!.Script;

            // Pass OrgId as a parameter
            var rejectedRows = await conn.QueryAsync<Ins_Policy>(exportSql,new { OrgId = orgId });

            if (rejectedRows.Any())
            {
                // Step 2: Generate Excel file for rejected rows
                var fileName = $"Rejected_Policies_{orgId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                var rootFolder = @"E:\HMS2\Jobs\Tasks\RejectedFiles"; // choose your folder
                Directory.CreateDirectory(rootFolder);

                var filePath = Path.Combine(rootFolder, fileName);

                // Step 3: Save the object list to Excel
                await using var stream = new FileStream(filePath, FileMode.Create);
                await MiniExcel.SaveAsAsync(stream, rejectedRows);

                _logger.LogInformation("Rejected rows exported to Excel: {FilePath}", filePath);
            }
            else
            {
                _logger.LogInformation("No rejected rows found for OrgId={OrgId}", orgId);
            }
        }

        private static List<string> ValidatePolicyRow(Ins_Policy row, int rowNo)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(row.PolicyNo))
                errors.Add("PolicyNo is mandatory");

            if (string.IsNullOrWhiteSpace(row.PolicySuffix))
                errors.Add("Policy Suffix is mandatory");

            if (row.RiskStartDt == null)
                errors.Add("RiskStartDt is mandatory");

            if (row.RiskEndDt == null)
                errors.Add("RiskEndDt is mandatory");

            if (row.RiskStartDt != null && row.RiskEndDt != null &&
                row.RiskEndDt < row.RiskStartDt)
                errors.Add("RiskEndDt cannot be before RiskStartDt");

            if (row.ModalBasePremium != null && row.ModalBasePremium < 0)
                errors.Add("ModalBasePremium cannot be negative");

            if (!string.IsNullOrEmpty(row.InsuredPan) &&
                !Regex.IsMatch(row.InsuredPan, "^[A-Z]{5}[0-9]{4}[A-Z]{1}$"))
                errors.Add("Invalid Insured PAN format");

            return errors;
        }

    }
}
