using Quartz;
using Tasks.Models;

namespace Tasks.Repository
{
    public interface IJobTriggerRepository
    {
        Task<JobExeHist> CreateJobTriggerDetails(IJobExecutionContext context);
        Task<JobExeHist> UpdateJobTriggerDetails(IJobExecutionContext context);
        Task<JobExeHist> UpdateJobStatus(IJobExecutionContext context, string exe_status);
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
