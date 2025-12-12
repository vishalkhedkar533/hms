using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("performance_snapshots", Schema = "comss")]
    public class PerformanceSnapshot
    {
        [Key]
        [Column("snapshotid")]
        public int SnapshotId { get; set; }

        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("periodfrom")]
        public DateTime PeriodFrom { get; set; }

        [Column("periodto")]
        public DateTime PeriodTo { get; set; }

        [Column("commissionbudget")]
        public double? CommissionBudget { get; set; }

        [Column("commissionactual")]
        public double? CommissionActual { get; set; }
    }
}
