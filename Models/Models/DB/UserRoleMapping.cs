using SharedModels.BackEndCalculation;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
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
        [SwaggerSchema("Mapping ID is required.")]
        public long MappingId { get; set; }

        [Column("user_id")]
        [SwaggerSchema("User ID is required.")]
        public int UserId { get; set; }

        [Column("role_id")]
        [SwaggerSchema("Role ID is required.")]
        public int RoleId { get; set; }

        [Column("is_primary")]
        [SwaggerSchema("IsPrimary is required.")]
        public bool IsPrimary { get; set; }

        [Column("effective_from", TypeName = "date")]
        [SwaggerSchema("EffectiveFrom is required.")]
        public DateTime EffectiveFrom { get; set; }

        [Column("effective_to", TypeName = "date")]
        public DateTime? EffectiveTo { get; set; }

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

        // Navigation Properties (optional, useful for eager/lazy loading)
        public User? User { get; set; }
        public Role? Role { get; set; }
    }
}
