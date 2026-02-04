using CommonLibrary;
using HMS.Data;
using HMS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DTO;
using Models.HMSConsts;
using System.Security.Claims;

namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HMSDashboardController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<HMSDashboardController> _logger;
        private readonly IAuthClaimService _authClaimService;

        public HMSDashboardController(HMSContext context, IConfiguration config, ILogger<HMSDashboardController> logger,IAuthClaimService authClaimService)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _authClaimService = authClaimService;
        }

        [HttpPost("GetHMSDashboard")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<ActionResult> GetHMSDashboard()
        {
            HmsResponse hmsResponse = new HmsResponse();
            try
            {
                var username = _authClaimService.GetClaim(ClaimTypes.Name);

                if (username == null)
                {
                    return NotFound("User not found.");
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return NotFound("UserId claim not found or invalid.");
                }

                var hmsrecord = await _context.HMSDashboard.FirstOrDefaultAsync(x => x.UserId == userId);

                #region Later Integration
                //var today = DateTime.UtcNow;
                //var firstDayThisMonth = new DateTime(today.Year, today.Month, 1);
                //var firstDayPrevMonth = firstDayThisMonth.AddMonths(-1);
                //// Total Entities
                //hmsrecord.TotalEntitiesCount = await _context.Agents.CountAsync();

                //// Created This Month
                //hmsrecord.EntitiesCreatedThisMonth = await _context.Agents
                //    .CountAsync(a => a.CreatedDate >= firstDayThisMonth);

                //// Created Prev Month (For calculating the trend like +120)
                //hmsrecord.EntitiesCreatedPrevMonth = await _context.Agents
                //    .CountAsync(a => a.CreatedDate >= firstDayPrevMonth && a.CreatedDate < firstDayThisMonth);

                //// Terminated This Month (Assuming is_active false means terminated)
                //hmsrecord.EntitiesTerminatedThisMonth = await _context.Agents
                //    .CountAsync(a => !a.IsActive && a.ModifiedDate >= firstDayThisMonth);

                //// Net Entities (Created - Terminated)
                //hmsrecord.EntitiesNetThisMonth = hmsrecord.EntitiesCreatedThisMonth - hmsrecord.EntitiesTerminatedThisMonth;

                //// Licenses Expiring in 30 Days
                //var expiryThreshold = today.AddDays(30);
                //hmsrecord.LicenseExpiringIn30Months = await _context.Agents
                //    .CountAsync(a => a.LicenseExpiryDate >= today && a.LicenseExpiryDate <= expiryThreshold);

                //// 4. CALCULATE THE TREND (The +120 logic)
                //// Note: You can add a property to your DTO specifically for "TrendValue" 
                //// which is: (EntitiesCreatedThisMonth - EntitiesCreatedPrevMonth)
                //hmsrecord.TotalEntitiesThisMonth = hmsrecord.EntitiesCreatedThisMonth - hmsrecord.EntitiesCreatedPrevMonth;
                #endregion
                if (hmsrecord != null)
                {
                    hmsResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    hmsResponse.responseHeader.ErrorMessage = "Success";
                    hmsResponse.responseBody.hmsDashboard = hmsrecord;
                    return Ok(hmsResponse);
                }
                else
                {
                    return NotFound("Hms Record Not Found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching HMS dashboard for User");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
