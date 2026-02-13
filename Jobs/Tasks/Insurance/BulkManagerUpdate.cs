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
    public class BulkManagerUpdate
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        private int orgId = 0;      
        public JobKey jobKey;
        private readonly ILogger<BulkManagerUpdate> _logger;
        private readonly IMappingProvider _mappingProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IConnectionScope _connectionScope;
        private readonly IBinaryImportFactory _bulkOpsFactory;

        public BulkManagerUpdate(IJobExecutionContext jobExecutionContext,
        ILogger<BulkManagerUpdate> logger,
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

        public async Task ProcessManagerUpdate(JobExeHist jobExeHist)
        {
            _logger.LogInformation("BulkManagerUpdate job started for OrgId={OrgId}", orgId);

            var operationMapping = _mappingProvider.GetScriptForOperation("Job", "Fetch")
                ?? throw new InvalidOperationException("Operation mapping for Job/Fetch not found.");

            var connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

            var conn = await _connectionScope.GetOpenConnectionAsync(connectionString);
            var token = CancellationToken.None;

            var tasksSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "GetPendingTasks")?.Script
                ?? throw new Exception("SQL for ManagerUpdate/GetPendingTasks missing");
            var tasks = (await conn.QueryAsync<FileProcessingTask>(tasksSql)).ToList();

            if (!tasks.Any())
            {
                _logger.LogInformation("No pending manager update tasks found");
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

                var rows = MiniExcel.Query<ManagerUpdateRow>(task.FilePath).ToList();
                var response = new ManagerUpdateResponse { TotalRows = rows.Count };

                if (rows.Count == 0)
                {
                    _logger.LogInformation("No rows found in file: {FilePath}", task.FilePath);
                    continue;
                }

                var username = string.IsNullOrWhiteSpace(task.CreatedBy) ? "System" : task.CreatedBy;
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var tempRows = new List<(string AgentCode, string SupervisorCode, DateOnly EffectiveDateOfChange, int OrgId)>();
                var todayRows = new List<(int RowNumber, string AgentCode, string SupervisorCode, DateOnly EffectiveDateOfChange)>();

                foreach (var item in rows.Select((row, index) => new { Row = row, RowNumber = index + 2 }))
                {
                    var agentCode = item.Row.AgentCode?.Trim();
                    var supervisorCode = item.Row.SupervisorCode?.Trim();
                    var effectiveDate = item.Row.EffectiveDateOfChange?.Date;

                    if (string.IsNullOrWhiteSpace(agentCode))
                    {
                        AddError(response, item.RowNumber, agentCode, supervisorCode, "Agent Code is required.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(supervisorCode))
                    {
                        AddError(response, item.RowNumber, agentCode, supervisorCode, "Reporting Agent Code is required.");
                        continue;
                    }

                    if (effectiveDate is null)
                    {
                        AddError(response, item.RowNumber, agentCode, supervisorCode, "Effective Date Of Change is required.");
                        continue;
                    }

                    var effectiveDateOnly = DateOnly.FromDateTime(effectiveDate.Value);
                    tempRows.Add((agentCode, supervisorCode, effectiveDateOnly, task.OrgId ?? orgId));

                    if (effectiveDateOnly == today)
                    {
                        todayRows.Add((item.RowNumber, agentCode, supervisorCode, effectiveDateOnly));
                    }
                }

                if (tempRows.Any())
                {
                    var bulkSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "BulkCopyTempManagerUpdate")?.Script
                        ?? throw new Exception("SQL for ManagerUpdate/BulkCopyTempManagerUpdate missing");

                    await using var writer = await _bulkOpsFactory.BeginBinaryImportAsync(conn, bulkSql, token);
                    foreach (var row in tempRows)
                    {
                        writer.StartRow();
                        writer.Write(row.AgentCode);
                        writer.Write(row.SupervisorCode);
                        writer.Write(row.EffectiveDateOfChange);
                        writer.Write("Pending");
                        writer.Write(row.OrgId);
                    }

                    await writer.CompleteAsync(token);
                }

                var updatedAgents = new List<object>();
                var auditEntries = new List<object>();
                var successRowsForExport = new List<object>();
                var updatedTempRows = new List<object>();

                if (todayRows.Count > 0)
                {
                    var allCodes = todayRows.SelectMany(r => new[] { r.AgentCode, r.SupervisorCode })
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    var agentsSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "GetAgentsByCode")?.Script
                        ?? throw new Exception("SQL for ManagerUpdate/GetAgentsByCode missing");

                    var agents = (await conn.QueryAsync<Agent>(agentsSql, new { orgId = task.OrgId ?? orgId, codes = allCodes })).ToList();
                    var agentByCode = agents
                        .Where(a => !string.IsNullOrWhiteSpace(a.AgentCode))
                        .ToDictionary(a => a.AgentCode, StringComparer.OrdinalIgnoreCase);

                    foreach (var item in todayRows)
                    {
                        var agentCode = item.AgentCode;
                        var supervisorCode = item.SupervisorCode;

                        if (!agentByCode.TryGetValue(agentCode, out var agent))
                        {
                            AddError(response, item.RowNumber, agentCode, supervisorCode, "Invalid Agent Code.");
                            continue;
                        }

                        if (!agentByCode.TryGetValue(supervisorCode, out var supervisor))
                        {
                            AddError(response, item.RowNumber, agentCode, supervisorCode, "Reporting Agent Code not found.");
                            continue;
                        }

                        if (agent.Channel != supervisor.Channel)
                        {
                            AddError(response, item.RowNumber, agentCode, supervisorCode,
                                $"Supervisor is in channel '{supervisor.Channel}', but Agent is in '{agent.Channel}'. Channels must match.");
                            continue;
                        }

                        if (agent.SupervisorId != supervisor.AgentId)
                        {
                            updatedAgents.Add(new
                            {
                                AgentId = agent.AgentId,
                                SupervisorId = supervisor.AgentId,
                                ModifiedBy = username
                            });

                            auditEntries.Add(new
                            {
                                AgentId = agent.AgentId,
                                FieldName = "SupervisorId",
                                OldValue = agent.SupervisorId?.ToString() ?? "None",
                                NewValue = supervisor.AgentId.ToString(),
                                ChangedBy = username,
                                ChangedDate = DateTime.UtcNow,
                                CreatedBy = username,
                                CreatedDate = DateTime.UtcNow,
                                ModifiedBy = username,
                                ModifiedDate = DateTime.UtcNow
                            });

                            updatedTempRows.Add(new
                            {
                                AgentCode = agentCode,
                                SupervisorCode = supervisorCode,
                                EffectiveDateOfChange = item.EffectiveDateOfChange,
                                Status = "Updated",
                                OrgId = task.OrgId ?? orgId
                            });

                            successRowsForExport.Add(new
                            {
                                Row_Number = item.RowNumber,
                                Agent_Code = agentCode,
                                New_Supervisor_Code = supervisorCode,
                                Previous_Supervisor_Id = agent.SupervisorId,
                                Status = "Updated"
                            });

                            response.UpdatedRows++;
                        }
                    }
                }

                response.FailedRows = response.Errors.Count;

                // Execute Database Updates
                if (updatedAgents.Count > 0)
                {
                    var updateSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "UpdateSupervisor")?.Script
                        ?? throw new Exception("SQL for ManagerUpdate/UpdateSupervisor missing");
                    var auditSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "InsertAuditTrail")?.Script
                        ?? throw new Exception("SQL for ManagerUpdate/InsertAuditTrail missing");
                    var updateTempSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "UpdateTempManagerUpdateStatus")?.Script
                        ?? throw new Exception("SQL for ManagerUpdate/UpdateTempManagerUpdateStatus missing");

                    await using var tx = await conn.BeginTransactionAsync(token);
                    await conn.ExecuteAsync(updateSql, updatedAgents, tx);
                    await conn.ExecuteAsync(auditSql, auditEntries, tx);
                    await conn.ExecuteAsync(updateTempSql, updatedTempRows, tx);
                    await tx.CommitAsync(token);
                }

                // --- HANDLE SUCCESS DATA EXPORT ---
                string? successDataEncoded = null;
                if (successRowsForExport.Any())
                {
                    using var successStream = new MemoryStream();
                    await MiniExcel.SaveAsAsync(successStream, successRowsForExport);
                    successDataEncoded = Convert.ToBase64String(successStream.ToArray());
                }

                // --- HANDLE ERROR DATA EXPORT ---
                if (response.Errors.Any())
                {
                    var errorExport = response.Errors.Select(e => new
                    {
                        Row_Number = e.RowNumber,
                        Agent_Code = e.AgentCode,
                        Supervisor_Code = e.SupervisorCode,
                        Error_Message = e.Message
                    });

                    var rootFolder = Path.Combine(AppContext.BaseDirectory, "RejectedFiles");
                    Directory.CreateDirectory(rootFolder);
                    var fileName = $"Rejected_Manager_Update_{orgId}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
                    var filePath = Path.Combine(rootFolder, fileName);

                    await using var memoryStream = new MemoryStream();
                    await MiniExcel.SaveAsAsync(memoryStream, errorExport);
                    response.ErrorFile = memoryStream.ToArray();

                    await File.WriteAllBytesAsync(filePath, response.ErrorFile, token);
                    _logger.LogInformation("Rejected rows exported to Excel: {FilePath}", filePath);
                }

                // Final Task Table Update
                var updateTaskSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "UpdateTask")?.Script
                    ?? throw new Exception("SQL for ManagerUpdate/UpdateTask missing");

                var errorMessageEncoded = response.ErrorFile != null
                    ? Convert.ToBase64String(response.ErrorFile)
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

                _logger.LogInformation("BulkManagerUpdate completed for TaskId={TaskId}. UpdatedRows={UpdatedRows}, FailedRows={FailedRows}",
                    task.Id, response.UpdatedRows, response.FailedRows);
            }

            _logger.LogInformation("BulkManagerUpdate job finished");
        }

        private static void AddError(ManagerUpdateResponse resp, int row, string? agent, string? supervisor, string msg)
        {
            resp.Errors.Add(new ManagerUpdateError
            {
                RowNumber = row,
                AgentCode = agent ?? string.Empty,
                SupervisorCode = supervisor ?? string.Empty,
                Message = msg
            });
        }
    }
}
