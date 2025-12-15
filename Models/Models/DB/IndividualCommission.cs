using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("individual_commission", Schema = "comss")]
    public class IndividualCommission
    {
        [Key]
        [Column("individualcommissionid")]
        public int IndividualCommissionId { get; set; }

        [Column("orgid")]
        public int? OrgId { get; set; }

        [Column("agent_id")]
        public int? AgentId { get; set; }

        [Column("agent_name")]
        public string? AgentName { get; set; }

        [Column("agent_code")]
        public string? AgentCode { get; set; }

        [Column("submittedon")]
        public DateTime? SubmittedOn { get; set; }

        [Column("submittedby")]
        public int? SubmittedBy { get; set; }

        [Column("status")]
        public string? Status { get; set; }
    }
}
