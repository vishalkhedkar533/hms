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
        private readonly IMappingProvider _mappingProvider;
        private readonly IConfiguration _configuration;
        public JobConfigRepository(IConnectionFactory connectionFactory, IConfiguration configuration
            , IMappingProvider mappingProvider)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _mappingProvider = mappingProvider ?? throw new ArgumentNullException(nameof(mappingProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IEnumerable<JobConfig>> GetEnabledAsync()
        {
            OperationMapping operationMapping = _mappingProvider.GetScriptForOperation("Job", "Fetch");
            await using var conn = (System.Data.Common.DbConnection)_connectionFactory
                .CreateConnection(_configuration
                .GetConnectionString( operationMapping.ConnectionStringKey));
            await conn.OpenAsync();
            var rows = await conn.QueryAsync<JobConfig>(operationMapping.Script, commandType: CommandType.Text);
            return rows.ToList();
        }
    }
}