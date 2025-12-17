namespace Models.DTO
{
    public class CommissionMgmtDashboardDto
    {
        public int? OrgId { get; set; }
        public double? CommissionBudget { get; set; }
        public double? CommissionPaid { get; set; }
        public double? CommissionOnHold { get; set; }
        public double? CommissionNotPaid { get; set; }
        public double? LastCycleCommission { get; set; }
        public double? LastCycleEntities { get; set; }
        public double? ThisCycleCommission { get; set; }
        public double? ThisCycleEntities { get; set; }
        public double? ThisCycleAvgCommission { get; set; }
        public List<IndividualCommissionDto>? IndividualCommissions { get; set; }
        public List<CycleCommissionDto>? CycleCommissions { get; set; }
        public List<AdhocCommissionDto>? AdhocCommissions { get; set; }
        public List<PerformanceSnapshotDto>? PerformanceSnapshot { get; set; }
        public List<CurrentBusinessCycleDto>? CurrentBusinessCycles { get; set; }
        public List<OnHoldPayoutDto>? OnHoldPayouts { get; set; }
        public List<ChannelDto>? Channels { get; set; }
    }

    public class IndividualCommissionDto
    {
        public int? CommissionId { get; set; }
        public int? OrgId { get; set; }
        public int? AgentId { get; set; }
        public string? AgentCode { get; set; }
        public string? AgentName { get; set; }
        public string? Status { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public string? SubmittedBy { get; set; }
    }

    public class CycleCommissionDto
    {
        public int CycleId { get; set; }
        public string? CycleCode { get; set; }
        public int OrgId { get; set; }
        public string? CommissionType { get; set; }
        public double? CountOfEntities { get; set; }
        public double? AvgCommission { get; set; }
        public double? NbRevenue { get; set; }
        public double? NbCommission { get; set; }
        public string? Status { get; set; }
    }

    public class AdhocCommissionDto
    {
        public int AdhocCommissionId { get; set; }
        public int OrgId { get; set; }
        public int BranchId { get; set; }
        public int? RequestId { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public int? SubmittedBy { get; set; }
        public DateTime CommissionDate { get; set; }
        public double? CommissionAmount { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
    }

    public class PerformanceSnapshotDto
    {
        public int OrgId { get; set; }
        public int SnapshotId { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public double? CommissionBudget { get; set; }
        public double? CommissionActual { get; set; }
    }

    public class CurrentBusinessCycleDto
    {
        public int OrgId { get; set; }
        public int CycleId { get; set; }
        public string? CycleCode { get; set; }
        public DateTime? CycleStart { get; set; }
        public DateTime? CycleEnd { get; set; }
    }

    public class OnHoldPayoutDto
    {
        public int PayoutId { get; set; }
        public int OrgId { get; set; }
        public int AgentId { get; set; }
        public double? Amount { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
    }

    public class ChannelDto
    {
        public int ChannelId { get; set; }
        public int OrgId { get; set; }
        public string? ChannelName { get; set; }
        public string? SubChannelName { get; set; }
    }

    public class ProcessCommissionLogDto
    {
        public int ProcessId { get; set; }
        public DateTime ProcessedDate { get; set; }
        public string? Period { get; set; }         
        public int RecordsCount { get; set; }
        public string? Status { get; set; } 
        public bool CanDownload { get; set; } 
        public bool CanViewDetails { get; set; } 
    }

    public class ProcessCommissionResponseDto
    {
        public int OrgId { get; set; }
        public string? PeriodType { get; set; }      
        public List<ProcessCommissionLogDto>? ProcessedRecordsLog { get; set; }
    }
}
