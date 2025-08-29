using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<SubchannelMaster>(entity =>
{
    entity.ToTable("SUBCHANNEL_MASTER", "hms");

    entity.HasOne(e => e.ChannelMaster)
          .WithMany()
          .HasForeignKey(e => e.ChannelCode)
          .HasConstraintName("fk_channel_code")
          .OnDelete(DeleteBehavior.Restrict);
});

     */
    [Table("SUBCHANNEL_MASTER", Schema = "hms")]
    public class SubchannelMaster
    {
        [Key]
        [Column("SUBCHANNEL_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Primary key: subchannel code.")]
        public string SubchannelCode { get; set; } = null!;

        [Required]
        [Column("CHANNEL_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Foreign key referencing channel code.")]
        public string ChannelCode { get; set; } = null!;

        [Required]
        [Column("SUBCHANNEL_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Name of the subchannel.")]
        public string SubchannelName { get; set; } = null!;

        [Column("DESCRIPTION", TypeName = "text")]
        [SwaggerSchema("Description of the subchannel.")]
        public string? Description { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the subchannel is active.")]
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
        [SwaggerSchema("The related channel.")]
        [ForeignKey(nameof(ChannelCode))]
        public ChannelMaster? ChannelMaster { get; set; }
    }
}
