using CommonLibrary.mapping;
using Dapper;
using Database;
using Quartz;
using Repository;
using SharedModels.BackEndCalculation;
using System.Data.Common;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

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
        private readonly ILogger<Commission> _logger;
        private readonly IConnectionScope _connectionScope;
        private readonly IBinaryImportFactory _bulkOpsFactory;
        private string connectionString;
        private OperationMapping? operationMapping;
        private DbConnection conn;
        public Commission(IJobExecutionContext jobExecutionContext,
            IMappingProvider mappingProvider,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            ILogger<Commission> logger,
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
            operationMapping = _mappingProvider.GetScriptForOperation("Commission", "ConfigByID")
                ?? throw new InvalidOperationException("Operation mapping for Commission/GetCommissionData not found.");

            connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

            conn = await _connectionScope.GetOpenConnectionAsync(connectionString);
            var commission_config = await conn.QueryFirstOrDefaultAsync<CommissionConfig>(
                operationMapping.Script,
                new { job_config_id = int.Parse(jobKey.Name), orgid = orgId });

            #region frameFormulafromDatabase
            var parameters = new[] {
                 Expression.Parameter(typeof(PremiumCollected), "premium"),
                 Expression.Parameter(typeof(Ins_Policy), "policy"),
                 Expression.Parameter(typeof(Agent), "agent"),
                 Expression.Parameter(typeof(Insured), "insured"),
                 Expression.Parameter(typeof(Owner), "owner"),
                 Expression.Parameter(typeof(CommRate), "commrate")
             };

            var e = DynamicExpressionParser.ParseLambda(parameters, typeof(decimal), commission_config.Formula);
            var compiledFormula = e.Compile();
            #endregion frameFormulafromDatabase

            #region RecordStartOfJob
            try
            {
                operationMapping = _mappingProvider.GetScriptForOperation("Job", "CreateExecutionCycle")
                    ?? throw new InvalidOperationException("Operation mapping for Job/CreateExecutionCycle not found.");

                connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                    ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");
                
                conn = await _connectionScope.GetOpenConnectionAsync(connectionString);
                //@job_config_id, now(), null, @exe_status, null, @orgid
                int JobTriggerCreated = (await conn.QueryAsync<int>(operationMapping.Script, new
                {
                    job_config_id = int.Parse(jobKey.Name),
                    exe_status = "CREATED",
                    orgid = orgId
                })).SingleOrDefault();
                if (JobTriggerCreated > 0)//record created and ID returned
                {
                    #region getListOfPremiumCollectedAgentPolicy
                    Console.WriteLine($"Calculating commissions as per job config: {_jobExecutionContext.JobDetail.Key}");
                    operationMapping = _mappingProvider.GetScriptForOperation("Commission", "GetCommissionData")
                        ?? throw new InvalidOperationException("Operation mapping for Commission/GetCommissionData not found.");

                    connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                        ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

                    conn = await _connectionScope.GetOpenConnectionAsync(connectionString);

                    Console.WriteLine($"Calculating commissions as per job config: {_jobExecutionContext.JobDetail.Key}");

                    var CommCalcInput = await conn.QueryAsync<
                        PremiumCollected,
                        Ins_Policy,
                        Agent,
                        Insured,
                        Owner,
                        CommRate,
                        CommissionCalcRecord>(
                        operationMapping.Script,
                        (prem, pol, agnt, ins, own, rate) => new CommissionCalcRecord
                        {
                            PremiumCollected = prem,
                            Policy = pol,
                            Agent = agnt,
                            Insured = ins,
                            Owner = own,
                            CommRate = rate
                        },
                        // The markers where each NEW object starts in the SELECT list:
                        new { orgid  = orgId}, null, splitOn: "PolicyRef,AgentId,InsuredID,OwnerID,CommRateId"
                    );
                    #endregion getListOfPremiumCollectedAgentPolicy
                    foreach (var record in CommCalcInput)
                    {
                        try
                        {
                            if (record.Policy == null || record.Agent == null || record.PremiumCollected == null)
                            {
                                _logger.LogWarning("Skipping record due to null values: {@Record}", record);
                                continue;
                            }
                            var comm_amt = compiledFormula.DynamicInvoke(record.PremiumCollected
                                , record.Policy, record.Agent, record.Insured
                                , record.Owner, record.CommRate);
                            operationMapping = _mappingProvider.GetScriptForOperation("Commission", "SaveCommissionPayable")
                                ?? throw new InvalidOperationException("Operation mapping for Commission/GetCommissionData not found.");

                            connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");
                            conn = await _connectionScope.GetOpenConnectionAsync(connectionString);

                            conn.Execute(operationMapping.Script, new
                            {
                                job_exe_hist_id = JobTriggerCreated,
                                agent_id = record.Agent.AgentId,
                                premiucollid = record.PremiumCollected.PremiuCollId,
                                premium_amt = record.PremiumCollected.PremiumAmt,                                
                                orgid = orgId,
                                formula = commission_config.Formula,
                                comm_amt = comm_amt,
                                logs = string.Empty
                            });

                            Console.WriteLine(comm_amt);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            #endregion RecordStartOfJob

        }
    }
}