namespace SharedModels.DTO.CommissionMgmt.Dashboard
{
    public class ProcessCommissionDTO
    {
        public int JobExeHistId { get; set; }
        public int JobConfigId { get; set; }
        public string CommissionName { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int? Records { get; set; }
        public string ExeStatus { get; set; } = null!;
        public string? DownloadLnk { get; set; }
        public int OrgId { get; set; }
        public int TotalCount { get; set; }
    }
    public class ProcessCommissionExcelDTO
    {
        // Agent / Commission
        public int AgentId { get; set; }
        public string AgentName { get; set; } = null!;
        public string Channel { get; set; } = null!;
        public int PremiumCollectionId { get; set; }
        public decimal PremiumAmount { get; set; }
        public string? Formula { get; set; }
        public decimal CommissionAmount { get; set; }
        public string Status { get; set; } = null!;
        public string? Logs { get; set; }

        // Premium Collected
        public int PcPremiumCollectionId { get; set; }
        public int PolicyRef { get; set; }
        public DateTime? PremiumReceivedDate { get; set; }
        public int? PremiumType { get; set; }
        public int PremiumCollectionYear { get; set; }
        public int PremiumCollectionQuarter { get; set; }
        public string PremiumCollectionFinYear { get; set; } = null!;

        // Policy
        public string? PolicyNo { get; set; }
        public string? PolicySuffix { get; set; }
        public DateTime? RiskStartDate { get; set; }
        public DateTime? RiskEndDate { get; set; }
        public int? PolicyTerm { get; set; }
        public int? PremiumPayingTerm { get; set; }
        public string? ProposerClientId { get; set; }
        public string? LifeInsuredClientId { get; set; }
        public bool? IsStaffPolicy { get; set; }
        public int? PolicySourceCode { get; set; }
        public string? InsuredPAN { get; set; }
        public string? ProposerPAN { get; set; }
        public DateTime? InsuredDOB { get; set; }
        public DateTime? ProposerDOB { get; set; }
        public DateTime? LoginDate { get; set; }
        public int? InsuredGender { get; set; }
        public int? ProposerGender { get; set; }
        public int? MaturityAgeInMonths { get; set; }
        public decimal? ModalBasePremium { get; set; }
        public decimal? ModalBaseRiderPremium { get; set; }
        public string? ProductCode { get; set; }
    }


}
