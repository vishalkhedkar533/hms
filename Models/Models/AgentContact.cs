using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMS.Models
{
    /*
     * modelBuilder.Entity<AgentContact>(entity =>
{
    entity.ToTable("AGENT_CONTACT", "hms");

    entity.HasOne(e => e.Agent)
          .WithMany()
          .HasForeignKey(e => e.AgentId)
          .HasConstraintName("fk_agent")
          .OnDelete(DeleteBehavior.Cascade);

    // Add foreign key configs for *_CODE columns if needed
});

     */
    [Table("AGENT_CONTACT", Schema = "hms")]
    public class AgentContact
    {
        [Key]
        [Column("AGENT_CONTACT_ID")]
        [SwaggerSchema("Primary key for agent contact.")]
        public int AgentContactId { get; set; }

        [Required]
        [Column("AGENT_ID")]
        [SwaggerSchema("Reference to the agent.")]
        public int AgentId { get; set; }

        [StringLength(20)]
        [Column("ADDRESS_TYPE_CODE")]
        [SwaggerSchema("Type code for address.")]
        public string? AddressTypeCode { get; set; }

        [Required]
        [Column("IS_PRIMARY")]
        [SwaggerSchema("Indicates if this is the primary contact address.")]
        public bool IsPrimary { get; set; }

        [StringLength(150)]
        [Column("ADDRESS1")]
        [SwaggerSchema("Address line 1.")]
        public string? Address1 { get; set; }

        [StringLength(150)]
        [Column("ADDRESS2")]
        [SwaggerSchema("Address line 2.")]
        public string? Address2 { get; set; }

        [StringLength(150)]
        [Column("ADDRESS3")]
        [SwaggerSchema("Address line 3.")]
        public string? Address3 { get; set; }

        [StringLength(100)]
        [Column("CITY")]
        [SwaggerSchema("City of the address.")]
        public string? City { get; set; }

        [StringLength(20)]
        [Column("STATE_CODE")]
        [SwaggerSchema("State code.")]
        public string? StateCode { get; set; }

        [StringLength(30)]
        [Column("PINCODE")]
        [SwaggerSchema("Postal code.")]
        public string? Pincode { get; set; }

        [StringLength(150)]
        [Column("WEBSITE")]
        [SwaggerSchema("Website URL.")]
        public string? Website { get; set; }

        [StringLength(20)]
        [Column("EMAIL_TYPE_CODE")]
        [SwaggerSchema("Type code for email.")]
        public string? EmailTypeCode { get; set; }

        [StringLength(100)]
        [Column("EMAIL_ID")]
        [SwaggerSchema("Email address.")]
        public string? EmailId { get; set; }

        [StringLength(20)]
        [Column("PHONE_TYPE_CODE")]
        [SwaggerSchema("Type code for phone.")]
        public string? PhoneTypeCode { get; set; }

        [StringLength(30)]
        [Column("PHONE_NO")]
        [SwaggerSchema("Phone number.")]
        public string? PhoneNo { get; set; }

        [StringLength(10)]
        [Column("EXTENSION")]
        [SwaggerSchema("Phone extension number.")]
        public string? Extension { get; set; }

        [Required]
        [Column("EFFECTIVE_FROM_DATE", TypeName = "date")]
        [SwaggerSchema("Start date of the contact's validity.")]
        public DateTime EffectiveFromDate { get; set; }

        [Column("EFFECTIVE_TO_DATE", TypeName = "date")]
        [SwaggerSchema("End date of the contact's validity.")]
        public DateTime? EffectiveToDate { get; set; }

        [Required]
        [StringLength(100)]
        [Column("CREATED_BY")]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Timestamp when the record was created.")]
        public DateTime CreatedDate { get; set; }

        [StringLength(100)]
        [Column("MODIFIED_BY")]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Timestamp of the last modification.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [ConcurrencyCheck]
        [SwaggerSchema("Used for optimistic concurrency control.")]
        public int? RowVersion { get; set; }

        // Navigation property for Agent
        [SwaggerSchema("Reference to the agent.")]
        public Agent? Agent { get; set; }

        // TODO: Define navigation properties for *_CODE foreign keys if master tables exist, e.g.
        // public AddressTypeMaster? AddressType { get; set; }
        // public StateMaster? State { get; set; }
        // public EmailTypeMaster? EmailType { get; set; }
        // public PhoneTypeMaster? PhoneType { get; set; }
    }
}
