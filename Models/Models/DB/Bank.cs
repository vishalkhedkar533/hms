using Models.Enums;

namespace Models.DB
{
    public class BankAccount
    {
        public int Id { get; set; }
        public int RefKey = 0;
        public ReferenceType? RefType = ReferenceType.Agent;
        public string AccountHolderName { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public string IFSC { get; set; } = null!;
        public string MICR { get; set; } = null!;
        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public BankAccType AccountType = BankAccType.Savings;
        public DateTime? ActiveSince = DateTime.Now;
        public string? FactoringHouse = null;
        public PreferredPaymentMode preferredPaymentMode = PreferredPaymentMode.BankTransfer;
    }
}
