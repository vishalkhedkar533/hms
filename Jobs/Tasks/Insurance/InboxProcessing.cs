using CommonLibrary.mapping;
using Dapper;
using Quartz;
using Repository;
using SharedModels.BackEndCalculation;
using Tasks.Models;

namespace Tasks.Insurance
{
    public class InboxProcessing
    {
        private readonly IMappingProvider _mappingProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InboxProcessing> _logger;
        private readonly IConnectionScope _connectionScope;
        private readonly IJobExecutionContext _jobExecutionContext;
        private readonly int _orgId;

        public InboxProcessing(
            IJobExecutionContext jobExecutionContext,
            IMappingProvider mappingProvider,
            IConfiguration configuration,
            ILogger<InboxProcessing> logger,
            IConnectionScope connectionScope)
        {
            _jobExecutionContext = jobExecutionContext;
            _orgId = int.Parse(jobExecutionContext.JobDetail.JobDataMap.Values
                .Select((value, index) => new { value, index })
                .FirstOrDefault(x => x.index.Equals(
                    jobExecutionContext.JobDetail.JobDataMap.Keys
                    .Select((value, index) => new { value, index })
                    .FirstOrDefault(x => x.value == "orgId").index))
                .value.ToString());
            _mappingProvider = mappingProvider;
            _configuration = configuration;
            _logger = logger;
            _connectionScope = connectionScope ?? throw new ArgumentNullException(nameof(connectionScope));
        }

        public async Task ProcessInboxData(JobExeHist jobExeHist)
        {
            _logger.LogInformation("InboxProcessing job started for OrgId={OrgId}", _orgId);

            var operationMapping = _mappingProvider.GetScriptForOperation("Inbox", "GetPendingInbox")
                ?? throw new InvalidOperationException("Operation mapping for Inbox/GetPendingInbox not found.");

            var connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

            var conn = await _connectionScope.GetOpenConnectionAsync(connectionString);
            var pendingSql = operationMapping.Script;

            var entries = (await conn.QueryAsync<InboxEntry>(pendingSql, new { orgId = _orgId })).ToList();
            if (entries.Count == 0)
            {
                _logger.LogInformation("No pending inbox entries found for OrgId={OrgId}.", _orgId);
                return;
            }

            var configSql = _mappingProvider.GetScriptForOperation("Inbox", "GetFieldConfig")?.Script
                ?? throw new InvalidOperationException("Operation mapping for Inbox/GetFieldConfig not found.");
            var insertSql = _mappingProvider.GetScriptForOperation("Inbox", "InsertSrApprover")?.Script
                ?? throw new InvalidOperationException("Operation mapping for Inbox/InsertSrApprover not found.");
            var updateInboxSql = _mappingProvider.GetScriptForOperation("Inbox", "UpdateInboxStatus")?.Script
                ?? throw new InvalidOperationException("Operation mapping for Inbox/UpdateInboxStatus not found.");

            var token = CancellationToken.None;
            foreach (var entry in entries)
            {
                if (entry.ControlId == null)
                {
                    _logger.LogWarning("Inbox entry SrNo={SrNo} missing ControlId; skipping.", entry.SrNo);
                    continue;
                }

                var config = (await conn.QueryAsync<InboxFieldConfig>(
                    configSql,
                    new { orgId = entry.OrgId, cntrlId = entry.ControlId },
                    commandType: System.Data.CommandType.Text)).FirstOrDefault();

                if (config == null)
                {
                    _logger.LogWarning("No field configuration found for OrgId={OrgId}, ControlId={ControlId}.", entry.OrgId, entry.ControlId);
                    continue;
                }

                var approverRoles = ResolveApproverRoles(config);
                if (approverRoles.Count == 0)
                {
                    _logger.LogWarning("No approver roles resolved for OrgId={OrgId}, ControlId={ControlId}, SrNo={SrNo}.", entry.OrgId, entry.ControlId, entry.SrNo);
                    continue;
                }

                await using var tx = await conn.BeginTransactionAsync(token);
                try
                {
                    var level = 1;
                    foreach (var roleId in approverRoles)
                    {
                        await conn.ExecuteAsync(
                            insertSql,
                            new
                            {
                                orgId = entry.OrgId,
                                srNo = entry.SrNo,
                                approverLevel = level,
                                allocatedRoleId = roleId
                            },
                            transaction: tx);
                        level++;
                    }

                    await conn.ExecuteAsync(
                        updateInboxSql,
                        new
                        {
                            orgId = entry.OrgId,
                            srNo = entry.SrNo,
                            srStatus = 2,
                            allocatedRoleId = approverRoles[0]
                        },
                        transaction: tx);

                    await tx.CommitAsync(token);
                    _logger.LogInformation("Processed inbox entry SrNo={SrNo} with {Count} approvers.", entry.SrNo, approverRoles.Count);
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync(token);
                    _logger.LogError(ex, "Failed to process inbox entry SrNo={SrNo}.", entry.SrNo);
                }
            }

            _logger.LogInformation("InboxProcessing job finished for OrgId={OrgId}", _orgId);
        }

        private static List<int> ResolveApproverRoles(InboxFieldConfig config)
        {
            var roles = new List<int>();
            var useDefault = config.UseDefaultApprover ?? true;

            if (useDefault)
            {
                if (config.RoleId.HasValue)
                {
                    roles.Add(config.RoleId.Value);
                }
            }
            else
            {
                if (config.ApproverOneId.HasValue)
                {
                    roles.Add(config.ApproverOneId.Value);
                }
                if (config.ApproverTwoId.HasValue)
                {
                    roles.Add(config.ApproverTwoId.Value);
                }
                if (config.ApproverThreeId.HasValue)
                {
                    roles.Add(config.ApproverThreeId.Value);
                }
            }

            return roles;
        }
    }
}
