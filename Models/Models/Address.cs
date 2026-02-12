namespace Models
{
    //[Index(nameof(RefKey), nameof(RefType), nameof(AddressType), IsUnique = true)]
    //[Table("Address", Schema = "hms")]
    //public class Address
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.None)] // Since it's not SERIAL/IDENTITY in SQL
    //    public long AddressID { get; set; }

    //    [Column]
    //    public AddressType? AddressType { get; set; } //enum AddressType

    //    [Column]
    //    public int RefKey { get; set; }

    //    [Column]
    //    public ReferenceType? RefType { get; set; }//enum RefType

    //    [StringLength(255)]
    //    [Column]
    //    public string? AddressLine1 { get; set; }

    //    [StringLength(255)]
    //    [Column]
    //    public string? AddressLine2 { get; set; }

    //    [StringLength(255)]
    //    [Column]
    //    public string? AddressLine3 { get; set; }

    //    [StringLength(100)]
    //    [Column]
    //    public string? City { get; set; } 

    //    [StringLength(100)]
    //    [Column]
    //    public string? State { get; set; }

    //    [StringLength(100)]
    //    [Column]
    //    public string? Country { get; set; }

    //    [StringLength(20)]
    //    [Column]
    //    public string? PIN { get; set; }

    //    [StringLength(255)]
    //    [Column]
    //    public string? Landmark { get; set; }

    //    // Define unique constraint (RefKey, RefType, AddressType) via Fluent API in DbContext
    //}
}
