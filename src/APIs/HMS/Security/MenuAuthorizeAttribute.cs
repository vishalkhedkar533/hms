using HMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace HMS.Security
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class MenuAuthorizeAttribute : TypeFilterAttribute
    {
        public MenuAuthorizeAttribute(int menuId) : base(typeof(MenuAuthorizeFilter))
        {
            Arguments = new object[] { menuId }; // Pass menuId explicitly
        }
    }
    public class MenuAuthorizeFilter : IAuthorizationFilter
    {
        private readonly int _menuId;
        private readonly HMSContext _context;
        private readonly ILogger<MenuAuthorizeFilter> _logger;

        public MenuAuthorizeFilter(int menuId, HMSContext context, ILogger<MenuAuthorizeFilter> logger)
        {
            _menuId = menuId;
            _context = context;
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Get active roles of user
            var roleIds = _context.UserRoleMappings
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.RoleId)
                .ToList();

            if (!roleIds.Any())
            {
                context.Result = new ForbidResult();
                return;
            }

            // Check ROLE_MENU_MAPPING for access
            bool hasAccess = _context.RoleMenuMapping
                .Any(rmm => roleIds.Contains(rmm.RoleId) && rmm.MenuId == _menuId && rmm.IsEnabled);

            if (!hasAccess)
            {
                context.Result = new ForbidResult();
                return;
            }

            _logger.LogInformation("User {UserId} granted access to MenuId {MenuId}", userId, _menuId);
        }
    }
}