using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<LearningCatalogMaster>(entity =>
{
    entity.ToTable("LEARNING_CATALOG_MASTER", "hms");

    entity.Property(e => e.Duration)
          .HasPrecision(4, 1);

    entity.Property(e => e.Status)
          .HasMaxLength(10);
});

     */
    [Table("LEARNING_CATALOG_MASTER", Schema = "hms")]
    public class LearningCatalogMaster
    {
        [Key]
        [Column("LEARNING_CODE")]
        [StringLength(50)]
        [SwaggerSchema("Primary key: unique code for the learning item.")]
        public string LearningCode { get; set; } = null!;

        [Required]
        [Column("LEARNING_NAME")]
        [StringLength(255)]
        [SwaggerSchema("Name of the learning program.")]
        public string LearningName { get; set; } = null!;

        [Required]
        [Column("LEARNING_TYPE")]
        [StringLength(20)]
        [SwaggerSchema("Type of learning (e.g., TRAINING, CERTIFICATION).")]
        public string LearningType { get; set; } = null!;

        [Column("DURATION", TypeName = "numeric(4,1)")]
        [Range(0.0, 9999.9)]
        [SwaggerSchema("Duration in hours (e.g., 2.5).")]
        public decimal? Duration { get; set; }

        [Required]
        [Column("IS_MANDATORY")]
        [SwaggerSchema("Indicates if the learning is mandatory.")]
        public bool IsMandatory { get; set; }

        [Required]
        [Column("STATUS")]
        [StringLength(10)]
        [SwaggerSchema("Status of the learning item (e.g., Active, Inactive).")]
        public string Status { get; set; } = null!;

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Date and time when the record was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Date and time of last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token for version control.")]
        public int? RowVersion { get; set; }
    }
}
