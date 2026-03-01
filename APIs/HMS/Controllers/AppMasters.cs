using AutoMapper;
using CommonLibrary;
using HMS.Caching;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;
using SharedModels.BackEndCalculation;
using System.Security.Claims;
using ChannelMaster = Models.DB.ChannelMaster;
using DesignationMaster = Models.DB.DesignationMaster;
using MasterTable = Models.DTO.MasterTable;

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
        private readonly IMapper _mapper;
        private int refreshInterval = 15;
        private readonly HMSContext _context;
        private int orgId;
        private readonly ILogger<AppMastersController> _logger;
        private readonly DatabaseService _db;
        public AppMastersController(HMSContext context, GenericCacheService cacheService, IConfiguration configuration
            , IAuthClaimService authClaimService, FileService fileService, ILogger<AppMastersController> logger,
            DatabaseService db, IMapper mapper)
        {
            _cacheService = cacheService;
            _configuration = configuration;
            _authClaimService = authClaimService;
            _fileService = fileService;
            int refreshInterval = _configuration.GetValue<int>("Caching:refreshIntervalMinutes", 15);
            _context = context;
            _logger = logger;
            _db = db;
            _mapper = mapper;
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
        [HttpPost("Channel/Fetch")]
        [MenuAuthorize(AuthorisationConstants.CreateUpdateDeleteChannel)]
        public async Task<IActionResult> ChannelFetch([FromBody] ChannelMasterDto dto)
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
                var channelMaster = _context.ChannelMaster.AsNoTracking().Where(x => x.OrgId == orgId &&
                x.ChannelId == (dto.ChannelId == null ? x.ChannelId : dto.ChannelId) &&
                x.ChannelCode == (dto.ChannelCode == null ? x.ChannelCode : dto.ChannelCode)).ToList();
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "Fetched Channel List";
                response.responseBody.channels = channelMaster;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting into hmsmaster.channel_master. DTO: {@dto}", dto);
                return StatusCode(500, response);
            }

        }
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
            if (_context.ChannelMaster.Any(x => x.OrgId == orgId && x.ChannelCode == dto.ChannelCode))
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "A channel with the same code already exists.";
                return Conflict(response);
            }
            try
            {
                // Use provided OrgId from DTO. If you want to pull from claims, adapt here.
                var channelMaster = await _context.ChannelMaster.AddAsync(new ChannelMaster
                {
                    ChannelCode = dto.ChannelCode,
                    ChannelName = dto.ChannelName,
                    Description = dto.Description,
                    IsActive = dto.IsActive ?? true,
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
                try
                {
                    var subChannelMaster = await _context.SubchannelMaster.AddAsync(new SubChannelMaster
                    {
                        SubChannelCode = channelMaster.Entity.ChannelCode,
                        ChannelId = channelMaster.Entity.ChannelId,
                        ChannelCode = channelMaster.Entity.ChannelCode,
                        SubChannelName = channelMaster.Entity.ChannelName,
                        Description = $" Default Sub Channel Created for {channelMaster.Entity.Description}",
                        IsActive = true,
                        OrgId = orgId,
                        CreatedBy = _authClaimService.GetClaim(ClaimTypes.NameIdentifier),
                        CreatedDate = DateTime.UtcNow,
                        RowVersion = 1,
                    });
                    _context.SaveChangesAsync();
                }
                catch (Exception exDfltSubChannel)
                {
                    _logger.LogError(exDfltSubChannel, "Error inserting into hmsmaster.subchannel_master. DTO: {@dto}", dto);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting into hmsmaster.channel_master. DTO: {@dto}", dto);
                return StatusCode(500, response);
            }

        }
        [HttpPost("{ChannelId}/Update")]
        [MenuAuthorize(AuthorisationConstants.CreateUpdateDeleteChannel)]
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
        [HttpPost("{ChannelId}/SubChannel/Fetch")]
        [MenuAuthorize(AuthorisationConstants.CreateUpdateDeleteChannel)]
        public async Task<IActionResult> SubChannelFetch([FromRoute] int ChannelId, [FromBody] SubChannelMasterDto SubChannelMaster)
        {
            var response = new HmsResponse();
            if (SubChannelMaster is null)
                return BadRequest("Request body is required.");

            if (SubChannelMaster?.ChannelId != ChannelId)
                return BadRequest("URL Channel ID does not match body id.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
                var IsChannelValid = _context.ChannelMaster.AsNoTracking().Any(x => x.ChannelId == SubChannelMaster.ChannelId
                && x.OrgId == orgId);

                if (!IsChannelValid)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "No Channel Found";
                    return NotFound(response);
                }

                var subChannels = _context.SubchannelMaster.AsNoTracking().
                    Where(x => x.ChannelId == SubChannelMaster.ChannelId && x.OrgId == orgId
                    && x.SubChannelId == (SubChannelMaster.SubChannelId == null ? x.SubChannelId : SubChannelMaster.SubChannelId)
                    && x.SubChannelCode == (SubChannelMaster.SubChannelCode == null ? x.SubChannelCode : SubChannelMaster.SubChannelCode)
                    ).
                    ToList();

                if (subChannels == null || subChannels.Count == 0)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "No SubChannel Found";
                    return NotFound(response);
                }
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = await _context.errorMaster
                        .Where(x => x.ErrorId == CommonConstants.SUCCESS && x.Area == "Common")
                        .Select(x => x.ErrorMsg)
                        .FirstOrDefaultAsync() ?? "Undefined Error Message";
                response.responseBody.subChannels = subChannels;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching sub Channel master");
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

                var Channel = await _context.ChannelMaster.AsNoTracking().
                    FirstOrDefaultAsync(x => x.ChannelId == ChannelId && x.OrgId == orgId);

                if (Channel == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid Channel ID";
                    return BadRequest(response);
                }
                if (_context.SubchannelMaster.AsNoTracking().
                    Any(x => x.OrgId == orgId && x.ChannelId == Channel.ChannelId &&
                    x.SubChannelCode == SubChannelMaster.SubChannelCode))
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "A SubChannel with the same code already exists.";
                    return Conflict(response);
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

            if (designationMaster == null)
            {
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

                // SAFELY build hierarchy path as a string.
                // Avoid using Enumerable.Concat which produces an iterator type when called on a non-string enumerable.
                var parentPathStr = parentDesignation?.HierarchyPath?.ToString() ?? string.Empty;
                // If parentPathStr is empty, the hierarchy path is just the current designation id.
                string heirarchyPath = string.IsNullOrEmpty(parentPathStr)
                    ? designation.DesignationId.ToString()
                    : parentPathStr + "." + designation.DesignationId.ToString();

                await _db.ExecuteQueryAsync<string>(
                            "Master",
                            "UpdateDesignation",
                            new
                            {
                                p_hierarchy_path = heirarchyPath,
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
        [HttpPost("{ChannelId}/{SubChannelId}/Designation/Fetch")]
        [MenuAuthorize(AuthorisationConstants.SaveChannelDetails)]
        public async Task<IActionResult> FetchDesignation([FromRoute] long ChannelId,
           [FromRoute] long SubChannelId,
           [FromBody] DesignationMasterDto designationMaster)
        {
            HmsResponse response = new HmsResponse();

            if (designationMaster == null)
            {
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


            try
            {

                // 1. Fetch data. EF Core now knows how to handle HierarchyPath (LTree)
                var flatList = await _context.DesignationMaster.AsNoTracking()
                    .Where(d => d.OrgId == orgId
                             && d.ChannelId == designationMaster.ChannelId
                             && d.SubChannelId == designationMaster.SubChannelId)
                    .OrderBy(d => d.HierarchyPath) // This now translates to: ORDER BY hierarchy_path
                    .ToListAsync();

                // 2. Build the tree in memory
                var nodeDict = flatList.ToDictionary(
                   d => d?.HierarchyPath?.ToString(),
                    d => new DesignationNode
                    {
                        Id = d.DesignationId,
                        Name = d.DesignationName,
                        Code = d.DesignationCode
                    });

                List<DesignationNode> rootNodes = new();

                foreach (var item in flatList)
                {
                    string path = item.HierarchyPath.ToString();
                    var node = nodeDict[path];

                    // Find parent path using LTree logic
                    int lastDot = path.LastIndexOf('.');

                    if (lastDot == -1)
                    {
                        rootNodes.Add(node); // It's a root
                    }
                    else
                    {
                        string parentPath = path.Substring(0, lastDot);
                        if (nodeDict.TryGetValue(parentPath, out var parentNode))
                        {
                            parentNode.ReportingDesignations.Add(node);
                        }
                    }
                }
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "Designations fetched successfully.";
                response.responseBody.designationHierarchy = rootNodes;
                return Ok(response); // This is your nested JSON-ready object
            }
            catch (Exception ex)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "An error occurred while fetching designations.";
                _logger.LogError(ex, $"Error fetching designations for ChannelID {designationMaster.ChannelId} and SubChannelID {designationMaster.SubChannelId}");
                return Conflict(response);
            }
        }
        [HttpPost("{ChannelId}/{SubChannelId}/Location/Save")]
        [MenuAuthorize(AuthorisationConstants.SaveChannelDetails)]
        public async Task<IActionResult> UpsertLocation([FromRoute] long ChannelId,
            [FromRoute] long SubChannelId, [FromBody] LocationMasterDto locationMaster)
        {
            HmsResponse response = new HmsResponse();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            // 1. UNIQUE CONSTRAINT CHECK
            // Check if another record (not this one) already uses the same unique combination

            if (ChannelId != locationMaster.ChannelId ||
                SubChannelId != locationMaster.SubChannelId)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Verify the channel and subchannel";
                return Conflict(response);
            }

            var validChannelSubChannel = await _context.SubchannelMaster.AsNoTracking().AnyAsync(x =>
                x.OrgId == orgId &&
                x.ChannelId == locationMaster.ChannelId &&
                x.SubChannelId == locationMaster.SubChannelId);
            if (!validChannelSubChannel)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Invalid channel and subchannel.";
                return Conflict(response);
            }

            var isDuplicate = await _context.LocationMasters.AsNoTracking().AnyAsync(x =>
                x.LocationMasterId != locationMaster.LocationMasterId && // Don't check against itself during update
                x.OrgId == orgId &&
                x.ChannelId == locationMaster.ChannelId &&
                x.SubChannelId == locationMaster.SubChannelId &&
                x.LocationCode == locationMaster.LocationCode);

            if (isDuplicate)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "A location with the same code already exists for this channel and subchannel.";
                return Conflict(response);
            }

            // 2. FIND EXISTING OR CREATE NEW
            var existingLocation = await _context.LocationMasters
                .FirstOrDefaultAsync(x => x.LocationMasterId == locationMaster.LocationMasterId);
            try
            {

                if (existingLocation == null)
                {
                    // --- INSERT ---
                    var newLocation = _mapper.Map<LocationMaster>(locationMaster);
                    newLocation.CreatedDate = DateTime.UtcNow;
                    newLocation.CreatedBy = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier));
                    _context.LocationMasters.Add(newLocation);
                    await _context.SaveChangesAsync();

                    response.responseBody.locations = new List<LocationMaster>();
                    response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    response.responseHeader.ErrorMessage = "Location created successfully.";
                    response.responseBody.locations.Add(newLocation);
                }
                else
                {
                    // --- UPDATE ---
                    _mapper.Map(locationMaster, existingLocation);
                    existingLocation.ModifiedDate = DateTime.UtcNow;
                    existingLocation.ModifiedBy = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier));
                    await _context.SaveChangesAsync();
                    response.responseBody.locations = new List<LocationMaster> { existingLocation };
                    response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    response.responseHeader.ErrorMessage = "Location updated successfully.";
                    response.responseBody.locations.Add(existingLocation);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Database error while upserting LocationMaster OrgID {orgId} ChannelID {locationMaster.ChannelId} SubChannelID {locationMaster.SubChannelId} LocationCode {locationMaster.LocationCode}");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Database error: Possible duplicate code or constraint violation.";
                return Conflict(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while upserting LocationMaster OrgID {orgId} ChannelID {locationMaster.ChannelId} SubChannelID {locationMaster.SubChannelId} LocationCode {locationMaster.LocationCode}");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "An unexpected error occurred while saving the location.";
                return StatusCode(500, response);
            }
            return Ok(response);
        }
        [HttpPost("{ChannelId}/{SubChannelId}/Location/Fetch")]
        [MenuAuthorize(AuthorisationConstants.SaveChannelDetails)]
        public async Task<IActionResult> FetcLocation([FromRoute] long ChannelId,
        [FromRoute] long SubChannelId, [FromBody] LocationMasterDto locationMaster)
        {
            HmsResponse response = new HmsResponse();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            // 1. UNIQUE CONSTRAINT CHECK
            // Check if another record (not this one) already uses the same unique combination
            if (locationMaster == null)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Invalid parameter";
                return BadRequest(response);
            }
            if (ChannelId != locationMaster.ChannelId ||
                SubChannelId != locationMaster.SubChannelId)
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Verify the channel and subchannel";
                return Conflict(response);
            }

            var locationList = _context.LocationMasters.AsNoTracking().Where(x =>
                x.OrgId == orgId
                && x.ChannelId == locationMaster.ChannelId
                && x.SubChannelId == locationMaster.SubChannelId
                && x.LocationCode == (string.IsNullOrEmpty(locationMaster.LocationCode) ? x.LocationCode : locationMaster.LocationCode)
                && x.LocationMasterId == (locationMaster.LocationMasterId ?? x.LocationMasterId)
                ).OrderByDescending(y => y.LocationCode).ThenBy(y => y.LocationDesc);

            if (locationList != null)
            {
                response.responseBody.locations = new List<LocationMaster>();
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "Location fetched successfully.";
                response.responseBody.locations.AddRange(locationList);
                return Ok(response);
            }
            else
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Location not found.";
                return NotFound(response);
            }
        }

        [HttpPost("{ChannelId}/{SubChannelId}/Branch/Save")]
        [MenuAuthorize(AuthorisationConstants.CreateUpdateDeleteChannel)]
        public async Task<IActionResult> InsertUpdateBranch([FromRoute] long ChannelId, [FromRoute] long SubChannelId,
        [FromBody] BranchMasterDto branchMasterDto)
        {
            var response = new HmsResponse();
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            var loggedInUserId = _authClaimService.GetClaim(ClaimTypes.NameIdentifier) ?? "0";
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (!(await _context.ChannelMaster.AsNoTracking().AnyAsync(x =>
                x.ChannelId == ChannelId && x.OrgId == orgId)))
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid ChannelId for the given Organisation.";
                    return Conflict(response);
                }

                if (!(await _context.SubchannelMaster.AsNoTracking().AnyAsync(x =>
                x.ChannelId == ChannelId && x.SubChannelId == SubChannelId && x.OrgId == orgId)))
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid SubChannelId for the given Organisation.";
                    return Conflict(response);
                }

                if (!(await _context.LocationMasters.AsNoTracking().AnyAsync(x =>
                x.ChannelId == ChannelId && x.SubChannelId == SubChannelId && x.OrgId == orgId
                && x.LocationMasterId == branchMasterDto.LocationMasterId)))
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid LocationMasterId for the given Channel and SubChannel.";
                    return Conflict(response);
                }
                var branch = await _context.BranchMaster.FirstOrDefaultAsync(x =>
                    x.BranchCode == branchMasterDto.BranchCode
                    && x.LocationMasterId == branchMasterDto.LocationMasterId
                    && x.OrgId == orgId);

                bool isNewBranch = branch == null;
                if (isNewBranch)
                {
                    branch = _mapper.Map<BranchMaster>(branchMasterDto);
                    branch.OrgId = orgId;
                    branch.CreatedBy = int.Parse(loggedInUserId);
                    branch.CreatedDate = DateTime.UtcNow;
                    _context.BranchMaster.Add(branch);
                }
                else
                {
                    _mapper.Map(branchMasterDto, branch);
                    branch.ModifiedBy = int.Parse(loggedInUserId);
                    branch.ModifiedDate = DateTime.UtcNow;
                    branch.CreatedDate = DateTime.SpecifyKind(branch.CreatedDate, DateTimeKind.Utc);
                    _context.BranchMaster.Update(branch);
                }

                await _context.SaveChangesAsync();

                var parentResult = await _db.ExecuteQueryAsync<ChannelBranchHeirarchy>(
                    "Master",
                    "GetBranchHierarchyByBranchId",
                    new
                    {
                        p_branchId = branchMasterDto.ParentBranchId,
                        p_orgId = orgId,
                        p_channelId = ChannelId,
                        p_subChannelId = SubChannelId
                    });

                var existingHierarchy = await _db.ExecuteQueryAsync<ChannelBranchHeirarchy>(
                    "Master",
                    "GetBranchHierarchyByBranchId",
                    new
                    {
                        p_branchId = branch.BranchId,
                        p_orgId = orgId,
                        p_channelId = ChannelId,
                        p_subChannelId = SubChannelId
                    });

                if (existingHierarchy == null)
                {
                    var newHierarchy = new ChannelBranchHeirarchy
                    {
                        OrgId = orgId,
                        ChannelId = ChannelId,
                        SubChannelId = SubChannelId,
                        CreatedBy = loggedInUserId,
                        CreatedDate = DateTime.UtcNow,
                        EffectiveFromDate = DateTime.UtcNow.Date,
                        HierarchyPath = null
                    };
                    _context.ChannelBranchHeirarchies.Add(newHierarchy);
                    await _context.SaveChangesAsync();
                }
                await _db.ExecuteQueryAsync<int>(
                    "Master",
                    "UpdateChildBranchesByBranchId",
                    new
                    {
                        p_oldParentPath = $"{existingHierarchy.FirstOrDefault()?.HierarchyPath ?? string.Empty}",
                        p_newParentPath = $"{parentResult.FirstOrDefault()?.HierarchyPath ?? string.Empty}.{branch.BranchId}",
                        p_branchId = branch.BranchId,
                        p_modifiedBy = loggedInUserId
                    });
                await transaction.CommitAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "Branch and Hierarchy saved successfully.";
                response.responseBody.branches = new List<BranchMaster> { branch };

                return Ok(response);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Record updated by another user");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during Branch and Hierarchy save.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{ChannelId}/{SubChannelId}/Branch/Fetch")]
        [MenuAuthorize(AuthorisationConstants.CreateUpdateDeleteChannel)]
        public async Task<IActionResult> FetchBranch([FromRoute] long ChannelId,
            [FromRoute] long SubChannelId, [FromBody] BranchMasterDto branchMaster)
        {
            var response = new HmsResponse();
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var flatList = await _context.ChannelBranchHeirarchies.AsNoTracking()
                    .Where(h => h.OrgId == orgId
                             && h.ChannelId == ChannelId
                             && h.SubChannelId == SubChannelId)
                    .OrderBy(h => h.HierarchyPath)
                    .ToListAsync();

                if (flatList.Count == 0)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Branch hierarchy not found.";
                    return NotFound(response);
                }

                long? ParseBranchId(string? path)
                {
                    if (string.IsNullOrWhiteSpace(path))
                        return null;

                    var lastSegment = path.Split('.').LastOrDefault();
                    return long.TryParse(lastSegment, out var id) ? id : null;
                }

                var branchIds = flatList
                    .Select(h => ParseBranchId(h.HierarchyPath))
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .Distinct()
                    .ToList();

                var branchMap = await _context.BranchMaster.AsNoTracking()
                    .Where(b => b.OrgId == orgId && branchIds.Contains(b.BranchId))
                    .ToDictionaryAsync(b => b.BranchId);

                var nodeDict = flatList
                    .Where(h => !string.IsNullOrWhiteSpace(h.HierarchyPath))
                    .ToDictionary(
                        h => h.HierarchyPath!,
                        h =>
                        {
                            var branchId = ParseBranchId(h.HierarchyPath) ?? 0;
                            branchMap.TryGetValue(branchId, out var branch);
                            return new BranchNode
                            {
                                Id = branchId,
                                Name = branch?.BranchName,
                                Code = branch?.BranchCode
                            };
                        });

                List<BranchNode> rootNodes = new();

                foreach (var item in flatList)
                {
                    if (string.IsNullOrWhiteSpace(item.HierarchyPath))
                        continue;

                    string path = item.HierarchyPath;
                    if (!nodeDict.TryGetValue(path, out var node))
                        continue;

                    int lastDot = path.LastIndexOf('.');

                    if (lastDot == -1)
                    {
                        rootNodes.Add(node);
                    }
                    else
                    {
                        string parentPath = path.Substring(0, lastDot);
                        if (nodeDict.TryGetValue(parentPath, out var parentNode))
                        {
                            parentNode.ReportingBranches.Add(node);
                        }
                    }
                }

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "Branch hierarchy fetched successfully.";
                response.responseBody.branchHierarchy = rootNodes;
                return Ok(response);
            }
            catch (DbUpdateException ex)
            {
                return Conflict(new { message = "Database constraint violation (check unique index)", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{ChannelId}/{SubChannelId}/PartnerBranchHierarchy/Save")]
        [MenuAuthorize(AuthorisationConstants.CreateUpdateDeleteChannel)]
        public async Task<IActionResult> SavePartnerBranchHierarchy(
            [FromRoute] long ChannelId,
            [FromRoute] long SubChannelId,
            [FromBody] PartnerBranchHierarchyDto dto)
        {
            // Initialize response wrapper correctly
            var hmsResponse = new HmsResponse { responseHeader = new HmsSResponseHeader() };

            var orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            var loggedInUserId = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier) ?? "0");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validation: Relation Manager
                bool agentExists = await _context.agent.AnyAsync(x => x.OrgId == orgId
                                            && x.AgentId == dto.RelationMgr
                                            && x.Channel == ChannelId
                                            && x.SubChannel == SubChannelId);

                if (!agentExists)
                {
                    hmsResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                    hmsResponse.responseHeader.ErrorMessage = "Relation Manager not found in the channel/subchannel.";
                    return Conflict(hmsResponse);
                }

                // 2. Fetch Parent Path
                string? parentPath = null;
                if (dto.ParentBranchHierarchyId.HasValue)
                {
                    parentPath = await _context.PartnerBranchHierarchies
                        .Where(x => x.PartnerBranchHeirarchyId == dto.ParentBranchHierarchyId)
                        .Select(x => x.HierarchyPath)
                        .FirstOrDefaultAsync();
                }

                // Determine if Update
                bool isUpdate = dto.PartnerBranchHierarchyId.HasValue && dto.PartnerBranchHierarchyId > 0;
                PartnerBranchHeirarchy? entity = null;

                if (isUpdate)
                {
                    entity = await _context.PartnerBranchHierarchies
                        .FirstOrDefaultAsync(x => x.PartnerBranchHeirarchyId == dto.PartnerBranchHierarchyId);

                    if (entity == null)
                    {
                        hmsResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                        hmsResponse.responseHeader.ErrorMessage = "Partner Branch Hierarchy not found.";
                        return NotFound(hmsResponse);
                    }

                    _mapper.Map(dto, entity);
                    entity.ModifiedBy = loggedInUserId;
                    entity.ModifiedDate = DateTime.UtcNow;

                    // Only update path if we have a parent
                    entity.HierarchyPath = string.IsNullOrEmpty(parentPath)
                        ? entity.PartnerBranchHeirarchyId.ToString()
                        : $"{parentPath}.{entity.PartnerBranchHeirarchyId}";
                }
                else
                {
                    entity = _mapper.Map<PartnerBranchHeirarchy>(dto);
                    entity.OrgId = orgId;
                    entity.ChannelId = ChannelId;
                    entity.SubChannelId = SubChannelId;
                    entity.CreatedBy = loggedInUserId;
                    entity.CreatedDate = DateTime.UtcNow;

                    await _context.PartnerBranchHierarchies.AddAsync(entity);
                    await _context.SaveChangesAsync(); // First save to generate Serial ID

                    entity.HierarchyPath = string.IsNullOrEmpty(parentPath)
                        ? entity.PartnerBranchHeirarchyId.ToString()
                        : $"{parentPath}.{entity.PartnerBranchHeirarchyId}";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                hmsResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hmsResponse.responseHeader.ErrorMessage = "Partner Branch Saved Successfully";
                hmsResponse.responseBody.PartnerBranchHierarchies = new List<PartnerBranchHeirarchy> { entity };
                return Ok(hmsResponse);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("{ChannelId}/{SubChannelId}/PartnerBranchHierarchy/Fetch")]
        [MenuAuthorize(AuthorisationConstants.CreateUpdateDeleteChannel)]
        public async Task<IActionResult> FetchPartnerBranchHierarchy([FromRoute] long ChannelId,
            [FromRoute] long SubChannelId, [FromBody] PartnerBranchHierarchySearchDto partnerBranchHierarchyDto)
        {
            var hmsResponse = new HmsResponse();
            var orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            var loggedInUserId = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier) ?? "0");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // 1. Fetch data. EF Core now knows how to handle HierarchyPath (LTree)
                var flatList = await _context.PartnerBranchHierarchies.AsNoTracking()
                            .Where(d => d.OrgId == orgId
                                     && d.ChannelId == ChannelId
                                     && d.SubChannelId == SubChannelId)
                            .OrderBy(d => d.HierarchyPath)
                            .ToListAsync();

                // 2. Build the tree in memory
                var nodeDict = flatList.ToDictionary(
                   d => d?.HierarchyPath?.ToString(),
                    d => new PartnerBranchNode
                    {
                        PartnerBranchHeirarchyId = d.PartnerBranchHeirarchyId,
                        Name = d.PartnerBranch,
                        PartnerBranchCode = d.PartnerBranchCode,
                        ChannelId = d.ChannelId,
                        SubChannelId = d.SubChannelId,
                        PartnerAddress = d.PartnerAddress,
                        PartnerMail = d.PartnerMail,
                        PartnerPhone = d.PartnerPhone,
                        RelationMgr = d.RelationMgr
                    });

                List<PartnerBranchNode> rootNodes = new();

                foreach (var item in flatList)
                {
                    string path = item.HierarchyPath.ToString();
                    var node = nodeDict[path];

                    // Find parent path using LTree logic
                    int lastDot = path.LastIndexOf('.');

                    if (lastDot == -1)
                    {
                        rootNodes.Add(node); // It's a root
                    }
                    else
                    {
                        string parentPath = path.Substring(0, lastDot);
                        if (nodeDict.TryGetValue(parentPath, out var parentNode))
                        {
                            parentNode.ReportingBranches?.Add(node);
                        }
                    }
                }
                hmsResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hmsResponse.responseHeader.ErrorMessage = "Partner Branch Updated Successfully";
                hmsResponse.responseBody.partnerBranchNode = rootNodes;
                return Ok(hmsResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("{ChannelId}/{SubChannelId}/GetAgents")]
        [MenuAuthorize(AuthorisationConstants.CreateUpdateDeleteChannel)]
        public async Task<IActionResult> GetAgents([FromRoute] long ChannelId,
            [FromRoute] long SubChannelId, [FromBody] SearchAgent searchAgent)
        {
            var response = new HmsResponse();
            var orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                if (!await _context.ChannelMaster.AsNoTracking()
                    .AnyAsync(x => x.ChannelId == ChannelId && x.OrgId == orgId))
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid Channel ID";
                    return Conflict(response);
                }

                if (!await _context.SubchannelMaster.AsNoTracking()
                    .AnyAsync(x => x.ChannelId == ChannelId && x.SubChannelId == SubChannelId && x.OrgId == orgId))
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid SubChannel ID";
                    return Conflict(response);
                }

                var agentsQuery = _context.agent.AsQueryable();

                if (!string.IsNullOrEmpty(searchAgent.SearchCondition))
                    agentsQuery = agentsQuery.Where(x => x.Channel == ChannelId && x.SubChannel == SubChannelId && x.OrgId == orgId
                    && (x.IrdaLicenseNumber == searchAgent.SearchCondition
                    || x.PanNumber == searchAgent.SearchCondition
                    || x.AgentCode == searchAgent.SearchCondition
                    || x.AgentName.Contains(searchAgent.SearchCondition)
                    || x.MobileNo == searchAgent.SearchCondition
                    || x.Email == searchAgent.SearchCondition
                    || x.GstNumber == searchAgent.SearchCondition));

                var agents = await agentsQuery.ToListAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "Agents fetched successfully.";
                response.responseBody.agents = new List<AgentDto>();
                foreach (var agent in agents)
                {
                    response.responseBody.agents.Add(new AgentDto
                    {
                        AgentId = agent.AgentId,
                        AgentCode = agent.AgentCode,
                        AgentName = agent.AgentName,
                        IrdaLicenseNumber = agent.IrdaLicenseNumber,
                        MobileNo = agent.MobileNo,
                        Email = agent.Email,
                        GstNumber = agent.GstNumber
                    });
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
    }
}