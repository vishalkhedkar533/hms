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
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;

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
        public CommissionConfigController(IAuthClaimService authClaimService,HMSContext hMSContext, ILogger<CommissionConfigController> logger, IWebHostEnvironment env)
        {
            _authClaimService = authClaimService;
            _context = hMSContext;
            _logger = logger;
            _env = env;
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
                        CommissionName = cc.CommissionName,
                        RunFrom = cc.RunFrom,
                        RunTo = cc.RunTo,

                        // Step 2: Formula
                        Conditions = cc.Conditions,

                        // Step 3: Schedule Configuration
                        JobConfigId = cc.JobConfigId,
                        JobType = jc.JobType,
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
                //Edit Existing Entry
                if (dto.CommissionConfigId > 0)
                {
                    commission = await _context.CommissionConfigs
                        .FirstOrDefaultAsync(x => x.CommissionConfigId == dto.CommissionConfigId && x.OrgId == orgId);

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

                    job = await _context.JobConfigs
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

                    // Update Job (Only fields relevant to Step 1)
                    job.JobName = dto.CommissionName;
                    job.StartAt = dto.RunFrom;
                    job.EndAt = dto.RunTo;
                    job.Comments = dto.Comments;
                    job.UpdatedAt = DateTime.Now;

                    // Update Commission
                    commission.CommissionName = dto.CommissionName;
                    commission.RunFrom = dto.RunFrom;
                    commission.RunTo = dto.RunTo;
                    commission.FilterCondition = dto.FilterConditions;
                    commission.Comments = dto.Comments;
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
                        StartAt = dto.RunFrom,
                        EndAt = dto.RunTo,
                        OrgId = orgId,
                        Comments = dto.Comments,
                        CreatedAt = DateTime.Now
                    };

                    _context.JobConfigs.Add(job);
                    await _context.SaveChangesAsync();

                    commission = new CommissionConfig
                    {
                        CommissionName = dto.CommissionName,
                        RunFrom = dto.RunFrom,
                        RunTo = dto.RunTo,
                        FilterCondition = dto.FilterConditions,
                        Comments = dto.Comments,
                        CreatedAt = DateTime.Now,
                        CreatedBy = username,
                        OrgId = orgId,
                        JobConfigId = job.JobConfigId
                    };

                    _context.CommissionConfigs.Add(commission);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                // Return the full DTO so the UI can proceed to Step 2 with the ID
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.commissionConfig = new List<CommissionConfigDTO>
                                            {
                                                new CommissionConfigDTO
                                                {
                                                    CommissionConfigId = commission.CommissionConfigId,
                                                    CommissionName = commission.CommissionName,
                                                    RunFrom = commission.RunFrom,
                                                    RunTo = commission.RunTo,
                                                    JobConfigId = commission.JobConfigId,
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

                config.Conditions = dto.Condition;

                //_context.CommissionConfigs.Update(config);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";

                response.responseBody.commissionConfig = new List<CommissionConfigDTO>
                                {
                                    new CommissionConfigDTO
                                    {
                                        CommissionConfigId = config.CommissionConfigId,
                                        CommissionName     = config.CommissionName,
                                        RunFrom            = config.RunFrom,
                                        RunTo              = config.RunTo,
                                        Conditions         = config.Conditions,
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
                                                                 CommissionName = commission.CommissionName,
                                                                 RunFrom = commission.RunFrom,
                                                                 RunTo = commission.RunTo,
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

                job.Enabled = dto.Enabled;
                job.TargetType = dto.TargetType ?? job.TargetType;
                job.TargetMethod = dto.TargetMethod ?? job.TargetMethod;
                job.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.commissionConfig = new List<CommissionConfigDTO>
                {
                    new CommissionConfigDTO
                    {
                        CommissionConfigId = commission.CommissionConfigId,
                        CommissionName = commission.CommissionName,
                        RunFrom = commission.RunFrom,
                        RunTo = commission.RunTo,
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

        [HttpPost("CommissionSearchFieldsJson")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> GetCommissionSearchFieldsJson()
        {
            HmsResponse response = new HmsResponse();

            try
            {
                string filePath = Path.Combine(_env.ContentRootPath, "CommissionMetaData", "commissionMetadata.json");

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Metadata file missing at: {Path}", filePath);
                    return NotFound(new { message = "Metadata configuration file not found." });
                }

                string jsonString = await System.IO.File.ReadAllTextAsync(filePath);

                var metadata = JsonSerializer.Deserialize<CommissionMetadata>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.CommissionMetadata = new List<CommissionMetadata> { metadata };

                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading commission metadata file");
                return StatusCode(500, "Internal server error reading configuration.");
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
                    where cc.OrgId == orgId
                    orderby cc.CreatedAt descending
                    select new CommissionConfigDTO
                    {
                        // -------- CommissionConfig --------
                        CommissionConfigId = cc.CommissionConfigId,
                        CommissionName = cc.CommissionName,
                        RunFrom = cc.RunFrom,
                        RunTo = cc.RunTo,
                        CreatedAt = cc.CreatedAt,
                        CreatedBy = cc.CreatedBy,
                        Conditions = cc.Conditions,
                        JobConfigId = cc.JobConfigId,

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
                        Comments=jc.Comments
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
    }
}
