using CommonLibrary.mapping;
using Dapper;
using Quartz;
using Repository;
using SharedModels.BackEndCalculation;
using System.Data.Common;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Tasks.Models;

namespace Tasks.Insurance
{
    public class InboxProcessing
    {
        private const string BackgroundJobUserNameCategory = "BackgroundJobUserName";
        private const string BackgroundJobUserPasswordCategory = "BackgroundJobUserPassword";
        private readonly IMappingProvider _mappingProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InboxProcessing> _logger;
        private readonly IConnectionScope _connectionScope;
        private readonly IJobExecutionContext _jobExecutionContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly int _orgId;

        public InboxProcessing(
            IJobExecutionContext jobExecutionContext,
            IMappingProvider mappingProvider,
            IConfiguration configuration,
            ILogger<InboxProcessing> logger,
            IConnectionScope connectionScope,
            IHttpClientFactory httpClientFactory)
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
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
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
            var managerRolesSql = _mappingProvider.GetScriptForOperation("Inbox", "GetManagerHierarchyRoles")?.Script
                ?? throw new InvalidOperationException("Operation mapping for Inbox/GetManagerHierarchyRoles not found.");

            var token = CancellationToken.None;
            foreach (var entry in entries)
            {
                if (entry.ComponentId == null)
                {
                    _logger.LogWarning("Inbox entry SrNo={SrNo} missing ComponentId; skipping.", entry.SrNo);
                    continue;
                }

                var config = (await conn.QueryAsync<InboxFieldConfig>(
                    configSql,
                    new { orgId = entry.OrgId, componentId = entry.ComponentId },
                    commandType: System.Data.CommandType.Text)).FirstOrDefault();

                if (config == null)
                {
                    _logger.LogWarning("No field configuration found for OrgId={OrgId}, ComponentId={ComponentId}.", entry.OrgId, entry.ComponentId);
                    continue;
                }

                if (config.UseDefaultApprover is null)
                {
                    await HandleAutoApprovalAsync(conn, insertSql, updateInboxSql, entry, config, token);
                    continue;
                }

                var approverRoles = config.UseDefaultApprover.Value
                    ? await ResolveManagerHierarchyRolesAsync(conn, managerRolesSql, entry)
                    : ResolveCustomApproverRoles(config);

                if (approverRoles.Count == 0)
                {
                    _logger.LogWarning("No approver roles resolved for OrgId={OrgId}, ComponentId={ComponentId}, SrNo={SrNo}.", entry.OrgId, entry.ComponentId, entry.SrNo);
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
                                allocatedRoleId = roleId,
                                decisionBy = entry.CreatedBy,
                                decisionOn = (DateTime?)null,
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
                            //allocatedRoleId = approverRoles[0]
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
        private async Task HandleAutoApprovalAsync(DbConnection conn, string insertSql, string updateInboxSql, InboxEntry entry, InboxFieldConfig config, CancellationToken token)
        {
            var applied = await TryInvokeApprovalEndpointAsync(conn, entry, token);
            if (!applied)
            {
                _logger.LogWarning("Auto-approval skipped for SrNo={SrNo} because approval call failed.", entry.SrNo);
                return;
            }

            await using var tx = await conn.BeginTransactionAsync(token);
            try
            {
                var allocatedRoleId = ResolveAutoApprovalRoleId(entry);
                if (allocatedRoleId.HasValue)
                {
                    await conn.ExecuteAsync(
                        insertSql,
                        new
                        {
                            orgId = entry.OrgId,
                            srNo = entry.SrNo,
                            approverLevel = 1,
                            allocatedRoleId = allocatedRoleId.Value,
                            decisionBy = entry.CreatedBy,
                            decisionOn = DateTime.UtcNow,
                            approverDecision = 2
                        },
                        transaction: tx);
                }
                else
                {
                    _logger.LogWarning("Auto-approval entry SrNo={SrNo} has no role id configured; skipping sr_approver insert.", entry.SrNo);
                }

                await conn.ExecuteAsync(
                    updateInboxSql,
                    new
                    {
                        orgId = entry.OrgId,
                        srNo = entry.SrNo,
                        srStatus = 3,
                        allocatedRoleId = (int?)null
                    },
                    transaction: tx);

                await tx.CommitAsync(token);
                _logger.LogInformation("Auto-approved inbox entry SrNo={SrNo}.", entry.SrNo);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(token);
                _logger.LogError(ex, "Failed to update auto-approved inbox entry SrNo={SrNo}.", entry.SrNo);
            }
        }
        private async Task<bool> TryInvokeApprovalEndpointAsync(DbConnection conn, InboxEntry entry, CancellationToken token)
        {
            var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                _logger.LogWarning("ApiSettings:BaseUrl is not configured. Skipping auto-approval for SrNo={SrNo}.", entry.SrNo);
                return false;
            }

            if (string.IsNullOrWhiteSpace(entry.ApprovalEndpoint) || string.IsNullOrWhiteSpace(entry.ApprovalPayload))
            {
                _logger.LogWarning("Inbox entry SrNo={SrNo} missing approval details; skipping auto-approval.", entry.SrNo);
                return false;
            }

            if (!TryBuildApprovalUri(baseUrl, entry.ApprovalEndpoint, out var requestUri))
            {
                _logger.LogWarning("Unable to resolve approval endpoint for SrNo={SrNo}.", entry.SrNo);
                return false;
            }

            if (!TryExtractPayload(entry.ApprovalPayload, out var payloadJson))
            {
                _logger.LogWarning("Unable to parse approval payload for SrNo={SrNo}.", entry.SrNo);
                return false;
            }

            var client = _httpClientFactory.CreateClient();
            var tokenValue = await GetBackgroundJobAuthTokenAsync(conn, token);
            if (!string.IsNullOrWhiteSpace(tokenValue))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);
            }

