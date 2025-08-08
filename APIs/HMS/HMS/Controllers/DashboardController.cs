using Azure.Core;
using HMS.Data;
using HMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
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

        public DashboardController(HMSContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: api/<DashboardController>
        //[Authorize(Roles = "Admin")]
        [HttpGet("GetDashboard")]
        public async Task<ActionResult> GetDashboard(Int32? UserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (await _context.hmsDashboard.AnyAsync(u => u.UserId == UserId))
            {
                return Conflict("User data is not available.");
            }


            HmsDashboard hmsDashboard = new HmsDashboard();

            //ChannelDetails channelDetails = new ChannelDetails();
            //channelDetails.ChannelId = 1;
            //channelDetails.ChannelName = "Digital Banking";
            //channelDetails.TotalEntities = 12;
            //channelDetails.Created = 10;
            //channelDetails.Terminated = 2;
            //hmsDashboard.channelDetails.Add(channelDetails);

            //StatusDetails statusDetails = new StatusDetails();
            //statusDetails.StatusName = "Code Movement";
            //statusDetails.PendingItem = 10;
            //statusDetails.LastUpdated = "2 hours ago";
            //statusDetails.Priority = "High";
            //hmsDashboard.statusDetails.Add(statusDetails);

            return Ok(hmsDashboard);
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet("GetCommissionDashboard")]
        public async Task<ActionResult> GetCommissionDashboard(string userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == userName);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            CommissionDashboard CommissionDashboard = new CommissionDashboard();
            return Ok(CommissionDashboard);
        }

    }
}
