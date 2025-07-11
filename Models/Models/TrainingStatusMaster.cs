using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /// <summary>
    /// Master table for training status such as ENROLLED, COMPLETED.
    /// </summary>
    [Table("TRAINING_STATUS_MASTER", Schema = "hms")]
    public class TrainingStatusMaster
    {
        [Key]
        [Column("STATUS_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Training status code, e.g., ENROLLED, COMPLETED")]
        public string StatusCode { get; set; } = null!;

        [Required]
        [Column("STATUS_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Description of the training status")]
        public string StatusName { get; set; } = null!;

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates whether the training status is active")]
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
        [SwaggerSchema("Audit: version control")]
        public int? RowVersion { get; set; }
    }
}
