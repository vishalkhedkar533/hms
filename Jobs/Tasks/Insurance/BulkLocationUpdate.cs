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

                var rejectedTempRows = new List<object>();
                var approvedTempRows = new List<object>();

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
                        rejectedTempRows.Add(new
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
                        rejectedTempRows.Add(new
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

                    approvedTempRows.Add(new
                    {
                        AgentCode = row.AgentCode,
                        LocationCode = row.LocationCode,
                        CurrentChannel = row.CurrentChannel,
                        CurrentSubChannel = row.CurrentSubChannel,
                        OrgId = row.OrgId
                    });
                }

                response.FailedRows = response.Errors.Count;

                if (rejectedTempRows.Count > 0)
                {
                    var reviewSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "UpdateTempLocationUpdateReview")?.Script
                        ?? throw new Exception("SQL for LocationUpdate/UpdateTempLocationUpdateReview missing");
                    await conn.ExecuteAsync(reviewSql, rejectedTempRows);
                }

                if (approvedTempRows.Count > 0)
                {
                    var statusSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "UpdateTempLocationUpdateStatus")?.Script
                        ?? throw new Exception("SQL for LocationUpdate/UpdateTempLocationUpdateStatus missing");
                    await conn.ExecuteAsync(statusSql, approvedTempRows);
                }

                // var applySql = _mappingProvider.GetScriptForOperation("LocationUpdate", "ApplyTempLocationUpdate")?.Script
                //     ?? throw new Exception("SQL for LocationUpdate/ApplyTempLocationUpdate missing");
                // response.UpdatedRows = await conn.ExecuteScalarAsync<int>(applySql, new { OrgId = task.OrgId ?? orgId, ModifiedBy = username });

                response.UpdatedRows = 0;

                var updateTaskSql = _mappingProvider.GetScriptForOperation("LocationUpdate", "UpdateTask")?.Script
                    ?? throw new Exception("SQL for LocationUpdate/UpdateTask missing");

                await conn.ExecuteAsync(updateTaskSql, new
                {
                    Id = task.Id,
                    RowsProcessed = response.UpdatedRows,
                    TotalRows = response.TotalRows,
                    RowsRejected = response.FailedRows,
                    ErrorMessage = (string?)null,
                    SuccessData = (string?)null,
                    Status = response.Errors.Any() ? "CompletedWithErrors" : "Completed"
                });

                _logger.LogInformation("BulkLocationUpdate completed for TaskId={TaskId}. UpdatedRows={UpdatedRows}, FailedRows={FailedRows}",
                    task.Id, response.UpdatedRows, response.FailedRows);
            }

            _logger.LogInformation("BulkLocationUpdate job finished");
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
    }
}
