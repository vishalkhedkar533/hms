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

        public HealthController(HMSContext context, IConfiguration configuration, ILogger<HealthController> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
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
                //HttpContext.User.Claims.Where(x=>x.Type.Equals("OrganisationId")).Select( x=> x.Value ).FirstOrDefault() ?? String.Empty
                //HttpContext.User.Claims.Where(x=>x.Type.Equals("OrganisationName")).Select( x=> x.Value ).FirstOrDefault() ?? String.Empty
                //HttpContext.User.Claims.Where(x=>x.Type.Equals("SubscriberId")).Select( x=> x.Value ).FirstOrDefault() ?? String.Empty
                //HttpContext.User.Claims.Where(x=>x.Type.Equals("SubscriberName")).Select( x=> x.Value ).FirstOrDefault() ?? String.Empty
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
            }
        }
    }
}