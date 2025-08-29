namespace Models.DTO
{
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
        public string? AgentLevel { get; set; }
        public string? LocationCode { get; set; }
        public string? StaffCode { get; set; }
        public int? Supervisor_Id { get; set; }
        public DateTime? ContractedDate { get; set; }
        public string? AgentStatusCode { get; set; }
        public DateTime? StatusDate { get; set; }
        public bool IsLicensed { get; set; }
        public string? PanNumber { get; set; }
        public string? AadhaarNumber { get; set; }
        public string? IrdaLicenseNumber { get; set; }
        public string? GstNumber { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? RowVersion { get; set; }
        public bool IsActive { get; set; } = true;
    }
}