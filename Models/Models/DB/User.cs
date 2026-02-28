using AutoMapper;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("user", Schema = "hms")] // Quoted "user" handled via EF Table attribute
    public class User
    {
        [Key]
        [Column("user_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // SERIAL → auto-increment
        public int UserId { get; set; }

        [Column("username")]
        [StringLength(100)]
        [SwaggerSchema("Username is required.")]
        public string Username { get; set; } = null!;

        [Column("email_id")]
        [StringLength(150)]
        [SwaggerSchema("Email ID is required.")]
        public string EmailId { get; set; } = null!;

        [Column("mobile_number")]
        [StringLength(20)]
        public string? MobileNumber { get; set; }

        [Column("password")]
        [StringLength(255)]
        [SwaggerSchema("Password is required.")]
        public string Password { get; set; } = null!;

        [Column("is_active")]
        [Required]
        public bool IsActive { get; set; }

        [Column("is_locked")]
        [Required]
        public bool IsLocked { get; set; }

        [Column("last_login_date")]
        public DateTime? LastLoginDate { get; set; }

        [Column("created_by")]
        [SwaggerSchema("CreatedBy is required.")]
        public int CreatedBy { get; set; }

        [Column("created_date")]
        [Required]
        public DateTime CreatedDate { get; set; }

        [Column("modified_by")]
        [StringLength(100)]
        public int? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }
        [Column("password_changed_date")]
        public DateTime? PasswordChangedDate { get; set; }
        [NotMapped]
        public HMSDashboard? HmsDashboard { get; set; } = new HMSDashboard();
        [Column("failedloginattempts")]
        public int failedloginattempts { get; set; } = 0;
        [Column("lockoutendtime")]
        public DateTime? lockoutendtime { get; set; } = null;
        [Column("orgid")]
        public int? OrgId { get; set; } = null;
        [Column("reporting_mgr")]
        public int? ReportingMgr { get; set; }
        [NotMapped]
        public string? OrgName { get; set; } = null;
        [NotMapped]
        public int? SubscriberId { get; set; } = null;
        [NotMapped]
        public string? SubscriberName { get; set; } = null;
        [ForeignKey("ReportingMgr")]
        public virtual User? Manager { get; set; }
    }
    public class UserCreateDto
    {
        public string Username { get; set; } = null!;
        public string EmailId { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? MobileNumber { get; set; }
        public int? ReportingMgr { get; set; }
    }

    public class UserOtherDetails
    {
        public string? Username { get; set; } = null!;
        public string? EmailId { get; set; } = null!;
        public string? MobileNumber { get; set; }
        public int? ReportingMgr { get; set; }
    }
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // 1. Mapping for User creation
            CreateMap<UserCreateDto, User>()
                // Default values
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.failedloginattempts, opt => opt.MapFrom(src => 0))

                // Explicitly ignore ALL unmapped destination members to stop the errors
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore()) // Set in service/controller
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordChangedDate, opt => opt.Ignore())
                .ForMember(dest => dest.lockoutendtime, opt => opt.Ignore())
                .ForMember(dest => dest.HmsDashboard, opt => opt.Ignore())
                .ForMember(dest => dest.OrgId, opt => opt.Ignore()) // Set in service/controller
                .ForMember(dest => dest.OrgName, opt => opt.Ignore())
                .ForMember(dest => dest.SubscriberId, opt => opt.Ignore())
                .ForMember(dest => dest.SubscriberName, opt => opt.Ignore())
                .ForMember(dest => dest.Manager, opt => opt.Ignore());

            // 2. Mapping for UserOtherDetails (Updates)
            CreateMap<UserOtherDetails, User>()
                // Ignore everything not in the DTO
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.IsLocked, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordChangedDate, opt => opt.Ignore())
                .ForMember(dest => dest.HmsDashboard, opt => opt.Ignore())
                .ForMember(dest => dest.failedloginattempts, opt => opt.Ignore())
                .ForMember(dest => dest.lockoutendtime, opt => opt.Ignore())
                .ForMember(dest => dest.OrgId, opt => opt.Ignore())
                .ForMember(dest => dest.OrgName, opt => opt.Ignore())
                .ForMember(dest => dest.SubscriberId, opt => opt.Ignore())
                .ForMember(dest => dest.SubscriberName, opt => opt.Ignore())
                .ForMember(dest => dest.Manager, opt => opt.Ignore())
                // Apply condition to only map non-null values
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}