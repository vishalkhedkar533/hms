using Dapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using System.Collections.Concurrent;
using System.Data;
using System.Text.RegularExpressions;

namespace HMS.Caching
{
    //Sample Code
    // Strongly typed (auto-refresh every 15 min)
    //await _cacheService.GetRecordsAsync<Customer>("hms", "customer", refreshIntervalMinutes: 15);
    // Dynamic (auto-refresh every 30 min)
    //await _cacheService.GetRecordsAsync("hms", "orders", refreshIntervalMinutes: 30);
    // Evict single
    //_cacheService.EvictCache("hms", "customer");
    // Refresh schema (reload all cached tables in schema)
    //await _cacheService.RefreshSchemaAsync("hms");

    public class GenericCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly NpgsqlConnection _connection;

        private static readonly ConcurrentDictionary<string, HashSet<string>> SchemaKeys
            = new(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<string, int> RefreshIntervals
            = new(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<string, DateTime> LastRefreshTimes
            = new(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<string, Type> TypeRegistry
            = new(StringComparer.OrdinalIgnoreCase);

        private static readonly Regex PgIdentifier = new(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

        public GenericCacheService(IMemoryCache cache, NpgsqlConnection connection)
        {
            _cache = cache;
            _connection = connection;
        }

        private MemoryCacheEntryOptions GetCacheOptions() =>
            new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

        private static string QuoteIdentifier(string identifier)
        {
            if (!PgIdentifier.IsMatch(identifier))
                throw new ArgumentException($"Invalid identifier: {identifier}");
            return $"\"{identifier}\"";
        }

        private void TrackKey(string schema, string key, int? refreshIntervalMinutes, Type? modelType = null)
        {
            SchemaKeys.AddOrUpdate(schema,
                _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { key },
                (_, set) =>
                {
                    lock (set) { set.Add(key); }
                    return set;
                });

            if (refreshIntervalMinutes.HasValue)
                RefreshIntervals[key] = refreshIntervalMinutes.Value;

            LastRefreshTimes[key] = DateTime.UtcNow;

            if (modelType != null)
                TypeRegistry[key] = modelType;
        }

        // 🔹 Dynamic
        public async Task<IEnumerable<dynamic>> GetRecordsAsync(int orgId, string entryCategory, int? refreshIntervalMinutes = null)
        {
            string cacheKey = $"KV.{orgId}.{entryCategory}";

            if (!_cache.TryGetValue(cacheKey, out IEnumerable<dynamic> records))
            {
                string sql = @"SELECT * FROM hmsmaster.KeyValueEntries 
                               WHERE OrgId = @OrgId AND EntryCategory = @EntryCategory";

                if (_connection.State != ConnectionState.Open)
                    await _connection.OpenAsync();

                records = await _connection.QueryAsync(sql, new { OrgId = orgId, EntryCategory = entryCategory });

                _cache.Set(cacheKey, records, GetCacheOptions());
                TrackKey("KeyValueEntries", cacheKey, refreshIntervalMinutes);
            }

            return records;
        }
        // 🔹 Strongly Typed
        public async Task<IEnumerable<T>> GetRecordsAsync<T>(string schema, string table, int? refreshIntervalMinutes = null)
        {
            string cacheKey = $"{schema}.{table}.{typeof(T).Name}";

            if (!_cache.TryGetValue(cacheKey, out IEnumerable<T> records))
            {
                string sql = $"SELECT * FROM {QuoteIdentifier(schema)}.{QuoteIdentifier(table)}";

                if (_connection.State != ConnectionState.Open)
                    await _connection.OpenAsync();

                records = await _connection.QueryAsync<T>(sql);

                _cache.Set(cacheKey, records, GetCacheOptions());
                TrackKey(schema, cacheKey, refreshIntervalMinutes, typeof(T));
            }

            return records;
        }

        // 🔹 Refresh single (dynamic)
        public async Task<IEnumerable<dynamic>> RefreshCacheAsync(string schema, string table)
        {
            string cacheKey = $"{schema}.{table}";

            _cache.Remove(cacheKey);

            string sql = $"SELECT * FROM \"{schema}\".\"{table}\"";

            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();

            var result = await _connection.QueryAsync(sql);

            _cache.Set(cacheKey, result, GetCacheOptions());

            LastRefreshTimes[cacheKey] = DateTime.UtcNow;

            return result;
        }

        // 🔹 Refresh single (typed)
        public async Task<IEnumerable<T>> RefreshCacheAsync<T>(string schema, string table)
        {
            string key = $"{schema}.{table}.{typeof(T).Name}";
            _cache.Remove(key);

            return await GetRecordsAsync<T>(schema, table);
        }

        // 🔹 Evict dynamic
        public void EvictCache(string schema, string table)
        {
            string dynamicKey = $"{schema}.{table}";
            _cache.Remove(dynamicKey);

            RefreshIntervals.TryRemove(dynamicKey, out _);
            LastRefreshTimes.TryRemove(dynamicKey, out _);
            TypeRegistry.TryRemove(dynamicKey, out _);

            if (SchemaKeys.TryGetValue(schema, out var set))
            {
                lock (set) { set.Remove(dynamicKey); }
            }
        }

        // 🔹 Evict typed
        public void EvictCache<T>(string schema, string table) where T : class
        {
            string typedKey = $"{schema}.{table}.{typeof(T).Name}";
            _cache.Remove(typedKey);

            RefreshIntervals.TryRemove(typedKey, out _);
            LastRefreshTimes.TryRemove(typedKey, out _);
            TypeRegistry.TryRemove(typedKey, out _);

            if (SchemaKeys.TryGetValue(schema, out var set))
            {
                lock (set) { set.Remove(typedKey); }
            }
        }

        // 🔹 Evict all tables in schema
        public void EvictSchema(string schema)
        {
            if (!SchemaKeys.TryRemove(schema, out var keys))
                return;

            lock (keys)
            {
                foreach (var key in keys)
                {
                    _cache.Remove(key);
                    RefreshIntervals.TryRemove(key, out _);
                    LastRefreshTimes.TryRemove(key, out _);
                    TypeRegistry.TryRemove(key, out _);
                }
            }
        }

        // 🔹 Refresh all tables in schema
        public async Task RefreshSchemaAsync(string schema)
        {
            if (SchemaKeys.TryGetValue(schema, out var keys))
            {
                foreach (var key in keys.ToList())
                {
                    var parts = key.Split('.');
                    if (parts.Length >= 2)
                    {
                        var table = parts[1];

                        if (TypeRegistry.TryGetValue(key, out var modelType) && modelType != null)
                        {
                            var method = typeof(GenericCacheService)
                                .GetMethod(nameof(RefreshCacheAsync), new[] { typeof(string), typeof(string) })
                                ?.MakeGenericMethod(modelType);

                            if (method != null)
                                await (Task)method.Invoke(this, new object[] { schema, table })!;
                        }
                        else
                        {
                            await RefreshCacheAsync(schema, table);
                        }
                    }
                }
            }
        }

        // 🔹 For background refresh service
        public Dictionary<string, (int Interval, DateTime LastRefreshed, Type? ModelType)> GetRefreshConfig()
        {
            return RefreshIntervals.ToDictionary(
                kv => kv.Key,
                kv => (
                    kv.Value,
                    LastRefreshTimes.TryGetValue(kv.Key, out var ts) ? ts : DateTime.MinValue,
                    TypeRegistry.TryGetValue(kv.Key, out var t) ? t : null
                )
            );
        }
    }
}