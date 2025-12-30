using CommonLibrary;
using HMS.Data;
using HMS.Security;
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

        #region Old Save Method
        //[HttpPost("Save")]
        //[Authorize]
        //[MenuAuthorize(1001)]
        //public async Task<IActionResult> SaveCommissionConfig([FromBody] CommissionConfigDTO config)
        //{
        //    HmsResponse response = new HmsResponse();

        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);
        //    try
        //    {
        //        int orgIdFromClaims = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
        //        var username = HttpContext?.User?.Identity?.Name;

        //        config.OrgId = orgIdFromClaims;
        //        config.CreatedBy= username;
        //        config.CreatedAt = DateTime.UtcNow;
        //        _context.CommissionConfigs.Add(config);
        //        await _context.SaveChangesAsync();
                
        //        response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
        //        response.responseHeader.ErrorMessage = "SUCCESS";
        //        response.responseBody.commissionConfig = new List<CommissionConfig> { config }; 
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "SaveCommissionConfig API failed");
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}
        #endregion

        [HttpPost("SaveCommissionConfig")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> SaveCommissionConfig([FromBody] CommissionConfigDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            HmsResponse response = new HmsResponse();

            int orgIdFromClaims = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

            var username =_authClaimService.GetClaim(ClaimTypes.Name);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var jobConfig = new JobConfig
                {
                    JobName = dto.CommissionName!,
                    JobType = dto.JobType,
                    Enabled = dto.Enabled,
                    TriggerType = dto.TriggerType,
                    CronExpression = dto.CronExpression,
                    IntervalSeconds = dto.IntervalSeconds,
                    StartAt=dto.RunFrom,
                    EndAt=dto.RunTo,
                    Parameters = dto.Parameters,
                    TargetType = dto.TargetType,
                    TargetMethod = dto.TargetMethod,
                    Args = dto.Args,
                    OrgId = orgIdFromClaims,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _context.JobConfigs.Add(jobConfig);
                await _context.SaveChangesAsync();

                var commissionConfig = new CommissionConfig
                {
                    CommissionName = dto.CommissionName,
                    RunFrom = dto.RunFrom,
                    RunTo = dto.RunTo,
                    Conditions = dto.Conditions,
                    OrgId = orgIdFromClaims,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,

                    JobConfigId = jobConfig.JobConfigId
                };

                _context.CommissionConfigs.Add(commissionConfig);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.commissionConfig = new List<CommissionConfig> { commissionConfig };

                return Ok(response);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database error in SaveCommissionConfig");

                return Conflict(new
                {
                    message = "Duplicate CommissionName or JobName Found."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "SaveCommissionConfig API failed");

                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPatch("UpdateCommissionCondition")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> UpdateCommissionCondition([FromBody] CommissionConditionUpdateDto commissionConditionUpdateDto )
        {
            HmsResponse response = new HmsResponse();
            try
            {
                var existingConfig = await _context.CommissionConfigs
                    .FirstOrDefaultAsync(x => x.CommissionConfigId == commissionConditionUpdateDto.CommissionConfigId);
                var username = HttpContext?.User?.Identity?.Name;

                if (existingConfig == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Commission configuration not found.";
                    return NotFound(response);
                }

                existingConfig.Conditions = commissionConditionUpdateDto.Condition;
                existingConfig.CreatedBy = username;
                existingConfig.CreatedAt = DateTime.SpecifyKind(existingConfig.CreatedAt, DateTimeKind.Utc);

                _context.CommissionConfigs.Update(existingConfig);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "Condition updated successfully";
                response.responseBody.commissionConfig = new List<CommissionConfig> { existingConfig };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateCommissionCondition failed for ID={CommissionConfigId}", commissionConditionUpdateDto.CommissionConfigId);
                return StatusCode(500, $"Internal server error : {ex.Message}");
            }
        }

        [HttpPost("list")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> GetCommissionConfigList()
        {
            HmsResponse response = new HmsResponse();
            int orgId = 0;
            try
            {
                orgId = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                var configList = await _context.CommissionConfigs
                                                .Where(x => x.OrgId == orgId)
                                                .AsNoTracking()
                                                .OrderByDescending(x => x.CreatedAt)
                                                .ToListAsync();
                
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.commissionConfig = configList;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCommissionConfigList API failed OrgId={OrgId}", orgId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
                //response.responseBody.commissionConfig = list;

                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCommissionConfigList API failed OrgId={OrgId}", orgId);
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
