using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("roles", Schema = "hms")]
    public class Role
    {
        [Key]
        [Column("role_id")]
        [SwaggerSchema("Role ID is required.")]
        // No [DatabaseGenerated] means this is manually assigned (not auto-increment)
        public int RoleId { get; set; }

        [Column("role_name")]
        [StringLength(100)]
        [SwaggerSchema("Role name is required.")]
        public string RoleName { get; set; } = null!;

        [Column("description")]
        [StringLength(255)]
        public string? Description { get; set; }

        [Column("is_system_role")]
        [SwaggerSchema("IsSystemRole is required.")]
        public bool IsSystemRole { get; set; }

        [Column("is_active")]
        [SwaggerSchema("IsActive is required.")]
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
