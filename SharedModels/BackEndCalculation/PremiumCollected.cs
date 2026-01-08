using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.BackEndCalculation
{
    public class PremiumCollected
    {
        [Column("premiu_coll_id")]
        [Description("premiu_coll_id")]
        public int PremiuCollId { get; set; }
        [Column("orgid")]
        [Description("orgid")]
        public int OrgId { get; set; }
        [Column("policyref")]
        [Description("policyref")]
        public int? PolicyRef { get; set; }
        [Column("premium_received_dt")]
        [Description("premium_received_dt")]
        public DateTime? PremiumReceivedDt { get; set; }
        [Column("premium_type")]
        [Description("premium_type")]
        public int? PremiumType { get; set; }
        [Column("premium_amt")]
        [Description("premium_amt")]
        public decimal? PremiumAmt { get; set; }
    }
}