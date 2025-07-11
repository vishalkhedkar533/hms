using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<BranchMaster>(entity =>
{
    entity.ToTable("BRANCH_MASTER", "hms");

    entity.HasOne(e => e.ChannelMaster)
          .WithMany()
          .HasForeignKey(e => e.ChannelCode)
          .HasConstraintName("fk_branch_channel")
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(e => e.HeadAgent)
          .WithMany()
          .HasForeignKey(e => e.HeadAgentId)
          .HasConstraintName("fk_branch_head_agent")
          .OnDelete(DeleteBehavior.Restrict);
});

     */
    [Table("BRANCH_MASTER", Schema = "hms")]
    public class BranchMaster
    {
        [Key]
        [Column("BRANCH_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Primary key: branch code.")]
        public string BranchCode { get; set; } = null!;

        [Required]
        [Column("BRANCH_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Name of the branch.")]
        public string BranchName { get; set; } = null!;

        [Column("REGION")]
        [StringLength(50)]
        [SwaggerSchema("Region of the branch.")]
        public string? Region { get; set; }

        [Column("CHANNEL_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Foreign key referencing channel code.")]
        public string? ChannelCode { get; set; }

        [Column("HEAD_AGENT_ID")]
        [StringLength(50)]
        [SwaggerSchema("Foreign key referencing the head agent code.")]
        public string? HeadAgentId { get; set; }

        [Column("ADDRESS")]
        [SwaggerSchema("Address of the branch.")]
        public string? Address { get; set; }

        [Column("PHONE_NUMBER")]
        [StringLength(20)]
        [SwaggerSchema("Phone number of the branch.")]
        public string? PhoneNumber { get; set; }

        [Column("EMAIL_ID")]
        [StringLength(100)]
        [SwaggerSchema("Email address of the branch.")]
        public string? EmailId { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the branch is active.")]
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

        // Navigation properties
        [ForeignKey(nameof(ChannelCode))]
        [SwaggerSchema("The related channel.")]
        public ChannelMaster? ChannelMaster { get; set; }

        [ForeignKey(nameof(HeadAgentId))]
        [SwaggerSchema("The head agent of the branch.")]
        public Agent? HeadAgent { get; set; }
    }
}
