using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
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
        [StringLength(100)]
        [SwaggerSchema("CreatedBy is required.")]
        public string CreatedBy { get; set; } = null!;

        [Column("created_date")]
        [Required]
        public DateTime CreatedDate { get; set; }

        [Column("modified_by")]
        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }
    }
}