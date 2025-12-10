using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<AgentLocationMapping>(entity =>
{
    entity.ToTable("AGENT_LOCATION_MAPPING", "hms");

    entity.HasOne(e => e.Agent)
          .WithMany()
          .HasForeignKey(e => e.AgentId)
          .HasConstraintName("fk_agent")
          .OnDelete(DeleteBehavior.Cascade);
});

     */
    [Table("AGENT_LOCATION_MAPPING", Schema = "hms")]
    public class AgentLocationMapping
    {
        [Key]
        [Column("MAPPING_ID")]
        [SwaggerSchema("Primary key for the agent location mapping.")]
        public long MappingId { get; set; }

        [Required]
        [Column("AGENT_ID")]
        [SwaggerSchema("Reference to the agent.")]
        public int AgentId { get; set; }

        [StringLength(20)]
        [Column("BRANCH_CODE")]
        [SwaggerSchema("Code of the branch.")]
        public string? BranchCode { get; set; }

        [StringLength(20)]
        [Column("LOCATION_CODE")]
        [SwaggerSchema("Code of the location.")]
        public string? LocationCode { get; set; }

        [Required]
        [Column("IS_PRIMARY")]
        [SwaggerSchema("Indicates if this is the primary location for the agent.")]
        public bool IsPrimary { get; set; }

        [Column("EFFECTIVE_DATE", TypeName = "date")]
        [SwaggerSchema("Date from which this location mapping is effective.")]
        public DateTime? EffectiveDate { get; set; }

        [Column("END_DATE", TypeName = "date")]
        [SwaggerSchema("Date until which this location mapping is effective.")]
        public DateTime? EndDate { get; set; }

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
