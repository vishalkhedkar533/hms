using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("uicontrol_master", Schema = "hmsmaster")]
    public class UiControlMaster
    {
        [Key]
        [Column("uicontrolmenu_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Typically None if you provide the ID, or Identity if managed by a sequence
        public long UiControlMenuId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("ui_object_name")]
        public string UiObjectName { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("ui_object_type")]
        public string UiObjectType { get; set; } = "not provided";

        // Navigation Properties

        /// <summary>
        /// Relationship to the hierarchy table
        /// </summary>
        public virtual ICollection<UiControlHierarchy> UiControlHierarchies { get; set; } = new List<UiControlHierarchy>();
    }
}
