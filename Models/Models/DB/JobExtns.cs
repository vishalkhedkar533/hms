using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("job_extns", Schema = "scheduler")]
    public class JobExtns
    {
        [Key]
        [Column("job_config_id")]
        public int JobConfigId { get; set; }

        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("comments")]
        public string? Comments { get; set; }

        [Column("filter")]
        public string? Filter { get; set; }
    }
}
