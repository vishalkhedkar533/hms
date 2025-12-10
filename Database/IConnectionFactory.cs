using System.Data;
namespace Database
{
    public interface IConnectionFactory
    {
        /// <summary>
        /// Creates a new, concrete database connection object using the provided connection string.
        /// The connection is NOT opened here.
        /// </summary>
        /// <param name="connectionString">The dynamic connection string to use.</param>
        /// <returns>An IDbConnection object (e.g., SqlConnection, NpgsqlConnection).</returns>
        IDbConnection CreateConnection(string connectionString);
    }
}
