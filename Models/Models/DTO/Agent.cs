using Models.DB;
using Models.Enums;
using System.Net.NetworkInformation;

namespace Models.DTO
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
    public class AgentDto
    {
        public int AgentId { get; set; }
        public string AgentCode { get; set; } = null!;
        public string? AgentTypeCode { get; set; }
        public string? AgentSubTypeCode { get; set; }
        public string? AgentName { get; set; }
        public string? BusinessName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Prefix { get; set; }
        public string? Suffix { get; set; }
        public string? Gender { get; set; }
        public DateTime? DOB { get; set; }
        public string? Nationality { get; set; }
        public string? MaritalStatusCode { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? ChannelCode { get; set; }
        public string? SubChannelCode { get; set; }
        public string? DesignationCode { get; set; }
        public string? Designation { get; set; }
        public string? AgentLevel { get; set; }
        public string? LocationCode { get; set; }
        public string? StaffCode { get; set; }
        public int? Supervisor_Id { get; set; }
        public DateTime? ContractedDate { get; set; }
        public string? AgentStatusCode { get; set; }
        public DateTime? StatusDate { get; set; }
        public bool IsLicensed { get; set; }
        public string? MaskedPanNumber { get; set; }
        public string? aadhaar_number { get; set; }
        public string? IrdaLicenseNumber { get; set; }
        public string? GstNumber { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? RowVersion { get; set; }
        public bool IsActive { get; set; } = true;
        public List<AgentDto>? Supervisors { get; set; }
        public List<AgentDto>? Reportees { get; set; }
        public List<AgentAuditTrailDTO>? agentAuditTrail { get; set; }
        public List<PeopleHeirarchyDto>? peopleHeirarchy { get; set; }
        public string? CandidateType { get; set; }
        public string? ApplicationDocketNo { get; set; }
        public string? Title { get; set; }
        public string? Father_Husband_Nm { get; set; }
        public string? Channel_Name { get; set; }
        public string? Sub_Channel { get; set; }
        public string? EmployeeCode { get; set; }
        public DateTime? StartDate { get; set; }
        public bool PanAadharLinkFlag { get; set; }
        public bool Sec206abFlag { get; set; }
        public List<Nominee>? nominees { get; set; }
        public string? PackageID { get; set; }
        public List<PersonalInfo>? personalInfo { get; set; }
        public string? CommissionClass { get; set; }
        public string? TaxStatus { get; set; }
        public string? StateEid { get; set; }
        public int? OccupationCode { get; set; }
        public String? Occupation { get; set; }
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
        public string? ServiceTaxNo { get; set; }
        public List<Address>? PermanentAddres { get; set; }
        public List<Address>? MailingAddres { get; set; }
        public bool UlipFlag { get; set; } = false;
        public string? TrainingGroupType { get; set; }
        public string? Ifs { get; set; }
        public bool RefresherTrainingCompleted { get; set; }
        public bool IsMigrated { get; set; }
        public string? MainPartnerClientCode { get; set; }
        public string? AgentMaincodevwEid { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? Vertical { get; set; }
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
        public List<KeyValueEntry>? bankAccType { get; set; }
        public List<KeyValueEntry>? titles { get; set; }
        public List<KeyValueEntry>? genders { get; set; }
        public List<KeyValueEntry>? channelNames { get; set; }
        public List<KeyValueEntry>? subChannels { get; set; }
        public List<KeyValueEntry>? occupations { get; set; }
        public List<KeyValueEntry>? agentTypeCategories { get; set; }
        public List<KeyValueEntry>? agentClassifications { get; set; }
        public List<KeyValueEntry>? maritalStatuses { get; set; }
        public List<KeyValueEntry>? educationCodes { get; set; }
        public List<KeyValueEntry>? stateNames { get; set; }
        public List<KeyValueEntry>? countries { get; set; }
    }
    public class AgentListRequest
    {
        public string userid
        {
            get; set;
        }
    }

    public class SearchAgent
    {
        public string? SearchCondition { get; set; }
        public string? Zone { get; set; }
        public int? AgentId { get; set; } = null!;
        public string? AgentCode { get; set; } = null!;
        //public string? AgentName { get; set; }
        //public string? ChannelCode { get; set; }
        //public string? SubChannelCode { get; set; }
        //public string? PanNumber { get; set; }
        //public string? Email { get; set; }
        //public string? MobileNo { get; set; }
        //public string? AadhaarNumber { get; set; }
        //public string? IrdaLicenseNumber { get; set; }
        //public string? GstNumber { get; set; }
        public Int64? PageNo { get; set; }
        public Int64? PageSize { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
        public bool FetchHierarchy { get; set; } = false;
    }
    public class AgentDtoResponse
    {
        public int agent_id { get; set; }
        public string agent_code { get; set; } = null!;
        public string? AgentTypeCode { get; set; }
        public string? AgentSubTypeCode { get; set; }
        public string? agent_name { get; set; }
        public string? BusinessName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Prefix { get; set; }
        public string? Suffix { get; set; }
        public string? Gender { get; set; }
        public DateTime? DOB { get; set; }
        public string? Nationality { get; set; }
        public string? MaritalStatusCode { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? ChannelCode { get; set; }
        public string? SubChannelCode { get; set; }
        public string? DesignationCode { get; set; }
        public string? AgentLevel { get; set; }
        public string? LocationCode { get; set; }
        public string? StaffCode { get; set; }
        public int? Supervisor_Id { get; set; }
        public DateTime? ContractedDate { get; set; }
        public string? AgentStatusCode { get; set; }
        public DateTime? StatusDate { get; set; }
        public bool IsLicensed { get; set; }
        public string? MaskedPanNumber { get; set; }
        public string? aadhaar_number { get; set; }
        public string? irda_license_number { get; set; }
        public string? gst_number { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? RowVersion { get; set; }
        public bool IsActive { get; set; } = true;
        public string? pan_number { get; set; }
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
        public int? total_count { get; set; }

    }
}