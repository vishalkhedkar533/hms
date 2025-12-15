using AutoMapper;
using CommonLibrary;
using HMS.Data;
using HMS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;

namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommissionMgmtController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;
        private readonly IAuthClaimService _authClaimService;
        private readonly IMapper _mapper;
        private readonly ILogger<CommissionMgmtController> _logger;

        public CommissionMgmtController(
            HMSContext context,
            IConfiguration config,
            IAuthClaimService authClaimService,
            IMapper mapper,
            ILogger<CommissionMgmtController> logger)
        {
            _context = context;
            _config = config;
            _authClaimService = authClaimService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("Dashboard")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<ActionResult<CommissionMgmtDashboardDto>> Dashboard([FromBody] FetchComssDashboard fetchComssDashboard)
        {
            HmsResponse response = new HmsResponse();
            try
            {
                int orgId = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                if (orgId != fetchComssDashboard.orgId)
                {
                    _logger.LogWarning("Unauthorized org access.",orgId, fetchComssDashboard.orgId);
                    return Unauthorized("Invalid organization claim.");
                }

                var commissionDashboard = await _context.CommissionMgmtDashboards.FirstOrDefaultAsync(x => x.orgId == orgId);

                if (commissionDashboard == null)
                {
                    _logger.LogInformation("No dashboard data found for OrgId={OrgId}", orgId);
                    return NoContent();
                }

                var commissionDashboardDto = _mapper.Map<CommissionMgmtDashboardDto>(commissionDashboard);

                commissionDashboardDto.IndividualCommissions =await _context.IndividualCommissions
                                                    .Where(x => x.OrgId == orgId)
                                                    .AsNoTracking()
                                                    .Select(x => new IndividualCommissionDto
                                                    {
                                                        CommissionId = x.IndividualCommissionId,
                                                        OrgId = x.OrgId,
                                                        AgentId = x.AgentId,
                                                        AgentCode = x.AgentCode,
                                                        AgentName = x.AgentName,
                                                        Status = x.Status,
                                                        SubmittedOn = x.SubmittedOn,
                                                        SubmittedBy = x.SubmittedBy.HasValue
                                                            ? x.SubmittedBy.Value.ToString()
                                                            : null
                                                    })
                                                    .ToListAsync();

                commissionDashboardDto.CycleCommissions =await _context.CommissionCycles
                                                .Where(x => x.OrgId == orgId)
                                                .AsNoTracking()
                                                .Select(x => new CycleCommissionDto
                                                {
                                                    CycleId = x.CycleId,
                                                    CycleCode = x.CycleCode,
                                                    OrgId = x.OrgId,
                                                    CommissionType = x.CommissionType,
                                                    CountOfEntities = x.CountOfEntities,
                                                    AvgCommission = x.AvgCommission,
                                                    NbRevenue = x.NbRevenue,
                                                    NbCommission = x.NbCommission
                                                })
                                                .ToListAsync();

                commissionDashboardDto.AdhocCommissions =await _context.AdhocCommissions
                                                .Where(x => x.OrgId == orgId)
                                                .AsNoTracking()
                                                .Select(x => new AdhocCommissionDto
                                                {
                                                    AdhocCommissionId = x.AdhocCommissionId,
                                                    OrgId = (int)x.OrgId,
                                                    BranchId = (int)x.BranchId,
                                                    RequestId = x.RequestId,
                                                    SubmittedOn = x.SubmittedOn,
                                                    SubmittedBy = x.SubmittedBy,
                                                    CommissionDate = (DateTime)x.SubmittedOn
                                                })
                                                .ToListAsync();

                commissionDashboardDto.PerformanceSnapshot =await _context.PerformanceSnapshots
                                                .Where(x => x.OrgId == orgId)
                                                .AsNoTracking()
                                                .Select(x => new PerformanceSnapshotDto
                                                {
                                                    OrgId = x.OrgId,
                                                    SnapshotId = x.SnapshotId,
                                                    PeriodFrom = x.PeriodFrom,
                                                    PeriodTo = x.PeriodTo,
                                                    CommissionBudget = x.CommissionBudget,
                                                    CommissionActual = x.CommissionActual
                                                })
                                                .ToListAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.commissionMgmtDashboards =new List<CommissionMgmtDashboardDto> { commissionDashboardDto };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Dashboard API failed at {UtcNow} OrgId={OrgId} Message={Message}",DateTime.UtcNow,fetchComssDashboard?.orgId,ex.Message);

                return StatusCode(500, "Internal server error.");
            }
        }

    }
}
