namespace Tasks.Models.DB
{
    public class CommRate
    {
        public int CommRateId { get; set; }
        public int OrgId { get; set; }
        public string? ProdCode { get; set; }
        public DateTime? ApplicableFrom { get; set; }
        public DateTime? ApplicableTo { get; set; }
        public decimal CommRateValue { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}