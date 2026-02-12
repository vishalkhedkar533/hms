using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.BackEndCalculation
{
    public class CommissionConfig
    {
        [Column("commission_config_id")]
        [Description("commission_config_id")]
        public int CommissionConfigId { get; set; }
        [Column("job_config_id")]
        [Description("job_config_id")]
        public int JobConfigId { get; set; }
        [Column("formula")]
        [Description("formula")]
        public string? Formula { get; set; }        
    }
}
