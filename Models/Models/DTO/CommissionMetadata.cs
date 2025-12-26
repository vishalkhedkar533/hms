namespace Models.DTO
{
    public class CommissionMetadata
    {
        public FieldInfo[]? policy { get; set; }
        public FieldInfo[]? premium { get; set; }
        public FieldInfo[]? customer { get; set; }
    }

    public class FieldInfo
    {
        public string? name { get; set; }
        public string? description { get; set; }
        public string? dataType { get; set; }
    }
}