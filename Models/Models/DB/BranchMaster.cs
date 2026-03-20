using AutoMapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("branch_master", Schema = "hmsmaster")]
    public class BranchMaster
    {
        [Key]
        [Column("branch_id")]
        public long BranchId { get; set; } // int8 -> long

        [Column("orgid")]
        public int? OrgId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("branch_code")]
        public string BranchCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("branch_name")]
        public string BranchName { get; set; } = string.Empty;

        [Column("address")]
        public string? Address { get; set; }

        [Column("state")]
        public int? State { get; set; }

        [MaxLength(20)]
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        [Column("email_id")]
        public string? EmailId { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("is_reported_to_regulator")]
        public bool IsReportedToRegulator { get; set; }

        [MaxLength(50)]
        [Column("regulator_code")]
        public string? RegulatorCode { get; set; }

        // FOREIGN KEY PROPERTY
        [Column("location_master_id")]
        public long? LocationMasterId { get; set; } // int8 -> long?

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

        [Column("rowversion")]
        public int? RowVersion { get; set; }

        // NAVIGATION PROPERTY
        [ForeignKey("LocationMasterId")]
        public virtual LocationMaster? Location { get; set; }
    }
    public class BranchMasterDto
    {
        public long? BranchId { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? Address { get; set; }
        public int? State { get; set; }
        public string? PhoneNumber { get; set; }
        [EmailAddress]
        public string? EmailId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsReportedToRegulator { get; set; }
        public string? RegulatorCode { get; set; }
        public long? LocationMasterId { get; set; }
        public long? ParentBranchId { get; set; } 
    }

    public class BranchNode
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public bool IsReportedToRegulator { get; set; }
        public string? RegulatorCode { get; set; }
        public List<BranchNode> ReportingBranches { get; set; } = new();
    }

    public class BranchListDto
    {
        public long BranchId { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public bool IsActive { get; set; }
        public bool IsReportedToRegulator { get; set; }
        public string? RegulatorCode { get; set; }
    }

    public class BranchSearchDto
    {
        public bool? IsActive { get; set; }
    }
    public class BranchMasterProfile : Profile
    {
        public BranchMasterProfile()
        {
            // 1. Request DTO -> Database Entity
            CreateMap<BranchMasterDto, BranchMaster>()
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.OrgId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Location, opt => opt.Ignore())
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore());

            // 2. Database Entity -> Response DTO
            CreateMap<BranchMaster, BranchMasterDto>()
                // FIX: Ignore ParentBranchId because it doesn't exist in the BranchMaster table
                .ForMember(dest => dest.ParentBranchId, opt => opt.Ignore())

                // Keep your existing location fallback logic
                .ForMember(dest => dest.LocationMasterId,
                           opt => opt.MapFrom(src => src.LocationMasterId ?? -1000));
        }
    }
}