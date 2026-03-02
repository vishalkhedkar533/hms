using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommonLibrary;
using HMS.Caching;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.Enums;
using Models.HMSConsts;
using System.Linq;
using System.Net.Http.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Claims;

namespace HMS.Controllers
{
    public class UserInboxController : Controller
    {
        private const string SrStatusEntryCategory = "SR_STATUS";
        private readonly GenericCacheService _cacheService;
        private readonly IConfiguration _configuration;
        private readonly IAuthClaimService _authClaimService;
        private readonly FileService _fileService;
        private readonly HMSContext _context;
        private readonly ILogger<UserInboxController> _logger;
        private readonly DatabaseService _db;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private int orgId;

        public UserInboxController(HMSContext context, GenericCacheService cacheService, IConfiguration configuration
                    , IAuthClaimService authClaimService, FileService fileService, ILogger<UserInboxController> logger,
                    DatabaseService db, IMapper mapper, IHttpClientFactory httpClientFactory)
        {
            _cacheService = cacheService;
            _configuration = configuration;
            _authClaimService = authClaimService;
            _fileService = fileService;
            int refreshInterval = _configuration.GetValue<int>("Caching:refreshIntervalMinutes", 15);
            _context = context;
            _logger = logger;
            _db = db;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("CreateSr")]
        [MenuAuthorize(AuthorisationConstants.CreateAgentUpdateSR)]
        public async Task<IActionResult> CreateServiceRequest([FromBody] InboxDto inboxDto)
        {
            var response = new HmsResponse();

            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            int userId = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier) ?? "0");

