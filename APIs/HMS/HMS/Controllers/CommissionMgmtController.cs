using HMS.Data;
using HMS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DB;
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
        [Authorize]
        [MenuAuthorize(1001)]
        public ActionResult<CommissionMgmtDashboard> Dashboard([FromBody] CommissionMgmtDashboardDTO commissionMgmtDashboardDTO)
        {
            CommissionMgmtDashboard commissionMgmtDashboard = new CommissionMgmtDashboard
            {
                bulkCommUpdates = new BulkCommUpdates
                {
                    BranchMaster = new Models.DB.BranchMaster
                    {
                        Address = "Branch Address",
                        BranchCode = "BR-1",
                        BranchName = "Branch Name",
                        ChannelCode = "Channel Code",
                        ChannelMaster = new Models.DB.ChannelMaster
                        {
                            ChannelCode = "Channel Code",
                            ChannelName = "ChannelName",
                            CreatedBy = "CreatedBy",
                            CreatedDate = DateTime.Now,
                            CreatedEntities = 10
                        },
                    },
                    RequestDate = DateTime.Now,
                    TotalRecords = 1000,
                }
            };
            return Ok(JsonConvert.SerializeObject(commissionMgmtDashboard));
        }
    }
}