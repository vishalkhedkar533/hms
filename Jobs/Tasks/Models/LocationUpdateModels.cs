using System.Runtime.InteropServices;

namespace Tasks.Models
{
    public class LocationUpdateRow
    {
        public string? AgentCode { get; set; }
        public string? LocationType { get; set; }
        public string? LocationCode { get; set; }
        public string? CurrentChannel { get; set; }
        public string? CurrentSubChannel { get; set; }
        public string? OrgId { get; set; }
        public string? Status { get; set; }
    }

    public class LocationUpdateError
    {
        public int RowNumber { get; set; }
        public string AgentCode { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class LocationUpdateResponse
    {
        public int TotalRows { get; set; }
        public int UpdatedRows { get; set; }
        public int FailedRows { get; set; }
        public List<LocationUpdateError> Errors { get; set; } = new();
        public byte[]? ErrorFile { get; set; }
    }

    public class LocationUpdateLookup
    {
        public string LocationCode { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public string SubChannelName { get; set; } = string.Empty;
        public long LocationMasterId { get; set; }
    }
}