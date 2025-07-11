using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /// <summary>
    /// Master table for source channels like REFERRAL, WALKIN.
    /// </summary>
    [Table("SOURCE_CHANNEL_MASTER", Schema = "hms")]
    public class SourceChannelMaster
    {
        [Key]
        [Column("SOURCE_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Source channel code, e.g., REFERRAL, WALKIN")]
        public string SourceCode { get; set; } = null!;

        [Required]
        [Column("SOURCE_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Descriptive name for the source channel")]
        public string SourceName { get; set; } = null!;

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates whether this source channel is active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("Audit: created by")]
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
        [SwaggerSchema("Audit/version control field")]
        public int? RowVersion { get; set; }
    }
}
