using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * 
     modelBuilder.Entity<RoleMenuMapping>(entity =>
{
    entity.ToTable("role_menu_mapping", "hms");

    entity.HasOne(rmm => rmm.Role)
          .WithMany()
          .HasForeignKey(rmm => rmm.RoleId)
          .HasConstraintName("fk_role")
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(rmm => rmm.Menu)
          .WithMany()
          .HasForeignKey(rmm => rmm.MenuId)
          .HasConstraintName("fk_menu")
          .OnDelete(DeleteBehavior.Cascade);
});
     */
    [Table("role_menu_mapping", Schema = "hms")]
    public class RoleMenuMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Based on your SQL 'NOT NULL' without Identity
        [Column("mapping_id")]
        public int MappingId { get; set; }

        [Required]
        [Column("role_id")]
        public int RoleId { get; set; }

        [Required]
        [Column("menu_id")]
        public int MenuId { get; set; }

        [Column("is_visible")]
        public bool IsVisible { get; set; }

        [Column("is_enabled")]
        public bool IsEnabled { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [MaxLength(100)]
        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        public int? RowVersion { get; set; }

        // New column per schema: orgid int4 DEFAULT 0 NULL
        [Column("orgid")]
        public int? OrgId { get; set; } = 0;
        // --- Navigation Properties ---

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        [ForeignKey("MenuId")]
        public virtual MenuMaster? Menu { get; set; }
        // --- Navigation Properties ---

        [ForeignKey("OrgId")]
        public virtual Organisation? Organisation { get; set; }

    }
    public class MenuRoleMappingDto
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public int? ParentMenuId { get; set; }
        public string? RoutePath { get; set; }
        public int? DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool? IsInternal { get; set; }
        public int? MappingId { get; set; }
        public bool IsMapped { get; set; }
        public bool? IsVisible { get; set; }
        public bool? IsEnabled { get; set; }
        public int? RoleId { get; set; }
    }
}
