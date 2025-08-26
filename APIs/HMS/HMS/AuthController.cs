using HMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models.Consts;
using Models.DB;
using Models.DTO;
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
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            HMSResponse response = new HMSResponse();
            string errorMsg = string.Empty;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                errorMsg = await _context.errorMaster
                    .Where(x => x.ErrorId == LoginConstants.INVALID_CREDENTIALS && x.Area == "LoginConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";
                response.responseHeader.ErrorCode = LoginConstants.INVALID_CREDENTIALS;
                response.responseHeader.ErrorMessage = errorMsg;
                return Unauthorized(response);
            }

            // Check if account is locked
            if (user.lockoutendtime.HasValue && user.lockoutendtime > DateTime.UtcNow)
            {
                errorMsg = await _context.errorMaster.
                    Where(x => x.ErrorId == LoginConstants.ACCOUNT_LOCKED && x.Area == "LoginConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";
                response.responseHeader.ErrorCode = LoginConstants.ACCOUNT_LOCKED;
                response.responseHeader.ErrorMessage = string.Format( errorMsg, user.lockoutendtime.Value.ToLocalTime());
                return Unauthorized(response);
            }

            // Check credentials
            if (!user.IsActive || user.IsLocked || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                user.failedloginattempts++;

                var allowedAttemptsConfig = await _context.apiConfig
                    .Where(u => u.ConfigKey == ApiConstants.wrong_attempts_allowed)
                    .Select(u => u.ConfigValue)
                    .FirstOrDefaultAsync();

                int allowedAttempts = int.TryParse(allowedAttemptsConfig, out var val)
                    ? val
                    : int.Parse(_config["DefaultValues:LoginAttempt"] ?? "3");

                if (user.failedloginattempts >= allowedAttempts)
                {
                    user.lockoutendtime = DateTime.UtcNow.AddMinutes(15); // Lock for 15 minutes
                    user.IsLocked = true; // Optional: depending on your logic
                }

                await _context.SaveChangesAsync();
                errorMsg = await _context.errorMaster
                    .Where(x => x.ErrorId == LoginConstants.INVALID_CREDENTIALS && x.Area == "LoginConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";
                response.responseHeader.ErrorCode = LoginConstants.INVALID_CREDENTIALS;
                response.responseHeader.ErrorMessage = errorMsg;
                return Unauthorized(response);
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
                .Select(urm => urm.Role != null ? urm.Role.RoleName : null)
                .Where(roleName => roleName != null)
                .Distinct()
                .ToListAsync();

            if (roleNames == null || !roleNames.Any())
            {
                errorMsg = await _context.errorMaster
                    .Where(x => x.ErrorId == LoginConstants.NO_ACTIVE_PRIMARY_ROLE && x.Area == "LoginConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";
                response.responseHeader.ErrorCode = LoginConstants.NO_ACTIVE_PRIMARY_ROLE;
                response.responseHeader.ErrorMessage = errorMsg;
                return Unauthorized(response);
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

            var claims = new List<Claim>
            {
                // Standard claims
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),   // 👈 JWT standard "sub"
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),     // 👈 .NET convention
                new Claim(ClaimTypes.Name, user.Username),                        // Username
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
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
    }
}