using Microsoft.AspNetCore.Http;

namespace CommonLibrary
{
    public class WhitelistHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HashSet<string> _allowedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Content-Type",
        "Accept" // optional, if you need it
    };

        public WhitelistHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Collect headers not in the whitelist
            var headersToRemove = context.Request.Headers
                .Where(h => !_allowedHeaders.Contains(h.Key))
                .Select(h => h.Key)
                .ToList();

            foreach (var header in headersToRemove)
            {
                context.Request.Headers.Remove(header);
            }

            await _next(context);
        }
    }
}
