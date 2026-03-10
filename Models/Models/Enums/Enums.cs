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
        /*
         * select *
         * from hmsmaster.keyvalueentries k 
         * where k.entrycategory  = 'SR_STATUS'
         * order by  entryidentity ;
         */
        Created = 1,
        PendingDecision = 2,
        Approved = 3,
        Rejected = 4,
    }
    public enum ApproverDecision : int
    {
        /*
         * select *
         * from hmsmaster.keyvalueentries k 
         * where k.entrycategory  = 'SR_DECISION'
         * order by  entryidentity ;
         */
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        OnHold = 4
    }
    public enum GroupDataByPeriod : int
    {
        ByNone = 1,
        ByMonth = 2,
        ByQuarter = 3
    }
}
