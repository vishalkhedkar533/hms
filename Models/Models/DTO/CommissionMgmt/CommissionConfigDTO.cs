using System.Text.Json;

namespace Models.DTO.CommissionMgmt
{
    public class CreateCommissionDto
    {
        public int CommissionConfigId { get; set; }
        public string CommissionName { get; set; } = null!;
        public DateTime? RunFrom { get; set; }
        public DateTime? RunTo { get; set; }
        public string? FilterConditions { get; set; }
        public string? Comments { get; set; }
        public string? TargetType { get; set; }
        public string? TargetMethod { get; set; }
    }
    public class CommissionConditionUpdateDto
    {
        public int CommissionConfigId { get; set; }
        public string Formula { get; set; } = string.Empty;
    }
    public class UpdateCronDto
    {
        public int CommissionConfigId { get; set; }
        public string JobType { get; set; } = null!;
        public string TriggerType { get; set; } = null!;
        public string CronExpression { get; set; } = null!;
    }

    public class EnableDisableJobDto
    {
        public int CommissionConfigId { get; set; }
        public bool Enabled { get; set; }    
    }

    public class CommissionConfigDTO
    {
        public int CommissionConfigId { get; set; }
        public string? CommissionName { get; set; }

        //public string? TriggerCycle { get; set; }
        public DateTime? RunFrom { get; set; }
        public DateTime? RunTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? Formula { get; set; }
        // Additional properties or methods can be added here as needed

        public int JobConfigId { get; set; }

        public string JobName { get; set; } = null!;
        public string JobType { get; set; } = null!;
        public bool Enabled { get; set; } = true;
        public string TriggerType { get; set; } = null!;
        public string? CronExpression { get; set; }
        public int? IntervalSeconds { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public JsonDocument? Parameters { get; set; }
        //public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? TargetType { get; set; }
        public string? TargetMethod { get; set; }
        public string? Args { get; set; }
        public string? FilterCondition { get; set; }
        public string? Comments { get; set; }
    }
    public class JobExecutionHistoryDto
    {
        public int JobExeHistId { get; set; }
        public int JobConfigId { get; set; }

        public string? JobName { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }

        public string? ExeStatus { get; set; }
        public string? DownloadLink { get; set; }
        public string? Comments { get; set; }
        public TimeSpan? Duration =>
            FinishedAt.HasValue ? FinishedAt.Value - StartedAt : null;
    }
}
