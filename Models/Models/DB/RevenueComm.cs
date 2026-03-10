using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("revenue_comm", Schema = "hms")]
    [Index(nameof(OrgId), nameof(PeriodId), IsUnique = true, Name = "idx_unique_revcomm_period")]
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
        [Column("periodid")] // Maps to int8
        public long PeriodId { get; set; }

        [Required]
        [Column("revenue")]
        public long Revenue { get; set; } = 0;

        [Required]
        [Column("commission")]
        public long Commission { get; set; } = 0;

        // Navigation Properties
        [ForeignKey("OrgId")]
        public virtual Organisation Organisation { get; set; }

        [ForeignKey("PeriodId")]
        public virtual OrganizationPeriod Period { get; set; }
    }
    public record RevenueCommResponseDto
    {
        public int Id { get; init; }
        public long PeriodId { get; init; }
        public long Revenue { get; init; }
        public long Commission { get; init; }
        public DateTime PeriodStartDate { get; init; }
    }

    //public record RevenueCommCreateDto
    //{
    //    [Required]
    //    public long PeriodId { get; init; }
    //    [Range(0, long.MaxValue)]
    //    public long Revenue { get; init; } = 0;
    //    [Range(0, long.MaxValue)]
    //    public long Commission { get; init; } = 0;
    //}
    public class RevenueCommProfile : Profile
    {
        public RevenueCommProfile()
        {
            // Model -> Response
            // If you want to include dates from the Period table, 
            // you would add .ForMember(d => d.StartDate, o => o.MapFrom(s => s.Period.StartDate))
            CreateMap<RevenueComm, RevenueCommResponseDto>();

            // Create DTO -> Model
            //CreateMap<RevenueCommCreateDto, RevenueComm>()
            //    .ForMember(dest => dest.Id, opt => opt.Ignore())
            //    .ForMember(dest => dest.OrgId, opt => opt.Ignore())
            //    .ForMember(dest => dest.Organisation, opt => opt.Ignore())
            //    .ForMember(dest => dest.Period, opt => opt.Ignore());
        }
    }
    public class SearchRevenueCommDto
    {
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public GroupDataByPeriod GroupBy { get; init; }
    }

    public class GraphRevenueCommDto
    {
        public string filtertype { get; set; } = "MonthlyPeriod";
        public List<GraphDataByPeriod> Data { get; set; }
    }

    public class GraphDataByPeriod
    {
        public DateTime date { get; set; }
        public long revenue { get; set; }
        public long commission { get; set; }
    }
}