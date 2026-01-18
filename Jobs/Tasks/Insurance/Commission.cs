using CommonLibrary.mapping;
using Dapper;
using Database;
using Quartz;
using Repository;
using SharedModels.BackEndCalculation;
using System.Data.Common;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Tasks.Models;
using Tasks.Repository;

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
        private readonly IJobTriggerRepository _jobTriggerRepository;
        public Commission(IJobExecutionContext jobExecutionContext,
            IMappingProvider mappingProvider,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            ILogger<Commission> logger,
            IConnectionScope connectionScope,
            IBinaryImportFactory bulkOpsFactory,
            IJobTriggerRepository jobTriggerRepository
            )
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

        public async Task Calculate(JobExeHist jobExeHist)
        {
            operationMapping = _mappingProvider.GetScriptForOperation("Job", "GetJobExtn")
                ?? throw new InvalidOperationException("Operation mapping for Commission/GetCommissionData not found.");

            connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

            conn = await _connectionScope.GetOpenConnectionAsync(connectionString);

            operationMapping = _mappingProvider.GetScriptForOperation("Job", "GetLastJobTriggered")
                ?? throw new InvalidOperationException("Operation mapping for Job/CreateExecutionCycle not found.");

            JobExeHist LastExecutedJob = await conn.QuerySingleOrDefaultAsync<JobExeHist>(operationMapping.Script, new
            {
                job_config_id = int.Parse(jobKey.Name),
                orgid = orgId,
                fireinstanceid = null as long?
            });

            var jobExtn = await conn.QueryFirstOrDefaultAsync<JobExtn>(
                operationMapping.Script,
                new { orgid = orgId, job_config_id = int.Parse(jobKey.Name) });

            JobExeHist jobTriggerDetails = await _jobTriggerRepository.CreateJobTriggerDetails(_jobExecutionContext);
            operationMapping = _mappingProvider.GetScriptForOperation("Commission", "ConfigByID")
                ?? throw new InvalidOperationException("Operation mapping for Commission/GetCommissionData not found.");

            connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

            var commission_config = await conn.QueryFirstOrDefaultAsync<CommissionConfig>(
                operationMapping.Script,
                new { job_config_id = int.Parse(jobKey.Name), 
                    orgid = orgId});

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

            try
            {
                #region getListOfPremiumCollectedAgentPolicy
                Console.WriteLine($"Calculating commissions as per job config: {_jobExecutionContext.JobDetail.Key}");
                operationMapping = _mappingProvider.GetScriptForOperation("Commission", "GetCommissionData")
                    ?? throw new InvalidOperationException("Operation mapping for Commission/GetCommissionData not found.");

                connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                    ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

                string FilterCriteria = string.IsNullOrEmpty(jobExtn?.Filter) ? " 1=1 " : jobExtn.Filter;
                FilterCriteria = FilterCriteria.Replace("&&", " AND ");
                FilterCriteria = FilterCriteria.Replace("||", " OR ");

                operationMapping.Script = operationMapping.Script.Replace("{{FilterCriteria}}", FilterCriteria);

                var CommCalcInput = await conn.QueryAsync<
                    PremiumCollected,Ins_Policy,Agent,Insured,Owner,CommRate,CommissionCalcRecord>(
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
                    new { orgid = orgId, job_exe_hist_id = LastExecutedJob?.JobExeHistId, job_config_id = int.Parse(jobKey.Name) }, 
                    null, 
                    splitOn: "PolicyRef,AgentId,InsuredID,OwnerID,CommRateId"
                );
                #endregion getListOfPremiumCollectedAgentPolicy

                foreach (var record in CommCalcInput)
                {
                    if (record.Policy == null || record.Agent == null || record.PremiumCollected == null)
                    {
                        conn.Execute(operationMapping.Script, new
                        {
                            job_exe_hist_id = jobExeHist.JobExeHistId,
                            agent_id = record?.Agent?.AgentId ?? -1000,
                            premiucollid = record?.PremiumCollected?.PremiuCollId ?? -1000,
                            premium_amt = record?.PremiumCollected?.PremiumAmt ?? 0,
                            orgid = orgId,
                            formula = commission_config.Formula,
                            comm_amt = 0,
                            logs = "Null Object Cannot proceed"
                        });
                        continue;
                    }
                    operationMapping = _mappingProvider.GetScriptForOperation("Commission", "GetProcessedRecords")
                    ?? throw new InvalidOperationException("Operation mapping for Commission/GetProcessedRecords not found.");

                    var processedRecords = await conn.QueryAsync<CommJobExeDtls>(operationMapping.Script,
                        new
                        {
                            orgid = orgId,
                            job_exe_hist_id = jobExeHist.JobExeHistId,
                            agent_id = record.Agent.AgentId,
                            premiucollid = record.PremiumCollected.PremiuCollId
                        });
                    if (processedRecords != null && processedRecords.Count() > 0)
                    {
                        //skip already processed records
                        continue;
                    }
                    operationMapping = _mappingProvider.GetScriptForOperation("Commission", "SaveCommissionPayable")
                        ?? throw new InvalidOperationException("Operation mapping for Commission/SaveCommissionPayable not found.");

                    connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                        ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

                    try
                    {
                        var comm_amt = compiledFormula.DynamicInvoke(record.PremiumCollected
                            , record.Policy, record.Agent, record.Insured
                            , record.Owner, record.CommRate);
                        conn.Execute(operationMapping.Script, new
                        {
                            job_exe_hist_id = jobExeHist.JobExeHistId,
                            agent_id = record.Agent.AgentId,
                            premiucollid = record.PremiumCollected.PremiuCollId,
                            premium_amt = record.PremiumCollected.PremiumAmt,
                            orgid = orgId,
                            formula = commission_config.Formula,
                            comm_amt = comm_amt,
                            logs = string.Empty
                        });
                    }
                    catch (Exception exCalc)
                    {
                        conn.Execute(operationMapping.Script, new
                        {
                            job_exe_hist_id = jobExeHist.JobExeHistId,
                            agent_id = record.Agent.AgentId,
                            premiucollid = record.PremiumCollected.PremiuCollId,
                            premium_amt = record.PremiumCollected.PremiumAmt,
                            orgid = orgId,
                            formula = commission_config.Formula,
                            comm_amt = 0,
                            logs = exCalc.Message
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                _jobTriggerRepository.WriteExecutionLogs(_jobExecutionContext,
                    new JobExeLog
                    {
                        OrgId = orgId,
                        JobExeHistId = jobExeHist.JobExeHistId,
                        ExeLogs = ex.Message,
                        LogLevel = nameof(LoggingLevel.Error)
                    },
                    LoggingLevel.Error).Wait();
            }
            JobExeHist CompletedJob = await _jobTriggerRepository.UpdateJobStatus(_jobExecutionContext , "COMPLETED");
        }
    }
}