using Models.HMSConsts;

namespace Models.DB
{
    public class Address
    {
        Int64 AddressID { get; set; }
        AddressType addressType  = AddressType.Permanent;
        public int RefKey = 0;
        public Reftype? RefType = Reftype.Agent;
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string City { get; set; } = null!;
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PIN { get; set; }
        public string? landmark { get; set;}
    }
}
