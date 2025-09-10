using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    /*
     * modelBuilder.Entity<Agent>(entity =>
{
    entity.ToTable("agent", "hms");

    entity.HasIndex(e => e.AgentCode).IsUnique();

    entity.HasOne(e => e.Supervisor)
          .WithMany()
          .HasForeignKey(e => e.SupervisorCode)
          .HasConstraintName("fk_supervisor")
          .OnDelete(DeleteBehavior.SetNull);
});

     */
    [Table("agent", Schema = "hms")] // ✅ lowercase to match DB
    public class Agent
    {
        [Key]
        [Column("agent_id")] // ✅ lowercase
        [SwaggerSchema("Primary key for the agent.")]
        public int AgentId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("agent_code")]
        [SwaggerSchema("Unique code identifying the agent.")]
        public string AgentCode { get; set; } = null!;

        [StringLength(20)]
        [Column("agent_type_code")]
        public string? AgentTypeCode { get; set; }

        [StringLength(20)]
        [Column("agent_sub_type_code")]
        public string? AgentSubTypeCode { get; set; }

        [StringLength(150)]
        [Column("agent_name")]
        public string? AgentName { get; set; }

        [StringLength(300)]
        [Column("business_name")]
        public string? BusinessName { get; set; }

        [StringLength(150)]
        [Column("first_name")]
        public string? FirstName { get; set; }

        [StringLength(150)]
        [Column("middle_name")]
        public string? MiddleName { get; set; }

        [StringLength(150)]
        [Column("last_name")]
        public string? LastName { get; set; }

        [StringLength(20)]
        [Column("prefix")]
        public string? Prefix { get; set; }

        [StringLength(20)]
        [Column("suffix")]
        public string? Suffix { get; set; }

        [StringLength(10)]
        [Column("gender")]
        public string? Gender { get; set; }

        [Column("dob", TypeName = "date")]
        public DateTime? DOB { get; set; }

        [StringLength(40)]
        [Column("nationality")]
        public string? Nationality { get; set; }

        [StringLength(20)]
        [Column("marital_status_code")]
        public string? MaritalStatusCode { get; set; }

        [StringLength(22)]
        [Column("preferred_language")]
        public string? PreferredLanguage { get; set; }

        [StringLength(20)]
        [Column("channel_code")]
        public string? ChannelCode { get; set; }

        [StringLength(20)]
        [Column("sub_channel_code")]
        public string? SubChannelCode { get; set; }

        [StringLength(20)]
        [Column("designation_code")]
        public string? DesignationCode { get; set; }

        [StringLength(20)]
        [Column("agent_level")]
        public string? AgentLevel { get; set; }

        [StringLength(20)]
        [Column("location_code")]
        public string? LocationCode { get; set; }

        [StringLength(20)]
        [Column("staff_code")]
        public string? StaffCode { get; set; }

        [Column("supervisor_id")] // ✅ matches DB
        public int? SupervisorId { get; set; }

        [Column("contracted_date", TypeName = "date")]
        public DateTime? ContractedDate { get; set; }

        [StringLength(20)]
        [Column("agent_status_code")]
        public string? AgentStatusCode { get; set; }

        [Column("status_date", TypeName = "date")]
        public DateTime? StatusDate { get; set; }

        [Required]
        [Column("is_licensed")]
        public bool IsLicensed { get; set; }

        [StringLength(40)]
        [Column("pan_number")]
        public string? PanNumber { get; set; }

        [StringLength(40)]
        [Column("aadhaar_number")]
        public string? AadhaarNumber { get; set; }

        [StringLength(40)]
        [Column("irda_license_number")]
        public string? IrdaLicenseNumber { get; set; }

        [StringLength(100)]
        [Column("gst_number")]
        public string? GstNumber { get; set; }

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

        [Column("rowversion")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        [Column("email")]
        public string? Email { get; set; }
        [StringLength(20)]
        [Column("mobileno")]
        public string? MobileNo { get; set; }

        // 🔗 Navigation property
        [ForeignKey(nameof(SupervisorId))]
        public Agent? Supervisor { get; set; }
    }
}
