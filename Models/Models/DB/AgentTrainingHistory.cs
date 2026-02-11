using SharedModels.BackEndCalculation;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<AgentTrainingHistory>(entity =>
{
    entity.ToTable("AGENT_TRAINING_HISTORY", "hms");

    entity.HasOne(e => e.Agent)
          .WithMany()
          .HasForeignKey(e => e.AgentId)
          .HasConstraintName("fk_agent")
          .OnDelete(DeleteBehavior.Cascade);
});

     */
    [Table("AGENT_TRAINING_HISTORY", Schema = "hms")]
    public class AgentTrainingHistory
    {
        [Key]
        [Column("LEARNING_ID")]
        [SwaggerSchema("Primary key for the training history record.")]
        public long LearningId { get; set; }

        [Required]
        [Column("AGENT_ID")]
        [SwaggerSchema("Reference to the agent.")]
        public int AgentId { get; set; }

        [Required]
        [StringLength(20)]
        [Column("LEARNING_TYPE")]
        [SwaggerSchema("Type of the learning/training.")]
        public string LearningType { get; set; } = null!;

        [Required]
        [StringLength(50)]
        [Column("LEARNING_CODE")]
        [SwaggerSchema("Code identifying the learning.")]
        public string LearningCode { get; set; } = null!;

        [Required]
        [StringLength(255)]
        [Column("LEARNING_NAME")]
        [SwaggerSchema("Name of the learning/training.")]
        public string LearningName { get; set; } = null!;

        [StringLength(100)]
        [Column("ISSUING_AUTHORITY")]
        [SwaggerSchema("Authority issuing the training certificate.")]
        public string? IssuingAuthority { get; set; }

        [Column("START_DATE", TypeName = "date")]
        [SwaggerSchema("Start date of the training.")]
        public DateTime? StartDate { get; set; }

        [Column("COMPLETION_DATE", TypeName = "date")]
        [SwaggerSchema("Completion date of the training.")]
        public DateTime? CompletionDate { get; set; }

        [Column("EXPIRY_DATE", TypeName = "date")]
        [SwaggerSchema("Expiry date of the training certification.")]
        public DateTime? ExpiryDate { get; set; }

        [Column("SCORE", TypeName = "numeric(5,2)")]
        [SwaggerSchema("Score achieved in the training.")]
        public decimal? Score { get; set; }

        [Required]
        [StringLength(20)]
        [Column("STATUS_CODE")]
        [SwaggerSchema("Status code of the training.")]
        public string StatusCode { get; set; } = null!;

        [Column("DOCUMENT_ID")]
        [SwaggerSchema("Reference to a related document.")]
        public long? DocumentId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("CREATED_BY")]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Timestamp when the record was created.")]
        public DateTime CreatedDate { get; set; }

        [StringLength(100)]
        [Column("MODIFIED_BY")]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token.")]
        public int? RowVersion { get; set; }

        // Navigation property
        [SwaggerSchema("Reference to the agent.")]
        public Agent? Agent { get; set; }
    }
}
