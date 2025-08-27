using Npgsql;
using System.Data;
using System.Diagnostics;
using System.Threading.Channels;

namespace HMS.Logging
{
    // ----------------------
    // Log Entry Record
    // ----------------------
    public record AppLogEntry(
        DateTime Timestamp,
        string LogLevel,
        string Message,
        string? Exception,
        string? UserId,
        string? UserName,
        string? SourceFile,
        int? SourceLine,
        string? SourceMember,
        string? Category,
        string? StackTrace,
        string? JobName
    );

    // ----------------------
    // Filter Policy
    // ----------------------
    public class AppLogFilterState
    {
        public LogLevel Minimum_Level { get; private set; } = LogLevel.Information;
        public HashSet<string> ExcludedCategories { get; private set; } = new();

        public void Update(string minLevel, IEnumerable<string>? excludedCategories)
        {
            Minimum_Level = Enum.TryParse<LogLevel>(minLevel, true, out var level) ? level : LogLevel.Information;
            ExcludedCategories = excludedCategories != null
                ? new HashSet<string>(excludedCategories, StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>();
        }
    }

    // ----------------------
    // Custom Logger
    // ----------------------
    public class AppLogLogger : ILogger
    {
        private readonly string _category;
        private readonly Channel<AppLogEntry> _channel;
        private readonly AppLogFilterState _filter;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _jobName;

        public AppLogLogger(
            string category,
            Channel<AppLogEntry> channel,
            AppLogFilterState filter,
            IHttpContextAccessor httpContextAccessor,
            string? jobName = null)
        {
            _category = category;
            _channel = channel;
            _filter = filter;
            _httpContextAccessor = httpContextAccessor;
            _jobName = jobName;
        }

        public IDisposable? BeginScope<TState>(TState state) => default;
        public bool IsEnabled(LogLevel logLevel) =>
            logLevel >= _filter.Minimum_Level && !_filter.ExcludedCategories.Contains(_category);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
    Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            // Capture user info from HttpContext if available
            var http = _httpContextAccessor.HttpContext;
            var userId = http?.User?.FindFirst("sub")?.Value ?? "system";
            var userName = http?.User?.Identity?.Name ?? "system";

            // Capture caller info from stack trace
            string? sourceFile = null;
            int? sourceLine = null;
            string? sourceMember = null;

            try
            {
                var stackTrace = new StackTrace(true);
                // Skip 0 => this method, 1 => ILogger.Log, 2 => caller
                var frame = stackTrace.GetFrame(2);
                if (frame != null)
                {
                    sourceFile = frame.GetFileName();
                    sourceLine = frame.GetFileLineNumber();
                    sourceMember = frame.GetMethod()?.Name;
                }
            }
            catch
            {
                // ignore any stack trace issues
            }

            var entry = new AppLogEntry(
                Timestamp: DateTime.UtcNow,
                LogLevel: logLevel.ToString(),
                Message: formatter(state, exception),
                Exception: exception?.ToString(),
                UserId: userId,
                UserName: userName,
                Category: _category,
                StackTrace: exception?.StackTrace,
                JobName: _jobName,
                SourceFile: sourceFile,
                SourceLine: sourceLine,
                SourceMember: sourceMember
            );

            _channel.Writer.TryWrite(entry);
        }

        // ----------------------
        // Logger Provider
        // ----------------------
        public class AppLogLoggerProvider : ILoggerProvider
        {
            private readonly Channel<AppLogEntry> _channel;
            private readonly AppLogFilterState _filter;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly string? _jobName;

            public AppLogLoggerProvider(Channel<AppLogEntry> channel, AppLogFilterState filter,
                IHttpContextAccessor httpContextAccessor, string? jobName = null)
            {
                _channel = channel;
                _filter = filter;
                _httpContextAccessor = httpContextAccessor;
                _jobName = jobName;
            }

            public ILogger CreateLogger(string categoryName) =>
                new AppLogLogger(categoryName, _channel, _filter, _httpContextAccessor, _jobName);

            public void Dispose() { }
        }

        // ----------------------
        // Background service: flush logs to PostgreSQL in batches
        // ----------------------
        public class AppLogBackgroundService : BackgroundService
        {
            private readonly Channel<AppLogEntry> _channel;
            private readonly string _connectionString;
            private readonly int _batchSize;
            private readonly TimeSpan _flushInterval;

