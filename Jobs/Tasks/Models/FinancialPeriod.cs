using SharedModels.BackEndCalculation;

namespace Tasks.Models
{
    public class FinancialPeriod
    {
        public int PeriodId { get; set; }
        public int OrgId { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
        public int TdsThresholdLimit { get; set; } = 0;
        public decimal TdsStandardRate { get; set; } = 0;
        public decimal NoPanRate { get; set; } = 0;
        // Navigation Property
        public Organisation? Organisation { get; set; }
    }
}
