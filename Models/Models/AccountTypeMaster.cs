
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace HMS.Models
{
    /// <summary>
    /// Master table for account types such as SAVINGS, CURRENT.
    /// </summary>
    [Table("ACCOUNT_TYPE_MASTER", Schema = "hms")]
    public class AccountTypeMaster
    {
        [Key]
        [Column("ACCOUNT_TYPE_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Account type code, e.g., SAVINGS, CURRENT")]
        public string AccountTypeCode { get; set; } = null!;

        [Required]
        [Column("ACCOUNT_TYPE_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Display name of the account type")]
        public string AccountTypeName { get; set; } = null!;

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates whether the account type is active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("Audit: created by user")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE", TypeName = "timestamptz")]
        [SwaggerSchema("Audit: creation timestamp with timezone")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("Audit: last modified by user")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE", TypeName = "timestamptz")]
        [SwaggerSchema("Audit: last modification timestamp with timezone")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [SwaggerSchema("Audit/version control integer field")]
        public int? RowVersion { get; set; }
    }
}
