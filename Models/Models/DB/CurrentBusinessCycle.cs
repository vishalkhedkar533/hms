using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("currentbusscycle", Schema = "comss")]
    public class CurrentBusinessCycle
    {
        [Key]
        [Column("currentbusscycleid")]
        public int CurrentBusinessCycleId { get; set; }

        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("cycle")]
        public string CycleType { get; set; } = null!;

        [Column("revenue")]
        public decimal RevenueAmount { get; set; }

        [Column("commission")]
        public decimal CommissionAmount { get; set; }

        [Column("revcommperct")]
        public decimal Percentage { get; set; }

    }
}
