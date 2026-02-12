using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.BackEndCalculation
{
    public class Agent
    {
        // Primary Key
        [Column("agent_id")]
        [Description("agent_id")]
        public int AgentId { get; set; }

        // Required Identity & Status Fields
        [Column("agent_code")]
        [Description("agent_code")]
        public string AgentCode { get; set; } = string.Empty;
        [Column("agent_name")]
        [Description("agent_name")]
        public string? AgentName { get; set; }
        [Column("business_name")]
        [Description("business_name")]
        public string? BusinessName { get; set; }
        [Column("first_name")]
        [Description("first_name")]
        public string? FirstName { get; set; }
        [Column("middle_name")]
        [Description("middle_name")]
        public string? MiddleName { get; set; }
        [Column("last_name")]
        [Description("last_name")]
        public string? LastName { get; set; }
        [Column("prefix")]
        [Description("prefix")]
        public string? Prefix { get; set; }
        [Column("suffix")]
        [Description("suffix")]
        public string? Suffix { get; set; }
        [Column("dob")]
        [Description("dob")]
        public DateTime? DoB { get; set; }
        [Column("nationality")]
        [Description("nationality")]
        public string? Nationality { get; set; }
        [Column("preferred_language")]
        [Description("preferred_language")]
        public string? PreferredLanguage { get; set; }
        [Column("agent_level")]
        [Description("agent_level")]
        public string? AgentLevel { get; set; }
        [Column("staff_code")]
        [Description("staff_code")]
        public string? StaffCode { get; set; }
        [Column("contracted_date")]
        [Description("contracted_date")]
        public DateTime? ContractedDate { get; set; }
        [Column("agent_status_code")]
        [Description("agent_status_code")]
        public string? AgentStatusCode { get; set; }
        [Column("status_date")]
        [Description("status_date")]
        public DateTime? StatusDate { get; set; }
        [Column("is_licensed")]
        [Description("is_licensed")]
        public bool IsLicensed { get; set; }
        [Column("pan_number")]
        [Description("pan_number")]
        public string? PanNumber { get; set; }
        [Column("aadhaar_number")]
        [Description("aadhaar_number")]
        public string? AadhaarNumber { get; set; }
        [Column("irda_license_number")]
        [Description("irda_license_number")]
        public string? IrdaLicenseNumber { get; set; }
        [Column("gst_number")]
        [Description("gst_number")]
        public string? GstNumber { get; set; }
        [Column("created_by")]
        [Description("created_by")]
        public string CreatedBy { get; set; } = string.Empty;
        [Column("created_date")]
        [Description("created_date")]
        public DateTime CreatedDate { get; set; }
        [Column("modified_by")]
        [Description("modified_by")]
        public string? ModifiedBy { get; set; }
        [Column("modified_date")]
        [Description("modified_date")]
        public DateTime? ModifiedDate { get; set; }
        [Column("rowversion")]
        [Description("rowversion")]
        public int? Rowversion { get; set; }
        [Column("supervisor_id")]
        [Description("supervisor_id")]
        public int? SupervisorId { get; set; }
        [Column("is_active")]
        [Description("is_active")]
        public bool IsActive { get; set; }
        [Column("email")]
        [Description("email")]
        public string? Email { get; set; }
        [Column("mobileno")]
        [Description("mobileno")]
        public string? Mobileno { get; set; }
        public string? ApplicationNo { get; set; }
        [Column("applicationdocketno")]
        [Description("applicationdocketno")]
        public string? ApplicationDocketNo { get; set; }
        [Column("father_husband_nm")]
        [Description("fatherhusbandnm")]
        public string? FatherHusbandNm { get; set; }
        [Column("employeecode")]
        [Description("employeecode")]
        public string? Employeecode { get; set; }
        [Column("startdate")]
        [Description("startdate")]
        public DateTime? Startdate { get; set; }
        [Column("panaadharlinkflag")]
        [Description("panaadharlinkflag")]
        public bool Panaadharlinkflag { get; set; }
        [Column("sec206abflag")]
        [Description("sec206abflag")]
        public bool Sec206abflag { get; set; }
        [Column("taxstatus")]
        [Description("taxstatus")]
        public string? Taxstatus { get; set; }
        [Column("stateeid")]
        [Description("stateeid")]
        public string? Stateeid { get; set; }
        [Column("urn")]
        [Description("urn")]
        public string? Urn { get; set; }
        [Column("additionalcomment")]
        [Description("additionalcomment")]
        public string? Additionalcomment { get; set; }
        [Column("appointmentdate")]
        [Description("appointmentdate")]
        public DateTime? Appointmentdate { get; set; }
        [Column("incorporationdate")]
        [Description("incorporationdate")]
        public DateTime? Incorporationdate { get; set; }
        [Column("cnctpersondesig")]
        [Description("cnctpersondesig")]
        public string? Cnctpersondesig { get; set; }
        [Column("cnctpersonmobileno")]
        [Description("cnctpersonmobileno")]
        public string? Cnctpersonmobileno { get; set; }
        [Column("cnctpersonemail")]
        [Description("cnctpersonemail")]
        public string? Cnctpersonemail { get; set; }
        [Column("cnctpersonname")]
        [Description("cnctpersonname")]
        public string? Cnctpersonname { get; set; }
        [Column("agenttypecategory")]
        [Description("agenttypecategory")]
        public string? Agenttypecategory { get; set; }
        [Column("agentclassification")]
        [Description("agentclassification")]
        public string? Agentclassification { get; set; }
        [Column("cmsagenttype")]
        [Description("cmsagenttype")]
        public string? Cmsagenttype { get; set; }
        [Column("packageid")]
        [Description("packageid")]
        public string? Packageid { get; set; }
        [Column("servicetaxno")]
        [Description("servicetaxno")]
        public string? Servicetaxno { get; set; }
        [Column("ulipflag")]
        [Description("ulipflag")]
        public bool? Ulipflag { get; set; }
        [Column("traininggrouptype")]
        [Description("traininggrouptype")]
        public string? Traininggrouptype { get; set; }
        [Column("ifs")]
        [Description("ifs")]
        public string? Ifs { get; set; }
        [Column("refreshertrainingcompleted")]
        [Description("refreshertrainingcompleted")]
        public bool? Refreshertrainingcompleted { get; set; }
        [Column("ismigrated")]
        [Description("ismigrated")]
        public bool? Ismigrated { get; set; }
        [Column("mainpartnerclientcode")]
        [Description("mainpartnerclientcode")]
        public string? Mainpartnerclientcode { get; set; }
        [Column("agentmaincodevweid")]
        [Description("agentmaincodevweid")]
        public string? AgentMainCodeVWEid { get; set; }
        [Column("registrationdate")]
        [Description("registrationdate")]
        public DateTime? Registrationdate { get; set; }
        [Column("vertical")]
        [Description("vertical")]
        public string? Vertical { get; set; }
        [Column("branchcode")]
        [Description("branchcode")]
        public string? BranchCode { get; set; }
        [Column("branchname")]
        [Description("branchname")]
        public string? BranchName { get; set; }
        [Column("ic36trngcompletiondate")]
        [Description("ic36trngcompletiondate")]
        public DateTime? IC36TrngCompletionDate { get; set; }
        [Column("strngcompletiondate")]
        [Description("strngcompletiondate")]
        public DateTime? STrngCompletionDate { get; set; }
        [Column("confirmationdate")]
        [Description("confirmationdate")]
        public DateTime? ConfirmationDate { get; set; }
        [Column("fgrockstartrainingdate")]
        [Description("fgrockstartrainingdate")]
        public DateTime? FgRockStarTrainingDate { get; set; }
        [Column("incrementdate")]
        [Description("incrementdate")]
        public DateTime? IncrementDate { get; set; }
        [Column("lastpromotiondate")]
        [Description("lastpromotiondate")]
        public DateTime? LastPromotionDate { get; set; }
        [Column("hrdoj")]
        [Description("hrdoj")]
        public DateTime? HRDoj { get; set; }
        [Column("fgvaluetrngdate")]
        [Description("fgvaluetrngdate")]
        public DateTime? FgValueTrngDate { get; set; }
        [Column("hsecpolicytrngdate")]
        [Description("hsecpolicytrngdate")]
        public DateTime? HSecPolicyTrngDate { get; set; }
        [Column("itsecpolicytrngdate")]
        [Description("itsecpolicytrngdate")]
        public DateTime? ITSecPolicyTrngDate { get; set; }
        [Column("npstrngcompletiondate")]
        [Description("npstrngcompletiondate")]
        public DateTime? NPSTrngCompletionDate { get; set; }
        [Column("whistleblowertrngdate")]
        [Description("whistleblowertrngdate")]
        public DateTime? WhistleBlowerTrngDate { get; set; }
        [Column("govpolicytrngdate")]
        [Description("govpolicytrngdate")]
        public DateTime? Govpolicytrngdate { get; set; }
        [Column("inductiontrngdate")]
        [Description("inductiontrngdate")]
        public DateTime? InductionTrngDate { get; set; }
        [Column("lastworkingdate")]
        [Description("lastworkingdate")]
        public DateTime? LastWorkingDate { get; set; }
        [Column("licenseno")]
        [Description("licenseno")]
        public string? LicenseNo { get; set; }
        [Column("licensetype")]
        [Description("licensetype")]
        public string? LicenseType { get; set; }
        [Column("licenseissuedate")]
        [Description("licenseissuedate")]
        public DateTime? LicenseIssueDate { get; set; }
        [Column("licenseexpirydate")]
        [Description("licenseexpirydate")]
        public DateTime? LicenseExpiryDate { get; set; }
        [Column("licensestatus")]
        [Description("licensestatus")]
        public string? LicenseStatus { get; set; }
        public int? ActivePermAddress { get; set; }
        public int? ActiveMailAddress { get; set; }
        [Column("orgid")]
        [Description("orgid")]
        public int? OrgId { get; set; }
        [Column("bankacctype")]
        [Description("bankacctype")]
        public int? BankAccType { get; set; }
        [Column("channel")]
        [Description("channel")]
        public int? Channel { get; set; }
        [Column("subchannel")]
        [Description("subchannel")]
        public int? SubChannel { get; set; }
        [Column("agent_type_cat")]
        [Description("agenttypecat")]
        public int? AgentTypeCat { get; set; }
        [Column("agent_class")]
        [Description("agentclass")]
        public int? AgentClass { get; set; }
        [Column("martial_status")]
        [Description("maritalstatus")]
        public int? MartialStatus { get; set; }
        [Column("education")]
        [Description("education")]
        public int? Education { get; set; }
        [Column("state")]
        [Description("state")]
        public int? State { get; set; }
        [Column("country")]
        [Description("country")]
        public int? Country { get; set; }
        [Column("gender")]
        [Description("gender")]
        public int? Gender { get; set; }
        [Column("title")]
        [Description("title")]
        public int? Title { get; set; }
        [Column("occupation")]
        [Description("occupation")]
        public int? Occupation { get; set; }
        [Column("agent_sub_type_code")]
        [Description("agent_sub_type_code")]
        public int? AgentSubTypeCode { get; set; }
        [Column("designation_code")]
        [Description("designation_code")]
        public int? DesignationCode { get; set; }
        [Column("agent_type_code")]
        [Description("agent_type_code")]
        public int? AgentTypeCode { get; set; }
        [Column("location_code")]
        [Description("location_code")]
        public int? LocationCode { get; set; }
        [Column("candidatetype")]
        [Description("candidatetype")]
        public int? CandidateType { get; set; }
        [Column("agenttype")]
        [Description("agenttype")]
        public int? AgentType { get; set; }
        [Column("commissionclass")]
        [Description("commissionclass")]
        public int? CommissionClass { get; set; }
        public string? AgentClassDesc { get; set; }
        public string? Comments { get; set; }
        public string? Reason { get; set; }
        //public DateTime? DOB { get; set; }
        public string? Supervisor_Code { get; set; }
        public string? MaskedPanNumber { get; set; }
        //public string? aadhaar_number { get; set; }
        public int? RowVersion { get; set; }
        public string? Father_Husband_Nm { get; set; }
        public string? EmployeeCode { get; set; }
        public DateTime? StartDate { get; set; }
        public bool PanAadharLinkFlag { get; set; }
        public bool Sec206abFlag { get; set; }
        public List<Nominee>? nominees { get; set; }
        public string? NomineeName { get; set; }
        public string? Relationship { get; set; }
        public decimal PercentageShare { get; set; }
        public long NomineeAge { get; set; }
        public string? PackageID { get; set; }
        public List<PersonalInfo>? personalInfo { get; set; }
        //public DateTime DateOfBirth { get; set; }
        public string? WorkContactNo { get; set; }
        public string? ResidenceContactNo { get; set; }
        public string? BloodGroup { get; set; }
        public string? BirthPlace { get; set; }
        public int? EducationCode { get; set; }
        public string? EducationLevel { get; set; }
        public string? WorkProfile { get; set; }
        public decimal? AnnualIncome { get; set; }
        public int? WorkExpMonths { get; set; }
        public string? TaxStatus { get; set; }
        public string? StateEid { get; set; }
        public string? URN { get; set; }
        public string? AdditionalComment { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public DateTime? IncorporationDate { get; set; }
        public string? CnctPersonDesig { get; set; }
        public string? CnctPersonMobileNo { get; set; }
        public string? CnctPersonEmail { get; set; }
        public string? CnctPersonName { get; set; }
        public string? AgentTypeCategory { get; set; }
        public string? AgentClassification { get; set; }
        public string? CMSAgentType { get; set; }
        public List<BankAccount>? bankAccounts { get; set; }
        public string? AccountHolderName { get; set; } = null!;
        public string? AccountNumber { get; set; } = null!;
        public string? IFSC { get; set; } = null!;
        public string? MICR { get; set; } = null!;
        public string? BankName { get; set; }
        public string? BankAccBranchName { get; set; }
        public int AccountType { get; set; } = 1;
        public DateTime? ActiveSince { get; set; } = DateTime.Now;
        public string? FactoringHouse { get; set; }
        public int PreferredPaymentMode { get; set; }
        public string? ServiceTaxNo { get; set; }
        public List<Address>? PermanentAddres { get; set; }
        public List<Address>? MailingAddres { get; set; }
        public bool UlipFlag { get; set; } = false;
        public string? TrainingGroupType { get; set; }
        public bool RefresherTrainingCompleted { get; set; }
        public bool IsMigrated { get; set; }
        public string? MainPartnerClientCode { get; set; }
        //public string? AgentMaincodevwEid { get; set; }
        public DateTime? RegistrationDate { get; set; }
        //public string? BranchCode { get; set; }
        //public string? BranchName { get; set; }
        //public DateTime? Ic36TrngCompletionDate { get; set; }
        //public DateTime? STrngCompletionDate { get; set; }
        //public DateTime? ConfirmationDate { get; set; }
        //public DateTime? FgRockstarTrainingDate { get; set; }
        //public DateTime? IncrementDate { get; set; }
        //public DateTime? LastPromotionDate { get; set; }
        //public DateTime? HRDoj { get; set; }
        //public DateTime? FgValueTrngDate { get; set; }
        //public DateTime? HSecPolicyTrngDate { get; set; }
        //public DateTime? ItSecPolicyTrngDate { get; set; }
        //public DateTime? NpsTrngCompletionDate { get; set; }
        //public DateTime? WhistleBlowerTrngDate { get; set; }
        public DateTime? GovPolicyTrngDate { get; set; }
        //public DateTime? InductionTrngDate { get; set; }
        //public DateTime? LastWorkingDate { get; set; }
        //public string? LicenseNo { get; set; }
        //public string? LicenseType { get; set; }
        //public DateTime? LicenseIssueDate { get; set; }
        //public DateTime? LicenseExpiryDate { get; set; }
        //public string? LicenseStatus { get; set; }
        //public int? OrgId { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? City { get; set; }
        public string? Pin { get; set; }
        public string? Landmark { get; set; }
        public string? MobileNo { get; set; }
        //public int? BankAccType { get; set; }
        //public int? SubChannel { get; set; }
        //public int? MaritalStatus { get; set; }
        //public int? CandidateType { get; set; }
        //public int? CommissionClass { get; set; }
        //public int? AgentType { get; set; }
        public string? BankAccTypeDesc { get; set; }
        public string? GenderDesc { get; set; }
        public string? TitleDesc { get; set; }
        public string? ChannelDesc { get; set; }
        public string? SubChannelDesc { get; set; }
        public string? OccupationDesc { get; set; }
        public string? AgentTypeCatDesc { get; set; }
        public string? MaritalStatusDesc { get; set; }
        public string? EducationDesc { get; set; }
        public string? StateDesc { get; set; }
        public string? CountryDesc { get; set; }
        public string? DesignationCodeDesc { get; set; }
        public string? LocationCodeDesc { get; set; }
        public string? AgentTypeCodeDesc { get; set; }
        public string? AgentSubTypeCodeDesc { get; set; }
        public string? CandidateTypeDesc { get; set; }
        public string? CommissionClassDesc { get; set; }
        public string? AgentTypeDesc { get; set; }
    }
}