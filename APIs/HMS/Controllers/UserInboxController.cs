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
        public async Task<IActionResult> CreateServiceRequest([FromRoute] int ChannelId, [FromBody] List<InboxDto> InboxDto)
        {
            var response = new HmsResponse();
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            if (InboxDto is null || !InboxDto.Any())
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (ChannelId != SubChannelMaster.ChannelId)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Channel ID in URL does not match Channel ID in body.";
                    return BadRequest(response);
                }

                var Channel = await _context.ChannelMaster.FirstOrDefaultAsync(x => x.ChannelId == ChannelId && x.OrgId == orgId);
                if (Channel == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid Channel ID";
                    return BadRequest(response);
                }
                // Use provided OrgId from DTO. If you want to pull from claims, adapt here.
                var subChannelMaster = await _context.SubchannelMaster.AddAsync(new SubChannelMaster
                {
                    SubChannelCode = SubChannelMaster.SubChannelCode,
                    ChannelId = Channel.ChannelId,
                    ChannelCode = Channel.ChannelCode,
                    SubChannelName = SubChannelMaster.SubChannelName,
                    Description = SubChannelMaster.Description,
                    IsActive = true,
                    OrgId = orgId,
                    CreatedBy = _authClaimService.GetClaim(ClaimTypes.NameIdentifier),
                    CreatedDate = DateTime.UtcNow,
                    RowVersion = 1,
                });
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = $"SubChannel master created successfully under Channel {Channel.ChannelName}";
                var result = await _context.SaveChangesAsync();
                response.responseBody.subChannels = new List<SubChannelMaster>();
                response.responseBody.subChannels.Add(subChannelMaster.Entity);
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = $"An error occurred while creating the sub channel master {SubChannelMaster.SubChannelName}";
                _logger.LogError(ex, response.responseHeader.ErrorMessage);
                return BadRequest(response);
            }
        }
    }
}