            public AppLogBackgroundService(Channel<AppLogEntry> channel, string connectionString,
                int batchSize = 50, int flushSeconds = 2)
            {
                _channel = channel;
                _connectionString = connectionString;
                _batchSize = batchSize;
                _flushInterval = TimeSpan.FromSeconds(flushSeconds);
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                var buffer = new List<AppLogEntry>();

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        while (buffer.Count < _batchSize &&
                               await _channel.Reader.WaitToReadAsync(stoppingToken))
                        {
                            while (_channel.Reader.TryRead(out var log))
                                buffer.Add(log);
                        }

                        if (buffer.Count > 0)
                        {
                            await InsertBatchAsync(buffer, stoppingToken);
                            buffer.Clear();
                        }
                        else
                        {
                            await Task.Delay(_flushInterval, stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Logging service error: " + ex.Message);
                    }
                }
            }

            private async Task InsertBatchAsync(List<AppLogEntry> logs, CancellationToken token)
            {
                if (logs == null || logs.Count == 0) return;

                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync(token);

                await using var tx = await conn.BeginTransactionAsync(token);
                await using var cmd = new NpgsqlCommand
                {
                    Connection = conn,
                    Transaction = tx,
                    CommandText = @"INSERT INTO hms.applog
    (""Timestamp"", ""LogLevel"", ""Message"", ""Exception"", ""UserId"", ""UserName"", 
     ""SourceFile"", ""SourceLine"", ""SourceMember"", ""Category"", ""StackTrace"", ""JobName"")VALUES
    (@ts, @level, @msg, @ex, @uid, @uname, @srcfile, @srcline, @srcmem, @cat, @stack, @job)"
                };

                cmd.Parameters.Add(new NpgsqlParameter("ts", DbType.DateTime));
                cmd.Parameters.Add(new NpgsqlParameter("level", DbType.String));
                cmd.Parameters.Add(new NpgsqlParameter("msg", DbType.String));
                cmd.Parameters.Add(new NpgsqlParameter("ex", DbType.String));
                cmd.Parameters.Add(new NpgsqlParameter("uid", DbType.String));
                cmd.Parameters.Add(new NpgsqlParameter("uname", DbType.String));
                cmd.Parameters.Add(new NpgsqlParameter("srcfile", DbType.String));
                cmd.Parameters.Add(new NpgsqlParameter("srcline", DbType.Int32));
                cmd.Parameters.Add(new NpgsqlParameter("srcmem", DbType.String));
                cmd.Parameters.Add(new NpgsqlParameter("cat", DbType.String));
                cmd.Parameters.Add(new NpgsqlParameter("stack", DbType.String));
                cmd.Parameters.Add(new NpgsqlParameter("job", DbType.String));

                foreach (var log in logs)
                {
                    cmd.Parameters["ts"].Value = log.Timestamp;
                    cmd.Parameters["level"].Value = log.LogLevel;
                    cmd.Parameters["msg"].Value = log.Message;
                    cmd.Parameters["ex"].Value = (object?)log.Exception ?? DBNull.Value;
                    cmd.Parameters["uid"].Value = (object?)log.UserId ?? DBNull.Value;
                    cmd.Parameters["uname"].Value = (object?)log.UserName ?? DBNull.Value;
                    cmd.Parameters["srcfile"].Value = (object?)log.SourceFile ?? DBNull.Value;
                    cmd.Parameters["srcline"].Value = (object?)log.SourceLine ?? DBNull.Value;
                    cmd.Parameters["srcmem"].Value = (object?)log.SourceMember ?? DBNull.Value;
                    cmd.Parameters["cat"].Value = (object?)log.Category ?? DBNull.Value;
                    cmd.Parameters["stack"].Value = (object?)log.StackTrace ?? DBNull.Value;
                    cmd.Parameters["job"].Value = (object?)log.JobName ?? DBNull.Value;

                    await cmd.ExecuteNonQueryAsync(token);
                }

                await tx.CommitAsync(token);
            }
        }

        // ----------------------
        // Filter refresher from DB
        // ----------------------
        public class AppLogFilterRefresher : BackgroundService
        {
            private readonly AppLogFilterState _state;
            private readonly string _connectionString;

            public AppLogFilterRefresher(AppLogFilterState state, string connectionString)
            {
                _state = state;
                _connectionString = connectionString;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await using var conn = new NpgsqlConnection(_connectionString);
                        await conn.OpenAsync(stoppingToken);

                        await using var cmd = new NpgsqlCommand(
                            "SELECT minimum_level, excluded_categories FROM hms.applog_filter_policy ORDER BY updated_at DESC LIMIT 1",
                            conn);

                        await using var reader = await cmd.ExecuteReaderAsync(stoppingToken);
                        if (await reader.ReadAsync(stoppingToken))
                        {
                            var minimumLevel = reader.GetString(0); // text -> string
                            string[]? excludedArray = null;

                            if (!reader.IsDBNull(1))
                            {
                                excludedArray = reader.GetFieldValue<string[]>(1); // text[] -> string[]
                            }

                            _state.Update(minimumLevel, excludedArray);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Filter refresh error: " + ex.Message);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
        }
    }
}