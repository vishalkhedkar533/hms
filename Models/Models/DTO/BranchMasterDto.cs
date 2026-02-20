namespace Models.DTO
{
    public record BranchMasterDto
    {
        public long? BranchId { get; init; }
        public string? BranchCode { get; init; }
        public string? BranchName { get; init; }
        public string? Address { get; init; }
        public int? State { get; init; }
        public string? PhoneNumber { get; init; }
        public string? EmailId { get; init; }
        public bool IsActive { get; init; } = true;
        public long? LocationMasterId { get; init; }
    }
}