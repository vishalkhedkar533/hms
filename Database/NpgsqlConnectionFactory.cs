using System.Data;
using Npgsql;
namespace Database
{
    public class NpgsqlConnectionFactory : IConnectionFactory
    {
        // The constructor is clean, relying on DI for IConnectionFactory registration only.
        public NpgsqlConnectionFactory() { }

        public IDbConnection CreateConnection(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");
            }

            // Returns the concrete connection object for PostgreSQL
            return new NpgsqlConnection(connectionString);
        }
    }
}
