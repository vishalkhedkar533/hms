using Npgsql;
using System.Data.Common;

namespace Database
{
    /// <summary>
    /// Npgsql implementation of IBinaryImportFactory/IBinaryImporter.
    /// Returns a thin wrapper around NpgsqlBinaryImporter so application code can remain provider-agnostic.
    /// </summary>
    public class NpgBulkOpsFactory : IBinaryImportFactory
    {
        public async Task<IBinaryImporter> BeginBinaryImportAsync(DbConnection connection
            , string copyCommand
            , CancellationToken cancellationToken = default)
        {
            if (connection is not NpgsqlConnection npgsqlConn)
                throw new ArgumentException("Connection must be an NpgsqlConnection", nameof(connection));

            var importer = await npgsqlConn.BeginBinaryImportAsync(copyCommand, cancellationToken).ConfigureAwait(false);
            return new NpgBulkImporterWrapper(importer);
        }

        // Bulk update and delete delegate to the same COPY-based import mechanism.
        // The caller should prepare an appropriate COPY command for the intended operation.
        public Task<IBinaryImporter> BeginBinaryUpdateAsync(DbConnection connection, string copyCommand, CancellationToken cancellationToken = default)
            => BeginBinaryImportAsync(connection, copyCommand, cancellationToken);

        public Task<IBinaryImporter> BeginBinaryDeleteAsync(DbConnection connection, string copyCommand, CancellationToken cancellationToken = default)
            => BeginBinaryImportAsync(connection, copyCommand, cancellationToken);

        private sealed class NpgBulkImporterWrapper : IBinaryImporter
        {
            private readonly NpgsqlBinaryImporter _inner;
            private bool _disposed;

            public NpgBulkImporterWrapper(NpgsqlBinaryImporter inner) => _inner = inner ?? throw new ArgumentNullException(nameof(inner));

            public void StartRow()
            {
                ThrowIfDisposed();
                _inner.StartRow();
            }

            public void Write<T>(T value)
            {
                ThrowIfDisposed();
                _inner.Write(value);
            }

            public async Task CompleteAsync(CancellationToken cancellationToken = default)
            {
                ThrowIfDisposed();
                await _inner.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }

            public ValueTask DisposeAsync()
            {
                if (_disposed) return ValueTask.CompletedTask;
                _disposed = true;
                return _inner.DisposeAsync();
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _inner.Dispose();
            }

            private void ThrowIfDisposed()
            {
                if (_disposed) throw new ObjectDisposedException(nameof(NpgBulkImporterWrapper));
            }
        }
    }
}