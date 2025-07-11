using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<AgentBankDetails>(entity =>
{
    entity.ToTable("AGENT_BANK_DETAILS", "hms");

    entity.HasOne(e => e.Agent)
          .WithMany()
          .HasForeignKey(e => e.AgentId)
          .HasConstraintName("fk_agent")
          .OnDelete(DeleteBehavior.Cascade);
});

     */
    [Table("AGENT_BANK_DETAILS", Schema = "hms")]
    public class AgentBankDetails
    {
        [Key]
        [Column("AGENT_BANK_DETAILS_ID")]
        [SwaggerSchema("Primary key for agent bank details.")]
        public int AgentBankDetailsId { get; set; }

        [Required]
        [Column("AGENT_ID")]
        [SwaggerSchema("Reference to the associated agent.")]
        public int AgentId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("BANK_NAME")]
        [SwaggerSchema("Name of the bank.")]
        public string BankName { get; set; } = null!;

        [Required]
        [StringLength(100)]
        [Column("ACCOUNT_HOLDER_NAME")]
        [SwaggerSchema("Name of the account holder.")]
        public string AccountHolderName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        [Column("ACCOUNT_NUMBER")]
        [SwaggerSchema("Bank account number.")]
        public string AccountNumber { get; set; } = null!;

        [StringLength(20)]
        [Column("ACCOUNT_TYPE")]
        [SwaggerSchema("Type of bank account.")]
        public string? AccountType { get; set; }

        [StringLength(20)]
        [Column("IFSC_CODE")]
        [SwaggerSchema("IFSC code of the bank branch.")]
        public string? IFSCCode { get; set; }

        [StringLength(100)]
        [Column("BRANCH_NAME")]
        [SwaggerSchema("Name of the bank branch.")]
        public string? BranchName { get; set; }

        [StringLength(20)]
        [Column("MICR_CODE")]
        [SwaggerSchema("MICR code of the bank branch.")]
        public string? MICRCode { get; set; }

        [Required]
        [Column("IS_PREFERRED")]
        [SwaggerSchema("Indicates if this is the preferred bank account.")]
        public bool IsPreferred { get; set; }

        [Required]
        [Column("EFFECTIVE_FROM_DATE", TypeName = "date")]
        [SwaggerSchema("Start date of the bank details validity.")]
        public DateTime EffectiveFromDate { get; set; }

        [Column("EFFECTIVE_TO_DATE", TypeName = "date")]
        [SwaggerSchema("End date of the bank details validity.")]
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
