using Models.DTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("commssiondashboard", Schema = "hms")]
    public class CommissionMgmtDash
    {
        [Key]
        public int? orgId { get; set; }
        public double? CommissionBudget { get; set; }
        public double? CommissionPaid { get; set; }
        public double? CommissionOnHold { get; set; }
        public double? CommissionNotPaid { get; set; }
        public double? LastCycleCommission { get; set; }
        public double? LastCycleEntities { get; set; }
        public double? ThisCycleCommission { get; set; }
        public double? ThisCycleEntities { get; set; }
        public double? ThisCycleAvgCommission { get; set; }
        public List<IndividualCommission>? individualCommissions { get; set; }
        [NotMapped] 
        public List<CycleCommissionDto>? cycleCommissions { get; set; }
        [NotMapped] 
        public List<AdhocCommissionDto>? adhocCommissions { get; set; }
        [NotMapped] 
        public List<PerformanceSnapshotDto>? performanceSnapshot { get; set; }
        [NotMapped] 
        public List<CurrentBusinessCycleDto>? currentBusinessCycles { get; set; }
        [NotMapped] 
        public List<OnHoldPayoutDto>? onHoldPayouts { get; set; }
        [NotMapped] 
        public List<ChannelDto>? channels { get; set; }
    }

    public class CommissionMgmtDashboardDTO
    {
        public int? orgId { get; set; }
    }
    //public class IndividualCommissionDto
    //{
    //    [Key]
    //    public int? orgId { get; set; }
    //    public string? AgentName { get; set; }
    //    public string? AgentCode { get; set; }
    //    public DateTime SubmittedOn { get; set; }
    //    public string? SubmittedBy { get; set; }
    //    public string? Status { get; set; } 
    //}
    public class CycleCommissionDto
    {
        public DateTime RequestDate { get; set; }
        public string? CycleCode { get; set; }
        public int TotalRecords { get; set; }
        public string? SubmittedBy { get; set; }
        public string? Status { get; set; }
    }

    public class AdhocCommissionDto
    {
        public int? orgId { get; set; }
        public string? RequestId { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string? SubmittedBy { get; set; }
        public string? Status { get; set; }
    }

    public class PerformanceSnapshotDto
    {
        public string? Particular { get; set; }
        public double Budget { get; set; }
        public double Actual { get; set; }
        public double Percentage { get; set; }
    }

    public class CurrentBusinessCycleDto
    {
        public string? Cycle { get; set; }
        public double Revenue { get; set; }
        public double Commission { get; set; }
        public double Percentage { get; set; }
    }

    public class OnHoldPayoutDto
    {
        public string? Reason { get; set; }
        public double Less30 { get; set; }
        public double More30 { get; set; }
        public double More90 { get; set; }
    }

    public class ChannelDto
    {
        public string? Reason { get; set; }
        public double Less0_6Months { get; set; }
        public double More7_12Months { get; set; }
        public double More12PlusMonths { get; set; }
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
}
