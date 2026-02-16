using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("org_uicontrol", Schema = "hmsmaster")]
    public class OrgUiControl
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("org_uicontrol_id")]
        public long OrgUiControlId { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Required]
        [Column("uicontrolmenu_id")]
        public long UiControlMenuId { get; set; }

        /// <summary>
        /// Maps to PostgreSQL 'ltree'. 
        /// Requires Npgsql.EntityFrameworkCore.PostgreSQL for proper mapping.
        /// </summary>
        [Column("hierarchy_path", TypeName = "ltree")]
        public string? HierarchyPath { get; set; }

        [Required]
        [Column("role_id")]
        public long RoleId { get; set; }

        [Required]
        [Column("allow_read")]
        public bool AllowRead { get; set; } = false;

        [Required]
        [Column("allow_edit")]
        public bool AllowEdit { get; set; } = false;

        [Column("render_control")]
        public bool? RenderControl { get; set; } = false;

        [Column("access_granted_on")]
        public DateTime? AccessGrantedOn { get; set; }

        [Column("access_granted_by")]
        public int? AccessGrantedBy { get; set; }

        // --- Navigation Properties ---

        [ForeignKey("UiControlMenuId")]
        public virtual UiControlMaster? UiControlMaster { get; set; }

        // Note: You would typically have models for Organisation, Role, and User 
        // to link these foreign keys as virtual properties.
    }

}
