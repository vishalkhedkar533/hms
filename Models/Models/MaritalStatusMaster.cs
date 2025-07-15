
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace HMS.Models
{
    /// <summary>
    /// Master table for marital status values (e.g., SINGLE, MARRIED).
    /// </summary>
    [Table("MARITAL_STATUS_MASTER", Schema = "hms")]
    public class MaritalStatusMaster
    {
        [Key]
        [Column("MARITAL_STATUS_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Primary key. Unique code for marital status (e.g., SINGLE, MARRIED).")]
        public string MaritalStatusCode { get; set; } = null!;

        [Required]
        [Column("MARITAL_STATUS_NAME")]
        [StringLength(50)]
        [SwaggerSchema("Descriptive name for the marital status.")]
        public string MaritalStatusName { get; set; } = null!;

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates whether this marital status is currently active.")]
        public bool IsActive { get; set; }

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Timestamp when the record was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp when the record was last modified.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Row version used for optimistic concurrency control.")]
        public int? RowVersion { get; set; }
    }
}
