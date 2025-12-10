using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /// <summary>
    /// Master table for states.
    /// </summary>
    [Table("STATE_MASTER", Schema = "hms")]
    public class StateMaster
    {
        [Key]
        [Column("STATE_CODE")]
        [StringLength(20)]
        [SwaggerSchema("State code, e.g., MH, KA")]
        public string StateCode { get; set; } = null!;

        [Required]
        [Column("STATE_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Full state name, e.g., Maharashtra")]
        public string StateName { get; set; } = null!;

        [Column("COUNTRY_CODE")]
        [StringLength(10)]
        [SwaggerSchema("Optional country code, FK to COUNTRY_MASTER")]
        public string? CountryCode { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Whether the state is active/in use")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("Audit: user who created the record")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE", TypeName = "timestamptz")]
        [SwaggerSchema("Audit: creation timestamp with timezone")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("Audit: user who last modified")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE", TypeName = "timestamptz")]
        [SwaggerSchema("Audit: last modification timestamp with timezone")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [SwaggerSchema("For optimistic concurrency/version control")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }
    }
}
