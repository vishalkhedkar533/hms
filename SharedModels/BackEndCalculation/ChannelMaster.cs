namespace SharedModels.BackEndCalculation
{
    public class ChannelMaster
    {
        public long ChannelId { get; set; } // int8 maps to long
        public string ChannelCode { get; set; } = null!;
        public string ChannelName { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? RowVersion { get; set; }
        public int? OrgId { get; set; }
        public long? TotalEntries { get; set; }
        public long? TotalEntriesMon { get; set; }
    }
}
