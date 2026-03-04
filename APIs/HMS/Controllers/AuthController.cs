using Azure;
using CommonLibrary;
using Communication;
using HMS.Data;
using HMS.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models.DB;
using Models.DTO;
using Models.HMSConsts;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AuthController : ControllerBase
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;
        private readonly FileService _fileService;
        public AuthController(HMSContext context, IConfiguration config, FileService fileService)
        {
            _context = context;
            _config = config;
            _fileService = fileService;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            HmsResponse response = new HmsResponse();
            var handler = new JwtSecurityTokenHandler();
            DateTimeOffset expTime = DateTimeOffset.UtcNow;
            string errorMsg = string.Empty;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                errorMsg = await _context.errorMaster.AsNoTracking()
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
                errorMsg = await _context.errorMaster.AsNoTracking().
                    Where(x => x.ErrorId == LoginConstants.ACCOUNT_LOCKED && x.Area == "LoginConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";
                response.responseHeader.ErrorCode = LoginConstants.ACCOUNT_LOCKED;
                response.responseHeader.ErrorMessage = string.Format(errorMsg, user.lockoutendtime.Value.ToLocalTime());
                return Unauthorized(response);
            }

            // Check credentials
            if (!user.IsActive || user.IsLocked || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                user.failedloginattempts++;

                var allowedAttemptsConfig = await _context.apiConfig
                    .Where(u => u.ConfigKey == ApiConstants.wrong_attempts_allowed)
                    .Select(u => u.ConfigValue).FirstOrDefaultAsync();

                int allowedAttempts = int.TryParse(allowedAttemptsConfig, out var val)
                    ? val : int.Parse(_config["DefaultValues:LoginAttempt"] ?? "3");

                if (user.failedloginattempts >= allowedAttempts)
                {
                    user.lockoutendtime = DateTime.UtcNow.AddMinutes(15); // Lock for 15 minutes
                    user.IsLocked = true; // Optional: depending on your logic
                    EmailService emailService = new EmailService(_config);
                    var message = new MailMessage("donotreply@hms.com", request.Username);
                    message.Subject = "Account Locked";
                    message.Body = _fileService.GetTemplate(Path.Combine("Templates", "Mail"), "accountlocked.html");
                    message.IsBodyHtml = true;
                    await emailService.SendEmailAsync(message);
                }

                await _context.SaveChangesAsync();
                errorMsg = await _context.errorMaster.AsNoTracking()
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

            var roleNames = await _context.UserRoleMappings.AsNoTracking()
                .Where(urm => urm.UserId == user.UserId && urm.IsActive
                              && (urm.EffectiveTo == null || urm.EffectiveTo >= DateTime.Today)
                              && urm.EffectiveFrom <= DateTime.Today)
                .Select(urm => urm.Role != null ? urm.Role.RoleName : null)
                .Where(roleName => roleName != null)
                .Distinct().ToListAsync();

            if (roleNames == null || !roleNames.Any())
            {
                errorMsg = await _context.errorMaster.AsNoTracking()
                    .Where(x => x.ErrorId == LoginConstants.NO_ACTIVE_PRIMARY_ROLE && x.Area == "LoginConstants")
                    .Select(x => x.ErrorMsg).FirstOrDefaultAsync() ?? "Undefined Error Message";
                response.responseHeader.ErrorCode = LoginConstants.NO_ACTIVE_PRIMARY_ROLE;
                response.responseHeader.ErrorMessage = errorMsg;
                return Unauthorized(response);
            }

            var token = GenerateJwtToken(user, roleNames);
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = BCrypt.Net.BCrypt.HashPassword(refreshToken);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(1);
            user.LastLoginDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Must be true for HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
            response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
            response.responseHeader.ErrorMessage = await _context.errorMaster.AsNoTracking()
                .Where(x => x.ErrorId == CommonConstants.SUCCESS && x.Area == "Common")
                .Select(x => x.ErrorMsg).FirstOrDefaultAsync() ?? "Undefined Message";
            var jwtToken = handler.ReadJwtToken(token);
            var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (expClaim != null)
            {
                expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;
            }

            bool Encrypt_Api_Calls = (((await _context.apiConfig.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.ConfigKey == ApiConstants.encrypt_api_calls))
                    .ConfigValue) ?? "1") == "1";
            
            response.responseBody.loginResponse = new LoginResponse
            {
                Token = token,
                Expiration = expTime.LocalDateTime,
                UserId = user.UserId,
                Username = user.Username,
                Encrypt_Api_Calls = Encrypt_Api_Calls
            };
            return Ok(response);
        }
        private string GenerateJwtToken(User user, List<string> roleNames)
        {
            var key = _config["Jwt:Key"] ?? "super_secret_jwt_key";
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            int expiresTime = int.TryParse(_config["Jwt:ExpiresTime"], out var time) ? time : 60; // Default to 60 minutes if not set

            var orgName  = _context.Organisation.AsNoTracking()
                .Where(o => o.OrgId == user.OrgId)
                .FirstOrDefault();
            
            if (orgName != null)
            {
                user.OrgName = orgName.OrgName;

                var subscriber = _context.Subscriber.AsNoTracking()
                    .Where(s => s.SubscriberId == orgName.SubscriberId)
                    .FirstOrDefault() ?? new Subscriber();

                if (subscriber!= null)
                {
                    user.SubscriberName = subscriber.SubscriberName;
                    user.SubscriberId = subscriber.SubscriberId;
                }
            }

            var claims = new List<Claim>
            {
                // Standard claims
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),   // 👈 JWT standard "sub"
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),     // 👈 .NET convention
                new Claim(ClaimTypes.Name, user.Username),                        // Username
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID,
                new Claim(ApiConstants.OrganisationId, user.OrgId.ToString()), // Custom claim for User ID
                new Claim(ApiConstants.OrganisationName, user.OrgName.ToString()), // Custom claim for User ID
                new Claim(ApiConstants.SubscriberId, user.SubscriberId.ToString()), // Custom claim for User ID
                new Claim(ApiConstants.SubscriberName, user.SubscriberName.ToString()) // Custom claim for User ID
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
        [HttpPost("GetOTP_To_UnLock")]
        [AllowAnonymous]
        public async Task<ActionResult> GetOTP_To_UnLock([FromBody] LoginRequest request)
        {
            OtpResponse response = new OtpResponse();

            User user = await _context.Users.Where(u => u.Username == request.Username).FirstOrDefaultAsync();

            if (user != null)
            {

                bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
                if (!isOldPasswordValid)
                {
                    return BadRequest("Old password is incorrect.");
                }

                Random rnd = new Random();
                string otp = rnd.Next(1000, 9999).ToString();
                EmailService emailService = new EmailService(_config);
                var message = new MailMessage("donotreply@hms.com", user.EmailId);
                message.Subject = "OTP - Account Unlocked";
                message.Body = _fileService.GetTemplate(Path.Combine("Templates", "Mail"), "otp.html").Replace("{{OTP}}", otp).Replace("{{User}}", user.Username);
                message.IsBodyHtml = true;
                await emailService.SendEmailAsync(message);
                response.responseBody = new OtpResponseBody
                {
                    username = user.Username,
                    otp = otp
                };
                response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                response.responseHeader.ErrorMessage = "An OTP has been sent to your registered email address. Please check your email to proceed with unlocking your account.";

            }
            else
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "No matching user record was found in the system.";
            }
            return Ok(response);
        }

        [HttpPost("ConfirmOTP_To_UnLock")]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmOTP_To_UnLock([FromBody] LoginRequest request)
        {
            HmsResponse response = new HmsResponse();
            User user = await _context.Users.Where(u => u.Username == request.Username).FirstOrDefaultAsync();
            if (user != null)
            {
                if (user.failedloginattempts >= 5 || user.IsLocked)
                {
                    user.IsLocked = false;
                    user.IsActive = true;
                    user.failedloginattempts = 0;
                    user.ModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    EmailService emailService = new EmailService(_config);
                    var message = new MailMessage("donotreply@hms.com", user.EmailId);
                    message.Subject = "Account Unlocked";
                    message.Body = _fileService.GetTemplate(Path.Combine("Templates", "Mail"), "accountunlocked.html").Replace("{{User}}", user.Username);
                    message.IsBodyHtml = true;
                    await emailService.SendEmailAsync(message);
                    response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    response.responseHeader.ErrorMessage = "Your account has been successfully unlocked. You can now log in using your credentials.";
                }
                else
                {
                    response.responseHeader.ErrorCode = CommonConstants.SUCCESS;
                    response.responseHeader.ErrorMessage = "The user account is currently active and accessible.";
                }
            }
            else
            {
                response.responseHeader.ErrorCode = CommonConstants.FAILED;
                response.responseHeader.ErrorMessage = "No matching user record was found in the system.";
            }
            return Ok(response);
        }
        [HttpPost("refresh")]
        public async Task<ActionResult> Refresh()
        {
            HmsResponse response = new HmsResponse();
            DateTimeOffset expTime = DateTimeOffset.UtcNow;
            var handler = new JwtSecurityTokenHandler();
            if (!Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
                return Unauthorized("Refresh token missing.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized("Invalid or expired session.");

            var roleNames = await _context.UserRoleMappings.AsNoTracking()
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
                response.responseHeader.ErrorCode = LoginConstants.NO_ACTIVE_PRIMARY_ROLE;
                response.responseHeader.ErrorMessage = await _context.errorMaster.AsNoTracking()
                    .Where(x => x.ErrorId == LoginConstants.NO_ACTIVE_PRIMARY_ROLE && x.Area == "LoginConstants")
                    .Select(x => x.ErrorMsg)
                    .FirstOrDefaultAsync() ?? "Undefined Error Message";
                return Unauthorized(response);
            }
            // Generate new set
            var newAccessToken = GenerateJwtToken(user, roleNames);
            var newRefreshToken = GenerateRefreshToken();

            // Rotate token (security best practice)
            user.RefreshToken = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();
            bool Encrypt_Api_Calls = (((await _context.apiConfig.AsNoTracking()
                   .FirstOrDefaultAsync(u => u.ConfigKey == ApiConstants.encrypt_api_calls))
                   .ConfigValue) ?? "1") == "1";
            var jwtToken = handler.ReadJwtToken(newAccessToken);
            var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (expClaim != null)
            {
                expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;
            }
            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });
            response.responseBody.loginResponse = new LoginResponse
            {
                Token = newAccessToken,
                Expiration = expTime.LocalDateTime,
                UserId = user.UserId,
                Username = user.Username,
                Encrypt_Api_Calls = Encrypt_Api_Calls
            };
            return Ok(response);
        }
        [HttpPost("logout")]
        [MenuAuthorize(AuthorisationConstants.AuthenticationService)]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            // Find the user whose stored hash matches the cookie token
            var usersWithTokens = await _context.Users.Where(u => u.RefreshToken != null).ToListAsync();
            var user = usersWithTokens.FirstOrDefault(u => BCrypt.Net.BCrypt.Verify(refreshToken, u.RefreshToken));
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _context.SaveChangesAsync();
            }
            Response.Cookies.Delete("refreshToken");
            return Ok("Logged out.");
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}