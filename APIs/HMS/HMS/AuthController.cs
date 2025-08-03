using HMS.Data;
using HMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HMS
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;

        public AuthController(HMSContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            // Check if account is locked
            if (user.LockoutEndTime.HasValue && user.LockoutEndTime > DateTime.UtcNow)
            {
                return Unauthorized($"Account is locked. Try again after {user.LockoutEndTime.Value.ToLocalTime():g}");
            }

            // Check credentials
            if (!user.IsActive || user.IsLocked || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEndTime = DateTime.UtcNow.AddMinutes(15); // Lock for 15 minutes
                    user.IsLocked = true; // Optional: depending on your logic
                }

                await _context.SaveChangesAsync();
                return Unauthorized("Invalid credentials.");
            }

            // Successful login: reset failed attempts
            user.FailedLoginAttempts = 0;
            user.LockoutEndTime = null;
            user.IsLocked = false;

            var roleMapping = await _context.UserRoleMappings
                .Include(urm => urm.Role)
                .Where(urm => urm.UserId == user.UserId && urm.IsActive && urm.IsPrimary)
                .FirstOrDefaultAsync();

            if (roleMapping?.Role == null)
            {
                return Unauthorized("User has no active primary role.");
            }

            var token = GenerateJwtToken(user, roleMapping.Role.RoleName);
            user.LastLoginDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { token });
        }

        private string GenerateJwtToken(User user, string roleName)
        {
            var key = _config["Jwt:Key"] ?? "super_secret_jwt_key";
            var keyBytes = Encoding.UTF8.GetBytes(key);

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, roleName)
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "myapi",
                Audience = "myapi",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Organisation { get; set; } = string.Empty;
        }
    }
}