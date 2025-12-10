using MetaDataLoader;
using System.Data.Common;
using Dapper;

namespace Database
{
    public abstract class DapperRepositoryBase
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IMetadataLoader _metadataLoader;
        private readonly string _entityName;

        public DapperRepositoryBase(IConnectionFactory connectionFactory, IMetadataLoader metadataLoader, string entityName)
        {
            _connectionFactory = connectionFactory;
            _metadataLoader = metadataLoader;
            _entityName = entityName;
        }

        /// <summary>
        /// Retrieves the SQL script and connection string key from the metadata loader.
        /// </summary>
        private (string SqlScript, string ConnString) GetOperationDetails(string operationName)
        {
            var op = _metadataLoader.GetOperation(_entityName, operationName);

            // 1. Get the key (e.g., "HMSContext") from the root of the metadata JSON
            var key = _metadataLoader.GetDefaultConnectionStringKey();

            // 2. Use the key to fetch the actual connection string from the application's IConfiguration
            var connString = _metadataLoader.GetConnectionString(key);

            return (op.Script, connString);
        }

        // --- Core Transactional Method (Create/Update/Delete) ---
        protected async Task<TResult> ExecuteInTransactionAsync<TResult>(
            string operationName, // Key to look up the script (e.g., "Insert")
            object parameters)    // Dapper parameters object (your DTO/model)
        {
            var (sql, connectionString) = GetOperationDetails(operationName);

            await using var connection = (DbConnection)_connectionFactory.CreateConnection(connectionString);
            await connection.OpenAsync();

            // 2. Start transaction
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var result = await connection.ExecuteScalarAsync<TResult>(
                    sql,
                    parameters,
                    transaction: transaction // <-- CRITICAL: Pass the transaction
                );
                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                // 5. Rollback on exception
                await transaction.RollbackAsync();

                // Log the exception here if appropriate
                throw;
            }
        }

        // --- Core Query Method (Reads) ---
        protected async Task<IEnumerable<T>> QueryAsync<T>(
            string operationName, // Key to look up the script (e.g., "Search")
            object parameters)
        {
            var (sql, connectionString) = GetOperationDetails(operationName);

            await using var connection = (DbConnection)_connectionFactory.CreateConnection(connectionString);
            await connection.OpenAsync();

            // Use Dapper's QueryAsync to fetch a collection of T
            return await connection.QueryAsync<T>(sql, parameters);
        }
    }
}