namespace Common
{
    /// <summary>
    /// Small, dependency‑free retry helper with exponential backoff + jitter.
    /// Use for transient or concurrency failures. For advanced scenarios prefer Polly.
    /// </summary>
    public static class RetryHelper
    {
        public static async Task<T> RetryOnExceptionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            Func<Exception, bool>? shouldRetry = null,
            int maxAttempts = 3,
            int initialDelayMs = 100,
            int maxJitterMs = 100,
            CancellationToken cancellationToken = default)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));

            var rnd = new Random();
            int attempt = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return await operation(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (shouldRetry == null || shouldRetry(ex))
                {
                    attempt++;
                    if (attempt >= maxAttempts)
                        throw;

                    // exponential backoff + jitter
                    var backoff = (int)(initialDelayMs * Math.Pow(2, attempt - 1));
                    var jitter = rnd.Next(0, maxJitterMs);
                    var delay = TimeSpan.FromMilliseconds(backoff + jitter);

                    try
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { throw; }
                }
            }
        }
    }
}   