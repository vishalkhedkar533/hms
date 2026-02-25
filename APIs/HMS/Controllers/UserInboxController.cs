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
        private int orgId;

        public UserInboxController(HMSContext context, GenericCacheService cacheService, IConfiguration configuration
                    , IAuthClaimService authClaimService, FileService fileService, ILogger<UserInboxController> logger,
                    DatabaseService db, IMapper mapper)
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

            if (inboxDto.ControlId == null)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Control Id is required.";
                return BadRequest(response);
            }

            var isValidControlId = await _context.uiFieldsSettings
                .AsNoTracking()
                .AnyAsync(setting => setting.OrgId == orgId && setting.CntrlId == inboxDto.ControlId);

            if (!isValidControlId)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Invalid Control Id for this organisation.";
                return BadRequest(response);
            }

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

            try
            {
                // 1. Build the base query (IQueryable)
                var query = from i in _context.Inbox.AsNoTracking()
                            join sra in _context.SrApprovers
                                on new { i.SrNo, i.OrgId } equals new { sra.SrNo, sra.OrgId }
                            join u in _context.Users.AsNoTracking()
                                on new { UserId = i.CreatedBy, i.OrgId } equals new { u.UserId, OrgId = u.OrgId ?? 0 }
                            where i.OrgId == orgId && // Ensure tenant isolation
                                  (int)i.SrStatus == 2 &&
                                  _context.UserRoleMappings.AsNoTracking().Any(urm =>
                                      urm.RoleId == sra.AllocatedRoleId &&
                                      urm.UserId == userId)
                            select new { i, u.Username };

                // 2. Apply Dynamic Filters from SearchInboxDto
                if (searchInboxDto.CreatedDateFrom.HasValue)
                    query = query.Where(x => x.i.CreatedDate >= searchInboxDto.CreatedDateFrom.Value);

                if (searchInboxDto.CreatedDateTo.HasValue)
                    query = query.Where(x => x.i.CreatedDate <= searchInboxDto.CreatedDateTo.Value);

                // 3. Get Total Count for Pagination Metadata (Before Skip/Take)
                int totalRecords = await query.CountAsync();


                // 4. Apply Pagination
                int pageNo = searchInboxDto.PageNo ?? 1;
                int pageSize = searchInboxDto.PageSize ?? 10;

                var pagedData = await query.AsNoTracking()
                    .OrderByDescending(x => x.i.CreatedDate) // Always order before paging
                    .Skip((pageNo - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 5. Map to Model and include Username
                var result = pagedData.Select(x =>
                {
                    x.i.CreatedByUsername = x.Username;
                    return x.i;
                }).ToList();

                // 6. Finalize Response
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = (result.Count == 0 ?
                    "No Service Requests found" : $"{totalRecords} Service Request(s) found");

                response.responseBody.InboxData = result;
                // Optional: If your responseBody has space for metadata:
                // response.responseBody.TotalCount = totalRecords; 

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
                    response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    response.responseHeader.ErrorMessage = "Service Request decision updated successfully.";
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

    }
}