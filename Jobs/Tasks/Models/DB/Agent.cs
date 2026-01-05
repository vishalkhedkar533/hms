namespace Tasks.Models.DB
{
    public class PeopleHeirarchyDto
    {
        public int? AgentId { get; set; }
        public long? HierarchyId { get; set; }
        public string? AgentCode { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public PeopleHeirarchyDto? Supervisors { get; set; }
        public string? HierarchyPath { get; set; } = string.Empty;
    }

    public class Agent
    {
        // Primary Key
        public int AgentId { get; set; }

        // Required Identity & Status Fields
        public string AgentCode { get; set; } = string.Empty;
        public string? AgentName { get; set; }
        public string? BusinessName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Prefix { get; set; }
        public string? Suffix { get; set; }

        // Dates (SQL date -> DateOnly)
        public DateTime? Dob { get; set; }
        public string? Nationality { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? AgentLevel { get; set; }
        public string? StaffCode { get; set; }
        public DateTime? ContractedDate { get; set; }
        public string? AgentStatusCode { get; set; }
        public DateTime? StatusDate { get; set; }

        // Compliance & Licensing
        public bool IsLicensed { get; set; }
        public string? PanNumber { get; set; }
        public string? AadhaarNumber { get; set; }
        public string? IrdaLicenseNumber { get; set; }
        public string? GstNumber { get; set; }

        // Audit Fields (SQL timestamp -> DateTime)
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? Rowversion { get; set; }

        // Hierarchy & Status
        public int? SupervisorId { get; set; }
        public bool IsActive { get; set; }
        public string? Email { get; set; }
        public string? Mobileno { get; set; }
        public string? ApplicationDocketNo { get; set; }
        public string? FatherHusbandNm { get; set; }
        public string? Employeecode { get; set; }
        public DateTime? Startdate { get; set; }

        // Compliance Flags
        public bool Panaadharlinkflag { get; set; }
        public bool Sec206abflag { get; set; }
        public string? Taxstatus { get; set; }
        public string? Stateeid { get; set; }
        public string? Urn { get; set; }
        public string? Additionalcomment { get; set; }
        public DateTime? Appointmentdate { get; set; }
        public DateTime? Incorporationdate { get; set; }

        // Contact Person Details
        public string? Cnctpersondesig { get; set; }
        public string? Cnctpersonmobileno { get; set; }
        public string? Cnctpersonemail { get; set; }
        public string? Cnctpersonname { get; set; }

        // Classifications & Training
        public string? Agenttypecategory { get; set; }
        public string? Agentclassification { get; set; }
        public string? Cmsagenttype { get; set; }
        public string? Packageid { get; set; }
        public string? Servicetaxno { get; set; }
        public bool? Ulipflag { get; set; }
        public string? Traininggrouptype { get; set; }
        public string? Ifs { get; set; }
        public bool? Refreshertrainingcompleted { get; set; }
        public bool? Ismigrated { get; set; }

        // Branch & Partner Info
        public string? Mainpartnerclientcode { get; set; }
        public string? Agentmaincodevweid { get; set; }
        public DateTime? Registrationdate { get; set; }
        public string? Vertical { get; set; }
        public string? Branchcode { get; set; }
        public string? Branchname { get; set; }

        // Extensive HR/Training Dates
        public DateTime? Ic36Trngcompletiondate { get; set; }
        public DateTime? Strngcompletiondate { get; set; }
        public DateTime? Confirmationdate { get; set; }
        public DateTime? Fgrockstartrainingdate { get; set; }
        public DateTime? Incrementdate { get; set; }
        public DateTime? Lastpromotiondate { get; set; }
        public DateTime? Hrdoj { get; set; }
        public DateTime? Fgvaluetrngdate { get; set; }
        public DateTime? Hsecpolicytrngdate { get; set; }
        public DateTime? Itsecpolicytrngdate { get; set; }
        public DateTime? Npstrngcompletiondate { get; set; }
        public DateTime? Whistleblowertrngdate { get; set; }
        public DateTime? Govpolicytrngdate { get; set; }
        public DateTime? Inductiontrngdate { get; set; }
        public DateTime? Lastworkingdate { get; set; }

        // License Details
        public string? Licenseno { get; set; }
        public string? Licensetype { get; set; }
        public DateTime? Licenseissuedate { get; set; }
        public DateTime? Licenseexpirydate { get; set; }
        public string? Licensestatus { get; set; }

        // Foreign Key IDs (Lookup codes)
        public int? ActivePermAddress { get; set; }
        public int? ActiveMailAddress { get; set; }
        public int? Orgid { get; set; }
        public int? Bankacctype { get; set; }
        public int? Channel { get; set; }
        public int? Subchannel { get; set; }
        public int? AgentTypeCat { get; set; }
        public int? AgentClass { get; set; }
        public int? MartialStatus { get; set; }
        public int? Education { get; set; }
        public int? State { get; set; }
        public int? Country { get; set; }
        public int? Gender { get; set; }
        public int? Title { get; set; }
        public int? Occupation { get; set; }
        public int? AgentSubTypeCode { get; set; }
        public int? DesignationCode { get; set; }
        public int? AgentTypeCode { get; set; }
        public int? LocationCode { get; set; }
        public int? Candidatetype { get; set; }
        public int? Agenttype { get; set; }
        public int? Commissionclass { get; set; }
        public string? AgentClassDesc { get; set; }
        public string? Comments { get; set; }
        public string? Reason { get; set; }
        public DateTime? DOB { get; set; }
        public string? Supervisor_Code { get; set; }
        public string? MaskedPanNumber { get; set; }
        public string? aadhaar_number { get; set; }
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
        public DateTime DateOfBirth { get; set; }
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
        public string? AgentMaincodevwEid { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public DateTime? Ic36TrngCompletionDate { get; set; }
        public DateTime? STrngCompletionDate { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public DateTime? FgRockstarTrainingDate { get; set; }
        public DateTime? IncrementDate { get; set; }
        public DateTime? LastPromotionDate { get; set; }
        public DateTime? HRDoj { get; set; }
        public DateTime? FgValueTrngDate { get; set; }
        public DateTime? HSecPolicyTrngDate { get; set; }
        public DateTime? ItSecPolicyTrngDate { get; set; }
        public DateTime? NpsTrngCompletionDate { get; set; }
        public DateTime? WhistleBlowerTrngDate { get; set; }
        public DateTime? GovPolicyTrngDate { get; set; }
        public DateTime? InductionTrngDate { get; set; }
        public DateTime? LastWorkingDate { get; set; }
        public string? LicenseNo { get; set; }
        public string? LicenseType { get; set; }
        public DateTime? LicenseIssueDate { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public string? LicenseStatus { get; set; }
        public int? OrgId { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? City { get; set; }
        public string? Pin { get; set; }
        public string? Landmark { get; set; }
        public string? MobileNo { get; set; }
        public int? BankAccType { get; set; }
        public int? SubChannel { get; set; }
        public int? MaritalStatus { get; set; }
        public int? CandidateType { get; set; }
        public int? CommissionClass { get; set; }
        public int? AgentType { get; set; }
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
    public class Address
    {
        public long AddressID { get; set; }
        public string? AddressType { get; set; } //enum AddressType
        public int RefKey { get; set; }
        public string? RefType { get; set; }//enum RefType
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PIN { get; set; }
        public string? Landmark { get; set; }
    }
    public class BankAccount
    {
        public int Id { get; set; }
        public int RefKey { get; set; }
        public string RefType { get; set; }
        public string AccountHolderName { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public string IFSC { get; set; } = null!;
        public string? MICR { get; set; } = null!;
        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public int AccountType { get; set; } = 1;
        public DateTime? ActiveSince { get; set; } = DateTime.Now;
        public string? FactoringHouse { get; set; }
        public string? PreferredPaymentMode { get; set; }
        public string? AccountTypeDesc { get; set; }
    }

    public class PersonalInfo
    {
        public int PersonalInfoId { get; set; }
        public int RefKey { get; set; }
        public string? RefType { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PanNumber { get; set; }
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
        public string? WorkContactNo { get; set; }
        public string? ResidenceContactNo { get; set; }
        public string? BloodGroup { get; set; }
        public string? BirthPlace { get; set; }
        public int? EducationCode { get; set; }
        public string? EducationLevel { get; set; }
        public string? WorkProfile { get; set; }
        public decimal? AnnualIncome { get; set; }
        public int? WorkExpMonths { get; set; }
    }

    public class Nominee
    {
        public int NomineeID { get; set; }
        public int RefKey { get; set; }
        public string? RefType { get; set; }
        public string NomineeName { get; set; }
        public string Relationship { get; set; }
        public decimal PercentageShare { get; set; }
        public bool IsActive { get; set; }
        public long NomineeAge { get; set; }
    }
}
