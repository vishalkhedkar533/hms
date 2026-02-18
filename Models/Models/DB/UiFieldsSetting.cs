using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("ui_fields_setting", Schema = "hmsmaster")]
    public class UiFieldsSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("cntrl_id")]
        public int? CntrlId { get; set; }

        [Column("render")]
        public bool? Render { get; set; } = true;

        [Column("allow_edit")]
        public bool? AllowEdit { get; set; } = false;

        [Column("sort_order")]
        public int? SortOrder { get; set; } = 0;

        [Required]
        [Column("access_granted_on")]
        public DateTime AccessGrantedOn { get; set; }

        [Required]
        [Column("access_granted_by")]
        public int AccessGrantedBy { get; set; }

        [Column("role_id")]
        public int? RoleId { get; set; }

        [Column("approveroneid")]
        public int? ApproverOneId { get; set; }

        [Column("approvertwoid")]
        public int? ApproverTwoId { get; set; }

        [Column("approverthreeid")]
        public int? ApproverThreeId { get; set; }

        [Column("usedefaultapprover")]
        public bool? UseDefaultApprover { get; set; } = true;

        // --- Navigation Properties ---

        [ForeignKey("CntrlId")]
        public virtual UiField? UiField { get; set; }

        [ForeignKey("AccessGrantedBy")]
        public virtual User? GrantedByUser { get; set; }

        [ForeignKey("ApproverOneId")]
        public virtual User? ApproverOne { get; set; }

        [ForeignKey("ApproverTwoId")]
        public virtual User? ApproverTwo { get; set; }

        [ForeignKey("ApproverThreeId")]
        public virtual User? ApproverThree { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }
    }
}
