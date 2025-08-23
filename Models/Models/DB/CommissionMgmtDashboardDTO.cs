namespace Models.DB
{
    public class CommissionMgmtDashboardDTO
    {
    }
    public class CommissionMgmtDashboard
    {
        public IndividualCommCharges individualCommCharges { get; set; } = new IndividualCommCharges();
        public BulkCommUpdates bulkCommUpdates { get; set; } = new BulkCommUpdates();
        public RejectedCommRequest rejectedCommRequest { get; set; } = new RejectedCommRequest();
        public List<CurrentBusinessCycle> currentBusinessCycles { get; set; } = new List<CurrentBusinessCycle>();
        public List<Channel> channels { get; set; } = new List<Channel>();
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
        public long TotalRecords { get; set; } = 1000;
    }
    public class RejectedCommRequest
    {
       public string RequestID { get; set; } = string.Empty;
       public string SubmittedBy { get; set; } = string.Empty;
       public string RejectionReason { get; set; } = string.Empty;
       public DateTime RejectionOn { get; set; } = DateTime.MinValue;
    }
    //public class CurrentBusinessCycle 
    //{
    //    public string Cycle {  get; set; } = string.Empty;
    //    public Int64 Revenue { get; set; } = 100000;
    //    public Int64 Commssion { get; set; } = 10000;
    //    public Int32 Percentage { get; set; } = 15;
    //}
    //public class Channel
    //{
    //    public string Reason { get; set; } = string.Empty;
    //    public Int64 Period_00_06_Months { get; set; } = 100000;
    //    public Int64 Period_07_12_Months { get; set; } = 200000;
    //    public Int64 Period_13_Onwards_Months { get; set; } = 300000;
    //}
}
