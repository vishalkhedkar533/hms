using HMS.Caching;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly GenericCacheService _cacheService;
        private readonly IConfiguration _configuration;

        public CacheController(GenericCacheService cacheService, IConfiguration configuration)
        {
            _cacheService = cacheService;
            _configuration = configuration;
        }

        // 🔹 Fetch records (dynamic) - uses refreshInterval from appsettings.json
        // POST api/cache/get/hms/customer
        [HttpPost("get/{schema}/{table}")]
        public async Task<IActionResult> GetRecords(string schema, string table)
        {
            int refreshInterval = _configuration.GetValue<int>("Caching:refreshIntervalMinutes", 15);

            var result = await _cacheService.GetRecordsAsync(schema, table, refreshInterval);
            return Ok(result);
        }

        // 🔹 Refresh table (dynamic)
        // POST api/cache/refresh/hms/customer
        [HttpPost("refresh/{schema}/{table}")]
        public async Task<IActionResult> Refresh(string schema, string table)
        {
            var result = await _cacheService.RefreshCacheAsync(schema, table);
            return Ok(result);
        }

        // 🔹 Evict table
        // POST api/cache/evict/hms/customer
        [HttpPost("evict/{schema}/{table}")]
        public IActionResult Evict(string schema, string table)
        {
            _cacheService.EvictCache(schema, table);
            return Ok($"Cache for {schema}.{table} evicted.");
        }

        // 🔹 Evict entire schema
        // POST api/cache/evict/schema/hms
        [HttpPost("evict/schema/{schema}")]
        public IActionResult EvictSchema(string schema)
        {
            _cacheService.EvictSchema(schema);
            return Ok($"All cache entries for schema {schema} evicted.");
        }

        // 🔹 Refresh entire schema
        // POST api/cache/refresh/schema/{schema}
        [HttpPost("refresh/schema/{schema}")]
        public async Task<IActionResult> RefreshSchema(string schema)
        {
            await _cacheService.RefreshSchemaAsync(schema);
            return Ok($"Schema {schema} refreshed.");
        }
    }
}