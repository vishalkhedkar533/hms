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
using Models.Enums;
using Models.HMSConsts;
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
    }
}