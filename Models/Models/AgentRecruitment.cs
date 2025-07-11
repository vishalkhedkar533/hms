using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<AgentRecruitment>(entity =>
{
    entity.ToTable("AGENT_RECRUITMENT", "hms");

    entity.HasOne(e => e.Recruiter)
          .WithMany()
          .HasForeignKey(e => e.RecruiterId)
          .HasConstraintName("fk_recruiter")
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.Recruit)
          .WithMany()
          .HasForeignKey(e => e.RecruitId)
          .HasConstraintName("fk_recruit")
          .OnDelete(DeleteBehavior.Cascade);
});

     */
    [Table("AGENT_RECRUITMENT", Schema = "hms")]
    public class AgentRecruitment
    {
        [Key]
        [Column("RECRUITMENT_ID")]
        [SwaggerSchema("Primary key for the recruitment record.")]
        public long RecruitmentId { get; set; }

        [Required]
        [Column("RECRUITER_ID")]
        [SwaggerSchema("Reference to the recruiting agent.")]
        public int RecruiterId { get; set; }

        [Required]
        [Column("RECRUIT_ID")]
        [SwaggerSchema("Reference to the recruited agent.")]
        public int RecruitId { get; set; }

        [Required]
        [Column("RECRUITMENT_DATE", TypeName = "date")]
        [SwaggerSchema("Date when the recruitment happened.")]
        public DateTime RecruitmentDate { get; set; }

        [StringLength(20)]
        [Column("SOURCE_CHANNEL")]
        [SwaggerSchema("Channel from which the recruit was sourced.")]
        public string? SourceChannel { get; set; }

        [StringLength(1000)]
        [Column("COMMENTS")]
        [SwaggerSchema("Additional comments related to the recruitment.")]
        public string? Comments { get; set; }

        [Required]
        [StringLength(100)]
        [Column("CREATED_BY")]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Timestamp when the record was created.")]
        public DateTime CreatedDate { get; set; }

        [StringLength(100)]
        [Column("MODIFIED_BY")]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token.")]
        public int? RowVersion { get; set; }

        // Navigation properties
        [SwaggerSchema("The recruiting agent.")]
        [ForeignKey(nameof(RecruiterId))]
        public Agent? Recruiter { get; set; }

        [SwaggerSchema("The recruited agent.")]
        [ForeignKey(nameof(RecruitId))]
        public Agent? Recruit { get; set; }
    }
}
