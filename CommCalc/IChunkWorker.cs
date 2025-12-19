namespace CommCalc.Jobs;

public interface IChunkWorker
{
    /// <summary>
    /// Processes a single chunk. Hangfire will resolve an instance from DI.
    /// </summary>
    Task ProcessChunk(int chunkIndex, int offset, int limit, Guid runId, CancellationToken cancellationToken = default);
}