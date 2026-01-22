using AutoMapper;
using CommonLibrary;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.DTO.CommissionMgmt.Dashboard;
using Models.HMSConsts;
using System.ComponentModel;
using MiniExcelLibs;

namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommissionMgmtController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IAuthClaimService _authClaimService;
        private readonly IMapper _mapper;
        private readonly ILogger<CommissionMgmtController> _logger;
        private readonly DatabaseService _db;


        public CommissionMgmtController(
            HMSContext context,
            Microsoft.Extensions.Configuration.IConfiguration config,
            IAuthClaimService authClaimService,
            IMapper mapper,
            ILogger<CommissionMgmtController> logger,DatabaseService databaseService)
        {
            _context = context;
            _config = config;
            _authClaimService = authClaimService;
            _mapper = mapper;
            _logger = logger;
            _db    = databaseService;
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

                commissionDashboardDto.IndividualCommissions = await _context.IndividualCommissions
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

                commissionDashboardDto.CycleCommissions = await _context.CommissionCycles
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

                commissionDashboardDto.AdhocCommissions = await _context.AdhocCommissions
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

                commissionDashboardDto.PerformanceSnapshot = await _context.PerformanceSnapshots
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
                                                    RevenueAmount = x.RevenueAmount,
                                                    CommissionAmount = x.CommissionAmount,
                                                    Percentage = x.Percentage
                                                })
                                                .ToListAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.commissionMgmtDashboards = new List<CommissionMgmtDashboardDto> { commissionDashboardDto };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard API failed at {UtcNow} OrgId={OrgId} Message={Message}", DateTime.UtcNow, orgId, ex.Message);

                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("ProcessCommission")]
        [Authorize]
        [MenuAuthorize(1001)]
        public IActionResult GetProcessCommissionLog([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            HmsResponse response = new HmsResponse();
            int orgId = 0;
            try
            {
                orgId = Convert.ToInt32(
                _authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"
            );

                var allLogs = new List<ProcessCommissionLogDto>
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
                };

                var totalRecords = allLogs.Count;
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                var paged = allLogs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var processCommissionData = new ProcessCommissionResponseDto
                {
                    OrgId = orgId,
                    PeriodType = "Monthly",
                    ProcessedRecordsLog = paged
                };

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.processCommission = processCommissionData;
                response.responseBody.pagination = new { PageNumber = pageNumber, PageSize = pageSize, TotalRecords = totalRecords, TotalPages = totalPages };
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
        public IActionResult GetHoldCommission([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            HmsResponse response = new HmsResponse();
            int orgId = 0;

            try
            {
                orgId = Convert.ToInt32(
                    _authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"
                );

                var allRecords = new List<HoldCommissionRecordDto>
                {
                    new HoldCommissionRecordDto
                    {
                        HoldId = 201,
                        AgentName = "Ramesh Yadav",
                        Reason = "Incorrect Slab",
                        Amount = 22300,
                        HeldOn = new DateTime(2025, 7, 12),
                        Status = "On Hold",
                        CanRelease = true
                    },
                    new HoldCommissionRecordDto
                    {
                        HoldId = 202,
                        AgentName = "Mohan Pratap",
                        Reason = "Documentation Issue",
                        Amount = 18300,
                        HeldOn = new DateTime(2025, 7, 12),
                        Status = "Released",
                        CanRelease = false
                    }
                };

                var totalRecords = allRecords.Count;
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                var paged = allRecords.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var holdCommissionData = new HoldCommissionResponseDto
                {
                    OrgId = orgId,
                    AmountOnHold = 245890,
                    CurrentlyOnHold = 20,
                    ReleasedThisMonth = 11,
                    Records = paged
                };

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.holdCommission = holdCommissionData;
                response.responseBody.pagination = new { PageNumber = pageNumber, PageSize = pageSize, TotalRecords = totalRecords, TotalPages = totalPages };

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
        public IActionResult GetAdjustCommission([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            HmsResponse response = new HmsResponse();
            int orgId = 0;

            try
            {
                orgId = Convert.ToInt32(
                    _authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"
                );

                var allRecords = new List<AdjustCommissionLogDto>
                {
                    new AdjustCommissionLogDto
                    {
                        AdjustmentId = 301,
                        Date = new DateTime(2025, 5, 12),
                        Period = "May 6",
                        AdjustmentType = "TDS",
                        UploadedBy = "Rakesh",
                        RecordsCount = 1250,
                        Status = "Rejected"
                    },
                    new AdjustCommissionLogDto
                    {
                        AdjustmentId = 302,
                        Date = new DateTime(2025, 5, 11),
                        Period = "May 13",
                        AdjustmentType = "Commission",
                        UploadedBy = "Mahesh",
                        RecordsCount = 2250,
                        Status = "Approved"
                    }
                };

                var totalRecords = allRecords.Count;
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                var paged = allRecords.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var adjustCommissionData = new AdjustCommissionResponseDto
                {
                    OrgId = orgId,
                    Approved = 32,
                    PendingReview = 24,
                    Rejected = 20,
                    TotalRecords = 76,
                    Records = paged
                };

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.adjustCommission = adjustCommissionData;
                response.responseBody.pagination = new { PageNumber = pageNumber, PageSize = pageSize, TotalRecords = totalRecords, TotalPages = totalPages };

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
        public IActionResult GetApproveCommission([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            HmsResponse response = new HmsResponse();
            int orgId = 0;

            try
            {
                orgId = Convert.ToInt32(
                    _authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"
                );

                var allRecords = new List<ApproveCommissionLogDto>
                {
                    new ApproveCommissionLogDto
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
                    new ApproveCommissionLogDto
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
                };

                var totalRecords = allRecords.Count;
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                var paged = allRecords.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var approveCommissionData = new ApproveCommissionResponseDto
                {
                    OrgId = orgId,
                    TotalAmountApproved = 245890,
                    TotalRecords = 431,
                    PendingApproval = 2,
                    Records = paged
                };

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.approveCommission = approveCommissionData;
                response.responseBody.pagination = new { PageNumber = pageNumber, PageSize = pageSize, TotalRecords = totalRecords, TotalPages = totalPages };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApproveCommission API failed OrgId={OrgId}", orgId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("ProcessCommissions")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> ProcessCommissionRecord([FromBody] PaginationRequest paginationRequest)
        {
            HmsResponse response = new HmsResponse();

            paginationRequest.PageNumber = paginationRequest.PageNumber <= 0 ? 1 : paginationRequest.PageNumber;
            paginationRequest.PageSize = paginationRequest.PageSize <= 0 ? 10 : paginationRequest.PageSize;

            try
            {
                int orgId = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                var results = await _db.ExecuteQueryAsync<ProcessCommissionDTO>(
                    "Commission",
                    "GetProcessComissionList",
                    new
                    {
                        p_orgid = orgId,
                        p_page_number = paginationRequest.PageNumber,
                        p_page_size = paginationRequest.PageSize
                    });

                var processList = results?.ToList() ?? new List<ProcessCommissionDTO>();

                if (processList.Any())
                {
                    int totalItems = processList.First().TotalCount;

                    response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    response.responseHeader.ErrorMessage = "SUCCESS";
                    response.responseBody.processCommissionList = processList;
                    response.responseBody.pagination = new
                    {
                        currentPage = paginationRequest.PageNumber,
                        totalPages = (int)Math.Ceiling(totalItems / (double)paginationRequest.PageSize),
                        pageSize = paginationRequest.PageSize,
                        totalItems = totalItems
                    };

                    return Ok(response);
                }
                else
                {
                    response.responseHeader.ErrorCode = CommissionConstants.COMMISSION_NOTFOUND;
                    response.responseHeader.ErrorMessage = await _context.errorMaster
                        .Where(x => x.ErrorId == CommissionConstants.COMMISSION_NOTFOUND
                                 && x.Area == "CommissionConstants")
                        .Select(x => x.ErrorMsg)
                        .FirstOrDefaultAsync() ?? "Undefined Error Message";

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch process commission list");
                return StatusCode(500, "Internal server error");
            }
        }

        private byte[] GenerateCommissionExcel(List<ProcessCommissionExcelDTO> data)
        {
            var excelData = data.Select(x => new
            {
                Agent_Id = x.AgentId,
                Premium_Collection_Id = x.PremiumCollectionId,
                Premium_Amount = x.PremiumAmount,
                Formula = x.Formula,
                Commission_Amount = x.CommissionAmount,
                Status = x.Status,
                Logs = x.Logs
            });

            using var stream = new MemoryStream();
            stream.SaveAs(excelData);
            return stream.ToArray();
        }

        [HttpGet("DownloadCommissionExcel/{jobExeHistId}")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> DownloadCommissionExcel(int jobExeHistId)
        {
            int orgId = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            try
            {
                var data = (await _db.ExecuteQueryAsync<ProcessCommissionExcelDTO>(
                "Commission",
                "GetProcessCommissionExcel",
                new
                {
                    p_orgid = orgId,
                    p_job_exe_hist_id = jobExeHistId
                }
            ))?.ToList() ?? new List<ProcessCommissionExcelDTO>();

                if (!data.Any())
                    return NoContent();

                var fileBytes = GenerateCommissionExcel(data);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Commission_{jobExeHistId}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download file");
                return StatusCode(500, "Internal server error");
            }
            
        }
    }
}
