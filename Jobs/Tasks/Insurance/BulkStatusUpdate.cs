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

                if (!int.TryParse(task.CreatedBy, out var createdByUserId) || createdByUserId <= 0)
                {
                    _logger.LogError("Invalid CreatedBy in fileprocessingtasks. TaskId={TaskId}, CreatedBy={CreatedBy}", task.Id, task.CreatedBy);
                    continue;
                }

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
                    
                    todayRows.Add((item.RowNumber, agentCode, status, effectiveDateOnly));

                    //if (effectiveDateOnly == today)
                    //{
                    //    todayRows.Add((item.RowNumber, agentCode, status, effectiveDateOnly));
                    //}
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

                var InvalidTempRows = new List<object>();
                var cleanTempRows = new List<object>();
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
                            InvalidTempRows.Add(new
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

                        cleanTempRows.Add(new
                        {
                            AgentCode = agentCode,
                            Status = status,
                            BusinessEffectiveDate = item.BusinessEffectiveDate.ToDateTime(TimeOnly.MinValue),
                            OrgId = task.OrgId ?? orgId
                        });
                    }
                }

                response.FailedRows = response.Errors.Count;

                if (InvalidTempRows.Count > 0)
                {
                    var reviewSql = _mappingProvider.GetScriptForOperation("StatusUpdate", "UpdateTempStatusUpdateReview")?.Script
                        ?? throw new Exception("SQL for StatusUpdate/UpdateTempStatusUpdateReview missing");
                    await conn.ExecuteAsync(reviewSql, InvalidTempRows);
                }

                if (cleanTempRows.Count > 0)
                {
                    var inboxEntries = await InsertInboxEntriesAsync(conn, task, cleanTempRows, createdByUserId);
                    var componentId = await ResolveComponentIdAsync(conn);

                    var approvalSettingsql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "GetApprovalSettingForComponent")?.Script
                        ?? throw new Exception("SQL for approval setting missing");
                    var approvalsetting = componentId.HasValue
                        ? await conn.QuerySingleOrDefaultAsync<InboxFieldConfig>(approvalSettingsql, new { componentId = componentId.Value, orgId = task.OrgId ?? orgId })
                        : null;

                    var useDefaultApprover = approvalsetting?.UseDefaultApprover;
                    if (useDefaultApprover is null)
                    {
                        response.UpdatedRows = await ApplyStatusUpdatesWithoutApprovalAsync(
                            conn,
                            inboxEntries,
                            createdByUserId,
                            token);
                    }
                    else
                    {
                        var statusSql = _mappingProvider.GetScriptForOperation("StatusUpdate", "UpdateTempStatusUpdateStatus")?.Script
                            ?? throw new Exception("SQL for StatusUpdate/UpdateTempStatusUpdateStatus missing");
                        await conn.ExecuteAsync(statusSql, cleanTempRows);

                        if (useDefaultApprover.Value)
                        {
                            await AssignUserHierarchyApproversAsync(conn, inboxEntries, task.OrgId ?? orgId, createdByUserId, token);
                            _logger.LogInformation("Status update entries are pending approval using user hierarchy. TaskId={TaskId}", task.Id);
                        }
                        else
                        {
                            await AssignCustomHierarchyApproversAsync(conn, inboxEntries, task.OrgId ?? orgId, createdByUserId, approvalsetting, token);
                            _logger.LogInformation("Status update entries are pending approval using custom hierarchy. TaskId={TaskId}", task.Id);
                        }
                    }
                }

                response.FailedRows = response.Errors.Count;

                //var updateTaskSql = _mappingProvider.GetScriptForOperation("StatusUpdate", "UpdateTask")?.Script
                //    ?? throw new Exception("SQL for StatusUpdate/UpdateTask missing");

                //var errorMessageEncoded = errorFile != null
                //    ? Convert.ToBase64String(errorFile)
                //    : null;

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

                _logger.LogInformation("BulkStatusUpdate completed for TaskId={TaskId}. UpdatedRows={UpdatedRows}, FailedRows={FailedRows}",
                    task.Id, response.UpdatedRows, response.FailedRows);
            }

            _logger.LogInformation("BulkStatusUpdate job finished");
        }

        private async Task<int?> ResolveComponentIdAsync(DbConnection conn)
        {
            var componentSql = _mappingProvider.GetScriptForOperation("StatusUpdate", "GetStatusUpdateComponentId")?.Script;
            if (string.IsNullOrWhiteSpace(componentSql))
            {
                return null;
            }

            return await conn.QuerySingleOrDefaultAsync<int?>(componentSql);
        }

        private async Task<List<StatusUpdateInboxEntry>> InsertInboxEntriesAsync(
            DbConnection conn,
            FileProcessingTask task,
            List<object> approvedRows,
            int createdByUserId)
        {
            var taskOrgId = task.OrgId ?? orgId;
            var componentId = await ResolveComponentIdAsync(conn);

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

            var inboxEntries = new List<StatusUpdateInboxEntry>();
            foreach (var row in approvedRows)
            {
                var agentCode = row.GetType().GetProperty("AgentCode")?.GetValue(row)?.ToString() ?? string.Empty;
                var status = row.GetType().GetProperty("Status")?.GetValue(row)?.ToString() ?? string.Empty;
                var effectiveDate = row.GetType().GetProperty("BusinessEffectiveDate")?.GetValue(row);

                var requestorNote = JsonSerializer.Serialize(new[]
                {
                    new { FieldName = "AgentCode", OldValue = string.Empty, NewValue = agentCode },
                    new { FieldName = "Status", OldValue = string.Empty, NewValue = status },
                    new { FieldName = "BusinessEffectiveDate", OldValue = string.Empty, NewValue = effectiveDate?.ToString() ?? string.Empty }
                });

                var srNo = await conn.ExecuteScalarAsync<int>(insertInboxSql, new
                {
                    OrgId = taskOrgId,
                    RequestDets = "Bulk Status Updated",
                    RequestorNote = requestorNote,
                    CreatedBy = createdByUserId,
                    CreatedDate = DateTime.UtcNow,
                    SrStatus = 1,
                    ComponentId = componentId,
                    AllocatedToRole = allocatedToRole,
                    ApprovalEndpoint = (string?)null,
                    ApprovalPayload = (string?)null,
                    ObjectName = "BulkStatusUpdate",
                    ObjectReference = task.Id
                });

                inboxEntries.Add(new StatusUpdateInboxEntry(srNo, taskOrgId, agentCode, status));
            }

            return inboxEntries;
        }

        private async Task MarkInboxAsApprovedAsync(DbConnection conn, List<StatusUpdateInboxEntry> inboxEntries)
        {
            if (inboxEntries.Count == 0)
            {
                return;
            }

            var updateInboxSql = _mappingProvider.GetScriptForOperation("Inbox", "UpdateInboxStatus")?.Script
                ?? throw new Exception("SQL for Inbox/UpdateInboxStatus missing");

            foreach (var entry in inboxEntries)
            {
                await conn.ExecuteAsync(updateInboxSql, new
                {
                    orgId = entry.OrgId,
                    srNo = entry.SrNo,
                    srStatus = 3
                });
            }
        }

        private async Task AssignUserHierarchyApproversAsync(
            DbConnection conn,
            List<StatusUpdateInboxEntry> inboxEntries,
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
            List<StatusUpdateInboxEntry> inboxEntries,
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

        private async Task<int> ApplyStatusUpdatesWithoutApprovalAsync(
            DbConnection conn,
            List<StatusUpdateInboxEntry> inboxEntries,
            int decisionBy,
            CancellationToken token)
        {
            if (inboxEntries.Count == 0)
            {
                return 0;
            }

            var agentsSql = _mappingProvider.GetScriptForOperation("StatusUpdate", "GetAgentsByCode")?.Script
                ?? throw new Exception("SQL for StatusUpdate/GetAgentsByCode missing");
            var updateSql = _mappingProvider.GetScriptForOperation("StatusUpdate", "UpdateAgentStatus")?.Script
                ?? throw new Exception("SQL for StatusUpdate/UpdateAgentStatus missing");
            var auditSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "InsertAuditTrail")?.Script
                ?? throw new Exception("SQL for ManagerUpdate/InsertAuditTrail missing");
            var updateInboxSql = _mappingProvider.GetScriptForOperation("Inbox", "UpdateInboxStatus")?.Script
                ?? throw new Exception("SQL for Inbox/UpdateInboxStatus missing");
            var updateTempApprovedSql = _mappingProvider.GetScriptForOperation("StatusUpdate", "UpdateTempStatusUpdateApprovedStatus")?.Script
                ?? throw new Exception("SQL for StatusUpdate/UpdateTempStatusUpdateApprovedStatus missing");

            var orgIdToCodes = inboxEntries
                .GroupBy(x => x.OrgId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.AgentCode)
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

            var updatedRows = 0;
            foreach (var serviceRequest in inboxEntries)
            {
                if (!agentLookup.TryGetValue((serviceRequest.OrgId, serviceRequest.AgentCode), out var agent))
                {
                    _logger.LogWarning("Auto-apply skipped for SrNo={SrNo}; agent code {AgentCode} not found.", serviceRequest.SrNo, serviceRequest.AgentCode);
                    continue;
                }

                await using var tx = await conn.BeginTransactionAsync(token);
                try
                {
                    await conn.ExecuteAsync(updateSql, new
                    {
                        AgentId = agent.AgentId,
                        Status = serviceRequest.Status,
                        ModifiedBy = decisionBy.ToString()
                    }, tx);

                    await conn.ExecuteAsync(auditSql, new
                    {
                        AgentId = agent.AgentId,
                        FieldName = "AgentStatusCode",
                        OldValue = agent.AgentStatusCode ?? string.Empty,
                        NewValue = serviceRequest.Status,
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
                        Status = serviceRequest.Status
                    }, tx);

                    await tx.CommitAsync(token);
                    updatedRows++;
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync(token);
                    _logger.LogError(ex, "Failed to auto-apply status update for SrNo={SrNo}", serviceRequest.SrNo);
                }
            }

            return updatedRows;
        }

        private sealed record StatusUpdateInboxEntry(int SrNo, int OrgId, string AgentCode, string Status);

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
