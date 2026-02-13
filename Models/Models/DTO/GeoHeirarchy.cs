namespace Models.DTO
{
    public class GeoHierarchyDto
    {
        public long BranchMasterId { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? LocationCode { get; set; }
        public string? LocationDesc { get; set; }

        // This allows the DTO to nest infinitely, matching your JSON structure
        public GeoHierarchyDto? ParentLocation { get; set; }

        // Helpful for tracking the full breadcrumb
        public string? HierarchyPath { get; set; }
    }
    public class GeoSearchRequest
    {
        public long? ChannelCode { get; set; }
        public int? SubChannelCode { get; set; }
        public int? BranchCode { get; set; }
    }
    public class GeoChildrenRequest
    {
        public long ParentBranchId { get; set; }
    }
    public class ReporteesByLocationRequest
    {
        public string ChannelCode { get; set; } = null!;
        public string LocationCode { get; set; } = null!;
    }
    public class GeoHierarchyAgentDto
    {
        public long AgentId { get; set; }
        public string? AgentName { get; set; }
        public string? AgentDesignation { get; set; }
        public string? AgentCode { get; set; }
        public string? Location { get; set; }

    }
}