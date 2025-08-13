using HMS.Data;
using Microsoft.AspNetCore.Mvc;
using Models;
using Newtonsoft.Json;

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
        //public async Task<ActionResult<User>> CreateUser(User user)
        [HttpPost("Dashboard")]
        public async Task<ActionResult<CommissionMgmtDashboard>> Dashboard([FromBody] CommissionMgmtDashboardDTO commissionMgmtDashboardDTO)
        {
            CommissionMgmtDashboard commissionMgmtDashboard = new CommissionMgmtDashboard();
            commissionMgmtDashboard.bulkCommUpdates = new BulkCommUpdates();
            commissionMgmtDashboard.bulkCommUpdates.TotalRecords = 10000;
            commissionMgmtDashboard.bulkCommUpdates.BranchMaster = new Models.BranchMaster
            {
                Address = "Branch Address",
                BranchCode = "BR-1",
                BranchName ="Branch Name",
                ChannelCode ="Channel Code",
                ChannelMaster = new Models.ChannelMaster {
                    ChannelCode ="Channel Code",
                    ChannelName ="ChannelName",
                    CreatedBy = "CreatedBy",
                    CreatedDate = DateTime.Now,
                    CreatedEntities = 10
                },
                CreatedBy = "CreatedBy"
            };
            return Ok(JsonConvert.SerializeObject(commissionMgmtDashboard));
        }
    }
}