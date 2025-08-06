using HMS.Data;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommissionMgmtController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;
        public CommissionMgmtController(HMSContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpPost("Request")]
        public async Task<IActionResult> RequestTermination([FromBody] CommissionMgmtDashboardDTO commissionMgmtDashboardDTO)
        {
            CommissionMgmtDashboard commissionMgmtDashboard = new CommissionMgmtDashboard();
            return Ok(commissionMgmtDashboard);
        }

    }
}
