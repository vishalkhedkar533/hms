namespace Tasks.Models.DB
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
        public int InsuredID { get { return this.ClientId; } set { this.ClientId = value; } }
        public int OrgId { get; set; }
        public int InsuredOrgId { get { return this.OrgId; } set { this.OrgId = value; } }
        public string? PasClientId { get; set; }
        public string? InsuredPasId { get { return this.PasClientId; } set { this.PasClientId = value; } }
        public bool IsStaff { get; set; }
        public bool InsuredIsStaff { get { return this.IsStaff; } set { this.IsStaff = value; } }
        public DateTime? CreatedAt { get; set; }
        public DateTime? InsuredCreatedAt { get { return this.CreatedAt; } set { this.CreatedAt = value; } }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? InsuredUpdatedAt { get { return this.UpdatedAt; } set { this.UpdatedAt = value; } }
        public DateTime? DoB { get; set; }
        public DateTime? InsuredDoB { get { return this.DoB; } set { this.DoB = value; } }
        public int? Gender { get; set; }
        public int? InsuredGender { get { return this.Gender; } set { this.Gender = value; } }
    }
    public class Owner : ICustomer
    {
        public int ClientId { get; set; }
        public int OwnerID { get { return this.ClientId; } set { this.ClientId = value; } }
        public int OrgId { get; set; }
        public int OwnerOrgId { get { return this.OrgId; } set { this.OrgId = value; } }
        public string? PasClientId { get; set; }
        public string? OwnerPasId { get { return this.PasClientId; } set { this.PasClientId = value; } }
        public bool IsStaff { get; set; }
        public bool OwnerIsStaff { get { return this.IsStaff; } set { this.IsStaff = value; } }
        public DateTime? CreatedAt { get; set; }
        public DateTime? OwnerCreatedAt { get { return this.CreatedAt; } set { this.CreatedAt = value; } }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? OwnerUpdatedAt { get { return this.UpdatedAt; } set { this.UpdatedAt = value; } }
        public DateTime? DoB { get; set; }
        public DateTime? OwnerDoB { get { return this.DoB; } set { this.DoB = value; } }
        public int? Gender { get; set; }
        public int? OwnerGender { get { return this.Gender; } set { this.Gender = value; } }
    }
}