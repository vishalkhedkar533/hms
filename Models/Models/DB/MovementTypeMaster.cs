
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Models.DB
{
    /// <summary>
    /// Master table for movement types like TRANSFER, PROMOTION.
    /// </summary>
    [Table("MOVEMENT_TYPE_MASTER", Schema = "hms")]
    public class MovementTypeMaster
    {
        [Key]
        [Column("TYPE_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Movement type code, e.g., TRANSFER, PROMOTION")]
        public string TypeCode { get; set; } = null!;

        [Required]
        [Column("TYPE_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Movement type description")]
        public string TypeName { get; set; } = null!;

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates whether this type is active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("Audit: record creator")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE", TypeName = "timestamptz")]
        [SwaggerSchema("Audit: creation timestamp with timezone")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("Audit: last modified by")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE", TypeName = "timestamptz")]
        [SwaggerSchema("Audit: last modified timestamp with timezone")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [SwaggerSchema("For optimistic concurrency/version control")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }
    }
}
