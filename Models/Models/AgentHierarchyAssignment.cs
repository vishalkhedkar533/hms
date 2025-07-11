using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<AgentHierarchyAssignment>(entity =>
{
    entity.ToTable("AGENT_HIERARCHY_ASSIGNMENT", "hms");

    entity.HasOne(e => e.Node)
          .WithMany()
          .HasForeignKey(e => e.NodeId)
          .HasConstraintName("fk_node")
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.HierarchyType)
          .WithMany()
          .HasForeignKey(e => e.HierarchyTypeId)
          .HasConstraintName("fk_hierarchy_type")
          .OnDelete(DeleteBehavior.Cascade);
});
     */
    [Table("AGENT_HIERARCHY_ASSIGNMENT", Schema = "hms")]
    public class AgentHierarchyAssignment
    {
        [Key]
        [Column("ASSIGNMENT_ID")]
        [SwaggerSchema("Unique identifier for the assignment.")]
        public int AssignmentId { get; set; }

        [Column("AGENT_ID")]
        [Required]
        [StringLength(50)]
        [SwaggerSchema("Agent identifier.")]
        public string AgentId { get; set; } = null!;

        [Column("NODE_ID")]
        [Required]
        [SwaggerSchema("Assigned node ID.")]
        public int NodeId { get; set; }

        [Column("HIERARCHY_TYPE_ID")]
        [Required]
        [SwaggerSchema("Hierarchy type associated with the assignment.")]
        public int HierarchyTypeId { get; set; }

        [Column("IS_PRIMARY")]
        [Required]
        [SwaggerSchema("Indicates if this is the primary assignment.")]
        public bool IsPrimary { get; set; }

        [Column("EFFECTIVE_FROM", TypeName = "date")]
        [Required]
        [SwaggerSchema("Start date of the assignment.")]
        public DateTime EffectiveFrom { get; set; }

        [Column("EFFECTIVE_TO", TypeName = "date")]
        [SwaggerSchema("Optional end date of the assignment.")]
        public DateTime? EffectiveTo { get; set; }

        [Column("REMARKS")]
        [StringLength(1000)]
        [SwaggerSchema("Additional remarks about the assignment.")]
        public string? Remarks { get; set; }

        [Column("CREATED_BY")]
        [Required]
        [StringLength(100)]
        [SwaggerSchema("User who created the assignment.")]
        public string CreatedBy { get; set; } = null!;

        [Column("CREATED_DATE")]
        [Required]
        [SwaggerSchema("Timestamp when the assignment was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the assignment.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema(] ()
