using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<AgentAuditTrail>(entity =>
{
    entity.ToTable("agent_audit_trail", "hms");

    entity.HasOne(e => e.Agent)
          .WithMany()
          .HasForeignKey(e => e.AgentId)
          .HasConstraintName("fk_agent")
          .OnDelete(DeleteBehavior.Cascade);
});

     */
    //[Table("agent_audit_trail", Schema = "hms")]
    //public class AgentAuditTrail
    //{
    //    [Key]
    //    [Column("audit_id")]
    //    [SwaggerSchema("Primary key for the audit trail record.")]
    //    public long AuditId { get; set; }

    //    [Required]
    //    [Column("agent_id")]
    //    [SwaggerSchema("Reference to the agent whose field was changed.")]
    //    public int AgentId { get; set; }

    //    [Required]
    //    [StringLength(100)]
    //    [Column("field_name")]
    //    [SwaggerSchema("Name of the field that was changed.")]
    //    public string FieldName { get; set; } = null!;

    //    [StringLength(255)]
    //    [Column("old_value")]
    //    [SwaggerSchema("Previous value of the field.")]
    //    public string? OldValue { get; set; }

    //    [StringLength(255)]
    //    [Column("new_value")]
    //    [SwaggerSchema("New value of the field.")]
    //    public string? NewValue { get; set; }

    //    [Required]
    //    [StringLength(100)]
    //    [Column("changed_by")]
    //    [SwaggerSchema("User who made the change.")]
    //    public string ChangedBy { get; set; } = null!;

    //    [Required]
    //    [Column("changed_date")]
    //    [SwaggerSchema("Timestamp when the change was made.")]
    //    public DateTime ChangedDate { get; set; }

    //    [Required]
    //    [StringLength(100)]
    //    [Column("created_by")]
    //    [SwaggerSchema("User who created the audit record.")]
    //    public string CreatedBy { get; set; } = null!;

    //    [Required]
    //    [Column("created_date")]
    //    [SwaggerSchema("Timestamp when the audit record was created.")]
    //    public DateTime CreatedDate { get; set; }

    //    [StringLength(100)]
    //    [Column("modified_by")]
    //    [SwaggerSchema("User who last modified the audit record.")]
    //    public string? ModifiedBy { get; set; }

    //    [Column("modified_date")]
    //    [SwaggerSchema("Timestamp of last modification.")]
    //    public DateTime? ModifiedDate { get; set; }

    //    [Column("rowversion")]
    //    [ConcurrencyCheck]
    //    [SwaggerSchema("Concurrency token.")]
    //    public int? RowVersion { get; set; }

    //    // Navigation property
    //    [SwaggerSchema("Reference to the agent.")]
    //    [NotMapped]
    //    public Agent? Agent { get; set; }
    //}

    //public class AgentAuditTrailDTO {
    //    public long AgentCode { get; set; }
    //    public string AgentId { get; set; }
    //    public DateTime? ChangedOn { get; set; }
    //    public string FieldName { get; set; } = null!;
    //    public string? OldValue { get; set; }
    //    public string? NewValue { get; set; }
    //    public string? ModifiedBy { get; set; }
    //    public DateTime? ModifiedDate { get; set; }
    //    public string ChangeDescription { get 
    //        {
    //            return $"{ModifiedBy} Modified {FieldName} changed from '{OldValue}' to '{NewValue}'";
    //        }
    //    }
    //}

}
