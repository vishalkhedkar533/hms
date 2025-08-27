using HMS.Data;
using HMS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;
using Serilog;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<DashboardController> _logger;
        public DashboardController(HMSContext context, IConfiguration config, ILogger<DashboardController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        // GET: api/<DashboardController>        
        [HttpPost("GetDashboard")]
        [Authorize]
        [MenuAuthorize(1002)]
        public async Task<ActionResult> GetDashboard([FromBody] FetchUserDashboard fetchUserInfo)
        {            
            HMSResponse hmsResponse = new HMSResponse();
            try
            {
                _logger.LogInformation("Seri Log is Working");
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == fetchUserInfo.UserId);
                if (user == null)
                {
                    _logger.LogError(fetchUserInfo.UserId + " User not found.");
                    return NotFound("User not found.");
                }

                HMSDashboard hmsrecord = await _context.HMSDashboard.FirstOrDefaultAsync(x => x.UserId == fetchUserInfo.UserId);

                if (hmsrecord == null)
                {
                    //_logger.LogError("HMSDashboard data not found for UserId: {UserId}", fetchUserInfo.UserId);
                    //return NotFound("HMSDashboard data not found for UserId: " + fetchUserInfo.UserId.ToString());
                    _context.HMSDashboard.Add(new HMSDashboard
                    {
                        UserId = fetchUserInfo.UserId.Value,
                        CertificateExpiringIn30Months = 0,
                        EntitiesCreatedPrevMonth = 0,
                        EntitiesCreatedThisMonth = 0,
                        EntitiesTerminatedPrevMonth = 0,
                        EntitiesNetThisMonth = 0,
                        EntitiesTerminatedThisMonth = 0,
                        LicenseExpiringIn30Months = 0,
                        MBGCriteriaNotMet = 0,
                        TotalEntitiesCount = 0,
                        TotalEntitiesThisMonth = 0,
                        channelDetails = new List<ChannelDetails>(),
                        statusDetails = new List<StatusDetails>()
                    });
                    _context.SaveChangesAsync();
                }

                ChannelDetails channelDetails = new ChannelDetails();
                channelDetails.UserId = 1;
                channelDetails.ChannelId = 1;
                channelDetails.ChannelName = "Digital Banking";
                channelDetails.TotalEntities = 12;
                channelDetails.Created = 10;
                channelDetails.Terminated = 2;
                hmsrecord.channelDetails.Add(channelDetails);

                StatusDetails statusDetails = new StatusDetails();
                statusDetails.UserId = 1;
                statusDetails.StatusId = 1;
                statusDetails.StatusName = "Code Movement";
                statusDetails.PendingItem = 10;
                statusDetails.LastUpdated = "2 hours ago";
                statusDetails.Priority = "High";
                hmsrecord.statusDetails.Add(statusDetails);

                hmsResponse.responseHeader = new HMSResponseHeader
                {
                    ErrorCode = CommonConstants.SUCCESS,
                    ErrorMessage = await _context.errorMaster
                    .Where(x => x.ErrorId == CommonConstants.SUCCESS && x.Area == "Common")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message"
                };

                hmsResponse.responseBody.hmsDashboard = hmsrecord;
                throw new InvalidOperationException("Something went wrong");
                return Ok(hmsResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ErrorTest endpoint failed");
                return StatusCode(500, "Error logged");
            }            
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("GetCommissionDashboard")]
        [Authorize]
        [MenuAuthorize(1003)]
        public async Task<ActionResult> GetCommissionDashboard([FromBody] FetchUserDashboard fetchUserInfo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == fetchUserInfo.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            CommissionDashboard CommissionDashboard = new CommissionDashboard();
            return Ok(CommissionDashboard);
        }

    }
}
