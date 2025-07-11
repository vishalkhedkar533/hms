using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    [Table("permissions", Schema = "hms")]
    public class Permission
    {
        [Key]
        [Column("permission_id")]
        [StringLength(30)]
        [Required(ErrorMessage = "Permission ID is required.")]
        public string PermissionId { get; set; }

        [Column("permission_name")]
        [StringLength(100)]
        [Required(ErrorMessage = "Permission name is required.")]
        public string PermissionName { get; set; }

        [Column("description")]
        [StringLength(255)]
        public string? Description { get; set; }

        [Column("module_name")]
        [StringLength(50)]
        public string? ModuleName { get; set; }

        [Column("is_active")]
        [Required(ErrorMessage = "Active status is required.")]
        public bool IsActive { get; set; }

        [Column("created_by")]
        [StringLength(100)]
        [Required(ErrorMessage = "CreatedBy is required.")]
        public string CreatedBy { get; set; }

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