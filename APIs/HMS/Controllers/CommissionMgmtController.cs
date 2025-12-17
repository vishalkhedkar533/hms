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
        public async Task<ActionResult<CommissionMgmtDashboardDto>> Dashboard()
        {
            HmsResponse response = new HmsResponse();
            int orgId = 0;
            try
            {
                orgId = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

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
                                                    NbCommission = x.NbCommission,
                                                    Status = x.Status,
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
                                                    CommissionDate = (DateTime)x.SubmittedOn,
                                                    Status = x.Status,
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
                _logger.LogError(ex,"Dashboard API failed at {UtcNow} OrgId={OrgId} Message={Message}",DateTime.UtcNow,orgId,ex.Message);

                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("ProcessCommission")]
        [Authorize]
        [MenuAuthorize(1001)]
        public IActionResult GetProcessCommissionLog()
        {
            HmsResponse response = new HmsResponse();
            int orgId = 0;
            try
            {
                orgId = Convert.ToInt32(
                _authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"
            );

                var processCommissionData = new ProcessCommissionResponseDto
                {
                    OrgId = orgId,
                    PeriodType = "Monthly",
                    ProcessedRecordsLog = new List<ProcessCommissionLogDto>
                    {
                        new ProcessCommissionLogDto
                        {
                            ProcessId=101,
                            ProcessedDate = new DateTime(2025, 5, 12),
                            Period = "May 6",
                            RecordsCount = 1250,
                            Status = "In Process",
                            CanViewDetails = true,
                            CanDownload = false
                        },
                        new ProcessCommissionLogDto
                        {
                            ProcessId=102,
                            ProcessedDate = new DateTime(2025, 5, 11),
                            Period = "May 13",
                            RecordsCount = 2250,
                            Status = "Completed",
                            CanViewDetails = false,
                            CanDownload = true
                        },
                        new ProcessCommissionLogDto
                        {
                            ProcessId=103,
                            ProcessedDate = new DateTime(2025, 5, 10),
                            Period = "Jun 25",
                            RecordsCount = 1152,
                            Status = "Completed",
                            CanViewDetails = false,
                            CanDownload = true
                        },
                        new ProcessCommissionLogDto
                        {
                            ProcessId=104,
                            ProcessedDate = new DateTime(2025, 5, 10),
                            Period = "Aug 5",
                            RecordsCount = 933,
                            Status = "Completed",
                            CanViewDetails = false,
                            CanDownload = true
                        },
                        new ProcessCommissionLogDto
                        {
                            ProcessId=105,
                            ProcessedDate = new DateTime(2025, 5, 9),
                            Period = "July 10",
                            RecordsCount = 825,
                            Status = "Completed",
                            CanViewDetails = false,
                            CanDownload = true
                        }
                    }
                };

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.processCommission = processCommissionData;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessCommission API failed OrgId={OrgId}", orgId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
