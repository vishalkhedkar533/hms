namespace CommCalc.Services;

public interface IChunkProcessor
{
    /// <summary>
    /// Implement actual processing logic here. This example provides a cancellable, scoped worker
    /// that processes up to <paramref name="limit"/> records starting at <paramref name="offset"/>.
    /// </summary>
    Task ProcessChunkAsync(int chunkIndex, int offset, int limit, Guid runId, CancellationToken cancellationToken = default);
}