using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("channel_branch_heirarchy", Schema = "hmsmaster")]
    public class ChannelBranchHeirarchy
    {
        [Key]
        [Column("channel_location_heirarchy_id")]
        public long ChannelLocationHeirarchyId { get; set; }
        [Column("orgid")]
        public int? OrgId { get; set; }

        [Required]
        [Column("channel_id")]
        public long ChannelId { get; set; }

        [Column("sub_channel_id")]
        public long? SubChannelId { get; set; }

        // ltree is typically mapped to string in C#
        [Column("hierarchy_path")]
        public string? HierarchyPath { get; set; }

        [Required]
        [StringLength(100)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [StringLength(100)]
        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Required]
        [Column("effective_from_date")]
        public DateTime EffectiveFromDate { get; set; }

        [Column("effective_to_date")]
        public DateTime? EffectiveToDate { get; set; }

        [Column("branch_id")]
        public long? BranchId { get; set; }
    }
}