using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("ui_fields", Schema = "hmsmaster")]
    public class UiField
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Since it's not a serial type in your SQL
        [Column("cntrl_id")]
        public int CntrlId { get; set; }

        [Column("component_id")]
        public int? ComponentId { get; set; }

        [Required]
        [Column("cntrl_name")]
        public string CntrlName { get; set; } = string.Empty;

        // Navigation Property for the foreign key
        [ForeignKey("ComponentId")]
        public virtual UiComponent? Component { get; set; }

        // Navigation Property for the related settings (one-to-many)
        public virtual ICollection<UiFieldsSetting> Settings { get; set; } = new List<UiFieldsSetting>();
    }
}
