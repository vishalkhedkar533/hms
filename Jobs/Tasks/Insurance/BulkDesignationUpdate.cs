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

                if (!int.TryParse(task.CreatedBy, out var createdByUserId) || createdByUserId <= 0)
                {
                    _logger.LogError("Invalid CreatedBy in fileprocessingtasks. TaskId={TaskId}, CreatedBy={CreatedBy}", task.Id, task.CreatedBy);
                    continue;
                }

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

                    todayRows.Add((item.RowNumber, agentCode, designation, effectiveDateOnly));

                    //if (effectiveDateOnly == today)
                    //{
                    //    todayRows.Add((item.RowNumber, agentCode, designation, effectiveDateOnly));
                    //}
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

                var InvaildRows = new List<object>();
                var CleanTempRows = new List<object>();
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
                            InvaildRows.Add(new
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
                            InvaildRows.Add(new
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

                        CleanTempRows.Add(new
                        {
                            AgentCode = agentCode,
                            Designation = designation,
                            BusinessEffectiveDate = item.BusinessEffectiveDate.ToDateTime(TimeOnly.MinValue),
                            OrgId = task.OrgId ?? orgId
                        });
                    }
                }

                response.FailedRows = response.Errors.Count;

                if (InvaildRows.Count > 0)
                {
                    var reviewSql = _mappingProvider.GetScriptForOperation("DesignationUpdate", "UpdateTempDesignationUpdateReview")?.Script
                        ?? throw new Exception("SQL for DesignationUpdate/UpdateTempDesignationUpdateReview missing");
                    await conn.ExecuteAsync(reviewSql, InvaildRows);
                }

                if (CleanTempRows.Count > 0)
                {
                    var inboxEntries = await InsertInboxEntriesAsync(conn, task, CleanTempRows, createdByUserId);
                    var componentId = await ResolveComponentIdAsync(conn);

                    var approvalSettingsql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "GetApprovalSettingForComponent")?.Script
                        ?? throw new Exception("SQL for approval setting missing");
                    var approvalsetting = componentId.HasValue
                        ? await conn.QuerySingleOrDefaultAsync<InboxFieldConfig>(approvalSettingsql, new { componentId = componentId.Value, orgId = task.OrgId ?? orgId })
                        : null;

                    var useDefaultApprover = approvalsetting?.UseDefaultApprover;

                    if (useDefaultApprover is null)
                    {
                        response.UpdatedRows = await ApplyDesignationUpdatesWithoutApprovalAsync(
                            conn,
                            inboxEntries,
                            createdByUserId,
                            token);
                    }
                    else
                    {
                        var statusSql = _mappingProvider.GetScriptForOperation("DesignationUpdate", "UpdateTempDesignationUpdateStatus")?.Script
                            ?? throw new Exception("SQL for DesignationUpdate/UpdateTempDesignationUpdateStatus missing");
                        await conn.ExecuteAsync(statusSql, CleanTempRows);

                        if (useDefaultApprover.Value)
                        {
                            await AssignUserHierarchyApproversAsync(conn, inboxEntries, task.OrgId ?? orgId, createdByUserId, token);
                            _logger.LogInformation("Designation update entries are pending approval using user hierarchy. TaskId={TaskId}", task.Id);
                        }
                        else
                        {
                            await AssignCustomHierarchyApproversAsync(conn, inboxEntries, task.OrgId ?? orgId, createdByUserId, approvalsetting, token);
                            _logger.LogInformation("Designation update entries are pending approval using custom hierarchy. TaskId={TaskId}", task.Id);
                        }
                    }
                }

                response.FailedRows = response.Errors.Count;

                //var updateTaskSql = _mappingProvider.GetScriptForOperation("DesignationUpdate", "UpdateTask")?.Script
                //    ?? throw new Exception("SQL for DesignationUpdate/UpdateTask missing");

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

                _logger.LogInformation("BulkDesignationUpdate completed for TaskId={TaskId}. UpdatedRows={UpdatedRows}, FailedRows={FailedRows}",
                    task.Id, response.UpdatedRows, response.FailedRows);
            }

            _logger.LogInformation("BulkDesignationUpdate job finished");
        }

        private async Task<int?> ResolveComponentIdAsync(DbConnection conn)
        {
            var componentSql = _mappingProvider.GetScriptForOperation("DesignationUpdate", "GetDesignationUpdateComponentId")?.Script;
            if (string.IsNullOrWhiteSpace(componentSql))
            {
                return null;
            }
            return await conn.QuerySingleOrDefaultAsync<int?>(componentSql);
        }

        private async Task<List<DesignationUpdateInboxEntry>> InsertInboxEntriesAsync(
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

            var inboxEntries = new List<DesignationUpdateInboxEntry>();
            foreach (var row in approvedRows)
            {
                var agentCode = row.GetType().GetProperty("AgentCode")?.GetValue(row)?.ToString() ?? string.Empty;
                var designation = row.GetType().GetProperty("Designation")?.GetValue(row)?.ToString() ?? string.Empty;
                var effectiveDate = row.GetType().GetProperty("BusinessEffectiveDate")?.GetValue(row);

                var requestorNote = JsonSerializer.Serialize(new[]
                {
                    new { FieldName = "AgentCode", OldValue = string.Empty, NewValue = agentCode },
                    new { FieldName = "Designation", OldValue = string.Empty, NewValue = designation },
                    new { FieldName = "BusinessEffectiveDate", OldValue = string.Empty, NewValue = effectiveDate?.ToString() ?? string.Empty }
                });

                var srNo = await conn.ExecuteScalarAsync<int>(insertInboxSql, new
                {
                    OrgId = taskOrgId,
                    RequestDets = "Bulk Designation Updated",
                    RequestorNote = requestorNote,
                    CreatedBy = createdByUserId,
                    CreatedDate = DateTime.UtcNow,
                    SrStatus = 1,
                    ComponentId = componentId,
                    AllocatedToRole = allocatedToRole,
                    ApprovalEndpoint = (string?)null,
                    ApprovalPayload = (string?)null,
                    ObjectName = "BulkDesignationUpdate",
                    ObjectReference = task.Id
                });

                inboxEntries.Add(new DesignationUpdateInboxEntry(srNo, taskOrgId));
            }

            return inboxEntries;
        }

        private async Task MarkInboxAsApprovedAsync(DbConnection conn, List<DesignationUpdateInboxEntry> inboxEntries)
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
            List<DesignationUpdateInboxEntry> inboxEntries,
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
            List<DesignationUpdateInboxEntry> inboxEntries,
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

        private sealed record DesignationUpdateInboxEntry(int SrNo, int OrgId);

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

        private async Task<int> ApplyDesignationUpdatesWithoutApprovalAsync(
            DbConnection conn,
            List<DesignationUpdateInboxEntry> inboxEntries,
            int decisionBy,
            CancellationToken token)
        {
            if (inboxEntries.Count == 0)
            {
                return 0;
            }

            var applySql = _mappingProvider.GetScriptForOperation("DesignationUpdate", "ApplyTempDesignationUpdate")?.Script
                ?? throw new Exception("SQL for DesignationUpdate/ApplyTempDesignationUpdate missing");
            var updateInboxSql = _mappingProvider.GetScriptForOperation("Inbox", "UpdateInboxStatus")?.Script
                ?? throw new Exception("SQL for Inbox/UpdateInboxStatus missing");

            var totalUpdatedRows = 0;
            var orgIds = inboxEntries.Select(x => x.OrgId).Distinct().ToList();

            await using var tx = await conn.BeginTransactionAsync(token);
            try
            {
                foreach (var currentOrgId in orgIds)
                {
                    totalUpdatedRows += await conn.ExecuteScalarAsync<int>(applySql, new
                    {
                        OrgId = currentOrgId,
                        ModifiedBy = decisionBy.ToString()
                    }, tx);
                }

                foreach (var serviceRequest in inboxEntries)
                {
                    await conn.ExecuteAsync(updateInboxSql, new
                    {
                        orgId = serviceRequest.OrgId,
                        srNo = serviceRequest.SrNo,
                        srStatus = 3
                    }, tx);
                }

                await tx.CommitAsync(token);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(token);
                _logger.LogError(ex, "Failed to auto-apply designation updates without approval.");
                throw;
            }

            return totalUpdatedRows;
        }
    }
}
