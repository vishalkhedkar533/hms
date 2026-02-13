using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("roles", Schema = "hms")]
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("role_id")]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("role_name")]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("is_system_role")]
        public bool IsSystemRole { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Using DateTimeOffset to match PostgreSQL 'timestamptz'
        /// </summary>
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [MaxLength(100)]
        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        public int? RowVersion { get; set; }

        [Column("orgid")]
        public int OrgId { get; set; } = 0;

        // Navigation Property for Foreign Key: fk_role_org
        [ForeignKey("OrgId")]
        public virtual Organisation? Organisation { get; set; }
    }
}
