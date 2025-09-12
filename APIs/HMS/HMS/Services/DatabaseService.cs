//using System.Data.SqlClient;
using Dapper;
//using Oracle.ManagedDataAccess.Client;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.Data;
using System.Data.Common;

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

            // Detect provider (PostgreSQL / MSSQL / Oracle)
            _provider = _config.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";

            var mappingFile = Path.Combine(AppContext.BaseDirectory, $"mappings.{_provider.ToLower()}.json");
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
                "postgresql" => new NpgsqlConnection(_connectionString)
                //, "mssql" => new SqlConnection(_connectionString)
                //, "oracle" => new OracleConnection(_connectionString)
                , _ => throw new NotSupportedException($"Database provider {_provider} is not supported")
            };
        }

        private string GetScript(string entity, string action)
        {
            var script = _dbMappings["Entities"]?[entity]?[action]?["Script"]?.ToString();
            if (string.IsNullOrEmpty(script))
                throw new InvalidOperationException($"Script not defined for {entity}.{action}");
            return script;
        }

        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(
            string entity,
            string action,
            object parameters)
        {
            var script = GetScript(entity, action);
            using var conn = CreateConnection();
            await conn.OpenAsync(); // ✅ works since DbConnection exposes it
            return await conn.QueryAsync<T>(script, parameters, commandType: CommandType.Text);
        }

        public async Task<T> ExecuteScalarAsync<T>(
            string entity,
            string action,
            object parameters)
        {
            var script = GetScript(entity, action);
            using var conn = CreateConnection();
            await conn.OpenAsync(); // ✅ async open works here too
            return await conn.ExecuteScalarAsync<T>(script, parameters, commandType: CommandType.Text);
        }
    }
}
