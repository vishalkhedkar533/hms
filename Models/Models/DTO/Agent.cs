using AutoMapper;
using Models.DB;
using Models.Enums;

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
        public string CandidateType { get; set; }  = "Corporate";
        public string ApplicationDocketNo { get; set; } = "AppDoc001";
        public string Title { get; set; } = "Mr.";
        public string Father_Husband_Nm { get; set; } = "Father_Husband_Nm";
        public string Channel_Name { get; set; } = "Channel_Name";
        public string Sub_Channel { get; set; } = "Sub_Channel";
        public string EmployeeCode { get; set; } = "EMP001";
        public DateTime? StartDate { get; set; } = DateTime.Now;
        public bool PanAadharLinkFlag { get; set; } = false;
        public bool Sec206abFlag { get; set; } = false;
        public List<Nominee> nominees { get; set; } = new List<Nominee>();
        public string? PackageID { get; set; } = "PackageID";
        public PersonalInfo? personalInfo { get; set; } = new PersonalInfo()
        {
            DateOfBirth = DateTime.Now,
            Email = "anc@gmail.com",
            FirstName = "FirstName",
            Id = 0,
            LastName = "LastName",
            MobileNo = "9833982266",
            BloodGroup = "O -ve",
            PanNumber = "ERTYU4444K"
        };
        public string? CommissionClass  { get; set; } = "CommissionClass";
        public string? TaxStatus  { get; set; } = "TaxStatus";
        public string? StateEid  { get; set; } = "StateEid";
        public int? OccupationCode  { get; set; } = 0;
        public String? Occupation  { get; set; } = "Occupation";
        public string? URN  { get; set; } = "URN";
        public string? AdditionalComment  { get; set; } = "AdditionalComment";
        public DateTime? AppointmentDate  { get; set; } = DateTime.Now;
        public DateTime? IncorporationDate  { get; set; } = DateTime.Now;
        public string? CnctPersonDesig  { get; set; } = "Designation";
        public string? CnctPersonMobileNo  { get; set; } = "9833982266";
        public string? CnctPersonEmail  { get; set; } = "contactperson@gmail.com";
        public string? CnctPersonName  { get; set; } = "Contact Person Name";
        public string? AgentTypeCategory  { get; set; } = "AgentTypeCategory";
        public string? AgentClassification  { get; set; } = "AgentClassification";
        public string? CMSAgentType  { get; set; } = "CMSAgentType";
        public List<BankAccount>? bankAccounts { get; set; } = new() {
            new BankAccount() {
            AccountHolderName = "AccountHolderName",
            AccountNumber = "1234567890",
            AccountType = Enums.BankAccType.Savings,
            BankName = "Bank Name",
            BranchName = "Branch Name",
            ActiveSince = DateTime.Now,
            FactoringHouse = "FactoringHouse",
            Id = 1000,
            IFSC = "123456789",
            MICR = "789563231",
            preferredPaymentMode = Enums.PreferredPaymentMode.Wallet
        }
        };
        public string? ServiceTaxNo { get; set; } = "456789123";
        public Address? PermanentAddres { get; set; } = new Address()
        {
            AddressID = 1000,
            AddressLine1 = "Perm Address Line 1",
            AddressLine2 = "Perm Address Line 2",
            AddressLine3 = "Perm Address Line 3",
            AddressType = AddressType.Permanent,
        };
        public Address? MailingAddres { get; set; } = new Address()
        {
            AddressID = 1000,
            AddressLine1 = "Mailing Address Line 1",
            AddressLine2 = "Mailing Address Line 2",
            AddressLine3 = "Mailing Address Line 3",
            AddressType = AddressType.Correspondence_1,
        };

        public bool UlipFlag { get; set; } = false;
        public string? TrainingGroupType { get; set; } = "DefaultTrainingGroup";
        public string? Ifs { get; set; } = "IFS Default";
        public bool RefresherTrainingCompleted { get; set; } = false;
        public bool IsMigrated { get; set; } = false;
        public string? MainPartnerClientCode { get; set; } = "MPClient001";
        public string? AgentMaincodevwEid { get; set; } = "EID001";
        public DateTime? RegistrationDate { get; set; } = DateTime.Now;
        public string? Vertical { get; set; } = "DefaultVertical";

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