using CommonLibrary.mapping;
using Dapper;
using Database;
using Quartz;
using Repository;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Tasks.Models.DB;

namespace Tasks.Insurance
{
    public class Commission
    {

        private readonly IJobExecutionContext _jobExecutionContext;
        private int orgId = 0;
        public JobKey jobKey;

        private readonly IMappingProvider _mappingProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _baseDir = AppContext.BaseDirectory;
        private readonly ILogger<AgentCreateExcel> _logger;
        private readonly IConnectionScope _connectionScope;
        private readonly IBinaryImportFactory _bulkOpsFactory;
        private string connectionString;
        private OperationMapping? operationMapping;
        public Commission(IJobExecutionContext jobExecutionContext,
            IMappingProvider mappingProvider,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            ILogger<AgentCreateExcel> logger,
            IConnectionScope connectionScope,
            IBinaryImportFactory bulkOpsFactory)
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

        }

        public async Task Calculate()
        {
            // Execution logic for commission calculation
            Console.WriteLine($"Calculating commissions as per job config: {_jobExecutionContext.JobDetail.Key}");
            /*
             using System.Linq.Dynamic.Core;
             using System.Linq.Expressions;
             
             // 1. Define your data (These could come from a DB)
             var pol = new Policy { Age = 25, Amount = 1000 };
             var agent = new Agent { Level = "Senior", CommissionRate = 0.05 };
             var premium = new Premium { TotalCollected = 1200 };
             
             // 2. The formula entered by the user in the UI
             // Example: "(pol.Age > 18 ? 0.5 : 0.1) * pol.Amount + (premium.TotalCollected * agent.CommissionRate)"
             string userFormula = "(pol.Age > 18 ? 0.5 : 0.1) * pol.Amount + (premium.TotalCollected * agent.CommissionRate)";
             
             // 3. Define the parameters for the engine
             var parameters = new[] {
                 Expression.Parameter(typeof(Policy), "pol"),
                 Expression.Parameter(typeof(Agent), "agent"),
                 Expression.Parameter(typeof(Premium), "premium")
             };
             
             // 4. Parse the formula into a compiled lambda
             var e = DynamicExpressionParser.ParseLambda(parameters, typeof(double), userFormula);
             var compiledFormula = e.Compile();
             
             // 5. Execute by passing the actual object instances
             var result = compiledFormula.DynamicInvoke(pol, agent, premium);
             
             Console.WriteLine($"Calculated Result: {result}");
             */
            operationMapping = _mappingProvider.GetScriptForOperation("Commission", "GetCommissionData")
                ?? throw new InvalidOperationException("Operation mapping for Commission/GetCommissionData not found.");

            connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

            var conn = await _connectionScope.GetOpenConnectionAsync(connectionString);

            var CommCalcInput = await conn.QueryAsync<
                PremiumCollected,
                Ins_Policy,
                Organisation,
                Agent,
                Insured,
                Owner,
                CommRate,
                CommissionCalcRecord>(
                operationMapping.Script,
                (prem, pol, org, agnt, ins, own, rate) => new CommissionCalcRecord
                {
                    PremiumCollected = prem,
                    Policy = pol,
                    Organisation = org,
                    Agent = agnt,
                    Insured = ins,
                    Owner = own,
                    CommRate = rate
                },
                // The markers where each NEW object starts in the SELECT list:
                splitOn: "PolicyRef,AgentId,InsuredID,OwnerID,CommRateId"
            );
            var parameters = new[] {
                 Expression.Parameter(typeof(PremiumCollected), "premium"),
                 Expression.Parameter(typeof(Ins_Policy), "policy"),
                 Expression.Parameter(typeof(Organisation), "org"),
                 Expression.Parameter(typeof(Agent), "agent"),
                 Expression.Parameter(typeof(Insured), "insured"),
                 Expression.Parameter(typeof(Owner), "owner"),
                 Expression.Parameter(typeof(CommRate), "commrate")
             };

            string userFormula = "(premium.PremiumAmt ?? 0) * commrate.CommRateValue";
            var e = DynamicExpressionParser.ParseLambda(parameters, typeof(decimal), userFormula);
            var compiledFormula = e.Compile();

            foreach (var record in CommCalcInput)
            {
                if (record.Policy == null || record.Agent == null || record.PremiumCollected == null)
                {
                    _logger.LogWarning("Skipping record due to null values: {@Record}", record);
                    continue;
                }
                var result = compiledFormula.DynamicInvoke(record.PremiumCollected
                    , record.Policy
                    , record.Organisation
                    , record.Agent
                    , record.Insured
                    , record.Owner
                    , record.CommRate);

                Console.WriteLine( result );
            }
        }
    }
}