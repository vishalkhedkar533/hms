using Models;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<UserAccessLog>(entity =>
{
    entity.ToTable("USER_ACCESS_LOG", "hms");

    entity.HasOne(e => e.User)
          .WithMany()
          .HasForeignKey(e => e.UserId)
          .HasConstraintName("fk_user")
          .OnDelete(DeleteBehavior.Restrict);
});

     */
    [Table("USER_ACCESS_LOG", Schema = "hms")]
    public class UserAccessLog
    {
        [Key]
        [Column("LOG_ID")]
        [SwaggerSchema("Primary key for the user access log.")]
        public long LogId { get; set; }

        [Required]
        [Column("USER_ID")]
        [SwaggerSchema("Reference to the user.")]
        public int UserId { get; set; }

        [Required]
        [Column("LOGIN_TIMESTAMP")]
        [SwaggerSchema("Timestamp when user logged in.")]
        public DateTime LoginTimestamp { get; set; }

        [Column("LOGOUT_TIMESTAMP")]
        [SwaggerSchema("Timestamp when user logged out.")]
        public DateTime? LogoutTimestamp { get; set; }

        [StringLength(50)]
        [Column("IP_ADDRESS")]
        [SwaggerSchema("IP address from which the user accessed.")]
        public string? IpAddress { get; set; }

        [StringLength(255)]
        [Column("DEVICE_INFO")]
        [SwaggerSchema("Information about the device used.")]
        public string? DeviceInfo { get; set; }

        [StringLength(100)]
        [Column("SESSION_ID")]
        [SwaggerSchema("Session identifier.")]
        public string? SessionId { get; set; }

        [Required]
        [StringLength(20)]
        [Column("STATUS")]
        [SwaggerSchema("Login status, e.g. 'SUCCESS' or 'FAILURE'.")]
        public string Status { get; set; } = null!;

        [StringLength(255)]
        [Column("FAILURE_REASON")]
        [SwaggerSchema("Reason for login failure, if any.")]
        public string? FailureReason { get; set; }

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Timestamp when the log record was created.")]
        public DateTime CreatedDate { get; set; }

        // Navigation property
        [SwaggerSchema("Reference to the user.")]
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}
