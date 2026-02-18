using CommonLibrary;
using HMS.Data;
using HMS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HMSContext _context;
        private readonly ILogger<HealthController> _logger;
        private readonly IAuthClaimService _authClaimService;
        public HealthController(HMSContext context, IConfiguration configuration, ILogger<HealthController> logger
            , IAuthClaimService authClaimService)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _authClaimService = authClaimService;
        }

        [HttpPost("Check")]
        [Authorize]
        [MenuAuthorize(1002)]
        public async Task<IActionResult> Check()
        {
            try
            {
                //_logger.LogInformation("Information Log is Working");
                //_logger.LogWarning("Warning Log is Working");
                /*
                var claims = new List<Claim>
                {
                    // Standard claims
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),        // JWT standard "sub"
                    _authClaimService.GetClaim(ClaimTypes.NameIdentifier),          // .NET convention
                    new Claim(ClaimTypes.Name, user.Username),                             // Username
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),     // Unique token ID,
                    new Claim(ApiConstants.OrganisationId, user.OrgId.ToString()),         // Custom claim for User ID
                    new Claim(ApiConstants.OrganisationName, user.OrgName.ToString()),     // Custom claim for User ID
                    new Claim(ApiConstants.SubscriberId, user.SubscriberId.ToString()),    // Custom claim for User ID
                    new Claim(ApiConstants.SubscriberName, user.SubscriberName.ToString()) // Custom claim for User ID
                };
                 */

                await _context.agent.FindAsync(-1000);
                return Ok(new
                {
                    status = "Healthy",
                    database = "Connected",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed at {UtcNow} Exception {message}", DateTime.UtcNow, ex.Message);
                return StatusCode(503, new
                {
                    status = "Unhealthy",
                    database = "Not Connected",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
                /*
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = $"An error occurred while creating the sub channel master {SubChannelMaster.SubChannelName}";
                _logger.LogError(ex, response.responseHeader.ErrorMessage);
                return BadRequest(response);
                 */
            }
        }
    }
}