using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("user_branch_mapping", Schema = "hmsmaster")]
    public class UserBranchMapping
    {
        [Key]
        [Column("user_branch_mapping_id")]
        public int UserBranchMappingId { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("branch_id")]
        public long BranchId { get; set; }

        [Required]
        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Required]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("modified_by")]
        public int? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey(nameof(OrgId))]
        public virtual Organisation? Organisation { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(BranchId))]
        public virtual BranchMaster? Branch { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User? CreatedByUser { get; set; }
    }

    public class UserBranchMappingDto
    {
        public int UserId { get; set; }
        public List<long>? BranchIds { get; set; }
    }
}
