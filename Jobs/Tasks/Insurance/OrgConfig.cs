using CommonLibrary.mapping;
using Dapper;
using Database;
using Quartz;
using Repository;
using System.Data.Common;
using System.Linq.Dynamic.Core;
using Tasks.Models;
using Tasks.Repository;

namespace Tasks.Finance
{
    public class OrgConfig
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        private int orgId = 0;
        public JobKey jobKey;

        private readonly IMappingProvider _mappingProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _baseDir = AppContext.BaseDirectory;
        private readonly ILogger<OrgConfig> _logger;
        private readonly IConnectionScope _connectionScope;
        private readonly IBinaryImportFactory _bulkOpsFactory;
        private string connectionString;
        private OperationMapping? operationMapping;
        private DbConnection conn;
        private readonly IJobTriggerRepository _jobTriggerRepository;
        public  OrgConfig(IJobExecutionContext jobExecutionContext,
            IMappingProvider mappingProvider,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            ILogger<OrgConfig> logger,
            IConnectionScope connectionScope,
            IBinaryImportFactory bulkOpsFactory,
            IJobTriggerRepository jobTriggerRepository)
        {
            _jobExecutionContext = jobExecutionContext;
            orgId = int.Parse(jobExecutionContext.JobDetail.JobDataMap.Values
                .Select((value, index) => new { value, index })
                .FirstOrDefault(x => x.index.Equals(
                    jobExecutionContext.JobDetail.JobDataMap.Keys
                    .Select((value, index) => new { value, index })
                    .FirstOrDefault(x => x.value == "orgId").index))
                .value.ToString());
            jobKey = jobExecutionContext.JobDetail.Key;
            _mappingProvider = mappingProvider;
            _configuration = configuration;
            _logger = logger;
            _connectionScope = connectionScope ?? throw new ArgumentNullException(nameof(connectionScope));
            _bulkOpsFactory = bulkOpsFactory ?? throw new ArgumentNullException(nameof(bulkOpsFactory));
            _jobTriggerRepository = jobTriggerRepository;
        }

        public async Task SetMonthlyCondfiguration(JobExeHist jobExeHist,CancellationToken ct)
        {
            string sqlCommand = string.Empty;
            sqlCommand = _mappingProvider.GetScriptForOperation("Organisation", "GenerateFinancialMonths")?.Script;
            using (var tx = await conn.BeginTransactionAsync(ct))
            {
                var command = new CommandDefinition(
                    commandText: sqlCommand,
                    parameters: new { OrgId = jobExeHist.OrgId }, // Assuming OrgId is in jobExeHist
                    transaction: tx,
                    cancellationToken: ct
                    );
                await conn.ExecuteAsync(command);
                await tx.CommitAsync(ct);
            }
            sqlCommand = _mappingProvider.GetScriptForOperation("Organisation", "GenerateFinancialQuarters")?.Script;

            using (var tx = await conn.BeginTransactionAsync(ct))
            {
                var command = new CommandDefinition(
                    commandText: sqlCommand,
                    parameters: new { OrgId = jobExeHist.OrgId }, // Assuming OrgId is in jobExeHist
                    transaction: tx,
                    cancellationToken: ct
                    );
                await conn.ExecuteAsync(command);
                await tx.CommitAsync(ct);
            }
        }
    }
}
