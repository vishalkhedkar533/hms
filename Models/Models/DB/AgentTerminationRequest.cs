using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("AGENT_TERMINATION_REQUEST", Schema = "hms")]
    public class AgentTerminationRequest
    {
        [Key]
        [Column("REQUEST_ID")]
        public int RequestId { get; set; }

        [Required]
        [Column("AGENT_ID")]
        public int AgentId { get; set; }

        [Required]
        [Column("REQUESTED_BY")]
        public string RequestedBy { get; set; } = null!;

        [Column("REQUESTED_DATE")]
        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;

        [Column("REASON")]
        public string? Reason { get; set; }

        [Column("STATUS")]
        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"

        [Column("APPROVED_BY")]
        public string? ApprovedBy { get; set; }

        [Column("APPROVED_DATE")]
        public DateTime? ApprovedDate { get; set; }

        [Column("REJECTED_BY")]
        public string? RejectedBy { get; set; }

        [Column("REJECTED_DATE")]
        public DateTime? RejectedDate { get; set; }

        public Agent? Agent { get; set; }
    }

}
