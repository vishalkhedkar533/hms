using Microsoft.EntityFrameworkCore;
using Models.HMSConsts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{

    [Index(nameof(RefKey), nameof(RefType), nameof(AddressType), IsUnique = true)]
    [Table("Address", Schema = "hms")]
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Since it's not SERIAL/IDENTITY in SQL
        public long AddressID { get; set; }

        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public int AddressType { get; set; } //enum AddressType

        [Required]
        [Column(TypeName = "integer")]
        public int RefKey { get; set; } = 0;

        [Column(TypeName = "integer")]
        public int? RefType { get; set; }//enum RefType

        [Required]
        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string AddressLine1 { get; set; } = null!;

        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string? AddressLine2 { get; set; }

        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string? AddressLine3 { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string City { get; set; } = null!;

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? State { get; set; }

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? Country { get; set; }

        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string? PIN { get; set; }

        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string? Landmark { get; set; }

        // Define unique constraint (RefKey, RefType, AddressType) via Fluent API in DbContext
    }
}
