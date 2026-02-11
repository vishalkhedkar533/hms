namespace SharedModels.DTO.CommissionMgmt.Dashboard
{
    public class HoldCommissionResponseDto
    {
        public int OrgId { get; set; }
        public decimal AmountOnHold { get; set; }
        public int CurrentlyOnHold { get; set; }
        public int ReleasedThisMonth { get; set; }
        public List<HoldCommissionRecordDto>? Records { get; set; }
    }

    public class HoldCommissionRecordDto
    {
        public int HoldId { get; set; }
        public string? AgentName { get; set; }
        public string? Reason { get; set; }
        public decimal Amount { get; set; }
        public DateTime HeldOn { get; set; }
        public string? Status { get; set; }
        public bool CanRelease { get; set; }
    }
}
