using HMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HMS
{
    public class MenuAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;

        public MenuAuthorizeAttribute(HMSContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        private readonly int _menuId;
        public MenuAuthorizeAttribute(int menuId) => _menuId = menuId;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<HMSContext>();

            // Get roles from JWT
            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Check DB if any of user's roles are allowed for this menu
            bool hasAccess = _context.RoleMenuMapping
                .Include(rmm => rmm.Role)
                .Any(rmm => rmm.MenuId == _menuId
                            && rmm.IsEnabled
                            && userRoles.Contains(rmm.Role.RoleName));

            if (!hasAccess)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}