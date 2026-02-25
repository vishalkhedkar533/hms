using AutoMapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    //also called officetype 
    [Table("location_master", Schema = "hmsmaster")]
    public class LocationMaster
    {
        [Key]
        [Column("location_master_id")]
        public long LocationMasterId { get; set; } // int8 -> long

        [Required]
        [Column("channel_id")]
        public long ChannelId { get; set; }

        [Required]
        [Column("sub_channel_id")]
        public long SubChannelId { get; set; }

        [Column("orgid")]
        public int OrgId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("location_code")]
        public string LocationCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("location_desc")]
        public string LocationDesc { get; set; } = string.Empty;

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; }

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
    }

    public class LocationMasterDto
    {
        public long? LocationMasterId { get; set; }
        public long? ChannelId { get; set; }
        public long? SubChannelId { get; set; }
        [StringLength(20)]
        public string? LocationCode { get; set; }
        [StringLength(20)]
        public string? LocationDesc { get; set; }
        public bool IsActive { get; set; }
    }
    public class LocationMappingProfile : Profile
    {
        public LocationMappingProfile()
        {
            // DTO -> Entity
            CreateMap<LocationMasterDto, LocationMaster>()
                // Ignore the Primary Key so AutoMapper doesn't overwrite it during Updates
                .ForMember(dest => dest.LocationMasterId, opt => opt.Ignore())
                .ForMember(dest => dest.OrgId, opt=> opt.Ignore())
                // Ignore Audit fields (These should be set in the Controller/Service)
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore());

            // Entity -> DTO
            CreateMap<LocationMaster, LocationMasterDto>();
        }
    }
}