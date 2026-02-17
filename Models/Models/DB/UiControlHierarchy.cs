using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("uicontrol_hierarchy", Schema = "hmsmaster")]
    public class UiControlHierarchy
    {
        [Key]
        [Column("hierarchy_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long HierarchyId { get; set; }

        [Required]
        [Column("uicontrolmenu_id")]
        public long UiControlMenuId { get; set; }

        /// <summary>
        /// PostgreSQL ltree type is generally mapped to string in Npgsql.
        /// Ensure the ltree extension is enabled in your DB.
        /// </summary>
        [Column("hierarchy_path", TypeName = "ltree")]
        public string HierarchyPath { get; set; }

        // Navigation Properties

        [ForeignKey("UiControlMenuId")]
        public virtual UiControlMaster UiControlMaster { get; set; }
    }
}
