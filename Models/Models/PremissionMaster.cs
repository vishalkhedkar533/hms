using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class PremissionMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [SwaggerSchema("Primary key of the permission")]
        public int PERMISSION_ID { get; set; }

        [Required]
        [StringLength(100)]
        [SwaggerSchema("Name of the permission")]
        public string PERMISSION_NAME { get; set; }

        [StringLength(255)]
        [SwaggerSchema("Description of the permission")]
        public string? DESCRIPTION { get; set; }

        [StringLength(50)]
        [SwaggerSchema("Module name associated with the permission")]
        public string? MODULE_NAME { get; set; }

        [Required]
        [SwaggerSchema("Indicates if the permission is active")]
        public bool IS_ACTIVE { get; set; }

        [Required]
        [StringLength(100)]
        [SwaggerSchema("User who created the permission")]
        public string CREATED_BY { get; set; }

        [Required]
        [SwaggerSchema("Date and time when the permission was created")]
        public DateTime CREATED_DATE { get; set; }

        [StringLength(100)]
        [SwaggerSchema("User who last modified the permission")]
        public string? MODIFIED_BY { get; set; }

        [SwaggerSchema("Date and time when the permission was last modified")]
        public DateTime? MODIFIED_DATE { get; set; }

        [SwaggerSchema("Row version for concurrency control")]
        public int? ROWVERSION { get; set; }
    }
}
