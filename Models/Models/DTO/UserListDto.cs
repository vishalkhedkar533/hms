namespace Models.DTO
{
    public sealed record UserListDto
    {
        public int UserId { get; init; }
        public string? Username { get; init; }
        public string? EmailId { get; init; }
        public bool IsActive { get; init; }
        public bool IsLocked { get; init; }
        public int FailedLoginAttempts { get; init; }
    }
}
