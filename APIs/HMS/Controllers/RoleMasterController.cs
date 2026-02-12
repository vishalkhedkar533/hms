using HMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.HMSConsts;
using SharedModels.BackEndCalculation;
using SharedModels.DTO;

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

        [HttpPost("RolesList")]
        public async Task<ActionResult<HmsResponse>> GetRoles()
        {
            var response = new HmsResponse();
            try
            {
                var roles = await _context.Roles.AsNoTracking().ToListAsync();
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
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

        [HttpPost("CreateRole")]
        public async Task<ActionResult<HmsResponse>> CreateRole([FromBody] RoleSaveDto roleDto)
        {
            var response = new HmsResponse();
            try
            {
                if (roleDto == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid role payload.";
                    return BadRequest(response);
                }

                if (!roleDto.Role_ID.HasValue)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Role ID is required.";
                    return BadRequest(response);
                }

                var nameExists = await _context.Roles.AnyAsync(r => r.RoleName.ToLower() == roleDto.RoleName.ToLower());

                if (nameExists)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = $"Role name '{roleDto.RoleName}' already exists.";
                    return Conflict(response); 
                }

                var newRole = new Role
                {
                    RoleId = (int)roleDto.Role_ID.Value,
                    RoleName = roleDto.RoleName,
                    Description = roleDto.Description,
                    IsSystemRole = roleDto.IsSystemRole,
                    IsActive = roleDto.IsActive,
                    CreatedBy = "Admin",
                    CreatedDate = DateTime.UtcNow,
                    RowVersion = 1
                };

                _context.Roles.Add(newRole);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.roles = new List<Role> { newRole };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create role");
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("UpdateRole/{id}")]
        public async Task<ActionResult<HmsResponse>> UpdateRole([FromRoute]int id, [FromBody] RoleSaveDto roleDto)
        {
            var response = new HmsResponse();
            try
            {
                if (roleDto == null || id != roleDto.Role_ID)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Mismatched or invalid role ID.";
                    return BadRequest(response);
                }

                var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == id);
                if (existingRole == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Role not found.";
                    return NotFound(response);
                }

                var nameExists = await _context.Roles
                    .AnyAsync(r => r.RoleName.ToLower() == roleDto.RoleName.ToLower() && r.RoleId != id);

                if (nameExists)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = $"The role name '{roleDto.RoleName}' is already assigned to another role.";
                    return Conflict(response);
                }

                existingRole.RoleName = roleDto.RoleName;
                existingRole.Description = roleDto.Description;
                existingRole.IsSystemRole = roleDto.IsSystemRole;
                existingRole.IsActive = roleDto.IsActive;

                existingRole.ModifiedBy = "Admin";
                existingRole.ModifiedDate = DateTime.UtcNow;
                if (existingRole.CreatedDate.Kind == DateTimeKind.Local)
                {
                    existingRole.CreatedDate = existingRole.CreatedDate.ToUniversalTime();
                }
                else if (existingRole.CreatedDate.Kind == DateTimeKind.Unspecified)
                {
                    existingRole.CreatedDate = DateTime.SpecifyKind(existingRole.CreatedDate, DateTimeKind.Utc);
                }
                if (roleDto.RowVersion.HasValue)
                {
                    existingRole.RowVersion = roleDto.RowVersion + 1;
                }

                _context.Roles.Update(existingRole);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.roles = new List<Role> { existingRole };

                return Ok(response);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict during update for ID {Id}", id);
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "The record was modified by another user. Please reload.";
                return StatusCode(409, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update role for ID {Id}", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("DeleteRole/{id}")]
        public async Task<ActionResult<HmsResponse>> DeleteRole([FromRoute]int id)
        {
            var response = new HmsResponse();
            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == id);
                if (role == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Role not found.";
                    return NotFound(response);
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.roles = new List<Role> { role };

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
    }
}
