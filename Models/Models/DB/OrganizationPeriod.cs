using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("organization_periods", Schema = "hmsmaster")]
    public class OrganizationPeriod
    {
        [Key]
        [Column("periodid")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PeriodId { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("range_type")]
        [StringLength(30)]
        public string RangeType { get; set; } = "MonthPeriod";

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Use Identity/Computed depending on your DB migration strategy
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(OrgId))]
        public virtual Organisation Organisation { get; set; }
    }
}
