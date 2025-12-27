using System.Data;
using System.Data.Common;

namespace Database
{
    // Define IConnectionFactory if it does not exist elsewhere in your project
    public interface IConnectionFactory
    {
        IDbConnection CreateConnection(string connectionString);
    }
    // Provider-agnostic IConnectionFactory implementation.
    // It uses DbProviderFactory by invariant name so no project code references Npgsql types.
    public class ProviderConnectionFactory : IConnectionFactory
    {
        private readonly DbProviderFactory _factory;

        public ProviderConnectionFactory(IConfiguration configuration)
        {
            var providerInvariantName = configuration.GetValue<string>("DatabaseProviderInvariantName");
            if (string.IsNullOrWhiteSpace(providerInvariantName))
            {
                throw new InvalidOperationException("Configuration key 'DatabaseProviderInvariantName' is required.");
            }

            try
            {
                _factory = DbProviderFactories.GetFactory(providerInvariantName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to obtain DbProviderFactory for '{providerInvariantName}'. Ensure the provider is registered and the provider package is referenced.",
                    ex);
            }
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            var conn = _factory.CreateConnection();
            if (conn is null)
            {
                throw new InvalidOperationException("DbProviderFactory produced a null connection.");
            }

            conn.ConnectionString = connectionString;
            return conn;
        }
    }
}
