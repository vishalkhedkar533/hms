using Models;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<OrgUserMapping>(entity =>
{
    entity.ToTable("ORG_USER_MAPPING", "hms");

    entity.HasOne(e => e.User)
          .WithMany()
          .HasForeignKey(e => e.UserId)
          .HasConstraintName("fk_user")
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(e => e.HierarchyNode)
          .WithMany()
          .HasForeignKey(e => e.NodeId)
          .HasConstraintName("fk_node")
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(e => e.HierarchyType)
          .WithMany()
          .HasForeignKey(e => e.HierarchyTypeId)
          .HasConstraintName("fk_hierarchy_type")
          .OnDelete(DeleteBehavior.Restrict);
});

     */
    [Table("ORG_USER_MAPPING", Schema = "hms")]
    public class OrgUserMapping
    {
        [Key]
        [Column("MAPPING_ID")]
        [SwaggerSchema("Primary key for the organization user mapping.")]
        public long MappingId { get; set; }

        [Required]
        [Column("USER_ID")]
        [SwaggerSchema("Reference to the user.")]
        public int UserId { get; set; }

        [Required]
        [Column("NODE_ID")]
        [SwaggerSchema("Reference to the hierarchy node.")]
        public int NodeId { get; set; }

        [Required]
        [Column("HIERARCHY_TYPE_ID")]
        [SwaggerSchema("Reference to the hierarchy type.")]
        public int HierarchyTypeId { get; set; }

        [Required]
        [Column("IS_PRIMARY")]
        [SwaggerSchema("Indicates if this is the primary mapping.")]
        public bool IsPrimary { get; set; }

        [Column("EFFECTIVE_FROM", TypeName = "date")]
        [SwaggerSchema("Date from which the mapping is effective.")]
        public DateTime? EffectiveFrom { get; set; }

        [Column("EFFECTIVE_TO", TypeName = "date")]
        [SwaggerSchema("Date until which the mapping is effective.")]
        public DateTime? EffectiveTo { get; set; }

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

        // Navigation properties
        [SwaggerSchema("The mapped user.")]
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [SwaggerSchema("The hierarchy node.")]
        [ForeignKey(nameof(NodeId))]
        public HierarchyNode? HierarchyNode { get; set; }

        [SwaggerSchema("The hierarchy type.")]
        [ForeignKey(nameof(HierarchyTypeId))]
        public HierarchyType? HierarchyType { get; set; }
    }
}
