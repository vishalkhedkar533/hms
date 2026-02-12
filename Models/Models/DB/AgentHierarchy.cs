using SharedModels.BackEndCalculation;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
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
    [Table("agent_hierarchy", Schema = "hms")]
    public class AgentHierarchy
    {
        [Key]
        [Column("hierarchy_id")]
        [SwaggerSchema("Primary key for the agent hierarchy record.")]
        public long HierarchyId { get; set; }

        [Required]
        [Column("agent_id")]
        [SwaggerSchema("Reference to the agent.")]
        public int AgentId { get; set; }

        [Required]
        [Column("supervisor_code")]
        [SwaggerSchema("Code of the supervisor.")]
        public int SupervisorCode { get; set; }

        [Required]
        [Column("effective_from_date", TypeName = "date")]
        [SwaggerSchema("Date from which the hierarchy record is effective.")]
        public DateTime EffectiveFromDate { get; set; }

        [Column("effective_to_date", TypeName = "date")]
        [SwaggerSchema("Date until which the hierarchy record is effective.")]
        public DateTime? EffectiveToDate { get; set; }

        [StringLength(20)]
        [Column("channel_code")]
        [SwaggerSchema("Channel code associated with the hierarchy.")]
        public string? ChannelCode { get; set; }

        [StringLength(20)]
        [Column("designation_code")]
        [SwaggerSchema("Designation code for the agent.")]
        public string? DesignationCode { get; set; }

        [Required]
        [StringLength(100)]
        [Column("created_by")]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("created_date")]
        [SwaggerSchema("Timestamp when the record was created.")]
        public DateTime CreatedDate { get; set; }

        [StringLength(100)]
        [Column("modified_by")]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        // Navigation property
        [SwaggerSchema("Reference to the agent.")]
        public Agent? Agent { get; set; }
    }
}
