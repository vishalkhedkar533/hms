using AutoMapper;
using Models.DB;
using Models.DTO;

namespace Models.Mapping
{
    public class AgentProfile : Profile
    {
        public AgentProfile()
        {
            CreateMap<Agent, AgentDto>()
                // ---- Forward: Agent → AgentDto ----
                .ForMember(dest => dest.Supervisor_Id, opt => opt.MapFrom(src => src.SupervisorId))
                .ForMember(dest => dest.aadhaar_number, opt => opt.MapFrom(src => src.AadhaarNumber))
                .ForMember(dest => dest.MaskedPanNumber, opt => opt.MapFrom(src => MaskPan(src.PanNumber)))
                .ForMember(dest => dest.DesignationCode, opt => opt.MapFrom(src => src.DesignationCode))
                .ForMember(dest => dest.Designation, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateType, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationDocketNo, opt => opt.Ignore())
                .ForMember(dest => dest.Title, opt => opt.Ignore())
                .ForMember(dest => dest.Father_Husband_Nm, opt => opt.Ignore())
                .ForMember(dest => dest.Channel_Name, opt => opt.Ignore())
                .ForMember(dest => dest.Sub_Channel, opt => opt.Ignore())
                .ForMember(dest => dest.EmployeeCode, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.PanAadharLinkFlag, opt => opt.Ignore())
                .ForMember(dest => dest.Sec206abFlag, opt => opt.Ignore())
                .ForMember(dest => dest.nominees, opt => opt.Ignore())
                .ForMember(dest => dest.PackageID, opt => opt.Ignore())
                .ForMember(dest => dest.personalInfo, opt => opt.Ignore())
                .ForMember(dest => dest.CommissionClass, opt => opt.Ignore())
                .ForMember(dest => dest.TaxStatus, opt => opt.Ignore())
                .ForMember(dest => dest.StateEid, opt => opt.Ignore())
                .ForMember(dest => dest.OccupationCode, opt => opt.Ignore())
                .ForMember(dest => dest.Occupation, opt => opt.Ignore())
                .ForMember(dest => dest.URN, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalComment, opt => opt.Ignore())
                .ForMember(dest => dest.AppointmentDate, opt => opt.Ignore())
                .ForMember(dest => dest.IncorporationDate, opt => opt.Ignore())
                .ForMember(dest => dest.CnctPersonDesig, opt => opt.Ignore())
                .ForMember(dest => dest.CnctPersonMobileNo, opt => opt.Ignore())
                .ForMember(dest => dest.CnctPersonEmail, opt => opt.Ignore())
                .ForMember(dest => dest.CnctPersonName, opt => opt.Ignore())
                .ForMember(dest => dest.AgentTypeCategory, opt => opt.Ignore())
                .ForMember(dest => dest.AgentClassification, opt => opt.Ignore())
                .ForMember(dest => dest.CMSAgentType, opt => opt.Ignore())
                .ForMember(dest => dest.bankAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceTaxNo, opt => opt.Ignore())
                .ForMember(dest => dest.PermanentAddres, opt => opt.Ignore())
                .ForMember(dest => dest.MailingAddres, opt => opt.Ignore())
                .ForMember(dest => dest.Supervisors, opt => opt.Ignore())
                .ForMember(dest => dest.Reportees, opt => opt.Ignore())
                .ForMember(dest => dest.agentAuditTrail, opt => opt.Ignore())
                .ForMember(dest => dest.peopleHeirarchy, opt => opt.Ignore())
                // ---- Reverse: AgentDto → Agent ----
                .ReverseMap()
                .ForMember(dest => dest.SupervisorId, opt => opt.MapFrom(src => src.Supervisor_Id))
                .ForMember(dest => dest.AadhaarNumber, opt => opt.MapFrom(src => src.aadhaar_number))
                .ForMember(dest => dest.PanNumber, opt => opt.MapFrom(src => src.MaskedPanNumber))
                .ForMember(dest => dest.Supervisor, opt => opt.Ignore())
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                // ✅ Safe for both directions
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
        private static string? MaskPan(string? panNumber)
        {
            if (string.IsNullOrWhiteSpace(panNumber) || panNumber.Length < 10)
                return panNumber;

            // Example mask: ABCDE1234F → ABC**1234F
            return $"{panNumber.Substring(0, 3)}**{panNumber.Substring(5)}";
        }
    }
    public class AuditTrailProfile : Profile
    {
        public AuditTrailProfile()
        {
            CreateMap<AgentAuditTrailDTO, AgentAuditTrail>()
                .ForMember(dest => dest.AuditId, opt => opt.Ignore())
                .ForMember(dest => dest.ChangedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ChangedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
                .ForMember(dest => dest.Agent, opt => opt.Ignore())
                .ReverseMap()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}