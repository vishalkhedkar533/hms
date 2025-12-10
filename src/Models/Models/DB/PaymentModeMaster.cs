using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /// <summary>
    /// Master data for available payment modes (e.g., NEFT, UPI, CHEQUE, WALLET).
    /// </summary>
    [Table("PAYMENT_MODE_MASTER", Schema = "hms")]
    public class PaymentModeMaster
    {
        [Key]
        [Column("PAYMENT_MODE_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Unique code representing the payment mode (e.g., NEFT, UPI).")]
        public string PaymentModeCode { get; set; } = null!;

        [Required]
        [Column("PAYMENT_MODE_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Name or label of the payment mode to be shown on UI.")]
        public string PaymentModeName { get; set; } = null!;

        [Column("DESCRIPTION")]
        [StringLength(255)]
        [SwaggerSchema("Optional description or usage notes.")]
        public string? Description { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the payment mode is currently active.")]
        public bool IsActive { get; set; }

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Timestamp when the record was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp when the record was last modified.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [SwaggerSchema("Used for optimistic concurrency control.")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }
    }
}
