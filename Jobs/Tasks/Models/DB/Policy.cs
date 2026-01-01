using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("policy", Schema = "insu_core")]
    public class Policy
    {
        [Key]
        [Column("policyref")]
        public int PolicyRef { get; set; }

        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("policyno")]
        [StringLength(20)]
        public string? PolicyNo { get; set; }

        [Column("policysuffix")]
        [StringLength(10)]
        public string? PolicySuffix { get; set; }

        [Column("riskstartdt", TypeName = "date")]
        public DateTime? RiskStartDate { get; set; }

        [Column("riskenddt", TypeName = "date")]
        public DateTime? RiskEndDate { get; set; }

        [Column("policyterm")]
        public int? PolicyTerm { get; set; }

        [Column("prempayingterm")]
        public int? PremPayingTerm { get; set; }

        [Column("proposerclientid")]
        [StringLength(10)]
        public string? ProposerClientId { get; set; }

        [Column("lifeinsuredclientid")]
        [StringLength(10)]
        public string? LifeInsuredClientId { get; set; }

        [Column("agent_id")]
        public int? AgentId { get; set; }

        // Navigation property (optional; ensure hms.Agent model exists in project)
        //[ForeignKey(nameof(AgentId))]
        //public Models.DB.Agent? Agent { get; set; }
    }
}