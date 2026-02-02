using Dapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using System.Collections.Concurrent;
using System.Data;
using System.Text;
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

        public async Task<IEnumerable<dynamic>> GetRecordsAsync(string schema,
            string table,
            Int64 OrgID = 0,
            string FilterCriteria = "",
            int? refreshIntervalMinutes = null)
        {
            string cacheKey = $"{OrgID}.{schema}.{table}.{FilterCriteria}";

            if (!_cache.TryGetValue(cacheKey, out IEnumerable<dynamic> records))
            {
                StringBuilder sql = new StringBuilder();
                sql.Append($"SELECT * FROM {QuoteIdentifier(schema)}.{QuoteIdentifier(table)} WHERE orgid= {OrgID}");

                if (!FilterCriteria.Equals(string.Empty)) sql.Append(FilterCriteria);

                if (_connection.State != ConnectionState.Open)
                    await _connection.OpenAsync();

                records = await _connection.QueryAsync(sql.ToString());

                _cache.Set(cacheKey, records, GetCacheOptions());
                TrackKey(schema, cacheKey, refreshIntervalMinutes);
            }

            return records;
        }

        // 🔹 Strongly Typed
        public async Task<IEnumerable<T>> GetRecordsAsync<T>(string schema,
    string table,
    Int64 OrgID = 0,
    string FilterCriteria = "",
    string ColumnAliases = "",
    int? refreshIntervalMinutes = null) where T : class
        {
            string cacheKey = $"{OrgID}.{schema}.{table}.{FilterCriteria}";
            // AND channel_id AS entryIdentity, channel_name AS entryDesc,
            // channel_code AS entryCategory, is_active AS activeStatus, orgid
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<T> records))
            {
                string selectColumns = "*";
                string rowFilters = string.Empty;

                // Logic to handle column mapping Aliases vs standard Filtering
                if (!string.IsNullOrEmpty(FilterCriteria))
                {
                    // Otherwise, it's a standard WHERE clause addition
                    rowFilters = FilterCriteria;
                }
                if (!string.IsNullOrEmpty(ColumnAliases))
                {
                    selectColumns = ColumnAliases;
                }

                StringBuilder sql = new StringBuilder();
                // Place columns in SELECT and filters in WHERE
                sql.Append($"SELECT {selectColumns} FROM {QuoteIdentifier(schema)}.{QuoteIdentifier(table)} ");
                sql.Append($"WHERE orgid = {OrgID}");

                if (!string.IsNullOrEmpty(rowFilters))
                {
                    sql.Append(rowFilters);
                }

                if (_connection.State != ConnectionState.Open)
                    await _connection.OpenAsync();

                // Log sql.ToString() here to verify the generated query
                records = await _connection.QueryAsync<T>(sql.ToString());

                _cache.Set(cacheKey, records, GetCacheOptions());
                TrackKey(schema, cacheKey, refreshIntervalMinutes, typeof(T));
            }

            return records;
        }

        public async Task<IEnumerable<dynamic>> RefreshCacheAsync(string schema, string table,
            Int64 OrgID = 0, string EntryCategory = "")
        {
            EvictCache(schema, table, OrgID, EntryCategory);
            var result = await GetRecordsAsync(schema, table);
            LastRefreshTimes[$"{schema}.{table}"] = DateTime.UtcNow;
            return result;
        }

        // 🔹 Refresh single (typed)
        public async Task<IEnumerable<T>> RefreshCacheAsync<T>(string schema, string table,
            Int64 OrgID = 0,
            string EntryCategory = "") where T : class
        {
            EvictCache<T>(schema, table, OrgID, EntryCategory);
            var result = await GetRecordsAsync<T>(schema, table);
            LastRefreshTimes[$"{schema}.{table}.{typeof(T).Name}"] = DateTime.UtcNow;
            return result;
        }

        // 🔹 Evict dynamic
        public void EvictCache(string schema, string table, Int64 OrgID = 0, string EntryCategory = "")
        {
            string dynamicKey = $"{OrgID}.{schema}.{table}.{EntryCategory}";
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
        public void EvictCache<T>(string schema, string table, Int64 OrgID = 0, string FilterCriteria = "") where T : class
        {
            string typedKey = $"{OrgID}.{schema}.{table}.{FilterCriteria}";
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
        public void EvictSchema(string schema, Int64 OrgID = 0)
        {
            if (SchemaKeys.TryRemove($"{OrgID}.{schema}", out var keys))
            {
                lock (keys)
                {
                    foreach (var key in keys)
                    {
                        _cache.Remove(key);
                        RefreshIntervals.TryRemove(key, out _);
                        LastRefreshTimes.TryRemove(key, out _);
                        TypeRegistry.TryRemove(key, out _);
                    }
                    keys.Clear();
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