namespace CommonLibrary.mapping
{
    public sealed class MappingRoot
    {
        public string? ConnectionStringKey { get; set; }
        public Dictionary<string, EntityMapping>? Entities { get; set; }
    }

    // Entity mapping represents a map: OperationName -> OperationMapping
    // Matches JSON like:
    // "Entities": { "Job": { "Insert": { "Function": "...", "Script": "..." } } }
    public sealed class EntityMapping : Dictionary<string, OperationMapping>
    {
    }

    public sealed class OperationMapping
    {
        public string? Function { get; set; }
        public string? Script { get; set; }
    }
}