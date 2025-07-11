using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<AgentPaymentMethod>(entity =>
{
    entity.ToTable("AGENT_PAYMENT_METHOD", "hms");

    entity.HasOne(e => e.Agent)
          .WithMany()
          .HasForeignKey(e => e.AgentId)
          .HasConstraintName("fk_agent")
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.BankDetails)
          .WithMany()
          .HasForeignKey(e => e.BankDetailId)
          .HasConstraintName("fk_bank_details")  // You can name this constraint as needed
          .OnDelete(DeleteBehavior.SetNull);
});

     */
    [Table("AGENT_PAYMENT_METHOD", Schema = "hms")]
    public class AgentPaymentMethod
    {
        [Key]
        [Column("ID")]
        [SwaggerSchema("Primary key for the agent payment method.")]
        public long Id { get; set; }

        [Required]
        [Column("AGENT_ID")]
        [SwaggerSchema("Reference to the associated agent.")]
        public int AgentId { get; set; }

        [Required]
        [StringLength(20)]
        [Column("PAYMENT_MODE_CODE")]
        [SwaggerSchema("Code representing the payment mode.")]
        public string PaymentModeCode { get; set; } = null!;

        [Column("BANK_DETAIL_ID")]
        [SwaggerSchema("Reference to the bank details if applicable.")]
        public long? BankDetailId { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the payment method is active.")]
        public bool IsActive { get; set; }

        [Required]
        [Column("EFFECTIVE_FROM_DATE", TypeName = "date")]
        [SwaggerSchema("Date from which the payment method is effective.")]
        public DateTime EffectiveFromDate { get; set; }

        [Column("EFFECTIVE_TO_DATE", TypeName = "date")]
        [SwaggerSchema("Date until which the payment method is effective.")]
        public DateTime? EffectiveToDate { get; set; }

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
        [SwaggerSchema("Reference to the associated agent.")]
        public Agent? Agent { get; set; }

        [SwaggerSchema("Reference to the associated bank details.")]
        public AgentBankDetails? BankDetails { get; set; }
    }
}
