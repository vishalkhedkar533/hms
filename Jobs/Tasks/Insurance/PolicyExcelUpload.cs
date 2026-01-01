using Quartz;
using System.Diagnostics;

namespace Tasks.Insurance
{
    public class PolicyExcelUpload
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        private int orgId = 0;
        public JobKey jobKey;
        public PolicyExcelUpload(IJobExecutionContext jobExecutionContext)
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
        public void UploadPolicyData()
        {
            // Simulate uploading policies from an Excel file
            Debug.WriteLine($"Uploading policies from ...");
            // Implementation goes here
        }
    }
}
