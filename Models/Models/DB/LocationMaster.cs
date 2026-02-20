using AutoMapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("location_master", Schema = "hmsmaster")]
    //also called officetype 
    public class LocationMaster
    {
        [Key]
        [Column("location_master_id")]
        public long LocationMasterId { get; set; }

        [Column("channel_id")]
        public long ChannelId { get; set; }

        [Column("sub_channel_id")]
        public long SubChannelId { get; set; }

        [Column("orgid")]
        public int? OrgId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("location_code")]
        public string LocationCode { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("location_desc")]
        public string LocationDesc { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("created_by")]
        public string CreatedBy { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [MaxLength(100)]
        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        public int? RowVersion { get; set; }
    }
    public class LocationMasterDto
    {
        public long LocationMasterId { get; set; }
        [Required]
        public long ChannelId { get; set; }
        [Required]
        public long SubChannelId { get; set; }
        [Required]
        [StringLength(20)]
        public string LocationCode { get; set; }
        [Required]
        [StringLength(20)]
        public string LocationDesc { get; set; }
        public bool IsActive { get; set; }
    }
    public class LocationMappingProfile : Profile
    {
        public LocationMappingProfile()
        {
            // DTO -> Entity
            CreateMap<LocationMasterDto, LocationMaster>()
                // 1. Ignore the Primary Key so AutoMapper doesn't try to change it
                .ForMember(dest => dest.LocationMasterId, opt => opt.Ignore())

                // 3. Protect Audit fields from being overwritten with null/defaults
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore()) // Handled manually or via Base Class
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
                .ForMember(dest => dest.OrgId, opt => opt.Ignore())
                .ForMember(dest => dest.OrgId, opt => opt.Ignore());

            // Entity -> DTO
            CreateMap<LocationMaster, LocationMasterDto>();
        }
    }
}