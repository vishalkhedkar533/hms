using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<Agent>(entity =>
{
    entity.ToTable("AGENT", "hms");

    entity.HasIndex(e => e.AgentCode).IsUnique();

    entity.HasOne(e => e.Supervisor)
          .WithMany()
          .HasForeignKey(e => e.SupervisorCode)
          .HasConstraintName("fk_supervisor")
          .OnDelete(DeleteBehavior.SetNull);
});

     */
    [Table("AGENT", Schema = "hms")]
    public class Agent
    {
        [Key]
        [Column("AGENT_ID")]
        [SwaggerSchema("Primary key for the agent.")]
        public int AgentId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("AGENT_CODE")]
        [SwaggerSchema("Unique code identifying the agent.")]
        public string AgentCode { get; set; } = null!;

        [StringLength(20)]
        [Column("AGENT_TYPE_CODE")]
        [SwaggerSchema("Agent type code.")]
        public string? AgentTypeCode { get; set; }

        [StringLength(20)]
        [Column("AGENT_SUB_TYPE_CODE")]
        [SwaggerSchema("Agent sub-type code.")]
        public string? AgentSubTypeCode { get; set; }

        [StringLength(150)]
        [Column("AGENT_NAME")]
        [SwaggerSchema("Full name or registered name of the agent.")]
        public string? AgentName { get; set; }

        [StringLength(300)]
        [Column("BUSINESS_NAME")]
        [SwaggerSchema("Business or trade name of the agent.")]
        public string? BusinessName { get; set; }

        [StringLength(150)]
        [Column("FIRST_NAME")]
        [SwaggerSchema("First name of the agent.")]
        public string? FirstName { get; set; }

        [StringLength(150)]
        [Column("MIDDLE_NAME")]
        [SwaggerSchema("Middle name of the agent.")]
        public string? MiddleName { get; set; }

        [StringLength(150)]
        [Column("LAST_NAME")]
        [SwaggerSchema("Last name of the agent.")]
        public string? LastName { get; set; }

        [StringLength(20)]
        [Column("PREFIX")]
        [SwaggerSchema("Name prefix (e.g., Mr., Dr.).")]
        public string? Prefix { get; set; }

        [StringLength(20)]
        [Column("SUFFIX")]
        [SwaggerSchema("Name suffix (e.g., Jr., Sr.).")]
        public string? Suffix { get; set; }

        [StringLength(10)]
        [Column("GENDER")]
        [SwaggerSchema("Gender of the agent.")]
        public string? Gender { get; set; }

        [Column("DOB", TypeName = "date")]
        [SwaggerSchema("Date of birth of the agent.")]
        public DateTime? DOB { get; set; }

        [StringLength(40)]
        [Column("NATIONALITY")]
        [SwaggerSchema("Nationality of the agent.")]
        public string? Nationality { get; set; }

        [StringLength(20)]
        [Column("MARITAL_STATUS_CODE")]
        [SwaggerSchema("Marital status code.")]
        public string? MaritalStatusCode { get; set; }

        [StringLength(22)]
        [Column("PREFERRED_LANGUAGE")]
        [SwaggerSchema("Preferred communication language.")]
        public string? PreferredLanguage { get; set; }

        [StringLength(20)]
        [Column("CHANNEL_CODE")]
        [SwaggerSchema("Channel code the agent belongs to.")]
        public string? ChannelCode { get; set; }

        [StringLength(20)]
        [Column("SUB_CHANNEL_CODE")]
        [SwaggerSchema("Sub-channel code.")]
        public string? SubChannelCode { get; set; }

        [StringLength(20)]
        [Column("DESIGNATION_CODE")]
        [SwaggerSchema("Designation code of the agent.")]
        public string? DesignationCode { get; set; }

        [StringLength(20)]
        [Column("AGENT_LEVEL")]
        [SwaggerSchema("Agent level or hierarchy.")]
        public string? AgentLevel { get; set; }

        [StringLength(20)]
        [Column("LOCATION_CODE")]
        [SwaggerSchema("Location code assigned to the agent.")]
        public string? LocationCode { get; set; }

        [StringLength(20)]
        [Column("STAFF_CODE")]
        [SwaggerSchema("Staff code if applicable.")]
        public string? StaffCode { get; set; }

        [StringLength(50)]
        [Column("Supervisor_Id")]
        [SwaggerSchema("Code of the supervising agent.")]
        public int? Supervisor_Id { get; set; }

        [Column("CONTRACTED_DATE", TypeName = "date")]
        [SwaggerSchema("Date the agent was contracted.")]
        public DateTime? ContractedDate { get; set; }

        [StringLength(20)]
        [Column("AGENT_STATUS_CODE")]
        [SwaggerSchema("Current status code of the agent.")]
        public string? AgentStatusCode { get; set; }

        [Column("STATUS_DATE", TypeName = "date")]
        [SwaggerSchema("Date when the current status was set.")]
        public DateTime? StatusDate { get; set; }

        [Required]
        [Column("IS_LICENSED")]
        [SwaggerSchema("Indicates whether the agent is licensed.")]
        public bool IsLicensed { get; set; }

        [StringLength(40)]
        [Column("PAN_NUMBER")]
        [SwaggerSchema("PAN number (tax identifier).")]
        public string? PanNumber { get; set; }

        [StringLength(40)]
        [Column("AADHAAR_NUMBER")]
        [SwaggerSchema("Aadhaar number (national ID).")]
        public string? AadhaarNumber { get; set; }

        [StringLength(40)]
        [Column("IRDA_LICENSE_NUMBER")]
        [SwaggerSchema("IRDA license number.")]
        public string? IrdaLicenseNumber { get; set; }

        [StringLength(100)]
        [Column("GST_NUMBER")]
        [SwaggerSchema("GST registration number.")]
        public string? GstNumber { get; set; }

        [Required]
        [StringLength(100)]
        [Column("CREATED_BY")]
        [SwaggerSchema("User who created the agent record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Date and time the record was created.")]
        public DateTime CreatedDate { get; set; }

        [StringLength(100)]
        [Column("MODIFIED_BY")]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Used for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        // Navigation property
        [ForeignKey(nameof(Supervisor_Id))]
        [SwaggerSchema("Reference to the supervisor agent.")]
        public Agent? Supervisor { get; set; }
        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates whether the agent is currently active.")]
        public bool IsActive { get; set; } = true;
    }
}
