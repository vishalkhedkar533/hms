using CommonLibrary;
using Models.DTO;
using Npgsql;

namespace Data
{
    public class PostgresBulkInsertService<T> : IBulkInsertService<T> where T : AgentDto
    {
        private readonly string _connectionString;

        public PostgresBulkInsertService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task BulkInsertAsync(IEnumerable<T> rows, CancellationToken token)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(token);

            using var writer = conn.BeginBinaryImport(
                "COPY my_table (col1, col2, col3) FROM STDIN (FORMAT BINARY)");

            foreach (var r in rows)
            {
                writer.StartRow();
                writer.Write(r.AgentId);
                writer.Write(r.AgentLevel);
                writer.Write(r.AgentName);
            }

            await writer.CompleteAsync(token);
        }
    }
}
