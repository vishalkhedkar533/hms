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
                .ForMember(d => d.DOB, o => o.MapFrom(s => s.Dob))
                .ForMember(d => d.Nationality, o => o.MapFrom(s => s.Nationality))
                .ForMember(d => d.MaritalStatus, o => o.MapFrom(s => s.MaritalStatus))
                .ForMember(d => d.PreferredLanguage, o => o.MapFrom(s => s.PreferredLanguage))
                .ForMember(d => d.Channel, o => o.MapFrom(s => s.Channel))
                .ForMember(d => d.SubChannel, o => o.MapFrom(s => s.SubChannel))
                .ForMember(d => d.AgentLevel, o => o.MapFrom(s => s.AgentLevel))
                .ForMember(d => d.LocationCode, o => o.MapFrom(s => s.LocationCode))
                .ForMember(d => d.StaffCode, o => o.MapFrom(s => s.StaffCode))
                .ForMember(d => d.ContractedDate, o => o.MapFrom(s => s.ContractedDate.HasValue ? s.ContractedDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null))
                .ForMember(d => d.AgentStatusCode, o => o.MapFrom(s => s.AgentStatusCode))
                .ForMember(d => d.StatusDate, o => o.MapFrom(s => s.StatusDate.HasValue ? s.StatusDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null))
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
                .ForMember(d => d.DesignationCode, o => o.MapFrom(s => s.DesignationCode))
                .ForMember(d => d.Channel, o => o.MapFrom(s => s.Channel))
                .ForMember(d => d.SubChannel, o => o.MapFrom(s => s.SubChannel))
                .ForMember(d => d.Occupation, o => o.MapFrom(s => s.Occupation))
                .ForMember(d => d.AgentTypeCode, o => o.MapFrom(s => s.AgentTypeCode))
                .ForMember(d => d.AgentTypeCat, o => o.MapFrom(s => s.AgentTypeCat))
                .ForMember(d => d.AgentClass, o => o.MapFrom(s => s.AgentClass))
                .ForMember(d => d.CandidateType, o => o.MapFrom(s => s.CandidateType))
                .ForMember(d => d.Title, o => o.MapFrom(s => s.Title))
                .ForMember(d => d.CommissionClass, o => o.MapFrom(s => s.CommissionClass))
                .ForMember(d => d.AgentType, o => o.MapFrom(s => s.AgentType))
                .ForMember(d => d.Country, o => o.MapFrom(s => s.Country))
                .ForMember(d => d.Father_Husband_Nm, o => o.MapFrom(s => s.FatherHusbandNm))
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
                .ForMember(d => d.Supervisor_Id, o => o.Ignore())
                .ForMember(d => d.Reportees, o => o.Ignore())
                .ForMember(d => d.agentAuditTrail, o => o.Ignore())
                .ForMember(d => d.peopleHeirarchy, o => o.Ignore())
                .ForMember(d => d.ApplicationDocketNo, o => o.Ignore())
                .ForMember(d => d.EmployeeCode, o => o.Ignore())
                .ForMember(d => d.StartDate, o => o.Ignore())
                .ForMember(d => d.PanAadharLinkFlag, o => o.Ignore())
                .ForMember(d => d.Sec206abFlag, o => o.Ignore())
                .ForMember(d => d.nominees, o => o.Ignore())
                .ForMember(d => d.PackageID, o => o.Ignore())
                .ForMember(d => d.personalInfo, o => o.Ignore())
                .ForMember(d => d.TaxStatus, o => o.Ignore())
                .ForMember(d => d.StateEid, o => o.Ignore())
                .ForMember(d => d.URN, o => o.Ignore())
                .ForMember(d => d.AdditionalComment, o => o.Ignore())
                .ForMember(d => d.AppointmentDate, o => o.Ignore())
                .ForMember(d => d.Supervisors, o => o.Ignore())

                // Any DTO-only list fields
                .ForMember(d => d.bankAccounts, o => o.Ignore())
                .ForMember(d => d.PermanentAddres, o => o.Ignore())
                .ForMember(d => d.MailingAddres, o => o.Ignore())
                .ForMember(d => d.Comments, o => o.Ignore())
                .ForMember(d => d.Reason, o => o.Ignore())
                .ForMember(d => d.OrgId, o => o.Ignore())
                .ForMember(d => d.AddressLine1, o => o.Ignore())
                .ForMember(d => d.AddressLine2, o => o.Ignore())
                .ForMember(d => d.AddressLine3, o => o.Ignore())
                .ForMember(d => d.City, o => o.Ignore())
                .ForMember(d => d.State, o => o.Ignore())
                .ForMember(d => d.Pin, o => o.Ignore())
                .ForMember(d => d.Landmark, o => o.Ignore())
                .ForMember(d => d.Supervisor_Code, o => o.Ignore())
                .ForMember(d => d.AgentClassDesc, s => s.Ignore())
                .ForMember(d => d.DesignationCodeDesc, s => s.Ignore())
                .ForMember(d => d.CountryDesc, s => s.Ignore())
                .ForMember(d => d.StateDesc, s => s.Ignore())
                .ForMember(d => d.EducationDesc, s => s.Ignore())
                .ForMember(d => d.MaritalStatusDesc, s => s.Ignore())
                .ForMember(d => d.AgentTypeCatDesc, s => s.Ignore())
                .ForMember(d => d.OccupationDesc, s => s.Ignore())
                .ForMember(d => d.SubChannelDesc, s => s.Ignore())
                .ForMember(d => d.ChannelDesc, s => s.Ignore())
                .ForMember(d => d.TitleDesc, s => s.Ignore())
                .ForMember(d => d.GenderDesc, s => s.Ignore())
                .ForMember(d => d.BankAccTypeDesc, s => s.Ignore())
                .ForMember(d => d.AgentSubTypeCodeDesc, s => s.Ignore())
                .ForMember(d => d.AgentTypeCodeDesc, s => s.Ignore())
                .ForMember(d => d.LocationCodeDesc, s => s.Ignore())
                .ForMember(d => d.CandidateTypeDesc, s => s.Ignore())
                .ForMember(d => d.AgentTypeDesc, s => s.Ignore())
                .ForMember(d => d.CommissionClassDesc, s => s.Ignore())
                .ForMember(d => d.LicenceType, o => o.MapFrom(s => s.LicenseType))
                .ForMember(d => d.LicenceStatus, o => o.MapFrom(s => s.LicenseStatus))
                .ForMember(d => d.LicenceTypeDesc, o => o.Ignore())
                .ForMember(d => d.LicenceStatusDesc, o => o.Ignore())
                .ForMember(d => d.VerticalDesc, o => o.Ignore())
                .ForMember(d => d.TrainingGroupTypeDesc, o => o.Ignore())
                
                ;
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
                .ForMember(d => d.Dob, o => o.MapFrom(s => s.DOB))
                .ForMember(d => d.Nationality, o => o.MapFrom(s => s.Nationality))
                .ForMember(d => d.MaritalStatus, o => o.MapFrom(s => s.MaritalStatus))
                .ForMember(d => d.PreferredLanguage, o => o.MapFrom(s => s.PreferredLanguage))
                .ForMember(d => d.Channel, o => o.MapFrom(s => s.Channel))
                .ForMember(d => d.SubChannel, o => o.MapFrom(s => s.SubChannel))
                .ForMember(d => d.AgentLevel, o => o.MapFrom(s => s.AgentLevel))
                .ForMember(d => d.LocationCode, o => o.MapFrom(s => s.LocationCode))
                .ForMember(d => d.StaffCode, o => o.MapFrom(s => s.StaffCode))
                .ForMember(d => d.ContractedDate, o => o.MapFrom(s => s.ContractedDate.HasValue ? DateOnly.FromDateTime(s.ContractedDate.Value) : (DateOnly?)null))
                .ForMember(d => d.AgentStatusCode, o => o.MapFrom(s => s.AgentStatusCode))
                .ForMember(d => d.StatusDate, o => o.MapFrom(s => s.StatusDate.HasValue ? DateOnly.FromDateTime(s.StatusDate.Value) : (DateOnly?)null))
                .ForMember(d => d.IsLicensed, o => o.MapFrom(s => s.IsLicensed))
                .ForMember(d => d.FatherHusbandNm, o => o.MapFrom(s => s.Father_Husband_Nm))
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
                .ForMember(d => d.MobileNo, o => o.MapFrom(s => s.MobileNo))
                .ForMember(d => d.CandidateType, o => o.MapFrom(s => s.CandidateType))
                .ForMember(d => d.Title, o => o.MapFrom(s => s.Title))
                .ForMember(d => d.CommissionClass, o => o.MapFrom(s => s.CommissionClass))
                .ForMember(d => d.AgentType, o => o.MapFrom(s => s.AgentType))
                .ForMember(d => d.Country, o => o.MapFrom(s => s.Country))

                .ForMember(d => d.LicenseType, o => o.MapFrom(s => s.LicenceType))
                .ForMember(d => d.LicenseStatus, o => o.MapFrom(s => s.LicenceStatus))
                .ForMember(d => d.Vertical, o => o.MapFrom(s => s.Vertical))
                .ForMember(d => d.TrainingGroupType, o => o.MapFrom(s => s.TrainingGroupType))
                ;
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