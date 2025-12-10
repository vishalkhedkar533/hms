using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /// <summary>
    /// Master table for different phone types (e.g., Mobile, Landline).
    /// </summary>
    [Table("PHONE_TYPE_MASTER", Schema = "hms")]
    public class PhoneTypeMaster
    {
        [Key]
        [Column("PHONE_TYPE_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Unique code representing the phone type (e.g., MOBILE, LANDLINE).")]
        public string PhoneTypeCode { get; set; } = null!;

        [Required]
        [Column("PHONE_TYPE_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Display name of the phone type.")]
        public string PhoneTypeName { get; set; } = null!;

        [Column("DESCRIPTION")]
        [StringLength(255)]
        [SwaggerSchema("Optional notes for internal use.")]
        public string? Description { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the phone type is active.")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE", TypeName = "timestamptz")]
        [SwaggerSchema("Timestamp with timezone when the record was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE", TypeName = "timestamptz")]
        [SwaggerSchema("Timestamp with timezone when the record was last modified.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [SwaggerSchema("Optimistic concurrency control field.")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }
    }
}
