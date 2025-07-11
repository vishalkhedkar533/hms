using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<AgentDocument>(entity =>
{
    entity.ToTable("AGENT_DOCUMENT", "hms");

    entity.HasOne(e => e.Agent)
          .WithMany()
          .HasForeignKey(e => e.AgentId)
          .HasConstraintName("fk_agent")
          .OnDelete(DeleteBehavior.Cascade);
});

     */
    [Table("AGENT_DOCUMENT", Schema = "hms")]
    public class AgentDocument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("DOCUMENT_ID")]
        [SwaggerSchema("Primary key, auto-incremented document identifier.")]
        public int DocumentId { get; set; }

        [Required]
        [Column("AGENT_ID")]
        [SwaggerSchema("Reference to the associated agent.")]
        public int AgentId { get; set; }

        [Required]
        [StringLength(20)]
        [Column("DOCUMENT_TYPE_CODE")]
        [SwaggerSchema("Type code of the document.")]
        public string DocumentTypeCode { get; set; } = null!;

        [Required]
        [StringLength(255)]
        [Column("DOCUMENT_NAME")]
        [SwaggerSchema("Name of the document.")]
        public string DocumentName { get; set; } = null!;

        [StringLength(500)]
        [Column("DOCUMENT_PATH")]
        [SwaggerSchema("Path or URL where the document is stored.")]
        public string? DocumentPath { get; set; }

        [StringLength(100)]
        [Column("ISSUING_AUTHORITY")]
        [SwaggerSchema("Authority that issued the document.")]
        public string? IssuingAuthority { get; set; }

        [StringLength(100)]
        [Column("DOCUMENT_NUMBER")]
        [SwaggerSchema("Document number or identifier.")]
        public string? DocumentNumber { get; set; }

        [Column("ISSUE_DATE", TypeName = "date")]
        [SwaggerSchema("Date when the document was issued.")]
        public DateTime? IssueDate { get; set; }

        [Column("EXPIRY_DATE", TypeName = "date")]
        [SwaggerSchema("Expiry date of the document, if applicable.")]
        public DateTime? ExpiryDate { get; set; }

        [Required]
        [Column("IS_VERIFIED")]
        [SwaggerSchema("Indicates if the document has been verified.")]
        public bool IsVerified { get; set; }

        [StringLength(50)]
        [Column("VERIFIED_BY")]
        [SwaggerSchema("User who verified the document.")]
        public string? VerifiedBy { get; set; }

        [Column("VERIFIED_DATE")]
        [SwaggerSchema("Date and time when the document was verified.")]
        public DateTime? VerifiedDate { get; set; }

        [Required]
        [Column("IS_MANDATORY")]
        [SwaggerSchema("Indicates if the document is mandatory.")]
        public bool IsMandatory { get; set; }

        [Column("UPLOAD_DATE")]
        [SwaggerSchema("Date and time when the document was uploaded.")]
        public DateTime? UploadDate { get; set; }

        [Required]
        [StringLength(100)]
        [Column("CREATED_BY")]
        [SwaggerSchema("User who created the document record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Timestamp when the document record was created.")]
        public DateTime CreatedDate { get; set; }

        [StringLength(100)]
        [Column("MODIFIED_BY")]
        [SwaggerSchema("User who last modified the document record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        // Navigation property
        [SwaggerSchema("Reference to the associated agent.")]
        public Agent? Agent { get; set; }
    }
}
