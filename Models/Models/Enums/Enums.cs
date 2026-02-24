namespace Models.Enums
{
    public enum AddressType : int
    {
        Work = 1,
        Correspondence = 2,
        Permanent = 3
    }
    public enum ReferenceType : int
    {
        Agent = 1,
    }
    public enum BankAccType
    {
        Savings = 1,
        Current = 2,
    }
    public enum MartialStatus :int
    {
        Single = 1,
        Married = 2,
        Divorced = 3,
        Widowed = 4
    }
    public enum PreferredPaymentMode
    {
        BankTransfer = 1,
        Cheque = 2,
        Cash = 3,
        UPI = 4,
        Wallet = 5
    }
    public enum CommissionStatus :int
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        OnHold = 4,
        Paid = 5
    }
    public enum MenuSearchFor : int
    {
        User = 1,
        Role = 2
    }

    public enum SrStatus : int
    {
        Created = 1,
        Pending = 2,
        Approved = 3,
        Rejected = 4,
    }
}
