using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class CommissionDashboard
    {
        public Int64? TotalCommissionRequests { get; set; } = 10;
        public Int64? ApprovedRequest { get; set; } = 20;
        public Int64? RejectedRequest { get; set; } = 30;
        public Int64? PendingRequest { get; set; } = 40;
        public Int64? TotalCommissionProcessed { get; set; } = 50;
        public Int64? CommissionOnHold        { get; set; } = 60;
        public Int64? AvgCommission { get; set; } = 70;
        public List<IndividualCommissionChanges>? IndividualCommissionChanges { get; set; } = new List<IndividualCommissionChanges>();
        public List<BulkCommissionUpdates>? BulkCommissionUpdates { get; set; } = new List<BulkCommissionUpdates>();
        public List<RejectedCommissionRequests>? RejectedCommissionRequests { get; set; } = new List<RejectedCommissionRequests>();
        public List<PerformanceSnapshot>? PerformanceSnapshot { get; set; } = new List<PerformanceSnapshot>();
        public List<CurrentBusinessCycle>? CurrentBusinessCycle { get; set; } = new List<CurrentBusinessCycle>();
        public List<OnHoldPayouts>? OnHoldPayouts { get; set; } = new List<OnHoldPayouts>();
        public List<Channel>? Channel { get; set; } = new List<Channel>();
        public List<DownloadCycle>? DownloadCycle { get; set; } = new List<DownloadCycle>();
        public List<ForecastDetails>? ForecastDetails { get; set; } = new List<ForecastDetails>();
    }

    public class IndividualCommissionChanges
    {
        public string? AgentCode { get; set; } 
        public string? AgentName { get; set; } 
        public string? Branch { get; set; } 
        public string? Status { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public string? ChangeType { get; set; } 
    }

    public class BulkCommissionUpdates
    {
        public string? RequestId { get; set; }
        public DateTime? RequestDate { get; set; }
        public string? Branch { get; set; }
        public string? Status { get; set; }
        public Int64? TotalRecords { get; set; }
    }

    public class RejectedCommissionRequests
    {
        public string? RequestId { get; set; }
        public DateTime? RejectedOn { get; set; }
        public string? SubmittedBy { get; set; }
        public string? Status { get; set; }
        public string? RejectionReason { get; set; }
    }
    public class PerformanceSnapshot
    {
        public string? Particulars { get; set; }
        public decimal? Budget { get; set; }
        public decimal? Actuals { get; set; }
        public decimal? Percentage { get; set; }
    }
    public class CurrentBusinessCycle
    {
        public string? Cycle { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? Commission { get; set; }
        public decimal? Percentage { get; set; }
    }
    public class OnHoldPayouts
    {
        public string? Reasons { get; set; }
        public decimal? lessthan30D { get; set; }
        public decimal? Greaterthan30D { get; set; }
        public decimal? Greaterthan90D { get; set; }
    }
    public class Channel
    {
        public string? Reasons { get; set; }
        public decimal? LessthanhalfYear { get; set; }
        public decimal? Lessthan2Years { get; set; }
        public decimal? Greaterthan2Years { get; set; }
    }
    public class ForecastDetails
    {
        public string? Month { get; set; }
        public decimal? Payout { get; set; }
    }
    public class DownloadCycle
    {
        public string? FileType { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
    }
}
