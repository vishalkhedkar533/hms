using Models;
using SharedModels.BackEndCalculation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasks.Models
{
    [Table("job_extns", Schema = "scheduler")]
    public class JobExtn
    {
        [Key]
        [Column("job_config_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int JobConfigId { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [StringLength(500)]
        [Column("comments")]
        public string? Comments { get; set; }

        [StringLength(10000)]
        [Column("filter")]
        public string? Filter { get; set; }

        // Navigation Properties
        [ForeignKey("JobConfigId")]
        public virtual JobConfig JobConfig { get; set; } = null!;

        [ForeignKey("OrgId")]
        public virtual Organisation Organisation { get; set; } = null!;
    }
}
