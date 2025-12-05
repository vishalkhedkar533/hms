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

        [StringLength(100)]
        [Column("candidatetype")]
        public string? CandidateType { get; set; }

        [StringLength(100)]
        [Column("applicationdocketno")]
        public string? ApplicationDocketNo { get; set; }

        [StringLength(20)]
        [Column("title")]
        public string? Title { get; set; }

        [StringLength(200)]
        [Column("father_husband_nm")]
        public string? Father_Husband_Nm { get; set; }

        [StringLength(100)]
        [Column("channel_name")]
        public string? Channel_Name { get; set; }

        [StringLength(100)]
        [Column("sub_channel")]
        public string? Sub_Channel { get; set; }

        [StringLength(50)]
        [Column("employeecode")]
        public string? EmployeeCode { get; set; }

        [Column("startdate")]
        public DateTime? StartDate { get; set; }

        [Column("panaadharlinkflag")]
        public bool PanAadharLinkFlag { get; set; }

        [Column("sec206abflag")]
        public bool Sec206abFlag { get; set; }

        [StringLength(50)]
        [Column("packageid")]
        public string? PackageID { get; set; }

        [StringLength(50)]
        [Column("commissionclass")]
        public string? CommissionClass { get; set; }

        [StringLength(50)]
        [Column("taxstatus")]
        public string? TaxStatus { get; set; }

        [StringLength(50)]
        [Column("stateeid")]
        public string? StateEid { get; set; }

        [Column("occupationcode")]
        public int? OccupationCode { get; set; }

        [StringLength(150)]
        [Column("occupation")]
        public string? Occupation { get; set; }

        [StringLength(50)]
        [Column("urn")]
        public string? URN { get; set; }

        [StringLength(200)]
        [Column("additionalcomment")]
        public string? AdditionalComment { get; set; }

        [Column("appointmentdate")]
        public DateTime? AppointmentDate { get; set; }

        [Column("incorporationdate")]
        public DateTime? IncorporationDate { get; set; }

        [StringLength(100)]
        [Column("cnctpersondesig")]
        public string? CnctPersonDesig { get; set; }

        [StringLength(20)]
        [Column("cnctpersonmobileno")]
        public string? CnctPersonMobileNo { get; set; }

        [StringLength(100)]
        [Column("cnctpersonemail")]
        public string? CnctPersonEmail { get; set; }

        [StringLength(150)]
        [Column("cnctpersonname")]
        public string? CnctPersonName { get; set; }

        [StringLength(100)]
        [Column("agenttypecategory")]
        public string? AgentTypeCategory { get; set; }

        [StringLength(100)]
        [Column("agentclassification")]
        public string? AgentClassification { get; set; }

        [StringLength(100)]
        [Column("cmsagenttype")]
        public string? CMSAgentType { get; set; }

        [StringLength(50)]
        [Column("servicetaxno")]
        public string? ServiceTaxNo { get; set; }

        [Column("ulipflag")]
        public bool UlipFlag { get; set; }

        [StringLength(100)]
        [Column("traininggrouptype")]
        public string? TrainingGroupType { get; set; }

        [StringLength(100)]
        [Column("ifs")]
        public string? Ifs { get; set; }

        [Column("refreshertrainingcompleted")]
        public bool RefresherTrainingCompleted { get; set; }

        [Column("ismigrated")]
        public bool IsMigrated { get; set; }

        [StringLength(50)]
        [Column("mainpartnerclientcode")]
        public string? MainPartnerClientCode { get; set; }

        [StringLength(50)]
        [Column("agentmaincodevweid")]
        public string? AgentMaincodevwEid { get; set; }

        [Column("registrationdate")]
        public DateTime? RegistrationDate { get; set; }

        [StringLength(100)]
        [Column("vertical")]
        public string? Vertical { get; set; }

        [StringLength(50)]
        [Column("branchcode")]
        public string? BranchCode { get; set; }

        [StringLength(100)]
        [Column("branchname")]
        public string? BranchName { get; set; }

        [Column("ic36trngcompletiondate")]
        public DateTime? Ic36TrngCompletionDate { get; set; }

        [Column("strngcompletiondate")]
        public DateTime? STrngCompletionDate { get; set; }

        [Column("confirmationdate")]
        public DateTime? ConfirmationDate { get; set; }

        [Column("fgrockstartrainingdate")]
        public DateTime? FgRockstarTrainingDate { get; set; }

        [Column("incrementdate")]
        public DateTime? IncrementDate { get; set; }

        [Column("lastpromotiondate")]
        public DateTime? LastPromotionDate { get; set; }

        [Column("hrdoj")]
        public DateTime? HRDoj { get; set; }

        [Column("fgvaluetrngdate")]
        public DateTime? FgValueTrngDate { get; set; }

        [Column("hsecpolicytrngdate")]
        public DateTime? HSecPolicyTrngDate { get; set; }

        [Column("itsecpolicytrngdate")]
        public DateTime? ItSecPolicyTrngDate { get; set; }

        [Column("npstrngcompletiondate")]
        public DateTime? NpsTrngCompletionDate { get; set; }

        [Column("whistleblowertrngdate")]
        public DateTime? WhistleBlowerTrngDate { get; set; }

        [Column("govpolicytrngdate")]
        public DateTime? GovPolicyTrngDate { get; set; }

        [Column("inductiontrngdate")]
        public DateTime? InductionTrngDate { get; set; }

        [Column("lastworkingdate")]
        public DateTime? LastWorkingDate { get; set; }

        [StringLength(50)]
        [Column("licenseno")]
        public string? LicenseNo { get; set; }

        [StringLength(50)]
        [Column("licensetype")]
        public string? LicenseType { get; set; }

        [Column("licenseissuedate")]
        public DateTime? LicenseIssueDate { get; set; }

        [Column("licenseexpirydate")]
        public DateTime? LicenseExpiryDate { get; set; }

        [StringLength(50)]
        [Column("licensestatus")]
        public string? LicenseStatus { get; set; }
        [ForeignKey(nameof(SupervisorId))]
        public Agent? Supervisor { get; set; }
        public int? OrgId { get; set; }
    }
}