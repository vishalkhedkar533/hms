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
        [MenuAuthorize(AuthorisationConstants.GetCommissionData)]
        public async Task<ActionResult<HmsResponse>> GetGraphData([FromBody] SearchRevenueCommDto searchRevenueCommDto)
        {
            var response = new HmsResponse();
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            LoggedInUserId = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier));

            try
            {
                string RangeType = string.Empty;
                switch (searchRevenueCommDto.GroupBy)
                {
                    case Models.Enums.GroupDataByPeriod.ByMonth:
                        RangeType = "MonthPeriod";
                        break;
                    case Models.Enums.GroupDataByPeriod.ByQuarter:
                        RangeType = "QuarterPeriod";
                        break;
                    default:
                        RangeType = "MonthPeriod"; //defaulting to month period if no group by is sent, as graph needs to be shown in x and y axis and for that we need period data
                        break;
                }

                var organizationPeriod = await _context.OrganizationPeriods.AsNoTracking()
                    .Where(x => x.OrgId == orgId 
                    && x.StartDate >= searchRevenueCommDto.StartDate
                    && x.EndDate <= searchRevenueCommDto.EndDate
                    && x.RangeType == (string.IsNullOrEmpty(RangeType) ? x.RangeType : RangeType))
                    .OrderByDescending(x => x.CreatedAt).Select( x=> x.PeriodId).ToListAsync();

                if (organizationPeriod == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = $"Month/Quarter Period not defined for {searchRevenueCommDto.StartDate} and {searchRevenueCommDto.EndDate}";
                    return Conflict(response);
                }

                var graphDataByPeriod = _context.RevenueComms.AsNoTracking()
                    .Where(x =>  organizationPeriod.Contains(x.PeriodId)
                    && x.OrgId == orgId)
                    .Select(y => new GraphDataByPeriod {
                        commission = y.Commission, 
                        revenue = y.Revenue ,
                        date = y.Period.StartDate}).ToList();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.GraphRevenueCommData = new GraphRevenueCommDto() { filtertype =  RangeType  , Data = graphDataByPeriod };
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
