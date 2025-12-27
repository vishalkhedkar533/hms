namespace CommonLibrary.mapping
{
    public interface IMappingProvider
    {
        bool TryGetByConnectionStringKey(string connectionStringKey, out MappingRoot? mapping);
        bool TryGetByDatabaseToken(string databaseToken, out MappingRoot? mapping);
        IReadOnlyDictionary<string, MappingRoot> GetAll();
        public string? GetScriptForOperation(string entityName, string operationName);
    }
}
