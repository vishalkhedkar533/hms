namespace Database
{
    /// <summary>
    /// Minimal abstraction over a DB binary importer (eg. NpgsqlBinaryImporter).
    /// Keep the surface small to avoid leaking provider types.
    /// </summary>
    public interface IBinaryImporter : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Start a new row for the current binary import.
        /// </summary>
        void StartRow();

        /// <summary>
        /// Write a value for the current row (provider-specific type mapping performed by implementation).
        /// </summary>
        void Write<T>(T value);

        /// <summary>
        /// Complete the import and flush remaining data asynchronously.
        /// </summary>
        Task CompleteAsync(CancellationToken cancellationToken = default);
    }
}