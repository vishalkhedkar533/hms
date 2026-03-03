using System.Collections.Concurrent;
using System.Text.Json;

namespace CommonLibrary.mapping
{
    public sealed class FileMappingProvider : IMappingProvider
    {
        private readonly ConcurrentDictionary<string, MappingModel> _byToken = new();
        private readonly ConcurrentDictionary<string, MappingModel> _byConnectionKey = new();

        private readonly string _folderPath;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public FileMappingProvider(string folderPath)
        {
            _folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
            LoadAll();
        }

        private void LoadAll()
        {
            if (!Directory.Exists(_folderPath))
            {
                return;
            }

            foreach (var file in Directory.EnumerateFiles(_folderPath, "mappings.*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var mapping = JsonSerializer.Deserialize<MappingModel>(json, _jsonOptions);
                    if (mapping is null)
                        continue;

                    var fileName = Path.GetFileNameWithoutExtension(file); // mappings.postgresql
                    var parts = fileName.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    var token = parts.Length >= 2 ? parts[1] : fileName;

                    _byToken[token] = mapping;

                    // Populate connection-key index if mapping operations declare a ConnectionStringKey
                    if (mapping.Entities is not null)
                    {
                        foreach (var entityMapping in mapping.Entities.Values)
                        {
                            if (entityMapping is null)
                                continue;

                            foreach (var opMapping in entityMapping.Values)
                            {
                                if (opMapping is null)
                                    continue;

                                // If the operation mapping specifies a ConnectionStringKey, index it
                                var csKey = opMapping.ConnectionStringKey;
                                if (!string.IsNullOrWhiteSpace(csKey))
                                {
                                    _byConnectionKey[csKey] = mapping;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // ignore malformed files; keep provider resilient
                }
            }
        }

        public bool TryGetByConnectionStringKey(string connectionStringKey, out MappingModel? mapping)
        {
            return _byConnectionKey.TryGetValue(connectionStringKey, out mapping);
        }

        public bool TryGetByDatabaseToken(string databaseToken, out MappingModel? mapping)
        {
            return _byToken.TryGetValue(databaseToken, out mapping);
        }

        public IReadOnlyDictionary<string, MappingModel> GetAll() => _byToken;

        private string LoadSqlFromFile(string sqlFile)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Mappings", sqlFile);
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"SQL file not found: {filePath}");
            return File.ReadAllText(filePath);
        }
        public OperationMapping? GetScriptForOperation(string entityName, string operationName)
        {
            // First, search all token-based mappings (e.g. postgresql)
            foreach (var mapping in _byToken.Values)
            {
                if (mapping?.Entities != null &&
                    mapping.Entities.TryGetValue(entityName, out var entityMapping) &&
                    entityMapping is not null &&
                    entityMapping.TryGetValue(operationName, out var opMapping))
                {
                    if (opMapping?.SQLFile != null) 
                    {
                        opMapping.Script = LoadSqlFromFile(opMapping.SQLFile);
                    }
                    return opMapping;
                }
            }

            // Fallback: search connection-key indexed mappings
            foreach (var mapping in _byConnectionKey.Values)
            {
                if (mapping?.Entities != null &&
                    mapping.Entities.TryGetValue(entityName, out var entityMapping) &&
                    entityMapping is not null &&
                    entityMapping.TryGetValue(operationName, out var opMapping))
                {
                    return opMapping;
                }
            }
            //will reach here if no mapping found for the given entity and operation
            throw new InvalidOperationException($"Operation mapping for {entityName}/{operationName} not found.");
        }
    }
}