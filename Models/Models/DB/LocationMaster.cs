using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    // Note: Updated Schema to "hmsmaster" to match your SQL Script
    [Table("location_master", Schema = "hmsmaster")]
    public class LocationMaster
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("location_master_id")]
        [SwaggerSchema("Identity key for location master.")]
        public long LocationMasterId { get; set; }

        [Required]
        [Column("channel_id")]
        [SwaggerSchema("Foreign key to channel master.")]
        public long ChannelId { get; set; }

        [Column("sub_channel_id")]
        [SwaggerSchema("Foreign key to channel master.")]
        public long SubChannelId { get; set; }

        [Column("orgid")]
        [SwaggerSchema("Organization identifier.")]
        public int? OrgId { get; set; }

        [Key] // Kept as key since your SQL defines location_code as the Primary Key
        [Column("location_code")]
        [StringLength(20)]
        [SwaggerSchema("Primary key: location code.")]
        public string LocationCode { get; set; } = null!;

        [Required]
        [Column("location_desc")]
        [StringLength(100)]
        [SwaggerSchema("Description of the location.")]
        public string LocationDesc { get; set; } = null!;

        [Required]
        [Column("is_active")]
        [SwaggerSchema("Indicates if the location is active.")]
        public bool IsActive { get; set; }

        [Required]
        [Column("created_by")]
        [StringLength(100)]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("created_date")]
        [SwaggerSchema("Date and time when the record was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("modified_by")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        [SwaggerSchema("Date and time of last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token.")]
        public int? RowVersion { get; set; }

        /* Note: The SQL script you provided does NOT have PARENT_LOCATION_CODE. 
           In your architecture, hierarchy is managed via the 'channel_location_heirarchy' table.
           If you want to keep the self-reference, you must add the column to your DB first.
        */
    }
}