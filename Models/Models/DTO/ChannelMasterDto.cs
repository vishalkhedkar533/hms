namespace Models.DTO
{
    // DTO used for binding create/update payloads
    public record ChannelMasterDto
    {
        public int? ChannelId { get; init; }
        public string? ChannelCode { get; init; }
        public string? ChannelName { get; init; }
        public string? Description { get; init; }
        public bool IsActive { get; init; } = true;
    }
}
