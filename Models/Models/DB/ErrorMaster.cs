using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Index = Microsoft.EntityFrameworkCore.Metadata.Internal.Index;

namespace Models.DB
{
    [Table("errormaster", Schema = "hms")]
    [Index(nameof(ErrorId), nameof(Area), IsUnique = true)]
    public class ErrorMaster
    {
        [Key]
        [Column("error_id")]
        public int ErrorId { get; set; }

        [Required]
        [Column("area")]
        [StringLength(50)]
        public string Area { get; set; } = string.Empty;

        [Required]
        [Column("error_msg")]
        [StringLength(1000)]
        public string ErrorMsg { get; set; } = string.Empty;
    }
}
