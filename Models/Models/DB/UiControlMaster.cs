using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    public class UiControlMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("uicontrolmenu_id")]
        public long UiControlMenuId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("ui_object_name")]
        public string UiObjectName { get; set; } = string.Empty;
    }
}
