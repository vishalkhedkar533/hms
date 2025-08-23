using HMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models.DB;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HMS.Controllers
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
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            // Check if account is locked
            if (user.lockoutendtime.HasValue && user.lockoutendtime > DateTime.UtcNow)
            {
                return Unauthorized($"Account is locked. Try again after {user.lockoutendtime.Value.ToLocalTime():g}");
            }

            // Check credentials
            if (!user.IsActive || user.IsLocked || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                user.failedloginattempts++;

                if (user.failedloginattempts >=
                    int.Parse(_context.apiConfig.FirstOrDefaultAsync(u => u.ConfigKey == ApiConstants.wrong_attempts_allowed)
                    ?.Result?.ConfigValue ?? _config["DefaultValues:LoginAttempt"].ToString()))
                {
                    user.lockoutendtime = DateTime.UtcNow.AddMinutes(15); // Lock for 15 minutes
                    user.IsLocked = true; // Optional: depending on your logic
                }

                await _context.SaveChangesAsync();
                return Unauthorized("Invalid credentials.");
            }

            // Successful login: reset failed attempts
            user.failedloginattempts = 0;
            user.lockoutendtime = null;
            user.IsLocked = false;

            var roleNames = await _context.UserRoleMappings
                .Where(urm => urm.UserId == user.UserId
                              && urm.IsActive
                              && (urm.EffectiveTo == null || urm.EffectiveTo >= DateTime.Today)
                              && urm.EffectiveFrom <= DateTime.Today)
                .Select(urm => urm.Role.RoleName) // assumes Role table has RoleName
                .Distinct()
                .ToListAsync();

            if (roleNames == null)
            {
                return Unauthorized("User has no active primary role.");
            }

            var token = GenerateJwtToken(user, roleNames);
            user.LastLoginDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { token });
        }

        private string GenerateJwtToken(User user, List<string> roleNames)
        {
            var key = _config["Jwt:Key"] ?? "super_secret_jwt_key";
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            int expiresTime = int.TryParse(_config["Jwt:ExpiresTime"], out var time) ? time : 60; // Default to 60 minutes if not set

            //var claims = new[]
            //{
            //    new Claim(ClaimTypes.Name, user.Username),
            //    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            //    new Claim(ClaimTypes.Role, roleName)
            //};
            var claims = new List<Claim>{
               new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // 👈 Store UserId here
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add role claims
            claims.AddRange(roleNames.Select(role => new Claim(ClaimTypes.Role, role)));
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
        }
    }
}