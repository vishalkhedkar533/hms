using Microsoft.EntityFrameworkCore;
using SharedModels.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.BackEndCalculation
{
    [Index(nameof(RefKey), nameof(RefType), IsUnique = true)]
    [Table("Nominee", Schema = "hms")]
    public class Nominee
    {
        [Key]
        public int NomineeID { get; set; }

        [Required]
        public int RefKey { get; set; }

        public ReferenceType? RefType { get; set; }

        [Required]
        [MaxLength(255)]
        public string? NomineeName { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Relationship { get; set; }

        [Column]
        public decimal PercentageShare { get; set; }

        public bool IsActive { get; set; }

        public long NomineeAge { get; set; }
    }
}
