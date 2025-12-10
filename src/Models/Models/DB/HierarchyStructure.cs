using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<HierarchyStructure>(entity =>
{
    entity.ToTable("HIERARCHY_STRUCTURE", "hms");

    entity.HasOne(e => e.HierarchyType)
        .WithMany()
        .HasForeignKey(e => e.HierarchyTypeId)
        .HasConstraintName("fk_hierarchy_type")
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.ParentNode)
        .WithMany()
        .HasForeignKey(e => e.ParentNodeId)
        .HasConstraintName("fk_parent_node")
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.ChildNode)
        .WithMany()
        .HasForeignKey(e => e.ChildNodeId)
        .HasConstraintName("fk_child_node")
        .OnDelete(DeleteBehavior.Cascade);
});

     */
    [Table("HIERARCHY_STRUCTURE", Schema = "hms")]
    public class HierarchyStructure
    {
        [Key]
        [Column("STRUCTURE_ID")]
        [SwaggerSchema("Unique identifier for the hierarchy structure.")]
        public int StructureId { get; set; }

        [Column("HIERARCHY_TYPE_ID")]
        [Required]
        [SwaggerSchema("Foreign key to the hierarchy type.")]
        public int HierarchyTypeId { get; set; }

        [Column("PARENT_NODE_ID")]
        [Required]
        [SwaggerSchema("Reference to the parent node.")]
        public int ParentNodeId { get; set; }

        [Column("CHILD_NODE_ID")]
        [Required]
        [SwaggerSchema("Reference to the child node.")]
        public int ChildNodeId { get; set; }

        [Column("RELATIONSHIP_TYPE")]
        [StringLength(50)]
        [SwaggerSchema("Describes the nature of the relationship.")]
        public string? RelationshipType { get; set; }

        [Column("IS_ACTIVE")]
        [Required]
        [SwaggerSchema("Indicates whether the structure is active.")]
        public bool IsActive { get; set; }

        [Column("EFFECTIVE_FROM", TypeName = "date")]
        [Required]
        [SwaggerSchema("Date from which this relationship is effective.")]
        public DateTime EffectiveFrom { get; set; }

        [Column("EFFECTIVE_TO", TypeName = "date")]
        [SwaggerSchema("Optional end date of the relationship.")]
        public DateTime? EffectiveTo { get; set; }

        [Column("CREATED_BY")]
        [StringLength(100)]
        [Required]
        [SwaggerSchema("User who created this record.")]
        public string CreatedBy { get; set; } = null!;

        [Column("CREATED_DATE")]
        [Required]
        [SwaggerSchema("Timestamp when the record was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Used for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        // Navigation Properties
        [SwaggerSchema("Reference to the hierarchy type.")]
        public HierarchyType? HierarchyType { get; set; }

        [SwaggerSchema("Reference to the parent node.")]
        public HierarchyNode? ParentNode { get; set; }

        [SwaggerSchema("Reference to the child node.")]
        public HierarchyNode? ChildNode { get; set; }
    }
}
