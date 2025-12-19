using Microsoft.Extensions.Logging;
using System.Threading;

namespace CommCalc.Services;

internal sealed class ChunkProcessor : IChunkProcessor
{
    private readonly ILogger<ChunkProcessor> _logger;

    // Example: if you have a DbContext or other scoped services, inject them here.
    public ChunkProcessor(ILogger<ChunkProcessor> logger)
    {
        _logger = logger;
    }

    public async Task ProcessChunkAsync(int chunkIndex, int offset, int limit, Guid runId, CancellationToken cancellationToken = default)
    {
        // This implementation simulates processing 'limit' records.
        // Replace the inner loop with your database fetch + business logic.
        const int innerBatch = 1000; // process small inner batches to reduce memory pressure
        var processed = 0;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        for (var processedInChunk = 0; processedInChunk < limit; processedInChunk += innerBatch)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var toProcess = Math.Min(innerBatch, limit - processedInChunk);

            // Simulate IO/CPU work. Replace with actual DB call + processing.
            await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken).ConfigureAwait(false);

            processed += toProcess;

            // Log progress every N records to avoid log spam
            if (processed % 5_000 == 0)
            {
                _logger.LogInformation("Run {RunId} chunk {ChunkIndex}: processed {Processed}/{Limit} records (elapsed {Elapsed})",
                    runId, chunkIndex, processed, limit, sw.Elapsed);
            }
        }

        sw.Stop();
        _logger.LogInformation("Run {RunId} chunk {ChunkIndex} complete: processed {Processed} records in {Elapsed}",
            runId, chunkIndex, processed, sw.Elapsed);
    }
}