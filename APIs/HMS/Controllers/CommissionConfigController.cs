using CommonLibrary;
using HMS.Data;
using HMS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DB;

namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommissionConfigController : ControllerBase
    {
        private readonly IAuthClaimService _authClaimService;
        private readonly HMSContext _context;
        public CommissionConfigController(IAuthClaimService authClaimService,HMSContext hMSContext)
        {
            _authClaimService = authClaimService;
            _context = hMSContext;
        }

        [HttpPost("Save")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> SaveConfig([FromBody] CommissionConfig config)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                int orgIdFromClaims = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
                config.OrgId = orgIdFromClaims;
                _context.CommissionConfigs.Add(config);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Commission saved successfully", id = config.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
