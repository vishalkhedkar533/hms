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
    public class CommissionConfigController : ControllerBase
    {
        private readonly IAuthClaimService _authClaimService;
        private readonly HMSContext _context;
        private readonly ILogger<CommissionConfigController> _logger;
        public CommissionConfigController(IAuthClaimService authClaimService,HMSContext hMSContext, ILogger<CommissionConfigController> logger)
        {
            _authClaimService = authClaimService;
            _context = hMSContext;
            _logger = logger;
        }

        [HttpPost("Save")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> SaveCommissionConfig([FromBody] CommissionConfig config)
        {
            HmsResponse response = new HmsResponse();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                int orgIdFromClaims = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
                var username = HttpContext?.User?.Identity?.Name;

                config.OrgId = orgIdFromClaims;
                config.CreatedBy= username;
                config.CreatedAt = DateTime.UtcNow;
                _context.CommissionConfigs.Add(config);
                await _context.SaveChangesAsync();
                
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.commissionConfig = new List<CommissionConfig> { config }; 
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveCommissionConfig API failed");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("update-condition")]
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
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Internal server error";
                return StatusCode(500, response);
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
    }
}
