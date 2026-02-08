namespace SharedModels.BackEndCalculation
{
    public class MasterTable
    {
        public int OrgId { get; set; }
        public string EntryCategory { get; set; } = string.Empty;
        public string SchemaName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string? FilterCriteria { get; set; }
        public string? ColumnAlias { get; set; }
    }
}
