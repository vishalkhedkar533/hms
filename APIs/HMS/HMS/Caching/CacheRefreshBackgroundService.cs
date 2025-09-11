namespace HMS.Caching
{
    public class CacheRefreshBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromMinutes(1);

        public CacheRefreshBackgroundService(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(_pollingInterval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                using var scope = _services.CreateScope();
                var cacheService = scope.ServiceProvider.GetRequiredService<GenericCacheService>();

                var refreshConfig = cacheService.GetRefreshConfig();

                foreach (var entry in refreshConfig)
                {
                    var cacheKey = entry.Key;
                    var interval = entry.Value.Interval;
                    var lastRefreshed = entry.Value.LastRefreshed;
                    var modelType = entry.Value.ModelType;

                    if (DateTime.UtcNow - lastRefreshed >= TimeSpan.FromMinutes(interval))
                    {
                        var parts = cacheKey.Split('.');
                        if (parts.Length >= 2)
                        {
                            var schema = parts[0];
                            var table = parts[1];

                            if (modelType == null)
                            {
                                await cacheService.RefreshCacheAsync(schema, table);
                            }
                            else
                            {
                                var method = typeof(GenericCacheService)
                                    .GetMethod(nameof(GenericCacheService.RefreshCacheAsync), new[] { typeof(string), typeof(string) })
                                    ?.MakeGenericMethod(modelType);

                                if (method != null)
                                    await (Task)method.Invoke(cacheService, new object[] { schema, table })!;
                            }
                        }
                    }
                }
            }
        }
    }
}