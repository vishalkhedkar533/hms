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
    public class RevenueCommController : Controller
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        //private readonly DatabaseService _db;
        private readonly IAuthClaimService _authClaimService;
        private readonly GenericCacheService _cacheService;
        private int refreshInterval = 15;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RevenueCommController> _logger;
        private int orgId;
        private int LoggedInUserId;
        public RevenueCommController(HMSContext context, ILogger<RevenueCommController> logger, IAuthClaimService authClaimService,
            IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _authClaimService = authClaimService;
            //_db = db;
            _mapper = mapper;
        }
        [HttpPost("GetGraphData")]
        [MenuAuthorize(AuthorisationConstants.ReadMasters)]
        public async Task<ActionResult<HmsResponse>> GetGraphData()
        {
            var response = new HmsResponse();
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            LoggedInUserId = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier));
            try
            {
                var roles = await _context.Roles.Where(x => x.OrgId == orgId).AsNoTracking().ToListAsync();
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.roles = roles;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch roles");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Internal server error.";
                return BadRequest(response);
            }
        }
    }
}
