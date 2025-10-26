using Dapper;
using Models.DB;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace HMS.Services
{
    public class DatabaseService
    {
        private readonly IConfiguration _config;
        private readonly JObject _dbMappings;
        private readonly string _provider;
        private readonly string _connectionString;

        public DatabaseService(IConfiguration config)
        {
            _config = config;

            _provider = _config.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";

            var mappingFile = Path.Combine(AppContext.BaseDirectory, "Mappings", $"mappings.{_provider.ToLower()}.json");
            if (!File.Exists(mappingFile))
                throw new FileNotFoundException("Database mapping file not found.", mappingFile);

            _dbMappings = JObject.Parse(File.ReadAllText(mappingFile));

            var connKey = _dbMappings["ConnectionStringKey"]?.ToString();
            if (string.IsNullOrEmpty(connKey))
                throw new InvalidOperationException("ConnectionStringKey missing in mappings file.");

            _connectionString = _config.GetConnectionString(connKey)!;
        }

        private DbConnection CreateConnection()
        {
            return _provider.ToLower() switch
            {
                "postgresql" => new NpgsqlConnection(_connectionString),
                _ => throw new NotSupportedException($"Database provider {_provider} is not supported")
            };
        }

        private string GetScript(string entity, string action)
        {
            var script = _dbMappings["Entities"]?[entity]?[action]?["Script"]?.ToString();
            if (string.IsNullOrEmpty(script))
                throw new InvalidOperationException($"Script not defined for {entity}.{action}");

            // ✅ Auto-cast any ltree columns to text
            if (_provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                script = CastLtreeColumnsToText(script);
            }

            return script;
        }

        /// <summary>
        /// Detects columns or expressions that look like "alias.column" or "column"
        /// named something like "hierarchy_path" or declared as ltree and casts them to text.
        /// </summary>
        private string CastLtreeColumnsToText(string script)
        {
            // Extract SELECT ... FROM part
            var selectMatch = Regex.Match(script, @"(?is)^(\s*SELECT\s+)(.+?)(\s+FROM\s+)", RegexOptions.IgnoreCase);
            if (!selectMatch.Success) return script;

            var selectClause = selectMatch.Groups[2].Value;

            // Split the SELECT clause by commas to handle each column separately
            var columns = selectClause.Split(',')
                .Select(c =>
                {
                    var trimmed = c.Trim();

                    // Check if the column already has ::text or is an alias (AS ...)
                    if (trimmed.Contains("::text", StringComparison.OrdinalIgnoreCase) ||
                        Regex.IsMatch(trimmed, @"\s+AS\s+", RegexOptions.IgnoreCase))
                    {
                        return trimmed;
                    }

                    // Check if the column name matches ltree columns
                    var colRegex = new Regex(@"\b([\w]+\.)?(hierarchy_path|ltree_path|path)\b", RegexOptions.IgnoreCase);
                    if (colRegex.IsMatch(trimmed))
                    {
                        return colRegex.Replace(trimmed, m => $"{m.Value}::text");
                    }

                    return trimmed;
                });

            var modifiedSelect = string.Join(", ", columns);

            // Rebuild the SQL
            return script.Substring(0, selectMatch.Groups[1].Index)
                   + selectMatch.Groups[1].Value
                   + modifiedSelect
                   + selectMatch.Groups[3].Value
                   + script.Substring(selectMatch.Groups[3].Index + selectMatch.Groups[3].Length);
        }



        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string entity, string action, object parameters)
        {
            try
            {
                var script = GetScript(entity, action);
                using var conn = CreateConnection();
                await conn.OpenAsync();
                var result = await conn.QueryAsync<T>(script, parameters, commandType: CommandType.Text);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExecuteQueryAsync: {ex.Message}");
                return Enumerable.Empty<T>();
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string entity, string action, object parameters)
        {
            var script = GetScript(entity, action);
            using var conn = CreateConnection();
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<T>(script, parameters, commandType: CommandType.Text);
        }
    }
}
