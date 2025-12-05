using System.IdentityModel.Tokens.Jwt;

namespace CommonLibrary
{
    public class JwtDecoder
    {
        /// <summary>
        /// Decodes a JWT string and extracts all claims from its payload.
        /// </summary>
        /// <param name="token">The JWT string to decode.</param>
        /// <returns>A dictionary of claim type and claim value, or an empty dictionary if decoding fails.</returns>
        public Dictionary<string, string> GetClaimsFromJwt(string token)
        {
            var claimsDictionary = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(token))
            {
                return claimsDictionary;
            }

            try
            {
                // 1. Create a JwtSecurityTokenHandler instance
                var handler = new JwtSecurityTokenHandler();

                // 2. Read the token. This method only reads the content; it does NOT validate the signature.
                var jwtToken = handler.ReadJwtToken(token);

                // 3. Access the Claims (the payload)
                var claims = jwtToken.Claims;

                // 4. Iterate over the claims and populate the dictionary
                foreach (var claim in claims)
                {
                    // The Type is the key (e.g., "sub", "email", "role")
                    // The Value is the value associated with that key
                    claimsDictionary.Add(claim.Type, claim.Value);
                }

                return claimsDictionary;
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., token format is invalid)
                Console.WriteLine($"Error decoding JWT: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        // Example usage of getting a specific claim:
        public string GetSpecificClaim(string token, string claimType)
        {
            var claims = GetClaimsFromJwt(token);

            // Use TryGetValue to safely retrieve the claim value
            claims.TryGetValue(claimType, out string value);

            return value;
        }
    }
}
