using AutoMapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("partner_branch_heirarchy", Schema = "hms")]
    public class PartnerBranchHeirarchy
    {
        [Key]
        [Column("partner_branch_heirarchy_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PartnerBranchHeirarchyId { get; set; }

        [Column("orgid")]
        public int? OrgId { get; set; }

        [Column("channel_id")]
        [Required]
        public long ChannelId { get; set; } // int8 -> long

        [Column("sub_channel_id")]
        public long? SubChannelId { get; set; } // int8 -> long

        [Column("partner_branch_code")]
        [Required]
        [StringLength(20)]
        public string PartnerBranchCode { get; set; } = null!;

        [Column("partner_branch")]
        [Required]
        [StringLength(100)]
        public string PartnerBranch { get; set; } = null!;

        [Column("partner_address")]
        [Required]
        [StringLength(1000)]
        public string PartnerAddress { get; set; } = null!;

        [Column("partner_mail")]
        [StringLength(50)]
        public string? PartnerMail { get; set; }

        [Column("partner_phone")]
        [StringLength(50)]
        public string? PartnerPhone { get; set; }

        // ltree is a custom PG type. Treat as string in C#.
        [Column("hierarchy_path", TypeName = "ltree")]
        public string? HierarchyPath { get; set; }

        [Column("created_by")]
        [Required]
        public int CreatedBy { get; set; }

        [Column("created_date")]
        [Required]
        public DateTime CreatedDate { get; set; }

        [Column("modified_by")]
        public int? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Column("relation_mgr")]
        public int? RelationMgr { get; set; }

        // --- Navigation Properties ---

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }

        [ForeignKey("ModifiedBy")]
        public virtual User? ModifiedByUser { get; set; }

        // Assuming you have an Agent model
        [ForeignKey("RelationMgr")]
        public virtual Agent? RelationshipManager { get; set; }
    }
    public class PartnerBranchHierarchyDto
    {
        // Null for New, Value for Update
        public int? PartnerBranchHierarchyId { get; set; }

        // This likely relates to the ltree 'hierarchy_path' logic
        public int? ParentBranchHierarchyId { get; set; }

        [Required(ErrorMessage = "Channel is required")]
        public long? ChannelId { get; set; } // Changed to long? for consistency with nullable DTO patterns

        [Required(ErrorMessage = "SubChannel is required")]
        public long? SubChannelId { get; set; } // Changed to long?

        [Required(ErrorMessage = "PartnerBranchCode is required")]
        public string? PartnerBranchCode { get; set; }

        [Required(ErrorMessage = "PartnerBranch is required")]
        public string? PartnerBranch { get; set; }

        [Required(ErrorMessage = "PartnerAddress is required")]
        public string? PartnerAddress { get; set; }
        public string? PartnerMail { get; set; }
        public string? PartnerPhone { get; set; }
        public string? HierarchyPath { get; set; }
        public int? RelationMgr { get; set; }
    }
    public class PartnerBranchHierarchySearchDto
    {
        // These are nullable (or optional) so validation doesn't trigger
        public long? ChannelId { get; set; }
        public long? SubChannelId { get; set; }
        public string? PartnerBranchCode { get; set; }
    }
    public class PartnerBranchHierarchyProfile : Profile
    {
        public PartnerBranchHierarchyProfile()
        {
            // 1. Model -> DTO (Read)
            CreateMap<PartnerBranchHeirarchy, PartnerBranchHierarchyDto>()
                // Map the entity property to the DTO property
                .ForMember(dest => dest.PartnerBranchHierarchyId, opt => opt.MapFrom(src => src.PartnerBranchHeirarchyId))
                // Explicitly ignore properties that don't exist in the Model
                .ForMember(dest => dest.ParentBranchHierarchyId, opt => opt.Ignore());

            // 2. DTO -> Model (Create/Update)
            CreateMap<PartnerBranchHierarchyDto, PartnerBranchHeirarchy>()
                // Ignore Primary Key and Audit/System fields
                .ForMember(dest => dest.PartnerBranchHeirarchyId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                // Ignore Navigation Properties
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.RelationshipManager, opt => opt.Ignore())
                .ForMember(dest => dest.OrgId, opt => opt.Ignore())

                // Apply the 'Partial Update' logic: 
                // Only map properties that are provided (not null) in the DTO
                .ForAllMembers(opts => {
                    opts.Condition((src, dest, srcMember) => srcMember != null);
                });
        }
    }

    public class PartnerBranchNode
    {
        public int? PartnerBranchHeirarchyId { get; set; }
        public string? Name { get; set; }
        public string? PartnerBranchCode { get; set; } = null!;
        public long ChannelId { get; set; } // int8 -> long
        public long? SubChannelId { get; set; } // int8 -> long
        public string? PartnerBranch { get; set; }
        public string? PartnerAddress { get; set; }
        public string? PartnerMail { get; set; }
        public string? PartnerPhone { get; set; }
        public int? RelationMgr { get; set; }
        public List<PartnerBranchNode>? ReportingBranches { get; set; } = new();
    }
}
