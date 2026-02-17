using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("org_uicontrol", Schema = "hmsmaster")]
    public class OrgUiControl
    {
        [Key]
        [Column("org_uicontrol_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrgUiControlId { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Required]
        [Column("hierarchy_id")]
        public long HierarchyId { get; set; }

        [Required]
        [Column("role_id")]
        public long RoleId { get; set; }

        [Required]
        [Column("allow_edit")]
        public bool AllowEdit { get; set; } = false;

        [Column("render_control")]
        public bool? RenderControl { get; set; } = false;

        [Column("access_granted_on")]
        public DateTime? AccessGrantedOn { get; set; }

        [Column("access_granted_by")]
        public int? AccessGrantedBy { get; set; }

        // Navigation Properties (Optional, based on your FKs)

        [ForeignKey("OrgId")]
        public virtual Organisation Organisation { get; set; }

        [ForeignKey("HierarchyId")]
        public virtual UiControlHierarchy UiControlHierarchy { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        [ForeignKey("AccessGrantedBy")]
        public virtual User AccessGrantedByUser { get; set; }
    }
}
