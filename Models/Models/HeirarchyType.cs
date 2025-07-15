using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<HierarchyType>()
    .ToTable("HIERARCHY_TYPE", "hms");
     */
    [Table("HIERARCHY_TYPE", Schema = "hms")]
    public class HierarchyType
    {
        [Key]
        [Column("HIERARCHY_TYPE_ID")]
        [SwaggerSchema("Unique identifier for the hierarchy type.")]
        public int HierarchyTypeId { get; set; }

        [Column("HIERARCHY_TYPE_NAME")]
        [SwaggerSchema("Hierarchy type name is required.")]
        [StringLength(100)]
        [SwaggerSchema("Name of the hierarchy type.")]
        public string HierarchyTypeName { get; set; } = null!;

        [Column("DESCRIPTION")]
        [StringLength(255)]
        [SwaggerSchema("Description of the hierarchy type.")]
        public string? Description { get; set; }

        [Column("IS_ACTIVE")]
        [Required]
        [SwaggerSchema("Indicates whether the hierarchy type is active.")]
        public bool IsActive { get; set; }

        [Column("CREATED_BY")]
        [Required]
        [StringLength(100)]
        [SwaggerSchema("User who created the hierarchy type.")]
        public string CreatedBy { get; set; } = null!;

        [Column("CREATED_DATE")]
        [Required]
        [SwaggerSchema("Timestamp when the hierarchy type was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the hierarchy type.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Used for optimistic concurrency control.")]
        public int? RowVersion { get; set; }
    }
}
