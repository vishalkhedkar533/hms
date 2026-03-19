using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("user_audit_trail", Schema = "hms")]
    public class UserAuditTrail
    {
        [Key]
        [Column("audit_id")]
        public long AuditId { get; set; }

        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [StringLength(100)]
        [Column("field_name")]
        public string? FieldName { get; set; } 

        [StringLength(255)]
        [Column("old_value")]
        public string? OldValue { get; set; }

        [StringLength(255)]
        [Column("new_value")]
        public string? NewValue { get; set; }

        [StringLength(100)]
        [Column("changed_by")]
        public string? ChangedBy { get; set; }

        [Column("changed_date")]
        public DateTime? ChangedDate { get; set; }
        
        [StringLength(100)]
        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }          
        [StringLength(100)]
        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Column("rowversion")]
        [ConcurrencyCheck]
        public int? RowVersion { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
