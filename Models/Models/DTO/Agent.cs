using Models.DB;
using CommonLibrary;

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
        public string? PanNumber { get; set; }
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
    }

    public static class AgentMapper
    {
        public static AgentDto ToDto(Agent agent)
        {
            return new AgentDto
            {
                AgentId = agent.AgentId,
                AgentCode = agent.AgentCode,
                AgentName = agent.AgentName,
                AgentTypeCode = agent.AgentTypeCode,
                AgentSubTypeCode = agent.AgentSubTypeCode,
                BusinessName = agent.BusinessName,
                FirstName = agent.FirstName,
                MiddleName = agent.MiddleName,
                LastName = agent.LastName,
                Prefix = agent.Prefix,
                Suffix = agent.Suffix,
                SubChannelCode = agent.SubChannelCode,
                ChannelCode = agent.ChannelCode,
                aadhaar_number = agent.AadhaarNumber,
                IrdaLicenseNumber = agent.IrdaLicenseNumber,
                GstNumber = agent.GstNumber,
                AgentLevel = agent.AgentLevel,
                DesignationCode = agent.DesignationCode,
                LocationCode = agent.LocationCode,
                StaffCode = agent.StaffCode,
                Supervisor_Id = agent.SupervisorId,
                AgentStatusCode = agent.AgentStatusCode,
                StatusDate = agent.StatusDate,
                IsLicensed = agent.IsLicensed,
                CreatedBy = agent.CreatedBy,
                CreatedDate = agent.CreatedDate,
                ModifiedBy = agent.ModifiedBy,
                ModifiedDate = agent.ModifiedDate,
                RowVersion = agent.RowVersion,
                IsActive = agent.IsActive,
                DOB = agent.DOB,
                ContractedDate = agent.ContractedDate,
                Gender = agent.Gender,
                MaritalStatusCode = agent.MaritalStatusCode,
                Nationality = agent.Nationality,
                PreferredLanguage = agent.PreferredLanguage,
                MaskedPanNumber = MaskingHelper.MaskPan(agent.PanNumber),
                PanNumber = string.Empty,
                Email = agent.Email,
                MobileNo = agent.MobileNo
            };
        }
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
        public string? AgentCode { get; set; } = null!;
        public string? AgentName { get; set; }
        public string? ChannelCode { get; set; }
        public string? SubChannelCode { get; set; }
        public string? PanNumber { get; set; }
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
        public string? AadhaarNumber { get; set; }
        public string? IrdaLicenseNumber { get; set; }
        public string? GstNumber { get; set; }
        public Int64? PageNo { get; set; }
        public Int64? PageSize { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }

    }
}