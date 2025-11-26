using CommonLibrary.Background;
using System.Threading.Channels;

namespace Workers
{
    public class ExcelProcessingQueue : IExcelProcessingQueue
    {
        private readonly Channel<string> _channel;

        public ExcelProcessingQueue()
        {
            _channel = Channel.CreateUnbounded<string>();
        }

        public void Enqueue(string filePath)
        {
            _channel.Writer.TryWrite(filePath);
        }

        public async ValueTask<string> DequeueAsync(CancellationToken token)
        {
            return await _channel.Reader.ReadAsync(token);
        }
    }
}
