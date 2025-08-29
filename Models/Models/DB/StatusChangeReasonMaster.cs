using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /// <summary>
    /// Master table for status change reasons.
    /// </summary>
    [Table("STATUS_CHANGE_REASON_MASTER", Schema = "hms")]
    public class StatusChangeReasonMaster
    {
        [Key]
        [Column("REASON_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Code for the reason, e.g., RESIGN, DECEASED")]
        public string ReasonCode { get; set; } = null!;

        [Required]
        [Column("REASON_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Human-readable reason name")]
        public string ReasonName { get; set; } = null!;

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the reason is available for use")]
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
        [SwaggerSchema("Audit: last modification timestamp with timezone")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [SwaggerSchema("For optimistic concurrency/version control")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }
    }
}
