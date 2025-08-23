using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /// <summary>
    /// Master table defining reasons for agent status changes (e.g., resignation, deceased).
    /// </summary>
    [Table("STATUS_REASON_MASTER", Schema = "hms")]
    public class StatusReasonMaster
    {
        [Key]
        [Column("REASON_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Primary key. Unique code for the reason (e.g., RESIGNATION, DECEASED).")]
        public string ReasonCode { get; set; } = null!;

        [Required]
        [Column("REASON_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Descriptive label for the reason.")]
        public string ReasonName { get; set; } = null!;

        [Column("STATUS_CODE")]
        [StringLength(20)]
        [ForeignKey("AgentStatus")]
        [SwaggerSchema("Optional foreign key linking to agent status (e.g., ACTIVE, TERMINATED).")]
        public string? StatusCode { get; set; }

        [SwaggerSchema("Navigation property to AGENT_STATUS_MASTER.")]
        public AgentStatusMaster? AgentStatus { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the reason is currently active.")]
        public bool IsActive { get; set; }

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who created the reason entry.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Timestamp when the entry was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the entry.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token for optimistic locking.")]
        public int? RowVersion { get; set; }
    }
}
