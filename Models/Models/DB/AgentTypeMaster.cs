
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<AgentTypeMaster>(entity =>
{
    entity.ToTable("AGENT_TYPE_MASTER", "hms");

    entity.HasOne(e => e.ChannelMaster)
          .WithMany()
          .HasForeignKey(e => e.ChannelCode)
          .HasConstraintName("fk_agent_type_channel")
          .OnDelete(DeleteBehavior.Restrict);
});

     */
    [Table("AGENT_TYPE_MASTER", Schema = "hms")]
    public class AgentTypeMaster
    {
        [Key]
        [Column("AGENT_TYPE_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Primary key: agent type code.")]
        public string AgentTypeCode { get; set; } = null!;

        [Required]
        [Column("AGENT_TYPE_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Name of the agent type.")]
        public string AgentTypeName { get; set; } = null!;

        [Column("CHANNEL_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Foreign key referencing channel code.")]
        public string? ChannelCode { get; set; }

        [Column("REGULATORY_CLASS")]
        [StringLength(50)]
        [SwaggerSchema("Regulatory classification of the agent type.")]
        public string? RegulatoryClass { get; set; }

        [Required]
        [Column("IS_DISTRIBUTION_PARTNER")]
        [SwaggerSchema("Indicates if this agent type is a distribution partner.")]
        public bool IsDistributionPartner { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the agent type is active.")]
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
