using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /// <summary>
    /// Represents a subchannel master
    /// </summary>
    [Table("subchannel_master", Schema = "hmsmaster")]
    public class SubChannelMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("sub_channel_id")]
        [SwaggerSchema("Primary key: unique identifier for the subchannel.")]
        public long SubChannelId { get; set; }

        [Required]
        [StringLength(20)]
        [Column("subchannel_code")]
        [SwaggerSchema("Unique code for the subchannel.")]
        public string SubChannelCode { get; set; } = null!;

        [Required]
        [StringLength(20)]
        [Column("channel_code")]
        [SwaggerSchema("Code of the parent channel.")]
        public string ChannelCode { get; set; } = null!;

        [Required]
        [StringLength(100)]
        [Column("subchannel_name")]
        [SwaggerSchema("Name of the subchannel.")]
        public string SubChannelName { get; set; } = null!;

        [Column("description", TypeName = "text")]
        [SwaggerSchema("Description of the subchannel.")]
        public string? Description { get; set; }

        [Required]
        [Column("is_active")]
        [SwaggerSchema("Indicates if the subchannel is active.")]
        public bool IsActive { get; set; }

        [Required]
        [StringLength(100)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [StringLength(100)]
        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [ConcurrencyCheck]
        [Column("rowversion")]
        [SwaggerSchema("Concurrency token (int4).")]
        public int? RowVersion { get; set; }

        [Column("orgid")]
        [SwaggerSchema("Organization ID reference.")]
        public int? OrgId { get; set; }

        [Column("channel_id")]
        [SwaggerSchema("Foreign key referencing the channel master ID.")]
        public long? ChannelId { get; set; }

        // Navigation Property
        [ForeignKey("ChannelId")]
        public virtual ChannelMaster? Channel { get; set; }
    }
    public class SubChannelMasterDto
    {
        public long? SubChannelId { get; set; }
        [StringLength(20)]
        public string? SubChannelCode { get; set; } = null!;
        [StringLength(20)]
        public string? ChannelCode { get; set; } = null!;
        [StringLength(100)]
        public string? SubChannelName { get; set; } = null!;
        public string? Description { get; set; }
        public bool? IsActive { get; set; } = true;
        public long? ChannelId { get; set; }
    }
}