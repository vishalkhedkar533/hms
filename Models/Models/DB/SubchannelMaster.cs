using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /// <summary>
    /// Represents a subchannel master
    /// </summary>
    [Table("subchannel_master", Schema = "hmsmaster")]
    public class SubchannelMaster
    {
        [Key]
        [Column("sub_channel_id")]
        [SwaggerSchema("Primary key: unique identifier for the subchannel.")]
        public long SubChannelId { get; set; } // Matches int8 primary key

        [Required]
        [Column("subchannel_code")]
        [StringLength(20)]
        [SwaggerSchema("Unique code for the subchannel.")]
        public string SubchannelCode { get; set; } = null!;

        [Required]
        [Column("channel_code")]
        [StringLength(20)]
        [SwaggerSchema("Code of the parent channel.")]
        public string ChannelCode { get; set; } = null!;

        [Required]
        [Column("subchannel_name")]
        [StringLength(100)]
        [SwaggerSchema("Name of the subchannel.")]
        public string SubchannelName { get; set; } = null!;

        [Column("description", TypeName = "text")]
        [SwaggerSchema("Description of the subchannel.")]
        public string? Description { get; set; }

        [Required]
        [Column("is_active")]
        [SwaggerSchema("Indicates if the subchannel is active.")]
        public bool IsActive { get; set; }

        [Required]
        [Column("orgid")]
        [SwaggerSchema("Organization ID reference.")]
        public int? OrgId { get; set; } 

        [Column("channel_id")]
        [SwaggerSchema("Foreign key referencing the channel master ID.")]
        public long? ChannelId { get; set; }

        [Required]
        [Column("created_by")]
        [StringLength(100)]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = "System";

        [Required]
        [Column("created_date")]
        [SwaggerSchema("Date and time the record was created.")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Column("modified_by")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        [SwaggerSchema("Date and time of last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        // Navigation property
        [ForeignKey(nameof(ChannelId))]
        [SwaggerSchema("The related channel master entity.")]
        public virtual ChannelMaster? ChannelMaster { get; set; }
    }
}