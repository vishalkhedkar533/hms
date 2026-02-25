using AutoMapper;
using CommonLibrary;
using HMS.Caching;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;
using System.Security.Claims;

namespace HMS.Controllers
{
    public class UserInboxController : Controller
    {
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
    }
}