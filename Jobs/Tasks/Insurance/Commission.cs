using Quartz;

namespace Tasks.Insurance
{
    public class Commission
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        public Commission(IJobExecutionContext jobExecutionContext)
        {
            _jobExecutionContext = jobExecutionContext;
        }

        public void Calculate()
        {
            // Execution logic for commission calculation
            Console.WriteLine($"Calculating commissions as per job config: {_jobExecutionContext.JobDetail.Key}");
        }
    }
}