using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<AgentAuditTrail>(entity =>
{
    entity.ToTable("AGENT_AUDIT_TRAIL", "hms");

    entity.HasOne(e => e.Agent)
          .WithMany()
          .HasForeignKey(e => e.AgentId)
          .HasConstraintName("fk_agent")
          .OnDelete(DeleteBehavior.Cascade);
});

     */
    [Table("AGENT_AUDIT_TRAIL", Schema = "hms")]
    public class AgentAuditTrail
    {
        [Key]
        [Column("AUDIT_ID")]
        [SwaggerSchema("Primary key for the audit trail record.")]
        public long AuditId { get; set; }

        [Required]
        [Column("AGENT_ID")]
        [SwaggerSchema("Reference to the agent whose field was changed.")]
        public int AgentId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("FIELD_NAME")]
        [SwaggerSchema("Name of the field that was changed.")]
        public string FieldName { get; set; } = null!;

        [StringLength(255)]
        [Column("OLD_VALUE")]
        [SwaggerSchema("Previous value of the field.")]
        public string? OldValue { get; set; }

        [StringLength(255)]
        [Column("NEW_VALUE")]
        [SwaggerSchema("New value of the field.")]
        public string? NewValue { get; set; }

        [Required]
        [StringLength(100)]
        [Column("CHANGED_BY")]
        [SwaggerSchema("User who made the change.")]
        public string ChangedBy { get; set; } = null!;

        [Required]
        [Column("CHANGED_DATE")]
        [SwaggerSchema("Timestamp when the change was made.")]
        public DateTime ChangedDate { get; set; }

        [Required]
        [StringLength(100)]
        [Column("CREATED_BY")]
        [SwaggerSchema("User who created the audit record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Timestamp when the audit record was created.")]
        public DateTime CreatedDate { get; set; }

        [StringLength(100)]
        [Column("MODIFIED_BY")]
        [SwaggerSchema("User who last modified the audit record.")]
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
