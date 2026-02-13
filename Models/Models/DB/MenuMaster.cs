using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
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
    [Table("menu_master", Schema = "hms")]
    public class MenuMaster
    {
        [Key]
        [Column("menu_id")]
        public int MenuId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("menu_name")]
        public string MenuName { get; set; } = string.Empty;

        [Column("parent_menu_id")]
        public int? ParentMenuId { get; set; }

        [MaxLength(255)]
        [Column("route_path")]
        public string? RoutePath { get; set; }

        [Column("display_order")]
        public int? DisplayOrder { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("is_internal")]
        public bool? IsInternal { get; set; }

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

        // Navigation Properties
        [ForeignKey("ParentMenuId")]
        public virtual MenuMaster? ParentMenu { get; set; }
    }
}
