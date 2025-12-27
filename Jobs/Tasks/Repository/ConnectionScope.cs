using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using Tasks.Database;

namespace Repository
{
    public sealed class ConnectionScope : IConnectionScope
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ConcurrentDictionary<string, DbConnection> _connections = new();
        private bool _disposed;

        public ConnectionScope(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<DbConnection> GetOpenConnectionAsync(string connectionString)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ConnectionScope));
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Connection string is required.", nameof(connectionString));

            if (_connections.TryGetValue(connectionString, out var existing))
            {
                if (existing.State != ConnectionState.Open)
                {
                    await existing.OpenAsync();
                }
                return existing;
            }

            var conn = (DbConnection)_connectionFactory.CreateConnection(connectionString);
            await conn.OpenAsync();
            _connections[connectionString] = conn;
            return conn;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var kv in _connections)
            {
                var conn = kv.Value;
                try
                {
                    await conn.DisposeAsync();
                }
                catch
                {
                    try { conn.Dispose(); } catch { /* ignore */ }
                }
            }

            _connections.Clear();
        }
    }
}