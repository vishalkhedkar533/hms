namespace Tasks.Models
{
    public class StatusUpdateRow
    {
        public string? AgentCode { get; set; }
        public string? Status { get; set; }
        public DateTime? BusinessEffectiveDateOfChange { get; set; }
    }

    public class StatusUpdateError
    {
        public int RowNumber { get; set; }
        public string AgentCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class StatusUpdateResponse
    {
        public int TotalRows { get; set; }
        public int UpdatedRows { get; set; }
        public int FailedRows { get; set; }
        public List<StatusUpdateError> Errors { get; set; } = new();
        public byte[]? ErrorFile { get; set; }
    }
}
