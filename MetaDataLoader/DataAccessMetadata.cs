namespace MetaDataLoader
{
    public class OperationConfig
    {
        // The name of the stored procedure or function (can be "na" for raw SQL)
        public string Function { get; set; } = string.Empty;

        // The actual SQL script or stored procedure call.
        public string Script { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the configuration for a single logical entity (e.g., Agent, Master).
    /// The keys of this dictionary are the names of the operations (Insert, Search, etc.).
    /// </summary>
    public class EntityConfig : Dictionary<string, OperationConfig>
    {
        // Inheriting from Dictionary<string, OperationConfig> directly simplifies JSON deserialization 
        // for the inner structure of "Agent" or "Master".
    }

    /// <summary>
    /// Represents the top-level structure of the data access configuration.
    /// </summary>
    public class DataAccessMetadata
    {
        // The key used to look up the actual connection string in appsettings.json
        public string ConnectionStringKey { get; set; } = string.Empty;

        // Dictionary where the key is the entity name (e.g., "Agent", "Master").
        public Dictionary<string, EntityConfig> Entities { get; set; } = new();
    }
}
