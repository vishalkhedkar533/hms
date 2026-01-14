using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasks.Models
{
    [Table("job_exe_hist", Schema = "scheduler")]
    public class JobExeHist
    {
        [Key]
        [Column("job_exe_hist_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long JobExeHistId { get; set; }

        [Required]
        [Column("job_config_id")]
        public int JobConfigId { get; set; }

        [Required]
        [Column("started_at")]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        [Column("finished_at")]
        public DateTime? FinishedAt { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("exe_status")]
        public string ExeStatus { get; set; }

        [MaxLength(1000)]
        [Column("download_lnk")]
        public string DownloadLnk { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("fireinstanceid")]
        public Int64? FireInstanceId { get; set; }

        [MaxLength(500)]
        [Column("triggerobject")]
        public string TriggerObject { get; set; }

        [MaxLength(500)]
        [Column("jobdetail")]
        public string JobDetail { get; set; }

        [Column("firetimeutc")]
        public DateTime? FireTimeUtc { get; set; }
    }
}