            using var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(requestUri, content, token);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Approval endpoint failed for SrNo={SrNo}. Status={Status}", entry.SrNo, response.StatusCode);
                return false;
            }

            return true;
        }
        private async Task<string?> GetBackgroundJobAuthTokenAsync(DbConnection conn, CancellationToken token)
        {
            var masterSql = _mappingProvider.GetScriptForOperation("Master", "KeyValueEntries")?.Script
                ?? throw new InvalidOperationException("Operation mapping for Master/KeyValueEntries not found.");

            var sql = masterSql.Replace("{{FilterCriteria}}", "AND k.entrycategory = ANY(@entryCategories) AND (k.activestatus IS NULL OR k.activestatus = true)");
            var entries = (await conn.QueryAsync<KeyValueEntry>(sql, new
            {
                OrgId = _orgId,
                entryCategories = new[] { BackgroundJobUserNameCategory, BackgroundJobUserPasswordCategory }
            })).ToList();

            var username = entries.FirstOrDefault(e => e.EntryCategory == BackgroundJobUserNameCategory)?.EntryDesc;
            var password = entries.FirstOrDefault(e => e.EntryCategory == BackgroundJobUserPasswordCategory)?.EntryDesc;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Background job credentials missing in keyvalueentries for OrgId={OrgId}.", _orgId);
                return null;
            }

            var baseUrl = _configuration.GetValue<string>("ApiSettings:BaseUrl");
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                _logger.LogWarning("ApiSettings:BaseUrl is not configured. Cannot obtain auth token.");
                return null;
            }

            var loginUri = new Uri(new Uri(baseUrl), "api/Auth/login");
            var client = _httpClientFactory.CreateClient();

            var payload = JsonSerializer.Serialize(new { Username = username, Password = password });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(loginUri, content, token);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(token);
                _logger.LogWarning("Auth login failed for background job. Status={Status}. Response={Response}", response.StatusCode, errorBody);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: token);
            if (document.RootElement.TryGetProperty("responseBody", out var responseBody)
                && responseBody.TryGetProperty("loginResponse", out var loginResponse)
                && loginResponse.TryGetProperty("token", out var tokenElement))
            {
                return tokenElement.GetString();
            }

            _logger.LogWarning("Auth token not found in login response for background job.");
            return null;
        }
        private static bool TryBuildApprovalUri(string baseUrl, string endpoint, out Uri requestUri)
        {
            requestUri = null!;
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(endpoint))
            {
                return false;
            }

            try
            {
                // Ensure baseUrl ends with '/' and endpoint does not start with '/'
                var normalizedBaseUrl = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
                var normalizedEndpoint = endpoint.StartsWith("/") ? endpoint.Substring(1) : endpoint;
                requestUri = new Uri(new Uri(normalizedBaseUrl), normalizedEndpoint);
                return true;
            }
            catch
            {
                requestUri = null!;
                return false;
            }
        }
        private static bool TryExtractPayload(string payloadJson, out string normalizedPayload)
        {
            normalizedPayload = string.Empty;
            try
            {
                using var document = JsonDocument.Parse(payloadJson);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                if (document.RootElement.TryGetProperty("payload", out var payloadElement))
                {
                    normalizedPayload = payloadElement.GetRawText();
                    return true;
                }

                normalizedPayload = document.RootElement.GetRawText();
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
        private static int? ResolveAutoApprovalRoleId(InboxEntry entry)
        {
            if (entry.AllocatedToRole.HasValue)
            {
                return entry.AllocatedToRole.Value;
            }
            return null;
        }
        private async Task<List<int>> ResolveManagerHierarchyRolesAsync(DbConnection conn, string managerRolesSql, InboxEntry entry)
        {
            if (entry.CreatedBy == 0)
            {
                _logger.LogWarning("Inbox entry SrNo={SrNo} missing CreatedBy; cannot resolve manager hierarchy.", entry.SrNo);
                return new List<int>();
            }

            var managerId = (await conn.QueryAsync<ManagerHierarchyRole>(
                managerRolesSql,
                new { orgId = entry.OrgId, userId = entry.CreatedBy }))
                .OrderBy(r => r.Level)
                .Select(r => r.RoleId)
                .FirstOrDefault();

            if (managerId <= 0)
            {
                return new List<int>();
            }

            return new List<int> { managerId, managerId, managerId };
        }
        private static List<int> ResolveCustomApproverRoles(InboxFieldConfig config)
        {
            var roles = new List<int>();

            if (config.ApproverOneId.HasValue && config.ApproverOneId.Value > 0)
            {
                roles.Add(config.ApproverOneId.Value);
            }
            if (config.ApproverTwoId.HasValue && config.ApproverTwoId.Value > 0)
            {
                roles.Add(config.ApproverTwoId.Value);
            }
            if (config.ApproverThreeId.HasValue && config.ApproverThreeId.Value > 0)
            {
                roles.Add(config.ApproverThreeId.Value);
            }

            return roles;
        }
    }
}
