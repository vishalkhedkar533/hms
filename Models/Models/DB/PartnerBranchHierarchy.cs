using AutoMapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("partner_branch_heirarchy", Schema = "hms")]
    public class PartnerBranchHierarchy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("partner_branch_heirarchy_id")]
        public int PartnerBranchHierarchyId { get; set; }

        [Column("orgid")]
        public int? OrgId { get; set; }

        [Required]
        [Column("channel_id")]
        public long ChannelId { get; set; }

        [Column("sub_channel_id")]
        public long? SubChannelId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("partner_branch_code")]
        public string PartnerBranchCode { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("partner_branch")]
        public string PartnerBranch { get; set; }

        [Required]
        [MaxLength(1000)]
        [Column("partner_address")]
        public string PartnerAddress { get; set; }

        [MaxLength(50)]
        [EmailAddress]
        [Column("partner_mail")]
        public string PartnerMail { get; set; }

        [MaxLength(50)]
        [Phone]
        [Column("partner_phone")]
        public string PartnerPhone { get; set; }

        /// <summary>
        /// Maps to Postgres 'ltree' type. 
        /// Use dot-separated labels (e.g., "Top.Middle.Leaf")
        /// </summary>
        [Column("hierarchy_path", TypeName = "ltree")]
        public string HierarchyPath { get; set; }

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
        // New column added from SQL update
        [Column("relation_mgr")]
        public int? RelationMgr { get; set; }

        // --- Navigation Properties ---

        [ForeignKey("CreatedBy")]
        public virtual User Creator { get; set; }

        [ForeignKey("ModifiedBy")]
        public virtual User Modifier { get; set; }

        /* [ForeignKey("OrgId")]
        public virtual Organisation Organisation { get; set; }

        [ForeignKey("ChannelId")]
        public virtual ChannelMaster Channel { get; set; }

        [ForeignKey("SubChannelId")]
        public virtual SubChannelMaster SubChannel { get; set; }
        */
    }

    public class PartnerBranchHierarchyDto
    {
        // Null for New, Value for Update
        public int? PartnerBranchHierarchyId { get; set; }

        public int? ParentBranchHierarchyId { get; set; }
        public int? OrgId { get; set; }

        [Required(ErrorMessage = "Channel is required")]
        public long ChannelId { get; set; }

        public long? SubChannelId { get; set; }

        //[Required]
        [StringLength(20)]
        public string PartnerBranchCode { get; set; }

        //[Required]
        [StringLength(100)]
        public string PartnerBranch { get; set; }

        //[Required]
        [StringLength(1000)]
        public string PartnerAddress { get; set; }

        //[EmailAddress]
        [StringLength(50)]
        public string? PartnerMail { get; set; }

        //[Phone]
        [StringLength(50)]
        public string? PartnerPhone { get; set; }

        /// <summary>
        /// Format: Label1.Label2.Label3
        /// </summary>
        public string? HierarchyPath { get; set; }
        public int? RelationMgr { get; set; }
    }
    public class PartnerBranchHierarchyProfile : Profile
    {
        public PartnerBranchHierarchyProfile()
        {
            // 1. Model -> DTO (Read)
            CreateMap<PartnerBranchHierarchy, PartnerBranchHierarchyDto>()
                // Map CreatedBy to UserId for the response
                // ParentBranchHierarchyId doesn't exist in the Model, 
                // so it will remain null/default unless you manually set it in the service
                .ForMember(dest => dest.ParentBranchHierarchyId, opt => opt.Ignore());

            // 2. DTO -> Model (Create/Update)
            CreateMap<PartnerBranchHierarchyDto, PartnerBranchHierarchy>()
                // Ignore the Primary Key so the DB handles auto-increment (serial4)
                .ForMember(dest => dest.PartnerBranchHierarchyId, opt => opt.Ignore())

                // Ignore Audit fields to prevent overwriting with defaults
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())

                // Ignore Navigation Properties
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.Modifier, opt => opt.Ignore())

                // Conditional mapping: only update properties that are not null in the DTO
                // This is vital for PATCH-style updates
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
    public class PartnerBranchNode
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public List<PartnerBranchNode>? ReportingBranches { get; set; } = new();
    }
}
