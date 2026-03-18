using CommonLibrary.mapping;
using Dapper;
using Database;
using MiniExcelLibs;
using Quartz;
using Repository;
using SharedModels.BackEndCalculation;
using System.Data.Common;
using System.Text.Json;
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

                if (!int.TryParse(task.CreatedBy, out var createdByUserId) || createdByUserId <= 0)
                {
                    _logger.LogError("Invalid CreatedBy in fileprocessingtasks. TaskId={TaskId}, CreatedBy={CreatedBy}", task.Id, task.CreatedBy);
                    continue;
                }

                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var tempRows = new List<(string AgentCode, string SupervisorCode, DateOnly EffectiveDateOfChange, int OrgId, string Comments, string Reason)>();
                var todayRows = new List<(int RowNumber, string AgentCode, string SupervisorCode, DateOnly EffectiveDateOfChange)>();

                foreach (var item in rows.Select((row, index) => new { Row = row, RowNumber = index + 2 }))
                {
                    var agentCode = item.Row.AgentCode?.Trim();
                    var supervisorCode = item.Row.SupervisorCode?.Trim();
                    var effectiveDate = item.Row.EffectiveDateOfChange?.Date;

                    if (string.IsNullOrWhiteSpace(agentCode))
                    {
                        AddError(response, item.RowNumber, agentCode, supervisorCode, "Agent Code is required.");
                        tempRows.Add((agentCode ?? string.Empty, supervisorCode ?? string.Empty, DateOnly.MinValue, task.OrgId ?? orgId, "Rejected", "Agent Code is required."));
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(supervisorCode))
                    {
                        AddError(response, item.RowNumber, agentCode, supervisorCode, "Reporting Agent Code is required.");
                        tempRows.Add((agentCode, supervisorCode ?? string.Empty, DateOnly.MinValue, task.OrgId ?? orgId, "Rejected", "Reporting Agent Code is required."));
                        continue;
                    }

                    if (effectiveDate is null)
                    {
                        AddError(response, item.RowNumber, agentCode, supervisorCode, "Effective Date Of Change is required.");
                        tempRows.Add((agentCode, supervisorCode, DateOnly.MinValue, task.OrgId ?? orgId, "Rejected", "Effective Date Of Change is required."));
                        continue;
                    }

                    var effectiveDateOnly = DateOnly.FromDateTime(effectiveDate.Value);
                    tempRows.Add((agentCode, supervisorCode, effectiveDateOnly, task.OrgId ?? orgId, "Proceed", string.Empty));

                    todayRows.Add((item.RowNumber, agentCode, supervisorCode, effectiveDateOnly));

                    // Future (today-only processing):
                    // if (effectiveDateOnly == today)
                    // {
                    //     todayRows.Add((item.RowNumber, agentCode, supervisorCode, effectiveDateOnly));
                    // }
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
                        writer.Write(row.EffectiveDateOfChange == DateOnly.MinValue ? (DateOnly?)null : row.EffectiveDateOfChange);
                        writer.Write("Pending");
                        writer.Write(row.OrgId);
                        writer.Write(row.Comments);
                        writer.Write(row.Reason);
                    }

                    await writer.CompleteAsync(token);
                }

                var InvalidTempRows = new List<object>();
                var CleanTempRows = new List<object>();
                string? successDataEncoded = null;
                byte[]? errorFile = null;

                int? componentId = null;
                var componentSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "GetManagerUpdateComponentId")?.Script;
                if (!string.IsNullOrWhiteSpace(componentSql))
                {
                    componentId = await conn.QuerySingleOrDefaultAsync<int?>(componentSql);
                }

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
                            var reason = "Invalid Agent Code.";
                            AddError(response, item.RowNumber, agentCode, supervisorCode, reason);
                            InvalidTempRows.Add(new
                            {
                                AgentCode = agentCode,
                                SupervisorCode = supervisorCode,
                                EffectiveDateOfChange = item.EffectiveDateOfChange.ToDateTime(TimeOnly.MinValue),
                                Comments = "Rejected",
                                Reason = reason,
                                OrgId = task.OrgId ?? orgId
                            });
                            continue;
                        }
                        //check active status of agent
                        if (!agentByCode.TryGetValue(supervisorCode, out var supervisor))
                        {
                            var reason = "Reporting Agent Code not found.";
                            AddError(response, item.RowNumber, agentCode, supervisorCode, reason);
                            InvalidTempRows.Add(new
                            {
                                AgentCode = agentCode,
                                SupervisorCode = supervisorCode,
                                EffectiveDateOfChange = item.EffectiveDateOfChange.ToDateTime(TimeOnly.MinValue),
                                Comments = "Rejected",
                                Reason = reason,
                                OrgId = task.OrgId ?? orgId
                            });
                            continue;
                        }
                        //check active status of supervisor
                        if (agent.Channel != supervisor.Channel)
                        {
                            var reason = $"Supervisor is in channel '{supervisor.Channel}', but Agent is in '{agent.Channel}'. Channels must match.";
                            AddError(response, item.RowNumber, agentCode, supervisorCode, reason);
                            InvalidTempRows.Add(new
                            {
                                AgentCode = agentCode,
                                SupervisorCode = supervisorCode,
                                EffectiveDateOfChange = item.EffectiveDateOfChange.ToDateTime(TimeOnly.MinValue),
                                Comments = "Rejected",
                                Reason = reason,
                                OrgId = task.OrgId ?? orgId
                            });
                            continue;
                        }

                        CleanTempRows.Add(new
                        {
                            AgentCode = agentCode,
                            SupervisorCode = supervisorCode,
                            EffectiveDateOfChange = item.EffectiveDateOfChange.ToDateTime(TimeOnly.MinValue),
                            OrgId = task.OrgId ?? orgId
                        });
                    }
                }

                response.FailedRows = response.Errors.Count;

                if (InvalidTempRows.Count > 0)
                {
                    var reviewSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "UpdateTempManagerUpdateReview")?.Script
                        ?? throw new Exception("SQL for ManagerUpdate/UpdateTempManagerUpdateReview missing");
                    await conn.ExecuteAsync(reviewSql, InvalidTempRows);
                }
                if (InvalidTempRows.Count > 0)
                {
                    var reviewSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "UpdateTempManagerUpdateReview")?.Script
                        ?? throw new Exception("SQL for ManagerUpdate/UpdateTempManagerUpdateReview missing");

                    await using var tx = await conn.BeginTransactionAsync(token);
                    await conn.ExecuteAsync(reviewSql, InvalidTempRows, tx);
                    await tx.CommitAsync(token);
                }
                if (CleanTempRows.Count > 0)
                {
                    var statusSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "UpdateTempManagerUpdateStatus")?.Script
                        ?? throw new Exception("SQL for ManagerUpdate/UpdateTempManagerUpdateStatus missing");
                    await conn.ExecuteAsync(statusSql, CleanTempRows);
                    var inboxEntries = await InsertInboxEntriesAsync(conn, task, CleanTempRows, createdByUserId);

                    var approvalSettingsql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "GetApprovalSettingForComponent")?.Script
                        ?? throw new Exception("SQL for approvaSetting missing");
                    var approvalsetting = await conn.QuerySingleOrDefaultAsync<InboxFieldConfig>(approvalSettingsql, new { componentId = componentId, orgId = task.OrgId ?? orgId });
                    var useDefaultApprover = approvalsetting?.UseDefaultApprover;

                    if (useDefaultApprover is null)
                    {
                        await ApplyManagerUpdatesWithoutApprovalAsync(conn, inboxEntries, createdByUserId, token);
                    }
                    else if (useDefaultApprover.Value)
                    {
                        await AssignUserHierarchyApproversAsync(conn, inboxEntries, task.OrgId ?? orgId, createdByUserId, token);
                        _logger.LogInformation("Manager update entries are pending approval using user hierarchy. TaskId={TaskId}", task.Id);
                    }
                    else
                    {
                        await AssignCustomHierarchyApproversAsync(conn, inboxEntries, task.OrgId ?? orgId, createdByUserId, approvalsetting, token);
                        _logger.LogInformation("Manager update entries are pending approval using custom hierarchy. TaskId={TaskId}", task.Id);
                    }
                }

                // response.UpdatedRows = CleanTempRows.Count;
                //await conn.ExecuteAsync(updateTaskSql, new
                //{
                //    Id = task.Id,
                //    RowsProcessed = response.UpdatedRows,
                //    TotalRows = response.TotalRows,
                //    RowsRejected = response.FailedRows,
                //    ErrorMessage = errorMessageEncoded,
                //    SuccessData = successDataEncoded,
                //    Status = response.Errors.Any() ? "CompletedWithErrors" : "Completed"
                //});

                _logger.LogInformation("BulkManagerUpdate completed for TaskId={TaskId}. UpdatedRows={UpdatedRows}, FailedRows={FailedRows}",
                    task.Id, response.UpdatedRows, response.FailedRows);
            }

            _logger.LogInformation("BulkManagerUpdate job finished");
        }

        private async Task<List<ManagerUpdateInboxEntry>> InsertInboxEntriesAsync(
            DbConnection conn,
            FileProcessingTask task,
            List<object> approvedRows,
            int createdByUserId)
        {
            var taskOrgId = task.OrgId ?? orgId;
            int? componentId = null;
            var componentSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "GetManagerUpdateComponentId")?.Script;
            if (!string.IsNullOrWhiteSpace(componentSql))
            {
                componentId = await conn.QuerySingleOrDefaultAsync<int?>(componentSql);
            }

            int? allocatedToRole = null;
            if (componentId.HasValue)
            {
                var approvalSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "GetApprovalSettingForComponent")?.Script;
                if (!string.IsNullOrWhiteSpace(approvalSql))
                {
                    var setting = await conn.QuerySingleOrDefaultAsync<InboxFieldConfig>(approvalSql, new { componentId = componentId.Value, orgId = taskOrgId });
                    if (setting != null && setting.UseDefaultApprover == false)
                    {
                        allocatedToRole = setting.ApproverOneId ?? setting.ApproverTwoId ?? setting.ApproverThreeId;
                    }
                }
            }

            var insertInboxSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "InsertInboxEntry")?.Script
                ?? throw new Exception("SQL for ManagerUpdate/InsertInboxEntry missing");

            var inboxEntries = new List<ManagerUpdateInboxEntry>();
            foreach (var row in approvedRows)
            {
                var agentCode = row.GetType().GetProperty("AgentCode")?.GetValue(row)?.ToString() ?? string.Empty;
                var supervisorCode = row.GetType().GetProperty("SupervisorCode")?.GetValue(row)?.ToString() ?? string.Empty;
                var effectiveDate = row.GetType().GetProperty("EffectiveDateOfChange")?.GetValue(row);
                //var effectiveDateValue = effectiveDate as DateTime?;

                //var requestDets = $"Bulk Manager Update: Agent {agentCode} -> Supervisor {supervisorCode}";

                var requestorNote = JsonSerializer.Serialize(new[]
                {
                    new { FieldName = "AgentCode", OldValue = string.Empty, NewValue = agentCode },
                    new { FieldName = "SupervisorCode", OldValue = string.Empty, NewValue = supervisorCode },
                    new { FieldName = "EffectiveDateOfChange", OldValue = string.Empty, NewValue = effectiveDate?.ToString() ?? string.Empty }
                });

                var srNo = await conn.ExecuteScalarAsync<int>(insertInboxSql, new
                {
                    OrgId = taskOrgId,
                    RequestDets = "Bulk Manager Updated",
                    RequestorNote = requestorNote,
                    CreatedBy = createdByUserId,
                    CreatedDate = DateTime.UtcNow,
                    SrStatus = 1,
                    ComponentId = componentId,
                    AllocatedToRole = allocatedToRole,
                    ApprovalEndpoint = (string?)null,
                    ApprovalPayload = (string?)null,
                    ObjectName = "BulkManagerUpdate",
                    ObjectReference = task.Id
                });

                inboxEntries.Add(new ManagerUpdateInboxEntry(srNo, taskOrgId, agentCode, supervisorCode));
                _logger.LogInformation("Inbox entry created SrNo={SrNo} for Agent={AgentCode} -> Supervisor={SupervisorCode}",
                    srNo, agentCode, supervisorCode);
            }

            return inboxEntries;
        }

        private async Task ApplyManagerUpdatesWithoutApprovalAsync(
            DbConnection conn,
            List<ManagerUpdateInboxEntry> inboxEntries,
            int decisionBy,
            CancellationToken token)
        {
            if (inboxEntries.Count == 0)
            {
                return;
            }

            var allCodes = inboxEntries
                .SelectMany(x => new[] { x.AgentCode, x.SupervisorCode })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var agentsSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "GetAgentsByCode")?.Script
                ?? throw new Exception("SQL for ManagerUpdate/GetAgentsByCode missing");
            var updateSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "UpdateSupervisor")?.Script
                ?? throw new Exception("SQL for ManagerUpdate/UpdateSupervisor missing");
            var auditSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "InsertAuditTrail")?.Script
                ?? throw new Exception("SQL for ManagerUpdate/InsertAuditTrail missing");
            var updateInboxSql = _mappingProvider.GetScriptForOperation("Inbox", "UpdateInboxStatus")?.Script
                ?? throw new Exception("SQL for Inbox/UpdateInboxStatus missing");
            var updateTempApprovedSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "UpdateTempManagerUpdateApprovedStatus")?.Script
                ?? throw new Exception("SQL for ManagerUpdate/UpdateTempManagerUpdateApprovedStatus missing");

            var orgIdToCodes = inboxEntries
                .GroupBy(x => x.OrgId)
                .ToDictionary(g => g.Key, g => g.SelectMany(x => new[] { x.AgentCode, x.SupervisorCode })
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray());

            var agentLookup = new Dictionary<(int OrgId, string AgentCode), Agent>();
            foreach (var entry in orgIdToCodes)
            {
                var agents = await conn.QueryAsync<Agent>(agentsSql, new { orgId = entry.Key, codes = entry.Value });
                foreach (var agent in agents.Where(a => !string.IsNullOrWhiteSpace(a.AgentCode)))
                {
                    agentLookup[(entry.Key, agent.AgentCode)] = agent;
                }
            }

            foreach (var serviceRequest in inboxEntries)
            {
                if (!agentLookup.TryGetValue((serviceRequest.OrgId, serviceRequest.AgentCode), out var agent))
                {
                    _logger.LogWarning("Auto-apply skipped for SrNo={SrNo}; agent code {AgentCode} not found.", serviceRequest.SrNo, serviceRequest.AgentCode);
                    continue;
                }

                if (!agentLookup.TryGetValue((serviceRequest.OrgId, serviceRequest.SupervisorCode), out var supervisor))
                {
                    _logger.LogWarning("Auto-apply skipped for SrNo={SrNo}; supervisor code {SupervisorCode} not found.", serviceRequest.SrNo, serviceRequest.SupervisorCode);
                    continue;
                }

                await using var tx = await conn.BeginTransactionAsync(token);
                try
                {
                    await conn.ExecuteAsync(updateSql, new
                    {
                        AgentId = agent.AgentId,
                        SupervisorId = supervisor.AgentId,
                        ModifiedBy = decisionBy.ToString()
                    }, tx);

                    await conn.ExecuteAsync(auditSql, new
                    {
                        AgentId = agent.AgentId,
                        FieldName = "SupervisorId",
                        OldValue = agent.SupervisorId?.ToString() ?? string.Empty,
                        NewValue = supervisor.AgentId.ToString(),
                        ChangedBy = decisionBy.ToString(),
                        ChangedDate = DateTime.UtcNow,
                        CreatedBy = decisionBy.ToString(),
                        CreatedDate = DateTime.UtcNow,
                        ModifiedBy = decisionBy.ToString(),
                        ModifiedDate = DateTime.UtcNow
                    }, tx);

                    await conn.ExecuteAsync(updateInboxSql, new
                    {
                        orgId = serviceRequest.OrgId,
                        srNo = serviceRequest.SrNo,
                        srStatus = 3
                    }, tx);

                    await conn.ExecuteAsync(updateTempApprovedSql, new
                    {
                        OrgId = serviceRequest.OrgId,
                        AgentCode = serviceRequest.AgentCode,
                        SupervisorCode = serviceRequest.SupervisorCode
                    }, tx);

                    await tx.CommitAsync(token);
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync(token);
                    _logger.LogError(ex, "Failed to auto-apply manager update for SrNo={SrNo}", serviceRequest.SrNo);
                }
            }
        }

        private async Task AssignUserHierarchyApproversAsync(
            DbConnection conn,
            List<ManagerUpdateInboxEntry> inboxEntries,
            int orgId,
            int createdByUserId,
            CancellationToken token)
        {
            if (inboxEntries.Count == 0)
            {
                return;
            }

            var insertSql = _mappingProvider.GetScriptForOperation("Inbox", "InsertSrApprover")?.Script
                ?? throw new Exception("SQL for Inbox/InsertSrApprover missing");
            var updateInboxSql = _mappingProvider.GetScriptForOperation("Inbox", "UpdateInboxStatus")?.Script
                ?? throw new Exception("SQL for Inbox/UpdateInboxStatus missing");
            var reportingMgrSql = _mappingProvider.GetScriptForOperation("Inbox", "GetManagerHierarchyRoles")?.Script
                ?? throw new Exception("SQL for Inbox/GetManagerHierarchyRoles missing");

            var reportingMgrId = await conn.QuerySingleOrDefaultAsync<int?>(reportingMgrSql, new { orgId, userId = createdByUserId });
            if (!reportingMgrId.HasValue || reportingMgrId.Value <= 0)
            {
                _logger.LogWarning("No reporting manager resolved for OrgId={OrgId}, CreatedBy={CreatedBy}", orgId, createdByUserId);
                return;
            }

            foreach (var entry in inboxEntries)
            {
                await using var tx = await conn.BeginTransactionAsync(token);
                try
                {
                    await conn.ExecuteAsync(insertSql, new
                    {
                        orgId = entry.OrgId,
                        srNo = entry.SrNo,
                        approverLevel = 1,
                        allocatedRoleId = (int?)null,
                        reportingMgr = reportingMgrId.Value,
                        decisionBy = createdByUserId,
                        decisionOn = DateTime.UtcNow,
                        approverDecision = 1
                    }, tx);

                    await conn.ExecuteAsync(updateInboxSql, new
                    {
                        orgId = entry.OrgId,
                        srNo = entry.SrNo,
                        srStatus = 2
                    }, tx);

                    await tx.CommitAsync(token);
                }
                catch
                {
                    await tx.RollbackAsync(token);
                    throw;
                }
            }
        }

        private async Task AssignCustomHierarchyApproversAsync(
            DbConnection conn,
            List<ManagerUpdateInboxEntry> inboxEntries,
            int orgId,
            int createdByUserId,
            InboxFieldConfig? approvalSetting,
            CancellationToken token)
        {
            if (inboxEntries.Count == 0)
            {
                return;
            }

            var approverRoles = new List<int>();
            if (approvalSetting?.ApproverOneId is > 0)
            {
                approverRoles.Add(approvalSetting.ApproverOneId.Value);
            }
            if (approvalSetting?.ApproverTwoId is > 0)
            {
                approverRoles.Add(approvalSetting.ApproverTwoId.Value);
            }
            if (approvalSetting?.ApproverThreeId is > 0)
            {
                approverRoles.Add(approvalSetting.ApproverThreeId.Value);
            }

            if (approverRoles.Count == 0)
            {
                _logger.LogWarning("No custom approver roles resolved for OrgId={OrgId}.", orgId);
                return;
            }

            var insertSql = _mappingProvider.GetScriptForOperation("Inbox", "InsertSrApprover")?.Script
                ?? throw new Exception("SQL for Inbox/InsertSrApprover missing");
            var updateInboxSql = _mappingProvider.GetScriptForOperation("Inbox", "UpdateInboxStatus")?.Script
                ?? throw new Exception("SQL for Inbox/UpdateInboxStatus missing");

            foreach (var entry in inboxEntries)
            {
                await using var tx = await conn.BeginTransactionAsync(token);
                try
                {
                    var level = 1;
                    foreach (var roleId in approverRoles)
                    {
                        await conn.ExecuteAsync(insertSql, new
                        {
                            orgId = entry.OrgId,
                            srNo = entry.SrNo,
                            approverLevel = level,
                            allocatedRoleId = roleId,
                            reportingMgr = (int?)null,
                            decisionBy = createdByUserId,
                            decisionOn = DateTime.UtcNow,
                            approverDecision = 1
                        }, tx);
                        level++;
                    }

                    await conn.ExecuteAsync(updateInboxSql, new
                    {
                        orgId = entry.OrgId,
                        srNo = entry.SrNo,
                        srStatus = 2
                    }, tx);

                    await tx.CommitAsync(token);
                }
                catch
                {
                    await tx.RollbackAsync(token);
                    throw;
                }
            }
        }

        private sealed record ManagerUpdateInboxEntry(int SrNo, int OrgId, string AgentCode, string SupervisorCode);

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
