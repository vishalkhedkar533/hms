namespace Models.DB
{
    //[Index(nameof(RefKey), nameof(RefType), IsUnique = true)]
    //[Table("PersonalInfo", Schema = "hms")]
    //public class PersonalInfo
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public int PersonalInfoId { get; set; }

    //    [Required]
    //    [Column("RefKey")]
    //    public int RefKey { get; set; }

    //    [Column("RefType")]
    //    public ReferenceType? RefType { get; set; }

    //    [Required]
    //    [Column("DateOfBirth",TypeName ="date")]
    //    public DateOnly DateOfBirth { get; set; }

    //    [Column("PanNumber")]
    //    [StringLength(10)]
    //    public string? PanNumber { get; set; }

    //    [Column("Email")]
    //    [EmailAddress]
    //    public string? Email { get; set; }

    //    [Column("MobileNo")]
    //    [Phone]
    //    public string? MobileNo { get; set; }

    //    [Column("WorkContactNo")]
    //    [Phone]
    //    public string? WorkContactNo { get; set; }

    //    [Column("ResidenceContactNo")]
    //    [Phone]
    //    public string? ResidenceContactNo { get; set; }

    //    [Column("BloodGroup")]
    //    [StringLength(5)]
    //    public string? BloodGroup { get; set; }

    //    [Column("BirthPlace")]
    //    public string? BirthPlace { get; set; }

    //    [Column("MartialStatus")]
    //    public MartialStatus? MartialStatus { get; set; }

    //    [Column("EducationCode")]
    //    public int? EducationCode { get; set; }

    //    [Column("EducationLevel")]
    //    [StringLength(50)]
    //    public string? EducationLevel { get; set; }

    //    [Column("WorkProfile")]
    //    [StringLength(100)]
    //    public string? WorkProfile { get; set; }

    //    [Column("AnnualIncome")]
    //    [Range(0, double.MaxValue)]
    //    public decimal? AnnualIncome { get; set; }

    //    [Column("WorkExpMonths")]
    //    [Range(0, 600)]
    //    public int? WorkExpMonths { get; set; }
    //}

}
