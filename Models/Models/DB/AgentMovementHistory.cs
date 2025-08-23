using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<AgentMovementHistory>(entity =>
{
    entity.ToTable("AGENT_MOVEMENT_HISTORY", "hms");

    entity.HasOne(e => e.Agent)
          .WithMany()
          .HasForeignKey(e => e.AgentId)
          .HasConstraintName("fk_agent")
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.OldSupervisor)
          .WithMany()
          .HasForeignKey(e => e.OldSupervisorCode)
          .HasConstraintName("fk_old_supervisor")
          .OnDelete(DeleteBehavior.SetNull);

    entity.HasOne(e => e.NewSupervisor)
          .WithMany()
          .HasForeignKey(e => e.NewSupervisorCode)
          .HasConstraintName("fk_new_supervisor")
          .OnDelete(DeleteBehavior.Cascade);
});

     */
    [Table("AGENT_MOVEMENT_HISTORY", Schema = "hms")]
    public class AgentMovementHistory
    {
        [Key]
        [Column("MOVEMENT_ID")]
        [SwaggerSchema("Primary key for agent movement history.")]
        public long MovementId { get; set; }

        [Required]
        [Column("AGENT_ID")]
        [SwaggerSchema("Reference to the agent.")]
        public int AgentId { get; set; }

        [Column("OLD_SUPERVISOR_CODE")]
        [SwaggerSchema("Old supervisor agent ID.")]
        public int? OldSupervisorCode { get; set; }

        [Required]
        [Column("NEW_SUPERVISOR_CODE")]
        [SwaggerSchema("New supervisor agent ID.")]
        public int NewSupervisorCode { get; set; }

        [Column("OLD_DESIGNATION_CODE")]
        [SwaggerSchema("Old designation code.")]
        public int? OldDesignationCode { get; set; }

        [Required]
        [Column("NEW_DESIGNATION_CODE")]
        [SwaggerSchema("New designation code.")]
        public int NewDesignationCode { get; set; }

        [Required]
        [StringLength(50)]
        [Column("MOVEMENT_TYPE")]
        [SwaggerSchema("Type of movement.")]
        public string MovementType { get; set; } = null!;

        [StringLength(20)]
        [Column("REASON_CODE")]
        [SwaggerSchema("Reason code for the movement.")]
        public string? ReasonCode { get; set; }

        [Required]
        [Column("EFFECTIVE_FROM_DATE", TypeName = "date")]
        [SwaggerSchema("Effective start date of the movement.")]
        public DateTime EffectiveFromDate { get; set; }

        [Column("EFFECTIVE_TO_DATE", TypeName = "date")]
        [SwaggerSchema("Effective end date of the movement.")]
        public DateTime? EffectiveToDate { get; set; }

        [StringLength(1000)]
        [Column("REMARKS")]
        [SwaggerSchema("Additional remarks.")]
        public string? Remarks { get; set; }

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
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the movement is active.")]
        public bool IsActive { get; set; } = true;
        [StringLength(100)]
        [Column("APPROVED_BY")]
        [SwaggerSchema("User ID of the approver.")]
        public string? ApprovedBy { get; set; }

        [Column("APPROVED_DATE")]
        [SwaggerSchema("Timestamp of approval.")]
        public DateTime? ApprovedDate { get; set; }

        [StringLength(100)]
        [Column("REJECTED_BY")]
        [SwaggerSchema("User ID of the rejector.")]
        public string? RejectedBy { get; set; }

        [Column("REJECTED_DATE")]
        [SwaggerSchema("Timestamp of rejection.")]
        public DateTime? RejectedDate { get; set; }
        [StringLength(20)]

        [Column("STATUS")]
        [SwaggerSchema("Approval status: Pending, Approved, Rejected")]
        public string Status { get; set; } = "Pending";

        // Navigation properties
        [SwaggerSchema("Reference to the agent.")]
        public Agent? Agent { get; set; }

        [ForeignKey(nameof(OldSupervisorCode))]
        [SwaggerSchema("Reference to old supervisor agent.")]
        public Agent? OldSupervisor { get; set; }

        [ForeignKey(nameof(NewSupervisorCode))]
        [SwaggerSchema("Reference to new supervisor agent.")]
        public Agent? NewSupervisor { get; set; }

    }
}
