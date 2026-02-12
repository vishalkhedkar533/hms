using SharedModels.BackEndCalculation;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("branch_master", Schema = "hmsmaster")]
    public class BranchMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("branch_id")]
        [SwaggerSchema("Primary key: unique branch identifier.")]
        public long BranchId { get; set; }

        [Column("orgid")]
        [SwaggerSchema("Foreign key referencing the organisation.")]
        public int? OrgId { get; set; }

        [Required]
        [Column("branch_code")]
        [StringLength(20)]
        [SwaggerSchema("Unique code for the branch within an organization.")]
        public string BranchCode { get; set; } = null!;

        [Required]
        [Column("branch_name")]
        [StringLength(100)]
        [SwaggerSchema("Name of the branch.")]
        public string BranchName { get; set; } = null!;

        [Column("address")]
        [SwaggerSchema("Physical address of the branch.")]
        public string? Address { get; set; }

        [Column("state")]
        [SwaggerSchema("State identifier.")]
        public int? State { get; set; }

        [Column("phone_number")]
        [StringLength(20)]
        [SwaggerSchema("Contact phone number.")]
        public string? PhoneNumber { get; set; }

        [Column("email_id")]
        [StringLength(100)]
        [SwaggerSchema("Contact email address.")]
        public string? EmailId { get; set; }

        [Required]
        [Column("is_active")]
        [SwaggerSchema("Indicates if the branch is active.")]
        public bool IsActive { get; set; }

        [Column("location_master_id")]
        [SwaggerSchema("Foreign key referencing the location master.")]
        public long? LocationMasterId { get; set; }

        [Required]
        [Column("created_by")]
        [StringLength(100)]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("created_date")]
        [SwaggerSchema("Timestamp when the record was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("modified_by")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        [ConcurrencyCheck]
        [SwaggerSchema("Concurrency token.")]
        public int? RowVersion { get; set; }

        // --- Navigation Properties ---

        [ForeignKey(nameof(OrgId))]
        public virtual Organisation? Organisation { get; set; }

        [ForeignKey(nameof(LocationMasterId))]
        public virtual LocationMaster? LocationMaster { get; set; }
    }
}