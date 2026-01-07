namespace Tasks.Models.DB
{
    public class Ins_Policy
    {
        public int PolicyRef { get; set; }
        public int OrgId { get; set; }
        public string? PolicyNo { get; set; }
        public string? PolicySuffix { get; set; }
        public DateTime? RiskStartDt { get; set; }
        public DateTime? RiskEndDt { get; set; }
        public int? PolicyTerm { get; set; }
        public int? PremPayingTerm { get; set; }
        public string? ProposerClientId { get; set; }
        public string? LifeInsuredClientId { get; set; }
        public int? AgentId { get; set; }
        public bool? IsStaffPolicy { get; set; }
        public int? PolicySourceCode { get; set; }
        public string? InsuredPan { get; set; }
        public string? ProposerPan { get; set; }
        public DateTime? InsuredDob { get; set; }
        public DateTime? ProposerDob { get; set; }
        public DateTime? LoginDt { get; set; }
        public int? InsuredGender { get; set; }
        public int? ProposerGender { get; set; }
        public int? MaturityAgeInMonths { get; set; }
        public decimal? ModalBasePremium { get; set; }
        public decimal? ModalBaseRiderPremium { get; set; }
        public string? ProdCode { get; set; }
    }
}