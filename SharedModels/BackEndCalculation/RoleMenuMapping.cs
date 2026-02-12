using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.BackEndCalculation
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
        [Column("mapping_id")]
        [SwaggerSchema("Mapping ID is required.")]
        public int MappingId { get; set; }

        [Column("role_id")]
        [SwaggerSchema("Role ID is required.")]
        public int RoleId { get; set; }

        [Column("menu_id")]
        [SwaggerSchema("Menu ID is required.")]
        public int MenuId { get; set; }

        [Column("is_visible")]
        [SwaggerSchema("IsVisible is required.")]
        public bool IsVisible { get; set; }

        [Column("is_enabled")]
        [SwaggerSchema("IsEnabled is required.")]
        public bool IsEnabled { get; set; }

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

        // Optional: Navigation properties for JOIN support
        public Role? Role { get; set; }
        public MenuMaster? Menu { get; set; }
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
