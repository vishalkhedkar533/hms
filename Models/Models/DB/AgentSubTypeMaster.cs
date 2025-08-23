using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<AgentSubTypeMaster>(entity =>
{
    entity.ToTable("AGENT_SUB_TYPE_MASTER", "hms");

    entity.HasOne(e => e.AgentTypeMaster)
          .WithMany()
          .HasForeignKey(e => e.AgentTypeCode)
          .HasConstraintName("fk_agent_sub_type_to_type")
          .OnDelete(DeleteBehavior.Restrict);
});

     */
    [Table("AGENT_SUB_TYPE_MASTER", Schema = "hms")]
    public class AgentSubTypeMaster
    {
        [Key]
        [Column("AGENT_SUB_TYPE_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Primary key: agent sub-type code.")]
        public string AgentSubTypeCode { get; set; } = null!;

        [Required]
        [Column("AGENT_SUB_TYPE_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Name of the agent sub-type.")]
        public string AgentSubTypeName { get; set; } = null!;

        [Column("AGENT_TYPE_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Foreign key referencing agent type code.")]
        public string? AgentTypeCode { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the agent sub-type is active.")]
        public bool IsActive { get; set; }

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
        [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        // Navigation property
        [ForeignKey(nameof(AgentTypeCode))]
        [SwaggerSchema("The related agent type.")]
        public AgentTypeMaster? AgentTypeMaster { get; set; }
    }
}
