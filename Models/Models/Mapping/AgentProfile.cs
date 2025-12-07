using AutoMapper;
using Models.DB;
using Models.DTO;

namespace Models.Mapping
{
    public class AgentProfile : Profile
    {
        public AgentProfile()
        {
            // ================================
            // Agent → AgentDto
            // ================================
            CreateMap<Agent, AgentDto>()

                // Direct scalar mappings 
                .ForMember(d => d.AgentId, o => o.MapFrom(s => s.AgentId))
                .ForMember(d => d.AgentCode, o => o.MapFrom(s => s.AgentCode))
                .ForMember(d => d.AgentTypeCode, o => o.MapFrom(s => s.AgentTypeCode))
                .ForMember(d => d.AgentSubTypeCode, o => o.MapFrom(s => s.AgentSubTypeCode))
                .ForMember(d => d.AgentName, o => o.MapFrom(s => s.AgentName))
                .ForMember(d => d.BusinessName, o => o.MapFrom(s => s.BusinessName))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForMember(d => d.MiddleName, o => o.MapFrom(s => s.MiddleName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                .ForMember(d => d.Prefix, o => o.MapFrom(s => s.Prefix))
                .ForMember(d => d.Suffix, o => o.MapFrom(s => s.Suffix))
                .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender))
                .ForMember(d => d.DOB, o => o.MapFrom(s => s.DOB))
                .ForMember(d => d.Nationality, o => o.MapFrom(s => s.Nationality))
                .ForMember(d => d.MaritalStatusCode, o => o.MapFrom(s => s.MaritalStatusCode))
                .ForMember(d => d.PreferredLanguage, o => o.MapFrom(s => s.PreferredLanguage))
                .ForMember(d => d.ChannelCode, o => o.MapFrom(s => s.ChannelCode))
                .ForMember(d => d.SubChannelCode, o => o.MapFrom(s => s.SubChannelCode))
                .ForMember(d => d.AgentLevel, o => o.MapFrom(s => s.AgentLevel))
                .ForMember(d => d.LocationCode, o => o.MapFrom(s => s.LocationCode))
                .ForMember(d => d.StaffCode, o => o.MapFrom(s => s.StaffCode))
                .ForMember(d => d.ContractedDate, o => o.MapFrom(s => s.ContractedDate))
                .ForMember(d => d.AgentStatusCode, o => o.MapFrom(s => s.AgentStatusCode))
                .ForMember(d => d.StatusDate, o => o.MapFrom(s => s.StatusDate))
                .ForMember(d => d.IsLicensed, o => o.MapFrom(s => s.IsLicensed))
                .ForMember(d => d.aadhaar_number, o => o.MapFrom(s => s.AadhaarNumber))
                .ForMember(d => d.IrdaLicenseNumber, o => o.MapFrom(s => s.IrdaLicenseNumber))
                .ForMember(d => d.GstNumber, o => o.MapFrom(s => s.GstNumber))
                .ForMember(d => d.CreatedBy, o => o.MapFrom(s => s.CreatedBy))
                .ForMember(d => d.CreatedDate, o => o.MapFrom(s => s.CreatedDate))
                .ForMember(d => d.ModifiedBy, o => o.MapFrom(s => s.ModifiedBy))
                .ForMember(d => d.ModifiedDate, o => o.MapFrom(s => s.ModifiedDate))
                .ForMember(d => d.RowVersion, o => o.MapFrom(s => s.RowVersion))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive))
                .ForMember(d => d.Supervisor_Id, o => o.MapFrom(s => s.SupervisorId))

                // PAN masking
                .ForMember(d => d.MaskedPanNumber,
                    o => o.MapFrom(s =>
                        string.IsNullOrWhiteSpace(s.PanNumber)
                            ? null
                            : s.PanNumber.Length <= 10
                                ? s.PanNumber.Substring(0, 3) + "****" + s.PanNumber.Substring(s.PanNumber.Length - 3)
                                : "*********"
                    ))


                // Reverse fields that don't exist in entity
                .ForMember(d => d.DesignationCode, o => o.Ignore())
                .ForMember(d => d.Designation, o => o.Ignore())
                .ForMember(d => d.Supervisor_Id, o => o.Ignore())
                .ForMember(d => d.Reportees, o => o.Ignore())
                .ForMember(d => d.agentAuditTrail, o => o.Ignore())
                .ForMember(d => d.peopleHeirarchy, o => o.Ignore())
                .ForMember(d => d.CandidateType, o => o.Ignore())
                .ForMember(d => d.ApplicationDocketNo, o => o.Ignore())
                .ForMember(d => d.Title, o => o.Ignore())
                .ForMember(d => d.Father_Husband_Nm, o => o.Ignore())
                .ForMember(d => d.Channel_Name, o => o.Ignore())
                .ForMember(d => d.Sub_Channel, o => o.Ignore())
                .ForMember(d => d.EmployeeCode, o => o.Ignore())
                .ForMember(d => d.StartDate, o => o.Ignore())
                .ForMember(d => d.PanAadharLinkFlag, o => o.Ignore())
                .ForMember(d => d.Sec206abFlag, o => o.Ignore())
                .ForMember(d => d.nominees, o => o.Ignore())
                .ForMember(d => d.PackageID, o => o.Ignore())
                .ForMember(d => d.personalInfo, o => o.Ignore())
                .ForMember(d => d.CommissionClass, o => o.Ignore())
                .ForMember(d => d.TaxStatus, o => o.Ignore())
                .ForMember(d => d.StateEid, o => o.Ignore())
                .ForMember(d => d.OccupationCode, o => o.Ignore())
                .ForMember(d => d.Occupation, o => o.Ignore())
                .ForMember(d => d.URN, o => o.Ignore())
                .ForMember(d => d.AdditionalComment, o => o.Ignore())
                .ForMember(d => d.AppointmentDate, o => o.Ignore())
                .ForMember(d => d.Supervisors, o => o.Ignore())

