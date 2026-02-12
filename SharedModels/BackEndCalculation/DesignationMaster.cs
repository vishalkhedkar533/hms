namespace SharedModels.BackEndCalculation
{
    public class DesignationMaster
    {
        public long DesignationId { get; set; } // int8 -> long
        public string DesignationCode { get; set; } = null!;
        public string DesignationName { get; set; } = null!;
        public int? DesignationLevel { get; set; } // int4 -> int
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? RowVersion { get; set; }
        public long? ChannelId { get; set; }
        public int? OrgId { get; set; }
        public string? HierarchyPath { get; set; } // ltree handled as string
        public string? CodeFormat { get; set; }
    }
}
