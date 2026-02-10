namespace Tasks.Models
{
    public class CommsLedger
    {
        public long AgentPeriodCommsId { get; set; }

        public int? OrgId { get; set; }

        public int AgentID { get; set; }

        public int JobExeHistId { get; set; }

        public DateTime? EntryDate { get; set; }

        public DateTime? FinPeriodFrom { get; set; }

        public DateTime? FinPeriodTo { get; set; }

        public string TransType { get; set; } = "0";

        public decimal TransAmt { get; set; } = 0;

        public decimal BalCommAmt { get; set; } = 0;
    }
}
