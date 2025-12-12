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

        public CommissionMgmtController(
            HMSContext context,
            IConfiguration config,
            IAuthClaimService authClaimService,
            IMapper mapper)
        {
            _context = context;
            _config = config;
            _authClaimService = authClaimService;
            _mapper = mapper;
        }

        [HttpPost("Dashboard")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<ActionResult<CommissionMgmtDashboardDto>> Dashboard([FromBody] FetchComssDashboard fetchComssDashboard)
        {
            HmsResponse response = new HmsResponse();

            int orgId = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            if (orgId != fetchComssDashboard.orgId)
                return Unauthorized("Invalid organization claim.");

            var dbDashboard = await _context.CommissionMgmtDashboards
                                .Where(x => x.orgId == orgId)
                                .FirstOrDefaultAsync();

            if (dbDashboard == null)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "No dashboard data";
                return NotFound(response);
            }

            var dashboardDto = _mapper.Map<CommissionMgmtDashboardDto>(dbDashboard);

            dashboardDto.IndividualCommissions = await _context.IndividualCommissions
                                                .Where(x => x.OrgId == orgId)
                                                .Select(x => new IndividualCommissionDto
                                                {
                                                    CommissionId = x.IndividualCommissionId,
                                                    OrgId=x.OrgId,
                                                    AgentId = x.AgentId,
                                                    AgentCode = x.AgentCode,
                                                    AgentName = x.AgentName,
                                                    Status = x.Status,
                                                    SubmittedOn = x.SubmittedOn,
                                                    SubmittedBy = x.SubmittedBy.HasValue ? x.SubmittedBy.Value.ToString() : null
                                                })
                                                .ToListAsync();

            dashboardDto.CycleCommissions = await _context.CommissionCycles
                                                .Where(x => x.OrgId == orgId)
                                                .Select(x => new Models.DTO.CycleCommissionDto
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

            dashboardDto.AdhocCommissions = await _context.AdhocCommissions
                                                .Where(x => x.OrgId == orgId)
                                                .Select(x => new Models.DTO.AdhocCommissionDto
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

            dashboardDto.PerformanceSnapshot = await _context.PerformanceSnapshots
                                                .Where(x => x.OrgId == orgId)
                                                .Select(x => new Models.DTO.PerformanceSnapshotDto 
                                                {
                                                    OrgId = x.OrgId,
                                                    SnapshotId = x.SnapshotId,
                                                    PeriodFrom = x.PeriodFrom,
                                                    PeriodTo = x.PeriodTo,
                                                    CommissionBudget = x.CommissionBudget,
                                                    CommissionActual = x.CommissionActual
                                                })
                                                .ToListAsync();
            //var individualList = await _context.IndividualCommissions
            //                    .Where(x => x.OrgId == orgId)
            //                    .ToListAsync();
            //dashboardDto.IndividualCommissions = _mapper.Map<List<IndividualCommissionDto>>(individualList);
            response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
            response.responseHeader.ErrorMessage = "SUCCESS";
            response.responseBody.commissionMgmtDashboards = new List<CommissionMgmtDashboardDto>
            {
                dashboardDto
            };

            return Ok(response);
        }
    }
}
