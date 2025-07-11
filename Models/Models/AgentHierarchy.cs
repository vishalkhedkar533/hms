using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<AgentHierarchy>(entity =>
{
    entity.ToTable("AGENT_HIERARCHY", "hms");

    entity.HasOne(e => e.Agent)
          .WithMany()
          .HasForeignKey(e => e.AgentId)
          .HasConstraintName("fk_agent")
          .OnDelete(DeleteBehavior.Cascade);
});
     */
    [Table("AGENT_HIERARCHY", Schema = "hms")]
    public class AgentHierarchy
    {
        [Key]
        [Column("HIERARCHY_ID")]
        [SwaggerSchema("Primary key for the agent hierarchy record.")]
        public long HierarchyId { get; set; }

        [Required]
        [Column("AGENT_ID")]
        [SwaggerSchema("Reference to the agent.")]
        public int AgentId { get; set; }

        [Required]
        [Column("SUPERVISOR_CODE")]
        [SwaggerSchema("Code of the supervisor.")]
        public int SupervisorCode { get; set; }

        [Required]
        [Column("EFFECTIVE_FROM_DATE", TypeName = "date")]
        [SwaggerSchema("Date from which the hierarchy record is effective.")]
        public DateTime EffectiveFromDate { get; set; }

        [Column("EFFECTIVE_TO_DATE", TypeName = "date")]
        [SwaggerSchema("Date until which the hierarchy record is effective.")]
        public DateTime? EffectiveToDate { get; set; }

        [StringLength(20)]
        [Column("CHANNEL_CODE")]
        [SwaggerSchema("Channel code associated with the hierarchy.")]
        public string? ChannelCode { get; set; }

        [StringLength(20)]
        [Column("DESIGNATION_CODE")]
        [SwaggerSchema("Designation code for the agent.")]
        public string? DesignationCode { get; set; }

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
