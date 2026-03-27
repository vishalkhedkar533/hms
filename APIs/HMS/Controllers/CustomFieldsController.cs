using CommonLibrary;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomFieldsController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly IAuthClaimService _authClaimService;
        private readonly ILogger<CustomFieldsController> _logger;

        public CustomFieldsController(HMSContext context, IAuthClaimService authClaimService,
            ILogger<CustomFieldsController> logger)
        {
            _context = context;
            _authClaimService = authClaimService;
            _logger = logger;
        }

        [HttpPost("Save")]
        [Authorize]
        public async Task<IActionResult> Save([FromBody] SaveCustomFieldRequest request)
        {
            HmsResponse response = new HmsResponse();

            if (string.IsNullOrWhiteSpace(request.Key) ||
                string.IsNullOrWhiteSpace(request.Label) ||
                string.IsNullOrWhiteSpace(request.Type) ||
                string.IsNullOrWhiteSpace(request.TargetScreen) ||
                string.IsNullOrWhiteSpace(request.TargetTab) ||
                string.IsNullOrWhiteSpace(request.TargetSection))
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Key, Label, Type, TargetScreen, TargetTab and TargetSection are required.";
                return BadRequest(response);
            }

            int orgId = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

            try
            {
                var entity = new CustomField
                {
                    Key = request.Key,
                    Label = request.Label,
                    Type = request.Type,
                    Placeholder = request.Placeholder,
                    Required = request.Required,
                    MinLength = request.MinLength,
                    MaxLength = request.MaxLength,
                    Pattern = request.Pattern,
                    ValidationMessage = request.ValidationMessage,
                    Options = request.Options,
                    TargetScreen = request.TargetScreen,
                    TargetTab = request.TargetTab,
                    TargetSection = request.TargetSection,
                    SortOrder = request.SortOrder,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    OrgId = orgId
                };

                _context.CustomFields.Add(entity);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "Success";
                response.responseBody.FieldId = entity.FieldId;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving custom field for screen: {Screen}, tab: {Tab}", request.TargetScreen, request.TargetTab);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("Fetch")]
        [Authorize]
        public async Task<IActionResult> Fetch([FromBody] FetchCustomFieldsRequest request)
        {
            HmsResponse response = new HmsResponse();

            if (string.IsNullOrWhiteSpace(request.TargetScreen) ||
                string.IsNullOrWhiteSpace(request.TargetTab))
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "TargetScreen and TargetTab are required.";
                return BadRequest(response);
            }

            int orgId = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

            try
            {
                var query = _context.CustomFields.AsNoTracking()
                    .Where(f => f.TargetScreen == request.TargetScreen
                             && f.TargetTab == request.TargetTab
                             && f.OrgId == orgId);

                if (request.IsActive.HasValue)
                    query = query.Where(f => f.IsActive == request.IsActive.Value);

                var fields = await query
                    .OrderBy(f => f.SortOrder)
                    .Select(f => new CustomFieldDto
                    {
                        FieldId = f.FieldId,
                        Key = f.Key,
                        Label = f.Label,
                        Type = f.Type,
                        Placeholder = f.Placeholder,
                        Required = f.Required,
                        MinLength = f.MinLength,
                        MaxLength = f.MaxLength,
                        Pattern = f.Pattern,
                        ValidationMessage = f.ValidationMessage,
                        Options = f.Options,
                        TargetScreen = f.TargetScreen,
                        TargetTab = f.TargetTab,
                        TargetSection = f.TargetSection,
                        SortOrder = f.SortOrder,
                        IsActive = f.IsActive
                    })
                    .ToListAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "Success";
                response.responseBody.CustomFields = fields;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching custom fields for screen: {Screen}, tab: {Tab}", request.TargetScreen, request.TargetTab);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
