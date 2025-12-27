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
        public JobConfigRepository(IConnectionFactory connectionFactory, IConfiguration configuration
            , IMappingProvider mappingProvider)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _mappingProvider = mappingProvider ?? throw new ArgumentNullException(nameof(mappingProvider));
        }

        public async Task<IEnumerable<JobConfig>> GetEnabledAsync()
        {
            OperationMapping operationMapping = _mappingProvider.GetScriptForOperation("Job", "Fetch");
            await using var conn = (System.Data.Common.DbConnection)_connectionFactory.CreateConnection(operationMapping.ConnectionStringKey);
            await conn.OpenAsync();
            var rows = await conn.QueryAsync<JobConfig>(operationMapping.Script, commandType: CommandType.Text);
            return rows.ToList();
        }
    }
}