using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<DesignationMaster>(entity =>
{
    entity.ToTable("DESIGNATION_MASTER", "hms");

    entity.HasOne(d => d.ChannelMaster)
          .WithMany()
          .HasForeignKey(d => d.ChannelCode)
          .HasConstraintName("fk_channel_code")
          .OnDelete(DeleteBehavior.Restrict);
});

     */
    [Table("DESIGNATION_MASTER", Schema = "hms")]
    public class DesignationMaster
    {
        [Key]
        [Column("DESIGNATION_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Primary key: designation code.")]
        public string DesignationCode { get; set; } = null!;

        [Required]
        [Column("DESIGNATION_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Name of the designation.")]
        public string DesignationName { get; set; } = null!;

        [Column("CHANNEL_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Foreign key referencing channel code.")]
        public string? ChannelCode { get; set; }

        [Column("LEVEL")]
        [SwaggerSchema("Hierarchy level of the designation.")]
        public int? Level { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the designation is active.")]
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

        // Navigation property
        [ForeignKey(nameof(ChannelCode))]
        [SwaggerSchema("The related channel.")]
        public ChannelMaster? ChannelMaster { get; set; }
    }
}
