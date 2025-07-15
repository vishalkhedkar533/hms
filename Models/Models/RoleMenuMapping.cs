using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * 
     modelBuilder.Entity<RoleMenuMapping>(entity =>
{
    entity.ToTable("ROLE_MENU_MAPPING", "hms");

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
    [Table("ROLE_MENU_MAPPING", Schema = "hms")]
    public class RoleMenuMapping
    {
        [Key]
        [Column("MAPPING_ID")]
        [SwaggerSchema("Mapping ID is required.")]
        public int MappingId { get; set; }

        [Column("ROLE_ID")]
        [SwaggerSchema("Role ID is required.")]
        public int RoleId { get; set; }

        [Column("MENU_ID")]
        [SwaggerSchema("Menu ID is required.")]
        public int MenuId { get; set; }

        [Column("IS_VISIBLE")]
        [SwaggerSchema("IsVisible is required.")]
        public bool IsVisible { get; set; }

        [Column("IS_ENABLED")]
        [SwaggerSchema("IsEnabled is required.")]
        public bool IsEnabled { get; set; }

        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("CreatedBy is required.")]
        public string CreatedBy { get; set; } = null!;

        [Column("CREATED_DATE")]
        [SwaggerSchema("CreatedDate is required.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }

        // Optional: Navigation properties for JOIN support
        public Role? Role { get; set; }
        public MenuMaster? Menu { get; set; }
    }
}
