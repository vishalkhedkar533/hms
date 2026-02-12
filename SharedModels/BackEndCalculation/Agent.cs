using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.BackEndCalculation
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
    [Table("agent", Schema = "hms")]
    public class Agent
    {
        [Key]
        [Column("agent_id")]
        [SwaggerSchema("Primary key for the agent.")]
        public int AgentId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("agent_code")]
        [SwaggerSchema("Unique code identifying the agent.")]
        public string AgentCode { get; set; } = null!;

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

        [Column("dob", TypeName = "date")]
        public DateTime? Dob { get; set; }

        [StringLength(40)]
        [Column("nationality")]
        public string? Nationality { get; set; }

        [StringLength(22)]
        [Column("preferred_language")]
        public string? PreferredLanguage { get; set; }

        [StringLength(20)]
        [Column("agent_level")]
        public string? AgentLevel { get; set; }

        [StringLength(20)]
        [Column("staff_code")]
        public string? StaffCode { get; set; }

        [Column("contracted_date", TypeName = "date")]
        public DateOnly? ContractedDate { get; set; }

        [StringLength(20)]
        [Column("agent_status_code")]
        public string? AgentStatusCode { get; set; }

        [Column("status_date", TypeName = "date")]
        public DateOnly? StatusDate { get; set; }

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

        [Column("supervisor_id")]
        public int? SupervisorId { get; set; }

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
        [Column("applicationdocketno")]
        public string? ApplicationDocketNo { get; set; }

        [StringLength(200)]
        [Column("father_husband_nm")]
        public string? FatherHusbandNm { get; set; }

        [StringLength(50)]
        [Column("employeecode")]
        public string? EmployeeCode { get; set; }

        [Column("startdate")]
        public DateTime? StartDate { get; set; }

        [Required]
        [Column("panaadharlinkflag")]
        public bool PanAadharLinkFlag { get; set; }

        [Required]
        [Column("sec206abflag")]
        public bool Sec206abFlag { get; set; }

        [StringLength(50)]
        [Column("taxstatus")]
        public string? TaxStatus { get; set; }

        [StringLength(50)]
        [Column("stateeid")]
        public string? StateEid { get; set; }

        [StringLength(50)]
        [Column("urn")]
        public string? Urn { get; set; }

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
        public string? CmsAgentType { get; set; }

        [StringLength(50)]
        [Column("packageid")]
        public string? PackageId { get; set; }

        [StringLength(50)]
        [Column("servicetaxno")]
        public string? ServiceTaxNo { get; set; }

        [Column("ulipflag")]
        public bool? UlipFlag { get; set; }

        [Column("traininggrouptype")]
        public int? TrainingGroupType { get; set; }

        [StringLength(100)]
        [Column("ifs")]
        public string? Ifs { get; set; }

        [Column("refreshertrainingcompleted")]
        public bool? RefresherTrainingCompleted { get; set; }

        [Column("ismigrated")]
        public bool? IsMigrated { get; set; }

        [StringLength(50)]
        [Column("mainpartnerclientcode")]
        public string? MainPartnerClientCode { get; set; }

        [StringLength(50)]
        [Column("agentmaincodevweid")]
        public string? AgentMaincodeVweid { get; set; }

        [Column("registrationdate")]
        public DateTime? RegistrationDate { get; set; }

        [Column("vertical")]
        public int? Vertical { get; set; }

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
        public DateTime? HrDoj { get; set; }

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

        [Column("licensetype")]
        public int? LicenseType { get; set; }

        [Column("licenseissuedate")]
        public DateTime? LicenseIssueDate { get; set; }

        [Column("licenseexpirydate")]
        public DateTime? LicenseExpiryDate { get; set; }

        [Column("licensestatus")]
        public int? LicenseStatus { get; set; }

        [Column("orgid")]
        public int? OrgId { get; set; }

        [Column("bankacctype")]
        public int? BankAccType { get; set; }

        [Column("channel")]
        public int? Channel { get; set; }

        [Column("subchannel")]
        public int? SubChannel { get; set; }

        [Column("agent_type_cat")]
        public int? AgentTypeCat { get; set; }

        [Column("agent_class")]
        public int? AgentClass { get; set; }

        [Column("martial_status")]
        public int? MaritalStatus { get; set; }

        [Column("education")]
        public int? Education { get; set; }

        [Column("state")]
        public int? State { get; set; }

        [Column("country")]
        public int? Country { get; set; }

        [Column("gender")]
        public int? Gender { get; set; }

        [Column("title")]
        public int? Title { get; set; }

        [Column("occupation")]
        public int? Occupation { get; set; }

        [Column("agent_sub_type_code")]
        public int? AgentSubTypeCode { get; set; }

        [Column("designation_code")]
        public int? DesignationCode { get; set; }

        [Column("agent_type_code")]
        public int? AgentTypeCode { get; set; }

        [Column("location_code")]
        public int? LocationCode { get; set; }

        [Column("candidatetype")]
        public int? CandidateType { get; set; }

        [Column("agenttype")]
        public int? AgentType { get; set; }

        [Column("commissionclass")]
        public int? CommissionClass { get; set; }

        // Navigation Property
        [ForeignKey(nameof(SupervisorId))]
        public virtual Agent? Supervisor { get; set; }

        [Column("branch")]
        public int? Branch { get; set; }
    }

}