using HMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;

namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleMasterController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly ILogger<RoleMasterController> _logger;

        public RoleMasterController(HMSContext context, ILogger<RoleMasterController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("List")]
        public async Task<ActionResult<HmsResponse>> GetRoles()
        {
            var response = new HmsResponse();
            try
            {
                var roles = await _context.RoleMasters.AsNoTracking().ToListAsync();
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = await GetErrorMessageAsync(CommonConstants.SUCCESS, "Common", "SUCCESS");
                response.responseBody.roles = roles;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch roles");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Internal server error.";
                return StatusCode(500, response);
            }
        }

        [HttpPost("Create")]
        public async Task<ActionResult<HmsResponse>> CreateRole([FromBody] RoleMaster role)
        {
            var response = new HmsResponse();
            try
            {
                if (role == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid role payload.";
                    return BadRequest(response);
                }

                var exists = await _context.RoleMasters.AnyAsync(r => r.ROLE_ID == role.ROLE_ID);
                if (exists)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Role already exists.";
                    return Conflict(response);
                }

                role.CreatedDate = DateTime.UtcNow;
                role.ModifiedDate = null;

                _context.RoleMasters.Add(role);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = await GetErrorMessageAsync(CommonConstants.SUCCESS, "Common", "SUCCESS");
                response.responseBody.roles = new List<RoleMaster> { role };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create role");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Internal server error.";
                return StatusCode(500, response);
            }
        }

        [HttpPut("Update/{id:long}")]
        public async Task<ActionResult<HmsResponse>> UpdateRole(long id, [FromBody] RoleMaster role)
        {
            var response = new HmsResponse();
            try
            {
                if (role == null || id != role.ROLE_ID)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid role payload.";
                    return BadRequest(response);
                }

                var existing = await _context.RoleMasters.FirstOrDefaultAsync(r => r.ROLE_ID == id);
                if (existing == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Role not found.";
                    return NotFound(response);
                }

                existing.RoleName = role.RoleName;
                existing.Description = role.Description;
                existing.IsSystemRole = role.IsSystemRole;
                existing.IsActive = role.IsActive;
                existing.ModifiedBy = role.ModifiedBy;
                existing.ModifiedDate = DateTime.UtcNow;
                existing.RowVersion = role.RowVersion;

                _context.RoleMasters.Update(existing);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = await GetErrorMessageAsync(CommonConstants.SUCCESS, "Common", "SUCCESS");
                response.responseBody.roles = new List<RoleMaster> { existing };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update role");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Internal server error.";
                return StatusCode(500, response);
            }
        }

        [HttpDelete("Delete/{id:long}")]
        public async Task<ActionResult<HmsResponse>> DeleteRole(long id)
        {
            var response = new HmsResponse();
            try
            {
                var role = await _context.RoleMasters.FirstOrDefaultAsync(r => r.ROLE_ID == id);
                if (role == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Role not found.";
                    return NotFound(response);
                }

                _context.RoleMasters.Remove(role);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = await GetErrorMessageAsync(CommonConstants.SUCCESS, "Common", "SUCCESS");
                response.responseBody.roles = new List<RoleMaster> { role };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete role");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Internal server error.";
                return StatusCode(500, response);
            }
        }

        private async Task<string> GetErrorMessageAsync(int errorId, string area, string fallback)
        {
            return await _context.errorMaster
                .Where(x => x.ErrorId == errorId && x.Area == area)
                .Select(x => x.ErrorMsg)
                .FirstOrDefaultAsync() ?? fallback;
        }
    }
}
