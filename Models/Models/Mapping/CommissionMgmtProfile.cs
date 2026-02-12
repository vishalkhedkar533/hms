using AutoMapper;
using Models.DB;
using Models.DTO;

namespace HMS.MapperProfiles
{
    // In HMS.MapperProfiles.CommissionMgmtProfile.cs
    public class CommissionMgmtProfile : Profile
    {
        public CommissionMgmtProfile()
        {
            CreateMap<Models.DB.CommissionDashboard, Models.DTO.CommissionMgmtDashboardDto>()
                .ForMember(dest => dest.OrgId, opt => opt.MapFrom(src => src.orgId))
                .ForMember(d => d.IndividualCommissions, o => o.Ignore())
                .ForMember(d => d.CycleCommissions, o => o.Ignore())
                .ForMember(d => d.AdhocCommissions, o => o.Ignore())
                .ForMember(d => d.PerformanceSnapshot, o => o.Ignore())
                .ForMember(d => d.CurrentBusinessCycles, o => o.Ignore())
                .ForMember(d => d.OnHoldPayouts, o => o.Ignore())
                .ForMember(d => d.Channels, o => o.Ignore())
                .ReverseMap();
        }
    }
}