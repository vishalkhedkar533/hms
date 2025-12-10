namespace CommonLibrary.Background
{
    public interface IExcelProcessingQueue
    {
        void Enqueue(string filePath);
        ValueTask<string> DequeueAsync(CancellationToken token);
    }
}
