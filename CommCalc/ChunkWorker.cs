using Microsoft.Extensions.Logging;
using CommCalc.Services;

namespace CommCalc.Jobs;

internal sealed class ChunkWorker : IChunkWorker
{
    private readonly IChunkProcessor _processor;
    private readonly ILogger<ChunkWorker> _logger;

    public ChunkWorker(IChunkProcessor processor, ILogger<ChunkWorker> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    public async Task ProcessChunk(int chunkIndex, int offset, int limit, Guid runId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Run {RunId}: starting chunk {ChunkIndex} (offset={Offset}, limit={Limit})", runId, chunkIndex, offset, limit);

        try
        {
            await _processor.ProcessChunkAsync(chunkIndex, offset, limit, runId, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Run {RunId}: finished chunk {ChunkIndex}", runId, chunkIndex);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Run {RunId}: chunk {ChunkIndex} cancelled", runId, chunkIndex);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Run {RunId}: chunk {ChunkIndex} failed", runId, chunkIndex);
            throw;
        }
    }
}