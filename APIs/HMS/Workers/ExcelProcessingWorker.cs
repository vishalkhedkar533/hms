using CommonLibrary;
using CommonLibrary.Background;
using MiniExcelLibs;
using Models.DTO;
using Npgsql;
using System.Threading.Channels;

namespace Workers
{
    public class ExcelProcessingWorker : BackgroundService
    {
        private readonly IBulkInsertService<AgentDto> _bulkInsert;
        private readonly ILogger<ExcelProcessingWorker> _logger;
        private readonly IExcelProcessingQueue _queue;
        private readonly Channel<string> _channel;
        private readonly IBulkInsertService<AgentDto> _inserter;
        public ExcelProcessingWorker(ILogger<ExcelProcessingWorker> logger, 
            IExcelProcessingQueue queue,
            IBulkInsertService<AgentDto> bulkInsert)
        {
            _logger = logger;
            _queue = queue;
            _bulkInsert = bulkInsert;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string filePath = await _queue.DequeueAsync(stoppingToken);

                await foreach (var chunk in _reader.ReadInChunksAsync(filePath, 10_000))
                {
                    await _inserter.BulkInsertAsync(chunk, stoppingToken);
                }
            }
        }
        private async Task ProcessFile(string file, CancellationToken token)
        {
            var batch = new List<AgentDto>(5000);

            foreach (var row in MiniExcel.Query(file))
            {
                var d = (IDictionary<string, object>)row;
                batch.Add(new AgentDto
                {
                    AgentId = int.Parse( d["AgentId"]?.ToString()),
                    AgentLevel = d["AgentLevel"]?.ToString(),
                    AgentName = d["AgentName"]?.ToString()
                });

                if (batch.Count >= 5000)
                {
                    await _bulkInsert.BulkInsertAsync(batch, token);
                    batch.Clear();
                }
            }

            if (batch.Any())
                await _bulkInsert.BulkInsertAsync(batch, token);
        }
    }
}