                // Any DTO-only list fields
                .ForMember(d => d.bankAccounts, o => o.Ignore())
                .ForMember(d => d.PermanentAddres, o => o.Ignore())
                .ForMember(d => d.MailingAddres, o => o.Ignore())
                //.ForMember(d => d.bankAccType, o => o.Ignore())
                //.ForMember(d => d.titles, o => o.Ignore())
                //.ForMember(d => d.genders, o => o.Ignore())
                //.ForMember(d => d.channelNames, o => o.Ignore())
                //.ForMember(d => d.subChannels, o => o.Ignore())
                //.ForMember(d => d.occupations, o => o.Ignore())
                //.ForMember(d => d.agentTypeCategories, o => o.Ignore())
                //.ForMember(d => d.agentClassifications, o => o.Ignore())
                //.ForMember(d => d.maritalStatuses, o => o.Ignore())
                //.ForMember(d => d.educationCodes, o => o.Ignore())
                //.ForMember(d => d.stateNames, o => o.Ignore())
                //.ForMember(d => d.countries, o => o.Ignore())
                .ForMember(d => d.Comments, o => o.Ignore())
                .ForMember(d => d.Reason, o => o.Ignore())
                .ForMember(d => d.OrgId, o => o.Ignore())
                .ForMember(d => d.AddressLine1, o => o.Ignore())
                .ForMember(d => d.AddressLine2, o => o.Ignore())
                .ForMember(d => d.AddressLine3, o => o.Ignore())
                .ForMember(d => d.City, o => o.Ignore())
                .ForMember(d => d.State, o => o.Ignore())
                .ForMember(d => d.Country, o => o.Ignore())
                .ForMember(d => d.Pin, o => o.Ignore())
                .ForMember(d => d.Landmark, o => o.Ignore())
                .ForMember(d => d.Supervisor_Code, o => o.Ignore());


            // ================================
            // AgentDto → Agent
            // ================================
            CreateMap<AgentDto, Agent>()

                .ForMember(d => d.AgentId, o => o.MapFrom(s => s.AgentId))
                .ForMember(d => d.AgentCode, o => o.MapFrom(s => s.AgentCode))
                .ForMember(d => d.AgentTypeCode, o => o.MapFrom(s => s.AgentTypeCode))
                .ForMember(d => d.AgentSubTypeCode, o => o.MapFrom(s => s.AgentSubTypeCode))
                .ForMember(d => d.AgentName, o => o.MapFrom(s => s.AgentName))
                .ForMember(d => d.BusinessName, o => o.MapFrom(s => s.BusinessName))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForMember(d => d.MiddleName, o => o.MapFrom(s => s.MiddleName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                .ForMember(d => d.Prefix, o => o.MapFrom(s => s.Prefix))
                .ForMember(d => d.Suffix, o => o.MapFrom(s => s.Suffix))
                .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender))
                .ForMember(d => d.DOB, o => o.MapFrom(s => s.DOB))
                .ForMember(d => d.Nationality, o => o.MapFrom(s => s.Nationality))
                .ForMember(d => d.MaritalStatusCode, o => o.MapFrom(s => s.MaritalStatusCode))
                .ForMember(d => d.PreferredLanguage, o => o.MapFrom(s => s.PreferredLanguage))
                .ForMember(d => d.ChannelCode, o => o.MapFrom(s => s.ChannelCode))
                .ForMember(d => d.SubChannelCode, o => o.MapFrom(s => s.SubChannelCode))
                .ForMember(d => d.AgentLevel, o => o.MapFrom(s => s.AgentLevel))
                .ForMember(d => d.LocationCode, o => o.MapFrom(s => s.LocationCode))
                .ForMember(d => d.StaffCode, o => o.MapFrom(s => s.StaffCode))
                .ForMember(d => d.ContractedDate, o => o.MapFrom(s => s.ContractedDate))
                .ForMember(d => d.AgentStatusCode, o => o.MapFrom(s => s.AgentStatusCode))
                .ForMember(d => d.StatusDate, o => o.MapFrom(s => s.StatusDate))
                .ForMember(d => d.IsLicensed, o => o.MapFrom(s => s.IsLicensed))

                // IMPORTANT: prevent corruption
                .ForMember(d => d.PanNumber, o => o.Ignore())

                .ForMember(d => d.AadhaarNumber, o => o.MapFrom(s => s.aadhaar_number))
                .ForMember(d => d.IrdaLicenseNumber, o => o.MapFrom(s => s.IrdaLicenseNumber))
                .ForMember(d => d.GstNumber, o => o.MapFrom(s => s.GstNumber))

                .ForMember(d => d.RowVersion, o => o.MapFrom(s => s.RowVersion))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive))

                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.CreatedDate, o => o.Ignore())
                .ForMember(d => d.Supervisor, o => o.Ignore())

                .ForMember(d => d.ModifiedBy, o => o.Ignore())
                .ForMember(d => d.ModifiedDate, o => o.Ignore())
                .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
                .ForMember(d => d.MobileNo, o => o.MapFrom(s => s.MobileNo));
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