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
    public class BulkLocationUpdate
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        private int orgId = 0;
        public JobKey jobKey;
        private readonly ILogger<BulkLocationUpdate> _logger;
        private readonly IMappingProvider _mappingProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IConnectionScope _connectionScope;
        private readonly IBinaryImportFactory _bulkOpsFactory;

        public BulkLocationUpdate(IJobExecutionContext jobExecutionContext,
            ILogger<BulkLocationUpdate> logger,
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

        public async Task ProcessLocationUpdate(JobExeHist jobExeHist)
        {
            _logger.LogInformation("BulkLocationUpdate job started for OrgId={OrgId}", orgId);

            var operationMapping = _mappingProvider.GetScriptForOperation("Job", "Fetch")
                ?? throw new InvalidOperationException("Operation mapping for Job/Fetch not found.");

            var connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

            var conn = await _connectionScope.GetOpenConnectionAsync(connectionString);
            var token = CancellationToken.None;

            var tasksSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "GetPendingTasks")?.Script
                ?? throw new Exception("SQL for LocationUpdate/GetPendingTasks missing");
            var tasks = (await conn.QueryAsync<FileProcessingTask>(tasksSql)).ToList();

            if (!tasks.Any())
            {
                _logger.LogInformation("No pending location update tasks found");
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

                var rows = MiniExcel.Query<LocationUpdateRow>(task.FilePath).ToList();
                var response = new LocationUpdateResponse { TotalRows = rows.Count };

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

                var tempRows = new List<LocationTempRow>();

                foreach (var item in rows.Select((row, index) => new { Row = row, RowNumber = index + 2 }))
                {
                    var agentCode = item.Row.AgentCode?.Trim();
                    var locationType = item.Row.LocationType?.Trim();
                    var locationCode = item.Row.LocationCode?.Trim();
                    var currentChannel = item.Row.CurrentChannel?.Trim();
                    var currentSubChannel = item.Row.CurrentSubChannel?.Trim();
                    var status = item.Row.Status?.Trim();

                    if (string.IsNullOrWhiteSpace(agentCode))
                    {
                        AddError(response, item.RowNumber, agentCode, locationCode, "Agent Code is required.");
                        tempRows.Add(new LocationTempRow(item.RowNumber, agentCode ?? string.Empty, locationType ?? string.Empty, locationCode ?? string.Empty, currentChannel ?? string.Empty, currentSubChannel ?? string.Empty, status ?? string.Empty, task.OrgId ?? orgId, "Rejected", "Agent Code is required."));
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(locationCode))
                    {
                        AddError(response, item.RowNumber, agentCode, locationCode, "Location Code is required.");
                        tempRows.Add(new LocationTempRow(item.RowNumber, agentCode, locationType ?? string.Empty, locationCode ?? string.Empty, currentChannel ?? string.Empty, currentSubChannel ?? string.Empty, status ?? string.Empty, task.OrgId ?? orgId, "Rejected", "Location Code is required."));
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(currentChannel))
                    {
                        AddError(response, item.RowNumber, agentCode, locationCode, "Current Channel is required.");
                        tempRows.Add(new LocationTempRow(item.RowNumber, agentCode, locationType ?? string.Empty, locationCode, currentChannel ?? string.Empty, currentSubChannel ?? string.Empty, status ?? string.Empty, task.OrgId ?? orgId, "Rejected", "Current Channel is required."));
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(currentSubChannel))
                    {
                        AddError(response, item.RowNumber, agentCode, locationCode, "Current Sub Channel is required.");
                        tempRows.Add(new LocationTempRow(item.RowNumber, agentCode, locationType ?? string.Empty, locationCode, currentChannel, currentSubChannel ?? string.Empty, status ?? string.Empty, task.OrgId ?? orgId, "Rejected", "Current Sub Channel is required."));
                        continue;
                    }

                    tempRows.Add(new LocationTempRow(item.RowNumber, agentCode, locationType ?? string.Empty, locationCode, currentChannel, currentSubChannel, "PENDING", task.OrgId ?? orgId, "Proceed", string.Empty));
                }

                if (tempRows.Any())
                {
                    var bulkSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "BulkCopyTempLocationUpdate")?.Script
                        ?? throw new Exception("SQL for LocationUpdate/BulkCopyTempLocationUpdate missing");

                    await using var writer = await _bulkOpsFactory.BeginBinaryImportAsync(conn, bulkSql, token);
                    foreach (var row in tempRows)
                    {
                        writer.StartRow();
                        writer.Write(row.AgentCode);
                        writer.Write(row.LocationType);
                        writer.Write(row.LocationCode);
                        writer.Write(row.CurrentChannel);
                        writer.Write(row.CurrentSubChannel);
                        writer.Write(row.Status);
                        writer.Write(row.OrgId);
                        writer.Write(row.Comment);
                        writer.Write(row.Reason);
                    }

                    await writer.CompleteAsync(token);
                }

                var InvaildRows = new List<object>();
                var CleanTempRows = new List<object>();

                var agentCodes = tempRows.Select(r => r.AgentCode)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                var locationCodes = tempRows.Select(r => r.LocationCode)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                var channels = tempRows.Select(r => r.CurrentChannel)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                var subChannels = tempRows.Select(r => r.CurrentSubChannel)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                var agentsSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "GetAgentsByCode")?.Script
                    ?? throw new Exception("SQL for LocationUpdate/GetAgentsByCode missing");
                var locationsSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "GetLocationsForValidation")?.Script
                    ?? throw new Exception("SQL for LocationUpdate/GetLocationsForValidation missing");

                var agents = (await conn.QueryAsync<Agent>(agentsSql, new { orgId = task.OrgId ?? orgId, codes = agentCodes })).ToList();
                var agentByCode = agents
                    .Where(a => !string.IsNullOrWhiteSpace(a.AgentCode))
                    .ToDictionary(a => a.AgentCode, StringComparer.OrdinalIgnoreCase);

                var locations = (await conn.QueryAsync<LocationUpdateLookup>(locationsSql, new
                {
                    orgId = task.OrgId ?? orgId,
                    locationCodes,
                    channels,
                    subChannels
                })).ToList();

                var locationByKey = locations.ToDictionary(
                    l => BuildLocationKey(l.LocationCode, l.ChannelName, l.SubChannelName),
                    StringComparer.OrdinalIgnoreCase);

                foreach (var row in tempRows.Where(r => r.Comment.Equals("Proceed", StringComparison.OrdinalIgnoreCase)))
                {
                    if (!agentByCode.ContainsKey(row.AgentCode))
                    {
                        var reason = "Invalid Agent Code.";
                        AddError(response, row.RowNumber, row.AgentCode, row.LocationCode, reason);
                        InvaildRows.Add(new
                        {
                            AgentCode = row.AgentCode,
                            LocationType = row.LocationType,
                            LocationCode = row.LocationCode,
                            CurrentChannel = row.CurrentChannel,
                            CurrentSubChannel = row.CurrentSubChannel,
                            Status = row.Status,
                            OrgId = row.OrgId,
                            Comment = "Rejected",
                            Reason = reason
                        });
                        continue;
                    }

                    var key = BuildLocationKey(row.LocationCode, row.CurrentChannel, row.CurrentSubChannel);
                    if (!locationByKey.ContainsKey(key))
                    {
                        var reason = "Invalid location for channel/subchannel.";
                        AddError(response, row.RowNumber, row.AgentCode, row.LocationCode, reason);
                        InvaildRows.Add(new
                        {
                            AgentCode = row.AgentCode,
                            LocationType = row.LocationType,
                            LocationCode = row.LocationCode,
                            CurrentChannel = row.CurrentChannel,
                            CurrentSubChannel = row.CurrentSubChannel,
                            Status = row.Status,
                            OrgId = row.OrgId,
                            Comment = "Rejected",
                            Reason = reason
                        });
                        continue;
                    }

                    CleanTempRows.Add(new
                    {
                        AgentCode = row.AgentCode,
                        LocationCode = row.LocationCode,
                        CurrentChannel = row.CurrentChannel,
                        CurrentSubChannel = row.CurrentSubChannel,
                        OrgId = row.OrgId
                    });
                }

                response.FailedRows = response.Errors.Count;

                if (InvaildRows.Count > 0)
                {
                    var reviewSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "UpdateTempLocationUpdateReview")?.Script
                        ?? throw new Exception("SQL for LocationUpdate/UpdateTempLocationUpdateReview missing");
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
                        response.UpdatedRows = await ApplyLocationUpdatesWithoutApprovalAsync(
                            conn,
                            inboxEntries,
                            createdByUserId,
                            token);
                    }
                    else
                    {
                        var statusSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "UpdateTempLocationUpdateStatus")?.Script
                            ?? throw new Exception("SQL for LocationUpdate/UpdateTempLocationUpdateStatus missing");
                        await conn.ExecuteAsync(statusSql, CleanTempRows);

                        if (useDefaultApprover.Value)
                        {
                            await AssignUserHierarchyApproversAsync(conn, inboxEntries, task.OrgId ?? orgId, createdByUserId, token);
                            _logger.LogInformation("Location update entries are pending approval using user hierarchy. TaskId={TaskId}", task.Id);
                        }
                        else
                        {
                            await AssignCustomHierarchyApproversAsync(conn, inboxEntries, task.OrgId ?? orgId, createdByUserId, approvalsetting, token);
                            _logger.LogInformation("Location update entries are pending approval using custom hierarchy. TaskId={TaskId}", task.Id);
                        }
                    }
                }

                // var applySql = _mappingProvider.GetScriptForOperation("LocationUpdate", "ApplyTempLocationUpdate")?.Script
                //     ?? throw new Exception("SQL for LocationUpdate/ApplyTempLocationUpdate missing");
                // response.UpdatedRows = await conn.ExecuteScalarAsync<int>(applySql, new { OrgId = task.OrgId ?? orgId, ModifiedBy = username });

                // Keep UpdatedRows from auto-apply path when set
                // response.UpdatedRows = 0;

                //var updateTaskSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "UpdateTask")?.Script
                //    ?? throw new Exception("SQL for LocationUpdate/UpdateTask missing");

                //await conn.ExecuteAsync(updateTaskSql, new
                //{
                //    Id = task.Id,
                //    RowsProcessed = response.UpdatedRows,
                //    TotalRows = response.TotalRows,
                //    RowsRejected = response.FailedRows,
                //    ErrorMessage = (string?)null,
                //    SuccessData = (string?)null,
                //    Status = response.Errors.Any() ? "CompletedWithErrors" : "Completed"
                //});

                _logger.LogInformation("BulkLocationUpdate completed for TaskId={TaskId}. UpdatedRows={UpdatedRows}, FailedRows={FailedRows}",
                    task.Id, response.UpdatedRows, response.FailedRows);
            }

            _logger.LogInformation("BulkLocationUpdate job finished");
        }

        private async Task<int?> ResolveComponentIdAsync(DbConnection conn)
        {
            var componentSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "GetLocationUpdateComponentId")?.Script;
            if (string.IsNullOrWhiteSpace(componentSql))
            {
                return null;
            }

            return await conn.QuerySingleOrDefaultAsync<int?>(componentSql);
        }

        private async Task<List<LocationUpdateInboxEntry>> InsertInboxEntriesAsync(
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

            var inboxEntries = new List<LocationUpdateInboxEntry>();
            foreach (var row in approvedRows)
            {
                var agentCode = row.GetType().GetProperty("AgentCode")?.GetValue(row)?.ToString() ?? string.Empty;
                var locationCode = row.GetType().GetProperty("LocationCode")?.GetValue(row)?.ToString() ?? string.Empty;
                var currentChannel = row.GetType().GetProperty("CurrentChannel")?.GetValue(row)?.ToString() ?? string.Empty;
                var currentSubChannel = row.GetType().GetProperty("CurrentSubChannel")?.GetValue(row)?.ToString() ?? string.Empty;

                var requestorNote = JsonSerializer.Serialize(new[]
                {
                    new { FieldName = "AgentCode", OldValue = string.Empty, NewValue = agentCode },
                    new { FieldName = "LocationCode", OldValue = string.Empty, NewValue = locationCode },
                    new { FieldName = "CurrentChannel", OldValue = string.Empty, NewValue = currentChannel },
                    new { FieldName = "CurrentSubChannel", OldValue = string.Empty, NewValue = currentSubChannel }
                });

                var srNo = await conn.ExecuteScalarAsync<int>(insertInboxSql, new
                {
                    OrgId = taskOrgId,
                    RequestDets = "Bulk Location Updated",
                    RequestorNote = requestorNote,
                    CreatedBy = createdByUserId,
                    CreatedDate = DateTime.UtcNow,
                    SrStatus = 1,
                    ComponentId = componentId,
                    AllocatedToRole = allocatedToRole,
                    ApprovalEndpoint = (string?)null,
                    ApprovalPayload = (string?)null,
                    ObjectName = "BulkLocationUpdate",
                    ObjectReference = task.Id
                });

                inboxEntries.Add(new LocationUpdateInboxEntry(srNo, taskOrgId, agentCode, locationCode, currentChannel, currentSubChannel));
            }

            return inboxEntries;
        }

        private async Task MarkInboxAsApprovedAsync(DbConnection conn, List<LocationUpdateInboxEntry> inboxEntries)
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
            List<LocationUpdateInboxEntry> inboxEntries,
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
            List<LocationUpdateInboxEntry> inboxEntries,
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

        private async Task<int> ApplyLocationUpdatesWithoutApprovalAsync(
            DbConnection conn,
            List<LocationUpdateInboxEntry> inboxEntries,
            int decisionBy,
            CancellationToken token)
        {
            if (inboxEntries.Count == 0)
            {
                return 0;
            }

            var agentsSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "GetAgentsByCode")?.Script
                ?? throw new Exception("SQL for LocationUpdate/GetAgentsByCode missing");
            var locationsSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "GetLocationsForValidation")?.Script
                ?? throw new Exception("SQL for LocationUpdate/GetLocationsForValidation missing");
            var updateSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "UpdateAgentLocation")?.Script
                ?? throw new Exception("SQL for LocationUpdate/UpdateAgentLocation missing");
            var auditSql = _mappingProvider.GetScriptForOperation("ManagerUpdate", "InsertAuditTrail")?.Script
                ?? throw new Exception("SQL for ManagerUpdate/InsertAuditTrail missing");
            var updateInboxSql = _mappingProvider.GetScriptForOperation("Inbox", "UpdateInboxStatus")?.Script
                ?? throw new Exception("SQL for Inbox/UpdateInboxStatus missing");
            var updateTempApprovedSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "UpdateTempLocationUpdateApprovedStatus")?.Script
                ?? throw new Exception("SQL for LocationUpdate/UpdateTempLocationUpdateApprovedStatus missing");

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

            var allLocationCodes = inboxEntries.Select(x => x.LocationCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var allChannels = inboxEntries.Select(x => x.CurrentChannel)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var allSubChannels = inboxEntries.Select(x => x.CurrentSubChannel)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var locationLookup = new Dictionary<(int OrgId, string LocationCode, string Channel, string SubChannel), LocationUpdateLookup>();
            foreach (var org in inboxEntries.Select(x => x.OrgId).Distinct())
            {
                var locations = await conn.QueryAsync<LocationUpdateLookup>(locationsSql, new
                {
                    orgId = org,
                    locationCodes = allLocationCodes,
                    channels = allChannels,
                    subChannels = allSubChannels
                });

                foreach (var location in locations)
                {
                    locationLookup[(org, location.LocationCode, location.ChannelName, location.SubChannelName)] = location;
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

                if (!locationLookup.TryGetValue((serviceRequest.OrgId, serviceRequest.LocationCode, serviceRequest.CurrentChannel, serviceRequest.CurrentSubChannel), out var location))
                {
                    _logger.LogWarning("Auto-apply skipped for SrNo={SrNo}; location mapping not found for {LocationCode}/{Channel}/{SubChannel}.",
                        serviceRequest.SrNo, serviceRequest.LocationCode, serviceRequest.CurrentChannel, serviceRequest.CurrentSubChannel);
                    continue;
                }

                await using var tx = await conn.BeginTransactionAsync(token);
                try
                {
                    await conn.ExecuteAsync(updateSql, new
                    {
                        AgentId = agent.AgentId,
                        LocationMasterId = location.LocationMasterId,
                        ModifiedBy = decisionBy.ToString()
                    }, tx);

                    await conn.ExecuteAsync(auditSql, new
                    {
                        AgentId = agent.AgentId,
                        FieldName = "LocationCode",
                        OldValue = agent.LocationCode?.ToString() ?? string.Empty,
                        NewValue = location.LocationMasterId.ToString(),
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
                        LocationCode = serviceRequest.LocationCode,
                        CurrentChannel = serviceRequest.CurrentChannel,
                        CurrentSubChannel = serviceRequest.CurrentSubChannel
                    }, tx);

                    await tx.CommitAsync(token);
                    updatedRows++;
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync(token);
                    _logger.LogError(ex, "Failed to auto-apply location update for SrNo={SrNo}", serviceRequest.SrNo);
                }
            }

            return updatedRows;
        }

        private static string BuildLocationKey(string locationCode, string channel, string subChannel)
        {
            return $"{locationCode.Trim().ToLowerInvariant()}|{channel.Trim().ToLowerInvariant()}|{subChannel.Trim().ToLowerInvariant()}";
        }

        private static void AddError(LocationUpdateResponse resp, int row, string? agent, string? locationCode, string msg)
        {
            resp.Errors.Add(new LocationUpdateError
            {
                RowNumber = row,
                AgentCode = agent ?? string.Empty,
                LocationCode = locationCode ?? string.Empty,
                Message = msg
            });
        }

        private sealed record LocationTempRow(
            int RowNumber,
            string AgentCode,
            string LocationType,
            string LocationCode,
            string CurrentChannel,
            string CurrentSubChannel,
            string Status,
            int OrgId,
            string Comment,
            string Reason);

        private sealed record LocationUpdateInboxEntry(
            int SrNo,
            int OrgId,
            string AgentCode,
            string LocationCode,
            string CurrentChannel,
            string CurrentSubChannel);
    }
}
