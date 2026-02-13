using CommonLibrary;
using HMS.Data;
using HMS.Security;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;

namespace HMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessController : Controller
    {
        private readonly HMSContext _context;
        private readonly ILogger<AccessController> _logger;
        private readonly IAuthClaimService _authClaimService;
        private int orgId;
        public AccessController(HMSContext context, ILogger<AccessController> logger, IAuthClaimService authClaimService)
        {
            _context = context;
            _logger = logger;
            _authClaimService = authClaimService;
        }
        [HttpPost("Role/List")]
        [MenuAuthorize(1001)]
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
        [HttpPost("Role/Create")]
        [MenuAuthorize(1001)]
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
                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
                var nameExists = await _context.Roles.
                    AnyAsync(r => r.RoleName.ToLower() == roleDto.RoleName.ToLower()
                    && r.OrgId == orgId);

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
                    RowVersion = 1,
                    OrgId = orgId
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
        [HttpPost("Role/Update/{roleId}")]
        [MenuAuthorize(1001)]
        public async Task<ActionResult<HmsResponse>> UpdateRole([FromRoute] int roleId, [FromBody] RoleSaveDto roleDto)
        {
            var response = new HmsResponse();
            try
            {
                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
                if (roleDto == null || roleId != roleDto.Role_ID)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Mismatched or invalid role ID.";
                    return BadRequest(response);
                }

                var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId && r.OrgId == orgId);
                if (existingRole == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Role not found.";
                    return NotFound(response);
                }

                var nameExists = await _context.Roles
                    .AnyAsync(r => r.RoleName.ToLower() == roleDto.RoleName.ToLower()
                    && r.RoleId != roleId && r.OrgId == orgId);
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
                _logger.LogError(ex, "Concurrency conflict during update for ID {Id}", roleId);
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "The record was modified by another user. Please reload.";
                return StatusCode(409, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update role for ID {Id}", roleId);
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("Role/Delete/{roleId}")]
        [MenuAuthorize(1001)]
        public async Task<ActionResult<HmsResponse>> DeleteRole([FromRoute] int roleId)
        {
            var response = new HmsResponse();
            try
            {
                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId && r.OrgId == orgId);
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
        [HttpPost("Role/MenuAccess/{roleId}")]
        [MenuAuthorize(1001)]
        public async Task<ActionResult<HmsResponse>> RoleDetails([FromRoute] int roleId)
        {
            var response = new HmsResponse();
            try
            {
                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId && r.OrgId == orgId);
                if (role == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Role not found.";
                    return NotFound(response);
                }

                var menuAccessList = await (from mm in _context.MenuMasters.AsNoTracking()
                                            join parent in _context.MenuMasters.AsNoTracking()
                                                on mm.ParentMenuId equals parent.MenuId into parentJoin
                                            from parentMenu in parentJoin.DefaultIfEmpty()
                                            join rmm in _context.RoleMenuMapping
                                                .AsNoTracking()
                                                .Where(r => r.RoleId == roleId 
                                                && r.OrgId == orgId)
                                                on mm.MenuId equals rmm.MenuId into rmmJoin
                                            from mapping in rmmJoin.DefaultIfEmpty()
                                            select new MenuAccessDto
                                            {
                                                MenuId = mm.MenuId,
                                                MenuName = mm.MenuName,
                                                ParentMenuId = parentMenu != null ? (int?)parentMenu.MenuId : null,
                                                ParentMenuName = parentMenu != null ? parentMenu.MenuName : null,
                                                HasAccess = mapping != null
                                            })
                                           .ToListAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "SUCCESS";
                response.responseBody.MenuAccessList = menuAccessList;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch menu access for role {RoleId}", roleId);
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Internal server error.";
                return StatusCode(500, response);
            }
        }
        [HttpPost("Role/UserList/{roleId}")]
        [MenuAuthorize(1001)]
        public async Task<ActionResult<HmsResponse>> GetUserAccessForRole([FromRoute] int roleId)
        {
            var hMSResponse = new HmsResponse();
            try
            {
                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                List<UserListDto> userList =  await _context.Users
                .AsNoTracking()
                .Where(u => u.OrgId == orgId
                            && _context.UserRoleMappings.Any(urm => urm.UserId == u.UserId))
                .Select(u => new UserListDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    EmailId = u.EmailId,
                    IsActive = u.IsActive,
                    IsLocked = u.IsLocked,
                    FailedLoginAttempts = u.failedloginattempts
                })
                .ToListAsync();

                hMSResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hMSResponse.responseHeader.ErrorMessage = "SUCCESS";
                hMSResponse.responseBody.UserList =userList;

                return Ok(hMSResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch menu access for role {RoleId}", roleId);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}