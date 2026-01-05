namespace Tasks.Models.DB
{
    public class CommissionCalcRecord
    {
        public Ins_Policy? Policy { get; set; } = null;
        public PremiumCollected? PremiumCollected { get; set; } = null;
        public Agent? Agent { get; set; } = null;
        public Organisation? Organisation { get; set; } = null;
    }
}
