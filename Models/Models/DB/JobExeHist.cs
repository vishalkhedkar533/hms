using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("job_exe_hist", Schema = "scheduler")]
    public class JobExeHist
    {
        [Key]
        [Column("job_exe_hist_id")]
        public int JobExeHistId { get; set; }

        [Column("job_config_id")]
        public int JobConfigId { get; set; }

        [Column("started_at")]
        public DateTime StartedAt { get; set; }

        [Column("finished_at")]
        [Description("Job - last executed on")]
        public DateTime? FinishedAt { get; set; }

        [MaxLength(10)]
        [Column("exe_status")]
        public string ExeStatus { get; set; } = null!;

        [MaxLength(1000)]
        [Column("download_lnk")]
        public string? DownloadLnk { get; set; }

        [Column("orgid")]
        public int OrgId { get; set; }

        [ForeignKey(nameof(JobConfigId))]
        public JobConfig? JobConfig { get; set; }
    }
}