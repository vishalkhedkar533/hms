using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace HMS.Models
{
    [Table("permissions", Schema = "hms")]
    public class Permission
    {
        [Key]
        [Column("permission_id")]
        [SwaggerSchema("Permission ID is required.")]
        // No [DatabaseGenerated] attribute means EF will NOT auto-generate this value
        public int PermissionId { get; set; }

        [Column("permission_name")]
        [StringLength(100)]
        [SwaggerSchema("Permission name is required.")]
        public string PermissionName { get; set; } = null!;

        [Column("description")]
        [StringLength(255)]
        public string? Description { get; set; }

        [Column("module_name")]
        [StringLength(50)]
        public string? ModuleName { get; set; }

        [Column("is_active")]
        [SwaggerSchema("Active status is required.")]
        public bool IsActive { get; set; }

        [Column("created_by")]
        [StringLength(100)]
        [SwaggerSchema("CreatedBy is required.")]
        public string CreatedBy { get; set; } = null!;

        [Column("created_date")]
        [SwaggerSchema("CreatedDate is required.")]
        public DateTime CreatedDate { get; set; }

        [Column("modified_by")]
        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }
    }
}