            if (inboxDto == null)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Inbox Details Needed.";
                return BadRequest(response);
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!Enum.IsDefined(typeof(SrStatus), inboxDto.SrStatus))
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Invalid Service Request status.";
                return BadRequest(response);
            }

            var srStatusEntries = await _db.ExecuteQueryAsync<KeyValueEntry>(
                "Master",
                "getKeyValueEntries",
                new
                {
                    orgid = orgId,
                    EntryCategory = SrStatusEntryCategory
                });

            var isValidSrStatus = srStatusEntries.Any(entry =>
                entry.EntryIdentity == (int)inboxDto.SrStatus && (entry.ActiveStatus ?? true));

            if (!isValidSrStatus)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Service Request status is not valid for this organisation.";
                return BadRequest(response);
            }

            if (inboxDto.ComponentId == null)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Component Id is required.";
                return BadRequest(response);
            }

            var isValidComponentId = await _context.uiComponent
                .AsNoTracking()
                .AnyAsync(component => component.ComponentId == inboxDto.ComponentId);

            if (!isValidComponentId)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Invalid Component Id.";
                return BadRequest(response);
            }

            try
            {
                var inboxEntry = _mapper.Map<Inbox>(inboxDto);

                // Manually set fields ignored by AutoMapper (Security/Audit fields)
                inboxEntry.OrgId = orgId;
                inboxEntry.CreatedBy = userId;
                inboxEntry.CreatedDate = DateTime.UtcNow;
                // StatusUpdatedBy and StatusModifiedOn stay null for a new record

                await _context.Inbox.AddAsync(inboxEntry);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "Service Request created successfully.";
                response.responseBody.InboxData = new List<Inbox> { inboxEntry };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the Service Request");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("FetchSr")]
        [MenuAuthorize(AuthorisationConstants.FetchSRs)]
        public async Task<IActionResult> FetchServiceRequest([FromBody] SearchInboxDto searchInboxDto)
        {
            var response = new HmsResponse();

            // Get claims
            int orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            int userId = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier) ?? "0");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var srStatusEntries = await _db.ExecuteQueryAsync<KeyValueEntry>(
                "Master",
                "getKeyValueEntries",
                new
                {
                    orgid = orgId,
                    EntryCategory = "SR_STATUS"
                });

            // materialize and prepare lookup for descriptions
            var srStatusList = srStatusEntries.ToList();
            var srStatusIds = srStatusList.Select(e => e.EntryIdentity).ToList();
            var srStatusDict = srStatusList.ToDictionary(e => e.EntryIdentity, e => e.EntryDesc);

            try
            {
                // 1. Build the base query (IQueryable) — do not join to the in-memory collection
                var query = from i in _context.Inbox.AsNoTracking()
                            join sra in _context.SrApprovers
                                on new { i.SrNo, i.OrgId } equals new { sra.SrNo, sra.OrgId }
                            join u in _context.Users.AsNoTracking()
                                on new { UserId = i.CreatedBy, i.OrgId } equals new { u.UserId, OrgId = u.OrgId ?? 0 }
                            where i.OrgId == orgId && // Ensure tenant isolation
                                  (int)i.SrStatus == 2 &&
                                  srStatusIds.Contains((int)i.SrStatus) &&
                                  _context.UserRoleMappings.AsNoTracking().Any(urm =>
                                      urm.RoleId == sra.AllocatedRoleId &&
                                      urm.UserId == userId)
                            select new { i, u.Username };

                // 2. Apply Dynamic Filters from SearchInboxDto
                if (searchInboxDto.SrNo.HasValue)
                    query = query.Where(x => x.i.SrNo == searchInboxDto.SrNo.Value);

                if (searchInboxDto.CreatedDateFrom.HasValue)
                    query = query.Where(x => x.i.CreatedDate >= searchInboxDto.CreatedDateFrom.Value);

                if (searchInboxDto.CreatedDateTo.HasValue)
                    query = query.Where(x => x.i.CreatedDate <= searchInboxDto.CreatedDateTo.Value);

                // 3. Get Total Count for Pagination Metadata (Before Skip/Take)
                int totalRecords = await query.CountAsync();

                // 4. Apply Pagination
                int pageNo = searchInboxDto.PageNo ?? 1;
                int pageSize = searchInboxDto.PageSize ?? 10;

                var pagedData = await query
                    .OrderByDescending(x => x.i.CreatedDate) // Always order before paging
                    .Skip((pageNo - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 5. Map to Model and include Username and SrStatusDesc
                var result = pagedData.Select(x =>
                {
                    x.i.CreatedByUsername = x.Username;
                    x.i.SrStatusDesc = srStatusDict.TryGetValue((int)x.i.SrStatus, out var desc) ? desc : null;
                    return x.i;
                }).ToList();

                // 6. Finalize Response
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = (result.Count == 0 ?
                    "No Service Requests found" : $"{totalRecords} Service Request(s) found");

                response.responseBody.InboxData = result;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the Service Requests");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "An error occurred while fetching Service Requests.";
                return BadRequest(response);
            }
        }


        [HttpPost("UpdateSrDecision")]
        [MenuAuthorize(AuthorisationConstants.UpdateSRDecision)]
        public async Task<IActionResult> UpdateSrDecision([FromBody] SrApproverDto  srApproverDto)
        {
            var response = new HmsResponse();

            // Get claims
            int orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            int userId = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier) ?? "0");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var approvalPendingWith = _context.SrApprovers
                    .Where(sa => sa.SrNo == srApproverDto.SrNo && sa.OrgId == orgId)
                    .OrderBy(sa => sa.ApproverLevel);

                var inbox = await _context.Inbox.FirstOrDefaultAsync(i => i.SrNo == srApproverDto.SrNo && i.OrgId == orgId);

                if (inbox ==  null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Service Request not found.";
                    return BadRequest(response);
                }

                if (approvalPendingWith?.Any(x=> x.ApproverDecision == null) ?? false)// pending approval
                {
                    var currentApprover = approvalPendingWith.FirstOrDefault(x => x.ApproverDecision == null);
                    var currentApproverLevel = currentApprover?.ApproverLevel ?? 0;
                    var nextApprover = approvalPendingWith
                        .Where(x => x.ApproverLevel > currentApproverLevel)
                        .OrderBy(x => x.ApproverLevel)
                        .FirstOrDefault();
                    if (currentApprover != null)
                    {
                        currentApprover.ApproverDecision = srApproverDto.ApproverDecision;
                        currentApprover.DecisionOn = DateTime.UtcNow;
                        currentApprover.DecisionBy = userId;
                        //var approverEntity = _mapper.Map<SrApprover>(currentApprover);
                        inbox.StatusUpdatedBy = userId;
                        inbox.StatusModifiedOn = DateTime.UtcNow;


                        if (nextApprover != null)
                        {
                            //allocate to next approver
                            switch (srApproverDto.ApproverDecision)
                            {
                                case ApproverDecision.Rejected:
                                    //since current user has rejected, we can move the status to rejected and no more approver can take action
                                    inbox.SrStatus = SrStatus.Rejected; // Rejected
                                    break;
                                default:
                                    //case ApproverDecision.Approved:
                                    //case ApproverDecision.OnHold:
                                    inbox.SrStatus = SrStatus.PendingDecision; // Rejected
                                    break;
                            }
                            inbox.AllocatedToRole = nextApprover.AllocatedRoleId;
                        }
                        else
                        {
                            switch (srApproverDto.ApproverDecision)
                            {
                                case ApproverDecision.Approved:
                                    inbox.SrStatus = SrStatus.Approved; // Rejected
                                    break;
                                case ApproverDecision.Rejected:
                                    //since current user has rejected, we can move the status to rejected and no more approver can take action
                                    inbox.SrStatus = SrStatus.Rejected; // Rejected
                                    break;
                                default:
                                    //case ApproverDecision.OnHold:
                                    inbox.SrStatus = SrStatus.PendingDecision; // Rejected
                                    break;
                            }
                            //no more approver, mark as completed

                        }
                        await _context.SaveChangesAsync();

                        response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                        response.responseHeader.ErrorMessage = "Service Request decision updated successfully.";
                    }
                }
                else 
                {
                    switch (srApproverDto.ApproverDecision)
                    {
                        case ApproverDecision.Approved:
                            inbox.SrStatus = SrStatus.Approved; // Rejected
                            break;
                        case ApproverDecision.Rejected:
                            //since current user has rejected, we can move the status to rejected and no more approver can take action
                            inbox.SrStatus = SrStatus.Rejected; // Rejected
                            break;
                        default:
                            //case ApproverDecision.OnHold:
                            inbox.SrStatus = SrStatus.PendingDecision; // Rejected
                            break;
                    }
                    await _context.SaveChangesAsync();
                    response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    response.responseHeader.ErrorMessage = "Service Request decision updated successfully.";
                }
                if (inbox.SrStatus == SrStatus.Approved)
                {
                    //invoke approvalendpoint + approvalpayload
                    await InvokeApprovalEndpointAsync(inbox);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the Service Request decision");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "An error occurred while updating the Service Request decision.";
                return BadRequest(response);
            }
        }

        private async Task InvokeApprovalEndpointAsync(Inbox inbox)
        {
            if (string.IsNullOrWhiteSpace(inbox.ApprovalEndpoint) || string.IsNullOrWhiteSpace(inbox.ApprovalPayload))
            {
                return;
            }

            if (!TryParseApprovalEndpoint(inbox.ApprovalEndpoint, out var agentId, out var sectionName))
            {
                _logger.LogWarning("Approval endpoint format not recognized: {Endpoint}", inbox.ApprovalEndpoint);
                return;
            }

            var agentDto = BuildAgentDtoFromPayload(inbox.ApprovalPayload);
            if (agentDto == null)
            {
                _logger.LogWarning("Approval payload could not be parsed for SR {SrNo}", inbox.SrNo);
                return;
            }

            var client = _httpClientFactory.CreateClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var targetUrl = new Uri(new Uri(baseUrl), $"/api/Agent/UpdateAgentAfterApproval/{agentId}/{sectionName}");

            using var request = new HttpRequestMessage(HttpMethod.Post, targetUrl)
            {
                Content = JsonContent.Create(agentDto)
            };

            if (Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                request.Headers.TryAddWithoutValidation("Authorization", (string)authHeader);
            }

            var response = await client.SendAsync(request);
            inbox.ApprovalApiResponse = await response.Content.ReadAsStringAsync();
            await _context.SaveChangesAsync();
        }

        private static bool TryParseApprovalEndpoint(string endpoint, out int agentId, out string sectionName)
        {
            agentId = 0;
            sectionName = string.Empty;

            var segments = endpoint.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            var index = Array.FindIndex(segments, s => s.Equals("UpdateAgentAfterApproval", StringComparison.OrdinalIgnoreCase));
            if (index < 0 || segments.Length <= index + 2)
            {
                return false;
            }

            if (!int.TryParse(segments[index + 1], out agentId))
            {
                return false;
            }

            sectionName = segments[index + 2];
            return !string.IsNullOrWhiteSpace(sectionName);
        }

        private static AgentDto? BuildAgentDtoFromPayload(string payloadJson)
        {
            JObject? root;
            try
            {
                root = JObject.Parse(payloadJson);
            }
            catch
            {
                return null;
            }

            var payloadToken = root["payload"] as JObject;
            if (payloadToken == null)
            {
                return null;
            }

            var agentDto = new AgentDto();
            var properties = typeof(AgentDto)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            foreach (var prop in payloadToken.Properties())
            {
                if (!properties.TryGetValue(prop.Name, out var propInfo))
                {
                    continue;
                }

                var rawValue = prop.Value.Type == JTokenType.Null ? null : prop.Value.ToString();
                var converted = ConvertToType(rawValue, propInfo.PropertyType);
                if (converted != null || Nullable.GetUnderlyingType(propInfo.PropertyType) != null || propInfo.PropertyType == typeof(string))
                {
                    propInfo.SetValue(agentDto, converted);
                }
            }

            return agentDto;
        }

        private static object? ConvertToType(string? value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType == typeof(string))
            {
                return value;
            }

            if (underlyingType == typeof(int))
            {
                return int.TryParse(value, out var parsed) ? parsed : null;
            }

            if (underlyingType == typeof(long))
            {
                return long.TryParse(value, out var parsed) ? parsed : null;
            }

            if (underlyingType == typeof(bool))
            {
                if (bool.TryParse(value, out var parsed))
                {
                    return parsed;
                }

                if (int.TryParse(value, out var intValue))
                {
                    return intValue != 0;
                }

                return null;
            }

            if (underlyingType == typeof(DateTime))
            {
                return DateTime.TryParse(value, out var parsed) ? parsed : null;
            }

            if (underlyingType == typeof(decimal))
            {
                return decimal.TryParse(value, out var parsed) ? parsed : null;
            }

            if (underlyingType == typeof(double))
            {
                return double.TryParse(value, out var parsed) ? parsed : null;
            }

            try
            {
                return Convert.ChangeType(value, underlyingType);
            }
            catch
            {
                return null;
            }
        }

    }
}