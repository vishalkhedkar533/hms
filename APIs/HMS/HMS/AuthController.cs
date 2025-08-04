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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive && !u.IsLocked);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("Invalid credentials or inactive user.");
            }

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
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            int expiresTime = int.TryParse(_config["Jwt:ExpiresTime"], out var time) ? time : 60; // Default to 60 minutes if not set

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, roleName)
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiresTime),
                Issuer = issuer,
                Audience = audience,
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