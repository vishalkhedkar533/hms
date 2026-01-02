using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Models.DB
{
    [Table("job_config", Schema = "scheduler")]
    [Index(nameof(JobName), nameof(OrgId), IsUnique = true)]
    public class JobConfig
    {
        [Key]
        [Column("job_config_id")]
        public int JobConfigId { get; set; }

        [Column("job_name")]
        public string JobName { get; set; } = null!;

        [Column("job_type")]
        public string JobType { get; set; } = null!;

        [Column("enabled")]
        public bool Enabled { get; set; } = true;

        [Column("trigger_type")]
        public string TriggerType { get; set; } = null!;

        [Column("cron_expression")]
        public string? CronExpression { get; set; }

        [Column("interval_seconds")]
        public int? IntervalSeconds { get; set; }

        [Column("start_at")]
        public DateOnly? StartAt { get; set; }

        [Column("end_at")]
        public DateOnly? EndAt { get; set; }

        [Column("parameters", TypeName = "jsonb")]
        public JsonDocument? Parameters { get; set; }

        [Column("created_at", TypeName = "timestamp")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at", TypeName = "timestamp")]
        public DateTime? UpdatedAt { get; set; }

        [Column("targettype")]
        [MaxLength(255)]
        public string? TargetType { get; set; }

        [Column("targetmethod")]
        [MaxLength(100)]
        public string? TargetMethod { get; set; }

        [Column("args")]
        [MaxLength(500)]
        public string? Args { get; set; }

        [Column("orgid")]
        public int? OrgId { get; set; }
    }
}
