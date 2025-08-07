using HMS.Models;

namespace Models
{
    public class CommissionMgmtDashboardDTO
    {
    }
    public class CommissionMgmtDashboard
    {
        public IndividualCommCharges individualCommCharges = new IndividualCommCharges();
        public BulkCommUpdates bulkCommUpdates = new BulkCommUpdates();
        public RejectedCommRequest rejectedCommRequest = new RejectedCommRequest();
        public List<CurrentBusinessCycle> currentBusinessCycles = new List<CurrentBusinessCycle>();
        public List<Channel> channels = new List<Channel>();
    }
    public class IndividualCommCharges 
    {
        public Agent Agent { get; set; } = new Agent();
        public BranchMaster BranchMaster { get; set; } = new BranchMaster();
        public DateTime SubmittedOn { get; set; } = new DateTime();
        public string ChangeType { get; set; } = string.Empty;
        public string Status {  get; set; } = string.Empty;

    }
    public class BulkCommUpdates
    {
        public BranchMaster BranchMaster { get; set; } = new BranchMaster();
        public DateTime RequestDate { get; set; } = new DateTime();
        public Int64 TotalRecords { get; set; } = 1000;
    }
    public class RejectedCommRequest
    {
       public string RequestID { get; set; } = string.Empty;
       public string SubmittedBy { get; set; } = string.Empty;
       public string RejectionReason { get; set; } = string.Empty;
       public DateTime RejectionOn { get; set; } = DateTime.MinValue;
    }
    public class CurrentBusinessCycle 
    {
        public string Cycle {  get; set; } = string.Empty;
        public Int64 Revenue { get; set; } = 100000;
        public Int64 Commssion { get; set; } = 10000;
        public Int32 Percentage { get; set; } = 15;
    }
    public class Channel
    {
        public string Reason { get; set; } = string.Empty;
        public Int64 Period_00_06_Months { get; set; } = 100000;
        public Int64 Period_07_12_Months { get; set; } = 200000;
        public Int64 Period_13_Onwards_Months { get; set; } = 300000;
    }
}
