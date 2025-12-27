using System.Data;

namespace Tasks.Database
{
    // Define IConnectionFactory if it does not exist elsewhere in your project
    public interface IConnectionFactory
    {
        IDbConnection CreateConnection(string connectionString);
    }
}
