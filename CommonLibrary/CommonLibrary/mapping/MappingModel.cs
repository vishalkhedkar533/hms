namespace CommonLibrary.mapping
{
    public sealed class MappingModel
    {
        public Dictionary<string, EntityMapping>? Entities { get; set; }
    }

    public sealed class EntityMapping : Dictionary<string, OperationMapping>
    {
    }

    public sealed class OperationMapping
    {
        public string? ConnectionStringKey { get; set; }
        public string? Function { get; set; }
        public string? Script { get; set; }
    }
}