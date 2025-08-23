using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<LocationMaster>(entity =>
{
    entity.ToTable("LOCATION_MASTER", "hms");

    entity.HasOne(e => e.ParentLocation)
          .WithMany()
          .HasForeignKey(e => e.ParentLocationCode)
          .HasConstraintName("fk_location_parent")
          .OnDelete(DeleteBehavior.Restrict);
});

     */
    [Table("LOCATION_MASTER", Schema = "hms")]
    public class LocationMaster
    {
        [Key]
        [Column("LOCATION_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Primary key: location code.")]
        public string LocationCode { get; set; } = null!;

        [Required]
        [Column("LOCATION_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Name of the location.")]
        public string LocationName { get; set; } = null!;

        [Required]
        [Column("LOCATION_TYPE")]
        [StringLength(20)]
        [SwaggerSchema("Type of the location.")]
        public string LocationType { get; set; } = null!;

        [Column("PARENT_LOCATION_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Parent location code (self-referencing foreign key).")]
        public string? ParentLocationCode { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the location is active.")]
        public bool IsActive { get; set; }

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Date and time when the record was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Date and time of last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        // Navigation property for parent location (self-reference)
        [ForeignKey(nameof(ParentLocationCode))]
        [SwaggerSchema("The parent location.")]
        public LocationMaster? ParentLocation { get; set; }
    }
}
