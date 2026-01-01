using Quartz;

namespace Tasks.Insurance
{
    public class AgentCreateExcel
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        public AgentCreateExcel(IJobExecutionContext jobExecutionContext)
        {
            _jobExecutionContext = jobExecutionContext;
        }
    }
}