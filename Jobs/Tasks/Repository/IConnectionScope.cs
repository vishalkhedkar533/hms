using System.Data.Common;

namespace Repository
{
    public interface IConnectionScope : IAsyncDisposable
    {
        /// <summary>
        /// Returns an open <see cref="DbConnection"/> for the supplied connection string.
        /// The implementation may cache and reuse the connection for the lifetime of the DI scope.
        /// Caller MUST NOT dispose the returned connection; the scope will dispose it.
        /// </summary>
        Task<DbConnection> GetOpenConnectionAsync(string connectionString);
    }
}