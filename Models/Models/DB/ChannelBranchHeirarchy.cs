using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("channel_branch_heirarchy", Schema = "hmsmaster")]
    public class ChannelBranchHeirarchy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("channel_location_heirarchy_id")]
        public long ChannelLocationHeirarchyId { get; set; }

        [Column("orgid")]
        public int? OrgId { get; set; }

        [Required]
        [Column("channel_id")]
        public long ChannelId { get; set; }

        [Column("sub_channel_id")]
        public long? SubChannelId { get; set; }

        // Note: ltree is treated as a string in C# but mapped to ltree type in Postgres
        [Column("hierarchy_path", TypeName = "ltree")]
        public string? HierarchyPath { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [MaxLength(100)]
        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Required]
        [Column("effective_from_date")]
        public DateTime EffectiveFromDate { get; set; }

        [Column("effective_to_date")]
        public DateTime? EffectiveToDate { get; set; }

        // Navigation Properties
        [ForeignKey("ChannelId")]
        public virtual ChannelMaster? Channel { get; set; }

        [ForeignKey("SubChannelId")]
        public virtual SubChannelMaster? SubChannel { get; set; }
    }
}