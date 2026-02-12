using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.BackEndCalculation
{
    /// <summary>
    /// Represents a designation 
    /// </summary>
    [Table("designation_master", Schema = "hmsmaster")]
    public class DesignationMaster
    {
        [Key]
        [Column("designation_id")]
        [SwaggerSchema("Primary key: unique identifier for the designation.")]
        public long DesignationId { get; set; } 

        [Required]
        [Column("designation_code")]
        [StringLength(20)]
        [SwaggerSchema("Unique code for the designation.")]
        public string DesignationCode { get; set; } = null!;

        [Required]
        [Column("designation_name")]
        [StringLength(100)]
        [SwaggerSchema("Name of the designation.")]
        public string DesignationName { get; set; } = null!;
        [Column("designation_level")]
        [SwaggerSchema("Hierarchy level of the designation.")]
        public int? DesignationLevel { get; set; } // Updated name to match SQL

        [Required]
        [Column("is_active")]
        [SwaggerSchema("Indicates if the designation is active.")]
        public bool IsActive { get; set; }

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
        [SwaggerSchema("Date and time of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        [Column("channel_id")]
        [SwaggerSchema("Foreign key referencing the channel master ID.")]
        public long? ChannelId { get; set; }

        [Column("orgid")]
        [SwaggerSchema("Organization ID reference.")]
        public int? OrgId { get; set; }
        public string? HierarchyPath { get; set; } // ltree handled as string
        public string? CodeFormat { get; set; }
    }
}