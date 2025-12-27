using CommonLibrary.mapping;
using Dapper;
using Database;
using Models;
using System.Data;

namespace Repository
{
    public class JobConfigRepository : IJobConfigRepository
    {
        private readonly IConnectionScope _connectionScope;
        private readonly IMappingProvider _mappingProvider;
        private readonly IConfiguration _configuration;

        public JobConfigRepository(IConnectionScope connectionScope, IConfiguration configuration, IMappingProvider mappingProvider)
        {
            _connectionScope = connectionScope ?? throw new ArgumentNullException(nameof(connectionScope));
            _mappingProvider = mappingProvider ?? throw new ArgumentNullException(nameof(mappingProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IEnumerable<JobConfig>> GetEnabledAsync()
        {
            var operationMapping = _mappingProvider.GetScriptForOperation("Job", "Fetch")
                ?? throw new InvalidOperationException("Operation mapping for Job/Fetch not found.");

            var connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

            // Get (and reuse) an open connection from the scoped connection manager.
            var conn = await _connectionScope.GetOpenConnectionAsync(connectionString);

            // Do NOT dispose the returned connection here; the scope will dispose it when the DI scope ends.
            var rows = await conn.QueryAsync<JobConfig>(operationMapping.Script, commandType: CommandType.Text);
            return rows.ToList();
        }
    }
}