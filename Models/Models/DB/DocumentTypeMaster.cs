using SharedModels.BackEndCalculation;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<DocumentTypeMaster>(entity =>
{
    entity.ToTable("DOCUMENT_TYPE_MASTER", "hms");

    entity.HasOne(e => e.ApplicableAgentType)
          .WithMany()
          .HasForeignKey(e => e.ApplicableAgentTypeCode)
          .HasConstraintName("fk_document_agent_type")
          .OnDelete(DeleteBehavior.Restrict);
});

     */
    [Table("DOCUMENT_TYPE_MASTER", Schema = "hms")]
    public class DocumentTypeMaster
    {
        [Key]
        [Column("DOCUMENT_TYPE_ID")]
        [SwaggerSchema("Primary key: unique identifier for the document type.")]
        public long DocumentTypeId { get; set; }

        [Column("DOCUMENT_TYPE_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Code representing the document type.")]
        public string? DocumentTypeCode { get; set; }

        [Column("DOCUMENT_TYPE_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Name of the document type.")]
        public string? DocumentTypeName { get; set; }

        [Column("IS_MANDATORY")]
        [SwaggerSchema("Indicates if the document type is mandatory.")]
        public bool? IsMandatory { get; set; }

        [Column("APPLICABLE_AGENT_TYPE")]
        [StringLength(50)]
        [SwaggerSchema("Foreign key referencing applicable agent type.")]
        public string? ApplicableAgentTypeCode { get; set; }

        [Column("STATUS")]
        [StringLength(10)]
        [SwaggerSchema("Status of the document type.")]
        public string? Status { get; set; }

        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who created the record.")]
        public string? CreatedBy { get; set; }

        [Column("CREATED_DATE")]
        [SwaggerSchema("Date and time when the record was created.")]
        public DateTime? CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Date and time of last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        // Navigation property
        [ForeignKey(nameof(ApplicableAgentTypeCode))]
        [SwaggerSchema("The agent type to which this document applies.")]
        public AgentTypeMaster? ApplicableAgentType { get; set; }
    }
}
