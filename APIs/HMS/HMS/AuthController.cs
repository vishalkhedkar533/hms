using Microsoft.AspNetCore.Mvc;
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
        private readonly string _key = "jwt_access_key";

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.Username == "admin" 
                && request.Password == "password" 
                && request.Organisation == "organisation")
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var keyBytes = Encoding.UTF8.GetBytes(_key);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, request.Username),
                        new Claim(ClaimTypes.Role, "Admin")                    
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = "HMS-eARK",
                    Audience = "HMS-eARK",
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), 
                    SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwt = tokenHandler.WriteToken(token);

                return Ok(new { token = jwt });
            }

            return Unauthorized();
        }

        public class LoginRequest
        {
            public string Organisation { get; set; } = "";
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
        }
    }
}