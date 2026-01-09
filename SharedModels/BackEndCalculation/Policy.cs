using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.BackEndCalculation
{
    public class Ins_Policy
    {
        [Column("policyref")]
        [Description("policyref")]
        public int PolicyRef { get; set; }
        [Column("orgid")]
        [Description("orgid")]
        public int OrgId { get; set; }
        [Column("policyno")]
        [Description("policyno")]
        public string? PolicyNo { get; set; }
        [Column("policysuffix")]
        [Description("policysuffix")]
        public string? PolicySuffix { get; set; }
        [Column("riskstartdt")]
        [Description("riskstartdt")]
        public DateTime? RiskStartDt { get; set; }
        [Column("riskenddt")]
        [Description("riskenddt")]
        public DateTime? RiskEndDt { get; set; }
        [Column("policyterm")]
        [Description("policyterm")]
        public int? PolicyTerm { get; set; }
        [Column("prempayingterm")]
        [Description("prempayingterm")]
        public int? PremPayingTerm { get; set; }
        [Column("proposerclientid")]
        [Description("proposerclientid")]
        public string? ProposerClientId { get; set; }
        [Column("lifeinsuredclientid")]
        [Description("lifeinsuredclientid")]
        public string? LifeInsuredClientId { get; set; }
        [Column("agent_id")]
        [Description("agentid")]
        public int? AgentId { get; set; }
        [Column("isstaffpolicy")]
        [Description("isstaffpolicy")]
        public bool? IsStaffPolicy { get; set; }
        [Column("policysourcecode")]
        [Description("policysourcecode")]
        public int? PolicySourceCode { get; set; }
        [Column("insuredpan")]
        [Description("insuredpan")]
        public string? InsuredPan { get; set; }
        [Column("proposerpan")]
        [Description("proposerpan")]
        public string? ProposerPan { get; set; }
        [Column("insureddob")]
        [Description("insureddob")]
        public DateTime? InsuredDob { get; set; }
        [Column("proposerdob")]
        [Description("proposerdob")]
        public DateTime? ProposerDob { get; set; }
        [Column("logindt")]
        [Description("logindt")]
        public DateTime? LoginDt { get; set; }
        [Column("insuredgender")]
        [Description("insuredgender")]
        public int? InsuredGender { get; set; }
        [Column("proposergender")]
        [Description("proposergender")]
        public int? ProposerGender { get; set; }
        [Column("maturityageinmonths")]
        [Description("maturityageinmonths")]
        public int? MaturityAgeInMonths { get; set; }
        [Column("modalbasepremium")]
        [Description("modalbasepremium")]
        public decimal? ModalBasePremium { get; set; }
        [Column("modalbaseriderpremium")]
        [Description("modalbaseriderpremium")]
        public decimal? ModalBaseRiderPremium { get; set; }
        [Column("prod_code")]
        [Description("prodcode")]
        public string? ProdCode { get; set; }
    }
}