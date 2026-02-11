namespace Models.DB
{
    /// <summary>
    /// Represents a channel master record for the Hierarchy Management System.
    /// </summary>
    //[Table("channel_master", Schema = "hmsmaster")]
    //public class ChannelMaster
    //{
    //    [Key]
    //    [Column("channel_id")]
    //    [SwaggerSchema("Primary key: unique identifier for the channel.")]
    //    public long ChannelId { get; set; } // Changed to long to match int8

    //    [Column("channel_code")]
    //    [Required]
    //    [StringLength(20)]
    //    [SwaggerSchema("Unique code for the channel.")]
    //    public string ChannelCode { get; set; } = null!;

    //    [Required]
    //    [Column("channel_name")]
    //    [StringLength(100)]
    //    [SwaggerSchema("Name of the channel.")]
    //    public string ChannelName { get; set; } = null!;

    //    [Column("description", TypeName = "text")]
    //    [SwaggerSchema("Description of the channel.")]
    //    public string? Description { get; set; }

    //    [Required]
    //    [Column("is_active")]
    //    [SwaggerSchema("Indicates if the channel is active.")]
    //    public bool IsActive { get; set; }

    //    [Required]
    //    [Column("orgid")]
    //    [SwaggerSchema("Organization ID reference.")]
    //    public int OrgId { get; set; } // Added to match DB script

    //    [Required]
    //    [Column("created_by")]
    //    [StringLength(100)]
    //    [SwaggerSchema("User who created the record.")]
    //    public string CreatedBy { get; set; } = "System";

    //    [Required]
    //    [Column("created_date")]
    //    [SwaggerSchema("Date and time the record was created.")]
    //    public DateTime CreatedDate { get; set; } = DateTime.Now;

    //    [Column("modified_by")]
    //    [StringLength(100)]
    //    [SwaggerSchema("User who last modified the record.")]
    //    public string? ModifiedBy { get; set; }

    //    [Column("modified_date")]
    //    [SwaggerSchema("Date and time of the last modification.")]
    //    public DateTime? ModifiedDate { get; set; }

    //    [Column("rowversion")]
    //    [ConcurrencyCheck]
    //    [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
    //    public int? RowVersion { get; set; }

    //    #region NotMapped Properties for UI/Dashboard

    //    [NotMapped]
    //    public long? TotalEntities { get; set; } = 100;

    //    [NotMapped]
    //    public long? CreatedEntities { get; set; } = 200;

    //    [NotMapped]
    //    public long? TerminatedEntities { get; set; } = 300;

    //    #endregion
    //}

}