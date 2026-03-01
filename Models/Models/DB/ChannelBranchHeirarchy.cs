using AutoMapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("channel_branch_heirarchy", Schema = "hmsmaster")]
    public class ChannelBranchHeirarchy
    {
        [Key]
        [Column("channel_location_heirarchy_id")]
        public long ChannelLocationHeirarchyId { get; set; }
        [Column("orgid")]
        public int? OrgId { get; set; }

        [Required]
        [Column("channel_id")]
        public long ChannelId { get; set; }

        [Column("sub_channel_id")]
        public long? SubChannelId { get; set; }

        // ltree is typically mapped to string in C#
        [Column("hierarchy_path")]
        public string? HierarchyPath { get; set; }

        [Required]
        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Required]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("modified_by")]
        public int? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Required]
        [Column("effective_from_date")]
        public DateTime EffectiveFromDate { get; set; }

        [Column("effective_to_date")]
        public DateTime? EffectiveToDate { get; set; }

        [Column("branch_id")]
        public long? BranchId { get; set; }
    }
    public class ChannelBranchHierarchyDto
    {
        public long? ChannelLocationHierarchyId { get; set; }

        public int? OrgId { get; set; }

        public long? ChannelId { get; set; }

        public long? SubChannelId { get; set; }

        public string? HierarchyPath { get; set; }

        public DateTime? EffectiveFromDate { get; set; }

        public DateTime? EffectiveToDate { get; set; }

        public long? BranchId { get; set; }
    }
    public class ChannelBranchHierarchyProfile : Profile
    {
        public ChannelBranchHierarchyProfile()
        {
            // CreateMap<Source, Destination>
            CreateMap<ChannelBranchHeirarchy, ChannelBranchHierarchyDto>()
                // The fields below exist in the Entity but not in the DTO, so we ignore them.
                .ForMember(dest => dest.ChannelLocationHierarchyId, opt => opt.MapFrom(src => src.ChannelLocationHeirarchyId))
                .ForMember(dest => dest.OrgId, opt => opt.MapFrom(src => src.OrgId))
                .ForMember(dest => dest.ChannelId, opt => opt.MapFrom(src => src.ChannelId))
                .ForMember(dest => dest.SubChannelId, opt => opt.MapFrom(src => src.SubChannelId))
                .ForMember(dest => dest.HierarchyPath, opt => opt.MapFrom(src => src.HierarchyPath))
                .ForMember(dest => dest.EffectiveFromDate, opt => opt.MapFrom(src => src.EffectiveFromDate))
                .ForMember(dest => dest.EffectiveToDate, opt => opt.MapFrom(src => src.EffectiveToDate))
                .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId))
                // Explicitly ignoring fields that don't exist in the DTO if you do a ReverseMap
                .ReverseMap()
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore());
        }
    }
}