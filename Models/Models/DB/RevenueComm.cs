using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("revenue_comm", Schema = "hms")]
    [Index(nameof(OrgId), nameof(StartDate), nameof(EndDate), IsUnique = true, Name = "idx_unique_revcomm_period")]
    public class RevenueComm
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Required]
        [Column("start_date", TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date", TypeName = "date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("revenue")]
        public long Revenue { get; set; } = 0;

        [Required]
        [Column("commission")]
        public long Commission { get; set; } = 0;

        //Navigation Property(Optional)
        [ForeignKey("OrgId")]
        public virtual Organisation Organisation { get; set; }
    }

    public record RevenueCommResponseDto
    {
        public int Id { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public long Revenue { get; init; }
        public long Commission { get; init; }
    }

    public record RevenueCommCreateDto
    {
        [Required]
        public DateTime StartDate { get; init; }

        [Required]
        public DateTime EndDate { get; init; }

        [Range(0, long.MaxValue)]
        public long Revenue { get; init; } = 0;

        [Range(0, long.MaxValue)]
        public long Commission { get; init; } = 0;
    }
    public class RevenueCommProfile : Profile
    {
        public RevenueCommProfile()
        {
            // 1. Model -> Response DTO
            // Maps automatically because property names match exactly.
            CreateMap<RevenueComm, RevenueCommResponseDto>();

            // 2. Create DTO -> Model
            // We tell AutoMapper to ignore 'Id' (DB generated) 
            // and 'OrgId' (set manually from JWT in the service/controller).
            CreateMap<RevenueCommCreateDto, RevenueComm>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrgId, opt => opt.Ignore())
                .ForMember(dest => dest.Organisation, opt => opt.Ignore());
        }
    }
    public class SearchRevenueCommDto
    {
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
    }
}