using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /// <summary>
    /// Represents a channel master record.
    /// </summary>
    [Table("CHANNEL_MASTER", Schema = "hms")]
    public class ChannelMaster
    {
        [Key]
        [Column("CHANNEL_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Primary key: channel code.")]
        public string ChannelCode { get; set; } = null!;

        [Required]
        [Column("CHANNEL_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Name of the channel.")]
        public string ChannelName { get; set; } = null!;

        [Column("DESCRIPTION", TypeName = "text")]
        [SwaggerSchema("Description of the channel.")]
        public string? Description { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates if the channel is active.")]
        public bool IsActive { get; set; }

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Date and time the record was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Date and time of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
        public int? RowVersion { get; set; }
        [NotMapped]
        public Int64? TotalEntities { get; set; } = 100;
        [NotMapped]
        public Int64? CreatedEntities { get; set; } = 200;
        [NotMapped]
        public Int64? TerminatedEntities { get; set; } = 300;

    }
}
