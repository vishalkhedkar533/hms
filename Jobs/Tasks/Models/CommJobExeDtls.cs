using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasks.Models
{
    [Table("comm_job_exe_dtls", Schema = "comss")]
    public class CommJobExeDtls
    {
        [Key]
        [Column("comm_job_exe_dtls_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommJobExeDtlsId { get; set; }

        [Required]
        [Column("job_exe_hist_id")]
        public int JobExeHistId { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Required]
        [Column("agent_id")]
        public int AgentId { get; set; }

        [Required]
        [Column("premiucollid")]
        public int PremiuCollId { get; set; }

        [Required]
        [Column("premium_amt")]
        public int PremiumAmt { get; set; }

        [MaxLength(10000)]
        [Column("formula")]
        public string? Formula { get; set; }

        [Required]
        [Column("comm_amt")]
        public int CommAmt { get; set; } = 0;

        [MaxLength(10000)]
        [Column("logs")]
        public string? Logs { get; set; }

        // Navigation Properties (Optional, for Entity Framework)
        /*
        [ForeignKey("JobExeHistId")]
        public virtual JobExeHist JobExeHist { get; set; }

        [ForeignKey("AgentId")]
        public virtual Agent Agent { get; set; }
        */
    }
}
