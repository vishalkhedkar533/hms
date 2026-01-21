using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.BackEndCalculation
{
    public class CommRate
    {
        [Column("comm_rate_id")]
        [Description("comm_rate_id")]
        public int CommRateId { get; set; }
        [Column("orgid")]
        [Description("orgid")]
        public int OrgId { get; set; }
        [Column("prod_code")]
        [Description("prod_code")]
        public string? ProdCode { get; set; }
        [Column("applicable_from")]
        [Description("applicable_from")]
        public DateTime? ApplicableFrom { get; set; }
        [Column("applicable_to")]
        [Description("applicable_to")]
        public DateTime? ApplicableTo { get; set; }
        [Column("comm_rate")]
        [Description("comm_rate_value")]
        public decimal CommRateValue { get; set; }
        [Column("created_at")]
        [Description("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        [Column("created_by")]
        [Description("created_by")]
        public string? CreatedBy { get; set; }
        [Column("updated_at")]
        [Description("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        [Column("updated_by")]
        [Description("updated_by")]
        public string? UpdatedBy { get; set; }
        [Column("pol_yr_from")]
        [Description("pol_yr_from")]
        public int? PolicyYearFrom { get; set; }
        [Column("pol_yr_to")]
        [Description("pol_yr_to")]
        public int? PolicyYearTo { get; set; }
    }
}