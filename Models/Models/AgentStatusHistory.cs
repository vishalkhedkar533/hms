using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<AgentStatusHistory>(entity =>
{
    entity.ToTable("AGENT_STATUS_HISTORY", "hms");

    entity.HasOne(e => e.Agent)
          .WithMany()
          .HasForeignKey(e => e.AgentId)
          .HasConstraintName("fk_agent")
          .OnDelete(DeleteBehavior.Cascade);
});

     */
    [Table("AGENT_STATUS_HISTORY", Schema = "hms")]
    public class AgentStatusHistory
    {
        [Key]
        [Column("STATUS_HISTORY_ID")]
        [SwaggerSchema("Primary key for the status history record.")]
        public int StatusHistoryId { get; set; }

        [Required]
        [Column("AGENT_ID")]
        [SwaggerSchema("Reference to the agent.")]
        public int AgentId { get; set; }

        [Required]
        [StringLength(20)]
        [Column("STATUS_CODE")]
        [SwaggerSchema("Status code representing the agent's status.")]
        public string StatusCode { get; set; } = null!;

        [StringLength(20)]
        [Column("REASON_CODE")]
        [SwaggerSchema("Reason code for the status change.")]
        public string? ReasonCode { get; set; }

        [Required]
        [Column("EFFECTIVE_DATE", TypeName = "date")]
        [SwaggerSchema("Date when the status became effective.")]
        public DateTime EffectiveDate { get; set; }

        [Column("END_DATE", TypeName = "date")]
        [SwaggerSchema("Date when the status ended.")]
        public DateTime? EndDate { get; set; }

        [Required]
        [StringLength(50)]
        [Column("CHANGED_BY")]
        [SwaggerSchema("User who changed the status.")]
        public string ChangedBy { get; set; } = null!;

        [Required]
        [Column("CHANGED_DATE")]
        [SwaggerSchema("Timestamp when the status was changed.")]
        public DateTime ChangedDate { get; set; }

        [StringLength(1000)]
        [Column("REMARKS")]
        [SwaggerSchema("Additional remarks related to the status change.")]
        public string? Remarks { get; set; }

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
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        // Navigation property
        [SwaggerSchema("Reference to the agent.")]
        public Agent? Agent { get; set; }
    }
}
