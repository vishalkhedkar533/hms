using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.BackEndCalculation
{
    [Table("organisation", Schema = "app_subscription")]
    public class Organisation
    {
        // Corresponds to 'orgid serial4 NOT NULL'
        // [Key] defines the primary key.
        // [DatabaseGenerated(DatabaseGeneratedOption.Identity)] is implicit for serial/int primary keys
        // but is explicitly added here for clarity, mapping to serial4.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("orgid")]
        public int OrgId { get; set; }

        // Corresponds to 'subscriberid int4 NOT NULL'
        // [Required] ensures NOT NULL constraint.
        // [Column("subscriberid")] maps the C# property name to the database column name.
        [Required]
        [Column("subscriberid")]
        public int SubscriberId { get; set; }

        // Corresponds to 'orgname varchar(500) NOT NULL'
        // [Required] ensures NOT NULL.
        // [StringLength(500)] limits the maximum length of the string.
        [Required]
        [StringLength(500)]
        [Column("orgname")]
        public string OrgName { get; set; }

        // --- Navigation Property (Foreign Key) ---

        // This property represents the one-to-many relationship defined by the foreign key:
        // ALTER TABLE hms.organisation ADD CONSTRAINT fk_subscriberid FOREIGN KEY (subscriberid) REFERENCES hms.subscriber(subscriberid);

        // The [ForeignKey("SubscriberId")] annotation is optional here as EF Core's conventions 
        // usually figure this out based on the property name (SubscriberId).
        [ForeignKey("SubscriberId")]
        public virtual Subscriber Subscriber { get; set; }

        [Required]
        [Column("state")]
        public int State { get; set; }
    }

}