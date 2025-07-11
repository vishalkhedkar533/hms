using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /// <summary>
    /// Master table for different email types (e.g., Personal, Official).
    /// </summary>
    [Table("EMAIL_TYPE_MASTER", Schema = "hms")]
    public class EmailTypeMaster
    {
        [Key]
        [Column("EMAIL_TYPE_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Unique code representing the email type (e.g., PERSONAL, OFFICIAL).")]
        public string EmailTypeCode { get; set; } = null!;

        [Required]
        [Column("EMAIL_TYPE_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Display name of the email type.")]
        public string EmailTypeName { get; set; } = null!;

        [Column("DESCRIPTION")]
        [StringLength(255)]
        [SwaggerSchema("Optional clarification or usage notes.")]
        public string? Description { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates whether this email type is active.")]
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
        [SwaggerSchema("Used for optimistic concurrency control.")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }
    }
}
