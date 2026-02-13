namespace Models.DTO
{
    public sealed record MenuAccessDto
    {
        public int? ParentMenuId { get; init; }
        public string? ParentMenuName { get; init; }
        public int MenuId { get; init; }
        public string? MenuName { get; init; }
        public bool HasAccess { get; init; }
    }
}
