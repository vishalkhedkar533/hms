using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    /*
     *code for AppDbContext.cs
      modelBuilder.Entity<UserRoleMapping>(entity =>
{
    entity.ToTable("user_role_mapping", "hms");

    entity.HasOne<User>()
          .WithMany()
          .HasForeignKey(e => e.UserId)
          .HasConstraintName("fk_user")
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne<Role>()
          .WithMany()
          .HasForeignKey(e => e.RoleId)
          .HasConstraintName("fk_role")
          .OnDelete(DeleteBehavior.Restrict);
});
     */
    [Table("user_role_mapping", Schema = "hms")]
    public class UserRoleMapping
    {
        [Key]
        [Column("mapping_id")]
        [Required(ErrorMessage = "Mapping ID is required.")]
        public long MappingId { get; set; }

        [Column("user_id")]
        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }

        [Column("role_id")]
        [Required(ErrorMessage = "Role ID is required.")]
        public int RoleId { get; set; }

        [Column("is_primary")]
        [Required(ErrorMessage = "IsPrimary is required.")]
        public bool IsPrimary { get; set; }

        [Column("effective_from", TypeName = "date")]
        [Required(ErrorMessage = "EffectiveFrom is required.")]
        public DateTime EffectiveFrom { get; set; }

        [Column("effective_to", TypeName = "date")]
        public DateTime? EffectiveTo { get; set; }

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

        // Navigation Properties (optional, useful for eager/lazy loading)
        public User? User { get; set; }
        public Role? Role { get; set; }
    }
}
