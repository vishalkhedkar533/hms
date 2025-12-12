using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("adhoc_commission", Schema = "comss")]
    public class AdhocCommission
    {
        [Key]
        [Column("adhoccommissionid")]
        public int AdhocCommissionId { get; set; }

        [Column("orgid")]
        public int? OrgId { get; set; }

        [Column("branchid")]
        public int? BranchId { get; set; }

        [Column("requestid")]
        public int? RequestId { get; set; }

        [Column("submittedon")]
        public DateTime? SubmittedOn { get; set; }

        [Column("submittedby")]
        public int? SubmittedBy { get; set; }
    }
}
