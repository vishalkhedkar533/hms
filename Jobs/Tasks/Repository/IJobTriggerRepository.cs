using Quartz;
using Tasks.Models;

namespace Tasks.Repository
{
    public interface IJobTriggerRepository
    {
        Task<JobExeHist> CreateJobTriggerDetails(IJobExecutionContext context);
        Task<JobExeHist> UpdateJobTriggerDetails(IJobExecutionContext context);
    }
}
