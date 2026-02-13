namespace Tasks.Models
{
    public class ManagerUpdateRow
    {
        public string? AgentCode { get; set; }
        public string? SupervisorCode { get; set; }
        public DateTime? EffectiveDateOfChange { get; set; }
    }

    public class ManagerUpdateError 
    {
        public int RowNumber { get; set; }
        public string AgentCode { get; set; } = string.Empty;
        public string SupervisorCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ManagerUpdateResponse
    {
        public int TotalRows { get; set; }
        public int UpdatedRows { get; set; }
        public int FailedRows { get; set; }
        public List<ManagerUpdateError> Errors { get; set; } = new();
        public byte[]? ErrorFile { get; set; }
    }
}
