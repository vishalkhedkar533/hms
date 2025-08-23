using AutoMapper;
using Models.DB;
using Models.DTO;

namespace Models.Mapping
{
    public class AgentProfile : Profile
    {
        public AgentProfile()
        {
            CreateMap<AgentDto, Agent>().ReverseMap();
        }
    }
}