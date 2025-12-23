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
            _logger = (ILogger<CommissionConfigController>?)logger;
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
                
                config.OrgId = orgIdFromClaims;
                _context.CommissionConfigs.Add(config);
                await _context.SaveChangesAsync();
                
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.commissionConfig = new List<CommissionConfig> { config }; return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveCommissionConfig API failed", config.Id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
