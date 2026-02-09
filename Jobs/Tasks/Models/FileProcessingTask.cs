namespace Tasks.Models
{
    public class FileProcessingTask
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string Status { get; set; } = "Pending";
        public string CreatedBy { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string FileType { get; set; }
        public long? FileSize { get; set; }
        public bool? IsReadOnly { get; set; }
        public DateTime? FileCreationTime { get; set; }
        public DateTime? FileLastWriteTime { get; set; }
        public int? TotalRows { get; set; }
        public int? RowsProcessed { get; set; }
        public int? RowsRejected { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessData { get; set; }
        public int? OrgId { get; set; }
    }
}
