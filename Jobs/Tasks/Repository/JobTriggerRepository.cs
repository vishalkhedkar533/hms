using CommonLibrary.mapping;
using Dapper;
using Newtonsoft.Json;
using Quartz;
using Repository;
using System.Data.Common;
using System.Text.Json.Serialization;
using Tasks.Models;

namespace Tasks.Repository
{
    internal class JobTriggerRepository : IJobTriggerRepository
    {   
        private readonly IMappingProvider _mappingProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IConnectionScope _connection_scope;
        private string connectionString;
        private OperationMapping? operationMapping;
        private DbConnection conn;
        private int orgId = 0;
        private JobKey jobKey;

        public JobTriggerRepository(
            IConnectionScope connectionScope,
            IMappingProvider mappingProvider,
            Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _connection_scope = connectionScope ?? throw new ArgumentNullException(nameof(connectionScope));
            _mappingProvider = mappingProvider ?? throw new ArgumentNullException(nameof(mappingProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<JobExeHist> CreateJobTriggerDetails(IJobExecutionContext jobExecutionContext)
        {
            if (jobExecutionContext is null) throw new ArgumentNullException(nameof(jobExecutionContext));

            // safe extraction of orgId
            var dataMap = jobExecutionContext.JobDetail.JobDataMap;
            if (dataMap.ContainsKey("orgId") && int.TryParse(dataMap.GetString("orgId"), out var parsedOrg))
            {
                orgId = parsedOrg;
            }

            jobKey = jobExecutionContext.JobDetail.Key;
            if (string.IsNullOrEmpty(jobKey?.Name) || !int.TryParse(jobKey.Name, out _))
            {
                throw new InvalidOperationException($"Job key name is not a valid integer: '{jobKey?.Name ?? "<null>"}'. Expected job_config_id as integer in JobKey.Name.");
            }

            #region RecordStartOfJob
            operationMapping = _mappingProvider.GetScriptForOperation("Job", "CreateExecutionCycle")
                ?? throw new InvalidOperationException("Operation mapping for Job/CreateExecutionCycle not found.");

            connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

            conn = await _connection_scope.GetOpenConnectionAsync(connectionString);

            int? fireInstanceId = null;
            if (!string.IsNullOrEmpty(jobExecutionContext.FireInstanceId) && int.TryParse(jobExecutionContext.FireInstanceId, out var fid))
            {
                fireInstanceId = fid;
            }

            var parameters = new
            {
                job_config_id = int.Parse(jobKey.Name),
                exe_status = "CREATED",
                orgid = orgId,
                FireInstanceId = fireInstanceId,
                TriggerObject = JsonConvert.SerializeObject(jobExecutionContext.Trigger),
                JobDetailObject = JsonConvert.SerializeObject(jobExecutionContext.JobDetail)
            };

            // Use QuerySingleOrDefaultAsync<long?> so missing return is detectable
            long? JobTriggerCreated;
            try
            {
                JobTriggerCreated = await conn.QuerySingleOrDefaultAsync<long?>(operationMapping.Script, parameters);
            }
            catch (Exception ex)
            {
                // rethrow with context to help debugging
                throw new InvalidOperationException($"Error executing CreateExecutionCycle SQL. SQL should return the generated id (e.g. use RETURNING). SQL: {operationMapping.Script}", ex);
            }

            if (!JobTriggerCreated.HasValue || JobTriggerCreated.Value == 0)
            {
                throw new InvalidOperationException("CreateExecutionCycle did not return a generated id. Ensure the SQL mapping includes a RETURNING clause (e.g. INSERT ... RETURNING job_exe_hist_id) or otherwise selects the id.");
            }
            #endregion RecordStartOfJob

            return new JobExeHist
            {
                JobExeHistId = JobTriggerCreated.Value,
                JobConfigId = int.Parse(jobKey.Name),
                ExeStatus = "CREATED",
                OrgId = orgId,
                FireInstanceId = fireInstanceId,
                TriggerObject = JsonConvert.SerializeObject(jobExecutionContext.Trigger),
                JobDetail = JsonConvert.SerializeObject(jobExecutionContext.JobDetail),
                FireTimeUtc = DateTime.UtcNow
            };
        }

        public Task<JobExeHist> UpdateJobTriggerDetails(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
