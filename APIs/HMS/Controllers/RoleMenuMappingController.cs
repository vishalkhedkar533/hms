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
    public class RoleMenuMappingController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly ILogger<RoleMenuMappingController> _logger;

        public RoleMenuMappingController(HMSContext context, ILogger<RoleMenuMappingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("MenuList")]
        public async Task<ActionResult<HmsResponse>> GetMenuList([FromQuery] int roleId)
        {
            var response = new HmsResponse();
            try
            {
                var menuMappings = await _context.MenuMasters
                    .AsNoTracking()
                    .GroupJoin(
                        _context.RoleMenuMapping.AsNoTracking().Where(r => r.RoleId == roleId),
                        menu => menu.MenuId,
                        mapping => mapping.MenuId,
                        (menu, mappings) => new { menu, mapping = mappings.FirstOrDefault() })
                    .Select(x => new MenuRoleMappingDto
                    {
                        MenuId = x.menu.MenuId,
                        MenuName = x.menu.MenuName,
                        ParentMenuId = x.menu.ParentMenuId,
                        RoutePath = x.menu.RoutePath,
                        DisplayOrder = x.menu.DisplayOrder,
                        IsActive = x.menu.IsActive,
                        IsInternal = x.menu.IsInternal,
                        MappingId = x.mapping != null ? x.mapping.MappingId : null,
                        IsMapped = x.mapping != null,
                        IsVisible = x.mapping != null ? x.mapping.IsVisible : null,
                        IsEnabled = x.mapping != null ? x.mapping.IsEnabled : null,
                        RoleId = x.mapping != null ? x.mapping.RoleId : roleId
                    })
                    .ToListAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = await GetErrorMessageAsync(CommonConstants.SUCCESS, "Common", "SUCCESS");
                response.responseBody.menuRoleMappings = menuMappings;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch role menu mapping list");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Internal server error.";
                return StatusCode(500, response);
            }
        }

        [HttpGet("Menus")]
        public async Task<ActionResult<HmsResponse>> GetMenus()
        {
            var response = new HmsResponse();
            try
            {
                var menus = await _context.MenuMasters.AsNoTracking().ToListAsync();
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = await GetErrorMessageAsync(CommonConstants.SUCCESS, "Common", "SUCCESS");
                response.responseBody.menus = menus;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch menus");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Internal server error.";
                return StatusCode(500, response);
            }
        }

        [HttpPost("Menus")]
        public async Task<ActionResult<HmsResponse>> CreateMenu([FromBody] MenuMaster menu)
        {
            var response = new HmsResponse();
            try
            {
                if (menu == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid menu payload.";
                    return BadRequest(response);
                }

                var exists = await _context.MenuMasters.AnyAsync(m => m.MenuId == menu.MenuId);
                if (exists)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Menu already exists.";
                    return Conflict(response);
                }

                menu.CreatedDate = DateTime.UtcNow;
                menu.ModifiedDate = null;

                _context.MenuMasters.Add(menu);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = await GetErrorMessageAsync(CommonConstants.SUCCESS, "Common", "SUCCESS");
                response.responseBody.menus = new List<MenuMaster> { menu };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create menu");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Internal server error.";
                return StatusCode(500, response);
            }
        }

        [HttpDelete("Menus/{id:int}")]
        public async Task<ActionResult<HmsResponse>> DeleteMenu(int id)
        {
            var response = new HmsResponse();
            try
            {
                var menu = await _context.MenuMasters.FirstOrDefaultAsync(m => m.MenuId == id);
                if (menu == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Menu not found.";
                    return NotFound(response);
                }

                _context.MenuMasters.Remove(menu);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = await GetErrorMessageAsync(CommonConstants.SUCCESS, "Common", "SUCCESS");
                response.responseBody.menus = new List<MenuMaster> { menu };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete menu");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Internal server error.";
                return StatusCode(500, response);
            }
        }

        [HttpGet("List")]
        public async Task<ActionResult<HmsResponse>> GetMappings([FromQuery] int? roleId = null)
        {
            var response = new HmsResponse();
            try
            {
                var query = _context.RoleMenuMapping.AsNoTracking();
                if (roleId.HasValue)
                {
                    query = query.Where(r => r.RoleId == roleId.Value);
                }

                var mappings = await query.ToListAsync();
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = await GetErrorMessageAsync(CommonConstants.SUCCESS, "Common", "SUCCESS");
                response.responseBody.roleMenuMappings = mappings;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch role menu mappings");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Internal server error.";
                return StatusCode(500, response);
            }
        }

        [HttpPost("Create")]
        public async Task<ActionResult<HmsResponse>> CreateMapping([FromBody] RoleMenuMapping mapping)
        {
            var response = new HmsResponse();
            try
            {
                if (mapping == null)
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Invalid mapping payload.";
                    return BadRequest(response);
                }

                mapping.CreatedDate = DateTime.UtcNow;
                mapping.ModifiedDate = null;

                _context.RoleMenuMapping.Add(mapping);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = await GetErrorMessageAsync(CommonConstants.SUCCESS, "Common", "SUCCESS");
                response.responseBody.roleMenuMappings = new List<RoleMenuMapping> { mapping };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create role menu mapping");
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "Internal server error.";
                return StatusCode(500, response);
            }
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult<HmsResponse>> DeleteMappings([FromQuery] int roleId, [FromQuery] int? menuId = null)
        {
            var response = new HmsResponse();
            try
            {
                var query = _context.RoleMenuMapping.Where(r => r.RoleId == roleId);
                if (menuId.HasValue)
                {
                    query = query.Where(r => r.MenuId == menuId.Value);
                }

                var mappings = await query.ToListAsync();
                if (!mappings.Any())
                {
                    response.responseHeader.ErrorCode = CommonConstants.FAILED;
                    response.responseHeader.ErrorMessage = "Role menu mapping not found.";
                    return NotFound(response);
                }

                _context.RoleMenuMapping.RemoveRange(mappings);
                await _context.SaveChangesAsync();

                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = await GetErrorMessageAsync(CommonConstants.SUCCESS, "Common", "SUCCESS");
                response.responseBody.roleMenuMappings = mappings;

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete role menu mappings");
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
