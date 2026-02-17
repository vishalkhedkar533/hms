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
    public class BulkDesignationUpdate
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        private int orgId = 0;
        public JobKey jobKey;
        private readonly ILogger<BulkDesignationUpdate> _logger;
        private readonly IMappingProvider _mappingProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IConnectionScope _connectionScope;
        private readonly IBinaryImportFactory _bulkOpsFactory;

        public BulkDesignationUpdate(IJobExecutionContext jobExecutionContext,
            ILogger<BulkDesignationUpdate> logger,
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

        public async Task ProcessDesignationUpdate(JobExeHist jobExeHist)
        {
            _logger.LogInformation("BulkDesignationUpdate job started for OrgId={OrgId}", orgId);

            var operationMapping = _mappingProvider.GetScriptForOperation("Job", "Fetch")
                ?? throw new InvalidOperationException("Operation mapping for Job/Fetch not found.");

            var connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

            var conn = await _connectionScope.GetOpenConnectionAsync(connectionString);
            var token = CancellationToken.None;

            var tasksSql = _mappingProvider.GetScriptForOperation("DesignationUpdate", "GetPendingTasks")?.Script
                ?? throw new Exception("SQL for DesignationUpdate/GetPendingTasks missing");
            var tasks = (await conn.QueryAsync<FileProcessingTask>(tasksSql)).ToList();

            if (!tasks.Any())
            {
                _logger.LogInformation("No pending designation update tasks found");
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

                var rows = MiniExcel.Query<DesignationUpdateRow>(task.FilePath).ToList();
                var response = new DesignationUpdateResponse { TotalRows = rows.Count };

                if (rows.Count == 0)
                {
                    _logger.LogInformation("No rows found in file: {FilePath}", task.FilePath);
                    continue;
                }

                var username = string.IsNullOrWhiteSpace(task.CreatedBy) ? "System" : task.CreatedBy;
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var tempRows = new List<(string AgentCode, string Designation, DateOnly BusinessEffectiveDate, int OrgId, string Comments, string Reason)>();
                var todayRows = new List<(int RowNumber, string AgentCode, string Designation, DateOnly BusinessEffectiveDate)>();

                foreach (var item in rows.Select((row, index) => new { Row = row, RowNumber = index + 2 }))
                {
                    var agentCode = item.Row.AgentCode?.Trim();
                    var designation = item.Row.Designation?.Trim();
                    var effectiveDate = ParseBusinessEffectiveDate(item.Row.BusinessEffectiveDateOfChange);

                    if (string.IsNullOrWhiteSpace(agentCode))
                    {
                        AddError(response, item.RowNumber, agentCode, designation, "Agent Code is required.");
                        tempRows.Add((agentCode ?? string.Empty, designation ?? string.Empty, DateOnly.MinValue, task.OrgId ?? orgId, "Rejected", "Agent Code is required."));
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(designation))
                    {
                        AddError(response, item.RowNumber, agentCode, designation, "Designation is required.");
                        tempRows.Add((agentCode, designation ?? string.Empty, DateOnly.MinValue, task.OrgId ?? orgId, "Rejected", "Designation is required."));
                        continue;
                    }

                    if (effectiveDate is null)
                    {
                        AddError(response, item.RowNumber, agentCode, designation, "Business Effective Date is required.");
                        tempRows.Add((agentCode, designation, DateOnly.MinValue, task.OrgId ?? orgId, "Rejected", "Business Effective Date is required."));
                        continue;
                    }

                    var effectiveDateOnly = DateOnly.FromDateTime(effectiveDate.Value);
                    tempRows.Add((agentCode, designation, effectiveDateOnly, task.OrgId ?? orgId, "Proceed", string.Empty));

                    if (effectiveDateOnly == today)
                    {
                        todayRows.Add((item.RowNumber, agentCode, designation, effectiveDateOnly));
                    }
                }

                if (tempRows.Any())
                {
                    var bulkSql = _mappingProvider.GetScriptForOperation("DesignationUpdate", "BulkCopyTempDesignationUpdate")?.Script
                        ?? throw new Exception("SQL for DesignationUpdate/BulkCopyTempDesignationUpdate missing");

                    await using var writer = await _bulkOpsFactory.BeginBinaryImportAsync(conn, bulkSql, token);
                    foreach (var row in tempRows)
                    {
                        writer.StartRow();
                        writer.Write(row.AgentCode);
                        writer.Write(row.Designation);
                        writer.Write(row.BusinessEffectiveDate == DateOnly.MinValue ? (DateOnly?)null : row.BusinessEffectiveDate);
                        writer.Write("Pending");
                        writer.Write(row.Comments);
                        writer.Write(row.Reason);
                        writer.Write(row.OrgId);
                    }

                    await writer.CompleteAsync(token);
                }

                var rejectedTempRows = new List<object>();
                string? successDataEncoded = null;
                byte[]? errorFile = null;

                if (todayRows.Count > 0)
                {
                    var allCodes = todayRows.Select(r => r.AgentCode)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();
                    var allDesignations = todayRows.Select(r => r.Designation)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    var agentsSql = _mappingProvider.GetScriptForOperation("DesignationUpdate", "GetAgentsByCode")?.Script
                        ?? throw new Exception("SQL for DesignationUpdate/GetAgentsByCode missing");
                    var designationsSql = _mappingProvider.GetScriptForOperation("DesignationUpdate", "GetDesignationsByName")?.Script
                        ?? throw new Exception("SQL for DesignationUpdate/GetDesignationsByName missing");

                    var agents = (await conn.QueryAsync<Agent>(agentsSql, new { orgId = task.OrgId ?? orgId, codes = allCodes })).ToList();
                    var agentByCode = agents
                        .Where(a => !string.IsNullOrWhiteSpace(a.AgentCode))
                        .ToDictionary(a => a.AgentCode, StringComparer.OrdinalIgnoreCase);

                    var designations = (await conn.QueryAsync<DesignationMaster>(designationsSql,
                        new { orgId = task.OrgId ?? orgId, names = allDesignations })).ToList();
                    var designationByName = designations
                        .Where(d => !string.IsNullOrWhiteSpace(d.DesignationName))
                        .ToDictionary(d => d.DesignationName, StringComparer.OrdinalIgnoreCase);

                    foreach (var item in todayRows)
                    {
                        var agentCode = item.AgentCode;
                        var designation = item.Designation;

                        if (!agentByCode.ContainsKey(agentCode))
                        {
                            var reason = "Invalid Agent Code.";
                            AddError(response, item.RowNumber, agentCode, designation, reason);
                            rejectedTempRows.Add(new
                            {
                                AgentCode = agentCode,
                                Designation = designation,
                                BusinessEffectiveDate = item.BusinessEffectiveDate.ToDateTime(TimeOnly.MinValue),
                                Comments = "Rejected",
                                Reason = reason,
                                OrgId = task.OrgId ?? orgId
                            });
                            continue;
                        }

                        if (!designationByName.ContainsKey(designation))
                        {
                            var reason = "Invalid Designation.";
                            AddError(response, item.RowNumber, agentCode, designation, reason);
                            rejectedTempRows.Add(new
                            {
                                AgentCode = agentCode,
                                Designation = designation,
                                BusinessEffectiveDate = item.BusinessEffectiveDate.ToDateTime(TimeOnly.MinValue),
                                Comments = "Rejected",
                                Reason = reason,
                                OrgId = task.OrgId ?? orgId
                            });
                        }
                    }
                }

                response.FailedRows = response.Errors.Count;

                if (rejectedTempRows.Count > 0)
                {
                    var reviewSql = _mappingProvider.GetScriptForOperation("DesignationUpdate", "UpdateTempDesignationUpdateReview")?.Script
                        ?? throw new Exception("SQL for DesignationUpdate/UpdateTempDesignationUpdateReview missing");
                    await conn.ExecuteAsync(reviewSql, rejectedTempRows);
                }

                var applySql = _mappingProvider.GetScriptForOperation("DesignationUpdate", "ApplyTempDesignationUpdate")?.Script
                    ?? throw new Exception("SQL for DesignationUpdate/ApplyTempDesignationUpdate missing");
                response.UpdatedRows = await conn.ExecuteScalarAsync<int>(applySql, new { OrgId = task.OrgId ?? orgId, ModifiedBy = username });

                response.FailedRows = response.Errors.Count;

                var updateTaskSql = _mappingProvider.GetScriptForOperation("DesignationUpdate", "UpdateTask")?.Script
                    ?? throw new Exception("SQL for DesignationUpdate/UpdateTask missing");

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

                _logger.LogInformation("BulkDesignationUpdate completed for TaskId={TaskId}. UpdatedRows={UpdatedRows}, FailedRows={FailedRows}",
                    task.Id, response.UpdatedRows, response.FailedRows);
            }

            _logger.LogInformation("BulkDesignationUpdate job finished");
        }

        private static DateTime? ParseBusinessEffectiveDate(string? rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }

            var trimmed = rawValue.Trim();
            var formats = new[]
            {
                "dd-MM-yyyy",
                "d-M-yyyy",
                "dd/MM/yyyy",
                "d/M/yyyy",
                "yyyy-MM-dd",
                "yyyy/MM/dd"
            };

            if (DateTime.TryParseExact(trimmed, formats, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var exact))
            {
                return exact;
            }

            if (DateTime.TryParse(trimmed, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var parsed))
            {
                return parsed;
            }

            return null;
        }

        private static void AddError(DesignationUpdateResponse resp, int row, string? agent, string? designation, string msg)
        {
            resp.Errors.Add(new DesignationUpdateError
            {
                RowNumber = row,
                AgentCode = agent ?? string.Empty,
                Designation = designation ?? string.Empty,
                Message = msg
            });
        }
    }
}
