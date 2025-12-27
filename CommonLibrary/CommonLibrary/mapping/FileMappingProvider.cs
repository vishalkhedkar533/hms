using System.Collections.Concurrent;
using System.Text.Json;

namespace CommonLibrary.mapping
{
    public sealed class FileMappingProvider : IMappingProvider
    {
        private readonly ConcurrentDictionary<string, MappingRoot> _byToken = new();
        private readonly ConcurrentDictionary<string, MappingRoot> _byConnectionKey = new();

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
                    var mapping = JsonSerializer.Deserialize<MappingRoot>(json, _jsonOptions);
                    if (mapping is null)
                        continue;

                    var fileName = Path.GetFileNameWithoutExtension(file); // mappings.postgresql
                    var parts = fileName.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    var token = parts.Length >= 2 ? parts[1] : fileName;

                    _byToken[token] = mapping;

                    if (!string.IsNullOrWhiteSpace(mapping.ConnectionStringKey))
                    {
                        _byConnectionKey[mapping.ConnectionStringKey] = mapping;
                    }
                }
                catch
                {
                    // ignore malformed files; keep provider resilient
                }
            }
        }

        public bool TryGetByConnectionStringKey(string connectionStringKey, out MappingRoot? mapping)
        {
            return _byConnectionKey.TryGetValue(connectionStringKey, out mapping);
        }

        public bool TryGetByDatabaseToken(string databaseToken, out MappingRoot? mapping)
        {
            return _byToken.TryGetValue(databaseToken, out mapping);
        }

        public IReadOnlyDictionary<string, MappingRoot> GetAll() => _byToken;
        public string? GetScriptForOperation(string entityName, string operationName)
        {
            // First try by connection-string key (matches ConnectionStringKey in mapping JSON)
            if (this.TryGetByConnectionStringKey("HMSContext", out var mapping))
            {
                if (mapping?.Entities != null &&
                    mapping.Entities.TryGetValue(entityName, out var entityMapping))
                {
                    // EntityMapping is designed to map operation name -> OperationMapping
                    if (entityMapping is not null && entityMapping.TryGetValue(operationName, out var opMapping))
                    {
                        return opMapping?.Script;
                    }
                }
            }

            // Fallback: try by common database token (e.g. "postgresql")
            if (this.TryGetByDatabaseToken("postgresql", out var tokenMapping))
            {
                if (tokenMapping?.Entities != null &&
                    tokenMapping.Entities.TryGetValue(entityName, out var tokenEntityMapping))
                {
                    if (tokenEntityMapping is not null && tokenEntityMapping.TryGetValue(operationName, out var opMapping))
                    {
                        return opMapping?.Script;
                    }
                }
            }

            return null;
        }

    }
}



