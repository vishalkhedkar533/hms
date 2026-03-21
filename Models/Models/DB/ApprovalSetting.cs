using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("approval_setting", Schema = "hmsmaster")]
    [Index(nameof(OrgId), nameof(ComponentId), IsUnique = true, Name = "uq_org_comp")]
    public class ApprovalSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int settingId { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("component_id")]
        public int? ComponentId { get; set; }

        [Column("approveroneid")]
        public int? ApproverOneId { get; set; }

        [Column("approvertwoid")]
        public int? ApproverTwoId { get; set; }

        [Column("approverthreeid")]
        public int? ApproverThreeId { get; set; }

        [Column("usedefaultapprover")]
        public bool? UseDefaultApprover { get; set; } = true;

        [Column("is_log")]
        public bool? IsLog { get; set; } = false;

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

        // Navigation Properties (Optional, if using EF Core)
        [ForeignKey(nameof(OrgId))]
        public virtual Organisation Organisation { get; set; }

        [ForeignKey(nameof(ApproverOneId))]
        public virtual Role ApproverOne { get; set; }

        [ForeignKey(nameof(ComponentId))]
        public virtual UiComponent UiComponent { get; set; }
    }
    public class ApprovalSettingDto
    {
        // For updates, the ID is required. 
        // For "Create" operations, you might omit this or leave it nullable.
        public int? settingId { get; set; }
        public int? ComponentId { get; set; }

        public int? ApproverOneId { get; set; }

        public int? ApproverTwoId { get; set; }

        public int? ApproverThreeId { get; set; }

        public bool UseDefaultApprover { get; set; } = true;

        public bool? IsLog { get; set; } = false;
    }
    public class ApprovalSettingProfile : Profile
    {
        public ApprovalSettingProfile()
        {
            CreateMap<ApprovalSetting, ApprovalSettingDto>()
                .ReverseMap()
                // Ignore fields that should not be mapped back from DTO to Entity
                .ForMember(dest => dest.Organisation, opt => opt.Ignore())
                .ForMember(dest => dest.ApproverOne, opt => opt.Ignore())
                .ForMember(dest => dest.UiComponent, opt => opt.Ignore())

                // Ignore audit fields (Set by the service/database)
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())

                // NOTE: Since OrgId is [Required] in your Entity but missing 
                // from your current DTO, we must ignore it or your mapping will fail.
                .ForMember(dest => dest.OrgId, opt => opt.Ignore())
                .ForMember(dest => dest.settingId, opt => opt.Ignore());
        }
    }
}
