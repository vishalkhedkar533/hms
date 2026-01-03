using System.Data.Common;

namespace Database
{
    /// <summary>
    /// Factory that starts a binary import session for a given open connection.
    /// Implementations may depend on provider-specific APIs (eg. Npgsql).
    /// </summary>
    public interface IBinaryImportFactory
    {
        /// <summary>
        /// Begin a binary import for the supplied connection using the COPY command.
        /// The connection must be open and of a compatible provider type.
        /// </summary>
        Task<IBinaryImporter> BeginBinaryImportAsync(DbConnection connection, string copyCommand, CancellationToken cancellationToken = default);

        /// <summary>
        /// Begin a binary update flow for bulk updates.
        /// The caller is responsible for preparing an appropriate COPY command that writes
        /// the necessary key and update columns (typically into a temp table).
        /// </summary>
        Task<IBinaryImporter> BeginBinaryUpdateAsync(DbConnection connection, string copyCommand, CancellationToken cancellationToken = default);

        /// <summary>
        /// Begin a binary delete flow for bulk deletes.
        /// The caller is responsible for preparing an appropriate COPY command that writes
        /// the key columns used to identify rows to delete.
        /// </summary>
        Task<IBinaryImporter> BeginBinaryDeleteAsync(DbConnection connection, string copyCommand, CancellationToken cancellationToken = default);
    }
}