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
                var job = new JobConfig
                {
                    JobName = dto.CommissionName,
                    JobType = "CRON",
                    TriggerType = "CRON",
                    Enabled = false,
                    StartAt =dto.RunFrom,
                    EndAt = dto.RunTo,
                    OrgId = orgId
                };

                _context.JobConfigs.Add(job);
                await _context.SaveChangesAsync();

                var commission = new CommissionConfig
                {
                    CommissionName = dto.CommissionName,
                    RunFrom = dto.RunFrom,
                    RunTo = dto.RunTo,
                    CreatedAt = DateTime.Now,
                    CreatedBy = username,
                    OrgId = orgId,
                    JobConfigId = job.JobConfigId
                };

                _context.CommissionConfigs.Add(commission);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

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
                            CreatedAt = commission.CreatedAt,
                            CreatedBy = commission.CreatedBy
                        }
                    };

                return Ok(response);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                await tx.RollbackAsync();
                _logger.LogWarning("Duplicate Commission or Job Name: {Name}", dto.CommissionName);

                return Conflict(new { Message = "A commission or job with this name already exists." });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Unexpected error creating commission");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPost("UpdateCommissionFormula")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> UpdateCommissionFormula([FromBody] CommissionConditionUpdateDto dto)
        {
            HmsResponse response = new HmsResponse();

            var config = await _context.CommissionConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CommissionConfigId == dto.CommissionConfigId);

            if (config == null)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Commission not found";
                return NotFound(response);
            }

            config.Conditions = dto.Condition;
            _context.CommissionConfigs.Update(config);
            await _context.SaveChangesAsync();

            response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
            response.responseHeader.ErrorMessage = "SUCCESS";
            response.responseBody.commissionConfig = new List<CommissionConfigDTO>
            {
                new CommissionConfigDTO
                {
                    CommissionConfigId = config.CommissionConfigId,
                    CommissionName = config.CommissionName,
                    RunFrom = config.RunFrom,
                    RunTo = config.RunTo,
                    Conditions = config.Conditions,
                    CreatedAt = config.CreatedAt,
                    CreatedBy = config.CreatedBy,
                    JobConfigId = config.JobConfigId
                }
            };

            return Ok(response);
        }

        [HttpPost("UpdateCronSetting")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> UpdateCronSetting([FromBody] UpdateCronDto dto)
        {
            HmsResponse response = new HmsResponse();

            var commission = await _context.CommissionConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CommissionConfigId == dto.CommissionConfigId);

            if (commission == null)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Commission not found";
                return NotFound(response);
            }

            var job = await _context.JobConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.JobConfigId == commission.JobConfigId);

            if (job == null)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Job not found";
                return NotFound(response);
            }

            job.JobType = dto.JobType;
            job.TriggerType = dto.TriggerType;
            job.CronExpression = dto.CronExpression;
            job.UpdatedAt = DateTime.Now;

            _context.JobConfigs.Update(job);
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

        [HttpPost("EnableDisableJob")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> EnableDisableJob([FromBody] EnableDisableJobDto dto)
        {
            HmsResponse response = new HmsResponse();

            var commission = await _context.CommissionConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CommissionConfigId == dto.CommissionConfigId);

            if (commission == null)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Commission not found";
                return NotFound(response);
            }

            var job = await _context.JobConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.JobConfigId == commission.JobConfigId);

            if (job == null)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Job not found";
                return NotFound(response);
            }

            job.Enabled = dto.Enabled;
            job.TargetType = dto.TargetType;
            job.TargetMethod = dto.TargetMethod;
            job.UpdatedAt = DateTime.Now;

            _context.JobConfigs.Update(job);
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

        [HttpPost("CommissionSearchFieldsJson")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> GetCommissionMetadata()
        {
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

                return Ok(metadata);
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
                        DownloadLink = h.DownloadLnk
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
