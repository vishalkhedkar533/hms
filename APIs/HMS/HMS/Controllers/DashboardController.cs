using HMS.Data;
using HMS.Models;
using HMS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
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
        [HttpGet("GetDashboard")]
        [Authorize]
        [MenuAuthorize(1002)]
        public async Task<ActionResult> GetDashboard(Int32? UserId)
        {
            _logger.LogInformation("Seri Log is Working");
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == UserId);
            if (user == null)
            {
                _logger.LogError(UserId + " User not found.");
                return NotFound("User not found.");
            }

            HMSDashboard hmsrecord = await _context.HMSDashboard.FirstOrDefaultAsync(x => x.UserId == UserId);

            if (hmsrecord == null)
            {
                _logger.LogError("HMSDashboard data not found for UserId: {UserId}", UserId);
                return NotFound("HMSDashboard data not found for UserId: " + UserId.ToString());
            }

            //var filteredList = await _context.ChannelDetails.Where(y => y.UserId == UserId) .ToListAsync();

            //int totalCount = _context.ChannelDetails.Count();

            //var channelDetails1 = await _context.ChannelDetails.FirstOrDefaultAsync(y => y.UserId == UserId);

            //List<ChannelDetails> channelDetails = _context.ChannelDetails.Where(y => y.UserId == UserId).ToList();

            //  await _context.ChannelDetails.Select(y => y.UserId == UserId).ToList();

            //hmsrecord.channelDetails = channelDetails;

            //int totalCount = _context.StatusDetails.Count();
            //StatusDetails statusDetails = await _context.StatusDetails.FirstOrDefaultAsync(y => y.UserId == UserId);



            HMSDashboard hmsDashboard = new HMSDashboard();

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

            return Ok(hmsrecord);
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet("GetCommissionDashboard")]
        public async Task<ActionResult> GetCommissionDashboard(Int32? UserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            CommissionDashboard CommissionDashboard = new CommissionDashboard();
            return Ok(CommissionDashboard);
        }

    }
}
