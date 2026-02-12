using Microsoft.EntityFrameworkCore;
using Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("bankaccount", Schema = "hms")]
    [Index(nameof(RefKey), nameof(RefType), IsUnique = true, Name = "ix_bankaccount_refkey_reftype")]
    public class BankAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("refkey")]
        public int RefKey { get; set; } = 0;

        /// <summary>
        /// 1 for Agent, etc.
        /// </summary>
        [Required]
        [Column("reftype")]
        public ReferenceType RefType { get; set; } = ReferenceType.Agent;

        [Required]
        [StringLength(200)]
        [Column("accountholdername")]
        public string AccountHolderName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        [Column("accountnumber")]
        public string AccountNumber { get; set; } = null!;

        [Required]
        [StringLength(20)]
        [Column("ifsc")]
        public string IFSC { get; set; } = null!;

        [Required]
        [StringLength(20)]
        [Column("micr")]
        public string MICR { get; set; } = null!;

        [StringLength(200)]
        [Column("bankname")]
        public string? BankName { get; set; }

        [StringLength(200)]
        [Column("branchname")]
        public string? BranchName { get; set; }

        [Required]
        [Column("accounttype")]
        public int AccountType { get; set; } = 1;

        [Column("activesince")]
        public DateTime? ActiveSince { get; set; } = DateTime.Now;

        [StringLength(200)]
        [Column("factoringhouse")]
        public string? FactoringHouse { get; set; }

        [Required]
        [Column("preferredpaymentmode")]
        public PreferredPaymentMode PreferredPaymentMode { get; set; } = PreferredPaymentMode.BankTransfer;

        [Column("orgid")]
        public int? OrgId { get; set; }

        // NotMapped property for UI/Description logic
        [NotMapped]
        public string? AccountTypeDesc { get; set; }
    }
}