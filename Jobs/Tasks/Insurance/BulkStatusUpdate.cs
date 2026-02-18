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
    public class BulkStatusUpdate
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        private int orgId = 0;
        public JobKey jobKey;
        private readonly ILogger<BulkStatusUpdate> _logger;
        private readonly IMappingProvider _mappingProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IConnectionScope _connectionScope;
        private readonly IBinaryImportFactory _bulkOpsFactory;

        public BulkStatusUpdate(IJobExecutionContext jobExecutionContext,
            ILogger<BulkStatusUpdate> logger,
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

        public async Task ProcessStatusUpdate(JobExeHist jobExeHist)
        {
            _logger.LogInformation("BulkStatusUpdate job started for OrgId={OrgId}", orgId);

            var operationMapping = _mappingProvider.GetScriptForOperation("Job", "Fetch")
                ?? throw new InvalidOperationException("Operation mapping for Job/Fetch not found.");

            var connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

            var conn = await _connectionScope.GetOpenConnectionAsync(connectionString);
            var token = CancellationToken.None;

            var tasksSql = _mappingProvider.GetScriptForOperation("StatusUpdate", "GetPendingTasks")?.Script
                ?? throw new Exception("SQL for StatusUpdate/GetPendingTasks missing");
            var tasks = (await conn.QueryAsync<FileProcessingTask>(tasksSql)).ToList();

            if (!tasks.Any())
            {
                _logger.LogInformation("No pending status update tasks found");
                return;
            }

            foreach (var task in tasks)
            {
                _logger.LogInformation("Processing File for OrgId={OrgId}", orgId);

                if (!File.Exists(task.FilePath))
                {
                    _logger.LogError("File not found: {FilePath}. Skipping TaskId={TaskId}", task.FilePath, task.Id);
                    continue;
                }

                var rows = MiniExcel.Query<StatusUpdateRow>(task.FilePath).ToList();
                var response = new StatusUpdateResponse { TotalRows = rows.Count };

                if (rows.Count == 0)
                {
                    _logger.LogInformation("No rows found in file: {FilePath}", task.FilePath);
                    continue;
                }

                var username = string.IsNullOrWhiteSpace(task.CreatedBy) ? "System" : task.CreatedBy;
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var tempRows = new List<(string AgentCode, string Status, DateOnly BusinessEffectiveDate, int OrgId, string Comments, string Reason)>();
                var todayRows = new List<(int RowNumber, string AgentCode, string Status, DateOnly BusinessEffectiveDate)>();

                foreach (var item in rows.Select((row, index) => new { Row = row, RowNumber = index + 2 }))
                {
                    var agentCode = item.Row.AgentCode?.Trim();
                    var status = item.Row.Status?.Trim();
                    var effectiveDate = item.Row.BusinessEffectiveDateOfChange?.Date;

                    if (string.IsNullOrWhiteSpace(agentCode))
                    {
                        AddError(response, item.RowNumber, agentCode, status, "Agent Code is required.");
                        tempRows.Add((agentCode ?? string.Empty, status ?? string.Empty, DateOnly.MinValue, task.OrgId ?? orgId, "Rejected", "Agent Code is required."));
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(status))
                    {
                        AddError(response, item.RowNumber, agentCode, status, "Status is required.");
                        tempRows.Add((agentCode, status ?? string.Empty, DateOnly.MinValue, task.OrgId ?? orgId, "Rejected", "Status is required."));
                        continue;
                    }

                    if (effectiveDate is null)
                    {
                        AddError(response, item.RowNumber, agentCode, status, "Business Effective Date is required.");
                        tempRows.Add((agentCode, status, DateOnly.MinValue, task.OrgId ?? orgId, "Rejected", "Business Effective Date is required."));
                        continue;
                    }

                    var effectiveDateOnly = DateOnly.FromDateTime(effectiveDate.Value);
                    tempRows.Add((agentCode, status, effectiveDateOnly, task.OrgId ?? orgId, "Proceed", string.Empty));

                    if (effectiveDateOnly == today)
                    {
                        todayRows.Add((item.RowNumber, agentCode, status, effectiveDateOnly));
                    }
                }

                if (tempRows.Any())
                {
                    var bulkSql = _mappingProvider.GetScriptForOperation("StatusUpdate", "BulkCopyTempStatusUpdate")?.Script
                        ?? throw new Exception("SQL for StatusUpdate/BulkCopyTempStatusUpdate missing");

                    await using var writer = await _bulkOpsFactory.BeginBinaryImportAsync(conn, bulkSql, token);
                    foreach (var row in tempRows)
                    {
                        writer.StartRow();
                        writer.Write(row.AgentCode);
                        writer.Write(row.Status);
                        writer.Write(row.BusinessEffectiveDate == DateOnly.MinValue ? (DateOnly?)null : row.BusinessEffectiveDate);
                        writer.Write("Pending");
                        writer.Write(row.Comments);
                        writer.Write(row.Reason);
                        writer.Write(row.OrgId);
                    }

                    await writer.CompleteAsync(token);
                }

                var rejectedTempRows = new List<object>();
                var approvedTempRows = new List<object>();
                string? successDataEncoded = null;
                byte[]? errorFile = null;

                if (todayRows.Count > 0)
                {
                    var allCodes = todayRows.Select(r => r.AgentCode)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    var agentsSql = _mappingProvider.GetScriptForOperation("StatusUpdate", "GetAgentsByCode")?.Script
                        ?? throw new Exception("SQL for StatusUpdate/GetAgentsByCode missing");

                    var agents = (await conn.QueryAsync<Agent>(agentsSql, new { orgId = task.OrgId ?? orgId, codes = allCodes })).ToList();
                    var agentByCode = agents
                        .Where(a => !string.IsNullOrWhiteSpace(a.AgentCode))
                        .ToDictionary(a => a.AgentCode, StringComparer.OrdinalIgnoreCase);

                    foreach (var item in todayRows)
                    {
                        var agentCode = item.AgentCode;
                        var status = item.Status;

                        if (!agentByCode.ContainsKey(agentCode))
                        {
                            var reason = "Invalid Agent Code.";
                            AddError(response, item.RowNumber, agentCode, status, reason);
                            rejectedTempRows.Add(new
                            {
                                AgentCode = agentCode,
                                Status = status,
                                BusinessEffectiveDate = item.BusinessEffectiveDate.ToDateTime(TimeOnly.MinValue),
                                Comments = "Rejected",
                                Reason = reason,
                                OrgId = task.OrgId ?? orgId
                            });
                            continue;
                        }

                        approvedTempRows.Add(new
                        {
                            AgentCode = agentCode,
                            Status = status,
                            BusinessEffectiveDate = item.BusinessEffectiveDate.ToDateTime(TimeOnly.MinValue),
                            OrgId = task.OrgId ?? orgId
                        });
                    }
                }

                response.FailedRows = response.Errors.Count;

                if (rejectedTempRows.Count > 0)
                {
                    var reviewSql = _mappingProvider.GetScriptForOperation("StatusUpdate", "UpdateTempStatusUpdateReview")?.Script
                        ?? throw new Exception("SQL for StatusUpdate/UpdateTempStatusUpdateReview missing");
                    await conn.ExecuteAsync(reviewSql, rejectedTempRows);
                }

                if (approvedTempRows.Count > 0)
                {
                    var statusSql = _mappingProvider.GetScriptForOperation("StatusUpdate", "UpdateTempStatusUpdateStatus")?.Script
                        ?? throw new Exception("SQL for StatusUpdate/UpdateTempStatusUpdateStatus missing");
                    await conn.ExecuteAsync(statusSql, approvedTempRows);
                }

                // var applySql = _mappingProvider.GetScriptForOperation("StatusUpdate", "ApplyTempStatusUpdate")?.Script
                //     ?? throw new Exception("SQL for StatusUpdate/ApplyTempStatusUpdate missing");
                // response.UpdatedRows = await conn.ExecuteScalarAsync<int>(applySql, new { OrgId = task.OrgId ?? orgId, ModifiedBy = username });

                response.UpdatedRows = 0;

                response.FailedRows = response.Errors.Count;

                var updateTaskSql = _mappingProvider.GetScriptForOperation("StatusUpdate", "UpdateTask")?.Script
                    ?? throw new Exception("SQL for StatusUpdate/UpdateTask missing");

                var errorMessageEncoded = errorFile != null
                    ? Convert.ToBase64String(errorFile)
                    : null;

                await conn.ExecuteAsync(updateTaskSql, new
                {
                    Id = task.Id,
                    RowsProcessed = response.UpdatedRows,
                    TotalRows = response.TotalRows,
                    RowsRejected = response.FailedRows,
                    ErrorMessage = errorMessageEncoded,
                    SuccessData = successDataEncoded,
                    Status = response.Errors.Any() ? "CompletedWithErrors" : "Completed"
                });

                _logger.LogInformation("BulkStatusUpdate completed for TaskId={TaskId}. UpdatedRows={UpdatedRows}, FailedRows={FailedRows}",
                    task.Id, response.UpdatedRows, response.FailedRows);
            }

            _logger.LogInformation("BulkStatusUpdate job finished");
        }

        private static void AddError(StatusUpdateResponse resp, int row, string? agent, string? status, string msg)
        {
            resp.Errors.Add(new StatusUpdateError
            {
                RowNumber = row,
                AgentCode = agent ?? string.Empty,
                Status = status ?? string.Empty,
                Message = msg
            });
        }
    }
}
