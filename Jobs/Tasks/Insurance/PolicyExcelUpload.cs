using Quartz;
using System.Diagnostics;

namespace Tasks.Insurance
{
    public class PolicyExcelUpload
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        public PolicyExcelUpload(IJobExecutionContext jobExecutionContext)
        {
            _jobExecutionContext = jobExecutionContext;
        }
        public void UploadPolicyData()
        {
            // Simulate uploading policies from an Excel file
            Debug.WriteLine($"Uploading policies from ...");
            // Implementation goes here
        }
    }
}
