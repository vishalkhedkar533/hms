using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /// <summary>
    /// Represents a channel master record.
    /// </summary>
    [Table("channel_master", Schema = "hms")]
    public class ChannelMaster
    {
        [Key]
        [Column("channel_code")]
        [StringLength(20)]
        [SwaggerSchema("Primary key: channel code.")]
        public string ChannelCode { get; set; } = null!;

        [Required]
        [Column("channel_name")]
        [StringLength(100)]
        [SwaggerSchema("Name of the channel.")]
        public string ChannelName { get; set; } = null!;

        [Column("description", TypeName = "text")]
        [SwaggerSchema("Description of the channel.")]
        public string? Description { get; set; }

        [Required]
        [Column("is_active")]
        [SwaggerSchema("Indicates if the channel is active.")]
        public bool IsActive { get; set; }

        [Required]
        [Column("created_by")]
        [StringLength(100)]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("created_date")]
        [SwaggerSchema("Date and time the record was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("modified_by")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        [SwaggerSchema("Date and time of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
        public int? RowVersion { get; set; }
        [NotMapped]
        public long? TotalEntities { get; set; } = 100;
        [NotMapped]
        public long? CreatedEntities { get; set; } = 200;
        [NotMapped]
        public long? TerminatedEntities { get; set; } = 300;
    }
}