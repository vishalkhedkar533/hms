using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<AgentLicense>(entity =>
{
    entity.ToTable("AGENT_LICENSE", "hms");

    entity.HasOne(e => e.Agent)
          .WithMany()
          .HasForeignKey(e => e.AgentId)
          .HasConstraintName("fk_agent")
          .OnDelete(DeleteBehavior.Cascade);
});

     */
    [Table("AGENT_LICENSE", Schema = "hms")]
    public class AgentLicense
    {
        [Key]
        [Column("ID")]
        [SwaggerSchema("Primary key for the agent license record.")]
        public long Id { get; set; }

        [Required]
        [Column("AGENT_ID")]
        [SwaggerSchema("Reference to the associated agent.")]
        public int AgentId { get; set; }

        [Required]
        [StringLength(60)]
        [Column("LICENSE_NO")]
        [SwaggerSchema("License number.")]
        public string LicenseNo { get; set; } = null!;

        [Required]
        [StringLength(20)]
        [Column("LICENSE_TYPE_CODE")]
        [SwaggerSchema("Type code of the license.")]
        public string LicenseTypeCode { get; set; } = null!;

        [Required]
        [Column("EFFECTIVE_FROM_DATE", TypeName = "date")]
        [SwaggerSchema("Date when the license becomes effective.")]
        public DateTime EffectiveFromDate { get; set; }

        [Column("EFFECTIVE_TO_DATE", TypeName = "date")]
        [SwaggerSchema("Date when the license expires (if any).")]
        public DateTime? EffectiveToDate { get; set; }

        [StringLength(40)]
        [Column("LICENSE_STATUS")]
        [SwaggerSchema("Status of the license.")]
        public string? LicenseStatus { get; set; }

        [Required]
        [StringLength(100)]
        [Column("CREATED_BY")]
        [SwaggerSchema("User who created the license record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Timestamp when the license record was created.")]
        public DateTime CreatedDate { get; set; }

        [StringLength(100)]
        [Column("MODIFIED_BY")]
        [SwaggerSchema("User who last modified the license record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        // Navigation property
        [SwaggerSchema("Reference to the associated agent.")]
        public Agent? Agent { get; set; }
    }
}
