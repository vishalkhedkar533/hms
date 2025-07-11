using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<HierarchyHistory>(entity =>
{
    entity.ToTable("HIERARCHY_HISTORY", "hms");

    entity.HasOne(h => h.HierarchyType)
        .WithMany()
        .HasForeignKey(h => h.HierarchyTypeId)
        .HasConstraintName("fk_hierarchy_type")
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(h => h.OldNode)
        .WithMany()
        .HasForeignKey(h => h.OldNodeId)
        .HasConstraintName("fk_old_node")
        .OnDelete(DeleteBehavior.SetNull);

    entity.HasOne(h => h.NewNode)
        .WithMany()
        .HasForeignKey(h => h.NewNodeId)
        .HasConstraintName("fk_new_node")
        .OnDelete(DeleteBehavior.Cascade);
});

     */
    [Table("HIERARCHY_HISTORY", Schema = "hms")]
    public class HierarchyHistory
    {
        [Key]
        [Column("HISTORY_ID")]
        [SwaggerSchema("Primary key for the hierarchy history record.")]
        public int HistoryId { get; set; }

        [Required]
        [Column("AGENT_ID")]
        [StringLength(50)]
        [SwaggerSchema("Identifier of the agent whose hierarchy changed.")]
        public string AgentId { get; set; } = null!;

        [Required]
        [Column("HIERARCHY_TYPE_ID")]
        [SwaggerSchema("Reference to the hierarchy type.")]
        public int HierarchyTypeId { get; set; }

        [Column("OLD_NODE_ID")]
        [SwaggerSchema("Previous node ID (can be null for first assignment).")]
        public int? OldNodeId { get; set; }

        [Required]
        [Column("NEW_NODE_ID")]
        [SwaggerSchema("New node ID after the change.")]
        public int NewNodeId { get; set; }

        [Required]
        [Column("EFFECTIVE_FROM_DATE", TypeName = "date")]
        [SwaggerSchema("Date from which the change takes effect.")]
        public DateTime EffectiveFromDate { get; set; }

        [Column("EFFECTIVE_TO_DATE", TypeName = "date")]
        [SwaggerSchema("Optional end date for the new node assignment.")]
        public DateTime? EffectiveToDate { get; set; }

        [Column("CHANGE_REASON")]
        [StringLength(100)]
        [SwaggerSchema("Short description or reason for the hierarchy change.")]
        public string? ChangeReason { get; set; }

        [Required]
        [Column("CHANGED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who made the change.")]
        public string ChangedBy { get; set; } = null!;

        [Required]
        [Column("CHANGED_DATE")]
        [SwaggerSchema("Timestamp when the change was made.")]
        public DateTime ChangedDate { get; set; }

        [Column("REMARKS")]
        [SwaggerSchema("Detailed remarks or notes about the change.")]
        public string? Remarks { get; set; }

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who created the history record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
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

        // Navigation properties
        [SwaggerSchema("Hierarchy type associated with the history.")]
        public HierarchyType? HierarchyType { get; set; }

        [SwaggerSchema("Previous node (optional).")]
        public HierarchyNode? OldNode { get; set; }

        [SwaggerSchema("New node after the change.")]
        public HierarchyNode? NewNode { get; set; }
    }
}
