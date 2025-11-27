using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("Subscriber", Schema = "hms")]
    public class Subscriber
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriberId { get; set; }

        [Required]
        [MaxLength(500)]
        public string SubscriberName { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Organisation> Organisations { get; set; } = new List<Organisation>();
    }
}
