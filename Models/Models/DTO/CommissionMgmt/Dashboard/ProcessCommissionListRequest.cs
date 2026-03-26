namespace Models.DTO.CommissionMgmt.Dashboard
{
    public class ProcessCommissionListRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? CommissionName { get; set; }
    }
}
