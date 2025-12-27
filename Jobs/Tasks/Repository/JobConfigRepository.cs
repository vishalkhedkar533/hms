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
    }
}