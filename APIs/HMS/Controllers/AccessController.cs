using CommonLibrary;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Security.Claims;

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
        private readonly DatabaseService _db;
        public AccessController(HMSContext context, ILogger<AccessController> logger, IAuthClaimService authClaimService,
            DatabaseService db)
        {
            _context = context;
            _logger = logger;
            _authClaimService = authClaimService;
            _db = db;
        }
        [HttpPost("Role/List")]
        [MenuAuthorize(AuthorisationConstants.FetchRoles)]
        public async Task<ActionResult<HmsResponse>> GetRoles()
        {
            var response = new HmsResponse();
            orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");
            try
            {
                var roles = await _context.Roles.Where(x=> x.OrgId == orgId).AsNoTracking().ToListAsync();
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
        [MenuAuthorize(AuthorisationConstants.CreateRole)]
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

                if (string.IsNullOrEmpty(roleDto.RoleName))
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Role Name is required.";
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
        [MenuAuthorize(AuthorisationConstants.DeleteRole)]
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
        [MenuAuthorize(AuthorisationConstants.DeleteRole)]
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
        [MenuAuthorize(AuthorisationConstants.GetMenuAccessForRole)]
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
        [MenuAuthorize(AuthorisationConstants.GetUserUnderRole)]
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

        [HttpPost("Role/AddUser")]
        [MenuAuthorize(AuthorisationConstants.AddUserUnderRole)]
        public async Task<ActionResult<HmsResponse>> AddUserToRole([FromBody] UserRoleMappingDTO userRoleMappingDTO)
        {
            var hMSResponse = new HmsResponse();
            try
            {
                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                if  (string.IsNullOrEmpty( userRoleMappingDTO.UserName) || 
                    userRoleMappingDTO.RoleId == null)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.USER_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";

                    return BadRequest(hMSResponse);
                }

                var Role = _context.Roles.FirstOrDefault(x => x.RoleId == userRoleMappingDTO.RoleId 
                && x.OrgId == orgId);
                if (Role == null)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.USER_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";

                    return BadRequest(hMSResponse);
                }

                var User = _context.Users.FirstOrDefault(x => x.Username == userRoleMappingDTO.UserName 
                && x.OrgId == orgId);
                if (User == null)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.USER_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";

                    return BadRequest(hMSResponse);
                }

                // 1. Check for existing mapping
                var newUserRoleMapping = await _context.UserRoleMappings.FirstOrDefaultAsync(
                    urm => urm.RoleId == userRoleMappingDTO.RoleId
                    && urm.OrgId == orgId
                    && urm.UserId == User.UserId);

                if (newUserRoleMapping == null)
                {
                    var newRecord = new UserRoleMapping
                    {
                        RoleId = (int)userRoleMappingDTO.RoleId,
                        UserId = User.UserId,
                        IsPrimary = true,
                        // Use .Date or DateTime.Today because your DB column is 'date' (no time)
                        EffectiveFrom = DateTime.UtcNow.Date,
                        IsActive = true,
                        CreatedBy = "Admin",
                        CreatedDate = DateTime.UtcNow.Date,
                        RowVersion = 1,
                        OrgId = orgId
                    };

                    try
                    {
                        _context.UserRoleMappings.Add(newRecord);
                        await _context.SaveChangesAsync();

                        // IMPORTANT: EF Core automatically populates newRecord.MappingId 
                        // after SaveChangesAsync(). You do NOT need to query again.
                        newUserRoleMapping = newRecord;
                    }
                    catch (DbUpdateException ex)
                    {
                        // This catches the PK or Unique constraint violation if another thread
                        // inserted the record between your 'if' check and your 'Save'.
                        newUserRoleMapping = await _context.UserRoleMappings.AsNoTracking().FirstOrDefaultAsync(
                            urm => urm.RoleId == userRoleMappingDTO.RoleId
                            && urm.OrgId == orgId
                            && urm.UserId == User.UserId);

                        if (newUserRoleMapping == null) throw; // If it's still null, it's a different DB error
                    }
                }

                hMSResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hMSResponse.responseHeader.ErrorMessage = "SUCCESS";
                hMSResponse.responseBody.UserRoleMapping = new List<UserRoleMapping> { newUserRoleMapping };

                return Ok(hMSResponse);
            }
            catch (Exception ex)
            {
                hMSResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                hMSResponse.responseHeader.ErrorMessage = "FAILED";
                _logger.LogError(ex, $"Failed to fetch menu access for User {userRoleMappingDTO.UserName} : Role {userRoleMappingDTO.RoleId}" );
                return StatusCode(503, hMSResponse);
            }
        }

        [HttpPost("Role/RemoveUser")]
        [MenuAuthorize(AuthorisationConstants.RemoveUserFromRole)]
        public async Task<ActionResult<HmsResponse>> RemoveUserFromRole([FromBody] UserRoleMappingDTO userRoleMappingDTO)
        {
            var hMSResponse = new HmsResponse();
            try
            {
                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                if (string.IsNullOrEmpty(userRoleMappingDTO.UserName) ||
                    userRoleMappingDTO.RoleId == null)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.USER_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";

                    return BadRequest(hMSResponse);
                }

                var Role = _context.Roles.FirstOrDefault(x => x.RoleId == userRoleMappingDTO.RoleId
                && x.OrgId == orgId);
                if (Role == null)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.USER_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";

                    return BadRequest(hMSResponse);
                }

                var User = _context.Users.FirstOrDefault(x => x.Username == userRoleMappingDTO.UserName
                && x.OrgId == orgId);
                if (User == null)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.USER_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";

                    return BadRequest(hMSResponse);
                }

                var newUserRoleMapping = _context.UserRoleMappings.Where(
                    urm => urm.RoleId == userRoleMappingDTO.RoleId
                    && urm.OrgId == orgId
                    && urm.UserId == User.UserId);

                if (newUserRoleMapping == null)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.USER_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";

                    return BadRequest(hMSResponse);
                }
                newUserRoleMapping.ExecuteDeleteAsync();

                hMSResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hMSResponse.responseHeader.ErrorMessage = "SUCCESS";

                return Ok(hMSResponse);
            }
            catch (Exception ex)
            {
                hMSResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                hMSResponse.responseHeader.ErrorMessage = "FAILED";
                _logger.LogError(ex, $"Failed to fetch menu access for User {userRoleMappingDTO.UserName} : Role {userRoleMappingDTO.RoleId}");
                return StatusCode(503, hMSResponse);
            }
        }
        [HttpPost("Role/MenuAccess/Grant")]
        [MenuAuthorize(AuthorisationConstants.GrantMenuAccess)]
        public async Task<ActionResult<HmsResponse>> GrantMenuAccess([FromBody] RoleMenuDTO roleMenuDTO )
        {
            var hMSResponse = new HmsResponse();
            try
            {
                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                if (roleMenuDTO.MenuId == 0 ||
                    roleMenuDTO.RoleId == 0)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.MENU_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";

                    return BadRequest(hMSResponse);
                }

                var Role = _context.Roles.FirstOrDefault(x => x.RoleId == roleMenuDTO.RoleId
                && x.OrgId == orgId);
                if (Role == null)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.MENU_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";

                    return BadRequest(hMSResponse);
                }

                var menu = _context.MenuMasters.FirstOrDefault(x => x.MenuId == roleMenuDTO.MenuId);

                if (menu == null)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.MENU_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";

                    return BadRequest(hMSResponse);
                }

                // 1. Check for existing mapping
                var newMenuRoleMapping = await _context.RoleMenuMapping.FirstOrDefaultAsync(
                    urm => urm.RoleId == roleMenuDTO.RoleId
                    && urm.OrgId == orgId
                    && urm.MenuId == roleMenuDTO.MenuId);

                if (newMenuRoleMapping == null)
                {
                    var newRecord = new RoleMenuMapping
                    {
                        CreatedBy = HttpContext?.User?.Identity?.Name,
                        CreatedDate = DateTime.UtcNow.Date,
                        MenuId = roleMenuDTO.MenuId,
                        IsEnabled = true,
                        IsVisible = true,
                        OrgId = orgId,
                        RoleId = roleMenuDTO.RoleId
                    };

                    try
                    {
                        _context.RoleMenuMapping.Add(newRecord);
                        await _context.SaveChangesAsync();

                        // IMPORTANT: EF Core automatically populates newRecord.MappingId 
                        // after SaveChangesAsync(). You do NOT need to query again.
                        newMenuRoleMapping = newRecord;
                    }
                    catch (DbUpdateException ex)
                    {
                        // This catches the PK or Unique constraint violation if another thread
                        // inserted the record between your 'if' check and your 'Save'.
                        newMenuRoleMapping = await _context.RoleMenuMapping.AsNoTracking().FirstOrDefaultAsync(
                            urm => urm.RoleId == roleMenuDTO.RoleId
                            && urm.OrgId == orgId
                            && urm.MenuId == roleMenuDTO.MenuId);

                        if (newMenuRoleMapping == null) throw; // If it's still null, it's a different DB error
                    }
                }

                hMSResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hMSResponse.responseHeader.ErrorMessage = "SUCCESS";
                hMSResponse.responseBody.RoleMenuMapping = new List<RoleMenuMapping> { newMenuRoleMapping };

                return Ok(hMSResponse);
            }
            catch (Exception ex)
            {
                hMSResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                hMSResponse.responseHeader.ErrorMessage = "FAILED";
                _logger.LogError(ex, $"Failed to fetch menu access for Menu {roleMenuDTO.MenuId} : Role {roleMenuDTO.RoleId}");
                return StatusCode(503, hMSResponse);
            }
        }
        [HttpPost("Role/MenuAccess/Revoke")]
        [MenuAuthorize(AuthorisationConstants.RevokeMenuAccess)]
        public async Task<ActionResult<HmsResponse>> RevokeMenuAccess([FromBody] RoleMenuDTO roleMenuDTO)
        {
            var hMSResponse = new HmsResponse();
            try
            {
                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                if (roleMenuDTO.MenuId == 0 ||
                    roleMenuDTO.RoleId == 0)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.MENU_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";

                    return BadRequest(hMSResponse);
                }

                var Role = _context.Roles.FirstOrDefault(x => x.RoleId == roleMenuDTO.RoleId
                && x.OrgId == orgId);
                if (Role == null)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.MENU_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";

                    return BadRequest(hMSResponse);
                }

                var menu = _context.MenuMasters.FirstOrDefault(x => x.MenuId == roleMenuDTO.MenuId);

                if (menu == null)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.MENU_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";

                    return BadRequest(hMSResponse);
                }

                // 1. Check for existing mapping
                var newMenuRoleMapping = _context.RoleMenuMapping.Where(
                    urm => urm.RoleId == roleMenuDTO.RoleId
                    && urm.OrgId == orgId
                    && urm.MenuId == roleMenuDTO.MenuId);

                if (newMenuRoleMapping == null)
                {
                    hMSResponse.responseHeader.ErrorCode = AccessConstants.MENU_ROLE_MAPPING_NOT_AVAILABLE;
                    hMSResponse.responseHeader.ErrorMessage = "FAILED";
                    return BadRequest(hMSResponse);
                }
                newMenuRoleMapping.ExecuteDeleteAsync();
                hMSResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hMSResponse.responseHeader.ErrorMessage = "SUCCESS";

                return Ok(hMSResponse);
            }
            catch (Exception ex)
            {
                hMSResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                hMSResponse.responseHeader.ErrorMessage = "FAILED";
                _logger.LogError(ex, $"Failed to fetch menu access for Menu {roleMenuDTO.MenuId} : Role {roleMenuDTO.RoleId}");
                return StatusCode(503, hMSResponse);
            }
        }
        [HttpPost("Role/UI/Control/UpdateAccess")]
        [MenuAuthorize(AuthorisationConstants.UpdateUIAccess)]
        public async Task<ActionResult<HmsResponse>> UpdateUIAccess([FromBody] OrgUiControlDTO orgUiControlDTO)
        {
            var hMSResponse = new HmsResponse();
            try
            {
                if (orgUiControlDTO == null) return BadRequest("Payload is null.");

                orgId = int.Parse(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

                // 1. Try to find an existing record based on the ID (if provided) 
                // or by the unique combination of Org + Menu + Role
                var existingRecord = await _context.OrgUiControls
                    .FirstOrDefaultAsync(x => x.OrgId == orgId
                                         && x.HierarchyId == orgUiControlDTO.HierarchyId
                                         && x.RoleId == orgUiControlDTO.RoleId);

                if (existingRecord != null)
                {
                    // --- UPDATE LOGIC ---
                    existingRecord.AllowEdit = orgUiControlDTO.AllowEdit;
                    existingRecord.RenderControl = orgUiControlDTO.RenderControl;

                    // Only update 'GrantedBy' if the DTO provides a new value
                    existingRecord.AccessGrantedBy = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier));
                    existingRecord.AccessGrantedOn = DateTime.UtcNow;

                    _context.OrgUiControls.Update(existingRecord);
                }
                else
                {
                    // --- INSERT LOGIC ---
                    var newRecord = new OrgUiControl
                    {
                        OrgId = orgId,
                        HierarchyId = orgUiControlDTO.HierarchyId,
                        RoleId = orgUiControlDTO.RoleId,
                        AllowEdit = orgUiControlDTO.AllowEdit,
                        RenderControl = orgUiControlDTO.RenderControl,
                        AccessGrantedOn = DateTime.UtcNow,
                        AccessGrantedBy = int.Parse(_authClaimService.GetClaim(ClaimTypes.NameIdentifier))
                    };
                    _context.OrgUiControls.Add(newRecord);
                }

                await _context.SaveChangesAsync();
                hMSResponse.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                hMSResponse.responseHeader.ErrorMessage = "SUCCESS";

                return Ok(hMSResponse);
            }
            catch (Exception ex)
            {
                hMSResponse.responseHeader.ErrorCode = CommonConstants.FAILED;
                hMSResponse.responseHeader.ErrorMessage = "FAILED";
                _logger.LogError(ex, $"Failed to Update menu access for RoleID {orgUiControlDTO.RoleId} : MenuID {orgUiControlDTO.HierarchyId}");
                return StatusCode(503, hMSResponse);
            }
        }
        [HttpPost("UIControlAccess")]
        [MenuAuthorize(AuthorisationConstants.UIControlAccess)]
        public async Task<IActionResult> GetUIControlAccess([FromBody] bool ShowAll = false)
        {
            HmsResponse hMSResponse = new HmsResponse();
            orgId = Convert.ToInt32(_authClaimService.GetClaim(ApiConstants.OrganisationId) ?? "0");

            try
            {
                //"Script": "select * from hms.get_ui_control_hierarchy(2, false)"
                var stringResponse = await _db.ExecuteQueryAsync<string>(
                    "Master",
                    "get_ui_control_hierarchy",
                    new
                    {
                        p_orgId = orgId,
                        p_ShowAll = ShowAll
                    });

                if (!string.IsNullOrEmpty(stringResponse.FirstOrDefault()))
                {
                    var uiMenuHeirarchy = JsonConvert.DeserializeObject<List<UIMenuHeirarchyDTO>>(
                        stringResponse.FirstOrDefault(),
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });

                    // If a root itself should be hidden if RenderControl is false:
                    //var finalMenu = uiMenuHeirarchy.Where(m => m.RenderControl).ToList();

                    hMSResponse.responseHeader.ErrorCode = 1101;
                    hMSResponse.responseHeader.ErrorMessage = "SUCCESS";
                    hMSResponse.responseBody.uiMenuHeirarchy = uiMenuHeirarchy;
                    return Ok(hMSResponse);
                }
                else
                {
                    hMSResponse.responseHeader.ErrorCode = AgentConstants.AGENT_GEOHEIRARCHY_NOTFOUND;
                    hMSResponse.responseHeader.ErrorMessage = "Geo Hierarchy not found for this selection.";
                    return NotFound(hMSResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred In GeoHierarchy");
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}