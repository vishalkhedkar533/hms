namespace Models.DB
{
    // Sets the table name and schema for EF Core mapping.
    //[Table("subscriber", Schema = "app_subscription")]
    //public class Subscriber
    //{
    //    /// <summary>
    //    /// Maps to 'subscriberid serial4 NOT NULL'. The primary key.
    //    /// </summary>
    //    [Key]
    //    [Column("subscriberid")]
    //    public int SubscriberId { get; set; }

    //    /// <summary>
    //    /// Maps to 'subscribername varchar(500) NOT NULL'.
    //    /// </summary>
    //    [Required]
    //    [StringLength(500)]
    //    [Column("subscribername")]
    //    public string SubscriberName { get; set; }

    //    // --- Navigation Property (One-to-Many Relationship) ---

    //    /// <summary>
    //    /// Collection of dependent Organisation entities.
    //    /// This is the 'one' side of the one-to-many relationship.
    //    /// </summary>
    //    public virtual ICollection<Organisation> Organisations { get; set; } = new List<Organisation>();
    //}

}
