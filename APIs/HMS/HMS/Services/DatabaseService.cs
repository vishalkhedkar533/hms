//using System.Data.SqlClient;
using Dapper;
using Models.DB;

//using Oracle.ManagedDataAccess.Client;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
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
                "postgresql" => new NpgsqlConnection(_connectionString)
                //, "mssql" => new SqlConnection(_connectionString)
                //, "oracle" => new OracleConnection(_connectionString)
                ,
                _ => throw new NotSupportedException($"Database provider {_provider} is not supported")
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
            try
            {
                var script = GetScript(entity, action);
                using var conn = CreateConnection();
                await conn.OpenAsync(); //works since DbConnection exposes it
                return await conn.QueryAsync<T>(script, parameters, commandType: CommandType.Text);
            }
            catch (Exception ex)
            {

                return Enumerable.Empty<T>();
            }

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


        public async Task<List<DtoAgentLicenseRes>> SaveAgentLicenseAsync(DtoAgentLicense agentLicense)
        {
            var result = new List<DtoAgentLicenseRes>();
            //using var conn = CreateConnection();
            string connString = _connectionString;// "Host=localhost;Port=5432;Username=postgres;Password=yourpassword;Database=yourdb";

            using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            using var cmd1 = new NpgsqlCommand("SELECT * FROM Agent.SaveAgentlicense(@p_agent_id, @p_license_no, @p_license_type_code, @p_effective_from_date, @p_created_by, @p_effective_to_date, @p_license_status, @p_modified_by, @p_rowversion)", conn);

            cmd1.Parameters.Add(new NpgsqlParameter("p_agent_id", NpgsqlDbType.Integer) { Value = 1 });
            cmd1.Parameters.Add(new NpgsqlParameter("p_license_no", NpgsqlDbType.Text) { Value = "ADASD1221" });
            cmd1.Parameters.Add(new NpgsqlParameter("p_license_type_code", NpgsqlDbType.Text) { Value = "DL" });
            cmd1.Parameters.Add(new NpgsqlParameter("p_effective_from_date", NpgsqlDbType.Date) { Value = DateTime.Today });
            cmd1.Parameters.Add(new NpgsqlParameter("p_created_by", NpgsqlDbType.Text) { Value = "System" });
            cmd1.Parameters.Add(new NpgsqlParameter("p_effective_to_date", NpgsqlDbType.Date) { Value = DateTime.Today });
            cmd1.Parameters.Add(new NpgsqlParameter("p_license_status", NpgsqlDbType.Text) { Value = "A" });
            cmd1.Parameters.Add(new NpgsqlParameter("p_modified_by", NpgsqlDbType.Text) { Value = "System" });
            cmd1.Parameters.Add(new NpgsqlParameter("p_rowversion", NpgsqlDbType.Integer) { Value = 1 });

            using var reader = await cmd1.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var dto = new DtoAgentLicenseRes
                {
                    // Map your columns here
                    //LicenseId = reader.GetInt32(reader.GetOrdinal("license_id")),
                    //LicenseNo = reader.GetString(reader.GetOrdinal("license_no")),
                    //Status = reader.GetString(reader.GetOrdinal("license_status")),
                    // Add other fields as needed
                };

                result.Add(dto);
            }

            return result;
        }

    }
}
