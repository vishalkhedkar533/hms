using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Models.DB
{
    /// <summary>
    /// Represents a role master record.
    /// </summary>
    //[Table("role_master", Schema = "hms")]
    //public class RoleMaster
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    [Column("role_id")]
    //    [SwaggerSchema("Primary key: role id")]
    //    public long Role_ID { get; set; }

    //    [Required]
    //    [Column("role_name")]
    //    [SwaggerSchema("Name of the role.")]
    //    public string RoleName { get; set; } = null!;

    //    [Column("description", TypeName = "text")]
    //    [SwaggerSchema("Description of the role.")]
    //    public string? Description { get; set; }

    //    [Required]
    //    [Column("is_system_role")]
    //    [SwaggerSchema("Indicates if this is a system role.")]
    //    public bool IsSystemRole { get; set; }

    //    [Required]
    //    [Column("is_active")]
    //    [SwaggerSchema("Indicates if the role is active.")]
    //    public bool IsActive { get; set; }

    //    [Required]
    //    [Column("created_by")]
    //    [SwaggerSchema("User who created the record.")]
    //    public string CreatedBy { get; set; } = null!;

    //    [Required]
    //    [Column("created_date")]
    //    [SwaggerSchema("Date and time when the record was created.")]
    //    public DateTime CreatedDate { get; set; }

    //    [Column("modified_by")]
    //    [SwaggerSchema("User who last modified the record.")]
    //    public string? ModifiedBy { get; set; }

    //    [Column("modified_date")]
    //    [SwaggerSchema("Date and time of last modification.")]
    //    public DateTime? ModifiedDate { get; set; }

    //    [Column("rowversion")]
    //    [ConcurrencyCheck]
    //    [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
    //    public int? RowVersion { get; set; }
    //}

    public class RoleSaveDto
    {
        [SwaggerSchema("Only required for Updates. Leave null for New roles.")]
        public long? Role_ID { get; set; }

        [Required(ErrorMessage = "Role name is mandatory")]
        [StringLength(100)]
        public string RoleName { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsSystemRole { get; set; }

        public bool IsActive { get; set; }

        [SwaggerSchema("Concurrency token for updates")]
        public int? RowVersion { get; set; }
    }

}
