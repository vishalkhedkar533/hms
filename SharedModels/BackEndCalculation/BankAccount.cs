using Microsoft.EntityFrameworkCore;
using SharedModels.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.BackEndCalculation
{
    [Table("BankAccount", Schema = "hms")]
    [Index(nameof(RefKey), nameof(RefType), IsUnique = true)]
    public class BankAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [Column]
        public int RefKey { get; set; }
        [Required]
        [Column]
        public ReferenceType RefType { get; set; } 
        [Required]
        [MaxLength(200)]
        public string AccountHolderName { get; set; } = null!;
        [Required]
        [MaxLength(50)]
        public string AccountNumber { get; set; } = null!;
        [Required]
        [MaxLength(20)]
        public string IFSC { get; set; } = null!;
        [MaxLength(20)]
        public string? MICR { get; set; } = null!;
        [MaxLength(200)]
        public string? BankName { get; set; }
        [MaxLength(200)]
        public string? BranchName { get; set; }
        [Required]
        public int AccountType { get; set; } = 1;
        [Column]
        public DateTime? ActiveSince { get; set; } = DateTime.Now;
        [MaxLength(200)]
        public string? FactoringHouse { get; set; }
        [Required]
        public PreferredPaymentMode PreferredPaymentMode { get; set; }
        [NotMapped]
        public string? AccountTypeDesc { get; set; }
    }

}