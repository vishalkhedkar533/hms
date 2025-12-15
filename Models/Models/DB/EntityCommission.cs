using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("entity_commission", Schema = "comss")]
    public class EntityCommission
    {
        [Key]
        public int entityCommId { get; set; }
        [Column("cycleid")]
        public int CycleId { get; set; }

        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("agent_id")]
        public int AgentId { get; set; }

        [Column("submittedon")]
        public DateTime? SubmittedOn { get; set; }

        [Column("submittedby")]
        public int? SubmittedBy { get; set; }

        [Column("commissionamount")]
        public double? CommissionAmount { get; set; }

        [Column("status")]
        public string? Status { get; set; }
    }
}
