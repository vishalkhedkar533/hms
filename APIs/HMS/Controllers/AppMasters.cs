using CommonLibrary;
using HMS.Caching;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;
using System.Security.Claims;
using System.Threading.Channels;

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
        private readonly DatabaseService _db;
        public AppMastersController(HMSContext context, GenericCacheService cacheService, IConfiguration configuration
            , IAuthClaimService authClaimService, FileService fileService, ILogger<AppMastersController> logger,
            DatabaseService db)
        {
            _cacheService = cacheService;
            _configuration = configuration;
            _authClaimService = authClaimService;
            _fileService = fileService;
            int refreshInterval = _configuration.GetValue<int>("Caching:refreshIntervalMinutes", 15);
            _context = context;
            _logger = logger;
            _db = db;
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
                , EntryCategory);
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
        [HttpPost("{ChannelId}/Update")]
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
                var channel = _context.ChannelMaster.FirstOrDefault(x => x.ChannelId == ChannelMaster.ChannelId && x.OrgId == orgId);

                if (channel == null)
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
        [HttpPost("{ChannelId}/SubChannel/Create")]
        [MenuAuthorize(AuthorisationConstants.CreateUpdateDeleteChannel)]
        public async Task<IActionResult> CreateSubChannel([FromRoute] int ChannelId, [FromBody] SubChannelMasterDto SubChannelMaster)
        {
            var response = new HmsResponse();
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            if (SubChannelMaster is null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (ChannelId != SubChannelMaster.ChannelId)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Channel ID in URL does not match Channel ID in body.";
                    return BadRequest(response);
                }

                var Channel = await _context.ChannelMaster.FirstOrDefaultAsync(x => x.ChannelId == ChannelId && x.OrgId == orgId);
                if (Channel == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid Channel ID";
                    return BadRequest(response);
                }
                // Use provided OrgId from DTO. If you want to pull from claims, adapt here.
                var subChannelMaster = await _context.SubchannelMaster.AddAsync(new SubChannelMaster
                {
                    SubChannelCode = SubChannelMaster.SubChannelCode,
                    ChannelId = Channel.ChannelId,
                    ChannelCode = Channel.ChannelCode,
                    SubChannelName = SubChannelMaster.SubChannelName,
                    Description = SubChannelMaster.Description,
                    IsActive = true,
                    OrgId = orgId,
                    CreatedBy = _authClaimService.GetClaim(ClaimTypes.NameIdentifier),
                    CreatedDate = DateTime.UtcNow,
                    RowVersion = 1,
                });
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = $"SubChannel master created successfully under Channel {Channel.ChannelName}";
                var result = await _context.SaveChangesAsync();
                response.responseBody.subChannels = new List<SubChannelMaster>();
                response.responseBody.subChannels.Add(subChannelMaster.Entity);
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = $"An error occurred while creating the sub channel master {SubChannelMaster.SubChannelName}";
                _logger.LogError(ex, response.responseHeader.ErrorMessage);
                return BadRequest(response);
            }

        }
        [HttpPost("{ChannelId}/{SubChannelId}/Update")]
        [MenuAuthorize(AuthorisationConstants.CreateUpdateDeleteChannel)]
        public async Task<IActionResult> UpdateSubChannel([FromRoute] int ChannelId, [FromRoute] int SubChannelId,
            [FromBody] SubChannelMasterDto subChannelMaster)
        {
            var response = new HmsResponse();
            if (subChannelMaster is null)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Request body is required.";
                return BadRequest(response);
            }

            if ((subChannelMaster.ChannelId.HasValue && subChannelMaster.ChannelId.Value != ChannelId) ||
                (subChannelMaster.SubChannelId.HasValue && subChannelMaster.SubChannelId.Value != SubChannelId))
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Channel Sub Channel ID does not match";
                return BadRequest(response);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
                var channel = _context.ChannelMaster.FirstOrDefault(x => x.ChannelId == subChannelMaster.ChannelId && x.OrgId == orgId);

                if (channel == null)
                {
                    response.responseHeader.ErrorCode = MastersConstants.MASTER_NOTFOUND;
                    response.responseHeader.ErrorMessage = await _context.errorMaster
                        .Where(x => x.ErrorId == MastersConstants.MASTER_NOTFOUND && x.Area == "MasterConstants")
                        .Select(x => x.ErrorMsg)
                        .FirstOrDefaultAsync() ?? "Undefined Error Message";
                    return NotFound(response);
                }

                var subChannel = _context.
                    SubchannelMaster.
                    FirstOrDefault(x => x.SubChannelId == subChannelMaster.SubChannelId
                    && x.OrgId == orgId
                    && x.ChannelId == subChannelMaster.ChannelId);

                subChannel.ChannelCode = channel.ChannelCode;
                subChannel.SubChannelCode = subChannelMaster.SubChannelCode;
                subChannel.SubChannelName = subChannelMaster.SubChannelName;
                subChannel.RowVersion = (subChannel.RowVersion ?? 0) + 1;
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
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "An error occurred while updating the sub channel master.";
                _logger.LogError(ex, $"Error updating ChannelID {subChannelMaster.ChannelId} and SubChannelID {subChannelMaster.SubChannelId}");
                return BadRequest(response);
            }
        }
        [HttpPost("{ChannelId}/{SubChannelId}/Designation/Save")]
        [MenuAuthorize(AuthorisationConstants.SaveChannelDetails)]
        public async Task<IActionResult> UpsertDesignation([FromRoute] long ChannelId,
            [FromRoute] long SubChannelId, 
            [FromBody] DesignationMasterDto designationMaster)
        {
            HmsResponse response = new HmsResponse();

            if (designationMaster == null) { 
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Request body is required.";
                return Conflict(response);
            }

            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            var LoggedInUserId = _authClaimService.GetClaim(ClaimTypes.NameIdentifier) ?? "Unknown";

            var channel = await _context.ChannelMaster.AsNoTracking().
                FirstOrDefaultAsync(x => x.ChannelId == designationMaster.ChannelId 
                && x.OrgId == orgId);

            var subChannel = await _context.SubchannelMaster.AsNoTracking().
                FirstOrDefaultAsync(x => x.SubChannelId == designationMaster.SubChannelId 
                && x.OrgId == orgId 
                && x.ChannelId == designationMaster.ChannelId);

            if (channel == null || 
                subChannel == null || 
                ChannelId != designationMaster.ChannelId || 
                SubChannelId != designationMaster.SubChannelId)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Verify the channel and subchannel belong to the organisation.";
                return Conflict(response);
            }

            // 1. Try to find existing record
            var designation = await _context.DesignationMaster
                .FirstOrDefaultAsync(x => x.DesignationCode == designationMaster.DesignationCode
                && x.OrgId == orgId
                && x.ChannelId == designationMaster.ChannelId
                && x.SubChannelId == designationMaster.SubChannelId
                );
            bool isNew = designation == null;

            var parentDesignation = await _context.DesignationMaster.AsNoTracking()
                .FirstOrDefaultAsync(x => x.DesignationId == (designationMaster.ParentDesignationId ?? -1000)
                && x.OrgId == orgId
                && x.ChannelId == designationMaster.ChannelId
                && x.SubChannelId == designationMaster.SubChannelId);

            if (isNew)
            {
                // Create new instance if not found
                designation = new DesignationMaster
                {
                    CreatedBy = LoggedInUserId, // Usually taken from User.Identity in production
                    CreatedDate = DateTime.UtcNow                    
                };
            }
            else
            {
                designation.ModifiedBy = LoggedInUserId;
                designation.ModifiedDate = DateTime.UtcNow; // Already UTC

                // Safety check: If the existing CreatedDate was loaded as 'Unspecified', 
                // force it to UTC so SaveChanges doesn't complain about it.
                if (designation.CreatedDate.Kind == DateTimeKind.Unspecified)
                {
                    designation.CreatedDate = DateTime.SpecifyKind(designation.CreatedDate, DateTimeKind.Utc);
                }
            }

            // 2. Map fields from DTO to Model
            designation.DesignationCode = designationMaster.DesignationCode;
            designation.DesignationName = designationMaster.DesignationName;
            designation.DesignationLevel = designationMaster.DesignationLevel;
            designation.IsActive = designationMaster.IsActive;
            designation.ChannelId = designationMaster.ChannelId;
            designation.OrgId = orgId;
            //ltree is not supported in EF Core, so we will handle HierarchyPath manually via a stored procedure after saving the record to get the generated DesignationId
            designation.HierarchyPath = null;
            designation.CodeFormat = designationMaster.CodeFormat;
            designation.SubChannelId = designationMaster.SubChannelId;

            if (isNew)
            {
                _context.DesignationMaster.Add(designation);
            }
            else
            {
                _context.DesignationMaster.Update(designation);
            }

            try
            {
                await _context.SaveChangesAsync();
                if (!string.IsNullOrEmpty(parentDesignation?.HierarchyPath))
                {
                    parentDesignation.HierarchyPath = parentDesignation.HierarchyPath.Concat(".").ToString();
                }
                await _db.ExecuteQueryAsync<string>(
                            "Channel",
                            "UpdateDesignation",
                            new
                            {
                                
                                p_hierarchy_path = string.Concat ((parentDesignation?.HierarchyPath?? string.Empty),designation.DesignationId.ToString()),
                                p_orgId = orgId,
                                p_channelID = designation.ChannelId,
                                p_subChannelId = designation.SubChannelId,
                                p_designation = designation.DesignationId
                            });

                // Update DTO with the generated ID if it was a new record
                designationMaster.DesignationId = designation.DesignationId;
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = isNew ? "Designation created successfully." : "Designation updated successfully.";
                response.responseBody.designations = new List<DesignationMaster>();
                response.responseBody.designations.Add(designation);

                return Ok(response);
            }
            catch (DbUpdateException ex)
            {
                // Handle Unique Constraint violations (DesignationCode, etc.)
                _logger.LogError(ex, $"Error updating DesignationMaster OrgID {orgId} ChannelID {designationMaster.ChannelId} SubChannelID {designationMaster.SubChannelId} DesignationCode {designationMaster.DesignationCode}");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Database error: Possible duplicate code or constraint violation.";
                return Conflict(response);
            }
        }

        [HttpPost("Branch/Create")]
        [MenuAuthorize(AuthorisationConstants.CreateUpdateDeleteChannel)]
        public async Task<IActionResult> CreateBranch([FromBody] BranchMasterDto dto)
        {
            var response = new HmsResponse();
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

            if (dto is null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var branch = await _context.BranchMaster.AddAsync(new BranchMaster
                {
                    BranchCode = dto.BranchCode ?? string.Empty,
                    BranchName = dto.BranchName ?? string.Empty,
                    Address = dto.Address,
                    State = dto.State,
                    PhoneNumber = dto.PhoneNumber,
                    EmailId = dto.EmailId,
                    IsActive = dto.IsActive,
                    LocationMasterId = dto.LocationMasterId,
                    OrgId = orgId,
                    CreatedBy = _authClaimService.GetClaim(ClaimTypes.NameIdentifier) ?? "System",
                    CreatedDate = DateTime.UtcNow,
                    RowVersion = 1
                });

                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "Branch master created successfully.";
                response.responseBody.branches = new List<BranchMaster> { branch.Entity };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting into hmsmaster.branch_master. DTO: {@dto}", dto);
                return StatusCode(500, response);
            }
        }

        [HttpPost("Branch/{BranchId}/Update")]
        [MenuAuthorize(AuthorisationConstants.CreateUpdateDeleteChannel)]
        public async Task<IActionResult> UpdateBranch([FromRoute] long BranchId, [FromBody] BranchMasterDto dto)
        {
            var response = new HmsResponse();

            if (dto is null)
                return BadRequest("Request body is required.");

            if (dto.BranchId.HasValue && dto.BranchId.Value != BranchId)
                return BadRequest("URL id does not match body id.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                var branch = await _context.BranchMaster
                    .FirstOrDefaultAsync(x => x.BranchId == BranchId && x.OrgId == orgId);

                if (branch == null)
                {
                    response.responseHeader.ErrorCode = MastersConstants.MASTER_NOTFOUND;
                    response.responseHeader.ErrorMessage = await _context.errorMaster
                        .Where(x => x.ErrorId == MastersConstants.MASTER_NOTFOUND && x.Area == "MasterConstants")
                        .Select(x => x.ErrorMsg)
                        .FirstOrDefaultAsync() ?? "Undefined Error Message";
                    return NotFound(response);
                }

                branch.BranchCode = dto.BranchCode ?? branch.BranchCode;
                branch.BranchName = dto.BranchName ?? branch.BranchName;
                branch.Address = dto.Address;
                branch.State = dto.State;
                branch.PhoneNumber = dto.PhoneNumber;
                branch.EmailId = dto.EmailId;
                branch.IsActive = dto.IsActive;
                branch.LocationMasterId = dto.LocationMasterId;
                branch.ModifiedBy = _authClaimService.GetClaim(ClaimTypes.NameIdentifier) ?? "System";
                branch.ModifiedDate = DateTime.UtcNow;
                branch.RowVersion = (branch.RowVersion ?? 0) + 1;

                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "Branch master updated successfully.";
                response.responseBody.branches = new List<BranchMaster> { branch };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hmsmaster.branch_master id {BranchId}", BranchId);
                return StatusCode(500, "An error occurred while updating the branch master.");
            }
        }
    }
}