namespace Tasks.Models
{
    public class AgentCommission
    {
        public int AgentID { get; set; }
        public int OrgId { get; set; }
        //entrydate, finperiodfrom, finperiodto
        public DateTime EntryDate { get; set; }
        public DateTime FinPeriodFrom { get; set; }
        public DateTime FinPeriodTo { get; set; }
        public decimal? TotalCommission { get; set; }
        public decimal? ProfTax { get; set; }
        public decimal? Tds { get; set; }= 0;
        public decimal? Igst { get; set; }= 0;
        public decimal? Cgst { get; set; }= 0;
        public decimal? Sgst { get; set; }= 0;
        public decimal? Ugst { get; set; }= 0;
        public int BalCommAmt { get; set; }= 0;
    }
}
