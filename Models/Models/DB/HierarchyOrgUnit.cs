using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<HierarchyOrgUnit>(entity =>
{
    entity.ToTable("HIERARCHY_ORG_UNIT", "hms");

    entity.HasOne(e => e.HierarchyNode)
          .WithMany()
          .HasForeignKey(e => e.NodeId)
          .HasConstraintName("fk_node")
          .OnDelete(DeleteBehavior.Restrict);
});
     */
    [Table("HIERARCHY_ORG_UNIT", Schema = "hms")]
    public class HierarchyOrgUnit
    {
        [Key]
        [Column("ORG_UNIT_ID")]
        [SwaggerSchema("Primary key for the organizational unit.")]
        public long OrgUnitId { get; set; }

        [Required]
        [Column("NODE_ID")]
        [SwaggerSchema("Reference to the hierarchy node.")]
        public long NodeId { get; set; }

        [StringLength(20)]
        [Column("REGION_CODE")]
        [SwaggerSchema("Region code of the organizational unit.")]
        public string? RegionCode { get; set; }

        [StringLength(20)]
        [Column("ZONE_CODE")]
        [SwaggerSchema("Zone code of the organizational unit.")]
        public string? ZoneCode { get; set; }

        [StringLength(20)]
        [Column("BRANCH_CODE")]
        [SwaggerSchema("Branch code of the organizational unit.")]
        public string? BranchCode { get; set; }

        [StringLength(20)]
        [Column("LOCATION_CODE")]
        [SwaggerSchema("Location code of the organizational unit.")]
        public string? LocationCode { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the organizational unit is active.")]
        public bool IsActive { get; set; }

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
        [SwaggerSchema("Reference to the hierarchy node.")]
        [ForeignKey(nameof(NodeId))]
        public HierarchyNode? HierarchyNode { get; set; }
    }
}
