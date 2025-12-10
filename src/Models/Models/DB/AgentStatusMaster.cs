using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /// <summary>
    /// Master table defining the possible statuses for agents (e.g., ACTIVE, TERMINATED).
    /// </summary>
    [Table("AGENT_STATUS_MASTER", Schema = "hms")]
    public class AgentStatusMaster
    {
        [Key]
        [Column("STATUS_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Primary key. Code representing the agent status (e.g., ACTIVE, TERMINATED).")]
        public string StatusCode { get; set; } = null!;

        [Required]
        [Column("STATUS_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Human-readable name of the status (e.g., Active, Terminated).")]
        public string StatusName { get; set; } = null!;

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates whether the status is currently in use.")]
        public bool IsActive { get; set; }

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who created the status entry.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Timestamp when the status entry was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the status entry.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Row version used for concurrency control.")]
        public int? RowVersion { get; set; }
    }
}
