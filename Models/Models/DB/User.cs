using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        [StringLength(100)]
        public string Username { get; set; } = null!;

        [StringLength(150)]
        public string EmailId { get; set; } = null!;

        [StringLength(20)]
        public string? MobileNumber { get; set; }

        public bool IsActive { get; set; }

        public bool IsLocked { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? RowVersion { get; set; }

        public DateTime? PasswordChangedDate { get; set; }

        public HMSDashboard? HmsDashboard { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTime? LockoutEndTime { get; set; }

        public int? OrgId { get; set; }

        public int? ReportingMgr { get; set; }

        public string? OrgName { get; set; }

        public int? SubscriberId { get; set; }

        public string? SubscriberName { get; set; }

        // Navigation property
        public UserOtherDetails? Manager { get; set; }
    }
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // 1. Mapping for User creation
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.EmailId, opt => opt.MapFrom(src => src.EmailId))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.MobileNumber, opt => opt.MapFrom(src => src.MobileNumber))
                .ForMember(dest => dest.ReportingMgr, opt => opt.MapFrom(src => src.ReportingMgr))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.failedloginattempts, opt => opt.MapFrom(src => 0))
                .ForAllMembers(opt => opt.Ignore());

            // 2. Mapping for UserOtherDetails (Updates)
            CreateMap<UserOtherDetails, User>()                
                .ForMember(dest => dest.Username, opt => {
                    opt.MapFrom(src => src.Username);
                    opt.Condition(src => src.Username != null);
                })
                .ForMember(dest => dest.EmailId, opt => {
                    opt.MapFrom(src => src.EmailId);
                    opt.Condition(src => src.EmailId != null);
                })
                .ForMember(dest => dest.MobileNumber, opt => {
                    opt.MapFrom(src => src.MobileNumber);
                    opt.Condition(src => src.MobileNumber != null);
                })
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.IsLocked))
                .ForMember(dest => dest.ReportingMgr, opt => opt.MapFrom(src => src.ReportingMgr))
                .ForMember(dest => dest.OrgId, opt => opt.MapFrom(src => src.OrgId))
                .ForAllMembers(opt => opt.Ignore());
            // 3. Mapping for reading User (Entity -> DTO)
            // No .Ignore() here! We want the properties to map automatically.
            CreateMap<User, UserOtherDetails>();
        }
    }
    public class SearchUser 
    {
        public string? Username { get; set; } = null!;
        public string? EmailId { get; set; } = null!;
        public string? MobileNumber { get; set; }
    }

}