namespace Models.DB
{
    /// <summary>
    /// Represents a role master record.
    /// </summary>
    //[Table("ROLE_MASTER", Schema = "hms")]
    //public class RoleMaster
    //{
    //    [Key]
    //    [Column("ROLE_ID")]
    //    [StringLength(50)]
    //    [SwaggerSchema("Primary key: role id")]
    //    public long ROLE_ID { get; set; }
    
    //    [Required]
    //    [Column("ROLE_NAME")]
    //    [StringLength(100)]
    //    [SwaggerSchema("Name of the role.")]
    //    public string RoleName { get; set; } = null!;

    //    [Column("DESCRIPTION", TypeName = "text")]
    //    [SwaggerSchema("Description of the role.")]
    //    public string? Description { get; set; }

    //    [Required]
    //    [Column("IS_SYSTEM_ROLE")]
    //    [SwaggerSchema("Indicates if this is a system role.")]
    //    public bool IsSystemRole { get; set; }

    //    [Required]
    //    [Column("IS_ACTIVE")]
    //    [SwaggerSchema("Indicates if the role is active.")]
    //    public bool IsActive { get; set; }

    //    [Required]
    //    [Column("CREATED_BY")]
    //    [StringLength(100)]
    //    [SwaggerSchema("User who created the record.")]
    //    public string CreatedBy { get; set; } = null!;

    //    [Required]
    //    [Column("CREATED_DATE")]
    //    [SwaggerSchema("Date and time when the record was created.")]
    //    public DateTime CreatedDate { get; set; }

    //    [Column("MODIFIED_BY")]
    //    [StringLength(100)]
    //    [SwaggerSchema("User who last modified the record.")]
    //    public string? ModifiedBy { get; set; }

    //    [Column("MODIFIED_DATE")]
    //    [SwaggerSchema("Date and time of last modification.")]
    //    public DateTime? ModifiedDate { get; set; }

    //    [Column("ROWVERSION")]
    //    [ConcurrencyCheck]
    //    [SwaggerSchema("Concurrency token for optimistic concurrency control.")]
    //    public int? RowVersion { get; set; }
    //}

}
