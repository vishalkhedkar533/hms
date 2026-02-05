using SharedModels.BackEndCalculation;

namespace Tasks.Models
{
    //hmsmaster.financialperiod
    public class FinancialPeriod
    {
        public int PeriodId { get; set; }
        public int OrgId { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
        public int TdsThresholdLimit { get; set; } = 0;
        public decimal TdsStandardRate { get; set; } = 0;
        public decimal NoPanRate { get; set; } = 0;
        // New GST Columns
        public decimal IGST { get; set; } = 0;
        public decimal SGST { get; set; } = 0;
        public decimal CGST { get; set; } = 0;
        public decimal UGST { get; set; } = 0;
        // Navigation Property
        public Organisation? Organisation { get; set; }
    }
}
