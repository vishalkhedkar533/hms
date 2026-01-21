using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Column("clientid")]
        [Description("clientid")]
        public int ClientId { get; set; }
        [Column("clientid")]
        [Description("clientid")]
        public int InsuredID { get { return ClientId; } set { ClientId = value; } }
        [Column("orgid")]
        [Description("orgid")]
        public int OrgId { get; set; }
        [Column("orgid")]
        [Description("orgid")]
        public int InsuredOrgId { get { return OrgId; } set { OrgId = value; } }
        [Column("pas_clientid")]
        [Description("pasclientid")]
        public string? PasClientId { get; set; }
        [Column("pas_clientid")]
        [Description("pasclientid")]
        public string? InsuredPasId { get { return PasClientId; } set { PasClientId = value; } }
        [Column("isstaff")]
        [Description("isstaff")]
        public bool IsStaff { get; set; }
        [Column("isstaff")]
        [Description("isstaff")]
        public bool InsuredIsStaff { get { return IsStaff; } set { IsStaff = value; } }
        [Column("created_at")]
        [Description("createdat")]
        public DateTime? CreatedAt { get; set; }
        [Column("created_at")]
        [Description("createdat")]
        public DateTime? InsuredCreatedAt { get { return CreatedAt; } set { CreatedAt = value; } }
        [Column("updated_at")]
        [Description("updatedat")]
        public DateTime? UpdatedAt { get; set; }
        [Column("updated_at")]
        [Description("updatedat")]
        public DateTime? InsuredUpdatedAt { get { return UpdatedAt; } set { UpdatedAt = value; } }
        [Column("dob")]
        [Description("dob")]
        public DateTime? DoB { get; set; }
        [Column("dob")]
        [Description("dob")]
        public DateTime? InsuredDoB { get { return DoB; } set { DoB = value; } }
        [Column("gender")]
        [Description("gender")]
        public int? Gender { get; set; }
        [Column("gender")]
        [Description("gender")]
        public int? InsuredGender { get { return Gender; } set { Gender = value; } }
    }
    public class Owner : ICustomer
    {
        [Column("clientid")]
        [Description("clientid")] 
        public int ClientId { get; set; }
        [Column("clientid")]
        [Description("clientid")] 
        public int OwnerID { get { return ClientId; } set { ClientId = value; } }
        [Column("orgid")]
        [Description("orgid")] 
        public int OrgId { get; set; }
        [Column("orgid")]
        [Description("orgid")] 
        public int OwnerOrgId { get { return OrgId; } set { OrgId = value; } }
        [Column("pas_clientid")]
        [Description("pasclientid")] 
        public string? PasClientId { get; set; }
        [Column("pas_clientid")]
        [Description("pasclientid")] 
        public string? OwnerPasId { get { return PasClientId; } set { PasClientId = value; } }
        [Column("isstaff")]
        [Description("isstaff")]
        public bool IsStaff { get; set; }
        [Column("isstaff")]
        [Description("isstaff")]
        public bool OwnerIsStaff { get { return IsStaff; } set { IsStaff = value; } }
        [Column("created_at")]
        [Description("createdat")] 
        public DateTime? CreatedAt { get; set; }
        [Column("created_at")]
        [Description("createdat")]
        public DateTime? OwnerCreatedAt { get { return CreatedAt; } set { CreatedAt = value; } }
        [Column("updated_at")]
        [Description("updatedat")]
        public DateTime? UpdatedAt { get; set; }
        [Column("updated_at")]
        [Description("updatedat")]
        public DateTime? OwnerUpdatedAt { get { return UpdatedAt; } set { UpdatedAt = value; } }
        [Column("dob")]
        [Description("dob")]
        public DateTime? DoB { get; set; }
        [Column("dob")]
        [Description("dob")]
        public DateTime? OwnerDoB { get { return DoB; } set { DoB = value; } }
        [Column("gender")]
        [Description("gender")]
        public int? Gender { get; set; }
        [Column("gender")]
        [Description("gender")]
        public int? OwnerGender { get { return Gender; } set { Gender = value; } }
    }
}