using AutoMapper;
using Microsoft.AspNetCore.Routing.Constraints;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("ui_fields_setting", Schema = "hmsmaster")]
    public class UiFieldsSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("cntrl_id")]
        public int? CntrlId { get; set; }

        [Column("render")]
        public bool? Render { get; set; } = true;

        [Column("allow_edit")]
        public bool? AllowEdit { get; set; } = false;

        [Column("sort_order")]
        public int? SortOrder { get; set; } = 0;

        [Required]
        [Column("access_granted_on")]
        public DateTime AccessGrantedOn { get; set; }

        [Required]
        [Column("access_granted_by")]
        public int AccessGrantedBy { get; set; }

        [Column("role_id")]
        public int? RoleId { get; set; }

        // --- Navigation Properties ---

        [ForeignKey("CntrlId")]
        public virtual UiField? UiField { get; set; }

        [ForeignKey("AccessGrantedBy")]
        public virtual User? GrantedByUser { get; set; }

        [ForeignKey("ApproverOneId")]
        public virtual User? ApproverOne { get; set; }

        [ForeignKey("ApproverTwoId")]
        public virtual User? ApproverTwo { get; set; }

        [ForeignKey("ApproverThreeId")]
        public virtual User? ApproverThree { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }
    }
    public class UiFieldsSettingDto
    {
        public int? Id { get; set; }
        public int? CntrlId { get; set; }
        // Settings with default values matching your DB logic
        public bool Render { get; set; } = true;
        public bool AllowEdit { get; set; } = false;
        //public int SortOrder { get; set; } = 0;
        // Permissions and Approvers
        public int? RoleId { get; set; }
    }
    public class UiFieldsMappingProfile : Profile
    {
        public UiFieldsMappingProfile()
        {
            // --- Entity to DTO ---
            CreateMap<UiFieldsSetting, UiFieldsSettingDto>()
                .ForMember(dest => dest.Render, opt => opt.MapFrom(src => src.Render ?? true))
                .ForMember(dest => dest.AllowEdit, opt => opt.MapFrom(src => src.AllowEdit ?? false));
            // Note: Navigation Properties are automatically ignored because 
            // the DTO has no matching "UiField" or "GrantedByUser" object properties.

            // --- DTO to Entity ---
            CreateMap<UiFieldsSettingDto, UiFieldsSetting>()
                // 1. Ignore Audit/Internal fields
                .ForMember(dest => dest.AccessGrantedOn, opt => opt.Ignore())
                .ForMember(dest => dest.AccessGrantedBy, opt => opt.Ignore())
                .ForMember(dest => dest.OrgId, opt => opt.Ignore())
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                // 2. Explicitly Ignore Navigation Objects
                // We do this to prevent AutoMapper from trying to create new empty objects 
                // or overwriting existing EF proxies with null.
                .ForMember(dest => dest.UiField, opt => opt.Ignore())
                .ForMember(dest => dest.GrantedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.ApproverOne, opt => opt.Ignore())
                .ForMember(dest => dest.ApproverTwo, opt => opt.Ignore())
                .ForMember(dest => dest.ApproverThree, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())

                // 3. Map the Foreign Key IDs (the actual database columns)
                .ForMember(dest => dest.CntrlId, opt => opt.MapFrom(src => src.CntrlId))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId));
        }
    }
}