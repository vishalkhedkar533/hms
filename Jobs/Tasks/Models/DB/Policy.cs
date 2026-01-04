namespace Tasks.Models.DB
{
    public class Policy
    {
        public int PolicyRef { get; set; }
        public int OrgId { get; set; }
        public string? PolicyNo { get; set; }
        public string? PolicySuffix { get; set; }
        public DateOnly? RiskStartDt { get; set; }
        public DateOnly? RiskEndDt { get; set; }
        public int? PolicyTerm { get; set; }
        public int? PremPayingTerm { get; set; }
        public string? ProposerClientId { get; set; }
        public string? LifeInsuredClientId { get; set; }
        public int? AgentId { get; set; }
        public bool? IsStaffPolicy { get; set; }
        public int? PolicySourceCode { get; set; }
        public string? InsuredPan { get; set; }
        public string? ProposerPan { get; set; }
        public DateOnly? InsuredDob { get; set; }
        public DateOnly? ProposerDob { get; set; }
        public DateOnly? LoginDt { get; set; }
        public int? InsuredGender { get; set; }
        public int? ProposerGender { get; set; }
        public int? MaturityAgeInMonths { get; set; }
        public decimal? ModalBasePremium { get; set; }
        public decimal? ModalBaseRiderPremium { get; set; }
    }
}