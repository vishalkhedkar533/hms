using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DB
{
    [Table("commssiondashboard", Schema = "hms")]
    public class CommissionDashboard
    {
        [Key]
        public int? orgId { get; set; }
        public double? CommissionBudget { get; set; }
        public double? CommissionPaid { get; set; }
        public double? CommissionOnHold { get; set; }
        public double? CommissionNotPaid { get; set; }
        public double? LastCycleCommission { get; set; }
        public double? LastCycleEntities { get; set; }
        public double? ThisCycleCommission { get; set; }
        public double? ThisCycleEntities { get; set; }
        public double? ThisCycleAvgCommission { get; set; }
    }
}
