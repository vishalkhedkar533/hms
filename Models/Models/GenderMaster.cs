
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace HMS.Models
{
    /// <summary>
    /// Master table for gender options (e.g., Male, Female, Other).
    /// </summary>
    [Table("GENDER_MASTER", Schema = "hms")]
    public class GenderMaster
    {
        [Key]
        [Column("GENDER_CODE")]
        [StringLength(10)]
        [SwaggerSchema("Primary key. Short code representing gender (e.g., M, F, O).")]
        public string GenderCode { get; set; } = null!;

        [Required]
        [Column("GENDER_LABEL")]
        [StringLength(20)]
        [SwaggerSchema("Descriptive label for the gender (e.g., Male, Female, Other).")]
        public string GenderLabel { get; set; } = null!;

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates whether the gender option is currently active.")]
        public bool IsActive { get; set; }

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who created the gender entry.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Timestamp of record creation.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the gender entry.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [SwaggerSchema("Row version used for optimistic concurrency control.")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }
    }
}
