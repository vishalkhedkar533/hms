using Quartz;
using Tasks.Models;

namespace Tasks.Repository
{
    public interface IJobTriggerRepository
    {
        Task<JobExeHist> CreateJobTriggerDetails(IJobExecutionContext context);
        Task<JobExeHist> UpdateJobTriggerDetails(IJobExecutionContext context);
        Task<bool> WriteExecutionLogs(IJobExecutionContext context, 
            JobExeLog jobExeLog,
            LoggingLevel loggingLevel);
    }
    public enum LoggingLevel
    {
        Info,
        Warning,
        Error
    }
}
