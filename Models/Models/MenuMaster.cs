using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<MenuMaster>(entity =>
{
    entity.ToTable("MENU_MASTER", "hms");

    entity.HasOne(m => m.ParentMenu)
          .WithMany(m => m.Children)
          .HasForeignKey(m => m.ParentMenuId)
          .HasConstraintName("fk_parent_menu")
          .OnDelete(DeleteBehavior.SetNull); // Matches ON DELETE SET NULL
});

     */
    [Table("MENU_MASTER", Schema = "hms")]
    public class MenuMaster
    {
        [Key]
        [Column("MENU_ID")]
        [SwaggerSchema("Menu ID is required.")]
        public int MenuId { get; set; }

        [Column("MENU_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Menu name is required.")]
        public string MenuName { get; set; } = null!;

        [Column("PARENT_MENU_ID")]
        public int? ParentMenuId { get; set; }

        [Column("ROUTE_PATH")]
        [StringLength(255)]
        public string? RoutePath { get; set; }

        [Column("DISPLAY_ORDER")]
        public int? DisplayOrder { get; set; }

        [Column("IS_ACTIVE")]
        [Required]
        public bool IsActive { get; set; }

        [Column("IS_INTERNAL")]
        public bool? IsInternal { get; set; }

        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("CreatedBy is required.")]
        public string CreatedBy { get; set; } = null!;

        [Column("CREATED_DATE")]
        [Required]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }

        // Self-referencing Navigation Properties
        [ForeignKey("ParentMenuId")]
        public MenuMaster? ParentMenu { get; set; }

        public ICollection<MenuMaster>? Children { get; set; }
    }
}
