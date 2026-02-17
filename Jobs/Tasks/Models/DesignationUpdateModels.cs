namespace Tasks.Models
{
    public class DesignationUpdateRow
    {
        public string? AgentCode { get; set; }
        public string? Designation { get; set; }
        public string? BusinessEffectiveDateOfChange { get; set; }
    }

    public class DesignationUpdateError
    {
        public int RowNumber { get; set; }
        public string AgentCode { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class DesignationUpdateResponse
    {
        public int TotalRows { get; set; }
        public int UpdatedRows { get; set; }
        public int FailedRows { get; set; }
        public List<DesignationUpdateError> Errors { get; set; } = new();
        public byte[]? ErrorFile { get; set; }
    }
}
