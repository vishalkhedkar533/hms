using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("premium_collected", Schema = "insu_core")]
    public class PremiumCollected
    {
        [Key]
        [Column("premiuCollId")]
        public int PremiuCollId { get; set; }

        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("policyref")]
        public int? PolicyRef { get; set; }

        [Column("premium_received_dt", TypeName = "date")]
        public DateTime? PremiumReceivedDate { get; set; }

        [Column("premium_type")]
        public int? PremiumType { get; set; }

        [Column("premium_amt", TypeName = "decimal")]
        public decimal? PremiumAmt { get; set; }

        // Navigation to Policy (optional)
        [ForeignKey(nameof(PolicyRef))]
        public Policy? Policy { get; set; }
    }
}