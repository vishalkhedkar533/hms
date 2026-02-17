using CommonLibrary;
using HMS.Caching;
using HMS.Data;
using HMS.Security;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;
using System.Security.Claims;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //Use this controllers to read master and keep this in cache
    public class AppMastersController : ControllerBase
    {
        private readonly GenericCacheService _cacheService;
        private readonly IConfiguration _configuration;
        private readonly IAuthClaimService _authClaimService;
        private readonly FileService _fileService;
        private int refreshInterval = 15;
        private readonly HMSContext _context;
        private int orgId;
        private readonly ILogger<AppMastersController> _logger;
        public AppMastersController(HMSContext context, GenericCacheService cacheService, IConfiguration configuration
            , IAuthClaimService authClaimService, FileService fileService, ILogger<AppMastersController> logger)
        {
            _cacheService = cacheService;
            _configuration = configuration;
            _authClaimService = authClaimService;
            _fileService = fileService;
            int refreshInterval = _configuration.GetValue<int>("Caching:refreshIntervalMinutes", 15);
            _context = context;
            _logger = logger;
        }

        // 🔹 Fetch records (dynamic) - uses refreshInterval from appsettings.json
        // POST api/cache/get/hms/customer
        [HttpPost("get/{EntryCategory}")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> GetRecords([FromRoute] string EntryCategory)
        {
            HmsResponse hMSResponse = new HmsResponse();
            List<KeyValueEntry>? result = null;
            var masterTableConfigs = (await _cacheService.GetRecordsAsync<MasterTable>(
                    "hmsmaster",
                    "mastertables",
                    Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"),
                    $" AND EntryCategory = '{EntryCategory}'",
                    "",
                    refreshInterval))
                .ToList().FirstOrDefault();

            if (!IsValidSchema(masterTableConfigs?.SchemaName ?? string.Empty))
            {
                hMSResponse.responseHeader.ErrorCode = MastersConstants.MASTER_NOTFOUND;
                hMSResponse.responseHeader.ErrorMessage = await _context.errorMaster
                    .Where(x => x.ErrorId == MastersConstants.MASTER_NOTFOUND && x.Area == "MasterConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";
                return NotFound(hMSResponse);
            }
            else
            {
                result = (await _cacheService.GetRecordsAsync<KeyValueEntry>(
                    masterTableConfigs?.SchemaName
                    , masterTableConfigs?.TableName
                    , Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0")
                    , (masterTableConfigs?.FilterCriteria ?? string.Empty)
                    , (masterTableConfigs?.columnalias ?? string.Empty)
                    , refreshInterval)).ToList();

                if (result == null)
                {
                    hMSResponse.responseHeader.ErrorCode = MastersConstants.MASTER_NOTFOUND;
                    hMSResponse.responseHeader.ErrorMessage = await _context.errorMaster
                        .Where(x => x.ErrorId == MastersConstants.MASTER_NOTFOUND && x.Area == "MasterConstants")
                        .Select(x => x.ErrorMsg)
                        .FirstOrDefaultAsync() ?? "Undefined Error Message";

                }
                else
                {
                    hMSResponse.responseBody.master = result;
                    hMSResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    hMSResponse.responseHeader.ErrorMessage = "SUCCESS";
                }
            }
            return result == null ? NotFound(hMSResponse) : Ok(hMSResponse);
        }

        // 🔹 Refresh table (dynamic)
        // POST api/cache/refresh/hms/customer
        [HttpPost("refresh/{EntryCategory}")]
        public async Task<IActionResult> Refresh([FromRoute] string EntryCategory)
        {

            var masterTableConfigs = (await _cacheService.GetRecordsAsync<MasterTable>(
                    "hmsmaster",
                    "mastertables",
                    Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"),
                    $" AND EntryCategory = '{EntryCategory}'",
                    "",
                    refreshInterval))
                .ToList().FirstOrDefault();

            if (!IsValidSchema(masterTableConfigs?.SchemaName ?? string.Empty))
                return Forbid("Access denied: invalid input." + EntryCategory);

            var result = await _cacheService.RefreshCacheAsync(masterTableConfigs.SchemaName
                , masterTableConfigs.TableName
                , Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0")
                , EntryCategory );
            return Ok(result);
        }

        // 🔹 Evict table
        // POST api/cache/evict/hms/customer
        [HttpPost("evict/{EntryCategory}")]
        public IActionResult Evict([FromRoute] string EntryCategory)
        {

            var masterTableConfigs = (_cacheService.GetRecordsAsync<MasterTable>(
                    "hmsmaster",
                    "mastertables",
                    Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0"),
                    $" AND EntryCategory = '{EntryCategory}'",
                    string.Empty,
                    refreshInterval)).Result.ToList().FirstOrDefault();


            if (!IsValidSchema(masterTableConfigs?.SchemaName ?? string.Empty))
                return Forbid("Access denied: invalid input." + EntryCategory);

            _cacheService.EvictCache(masterTableConfigs?.SchemaName
                , masterTableConfigs?.TableName
                , Convert.ToInt64(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0")
                , EntryCategory);

            return Ok($"Cache for {masterTableConfigs?.SchemaName}.{masterTableConfigs?.TableName} evicted.");
        }

        // 🔹 Evict entire schema
        // POST api/cache/evict/schema/hms
        [HttpPost("evict/schema/{schema}")]
        public IActionResult EvictSchema(string schema)
        {
            if (!IsValidSchema(schema)) return Forbid("Access denied: invalid schema." + schema);
            _cacheService.EvictSchema(schema);
            return Ok($"All cache entries for schema {schema} evicted.");
        }

        // 🔹 Refresh entire schema
        // POST api/cache/refresh/schema/{schema}
        //[HttpPost("refresh/schema/{schema}")]
        //public async Task<IActionResult> RefreshSchema(string schema)
        //{
        //    if (!IsValidSchema(schema)) return Forbid("Access denied: invalid schema." + schema);
        //    await _cacheService.RefreshSchemaAsync(schema);
        //    return Ok($"Schema {schema} refreshed.");
        //}

        private bool IsValidSchema(string schema) => string.Equals(schema, "hmsmaster", StringComparison.OrdinalIgnoreCase);

        // POST: api/hms/channelmaster
        [HttpPost("Channel/Create")]
        [MenuAuthorize(AuthorisationConstants.CreateUpdateDeleteChannel)]
        public async Task<IActionResult> Create([FromBody] ChannelMasterDto dto)
        {
            var response = new HmsResponse();
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            if (dto is null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Use provided OrgId from DTO. If you want to pull from claims, adapt here.
                var channelMaster = await _context.ChannelMaster.AddAsync(new ChannelMaster
                {
                    ChannelCode = dto.ChannelCode,
                    ChannelName = dto.ChannelName,
                    Description = dto.Description,
                    IsActive = dto.IsActive,
                    OrgId = orgId,
                    CreatedBy = _authClaimService.GetClaim(ClaimTypes.NameIdentifier),
                    CreatedDate = DateTime.UtcNow,
                    RowVersion = 1,
                    CreatedEntities = 0,
                    TerminatedEntities = 0,
                    TotalEntities = 0,
                });
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "Channel master created successfully.";
                var result = await _context.SaveChangesAsync();
                response.responseBody.channels = new List<ChannelMaster>();
                response.responseBody.channels.Add(channelMaster.Entity);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting into hmsmaster.channel_master. DTO: {@dto}", dto);
                return StatusCode(500, response);
            }

        }

        [HttpPost("Channel/Update/{ChannelId}")]
        public async Task<IActionResult> Update([FromRoute] int ChannelId, [FromBody] ChannelMasterDto ChannelMaster)
        {
            var response = new HmsResponse();
            if (ChannelMaster is null)
                return BadRequest("Request body is required.");

            if (ChannelMaster.ChannelId.HasValue && ChannelMaster.ChannelId.Value != ChannelId)
                return BadRequest("URL id does not match body id.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
                var channel =_context.ChannelMaster.FirstOrDefault(x=> x.ChannelId == ChannelMaster.ChannelId && x.OrgId == orgId);

                if (channel ==  null)
                {
                    response.responseHeader.ErrorCode = MastersConstants.MASTER_NOTFOUND;
                    response.responseHeader.ErrorMessage = await _context.errorMaster
                        .Where(x => x.ErrorId == MastersConstants.MASTER_NOTFOUND && x.Area == "MasterConstants")
                        .Select(x => x.ErrorMsg)
                        .FirstOrDefaultAsync() ?? "Undefined Error Message";
                    return NotFound(response);
                }

                channel.ChannelCode = ChannelMaster.ChannelCode;
                channel.ChannelName = ChannelMaster.ChannelName;
                await _context.SaveChangesAsync();
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = await _context.errorMaster
                        .Where(x => x.ErrorId == CommonConstants.SUCCESS && x.Area == "Common")
                        .Select(x => x.ErrorMsg)
                        .FirstOrDefaultAsync() ?? "Undefined Error Message"; ;
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating hmsmaster.channel_master id {ChannelMaster.ChannelId}");
                return StatusCode(500, "An error occurred while updating the channel master.");
            }
        }
    }
}
//new Comment