namespace CommonLibrary.mapping
{
    public interface IMappingProvider
    {
        bool TryGetByDatabaseToken(string databaseToken, out MappingModel? mapping);
        IReadOnlyDictionary<string, MappingModel> GetAll();
        public OperationMapping? GetScriptForOperation(string entityName, string operationName);
    }
}