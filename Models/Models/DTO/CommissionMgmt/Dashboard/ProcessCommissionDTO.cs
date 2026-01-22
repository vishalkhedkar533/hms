namespace Models.DTO.CommissionMgmt.Dashboard
{
    public class ProcessCommissionDTO
    {
        public int JobExeHistId { get; set; }
        public int JobConfigId { get; set; }
        public string CommissionName { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string ExeStatus { get; set; } = null!;
        public string? DownloadLnk { get; set; }
        public int OrgId { get; set; }
        public int TotalCount { get; set; }
    }
    public class ProcessCommissionExcelDTO
    {
        public int AgentId { get; set; }
        public int PremiumCollectionId { get; set; }
        public int PremiumAmount { get; set; }
        public string? Formula { get; set; }
        public int CommissionAmount { get; set; }
        public string Status { get; set; } = null!;
        public string? Logs { get; set; }
    }

}
