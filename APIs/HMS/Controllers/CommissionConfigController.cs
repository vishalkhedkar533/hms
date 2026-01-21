using CommonLibrary;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.DTO.CommissionMgmt;
using Models.HMSConsts;
using System.Security.Claims;

namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommissionConfigController : ControllerBase
    {
        private readonly IAuthClaimService _authClaimService;
        private readonly HMSContext _context;
        private readonly ILogger<CommissionConfigController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly DatabaseService _db;

        public CommissionConfigController(IAuthClaimService authClaimService,HMSContext hMSContext, ILogger<CommissionConfigController> logger, IWebHostEnvironment env,DatabaseService databaseService)
        {
            _authClaimService = authClaimService;
            _context = hMSContext;
            _logger = logger;
            _env = env;
            _db = databaseService;
        }

        [HttpPost("GetCommissionById/{commissionConfigId}")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> GetCommissionById(int commissionConfigId)
        {
            HmsResponse response = new HmsResponse();
            int orgId = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

            try
            {
                var data = await (
                    from cc in _context.CommissionConfigs.AsNoTracking()
                    join jc in _context.JobConfigs.AsNoTracking()
                        on cc.JobConfigId equals jc.JobConfigId
                    where cc.CommissionConfigId == commissionConfigId && cc.OrgId == orgId
                    select new CommissionConfigDTO
                    {
                        // Step 1: Basic Info
                        CommissionConfigId = cc.CommissionConfigId,
                        CommissionName = jc.JobName,
                        RunFrom = jc.StartAt,
                        RunTo = jc.EndAt,

                        // Step 2: Formula
                        Formula = cc.Formula,

                        // Step 3: Schedule Configuration
                        JobConfigId = cc.JobConfigId,
                        JobType = jc.JobType,
                        JobName = jc.JobName,
                        TargetType=jc.TargetType,
                        TargetMethod = jc.TargetMethod,
                        TriggerType = jc.TriggerType,
                        CronExpression = jc.CronExpression,

                        // Step 4: Status
                        Enabled = jc.Enabled,

                        UpdatedAt = jc.UpdatedAt
                    }
                ).FirstOrDefaultAsync();

                if (data == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Commission configuration not found.";
                    return NotFound(response);
                }

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.commissionConfig = new List<CommissionConfigDTO> { data };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching commission details for ID: {Id}", commissionConfigId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("CreateCommission")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> CreateCommission([FromBody] CreateCommissionDto dto)
        {
            HmsResponse response = new HmsResponse();
            int orgId = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            var username = _authClaimService.GetClaim(ClaimTypes.Name);

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                CommissionConfig commission;
                JobConfig job;
                JobExtns jobExtns;

                //Edit Existing Entry
                if (dto.CommissionConfigId > 0)
                {
                    commission = await _context.CommissionConfigs.FirstOrDefaultAsync(x => x.CommissionConfigId == dto.CommissionConfigId && x.OrgId == orgId);

                    if (commission == null)
                    {
                        response.responseHeader.ErrorCode = CommissionConstants.COMMISSION_NOTFOUND;
                        response.responseHeader.ErrorMessage = await _context.errorMaster
                            .Where(x => x.ErrorId == CommissionConstants.COMMISSION_NOTFOUND
                                     && x.Area == "CommissionConstants")
                            .Select(x => x.ErrorMsg)
                            .FirstOrDefaultAsync() ?? "Undefined Error Message";

                        return NoContent();
                    }

                    job = await _context.JobConfigs.FirstOrDefaultAsync(x => x.JobConfigId == commission.JobConfigId);

                    if (job == null)
                    {
                        response.responseHeader.ErrorCode = JobConstants.JOB_NOTFOUND;
                        response.responseHeader.ErrorMessage = await _context.errorMaster
                            .Where(x => x.ErrorId == JobConstants.JOB_NOTFOUND
                                     && x.Area == "JobConstants")
                            .Select(x => x.ErrorMsg)
                            .FirstOrDefaultAsync() ?? "Undefined Error Message";

                        return NoContent();
                    }
                    // Update Job (Only fields relevant to Step 1)
                    job.JobName = dto.CommissionName;
                    job.StartAt = dto.RunFrom.HasValue ? dto.RunFrom.Value.ToUniversalTime() : null;
                    job.EndAt = dto.RunTo.HasValue ? dto.RunTo.Value.ToUniversalTime() : null;
                    job.TargetType = dto.TargetType;
                    job.TargetMethod = dto.TargetMethod;
                    job.UpdatedAt = DateTime.Now;

                    jobExtns = await _context.JobExtns.FirstOrDefaultAsync(x => x.JobConfigId == job.JobConfigId && x.OrgId == orgId);

                    if (jobExtns == null)
                    {
                        jobExtns = new JobExtns
                        {
                            JobConfigId = job.JobConfigId,
                            OrgId = orgId
                        };
                        _context.JobExtns.Add(jobExtns);
                    }

                    jobExtns.Comments = dto.Comments;
                    jobExtns.Filter = dto.FilterConditions;
                }
                else
                {
                    //At time New Entry
                    job = new JobConfig
                    {
                        JobName = dto.CommissionName,
                        JobType = "CRON",
                        TriggerType = "CRON",
                        Enabled = false,
                        StartAt = dto.RunFrom.HasValue ? dto.RunFrom.Value.ToUniversalTime() : null,
                        EndAt = dto.RunTo.HasValue ? dto.RunTo.Value.ToUniversalTime() : null,
                        OrgId = orgId,
                        CreatedAt = DateTime.Now,
                        TargetType = dto.TargetType,
                        TargetMethod = dto.TargetMethod
                    };

                    _context.JobConfigs.Add(job);
                    await _context.SaveChangesAsync();

                     jobExtns = new JobExtns
                    {
                        JobConfigId = job.JobConfigId,
                        OrgId = orgId,
                        Comments = dto.Comments,
                        Filter = dto.FilterConditions
                    };

                    _context.JobExtns.Add(jobExtns);

                    commission = new CommissionConfig
                    {
                        CreatedAt = DateTime.Now,
                        CreatedBy = username,
                        OrgId = orgId,
                        JobConfigId = job.JobConfigId
                    };

                    _context.CommissionConfigs.Add(commission);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.commissionConfig = new List<CommissionConfigDTO>
                                            {
                                                new CommissionConfigDTO
                                                {
                                                    CommissionConfigId = commission.CommissionConfigId,
                                                    JobConfigId = commission.JobConfigId,
                                                    JobName=job.JobName,
                                                    RunFrom=job.StartAt,
                                                    RunTo=job.StartAt,
                                                    Formula=commission.Formula,
                                                    TargetType=job.TargetType,
                                                    TargetMethod=job.TargetMethod,
                                                    JobType=job.JobType,
                                                    TriggerType=job.TriggerType,
                                                    CronExpression=job.CronExpression,
                                                    Enabled=job.Enabled,
                                                    FilterCondition=jobExtns.Filter,
                                                    Comments=jobExtns.Comments,
                                                    CreatedAt = commission.CreatedAt
                                                }
                                            };
                return Ok(response);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique") == true)
            {
                await tx.RollbackAsync();
                return Conflict(new { Message = "A commission with this name already exists." });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Error in Create/Update Commission Step 1");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("UpdateCommissionFormula")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> UpdateCommissionFormula([FromBody] CommissionConditionUpdateDto dto)
        {
            HmsResponse response = new HmsResponse();

            try
            {
                var config = await _context.CommissionConfigs
                               .FirstOrDefaultAsync(c =>
                                c.CommissionConfigId == dto.CommissionConfigId
                                && _context.JobConfigs.Any(j => j.JobConfigId == c.JobConfigId)
                               );

                if (config == null)
                {
                    response.responseHeader.ErrorCode = CommissionConstants.COMMISSION_NOTFOUND;
                    response.responseHeader.ErrorMessage = await _context.errorMaster
                        .Where(x => x.ErrorId == CommissionConstants.COMMISSION_NOTFOUND
                                 && x.Area == "CommissionConstants")
                        .Select(x => x.ErrorMsg)
                        .FirstOrDefaultAsync() ?? "Undefined Error Message";

                    return NoContent();
                }

                config.Formula = dto.Formula;

                //_context.CommissionConfigs.Update(config);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";

                response.responseBody.commissionConfig = new List<CommissionConfigDTO>
                                {
                                    new CommissionConfigDTO
                                    {
                                        CommissionConfigId = config.CommissionConfigId,
                                        Formula         = config.Formula,
                                        CreatedAt          = config.CreatedAt,
                                        CreatedBy          = config.CreatedBy,
                                        JobConfigId        = config.JobConfigId
                                    }
                                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Commission Config UpdateCommissionFormula failed at {UtcNow} Exception {message}", DateTime.UtcNow, ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("UpdateCronSetting")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> UpdateCronSetting([FromBody] UpdateCronDto dto)
        {
            HmsResponse response = new HmsResponse();

            try
            {
                var commission = await _context.CommissionConfigs
                        .FirstOrDefaultAsync(x => x.CommissionConfigId == dto.CommissionConfigId);

                if (commission == null)
                {
                    response.responseHeader.ErrorCode = CommissionConstants.COMMISSION_NOTFOUND;
                    response.responseHeader.ErrorMessage = await _context.errorMaster
                        .Where(x => x.ErrorId == CommissionConstants.COMMISSION_NOTFOUND
                                 && x.Area == "CommissionConstants")
                        .Select(x => x.ErrorMsg)
                        .FirstOrDefaultAsync() ?? "Undefined Error Message";

                    return NoContent();
                }

                var job = await _context.JobConfigs
                    .FirstOrDefaultAsync(x => x.JobConfigId == commission.JobConfigId);

                if (job == null)
                {
                    response.responseHeader.ErrorCode = JobConstants.JOB_NOTFOUND;
                    response.responseHeader.ErrorMessage = await _context.errorMaster
                        .Where(x => x.ErrorId == JobConstants.JOB_NOTFOUND
                                 && x.Area == "JobConstants")
                        .Select(x => x.ErrorMsg)
                        .FirstOrDefaultAsync() ?? "Undefined Error Message";

                    return NoContent();
                }

                job.JobType = dto.JobType ?? job.JobType;
                job.TriggerType = dto.TriggerType ?? job.TriggerType;
                job.CronExpression = dto.CronExpression ?? job.CronExpression;
                job.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.commissionConfig = new List<CommissionConfigDTO>
                                                         {
                                                             new CommissionConfigDTO
                                                             {
                                                                 CommissionConfigId = commission.CommissionConfigId,
                                                                 JobConfigId = commission.JobConfigId,
                                                                 JobName = job.JobName,
                                                                 JobType = job.JobType,
                                                                 TriggerType = job.TriggerType,
                                                                 CronExpression = job.CronExpression,
                                                                 Enabled = job.Enabled,
                                                                 UpdatedAt = job.UpdatedAt
                                                             }
                                                         };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Commisssion Config UpdateCronSetting failed at {UtcNow} Exception {message}", DateTime.UtcNow, ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("EnableDisableJob")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> EnableDisableJob([FromBody] EnableDisableJobDto dto)
        {
            HmsResponse response = new HmsResponse();
            try
            {
                var commission = await _context.CommissionConfigs.FirstOrDefaultAsync(x => x.CommissionConfigId == dto.CommissionConfigId);

                if (commission == null)
                {
                    response.responseHeader.ErrorCode = CommissionConstants.COMMISSION_NOTFOUND;
                    response.responseHeader.ErrorMessage = await _context.errorMaster
                        .Where(x => x.ErrorId == CommissionConstants.COMMISSION_NOTFOUND
                                 && x.Area == "CommissionConstants")
                        .Select(x => x.ErrorMsg)
                        .FirstOrDefaultAsync() ?? "Undefined Error Message";

                    return NoContent();
                }

                var job = await _context.JobConfigs.FirstOrDefaultAsync(x => x.JobConfigId == commission.JobConfigId);

                if (job == null)
                {
                    response.responseHeader.ErrorCode = JobConstants.JOB_NOTFOUND;
                    response.responseHeader.ErrorMessage = await _context.errorMaster
                        .Where(x => x.ErrorId == JobConstants.JOB_NOTFOUND
                                 && x.Area == "JobConstants")
                        .Select(x => x.ErrorMsg)
                        .FirstOrDefaultAsync() ?? "Undefined Error Message";

                    return NoContent();
                }

                job.Enabled = dto.Enabled;
                job.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.commissionConfig = new List<CommissionConfigDTO>
                {
                    new CommissionConfigDTO
                    {
                        CommissionConfigId = commission.CommissionConfigId,
                        JobName = job.JobName,
                        JobConfigId = commission.JobConfigId,
                        JobType = job.JobType,
                        TriggerType = job.TriggerType,
                        CronExpression = job.CronExpression,
                        Enabled = job.Enabled,
                        UpdatedAt = job.UpdatedAt
                    }
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EnableDisableJob failed at {UtcNow} Exception {message}", DateTime.UtcNow, ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("CommissionJobConfigList")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> GetCommissionJobConfigList()
        {
            HmsResponse response = new HmsResponse();
            int orgId = 0;

            try
            {
                orgId = Convert.ToInt32(
                    _authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                var list = await (
                                from cc in _context.CommissionConfigs.AsNoTracking()
                                join jc in _context.JobConfigs.AsNoTracking()
                                    on cc.JobConfigId equals jc.JobConfigId
                                join je in _context.JobExtns.AsNoTracking()
                                    on jc.JobConfigId equals je.JobConfigId
                                where cc.OrgId == orgId
                                orderby cc.CreatedAt descending
                                select new CommissionConfigDTO
                                {
                                    // -------- CommissionConfig --------
                                    CommissionConfigId = cc.CommissionConfigId,
                                    CommissionName = jc.JobName,
                                    RunFrom = jc.StartAt,
                                    RunTo = jc.EndAt,
                                    Formula=cc.Formula,
                                    CreatedAt = cc.CreatedAt,
                                    CreatedBy = cc.CreatedBy,
                                    JobConfigId = cc.JobConfigId,

                                    // -------- JobExtns --------
                                    FilterCondition = je.Filter,
                                    Comments = je.Comments,

                                    // -------- JobConfig --------
                                    JobType = jc.JobType,
                                    Enabled = jc.Enabled,
                                    TriggerType = jc.TriggerType,
                                    CronExpression = jc.CronExpression,
                                    IntervalSeconds = jc.IntervalSeconds,
                                    Parameters = jc.Parameters,
                                    UpdatedAt = jc.UpdatedAt,
                                    TargetType = jc.TargetType,
                                    TargetMethod = jc.TargetMethod,
                                    Args = jc.Args
                                }
                            ).ToListAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.commissionConfig = list;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCommissionConfigList API failed OrgId={OrgId}", orgId);
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpPost("JobExecutionHistory/{jobConfigId}")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> GetJobExecutionHistory([FromRoute] int? jobConfigId)        
        {
            HmsResponse response = new HmsResponse();
            int orgId = 0;

            try
            {
                orgId = Convert.ToInt32(
                    _authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                var query =
                    from h in _context.JobExeHists.AsNoTracking()
                    join jc in _context.JobConfigs.AsNoTracking()
                        on h.JobConfigId equals jc.JobConfigId
                    join je in _context.JobExtns.AsNoTracking()
                        on jc.JobConfigId equals je.JobConfigId
                    where h.OrgId == orgId
                    select new JobExecutionHistoryDto
                    {
                        JobExeHistId = h.JobExeHistId,
                        JobConfigId = h.JobConfigId,
                        JobName = jc.JobName,

                        StartedAt = h.StartedAt,
                        FinishedAt = h.FinishedAt,
                        ExeStatus = h.ExeStatus,
                        DownloadLink = h.DownloadLnk,

                        Comments = je.Comments
                    };

                if (jobConfigId.HasValue)
                    query = query.Where(x => x.JobConfigId == jobConfigId.Value);

                var list = await query
                    .OrderByDescending(x => x.StartedAt)
                    .ToListAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.jobExecutionHistory = list;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GetJobExecutionHistory failed OrgId={OrgId}", orgId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("CommissionJobConfigPaginatedList")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> GetCommissionJobConfigList([FromBody] PaginationRequest request)
        {
            HmsResponse response = new HmsResponse();

            request.PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize <= 0 ? 10 :  request.PageSize;
            
            try
            {
                int orgId = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                var results = await _db.ExecuteQueryAsync<CommissionConfigDTO>(
                    "Commission",
                    "GetConfigList",
                    new
                    {
                        p_orgid = orgId,
                        p_page_number = request.PageNumber,
                        p_page_size = request.PageSize
                    });

                var configList = results?.ToList() ?? new List<CommissionConfigDTO>();
                if(configList.Any())
                {
                    int totalItems = configList.FirstOrDefault()?.TotalCount ?? 0;

                    response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    response.responseHeader.ErrorMessage = "SUCCESS";
                    response.responseBody.commissionConfig = configList;
                    response.responseBody.pagination = new
                    {
                        currentPage = request.PageNumber,
                        totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize),
                        pageSize = request.PageSize,
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
                _logger.LogError(ex, "Failed to fetch paginated commission list ");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
