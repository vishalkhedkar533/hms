using AutoMapper;
using CommonLibrary;
using HMS.Data;
using HMS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.DTO.CommissionMgmt.Dashboard;
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

                commissionDashboardDto.CurrentBusinessCycles = await _context.CurrentBusinessCycles
                                                .Where(x => x.OrgId == orgId)
                                                .AsNoTracking()
                                                .Select(x => new CurrentBusinessCycleDto
                                                {
                                                    CurrentBusinessCycleId = x.CurrentBusinessCycleId,
                                                    OrgId = x.OrgId,
                                                    Cycle = x.CycleType,
                                                    RevenueAmount =  x.RevenueAmount,
                                                    CommissionAmount =  x.CommissionAmount,
                                                    Percentage =  x.Percentage
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

        [HttpPost("HoldCommission")]
        [Authorize]
        [MenuAuthorize(1002)]
        public IActionResult GetHoldCommission()
{
    HmsResponse response = new HmsResponse();
    int orgId = 0;

    try
    {
        orgId = Convert.ToInt32(
            _authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"
        );

        var holdCommissionData = new HoldCommissionResponseDto
        {
            OrgId = orgId,
            AmountOnHold = 245890,
            CurrentlyOnHold = 20,
            ReleasedThisMonth = 11,
            Records = new List<HoldCommissionRecordDto>
            {
                new ()
                {
                    HoldId = 201,
                    AgentName = "Ramesh Yadav",
                    Reason = "Incorrect Slab",
                    Amount = 22300,
                    HeldOn = new DateTime(2025, 7, 12),
                    Status = "On Hold",
                    CanRelease = true
                },
                new ()
                {
                    HoldId = 202,
                    AgentName = "Mohan Pratap",
                    Reason = "Documentation Issue",
                    Amount = 18300,
                    HeldOn = new DateTime(2025, 7, 12),
                    Status = "Released",
                    CanRelease = false
                }
            }
        };

        response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
        response.responseHeader.ErrorMessage = "SUCCESS";
        response.responseBody.holdCommission = holdCommissionData;

        return Ok(response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "HoldCommission API failed OrgId={OrgId}", orgId);
        return StatusCode(500, "Internal server error");
    }
}

        [HttpPost("AdjustCommission")]
        [Authorize]
        [MenuAuthorize(1001)]
        public IActionResult GetAdjustCommission()
        {
            HmsResponse response = new HmsResponse();
            int orgId = 0;

            try
            {
                orgId = Convert.ToInt32(
                    _authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"
                );

                var adjustCommissionData = new AdjustCommissionResponseDto
                {
                    OrgId = orgId,
                    Approved = 32,
                    PendingReview = 24,
                    Rejected = 20,
                    TotalRecords = 76,
                    Records = new List<AdjustCommissionLogDto>
            {
                new ()
                {
                    AdjustmentId = 301,
                    Date = new DateTime(2025, 5, 12),
                    Period = "May 6",
                    AdjustmentType = "TDS",
                    UploadedBy = "Rakesh",
                    RecordsCount = 1250,
                    Status = "Rejected"
                },
                new ()
                {
                    AdjustmentId = 302,
                    Date = new DateTime(2025, 5, 11),
                    Period = "May 13",
                    AdjustmentType = "Commission",
                    UploadedBy = "Mahesh",
                    RecordsCount = 2250,
                    Status = "Approved"
                }
            }
                };

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.adjustCommission = adjustCommissionData;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdjustCommission API failed OrgId={OrgId}", orgId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("ApproveCommission")]
        [Authorize]
        [MenuAuthorize(1001)]
        public IActionResult GetApproveCommission()
        {
            HmsResponse response = new HmsResponse();
            int orgId = 0;

            try
            {
                orgId = Convert.ToInt32(
                    _authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"
                );

                var approveCommissionData = new ApproveCommissionResponseDto
                {
                    OrgId = orgId,
                    TotalAmountApproved = 245890,
                    TotalRecords = 431,
                    PendingApproval = 2,
                    Records = new List<ApproveCommissionLogDto>
                    {
                        new ()
                        {
                            ApprovalId = 401,
                            Date = new DateTime(2025, 5, 8),
                            Period = "Aug 5",
                            SubmittedBy = "HO Finance",
                            Amount = 4290450,
                            Status = "Pending",
                            CanApprove = true,
                            CanDownload = false
                        },
                        new ()
                        {
                            ApprovalId = 402,
                            Date = new DateTime(2025, 5, 11),
                            Period = "May 13",
                            SubmittedBy = "Zone Office",
                            Amount = 2890450,
                            Status = "Approved",
                            CanApprove = false,
                            CanDownload = true
                        }
                    }
                };

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.approveCommission = approveCommissionData;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApproveCommission API failed OrgId={OrgId}", orgId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
