using Hangfire;

namespace CommCalc.Jobs 
{
    public static class ChunkJobScheduler
    {
        /// <summary>
        /// Enqueue chunk jobs for a single processing run. This will create N background jobs (one per chunk).
        /// Use a runId to correlate logs and results.
        /// </summary>
        public static Guid EnqueueAllChunks(int totalRecords = 2_000_000, int chunkSize = 10_000, string queue = "default")
        {
            if (totalRecords <= 0) throw new ArgumentOutOfRangeException(nameof(totalRecords));
            if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));

            var runId = Guid.NewGuid();
            var chunks = (totalRecords + chunkSize - 1) / chunkSize;

            for (var i = 0; i < chunks; i++)
            {
                var offset = i * chunkSize;
                var limit = Math.Min(chunkSize, totalRecords - offset);

                // Enqueue using DI-resolved worker. Set queue to control worker assignment.
                BackgroundJob.Enqueue<IChunkWorker>(w => w.ProcessChunk(i, offset, limit, runId, CancellationToken.None));
                // If you want to route to a specific queue:
                // BackgroundJobClient client = new BackgroundJobClient();
                // client.Create(() => default(Void)), new EnqueuedState(queue));

                // To use queue name explicitly, use:
                // BackgroundJob.Enqueue<IChunkWorker>(w => w.ProcessChunk(i, offset, limit, runId, CancellationToken.None));
                // and rely on AddHangfireServer(options => options.Queues = new[] { "critical", "default" });
            }

            return runId;
        }
    }
}