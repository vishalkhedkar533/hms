namespace Tasks.Models.DB
{
    public class PremiumCollected
    {
        public int PremiuCollId { get; set; }
        public int OrgId { get; set; }
        public int? PolicyRef { get; set; }
        public DateTime? PremiumReceivedDt { get; set; }
        public int? PremiumType { get; set; }
        public decimal? PremiumAmt { get; set; }
    }
}