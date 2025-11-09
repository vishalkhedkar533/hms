using Microsoft.AspNetCore.SignalR.Protocol;
using Models.HMSConsts;
using System.Runtime.InteropServices;

namespace Models.DB
{
    public class PersonalInfo
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        public string? PanNumber { get; set; }
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
        public string? WorkContactNo { get; set; }
        public string? ResidenceContactNo { get; set; }
        public string? BloodGroup { get; set; }
        public Address? PermanentAddress { get; set; }
        public Address? CorrepondenceAddress1 { get; set; }
        public Address? CorrepondenceAddress2 { get; set; }
        public Address? CorrepondenceAddress3 { get; set; }
        public Address? WorkAddress { get; set; }
        public string? BirthPlace { get; set; }
        public MartialStatus? MartialStatus { get; set; }
        public string EducationCode { get; set; } = null!;
        public string EducationLevel { get; set; } = null!;
        public string WorkProfile { get; set; } = null!;
        public decimal AnnualIncome { get; set; }
        public Int64 WorkExperience { get; set; }
        /*
         * Eductaion Code
Eductaion Level
Work Profile
Annual Income 
Work Experience 
         */
    }
}
