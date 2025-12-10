
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Models.DB
{
    /// <summary>
    /// Master table defining reasons for agent movement (e.g., promotion, transfer).
    /// </summary>
    [Table("MOVEMENT_REASON_MASTER", Schema = "hms")]
    public class MovementReasonMaster
    {
        [Key]
        [Column("REASON_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Primary key. Unique code for the movement reason (e.g., PROMOTION, TRANSFER).")]
        public string ReasonCode { get; set; } = null!;

        [Required]
        [Column("REASON_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Descriptive name for the movement reason.")]
        public string ReasonName { get; set; } = null!;

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates whether the movement reason is currently active.")]
        public bool IsActive { get; set; }

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who created the movement reason entry.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Timestamp when the movement reason was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the movement reason entry.")]
        public string? Modified { get; set; }
    }
}
