using CommonLibrary.mapping;
using Dapper;
using Database;
using Models;
using System.Data;

namespace Repository
{
    public class JobConfigRepository : IJobConfigRepository
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly string _connectionString;
        private readonly IMappingProvider _mappingProvider;
        public JobConfigRepository(IConnectionFactory connectionFactory, IConfiguration configuration
            , IMappingProvider mappingProvider)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _connectionString = configuration.GetConnectionString("HMSContext")
                                ?? configuration.GetValue<string>("ConnectionString")
                                ?? throw new ArgumentException("Connection string 'HMSContext' not found.");
            _mappingProvider = mappingProvider ?? throw new ArgumentNullException(nameof(mappingProvider));
        }

        public async Task<IEnumerable<JobConfig>> GetEnabledAsync()
        {
            string sql = _mappingProvider.GetScriptForOperation("Job", "Insert");
            await using var conn = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection(_connectionString);
            await conn.OpenAsync();
            var rows = await conn.QueryAsync<JobConfig>(sql, commandType: CommandType.Text);
            return rows.ToList();
        }

        public async Task<JobConfig?> GetByIdAsync(int id)
        {
            const string sql = @"
SELECT job_config_id, job_name, job_type, enabled, trigger_type, cron_expression,
       interval_seconds, start_at, end_at, parameters, created_at, updated_at
FROM scheduler.job_config
WHERE job_config_id = @Id
LIMIT 1;
";
            await using var conn = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection(_connectionString);
            await conn.OpenAsync();
            var row = await conn.QueryFirstOrDefaultAsync<JobConfig>(sql, new { Id = id }, commandType: CommandType.Text);
            return row;
        }
    }
}