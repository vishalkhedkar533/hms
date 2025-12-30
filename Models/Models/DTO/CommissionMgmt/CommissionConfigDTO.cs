using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models.DTO.CommissionMgmt
{
    public class CommissionConfigDTO
    {
        public int CommissionConfigId { get; set; }
        public string? CommissionName { get; set; }

        //public string? TriggerCycle { get; set; }
        public DateOnly RunFrom { get; set; }
        public DateOnly RunTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? Conditions { get; set; }
        // Additional properties or methods can be added here as needed

        public int JobConfigId { get; set; }
        //public string JobName { get; set; } = null!;
        public string JobType { get; set; } = null!;
        public bool Enabled { get; set; } = true;
        public string TriggerType { get; set; } = null!;
        public string? CronExpression { get; set; }
        public int? IntervalSeconds { get; set; }
        //public DateTimeOffset? StartAt { get; set; }
        //public DateTimeOffset? EndAt { get; set; }
        public JsonDocument? Parameters { get; set; }
        //public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? TargetType { get; set; }
        public string? TargetMethod { get; set; }
        public string? Args { get; set; }
    }
}
