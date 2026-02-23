using AutoMapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("inbox", Schema = "hms")]
    public class Inbox
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("srno")]
        public int SrNo { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Required]
        [StringLength(1000)]
        [Column("requestdets")]
        public string RequestDets { get; set; }

        [Required]
        [StringLength(3000)]
        [Column("requestornote")]
        public string RequestorNote { get; set; }

        [Required]
        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Required]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Required]
        [Column("srstatus")]
        public int SrStatus { get; set; }

        [Column("statusupdated_by")]
        public int? StatusUpdatedBy { get; set; }

        [Column("statusmodified_on")]
        public DateTime? StatusModifiedOn { get; set; }
    }
    [Table("sr_approver", Schema = "hms")]
    public class SrApprover
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("sr_approverid")]
        public int SrApproverId { get; set; }
        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }
        [Required]
        [Column("approverlevel")]
        public int ApproverLevel { get; set; }

        [Required]
        [Column("allocatedroleid")]
        public int AllocatedRoleId { get; set; }

        [Column("decisionby")]
        public int? DecisionBy { get; set; }

        [Column("decisionon")]
        public DateTime? DecisionOn { get; set; }

        [Required]
        [StringLength(2000)]
        [Column("approvalendpoint")]
        public string ApprovalEndpoint { get; set; }

        [Required]
        [Column("approvalpayload")]
        public string ApprovalPayload { get; set; }

        [Required]
        [Column("approvalapiresponse")]
        public string ApprovalApiResponse { get; set; }
    }
    public class InboxDto
    {
        public int SrNo { get; set; }

        [Required]
        [StringLength(1000)]
        public string RequestDets { get; set; }

        [Required]
        [StringLength(3000)]
        public string RequestorNote { get; set; }

        [Required]
        public int SrStatus { get; set; }
    }
    public class SrApproverDto
    {
        public int SrApproverId { get; set; }

        [Required]
        public int ApproverLevel { get; set; }

        [Required]
        public int AllocatedRoleId { get; set; }

        public DateTime? DecisionOn { get; set; }

        [Required]
        [StringLength(2000)]
        public string ApprovalEndpoint { get; set; }

        [Required]
        public string ApprovalPayload { get; set; }

        [Required]
        public string ApprovalApiResponse { get; set; }
    }

    public class InboxProfile : Profile
    {
        public InboxProfile()
        {
            // 1. Entity to DTO (For Data Retrieval/GET)
            // Fields missing in DTO are ignored by default as they have no destination
            CreateMap<Inbox, InboxDto>();

            // 2. DTO to Entity (For Create/Update/POST/PUT)
            // We must explicitly ignore fields that exist in 'Inbox' but not in 'InboxDto'
            CreateMap<InboxDto, Inbox>()
                .ForMember(dest => dest.OrgId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.StatusUpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.StatusModifiedOn, opt => opt.Ignore());
        }
    }
    public class SrApproverProfile : Profile
    {
        public SrApproverProfile()
        {
            // 1. Entity to DTO (For Data Retrieval/GET)
            CreateMap<SrApprover, SrApproverDto>();

            // 2. DTO to Entity (For Create/Update/POST/PUT)
            // We must explicitly ignore fields that exist in 'SrApprover' but not in 'SrApproverDto'
            CreateMap<SrApproverDto, SrApprover>()
                .ForMember(dest => dest.OrgId, opt => opt.Ignore())
                .ForMember(dest => dest.DecisionBy, opt => opt.Ignore());
        }
    }
}