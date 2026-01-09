namespace Tasks.Models.DB
{
    public class CommissionCalcRecord
    {
        public PremiumCollected PremiumCollected { get; set; } = null!;
        public Ins_Policy Policy { get; set; } = null!;
        public Organisation Organisation { get; set; } = null!;
        public Agent Agent { get; set; } = null!;
        public Insured Insured { get; set; } = new Insured();
        public Owner Owner { get; set; } = new Owner();
        public CommRate CommRate { get; set; } = new CommRate();
    }
}
