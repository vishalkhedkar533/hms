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

            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            int userId = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier) ?? "0");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var inboxList = (from i in _context.Inbox
                                 join sra in _context.SrApprovers
                                     on new { i.SrNo, i.OrgId } equals new { sra.SrNo, sra.OrgId }
                                 join u in _context.Users
                                     on new { UserId = i.CreatedBy, i.OrgId } equals new { u.UserId, OrgId = u.OrgId ?? 0 }
                                 where (int)i.SrStatus == 2 &&
                                       _context.UserRoleMappings.Any(urm =>
                                           urm.RoleId == sra.AllocatedRoleId &&
                                           urm.UserId == userId)
                                 select new { i, u.Username }).ToList();

                // Map the username into the Inbox object's NotMapped property 
                // (Assuming you added CreatedByUsername to the Inbox class as well)
                var result = inboxList.Select(x =>
                {
                    x.i.CreatedByUsername = x.Username; // Assign the joined username
                    return x.i;
                }).ToList();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = (result.Count == 0 ?
                    "No Service Requests found" : $"{result.Count} Service Request(s) fetched successfully");

                // This will now work as 'result' is a List<Inbox>
                response.responseBody.InboxData = result;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the Service Request");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "An error occurred while fetching Service Requests.";
                return BadRequest(response);
            }

        }

    }
}