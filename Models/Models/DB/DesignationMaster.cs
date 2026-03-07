using AutoMapper;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("designation_master", Schema = "hmsmaster")]
    public class DesignationMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("designation_id")]
        public long DesignationId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("designation_code")]
        public string DesignationCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("designation_name")]
        public string DesignationName { get; set; } = string.Empty;

        [Column("designation_level")]
        public int? DesignationLevel { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [MaxLength(100)]
        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        public int? Rowversion { get; set; }

        [Column("channel_id")]
        public long? ChannelId { get; set; }

        [Column("orgid")]
        public int? OrgId { get; set; }

        [Column("hierarchy_path")]
        public string? HierarchyPath { get; set; }

        [MaxLength(5)]
        [Column("code_format")]
        public string? CodeFormat { get; set; }

        [Column("sub_channel_id")]
        public long? SubChannelId { get; set; }
    }
    public class DesignationMasterDto
    {
        public long? DesignationId { get; set; }
        public long? ParentDesignationId { get; set; }
        public string DesignationCode { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;
        public int? DesignationLevel { get; set; }
        public bool IsActive { get; set; } = true;
        public long? ChannelId { get; set; }
        [SwaggerSchema("Code will will be prefixed during code generation")]
        public string CodeFormat { get; set; }
        public long? SubChannelId { get; set; }
        public string? HierarchyPath { get; set; }
    }
    public class DesignationMasterProfile : Profile
    {
        public DesignationMasterProfile()
        {
            // 1. Entity -> DTO (Reading Data)
            CreateMap<DesignationMaster, DesignationMasterDto>()
                .ForMember(dest => dest.DesignationId, opt => opt.MapFrom(src => src.DesignationId))
                .ForMember(dest => dest.DesignationCode, opt => opt.MapFrom(src => src.DesignationCode))
                .ForMember(dest => dest.DesignationName, opt => opt.MapFrom(src => src.DesignationName))
                .ForMember(dest => dest.DesignationLevel, opt => opt.MapFrom(src => src.DesignationLevel))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.ChannelId, opt => opt.MapFrom(src => src.ChannelId))
                .ForMember(dest => dest.SubChannelId, opt => opt.MapFrom(src => src.SubChannelId))
                .ForMember(dest => dest.CodeFormat, opt => opt.MapFrom(src => src.CodeFormat))
                .ForMember(dest => dest.HierarchyPath, opt => opt.MapFrom(src => src.HierarchyPath))
                // ParentDesignationId requires manual parsing of HierarchyPath or is not directly mapped
                .ForMember(dest => dest.ParentDesignationId, opt => opt.Ignore());

            // 2. DTO -> Entity (Writing/Updating Data)
            CreateMap<DesignationMasterDto, DesignationMaster>()
                // Map only the fields that the user is allowed to set/change
                .ForMember(dest => dest.DesignationCode, opt => opt.MapFrom(src => src.DesignationCode))
                .ForMember(dest => dest.DesignationName, opt => opt.MapFrom(src => src.DesignationName))
                .ForMember(dest => dest.DesignationLevel, opt => opt.MapFrom(src => src.DesignationLevel))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.ChannelId, opt => opt.MapFrom(src => src.ChannelId))
                .ForMember(dest => dest.SubChannelId, opt => opt.MapFrom(src => src.SubChannelId))
                .ForMember(dest => dest.CodeFormat, opt => opt.MapFrom(src => src.CodeFormat))
                // Explicitly ignore fields that must NEVER be mapped from DTO
                .ForMember(dest => dest.HierarchyPath, opt => opt.Ignore()) // Let DB handle identity
                .ForMember(dest => dest.DesignationId, opt => opt.Ignore()) // Let DB handle identity
                .ForMember(dest => dest.OrgId, opt => opt.Ignore())         // Set by controller via Auth
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())     // Set by controller
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())   // Set by controller
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())    // Set by controller
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())  // Set by controller
                .ForMember(dest => dest.Rowversion, opt => opt.Ignore());    // Managed by DB/EF
        }
    }
    public class DesignationNode
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<DesignationNode> ReportingDesignations { get; set; } = new();
        public string Code { get; set; }
    }
}