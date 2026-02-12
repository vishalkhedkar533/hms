namespace SharedModels.DTO
{
    public class GeoSearchRequest
    {
        public long? ChannelCode { get; set; }
        public int? SubChannelCode { get; set; }
        public int? BranchCode{ get; set; }
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