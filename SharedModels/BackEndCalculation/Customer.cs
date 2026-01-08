namespace SharedModels.BackEndCalculation
{
    public interface ICustomer
    {
        int ClientId { get; set; }
        int OrgId { get; set; }
        string? PasClientId { get; set; }
        bool IsStaff { get; set; }
        DateTime? CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
        DateTime? DoB { get; set; }
        int? Gender { get; set; }
    }
    public class Insured : ICustomer
    {
        public int ClientId { get; set; }
        public int InsuredID { get { return ClientId; } set { ClientId = value; } }
        public int OrgId { get; set; }
        public int InsuredOrgId { get { return OrgId; } set { OrgId = value; } }
        public string? PasClientId { get; set; }
        public string? InsuredPasId { get { return PasClientId; } set { PasClientId = value; } }
        public bool IsStaff { get; set; }
        public bool InsuredIsStaff { get { return IsStaff; } set { IsStaff = value; } }
        public DateTime? CreatedAt { get; set; }
        public DateTime? InsuredCreatedAt { get { return CreatedAt; } set { CreatedAt = value; } }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? InsuredUpdatedAt { get { return UpdatedAt; } set { UpdatedAt = value; } }
        public DateTime? DoB { get; set; }
        public DateTime? InsuredDoB { get { return DoB; } set { DoB = value; } }
        public int? Gender { get; set; }
        public int? InsuredGender { get { return Gender; } set { Gender = value; } }
    }
    public class Owner : ICustomer
    {
        public int ClientId { get; set; }
        public int OwnerID { get { return ClientId; } set { ClientId = value; } }
        public int OrgId { get; set; }
        public int OwnerOrgId { get { return OrgId; } set { OrgId = value; } }
        public string? PasClientId { get; set; }
        public string? OwnerPasId { get { return PasClientId; } set { PasClientId = value; } }
        public bool IsStaff { get; set; }
        public bool OwnerIsStaff { get { return IsStaff; } set { IsStaff = value; } }
        public DateTime? CreatedAt { get; set; }
        public DateTime? OwnerCreatedAt { get { return CreatedAt; } set { CreatedAt = value; } }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? OwnerUpdatedAt { get { return UpdatedAt; } set { UpdatedAt = value; } }
        public DateTime? DoB { get; set; }
        public DateTime? OwnerDoB { get { return DoB; } set { DoB = value; } }
        public int? Gender { get; set; }
        public int? OwnerGender { get { return Gender; } set { Gender = value; } }
    }
}