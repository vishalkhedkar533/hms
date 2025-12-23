using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("commission_config", Schema = "comss")]
    public class CommissionConfig
    {
        [Key]
        [Column("commission_config_id")]
        public int Id { get; set; }

        [Column("orgid")]
        public int? OrgId { get; set; }

        [Required]
        [Column("commission_name")]
        public string CommissionName { get; set; } = string.Empty;

        [Column("trigger_cycle")]
        public string? TriggerCycle { get; set; }

        [Required]
        [Column("run_from")]
        public DateOnly RunFrom { get; set; }

        [Required]
        [Column("run_to")]
        public DateOnly RunTo { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
