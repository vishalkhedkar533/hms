using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<HierarchyNode>(entity =>
{
    entity.ToTable("HIERARCHY_NODE", "hms");

    // Unique constraint on NODE_CODE
    entity.HasIndex(e => e.NodeCode).IsUnique();

    // FK: HIERARCHY_TYPE_ID → HIERARCHY_TYPE
    entity.HasOne(e => e.HierarchyType)
          .WithMany()
          .HasForeignKey(e => e.HierarchyTypeId)
          .HasConstraintName("fk_hierarchy_type")
          .OnDelete(DeleteBehavior.Cascade);

    // FK: PARENT_NODE_ID → self
    entity.HasOne(e => e.ParentNode)
          .WithMany(p => p.Children)
          .HasForeignKey(e => e.ParentNodeId)
          .HasConstraintName("fk_parent_node")
          .OnDelete(DeleteBehavior.SetNull);
});

     */

    [Table("HIERARCHY_NODE", Schema = "hms")]
    public class HierarchyNode
    {
        [Key]
        [Column("NODE_ID")]
        [SwaggerSchema("Unique identifier for the node.")]
        public int NodeId { get; set; }

        [Column("HIERARCHY_TYPE_ID")]
        [Required]
        [SwaggerSchema("Foreign key to the hierarchy type.")]
        public int HierarchyTypeId { get; set; }

        [Column("NODE_CODE")]
        [Required]
        [StringLength(50)]
        [SwaggerSchema("Unique code representing the node.")]
        public string NodeCode { get; set; } = null!;

        [Column("NODE_NAME")]
        [Required]
        [StringLength(150)]
        [SwaggerSchema("Name of the node.")]
        public string NodeName { get; set; } = null!;

        [Column("LEVEL")]
        [SwaggerSchema("Level of the node in the hierarchy.")]
        public int? Level { get; set; }

        [Column("LOCATION_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Optional location code for the node.")]
        public string? LocationCode { get; set; }

        [Column("BRANCH_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Optional branch code for the node.")]
        public string? BranchCode { get; set; }

        [Column("PARENT_NODE_ID")]
        [SwaggerSchema("Optional parent node reference.")]
        public int? ParentNodeId { get; set; }

        [Column("CHANNEL_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Optional channel code for the node.")]
        public string? ChannelCode { get; set; }

        [Column("IS_ACTIVE")]
        [Required]
        [SwaggerSchema("Indicates whether the node is active.")]
        public bool IsActive { get; set; }

        [Column("CREATED_BY")]
        [Required]
        [StringLength(100)]
        [SwaggerSchema("User who created the node.")]
        public string CreatedBy { get; set; } = null!;

        [Column("CREATED_DATE")]
        [Required]
        [SwaggerSchema("Timestamp when the node was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the node.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Used for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        // Navigation properties
        [SwaggerSchema("Reference to hierarchy type.")]
        public HierarchyType? HierarchyType { get; set; }

        [SwaggerSchema("Reference to parent node.")]
        public HierarchyNode? ParentNode { get; set; }

        [SwaggerSchema("Child nodes under this parent.")]
        public ICollection<HierarchyNode>? Children { get; set; }
    }
}
