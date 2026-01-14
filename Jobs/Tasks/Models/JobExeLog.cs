using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasks.Models
{
    [Table("job_exe_logs", Schema = "scheduler")]
    public class JobExeLog
    {
        [Key]
        [Column("job_exe_log_id")]
        public int JobExeLogId { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Required]
        [Column("job_exe_hist_id")]
        public long JobExeHistId { get; set; }

        [Column("created_on")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [Column("exe_logs")]
        [StringLength(10000)]
        public string? ExeLogs { get; set; }
        [Column("LogLevel")]
        [StringLength(10)]
        public string? LogLevel { get; set; }
    }
}
