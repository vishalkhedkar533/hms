using Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("BankAccount", Schema = "hms")]
    public class BankAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "integer")]
        public int RefKey { get; set; } = 0;
        [Required]
        [Column(TypeName = "integer")]
        public int RefType { get; set; } = 1;
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
        [Column(TypeName = "timestamp")]
        public DateTime? ActiveSince { get; set; } = DateTime.Now;
        [MaxLength(200)]
        public string? FactoringHouse { get; set; }
        [Required]
        public PreferredPaymentMode PreferredPaymentMode { get; set; } = 1;
    }
}
