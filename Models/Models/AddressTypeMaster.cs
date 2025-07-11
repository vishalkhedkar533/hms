using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace HMS.Models
{
    /// <summary>
    /// Master table for different address types (e.g., Residence, Office).
    /// </summary>
    [Table("ADDRESS_TYPE_MASTER", Schema = "hms")]
    public class AddressTypeMaster
    {
        [Key]
        [Column("ADDRESS_TYPE_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Unique code representing the address type (e.g., RES, OFF).")]
        public string AddressTypeCode { get; set; } = null!;

        [Required]
        [Column("ADDRESS_TYPE_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Display name of the address type (e.g., Residence, Office).")]
        public string AddressTypeName { get; set; } = null!;

        [Column("DESCRIPTION")]
        [StringLength(255)]
        [SwaggerSchema("Optional notes or clarifications about the address type.")]
        public string? Description { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates whether this address type is active.")]
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
