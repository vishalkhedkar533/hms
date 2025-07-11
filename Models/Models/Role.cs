using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("roles", Schema = "hms")]
    public class Role
    {
        [Key]
        [Column("role_id")]
        [Required(ErrorMessage = "Role ID is required.")]
        // No [DatabaseGenerated] means this is manually assigned (not auto-increment)
        public int RoleId { get; set; }

        [Column("role_name")]
        [StringLength(100)]
        [Required(ErrorMessage = "Role name is required.")]
        public string RoleName { get; set; } = null!;

        [Column("description")]
        [StringLength(255)]
        public string? Description { get; set; }

        [Column("is_system_role")]
        [Required(ErrorMessage = "IsSystemRole is required.")]
        public bool IsSystemRole { get; set; }

        [Column("is_active")]
        [Required(ErrorMessage = "IsActive is required.")]
        public bool IsActive { get; set; }

        [Column("created_by")]
        [StringLength(100)]
        [Required(ErrorMessage = "CreatedBy is required.")]
        public string CreatedBy { get; set; } = null!;

        [Column("created_date")]
        [Required(ErrorMessage = "CreatedDate is required.")]
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
