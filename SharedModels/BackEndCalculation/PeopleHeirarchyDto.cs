namespace SharedModels.BackEndCalculation
{
    public class PeopleHeirarchyDto
    {
        public int? AgentId { get; set; }
        public long? HierarchyId { get; set; }
        public string? AgentCode { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public PeopleHeirarchyDto? Supervisors { get; set; }
        public string? HierarchyPath { get; set; } = string.Empty;
    }
    

    //public class Address
    //{
    //    public long AddressID { get; set; }
    //    public string? AddressType { get; set; } //enum AddressType
    //    public int RefKey { get; set; }
    //    public string? RefType { get; set; }//enum RefType
    //    public string? AddressLine1 { get; set; }
    //    public string? AddressLine2 { get; set; }
    //    public string? AddressLine3 { get; set; }
    //    public string? City { get; set; }
    //    public string? State { get; set; }
    //    public string? Country { get; set; }
    //    public string? PIN { get; set; }
    //    public string? Landmark { get; set; }
    //}
    //public class BankAccount
    //{
    //    public int Id { get; set; }
    //    public int RefKey { get; set; }
    //    public string RefType { get; set; }
    //    public string AccountHolderName { get; set; } = null!;
    //    public string AccountNumber { get; set; } = null!;
    //    public string IFSC { get; set; } = null!;
    //    public string? MICR { get; set; } = null!;
    //    public string? BankName { get; set; }
    //    public string? BranchName { get; set; }
    //    public int AccountType { get; set; } = 1;
    //    public DateTime? ActiveSince { get; set; } = DateTime.Now;
    //    public string? FactoringHouse { get; set; }
    //    public string? PreferredPaymentMode { get; set; }
    //    public string? AccountTypeDesc { get; set; }
    //}

    //public class PersonalInfo
    //{
    //    public int PersonalInfoId { get; set; }
    //    public int RefKey { get; set; }
    //    public string? RefType { get; set; }
    //    public DateTime DateOfBirth { get; set; }
    //    public string? PanNumber { get; set; }
    //    public string? Email { get; set; }
    //    public string? MobileNo { get; set; }
    //    public string? WorkContactNo { get; set; }
    //    public string? ResidenceContactNo { get; set; }
    //    public string? BloodGroup { get; set; }
    //    public string? BirthPlace { get; set; }
    //    public int? EducationCode { get; set; }
    //    public string? EducationLevel { get; set; }
    //    public string? WorkProfile { get; set; }
    //    public decimal? AnnualIncome { get; set; }
    //    public int? WorkExpMonths { get; set; }
    //}
    //public class Nominee
    //{
    //    public int NomineeID { get; set; }
    //    public int RefKey { get; set; }
    //    public string? RefType { get; set; }
    //    public string NomineeName { get; set; }
    //    public string Relationship { get; set; }
    //    public decimal PercentageShare { get; set; }
    //    public bool IsActive { get; set; }
    //    public long NomineeAge { get; set; }
    //}
}