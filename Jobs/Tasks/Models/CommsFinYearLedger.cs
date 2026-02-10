namespace Tasks.Models
{
    //refer comss.comms_fy_ledger
    public class CommsFinYearLedger
    {
        public long FyPeriodCommsId { get; set; }
        public int? OrgId { get; set; }
        public int AgentId { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime FinPeriodFrom { get; set; }
        public DateTime FinPeriodTo { get; set; }
        public decimal BalCommAmt { get; set; } = 0;
    }
}