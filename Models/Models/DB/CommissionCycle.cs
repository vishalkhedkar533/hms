using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("commission_cycle", Schema = "comss")]
    public class CommissionCycle
    {
        [Key]
        [Column("cycleid")]
        public int CycleId { get; set; }

        [Column("cyclecode")]
        public string? CycleCode { get; set; }

        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("commissiontype")]
        public string? CommissionType { get; set; }

        [Column("countofentities")]
        public double? CountOfEntities { get; set; }

        [Column("avgcommission")]
        public double? AvgCommission { get; set; }

        [Column("nb_revenue")]
        public double? NbRevenue { get; set; }

        [Column("nb_commission")]
        public double? NbCommission { get; set; }

        [Column("status")]
        public string? Status { get; set; }
    }
}
