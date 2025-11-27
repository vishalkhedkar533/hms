using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("Organisation", Schema = "hms")]
    public class Organisation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrgId { get; set; }

        [Required]
        [MaxLength(500)]
        public string OrgName { get; set; } = string.Empty;

        [Required]
        public int SubscriberId { get; set; }

        [ForeignKey(nameof(SubscriberId))]
        public Subscriber Subscriber { get; set; } = null!;
    }
}
