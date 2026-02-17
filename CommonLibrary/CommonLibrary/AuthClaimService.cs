using Microsoft.AspNetCore.Http;

namespace CommonLibrary
{

    public interface IAuthClaimService
    {
        string GetClaim(String ClaimType);
        string GetClaim(object nameIdentifier);
    }
    public class AuthClaimService : IAuthClaimService
    {
        // Declare a private field for the accessor
        private readonly IHttpContextAccessor _httpContextAccessor;

        // **STEP 2: Inject IHttpContextAccessor into the constructor**
        public AuthClaimService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetClaim(String ClaimType)
        {
            // Access the current HttpContext and the User's ClaimsPrincipal
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;

            // The code you asked about, used with null-safe operators (?.)
            var organisationIdClaim = claimsPrincipal?.Claims
                .FirstOrDefault(x => x.Type.Equals(ClaimType.Trim()));

            // Return the claim value, or null/empty if not found
            return organisationIdClaim?.Value;
        }
    }
}
