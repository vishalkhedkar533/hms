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
                .ForMember(dest => dest.CandidateType, opt => opt.MapFrom(src => src.CandidateType))
                .ForMember(dest => dest.ApplicationDocketNo, opt => opt.MapFrom(src => src.ApplicationDocketNo))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Father_Husband_Nm, opt => opt.MapFrom(src => src.Father_Husband_Nm))
                .ForMember(dest => dest.Channel_Name, opt => opt.MapFrom(src => src.Channel_Name))
                .ForMember(dest => dest.Sub_Channel, opt => opt.MapFrom(src => src.Sub_Channel))
                .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.EmployeeCode))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.PanAadharLinkFlag, opt => opt.MapFrom(src => src.PanAadharLinkFlag))
                .ForMember(dest => dest.Sec206abFlag, opt => opt.MapFrom(src => src.Sec206abFlag))
                .ForMember(dest => dest.nominees, opt => opt.Ignore())
                .ForMember(dest => dest.personalInfo, opt => opt.Ignore())
                .ForMember(dest => dest.PackageID, opt => opt.MapFrom(src => src.PackageID))
                .ForMember(dest => dest.CommissionClass, opt => opt.MapFrom(src => src.CommissionClass))
                .ForMember(dest => dest.TaxStatus, opt => opt.MapFrom(src => src.TaxStatus))
                .ForMember(dest => dest.StateEid, opt => opt.MapFrom(src => src.StateEid))
                .ForMember(dest => dest.OccupationCode, opt => opt.MapFrom(src => src.OccupationCode))
                .ForMember(dest => dest.Occupation, opt => opt.MapFrom(src => src.Occupation))
                .ForMember(dest => dest.URN, opt => opt.MapFrom(src => src.URN))
                .ForMember(dest => dest.AdditionalComment, opt => opt.MapFrom(src => src.AdditionalComment))
                .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.AppointmentDate))
                .ForMember(dest => dest.IncorporationDate, opt => opt.MapFrom(src => src.IncorporationDate))
                .ForMember(dest => dest.CnctPersonDesig, opt => opt.MapFrom(src => src.CnctPersonDesig))
                .ForMember(dest => dest.CnctPersonMobileNo, opt => opt.MapFrom(src => src.CnctPersonMobileNo))
                .ForMember(dest => dest.CnctPersonEmail, opt => opt.MapFrom(src => src.CnctPersonEmail))
                .ForMember(dest => dest.CnctPersonName, opt => opt.MapFrom(src => src.CnctPersonName))
                .ForMember(dest => dest.AgentTypeCategory, opt => opt.MapFrom(src => src.AgentTypeCategory))
                .ForMember(dest => dest.AgentClassification, opt => opt.MapFrom(src => src.AgentClassification))
                .ForMember(dest => dest.CMSAgentType, opt => opt.MapFrom(src => src.CMSAgentType))
                .ForMember(dest => dest.bankAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.PermanentAddres, opt => opt.Ignore())
                .ForMember(dest => dest.MailingAddres, opt => opt.Ignore())
                .ForMember(dest => dest.Supervisors, opt => opt.Ignore())
                .ForMember(dest => dest.Reportees, opt => opt.Ignore())
                .ForMember(dest => dest.agentAuditTrail, opt => opt.Ignore())
                .ForMember(dest => dest.peopleHeirarchy, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceTaxNo, opt => opt.MapFrom(src => src.ServiceTaxNo))
                .ForMember(dest => dest.UlipFlag, opt => opt.MapFrom(src => src.UlipFlag))
                .ForMember(dest => dest.TrainingGroupType, opt => opt.MapFrom(src => src.TrainingGroupType))
                .ForMember(dest => dest.Ifs, opt => opt.MapFrom(src => src.Ifs))
                .ForMember(dest => dest.RefresherTrainingCompleted, opt => opt.MapFrom(src => src.RefresherTrainingCompleted))
                .ForMember(dest => dest.IsMigrated, opt => opt.MapFrom(src => src.IsMigrated))
                .ForMember(dest => dest.MainPartnerClientCode, opt => opt.MapFrom(src => src.MainPartnerClientCode))
                .ForMember(dest => dest.AgentMaincodevwEid, opt => opt.MapFrom(src => src.AgentMaincodevwEid))
                .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.RegistrationDate))
                .ForMember(dest => dest.Vertical, opt => opt.MapFrom(src => src.Vertical))
                .ForMember(dest => dest.BranchCode, opt => opt.MapFrom(src => src.BranchCode))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.BranchName))
                .ForMember(dest => dest.Ic36TrngCompletionDate, opt => opt.MapFrom(src => src.Ic36TrngCompletionDate))
                .ForMember(dest => dest.STrngCompletionDate, opt => opt.MapFrom(src => src.STrngCompletionDate))
                .ForMember(dest => dest.ConfirmationDate, opt => opt.MapFrom(src => src.ConfirmationDate))
                .ForMember(dest => dest.FgRockstarTrainingDate, opt => opt.MapFrom(src => src.FgRockstarTrainingDate))
                .ForMember(dest => dest.IncrementDate, opt => opt.MapFrom(src => src.IncrementDate))
                .ForMember(dest => dest.LastPromotionDate, opt => opt.MapFrom(src => src.LastPromotionDate))
                .ForMember(dest => dest.HRDoj, opt => opt.MapFrom(src => src.HRDoj))
                .ForMember(dest => dest.FgValueTrngDate, opt => opt.MapFrom(src => src.FgValueTrngDate))
                .ForMember(dest => dest.HSecPolicyTrngDate, opt => opt.MapFrom(src => src.HSecPolicyTrngDate))
                .ForMember(dest => dest.ItSecPolicyTrngDate, opt => opt.MapFrom(src => src.ItSecPolicyTrngDate))
                .ForMember(dest => dest.NpsTrngCompletionDate, opt => opt.MapFrom(src => src.NpsTrngCompletionDate))
                .ForMember(dest => dest.WhistleBlowerTrngDate, opt => opt.MapFrom(src => src.WhistleBlowerTrngDate))
                .ForMember(dest => dest.GovPolicyTrngDate, opt => opt.MapFrom(src => src.GovPolicyTrngDate))
                .ForMember(dest => dest.InductionTrngDate, opt => opt.MapFrom(src => src.InductionTrngDate))
                .ForMember(dest => dest.LastWorkingDate, opt => opt.MapFrom(src => src.LastWorkingDate))
                .ForMember(dest => dest.LicenseNo, opt => opt.MapFrom(src => src.LicenseNo))
                .ForMember(dest => dest.LicenseType, opt => opt.MapFrom(src => src.LicenseType))
                .ForMember(dest => dest.LicenseIssueDate, opt => opt.MapFrom(src => src.LicenseIssueDate))
                .ForMember(dest => dest.LicenseExpiryDate, opt => opt.MapFrom(src => src.LicenseExpiryDate))
                .ForMember(dest => dest.LicenseStatus, opt => opt.MapFrom(src => src.LicenseStatus))
                .ForMember(dest => dest.bankAccType, opt => opt.Ignore())
                .ForMember(dest => dest.titles, opt => opt.Ignore())
                .ForMember(dest => dest.genders, opt => opt.Ignore())
                .ForMember(dest => dest.channelNames, opt => opt.Ignore())
                .ForMember(dest => dest.subChannels, opt => opt.Ignore())
                .ForMember(dest => dest.occupations, opt => opt.Ignore())
                .ForMember(dest => dest.agentTypeCategories, opt => opt.Ignore())
                .ForMember(dest => dest.agentClassifications, opt => opt.Ignore())
                .ForMember(dest => dest.maritalStatuses, opt => opt.Ignore())
                .ForMember(dest => dest.educationCodes, opt => opt.Ignore())
                .ForMember(dest => dest.stateNames, opt => opt.Ignore())
                .ForMember(dest => dest.countries, opt => opt.Ignore())
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