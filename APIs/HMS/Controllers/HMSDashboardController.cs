using CommonLibrary;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DB.Tasks.Models;
using Models.DTO;
using Models.HMSConsts;

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
        private readonly DatabaseService _db;

        public HMSDashboardController(HMSContext context, IConfiguration config, ILogger<HMSDashboardController> logger, IAuthClaimService authClaimService, DatabaseService db)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _authClaimService = authClaimService;
            _db = db;
        }

        [HttpPost("GetHMSDashboard")]
        [Authorize]
        [MenuAuthorize(AuthorisationConstants.AccessHMSDashboard)]
        public async Task<ActionResult> GetHMSDashboard()
        {
            HmsResponse hmsResponse = new HmsResponse();
            try
            {
                long orgId = long.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                var hmsrecord = await _context.HMSDashboard
                    .Where(x => x.OrgId == orgId)
                    .GroupBy(x => x.OrgId)
                    .Select(g => new HMSDashboard
                    {
                        OrgId = g.Key,
                        TotalEntitiesCount = g.Sum(x => x.TotalEntitiesCount),
                        TotalEntitiesThisMonth = g.Sum(x => x.TotalEntitiesThisMonth),
                        EntitiesCreatedThisMonth = g.Sum(x => x.EntitiesCreatedThisMonth),
                        EntitiesCreatedPrevMonth = g.Sum(x => x.EntitiesCreatedPrevMonth),
                        EntitiesTerminatedThisMonth = g.Sum(x => x.EntitiesTerminatedThisMonth),
                        EntitiesTerminatedPrevMonth = g.Sum(x => x.EntitiesTerminatedPrevMonth),
                        EntitiesNetThisMonth = g.Sum(x => x.EntitiesNetThisMonth),
                        LicenseExpiringIn30Months = g.Sum(x => x.LicenseExpiringIn30Months),
                        CertificateExpiringIn30Months = g.Sum(x => x.CertificateExpiringIn30Months),
                        MBGCriteriaNotMet = g.Sum(x => x.MBGCriteriaNotMet)
                    })
                    .FirstOrDefaultAsync();

                    hmsResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    hmsResponse.responseHeader.ErrorMessage = "Success";
                    hmsResponse.responseBody.hmsDashboard = hmsrecord;
                    return Ok(hmsResponse);

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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching aggregated HMS dashboard for OrgId");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("channel-stats")]
        [Authorize]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> GetChannelStats([FromBody] PaginationRequest paginationRequest)
        {
            ChannelStatsResponse response = new ChannelStatsResponse();
            var orgId = Convert.ToInt32(Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"));

            paginationRequest.PageNumber = paginationRequest.PageNumber <= 0 ? 1 : paginationRequest.PageNumber;
            paginationRequest.PageSize = paginationRequest.PageSize <= 0 ? 10 : paginationRequest.PageSize;

            try
            {
                var channels = (await _db.ExecuteQueryAsync<ChannelMaster>(
                    "Dashboard",
                    "GetChannelStats",
                    new { p_orgid = orgId }))
                    .ToList();

                if (channels.Any())
                {
                    var totalEntities = channels.Sum(x => x.TotalEntities ?? 0);
                    var terminatedEntities = channels.Sum(x => x.TerminatedEntities ?? 0);
                    var activeEntities = totalEntities - terminatedEntities;

                    var totalRecords = channels.Count;
                    var totalPages = (int)Math.Ceiling(totalRecords / (double)paginationRequest.PageSize);
                    var pagedChannels = channels
                        .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                        .Take(paginationRequest.PageSize)
                        .ToList();

                    response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    response.responseHeader.ErrorMessage = "SUCCESS";
                    response.responseBody.channels = pagedChannels;
                    response.responseBody.totalEntities = totalEntities;
                    response.responseBody.terminatedEntities = terminatedEntities;
                    response.responseBody.activeEntities = activeEntities;
                    response.responseBody.pagination = new
                    {
                        currentPage = paginationRequest.PageNumber,
                        totalPages = totalPages,
                        pageSize = paginationRequest.PageSize,
                        totalItems = totalRecords
                    };

                    return Ok(response);
                }

                response.responseHeader.ErrorCode = AgentConstants.AGENT_NOTFOUND;
                response.responseHeader.ErrorMessage = await _context.errorMaster.AsNoTracking()
                    .Where(x => x.ErrorId == AgentConstants.AGENT_NOTFOUND && x.Area == "AgentConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";

                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching channel stats for org {OrgId}", orgId);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
