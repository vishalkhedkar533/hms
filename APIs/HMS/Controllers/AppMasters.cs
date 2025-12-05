using HMS.Caching;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //Use this controllers to read master and keep this in cache
    public class AppMastersController : ControllerBase
    {
        private readonly GenericCacheService _cacheService;
        private readonly IConfiguration _configuration;

        public AppMastersController(GenericCacheService cacheService, IConfiguration configuration)
        {
            _cacheService = cacheService;
            _configuration = configuration;
        }

        // 🔹 Fetch records (dynamic) - uses refreshInterval from appsettings.json
        // POST api/cache/get/hms/customer
        [HttpPost("get/{schema}/{table}")]
        public async Task<IActionResult> GetRecords(string schema, string table)
        {
            if (!IsValidSchema(schema)) return Forbid("Access denied: invalid schema." + schema);

            int refreshInterval = _configuration.GetValue<int>("Caching:refreshIntervalMinutes", 15);

            var result = await _cacheService.GetRecordsAsync(schema, table, refreshInterval);
            return Ok(result);
        }

        // 🔹 Refresh table (dynamic)
        // POST api/cache/refresh/hms/customer
        [HttpPost("refresh/{schema}/{table}")]
        public async Task<IActionResult> Refresh(string schema, string table)
        {
            if (!IsValidSchema(schema)) return Forbid("Access denied: invalid schema." + schema);

            var result = await _cacheService.RefreshCacheAsync(schema, table);
            return Ok(result);
        }

        // 🔹 Evict table
        // POST api/cache/evict/hms/customer
        [HttpPost("evict/{schema}/{table}")]
        public IActionResult Evict(string schema, string table)
        {
            if (!IsValidSchema(schema)) return Forbid("Access denied: invalid schema." + schema);
            _cacheService.EvictCache(schema, table);
            return Ok($"Cache for {schema}.{table} evicted.");
        }

        // 🔹 Evict entire schema
        // POST api/cache/evict/schema/hms
        [HttpPost("evict/schema/{schema}")]
        public IActionResult EvictSchema(string schema)
        {
            if (!IsValidSchema(schema)) return Forbid("Access denied: invalid schema." + schema);
            _cacheService.EvictSchema(schema);
            return Ok($"All cache entries for schema {schema} evicted.");
        }

        // 🔹 Refresh entire schema
        // POST api/cache/refresh/schema/{schema}
        [HttpPost("refresh/schema/{schema}")]
        public async Task<IActionResult> RefreshSchema(string schema)
        {
            if (!IsValidSchema(schema)) return Forbid("Access denied: invalid schema." + schema);
            await _cacheService.RefreshSchemaAsync(schema);
            return Ok($"Schema {schema} refreshed.");
        }

        private bool IsValidSchema(string schema) => string.Equals(schema, "hmsmaster", StringComparison.OrdinalIgnoreCase);
    }
}
//new Comment