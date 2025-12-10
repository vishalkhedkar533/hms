namespace Models.Mapping
{
    public class DatabaseMappings
    {
        public string ConnectionStringKey { get; set; }
        public Dictionary<string, EntityMapping> Entities { get; set; }
    }

    public class EntityMapping
    {
        public string Insert { get; set; }
        public string Search { get; set; }
    }
}
