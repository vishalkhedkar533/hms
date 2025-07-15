using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<RolePermissionMapping>(entity =>
{
    entity.ToTable("ROLE_PERMISSION_MAPPING", "hms");

    entity.HasKey(e => new { e.RoleId, e.PermissionId });

    entity.HasOne(e => e.Role)
          .WithMany()
          .HasForeignKey(e => e.RoleId)
          .HasConstraintName("fk_role")
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.Permission)
          .WithMany()
          .HasForeignKey(e => e.PermissionId)
          .HasConstraintName("fk_permission")
          .OnDelete(DeleteBehavior.Cascade);



});
     */

    /*
     * Program.cs
     * builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

     */
    [Table("ROLE_PERMISSION_MAPPING", Schema = "hms")]
    public class RolePermissionMapping
    {
        [Column("ROLE_ID")]
        [Required]
        [SwaggerSchema("Unique identifier of the role.")]
        public int RoleId { get; set; }

        [Column("PERMISSION_ID")]
        [Required]
        [SwaggerSchema("Unique identifier of the permission.")]
        public int PermissionId { get; set; }

        [Column("IS_ALLOWED")]
        [Required]
        [SwaggerSchema("Indicates whether the permission is granted to the role.")]
        public bool IsAllowed { get; set; }

        [Column("CREATED_BY")]
        [StringLength(100)]
        [Required]
        [SwaggerSchema("User who created this mapping.")]
        public string CreatedBy { get; set; } = null!;

        [Column("CREATED_DATE")]
        [Required]
        [SwaggerSchema("Timestamp when this mapping was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified this mapping.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Used for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        // Optional navigation properties
        [SwaggerSchema(ReadOnly = true)]
        public Role? Role { get; set; }

        [SwaggerSchema(ReadOnly = true)]
        public Permission? Permission { get; set; }
    }
}
