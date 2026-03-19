using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.DB
{
    [Table("agent_branch_mapping", Schema = "hmsmaster")]
    [Index(nameof(OrgId), nameof(AgentId), nameof(BranchId), IsUnique = true, Name = "uq_agent_branch_mapping")]
    public class AgentBranchMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("mapping_id")]
        public long MappingId { get; set; }

        [Required]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Required]
        [Column("agent_id")]
        public int AgentId { get; set; }

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

        [ForeignKey(nameof(AgentId))]
        public virtual Agent? Agent { get; set; }

        [ForeignKey(nameof(BranchId))]
        public virtual BranchMaster? Branch { get; set; }
    }

    public class AgentBranchMappingDto
    {
        public long? MappingId { get; set; }
        public int AgentId { get; set; }
        public long BranchId { get; set; }
    }

    public class RegulatorBranchDto
    {
        public long MappingId { get; set; }
        public int OrgId { get; set; }
        public int AgentId { get; set; }
        public string? AgentCode { get; set; }
        public string? AgentName { get; set; }
        public long BranchId { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public bool IsReportedToRegulator { get; set; }
        public string? RegulatorCode { get; set; }
        public long? ChannelId { get; set; }
        public long? SubChannelId { get; set; }
        public long? LocationMasterId { get; set; }
    }
}
