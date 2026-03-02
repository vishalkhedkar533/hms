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
        public string? CodeFormat { get; set; }
        public long? SubChannelId { get; set; }
    }
    public class DesignationMasterProfile : Profile
    {
        public DesignationMasterProfile()
        {
            CreateMap<DesignationMaster, DesignationMasterDto>()
                // Map Entity -> DTO
                .ForMember(dest => dest.ParentDesignationId, opt => opt.Ignore()) // Requires custom logic
                .ReverseMap()
                // Map DTO -> Entity (Ignore System/Audit fields)
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Rowversion, opt => opt.Ignore())
                .ForMember(dest => dest.OrgId, opt => opt.Ignore())
                .ForMember(dest => dest.HierarchyPath, opt => opt.Ignore())
                // Ignore the property that doesn't exist on the entity
                .ForMember(dest => dest.GetType().GetProperty("ParentDesignationId") == null ? null : (object)null, opt => opt.Ignore());
            // Note: Use .ForPath if needed, or simply let it ignore if it's not on the entity
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