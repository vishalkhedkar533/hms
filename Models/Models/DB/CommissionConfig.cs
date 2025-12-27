using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Models.DB
{
    [Table("commission_config", Schema = "comss")]
    public class CommissionConfig
    {
        [Key]
        [Column("commission_config_id")]
        public int CommissionConfigId { get; set; }

        [JsonIgnore]
        [Column("orgid")]
        public int? OrgId { get; set; }

        [Required]
        [Column("commission_name")]
        public string? CommissionName { get; set; } 

        [Column("trigger_cycle")]
        public string? TriggerCycle { get; set; }

        [Required]
        [Column("run_from")]
        public DateOnly RunFrom { get; set; }

        [Required]
        [Column("run_to")]
        public DateOnly RunTo { get; set; }

        [JsonIgnore]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [JsonIgnore]
        [Column("conditions")]
        public string? Conditions { get; set; }

    }
    public class CommissionConditionUpdateDto
    {
        public int CommissionConfigId { get; set; }
        public string Condition { get; set; } = string.Empty;
    }
}
