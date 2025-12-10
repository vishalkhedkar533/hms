using Microsoft.Extensions.Configuration; // To access IConfiguration
                                          // File: MetadataLoader.cs (Updated for root-level file loading)

namespace MetaDataLoader
{

public interface IMetadataLoader
    {
        // ... methods remain the same ...
        OperationConfig GetOperation(string entityName, string operationName);
        string GetConnectionString(string applicationConnectionStringKey);
        string GetDefaultConnectionStringKey();
    }

    public class MetadataLoader : IMetadataLoader
    {
        private readonly DataAccessMetadata _metadata;
        private readonly IConfiguration _configuration;

        public MetadataLoader(IConfiguration configuration)
        {
            _configuration = configuration;

            // FIX: Instead of looking up a "DataAccess" section, we deserialize 
            // the root configuration into DataAccessMetadata. 
            // This relies on the host application loading the mappings file (e.g., mappings.postgresql.json).

            // We use Get<T>() on the root IConfiguration object.
            _metadata = configuration.Get<DataAccessMetadata>()
                ?? throw new InvalidOperationException("Data access metadata (from mappings file) is missing or invalid.");
        }

        public OperationConfig GetOperation(string entityName, string operationName)
        {
            if (!_metadata.Entities.TryGetValue(entityName, out var entityConfig))
            {
                throw new KeyNotFoundException($"Entity '{entityName}' not found in data access metadata.");
            }

            if (!entityConfig.TryGetValue(operationName, out var operationConfig))
            {
                throw new KeyNotFoundException($"Operation '{operationName}' not found for entity '{entityName}'.");
            }

            return operationConfig;
        }

        public string GetConnectionString(string applicationConnectionStringKey)
        {
            // This still uses IConfiguration to look up the connection string from appsettings.json
            return _configuration.GetConnectionString(applicationConnectionStringKey)
                ?? throw new ArgumentException($"Connection string for key '{applicationConnectionStringKey}' not found.");
        }

        public string GetDefaultConnectionStringKey()
        {
            return _metadata.ConnectionStringKey;
        }
    }
}
