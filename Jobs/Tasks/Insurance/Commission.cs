using Quartz;

namespace Tasks.Insurance
{
    public class Commission
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        private int orgId = 0;
        public JobKey jobKey;
        public Commission(IJobExecutionContext jobExecutionContext)
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
        }

        public void Calculate()
        {
            // Execution logic for commission calculation
            Console.WriteLine($"Calculating commissions as per job config: {_jobExecutionContext.JobDetail.Key}");
        }
    }
}