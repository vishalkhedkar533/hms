namespace CommonLibrary
{
    public interface IBulkInsertService<T>
    {
        Task BulkInsertAsync(IEnumerable<T> rows, CancellationToken token);
    }

}
